using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    [ExecuteInEditMode]
    /// <summary>
    /// Spawns a world object
    /// For gizmo mesh drawer, see here: https://github.com/PeterDekkers/unity3d-gizmo-mesh-preview
    /// </summary>
    public class Kit_SceneObjectSpawner : MonoBehaviour
    {
        public GameObject objectToSpawn;

        private void Start()
        {
            if (Application.isPlaying)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.InstantiateRoomObject(objectToSpawn.name, transform.position, transform.rotation);
                }
            }
        }

#if UNITY_EDITOR
        private void Awake()
        {
            gizmoMeshesCached = false;
        }


        /// <summary>
        /// You can turn gizmo mesh rendering off via the Inspector, if you want
        /// </summary>
        public bool showGizmoMesh = true;

        /// <summary>
        /// This gets set to 'true' once there are meshes cached.
        /// If you need to redraw the gizmo meshes (e.g. when your objectToSpawn changes)
        /// you can simply toggle this checkbox in the inspector and they
        /// will instantly update.
        /// </summary>
        public bool gizmoMeshesCached = false;

        /// <summary>
        /// We'll cache the meshes that we want to draw gizmos for.
        /// </summary>
        public MeshFilter[] gizmoMeshes = new MeshFilter[0];

        /// <summary>
        /// Cache transforms of all the meshes to draw gizmos for
        /// </summary>
        private Transform[] gizmoMeshTransforms;

        void OnDrawGizmos()
        {
            if (showGizmoMesh == false || objectToSpawn == null)
            {
                return;
            }

            // Fetch meshes inside the objectToSpawn once and cache them
            // and their transforms.
            if (!gizmoMeshesCached)
            {
                gizmoMeshes = objectToSpawn.GetComponentsInChildren<MeshFilter>(true);
                gizmoMeshTransforms = new Transform[gizmoMeshes.Length];
                for (int i = 0; i < gizmoMeshes.Length; i++)
                {
                    gizmoMeshTransforms[i] = gizmoMeshes[i].GetComponent<Transform>();
                }
                if (gizmoMeshes.Length > 0)
                {
                    gizmoMeshesCached = true;
                }
            }

            // If there are meshes in the array, draw a gizmo mesh for each
            if (gizmoMeshesCached)
            {

                for (int i = 0; i < gizmoMeshes.Length; i++)
                {

                    // Attempt to get a vertex color for the gizmo
                    if (gizmoMeshes[i].sharedMesh.colors.Length >= 1)
                    {
                        Gizmos.color = gizmoMeshes[i].sharedMesh.colors[0];
                    }
                    else
                    {
                        // Default to gray
                        Gizmos.color = Color.gray;
                    }

                    // Adjust the position and rotation of the gizmo mesh
                    Vector3 pos = transform.TransformPoint(gizmoMeshTransforms[i].position);
                    Quaternion rot = transform.rotation * gizmoMeshTransforms[i].rotation;

                    // Display the gizmo mesh
                    Gizmos.DrawMesh(gizmoMeshes[i].sharedMesh, pos, rot, Vector3.one);
                }
            }
        }
#endif
    }
}