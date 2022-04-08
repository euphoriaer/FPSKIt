using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        /// <summary>
        /// Implements a synced flashlight (FP, TP)
        /// This needs to be on both, FP AND TP! Onesided will not work.
        /// Thanks to Ciulama for sponsoring this!
        /// </summary>
        public class Kit_AttachmentFlashlight : Kit_AttachmentVisualBase
        {
            /// <summary>
            /// Light used for flashlight
            /// </summary>
            public Light flashlight;

            /// <summary>
            /// Is the flashlight enabled?
            /// </summary>
            private bool isFlashlightEnabled;

            /// <summary>
            /// Get Input for flashlight!
            /// </summary>
            private bool lastFlashlightInput;

            [HideInInspector]
            public AttachmentUseCase myUse = AttachmentUseCase.Drop;
            [HideInInspector]
            public Kit_PlayerBehaviour myPlayer;

            public override bool RequiresInteraction()
            {
                return true;
            }

            public override void Interaction(Kit_PlayerBehaviour pb)
            {
                if (lastFlashlightInput != pb.input.flashlight)
                {
                    lastFlashlightInput = pb.input.flashlight;
                    if (pb.input.flashlight)
                    {
                        //Switch...
                        isFlashlightEnabled = !isFlashlightEnabled;
                        //Manually call update once, for THIRD PERSON CASE!
                        Update();
                        if (thirdPersonEquivalent)
                        {
                            //Sync to TP
                            thirdPersonEquivalent.SyncFromFirstPerson(isFlashlightEnabled);
                        }
                        else
                        {
                            Debug.LogWarning("Shit bro! Flashlight script was not found on third person (same slot). Flashlight will not work in third person.");
                        }
                    }
                }
            }

            public override bool RequiresSyncing()
            {
                return true;
            }

            public override void SyncFromFirstPerson(object obj)
            {
                isFlashlightEnabled = (bool)obj;
            }

            public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info, Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data, int index)
            {
                if (stream.IsWriting)
                {
                    stream.SendNext(isFlashlightEnabled);
                }
                else
                {
                    isFlashlightEnabled = (bool)stream.ReceiveNext();

                    if (thirdPersonEquivalent)
                    {
                        //Sync to TP
                        thirdPersonEquivalent.SyncFromFirstPerson(isFlashlightEnabled);
                    }
                }
            }

            public override void Selected(Kit_PlayerBehaviour pb, AttachmentUseCase auc)
            {
                if (flashlight) flashlight.enabled = isFlashlightEnabled;

                //Assign use
                myUse = auc;
                myPlayer = pb;
                enabled = true;
            }

            void Update()
            {
                if (myPlayer)
                {
                    //If first person, only enable when third person is not active
                    if (myUse == AttachmentUseCase.FirstPerson)
                    {
                        if (myPlayer.looking.GetPerspective(myPlayer) == Kit_GameInformation.Perspective.ThirdPerson)
                        {
                            flashlight.enabled = false;
                        }
                        else
                        {
                            flashlight.enabled = isFlashlightEnabled;
                        }
                    }
                    //If third person, only enable when third person mode is active
                    else if (myUse == AttachmentUseCase.ThirdPerson)
                    {
                        if (myPlayer.looking.GetPerspective(myPlayer) == Kit_GameInformation.Perspective.ThirdPerson)
                        {
                            flashlight.enabled = isFlashlightEnabled;
                        }
                        else
                        {
                            flashlight.enabled = false;
                        }
                    }
                }
            }

            public override void SetVisibility(Kit_PlayerBehaviour pb, AttachmentUseCase auc, bool visible)
            {
                if (visible)
                {
                    enabled = true;
                }
                else
                {
                    enabled = false;
                    flashlight.enabled = false;
                }
            }
        }
    }
}