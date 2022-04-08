using UnityEngine;

namespace MarsFPSKit
{
    namespace Spectating
    {
        /// <summary>
        /// Defines whomst the local player can spectate
        /// </summary>
        public enum Spectateable { None = 0, Friendlies = 1, All = 2 }

        /// <summary>
        /// Impements a manager to spectate other players
        /// </summary>
        public abstract class Kit_SpectatorManagerBase : ScriptableObject
        {
            /// <summary>
            /// Setup
            /// </summary>
            /// <param name="main"></param>
            public abstract void Setup(Kit_IngameMain main);

            /// <summary>
            /// Can we spectate at all (global setting)?
            /// </summary>
            /// <param name="main"></param>
            /// <returns></returns>
            public abstract bool IsSpectatingEnabled(Kit_IngameMain main);

            /// <summary>
            /// Start spectating
            /// </summary>
            /// <param name="main"></param>
            public abstract void BeginSpectating(Kit_IngameMain main, bool leaveTeam);

            /// <summary>
            /// End our spectating
            /// </summary>
            /// <param name="main"></param>
            public abstract void EndSpectating(Kit_IngameMain main);

            /// <summary>
            /// Called when a player was spawned
            /// </summary>
            /// <param name="main"></param>
            /// <param name="pb"></param>
            public abstract void PlayerWasSpawned(Kit_IngameMain main, Kit_PlayerBehaviour pb);

            /// <summary>
            /// Called when a player was killed
            /// </summary>
            /// <param name="main"></param>
            /// <param name="pb"></param>
            public abstract void PlayerWasKilled(Kit_IngameMain main, Kit_PlayerBehaviour pb);

            /// <summary>
            /// True if spectator mode is currently active
            /// </summary>
            /// <param name="main"></param>
            /// <returns></returns>
            public abstract bool IsCurrentlySpectating(Kit_IngameMain main);
        }
    }
}