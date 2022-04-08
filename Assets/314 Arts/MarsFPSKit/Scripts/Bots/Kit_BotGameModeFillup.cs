using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    [CreateAssetMenu(menuName = "MarsFPSKit/Bots/Game Mode Behaviour/Simple Fillup")]
    /// <summary>
    /// This script fills up bots to the player limit, supports team based game modes too
    /// </summary>
    public class Kit_BotGameModeFillup : Kit_BotGameModeManagerBase
    {
        public override void Inizialize(Kit_BotManager manager)
        {
            if (manager.main.currentPvPGameModeBehaviour.isTeamGameMode)
            {
                int tries = 0;

                for (int i = 0; i < Mathf.Clamp(manager.main.gameInformation.allPvpTeams.Length, 0, manager.main.currentPvPGameModeBehaviour.maximumAmountOfTeams); i++)
                {
                    int team = i;
                    //Reset tries
                    tries = 0;
                    if (manager.GetPlayersInTeamX(i) + manager.GetBotsInTeamX(i) < PhotonNetwork.CurrentRoom.MaxPlayers / Mathf.Clamp(manager.main.gameInformation.allPvpTeams.Length, 0, manager.main.currentPvPGameModeBehaviour.maximumAmountOfTeams))
                    {
                        while (manager.GetPlayersInTeamX(i) + manager.GetBotsInTeamX(i) < PhotonNetwork.CurrentRoom.MaxPlayers / 2 && tries <= 20)
                        {
                            Kit_Bot bot = manager.AddNewBot();
                            bot.team = team;
                            tries++;
                        }
                    }
                    else if (manager.GetPlayersInTeamX(i) + manager.GetBotsInTeamX(i) > PhotonNetwork.CurrentRoom.MaxPlayers / Mathf.Clamp(manager.main.gameInformation.allPvpTeams.Length, 0, manager.main.currentPvPGameModeBehaviour.maximumAmountOfTeams))
                    {
                        while (manager.GetPlayersInTeamX(i) + manager.GetBotsInTeamX(i) > PhotonNetwork.CurrentRoom.MaxPlayers / Mathf.Clamp(manager.main.gameInformation.allPvpTeams.Length, 0, manager.main.currentPvPGameModeBehaviour.maximumAmountOfTeams) && tries <= 20)
                        {
                            manager.RemoveBotInTeam(team);
                            tries++;
                        }
                    }
                }
            }
            else
            {
                if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.CurrentRoom.MaxPlayers)
                    {
                        manager.AddNewBot();
                    }
                }
                else if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.CurrentRoom.MaxPlayers)
                    {
                        manager.RemoveRandomBot();
                    }
                }
            }
        }

        public override void PlayerJoinedTeam(Kit_BotManager manager)
        {
            if (manager.main.currentPvPGameModeBehaviour.isTeamGameMode)
            {
                int tries = 0;

                for (int i = 0; i < Mathf.Clamp(manager.main.gameInformation.allPvpTeams.Length, 0, manager.main.currentPvPGameModeBehaviour.maximumAmountOfTeams); i++)
                {
                    int team = i;
                    //Reset tries
                    tries = 0;
                    if (manager.GetPlayersInTeamX(i) + manager.GetBotsInTeamX(i) < PhotonNetwork.CurrentRoom.MaxPlayers / Mathf.Clamp(manager.main.gameInformation.allPvpTeams.Length, 0, manager.main.currentPvPGameModeBehaviour.maximumAmountOfTeams))
                    {
                        while (manager.GetPlayersInTeamX(i) + manager.GetBotsInTeamX(i) < PhotonNetwork.CurrentRoom.MaxPlayers / 2 && tries <= 20)
                        {
                            Kit_Bot bot = manager.AddNewBot();
                            bot.team = team;
                            tries++;
                        }
                    }
                    else if (manager.GetPlayersInTeamX(i) + manager.GetBotsInTeamX(i) > PhotonNetwork.CurrentRoom.MaxPlayers / Mathf.Clamp(manager.main.gameInformation.allPvpTeams.Length, 0, manager.main.currentPvPGameModeBehaviour.maximumAmountOfTeams))
                    {
                        while (manager.GetPlayersInTeamX(i) + manager.GetBotsInTeamX(i) > PhotonNetwork.CurrentRoom.MaxPlayers / Mathf.Clamp(manager.main.gameInformation.allPvpTeams.Length, 0, manager.main.currentPvPGameModeBehaviour.maximumAmountOfTeams) && tries <= 20)
                        {
                            manager.RemoveBotInTeam(team);
                            tries++;
                        }
                    }
                }
            }
            else
            {
                int tries = 0;
                if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.CurrentRoom.MaxPlayers && tries <= 20)
                    {
                        manager.AddNewBot();
                        tries++;
                    }
                }
                else if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.CurrentRoom.MaxPlayers && tries <= 20)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.CurrentRoom.MaxPlayers)
                    {
                        manager.RemoveRandomBot();
                        tries++;
                    }
                }
            }
        }

        public override void PlayerLeftTeam(Kit_BotManager manager)
        {
            if (manager.main.currentPvPGameModeBehaviour.isTeamGameMode)
            {
                int tries = 0;

                for (int i = 0; i < Mathf.Clamp(manager.main.gameInformation.allPvpTeams.Length, 0, manager.main.currentPvPGameModeBehaviour.maximumAmountOfTeams); i++)
                {
                    int team = i;
                    //Reset tries
                    tries = 0;
                    if (manager.GetPlayersInTeamX(i) + manager.GetBotsInTeamX(i) < PhotonNetwork.CurrentRoom.MaxPlayers / Mathf.Clamp(manager.main.gameInformation.allPvpTeams.Length, 0, manager.main.currentPvPGameModeBehaviour.maximumAmountOfTeams))
                    {
                        while (manager.GetPlayersInTeamX(i) + manager.GetBotsInTeamX(i) < PhotonNetwork.CurrentRoom.MaxPlayers / 2 && tries <= 20)
                        {
                            Kit_Bot bot = manager.AddNewBot();
                            bot.team = team;
                            tries++;
                        }
                    }
                    else if (manager.GetPlayersInTeamX(i) + manager.GetBotsInTeamX(i) > PhotonNetwork.CurrentRoom.MaxPlayers / Mathf.Clamp(manager.main.gameInformation.allPvpTeams.Length, 0, manager.main.currentPvPGameModeBehaviour.maximumAmountOfTeams))
                    {
                        while (manager.GetPlayersInTeamX(i) + manager.GetBotsInTeamX(i) > PhotonNetwork.CurrentRoom.MaxPlayers / Mathf.Clamp(manager.main.gameInformation.allPvpTeams.Length, 0, manager.main.currentPvPGameModeBehaviour.maximumAmountOfTeams) && tries <= 20)
                        {
                            manager.RemoveBotInTeam(team);
                            tries++;
                        }
                    }
                }
            }
            else
            {
                if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() < PhotonNetwork.CurrentRoom.MaxPlayers)
                    {
                        manager.AddNewBot();
                    }
                }
                else if (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    //Fill up bots till the limit
                    while (manager.GetAmountOfBots() + manager.GetAmountOfPlayers() > PhotonNetwork.CurrentRoom.MaxPlayers)
                    {
                        manager.RemoveRandomBot();
                    }
                }
            }
        }
    }
}
