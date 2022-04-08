using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    public enum AmmoSpawnType { Once, RespawnAfterTaken }

    /// <summary>
    /// Use this to place ammo packs in your levels
    /// </summary>
    public class Kit_AmmoSpawner : MonoBehaviourPunCallbacks, IPunObservable
    {
        /// <summary>
        /// The ammo prefab
        /// </summary>
        public GameObject ammoPrefab;

        /// <summary>
        /// Amount of clips this pickup will spawn
        /// </summary>
        public int amountOfClipsToPickup = 3;

        /// <summary>
        /// Respawn type of this ammo pickup
        /// </summary>
        public AmmoSpawnType spawnType;

        /// <summary>
        /// If this is set to respawn, it will repsawn after this (in s)
        /// </summary>
        public float respawnTime = 10f;

        /// <summary>
        /// Was the ammo pack spawned already?
        /// </summary>
        private bool wasAmmoSpawned;

        /// <summary>
        /// When will it be repsawned the next time
        /// </summary>
        private float nextRespawnTime;

        [HideInInspector]
        public Kit_AmmoPickup currentlySpawnedAmmo;

        void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (!currentlySpawnedAmmo)
                {
                    if (spawnType == AmmoSpawnType.RespawnAfterTaken || spawnType == AmmoSpawnType.Once && !wasAmmoSpawned)
                    {
                        if (spawnType == AmmoSpawnType.RespawnAfterTaken)
                        {
                            //Set time
                            nextRespawnTime = Time.time + respawnTime;
                            //Spawn
                            SpawnAmmoPickup();
                        }
                        else if (spawnType == AmmoSpawnType.Once)
                        {
                            //Tell it to not spawn again !
                            wasAmmoSpawned = true;
                            //And spawn
                            SpawnAmmoPickup();
                        }
                    }
                }
            }
        }

        void Update()
        {
            if (spawnType == AmmoSpawnType.RespawnAfterTaken)
            {
                if (currentlySpawnedAmmo)
                {
                    //Set time
                    nextRespawnTime = Time.time + respawnTime;
                }
                else
                {
                    //None is spawned
                    //Check if we should repsawn?
                    if (Time.time > nextRespawnTime)
                    {
                        //Respawn
                        nextRespawnTime = Time.time + respawnTime;
                        SpawnAmmoPickup();
                    }
                }
            }
        }

        void SpawnAmmoPickup()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                object[] instData = new object[2];
                //Set clips
                instData[0] = amountOfClipsToPickup;
                //Set view ID so it gets assigned
                instData[1] = photonView.ViewID;
                //Instantiate
                PhotonNetwork.Instantiate(ammoPrefab.name, transform.position, transform.rotation, 0, instData);
            }
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(wasAmmoSpawned);
                stream.SendNext(nextRespawnTime);
            }
            else
            {
                //Receive
                wasAmmoSpawned = (bool)stream.ReceiveNext();
                nextRespawnTime = (float)stream.ReceiveNext();
            }
        }

        /// <summary>
        /// Should be called by game mode scripts when game was started in the middle!
        /// </summary>
        public void GameModeBeginMiddle()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (currentlySpawnedAmmo)
                {
                    PhotonNetwork.Destroy(currentlySpawnedAmmo.gameObject);

                    //Reset
                    currentlySpawnedAmmo = null;
                    wasAmmoSpawned = false;

                    //Just do the same again
                    Start();
                }
            }
        }
    }
}
