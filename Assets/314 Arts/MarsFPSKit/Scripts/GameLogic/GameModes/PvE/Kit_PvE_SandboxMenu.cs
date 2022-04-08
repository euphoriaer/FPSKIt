using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ExitGames.Client.Photon;
using UnityEngine.UI;

namespace MarsFPSKit
{
    namespace UI
    {
        public class Kit_PvE_SandboxMenu : Kit_MenuPveGameModeBase, IOnEventCallback
        {
            /// <summary>
            /// Menu that gets opened when we are a singleplayer game mode
            /// </summary>
            public int singleplayerMenuId;
            /// <summary>
            /// Menu that gets opened when we are a coop game mode
            /// </summary>
            public int coopMainMenuId;
            /// <summary>
            /// Hosting / Lobby screen
            /// </summary>
            public int coopHostScreenId = 1;
            /// <summary>
            /// Browsing screen id
            /// </summary>
            public int coopBrowserScreenId = 2;

            /// <summary>
            /// Displays the name of our selected map
            /// </summary>
            [Header("Singleplayer Settings")]
            public TextMeshProUGUI spMapName;
            /// <summary>
            /// Currently selected map
            /// </summary>
            private int spCurMap;

            /// <summary>
            /// Displays the name of our selected map
            /// </summary>
            [Header("Coop Settings")]
            public TextMeshProUGUI coopMapName;
            /// <summary>
            /// Currently selected map
            /// </summary>
            private int coopCurMap;
            /// <summary>
            /// Start button
            /// </summary>
            public Button coopStartButton;

            /// <summary>
            /// The "Content" object of the Scroll view, where playerEntriesPrefab will be instantiated
            /// </summary>
            [Header("Coop Players")]
            public RectTransform playerEntriesGo;
            /// <summary>
            /// The Player Entry prefab
            /// </summary>
            public GameObject playerEntriesPrefab;
            /// <summary>
            /// Currently active player entries - used for cleanup
            /// </summary>
            private List<GameObject> activePlayerEntries = new List<GameObject>();

            /// <summary>
            /// The "Content" object of the Scroll view, where entriesPrefab will be instantiated
            /// </summary>
            [Header("Coop Browser")]
            public RectTransform entriesGo;
            /// <summary>
            /// The Server Browser Entry prefab
            /// </summary>
            public GameObject entriesPrefab;
            /// <summary>
            /// Currently active server browser entries - used for cleanup
            /// </summary>
            private List<GameObject> activeEntries = new List<GameObject>();
            /// <summary>
            /// Cached list of Photon Rooms
            /// </summary>
            private Dictionary<string, RoomInfo> cachedRoomList;
            /// <summary>
            /// Were we in a room? (To fire event for leaving a room)
            /// </summary>
            private bool wasInRoom;

            #region Unity Calls
            void Awake()
            {
                cachedRoomList = new Dictionary<string, RoomInfo>();
            }
            #endregion

            public override void SetupMenu(Kit_MenuManager mm, int state, int id)
            {
                base.SetupMenu(mm, state, id);

                //Redraw to default values
                if (myCurrentState == 0)
                {
                    RedrawSingleplayerMenu();
                }
                else
                {
                    RedrawCoopMenu();
                }
            }

            public override void OpenMenu()
            {
                if (myCurrentState == 0)
                {
                    ChangeMenuButton(singleplayerMenuId);
                }
                else
                {
                    ChangeMenuButton(coopMainMenuId);
                }
            }

            #region Button Calls


            public void SingleplayerStart()
            {
                //Create a room with this game mode
                if (myCurrentState == 0)
                {
                    StartCoroutine(SingleplayerStartRoutine());
                }
            }

            IEnumerator SingleplayerStartRoutine()
            {
                if (PhotonNetwork.IsConnected)
                    PhotonNetwork.Disconnect();
                while (PhotonNetwork.IsConnected) yield return null;
                PhotonNetwork.OfflineMode = true;
                //Create room options
                RoomOptions options = new RoomOptions();
                //Assign settings
                options.IsVisible = true;
                options.IsOpen = true;
                //Player Limit
                options.MaxPlayers = 1;
                //Create a new hashtable
                options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
                //Map
                options.CustomRoomProperties.Add("map", spCurMap);
                //Game Mode
                options.CustomRoomProperties.Add("gameModeType", 0);
                //Game Mode
                options.CustomRoomProperties.Add("gameMode", myId);
                string[] customLobbyProperties = new string[3];
                customLobbyProperties[0] = "map";
                customLobbyProperties[1] = "gameModeType";
                customLobbyProperties[2] = "gameMode";
                options.CustomRoomPropertiesForLobby = customLobbyProperties;
                //Try to create a new room
                if (PhotonNetwork.CreateRoom(null, options, null))
                {

                }
                else
                {
                    //Display error message
                }
            }

            public void SingleplayerNextMap()
            {
                spCurMap++;

                if (spCurMap >= menuManager.game.allSingleplayerGameModes[myId].maps.Length)
                {
                    spCurMap = 0;
                }

                RedrawSingleplayerMenu();
            }

            public void SingleplayerPreviousMap()
            {
                spCurMap--;

                if (spCurMap < 0)
                {
                    spCurMap = menuManager.game.allSingleplayerGameModes[myId].maps.Length - 1;
                }

                RedrawSingleplayerMenu();
            }

            /// <summary>
            /// Creates a coop lobby
            /// </summary>
            public void CoopHostGame()
            {
                //Check if we are connected to the Photon Server
                if (PhotonNetwork.IsConnected)
                {
                    //Create room options
                    RoomOptions options = new RoomOptions();
                    //Assign settings
                    options.IsVisible = true;
                    options.IsOpen = true;
                    //Player Limit
                    options.MaxPlayers = menuManager.game.allCoopGameModes[myId].coopPlayerAmount;
                    //Create a new hashtable
                    options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
                    //Map
                    options.CustomRoomProperties.Add("map", coopCurMap);
                    //Game Mode
                    options.CustomRoomProperties.Add("gameModeType", 1);
                    //Game Mode
                    options.CustomRoomProperties.Add("gameMode", myId);
                    string[] customLobbyProperties = new string[3];
                    customLobbyProperties[0] = "map";
                    customLobbyProperties[1] = "gameModeType";
                    customLobbyProperties[2] = "gameMode";
                    options.CustomRoomPropertiesForLobby = customLobbyProperties;
                    PhotonNetwork.OfflineMode = false;
                    //Try to create a new room
                    if (PhotonNetwork.CreateRoom(Kit_GameSettings.userName + "'s game", options, null))
                    {

                    }
                    else
                    {
                        //Display error
                    }
                }
                else
                {
                    if (menuManager.regionScreen)
                    {
                        menuManager.regionScreen.GameStartedNotConnected();
                    }
                }
            }

            public void CoopStart()
            {
                if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
                {
                    //Deactivate all input
                    menuManager.eventSystem.enabled = false;
                    //Load the map
                    Kit_SceneSyncer.instance.LoadScene(menuManager.game.allSingleplayerGameModes[myId].maps[coopCurMap].sceneName);
                }
            }

            public void CoopLeaveLobby()
            {
                if (PhotonNetwork.InRoom && wasInRoom)
                {
                    //Just leave dat room
                    PhotonNetwork.LeaveRoom();
                }
            }

            public void CoopNextMap()
            {
                if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
                {
                    //Remove Event
                    PhotonNetwork.RaiseEvent(100, coopCurMap, new RaiseEventOptions { CachingOption = EventCaching.RemoveFromRoomCache, Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });

                    coopCurMap++;

                    if (coopCurMap >= menuManager.game.allSingleplayerGameModes[myId].maps.Length)
                    {
                        coopCurMap = 0;
                    }

                    RedrawCoopMenu();

                    //Send Event
                    PhotonNetwork.RaiseEvent(100, coopCurMap, new RaiseEventOptions { CachingOption = EventCaching.AddToRoomCacheGlobal, Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
                }
            }

            public void CoopPreviousMap()
            {
                if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
                {
                    //Remove Event
                    PhotonNetwork.RaiseEvent(100, coopCurMap, new RaiseEventOptions { CachingOption = EventCaching.RemoveFromRoomCache, Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });

                    coopCurMap--;

                    if (coopCurMap < 0)
                    {
                        coopCurMap = menuManager.game.allSingleplayerGameModes[myId].maps.Length - 1;
                    }

                    RedrawCoopMenu();

                    //Send Event
                    PhotonNetwork.RaiseEvent(100, coopCurMap, new RaiseEventOptions { CachingOption = EventCaching.AddToRoomCacheGlobal, Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
                }
            }
            #endregion

            #region UI
            private void RedrawSingleplayerMenu()
            {
                spMapName.text = menuManager.game.allSingleplayerGameModes[myId].maps[spCurMap].mapName;
            }

            private void RedrawCoopMenu()
            {
                coopMapName.text = menuManager.game.allCoopGameModes[myId].maps[coopCurMap].mapName;

                if (PhotonNetwork.InRoom)
                {
                    //Redraw players

                    //Clean Up
                    for (int i = 0; i < activePlayerEntries.Count; i++)
                    {
                        //Destroy
                        Destroy(activePlayerEntries[i]);
                    }
                    //Reset list
                    activePlayerEntries = new List<GameObject>();

                    for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                    {
                        //Instantiate entry
                        GameObject go = Instantiate(playerEntriesPrefab, playerEntriesGo) as GameObject;
                        //Set it up
                        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
                        //Display that mf's name
                        txt.text = PhotonNetwork.PlayerList[i].NickName;
                        //Add it to our active list so it will get cleaned up next time
                        activePlayerEntries.Add(go);
                    }
                }
            }

            private void RedrawBrowser()
            {
                //Clean Up
                for (int i = 0; i < activeEntries.Count; i++)
                {
                    //Destroy
                    Destroy(activeEntries[i]);
                }
                //Reset list
                activeEntries = new List<GameObject>();

                //Instantiate new List
                foreach (RoomInfo info in cachedRoomList.Values)
                {
                    int gameModeType = (int)info.CustomProperties["gameModeType"];
                    int gameMode = (int)info.CustomProperties["gameMode"];

                    //1 = Coop
                    if (gameModeType == 1)
                    {
                        //Check if game mode matches
                        if (gameMode == myId)
                        {
                            //Instantiate entry
                            GameObject go = Instantiate(entriesPrefab, entriesGo) as GameObject;
                            //Set it up
                            Kit_CoopBrowserEntry entry = go.GetComponent<Kit_CoopBrowserEntry>();
                            entry.Setup(this, info);
                            //This sets up the join function
                            entry.joinButton.onClick.AddListener(delegate { JoinRoom(info); });
                            //Add it to our active list so it will get cleaned up next time
                            activeEntries.Add(go);
                        }
                    }
                }
            }

            public void JoinRoom(RoomInfo room)
            {
                //Just join room
                PhotonNetwork.JoinRoom(room.Name);
            }
            #endregion


            #region Photon Calls
            //We just created a room
            public override void OnCreatedRoom()
            {
                //Our room is created and ready
                //Lets load the appropriate map
                //Get the hashtable
                ExitGames.Client.Photon.Hashtable table = PhotonNetwork.CurrentRoom.CustomProperties;
                if ((int)table["gameModeType"] == 0) //Singleplayer - load map - coop does it on button click
                {
                    if ((int)table["gameMode"] == myId)
                    {
                        //Get the correct map
                        int mapToLoad = (int)table["map"];
                        //Deactivate all input
                        menuManager.eventSystem.enabled = false;
                        //Load the map
                        Kit_SceneSyncer.instance.LoadScene(menuManager.game.allSingleplayerGameModes[myId].maps[mapToLoad].sceneName);
                    }
                }

                if ((int)table["gameModeType"] == 1) //COOP
                {
                    if ((int)table["gameMode"] == myId)
                    {
                        //Enable button for host
                        coopStartButton.enabled = true;

                        wasInRoom = true;
                    }
                }
            }

            public override void OnJoinedRoom()
            {
                //Get the hashtable
                ExitGames.Client.Photon.Hashtable table = PhotonNetwork.CurrentRoom.CustomProperties;
                if ((int)table["gameModeType"] == 1) //COOP
                {
                    if ((int)table["gameMode"] == myId)
                    {
                        //Go to host screen
                        ChangeMenuButton(coopHostScreenId);

                        //Redraw that mf
                        RedrawCoopMenu();

                        if (!PhotonNetwork.IsMasterClient)
                        {
                            //Disable the button
                            coopStartButton.enabled = false;
                        }
                        else
                        {
                            coopStartButton.enabled = true;
                        }

                        wasInRoom = true;
                    }
                }
            }

            public override void OnMasterClientSwitched(Player newMasterClient)
            {
                //Get the hashtable
                ExitGames.Client.Photon.Hashtable table = PhotonNetwork.CurrentRoom.CustomProperties;
                if ((int)table["gameModeType"] == 1) //COOP
                {
                    if ((int)table["gameMode"] == myId)
                    {
                        if (newMasterClient.IsLocal)
                        {
                            //Enable the button
                            coopStartButton.enabled = true;
                        }
                    }
                }
            }

            public override void OnPlayerEnteredRoom(Player newPlayer)
            {
                if (wasInRoom)
                {
                    RedrawCoopMenu();
                }
            }

            public override void OnPlayerLeftRoom(Player otherPlayer)
            {
                if (wasInRoom)
                {
                    RedrawCoopMenu();
                }
            }

            public override void OnLeftRoom()
            {
                if (wasInRoom)
                {
                    //Go back to main screen
                    ChangeMenuButton(coopMainMenuId);
                    //Reset
                    wasInRoom = false;
                }
            }

            public override void OnRoomListUpdate(List<RoomInfo> roomList)
            {
                UpdateCachedRoomList(roomList);
                RedrawBrowser();
            }

            void IOnEventCallback.OnEvent(EventData photonEvent)
            {
                if (photonEvent.Code == 100)
                {
                    if (!PhotonNetwork.IsMasterClient)
                    {
                        //Set map
                        coopCurMap = (int)photonEvent.CustomData;
                        //Redraw
                        RedrawCoopMenu();
                    }
                }
            }
            #endregion

            private void UpdateCachedRoomList(List<RoomInfo> roomList)
            {
                foreach (RoomInfo info in roomList)
                {
                    // Remove room from cached room list if it got closed, became invisible or was marked as removed
                    if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
                    {
                        if (cachedRoomList.ContainsKey(info.Name))
                        {
                            cachedRoomList.Remove(info.Name);
                        }

                        continue;
                    }

                    // Update cached room info
                    if (cachedRoomList.ContainsKey(info.Name))
                    {
                        cachedRoomList[info.Name] = info;
                    }
                    // Add new room info to cache
                    else
                    {
                        cachedRoomList.Add(info.Name, info);
                    }
                }
            }
        }
    }
}