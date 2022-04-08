using Photon.Pun;
using System;
using UnityEngine;

namespace MarsFPSKit
{
    public class SpawnProtectionRuntimeData
    {
        public float timeLeft;
    }

    [CreateAssetMenu(menuName = "MarsFPSKit/Spawn Protection/Default")]
    public class Kit_SpawnProtectionDefault : Kit_SpawnProtectionBase
    {
        /// <summary>
        /// How long does the spawn protection last?
        /// </summary>
        public float spawnProtectionTime = 10f;

        public override void CustomStart(Kit_PlayerBehaviour pb)
        {
            //Assign data
            SpawnProtectionRuntimeData sprd = new SpawnProtectionRuntimeData();
            //Set default values
            sprd.timeLeft = spawnProtectionTime;
            //Assign to PB
            pb.customSpawnProtectionData = sprd;
        }

        public override bool CanTakeDamage(Kit_PlayerBehaviour pb)
        {
            //Check for data
            if (pb.customSpawnProtectionData != null && pb.customSpawnProtectionData.GetType() == typeof(SpawnProtectionRuntimeData))
            {
                SpawnProtectionRuntimeData sprd = pb.customSpawnProtectionData as SpawnProtectionRuntimeData;
                //Check if there is time left
                if (sprd.timeLeft > 0.1f)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return true;
        }

        public override void CustomUpdate(Kit_PlayerBehaviour pb)
        {
            //Check for data
            if (pb.customSpawnProtectionData != null && pb.customSpawnProtectionData.GetType() == typeof(SpawnProtectionRuntimeData))
            {
                SpawnProtectionRuntimeData sprd = pb.customSpawnProtectionData as SpawnProtectionRuntimeData;
                //Decrease time
                if (sprd.timeLeft > 0f)
                {
                    //It can only expire if we are playing
                    if (pb.main.currentPvPGameModeBehaviour.CanControlPlayer(pb.main))
                    {
                        sprd.timeLeft -= Time.deltaTime;
                    }
                }
                if (!pb.isBot)
                {
                    //Update hud
                    pb.main.hud.UpdateSpawnProtection(sprd.timeLeft > 0.1f, sprd.timeLeft);
                }
            }
        }

        public override void GunFired(Kit_PlayerBehaviour pb)
        {
            //Check for data
            if (pb.customSpawnProtectionData != null && pb.customSpawnProtectionData.GetType() == typeof(SpawnProtectionRuntimeData))
            {
                //It can only expire if we are playing
                if (pb.main.currentPvPGameModeBehaviour.CanControlPlayer(pb.main))
                {
                    SpawnProtectionRuntimeData sprd = pb.customSpawnProtectionData as SpawnProtectionRuntimeData;
                    //This stops our spawn protection
                    sprd.timeLeft = 0f;
                }
            }
        }

        public override void PlayerMoved(Kit_PlayerBehaviour pb)
        {
            //Check for data
            if (pb.customSpawnProtectionData != null && pb.customSpawnProtectionData.GetType() == typeof(SpawnProtectionRuntimeData))
            {
                //It can only expire if we are playing
                if (pb.main.currentPvPGameModeBehaviour.CanControlPlayer(pb.main))
                {
                    SpawnProtectionRuntimeData sprd = pb.customSpawnProtectionData as SpawnProtectionRuntimeData;
                    //This stops our spawn protection
                    sprd.timeLeft = 0f;
                }
            }
        }

        public override void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (pb.customSpawnProtectionData != null && pb.customSpawnProtectionData.GetType() == typeof(SpawnProtectionRuntimeData))
                {
                    SpawnProtectionRuntimeData sprd = pb.customSpawnProtectionData as SpawnProtectionRuntimeData;
                    //Send velocity
                    stream.SendNext(sprd.timeLeft);
                }
                else
                {
                    //Send dummies
                    //Send time
                    stream.SendNext(0f);
                }
            }
            else if (stream.IsReading) //To avoid errors before data arrives
            {
                //Check if the object is correct
                if (pb.customSpawnProtectionData == null || pb.customSpawnProtectionData.GetType() != typeof(SpawnProtectionRuntimeData))
                {
                    pb.customSpawnProtectionData = new SpawnProtectionRuntimeData();
                }
                SpawnProtectionRuntimeData sprd = pb.customSpawnProtectionData as SpawnProtectionRuntimeData;
                //Read time left
                sprd.timeLeft = (float)stream.ReceiveNext();
            }
        }
    }
}