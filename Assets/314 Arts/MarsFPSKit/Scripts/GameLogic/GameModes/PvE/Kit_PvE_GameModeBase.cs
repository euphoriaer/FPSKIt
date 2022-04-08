using UnityEngine;
using System.Collections;
using System;
using Photon.Pun;
using Photon.Realtime;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarsFPSKit
{
    [System.Serializable]
    public class PlayerModelConfig
    {
        /// <summary>
        /// Player model to spawn with
        /// </summary>
        public Kit_PlayerModelInformation information;
        /// <summary>
        /// Selected customizations
        /// </summary>
        public int[] customization;
    }

    public abstract class Kit_PvE_GameModeBase : Kit_WeaponInjection
    {
        /// <summary>
        /// Name of this game mode
        /// </summary>
        public string gameModeName = "Sandbox";

        /// <summary>
        /// Maps
        /// </summary>
        [Tooltip("Maps for this game mode")]
        public Kit_MapInformation[] maps;

        /// <summary>
        /// How many players in coop?
        /// </summary>
        public byte coopPlayerAmount = 4;

        /// <summary>
        /// Menu Prefab in the menu
        /// </summary>
        [Header("Modules")]
        public GameObject menuPrefab;
        /// <summary>
        /// Which HUD prefab should be used for this game mode? Can be null.
        /// </summary>
        public GameObject hudPrefab;
        /// <summary>
        /// The spawn system that we want to use
        /// </summary>
        public Kit_SpawnSystemBase spawnSystemToUse;
        /// <summary>
        /// The bot manager that this game mode should use
        /// </summary>
        public Kit_BotGameModeManagerBase botManagerToUse;

        /// <summary>
        /// Use this to override bot controls
        /// </summary>
        public Kit_PlayerBotControlBase botControlOverride;

        /// <summary>
        /// Gets player model for this player
        /// </summary>
        /// <param name="pb"></param>
        /// <returns></returns>
        public abstract PlayerModelConfig GetPlayerModel(Kit_PlayerBehaviour pb);

        /// <summary>
        /// Returns the spawn loadout
        /// </summary>
        /// <returns></returns>
        public abstract Loadout GetSpawnLoadout();

        /// <summary>
        /// Called when stats are being reset
        /// </summary>
        /// <param name="table"></param>
        public virtual void ResetStats(ExitGames.Client.Photon.Hashtable table)
        {

        }

        /// <summary>
        /// Started upon starting playing with this game mode
        /// </summary>
        /// <param name="main"></param>
        public abstract void GamemodeSetup(Kit_IngameMain main);

        /// <summary>
        /// Called after player setup
        /// </summary>
        /// <param name="main"></param>
        public abstract void GameModeProceed(Kit_IngameMain main);


        /// <summary>
        /// Called every frame as long as this game mode is active
        /// </summary>
        /// <param name="main"></param>
        public abstract void GameModeUpdate(Kit_IngameMain main);

        /// <summary>
        /// Called every frame as long as this game mode is active for other players
        /// </summary>
        /// <param name="main"></param>
        public virtual void GameModeUpdateOthers(Kit_IngameMain main)
        {

        }

        /// <summary>
        /// Called every time a player dies
        /// </summary>
        /// <param name="main"></param>
        public abstract void PlayerDied(Kit_IngameMain main, bool botKiller, int killer, bool botKilled, int killed);

        /// <summary>
        /// Called when a player spawned (others + bots)
        /// </summary>
        /// <param name="pb"></param>
        public virtual void OnPlayerSpawned(Kit_PlayerBehaviour pb)
        {

        }

        /// <summary>
        /// Called when another player was destroyed (bots aswell)
        /// </summary>
        /// <param name="pb"></param>
        public virtual void OnPlayerDestroyed(Kit_PlayerBehaviour pb)
        {

        }

        /// <summary>
        /// Called when we successfully spawned (not bots)
        /// </summary>
        /// <param name="pb"></param>
        public virtual void OnLocalPlayerSpawned(Kit_PlayerBehaviour pb)
        {

        }

        /// <summary>
        /// Called when the local (controlling) player is destroyed
        /// </summary>
        /// <param name="pb"></param>
        public virtual void OnLocalPlayerDestroyed(Kit_PlayerBehaviour pb)
        {

        }

        /// <summary>
        /// Called when our death camera is over
        /// </summary>
        /// <param name="pb"></param>
        public virtual void OnLocalPlayerDeathCameraEnded(Kit_IngameMain main)
        {

        }

        /// <summary>
        /// Called for the master client when a bot has gained a kill
        /// </summary>
        /// <param name="main"></param>
        /// <param name="bot"></param>
        public virtual void MasterClientBotScoredKill(Kit_IngameMain main, Kit_Bot bot)
        {

        }

        /// <summary>
        /// Called when the timer reaches zero
        /// </summary>
        /// <param name="main"></param>
        public abstract void TimeRunOut(Kit_IngameMain main);

        /// <summary>
        /// Returns a spawnpoint for the associated player
        /// </summary>
        /// <param name="main"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract Transform GetSpawn(Kit_IngameMain main, Photon.Realtime.Player player);

        /// <summary>
        /// Returns a spawnpoint for the associated player
        /// </summary>
        /// <param name="main"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract Transform GetSpawn(Kit_IngameMain main, Kit_Bot bot);

        /// <summary>
        /// Can we currently spawn?
        /// </summary>
        /// <param name=""></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract bool CanSpawn(Kit_IngameMain main, Photon.Realtime.Player player);

        /// <summary>
        /// Does this game mode have a custom spawn method?
        /// </summary>
        /// <returns></returns>
        public virtual bool UsesCustomSpawn()
        {
            return false;
        }

        public virtual GameObject DoCustomSpawn(Kit_IngameMain main)
        {
            throw new NotImplementedException("Game mode " + this.name + " uses custom spawn, but it has not been implemented [players]!");
        }

        public virtual Loadout DoCustomSpawnBot(Kit_IngameMain main, Kit_Bot bot)
        {
            throw new NotImplementedException("Game mode " + this.name + " uses custom spawn, but it has not been implemented [bots]!");
        }

        /// <summary>
        /// Can the player be controlled at this stage of this game mode?
        /// </summary>
        /// <param name="main"></param>
        /// <returns></returns>
        public abstract bool CanControlPlayer(Kit_IngameMain main);

        /// <summary>
        /// Called when the player properties have been changed
        /// </summary>
        /// <param name="playerAndUpdatedProps"></param>
        public virtual void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
        {

        }

        /// <summary>
        /// Relay for serialization to sync data
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public virtual void OnPhotonSerializeView(Kit_IngameMain main, PhotonStream stream, PhotonMessageInfo info)
        {

        }

        /// <summary>
        /// Relay for serialization to sync custom game mode data that is stored on the player!
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public virtual void PlayerOnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info)
        {

        }

        /// <summary>
        /// Can weapons be dropped in this game mode?
        /// </summary>
        /// <param name="main"></param>
        /// <returns></returns>
        public virtual bool CanDropWeapons(Kit_IngameMain main)
        {
            return true;
        }

        /// <summary>
        /// Can a vote be started currently?
        /// </summary>
        /// <param name="main"></param>
        /// <returns></returns>
        public abstract bool CanStartVote(Kit_IngameMain main);

        public virtual void OnPhotonEvent(Kit_IngameMain main, byte eventCode, object content, int senderId)
        {

        }


        /// <summary>
        /// Override if you want to disable spectating. This setting is for the "global" spectating.
        /// </summary>
        /// <param name="main"></param>
        /// <returns></returns>
        public virtual bool SpectatingEnabled(Kit_IngameMain main)
        {
            return true;
        }

#if UNITY_EDITOR
        /// <summary>
        /// For the scene checker, returns state to display
        /// </summary>
        public abstract string[] GetSceneCheckerMessages();

        /// <summary>
        /// For the scene checker, returns state to display
        /// </summary>
        /// <returns></returns>
        public abstract MessageType[] GetSceneCheckerMessageTypes();
#endif
    }
}
