using Photon.Pun;
using TMPro;
using UnityEngine;

namespace MarsFPSKit
{
    namespace UI
    {
        public class Kit_IngameMenuPauseMenu : MonoBehaviour
        {
            /// <summary>
            /// Main reference
            /// </summary>
            public Kit_IngameMain main;
            /// <summary>
            /// ID for the pause menu
            /// </summary>
            public int pauseMenuId = 1;
            /// <summary>
            /// Button that takes us to the loadout menu
            /// </summary>
            public GameObject loadoutButton;
            /// <summary>
            /// Spawn  Button
            /// </summary>
            public TextMeshProUGUI spawnButtonText;
            /// <summary>
            /// Because this button controls both, changing teams and commiting suicide, we need to adjust its text
            /// </summary>
            public TextMeshProUGUI changeTeamButtonText;
            /// <summary>
            /// The change team button
            /// </summary>
            public GameObject changeTeamButton;
            /// <summary>
            /// Button that lets us vote
            /// </summary>
            public GameObject voteButton;
            /// <summary>
            /// Transform where the button goes
            /// </summary>
            public RectTransform pluginButtonGo;
            /// <summary>
            /// Prefab for inejcting a button into the pause menu
            /// </summary>
            public GameObject pluginButtonPrefab;

            private void Start()
            {
                //Is loadout supported?
                if (main.currentPvPGameModeBehaviour && main.currentPvPGameModeBehaviour.LoadoutMenuSupported())
                {
                    loadoutButton.SetActive(true);
                }
                else
                {
                    loadoutButton.SetActive(false);
                }
            }

            private void Update()
            {
                #region Team - Suicide Button
                if (main.currentPvEGameModeBehaviour)
                {
                    changeTeamButton.gameObject.SetActiveOptimized(false);
                }
                else if (main.myPlayer)
                {
                    changeTeamButtonText.text = "Suicide";
                    changeTeamButton.gameObject.SetActiveOptimized(true);
                }
                else if (main.currentPvPGameModeBehaviour)
                {
                    changeTeamButtonText.text = "Change Team";
                    changeTeamButton.gameObject.SetActiveOptimized(true);
                }
                else
                {
                    changeTeamButton.gameObject.SetActiveOptimized(false);
                }
                #endregion

                #region Spawn/Resume Button
                if (main.myPlayer)
                {
                    spawnButtonText.text = "Resume";
                }
                else
                {
                    if (main.currentPvPGameModeBehaviour && main.currentPvPGameModeBehaviour.CanSpawn(main, PhotonNetwork.LocalPlayer))
                    {
                        spawnButtonText.text = "Spawn";
                    }
                    else
                    {
                        spawnButtonText.text = "Close";
                    }
                }
                #endregion

                #region Vote Button
                //Voting only in pvp game modes
                voteButton.SetActiveOptimized(main.currentPvPGameModeBehaviour);
                #endregion
            }
        }
    }
}