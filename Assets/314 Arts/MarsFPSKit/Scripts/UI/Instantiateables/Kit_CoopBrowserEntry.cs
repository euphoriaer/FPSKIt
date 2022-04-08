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
        /// This class contains references for the coop server browser
        /// </summary>
        public class Kit_CoopBrowserEntry : MonoBehaviour
        {
            /// <summary>
            /// The name of this room
            /// </summary>
            public TextMeshProUGUI serverName;
            /// <summary>
            /// The map that is currently played in this room
            /// </summary>
            public TextMeshProUGUI mapName;
            /// <summary>
            /// How many players are in this room
            /// </summary>
            public TextMeshProUGUI players;
            /// <summary>
            /// The ping of this room - The cloud
            /// </summary>
            public TextMeshProUGUI ping;
            /// <summary>
            /// Join Button
            /// </summary>
            public Button joinButton;
            private RoomInfo myRoom;

            /// <summary>
            /// Called from Main Menu to properly set this entry up
            /// </summary>
            public void Setup(Kit_MenuPveGameModeBase menu, RoomInfo curRoom)
            {
                myRoom = curRoom;

                if (myRoom != null)
                {
                    //Set Info
                    serverName.text = myRoom.Name;
                    int gameMode = (int)myRoom.CustomProperties["gameMode"];
                    //Map
                    mapName.text = menu.menuManager.game.allCoopGameModes[gameMode].maps[(int)myRoom.CustomProperties["map"]].mapName;
                    //Players
                    players.text = myRoom.PlayerCount + "/" + myRoom.MaxPlayers;
                    //Ping
                    ping.text = PhotonNetwork.GetPing().ToString();
                }

                //Reset scale (Otherwise it will be offset)
                transform.localScale = Vector3.one;
            }
        }
    }
}