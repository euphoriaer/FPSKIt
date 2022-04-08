using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// This script handles the victory screen. Since there is no special functions for this, it is not abstract. You can replace it with your own.
    /// </summary>
    public class Kit_VictoryScreen : MonoBehaviourPunCallbacks
    {
        void Start()
        {
            //Search main behaviour
            Kit_IngameMain main = FindObjectOfType<Kit_IngameMain>();
            //Assign to main behaviour
            main.currentVictoryScreen = this;
            //Callback
            main.VictoryScreenOpened();
            //Close Pause Menu
            main.SetPauseMenuState(false);
            //Unlock cursor
            if (MarsScreen.lockCursor)
            {
                MarsScreen.lockCursor = false;
            }
            //Disable scoreboard
            main.scoreboard.Disable();

            //Get instantiation data
            object[] instData = photonView.InstantiationData;
            //Get the type
            int winnerType = (int)instData[0];
            //Player won
            if (winnerType == 0)
            {
                bool botWon = (bool)instData[1];
                if (botWon)
                {
                    if (main.currentBotManager)
                    {
                        Kit_Bot winner = main.currentBotManager.GetBotWithID((int)instData[2]);
                        //Display UI
                        main.victoryScreenUI.DisplayBotWinner(winner);
                    }
                }
                else
                {
                    Photon.Realtime.Player winner = Kit_PhotonPlayerExtensions.Find((int)instData[2]);
                    //Check if we won
                    if (winner == PhotonNetwork.LocalPlayer)
                    {
                        //We won this match!
                        Debug.Log("Victory Screen: We won");
                    }
                    else
                    {
                        //Someone else won :(
                        Debug.Log("Victory Screen: A different player won");
                    }
                    //Display UI
                    main.victoryScreenUI.DisplayPlayerWinner(winner);
                }
            }
            //Team won (Or draw)
            else if (winnerType == 1)
            {
                //Check which team won
                int winner = (int)instData[1];
                if (winner < 0)
                {
                    //Draw
                    Debug.Log("Victory Screen: Draw");
                }
                else
                {
                    //Team x won
                    Debug.Log("Victory Screen: Team " + winner + " won");
                }
                //Check if the data inlcudes scores
                if (instData.Length > 2)
                {
                    int size = (int)instData[2];
                    int[] scores = new int[size];

                    //Get scores
                    for (int i = 0; i < size; i++)
                    {
                        scores[i] = (int)instData[3 + i];
                    }

                    //Display UI
                    main.victoryScreenUI.DisplayTeamWinnerWithScores(winner, scores);
                }
                else
                {
                    //Display UI
                    main.victoryScreenUI.DisplayTeamWinner(winner);
                }
            }

            //This is a good place to call the resetting of the stats since the scoreboard won't open anymore and the game is already over.
            //main.ResetStats();
        }

        void OnDestroy()
        {
            //Search main behaviour
            Kit_IngameMain main = FindObjectOfType<Kit_IngameMain>();
            if (main)
            {
                //Hide UI
                main.victoryScreenUI.CloseUI();
                //Enable scoreboard
                main.scoreboard.Enable();
            }
        }
    }
}
