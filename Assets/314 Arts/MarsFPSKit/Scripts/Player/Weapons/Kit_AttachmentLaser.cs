using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        /// <summary>
        /// Implements a synced laser (FP, TP)
        /// This needs to be on both, FP AND TP! Onesided will not work.
        /// Thanks to Ciulama for sponsoring this!
        /// </summary>
        public class Kit_AttachmentLaser : Kit_AttachmentVisualBase
        {
            /// <summary>
            /// Light used for laser
            /// </summary>
            public Light laser;

            /// <summary>
            /// Line Renderer used for laser
            /// </summary>
            public LineRenderer laserLine;

            /// <summary>
            /// Game Object from which raycast is fired
            /// </summary>
            public Transform laserGO;

            /// <summary>
            /// Maximum distance for raycast
            /// </summary>
            public float maxLaserDistance = 500f;

            /// <summary>
            /// Layer mask for laser ;)
            /// </summary>
            public LayerMask laserMask;

            /// <summary>
            /// Raycast hit..
            /// </summary>
            public RaycastHit hit;

            /// <summary>
            /// Is the flashlight enabled?
            /// </summary>
            private bool isLaserEnabled;

            /// <summary>
            /// Player reference
            /// </summary>
            private Kit_PlayerBehaviour myPlayer;

            /// <summary>
            /// Use
            /// </summary>
            private AttachmentUseCase myUse;

            /// <summary>
            /// Get input!;
            /// </summary>
            private bool lastLaserInput;

            public override bool RequiresSyncing()
            {
                return true;
            }

            public override bool RequiresInteraction()
            {
                return true;
            }

            public override void SyncFromFirstPerson(object obj)
            {
                isLaserEnabled = (bool)obj;
            }

            public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info, Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data, int index)
            {
                if (stream.IsWriting)
                {
                    stream.SendNext(isLaserEnabled);
                }
                else
                {
                    isLaserEnabled = (bool)stream.ReceiveNext();

                    if (thirdPersonEquivalent)
                    {
                        //Sync to TP
                        thirdPersonEquivalent.SyncFromFirstPerson(isLaserEnabled);
                    }
                }
            }

            public override void Selected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {
                myPlayer = pb;
                myUse = auc;
                SetVisibility(pb, auc, isLaserEnabled);

                if (!pb) enabled = false; //Drop or loadout
            }

            public override void Interaction(Kit_PlayerBehaviour pb)
            {
                if (lastLaserInput != myPlayer.input.laser)
                {
                    lastLaserInput = myPlayer.input.laser;
                    if (myPlayer.input.laser)
                    {
                        //Switch...
                        isLaserEnabled = !isLaserEnabled;
                        //Manually call update once, for THIRD PERSON CASE!
                        Update();
                        if (thirdPersonEquivalent)
                        {
                            //Sync to TP
                            thirdPersonEquivalent.SyncFromFirstPerson(isLaserEnabled);
                        }
                        else
                        {
                            Debug.LogWarning("Shit bro! Laser script was not found on third person (same slot). Laser will not work in third person.");
                        }
                    }
                }
            }

            void Update()
            {
                UpdateLaser();
            }

            void LateUpdate()
            {
                UpdateLaser();
            }

            void UpdateLaser()
            {
                if (Physics.Raycast(laserGO.position, laserGO.forward, out hit, maxLaserDistance, laserMask, QueryTriggerInteraction.Ignore))
                {
                    laserLine.SetPosition(0, laserGO.position);
                    laserLine.SetPosition(1, hit.point);
                    laser.transform.position = hit.point + hit.normal * 0.03f;
                }
                else
                {
                    laserLine.SetPosition(0, laserGO.position);
                    laserLine.SetPosition(1, laserGO.position + laserGO.forward * maxLaserDistance);
                    laser.transform.position = laserGO.position + laserGO.forward * maxLaserDistance;
                }

                if (myPlayer)
                {
                    //If first person, only enable when third person is not active
                    if (myUse == AttachmentUseCase.FirstPerson)
                    {
                        if (myPlayer.looking.GetPerspective(myPlayer) == Kit_GameInformation.Perspective.ThirdPerson)
                        {
                            laser.enabled = false;
                            laserLine.enabled = false;
                        }
                        else
                        {
                            laser.enabled = isLaserEnabled;
                            laserLine.enabled = isLaserEnabled;
                        }
                    }
                    //If third person, only enable when third person mode is active
                    else if (myUse == AttachmentUseCase.ThirdPerson)
                    {
                        if (myPlayer.looking.GetPerspective(myPlayer) == Kit_GameInformation.Perspective.ThirdPerson)
                        {
                            laser.enabled = isLaserEnabled;
                            laserLine.enabled = isLaserEnabled;
                        }
                        else
                        {
                            laser.enabled = false;
                            laserLine.enabled = false;
                        }
                    }
                }
            }

            public override void SetVisibility(Kit_PlayerBehaviour pb, AttachmentUseCase auc, bool visible)
            {
                if (visible)
                {
                    UpdateLaser();
                }
                else
                {
                    laserLine.enabled = false;
                    laser.enabled = false;
                }
            }
        }
    }
}