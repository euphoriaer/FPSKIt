using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace MarsFPSKit
{
    namespace UI
    {
        public class Kit_MenuServerBrowser : MonoBehaviourPunCallbacks
        {
            /// <summary>
            /// Menu Manager
            /// </summary>
            public Kit_MenuManager menuManager;

            /// <summary>
            /// The "Content" object of the Scroll view, where entriesPrefab will be instantiated
            /// </summary>
            public RectTransform entriesGo;
            /// <summary>
            /// The Server Browser Entry prefab
            /// </summary>
            public GameObject entriesPrefab;
            /// <summary>
            /// Currently active server browser entries - used for cleanup
            /// </summary>
            private List<GameObject> activeEntries = new List<GameObject>();

            #region Password
            [Header("Password")]
            /// <summary>
            /// The room we are currently trying to join
            /// </summary>
            public RoomInfo passwordRoom;
            /// <summary>
            /// Root for password
            /// </summary>
            public GameObject passwordUi;
            /// <summary>
            /// The password to compare
            /// </summary>
            public TMP_InputField passwordInput;
            /// <summary>
            /// Are we currently entering a password?
            /// </summary>
            private bool isPasswordActive;
            #endregion

            //This section includes everything needed for the error message window
            #region Error Message
            [Header("Error Message")]
            /// <summary>
            /// The root object of the error message.
            /// </summary>
            public GameObject em_root;
            /// <summary>
            /// The text object that will hold the error details
            /// </summary>
            public TextMeshProUGUI em_text;
            /// <summary>
            /// The "ok" button of the Error Mesesage.
            /// </summary>
            public Button em_button;
            #endregion

            private Dictionary<string, RoomInfo> cachedRoomList;

            void Awake()
            {
                cachedRoomList = new Dictionary<string, RoomInfo>();
            }

            /// <summary>
            /// Attempts to join a room
            /// <para>See also: <seealso cref="Kit_ServerBrowserEntry"/></para>
            /// </summary>
            /// <param name="room"></param>
            public void JoinRoom(RoomInfo room)
            {
                //Check for password
                string password = (string)room.CustomProperties["password"];
                //Join directly when there is no password
                if (password.Length <= 0)
                {
                    if (PhotonNetwork.JoinRoom(room.Name))
                    {

                    }
                }
                else
                {
                    //Ask for password.
                    //Set room
                    passwordRoom = room;
                    //Reset input
                    passwordInput.text = "";
                    //Open
                    passwordUi.SetActive(true);
                }
            }

            public void PasswordJoin()
            {
                //Check for password
                string password = (string)passwordRoom.CustomProperties["password"];
                if (password == passwordInput.text)
                {
                    if (PhotonNetwork.JoinRoom(passwordRoom.Name))
                    {

                    }
                }
                //Display error
                else
                {
                    DisplayErrorMessage("Password is wrong.");
                }
            }

            public void PasswordAbort()
            {
                //Close
                passwordUi.SetActive(false);
            }

            public void DisplayErrorMessage(string content)
            {
                //Set text
                em_text.text = content;
                //Show
                em_root.SetActive(true);
                //Select button
                em_button.Select();
            }

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

            public override void OnRoomListUpdate(List<RoomInfo> roomList)
            {
                UpdateCachedRoomList(roomList);

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

                    //2 = PvP MP
                    if (gameModeType == 2)
                    {
                        if (!(bool)info.CustomProperties["lobby"])
                        {
                            //Instantiate entry
                            GameObject go = Instantiate(entriesPrefab, entriesGo) as GameObject;
                            //Set it up
                            go.GetComponent<Kit_ServerBrowserEntry>().Setup(this, info);
                            //Add it to our active list so it will get cleaned up next time
                            activeEntries.Add(go);
                        }
                    }
                }
            }

            public override void OnDisconnected(DisconnectCause cause)
            {
                //Reset
                cachedRoomList = new Dictionary<string, RoomInfo>();

                //Clean Up
                for (int i = 0; i < activeEntries.Count; i++)
                {
                    //Destroy
                    Destroy(activeEntries[i]);
                }
                //Reset list
                activeEntries = new List<GameObject>();
            }
        }
    }
}