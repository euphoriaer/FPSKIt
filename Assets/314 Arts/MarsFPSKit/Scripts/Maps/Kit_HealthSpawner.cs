using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    public enum HealthSpawnType { Once, RespawnAfterTaken }

    /// <summary>
    /// Use this to place health packs in your levels
    /// </summary>
    public class Kit_HealthSpawner : MonoBehaviourPunCallbacks, IPunObservable
    {
        /// <summary>
        /// The health prefab
        /// </summary>
        public GameObject healthPrefab;

        /// <summary>
        /// Amount of clips this pickup will spawn
        /// </summary>
        public float healthRestored = 30;

        /// <summary>
        /// Respawn type of this health pickup
        /// </summary>
        public HealthSpawnType spawnType;

        /// <summary>
        /// If this is set to respawn, it will repsawn after this (in s)
        /// </summary>
        public float respawnTime = 10f;

        /// <summary>
        /// Was the health pack spawned already?
        /// </summary>
        private bool wasHealthSpawned;

        /// <summary>
        /// When will it be repsawned the next time
        /// </summary>
        private float nextRespawnTime;

        [HideInInspector]
        public Kit_HealthPickup currentlySpawnedHealth;

        void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (!currentlySpawnedHealth)
                {
                    if (spawnType == HealthSpawnType.RespawnAfterTaken || spawnType == HealthSpawnType.Once && !wasHealthSpawned)
                    {
                        if (spawnType == HealthSpawnType.RespawnAfterTaken)
                        {
                            //Set time
                            nextRespawnTime = Time.time + respawnTime;
                            //Spawn
                            SpawnHealthPickup();
                        }
                        else if (spawnType == HealthSpawnType.Once)
                        {
                            //Tell it to not spawn again !
                            wasHealthSpawned = true;
                            //And spawn
                            SpawnHealthPickup();
                        }
                    }
                }
            }
        }

        void Update()
        {
            if (spawnType == HealthSpawnType.RespawnAfterTaken)
            {
                if (currentlySpawnedHealth)
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
                        SpawnHealthPickup();
                    }
                }
            }
        }

        void SpawnHealthPickup()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                object[] instData = new object[2];
                //Set clips
                instData[0] = healthRestored;
                //Set view ID so it gets assigned
                instData[1] = photonView.ViewID;
                //Instantiate
                PhotonNetwork.Instantiate(healthPrefab.name, transform.position, transform.rotation, 0, instData);
            }
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(wasHealthSpawned);
                stream.SendNext(nextRespawnTime);
            }
            else
            {
                //Receive
                wasHealthSpawned = (bool)stream.ReceiveNext();
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
                if (currentlySpawnedHealth)
                {
                    PhotonNetwork.Destroy(currentlySpawnedHealth.gameObject);

                    //Reset
                    currentlySpawnedHealth = null;
                    wasHealthSpawned = false;

                    //Just do the same again
                    Start();
                }
            }
        }
    }
}
