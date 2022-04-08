using UnityEngine;

namespace MarsFPSKit
{
    public abstract class Kit_SpawnSystemBase : ScriptableObject
    {
        /// <summary>
        /// Returns true if we can spawn at this spawn point, according to the specific spawn system
        /// <para>See also: <seealso cref="Kit_GameModeBase.GetSpawn(Kit_IngameMain, Photon.Realtime.Player)"/></para>
        /// </summary>
        /// <param name="spawnPoint"></param>
        /// <param name="spawningPlayer"></param>
        /// <returns></returns>
        public abstract bool CheckSpawnPosition(Kit_IngameMain main, Transform spawnPoint, Photon.Realtime.Player spawningPlayer);

        public abstract bool CheckSpawnPosition(Kit_IngameMain main, Transform spawnPoint, Kit_Bot bot);
    }
}
