using UnityEngine;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;

namespace MarsFPSKit
{
    /// <summary>
    /// Helper class for the map voting
    /// </summary>
    public class MapGameModeCombo
    {
        /// <summary>
        /// The map of this combo
        /// </summary>
        public int map;
        /// <summary>
        /// The game mode of this combo
        /// </summary>
        public int gameMode;
    }

    public class Kit_MapVotingBehaviour : MonoBehaviourPunCallbacks, IPunObservable
    {
        /// <summary>
        /// Runtime reference to the main behaviour
        /// </summary>
        Kit_IngameMain main;
        /// <summary>
        /// The combos that can currently be voted for
        /// </summary>
        public List<MapGameModeCombo> combos = new List<MapGameModeCombo>();
        /// <summary>
        /// Votes for each map are stored here and updated frequently. This list is synced.
        /// </summary>
        public List<int> currentVotes = new List<int>();

        void Start()
        {
            //Find main
            main = FindObjectOfType<Kit_IngameMain>();
            //Assign
            main.currentMapVoting = this;
            //Callback
            main.MapVotingOpened();
            //Get data
            object[] data = photonView.InstantiationData;
            combos = new List<MapGameModeCombo>();

            //Loop through it and turn it back into combos
            for (int i = 0; i < data.Length; i++)
            {
                //Since they are in linear order (gameMode, map, new gameMode, next map, etc) we only do a new one every two steps
                if (i % 2 == 0)
                {
                    //Create new combo
                    MapGameModeCombo newCombo = new MapGameModeCombo();
                    //Read from the network
                    newCombo.gameMode = (int)data[i];
                    newCombo.map = (int)data[i + 1];
                    //Add to the list
                    combos.Add(newCombo);
                }
            }

            //Setup votes
            while (currentVotes.Count < combos.Count) currentVotes.Add(0);

            //Setup the UI
            main.mapVotingUI.SetupVotes(combos);

            //Reset votes for all
            if (PhotonNetwork.IsMasterClient)
            {
                main.ResetAllStatsEndOfRound();
            }
        }

        void OnDestroy()
        {
            if (main)
            {
                main.mapVotingUI.Hide();
            }
        }

        #region Photon Calls
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                //First send length
                stream.SendNext(currentVotes.Count);
                //Then send all votes in correct order
                for (int i = 0; i < currentVotes.Count; i++)
                {
                    stream.SendNext(currentVotes[i]);
                }
            }
            else
            {
                //Get count
                int count = (int)stream.ReceiveNext();
                //Check if we have enough count
                while (currentVotes.Count < count) currentVotes.Add(0);
                //Then receive all votes in correct order
                for (int i = 0; i < count; i++)
                {
                    currentVotes[i] = (int)stream.ReceiveNext();
                }
                if (main && main.mapVotingUI)
                {
                    //Then proceed to redraw
                    main.mapVotingUI.RedrawVotes(this);
                }
            }
        }
        #endregion

        #region Custom Calls
        public void RecalculateVotes()
        {
            //Only the master client calculates votes
            if (PhotonNetwork.IsMasterClient)
            {
                //Reset votes
                for (int i = 0; i < currentVotes.Count; i++)
                {
                    currentVotes[i] = 0;
                }

                //Loop through all players
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    Hashtable table = PhotonNetwork.PlayerList[i].CustomProperties;
                    //Check who this player has voted for
                    if (table["vote"] != null)
                    {
                        int vote = (int)table["vote"];
                        if (vote >= 0 && vote < currentVotes.Count)
                        {
                            if (vote < currentVotes.Count)
                            {
                                //Add that vote
                                currentVotes[vote]++;
                            }
                        }
                    }
                }

                //Redraw on the master client
                main.mapVotingUI.RedrawVotes(this);
            }
        }

        /// <summary>
        /// Returns the combo with the most votes
        /// </summary>
        /// <returns></returns>
        public MapGameModeCombo GetComboWithMostVotes()
        {
            MapGameModeCombo toReturn = combos[0];
            int mostVotes = 0;
            int mostVotesIndex = 0;

            //Check which one has the most votes
            for (int i = 0; i < currentVotes.Count; i++)
            {
                if (currentVotes[i] > mostVotes)
                {
                    mostVotes = currentVotes[i];
                    mostVotesIndex = i;
                }
            }

            //Set
            toReturn = combos[mostVotesIndex];

            //Return it
            return toReturn;
        }
        #endregion

        #region Static functions
        /// <summary>
        /// Get a new map and game mode combo. It will try to avoid things already in the used list. Depending on the amount you want that might not be possible.
        /// </summary>
        /// <param name="game">Game information to use</param>
        /// <param name="used">List of combos that are already used</param>
        /// <returns></returns>
        public static MapGameModeCombo GetMapGameModeCombo(Kit_GameInformation game, List<MapGameModeCombo> used)
        {
            //First select a random game mode and map
            int gameMode = Random.Range(0, game.allPvpGameModes.Length);
            int map = 0;

            if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
            {
                map = Random.Range(0, game.allPvpGameModes[gameMode].traditionalMaps.Length);
            }
            else if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
            {
                map = Random.Range(0, game.allPvpGameModes[gameMode].lobbyMaps.Length);
            }

            //To prevent an infite loop if all game modes are already used
            int tries = 0;
            while (IsGameModeUsed(gameMode, used) && tries < 10)
            {
                gameMode = Random.Range(0, game.allPvpGameModes.Length);
                tries++;
            }

            //Reset tries
            tries = 0;
            while (IsMapUsed(map, used) && tries < 10)
            {
                if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
                {
                    map = Random.Range(0, game.allPvpGameModes[gameMode].traditionalMaps.Length);
                }
                else if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
                {
                    map = Random.Range(0, game.allPvpGameModes[gameMode].lobbyMaps.Length);
                }
                tries++;
            }

            //Create class and return it
            return new MapGameModeCombo { map = map, gameMode = gameMode };
        }

        /// <summary>
        /// Checks if gameMode is used in the list
        /// </summary>
        /// <param name="gameMode"></param>
        /// <param name="used"></param>
        /// <returns></returns>
        static bool IsGameModeUsed(int gameMode, List<MapGameModeCombo> used)
        {
            for (int i = 0; i < used.Count; i++)
            {
                if (used[i].gameMode == gameMode)
                {
                    return true;
                }
            }
            return false;
        }

        static bool IsMapUsed(int map, List<MapGameModeCombo> used)
        {
            for (int i = 0; i < used.Count; i++)
            {
                if (used[i].map == map)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
