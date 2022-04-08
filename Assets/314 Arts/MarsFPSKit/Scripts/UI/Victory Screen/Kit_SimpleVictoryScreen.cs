using Photon.Pun;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    /// <summary>
    /// Displays a simple, COD4 style victory screen
    /// </summary>
    public class Kit_SimpleVictoryScreen : Kit_VictoryScreenUI
    {
        /// <summary>
        /// Reference to the main behaviour
        /// </summary>
        public Kit_IngameMain main;
        /// <summary>
        /// Root object of the UI elements
        /// </summary>
        public GameObject root;

        [Header("Player Win")]
        /// <summary>
        /// It uses different styles for player / team. This is the root of the player object
        /// </summary>
        public GameObject pwRoot;
        /// <summary>
        /// This displays whether we won or lost
        /// </summary>
        public TextMeshProUGUI pwVictoryLoose;
        /// <summary>
        /// This displays the name of the winning player
        /// </summary>
        public TextMeshProUGUI pwName;

        [Header("Team")]
        /// <summary>
        /// It uses different styles for player / team. This is the root of the team object
        /// </summary>
        public GameObject teamWinRoot;
        /// <summary>
        /// This displays whether we won or lost
        /// </summary>
        public TextMeshProUGUI teamWinVictoryLoose;
        /// <summary>
        /// Go object for team
        /// </summary>
        public RectTransform teamWinTeamGo;
        /// <summary>
        /// Prefab for one team
        /// </summary>
        public GameObject teamWinTeamPrefab;
        /// <summary>
        /// Active teams
        /// </summary>
        private List<GameObject> teamActives = new List<GameObject>();

        public override void CloseUI()
        {
            //Disable root
            root.SetActive(false);
        }

        public override void DisplayPlayerWinner(Photon.Realtime.Player winner)
        {
            //Reset roots
            pwRoot.SetActive(false);
            teamWinRoot.SetActive(false);

            //Check if we won
            if (winner == PhotonNetwork.LocalPlayer)
            {
                //We won
                //Display victory
                pwVictoryLoose.text = "Victory!";
                //Display the name
                pwName.text = winner.NickName;
            }
            else
            {
                //We lost
                //Display loose
                pwVictoryLoose.text = "Defeat!";
                //Display the name
                pwName.text = winner.NickName;
            }

            //Activate player root
            pwRoot.SetActive(true);
            //Enable root
            root.SetActive(true);
        }

        public override void DisplayBotWinner(Kit_Bot winner)
        {
            //Reset roots
            pwRoot.SetActive(false);
            teamWinRoot.SetActive(false);

            //We lost
            //Display loose
            pwVictoryLoose.text = "Defeat!";
            //Display the name
            pwName.text = winner.name;

            //Activate player root
            pwRoot.SetActive(true);
            //Enable root
            root.SetActive(true);
        }

        public override void DisplayTeamWinner(int winner)
        {
            //Reset roots
            pwRoot.SetActive(false);
            teamWinRoot.SetActive(false);

            if (winner < 0)
            {
                teamWinVictoryLoose.text = "Tie!";
            }
            else
            {
                //Check if we won
                if (main.assignedTeamID == winner)
                {
                    teamWinVictoryLoose.text = "Victory!";
                }
                else
                {
                    teamWinVictoryLoose.text = "Defeat!";
                }
            }

            //Delete old teams
            for (int i = 0; i < teamActives.Count; i++)
            {
                Destroy(teamActives[i]);
            }

            teamActives = new List<GameObject>(); 

            //Create Teams
            for (int i = 0; i < Mathf.Clamp(main.gameInformation.allPvpTeams.Length, 0, main.currentPvPGameModeBehaviour.maximumAmountOfTeams); i++)
            {
                GameObject go = Instantiate(teamWinTeamPrefab, teamWinTeamGo, false);
                Image img = go.GetComponentInChildren<Image>();
                if (img)
                {
                    img.sprite = main.gameInformation.allPvpTeams[i].teamImage;
                }

                TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();

                if (txt)
                {
                    txt.enabled = false;
                }

                teamActives.Add(go);
            }


            teamWinRoot.SetActive(true);
            //Enable root
            root.SetActive(true);
        }

        public override void DisplayTeamWinnerWithScores(int winner, int[] scores)
        {
            //Reset roots
            pwRoot.SetActive(false);
            teamWinRoot.SetActive(false);

            if (winner < 0)
            {
                teamWinVictoryLoose.text = "Tie!";
            }
            else
            {
                //Check if we won
                if (main.assignedTeamID == winner)
                {
                    teamWinVictoryLoose.text = "Victory!";
                }
                else
                {
                    teamWinVictoryLoose.text = "Defeat!";
                }
            }

            //Delete old teams
            for (int i = 0; i < teamActives.Count; i++)
            {
                Destroy(teamActives[i]);
            }

            teamActives = new List<GameObject>();

            //Create Teams
            for (int i = 0; i < Mathf.Clamp(main.gameInformation.allPvpTeams.Length, 0, main.currentPvPGameModeBehaviour.maximumAmountOfTeams); i++)
            {
                GameObject go = Instantiate(teamWinTeamPrefab, teamWinTeamGo, false);
                Image img = go.GetComponentInChildren<Image>();
                if (img)
                {
                    img.sprite = main.gameInformation.allPvpTeams[i].teamImage;
                }

                TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();

                if (txt)
                {
                    txt.text = scores[i].ToString();
                    txt.enabled = true;
                }

                teamActives.Add(go);
            }

            teamWinRoot.SetActive(true);
            //Enable root
            root.SetActive(true);
        }
    }
}
