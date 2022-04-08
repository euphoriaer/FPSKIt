using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// Use this class to implement your own bot controls
    /// </summary>
    public abstract class Kit_PlayerBotControlBase : ScriptableObject
    {
        /// <summary>
        /// This is called for everyone when this bot is created
        /// </summary>
        /// <param name="pb"></param>
        public abstract void InitializeControls(Kit_PlayerBehaviour pb);

        /// <summary>
        /// This is the Update of the player controls. Only called for the Master Client.
        /// </summary>
        /// <param name="pb"></param>
        public abstract void WriteToPlayerInput(Kit_PlayerBehaviour pb);

        /// <summary>
        /// Use this to synchronize input so that any player can take over the controls of the bots
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public abstract void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info);
    }
}