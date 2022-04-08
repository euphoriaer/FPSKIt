using ExitGames.Client.Photon;
using MarsFPSKit.UI;
using MarsFPSKit.Weapons;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MarsFPSKit
{
    public class Kit_Plugin : Kit_WeaponInjection
    {
        /// <summary>
        /// Called on Start before anything else is setup
        /// </summary>
        /// <param name="main"></param>
        public virtual void OnPreSetup(Kit_IngameMain main)
        {

        }

        /// <summary>
        /// Called when everything is done
        /// </summary>
        /// <param name="main"></param>
        public virtual void OnSetupDone(Kit_IngameMain main)
        {

        }

        /// <summary>
        /// Called in update by Kit_IngameMain
        /// </summary>
        /// <param name="main"></param>
        public virtual void PluginUpdate(Kit_IngameMain main)
        {

        }

        /// <summary>
        /// Called in LateUpdate Kit_IngameMain
        /// </summary>
        /// <param name="main"></param>
        public virtual void PluginLateUpdate(Kit_IngameMain main)
        {

        }

        /// <summary>
        /// Called when the local player (not bots) joins a team
        /// </summary>
        /// <param name="main"></param>
        /// <param name="newTeam"></param>
        public virtual void LocalPlayerChangedTeam(Kit_IngameMain main, int newTeam)
        {

        }

        /// <summary>
        /// Called when a player left the room
        /// </summary>
        /// <param name="main"></param>
        /// <param name="player"></param>
        public virtual void PlayerLeftRoom(Kit_IngameMain main, Player player)
        {

        }

        /// <summary>
        /// Called when a player joined the room
        /// </summary>
        /// <param name="main"></param>
        /// <param name="player"></param>
        public virtual void PlayerJoinedRoom(Kit_IngameMain main, Player player)
        {

        }

        /// <summary>
        /// Called when the master client switched
        /// </summary>
        /// <param name="main"></param>
        /// <param name="newMasterClient"></param>
        public virtual void MasterClientSwitched(Kit_IngameMain main, Player newMasterClient)
        {

        }

        /// <summary>
        /// Called when custom properties were changed
        /// </summary>
        /// <param name="main"></param>
        /// <param name="player"></param>
        /// <param name="changedProperties"></param>
        public virtual void OnPlayerPropertiesChanged(Kit_IngameMain main, Player player, Hashtable changedProperties)
        {

        }

        /// <summary>
        /// Called by ingame main for serialization
        /// </summary>
        /// <param name="main"></param>
        /// <param name="steam"></param>
        /// <param name="info"></param>
        public virtual void OnPhotonSerializeView(Kit_IngameMain main, PhotonStream stream, PhotonMessageInfo info)
        {

        }

        /// <summary>
        /// Called by ingame main for photon events
        /// </summary>
        /// <param name="main"></param>
        /// <param name="eventCode"></param>
        /// <param name="content"></param>
        /// <param name="senderId"></param>
        public virtual void OnPhotonEvent(Kit_IngameMain main, byte eventCode, object content, int senderId)
        {

        }

        /// <summary>
        /// Called when the local player spawned
        /// </summary>
        /// <param name="player"></param>
        public virtual void LocalPlayerSpawned(Kit_PlayerBehaviour player)
        {

        }

        /// <summary>
        /// Called when the local player died
        /// </summary>
        /// <param name="player"></param>
        public virtual void LocalPlayerDied(Kit_PlayerBehaviour player)
        {

        }

        /// <summary>
        /// Called when a player spawned
        /// </summary>
        /// <param name="player"></param>
        public virtual void PlayerSpawned(Kit_PlayerBehaviour player)
        {

        }

        /// <summary>
        /// Called when a player died
        /// </summary>
        /// <param name="player"></param>
        public virtual void PlayerDied(Kit_PlayerBehaviour player)
        {

        }

        /// <summary>
        /// Called by our local player in update
        /// </summary>
        /// <param name="player"></param>
        public virtual void LocalPlayerUpdate(Kit_PlayerBehaviour player)
        {

        }

        /// <summary>
        /// Called by other players and bots in update
        /// </summary>
        /// <param name="player"></param>
        public virtual void PlayerUpdate(Kit_PlayerBehaviour player)
        {

        }

        /// <summary>
        /// Serialize info for each player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public virtual void PlayerOnPhotonSerializeView(Kit_PlayerBehaviour player, PhotonStream stream, PhotonMessageInfo info)
        {

        }

        /// <summary>
        /// Called in start by the main menu. You can reset data here
        /// </summary>
        /// <param name=""></param>
        public virtual void Reset(Kit_MenuManager menu)
        {

        }

        public virtual void BotWasKilled(Kit_IngameMain main, Kit_Bot bot)
        {

        }

        public virtual void BotScoredKill(Kit_IngameMain main, Kit_Bot bot, Hashtable deathInformation)
        {

        }

        public virtual void LocalPlayerWasKilled(Kit_IngameMain main)
        {

        }

        public virtual void LocalPlayerScoredKill(Kit_IngameMain main, Hashtable deathInformation)
        {

        }

        /// <summary>
        /// Serialize info for each player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public virtual void BotManagerOnPhotonSerializeView(Kit_BotManager manager, PhotonStream stream, PhotonMessageInfo info)
        {

        }

        public virtual void BotWasCreated(Kit_BotManager manager, Kit_Bot bot)
        {

        }
    }
}