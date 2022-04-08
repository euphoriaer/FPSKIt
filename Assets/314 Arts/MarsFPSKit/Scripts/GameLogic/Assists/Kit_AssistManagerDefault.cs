using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace MarsFPSKit
{
    [System.Serializable]
    public class AssistedKillData
    {
        /// <summary>
        /// ID of the bot / Photon Actor Number of the player (if not a bot)
        /// </summary>
        public int id;
        /// <summary>
        /// Represents a bot?
        /// </summary>
        public bool bot;
    }

    [CreateAssetMenu(menuName = "MarsFPSKit/Assists/Default")]
    public class Kit_AssistManagerDefault : Kit_AssistManagerBase
    {
        /// <summary>
        /// How much xp is gained per assist?
        /// </summary>
        public int xpPerAssist = 20;

        public override void OnStart(Kit_IngameMain main)
        {

        }

        public override void PlayerDamaged(Kit_IngameMain main, bool botShot, int shotId, Kit_PlayerBehaviour damagedPlayer, float dmg)
        {
            //Assists only for team gamemodes
            if (main.currentPvPGameModeBehaviour && main.currentPvPGameModeBehaviour.isTeamGameMode)
            {
                if (damagedPlayer.damagedBy.Where(x => x.bot == botShot && x.id == shotId).Count() <= 0)
                {
                    damagedPlayer.damagedBy.Add(new AssistedKillData { bot = botShot, id = shotId });
                }
            }
        }

        public override void PlayerKilled(Kit_IngameMain main, bool botKiller, int idKiller, Kit_PlayerBehaviour killedPlayer)
        {
            if (main.currentPvPGameModeBehaviour && main.currentPvPGameModeBehaviour.isTeamGameMode)
            {
                for (int i = 0; i < killedPlayer.damagedBy.Count; i++)
                {
                    //Check if it counts as assist
                    if (!(killedPlayer.damagedBy[i].bot == botKiller && killedPlayer.damagedBy[i].id == idKiller))
                    {
                        int killerTeam = -1;
                        int assistTeam = -2;

                        if (botKiller)
                        {
                            Kit_Bot killerBot = main.currentBotManager.GetBotWithID(idKiller);
                            if (killerBot != null)
                            {
                                killerTeam = killerBot.team;
                            }
                        }
                        else
                        {
                            Player killerPlayer = Kit_PhotonPlayerExtensions.Find(idKiller);

                            if (killerPlayer != null)
                            {
                                killerTeam = (int)killerPlayer.CustomProperties["team"];
                            }
                        }

                        //Assist for bot
                        if (killedPlayer.damagedBy[i].bot)
                        {
                            Kit_Bot bot = main.currentBotManager.GetBotWithID(killedPlayer.damagedBy[i].id);
                            if (bot != null)
                            {
                                assistTeam = bot.team;

                                if (assistTeam == killerTeam)
                                {
                                    bot.assists++;
                                }
                            }
                        }
                        else
                        {
                            Player player = Kit_PhotonPlayerExtensions.Find(killedPlayer.damagedBy[i].id);

                            if (player != null)
                            {
                                assistTeam = (int)player.CustomProperties["team"];
                            }

                            if (assistTeam == killerTeam)
                            {
                                PhotonNetwork.RaiseEvent(Kit_EventIDs.assistEvent, null, new Photon.Realtime.RaiseEventOptions { TargetActors = new int[] { killedPlayer.damagedBy[i].id } }, new ExitGames.Client.Photon.SendOptions());
                            }
                        }
                    }
                }
            }
        }

        public override void OnPhotonEvent(Kit_IngameMain main, byte evCode, object content, int senderId)
        {
            if (evCode == Kit_EventIDs.assistEvent)
            {
                if (main.gameInformation.leveling)
                {
                    main.gameInformation.leveling.AddXp(main, xpPerAssist);
                }

                main.pointsUI.DisplayPoints(xpPerAssist, PointType.Assist);

                //Increment assists
                Hashtable myTable = PhotonNetwork.LocalPlayer.CustomProperties;
                int assists = (int)myTable["assists"];
                assists++;
                myTable["assists"] = assists;
                PhotonNetwork.LocalPlayer.SetCustomProperties(myTable);

                if (main.gameInformation.statistics)
                {
                    //Call
                    main.gameInformation.statistics.OnAssist(main);
                }
            }
        }
    }
}