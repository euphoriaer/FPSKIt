using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// This spawn system will always return true
    /// </summary>
    [CreateAssetMenu(menuName = "MarsFPSKit/Spawn Systems/Null Spawn System")]
    public class Kit_SpawnSystemNull : Kit_SpawnSystemBase
    {
        public override bool CheckSpawnPosition(Kit_IngameMain main, Transform spawnPoint, Photon.Realtime.Player spawningPlayer)
        {
            return true;
        }

        public override bool CheckSpawnPosition(Kit_IngameMain main, Transform spawnPoint, Kit_Bot bot)
        {
            return true;
        }
    }
}