using MarsFPSKit;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    public class Kit_Explosion : MonoBehaviour
    {
        /// <summary>
        /// Explosion radius
        /// </summary>
        public float radius = 5f;
        /// <summary>
        /// Explosion layers
        /// </summary>
        public LayerMask layers;
        /// <summary>
        /// Layers of the linecast
        /// </summary>
        public LayerMask linecastLayers;
        /// <summary>
        /// Force that is applied to ragdolls
        /// </summary>
        public float ragdollForce;
        /// <summary>
        /// Damage at the closest
        /// </summary>
        public float maxDamage = 150f;
        /// <summary>
        /// Damage at the furthest
        /// </summary>
        public float minDamage = 50f;
        /// <summary>
        /// How long until it is destroyed?
        /// </summary>
        public float liveTime = 5f;
        /// <summary>
        /// Source that plays sounds
        /// </summary>
        public AudioSource source;
        /// <summary>
        /// Explosion particle system!
        /// </summary>
        public ParticleSystem system;
        /// <summary>
        /// Clips to play
        /// </summary>
        public AudioClip[] clips;

        /// <summary>
        /// How much do we shake (max amount)
        /// </summary>
        public float shakeAmount;
        /// <summary>
        /// How long do we shake?
        /// </summary>
        public float shakeDuration;
        /// <summary>
        /// How far do we shake?
        /// </summary>
        public float shakeDistance = 10f;

        void Start()
        {
            source.clip = clips[Random.Range(0, clips.Length)];
            source.Play();
            system.Play(true);
            Collider[] affectedByExplosion = Physics.OverlapSphere(transform.position, radius, layers.value, QueryTriggerInteraction.Collide);
            for (int i = 0; i < affectedByExplosion.Length; i++)
            {
                RaycastHit hit;


                bool inWay = Physics.Linecast(transform.position, affectedByExplosion[i].transform.position, out hit, linecastLayers, QueryTriggerInteraction.Ignore);

                if (inWay && hit.collider == affectedByExplosion[i]) inWay = false;

                if (!inWay)
                {             
                    if (affectedByExplosion[i].GetComponent<Rigidbody>())
                    {
                        if (affectedByExplosion[i].GetComponent<Kit_ExplosionRigidbody>())
                        {
                            Kit_ExplosionRigidbody body = affectedByExplosion[i].GetComponent<Kit_ExplosionRigidbody>();
                            StartCoroutine(ApplyExplosionForceNetworked(body, ragdollForce, transform.position, radius));
                        }
                        else
                        {
                            affectedByExplosion[i].GetComponent<Rigidbody>().AddExplosionForce(ragdollForce, transform.position, radius);
                        }
                    }
                }
            }

            if (shakeAmount > 0f && shakeDuration > 0f)
            {
                Kit_IngameMain main = FindObjectOfType<Kit_IngameMain>();
                if (main)
                {
                    float dist = Vector3.Distance(main.mainCamera.transform.position, transform.position);
                    if (dist < shakeDistance)
                    {
                        main.cameraShake.ShakeCamera(Mathf.Lerp(shakeAmount, 0f, dist / shakeDistance), Mathf.Lerp(shakeDuration, 0f, dist / shakeDistance));
                    }
                }
            }

        }

        public IEnumerator ApplyExplosionForceNetworked(Kit_ExplosionRigidbody body, float explosionForce, Vector3 explosionPosition, float explosionRadius)
        {
            float time = 0f;
            while (body && body.body.isKinematic && time < 1f)
            {
                time += Time.deltaTime;
                yield return null;
            }

            if (body && !body.body.isKinematic)
            {
                body.body.AddExplosionForce(explosionForce, explosionPosition, explosionRadius);
            }
        }

        public void Explode(bool doDamage, bool botShot, int idWhoShot, int gunID)
        {
            Kit_IngameMain main = FindObjectOfType<Kit_IngameMain>();

            Collider[] affectedByExplosion = Physics.OverlapSphere(transform.position, radius, layers.value, QueryTriggerInteraction.Collide);
            for (int i = 0; i < affectedByExplosion.Length; i++)
            {
                if (doDamage)
                {
                    if (affectedByExplosion[i].GetComponent<Kit_PlayerDamageMultiplier>())
                    {
                        Kit_PlayerDamageMultiplier adm = affectedByExplosion[i].GetComponent<Kit_PlayerDamageMultiplier>();
                        if (affectedByExplosion[i].transform.root.GetComponent<Kit_PlayerBehaviour>())
                        {
                            Kit_PlayerBehaviour player = affectedByExplosion[i].transform.root.GetComponent<Kit_PlayerBehaviour>();
                            if (!Physics.Linecast(transform.position, player.playerCameraTransform.position, linecastLayers))
                            {
                                if ((!main.currentPvPGameModeBehaviour && player.id == idWhoShot && player.isBot == botShot) || (main.currentPvPGameModeBehaviour && main.currentPvPGameModeBehaviour.ArePlayersEnemies(main, idWhoShot, botShot, player, true)))
                                {
                                    player.LocalDamage(Mathf.SmoothStep(maxDamage, minDamage, Vector3.Distance(transform.position, adm.transform.position) / radius), gunID, transform.position, adm.transform.position - transform.position, ragdollForce, transform.position, adm.ragdollId, botShot, idWhoShot);

                                    if (!botShot)
                                    {
                                        PhotonNetwork.RaiseEvent(Kit_EventIDs.hitMarkerEvent, null, new Photon.Realtime.RaiseEventOptions { TargetActors = new int[1] { idWhoShot } }, new ExitGames.Client.Photon.SendOptions { Reliability = false });
                                    }
                                }
                            }
                        }
                    }
                    else if (affectedByExplosion[i].GetComponentInParent<IKitDamageable>() != null)
                    {
                        if (affectedByExplosion[i].GetComponentInParent<IKitDamageable>().LocalDamage(Mathf.SmoothStep(maxDamage, minDamage, Vector3.Distance(transform.position, affectedByExplosion[i].transform.position) / radius), gunID, transform.position, affectedByExplosion[i].transform.position - transform.position, ragdollForce, transform.position, botShot, idWhoShot))
                        {

                            if (!botShot)
                            {
                                PhotonNetwork.RaiseEvent(Kit_EventIDs.hitMarkerEvent, null, new Photon.Realtime.RaiseEventOptions { TargetActors = new int[1] { idWhoShot } }, new ExitGames.Client.Photon.SendOptions { Reliability = false });
                            }
                        }
                    }
                }
            }

            Destroy(gameObject, liveTime);
        }

        public void Explode(bool doDamage, bool botShot, int idWhoShot, string cause)
        {
            Kit_IngameMain main = FindObjectOfType<Kit_IngameMain>();

            Collider[] affectedByExplosion = Physics.OverlapSphere(transform.position, radius, layers.value, QueryTriggerInteraction.Collide);
            for (int i = 0; i < affectedByExplosion.Length; i++)
            {
                if (doDamage)
                {
                    if (affectedByExplosion[i].GetComponent<Kit_PlayerDamageMultiplier>())
                    {
                        Kit_PlayerDamageMultiplier adm = affectedByExplosion[i].GetComponent<Kit_PlayerDamageMultiplier>();
                        if (affectedByExplosion[i].transform.root.GetComponent<Kit_PlayerBehaviour>())
                        {
                            Kit_PlayerBehaviour player = affectedByExplosion[i].transform.root.GetComponent<Kit_PlayerBehaviour>();
                            if (!Physics.Linecast(transform.position, player.playerCameraTransform.position, linecastLayers))
                            {
                                if ((!main.currentPvPGameModeBehaviour && player.id == idWhoShot && player.isBot == botShot) || (main.currentPvPGameModeBehaviour && main.currentPvPGameModeBehaviour.ArePlayersEnemies(main, idWhoShot, botShot, player, true)))
                                {
                                    player.LocalDamage(Mathf.SmoothStep(maxDamage, minDamage, Vector3.Distance(transform.position, adm.transform.position) / radius), cause, transform.position, adm.transform.position - transform.position, ragdollForce, transform.position, adm.ragdollId, botShot, idWhoShot);

                                    if (!botShot)
                                    {
                                        PhotonNetwork.RaiseEvent(Kit_EventIDs.hitMarkerEvent, null, new Photon.Realtime.RaiseEventOptions { TargetActors = new int[1] { idWhoShot } }, new ExitGames.Client.Photon.SendOptions { Reliability = false });
                                    }
                                }
                            }
                        }
                    }
                    else if (affectedByExplosion[i].GetComponentInParent<IKitDamageable>() != null)
                    {
                        if (affectedByExplosion[i].GetComponentInParent<IKitDamageable>().LocalDamage(Mathf.SmoothStep(maxDamage, minDamage, Vector3.Distance(transform.position, affectedByExplosion[i].transform.position) / radius), 0, transform.position, affectedByExplosion[i].transform.position - transform.position, ragdollForce, transform.position, botShot, idWhoShot))
                        {

                            if (!botShot)
                            {
                                PhotonNetwork.RaiseEvent(Kit_EventIDs.hitMarkerEvent, null, new Photon.Realtime.RaiseEventOptions { TargetActors = new int[1] { idWhoShot } }, new ExitGames.Client.Photon.SendOptions { Reliability = false });
                            }
                        }
                    }
                }
            }

            Destroy(gameObject, liveTime);
        }
    }
}