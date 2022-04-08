using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    namespace World
    {
        /// <summary>
        /// This game object will be destroyed when the round / game mode resets
        /// If photonview is assigned, it will be network destroyed on that
        /// </summary>
        public class Kit_DestroyUponGameModeReset : MonoBehaviour
        {
            public PhotonView photonView;
        }
    }
}