using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    public class Kit_MapVotingUI : Kit_MapVotingUIBase
    {
        /// <summary>
        /// Reference to the main behaviour
        /// </summary>
        public Kit_IngameMain main;

        /// <summary>
        /// Root object for the voting
        /// </summary>
        public GameObject root;

        [Header("List")]
        /// <summary>
        /// Where will all the votes go?
        /// </summary>
        public RectTransform listGo;
        /// <summary>
        /// Entry prefab
        /// </summary>
        public GameObject listPrefab;

        //Runtime
        List<Kit_MapVotingEntry> activeEntries = new List<Kit_MapVotingEntry>();

        public override void SetupVotes(List<MapGameModeCombo> combos)
        {
            //Delete old entries
            for (int i = 0; i < activeEntries.Count; i++)
            {
                Destroy(activeEntries[i].gameObject);
            }

            activeEntries = new List<Kit_MapVotingEntry>();

            for (int i = 0; i < combos.Count; i++)
            {
                GameObject go = Instantiate(listPrefab, listGo, false);
                //Add to the entry list
                Kit_MapVotingEntry newEntry = go.GetComponent<Kit_MapVotingEntry>();
                activeEntries.Add(newEntry);
                //Setup the new entry
                newEntry.gameModeName.text = main.gameInformation.allPvpGameModes[combos[i].gameMode].gameModeName;
                if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Traditional)
                {
                    newEntry.mapName.text = main.gameInformation.allPvpGameModes[combos[i].gameMode].traditionalMaps[combos[i].map].mapName;
                    newEntry.mapImage.sprite = main.gameInformation.allPvpGameModes[combos[i].gameMode].traditionalMaps[combos[i].map].mapPicture;
                }
                else if (Kit_GameSettings.currentNetworkingMode == KitNetworkingMode.Lobby)
                {
                    newEntry.mapName.text = main.gameInformation.allPvpGameModes[combos[i].gameMode].lobbyMaps[combos[i].map].mapName;
                    newEntry.mapImage.sprite = main.gameInformation.allPvpGameModes[combos[i].gameMode].lobbyMaps[combos[i].map].mapPicture;
                }
                //Without this it would get changed as i changes.
                int vote = i;
                newEntry.myVote = vote;
            }

            //Show the root
            root.SetActive(true);
        }

        public override void RedrawVotes(Kit_MapVotingBehaviour behaviour)
        {
            //Get total votes
            int totalVotes = 0;
            for (int i = 0; i < behaviour.currentVotes.Count; i++)
            {
                totalVotes += behaviour.currentVotes[i];
            }

            //Redraw all entries
            for (int i = 0; i < behaviour.currentVotes.Count; i++)
            {
                if (i < activeEntries.Count)
                {
                    if (totalVotes > 0)
                    {
                        activeEntries[i].votePercentageImage.fillAmount = behaviour.currentVotes[i] / (float)totalVotes;
                        activeEntries[i].votePercentageText.text = ((behaviour.currentVotes[i] / (float)totalVotes) * 100f).ToString("F0") + "%";
                    }
                    else
                    {
                        activeEntries[i].votePercentageImage.fillAmount = 0;
                        activeEntries[i].votePercentageText.text = "0%";
                    }
                }
            }

            if (main)
            {
                main.SetPauseMenuState(false);
            }
        }

        public override void Hide()
        {
            //Disable root
            root.SetActive(false);
        }
    }
}
