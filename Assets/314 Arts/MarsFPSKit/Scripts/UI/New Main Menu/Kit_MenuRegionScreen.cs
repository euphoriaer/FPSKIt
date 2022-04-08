using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    namespace UI
    {
        public class Kit_MenuRegionScreen : MonoBehaviourPunCallbacks
        {
            /// <summary>
            /// Access to menu manager!
            /// </summary>
            public Kit_MenuManager menuManager;
            /// <summary>
            /// Id of this screen in the manager
            /// </summary>
            public int hostScreenId;
            /// <summary>
            /// Where the buttons go!
            /// </summary>
            public RectTransform regionButtonGo;
            /// <summary>
            /// Prefab for the region button
            /// </summary>
            public GameObject regionButtonPrefab;

            /// <summary>
            /// If set to true, photon will connect to best region!
            /// </summary>
            [Header("Settings")]
            public bool useBestRegionAsDefault = true;
            /// <summary>
            /// Default region if the <see cref="useBestRegionAsDefault"/> is set to false.
            /// </summary>
            public string defaultRegion = "eu";
            /// <summary>
            /// The currently selected region!
            /// </summary>
            private string currentRegion = "";
            /// <summary>
            /// For switching regions :)
            /// </summary>
            private bool reconnectUponDisconnect;

            private void Start()
            {
                //Setup Buttons
                for (int i = 0; i < menuManager.game.allRegions.Length; i++)
                {
                    GameObject go = Instantiate(regionButtonPrefab, regionButtonGo, false);
                    //Setup Text
                    TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
                    txt.text = menuManager.game.allRegions[i].regionName;
                    //Setup actual button
                    Button btn = go.GetComponent<Button>();
                    //Setup button call
                    int id = i;
                    btn.onClick.AddListener(delegate { ChangeRegion(id); });
                }

                if (PlayerPrefs.HasKey("region"))
                {
                    currentRegion = PlayerPrefs.GetString("region");
                }

                //Set Photon AppID!
                PhotonNetwork.NetworkingClient.AppVersion = PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion;
                PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
            }

            public void ChangeRegion(int to)
            {
                currentRegion = menuManager.game.allRegions[to].token;
                PlayerPrefs.SetString("region", currentRegion);

                if (PhotonNetwork.IsConnected)
                {
                    reconnectUponDisconnect = true;
                    PhotonNetwork.Disconnect();
                }
                else
                {
                    if (menuManager.login) PhotonNetwork.AuthValues = menuManager.login.GetAuthenticationValues();
                    PhotonNetwork.ConnectToRegion(currentRegion);
                    PhotonVoiceNetwork.Instance.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings.AppSettings);
                }

                //Go back to main screen
                menuManager.SwitchMenu(menuManager.mainScreen);
            }

            /// <summary>
            /// Called when the user logged in, connect!
            /// </summary>
            public void OnLoggedIn()
            {
                //If no region, connect to default one!
                if (currentRegion == "")
                {
                    if (useBestRegionAsDefault)
                    {
                        if (menuManager.login) PhotonNetwork.AuthValues = menuManager.login.GetAuthenticationValues();
                        PhotonNetwork.ConnectToBestCloudServer();
                        PhotonVoiceNetwork.Instance.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings.AppSettings);
                    }
                    else
                    {
                        if (menuManager.login) PhotonNetwork.AuthValues = menuManager.login.GetAuthenticationValues();
                        PhotonNetwork.ConnectToRegion(defaultRegion);
                        PhotonVoiceNetwork.Instance.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings.AppSettings);
                    }
                }
                //Connect to the one that we selected
                else
                {
                    if (menuManager.login) PhotonNetwork.AuthValues = menuManager.login.GetAuthenticationValues();
                    PhotonNetwork.ConnectToRegion(currentRegion);
                    PhotonVoiceNetwork.Instance.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings.AppSettings);
                }
            }

            public void GameStartedNotConnected()
            {
                //If no region, connect to default one!
                if (currentRegion == "")
                {
                    if (useBestRegionAsDefault)
                    {
                        if (menuManager.login) PhotonNetwork.AuthValues = menuManager.login.GetAuthenticationValues();
                        PhotonNetwork.ConnectToBestCloudServer();
                        PhotonVoiceNetwork.Instance.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings.AppSettings);
                    }
                    else
                    {
                        if (menuManager.login) PhotonNetwork.AuthValues = menuManager.login.GetAuthenticationValues();
                        PhotonNetwork.ConnectToRegion(defaultRegion);
                        PhotonVoiceNetwork.Instance.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings.AppSettings);
                    }
                }
                //Connect to the one that we selected
                else
                {
                    if (menuManager.login) PhotonNetwork.AuthValues = menuManager.login.GetAuthenticationValues();
                    PhotonNetwork.ConnectToRegion(currentRegion);
                    PhotonVoiceNetwork.Instance.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings.AppSettings);
                }
            }

            public override void OnConnectedToMaster()
            {
                if (!PhotonNetwork.OfflineMode)
                {
                    //Join Lobby
                    PhotonNetwork.JoinLobby();
                    //Get Region
                    currentRegion = PhotonNetwork.CloudRegion;
                    //Log
                    Debug.Log("[Region Screen] Connected to region: " + currentRegion);
                }
            }

            public override void OnDisconnected(DisconnectCause cause)
            {
                if (reconnectUponDisconnect)
                {
                    if (menuManager.login) PhotonNetwork.AuthValues = menuManager.login.GetAuthenticationValues();
                    PhotonNetwork.ConnectToRegion(currentRegion);
                    PhotonVoiceNetwork.Instance.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings.AppSettings);
                }
            }
        }
    }
}