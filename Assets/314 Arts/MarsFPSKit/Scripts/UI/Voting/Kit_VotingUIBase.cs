using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// Base to implement your own voting UI
    /// </summary>
    public abstract class Kit_VotingUIBase : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// Access to the kit's heart
        /// </summary>
        public Kit_IngameMain main;

        /// <summary>
        /// Called by <see cref="Kit_IngameMain"/> to initiate a vote
        /// </summary>
        public abstract void OpenVotingMenu();

        /// <summary>
        /// Called by <see cref="Kit_IngameMain"/> when the menu needs to be closed
        /// </summary>
        public abstract void CloseVotingMenu();

        /// <summary>
        /// Redraws the voting ui with the given voting base
        /// </summary>
        /// <param name="voting"></param>
        public abstract void RedrawVotingUI(Kit_VotingBase voting);

        /// <summary>
        /// Called be the voting behaviour when it ends
        /// </summary>
        /// <param name="voting"></param>
        public abstract void VoteEnded(Kit_VotingBase voting);
    }
}
