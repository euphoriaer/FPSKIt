using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;

namespace MarsFPSKit
{
    /// <summary>
    /// All input for the player (e.g. LMB, W,A,S,D, etc) should be stored here, so that bots may use the same scripts.
    /// </summary>
    public class Kit_PlayerInput
    {
        public float hor;
        public float ver;
        public bool crouch;
        public bool sprint;
        public bool jump;
        public bool dropWeapon;
        public bool lmb;
        public bool rmb;
        public bool reload;
        public float mouseX;
        public float mouseY;
        public bool leanLeft;
        public bool leanRight;
        public bool thirdPerson;
        public bool flashlight;
        public bool laser;
        public bool[] weaponSlotUses;
    }

    public class Kit_PlayerBehaviour : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Game Information
        [Header("Internal Game Information")]
        [Tooltip("This object contains all game information such as Maps, Game Modes and Weapons")]
        public Kit_GameInformation gameInformation;
        #endregion

        //This section contains everything for the local camera control
        #region Camera Control
        [Header("Camera Control")]
        public Transform playerCameraTransform;
        /// <summary>
        /// Transform that should be used for camera animations from weapons
        /// </summary>
        public Transform playerCameraAnimationTransform;
        /// <summary>
        /// Fall effects should be applied here
        /// </summary>
        public Transform playerCameraFallDownTransform;
        /// <summary>
        /// Hit reactions for camera should be applied here
        /// </summary>
        public Transform playerCameraHitReactionsTransform;
        #endregion

        //This section contains everything for the movement
        #region Movement
        [Header("Movement")]
        public Kit_MovementBase movement; //The system used for movement
        //Object used to store custom movement data
        [HideInInspector]
        public object customMovementData;

        /// <summary>
        /// Our Character Controller, assign it here
        /// </summary>
        public CharacterController cc;
        /// <summary>
        /// Our footstep audio source
        /// </summary>
        public AudioSource footstepSource;
        /// <summary>
        /// An audio source to play sounds from movement
        /// </summary>
        public AudioSource movementSoundSource;
        #endregion

        //This section contains everything for the Mouse Look
        #region Looking
        [Header("Mouse Look")]
        public Kit_MouseLookBase looking; //The system used for looking
        public Transform mouseLookObject; //The transform used for looking around
        [HideInInspector]
        /// <summary>
        /// This is used by the mouse looking script to apply the recoil and by the weapon script to set the recoil
        /// </summary>
        public Quaternion recoilApplyRotation;
        [HideInInspector]
        public object customMouseLookData; //Used to store custom mouse look data
        #endregion

        //This section contains everything for the weapons
        #region Weapons
        [Header("Weapons")]
        public Weapons.Kit_WeaponManagerBase weaponManager; //The system used for weapon management
        public Transform weaponsGo;
        /// <summary>
        /// Hit reactions for weapons should be applied here
        /// </summary>
        public Transform weaponsHitReactions;
        [HideInInspector]
        public object customWeaponManagerData; //Used to store custom weapon manager data

        /// <summary>
        /// Layermask for use with weapon Raycasts
        /// </summary>
        [Tooltip("These layers will be hit by Raycasts that weapons use")]
        public LayerMask weaponHitLayers;
        #endregion

        #region Player Vitals
        [Header("Player Vitals")]
        public Kit_VitalsBase vitalsManager;
        [HideInInspector]
        public object customVitalsData;
        #endregion

        #region Player Name UI
        [Header("Player Name UI")]
        public Kit_PlayerNameUIBase nameManager;
        public object customNameData;
        #endregion

        #region Spawn Protection
        [Header("Spawn Protection")]
        public Kit_SpawnProtectionBase spawnProtection;
        public object customSpawnProtectionData;
        #endregion

        #region Bots
        [Header("Bot Controls")]
        /// <summary>
        /// This module will control the behaviour of the bot
        /// </summary>
        public Kit_PlayerBotControlBase botControls;
        /// <summary>
        /// Use this to store runtime data for bot control
        /// </summary>
        public object botControlsRuntimeData;
        #endregion

        #region Voice
        [Header("Voice Manager")]
        /// <summary>
        /// If this is assigned, your characters can talk!
        /// </summary>
        public Kit_VoiceManagerBase voiceManager;

        /// <summary>
        /// use this  to store runtime data for the voice manager
        /// </summary>
        public object voiceManagerData;
        #endregion

        #region Input Manager
        [Header("Input Manager")]
        public Kit_InputManagerBase inputManager;
        /// <summary>
        /// Use this to store input manager runtime data
        /// </summary>
        public object inputManagerData;
        #endregion

        //This section contains internal variables
        #region Internal Variables
        //Team
        public int myTeam = -1;
        /// <summary>
        /// Returns true if this is our player
        /// </summary>
        public bool isController;

        /// <summary>
        /// True if first person view is active on this player. Does not mean that this is our local player. To determine actual perspective call looking.GetPerspective()
        /// </summary>
        public bool isFirstPersonActive
        {
            get
            {
                if (isController || isBeingSpectated)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Is input enabled?
        /// </summary>
        public bool enableInput
        {
            get
            {
                if (photonView.IsMine)
                {
                    if (isBot)
                    {
                        return canControlPlayer;
                    }
                    else
                    {
                        return MarsScreen.lockCursor && canControlPlayer;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Runtime data from the game mode can be stored here
        /// </summary>
        public object gameModeCustomRuntimeData;

        /// <summary>
        /// Input for the player, only assigned if we are controlling this player or we are the master client
        /// </summary>
        public Kit_PlayerInput input;

        /// <summary>
        /// Is this player being controlled by AI?
        /// </summary>
        [HideInInspector]
        public bool isBot;
        /// <summary>
        /// If this player is a bot, this is its ID
        /// </summary>
        [HideInInspector]
        public int botId;

        public int id
        {
            get
            {
                if (isBot) return botId;
                else return photonView.OwnerActorNr;
            }
        }
        [HideInInspector]
        public Kit_IngameMain main; //The main object of this scene, dynamically assigned at runtime

        [HideInInspector]
        //Position and rotation are synced by photon transform view
        public bool syncSetup;

        //We cache this value to avoid to calculate it many times
        [HideInInspector]
        public bool canControlPlayer = true;

        /// <summary>
        /// Currently active third person model
        /// </summary>
        [HideInInspector]
        public Kit_ThirdPersonPlayerModel thirdPersonPlayerModel;
        [HideInInspector]
        /// <summary>
        /// Id of the player model we are currently using! Only assigned in PvP game modes.
        /// </summary>
        public int thirdPersonPlayerModelID;
        [HideInInspector]
        /// <summary>
        /// Last forward vector from where we were shot
        /// </summary>
        public Vector3 ragdollForward;
        [HideInInspector]
        /// <summary>
        /// Last force which we were shot with
        /// </summary>
        public float ragdollForce;
        [HideInInspector]
        /// <summary>
        /// Last point from where we were shot
        /// </summary>
        public Vector3 ragdollPoint;
        [HideInInspector]
        /// <summary>
        /// Which collider should the force be applied to?
        /// </summary>
        public int ragdollId;
        [HideInInspector]
        /// <summary>
        /// The category to play
        /// </summary>
        public int deathSoundCategory;
        [HideInInspector]
        /// <summary>
        /// The specific sound to play
        /// </summary>
        public int deathSoundID;
        [HideInInspector]
        /// <summary>
        /// Who damaged us? For the assist manager.
        /// </summary>
        public List<AssistedKillData> damagedBy = new List<AssistedKillData>();
        [HideInInspector]
        /// <summary>
        /// Are we  currently being spectated?
        /// </summary>
        public bool isBeingSpectated;
        /// <summary>
        /// Position for sync
        /// </summary>
        private Vector3 syncPos;
        /// <summary>
        /// Rotation for sync
        /// </summary>
        private Quaternion syncRot;
        #endregion

        /// <summary>
        /// Sets up local player for controls, if owned by the local player
        /// </summary>
        public void TakeControl()
        {
            if (photonView.IsMine)
            {
                //Assign input
                input = new Kit_PlayerInput();
                //Start manager
                inputManager.InitializeControls(this);
                //Start coroutine to take control after player is setup.
                StartCoroutine(TakeControlWait());
            }
        }

        /// <summary>
        /// Because it can take a moment to set everything up, here we wait for it, then proceed.
        /// </summary>
        /// <returns></returns>
        IEnumerator TakeControlWait()
        {
            if (photonView.IsMine)
            {
                while (!thirdPersonPlayerModel) yield return null;

                //Lock the cursor
                MarsScreen.lockCursor = true;
                //Close pause menu
                Kit_IngameMain.isPauseMenuOpen = false;
                main.SwitchMenu(main.ingameFadeId, true);
                yield return new WaitForSeconds(main.menuScreens[main.currentScreen].fadeOutLength);
                //Move camera to the right position
                main.activeCameraTransform = playerCameraTransform;
                //Setup third person model
                //thirdPersonPlayerModel.FirstPerson();
                //Tell Minimap we spawned
                if (main.minimap)
                {
                    main.minimap.LocalPlayerSpawned(this);
                }
                //Tell touchscreen
                if (main.touchScreenCurrent)
                {
                    main.touchScreenCurrent.LocalPlayerSpawned(this);
                }
                //Auto spawn system callack
                if (main.autoSpawnSystem && main.currentPvPGameModeBehaviour)
                {
                    main.autoSpawnSystem.LocalPlayerSpawned();
                }
                if (main.currentPvPGameModeBehaviour)
                {
                    //Tell Game Mode
                    main.currentPvPGameModeBehaviour.OnLocalPlayerSpawned(this);
                }
                else if (main.currentPvEGameModeBehaviour)
                {
                    //Tell Game Mode
                    main.currentPvEGameModeBehaviour.OnLocalPlayerSpawned(this);
                }
                //Show HUD
                main.hud.SetVisibility(true);
                main.pluginOnForceClose.Invoke();
                //Call Plugin
                for (int i = 0; i < gameInformation.plugins.Length; i++)
                {
                    gameInformation.plugins[i].LocalPlayerSpawned(this);
                }

                //Call loadout
                main.loadoutMenu.LocalPlayerSpawned(this);
            }
        }

        #region Unity Calls
        void Start()
        {
            object[] instObjects = photonView.InstantiationData;
            Hashtable playerData = (Hashtable)instObjects[0];
            //Copy team
            myTeam = (int)playerData["team"];
            //Assign input if this is a bot
            isBot = (bool)playerData["bot"];
            //Set controller
            isController = !isBot && photonView.IsMine;

            if (isBot)
            {
                //Check for game mode override
                if (main.currentPvEGameModeBehaviour && main.currentPvEGameModeBehaviour.botControlOverride)
                {
                    botControls = main.currentPvEGameModeBehaviour.botControlOverride;
                }
                if (main.currentPvPGameModeBehaviour && main.currentPvPGameModeBehaviour.botControlOverride)
                {
                    botControls = main.currentPvPGameModeBehaviour.botControlOverride;
                }
                input = new Kit_PlayerInput();
                botId = (int)playerData["botid"];
                Kit_BotManager manager = FindObjectOfType<Kit_BotManager>();
                manager.AddActiveBot(this);
                //Initialize bot input
                botControls.InitializeControls(this);
            }

            int[] playerModelCustomizations = new int[0];
            GameObject go = null;

            switch (main.currentGameModeType)
            {
                case 0:
                    //Get player model
                    PlayerModelConfig pmcSp = main.currentPvEGameModeBehaviour.GetPlayerModel(this);
                    playerModelCustomizations = pmcSp.customization;

                    //Set up player model
                    //Instantiate one random player model for chosen team
                    go = Instantiate(pmcSp.information.prefab, transform, false);
                    //Assign
                    thirdPersonPlayerModel = go.GetComponent<Kit_ThirdPersonPlayerModel>();
                    //Set information
                    thirdPersonPlayerModel.information = pmcSp.information;
                    break;
                case 1:
                    //Get player model
                    PlayerModelConfig pmcCoop = main.currentPvEGameModeBehaviour.GetPlayerModel(this);
                    playerModelCustomizations = pmcCoop.customization;

                    //Set up player model
                    //Instantiate one random player model for chosen team
                    go = Instantiate(pmcCoop.information.prefab, transform, false);
                    //Assign
                    thirdPersonPlayerModel = go.GetComponent<Kit_ThirdPersonPlayerModel>();
                    //Set information
                    thirdPersonPlayerModel.information = pmcCoop.information;
                    break;
                case 2:
                    thirdPersonPlayerModelID = (int)playerData["playerModelID"];
                    playerModelCustomizations = (int[])playerData["playerModelCustomizations"];

                    //Set up player model
                    //Instantiate one random player model for chosen team
                    go = Instantiate(main.gameInformation.allPvpTeams[myTeam].playerModels[thirdPersonPlayerModelID].prefab, transform, false);
                    //Assign
                    thirdPersonPlayerModel = go.GetComponent<Kit_ThirdPersonPlayerModel>();
                    //And cache information
                    thirdPersonPlayerModel.information = main.gameInformation.allPvpTeams[myTeam].playerModels[thirdPersonPlayerModelID];
                    break;
            }

            //Reset scale
            go.transform.localScale = Vector3.one;
            //Setup
            thirdPersonPlayerModel.SetupModel(this);
            //Setup Customization
            thirdPersonPlayerModel.SetCustomizations(playerModelCustomizations, this);
            //Make it third person initially
            thirdPersonPlayerModel.ThirdPerson();

            if (thirdPersonPlayerModel.firstPersonArmsPrefab.Count == 0)
            {
                Debug.LogWarning("WARNING: Player Model does not have ANY first person arms prefabs assigned! Game might be broken! To fix, assign first person arms prefabs to this player model with your key (Default Key: Kit)", thirdPersonPlayerModel.gameObject);
            }

            if (main.currentGameModeType == 2)
            {
                //Start Spawn Protection
                if (spawnProtection)
                {
                    spawnProtection.CustomStart(this);
                }
            }
            else
            {
                //Spawn protection not used in SP / Coop
                spawnProtection = null;
            }

            //Setup weapon manager
            weaponManager.SetupManager(this, photonView.InstantiationData);

            //Setup Vitals
            vitalsManager.Setup(this);

            //Setup Looking
            looking.Setup(this);

            if (isBot)
            {
                //Setup marker
                if (nameManager)
                {
                    nameManager.StartRelay(this);
                }

                //Setup voice
                if (voiceManager)
                {
                    voiceManager.SetupOwner(this);
                }

                //Tell Minimap we spawned
                if (main.minimap)
                {
                    main.minimap.PlayerSpawned(this);
                }

                //Call Plugin
                for (int i = 0; i < gameInformation.plugins.Length; i++)
                {
                    gameInformation.plugins[i].PlayerSpawned(this);
                }

                if (main.currentPvPGameModeBehaviour)
                {
                    //Tell Game Mode
                    main.currentPvPGameModeBehaviour.OnPlayerSpawned(this);
                }
                else if (main.currentPvPGameModeBehaviour)
                {
                    //Tell Game Mode
                    main.currentPvEGameModeBehaviour.OnPlayerSpawned(this);
                }
            }
            else
            {
                //Setup weapon manager for the others
                if (!photonView.IsMine)
                {

                    //Setup marker
                    if (nameManager)
                    {
                        nameManager.StartRelay(this);
                    }

                    // Setup voice
                    if (voiceManager)
                    {
                        voiceManager.SetupOthers(this);
                    }

                    //Tell Minimap we spawned
                    if (main.minimap)
                    {
                        main.minimap.PlayerSpawned(this);
                    }

                    //Call Plugin
                    for (int i = 0; i < gameInformation.plugins.Length; i++)
                    {
                        gameInformation.plugins[i].PlayerSpawned(this);
                    }

                    if (main.currentPvPGameModeBehaviour)
                    {
                        //Tell Game Mode
                        main.currentPvPGameModeBehaviour.OnPlayerSpawned(this);
                    }
                    else if (main.currentPvEGameModeBehaviour)
                    {
                        //Tell Game Mode
                        main.currentPvEGameModeBehaviour.OnPlayerSpawned(this);
                    }
                }
                else
                {
                    main.hud.PlayerStart(this);
                    //Disable our own name hitbox
                    thirdPersonPlayerModel.enemyNameAboveHeadTrigger.enabled = false;
                    // Setup voice
                    if (voiceManager)
                    {
                        voiceManager.SetupOwner(this);
                    }
                }
            }

            syncSetup = true;

            //Call event system
            Kit_Events.onPlayerSpawned.Invoke(this);

            //Add us to player list
            main.allActivePlayers.Add(this);

            //Set name of player
            if (isBot)
            {
                Kit_Bot bot = main.currentBotManager.GetBotWithID(botId);
                if (bot == null)
                {
                    gameObject.name = "Bot";
                }
                else
                {
                    gameObject.name = bot.name;
                }
            }
            else
            {
                gameObject.name = photonView.Owner.NickName;
            }

            //Spectator call. Do this last so that this player is completely setup.
            if (main.spectatorManager)
            {
                main.spectatorManager.PlayerWasSpawned(main, this);
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            //Find main reference
            main = FindObjectOfType<Kit_IngameMain>();
        }

        bool isShuttingDown = false;

        void OnApplicationQuit()
        {
            isShuttingDown = true;
        }

        void OnDestroy()
        {
            if (!isShuttingDown)
            {
                //Hide HUD if we were killed
                if (isController)
                {
                    main.hud.SetVisibility(false);
                }
                if (!photonView.IsMine || !photonView.IsOwnerActive || isBot)
                {
                    //Release marker
                    if (nameManager)
                    {
                        nameManager.OnDestroyRelay(this);
                    }
                }

                if (isController)
                {
                    //Tell minimap
                    if (main.minimap)
                    {
                        main.minimap.LocalPlayerDied(this);
                    }
                    //Tell touchscreen
                    if (main.touchScreenCurrent)
                    {
                        main.touchScreenCurrent.LocalPlayerDied(this);
                    }
                    //Auto spawn system callack
                    if (main.autoSpawnSystem && main.currentPvPGameModeBehaviour)
                    {
                        main.autoSpawnSystem.LocalPlayerDied();
                    }
                    if (main.currentPvPGameModeBehaviour)
                    {
                        //Tell Game Mode
                        main.currentPvPGameModeBehaviour.OnLocalPlayerDestroyed(this);
                    }
                    else if (main.currentPvEGameModeBehaviour)
                    {
                        //Tell Game Mode
                        main.currentPvEGameModeBehaviour.OnLocalPlayerDestroyed(this);
                    }
                    //Tell HUD
                    main.hud.PlayerEnd(this);
                    //Call Plugin
                    for (int i = 0; i < gameInformation.plugins.Length; i++)
                    {
                        gameInformation.plugins[i].LocalPlayerDied(this);
                    }
                }
                else
                {
                    //Tell minimap
                    if (main.minimap)
                    {
                        main.minimap.PlayerDied(this);
                    }
                    if (main.currentPvPGameModeBehaviour)
                    {
                        //Tell Game Mode
                        main.currentPvPGameModeBehaviour.OnPlayerDestroyed(this);
                    }
                    else if (main.currentPvEGameModeBehaviour)
                    {
                        //Tell Game Mode
                        main.currentPvEGameModeBehaviour.OnPlayerDestroyed(this);
                    }
                    //Call Plugin
                    for (int i = 0; i < gameInformation.plugins.Length; i++)
                    {
                        gameInformation.plugins[i].PlayerDied(this);
                    }
                }

                //Spectator call
                if (main.spectatorManager)
                {
                    main.spectatorManager.PlayerWasKilled(main, this);
                }

                //Make sure the camera never gets destroyed
                if (main.activeCameraTransform == playerCameraTransform)
                {
                    main.activeCameraTransform = main.spawnCameraPosition;
                    //Set Fov
                    main.mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                }

                if (PhotonNetwork.InRoom && canControlPlayer)
                {
                    if (thirdPersonPlayerModel)
                    {
                        //Unparent sounds
                        thirdPersonPlayerModel.soundFire.transform.parent = null;
                        if (thirdPersonPlayerModel.soundFire.clip)
                        {
                            Destroy(thirdPersonPlayerModel.soundFire.gameObject, thirdPersonPlayerModel.soundFire.clip.length);
                        }
                        else
                        {
                            Destroy(thirdPersonPlayerModel.soundFire.gameObject, 1f);
                        }

                        //Setup ragdoll
                        thirdPersonPlayerModel.CreateRagdoll();
                    }
                }

                //Call event system
                Kit_Events.onPlayerDied.Invoke(this);

                //Remove us from list
                main.allActivePlayers.Remove(this);
            }
        }

        void Update()
        {
            if (photonView)
            {
                //If we are not the owner of the photonView, we need to update position and rotation
                if (!photonView.IsMine)
                {
                    if (isBot)
                    {
                        if (main.currentGameModeType == 2 && main.currentPvPGameModeBehaviour.AreWeEnemies(main, true, botId))
                        {
                            if (nameManager)
                            {
                                nameManager.UpdateEnemy(this);
                            }
                        }
                        else
                        {
                            if (nameManager)
                            {
                                nameManager.UpdateFriendly(this);
                            }
                            if (main.minimap)
                            {
                                main.minimap.PlayerFriendlyUpdate(this);
                            }
                        }
                    }
                    else
                    {
                        if (main.currentGameModeType == 2 && main.currentPvPGameModeBehaviour.AreWeEnemies(main, false, photonView.Owner.ActorNumber))
                        {
                            if (nameManager)
                            {
                                nameManager.UpdateEnemy(this);
                            }
                        }
                        else
                        {
                            if (nameManager)
                            {
                                nameManager.UpdateFriendly(this);
                            }
                            if (main.minimap)
                            {
                                main.minimap.PlayerFriendlyUpdate(this);
                            }
                        }
                    }

                    //Transform sync
                    transform.position = Vector3.Lerp(transform.position, syncPos, Time.deltaTime * 15f);
                    transform.rotation = Quaternion.Slerp(transform.rotation, syncRot, Time.deltaTime * 15f);

                    //Call Plugin
                    for (int i = 0; i < gameInformation.plugins.Length; i++)
                    {
                        gameInformation.plugins[i].PlayerUpdate(this);
                    }
                }
                else if (isBot)
                {
                    if (main.currentGameModeType == 2 && main.currentPvPGameModeBehaviour.AreWeEnemies(main, true, botId))
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateEnemy(this);
                        }
                    }
                    else
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateFriendly(this);
                        }
                    }
                }

                if (syncSetup)
                {
                    //Weapon manager update
                    weaponManager.CustomUpdate(this);
                    looking.CalculateLookUpdate(this);
                    movement.CalculateMovementUpdate(this);
                    vitalsManager.CustomUpdate(this);
                }

                if (photonView.IsMine)
                {
                    if (!isBot)
                    {
                        inputManager.WriteToPlayerInput(this);
                    }
                    else
                    {
                        //Get Bot Input
                        botControls.WriteToPlayerInput(this);
                        //Call Plugin
                        for (int i = 0; i < gameInformation.plugins.Length; i++)
                        {
                            gameInformation.plugins[i].PlayerUpdate(this);
                        }
                    }

                    if (main.currentPvPGameModeBehaviour)
                    {
                        //Update control value
                        canControlPlayer = main.currentPvPGameModeBehaviour.CanControlPlayer(main);
                    }
                    else if (main.currentPvEGameModeBehaviour)
                    {
                        //Update control value
                        canControlPlayer = main.currentPvEGameModeBehaviour.CanControlPlayer(main);
                    }

                    //If we are the controller, update everything
                    if (isController || isBot && PhotonNetwork.IsMasterClient)
                    {
                        //Update spawn protection
                        if (spawnProtection)
                        {
                            spawnProtection.CustomUpdate(this);
                        }

                        if (!isBot)
                        {
                            //Call Plugin
                            for (int i = 0; i < gameInformation.plugins.Length; i++)
                            {
                                gameInformation.plugins[i].LocalPlayerUpdate(this);
                            }
                        }
                    }
                }

                if (isFirstPersonActive)
                {
                    //Update hud
                    if (main && main.hud)
                    {
                        main.hud.PlayerUpdate(this);
                    }
                    //Update minimap
                    if (main.minimap)
                    {
                        main.minimap.LocalPlayerUpdate(this);
                    }
                }

                //Footstep callback
                movement.CalculateFootstepsUpdate(this);

            }
        }

        void LateUpdate()
        {
            //If we are the controller, update everything
            if (isController || isBot && PhotonNetwork.IsMasterClient)
            {
                movement.CalculateMovementLateUpdate(this);
                looking.CalculateLookLateUpdate(this);
            }

            //If we are not the owner of the photonView, we need to update position and rotation
            if (!photonView.IsMine)
            {
                if (isBot)
                {
                    if (main.currentGameModeType == 2 && main.currentPvPGameModeBehaviour.AreWeEnemies(main, true, botId))
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateEnemy(this);
                        }
                    }
                    else
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateFriendly(this);
                        }
                    }
                }
                else
                {
                    if (main.currentGameModeType == 2 && main.currentPvPGameModeBehaviour.AreWeEnemies(main, false, photonView.Owner.ActorNumber))
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateEnemy(this);
                        }
                    }
                    else
                    {
                        if (nameManager)
                        {
                            nameManager.UpdateFriendly(this);
                        }
                    }
                }
            }
            else if (isBot)
            {
                if (main.currentGameModeType == 2 && main.currentPvPGameModeBehaviour.AreWeEnemies(main, true, botId))
                {
                    if (nameManager)
                    {
                        nameManager.UpdateEnemy(this);
                    }
                }
                else
                {
                    if (nameManager)
                    {
                        nameManager.UpdateFriendly(this);
                    }
                }
            }
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            //Relay to movement script
            movement.OnControllerColliderHitRelay(this, hit);
            //Relay to mouse look script
            looking.OnControllerColliderHitRelay(this, hit);
            //Relay to weapon manager
            weaponManager.OnControllerColliderHitRelay(this, hit);
        }

        void OnTriggerEnter(Collider col)
        {
            if (photonView.IsMine)
            {
                //Check for amo
                if (col.transform.root.GetComponent<Kit_AmmoPickup>())
                {
                    Kit_AmmoPickup pickup = col.transform.root.GetComponent<Kit_AmmoPickup>();
                    //Relay to weapon manager
                    weaponManager.OnAmmoPickup(this, pickup);
                    //Hide first
                    pickup.renderRoot.SetActive(false);
                    //Destroy (RPC)
                    pickup.photonView.RPC("PickedUp", RpcTarget.MasterClient);
                }
                else if (col.transform.root.GetComponent<Kit_HealthPickup>())
                {
                    Kit_HealthPickup pickup = col.transform.root.GetComponent<Kit_HealthPickup>();
                    //Relay to health
                    vitalsManager.ApplyHeal(this, pickup.healthRestored);
                    //Hide first
                    pickup.renderRoot.SetActive(false);
                    //Destroy (RPC)
                    pickup.photonView.RPC("PickedUp", RpcTarget.MasterClient);
                }
            }

            //Relay to weapon manager
            weaponManager.OnTriggerEnterRelay(this, col);

            //Relay to movement
            movement.OnTriggerEnterRelay(this, col);
        }

        void OnTriggerExit(Collider col)
        {
            //Relay to weapon manager
            weaponManager.OnTriggerExitRelay(this, col);

            //Relay to movement
            movement.OnTriggerExitRelay(this, col);
        }
        #endregion

        #region Photon Calls
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //Sync data for ragdoll
            if (stream.IsWriting)
            {
                stream.SendNext(ragdollForce);
                stream.SendNext(ragdollForward);
                stream.SendNext(ragdollId);
                stream.SendNext(ragdollPoint);
                stream.SendNext(deathSoundCategory);
                stream.SendNext(deathSoundID);
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            else
            {
                ragdollForce = (float)stream.ReceiveNext();
                ragdollForward = (Vector3)stream.ReceiveNext();
                ragdollId = (int)stream.ReceiveNext();
                ragdollPoint = (Vector3)stream.ReceiveNext();
                deathSoundCategory = (int)stream.ReceiveNext();
                deathSoundID = (int)stream.ReceiveNext();
                syncPos = (Vector3)stream.ReceiveNext();

                if (Vector3.Distance(transform.position, syncPos) > 5f)
                {
                    transform.position = syncPos;
                }

                syncRot = (Quaternion)stream.ReceiveNext();
            }
            //Movement
            movement.OnPhotonSerializeView(this, stream, info);
            //Mouse Look
            looking.OnPhotonSerializeView(this, stream, info);
            //Spawn Protection
            if (spawnProtection)
            {
                spawnProtection.OnPhotonSerializeView(this, stream, info);
            }
            //Vitals
            vitalsManager.OnPhotonSerializeView(this, stream, info);
            //Weapon manager
            weaponManager.OnPhotonSerializeView(this, stream, info);
            //Relay
            if (isBot)
            {
                //Bot Controls
                botControls.OnPhotonSerializeView(this, stream, info);
            }
            if (main.currentPvPGameModeBehaviour)
            {
                //Game Mode Relay
                main.currentPvPGameModeBehaviour.PlayerOnPhotonSerializeView(this, stream, info);
            }
            else if (main.currentPvEGameModeBehaviour)
            {
                //Game Mode Relay
                main.currentPvEGameModeBehaviour.PlayerOnPhotonSerializeView(this, stream, info);
            }
            //Call Plugin
            for (int i = 0; i < gameInformation.plugins.Length; i++)
            {
                gameInformation.plugins[i].PlayerOnPhotonSerializeView(this, stream, info);
            }
        }
        #endregion

        #region Custom Calls
        public void LocalDamage(float dmg, int gunID, Vector3 shotPos, Vector3 forward, float force, Vector3 hitPos, int id, bool botShot, int idWhoShot)
        {
            if (main.assistManager)
            {
                main.assistManager.PlayerDamaged(main, botShot, idWhoShot, this, dmg);
            }

            ragdollForce = force;
            ragdollForward = forward;
            ragdollId = id;
            ragdollPoint = hitPos;
            deathSoundCategory = gameInformation.allWeapons[gunID].deathSoundCategory;

            if (photonView)
            {
                if (isBot)
                {
                    //Tell that player that we hit him
                    photonView.RPC("ApplyDamageNetwork", RpcTarget.MasterClient, dmg, botShot, idWhoShot, gunID, shotPos, forward, force, hitPos, id);
                }
                else
                {
                    //Tell that player that we hit him
                    photonView.RPC("ApplyDamageNetwork", photonView.Owner, dmg, botShot, idWhoShot, gunID, shotPos, forward, force, hitPos, id);
                }
            }
        }

        public void LocalDamage(float dmg, string deathCause, Vector3 shotPos, Vector3 forward, float force, Vector3 hitPos, int id, bool botShot, int idWhoShot)
        {
            if (main.assistManager)
            {
                main.assistManager.PlayerDamaged(main, botShot, idWhoShot, this, dmg);
            }

            ragdollForce = force;
            ragdollForward = forward;
            ragdollId = id;
            ragdollPoint = hitPos;
            deathSoundCategory = gameInformation.allWeapons[0].deathSoundCategory;

            if (photonView)
            {
                if (isBot)
                {
                    //Tell that player that we hit him
                    photonView.RPC("ApplyDamageNetwork", RpcTarget.MasterClient, dmg, botShot, idWhoShot, deathCause, shotPos, forward, force, hitPos, id);
                }
                else
                {
                    //Tell that player that we hit him
                    photonView.RPC("ApplyDamageNetwork", photonView.Owner, dmg, botShot, idWhoShot, deathCause, shotPos, forward, force, hitPos, id);
                }
            }
        }

        public void LocalBlind(float time, int gunID, Vector3 shotPos, bool botShot, int idWhoShot)
        {
            if (photonView)
            {
                if (isBot)
                {
                    //Tell that player that we hit him
                    photonView.RPC("ApplyBlindNetwork", RpcTarget.MasterClient, time, gunID, shotPos, botShot, idWhoShot);
                }
                else
                {
                    //Tell that player that we hit him
                    photonView.RPC("ApplyBlindNetwork", photonView.Owner, time, gunID, shotPos, botShot, idWhoShot);
                }
            }
        }

        public void ApplyFallDamage(float dmg)
        {
            if (isController && photonView)
            {
                vitalsManager.ApplyFallDamage(this, dmg);
            }
        }

        public void Suicide()
        {
            if (isController && photonView)
            {
                vitalsManager.Suicide(this);
            }
        }

        /// <summary>
        /// Kill the player by cause.
        /// </summary>
        /// <param name="cause"></param>
        public void Die(int cause)
        {
            if (photonView)
            {
                if (photonView.IsMine)
                {
                    if (main.assistManager)
                    {
                        main.assistManager.PlayerKilled(main, isBot, id, this);
                    }

                    //Tell weapon manager
                    weaponManager.PlayerDead(this);
                    //Tell master client we were killed
                    byte evCode = Kit_EventIDs.killEvent; //Event 0 = player dead
                                                          //Create a table that holds our death information
                    Hashtable deathInformation = new Hashtable(7);
                    if (isBot)
                    {
                        deathInformation[(byte)0] = true;
                        //Who killed us?
                        deathInformation[(byte)1] = botId;
                    }
                    else
                    {
                        deathInformation[(byte)0] = false;
                        //Who killed us?
                        deathInformation[(byte)1] = photonView.Owner.ActorNumber;
                    }
                    //Who was killed?
                    deathInformation[(byte)2] = isBot;
                    if (isBot)
                    {
                        deathInformation[(byte)3] = botId;
                    }
                    else
                    {
                        deathInformation[(byte)3] = photonView.Owner.ActorNumber;
                    }
                    deathInformation[(byte)4] = cause;
                    //Give our player model ID
                    deathInformation[(byte)5] = thirdPersonPlayerModelID;
                    //Ragdoll ID
                    deathInformation[(byte)6] = ragdollId;
                    PhotonNetwork.RaiseEvent(evCode, deathInformation, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
                    //Destroy the player
                    PhotonNetwork.Destroy(photonView);
                }
            }
        }

        public void Die(bool botShot, int killer, int gunID)
        {
            if (photonView)
            {
                if (photonView.IsMine)
                {
                    if (main.assistManager)
                    {
                        main.assistManager.PlayerKilled(main, botShot, killer, this);
                    }

                    //Tell weapon manager
                    weaponManager.PlayerDead(this);
                    //Tell master client we were killed
                    byte evCode = Kit_EventIDs.killEvent; //Event 0 = player dead
                    //Create a table that holds our death information
                    Hashtable deathInformation = new Hashtable(7);
                    deathInformation[(byte)0] = botShot;
                    //Who killed us?
                    deathInformation[(byte)1] = killer;
                    //Who was killed?
                    deathInformation[(byte)2] = isBot;
                    if (isBot)
                    {
                        deathInformation[(byte)3] = botId;
                    }
                    else
                    {
                        deathInformation[(byte)3] = photonView.Owner.ActorNumber;
                    }
                    //With which weapon were we killed?
                    deathInformation[(byte)4] = gunID;
                    //Give our player model ID
                    deathInformation[(byte)5] = thirdPersonPlayerModelID;
                    //Ragdoll ID
                    deathInformation[(byte)6] = ragdollId;
                    PhotonNetwork.RaiseEvent(evCode, deathInformation, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
                    //Destroy the player
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }

        public void Die(bool botShot, int killer, string cause)
        {
            if (photonView)
            {
                if (photonView.IsMine)
                {
                    if (main.assistManager)
                    {
                        main.assistManager.PlayerKilled(main, botShot, killer, this);
                    }

                    //Tell weapon manager
                    weaponManager.PlayerDead(this);
                    //Tell master client we were killed
                    byte evCode = Kit_EventIDs.killEvent; //Event 0 = player dead
                    //Create a table that holds our death information
                    Hashtable deathInformation = new Hashtable(7);
                    deathInformation[(byte)0] = botShot;
                    //Who killed us?
                    deathInformation[(byte)1] = killer;
                    //Who was killed?
                    deathInformation[(byte)2] = isBot;
                    if (isBot)
                    {
                        deathInformation[(byte)3] = botId;
                    }
                    else
                    {
                        deathInformation[(byte)3] = photonView.Owner.ActorNumber;
                    }
                    //With which weapon were we killed?
                    deathInformation[(byte)4] = cause;
                    //Give our player model ID
                    deathInformation[(byte)5] = thirdPersonPlayerModelID;
                    //Ragdoll ID
                    deathInformation[(byte)6] = ragdollId;
                    PhotonNetwork.RaiseEvent(evCode, deathInformation, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
                    //Destroy the player
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
        #endregion

        #region RPCs
        [PunRPC]
        public void ApplyDamageNetwork(float dmg, bool botShot, int idWhoShot, int gunID, Vector3 shotPos, Vector3 forward, float force, Vector3 hitPos, int id)
        {
            if (isController || isBot && PhotonNetwork.IsMasterClient)
            {
                ragdollForce = force;
                ragdollForward = forward;
                ragdollPoint = hitPos;
                ragdollId = id;
                deathSoundCategory = gameInformation.allWeapons[gunID].deathSoundCategory;
                if (voiceManager)
                {
                    deathSoundID = voiceManager.GetDeathSoundID(this, deathSoundCategory);
                }
                //Relay to the assigned manager
                vitalsManager.ApplyDamage(this, dmg, botShot, idWhoShot, gunID, shotPos);
                if (!isBot)
                {
                    //Tell HUD
                    main.hud.DisplayShot(shotPos);
                }
            }
        }

        [PunRPC]
        public void ApplyDamageNetwork(float dmg, bool botShot, int idWhoShot, string deathCause, Vector3 shotPos, Vector3 forward, float force, Vector3 hitPos, int id)
        {
            if (isController || isBot && PhotonNetwork.IsMasterClient)
            {
                ragdollForce = force;
                ragdollForward = forward;
                ragdollPoint = hitPos;
                ragdollId = id;
                deathSoundCategory = gameInformation.allWeapons[0].deathSoundCategory;
                if (voiceManager)
                {
                    deathSoundID = voiceManager.GetDeathSoundID(this, deathSoundCategory);
                }
                //Relay to the assigned manager
                vitalsManager.ApplyDamage(this, dmg, botShot, idWhoShot, deathCause, shotPos);
                if (!isBot)
                {
                    //Tell HUD
                    main.hud.DisplayShot(shotPos);
                }
            }
        }

        [PunRPC]
        public void ApplyBlindNetwork(float time, int gunID, Vector3 shotPos, bool botShot, int idWhoShot)
        {
            if (isController || isBot && PhotonNetwork.IsMasterClient)
            {
                if (!isBot)
                {
                    main.hud.DisplayBlind(time);
                    //Tell HUD
                    main.hud.DisplayShot(shotPos);
                }
            }
        }

        //If we fire using a semi auto weapon, this is called
        [PunRPC]
        public void WeaponSemiFireNetwork()
        {
            //Relay to weapon manager
            weaponManager.NetworkSemiRPCReceived(this);
        }

        //If we fire using a bolt action weapon, this is called
        [PunRPC]
        public void WeaponBoltActionFireNetwork(int state)
        {
            //Relay to weapon manager
            weaponManager.NetworkBoltActionRPCReceived(this, state);
        }

        [PunRPC]
        public void WeaponBurstFireNetwork(int burstLength)
        {
            //Relay to weapon manager
            weaponManager.NetworkBurstRPCReceived(this, burstLength);
        }

        [PunRPC]
        public void WeaponFirePhysicalBulletOthers(Vector3 pos, Vector3 dir)
        {
            //Relay to weapon manager
            weaponManager.NetworkPhysicalBulletFired(this, pos, dir);
        }

        //When we reload, this is called
        [PunRPC]
        public void WeaponReloadNetwork(bool empty)
        {
            //Reload to weapon manager
            weaponManager.NetworkReloadRPCReceived(this, empty);
        }

        //When a procedural reload occurs, this will be called with the correct stage
        [PunRPC]
        public void WeaponProceduralReloadNetwork(int stage)
        {
            //Relay to weapon manager
            weaponManager.NetworkProceduralReloadRPCReceived(this, stage);
        }

        [PunRPC]
        public void WeaponRaycastHit(Vector3 pos, Vector3 normal, string material)
        {
            //Relay to impact processor
            main.impactProcessor.ProcessImpact(main, pos, normal, material);
        }

        [PunRPC]
        public void MeleeStabNetwork(int state, int slot)
        {
            //Send to player model
            thirdPersonPlayerModel.PlayMeleeAnimation(0, state);
            //Weapon Manager
            weaponManager.NetworkMeleeStabRPCReceived(this, state, slot);
        }

        [PunRPC]
        public void MeleeChargeNetwork(int id, int slot)
        {
            //Send to player model
            thirdPersonPlayerModel.PlayMeleeAnimation(1, id);
            //Weapon Manager
            weaponManager.NetworkMeleeChargeRPCReceived(this, id, slot);
        }

        [PunRPC]
        public void MeleeHealNetwork(int id)
        {
            //Send to playyer model
            thirdPersonPlayerModel.PlayMeleeAnimation(2, id);
            //Weapon Manager
            weaponManager.NetworkMeleeHealRPCReceived(this, id);
        }

        [PunRPC]
        public void GrenadePullPinNetwork()
        {
            //Relay
            weaponManager.NetworkGrenadePullPinRPCReceived(this);
        }

        [PunRPC]
        public void GrenadeThrowNetwork()
        {
            //Relay
            weaponManager.NetworkGrenadeThrowRPCReceived(this);
        }

        [PunRPC]
        public void WeaponRestockAll(bool allWeapons)
        {
            if (photonView.IsMine)
            {
                //Relay
                weaponManager.RestockAmmo(this, allWeapons);
            }
        }

        [PunRPC]
        public void ReplaceWeapon(int[] slot, int weapon, int bulletsLeft, int bulletsLeftToReload, int[] attachments)
        {
            //Relay to weapon manager
            weaponManager.NetworkReplaceWeapon(this, slot, weapon, bulletsLeft, bulletsLeftToReload, attachments);
        }

        [PunRPC]
        public void PlayVoiceLine(int catId, int id)
        {
            if (voiceManager)
            {
                voiceManager.PlayVoiceRpcReceived(this, catId, id);
            }
        }

        [PunRPC]
        public void PlayVoiceLine(int catId, int id, int idTwo)
        {
            if (voiceManager)
            {
                voiceManager.PlayVoiceRpcReceived(this, catId, id, idTwo);
            }
        }

        [PunRPC]
        public void MovementPlaySound(int id, int id2, int arrayID)
        {
            //Relay to movement
            movement.PlaySound(this, id, id2, arrayID);
        }

        [PunRPC]
        public void MovementPlayAnimation(int id, int id2)
        {
            //Relay to movement
            movement.PlayAnimation(this, id, id2);
        }
        #endregion

        #region Spectating
        /// <summary>
        /// Begin spectating this player
        /// </summary>
        public void OnSpectatingStart()
        {
            //Move camera to the right position
            main.activeCameraTransform = playerCameraTransform;
            //Set bool
            isBeingSpectated = true;

            if (looking.GetPerspective(this) == Kit_GameInformation.Perspective.FirstPerson)
            {
                //Hide third person object
                thirdPersonPlayerModel.FirstPerson();
            }
            else
            {

            }

            //Call Weapon Manager
            weaponManager.BeginSpectating(this);

            //HUD
            main.hud.SetVisibility(true);

            //Minimap
            if (main.minimap)
            {
                main.minimap.LocalPlayerSpawned(this);
            }
        }

        /// <summary>
        /// End spectating this player
        /// </summary>
        public void OnSpectatingEnd()
        {
            //Set bool
            isBeingSpectated = false;
            //Third Person
            thirdPersonPlayerModel.ThirdPerson();

            //Weapon Manager
            weaponManager.EndSpectating(this);

            //HUD
            main.hud.SetVisibility(false);

            //Minimap
            if (main.minimap)
            {
                main.minimap.LocalPlayerDied(this);
            }
        }
        #endregion
    }
}
