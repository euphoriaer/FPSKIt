using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    namespace UI
    {
        /// <summary>
        /// This class contains references for the server browser and acts as a sender  
        /// </summary>
        public class Kit_ServerBrowserEntry : MonoBehaviour
        {
            public TextMeshProUGUI serverName; //The name of this room
            public TextMeshProUGUI mapName; //The map that is currently played in this room
            public TextMeshProUGUI gameModeName; //The game mode that is currently played in this room
            public TextMeshProUGUI players; //How many players are in this room
            public TextMeshProUGUI ping; //The ping of this room - The cloud
            public TextMeshProUGUI password; //If this room is password protected
            private Kit_MenuServerBrowser msb;
            private RoomInfo myRoom;

            /// <summary>
            /// Called from Main Menu to properly set this entry up
            /// </summary>
            public void Setup(Kit_MenuServerBrowser curMsb, RoomInfo curRoom)
            {
                msb = curMsb;
                myRoom = curRoom;

                if (myRoom != null)
                {
                    //Set Info
                    serverName.text = myRoom.Name;
                    int gameMode = (int)myRoom.CustomProperties["gameMode"];
                    //Game Mode
                    gameModeName.text = msb.menuManager.game.allPvpGameModes[gameMode].gameModeName;
                    //Map
                    mapName.text = msb.menuManager.game.allPvpGameModes[gameMode].traditionalMaps[(int)myRoom.CustomProperties["map"]].mapName;
                    bool bots = (bool)myRoom.CustomProperties["bots"];
                    if (bots)
                    {
                        //Players
                        players.text = myRoom.PlayerCount + "/" + myRoom.MaxPlayers + " (bots)";
                    }
                    else
                    {
                        //Players
                        players.text = myRoom.PlayerCount + "/" + myRoom.MaxPlayers;
                    }
                    //Ping
                    ping.text = PhotonNetwork.GetPing().ToString();
                    //Password
                    if (myRoom.CustomProperties["password"] != null && ((string)myRoom.CustomProperties["password"]).Length > 0) password.text = "Yes";
                    else password.text = "No";
                }

                //Reset scale (Otherwise it will be offset)
                transform.localScale = Vector3.one;
            }

            //Called from the button that is on this prefab, to join this room (attempt)
            public void OnClick()
            {
                //Check if this button is ready
                if (msb)
                {
                    if (myRoom != null)
                    {
                        //Attempt to join
                        msb.JoinRoom(myRoom);
                    }
                }
            }
        }
    }
}