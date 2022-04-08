using Photon.Pun;
using System;
using UnityEngine;

namespace MarsFPSKit
{
    public class VitalsRuntimeData
    {
        public float hitPoints;

        /// <summary>
        /// For displaying the bloody screen
        /// </summary>
        public float hitAlpha;
    }

    /// <summary>
    /// Implements a basic vitals
    /// </summary>
    [CreateAssetMenu(menuName = "MarsFPSKit/Vitals/Simple")]
    public class Kit_SimpleVitals : Kit_VitalsBase
    {
        public float bloodyScreenTime = 3f;
        /// <summary>
        /// First person hit reactions will be applied if this is set to true
        /// </summary>
        public bool hitReactionEnabled = true;
        /// <summary>
        /// Intensity of the hit reactions
        /// </summary>
        public float hitReactionsIntensity = 1.2f;
        /// <summary>
        /// How fast will we recover from the hit reactions?
        /// </summary>
        public float hitReactionsReturnSpeed = 5f;

        /// <summary>
        /// ID for fall death CAT
        /// </summary>
        public int fallDamageSoundCatID;
        /// <summary>
        /// ID for out of map death CAT
        /// </summary>
        public int outOfMapSoundCatID;

        public override void ApplyHeal(Kit_PlayerBehaviour pb, float heal)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(VitalsRuntimeData))
            {
                VitalsRuntimeData vrd = pb.customVitalsData as VitalsRuntimeData;
                vrd.hitPoints = Mathf.Clamp(vrd.hitPoints + heal, 0, 100f);
            }
        }

        public override void ApplyDamage(Kit_PlayerBehaviour pb, float dmg, bool botShot, int idWhoShot, int gunID, Vector3 shotFrom)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(VitalsRuntimeData))
            {
                VitalsRuntimeData vrd = pb.customVitalsData as VitalsRuntimeData;
                //Check if we can take damage
                if (!pb.spawnProtection || pb.spawnProtection.CanTakeDamage(pb))
                {
                    //Check for hitpoints
                    if (vrd.hitPoints > 0)
                    {
                        //Apply damage
                        vrd.hitPoints -= dmg;
                        //Hit reactions
                        if (hitReactionEnabled)
                        {
                            Vector3 dir = (pb.playerCameraTransform.InverseTransformDirection(Vector3.Cross(pb.playerCameraTransform.forward, pb.transform.position - shotFrom))).normalized * hitReactionsIntensity;
                            dir *= Mathf.Clamp(dmg / 30f, 0.3f, 1f);

                            Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(pb.playerCameraHitReactionsTransform, dir, 0.1f));

                            Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(pb.weaponsHitReactions, dir * 2f, 0.1f));
                        }
                        //Play voice
                        if (pb.voiceManager)
                        {
                            pb.voiceManager.DamageTaken(pb, Kit_VoiceManagerBase.DamageType.Projectile);
                        }
                        //Set damage effect
                        vrd.hitAlpha = 2f;
                        //Check for death
                        if (vrd.hitPoints <= 0)
                        {
                            //Call the die function on pb
                            pb.Die(botShot, idWhoShot, gunID);
                        }
                    }
                }
            }
        }

        public override void ApplyDamage(Kit_PlayerBehaviour pb, float dmg, bool botShot, int idWhoShot, string deathCause, Vector3 shotFrom)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(VitalsRuntimeData))
            {
                VitalsRuntimeData vrd = pb.customVitalsData as VitalsRuntimeData;
                //Check if we can take damage
                if (!pb.spawnProtection || pb.spawnProtection.CanTakeDamage(pb))
                {
                    //Check for hitpoints
                    if (vrd.hitPoints > 0)
                    {
                        //Apply damage
                        vrd.hitPoints -= dmg;
                        //Hit reactions
                        if (hitReactionEnabled)
                        {
                            Vector3 dir = (pb.playerCameraTransform.InverseTransformDirection(Vector3.Cross(pb.playerCameraTransform.forward, pb.transform.position - shotFrom))).normalized * hitReactionsIntensity;
                            dir *= Mathf.Clamp(dmg / 30f, 0.3f, 1f);

                            Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(pb.playerCameraHitReactionsTransform, dir, 0.1f));

                            Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(pb.weaponsHitReactions, dir * 2f, 0.1f));
                        }
                        //Play voice
                        if (pb.voiceManager)
                        {
                            pb.voiceManager.DamageTaken(pb, Kit_VoiceManagerBase.DamageType.Projectile);
                        }
                        //Set damage effect
                        vrd.hitAlpha = 2f;
                        //Check for death
                        if (vrd.hitPoints <= 0)
                        {
                            //Call the die function on pb
                            pb.Die(botShot, idWhoShot, deathCause);
                        }
                    }
                }
            }
        }

        public override void ApplyFallDamage(Kit_PlayerBehaviour pb, float dmg)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(VitalsRuntimeData))
            {
                VitalsRuntimeData vrd = pb.customVitalsData as VitalsRuntimeData;
                //Check for hitpoints
                if (vrd.hitPoints > 0)
                {
                    pb.deathSoundCategory = fallDamageSoundCatID;
                    if (pb.voiceManager)
                    {
                        pb.deathSoundID = pb.voiceManager.GetDeathSoundID(pb, pb.deathSoundCategory);
                    }
                    //Apply damage
                    vrd.hitPoints -= dmg;
                    //Set damage effect
                    vrd.hitAlpha = 2f;
                    //Play voice
                    if (pb.voiceManager)
                    {
                        pb.voiceManager.DamageTaken(pb, Kit_VoiceManagerBase.DamageType.Other);
                    }
                    //Check for death
                    if (vrd.hitPoints <= 0)
                    {
                        //Reset player force
                        pb.ragdollForce = 0f;
                        //Call the die function on pb
                        pb.Die(-2);
                    }
                }
            }
        }

        public override void ApplyEnvironmentalDamage(Kit_PlayerBehaviour pb, float dmg, int deathSoundCategory)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(VitalsRuntimeData))
            {
                VitalsRuntimeData vrd = pb.customVitalsData as VitalsRuntimeData;
                //Check for hitpoints
                if (vrd.hitPoints > 0)
                {
                    pb.deathSoundCategory = deathSoundCategory;
                    if (pb.voiceManager)
                    {
                        pb.deathSoundID = pb.voiceManager.GetDeathSoundID(pb, pb.deathSoundCategory);
                    }
                    //Apply damage
                    vrd.hitPoints -= dmg;
                    //Set damage effect
                    vrd.hitAlpha = 2f;
                    //Play voice
                    if (pb.voiceManager)
                    {
                        pb.voiceManager.DamageTaken(pb, Kit_VoiceManagerBase.DamageType.Other);
                    }
                    //Check for death
                    if (vrd.hitPoints <= 0)
                    {
                        //Reset player force
                        pb.ragdollForce = 0f;
                        //Call the die function on pb
                        pb.Die(-1);
                    }
                }
            }
        }

        public override void Suicide(Kit_PlayerBehaviour pb)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(VitalsRuntimeData))
            {
                //Reset player force
                pb.ragdollForce = 0f;
                //Call the die function on pb
                pb.Die(-3);
            }
        }

        public override void CustomUpdate(Kit_PlayerBehaviour pb)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(VitalsRuntimeData))
            {
                VitalsRuntimeData vrd = pb.customVitalsData as VitalsRuntimeData;
                //Clamp
                vrd.hitPoints = Mathf.Clamp(vrd.hitPoints, 0f, 100f);
                //Decrease hit alpha
                if (vrd.hitAlpha > 0)
                {
                    vrd.hitAlpha -= (Time.deltaTime * 2) / bloodyScreenTime;
                }

                if (pb.isFirstPersonActive)
                {
                    //Update hud
                    pb.main.hud.DisplayHealth(vrd.hitPoints);
                    pb.main.hud.DisplayHurtState(vrd.hitAlpha);
                }
                //Return hit reactions
                if (hitReactionEnabled)
                {
                    pb.playerCameraHitReactionsTransform.localRotation = Quaternion.Slerp(pb.playerCameraHitReactionsTransform.localRotation, Quaternion.identity, Time.deltaTime * hitReactionsReturnSpeed);
                    pb.weaponsHitReactions.localRotation = Quaternion.Slerp(pb.weaponsHitReactions.localRotation, Quaternion.identity, Time.deltaTime * hitReactionsReturnSpeed);
                }

                //Check if we are lower than death threshold
                if (pb.transform.position.y <= pb.main.mapDeathThreshold)
                {
                    pb.deathSoundCategory = outOfMapSoundCatID;
                    if (pb.voiceManager)
                    {
                        pb.deathSoundID = pb.voiceManager.GetDeathSoundID(pb, pb.deathSoundCategory);
                    }
                    pb.Die(-1);
                }
            }
        }

        public override void Setup(Kit_PlayerBehaviour pb)
        {
            //Create runtime data
            VitalsRuntimeData vrd = new VitalsRuntimeData();
            //Set standard values
            vrd.hitPoints = 100f;
            //Assign it
            pb.customVitalsData = vrd;
        }

        public override void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info)
        {
            if (pb.customVitalsData != null && pb.customVitalsData.GetType() == typeof(VitalsRuntimeData))
            {
                VitalsRuntimeData vrd = pb.customVitalsData as VitalsRuntimeData;

                if (stream.IsWriting)
                {
                    stream.SendNext(vrd.hitAlpha);
                    stream.SendNext(vrd.hitPoints);
                }
                else
                {
                    vrd.hitAlpha = (float)stream.ReceiveNext();
                    vrd.hitPoints = (float)stream.ReceiveNext();
                }
            }
            else
            {
                if (stream.IsWriting)
                {
                    stream.SendNext(0f);
                    stream.SendNext(100f);
                }
                else
                {
                    stream.ReceiveNext();
                    stream.ReceiveNext();
                }
            }
        }
    }
}
