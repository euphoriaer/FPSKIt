using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    namespace UI
    {
        public class Kit_IngameMenuTeamSelection : MonoBehaviour
        {
            /// <summary>
            /// Id in the team selection
            /// </summary>
            public int teamSelectionId;

            /// <summary>
            /// Reference to main
            /// </summary>
            public Kit_IngameMain main;
            /// <summary>
            /// Where the team selection goes
            /// </summary>
            public RectTransform teamGo;
            /// <summary>
            /// Prefab for team selection
            /// </summary>
            public GameObject teamPrefab;

            /// <summary>
            /// Action after team selection
            /// </summary>
            public AfterTeamSelection afterSelection;

            public void Setup()
            {
                for (int i = 0; i < Mathf.Clamp(main.gameInformation.allPvpTeams.Length, 0, main.currentPvPGameModeBehaviour.maximumAmountOfTeams); i++)
                {
                    int id = i;
                    GameObject go = Instantiate(teamPrefab, teamGo, false);
                    Button btn = go.GetComponentInChildren<Button>();
                    TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();

                    btn.onClick.AddListener(delegate { main.JoinTeam(id); });
                    txt.text = main.gameInformation.allPvpTeams[id].teamName;

                    //Move to right pos
                    go.transform.SetSiblingIndex(id);
                }

                if (main.spectatorManager && main.spectatorManager.IsSpectatingEnabled(main) && main.currentPvPGameModeBehaviour.SpectatingEnabled(main))
                {
                    GameObject go = Instantiate(teamPrefab, teamGo, false);
                    Button btn = go.GetComponentInChildren<Button>();
                    TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();

                    btn.onClick.AddListener(delegate { main.spectatorManager.BeginSpectating(main, true); });
                    txt.text = "Spectate";

                    //Move to right pos
                    go.transform.SetSiblingIndex(Mathf.Clamp(main.gameInformation.allPvpTeams.Length, 0, main.currentPvPGameModeBehaviour.maximumAmountOfTeams));
                }
            }
        }
    }
}