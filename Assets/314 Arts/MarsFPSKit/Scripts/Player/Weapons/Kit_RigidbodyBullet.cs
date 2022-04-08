using System.Linq;
using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        public class RigidbodyBulletSettings
        {
            /// <summary>
            /// Damage we apply
            /// </summary>
            public float damage;
            /// <summary>
            /// Our lifetime
            /// </summary>
            public float bulletLifeTime;
            /// <summary>
            /// Does the object stay alive after death for some time?
            /// </summary>
            public bool staysAliveAfterDeath;
            /// <summary>
            /// Time we stay alive after death
            /// </summary>
            public float staysAliveAfterDeathTime;
            /// <summary>
            /// Shot from this direction
            /// </summary>
            public Vector3 direction;
            /// <summary>
            /// Where does this bullet originate from?
            /// </summary>
            public Vector3 shotFromPosition;
            /// <summary>
            /// Was this fired locally (should apply damage)
            /// </summary>
            public bool isLocal;
            /// <summary>
            /// If <see cref="isLocal"/> is true, this will be assigned
            /// </summary>
            public Kit_PlayerBehaviour localOwner;
            /// <summary>
            /// ID of owner
            /// </summary>
            public int ownerID;
            /// <summary>
            /// Is the owner a bot?
            /// </summary>
            public bool ownerIsBot;
            /// <summary>
            /// Force to apply to ragdoll (if hit)
            /// </summary>
            public float ragdollForce = 500f;
            /// <summary>
            /// ID of the gun this bullet was fired with
            /// </summary>
            public int gameGunID;
        }

        public enum RigidbodyBulletOnImpact
        {
            Impact, Explosion
        }

        public class Kit_RigidbodyBullet : Kit_BulletBase
        {
            /// <summary>
            /// Our rigidbody
            /// </summary>
            public Rigidbody body;

            /// <summary>
            /// What do we do upon impact?
            /// </summary>
            public RigidbodyBulletOnImpact onImpact;

            /// <summary>
            /// Prefab
            /// </summary>
            public GameObject explosionPrefab;

            /// <summary>
            /// Visible renderer for bullet
            /// </summary>
            public GameObject bulletRenderer;

            #region Runtime
            /// <summary>
            /// Main.
            /// </summary>
            private Kit_IngameMain main;
            /// <summary>
            /// Since when does the bullet exist?
            /// </summary>
            private float bulletExistTime;

            private RigidbodyBulletSettings settings;

            /// <summary>
            /// After which frame should the bullet be shown?
            /// </summary>
            private int frameOfShow;
            /// <summary>
            /// Did we show bullet?
            /// </summary>
            private bool wasBulletShown;
            #endregion

            public override void Setup(Kit_IngameMain newMain, Kit_ModernWeaponScript ws, WeaponControllerRuntimeData data, Kit_PlayerBehaviour pb, Vector3 dir)
            {
                //Setup
                RigidbodyBulletSettings rbs = new RigidbodyBulletSettings();
                //Setup data
                rbs.damage = data.baseDamage;
                rbs.direction = dir;
                rbs.ragdollForce = data.ragdollForce;
                rbs.gameGunID = ws.gameGunID;
                rbs.bulletLifeTime = data.bulletLifeTime;
                rbs.staysAliveAfterDeath = data.bulletStaysAliveAfterDeath;
                rbs.staysAliveAfterDeathTime = data.bulletStaysAliveAfterDeathTime;
                rbs.shotFromPosition = pb.transform.position;
                rbs.isLocal = pb.photonView.IsMine;
                rbs.localOwner = pb;
                rbs.ownerIsBot = pb.isBot;
                if (pb.isBot)
                    rbs.ownerID = pb.botId;
                else
                    rbs.ownerID = pb.photonView.Owner.ActorNumber;

                //Set main
                main = newMain;
                //Set data
                settings = rbs;

                //Reset force
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;

                //Apply force
                body.velocity = pb.movement.GetVelocity(pb);
                body.AddRelativeForce(0, 0, data.bulletSpeed, ForceMode.Impulse);

                bulletExistTime = 0f;

                frameOfShow = Time.frameCount + data.bulletHideForFrames;
                wasBulletShown = false;

                if (bulletRenderer)
                {
                    if (data.bulletHideForFrames > 0)
                    {
                        bulletRenderer.SetActiveOptimized(false);
                    }
                    else
                    {
                        bulletRenderer.SetActiveOptimized(true);
                        wasBulletShown = true;
                    }
                }
            }

            #region Unity Calls
            void Update()
            {

                //Check if we should destroy
                bulletExistTime += Time.deltaTime;
                if (bulletExistTime > settings.bulletLifeTime && enabled) main.objectPooling.DestroyInstantiateable(gameObject);

                if (!wasBulletShown)
                {
                    if (Time.frameCount >= frameOfShow)
                    {
                        if (bulletRenderer)
                        {
                            bulletRenderer.SetActiveOptimized(true);
                        }

                        wasBulletShown = true;
                    }
                }
            }

            void OnCollisionEnter(Collision collision)
            {
                //Check if we hit ourself
                if (settings.localOwner)
                {
                    for (int i = 0; i < collision.contacts.Length; i++)
                    {
                        if (collision.contacts[i].otherCollider.transform.root == settings.localOwner.transform) return;
                    }
                }

                if (onImpact == RigidbodyBulletOnImpact.Impact)
                {
                    if (collision.contacts[0].otherCollider.transform.GetComponent<Kit_PlayerDamageMultiplier>() && main.currentGameModeType == 2)
                    {
                        Kit_PlayerDamageMultiplier pdm = collision.contacts[0].otherCollider.transform.GetComponent<Kit_PlayerDamageMultiplier>();
                        if (collision.contacts[0].otherCollider.transform.root.GetComponent<Kit_PlayerBehaviour>())
                        {
                            if (settings.isLocal)
                            {
                                Kit_PlayerBehaviour hitPb = collision.contacts[0].otherCollider.transform.root.GetComponent<Kit_PlayerBehaviour>();
                                //First check if we can actually damage that player
                                if ((settings.localOwner && main.currentPvPGameModeBehaviour.ArePlayersEnemies(settings.localOwner, hitPb)) || (!settings.localOwner && main.currentPvPGameModeBehaviour.ArePlayersEnemies(main, settings.ownerID, settings.ownerIsBot, hitPb, false)))
                                {
                                    //Check if he has spawn protection
                                    if (!hitPb.spawnProtection || hitPb.spawnProtection.CanTakeDamage(hitPb))
                                    {
                                        //Apply local damage
                                        hitPb.LocalDamage(settings.damage * pdm.damageMultiplier, settings.gameGunID, settings.shotFromPosition, settings.direction, settings.ragdollForce, collision.contacts[0].point, pdm.ragdollId, settings.ownerIsBot, settings.ownerID);
                                        if (!settings.ownerIsBot)
                                        {
                                            //Since we hit a player, show the hitmarker
                                            main.hud.DisplayHitmarker();
                                        }
                                    }
                                    else if (!settings.ownerIsBot)
                                    {
                                        //We hit a player but his spawn protection is active
                                        main.hud.DisplayHitmarkerSpawnProtected();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (settings.isLocal)
                        {
                            if (collision.contacts[0].otherCollider.GetComponentInParent<IKitDamageable>() != null)
                            {
                                if (collision.contacts[0].otherCollider.GetComponentInParent<IKitDamageable>().LocalDamage(settings.damage, settings.gameGunID, settings.shotFromPosition, settings.direction, settings.ragdollForce, collision.contacts[0].point, settings.ownerIsBot, settings.ownerID))
                                {
                                    if (!settings.ownerIsBot)
                                    {
                                        //Since we hit a player, show the hitmarker
                                        main.hud.DisplayHitmarker();
                                    }
                                }
                            }
                        }
                    }

                    main.impactProcessor.ProcessImpact(main, collision.contacts[0].point, collision.contacts[0].normal, collision.contacts[0].otherCollider.tag, collision.contacts[0].otherCollider.transform);
                }
                else if (onImpact == RigidbodyBulletOnImpact.Explosion)
                {
                    if (explosionPrefab)
                    {
                        GameObject go = Instantiate(explosionPrefab, transform.position, transform.rotation);
                        if (go.GetComponent<Kit_Explosion>())
                        {
                            go.GetComponent<Kit_Explosion>().Explode(settings.isLocal, settings.ownerIsBot, settings.ownerID, settings.gameGunID);
                        }
                        if (go.GetComponent<Kit_FlashbangExplosion>())
                        {
                            go.GetComponent<Kit_FlashbangExplosion>().Explode(settings.isLocal, settings.ownerIsBot, settings.ownerID, settings.gameGunID);
                        }
                    }
                }

                if (settings.staysAliveAfterDeath)
                {
                    enabled = false;
                    if (settings.staysAliveAfterDeathTime > 0f)
                    {
                        Invoke("DestroyPooled", settings.staysAliveAfterDeathTime);
                    }
                }
                else
                {
                    main.objectPooling.DestroyInstantiateable(gameObject);
                }
            }

            private void OnTriggerEnter(Collider other)
            {
                if (onImpact == RigidbodyBulletOnImpact.Impact)
                {
                    //Check if we hit ourself
                    if (settings.localOwner)
                    {
                        if (other.transform.root == settings.localOwner.transform) return;
                    }

                    if (other.transform.GetComponent<Kit_PlayerDamageMultiplier>() && main.currentGameModeType == 2)
                    {
                        Kit_PlayerDamageMultiplier pdm = other.transform.GetComponent<Kit_PlayerDamageMultiplier>();
                        if (other.transform.root.GetComponent<Kit_PlayerBehaviour>())
                        {
                            if (settings.isLocal)
                            {
                                Kit_PlayerBehaviour hitPb = other.transform.root.GetComponent<Kit_PlayerBehaviour>();
                                //First check if we can actually damage that player
                                if ((settings.localOwner && main.currentPvPGameModeBehaviour.ArePlayersEnemies(settings.localOwner, hitPb)) || (!settings.localOwner && main.currentPvPGameModeBehaviour.ArePlayersEnemies(main, settings.ownerID, settings.ownerIsBot, hitPb, false)))
                                {
                                    //Check if he has spawn protection
                                    if (!hitPb.spawnProtection || hitPb.spawnProtection.CanTakeDamage(hitPb))
                                    {
                                        //Apply local damage
                                        hitPb.LocalDamage(settings.damage * pdm.damageMultiplier, settings.gameGunID, settings.shotFromPosition, settings.direction, settings.ragdollForce, transform.position, pdm.ragdollId, settings.ownerIsBot, settings.ownerID);
                                        if (!settings.ownerIsBot)
                                        {
                                            //Since we hit a player, show the hitmarker
                                            main.hud.DisplayHitmarker();
                                        }
                                    }
                                    else if (!settings.ownerIsBot)
                                    {
                                        //We hit a player but his spawn protection is active
                                        main.hud.DisplayHitmarkerSpawnProtected();
                                    }
                                }
                            }
                        }

                        if (settings.staysAliveAfterDeath)
                        {
                            enabled = false;
                            if (settings.staysAliveAfterDeathTime > 0f)
                            {
                                Invoke("DestroyPooled", settings.staysAliveAfterDeathTime);
                            }
                        }
                        else
                        {
                            main.objectPooling.DestroyInstantiateable(gameObject);
                        }

                        main.impactProcessor.ProcessImpact(main, transform.position, transform.forward, other.tag, other.transform);
                    }
                }
            }
            #endregion

            #region Custom Calls
            void DestroyPooled()
            {
                Kit_IngameMain main = FindObjectOfType<Kit_IngameMain>();
                main.objectPooling.DestroyInstantiateable(gameObject);
            }
            #endregion
        }
    }
}