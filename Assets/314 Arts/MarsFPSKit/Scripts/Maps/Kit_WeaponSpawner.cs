using MarsFPSKit.Weapons;
using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    [System.Serializable]
    public class WeaponToSpawn
    {
        /// <summary>
        /// The ID of the weapon to spawn
        /// </summary>
        public int weaponID = 0;
        /// <summary>
        /// The attachments that this weapon will spawn with
        /// </summary>
        public int[] attachmentsOfThisWeapon = new int[1];

        /// <summary>
        /// The amount of bullets this weapon will spawn with
        /// </summary>
        public int bulletsLeft = 30;
        /// <summary>
        /// The amount of bullets left to reload this weapon will spawn with
        /// </summary>
        public int bulletsLeftToReload = 60;
    }

    public enum WeaponSpawnType { Once, RespawnAfterTaken, RespawnAfterTime }

    /// <summary>
    /// This script will spawn weapons if attached to an object with a photonview
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    public class Kit_WeaponSpawner : MonoBehaviourPunCallbacks, IPunObservable
    {
        /// <summary>
        /// Reference to main.
        /// </summary>
        public Kit_IngameMain main;

        /// <summary>
        /// Drop prefab
        /// </summary>
        public GameObject dropPrefab;

        /// <summary>
        /// List of weapons that could spawn here
        /// </summary>
        public WeaponToSpawn[] weaponsToSpawn = new WeaponToSpawn[0];

        /// <summary>
        /// Respawn type of this weapon.
        /// </summary>
        public WeaponSpawnType spawnType = WeaponSpawnType.Once;

        /// <summary>
        /// If <see cref="spawnType"/> is set  to <see cref="WeaponSpawnType.RespawnAfterTaken"/> or <see cref="WeaponSpawnType.RespawnAfterTime"/>, this is used
        /// </summary>
        public float respawnTime = 10f;

        /// <summary>
        /// At which time will this weapon be respawned next
        /// </summary>
        private float nextRespawnTime;

        /// <summary>
        /// Was this weapon spawned already?
        /// </summary>
        private bool wasWeaponSpawned;

        [HideInInspector]
        public Kit_DropBehaviour currentlySpawnedWeapon;

        void Start()
        {
            if (weaponsToSpawn.Length <= 0)
            {
                Debug.LogError("Weapon spawner (" + this + ") has no weapons assigned", this);
                return;
            }
            if (PhotonNetwork.IsMasterClient)
            {
                if (!currentlySpawnedWeapon && !wasWeaponSpawned)
                {
                    //Spawn Random weapon
                    SpawnWeapon(Random.Range(0, weaponsToSpawn.Length));

                    //If we only want to spawn once, set bool
                    if (spawnType == WeaponSpawnType.Once)
                    {
                        wasWeaponSpawned = true;
                    }
                    else if (spawnType == WeaponSpawnType.RespawnAfterTime)
                    {
                        nextRespawnTime = (float)PhotonNetwork.Time + respawnTime;
                    }
                    else if (spawnType == WeaponSpawnType.RespawnAfterTaken)
                    {
                        //Set time
                        nextRespawnTime = (float)PhotonNetwork.Time + respawnTime;
                    }
                }
            }
        }

        void Update()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (weaponsToSpawn.Length > 0)
                {
                    if (spawnType == WeaponSpawnType.RespawnAfterTime)
                    {
                        if (PhotonNetwork.Time > nextRespawnTime)
                        {
                            //Destroy old
                            if (currentlySpawnedWeapon)
                            {
                                PhotonNetwork.Destroy(currentlySpawnedWeapon.gameObject);
                            }

                            //Spawn new
                            SpawnWeapon(Random.Range(0, weaponsToSpawn.Length));

                            //Set time
                            nextRespawnTime = (float)PhotonNetwork.Time + respawnTime;
                        }
                    }
                    else if (spawnType == WeaponSpawnType.RespawnAfterTaken)
                    {
                        if (currentlySpawnedWeapon)
                        {
                            //Set time
                            nextRespawnTime = (float)PhotonNetwork.Time + respawnTime;
                        }
                        else
                        {
                            if (PhotonNetwork.Time > nextRespawnTime)
                            {
                                //Spawn new
                                SpawnWeapon(Random.Range(0, weaponsToSpawn.Length));

                                //Set time
                                nextRespawnTime = (float)PhotonNetwork.Time + respawnTime;
                            }
                        }
                    }
                }
            }
        }

        void SpawnWeapon(int id)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (main.gameInformation.allWeapons[weaponsToSpawn[id].weaponID].dropPrefab)
                {
                    object[] instData = new object[5 + weaponsToSpawn[id].attachmentsOfThisWeapon.Length];
                    //ID
                    instData[0] = weaponsToSpawn[id].weaponID;
                    //Bullets left
                    instData[1] = weaponsToSpawn[id].bulletsLeft;
                    //Bullets Left To Reload
                    instData[2] = weaponsToSpawn[id].bulletsLeftToReload;
                    //Attachments length
                    instData[3] = weaponsToSpawn[id].attachmentsOfThisWeapon.Length;
                    for (int i = 0; i < weaponsToSpawn[id].attachmentsOfThisWeapon.Length; i++)
                    {
                        instData[4 + i] = weaponsToSpawn[id].attachmentsOfThisWeapon[i];
                    }
                    instData[4 + weaponsToSpawn[id].attachmentsOfThisWeapon.Length] = photonView.ViewID;
                    //Instantiate
                    PhotonNetwork.InstantiateRoomObject(dropPrefab.name, transform.position, transform.rotation, 0, instData);
                }
            }
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(wasWeaponSpawned);
                stream.SendNext(nextRespawnTime);
            }
            else
            {
                //Receive
                wasWeaponSpawned = (bool)stream.ReceiveNext();
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
                //Destroy if necessary
                if (currentlySpawnedWeapon)
                {
                    PhotonNetwork.Destroy(currentlySpawnedWeapon.gameObject);
                }

                //Spawn Random weapon
                SpawnWeapon(Random.Range(0, weaponsToSpawn.Length));

                //If we only want to spawn once, set bool
                if (spawnType == WeaponSpawnType.Once)
                {
                    wasWeaponSpawned = true;
                }
                else if (spawnType == WeaponSpawnType.RespawnAfterTime)
                {
                    nextRespawnTime = (float)PhotonNetwork.Time + respawnTime;
                }
                else if (spawnType == WeaponSpawnType.RespawnAfterTaken)
                {
                    //Set time
                    nextRespawnTime = (float)PhotonNetwork.Time + respawnTime;
                }
            }
        }
    }
}