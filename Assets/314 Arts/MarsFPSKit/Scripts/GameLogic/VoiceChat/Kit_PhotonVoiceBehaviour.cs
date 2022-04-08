using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Voice;
using Photon.Voice.Unity;
using Photon.Voice.PUN;

namespace MarsFPSKit
{
    /// <summary>
    /// This holds the voice chat information and controls the display state
    /// </summary>
    public class Kit_PhotonVoiceBehaviour : MonoBehaviour
    {
#if !UNITY_WEBGL && !DISABLE_VOICECHAT
        public PhotonView photonView;
        public Speaker speaker;

        //UI
        /// <summary>
        /// Reference to the voice chat
        /// </summary>
        private Kit_PhotonVoiceChat photonVoiceChat;
        /// <summary>
        /// Assigned UI ID
        /// </summary>
        private int myUi = -1;
        //END

        void Start()
        {
            photonVoiceChat = FindObjectOfType<Kit_PhotonVoiceChat>();
            if (photonVoiceChat)
            {
                //Get UI
                myUi = photonVoiceChat.GetUnusedUI();
                //Set name
                photonVoiceChat.activeUis[myUi].playerName.text = photonView.Owner.NickName;
                //Set different color for myself
                if (photonView.IsMine)
                {
                    photonVoiceChat.activeUis[myUi].playerName.color = photonVoiceChat.uiOwnColor;
                }
            }
        }

        void Update()
        {
            if (myUi >= 0 && PhotonVoiceNetwork.Instance && PhotonVoiceNetwork.Instance.PrimaryRecorder)
            {
                if (photonView.IsMine)
                {
                    //Make sure its recording now.
                    if (PhotonVoiceNetwork.Instance.PrimaryRecorder.RequiresRestart)
                    {
                        PhotonVoiceNetwork.Instance.PrimaryRecorder.RestartRecording();
                    }

                    if (PhotonVoiceNetwork.Instance.PrimaryRecorder.IsCurrentlyTransmitting)
                    {
                        photonVoiceChat.activeUis[myUi].gameObject.SetActiveOptimized(true);
                    }
                    else
                    {
                        photonVoiceChat.activeUis[myUi].gameObject.SetActiveOptimized(false);
                    }
                }
                else
                {
                    if (speaker.IsPlaying)
                    {
                        photonVoiceChat.activeUis[myUi].gameObject.SetActiveOptimized(true);
                    }
                    else
                    {
                        photonVoiceChat.activeUis[myUi].gameObject.SetActiveOptimized(false);
                    }

                    //Set color based on Team
                    if (PhotonVoiceNetwork.Instance.PrimaryRecorder.InterestGroup == 1)
                    {
                        photonVoiceChat.activeUis[myUi].playerName.color = photonVoiceChat.uiTeamOneColor;
                    }
                    else if (PhotonVoiceNetwork.Instance.PrimaryRecorder.InterestGroup == 2)
                    {
                        photonVoiceChat.activeUis[myUi].playerName.color = photonVoiceChat.uiTeamTwoColor;
                    }
                    else
                    {
                        photonVoiceChat.activeUis[myUi].playerName.color = photonVoiceChat.uiAllColor;
                    }
                }
            }
        }

        void OnDestroy()
        {
            //If voice chat is active and we have an UI assigned, release it
            if (photonVoiceChat && myUi >= 0)
            {
                photonVoiceChat.ReleaseUI(myUi);
            }
        }
#endif
    }
}
