using ExitGames.Client.Photon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using TMPro;

namespace MarsFPSKit
{
    public class Kit_VotingUIDefault : Kit_VotingUIBase
    {
        public enum MenuState { Category, Player, Map, GameMode }
        private MenuState currentMenuState = MenuState.Category;
        /// <summary>
        /// Id of this menu
        /// </summary>
        public int menuIdMainPage;
        /// <summary>
        /// Sub page
        /// </summary>
        public int menuIdSubPage;

        [Header("Vote Start")]
        /// <summary>
        /// The prefab for the selection menu (Players, Maps, Game Modes)
        /// </summary>
        public GameObject voteMenuSelectionPrefab;
        /// <summary>
        /// Where the prefab is going to be parented to
        /// </summary>
        public RectTransform voteMenuSelectionGO;
        /// <summary>
        /// Currently active entries
        /// </summary>
        public List<GameObject> voteMenuSelectionEntries = new List<GameObject>();
        /// <summary>
        /// The back button in the selection list
        /// </summary>
        public GameObject voteMenuSelectionBack;

        /// <summary>
        /// How many seconds need to pass until we can start another votE?
        /// </summary>
        public float votingCooldown = 60f;

        /// <summary>
        /// When have we started a vote for the last time?
        /// </summary>
        private float lastVote;

        [Header("Mid Round Vote")]
        public GameObject mrvRoot;
        /// <summary>
        /// Displays the username who started
        /// </summary>
        public TextMeshProUGUI voteStartedBy;
        /// <summary>
        /// Displays what is being voted on
        /// </summary>
        public TextMeshProUGUI voteDescription;
        /// <summary>
        /// Displays our own vote OR the controls
        /// </summary>
        public TextMeshProUGUI voteOwn;
        /// <summary>
        /// Displays the amount of yes votes
        /// </summary>
        public TextMeshProUGUI yesVotes;
        /// <summary>
        /// Displays the amount of no votes
        /// </summary>
        public TextMeshProUGUI noVotes;

        public override void OpenVotingMenu()
        {
            if (main.currentPvPGameModeBehaviour.CanStartVote(main) && PhotonNetwork.PlayerList.Length > 1)
            {
                if (Time.time > lastVote)
                {
                    if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
                    {
                        if (main.SwitchMenu(menuIdMainPage))
                        {
                            //Set state
                            currentMenuState = MenuState.Category;
                        }
                    }
                }
                else
                {
                    main.DisplayMessage("You need to wait " + (lastVote - Time.time).ToString("F0") + " seconds before you can start another vote!");
                    BackToPauseMenu();
                }
            }
            else
            {
                main.DisplayMessage("A vote can currently not be started!");
                BackToPauseMenu();
            }
        }

        public override void CloseVotingMenu()
        {

        }

        public void KickPlayer()
        {
            if (PhotonNetwork.PlayerList.Length > 1)
            {
                //Clear list
                for (int i = 0; i < voteMenuSelectionEntries.Count; i++)
                {
                    Destroy(voteMenuSelectionEntries[i]);
                }
                voteMenuSelectionEntries = new List<GameObject>();

                //Loop through all players and list them
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    //Check if its not us
                    if (PhotonNetwork.PlayerList[i] != PhotonNetwork.LocalPlayer)
                    {
                        //Instantiate
                        GameObject go = Instantiate(voteMenuSelectionPrefab, voteMenuSelectionGO, false);
                        //Get Entry
                        Kit_VotingSelectionEntry entry = go.GetComponent<Kit_VotingSelectionEntry>();
                        int current = i; //This is necessary, otherwise 'i' would change.
                                         //Set name
                        entry.text.text = PhotonNetwork.PlayerList[i].NickName;
                        //Add delegate
                        entry.btn.onClick.AddListener(delegate { StartVotePlayer(PhotonNetwork.PlayerList[current]); });

                        //Add to list
                        voteMenuSelectionEntries.Add(go);
                    }
                }

                //Move back button to the lower part
                voteMenuSelectionBack.transform.SetAsLastSibling();

                LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);

                if (main.SwitchMenu(menuIdSubPage))
                {
                    //Set menu state
                    currentMenuState = MenuState.Player;
                }
            }
            else
            {
                BackToCategory();
                main.DisplayMessage("Only you are in this room!");
            }
        }

        public void ChangeMap()
        {
            if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
            {
                if (main.currentPvPGameModeBehaviour.traditionalMaps.Length > 1)
                {
                    //Clear list
                    for (int i = 0; i < voteMenuSelectionEntries.Count; i++)
                    {
                        Destroy(voteMenuSelectionEntries[i]);
                    }
                    voteMenuSelectionEntries = new List<GameObject>();

                    int currentMap = main.gameInformation.GetCurrentLevel();

                    //Loop through all maps and list them
                    for (int i = 0; i < main.currentPvPGameModeBehaviour.traditionalMaps.Length; i++)
                    {
                        //Check if its not the current map
                        if (i != currentMap)
                        {
                            //Instantiate
                            GameObject go = Instantiate(voteMenuSelectionPrefab, voteMenuSelectionGO, false);
                            //Get Entry
                            Kit_VotingSelectionEntry entry = go.GetComponent<Kit_VotingSelectionEntry>();
                            int current = i; //This is necessary, otherwise 'i' would change.
                                             //Set name
                            entry.text.text = main.currentPvPGameModeBehaviour.traditionalMaps[i].mapName;
                            //Add delegate
                            entry.btn.onClick.AddListener(delegate { StartVoteMap(current); });
                            //Add to list
                            voteMenuSelectionEntries.Add(go);
                        }
                    }

                    //Move back button to the lower part
                    voteMenuSelectionBack.transform.SetAsLastSibling();

                    if (main.SwitchMenu(menuIdSubPage))
                    {
                        //Set state
                        currentMenuState = MenuState.Map;
                    }
                }
                else
                {
                    main.DisplayMessage("Only one map is in this game!");
                    BackToCategory();
                }
            }
            else if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
            {
                if (main.currentPvPGameModeBehaviour.lobbyMaps.Length > 1)
                {
                    //Clear list
                    for (int i = 0; i < voteMenuSelectionEntries.Count; i++)
                    {
                        Destroy(voteMenuSelectionEntries[i]);
                    }
                    voteMenuSelectionEntries = new List<GameObject>();

                    int currentMap = main.gameInformation.GetCurrentLevel();

                    //Loop through all maps and list them
                    for (int i = 0; i < main.currentPvPGameModeBehaviour.lobbyMaps.Length; i++)
                    {
                        //Check if its not the current map
                        if (i != currentMap)
                        {
                            //Instantiate
                            GameObject go = Instantiate(voteMenuSelectionPrefab, voteMenuSelectionGO, false);
                            //Get Entry
                            Kit_VotingSelectionEntry entry = go.GetComponent<Kit_VotingSelectionEntry>();
                            int current = i; //This is necessary, otherwise 'i' would change.
                                             //Set name
                            entry.text.text = main.currentPvPGameModeBehaviour.lobbyMaps[i].mapName;
                            //Add delegate
                            entry.btn.onClick.AddListener(delegate { StartVoteMap(current); });
                            //Add to list
                            voteMenuSelectionEntries.Add(go);
                        }
                    }

                    if (main.SwitchMenu(menuIdSubPage))
                    {
                        //Move back button to the lower part
                        voteMenuSelectionBack.transform.SetAsLastSibling();

                        //Set state
                        currentMenuState = MenuState.Map;
                    }
                }
                else
                {
                    main.DisplayMessage("Only one map is in this game!");
                    BackToCategory();
                }
            }
        }

        public void ChangeGameMode()
        {
            if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
            {
                if (main.gameInformation.allPvpGameModes.Length > 1)
                {
                    //Clear list
                    for (int i = 0; i < voteMenuSelectionEntries.Count; i++)
                    {
                        Destroy(voteMenuSelectionEntries[i]);
                    }
                    voteMenuSelectionEntries = new List<GameObject>();

                    //Loop through all game modes and list them
                    for (int i = 0; i < main.gameInformation.allPvpGameModes.Length; i++)
                    {
                        //Check if its not the current map
                        if (i != main.currentGameMode)
                        {
                            //Instantiate
                            GameObject go = Instantiate(voteMenuSelectionPrefab, voteMenuSelectionGO, false);
                            //Get Entry
                            Kit_VotingSelectionEntry entry = go.GetComponent<Kit_VotingSelectionEntry>();
                            int current = i; //This is necessary, otherwise 'i' would change.
                                             //Set name
                            entry.text.text = main.gameInformation.allPvpGameModes[i].gameModeName;
                            //Add delegate
                            entry.btn.onClick.AddListener(delegate { StartVoteGameMode(current); });

                            //Add to list
                            voteMenuSelectionEntries.Add(go);
                        }
                    }

                    if (main.SwitchMenu(menuIdSubPage))
                    {
                        //Move back button to the lower part
                        voteMenuSelectionBack.transform.SetAsLastSibling();

                        //Set state
                        currentMenuState = MenuState.GameMode;
                    }
                }
                else
                {
                    main.DisplayMessage("This game only has one game mode!");
                }
            }
        }

        public void StartVotePlayer(Photon.Realtime.Player player)
        {
            //Send Event
            if (player != null)
            {
                //Set timer
                lastVote = Time.time + votingCooldown;
                //Tell master client we want to start a vote
                byte evCode = Kit_EventIDs.startVote; //Event 4 = start vote
                //Create a table that holds our vote information
                Hashtable voteInformation = new Hashtable(2);
                //Type
                voteInformation[(byte)0] = (byte)0;
                //ID
                voteInformation[(byte)1] = player.ActorNumber;
                PhotonNetwork.RaiseEvent(evCode, voteInformation, new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient }, SendOptions.SendReliable);
            }

            BackToPauseMenu();
        }

        public void StartVoteMap(int map)
        {
            //Set timer
            lastVote = Time.time + votingCooldown;
            //Tell master client we want to start a vote
            byte evCode = Kit_EventIDs.startVote; //Event 4 = start vote
                                                  //Create a table that holds our vote information
            Hashtable voteInformation = new Hashtable(2);
            //Type
            voteInformation[(byte)0] = (byte)1;
            //ID
            voteInformation[(byte)1] = map;
            PhotonNetwork.RaiseEvent(evCode, voteInformation, new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient }, SendOptions.SendReliable);
            BackToPauseMenu();
        }

        public void StartVoteGameMode(int gameMode)
        {
            //Set timer
            lastVote = Time.time + votingCooldown;
            //Tell master client we want to start a vote
            byte evCode = Kit_EventIDs.startVote; //Event 4 = start vote
                                                  //Create a table that holds our vote information
            Hashtable voteInformation = new Hashtable(2);
            //Type
            voteInformation[(byte)0] = (byte)2;
            //ID
            voteInformation[(byte)1] = gameMode;
            PhotonNetwork.RaiseEvent(evCode, voteInformation, new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient }, SendOptions.SendReliable);
            BackToPauseMenu();
        }

        public void BackToCategory()
        {
            if (main.SwitchMenu(menuIdMainPage))
            {
                //Set state
                currentMenuState = MenuState.Category;
            }
        }

        public void BackToPauseMenu()
        {
            main.SwitchMenu(main.pauseMenu.pauseMenuId);
        }

        public override void RedrawVotingUI(Kit_VotingBase voting)
        {
            if (voting)
            {
                if (!mrvRoot.activeSelf)
                    mrvRoot.SetActive(true);
                if (voteStartedBy && voting.voteStartedBy != null)
                {
                    //Set who started it
                    voteStartedBy.text = voting.voteStartedBy.NickName;
                }
                if (voteDescription)
                {
                    //Update description
                    if (voting.votingOn == Kit_VotingBase.VotingOn.Kick)
                    {
                        Photon.Realtime.Player toKick = Kit_PhotonPlayerExtensions.Find(voting.argument);
                        if (toKick != null)
                        {
                            voteDescription.text = "Kick player: " + toKick.NickName;
                        }
                        else
                        {
                            voteDescription.text = "Kick player: ";
                        }
                    }
                    else if (voting.votingOn == Kit_VotingBase.VotingOn.Map)
                    {
                        if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
                        {
                            voteDescription.text = "Switch map to: " + main.currentPvPGameModeBehaviour.traditionalMaps[voting.argument].mapName;
                        }
                        else if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
                        {
                            voteDescription.text = "Switch map to: " + main.currentPvPGameModeBehaviour.lobbyMaps[voting.argument].mapName;
                        }
                    }
                    else if (voting.votingOn == Kit_VotingBase.VotingOn.GameMode)
                    {
                        voteDescription.text = "Switch Game Mode to: " + main.gameInformation.allPvpGameModes[voting.argument].gameModeName;
                    }
                }
                if (yesVotes)
                {
                    //Yes Votes
                    yesVotes.text = voting.GetYesVotes().ToString();
                }
                if (noVotes)
                {
                    //No votes
                    noVotes.text = voting.GetNoVotes().ToString();
                }
                if (voteOwn)
                {
                    //Own vote
                    if (voting.myVote == -1)
                    {
                        voteOwn.text = "F1 <color=#00ff00ff>YES</color> F2 <color=#ff0000ff>NO</color>";
                    }
                    else if (voting.myVote == 0)
                    {
                        voteOwn.text = "You voted <color=#ff0000ff>NO</color>";
                    }
                    else if (voting.myVote == 1)
                    {
                        voteOwn.text = "You voted <color=#00ff00ff>YES</color>";
                    }
                }
            }
        }

        public override void VoteEnded(Kit_VotingBase voting)
        {
            //Hide the UI
            mrvRoot.SetActive(false);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (currentMenuState == MenuState.Player)
            {
                //Redraw
                KickPlayer();
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (currentMenuState == MenuState.Player)
            {
                if (PhotonNetwork.PlayerList.Length > 1)
                {
                    //Redraw
                    KickPlayer();
                }
                else
                {
                    BackToCategory();
                }
            }
        }
    }
}