using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    [System.Serializable]
    public class Kit_Bot
    {
        /// <summary>
        /// The ID of this bot
        /// </summary>
        public int id;
        /// <summary>
        /// The name of this bot
        /// </summary>
        public string name;
        /// <summary>
        /// Team of this bot
        /// </summary>
        public int team;
        /// <summary>
        /// Kills of this bot
        /// </summary>
        public int kills;
        /// <summary>
        /// Assists of this bot
        /// </summary>
        public int assists;
        /// <summary>
        /// Deaths of this bot
        /// </summary>
        public int deaths;
        /// <summary>
        /// Can this bot spawn?
        /// </summary>
        public bool canSpawn = true;
        /// <summary>
        /// Custom per-bot data that can be used by the game mode for example!
        /// </summary>
        public object customData;
        /// <summary>
        /// Custom data per bot for bots
        /// </summary>
        public List<object> pluginCustomBotData = new List<object>();
    }

    public class Kit_BotManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        public List<Kit_Bot> bots = new List<Kit_Bot>();
        private List<Kit_PlayerBehaviour> activeBots = new List<Kit_PlayerBehaviour>();

        /// <summary>
        /// Bot name manager to use
        /// </summary>
        public Kit_BotNameManager nameManager;

        public Kit_BotLoadoutManager loadoutManager;

        [HideInInspector]
        /// <summary>
        /// Reference to ingame main
        /// </summary>
        public Kit_IngameMain main;

        public float spawnFrequency = 1f;
        private float lastSpawn;
        private int lastId;

        void Awake()
        {
            //Find main
            main = FindObjectOfType<Kit_IngameMain>();
            //Assign
            main.currentBotManager = this;
        }

        void Update()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (Time.time > lastSpawn)
                {
                    lastSpawn = Time.time + spawnFrequency;
                    SpawnBots();
                }
            }
        }

        void SpawnBots()
        {
            for (int i = 0; i < bots.Count; i++)
            {
                if (!IsBotAlive(bots[i]) && bots[i].canSpawn)
                {
                    SpawnBot(bots[i]);
                    break;
                }
            }
        }

        void SpawnBot(Kit_Bot bot)
        {
            //Get a spawn
            Transform spawnLocation = main.gameInformation.allPvpGameModes[main.currentGameMode].GetSpawn(main, bot);
            if (spawnLocation)
            {
                //Create object array for photon use
                object[] instData = new object[0];
                //Assign the values
                if (!main.currentPvPGameModeBehaviour.UsesCustomSpawn())
                {
                    //Get the current loadout
                    Loadout curLoadout = loadoutManager.GetBotLoadout(main);
                    int length = 1;
                    Hashtable playerDataTable = new Hashtable();
                    playerDataTable["team"] = bot.team;
                    playerDataTable["bot"] = true;
                    playerDataTable["playerModelID"] = curLoadout.teamLoadout[bot.team].playerModelID;
                    playerDataTable["playerModelCustomizations"] = curLoadout.teamLoadout[bot.team].playerModelCustomizations;
                    playerDataTable["botid"] = bot.id;
                    length++; //How many weapons we spawn with
                              //Every weapon now has its own hashtable!
                    length += curLoadout.loadoutWeapons.Length;
                    //Create instData
                    instData = new object[length];
                    instData[0] = playerDataTable;
                    instData[1] = curLoadout.loadoutWeapons.Length;
                    for (int i = 0; i < curLoadout.loadoutWeapons.Length; i++)
                    {
                        Hashtable weaponTable = new Hashtable();
                        weaponTable["slot"] = curLoadout.loadoutWeapons[i].goesToSlot;
                        weaponTable["id"] = curLoadout.loadoutWeapons[i].weaponID;
                        weaponTable["attachments"] = curLoadout.loadoutWeapons[i].attachments;
                        instData[2 + i] = weaponTable;
                    }
                }
                else
                {
                    //Game mode is not loadout driven
                    //Get the current loadout
                    Loadout curLoadout = main.currentPvPGameModeBehaviour.DoCustomSpawnBot(main, bot);
                    int length = 1;
                    Hashtable playerDataTable = new Hashtable();
                    playerDataTable["team"] = bot.team;
                    playerDataTable["bot"] = true;
                    playerDataTable["playerModelID"] = curLoadout.teamLoadout[bot.team].playerModelID;
                    playerDataTable["playerModelCustomizations"] = curLoadout.teamLoadout[bot.team].playerModelCustomizations;
                    playerDataTable["botid"] = bot.id;
                    length++; //How many weapons we spawn with
                              //Every weapon now has its own hashtable!
                    length += curLoadout.loadoutWeapons.Length;
                    //Create instData
                    instData = new object[length];
                    instData[0] = playerDataTable;
                    instData[1] = curLoadout.loadoutWeapons.Length;
                    for (int i = 0; i < curLoadout.loadoutWeapons.Length; i++)
                    {
                        Hashtable weaponTable = new Hashtable();
                        weaponTable["slot"] = curLoadout.loadoutWeapons[i].goesToSlot;
                        weaponTable["id"] = curLoadout.loadoutWeapons[i].weaponID;
                        weaponTable["attachments"] = curLoadout.loadoutWeapons[i].attachments;
                        instData[2 + i] = weaponTable;
                    }
                }
                PhotonNetwork.InstantiateRoomObject(main.playerPrefab.name, spawnLocation.position, spawnLocation.rotation, 0, instData);
            }
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                //Send last id
                stream.SendNext(lastId);
                //Send Count
                stream.SendNext(bots.Count);
                //Send contents
                for (int i = 0; i < bots.Count; i++)
                {
                    stream.SendNext(bots[i].id);
                    stream.SendNext(bots[i].name);
                    stream.SendNext(bots[i].team);
                    stream.SendNext(bots[i].kills);
                    stream.SendNext(bots[i].assists);
                    stream.SendNext(bots[i].deaths);
                }
            }
            else
            {
                //Get last id
                lastId = (int)stream.ReceiveNext();
                //Get Count
                int count = (int)stream.ReceiveNext();
                //Adjust length
                if (bots.Count != count)
                {
                    while (bots.Count > count) bots.RemoveAt(bots.Count - 1);
                    while (bots.Count < count)
                    {
                        Kit_Bot bot = new Kit_Bot();

                        for (int i = 0; i < main.gameInformation.plugins.Length; i++)
                        {
                            main.gameInformation.plugins[i].BotWasCreated(this, bot);
                        }

                        bots.Add(bot);
                    }
                }
                //Get contents
                for (int i = 0; i < count; i++)
                {
                    bots[i].id = (int)stream.ReceiveNext();
                    bots[i].name = (string)stream.ReceiveNext();
                    bots[i].team = (int)stream.ReceiveNext();
                    bots[i].kills = (int)stream.ReceiveNext();
                    bots[i].assists = (int)stream.ReceiveNext();
                    bots[i].deaths = (int)stream.ReceiveNext();
                }
            }

            for (int i = 0; i < main.gameInformation.plugins.Length; i++)
            {
                main.gameInformation.plugins[i].BotManagerOnPhotonSerializeView(this, stream, info);
            }
        }

        /// <summary>
        /// Adds a new, empty bot
        /// </summary>
        /// <returns></returns>
        public Kit_Bot AddNewBot()
        {
            Kit_Bot newBot = new Kit_Bot();
            newBot.id = lastId;
            lastId++;
            //Get a new name
            newBot.name = nameManager.GetRandomName(this);
            //Send chat message
            main.chat.SendBotMessage(newBot.name, 0);
            bots.Add(newBot);

            for (int i = 0; i < main.gameInformation.plugins.Length; i++)
            {
                main.gameInformation.plugins[i].BotWasCreated(this, newBot);
            }

            return newBot;
        }

        /// <summary>
        /// Removes a random bot
        /// </summary>
        public void RemoveRandomBot()
        {
            Kit_Bot toRemove = GetBotWithID(Random.Range(0, bots.Count));
            //Send chat message
            main.chat.SendBotMessage(toRemove.name, 1);
            if (IsBotAlive(toRemove))
            {
                PhotonNetwork.Destroy(GetAliveBot(toRemove).photonView);
            }
            bots.Remove(toRemove);
        }

        /// <summary>
        /// Removes a bot that is in this team
        /// </summary>
        /// <param name="team"></param>
        public void RemoveBotInTeam(int team)
        {
            Kit_Bot toRemove = bots.Find(x => x.team == team);
            if (toRemove != null)
            {
                //Send chat message
                main.chat.SendBotMessage(toRemove.name, 1);
                if (IsBotAlive(toRemove))
                {
                    PhotonNetwork.Destroy(GetAliveBot(toRemove).photonView);
                }
                bots.Remove(toRemove);
            }
        }

        public Kit_Bot GetBotWithID(int id)
        {
            for (int i = 0; i < bots.Count; i++)
            {
                if (bots[i].id == id)
                {
                    return bots[i];
                }
            }
            return null;
        }

        public bool IsBotAlive(Kit_Bot bot)
        {
            for (int i = 0; i < activeBots.Count; i++)
            {
                if (activeBots[i] && activeBots[i].isBot && activeBots[i].botId == bot.id)
                {
                    return true;
                }
            }
            return false;
        }

        public Kit_PlayerBehaviour GetAliveBot(Kit_Bot bot)
        {
            for (int i = 0; i < activeBots.Count; i++)
            {
                if (activeBots[i] && activeBots[i].isBot && activeBots[i].botId == bot.id)
                {
                    return activeBots[i];
                }
            }
            return null;
        }

        public void AddActiveBot(Kit_PlayerBehaviour bot)
        {
            activeBots.Add(bot);
        }

        /// <summary>
        /// How many bots are there?
        /// </summary>
        public int GetAmountOfBots()
        {
            return bots.Count;
        }

        /// <summary>
        /// How many bots are in team one?
        /// </summary>
        /// <returns></returns>
        public int GetBotsInTeamX(int team)
        {
            int toReturn = 0;
            for (int i = 0; i < bots.Count; i++)
            {
                if (bots[i].team == team) toReturn++;
            }
            return toReturn;
        }

        /// <summary>
        /// How many players are there?
        /// </summary>
        /// <returns></returns>
        public int GetAmountOfPlayers()
        {
            return PhotonNetwork.PlayerList.Length;
        }

        /// <summary>
        /// How many players are in team one?
        /// </summary>
        /// <returns></returns>
        public int GetPlayersInTeamX(int team)
        {
            int toReturn = 0;
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (PhotonNetwork.PlayerList[i].CustomProperties["team"] != null)
                {
                    int myTeam = (int)PhotonNetwork.PlayerList[i].CustomProperties["team"];
                    if (myTeam == team) toReturn++;
                }
            }
            return toReturn;
        }

        public void KillAllBots()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Kit_PlayerBehaviour[] players = FindObjectsOfType<Kit_PlayerBehaviour>();

                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i] && players[i].isBot)
                    {
                        PhotonNetwork.Destroy(players[i].gameObject);
                    }
                }
            }
        }
    }
}