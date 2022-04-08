using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// Upon entering, this will be picked up
    /// </summary>
    public class Kit_HealthPickup : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// How much health will be restored? 
        /// </summary>
        public float healthRestored = 30f;
        /// <summary>
        /// Rigidbody
        /// </summary>
        public Rigidbody body;
        /// <summary>
        /// Root object of renderer to hide before destroyed
        /// </summary>
        public GameObject renderRoot;

        void Start()
        {
            if (photonView)
            {
                object[] instData = photonView.InstantiationData;
                //0 = amount
                healthRestored = (float)instData[0];

                if (instData.Length > 1)
                {
                    //1 = Spawner ID
                    int spawnerID = (int)instData[1];
                    //Find
                    PhotonView spawner = PhotonView.Find(spawnerID);
                    //Assign
                    spawner.GetComponent<Kit_HealthSpawner>().currentlySpawnedHealth = this;
                }
            }
        }

        void Update()
        {
            if (body)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    body.isKinematic = false;
                }
                else
                {
                    body.isKinematic = true;
                }
            }
        }

        [PunRPC]
        public void PickedUp()
        {
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(photonView);
            }
        }
    }
}