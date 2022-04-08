using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MarsFPSKit
{
    public class Kit_VotingDefault : Kit_VotingBase, IPunObservable
    {
        /// <summary>
        /// Reference to main
        /// </summary>
        public Kit_IngameMain main;

        public float timer = 30f;

        void Start()
        {
            //Find main
            main = FindObjectOfType<Kit_IngameMain>();
            //Check if enough time is left
            if (main.timer > timer)
            {
                //Assign it
                main.currentVoting = this;
                //Reset our own vote
                Hashtable table = PhotonNetwork.LocalPlayer.CustomProperties;
                if (table["vote"] != null)
                {
                    table["vote"] = -1;
                    PhotonNetwork.LocalPlayer.SetCustomProperties(table);
                }
                myVote = -1;

                //Get setup properties
                object[] voteProperties = photonView.InstantiationData;
                //Get vote
                int voting = (int)voteProperties[0];
                votingOn = (VotingOn)voting;
                int arg = (int)voteProperties[1];
                argument = arg;
                int started = (int)voteProperties[2];
                //Assign starting player
                voteStartedBy = Kit_PhotonPlayerExtensions.Find(started);
                //If we started this vote, automatically vote yes.
                if (voteStartedBy == PhotonNetwork.LocalPlayer)
                {
                    VoteYes();
                }

                //Redraw
                if (main.votingMenu)
                {
                    main.votingMenu.RedrawVotingUI(this);
                }
            }
            else
            {
                VoteFailed();
            }
        }

        void Update()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                //Decrease the timer
                if (timer > 0)
                {
                    timer -= Time.deltaTime;
                    if (timer <= 0)
                    {
                        TimeRanOut();
                    }
                }
            }

            //Check if we did not vote yet!
            if (myVote == -1)
            {
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    VoteYes();
                }
                else if (Input.GetKeyDown(KeyCode.F2))
                {
                    VoteNo();
                }
            }
        }

        void OnDestroy()
        {
            //Reset our own vote
            Hashtable table = PhotonNetwork.LocalPlayer.CustomProperties;
            if (table["vote"] != null)
            {
                table["vote"] = -1;
                PhotonNetwork.LocalPlayer.SetCustomProperties(table);
            }

            //Tell the voting behaviour
            if (main.votingMenu)
            {
                main.votingMenu.VoteEnded(this);
            }
        }

        public override int GetYesVotes()
        {
            int yesVotes = 0;
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                Hashtable cp = PhotonNetwork.PlayerList[i].CustomProperties;
                //Check if vote is set
                if (cp["vote"] != null)
                {
                    //Get the vote
                    int vote = (int)cp["vote"];
                    //Check if it is a yes vote
                    if (vote == 1)
                    {
                        yesVotes++;
                    }
                }
            }

            return yesVotes;
        }

        public override int GetNoVotes()
        {
            int noVotes = 0;
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                Hashtable cp = PhotonNetwork.PlayerList[i].CustomProperties;
                //Check if vote is set
                if (cp["vote"] != null)
                {
                    //Get the vote
                    int vote = (int)cp["vote"];
                    //Check if it is a no vote
                    if (vote == 0)
                    {
                        noVotes++;
                    }
                }
            }

            return noVotes;
        }

        public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
        {
            //A player's vote has possibly changed! Redraw!
            if (main && main.votingMenu)
            {
                main.votingMenu.RedrawVotingUI(this);
            }
            if (PhotonNetwork.IsMasterClient)
            {
                RecalculateVotes();
            }
        }

        //Check if enough votes were gained
        void RecalculateVotes()
        {
            //Have all players voted?
            if ((GetYesVotes() + GetNoVotes()) >= PhotonNetwork.PlayerList.Length)
            {
                //More yes than no votes?
                if (GetYesVotes() > GetNoVotes())
                {
                    VoteSucceeded();
                }
                else
                {
                    VoteFailed();
                }
            }
        }

        void TimeRanOut()
        {
            //More than 50% of all players need to vote and more yes votes than no votes!
            int totalVotes = GetYesVotes() + GetNoVotes();
            if (totalVotes > (PhotonNetwork.PlayerList.Length / 2))
            {
                if (GetYesVotes() > GetNoVotes())
                {
                    VoteSucceeded();
                }
                else
                {
                    VoteFailed();
                }
            }
            else
            {
                VoteFailed();
            }
        }

        void VoteSucceeded()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (votingOn == VotingOn.Kick)
                {
                    //Get that player
                    Photon.Realtime.Player toKick = Kit_PhotonPlayerExtensions.Find(argument);
                    //Kick that player
                    PhotonNetwork.CloseConnection(toKick);
                }
                else if (votingOn == VotingOn.Map)
                {
                    //Switch map
                    main.SwitchMap(argument);
                }
                else if (votingOn == VotingOn.GameMode)
                {
                    //Switch game mode
                    main.SwitchGameMode(argument);
                }

                //Destroy this
                PhotonNetwork.Destroy(gameObject);
            }
        }

        void VoteFailed()
        {
            //Destroy this
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                //Synchronize timer
                stream.SendNext(timer);
            }
            else
            {
                //Set timer
                timer = (float)stream.ReceiveNext();
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            //If the player who started the vote left, abort!
            if (otherPlayer == voteStartedBy)
            {
                VoteFailed();
            }
            //If we are voting on this player, also abort
            if (votingOn == VotingOn.Kick && otherPlayer.ActorNumber == argument)
            {
                VoteFailed();
            }
        }
    }
}