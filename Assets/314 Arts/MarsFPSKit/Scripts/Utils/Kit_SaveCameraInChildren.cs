using UnityEngine;

namespace MarsFPSKit
{
    public class Kit_SaveCameraInChildren : MonoBehaviour
    {
        public void OnDestroy()
        {
            Camera cam = GetComponentInChildren<Camera>();
            Kit_IngameMain main = FindObjectOfType<Kit_IngameMain>();
            if (cam && main)
            {
                if (cam == main.mainCamera)
                {
                    main.activeCameraTransform = main.spawnCameraPosition;
                }
            }
        }
    }
}