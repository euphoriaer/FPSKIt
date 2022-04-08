using System.Linq;
using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        public class ModernBulletSetupData
        {
            /// <summary>
            /// Damage of this bullet
            /// </summary>
            public float damage = 30f;
            /// <summary>
            /// Gravity multiplier
            /// </summary>
            public float gravityMultiplier = 1f;
            /// <summary>
            /// Speed of this bullet
            /// </summary>
            public float speed = 200f;
            /// <summary>
            /// Should we try to penetrate?
            /// </summary>
            public bool penetrationEnabled = true;
            /// <summary>
            /// How many times can this bullet penetrate an object
            /// </summary>
            public int penetrationValue = 4;
            /// <summary>
            /// Direction of the bullet (spread already applied.)
            /// </summary>
            public Vector3 direction;
            /// <summary>
            /// Force to apply to ragdoll (if hit)
            /// </summary>
            public float ragdollForce = 500f;
            /// <summary>
            /// ID of the gun this bullet was fired with
            /// </summary>
            public int gameGunID;
            /// <summary>
            /// Lifetime of the bullet (in s)
            /// </summary>
            public float bulletLifeTime = 10f;
            /// <summary>
            /// Should the bullet parent itself to its hit thing and stay alive? Useful for things like nails and arrodata.
            /// </summary>
            public bool staysAliveAfterDeath = false;
            /// <summary>
            /// If bullet stays alive after death, this is how long
            /// </summary>
            public float staysAliveAfterDeathTime = 10f;
            /// <summary>
            /// Mask of things we can hit
            /// </summary>
            public LayerMask mask;
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
        }

        public class Kit_ModernBullet : Kit_BulletBase
        {
            /// <summary>
            /// Visible renderer for bullet
            /// </summary>
            public GameObject bulletRenderer;

            #region Runtime
            /// <summary>
            /// Settings received from weapon script
            /// </summary>
            private ModernBulletSetupData settings;
            /// <summary>
            /// Main.
            /// </summary>
            private Kit_IngameMain main;
            /// <summary>
            /// Velocity of the bullet
            /// </summary>
            private Vector3 velocity;
            /// <summary>
            /// Next position of the bullet
            /// </summary>
            private Vector3 newPosition;
            /// <summary>
            /// Previous position of the bullet
            /// </summary>
            private Vector3 oldPosition;
            /// <summary>
            /// Temp direction
            /// </summary>
            private Vector3 tempDir;
            /// <summary>
            /// Temp distance
            /// </summary>
            private float tempDistance;
            /// <summary>
            /// Hit of raycast
            /// </summary>
            public RaycastHit tempHit;
            /// <summary>
            /// Was something penetrated?
            /// </summary>
            private bool tempPenetrated;
            /// <summary>
            /// Should bullet be destroyed after hit?
            /// </summary>
            private bool tempDestroyNext;
            /// <summary>
            /// Last hit ID for fronthit
            /// </summary>
            private int lastHitID;
            /// <summary>
            /// Last hit ID for backhit
            /// </summary>
            private int lastHitIDBack;
            /// <summary>
            /// Since when does the bullet exist?
            /// </summary>
            private float bulletExistTime;
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
                ModernBulletSetupData mbsd = new ModernBulletSetupData();
                //Setup data
                mbsd.damage = data.baseDamage;
                mbsd.gravityMultiplier = data.bulletGravityMultiplier;
                mbsd.speed = data.bulletSpeed;
                mbsd.penetrationEnabled = data.bulletsPenetrationEnabled;
                mbsd.penetrationValue = data.bulletsPenetrationForce + 1;
                mbsd.direction = dir;
                mbsd.ragdollForce = data.ragdollForce;
                mbsd.gameGunID = ws.gameGunID;
                mbsd.bulletLifeTime = data.bulletLifeTime;
                mbsd.staysAliveAfterDeath = data.bulletStaysAliveAfterDeath;
                mbsd.staysAliveAfterDeathTime = data.bulletStaysAliveAfterDeathTime;
                mbsd.mask = pb.weaponHitLayers;
                mbsd.shotFromPosition = pb.transform.position;
                mbsd.isLocal = pb.photonView.IsMine;
                mbsd.localOwner = pb;
                mbsd.ownerIsBot = pb.isBot;
                if (pb.isBot)
                    mbsd.ownerID = pb.botId;
                else
                    mbsd.ownerID = pb.photonView.Owner.ActorNumber;

                //Set main
                main = newMain;
                //Get settings
                settings = mbsd;
                //Set position default
                newPosition = transform.position;
                oldPosition = transform.position;
                velocity = mbsd.speed * transform.forward;

                bulletExistTime = 0f;
                tempDestroyNext = false;
                tempDir = Vector3.zero;
                tempDistance = 0f;
                tempPenetrated = false;
                lastHitID = 0;
                lastHitIDBack = 0;

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

                enabled = true;
            }

            #region Unity Calls
            void Update()
            {
                //Advance
                newPosition += (velocity + settings.direction + (Physics.gravity * settings.gravityMultiplier)) * Time.deltaTime;
                //Calculate direction
                tempDir = newPosition - oldPosition;
                //Calculate travelled distance
                tempDistance = tempDir.magnitude;
                //Divide
                tempDir /= tempDistance;
                //Check if we actually travelled
                if (tempDistance > 0f)
                {
                    RaycastHit[] hits = Physics.RaycastAll(oldPosition, tempDir, tempDistance, settings.mask);
                    hits = hits.OrderBy(h => h.distance).ToArray();
                    for (int i = 0; i < hits.Length; i++)
                    {
                        //Check if we hit ourselves
                        if (!settings.localOwner || (settings.localOwner && hits[i].transform.root != settings.localOwner.transform.root))
                        {
                            //Check if we hit last object again
                            if (hits[i].collider.GetInstanceID() != lastHitID)
                            {
                                //Set new position to hit position
                                newPosition = hits[i].point;
                                //Call function
                                OnHit(hits[i], tempDir);
                                break;
                            }
                        }
                    }

                    if (tempPenetrated)
                    {
                        //There must be something back hit
                        if (Physics.Raycast(newPosition, -tempDir, out tempHit, tempDistance, settings.mask))
                        {
                            //Check if we hit ourselves
                            if (!settings.localOwner || (settings.localOwner && tempHit.transform.root != settings.localOwner.transform.root))
                            {
                                //Check if we hit same object again
                                if (lastHitIDBack != tempHit.collider.GetInstanceID())
                                {
                                    OnHit(tempHit);
                                }
                            }
                        }
                    }
                }

                //Check if bullet penetration is over
                if (settings.penetrationValue <= 0)
                {
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

                if (tempDestroyNext)
                {
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

            void LateUpdate()
            {
                //Set position
                oldPosition = transform.position;
                //Move
                transform.position = newPosition;
            }
            #endregion

            #region Custom Calls
            void OnHit(RaycastHit hit, Vector3 dir)
            {
                if (settings.penetrationEnabled)
                {
                    //Check if we can penetrate
                    Kit_PenetrateableObject penetration = hit.collider.GetComponent<Kit_PenetrateableObject>();
                    if (penetration)
                    {
                        //Can we penetrate?
                        if (settings.penetrationValue >= penetration.cost)
                        {
                            //Subtract
                            settings.penetrationValue -= penetration.cost;
                            tempPenetrated = true;

                            tempDistance = (hit.point - newPosition).magnitude;

                            if (settings.penetrationValue <= 0)
                            {
                                if (settings.staysAliveAfterDeath)
                                {
                                    //Parent
                                    transform.SetParent(hit.collider.transform, true);
                                    transform.position = hit.point;
                                }
                            }
                        }
                        else
                        {
                            //We can't. Destroy next
                            tempDestroyNext = true;

                            if (settings.staysAliveAfterDeath)
                            {
                                //Parent
                                transform.SetParent(hit.collider.transform, true);
                                transform.position = hit.point;
                            }
                        }
                    }
                    else
                    {
                        //Nothing to penetrate. Destroy next
                        tempDestroyNext = true;

                        if (settings.staysAliveAfterDeath)
                        {
                            //Parent
                            transform.SetParent(hit.collider.transform, true);
                            transform.position = hit.point;
                        }
                    }
                }
                else
                {
                    //Nothing to penetrate. Destroy next
                    tempDestroyNext = true;

                    if (settings.staysAliveAfterDeath)
                    {
                        //Parent
                        transform.SetParent(hit.collider.transform, true);
                        transform.position = hit.point;
                    }
                }

                //Check if we hit a player and if we are playing pvp
                if (hit.transform.GetComponent<Kit_PlayerDamageMultiplier>() && main.currentGameModeType == 2)
                {
                    Kit_PlayerDamageMultiplier pdm = hit.transform.GetComponent<Kit_PlayerDamageMultiplier>();
                    if (hit.transform.root.GetComponent<Kit_PlayerBehaviour>())
                    {
                        if (settings.isLocal)
                        {
                            Kit_PlayerBehaviour hitPb = hit.transform.root.GetComponent<Kit_PlayerBehaviour>();
                            //First check if we can actually damage that player
                            if ((settings.localOwner && main.currentPvPGameModeBehaviour.ArePlayersEnemies(settings.localOwner, hitPb)) || (!settings.localOwner && main.currentPvPGameModeBehaviour.ArePlayersEnemies(main, settings.ownerID, settings.ownerIsBot, hitPb, false)))
                            {
                                //Check if he has spawn protection
                                if (!hitPb.spawnProtection || hitPb.spawnProtection.CanTakeDamage(hitPb))
                                {
                                    //Apply local damage, sample damage dropoff via distance
                                    hitPb.LocalDamage(settings.damage * pdm.damageMultiplier, settings.gameGunID, settings.shotFromPosition, settings.direction, settings.ragdollForce, hit.point, pdm.ragdollId, settings.ownerIsBot, settings.ownerID);
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
                        //Send to hit processor
                        main.impactProcessor.ProcessImpact(main, hit.point, hit.normal, hit.collider.tag, hit.transform);
                    }
                }
                else
                {
                    //Call
                    main.impactProcessor.ProcessImpact(main, hit.point, hit.normal, hit.collider.tag, hit.transform);

                    if (settings.isLocal)
                    {
                        if (hit.collider.GetComponentInParent<IKitDamageable>() != null)
                        {
                            if (hit.collider.GetComponentInParent<IKitDamageable>().LocalDamage(settings.damage, settings.gameGunID, settings.shotFromPosition, settings.direction, settings.ragdollForce, hit.point, settings.ownerIsBot, settings.ownerID))
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

                //Assign ID
                lastHitID = hit.collider.GetInstanceID();
            }

            void OnHit(RaycastHit hit)
            {
                //Call
                main.impactProcessor.ProcessImpact(main, hit.point, hit.normal, hit.collider.tag, hit.transform);

                //Assign ID
                lastHitIDBack = hit.collider.GetInstanceID();
            }

            void DestroyPooled()
            {
                Kit_IngameMain main = FindObjectOfType<Kit_IngameMain>();
                main.objectPooling.DestroyInstantiateable(gameObject);
            }
            #endregion
        }
    }
}