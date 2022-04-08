using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;
using UnityEngine.Events;
using MarsFPSKit.UI;
using System.Collections;
using TMPro;

#if INTEGRATION_STEAM
using Steamworks;
#endif

namespace MarsFPSKit
{
    //Pause Menu state enum
    public enum PauseMenuState { teamSelection = -1, main = 0 }

    public enum AfterTeamSelection { PauseMenu, AttemptSpawn, Loadout }

    /// <summary>
    /// This class is used to store spawns for a game mode internally
    /// </summary>
    [System.Serializable]
    public class InternalSpawns
    {
        public List<Kit_PlayerSpawn> spawns = new List<Kit_PlayerSpawn>();
    }

    /// <summary>
    /// The Main script of the ingame logic (This is the heart of the game)
    /// </summary>
    /// //It is a PunBehaviour so we can have all the callbacks that we need
    public class Kit_IngameMain : MonoBehaviourPunCallbacks, IOnEventCallback, IPunObservable
    {
        /// <summary>
        /// The root of all UI
        /// </summary>
        public GameObject ui_root;

        /// <summary>
        /// Main canvas
        /// </summary>
        public Canvas canvas;

        //The current state of the pause menu
        public PauseMenuState pauseMenuState = PauseMenuState.teamSelection;

        //This hols all game information
        #region Game Information
        [Header("Internal Game Information")]
        [Tooltip("This object contains all game information such as Maps, Game Modes and Weapons")]
        public Kit_GameInformation gameInformation;

        public GameObject playerPrefab; //The player prefab that we should use
        #endregion

        #region Menu Managing
        [Header("Menu Manager")]
        /// <summary>
        /// Menu screens
        /// </summary>
        public MenuScreen[] menuScreens;
        /// <summary>
        /// ID for fading INTO the game
        /// </summary>
        public int ingameFadeId = 2;
        /// <summary>
        /// Do we have a screen to fade out?
        /// </summary>
        private bool wasFirstScreenFadedIn;
        /// <summary>
        /// The menu screen that is currently visible (in order to fade it out)
        /// </summary>
        public int currentScreen = -1;
        /// <summary>
        /// True if we are currently switching a screen
        /// </summary>
        private bool isSwitchingScreens;
        /// <summary>
        /// Where we are currently switching screens to
        /// </summary>
        private Coroutine currentlySwitchingScreensTo;
        #endregion

        [Header("Map Settings")]
        /// <summary>
        /// If you are below this position on your y axis, you die
        /// </summary>
        public float mapDeathThreshold = -50f;

        //This contains all the game mode informations
        #region Game Mode Variables
        [Header("Game Mode Variables")]
        /// <summary>
        /// The game mode timer
        /// </summary>
        public float timer = 600f;
        /// <summary>
        /// A universal stage for game modes, since every game mode requires one like this
        /// </summary>
        public int gameModeStage;
        /// <summary>
        /// Used for the game mode stage changed callback (Called for everyone)
        /// </summary>
        private int lastGameModeStage;
        /// <summary>
        /// The game mode type (!) we are currently playing
        /// </summary>
        public int currentGameModeType;
        /// <summary>
        /// The game mode we are currently playing
        /// </summary>
        public int currentGameMode;
        /// <summary>
        /// Here you can store runtime data for the game mode. Just make sure to sync it to everybody
        /// </summary>
        public object currentGameModeRuntimeData;

        [HideInInspector]
        public List<InternalSpawns> internalSpawns = new List<InternalSpawns>();
        #endregion

        #region Team Selection
        /// <summary>
        /// Team selection module
        /// </summary>
        [Header("Team Selection")]
        public UI.Kit_IngameMenuTeamSelection ts;
        /// <summary>
        /// The text which displays that we cannot join that team
        /// </summary>
        public TextMeshProUGUI errorText;
        /// <summary>
        /// How long is the warning going to be displayed?
        /// </summary>
        public float errorTime = 3f;
        /// <summary>
        /// Current alpha of the cant join message
        /// </summary>
        private float errorAlpha = 0f;
        #endregion

        //This contains everything needed for the Pause Menu
        #region Pause Menu
        /// <summary>
        /// Pause Menu module
        /// </summary>
        [Header("Pause Menu, Use 'B' in the editor to open / close it")]
        public Kit_IngameMenuPauseMenu pauseMenu;
        #endregion

        /// <summary>
        /// New, modular options screen!!!
        /// </summary>
        [Header("Options")]
        public Kit_MenuOptions options;

        //This contains everything needed for the Scoreboard
        #region Scoreboard
        [Header("Scoreboard")]
        public float sb_pingUpdateRate = 1f; //After how many seconds the ping in our Customproperties should be updated
        private float sb_lastPingUpdate; //When our ping was updated for the last time
        #endregion

        //This contains the local camera control
        #region Camera Control
        [Header("Camera Control")]
        public Camera mainCamera; //The main camera to use for the whole game
#if INTEGRATION_FPV2
        public Camera weaponCamera; //This is a second camera used by FPV2 for the matrix.
#elif INTEGRATION_FPV3
        public Camera weaponCamera; //This is a second camera used by FPV2 for the matrix.
#endif
        /// <summary>
        /// Camera shake!
        /// </summary>
        public Kit_CameraShake cameraShake;
        //We recycle the same camera for the whole game, for easy setup of image effects
        //Be careful when changing near and far clip
        public Transform activeCameraTransform
        {
            get
            {
                if (mainCamera)
                {
                    return mainCamera.transform.parent;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value)
                {
                    Debug.Log("[Camera] Changing camera parent to " + value.name, value);
                }
                else
                {
                    Debug.Log("[Camera] Changing camera parent to null");
                }

                if (mainCamera)
                {
                    //We use one camera for the complete game
                    //Set parent
                    mainCamera.transform.parent = value;
                    //If the parent is not null, reset position and rotation
                    if (value)
                    {
                        mainCamera.transform.localPosition = Vector3.zero;
                        mainCamera.transform.localRotation = Quaternion.identity;
                    }
                }
            }
        }
        public Transform spawnCameraPosition; //The spawn position for the camera
        #endregion

        [Header("Modules")]
        [Header("HUD")]
        //This contains the HUD reference
        #region HUD
        /// <summary>
        /// Use this to access the Player HUD
        /// </summary>
        public Kit_PlayerHUDBase hud;
        #endregion

        //This contains the Killfeed reference
        #region Killfeed
        [Header("Killfeed")]
        public Kit_KillFeedBase killFeed;
        #endregion

        #region Chat
        [Header("Chat")]
        public Kit_ChatBase chat;
        #endregion

        #region Impact Processor
        [Header("Impact Processor")]
        public Kit_ImpactParticleProcessor impactProcessor;
        #endregion

        #region Scoreboard
        [Header("Scoreboard")]
        public Scoreboard.Kit_ScoreboardBase scoreboard;
        #endregion

        #region PointsUI
        [Header("Points UI")]
        public Kit_PointsUIBase pointsUI;
        #endregion

        #region Victory Screen
        [Header("Victory Screen")]
        public Kit_VictoryScreenUI victoryScreenUI;
        #endregion

        #region MapVoting
        [Header("Map Voting")]
        public Kit_MapVotingUIBase mapVotingUI;
        #endregion

        #region Ping Limit
        [Header("Ping Limit")]
        public Kit_PingLimitBase pingLimitSystem;
        public Kit_PingLimitUIBase pingLimitUI;
        #endregion

        #region AFK Limit
        [Header("AFK Limit")]
        public Kit_AfkLimitBase afkLimitSystem;
        public Kit_AfkLimitUIBase afkLimitUI;
        #endregion

        #region Loadout
        [Header("Loadout")]
        public Kit_LoadoutBase loadoutMenu;
        #endregion

        #region Voting
        [Header("Voting")]
        public Kit_VotingUIBase votingMenu;
        [HideInInspector]
        public Kit_VotingBase currentVoting;
        #endregion

        #region Voice Chat
        [Header("Voice Chat")]
        public Kit_VoiceChatBase voiceChat;
        #endregion

        #region Leveling UI
        /// <summary>
        /// If this is assigned, it will display level ups
        /// </summary>
        [Header("Leveling UI")]
        public Kit_LevelingUIBase levelingUi;
        #endregion

        #region Minimap
        [Header("Minimap")]
        public Kit_MinimapBase minimap;
        #endregion

        #region Auto Spawn System
        /// <summary>
        /// Spawn system to use
        /// </summary>
        [Header("Auto Spawn System")]
        public Kit_AutoSpawnSystemBase autoSpawnSystem;
        #endregion

        #region Object Pooling
        [Header("Object Pooling")]
        /// <summary>
        /// Object Pooling interface
        /// </summary>
        public Optimization.Kit_ObjectPoolingBase objectPooling;
        #endregion

        #region Assists
        /// <summary>
        /// Assist manager module
        /// </summary>
        [Header("Assists")]
        public Kit_AssistManagerBase assistManager;
        /// <summary>
        /// Runtime data for the assist manager
        /// </summary>
        public object assistManagerData;
        #endregion

        #region Spectating
        [Header("Spectator")]
        /// <summary>
        /// Spectator manager
        /// </summary>
        public Spectating.Kit_SpectatorManagerBase spectatorManager;
        /// <summary>
        /// Runtime data for the spectator manager
        /// </summary>
        public object spectatorManagerRuntimeData;
        #endregion

        /// <summary>
        /// A hud that is only visible when the player is alive can be instantiated here
        /// </summary>
        [Header("Plugins")]
        public RectTransform pluginPlayerActiveHudGo;
        /// <summary>
        /// A hud that is always active can be instantiated here
        /// </summary>
        public RectTransform pluginAlwaysActiveHudGo;
        /// <summary>
        /// Where external modules can be instantiated
        /// </summary>
        public Transform pluginModuleGo;

        [Header("Instantiateables")]
        /// <summary>
        /// This contains the prefab for the victory screen. Once its setup it will sync to all other players.
        /// </summary>
        public GameObject victoryScreen;
        [HideInInspector]
        /// <summary>
        /// A reference to the victory screen so it can be destroyed when it's not needed anymore.
        /// </summary>
        public Kit_VictoryScreen currentVictoryScreen;
        /// <summary>
        /// This contains the prefab for the map voting. Once its setup it will sync to all other players
        /// </summary>
        public GameObject mapVoting;
        [HideInInspector]
        /// <summary>
        /// A reference to the map voting. Can be null
        /// </summary>
        public Kit_MapVotingBehaviour currentMapVoting;
        /// <summary>
        /// The prefab for the player initiated voting
        /// </summary>
        public GameObject playerStartedVoting;

        /// <summary>
        /// Prefab for the bot manager
        /// </summary>
        [Header("Bots")]
        public GameObject botManagerPrefab;
        [HideInInspector]
        /// <summary>
        /// If Bots are enabled, this is the bot manager
        /// </summary>
        public Kit_BotManager currentBotManager;
        [HideInInspector]
        /// <summary>
        /// All bot nav points
        /// </summary>
        public Transform[] botNavPoints;

        /// <summary>
        /// This is the input that shall be used if we have a touchscreen
        /// </summary>
        [Header("Touchscreen Input")]
        public GameObject touchScreenPrefab;
        [HideInInspector]
        /// <summary>
        /// If touch screen input is enabled, this is assigned.
        /// </summary>
        public Kit_TouchscreenBase touchScreenCurrent;


        //This section contains internal variables used by the game
        #region Internal Variables
        /// <summary>
        /// Only used in PVP Game Modes. Assigned Team ID
        /// </summary>
        [HideInInspector]
        public int assignedTeamID = -1;
        /// <summary>
        /// Our own player, returns null if we have not spawned
        /// </summary>
        [HideInInspector]
        public Kit_PlayerBehaviour myPlayer;
        /// <summary>
        /// Is the pause menu currently opened?
        /// </summary>
        [HideInInspector]
        public static bool isPauseMenuOpen;
        [HideInInspector]
        /// <summary>
        /// Active PvE game mode
        /// </summary>
        public Kit_PvE_GameModeBase currentPvEGameModeBehaviour;
        [HideInInspector]
        /// <summary>
        /// Active PvP game mode
        /// </summary>
        public Kit_PvP_GameModeBase currentPvPGameModeBehaviour;
        /// <summary>
        /// Instance of current game mode HUD. Could be null.
        /// </summary>
        [HideInInspector]
        public Kit_GameModeHUDBase currentGameModeHUD;
        [HideInInspector]
        /// <summary>
        /// Is the ping limit system enabled by the user?
        /// </summary>
        public bool isPingLimiterEnabled = false;
        [HideInInspector]
        /// <summary>
        /// Is the afk limit system enabled by the user?
        /// </summary>
        public bool isAfkLimiterEnabled = false;
        [HideInInspector]
        /// <summary>
        /// Have we actually begun to play this game mode?
        /// </summary>
        public bool hasGameModeStarted = false;
        [HideInInspector]
        /// <summary>
        /// Is the camera fov overriden?
        /// </summary>
        public bool isCameraFovOverridden;

        public List<object> pluginRuntimeData = new List<object>();
        /// <summary>
        /// This is a list of all active players
        /// </summary>
        public List<Kit_PlayerBehaviour> allActivePlayers = new List<Kit_PlayerBehaviour>();
        #endregion

        #region Unity Calls
        void Awake()
        {
            //Hide HUD initially
            hud.SetVisibility(false);
            //Set pause menu state
            isPauseMenuOpen = false;

            //Disable all the roots
            for (int i = 0; i < menuScreens.Length; i++)
            {
                if (menuScreens[i].root)
                {
                    //Disable
                    menuScreens[i].root.SetActive(false);
                }
                else
                {
                    Debug.LogError("Menu root at index " + i + " is not assigned.", this);
                }
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            //Check if we shall replace camera
            if (gameInformation.mainCameraOverride)
            {
                //Instantiate new
                GameObject newCamera = Instantiate(gameInformation.mainCameraOverride, mainCamera.transform, false);
                //Reparent
                newCamera.transform.parent = spawnCameraPosition;
                //Destroy camera
                Destroy(mainCamera.gameObject);
                //Assign new camera
                mainCamera = newCamera.GetComponent<Camera>();
                //Camera Shake
                cameraShake = newCamera.GetComponentInChildren<Kit_CameraShake>();
#if INTEGRATION_FPV2
                //Get weapon camera
                weaponCamera = newCamera.GetComponentInChildren<FirstPersonView.ShaderMaterialSolution.FPV_SM_FirstPersonCamera>().GetComponent<Camera>();
                if (!weaponCamera) Debug.LogError("FPV2 is enabled but correct prefab is not assigned!", gameInformation.mainCameraOverride);
#elif INTEGRATION_FPV3
                //Get weapon camera
                weaponCamera = newCamera.GetComponentInChildren<FirstPersonView.FPV_Camera_FirstPerson>().GetComponent<Camera>();
                if (!weaponCamera) Debug.LogError("FPV3 is enabled but correct prefab is not assigned!", gameInformation.mainCameraOverride);
#endif
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        void Start()
        {
            //Call Plugin
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].OnPreSetup(this);
            }

            //Check if mobile input should be used
            if (gameInformation.enableTouchscreenInput)
            {
                //Create input
                GameObject go = Instantiate(touchScreenPrefab);
                touchScreenCurrent = go.GetComponent<Kit_TouchscreenBase>();
                //Setup
                touchScreenCurrent.Setup(this);
            }

            //Impact Processor
            if (impactProcessor)
            {
                impactProcessor.StartImpactProcessor(this);
            }

            //Assist Manager
            if (assistManager)
            {
                assistManager.OnStart(this);
            }

            //Set initial states
            pluginOnForceClose.Invoke();
            ui_root.SetActive(true);
            assignedTeamID = -1;

            //Make sure the main camera is child of the spawn camera position
            activeCameraTransform = spawnCameraPosition;

            if (gameInformation)
            {
                //Check if we're connected
                if (PhotonNetwork.InRoom)
                {
                    //Get type
                    int gameModeType = (int)PhotonNetwork.CurrentRoom.CustomProperties["gameModeType"];
                    //Assign
                    currentGameModeType = gameModeType;

                    if (gameModeType == 0)
                    {
                        //Setup Game Mode based on Custom properties
                        int gameMode = (int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];
                        currentGameMode = gameMode;
                        currentPvEGameModeBehaviour = gameInformation.allSingleplayerGameModes[gameMode];
                        gameInformation.allSingleplayerGameModes[gameMode].GamemodeSetup(this);
                        hasGameModeStarted = true;

                        //Setup HUD
                        if (currentPvEGameModeBehaviour.hudPrefab)
                        {
                            GameObject hudPrefab = Instantiate(currentPvEGameModeBehaviour.hudPrefab, hud.transform, false);
                            //Move to the back
                            hudPrefab.transform.SetAsFirstSibling();
                            //Reset scale
                            hudPrefab.transform.localScale = Vector3.one;
                            //Get script
                            currentGameModeHUD = hudPrefab.GetComponent<Kit_GameModeHUDBase>();
                            //Start
                            currentGameModeHUD.HUDInitialize(this);
                        }

                        //Set initial Custom properties
                        Hashtable myLocalTable = new Hashtable();
                        //Set inital team
                        //-1 = No Team
                        myLocalTable.Add("team", -1);
                        //Set inital stats
                        myLocalTable.Add("kills", 0);
                        myLocalTable.Add("assists", 0);
                        myLocalTable.Add("deaths", 0);
                        myLocalTable.Add("ping", PhotonNetwork.GetPing());
                        myLocalTable.Add("vote", -1); //For Map voting menu AND the player voting
                                                      //Assign to GameSettings
                        PhotonNetwork.LocalPlayer.SetCustomProperties(myLocalTable);
                    }
                    else if (gameModeType == 1)
                    {
                        //Setup Game Mode based on Custom properties
                        int gameMode = (int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];
                        currentGameMode = gameMode;
                        currentPvEGameModeBehaviour = gameInformation.allCoopGameModes[gameMode];
                        gameInformation.allCoopGameModes[gameMode].GamemodeSetup(this);
                        hasGameModeStarted = true;

                        //Setup HUD
                        if (currentPvEGameModeBehaviour.hudPrefab)
                        {
                            GameObject hudPrefab = Instantiate(currentPvEGameModeBehaviour.hudPrefab, hud.transform, false);
                            //Move to the back
                            hudPrefab.transform.SetAsFirstSibling();
                            //Reset scale
                            hudPrefab.transform.localScale = Vector3.one;
                            //Get script
                            currentGameModeHUD = hudPrefab.GetComponent<Kit_GameModeHUDBase>();
                            //Start
                            currentGameModeHUD.HUDInitialize(this);
                        }

                        //Set initial Custom properties
                        Hashtable myLocalTable = new Hashtable();
                        //Set inital team
                        //-1 = No Team
                        myLocalTable.Add("team", -1);
                        //Set inital stats
                        myLocalTable.Add("kills", 0);
                        myLocalTable.Add("assists", 0);
                        myLocalTable.Add("deaths", 0);
                        myLocalTable.Add("ping", PhotonNetwork.GetPing());
                        myLocalTable.Add("vote", -1); //For Map voting menu AND the player voting
                                                      //Assign to GameSettings
                        PhotonNetwork.LocalPlayer.SetCustomProperties(myLocalTable);
                    }
                    else if (gameModeType == 2)
                    {
                        //Setup Game Mode based on Custom properties
                        int gameMode = (int)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"];
                        currentGameMode = gameMode;
                        currentPvPGameModeBehaviour = gameInformation.allPvpGameModes[gameMode];
                        gameInformation.allPvpGameModes[gameMode].GamemodeSetup(this);

                        ts.Setup();

                        //If we already have a game mode hud, destroy it
                        if (currentGameModeHUD)
                        {
                            Destroy(currentGameModeHUD.gameObject);
                        }

                        //Initialize Loadout Menu
                        if (loadoutMenu)
                        {
                            loadoutMenu.Initialize();
                            //Force it to be closed
                            loadoutMenu.ForceClose();
                        }

                        //Setup HUD
                        if (currentPvPGameModeBehaviour.hudPrefab)
                        {
                            GameObject hudPrefab = Instantiate(currentPvPGameModeBehaviour.hudPrefab, hud.transform, false);
                            //Move to the back
                            hudPrefab.transform.SetAsFirstSibling();
                            //Reset scale
                            hudPrefab.transform.localScale = Vector3.one;
                            //Get script
                            currentGameModeHUD = hudPrefab.GetComponent<Kit_GameModeHUDBase>();
                            //Start
                            currentGameModeHUD.HUDInitialize(this);
                        }

                        //Set timer
                        int duration = (int)PhotonNetwork.CurrentRoom.CustomProperties["duration"];
                        //Assign global game length
                        Kit_GameSettings.gameLength = duration;

                        if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
                        {
                            //Get ping limit
                            int pingLimit = (int)PhotonNetwork.CurrentRoom.CustomProperties["ping"];

                            if (currentPvPGameModeBehaviour.traditionalPingLimits[pingLimit] > 0)
                            {
                                //Ping limit enabled
                                if (pingLimitSystem)
                                {
                                    //Tell the system to start
                                    pingLimitSystem.StartRelay(this, true, currentPvPGameModeBehaviour.traditionalPingLimits[pingLimit]);
                                    isPingLimiterEnabled = true;
                                }
                            }
                            else
                            {
                                //Ping limit disablde
                                if (pingLimitSystem)
                                {
                                    //Tell the system to not start
                                    pingLimitSystem.StartRelay(this, false);
                                    isPingLimiterEnabled = false;
                                }
                            }

                            //Get AFK limit
                            int afkLimit = (int)PhotonNetwork.CurrentRoom.CustomProperties["afk"];

                            if (currentPvPGameModeBehaviour.traditionalAfkLimits[afkLimit] > 0)
                            {
                                //AFK limit enabled
                                if (afkLimitSystem)
                                {
                                    //Relay to the system
                                    afkLimitSystem.StartRelay(this, true, currentPvPGameModeBehaviour.traditionalAfkLimits[afkLimit]);
                                    isAfkLimiterEnabled = true;
                                }
                            }
                            else
                            {
                                //AFK limit disabled
                                if (afkLimitSystem)
                                {
                                    afkLimitSystem.StartRelay(this, false);
                                    isAfkLimiterEnabled = false;
                                }
                            }
                        }
                        else if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
                        {
                            if (currentPvPGameModeBehaviour.lobbyPingLimit > 0)
                            {
                                //Ping limit enabled
                                if (pingLimitSystem)
                                {
                                    //Tell the system to start
                                    pingLimitSystem.StartRelay(this, true, currentPvPGameModeBehaviour.lobbyPingLimit);
                                    isPingLimiterEnabled = true;
                                }
                            }
                            else
                            {
                                //Ping limit disablde
                                if (pingLimitSystem)
                                {
                                    //Tell the system to not start
                                    pingLimitSystem.StartRelay(this, false);
                                    isPingLimiterEnabled = false;
                                }
                            }

                            if (currentPvPGameModeBehaviour.lobbyAfkLimit > 0)
                            {
                                //AFK limit enabled
                                if (afkLimitSystem)
                                {
                                    //Relay to the system
                                    afkLimitSystem.StartRelay(this, true, currentPvPGameModeBehaviour.lobbyAfkLimit);
                                    isAfkLimiterEnabled = true;
                                }
                            }
                            else
                            {
                                //AFK limit disabled
                                if (afkLimitSystem)
                                {
                                    afkLimitSystem.StartRelay(this, false);
                                    isAfkLimiterEnabled = false;
                                }
                            }
                        }

                        //Setup Bots
                        if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["bots"])
                        {
                            //Setup Nav Points
                            Kit_BotNavPoint[] navPoints = FindObjectsOfType<Kit_BotNavPoint>();

                            if (navPoints.Length == 0) throw new System.Exception("[Bots] No Nav Points have been found for this scene! You need to add some.");
                            List<Transform> tempNavPoints = new List<Transform>();
                            for (int i = 0; i < navPoints.Length; i++)
                            {
                                if (navPoints[i].gameModes.Contains(currentPvPGameModeBehaviour))
                                {
                                    if (navPoints[i].navPointGroupID == 0)
                                    {
                                        tempNavPoints.Add(navPoints[i].transform);
                                    }
                                }
                            }
                            botNavPoints = tempNavPoints.ToArray();

                            if (PhotonNetwork.IsMasterClient)
                            {
                                if (!currentBotManager)
                                {
                                    GameObject go = PhotonNetwork.InstantiateRoomObject(botManagerPrefab.name, Vector3.zero, Quaternion.identity, 0, null);
                                    currentBotManager = go.GetComponent<Kit_BotManager>();
                                    if (currentPvPGameModeBehaviour.botManagerToUse)
                                    {
                                        currentPvPGameModeBehaviour.botManagerToUse.Inizialize(currentBotManager);
                                    }
                                }
                            }
                            else
                            {
                                if (!currentBotManager)
                                {
                                    currentBotManager = FindObjectOfType<Kit_BotManager>();
                                }
                            }
                        }

                        //Set initial Custom properties
                        Hashtable myLocalTable = new Hashtable();
                        //Set inital team
                        //-1 = No Team
                        myLocalTable.Add("team", -1);
                        //Set inital stats
                        myLocalTable.Add("kills", 0);
                        myLocalTable.Add("assists", 0);
                        myLocalTable.Add("deaths", 0);
                        myLocalTable.Add("ping", PhotonNetwork.GetPing());
                        myLocalTable.Add("vote", -1); //For Map voting menu AND the player voting
                                                      //Assign to GameSettings
                        PhotonNetwork.LocalPlayer.SetCustomProperties(myLocalTable);

                        //Check if we already have enough players to start playing
                        if (currentPvPGameModeBehaviour.AreEnoughPlayersThere(this))
                        {
                            hasGameModeStarted = true;
                        }
                    }

                    if (voiceChat)
                    {
                        //Setup Voice Chat
                        voiceChat.Setup(this);
                    }

                    if (minimap)
                    {
                        //Tell Minimap to set itself up
                        minimap.Setup(this);
                    }

                    if (spectatorManager)
                    {
                        //Setup spectator manager
                        spectatorManager.Setup(this);
                    }

#if INTEGRATION_STEAM
                    //Set Steam Rich Presence
                    //Set connect
                    //Region@Room Name
                    SteamFriends.SetRichPresence("connect", PhotonNetwork.CloudRegion.ToString() + ":" + PhotonNetwork.CurrentRoom.Name);
                    //Set Status
                    SteamFriends.SetRichPresence("status", "Playing " + gameInformation.allPvpGameModes[gameMode].gameModeName + " on " + gameInformation.GetMapInformationFromSceneName(SceneManager.GetActiveScene().name).mapName);
#endif

                    //Unlock the cursor
                    MarsScreen.lockCursor = false;

                    //Call Plugin
                    for (int i = 0; i < gameInformation.plugins.Length; i++)
                    {
                        gameInformation.plugins[i].OnSetupDone(this);
                    }

                    switch (gameModeType)
                    {
                        case 0:
                            currentPvEGameModeBehaviour.GameModeProceed(this);
                            break;

                        case 1:
                            currentPvEGameModeBehaviour.GameModeProceed(this);
                            break;

                        case 2:
                            if (!currentMapVoting && !currentVictoryScreen)
                            {
                                SwitchMenu(ts.teamSelectionId);
                                //Set Pause Menu state
                                pauseMenuState = PauseMenuState.teamSelection;
                            }

                            break;
                    }
                }
                else
                {
                    //Go back to Main Menu
                    SceneManager.LoadScene(0);
                }
            }
            else
            {
                Debug.LogError("No Game Information assigned. Game will not work.");
            }
        }

        void Update()
        {
            //If we are in a room
            if (PhotonNetwork.InRoom)
            {
                //Host Logic
                if (PhotonNetwork.IsMasterClient && hasGameModeStarted)
                {
                    #region Timer
                    //Decrease timer
                    if (timer > 0)
                    {
                        timer -= Time.deltaTime;
                        //Check if the timer has run out
                        if (timer <= 0)
                        {
                            if (currentPvEGameModeBehaviour)
                                //Call the game mode callback
                                currentPvEGameModeBehaviour.TimeRunOut(this);

                            if (currentPvPGameModeBehaviour)
                                //Call the game mode callback
                                currentPvPGameModeBehaviour.TimeRunOut(this);
                        }
                    }
                    #endregion
                }

                #region Scoreboard ping update
                //Check if we send a new update
                if (Time.time > sb_lastPingUpdate + sb_pingUpdateRate)
                {
                    //Set last update
                    sb_lastPingUpdate = Time.time;
                    //Update hashtable
                    Hashtable table = PhotonNetwork.LocalPlayer.CustomProperties;
                    table["ping"] = PhotonNetwork.GetPing();
                    //Update hashtable
                    PhotonNetwork.LocalPlayer.SetCustomProperties(table);
                }
                #endregion

                #region Pause Menu
                //Check if the pause menu is ready to be opened and closed and if nothing is blocking it
                if (pauseMenuState >= 0 && !currentVictoryScreen && !currentMapVoting && (!loadoutMenu || loadoutMenu && currentScreen != loadoutMenu.menuScreenId))
                {
                    if (Input.GetKeyDown(KeyCode.Escape) && Application.platform != RuntimePlatform.WebGLPlayer || Input.GetKeyDown(KeyCode.B) && Application.isEditor || Input.GetKeyDown(KeyCode.M) && Application.platform == RuntimePlatform.WebGLPlayer || Application.isMobilePlatform && UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager.GetButtonDown("Pause")) //Escape (for non WebGL), B (For the editor), M (For WebGL)
                    {
                        //Change state
                        isPauseMenuOpen = !isPauseMenuOpen;
                        //Set state
                        if (isPauseMenuOpen)
                        {
                            SwitchMenu(pauseMenu.pauseMenuId, true);
                            //Unlock cursor
                            MarsScreen.lockCursor = false;
                            //Chat callback
                            chat.PauseMenuOpened();
                            //Auto spawn system callack
                            if (autoSpawnSystem && currentPvPGameModeBehaviour)
                            {
                                autoSpawnSystem.Interruption();
                            }
                        }
                        else
                        {
                            SwitchMenu(ingameFadeId, true);
                            pluginOnForceClose.Invoke();
                            //Lock cursor
                            MarsScreen.lockCursor = true;
                            //Chat callback
                            chat.PauseMenuClosed();
                        }
                    }
                }
                #endregion

                #region HUD Update
                if (currentGameModeHUD)
                {
                    //Relay update
                    currentGameModeHUD.HUDUpdate(this);
                }
                #endregion

                #region Game Mode
                if (PhotonNetwork.IsMasterClient)
                {
                    if (currentPvPGameModeBehaviour)
                    {
                        currentPvPGameModeBehaviour.GameModeUpdate(this);
                    }
                    else if (currentPvEGameModeBehaviour)
                    {
                        currentPvEGameModeBehaviour.GameModeUpdate(this);
                    }
                }
                else
                {
                    if (currentPvPGameModeBehaviour)
                    {
                        currentPvPGameModeBehaviour.GameModeUpdateOthers(this);
                    }
                    else if (currentPvEGameModeBehaviour)
                    {
                        currentPvEGameModeBehaviour.GameModeUpdateOthers(this);
                    }
                }

                //Check if the game mode stage has changed
                if (lastGameModeStage != gameModeStage)
                {
                    //Call the callback
                    GameModeStageChanged(lastGameModeStage, gameModeStage);
                    //Set value
                    lastGameModeStage = gameModeStage;
                }
                #endregion

                #region PvP only
                if (currentGameModeType == 2)
                {
                    #region Ping Limiter
                    if (isPingLimiterEnabled && pingLimitSystem)
                    {
                        pingLimitSystem.UpdateRelay(this);
                    }
                    #endregion

                    #region AFK Limiter
                    if (isAfkLimiterEnabled && afkLimitSystem)
                    {
                        afkLimitSystem.UpdateRelay(this);
                    }
                    #endregion

                    #region Waiting for Players
                    //Check if the game mode should begin
                    if (!hasGameModeStarted)
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            //Check if we now have enough players
                            if (currentPvPGameModeBehaviour.AreEnoughPlayersThere(this))
                            {
                                hasGameModeStarted = true;
                                currentPvPGameModeBehaviour.GameModeBeginMiddle(this);
                            }
                        }
                        //Show waiting on the HUD
                        hud.SetWaitingStatus(true);
                    }
                    else
                    {
                        //Hide waiting on the HUD
                        hud.SetWaitingStatus(false);
                    }
                    #endregion
                }
                else //no pvp - no waiting
                {
                    //Hide waiting on the HUD
                    hud.SetWaitingStatus(false);
                }
                #endregion

                #region Cannot Join Team
                if (errorAlpha > 0)
                {
                    //Decrease
                    errorAlpha -= Time.deltaTime;

                    //Set alpha
                    errorText.color = new Color(errorText.color.r, errorText.color.g, errorText.color.b, errorAlpha);

                    //Enable
                    errorText.enabled = true;
                }
                else
                {
                    //Just disable
                    errorText.enabled = false;
                }
                #endregion

                #region FOV
                if (!myPlayer && (!spectatorManager || !spectatorManager.IsCurrentlySpectating(this)))
                {
                    if (!isCameraFovOverridden)
                        mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                }
#if INTEGRATION_FPV2
                else
                {
                    if (weaponCamera)
                    {
                        //Make sure FOV is the same.
                        weaponCamera.fieldOfView = mainCamera.fieldOfView;
                    }
                }
#elif INTEGRATION_FPV3
                else
                {
                    if (weaponCamera)
                    {
                        //Make sure FOV is the same.
                        weaponCamera.fieldOfView = mainCamera.fieldOfView;
                    }
                }
#endif
                #endregion

                #region Plugin
                //Call Plugin
                for (int i = 0; i < gameInformation.plugins.Length; i++)
                {
                    gameInformation.plugins[i].PluginUpdate(this);
                }
                #endregion
            }
        }

        void LateUpdate()
        {
            if (PhotonNetwork.InRoom)
            {
                //Call Plugin
                for (int i = 0; i < gameInformation.plugins.Length; i++)
                {
                    gameInformation.plugins[i].PluginLateUpdate(this);
                }
            }
        }
        #endregion

        #region Photon Calls
        public override void OnPlayerLeftRoom(Player player)
        {
            //Someone left
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                //We are the master client, clean up.
                Debug.Log("Clean up after player " + player);
                PhotonNetwork.DestroyPlayerObjects(player);
            }

            if (currentBotManager && currentPvPGameModeBehaviour.botManagerToUse && PhotonNetwork.IsMasterClient)
            {
                currentPvPGameModeBehaviour.botManagerToUse.PlayerLeftTeam(currentBotManager);
            }

            //Inform chat
            chat.PlayerLeft(player);

            //Call Plugin
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].PlayerLeftRoom(this, player);
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            //Inform chat
            chat.PlayerJoined(newPlayer);

            //Call Plugin
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].PlayerJoinedRoom(this, newPlayer);
            }
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            //Check if we are the new master client
            if (PhotonNetwork.IsMasterClient || newMasterClient == PhotonNetwork.LocalPlayer)
            {
                Debug.Log("We are the new Master Client");
            }

            //Inform chat
            chat.MasterClientSwitched(newMasterClient);

            //Call Plugin
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].MasterClientSwitched(this, newMasterClient);
            }
        }

        [HideInInspector]
        /// <summary>
        /// Set to true if the application is closing
        /// </summary>
        public bool isShuttingDown = false;

        void OnApplicationQuit()
        {
            if (gameInformation.leveling)
            {
                gameInformation.leveling.Save();
            }

            if (gameInformation.statistics)
            {
                gameInformation.statistics.Save(this);
            }

            isShuttingDown = true;
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            if (!isShuttingDown)
            {
                Debug.Log("Disconnected!");
                //We have disconnected from Photon, go to Main Menu
                Kit_SceneSyncer.instance.LoadScene("MainMenu");
            }
        }

        public override void OnLeftRoom()
        {
            if (!isShuttingDown)
            {
                Debug.Log("Left room!");
                //We have disconnected from Photon, go to Main Menu
                Kit_SceneSyncer.instance.LoadScene("MainMenu");
            }
        }

        public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (currentMapVoting)
                {
                    //A player could have changed his vote. Recalculate.
                    currentMapVoting.RecalculateVotes();
                }
            }

            //Call Plugin
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].OnPlayerPropertiesChanged(this, target, changedProps);
            }
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                //Synchronize timer
                stream.SendNext(timer);
                //Synchronize stage
                stream.SendNext(gameModeStage);
                //Synchronize playing stage
                stream.SendNext(hasGameModeStarted);
            }
            else
            {
                //Set timer
                timer = (float)stream.ReceiveNext();
                //Set stage
                gameModeStage = (int)stream.ReceiveNext();
                //Set playing stage
                hasGameModeStarted = (bool)stream.ReceiveNext();
            }
            //Relay to game mode
            if (currentPvPGameModeBehaviour)
            {
                currentPvPGameModeBehaviour.OnPhotonSerializeView(this, stream, info);
            }
            //Relay to loadout
            if (loadoutMenu)
            {
                loadoutMenu.OnPhotonSerializeView(stream, info);
            }
            //Relay to plugins
            //Call Plugin
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].OnPhotonSerializeView(this, stream, info);
            }
        }
        #endregion

        #region Game Logic calls
        /// <summary>
        /// Tries to spawn a player
        /// <para>See also: <seealso cref="Kit_GameModeBase.CanSpawn(Kit_IngameMain, Photon.Realtime.Player)"/></para>
        /// </summary>
        public void Spawn(bool forceRespawn = false)
        {
            switch (currentGameModeType)
            {
                case 0:
                    //We can only spawn if we do not have a player currently
                    if (!myPlayer)
                    {
                        //Check if we can currently spawn
                        if (!currentPvEGameModeBehaviour.UsesCustomSpawn())
                        {
                            if (gameInformation.allSingleplayerGameModes[currentGameMode].CanSpawn(this, PhotonNetwork.LocalPlayer) || forceRespawn)
                            {
                                //Get a spawn
                                Transform spawnLocation = gameInformation.allSingleplayerGameModes[currentGameMode].GetSpawn(this, PhotonNetwork.LocalPlayer);
                                if (spawnLocation)
                                {
                                    //Make SURE we are not spectating.
                                    if (spectatorManager)
                                    {
                                        spectatorManager.EndSpectating(this);
                                    }

                                    //Create object array for photon use
                                    object[] instData = new object[0];
                                    //Assign the values
                                    //0 = Team
                                    //3 = Primary
                                    //4 = Secondary
                                    //5 = Length of primary attachments
                                    //5 + length ... primary attachments
                                    //5 + length pa + 1 = Length of secondary attachments
                                    //5 + length pa + 2 ... secondary attachments
                                    if (loadoutMenu)
                                    {
                                        //Get the current loadout
                                        Loadout curLoadout = currentPvEGameModeBehaviour.GetSpawnLoadout();
                                        int length = 1;
                                        Hashtable playerDataTable = new Hashtable();
                                        playerDataTable["team"] = assignedTeamID;
                                        playerDataTable["bot"] = false;
                                        length++; //How many weapons we spawn with
                                                  //Every weapon now has its own hashtable!
                                        length += curLoadout.loadoutWeapons.Length;
                                        //Create instData
                                        instData = new object[length];
                                        instData[0] = playerDataTable;
                                        instData[1] = curLoadout.loadoutWeapons.Length;
                                        for (int i = 0; i < curLoadout.loadoutWeapons.Length; i++)
                                        {
                                            Hashtable weaponTable = new Hashtable();
                                            weaponTable["slot"] = curLoadout.loadoutWeapons[i].goesToSlot;
                                            weaponTable["id"] = curLoadout.loadoutWeapons[i].weaponID;
                                            weaponTable["attachments"] = curLoadout.loadoutWeapons[i].attachments;
                                            instData[2 + i] = weaponTable;
                                        }
                                    }
                                    else
                                    {
                                        throw new System.Exception("No Loadout menu assigned. This is not allowed.");
                                    }
                                    GameObject go = PhotonNetwork.Instantiate(playerPrefab.name, spawnLocation.position, spawnLocation.rotation, 0, instData);
                                    //Copy player
                                    myPlayer = go.GetComponent<Kit_PlayerBehaviour>();
                                    //Take control using the token
                                    myPlayer.TakeControl();
                                }
                            }
                        }
                        else
                        {
                            GameObject player = currentPvEGameModeBehaviour.DoCustomSpawn(this);
                            if (player)
                            {
                                //Copy player
                                myPlayer = player.GetComponent<Kit_PlayerBehaviour>();
                                //Take control using the token
                                myPlayer.TakeControl();
                            }
                        }
                    }
                    break;
                case 1:
                    //We can only spawn if we do not have a player currently
                    if (!myPlayer)
                    {
                        //Check if we can currently spawn
                        if (!currentPvEGameModeBehaviour.UsesCustomSpawn())
                        {
                            if (gameInformation.allCoopGameModes[currentGameMode].CanSpawn(this, PhotonNetwork.LocalPlayer) || forceRespawn)
                            {
                                //Get a spawn
                                Transform spawnLocation = gameInformation.allCoopGameModes[currentGameMode].GetSpawn(this, PhotonNetwork.LocalPlayer);
                                if (spawnLocation)
                                {
                                    //Make SURE we are not spectating.
                                    if (spectatorManager)
                                    {
                                        spectatorManager.EndSpectating(this);
                                    }

                                    //Create object array for photon use
                                    object[] instData = new object[0];
                                    //Assign the values
                                    //0 = Team
                                    //3 = Primary
                                    //4 = Secondary
                                    //5 = Length of primary attachments
                                    //5 + length ... primary attachments
                                    //5 + length pa + 1 = Length of secondary attachments
                                    //5 + length pa + 2 ... secondary attachments
                                    if (loadoutMenu)
                                    {
                                        //Get the current loadout
                                        Loadout curLoadout = currentPvEGameModeBehaviour.GetSpawnLoadout();
                                        int length = 1;
                                        Hashtable playerDataTable = new Hashtable();
                                        playerDataTable["team"] = assignedTeamID;
                                        playerDataTable["bot"] = false;
                                        length++; //How many weapons we spawn with
                                                  //Every weapon now has its own hashtable!
                                        length += curLoadout.loadoutWeapons.Length;
                                        //Create instData
                                        instData = new object[length];
                                        instData[0] = playerDataTable;
                                        instData[1] = curLoadout.loadoutWeapons.Length;
                                        for (int i = 0; i < curLoadout.loadoutWeapons.Length; i++)
                                        {
                                            Hashtable weaponTable = new Hashtable();
                                            weaponTable["slot"] = curLoadout.loadoutWeapons[i].goesToSlot;
                                            weaponTable["id"] = curLoadout.loadoutWeapons[i].weaponID;
                                            weaponTable["attachments"] = curLoadout.loadoutWeapons[i].attachments;
                                            instData[2 + i] = weaponTable;
                                        }
                                    }
                                    else
                                    {
                                        throw new System.Exception("No Loadout menu assigned. This is not allowed.");
                                    }
                                    GameObject go = PhotonNetwork.Instantiate(playerPrefab.name, spawnLocation.position, spawnLocation.rotation, 0, instData);
                                    //Copy player
                                    myPlayer = go.GetComponent<Kit_PlayerBehaviour>();
                                    //Take control using the token
                                    myPlayer.TakeControl();
                                }
                            }
                        }
                        else
                        {
                            GameObject player = currentPvEGameModeBehaviour.DoCustomSpawn(this);
                            if (player)
                            {
                                //Copy player
                                myPlayer = player.GetComponent<Kit_PlayerBehaviour>();
                                //Take control using the token
                                myPlayer.TakeControl();
                            }
                        }
                    }

                    break;

                case 2:
                    //We can only spawn if we do not have a player currently and picked a team
                    if (!myPlayer && assignedTeamID >= 0)
                    {
                        //Check if we can currently spawn
                        if (!currentPvPGameModeBehaviour.UsesCustomSpawn())
                        {
                            if (gameInformation.allPvpGameModes[currentGameMode].CanSpawn(this, PhotonNetwork.LocalPlayer) || forceRespawn)
                            {
                                //Get a spawn
                                Transform spawnLocation = gameInformation.allPvpGameModes[currentGameMode].GetSpawn(this, PhotonNetwork.LocalPlayer);
                                if (spawnLocation)
                                {
                                    //Make SURE we are not spectating.
                                    if (spectatorManager)
                                    {
                                        spectatorManager.EndSpectating(this);
                                    }

                                    //Create object array for photon use
                                    object[] instData = new object[0];
                                    //Assign the values
                                    //0 = Team
                                    //3 = Primary
                                    //4 = Secondary
                                    //5 = Length of primary attachments
                                    //5 + length ... primary attachments
                                    //5 + length pa + 1 = Length of secondary attachments
                                    //5 + length pa + 2 ... secondary attachments
                                    if (loadoutMenu)
                                    {
                                        //Get the current loadout
                                        Loadout curLoadout = loadoutMenu.GetCurrentLoadout();
                                        int length = 1;
                                        Hashtable playerDataTable = new Hashtable();
                                        playerDataTable["team"] = assignedTeamID;
                                        playerDataTable["bot"] = false;
                                        playerDataTable["playerModelID"] = curLoadout.teamLoadout[assignedTeamID].playerModelID;
                                        playerDataTable["playerModelCustomizations"] = curLoadout.teamLoadout[assignedTeamID].playerModelCustomizations;
                                        length++; //How many weapons we spawn with
                                                  //Every weapon now has its own hashtable!
                                        length += curLoadout.loadoutWeapons.Length;
                                        //Create instData
                                        instData = new object[length];
                                        instData[0] = playerDataTable;
                                        instData[1] = curLoadout.loadoutWeapons.Length;
                                        for (int i = 0; i < curLoadout.loadoutWeapons.Length; i++)
                                        {
                                            Hashtable weaponTable = new Hashtable();
                                            weaponTable["slot"] = curLoadout.loadoutWeapons[i].goesToSlot;
                                            weaponTable["id"] = curLoadout.loadoutWeapons[i].weaponID;
                                            weaponTable["attachments"] = curLoadout.loadoutWeapons[i].attachments;
                                            instData[2 + i] = weaponTable;
                                        }
                                    }
                                    else
                                    {
                                        throw new System.Exception("No Loadout menu assigned. This is not allowed.");
                                    }
                                    GameObject go = PhotonNetwork.Instantiate(playerPrefab.name, spawnLocation.position, spawnLocation.rotation, 0, instData);
                                    //Copy player
                                    myPlayer = go.GetComponent<Kit_PlayerBehaviour>();
                                    //Take control using the token
                                    myPlayer.TakeControl();
                                }
                            }
                        }
                        else
                        {
                            GameObject player = currentPvPGameModeBehaviour.DoCustomSpawn(this);
                            if (player)
                            {
                                //Copy player
                                myPlayer = player.GetComponent<Kit_PlayerBehaviour>();
                                //Take control using the token
                                myPlayer.TakeControl();
                            }
                        }
                    }

                    break;
            }
        }

        private void InternalJoinTeam(int teamID)
        {
            Hashtable table = PhotonNetwork.LocalPlayer.CustomProperties;
            //Update our player's Hashtable
            table["team"] = teamID;
            PhotonNetwork.LocalPlayer.SetCustomProperties(table);
            //Assign local team ID
            assignedTeamID = teamID;
            //Call Event
            Kit_Events.onTeamSwitched.Invoke(teamID);
            //Tell all players that we switched teams
            PhotonNetwork.RaiseEvent(Kit_EventIDs.playerJoinedTeam, null, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
            //Loadout callback
            if (loadoutMenu)
            {
                loadoutMenu.TeamChanged(assignedTeamID);
            }
            //Voice Chat Callback
            try
            {
                if (voiceChat)
                {
                    voiceChat.JoinedTeam(teamID);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Voice Chat Error: " + e);
            }
            //Minimap callback
            if (minimap)
            {
                minimap.LocalPlayerSwitchedTeams(this);
            }

            //Should we attempt to spawn?
            if (ts.afterSelection == AfterTeamSelection.AttemptSpawn)
            {
                pluginOnForceClose.Invoke();
                pauseMenuState = PauseMenuState.main;
                //Activate scoreboard
                scoreboard.Enable();
                //Try to spawn
                Spawn();
                if (!myPlayer)
                {
                    SwitchMenu(pauseMenu.pauseMenuId, true);
                }
            }
            else if (ts.afterSelection == AfterTeamSelection.Loadout)
            {
                pauseMenuState = PauseMenuState.main;
                isPauseMenuOpen = true;
                //Activate scoreboard
                scoreboard.Enable();

                //Then go to loadout
                OpenLoadoutMenu();

                //TODO: SWITCH TO LOADOUT
            }
            else
            {
                SwitchMenu(pauseMenu.pauseMenuId, true);
                pauseMenuState = PauseMenuState.main;
                isPauseMenuOpen = true;
                //Activate scoreboard
                scoreboard.Enable();
            }

            //Call Plugin
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].LocalPlayerChangedTeam(this, teamID);
            }
        }

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;
            object content = photonEvent.CustomData;
            int senderId = photonEvent.Sender;
            if (currentPvPGameModeBehaviour)
            {
                //Relay
                currentPvPGameModeBehaviour.OnPhotonEvent(this, eventCode, content, senderId);
            }

            if (currentPvEGameModeBehaviour)
            {
                //Relay
                currentPvEGameModeBehaviour.OnPhotonEvent(this, eventCode, content, senderId);
            }
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].OnPhotonEvent(this, eventCode, content, senderId);
            }
            //Relay
            if (assistManager)
            {
                assistManager.OnPhotonEvent(this, eventCode, content, senderId);
            }
            //Find sender
            Photon.Realtime.Player sender = Kit_PhotonPlayerExtensions.Find(senderId);  // who sent this?
            //Player was killed
            if (eventCode == Kit_EventIDs.killEvent)
            {
                Hashtable deathInformation = (Hashtable)content;

                bool botShot = (bool)deathInformation[(byte)0];
                int killer = (int)deathInformation[(byte)1];
                bool botKilled = (bool)deathInformation[(byte)2];
                int killed = (int)deathInformation[(byte)3];

                //Update death stat
                if (botKilled)
                {
                    if (PhotonNetwork.IsMasterClient && currentBotManager)
                    {
                        Kit_Bot killedBot = currentBotManager.GetBotWithID(killed);
                        killedBot.deaths++;

                        for (int i = 0; i < gameInformation.plugins.Length; i++)
                        {
                            gameInformation.plugins[i].BotWasKilled(this, killedBot);
                        }
                    }
                }
                else
                {
                    if (killed == PhotonNetwork.LocalPlayer.ActorNumber)
                    {
                        Hashtable myTable = PhotonNetwork.LocalPlayer.CustomProperties;
                        int deaths = (int)myTable["deaths"];
                        deaths++;
                        myTable["deaths"] = deaths;
                        PhotonNetwork.LocalPlayer.SetCustomProperties(myTable);

                        for (int i = 0; i < gameInformation.plugins.Length; i++)
                        {
                            gameInformation.plugins[i].LocalPlayerWasKilled(this);
                        }

                        if (gameInformation.statistics)
                        {
                            //Check which statistics to call
                            if (deathInformation[(byte)4].GetType() == typeof(int))
                            {
                                int gun = (int)deathInformation[(byte)4];
                                //Call statistics
                                gameInformation.statistics.OnDeath(this, gun);
                            }
                            else if (deathInformation[(byte)4].GetType() == typeof(string))
                            {
                                string cause = (string)deathInformation[(byte)4];
                                //Call statistics
                                gameInformation.statistics.OnDeath(this, cause);
                            }
                        }
                    }
                }

                if (botShot)
                {
                    //Check if bot killed himself
                    if (!botKilled || botKilled && killer != killed)
                    {
                        if (PhotonNetwork.IsMasterClient && currentBotManager)
                        {
                            Kit_Bot killerBot = currentBotManager.GetBotWithID(killer);
                            killerBot.kills++;

                            if (PhotonNetwork.IsMasterClient)
                            {
                                //Call on game mode
                                currentPvPGameModeBehaviour.MasterClientBotScoredKill(this, killerBot);
                            }

                            for (int i = 0; i < gameInformation.plugins.Length; i++)
                            {
                                gameInformation.plugins[i].BotScoredKill(this, killerBot, deathInformation);
                            }
                        }
                    }
                }
                else
                {
                    if (killer == PhotonNetwork.LocalPlayer.ActorNumber && (botKilled || killed != PhotonNetwork.LocalPlayer.ActorNumber))
                    {
                        Hashtable myTable = PhotonNetwork.LocalPlayer.CustomProperties;
                        int kills = (int)myTable["kills"];
                        kills++;
                        myTable["kills"] = kills;
                        PhotonNetwork.LocalPlayer.SetCustomProperties(myTable);
                        //Display points
                        pointsUI.DisplayPoints(gameInformation.pointsPerKill, PointType.Kill);
                        //Add XP
                        if (gameInformation.leveling)
                        {
                            gameInformation.leveling.AddXp(this, gameInformation.pointsPerKill);
                        }
                        //Call on game mode
                        currentPvPGameModeBehaviour.LocalPlayerScoredKill(this);

                        //Call Plugins
                        for (int i = 0; i < gameInformation.plugins.Length; i++)
                        {
                            gameInformation.plugins[i].LocalPlayerScoredKill(this, deathInformation);
                        }

                        if (gameInformation.statistics)
                        {
                            //Check which statistics to call
                            if (deathInformation[(byte)4].GetType() == typeof(int))
                            {
                                int gun = (int)deathInformation[(byte)4];
                                //Call statistics
                                gameInformation.statistics.OnKill(this, gun);
                            }
                            else if (deathInformation[(byte)4].GetType() == typeof(string))
                            {
                                string cause = (string)deathInformation[(byte)4];
                                //Call statistics
                                gameInformation.statistics.OnKill(this, cause);
                            }
                        }
                    }
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    if (currentPvPGameModeBehaviour)
                    {
                        //Game Mode callback
                        currentPvPGameModeBehaviour.PlayerDied(this, botShot, killer, botKilled, killed);
                    }

                    if (currentPvEGameModeBehaviour)
                    {
                        //Game Mode callback
                        currentPvEGameModeBehaviour.PlayerDied(this, botShot, killer, botKilled, killed);
                    }
                }

                if (currentPvPGameModeBehaviour)
                {
                    if (deathInformation[(byte)4].GetType() == typeof(int))
                    {
                        int gun = (int)deathInformation[(byte)4];
                        int playerModel = (int)deathInformation[(byte)5];
                        int ragdollId = (int)deathInformation[(byte)6];
                        //Display in the killfeed
                        killFeed.Append(botShot, killer, botKilled, killed, gun, playerModel, ragdollId);
                    }
                    else if (deathInformation[(byte)4].GetType() == typeof(string))
                    {
                        string cause = (string)deathInformation[(byte)4];
                        int playerModel = (int)deathInformation[(byte)5];
                        int ragdollId = (int)deathInformation[(byte)6];
                        //Display in the killfeed
                        killFeed.Append(botShot, killer, botKilled, killed, cause, playerModel, ragdollId);
                    }
                }
            }
            //Request chat message
            else if (eventCode == Kit_EventIDs.requestChatMessage)
            {
                Hashtable chatInformation = (Hashtable)content;
                //Get information out of the hashtable
                int type = (int)chatInformation[(byte)0];
                //Message sent from player
                if (type == 0)
                {
                    //Master client only message
                    if (PhotonNetwork.IsMasterClient)
                    {
                        string message = (string)chatInformation[(byte)1];
                        int targets = (int)chatInformation[(byte)2];

                        //Check game mode
                        if (currentPvPGameModeBehaviour && currentPvPGameModeBehaviour.isTeamGameMode && targets == 1)
                        {
                            Hashtable CustomProperties = sender.CustomProperties;
                            //Send the message to this player's team only
                            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                            {
                                Hashtable CustomPropertiesReceiver = PhotonNetwork.PlayerList[i].CustomProperties;
                                //Check if we are in the same team
                                if (CustomProperties["team"] != null && CustomPropertiesReceiver["team"] != null && (int)CustomProperties["team"] == (int)CustomPropertiesReceiver["team"])
                                {
                                    Hashtable chatMessage = new Hashtable(3);
                                    chatMessage[(byte)0] = message;
                                    chatMessage[(byte)1] = targets;
                                    chatMessage[(byte)2] = senderId;
                                    //Send it to this player
                                    PhotonNetwork.RaiseEvent(Kit_EventIDs.chatMessageReceived, chatMessage, new RaiseEventOptions { TargetActors = new int[1] { PhotonNetwork.PlayerList[i].ActorNumber } }, SendOptions.SendReliable);
                                }
                            }
                        }
                        else
                        {
                            //Send the message to everyone
                            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                            {
                                Hashtable chatMessage = new Hashtable(3);
                                chatMessage[(byte)0] = message;
                                chatMessage[(byte)1] = 0; //Default to zero, since it is a non team based game mode
                                chatMessage[(byte)2] = senderId;
                                //Send it to this player
                                PhotonNetwork.RaiseEvent(Kit_EventIDs.chatMessageReceived, chatMessage, new RaiseEventOptions { TargetActors = new int[1] { PhotonNetwork.PlayerList[i].ActorNumber } }, SendOptions.SendReliable);
                            }
                        }
                    }
                }
                //Message sent directly from bot
                else if (type == 1)
                {
                    string botSender = (string)chatInformation[(byte)1];
                    int messageType = (int)chatInformation[(byte)2];

                    if (messageType == 0)
                    {
                        chat.BotJoined(botSender);
                    }
                    else if (messageType == 1)
                    {
                        chat.BotLeft(botSender);
                    }
                }
            }
            //Chat message received
            else if (eventCode == Kit_EventIDs.chatMessageReceived)
            {
                Hashtable chatInformation = (Hashtable)content;
                //Get sender
                Photon.Realtime.Player chatSender = Kit_PhotonPlayerExtensions.Find((int)chatInformation[(byte)2]);
                if (chatSender != null)
                {
                    //This is a final chat message, just display it.
                    chat.DisplayChatMessage(chatSender, (string)chatInformation[(byte)0], (int)chatInformation[(byte)1]);
                }
            }
            //Master Client asks us to reset ourselves.
            else if (eventCode == Kit_EventIDs.resetRequest)
            {
                //Reset Stats
                //Set initial Custom properties
                Hashtable myLocalTable = PhotonNetwork.LocalPlayer.CustomProperties;
                //Set inital team
                //2 = No Team
                //Set inital stats
                myLocalTable["kills"] = 0;
                myLocalTable["assists"] = 0;
                myLocalTable["deaths"] = 0;
                myLocalTable["assists"] = 0;
                myLocalTable["ping"] = PhotonNetwork.GetPing();
                myLocalTable["vote"] = -1; //For Map voting menu
                PhotonNetwork.LocalPlayer.SetCustomProperties(myLocalTable);
                //Kill our player and respawn
                if (myPlayer)
                {
                    PhotonNetwork.Destroy(myPlayer.photonView);
                }
                myPlayer = null;
                //Respawn
                Spawn();
            }
            //Start vote
            else if (eventCode == Kit_EventIDs.startVote)
            {
                if (playerStartedVoting)
                {
                    //Check if vote can be started
                    if (currentPvPGameModeBehaviour && currentPvPGameModeBehaviour.CanStartVote(this))
                    {
                        //Check if there is not vote in progress
                        if (!currentVoting)
                        {
                            //Get data
                            Hashtable voteInformation = (Hashtable)content;
                            int type = (byte)voteInformation[(byte)0];
                            int id = (int)voteInformation[(byte)1];

                            object[] data = new object[3];
                            data[0] = type; //Which type to vote on
                            data[1] = id; //What to vote on
                            data[2] = sender.ActorNumber; //Starter

                            PhotonNetwork.Instantiate(playerStartedVoting.name, transform.position, transform.rotation, 0, data);
                        }
                    }
                }
            }
            //Player joined team
            else if (eventCode == Kit_EventIDs.playerJoinedTeam)
            {
                if (currentBotManager && currentPvPGameModeBehaviour && currentPvPGameModeBehaviour.botManagerToUse && PhotonNetwork.IsMasterClient)
                {
                    currentPvPGameModeBehaviour.botManagerToUse.PlayerJoinedTeam(currentBotManager);
                }
            }
            //Spawn Scene Object event
            else if (eventCode == Kit_EventIDs.spawnSceneObject)
            {
                Hashtable instantiateInformation = (Hashtable)content;
                PhotonNetwork.InstantiateRoomObject((string)instantiateInformation[(byte)0], (Vector3)instantiateInformation[(byte)1], (Quaternion)instantiateInformation[(byte)2], (byte)instantiateInformation[(byte)3], (object[])instantiateInformation[(byte)4]);
            }
            //Hitmarker event
            else if (eventCode == Kit_EventIDs.hitMarkerEvent)
            {
                hud.DisplayHitmarker();
            }
            //XP Event
            else if (eventCode == Kit_EventIDs.xpEvent)
            {
                //Display points
                pointsUI.DisplayPoints((int)content, PointType.Kill);
                //Add XP
                if (gameInformation.leveling)
                {
                    gameInformation.leveling.AddXp(this, (int)content);
                }
            }
            //Master Client asks us to reset ourselves.
            else if (eventCode == Kit_EventIDs.respawnEvent)
            {
                if (PhotonNetwork.IsMasterClient && currentBotManager)
                {
                    currentBotManager.KillAllBots();
                }

                //Kill our player and respawn
                if (myPlayer)
                {
                    PhotonNetwork.Destroy(myPlayer.photonView);
                }
                myPlayer = null;
                //Respawn
                Spawn(true);
            }
        }

        /// <summary>
        /// Ends the game with the supplied Photon.Realtime.Player as winner
        /// </summary>
        /// <param name="winner">The Winner</param>
        public void EndGame(Kit_Player winner)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                object[] data = new object[3];
                //0 = Type of winner; 0 = Player; 1 = Team
                data[0] = 0;
                data[1] = winner.isBot;
                data[2] = winner.id;
                PhotonNetwork.InstantiateRoomObject(victoryScreen.name, Vector3.zero, Quaternion.identity, 0, data);

                //Call Event System
                Kit_Events.onEndGamePlayerWin.Invoke(winner);
            }
        }

        /// <summary>
        /// Ends the game with the supplied team (or 2 for draw) as winner
        /// </summary>
        /// <param name="winner">The winning team. 2 means draw.</param>
        public void EndGame(int winner)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                object[] data = new object[2];
                //0 = Type of winner; 0 = Player; 1 = Team
                data[0] = 1;
                data[1] = winner;
                PhotonNetwork.InstantiateRoomObject(victoryScreen.name, Vector3.zero, Quaternion.identity, 0, data);

                //Call Event System
                Kit_Events.onEndGameTeamWin.Invoke(winner);
            }
        }

        /// <summary>
        /// Ends the game and displays scores for two team
        /// </summary>
        /// <param name="winner"></param>
        /// <param name="scoreTeamOne"></param>
        /// <param name="scoreTeamTwo"></param>
        public void EndGame(int winner, int[] scores)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                object[] data = new object[3 + scores.Length];
                //0 = Type of winner; 0 = Player; 1 = Team
                data[0] = 1;
                data[1] = winner;
                data[2] = scores.Length;
                for (int i = 0; i < scores.Length; i++)
                {
                    data[3 + i] = scores[i];
                }
                PhotonNetwork.InstantiateRoomObject(victoryScreen.name, Vector3.zero, Quaternion.identity, 0, data);

                //Call Event System
                Kit_Events.onEndGameTeamWinWithScore.Invoke(winner, scores);
            }
        }

        /// <summary>
        /// Opens the voting menu if we are the master client
        /// </summary>
        public void OpenVotingMenu()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                List<MapGameModeCombo> usedCombos = new List<MapGameModeCombo>();

                //Get combos
                while (usedCombos.Count < mapVotingUI.amountOfAvailableVotes)
                {
                    //Get a new combo
                    usedCombos.Add(Kit_MapVotingBehaviour.GetMapGameModeCombo(gameInformation, usedCombos));
                }

                List<int> networkCombos = new List<int>();

                //Turn into an int list
                for (int i = 0; i < usedCombos.Count; i++)
                {
                    networkCombos.Add(usedCombos[i].gameMode);
                    networkCombos.Add(usedCombos[i].map);
                }

                object[] data = new object[mapVotingUI.amountOfAvailableVotes * 2];
                //Copy all combos
                for (int i = 0; i < networkCombos.Count; i++)
                {
                    data[i] = networkCombos[i];
                }

                PhotonNetwork.InstantiateRoomObject(mapVoting.name, Vector3.zero, Quaternion.identity, 0, data);
            }
        }

        /// <summary>
        /// Destroys all Players if we are the master client
        /// </summary>
        public void DeleteAllPlayers()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.PlayerList[i]);
                }

                if (currentBotManager)
                {
                    for (int i = 0; i < currentBotManager.bots.Count; i++)
                    {
                        if (currentBotManager.IsBotAlive(currentBotManager.bots[i]))
                        {
                            PhotonNetwork.Destroy(currentBotManager.GetAliveBot(currentBotManager.bots[i]).photonView);
                        }
                    }
                    currentBotManager.enabled = false;
                }
            }
        }

        /// <summary>
        /// Called when the victory screen opened
        /// </summary>
        public void VictoryScreenOpened()
        {
            //Reset alpha
            errorAlpha = 0f;
            //Force close loadout menu
            if (loadoutMenu)
            {
                loadoutMenu.ForceClose();
            }
        }

        /// <summary>
        /// Called when the map voting screen opened
        /// </summary>
        public void MapVotingOpened()
        {
            errorAlpha = 0f;
            //Force close loadout menu
            if (loadoutMenu)
            {
                loadoutMenu.ForceClose();
            }
        }

        /// <summary>
        /// Switches the map to
        /// </summary>
        /// <param name="to"></param>
        public void SwitchMap(int to)
        {
            if (PhotonNetwork.IsMasterClient && currentGameModeType == 2)
            {
                //Get the hashtable
                Hashtable table = PhotonNetwork.CurrentRoom.CustomProperties;
                //Update table
                table["gameMode"] = currentGameMode;
                table["map"] = to;
                PhotonNetwork.CurrentRoom.SetCustomProperties(table);
                if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
                {
                    //Load the map
                    Kit_SceneSyncer.instance.LoadScene(currentPvPGameModeBehaviour.traditionalMaps[to].sceneName);
                }
                else if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
                {
                    //Load the map
                    Kit_SceneSyncer.instance.LoadScene(currentPvPGameModeBehaviour.lobbyMaps[to].sceneName);
                }
            }
        }

        /// <summary>
        /// Switches the game mode to
        /// </summary>
        /// <param name="to"></param>
        public void SwitchGameMode(int to)
        {
            if (PhotonNetwork.IsMasterClient && currentGameModeType == 2)
            {
                //Get active map
                int map = gameInformation.GetCurrentLevel();
                //Get the hashtable
                Hashtable table = PhotonNetwork.CurrentRoom.CustomProperties;
                //Update table
                table["gameMode"] = to;
                table["map"] = map;
                PhotonNetwork.CurrentRoom.SetCustomProperties(table);
                if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
                {
                    //Load the map
                    Kit_SceneSyncer.instance.LoadScene(currentPvPGameModeBehaviour.traditionalMaps[map].sceneName);
                }
                else if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
                {
                    //Load the map
                    Kit_SceneSyncer.instance.LoadScene(currentPvPGameModeBehaviour.lobbyMaps[map].sceneName);
                }
            }
        }
        #endregion

        public void DisplayMessage(string msg)
        {
            //Display message
            errorText.text = msg;
            //Set alpha
            errorAlpha = errorTime;
        }

        #region ButtonCalls
        /// <summary>
        /// Attempt to join the team with teamID
        /// </summary>
        /// <param name="teamID"></param>
        public void JoinTeam(int teamID)
        {
            //We can just do this if we are in a room
            if (PhotonNetwork.InRoom)
            {
                //We only allow to change teams if we have not spawned
                if (!myPlayer)
                {
                    //Clamp the team id to the available teams
                    teamID = Mathf.Clamp(teamID, 0, Mathf.Clamp(gameInformation.allPvpTeams.Length, 0, currentPvPGameModeBehaviour.maximumAmountOfTeams) - 1);
                    //Check if we can join this team OR if we are already in that team
                    if (gameInformation.allPvpGameModes[currentGameMode].CanJoinTeam(this, PhotonNetwork.LocalPlayer, teamID) || teamID == assignedTeamID)
                    {
                        //Join the team
                        InternalJoinTeam(teamID);
                        //Hide message
                        errorAlpha = 0f;
                    }
                    else
                    {
                        //Display message
                        DisplayMessage("Could not join team");
                    }
                }
            }
        }

        public void NoTeam()
        {
            InternalJoinTeam(-1);
        }

        public void ChangeTeam()
        {
            //We only allow to change teams if we have not spawned
            if (!myPlayer)
            {
                SwitchMenu(ts.teamSelectionId);
                pauseMenuState = PauseMenuState.teamSelection;
            }
            else
            {
                //Commit suicide
                myPlayer.Suicide();
            }
        }

        /// <summary>
        /// Disconnect from the current room
        /// </summary>
        public void Disconnect()
        {
            //Save Leveling
            if (gameInformation.leveling)
            {
                gameInformation.leveling.Save();
            }
            if (gameInformation.statistics)
            {
                gameInformation.statistics.Save(this);
            }
            //Disconnect
            PhotonNetwork.Disconnect();
        }

        /// <summary>
        /// Press the resume button. Either locks cursor or tries to spawn
        /// </summary>
        public void ResumeButton()
        {
            //Check if we have spawned
            if (myPlayer)
            {
                //We have, just lock cursor
                //Close pause menu
                isPauseMenuOpen = false;
                SwitchMenu(ingameFadeId, true);
                pluginOnForceClose.Invoke();
                //Lock Cursor
                MarsScreen.lockCursor = true;
            }
            else if (currentPvPGameModeBehaviour && currentPvPGameModeBehaviour.CanSpawn(this, PhotonNetwork.LocalPlayer))
            {
                //We haven't, try to spawn
                Spawn();
            }
            else if (currentPvEGameModeBehaviour && currentPvEGameModeBehaviour.CanSpawn(this, PhotonNetwork.LocalPlayer))
            {
                //We haven't, try to spawn
                Spawn();
            }
            else if (spectatorManager && spectatorManager.IsCurrentlySpectating(this))
            {
                //Close pause menu
                isPauseMenuOpen = false;
                SwitchMenu(ingameFadeId, true);
                pluginOnForceClose.Invoke();
                //Lock Cursor
                MarsScreen.lockCursor = false;
            }
            else
            {
                //Close pause menu
                isPauseMenuOpen = false;
                SwitchMenu(ingameFadeId, true);
                pluginOnForceClose.Invoke();
                //Lock Cursor
                MarsScreen.lockCursor = false;
            }
        }

        /// <summary>
        /// Opens the loadout menu
        /// </summary>
        public void OpenLoadoutMenu()
        {
            //Check if something is blocking that
            if (!currentVictoryScreen && !currentMapVoting)
            {
                if (loadoutMenu)
                {
                    loadoutMenu.Open();
                }
            }
        }

        /// <summary>
        /// Opens the vote menu if no vote is in progress
        /// </summary>
        public void StartVote()
        {
            if (votingMenu)
            {
                votingMenu.OpenVotingMenu();
            }
        }

        public void OptionsButton()
        {
            if (options)
            {
                SwitchMenu(options.optionsScreenId);
            }
        }
        #endregion

        #region Plugin Calls
        /// <summary>
        /// Called when the menu is forcefully closed
        /// </summary>
        public UnityEvent pluginOnForceClose = new UnityEvent();

        public Button InjectButtonIntoPauseMenu(string txt)
        {
            GameObject go = Instantiate(pauseMenu.pluginButtonPrefab, pauseMenu.pluginButtonGo, false);
            go.transform.SetSiblingIndex(3);
            go.GetComponentInChildren<TextMeshProUGUI>().text = txt;
            return go.GetComponent<Button>();
        }
        #endregion

        #region Other Calls
        /// <summary>
        /// Opens or closes the pause menu
        /// </summary>
        /// <param name="open"></param>
        public void SetPauseMenuState(bool open, bool canLockCursor = true)
        {
            if (isPauseMenuOpen != open)
            {
                isPauseMenuOpen = open;
                //Set state
                if (isPauseMenuOpen)
                {
                    SwitchMenu(pauseMenu.pauseMenuId, true);
                    //Unlock cursor
                    MarsScreen.lockCursor = false;
                    //Chat callback
                    chat.PauseMenuOpened();
                    //Auto spawn system callack
                    if (autoSpawnSystem && currentPvPGameModeBehaviour)
                    {
                        autoSpawnSystem.Interruption();
                    }
                }
                else
                {
                    SwitchMenu(ingameFadeId, true);
                    pluginOnForceClose.Invoke();
                    if (canLockCursor)
                    {
                        //Lock cursor
                        MarsScreen.lockCursor = true;
                        //Chat callback
                        chat.PauseMenuClosed();
                    }
                }
            }
        }

        /// <summary>
        /// When the server tells us to reset ourselves!
        /// </summary>
        public UnityEvent pluginOnResetStats;

        public void ResetAllStatsEndOfRound()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    //Fetch old ping
                    int ping = 30;

                    if (PhotonNetwork.PlayerList[i].CustomProperties.ContainsKey("ping"))
                    {
                        ping = (int)PhotonNetwork.PlayerList[i].CustomProperties["ping"];
                    }

                    //Set initial Custom properties
                    Hashtable myLocalTable = new Hashtable();
                    //Set inital team
                    //2 = No Team
                    myLocalTable.Add("team", -1);
                    //Set inital stats
                    myLocalTable.Add("kills", 0);
                    myLocalTable.Add("assists", 0);
                    myLocalTable.Add("deaths", 0);
                    myLocalTable.Add("ping", ping);
                    myLocalTable.Add("vote", -1); //For Map voting menu

                    //Callbacks for game modes
                    if (currentPvPGameModeBehaviour)
                    {
                        currentPvPGameModeBehaviour.ResetStats(myLocalTable);
                    }

                    if (currentPvEGameModeBehaviour)
                    {
                        currentPvEGameModeBehaviour.ResetStats(myLocalTable);
                    }

                    PhotonNetwork.PlayerList[i].SetCustomProperties(myLocalTable);
                }
            }
        }

        /// <summary>
        /// Resets the player stats. Needs to be called at the end of the game. For everybody.
        /// </summary>
        public void ResetStats()
        {
            //Set initial Custom properties
            Hashtable myLocalTable = new Hashtable();
            //Set inital team
            //2 = No Team
            myLocalTable.Add("team", -1);
            //Set inital stats
            myLocalTable.Add("kills", 0);
            myLocalTable.Add("assists", 0);
            myLocalTable.Add("deaths", 0);
            myLocalTable.Add("ping", PhotonNetwork.GetPing());
            myLocalTable.Add("vote", -1); //For Map voting menu

            //Callbacks for game modes
            if (currentPvPGameModeBehaviour)
            {
                currentPvPGameModeBehaviour.ResetStats(myLocalTable);
            }

            if (currentPvEGameModeBehaviour)
            {
                currentPvEGameModeBehaviour.ResetStats(myLocalTable);
            }

            //Assign to player
            PhotonNetwork.LocalPlayer.SetCustomProperties(myLocalTable);

            //Call event
            pluginOnResetStats.Invoke();
        }

        /// <summary>
        /// Called when the game mode stage changes
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        void GameModeStageChanged(int from, int to)
        {
            //If we have gone back to 0 we need to call Start again. It can happen when the same map is played twice in a row since Photon does for some reason not sync the scene.
            if (to == 0 && from != 0)
            {
                Start();
            }
        }
        #endregion

        #region Menu Manager
        /// <summary>
        /// Call for buttons
        /// </summary>
        /// <param name="newMenu"></param>
        public void ChangeMenuButton(int newMenu)
        {
            if (!isSwitchingScreens)
            {
                //Start the coroutine
                StartCoroutine(SwitchRoutine(newMenu));
            }
        }

        public void ForceMenuActive(int menu)
        {
            //Disable all the roots
            for (int i = 0; i < menuScreens.Length; i++)
            {
                if (i != menu)
                {
                    if (menuScreens[i].root)
                    {
                        //Disable
                        menuScreens[i].root.SetActive(false);
                    }
                }
                else
                {
                    if (menuScreens[i].root)
                    {
                        //Disable
                        menuScreens[i].root.SetActive(true);
                    }
                }
            }
        }

        /// <summary>
        /// Switch to the given menu
        /// </summary>
        /// <param name="newMenu"></param>
        /// <returns></returns>
        public bool SwitchMenu(int newMenu)
        {
            Debug.Log("Requested switch from " + currentScreen + " to " + newMenu);

            if (currentScreen == newMenu) return true;

            if (!isSwitchingScreens)
            {
                //Start the coroutine
                currentlySwitchingScreensTo = StartCoroutine(SwitchRoutine(newMenu));
                //We are now switching
                return true;
            }

            //Not able to switch screens
            return false;
        }

        /// <summary>
        /// Switch to the given menu
        /// </summary>
        /// <param name="newMenu"></param>
        /// <returns></returns>
        public bool SwitchMenu(int newMenu, bool force)
        {
            Debug.Log("Requested switch from " + currentScreen + " to " + newMenu + ". Force? " + force);

            if (!isSwitchingScreens || force)
            {
                if (force)
                {
                    if (currentlySwitchingScreensTo != null)
                    {
                        StopCoroutine(currentlySwitchingScreensTo);
                    }

                    //Make sure all correct ones ARE disabled
                    //Disable all the roots
                    for (int i = 0; i < menuScreens.Length; i++)
                    {
                        if (i != currentScreen)
                        {
                            if (menuScreens[i].root)
                            {
                                //Disable
                                menuScreens[i].root.SetActive(false);
                            }
                        }
                    }
                }

                //Start the coroutine
                currentlySwitchingScreensTo = StartCoroutine(SwitchRoutine(newMenu));
                //We are now switching
                return true;
            }

            //Not able to switch screens
            return false;
        }

        private IEnumerator SwitchRoutine(int newMenu)
        {
            //Set bool
            isSwitchingScreens = true;
            if (wasFirstScreenFadedIn && currentScreen >= 0)
            {
                //Fade out screen
                //Play Animation
                menuScreens[currentScreen].anim.Play("Fade Out", 0, 0f);
                //Wait
                yield return new WaitForSeconds(menuScreens[currentScreen].fadeOutLength);
                menuScreens[currentScreen].root.SetActive(false);
            }

            //Fade in new screen
            //Set screen
            currentScreen = newMenu;
            if (currentScreen >= 0)
            {
                //Disable
                menuScreens[currentScreen].root.SetActive(true);
                //Play Animation
                menuScreens[currentScreen].anim.Play("Fade In", 0, 0f);
                //Wait
                yield return new WaitForSeconds(menuScreens[currentScreen].fadeInLength);
                //Set bool
                wasFirstScreenFadedIn = true;
            }
            //Done
            isSwitchingScreens = false;
        }
        #endregion
    }
}
