using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// Base to implement your own voting behaviour
    /// </summary>
    public abstract class Kit_VotingBase : MonoBehaviourPunCallbacks
    {
        public enum VotingOn { Kick = 0, Map = 1, GameMode = 2 }

        public VotingOn votingOn;
        /// <summary>
        /// ID of player to kick, Map ID or Game Mode ID, depending on <see cref="votingOn"/>
        /// </summary>
        public int argument;

        /// <summary>
        /// Our own vote. -1 = none, 0 = no, 1 = yes
        /// </summary>
        public int myVote = -1;

        public Photon.Realtime.Player voteStartedBy;

        /// <summary>
        /// Called to vote yes on the current vote
        /// </summary>
        public void VoteYes()
        {
            //Set our vote to yes!
            Hashtable table = PhotonNetwork.LocalPlayer.CustomProperties;
            if (table["vote"] != null)
            {
                table["vote"] = 1;
                PhotonNetwork.LocalPlayer.SetCustomProperties(table);
            }
            myVote = 1;
        }

        /// <summary>
        /// Called to vote no on the current vote
        /// </summary>
        public void VoteNo()
        {
            //Set our vote to no!
            Hashtable table = PhotonNetwork.LocalPlayer.CustomProperties;
            if (table["vote"] != null)
            {
                table["vote"] = 0;
                PhotonNetwork.LocalPlayer.SetCustomProperties(table);
            }
            myVote = 0;
        }

        /// <summary>
        /// Returns the yes votes for the current vote
        /// </summary>
        /// <returns></returns>
        public abstract int GetYesVotes();

        /// <summary>
        /// Returns the no votes for the current vote
        /// </summary>
        /// <returns></returns>
        public abstract int GetNoVotes();
    }
}
