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
    public class TeamDeathmatchRuntimeData
    {
        /// <summary>
        /// Points scored by each team
        /// </summary>
        public int[] teamPoints;
    }

    [CreateAssetMenu(menuName = ("MarsFPSKit/Gamemodes/Team Deathmatch Logic"))]
    public class Kit_PvP_GMB_TeamDeathmatch : Kit_PvP_GameModeBase
    {
        /// <summary>
        /// How many kills does a team need to win the match?
        /// </summary>
        public int killLimit = 75;

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
        [Tooltip("Spawn layer used for the teams during countdown")]
        [Header("Spawns")]
        public int[] teamsInitialSpawnLayer;
        /// <summary>
        /// Spawn layer used for team two during gameplay
        /// </summary>
        [Tooltip("Spawn layer used for teams during gameplay")]
        public int[] teamsGameplaySpawnLayer;

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
            //Get all spawns
            Kit_PlayerSpawn[] allSpawns = FindObjectsOfType<Kit_PlayerSpawn>();
            //Are there any spawns at all?
            if (allSpawns.Length <= 0) throw new Exception("This scene has no spawns.");
            //Filter all spawns that are appropriate for this game mode
            List<Kit_PlayerSpawn> filteredSpawns = new List<Kit_PlayerSpawn>();
            //Highest spawn index
            int highestIndex = 0;
            for (int i = 0; i < allSpawns.Length; i++)
            {
                int id = i;
                //Check if that spawn is useable for this game mode logic
                if (allSpawns[id].pvpGameModes.Contains(this))
                {
                    //Add it to the list
                    filteredSpawns.Add(allSpawns[id]);
                    //Set highest index
                    if (allSpawns[id].spawnGroupID > highestIndex) highestIndex = allSpawns[id].spawnGroupID;
                }
            }

            main.internalSpawns = new List<InternalSpawns>();
            for (int i = 0; i < (highestIndex + 1); i++)
            {
                main.internalSpawns.Add(null);
            }

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

            TeamDeathmatchRuntimeData tdrd = new TeamDeathmatchRuntimeData();
            tdrd.teamPoints = new int[Mathf.Clamp(main.gameInformation.allPvpTeams.Length, 0, maximumAmountOfTeams)];
            main.currentGameModeRuntimeData = tdrd;
        }

        public override void GameModeUpdate(Kit_IngameMain main)
        {

        }

        /// <summary>
        /// Checks all players in <see cref="PhotonNetwork.PlayerList"/> if they reached the kill limit, if the game is not over already
        /// </summary>
        void CheckForWinner(Kit_IngameMain main)
        {
            //Check if someone can still win
            if (main.gameModeStage < 2)
            {
                //Ensure we are using the correct runtime data
                if (main.currentGameModeRuntimeData == null || main.currentGameModeRuntimeData.GetType() != typeof(TeamDeathmatchRuntimeData))
                {
                    TeamDeathmatchRuntimeData tdrd = new TeamDeathmatchRuntimeData();
                    tdrd.teamPoints = new int[Mathf.Clamp(main.gameInformation.allPvpTeams.Length, 0, maximumAmountOfTeams)];
                    main.currentGameModeRuntimeData = tdrd;
                }
                TeamDeathmatchRuntimeData drd = main.currentGameModeRuntimeData as TeamDeathmatchRuntimeData;

                for (int i = 0; i < drd.teamPoints.Length; i++)
                {
                    if (drd.teamPoints[i] >= killLimit)
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
            //Define spawn tries
            int tries = 0;
            Transform spawnToReturn = null;
            //Try to get a spawn
            while (!spawnToReturn)
            {
                //To prevent an unlimited loop, only do it ten times
                if (tries >= 10)
                {
                    break;
                }

                int layer = 0;

                if (main.gameModeStage == 0)
                {
                    layer = teamsInitialSpawnLayer[Mathf.Clamp(main.assignedTeamID, 0, teamsInitialSpawnLayer.Length - 1)];
                }
                else
                {
                    layer = teamsGameplaySpawnLayer[Mathf.Clamp(main.assignedTeamID, 0, teamsGameplaySpawnLayer.Length - 1)];
                }

                //Team deathmatch has no fixed spawns in this behaviour. Only use one layer
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

            return spawnToReturn;
        }

        public override Transform GetSpawn(Kit_IngameMain main, Kit_Bot bot)
        {
            //Define spawn tries
            int tries = 0;
            Transform spawnToReturn = null;
            //Try to get a spawn
            while (!spawnToReturn)
            {
                //To prevent an unlimited loop, only do it ten times
                if (tries >= 10)
                {
                    break;
                }
                int layer = 0;

                if (main.gameModeStage == 0)
                {
                    layer = teamsInitialSpawnLayer[Mathf.Clamp(bot.team, 0, teamsInitialSpawnLayer.Length - 1)];
                }
                else
                {
                    layer = teamsInitialSpawnLayer[Mathf.Clamp(bot.team, 0, teamsGameplaySpawnLayer.Length - 1)];
                }

                //Team deathmatch has no fixed spawns in this behaviour. Only use one layer
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

            return spawnToReturn;
        }

        public override void PlayerDied(Kit_IngameMain main, bool botKiller, int killer, bool botKilled, int killed)
        {
            //Ensure we are using the correct runtime data
            if (main.currentGameModeRuntimeData == null || main.currentGameModeRuntimeData.GetType() != typeof(TeamDeathmatchRuntimeData))
            {
                TeamDeathmatchRuntimeData tdrd = new TeamDeathmatchRuntimeData();
                tdrd.teamPoints = new int[Mathf.Clamp(main.gameInformation.allPvpTeams.Length, 0, maximumAmountOfTeams)];
                main.currentGameModeRuntimeData = tdrd;
            }
            TeamDeathmatchRuntimeData drd = main.currentGameModeRuntimeData as TeamDeathmatchRuntimeData;
            if (botKiller)
            {
                if (main.currentBotManager)
                {
                    //Check if he killed himself
                    if (!botKilled || killed != killer)
                    {
                        //Get bot
                        Kit_Bot killerBot = main.currentBotManager.GetBotWithID(killer);
                        if (killerBot != null)
                        {
                            //Check in which team the killer is
                            int killerTeam = killerBot.team;
                            //Increase points
                            drd.teamPoints[killerTeam]++;
                        }
                    }
                }
            }
            else
            {
                //Check if he killed himself
                if (botKilled || killed != killer)
                {
                    Photon.Realtime.Player playerKiller = null;
                    //Get player
                    for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                    {
                        if (PhotonNetwork.PlayerList[i].ActorNumber == killer)
                        {
                            playerKiller = PhotonNetwork.PlayerList[i];
                            break;
                        }
                    }

                    if (playerKiller != null)
                    {
                        //Check in which team the killer is
                        int killerTeam = (int)playerKiller.CustomProperties["team"];
                        //Increase points
                        drd.teamPoints[killerTeam]++;
                    }
                }
            }
            //Check if a team has won
            CheckForWinner(main);
        }

        public override void TimeRunOut(Kit_IngameMain main)
        {
            //Ensure we are using the correct runtime data
            if (main.currentGameModeRuntimeData == null || main.currentGameModeRuntimeData.GetType() != typeof(TeamDeathmatchRuntimeData))
            {
                TeamDeathmatchRuntimeData tdrd = new TeamDeathmatchRuntimeData();
                tdrd.teamPoints = new int[Mathf.Clamp(main.gameInformation.allPvpTeams.Length, 0, maximumAmountOfTeams)];
                main.currentGameModeRuntimeData = tdrd;
            }
            TeamDeathmatchRuntimeData drd = main.currentGameModeRuntimeData as TeamDeathmatchRuntimeData;

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
            PhotonNetwork.RaiseEvent(3, null, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);

            if (main.currentGameModeRuntimeData != null && main.currentGameModeRuntimeData.GetType() == typeof(TeamDeathmatchRuntimeData))
            {
                TeamDeathmatchRuntimeData drd = main.currentGameModeRuntimeData as TeamDeathmatchRuntimeData;
                //Reset score
                for (int i = 0; i < drd.teamPoints.Length; i++)
                {
                    drd.teamPoints[i] = 0;
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
            if (main.currentGameModeRuntimeData == null || main.currentGameModeRuntimeData.GetType() != typeof(TeamDeathmatchRuntimeData))
            {
                TeamDeathmatchRuntimeData tdrd = new TeamDeathmatchRuntimeData();
                tdrd.teamPoints = new int[Mathf.Clamp(main.gameInformation.allPvpTeams.Length, 0, main.currentPvPGameModeBehaviour.maximumAmountOfTeams)];
                main.currentGameModeRuntimeData = tdrd;
            }
            TeamDeathmatchRuntimeData drd = main.currentGameModeRuntimeData as TeamDeathmatchRuntimeData;
            if (stream.IsWriting)
            {
                for (int i = 0; i < drd.teamPoints.Length; i++)
                {
                    //Send team points
                    stream.SendNext(drd.teamPoints[i]);
                }
            }
            else
            {
                for (int i = 0; i < drd.teamPoints.Length; i++)
                {
                    //Get team points
                    drd.teamPoints[i] = (int)stream.ReceiveNext();
                }
            }
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

        public override bool ArePlayersEnemies(Kit_PlayerBehaviour playerOne, Kit_PlayerBehaviour playerTwo)
        {
            if (playerOne.myTeam != playerTwo.myTeam) return true;
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
            string[] toReturn = new string[2];
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

            if (spawnsForThisGameMode.Count <= 0)
            {
                toReturn[0] = "[Spawns] No spawns for this game mode found!";
            }
            else if (spawnsForThisGameMode.Count <= 6)
            {
                toReturn[0] = "[Spawns] Maybe you should add a few more";
            }
            else
            {
                toReturn[0] = "[Spawns] All good.";
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

            if (navPointsForThis.Count <= 0)
            {
                toReturn[1] = "[Nav Points] No nav points for this game mode found!";
            }
            else if (navPointsForThis.Count <= 6)
            {
                toReturn[1] = "[Nav Points] Maybe you should add a few more";
            }
            else
            {
                toReturn[1] = "[Nav Points] All good.";
            }

            return toReturn;
        }

        public override MessageType[] GetSceneCheckerMessageTypes()
        {
            MessageType[] toReturn = new MessageType[2];
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

            if (spawnsForThisGameMode.Count <= 0)
            {
                toReturn[0] = MessageType.Error;
            }
            else if (spawnsForThisGameMode.Count <= 6)
            {
                toReturn[0] = MessageType.Warning;
            }
            else
            {
                toReturn[0] = MessageType.Info;
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

            if (navPointsForThis.Count <= 0)
            {
                toReturn[1] = MessageType.Error;
            }
            else if (navPointsForThis.Count <= 6)
            {
                toReturn[1] = MessageType.Warning;
            }
            else
            {
                toReturn[1] = MessageType.Info;
            }

            return toReturn;
        }
#endif
    }
}
