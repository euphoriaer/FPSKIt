using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// Implements assist management
    /// </summary>
    public abstract class Kit_AssistManagerBase : ScriptableObject
    {
        /// <summary>
        /// Called on start to set up the manager
        /// </summary>
        /// <param name="main"></param>
        public abstract void OnStart(Kit_IngameMain main);

        /// <summary>
        /// Called when a player is damaged
        /// </summary>
        /// <param name="who"></param>
        /// <param name="damaged"></param>
        public abstract void PlayerDamaged(Kit_IngameMain main, bool botShot, int shotId, Kit_PlayerBehaviour damagedPlayer, float dmg);

        /// <summary>
        /// Called when a player is killed
        /// </summary>
        /// <param name="botKilled"></param>
        /// <param name="idKilled"></param>
        /// <param name="botKiller"></param>
        /// <param name="idKiller"></param>
        public abstract void PlayerKilled(Kit_IngameMain main, bool botKiller, int idKiller, Kit_PlayerBehaviour killedPlayer);

        /// <summary>
        /// Photon Event Relay
        /// </summary>
        /// <param name="main"></param>
        /// <param name="evCode"></param>
        /// <param name="content"></param>
        /// <param name="senderId"></param>
        public abstract void OnPhotonEvent(Kit_IngameMain main, byte evCode, object content, int senderId);
    }
}