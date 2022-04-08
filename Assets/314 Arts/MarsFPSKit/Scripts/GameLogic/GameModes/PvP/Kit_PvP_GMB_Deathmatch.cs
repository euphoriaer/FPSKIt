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
    public class Kit_PlayerWithKills : Kit_Player
    {
        public int kills;
    }

    //Runtime data class
    public class DeathmatchRuntimeData
    {
        /// <summary>
        /// When did we check for a winner the last time?
        /// </summary>
        public float lastWinnerCheck;
    }

    [CreateAssetMenu(menuName = ("MarsFPSKit/Gamemodes/Deathmatch Logic"))]
    public class Kit_PvP_GMB_Deathmatch : Kit_PvP_GameModeBase
    {
        /// <summary>
        /// How many kills does a player need to win the match?
        /// </summary>
        public int killLimit = 30;

        /// <summary>
        /// How many seconds apart is being checked if someone won?
        /// </summary>
        public float winnerCheckTime = 1f;

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

        public override Spectateable GetSpectateable(Kit_IngameMain main)
        {
            return Spectateable.All;
        }

        public override bool CanJoinTeam(Kit_IngameMain main, Photon.Realtime.Player player, int team)
        {
            //As this is Deathmatch, any player can join any team
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
            for (int i = 0; i < allSpawns.Length; i++)
            {
                //Check if that spawn is useable for this game mode logic
                if (allSpawns[i].pvpGameModes.Contains(this))
                {
                    //Add it to the list
                    filteredSpawns.Add(allSpawns[i]);
                }
            }

            //This game mode doesn't use different game mode for teams, so just keep it one layered
            main.internalSpawns = new List<InternalSpawns>();
            //Create a new InternalSpawns instance
            InternalSpawns dmSpawns = new InternalSpawns();
            dmSpawns.spawns = filteredSpawns;
            main.internalSpawns.Add(dmSpawns);

            //Set stage and timer
            main.gameModeStage = 0;
            main.timer = preGameTime;
        }

        public override void GameModeUpdate(Kit_IngameMain main)
        {
            //Ensure we are using the correct runtime data
            if (main.currentGameModeRuntimeData == null || main.currentGameModeRuntimeData.GetType() != typeof(DeathmatchRuntimeData))
            {
                main.currentGameModeRuntimeData = new DeathmatchRuntimeData();
            }
            DeathmatchRuntimeData drd = main.currentGameModeRuntimeData as DeathmatchRuntimeData;
            if (Time.time > drd.lastWinnerCheck)
            {
                CheckForWinner(main);
                drd.lastWinnerCheck = Time.time + winnerCheckTime;
            }
        }


        /// <summary>
        /// Checks all players in <see cref="PhotonNetwork.PlayerList"/> if they reached the kill limit, if the game is not over already
        /// </summary>
        void CheckForWinner(Kit_IngameMain main)
        {
            //Check if someone can still win
            if (main.gameModeStage < 2)
            {
                List<Kit_PlayerWithKills> tempPlayers = new List<Kit_PlayerWithKills>();

                //Convert all Photon.Realtime.Players to kit players
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    //Check if he has kills stats
                    if (PhotonNetwork.PlayerList[i].CustomProperties["kills"] != null)
                    {
                        Kit_PlayerWithKills player = new Kit_PlayerWithKills();
                        player.isBot = false;
                        player.id = PhotonNetwork.PlayerList[i].ActorNumber;
                        player.kills = (int)PhotonNetwork.PlayerList[i].CustomProperties["kills"];
                        tempPlayers.Add(player);
                    }
                }

                //Convert all bots
                if (main.currentBotManager)
                {
                    for (int i = 0; i < main.currentBotManager.bots.Count; i++)
                    {
                        Kit_PlayerWithKills player = new Kit_PlayerWithKills();
                        player.isBot = true;
                        player.id = main.currentBotManager.bots[i].id;
                        player.kills = main.currentBotManager.bots[i].kills;
                        tempPlayers.Add(player);
                    }
                }

                //Loop through all players
                for (int i = 0; i < tempPlayers.Count; i++)
                {
                    //Check how many kills he has
                    //Compare with kill limit
                    if (tempPlayers[i].kills >= killLimit)
                    {
                        //He has won. Tell the world about it!
                        main.timer = endGameTime;
                        main.gameModeStage = 2;

                        //Tell the world about it
                        main.EndGame(tempPlayers[i]);
                        break;
                    }
                }
            }
        }


        //This can be used for a BR, made it real quick lol

        /*
        /// <summary>
        /// Checks all players in <see cref="PhotonNetwork.PlayerList"/> if they reached the kill limit, if the game is not over already
        /// </summary>
        void CheckForWinner(Kit_IngameMain main)
        {
            //Check if someone can still win
            if (main.gameModeStage < 2 && main.gameModeStage > 0)
            {
                //Get all player sleft
                Kit_PlayerBehaviour[] players = FindObjectsOfType<Kit_PlayerBehaviour>();

                if (players.Length == 1)
                {
                    //That player won
                    Kit_Player winPlayer = new Kit_Player();
                    winPlayer.isBot = players[0].isBot;

                    if (players[0].isBot)
                    {
                        winPlayer.id = players[0].botId;
                    }
                    else
                    {
                        winPlayer.id = players[0].photonView.OwnerActorNr;
                    }

                    main.EndGame(winPlayer);
                }
            }
        }
        */


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
                //As this is deathmatch, we only have one layer of spawns so we use [0]
                Transform spawnToTest = main.internalSpawns[0].spawns[UnityEngine.Random.Range(0, main.internalSpawns[0].spawns.Count)].transform;
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
                //As this is deathmatch, we only have one layer of spawns so we use [0]
                Transform spawnToTest = main.internalSpawns[0].spawns[UnityEngine.Random.Range(0, main.internalSpawns[0].spawns.Count)].transform;
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
            Debug.Log("Game Mode received kill");
            //Check if someone won
            CheckForWinner(main);
        }

        public override void TimeRunOut(Kit_IngameMain main)
        {
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

                Kit_Player wonPlayer = GetPlayerWithMostKills(main);

                if (wonPlayer != null)
                {
                    Debug.Log("Deathmatch ended. Winner: " + wonPlayer);
                    //We have a winner, tell the world (other players) about it
                    main.EndGame(wonPlayer);
                }
                else
                {
                    Debug.Log("Deathmatch ended. No winner");
                    //There is no winner. Tell the world about it.
                    main.EndGame(-1);
                }
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

        /// <summary>
        /// Returns the Photon.Realtime.Player with the most kills. If there are two or more players with the same amount of kills, noone will be returned
        /// </summary>
        /// <returns></returns>
        Kit_Player GetPlayerWithMostKills(Kit_IngameMain main)
        {
            int maxKills = 0;
            Kit_Player toReturn = null;

            List<Kit_PlayerWithKills> tempPlayers = new List<Kit_PlayerWithKills>();

            //Convert all Photon.Realtime.Players to kit players
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                //Check if he has kills stats
                if (PhotonNetwork.PlayerList[i].CustomProperties["kills"] != null)
                {
                    Kit_PlayerWithKills player = new Kit_PlayerWithKills();
                    player.isBot = false;
                    player.id = PhotonNetwork.PlayerList[i].ActorNumber;
                    player.kills = (int)PhotonNetwork.PlayerList[i].CustomProperties["kills"];
                    tempPlayers.Add(player);
                }
            }

            //Convert all bots
            if (main.currentBotManager)
            {
                for (int i = 0; i < main.currentBotManager.bots.Count; i++)
                {
                    Kit_PlayerWithKills player = new Kit_PlayerWithKills();
                    player.isBot = true;
                    player.id = main.currentBotManager.bots[i].id;
                    player.kills = main.currentBotManager.bots[i].kills;
                    tempPlayers.Add(player);
                }
            }

            //Loop through all players
            for (int i = 0; i < tempPlayers.Count; i++)
            {
                //Compare
                if (tempPlayers[i].kills > maxKills)
                {
                    maxKills = tempPlayers[i].kills;
                    toReturn = tempPlayers[i];
                }
            }

            int amountOfPlayersWithMaxKills = 0;

            if (toReturn != null)
            {
                //If we have a player with most kills, check if two players have the same amount of kills (which would be a draw)
                for (int i = 0; i < tempPlayers.Count; i++)
                {
                    //Compare
                    if (tempPlayers[i].kills == maxKills)
                    {
                        amountOfPlayersWithMaxKills++;
                    }
                }
            }

            //If theres more than one player with most kills, return none
            if (amountOfPlayersWithMaxKills > 1) toReturn = null;

            return toReturn;
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

        public override bool ArePlayersEnemies(Kit_PlayerBehaviour playerOne, Kit_PlayerBehaviour playerTwo)
        {
            if (playerOne == playerTwo) return false;
            //Everyone can kill everyone
            return true;
        }

        public override bool ArePlayersEnemies(Kit_IngameMain main, int playerOneID, bool playerOneBot, Kit_PlayerBehaviour playerTwo, bool canKillSelf)
        {
            if (playerOneBot && playerTwo.isBot && playerOneID == playerTwo.botId && !canKillSelf) return false;
            if (!playerOneBot && !playerTwo.isBot && playerOneID == playerTwo.id && !canKillSelf) return false;
            //Everyone can kill everyone
            return true;
        }

        public override bool ArePlayersEnemies(Kit_IngameMain main, int playerOneID, bool playerOneBot, int playerTwoID, bool playerTwoBot, bool canKillSelf = false)
        {
            if (playerOneBot && playerTwoBot && playerOneID == playerTwoID && !canKillSelf) return false;
            if (!playerOneBot && !playerTwoBot && playerOneID == playerTwoID && !canKillSelf) return false;
            //Everyone can kill everyone
            return true;
        }

        public override bool AreWeEnemies(Kit_IngameMain main, bool botEnemy, int enemyId)
        {
            if (!botEnemy && enemyId == PhotonNetwork.LocalPlayer.ActorNumber) return false;
            //Everyone is enemies
            return true;
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
