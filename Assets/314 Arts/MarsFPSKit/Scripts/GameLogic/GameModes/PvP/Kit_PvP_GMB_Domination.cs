using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using MarsFPSKit.Spectating;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarsFPSKit
{
    //Runtime data class
    public class DominationRuntimeData
    {
        /// <summary>
        /// Points scored by each team
        /// </summary>
        public int[] teamPoints;

        /// <summary>
        /// When did the last tick occur?
        /// </summary>
        public double lastTick;

        /// <summary>
        /// Flags currently used
        /// </summary>
        public DominationFlagData[] flags;
    }

    public class DominationFlagData
    {
        /// <summary>
        /// Who currently owns this flag?
        /// </summary>
        public int currentOwner;

        /// <summary>
        /// What is currently happening with this flag?
        /// -1 = multiple teams are capping
        /// 0 = nothing
        /// 1 + x = capture process (team 0 + x)
        /// </summary>
        public int currentState;

        /// <summary>
        /// Progress of capturing this flag
        /// </summary>
        public float captureProgress = 0f;

        /// <summary>
        /// Smoothed <see cref="captureProgress"/>
        /// </summary>
        public float smoothedCaptureProgress = 0f;

        /// <summary>
        /// If one team is capturing the flag, this is how many they are (min = 1)
        /// </summary>
        public int playersCapturingFlag = 1;

        /// <summary>
        /// The physical non synced representation of this map
        /// </summary>
        public Kit_Domination_FlagRuntime flagObject;

        /// <summary>
        /// Spawn points for this flag
        /// </summary>
        public List<Kit_PlayerSpawn> spawnForFlag = new List<Kit_PlayerSpawn>();

        /// <summary>
        /// Nav points for capturing this flag!
        /// </summary>
        public List<Kit_BotNavPoint> navPointsForFlag = new List<Kit_BotNavPoint>();
    }

    [CreateAssetMenu(menuName = ("MarsFPSKit/Gamemodes/Domination Logic"))]
    public class Kit_PvP_GMB_Domination : Kit_PvP_GameModeBase
    {
        [Header("Domination Settings")]
        /// <summary>
        /// How many points does a team need to win?
        /// </summary>
        public int pointLimit = 200;

        /// <summary>
        /// How many points does a team get per owned flag per tick?
        /// </summary>
        public int pointsPerOwnedFlag = 1;

        /// <summary>
        /// How long is a tick (time between addition of points)? It is recommended to use something larger than .256 seconds because of network in accuracies in photon
        /// </summary>
        public float tickTime = 5f;

        /// <summary>
        /// Speed at which flag capturing reaches 100% (base)
        /// </summary>
        public float flagCaptureSpeed = 5;

        /// <summary>
        /// Multiplier at which the flag capture speed is multiplied per player capturing
        /// </summary>
        public float flagCaptureSpeedPlayerCountMultiplier = 2f;

        /// <summary>
        /// The prefab used for the flags
        /// </summary>
        public GameObject flagPrefab;

        /// <summary>
        /// Material used when flag is neutral
        /// </summary>
        public Material flagMaterialNeutral;
        /// <summary>
        /// Material used by team one
        /// </summary>
        public Material[] flagMaterialTeams;

        /// <summary>
        /// Flag is owned by no one and no one is capturing it
        /// </summary>
        [Header("HUD Colors")]
        public Color hudColorNeutral = Color.white;
        /// <summary>
        /// Both teams are trying to capture this flag
        /// </summary>
        public Color hudColorFlagFightedFor = Color.yellow;

        [Tooltip("The maximum amount of difference the teams can have in player count")]
        /// <summary>
        /// The maximum amount of difference the teams can have in player count
        /// </summary>
        public int maxTeamDifference = 2;

        /// <summary>
        /// How many seconds need to be left in order to be able to start a vote?
        /// </summary>
        public float votingThreshold = 30f;

        [Header("Times")]
        /// <summary>
        /// How many seconds until we can start playing? This is the first countdown during which players cannot move or do anything other than spawn or chat.
        /// </summary>
        public float preGameTime = 20f;

        /// <summary>
        /// How many seconds until the map/gamemode voting menu is opened
        /// </summary>
        public float endGameTime = 10f;

        /// <summary>
        /// How many seconds do we have to vote on the next map and game mode?
        /// </summary>
        public float mapVotingTime = 20f;

        /// <summary>
        /// Spawn layer used for team one during countdown
        /// </summary>
        [Tooltip("Spawn layer used for team one during countdown")]
        [Header("Spawns")]
        public int[] teamInitialSpawnLayer;
        /// <summary>
        /// Spawn layer used for team two during gameplay
        /// </summary>
        [Tooltip("Spawn layer used for team one during gameplay")]
        public int[] teamGameplaySpawnLayer;
        /// <summary>
        /// What is the first index for flag spawns ? 
        /// </summary>
        [Tooltip("What is the first index for flag spawns?")]
        public int firstFlagSpawnIndex = 3;

        public override Spectateable GetSpectateable(Kit_IngameMain main)
        {
            if (main.assignedTeamID >= 0) return Spectateable.Friendlies;

            return Spectateable.All;
        }

        public override bool CanJoinTeam(Kit_IngameMain main, Photon.Realtime.Player player, int team)
        {
            int amountOfPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
            //Check if the team has met its limits
            if (team == 0 && playersInTeamOne >= (amountOfPlayers / 2))
            {
                return false;
            }
            else if (team == 1 && playersInTeamTwo >= (amountOfPlayers / 2))
            {
                return false;
            }

            //Check if the difference is too big
            if (team == 0)
            {
                if (playersInTeamOne - playersInTeamTwo > maxTeamDifference) return false;
            }
            else if (team == 1)
            {
                if (playersInTeamTwo - playersInTeamOne > maxTeamDifference) return false;
            }

            //If none of the excluding factors were met, return true
            return true;
        }

        public override void GamemodeSetup(Kit_IngameMain main)
        {
            //Assign runtime data
            DominationRuntimeData drd = new DominationRuntimeData();
            drd.teamPoints = new int[Mathf.Clamp(main.gameInformation.allPvpTeams.Length, 0, maximumAmountOfTeams)];
            main.currentGameModeRuntimeData = drd;

            //Get all spawns
            Kit_PlayerSpawn[] allSpawns = FindObjectsOfType<Kit_PlayerSpawn>();
            //Are there any spawns at all?
            if (allSpawns.Length <= 0) throw new Exception("This scene has no spawns.");

            //Get all flags
            Kit_Domination_Flag[] allFlags = FindObjectsOfType<Kit_Domination_Flag>().OrderBy(m => m.transform.GetSiblingIndex()).ToArray();

            //Create array of correct length
            drd.flags = new DominationFlagData[allFlags.Length];

            //Instantiate all flags
            for (int i = 0; i < allFlags.Length; i++)
            {
                GameObject go = Instantiate(flagPrefab, allFlags[i].transform.position, allFlags[i].transform.rotation);
                //Assign to array
                drd.flags[i] = new DominationFlagData();
                drd.flags[i].flagObject = go.GetComponent<Kit_Domination_FlagRuntime>();
                //Setup
                drd.flags[i].flagObject.Setup(allFlags[i]);
            }

            //Filter all spawns that are appropriate for this game mode
            List<Kit_PlayerSpawn> filteredSpawns = new List<Kit_PlayerSpawn>();
            //Highest spawn index
            int highestIndex = 0;

            for (int i = 0; i < allSpawns.Length; i++)
            {
                int id = i;
                if (allSpawns[id].pvpGameModes.Contains(this))
                {
                    if (allSpawns[id].spawnGroupID >= firstFlagSpawnIndex)
                    {
                        for (int o = 0; o < drd.flags.Length; o++)
                        {
                            if (allSpawns[id].spawnGroupID == firstFlagSpawnIndex + o)
                            {
                                drd.flags[o].spawnForFlag.Add(allSpawns[id]);
                            }
                        }
                    }
                    else
                    {
                        //Add it to the list
                        filteredSpawns.Add(allSpawns[id]);
                        //Set highest index
                        if (allSpawns[id].spawnGroupID > highestIndex) highestIndex = allSpawns[id].spawnGroupID;
                    }
                }
            }

            Kit_BotNavPoint[] navPoints = FindObjectsOfType<Kit_BotNavPoint>();

            List<Kit_BotNavPoint> dominationNavPoints = new List<Kit_BotNavPoint>();
            for (int i = 0; i < navPoints.Length; i++)
            {
                if (navPoints[i].gameModes.Contains(this))
                {
                    dominationNavPoints.Add(navPoints[i]);
                }
            }

            for (int i = 0; i < dominationNavPoints.Count; i++)
            {
                for (int o = 0; o < drd.flags.Length; o++)
                {
                    if (dominationNavPoints[i].navPointGroupID == 1 + o)
                    {
                        drd.flags[o].navPointsForFlag.Add(dominationNavPoints[i]);
                    }
                }
            }

            //Setup spawn list
            main.internalSpawns = new List<InternalSpawns>();
            for (int i = 0; i < (highestIndex + 1); i++)
            {
                main.internalSpawns.Add(null);
            }
            //Setup spawn lists
            for (int i = 0; i < main.internalSpawns.Count; i++)
            {
                int id = i;
                main.internalSpawns[id] = new InternalSpawns();
                main.internalSpawns[id].spawns = new List<Kit_PlayerSpawn>();
                for (int o = 0; o < filteredSpawns.Count; o++)
                {
                    int od = o;
                    if (filteredSpawns[od].spawnGroupID == id)
                    {
                        main.internalSpawns[id].spawns.Add(filteredSpawns[od]);
                    }
                }
            }

            //Set stage and timer
            main.gameModeStage = 0;
            main.timer = preGameTime;
        }

        public override void GameModeUpdate(Kit_IngameMain main)
        {
            if (main.currentGameModeRuntimeData != null && main.currentGameModeRuntimeData.GetType() == typeof(DominationRuntimeData))
            {
                DominationRuntimeData drd = main.currentGameModeRuntimeData as DominationRuntimeData;
                if (drd != null)
                {
                    //Update flags
                    UpdateFlagProgression(main, drd);

                    //Update points every tick
                    if (PhotonNetwork.Time > drd.lastTick)
                    {
                        drd.lastTick = PhotonNetwork.Time + tickTime;
                        OneTick(drd);
                        if (main.gameModeStage < 2)
                        {
                            CheckForWinner(main);
                        }
                    }

                    //Update Flag materials
                    UpdateFlagMaterial(drd);

                    //Smooth progress
                    for (int i = 0; i < drd.flags.Length; i++)
                    {
                        drd.flags[i].smoothedCaptureProgress = Mathf.Lerp(drd.flags[i].smoothedCaptureProgress, drd.flags[i].captureProgress, Time.deltaTime * 10f);
                    }
                }
            }
        }

        public override void GameModeUpdateOthers(Kit_IngameMain main)
        {
            if (main.currentGameModeRuntimeData != null && main.currentGameModeRuntimeData.GetType() == typeof(DominationRuntimeData))
            {
                DominationRuntimeData drd = main.currentGameModeRuntimeData as DominationRuntimeData;
                if (drd != null)
                {
                    //Update Flag materials
                    UpdateFlagMaterial(drd);

                    //Smooth progress
                    for (int i = 0; i < drd.flags.Length; i++)
                    {
                        drd.flags[i].smoothedCaptureProgress = Mathf.Lerp(drd.flags[i].smoothedCaptureProgress, drd.flags[i].captureProgress, Time.deltaTime * 10f);
                    }
                }
            }
        }

        void OneTick(DominationRuntimeData drd)
        {
            //Check through all flags
            for (int i = 0; i < drd.flags.Length; i++)
            {
                if (drd.flags[i].currentOwner > 0)
                {
                    drd.teamPoints[drd.flags[i].currentOwner - 1] += pointsPerOwnedFlag;
                }
            }

            for (int i = 0; i < drd.teamPoints.Length; i++)
            {
                //Clamp
                drd.teamPoints[i] = Mathf.Clamp(drd.teamPoints[i], 0, pointLimit);
            }
        }

        void UpdateFlagProgression(Kit_IngameMain main, DominationRuntimeData drd)
        {
            for (int i = 0; i < drd.flags.Length; i++)
            {
                if (drd.flags[i].currentState == 0)
                {
                    drd.flags[i].captureProgress = 0f;
                }
                else if (drd.flags[i].currentState >= 1)
                {
                    if (drd.flags[i].currentOwner != drd.flags[i].currentState)
                    {
                        //Calculate multiplier
                        float playerMultiplier = 1;
                        for (int o = 0; o < drd.flags[i].playersCapturingFlag; o++)
                        {
                            //Only if more than one player
                            if (o > 0)
                                playerMultiplier *= flagCaptureSpeedPlayerCountMultiplier;
                        }
                        drd.flags[i].captureProgress += Time.deltaTime * flagCaptureSpeed * playerMultiplier;
                        if (drd.flags[i].captureProgress >= 100f)
                        {
                            //Set flag owned by that team
                            drd.flags[i].currentOwner = drd.flags[i].currentState;
                            drd.flags[i].captureProgress = 0f;
                            //Inform bots
                            if (main.currentBotManager)
                            {
                                for (int o = 0; o < main.currentBotManager.bots.Count; o++)
                                {
                                    int t = o;
                                    if (main.currentBotManager.IsBotAlive(main.currentBotManager.bots[t]))
                                    {
                                        Kit_PlayerBehaviour pb = main.currentBotManager.GetAliveBot(main.currentBotManager.bots[t]);
                                        int p = i;
                                        (pb.botControls as Kit_PlayerDominationBotControl).FlagCaptured(pb, p, 0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        void UpdateFlagMaterial(DominationRuntimeData drd)
        {
            for (int i = 0; i < drd.flags.Length; i++)
            {
                drd.flags[i].flagObject.UpdateFlag(drd.flags[i].currentOwner, this);
            }
        }

        /// <summary>
        /// Called when the player state in a flag has changed
        /// </summary>
        /// <param name="main"></param>
        /// <param name="flag"></param>
        public void FlagStateChanged(Kit_IngameMain main, Kit_Domination_FlagRuntime flag)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (main.currentGameModeRuntimeData != null && main.currentGameModeRuntimeData.GetType() == typeof(DominationRuntimeData))
                {
                    DominationRuntimeData drd = main.currentGameModeRuntimeData as DominationRuntimeData;
                    //Find id
                    int id = 0;
                    for (int i = 0; i < drd.flags.Length; i++)
                    {
                        if (drd.flags[i].flagObject == flag)
                        {
                            id = i;
                            break;
                        }
                    }

                    if (flag.playersInTrigger.Count > 0)
                    {
                        bool[] teamCapping = new bool[Mathf.Clamp(main.gameInformation.allPvpTeams.Length, 0, maximumAmountOfTeams)];
                        for (int i = 0; i < flag.playersInTrigger.Count; i++)
                        {
                            teamCapping[flag.playersInTrigger[i].myTeam] = true;
                        }

                        bool oneTeamCapping = false;
                        bool multipleTeamsCapping = false;

                        for (int i = 0; i < teamCapping.Length; i++)
                        {
                            if (teamCapping[i])
                            {
                                if (!oneTeamCapping)
                                {
                                    oneTeamCapping = true;
                                }
                                else
                                {
                                    multipleTeamsCapping = true;
                                    break;
                                }
                            }
                        }

                        if (multipleTeamsCapping)
                        {
                            drd.flags[id].currentState = -1;
                            drd.flags[id].playersCapturingFlag = flag.playersInTrigger.Count;
                        }
                        else
                        {
                            int curTeamCapping = -1;

                            for (int i = 0; i < teamCapping.Length; i++)
                            {
                                int teamId = i;

                                if (teamCapping[i])
                                {
                                    curTeamCapping = teamId;
                                    break;
                                }
                            }

                            if (curTeamCapping == -1)
                            {
                                drd.flags[id].currentState = 0;
                                drd.flags[id].playersCapturingFlag = 0;
                            }
                            else
                            {
                                drd.flags[id].currentState = 1 + curTeamCapping;
                                drd.flags[id].playersCapturingFlag = flag.playersInTrigger.Count;
                            }
                        }
                    }
                    else
                    {
                        drd.flags[id].currentState = 0;
                        drd.flags[id].playersCapturingFlag = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Checks all players in <see cref="PhotonNetwork.PlayerList"/> if they reached the kill limit, if the game is not over already
        /// </summary>
        void CheckForWinner(Kit_IngameMain main)
        {
            //Check if someone can still win
            if (main.gameModeStage < 2 && AreEnoughPlayersThere(main))
            {
                //Ensure we are using the correct runtime data
                if (main.currentGameModeRuntimeData == null || main.currentGameModeRuntimeData.GetType() != typeof(DominationRuntimeData))
                {
                    DominationRuntimeData ndrd = new DominationRuntimeData();
                    ndrd.teamPoints = new int[Mathf.Clamp(main.gameInformation.allPvpTeams.Length, 0, maximumAmountOfTeams)];
                    main.currentGameModeRuntimeData = ndrd;
                }
                DominationRuntimeData drd = main.currentGameModeRuntimeData as DominationRuntimeData;

                for (int i = 0; i < drd.teamPoints.Length; i++)
                {
                    if (drd.teamPoints[i] >= pointLimit)
                    {
                        //End Game
                        main.EndGame(i, drd.teamPoints);
                        //Set game stage
                        main.timer = endGameTime;
                        main.gameModeStage = 2;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// How many players are in team one?
        /// </summary>
        int playersInTeamOne
        {
            get
            {
                int toReturn = 0;

                //Loop through all players
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    //Check if that player joined a team
                    if (PhotonNetwork.PlayerList[i].CustomProperties["team"] != null)
                    {
                        //Check if he is in team one
                        if ((int)PhotonNetwork.PlayerList[i].CustomProperties["team"] == 0) toReturn++;
                    }
                }

                //Return
                return toReturn;
            }
        }

        /// <summary>
        /// How many players are in team two?
        /// </summary>
        int playersInTeamTwo
        {
            get
            {
                int toReturn = 0;

                //Loop through all players
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    //Check if that player joined a team
                    if (PhotonNetwork.PlayerList[i].CustomProperties["team"] != null)
                    {
                        //Check if he is in team two
                        if ((int)PhotonNetwork.PlayerList[i].CustomProperties["team"] == 1) toReturn++;
                    }
                }

                //Return
                return toReturn;
            }
        }

        public override Transform GetSpawn(Kit_IngameMain main, Photon.Realtime.Player player)
        {
            Transform spawnToReturn = null;

            //Start spawns
            if (main.gameModeStage == 0)
            {
                int tries = 0;
                while (tries < 10 && !spawnToReturn)
                {
                    int team = (int)player.CustomProperties["team"];
                    int layer = teamInitialSpawnLayer[Mathf.Clamp(team, 0, teamInitialSpawnLayer.Length - 1)];
                    Transform spawnToTest = main.internalSpawns[layer].spawns[UnityEngine.Random.Range(0, main.internalSpawns[layer].spawns.Count)].transform;
                    //Test the spawn
                    if (spawnToTest)
                    {
                        if (spawnSystemToUse.CheckSpawnPosition(main, spawnToTest, player))
                        {
                            //Assign spawn
                            spawnToReturn = spawnToTest;
                            //Break the while loop
                            break;
                        }
                    }
                    tries++;
                }
            }
            else
            {
                DominationRuntimeData drd = main.currentGameModeRuntimeData as DominationRuntimeData;

                int tries = 0;
                while (tries < 10 && !spawnToReturn)
                {
                    for (int i = 0; i < drd.flags.Length; i++)
                    {
                        if (drd.flags[i].currentOwner == ((int)player.CustomProperties["team"] + 1))
                        {
                            Transform spawnToTest = drd.flags[i].spawnForFlag[UnityEngine.Random.Range(0, drd.flags[i].spawnForFlag.Count)].transform;
                            //Test the spawn
                            if (spawnToTest)
                            {
                                if (spawnSystemToUse.CheckSpawnPosition(main, spawnToTest, player))
                                {
                                    //Assign spawn
                                    spawnToReturn = spawnToTest;
                                    //Break the while loop
                                    break;
                                }
                            }
                            tries++;
                        }
                    }
                    tries++;
                }
                //Use backup spawns if there is still nothing
                if (!spawnToReturn)
                {
                    //Reset tries
                    tries = 0;
                    while (tries < 10 && !spawnToReturn)
                    {
                        int team = (int)player.CustomProperties["team"];
                        int layer = teamGameplaySpawnLayer[Mathf.Clamp(team, 0, teamGameplaySpawnLayer.Length - 1)];
                        //Backup spawns = spawns[0]
                        Transform spawnToTest = main.internalSpawns[layer].spawns[UnityEngine.Random.Range(0, main.internalSpawns[layer].spawns.Count)].transform;
                        //Test the spawn
                        if (spawnToTest)
                        {
                            if (spawnSystemToUse.CheckSpawnPosition(main, spawnToTest, player))
                            {
                                //Assign spawn
                                spawnToReturn = spawnToTest;
                                //Break the while loop
                                break;
                            }
                        }
                        tries++;
                    }
                }
            }

            return spawnToReturn;
        }

        public override Transform GetSpawn(Kit_IngameMain main, Kit_Bot bot)
        {
            Transform spawnToReturn = null;

            //Start spawns
            if (main.gameModeStage == 0)
            {
                int tries = 0;
                while (tries < 10 && !spawnToReturn)
                {
                    int team = bot.team;
                    int layer = teamInitialSpawnLayer[Mathf.Clamp(team, 0, teamInitialSpawnLayer.Length - 1)];
                    Transform spawnToTest = main.internalSpawns[layer].spawns[UnityEngine.Random.Range(0, main.internalSpawns[layer].spawns.Count)].transform;
                    //Test the spawn
                    if (spawnToTest)
                    {
                        if (spawnSystemToUse.CheckSpawnPosition(main, spawnToTest, bot))
                        {
                            //Assign spawn
                            spawnToReturn = spawnToTest;
                            //Break the while loop
                            break;
                        }
                    }
                    tries++;
                }
            }
            else
            {
                DominationRuntimeData drd = main.currentGameModeRuntimeData as DominationRuntimeData;

                int tries = 0;
                while (tries < 10 && !spawnToReturn)
                {
                    for (int i = 0; i < drd.flags.Length; i++)
                    {
                        if (drd.flags[i].currentOwner == (bot.team + 1))
                        {
                            Transform spawnToTest = drd.flags[i].spawnForFlag[UnityEngine.Random.Range(0, drd.flags[i].spawnForFlag.Count)].transform;
                            //Test the spawn
                            if (spawnToTest)
                            {
                                if (spawnSystemToUse.CheckSpawnPosition(main, spawnToTest, bot))
                                {
                                    //Assign spawn
                                    spawnToReturn = spawnToTest;
                                    //Break the while loop
                                    break;
                                }
                            }
                            tries++;
                        }
                    }
                    tries++;
                }
                //Use backup spawns if there is still nothing
                if (!spawnToReturn)
                {
                    //Reset tries
                    tries = 0;
                    while (tries < 10 && !spawnToReturn)
                    {
                        int layer = teamGameplaySpawnLayer[Mathf.Clamp(bot.team, 0, teamGameplaySpawnLayer.Length - 1)];
                        //Backup spawns = spawns[0]
                        Transform spawnToTest = main.internalSpawns[layer].spawns[UnityEngine.Random.Range(0, main.internalSpawns[layer].spawns.Count)].transform;
                        //Test the spawn
                        if (spawnToTest)
                        {
                            if (spawnSystemToUse.CheckSpawnPosition(main, spawnToTest, bot))
                            {
                                //Assign spawn
                                spawnToReturn = spawnToTest;
                                //Break the while loop
                                break;
                            }
                        }
                        tries++;
                    }
                }
            }

            return spawnToReturn;
        }

        public override void PlayerDied(Kit_IngameMain main, bool botKiller, int killer, bool botKilled, int killed)
        {
            DominationRuntimeData drd = main.currentGameModeRuntimeData as DominationRuntimeData;
            //Update flags
            for (int i = 0; i < drd.flags.Length; i++)
            {
                drd.flags[i].flagObject.PlayerDied();
            }
        }

        public override void TimeRunOut(Kit_IngameMain main)
        {
            //Ensure we are using the correct runtime data
            if (main.currentGameModeRuntimeData == null || main.currentGameModeRuntimeData.GetType() != typeof(DominationRuntimeData))
            {
                DominationRuntimeData ndrd = new DominationRuntimeData();
                ndrd.teamPoints = new int[Mathf.Clamp(main.gameInformation.allPvpTeams.Length, 0, maximumAmountOfTeams)];
                main.currentGameModeRuntimeData = ndrd;
            }
            DominationRuntimeData drd = main.currentGameModeRuntimeData as DominationRuntimeData;

            //Check stage
            if (main.gameModeStage == 0)
            {
                if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
                {
                    //Pre game time to main game
                    main.timer = main.currentPvPGameModeBehaviour.traditionalDurations[Kit_GameSettings.gameLength];
                }
                else if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
                {
                    //Pre game time to main game
                    main.timer = main.currentPvPGameModeBehaviour.lobbyGameDuration;
                }
                main.gameModeStage = 1;
            }
            //Time run out, determine winner
            else if (main.gameModeStage == 1)
            {
                main.timer = endGameTime;
                main.gameModeStage = 2;

                //Get most points
                int mostPoints = drd.teamPoints.Max();

                int teamWon = -2;

                for (int i = 0; i < drd.teamPoints.Length; i++)
                {
                    if (drd.teamPoints[i] == mostPoints)
                    {
                        //No other team has won yet
                        if (teamWon == -2)
                        {
                            int id = i;
                            teamWon = id;
                        }
                        //Another team has as many points as we have
                        else
                        {
                            //That means draw!
                            teamWon = -1;
                            break;
                        }
                    }
                }

                //End game according to results
                main.EndGame(teamWon, drd.teamPoints);
            }
            //Victory screen is over. Proceed to map voting.
            else if (main.gameModeStage == 2)
            {
                //Destroy victory screen
                if (main.currentVictoryScreen)
                {
                    PhotonNetwork.Destroy(main.currentVictoryScreen.photonView);
                }
                if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
                {
                    //Set time and stage
                    main.timer = mapVotingTime;
                    main.gameModeStage = 3;
                    //Open the voting menu
                    main.OpenVotingMenu();
                    //Delete all players
                    main.DeleteAllPlayers();
                }
                else if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
                {
                    //Delete all players
                    main.DeleteAllPlayers();
                    main.gameModeStage = 5;
                    //Load MM
                    Kit_SceneSyncer.instance.LoadScene("MainMenu");
                }
            }
            //End countdown is over, start new game
            else if (main.gameModeStage == 3)
            {
                //TODO: Load new map / game mode
                main.gameModeStage = 4;

                //Lets load the appropriate map
                //Get the hashtable
                Hashtable table = PhotonNetwork.CurrentRoom.CustomProperties;

                //Get combo
                MapGameModeCombo nextCombo = main.currentMapVoting.GetComboWithMostVotes();
                //Delete map voting
                PhotonNetwork.Destroy(main.currentMapVoting.gameObject);
                //Update table
                table["gameMode"] = nextCombo.gameMode;
                table["map"] = nextCombo.map;
                PhotonNetwork.CurrentRoom.SetCustomProperties(table);

                //Load the map
                Kit_SceneSyncer.instance.LoadScene(main.gameInformation.allPvpGameModes[nextCombo.gameMode].traditionalMaps[nextCombo.map].sceneName);
            }
        }

        public override bool CanSpawn(Kit_IngameMain main, Photon.Realtime.Player player)
        {
            //Check if game stage allows spawning
            if (main.gameModeStage < 2)
            {
                //Check if the player has joined a team and updated his Custom properties
                if (player.CustomProperties["team"] != null)
                {
                    if (player.CustomProperties["team"].GetType() == typeof(int))
                    {
                        int team = (int)player.CustomProperties["team"];
                        //Check if it is a valid team
                        if (team >= 0 && team < main.gameInformation.allPvpTeams.Length) return true;
                    }
                }
            }
            return false;
        }

        public override bool CanControlPlayer(Kit_IngameMain main)
        {
            //While we are waiting for enough players, we can move!
            if (!AreEnoughPlayersThere(main) && !main.hasGameModeStarted) return true;
            //We can only control our player if we are in the main phase
            return main.gameModeStage == 1;
        }

        public override bool AreEnoughPlayersThere(Kit_IngameMain main)
        {
            //If there are bots ...
            if (main && main.currentBotManager && main.currentBotManager.bots.Count > 0)
            {
                return true;
            }
            else
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties["lobby"] != null && (bool)PhotonNetwork.CurrentRoom.CustomProperties["lobby"])
                {
                    if (PhotonNetwork.PlayerList.Length >= main.currentPvPGameModeBehaviour.lobbyMinimumPlayersNeeded)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    //If there are 2 or more players, we can play!
                    if (PhotonNetwork.PlayerList.Length >= main.currentPvPGameModeBehaviour.traditionalPlayerNeeded[(int)PhotonNetwork.CurrentRoom.CustomProperties["playerNeeded"]])
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public override void GameModeBeginMiddle(Kit_IngameMain main)
        {
            //Ask players to reset themselves
            PhotonNetwork.RaiseEvent(Kit_EventIDs.resetRequest, null, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);

            //Reset all states
            if (main.currentGameModeRuntimeData != null && main.currentGameModeRuntimeData.GetType() == typeof(DominationRuntimeData))
            {
                DominationRuntimeData drd = main.currentGameModeRuntimeData as DominationRuntimeData;
                //Reset score
                for (int i = 0; i < drd.teamPoints.Length; i++)
                {
                    drd.teamPoints[i] = 0;
                }
                drd.lastTick = 0f;
                //Reset flags (material should change automatically next frame)
                for (int i = 0; i < drd.flags.Length; i++)
                {
                    drd.flags[i].captureProgress = 0f;
                    drd.flags[i].currentOwner = 0;
                    drd.flags[i].currentState = 0;
                    drd.flags[i].playersCapturingFlag = 0;
                }
            }

            //Find all weapon spawners
            Kit_WeaponSpawner[] weaponSpawners = FindObjectsOfType<Kit_WeaponSpawner>();
            //Reset them
            for (int i = 0; i < weaponSpawners.Length; i++)
            {
                weaponSpawners[i].GameModeBeginMiddle();
            }

            //Find all ammo spawners
            Kit_AmmoSpawner[] ammoSpawners = FindObjectsOfType<Kit_AmmoSpawner>();
            //Reset
            for (int i = 0; i < ammoSpawners.Length; i++)
            {
                ammoSpawners[i].GameModeBeginMiddle();
            }
        }

        public override void OnPhotonSerializeView(Kit_IngameMain main, PhotonStream stream, PhotonMessageInfo info)
        {
            //Ensure we are using the correct runtime data
            if (main.currentGameModeRuntimeData == null || main.currentGameModeRuntimeData.GetType() != typeof(DominationRuntimeData))
            {
                DominationRuntimeData ndrd = new DominationRuntimeData();
                ndrd.teamPoints = new int[Mathf.Clamp(main.gameInformation.allPvpTeams.Length, 0, maximumAmountOfTeams)];
                main.currentGameModeRuntimeData = ndrd;
            }
            DominationRuntimeData drd = main.currentGameModeRuntimeData as DominationRuntimeData;
            if (stream.IsWriting)
            {
                for (int i = 0; i < drd.teamPoints.Length; i++)
                {
                    //Send team points
                    stream.SendNext(drd.teamPoints[i]);
                }
                //Send last tick
                stream.SendNext(drd.lastTick);
                //Send flags
                for (int i = 0; i < drd.flags.Length; i++)
                {
                    stream.SendNext(drd.flags[i].currentOwner);
                    stream.SendNext(drd.flags[i].currentState);
                    stream.SendNext(drd.flags[i].captureProgress);
                    stream.SendNext(drd.flags[i].playersCapturingFlag);
                }
            }
            else if (stream.IsReading)
            {
                for (int i = 0; i < drd.teamPoints.Length; i++)
                {
                    //Get team points
                    drd.teamPoints[i] = (int)stream.ReceiveNext();
                }
                //Get last tick
                drd.lastTick = (double)stream.ReceiveNext(); ;
                for (int i = 0; i < drd.flags.Length; i++)
                {
                    drd.flags[i].currentOwner = (int)stream.ReceiveNext();
                    drd.flags[i].currentState = (int)stream.ReceiveNext();
                    drd.flags[i].captureProgress = (float)stream.ReceiveNext();
                    drd.flags[i].playersCapturingFlag = (int)stream.ReceiveNext();
                }
            }
        }

        public override bool ArePlayersEnemies(Kit_PlayerBehaviour playerOne, Kit_PlayerBehaviour playerTwo)
        {
            if (playerOne.myTeam != playerTwo.myTeam) return true;
            return false;
        }

        public override bool ArePlayersEnemies(Kit_IngameMain main, int playerOneID, bool playerOneBot, int playerTwoID, bool playerTwoBot, bool canKillSelf = false)
        {
            if (playerTwoBot && playerOneBot && playerOneID == playerTwoID && canKillSelf) return true;

            int teamOne = -1;
            int teamTwo = -2;

            if (playerOneBot)
            {
                Kit_Bot bot = main.currentBotManager.GetBotWithID(playerOneID);
                teamOne = bot.team;
            }
            else
            {
                Player player = Kit_PhotonPlayerExtensions.Find(playerOneID);
                teamOne = (int)player.CustomProperties["team"];
            }

            if (playerTwoBot)
            {
                Kit_Bot bot = main.currentBotManager.GetBotWithID(playerTwoID);
                teamTwo = bot.team;
            }
            else
            {
                Player player = Kit_PhotonPlayerExtensions.Find(playerTwoID);
                teamTwo = (int)player.CustomProperties["team"];
            }

            if (teamOne != teamTwo) return true;

            return false;
        }

        public override bool ArePlayersEnemies(Kit_IngameMain main, int playerOneID, bool playerOneBot, Kit_PlayerBehaviour playerTwo, bool canKillSelf)
        {
            if (playerTwo.isBot && playerOneBot && playerOneID == playerTwo.botId && canKillSelf) return true;

            int oneTeam = -1;

            if (playerOneBot)
            {
                Kit_Bot bot = main.currentBotManager.GetBotWithID(playerOneID);
                oneTeam = bot.team;
            }
            else
            {
                Photon.Realtime.Player player = Kit_PhotonPlayerExtensions.Find(playerOneID);
                oneTeam = (int)player.CustomProperties["team"];
            }

            if (oneTeam != playerTwo.myTeam) return true;

            return false;
        }

        public override bool AreWeEnemies(Kit_IngameMain main, bool botEnemy, int enemyId)
        {
            //So that we can blind/kill ourselves with grenades
            if (!botEnemy && enemyId == PhotonNetwork.LocalPlayer.ActorNumber) return true;

            int enemyTeam = -1;

            if (botEnemy)
            {
                Kit_Bot bot = main.currentBotManager.GetBotWithID(enemyId);
                if (bot != null)
                    enemyTeam = bot.team;
                else //If he doesn't exist, we can't be enemies
                    return false;
            }
            else
            {
                Photon.Realtime.Player player = Kit_PhotonPlayerExtensions.Find(enemyId);
                enemyTeam = (int)player.CustomProperties["team"];
            }

            if (main.assignedTeamID != enemyTeam) return true;

            return false;
        }

        public override bool CanStartVote(Kit_IngameMain main)
        {
            //While we are waiting for enough players, we can vote!
            if (!AreEnoughPlayersThere(main) && !main.hasGameModeStarted) return true;
            //We can only vote during the main phase and if enough time is left
            return main.gameModeStage == 1 && main.timer > votingThreshold;
        }

#if UNITY_EDITOR
        public override string[] GetSceneCheckerMessages()
        {
            List<string> toReturn = new List<string>();
            //Find spawns
            Kit_PlayerSpawn[] spawns = FindObjectsOfType<Kit_PlayerSpawn>();
            //Get good spawns
            List<Kit_PlayerSpawn> spawnsForThisGameMode = new List<Kit_PlayerSpawn>();
            for (int i = 0; i < spawns.Length; i++)
            {
                if (spawns[i].pvpGameModes.Contains(this))
                {
                    spawnsForThisGameMode.Add(spawns[i]);
                }
            }

            Kit_Domination_Flag[] flags = FindObjectsOfType<Kit_Domination_Flag>();

            int backupSpawns = 0;
            int teamOneSpawns = 0;
            int teamTwoSpawns = 0;
            int[] flagSpawns = new int[flags.Length];

            //Loop through all
            for (int i = 0; i < spawnsForThisGameMode.Count; i++)
            {
                if (spawnsForThisGameMode[i].spawnGroupID == 0)
                {
                    backupSpawns++;
                }
                else if (spawnsForThisGameMode[i].spawnGroupID == 1)
                {
                    teamOneSpawns++;
                }
                else if (spawnsForThisGameMode[i].spawnGroupID == 2)
                {
                    teamTwoSpawns++;
                }

                for (int o = 0; o < flags.Length; o++)
                {
                    if (spawnsForThisGameMode[i].spawnGroupID == 3 + o)
                    {
                        flagSpawns[o]++;
                    }
                }
            }

            //Now add string based on found spawns
            if (backupSpawns == 0)
            {
                toReturn.Add("[Backup Spawns; Spawn Group ID = 0] None found.");
            }
            else if (backupSpawns < 5)
            {
                toReturn.Add("[Backup Spawns; Spawn Group ID = 0] Maybe add a few more?");
            }
            else
            {
                toReturn.Add("[Backup Spawns; Spawn Group ID = 0] All good.");
            }

            if (teamOneSpawns == 0)
            {
                toReturn.Add("[Team One Start Spawns; Spawn Group ID = 1] None found.");
            }
            else if (teamOneSpawns < 5)
            {
                toReturn.Add("[Team One Start Spawns; Spawn Group ID = 1] Maybe add a few more?");
            }
            else
            {
                toReturn.Add("[Team One Start Spawns; Spawn Group ID = 1] All good.");
            }

            if (teamTwoSpawns == 0)
            {
                toReturn.Add("[Team Two Start Spawns; Spawn Group ID = 2] None found.");
            }
            else if (teamTwoSpawns < 5)
            {
                toReturn.Add("[Team Two Start Spawns; Spawn Group ID = 2] Maybe add a few more?");
            }
            else
            {
                toReturn.Add("[Team Two Start Spawns; Spawn Group ID = 2] All good.");
            }

            if (flags.Length <= 0)
            {
                toReturn.Add("[Flags] No flags found.");
            }
            else if (flags.Length <= 2)
            {
                toReturn.Add("[Flags] Maybe add a few more?");
            }
            else
            {
                toReturn.Add("[Flags] All Good.");
            }

            for (int i = 0; i < flagSpawns.Length; i++)
            {
                if (flagSpawns[i] == 0)
                {
                    toReturn.Add("[Flag #" + i + "; Spawn Group ID = " + (3 + i) + "] None found.");
                }
                else if (flagSpawns[i] < 5)
                {
                    toReturn.Add("[Flag #" + i + "; Spawn Group ID = " + (3 + i) + "] Maybe add a few more?");
                }
                else
                {
                    toReturn.Add("[Flag #" + i + "; Spawn Group ID = " + (3 + i) + "] All good.");
                }
            }

            Kit_BotNavPoint[] navPoints = FindObjectsOfType<Kit_BotNavPoint>();
            List<Kit_BotNavPoint> navPointsForThis = new List<Kit_BotNavPoint>();

            for (int i = 0; i < navPoints.Length; i++)
            {
                if (navPoints[i].gameModes.Contains(this))
                {
                    navPointsForThis.Add(navPoints[i]);
                }
            }

            int backupNavPoints = 0;
            int[] flagNavPoints = new int[flags.Length];

            //Loop through all
            for (int i = 0; i < navPointsForThis.Count; i++)
            {
                if (navPointsForThis[i].navPointGroupID == 0)
                {
                    backupNavPoints++;
                }

                for (int o = 0; o < flags.Length; o++)
                {
                    if (navPointsForThis[i].navPointGroupID == 1 + o)
                    {
                        flagNavPoints[o]++;
                    }
                }
            }

            if (backupNavPoints <= 0)
            {
                toReturn.Add("[Backup Nav Points] No nav points for this game mode found!");
            }
            else if (backupNavPoints <= 6)
            {
                toReturn.Add("[Backup Nav Points] Maybe you should add a few more");
            }
            else
            {
                toReturn.Add("[Backup Nav Points] All good.");
            }

            for (int i = 0; i < flagSpawns.Length; i++)
            {
                if (flagNavPoints[i] == 0)
                {
                    toReturn.Add("[Flag #" + i + "; Nav Point Group ID = " + (1 + i) + "] None found.");
                }
                else if (flagNavPoints[i] < 5)
                {
                    toReturn.Add("[Flag #" + i + "; Nav Point Group ID = " + (1 + i) + "] Maybe add a few more?");
                }
                else
                {
                    toReturn.Add("[Flag #" + i + "; Nav Point Group ID = " + (1 + i) + "] All good.");
                }
            }


            return toReturn.ToArray();
        }

        public override MessageType[] GetSceneCheckerMessageTypes()
        {
            List<MessageType> toReturn = new List<MessageType>();
            //Find spawns
            Kit_PlayerSpawn[] spawns = FindObjectsOfType<Kit_PlayerSpawn>();
            //Get good spawns
            List<Kit_PlayerSpawn> spawnsForThisGameMode = new List<Kit_PlayerSpawn>();
            for (int i = 0; i < spawns.Length; i++)
            {
                if (spawns[i].pvpGameModes.Contains(this))
                {
                    spawnsForThisGameMode.Add(spawns[i]);
                }
            }

            Kit_Domination_Flag[] flags = FindObjectsOfType<Kit_Domination_Flag>();

            int backupSpawns = 0;
            int teamOneSpawns = 0;
            int teamTwoSpawns = 0;
            int[] flagSpawns = new int[flags.Length];

            //Loop through all
            for (int i = 0; i < spawnsForThisGameMode.Count; i++)
            {
                if (spawnsForThisGameMode[i].spawnGroupID == 0)
                {
                    backupSpawns++;
                }
                else if (spawnsForThisGameMode[i].spawnGroupID == 1)
                {
                    teamOneSpawns++;
                }
                else if (spawnsForThisGameMode[i].spawnGroupID == 2)
                {
                    teamTwoSpawns++;
                }

                for (int o = 0; o < flags.Length; o++)
                {
                    if (spawnsForThisGameMode[i].spawnGroupID == 3 + o)
                    {
                        flagSpawns[o]++;
                    }
                }
            }

            //Now add string based on found spawns
            if (backupSpawns == 0)
            {
                toReturn.Add(MessageType.Error);
            }
            else if (backupSpawns < 5)
            {
                toReturn.Add(MessageType.Warning);
            }
            else
            {
                toReturn.Add(MessageType.Info);
            }

            if (teamOneSpawns == 0)
            {
                toReturn.Add(MessageType.Error);
            }
            else if (teamOneSpawns < 5)
            {
                toReturn.Add(MessageType.Warning);
            }
            else
            {
                toReturn.Add(MessageType.Info);
            }

            if (teamTwoSpawns == 0)
            {
                toReturn.Add(MessageType.Error);
            }
            else if (teamTwoSpawns < 5)
            {
                toReturn.Add(MessageType.Warning);
            }
            else
            {
                toReturn.Add(MessageType.Info);
            }

            if (flags.Length <= 0)
            {
                toReturn.Add(MessageType.Error);
            }
            else if (flags.Length <= 2)
            {
                toReturn.Add(MessageType.Warning);
            }
            else
            {
                toReturn.Add(MessageType.Info);
            }

            for (int i = 0; i < flagSpawns.Length; i++)
            {
                if (flagSpawns[i] == 0)
                {
                    toReturn.Add(MessageType.Error);
                }
                else if (flagSpawns[i] < 5)
                {
                    toReturn.Add(MessageType.Warning);
                }
                else
                {
                    toReturn.Add(MessageType.Info);
                }
            }


            Kit_BotNavPoint[] navPoints = FindObjectsOfType<Kit_BotNavPoint>();
            List<Kit_BotNavPoint> navPointsForThis = new List<Kit_BotNavPoint>();

            for (int i = 0; i < navPoints.Length; i++)
            {
                if (navPoints[i].gameModes.Contains(this))
                {
                    navPointsForThis.Add(navPoints[i]);
                }
            }

            int backupNavPoints = 0;
            int[] flagNavPoints = new int[flags.Length];

            //Loop through all
            for (int i = 0; i < navPointsForThis.Count; i++)
            {
                if (navPointsForThis[i].navPointGroupID == 0)
                {
                    backupNavPoints++;
                }

                for (int o = 0; o < flags.Length; o++)
                {
                    if (navPointsForThis[i].navPointGroupID == 1 + o)
                    {
                        flagNavPoints[o]++;
                    }
                }
            }

            if (backupNavPoints <= 0)
            {
                toReturn.Add(MessageType.Error);
            }
            else if (backupNavPoints <= 6)
            {
                toReturn.Add(MessageType.Warning);
            }
            else
            {
                toReturn.Add(MessageType.Info);
            }

            for (int i = 0; i < flagSpawns.Length; i++)
            {
                if (flagNavPoints[i] == 0)
                {
                    toReturn.Add(MessageType.Error);
                }
                else if (flagNavPoints[i] < 5)
                {
                    toReturn.Add(MessageType.Warning);
                }
                else
                {
                    toReturn.Add(MessageType.Info);
                }
            }
            return toReturn.ToArray();
        }
#endif
    }
}
