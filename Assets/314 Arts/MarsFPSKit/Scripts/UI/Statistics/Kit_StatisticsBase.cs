using UnityEngine;

namespace MarsFPSKit
{
    public abstract class Kit_StatisticsBase : ScriptableObject
    {
        public abstract void OnStart(UI.Kit_MenuManager menu);

        /// <summary>
        /// Called when we score a kill with the supplied weapon
        /// </summary>
        /// <param name="weapon"></param>
        public abstract void OnKill(Kit_IngameMain main, int weapon);

        /// <summary>
        /// Called when we score a kill with the supplied reason
        /// </summary>
        /// <param name="reason"></param>
        public abstract void OnKill(Kit_IngameMain main, string reason);

        /// <summary>
        /// Ccalled when we score an assists
        /// </summary>
        public abstract void OnAssist(Kit_IngameMain main);

        /// <summary>
        /// Called when we were killed
        /// </summary>
        /// <param name="weapon"></param>
        public abstract void OnDeath(Kit_IngameMain main, int weapon);

        /// <summary>
        /// Called when we were killed
        /// </summary>
        /// <param name="reason"></param>
        public abstract void OnDeath(Kit_IngameMain main, string weapon);

        /// <summary>
        /// Save
        /// </summary>
        /// <param name="main"></param>
        public abstract void Save(Kit_IngameMain main);

        /// <summary>
        /// Save
        /// </summary>
        /// <param name="menu"></param>
        public abstract void Save(UI.Kit_MenuManager menu);
    }
}