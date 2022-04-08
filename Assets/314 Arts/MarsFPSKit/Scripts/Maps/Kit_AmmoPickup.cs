using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// Upon entering, this will be picked up
    /// </summary>
    public class Kit_AmmoPickup : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// How many clips will be picked up
        /// </summary>
        public int amountOfClipsToPickup = 2;
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
                amountOfClipsToPickup = (int)instData[0];

                if (instData.Length > 1)
                {
                    //1 = Spawner ID
                    int spawnerID = (int)instData[1];
                    //Find
                    PhotonView spawner = PhotonView.Find(spawnerID);
                    //Assign
                    spawner.GetComponent<Kit_AmmoSpawner>().currentlySpawnedAmmo = this;
                }
            }
        }

        void Update()
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