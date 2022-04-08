using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// Use this to implement your own voice chat
    /// </summary>
    public abstract class Kit_VoiceChatBase : MonoBehaviour
    {
        public abstract void Setup(Kit_IngameMain main);

        /// <summary>
        /// Called when the player has joined a team or switched
        /// </summary>
        /// <param name="team"></param>
        public abstract void JoinedTeam(int team);
    }
}
