using UnityEngine;
using System.Collections.Generic;
using System;
using Photon.Pun;
using Photon.Voice.PUN;

namespace MarsFPSKit
{
    public enum VoiceTransmitMode { PushToTalk = 0, Detection = 1, Disabled = 2 }

    /// <summary>
    /// This implements a voice chat using Photon Voice
    /// </summary>
    public class Kit_PhotonVoiceChat : Kit_VoiceChatBase
    {
        /// <summary>
        /// The prefab of the 'voice' game object
        /// </summary>
        public GameObject voicePrefab;

        [HideInInspector]
        /// <summary>
        /// Our currently active behaviour
        /// </summary>
        public Kit_PhotonVoiceBehaviour currentBehaviour;

#if !UNITY_WEBGL && !DISABLE_VOICECHAT
        private Kit_IngameMain main;
#endif

        [Header("UI")]
        /// <summary>
        /// The prefab for displaying it in the UI
        /// </summary>
        public GameObject uiPrefab;
        /// <summary>
        /// Where the UI prefabs are going to be instantiated
        /// </summary>
        public RectTransform uiGo;
        /// <summary>
        /// Normal color
        /// </summary>
        public Color uiAllColor = Color.white;
        /// <summary>
        /// The color that will be used if we are speaking ourselves
        /// </summary>
        public Color uiOwnColor = Color.cyan;
        /// <summary>
        /// The color that will be used if this player is transmitting to team one
        /// </summary>
        public Color uiTeamOneColor = Color.red;
        /// <summary>
        /// The color that will be used if this player is transmitting to team two
        /// </summary>
        public Color uiTeamTwoColor = Color.blue;
        [HideInInspector]
        /// <summary>
        /// This contains all active UIs
        /// </summary>
        public List<Kit_PhotonVoiceChatUI> activeUis = new List<Kit_PhotonVoiceChatUI>();

        public override void Setup(Kit_IngameMain newMain)
        {
#if !UNITY_WEBGL && !DISABLE_VOICECHAT
            main = newMain;
            if (!currentBehaviour)
            {
                //Create it
                GameObject go = PhotonNetwork.Instantiate(voicePrefab.name, Vector3.zero, Quaternion.identity, 0);
                //Get behaviour
                currentBehaviour = go.GetComponent<Kit_PhotonVoiceBehaviour>();
            }
#endif
        }

        public override void JoinedTeam(int team)
        {
#if !UNITY_WEBGL && !DISABLE_VOICECHAT
            byte[] toJoin = new byte[1];
            byte[] toLeave = new byte[1];
            if (team == 0)
            {
                toJoin[0] = 1;
                toLeave[0] = 2;
            }
            else if (team == 1)
            {
                toJoin[0] = 2;
                toLeave[0] = 1;
            }
            //Set to global audio group
            PhotonVoiceNetwork.Instance.Client.GlobalInterestGroup = 0;
            //Set audio groups
            PhotonVoiceNetwork.Instance.Client.OpChangeGroups(toLeave, toJoin);
#endif
        }
#if !UNITY_WEBGL && !DISABLE_VOICECHAT

        void Update()
        {
            if (currentBehaviour && PhotonVoiceNetwork.Instance && PhotonVoiceNetwork.Instance.PrimaryRecorder)
            {
                //Set settings based on the Transmit Mode
                if (Kit_GameSettings.voiceChatTransmitMode == VoiceTransmitMode.PushToTalk)
                {
                    //Disable detector and use key instead
                    PhotonVoiceNetwork.Instance.PrimaryRecorder.VoiceDetection = false;
                    if (Input.GetButton("Push To Talk"))
                    {
                        PhotonVoiceNetwork.Instance.PrimaryRecorder.TransmitEnabled = true;
                    }
                    else
                    {
                        PhotonVoiceNetwork.Instance.PrimaryRecorder.TransmitEnabled = false;
                    }
                }
                else if (Kit_GameSettings.voiceChatTransmitMode == VoiceTransmitMode.Detection)
                {
                    //Enable detector
                    PhotonVoiceNetwork.Instance.PrimaryRecorder.VoiceDetection = true;
                    PhotonVoiceNetwork.Instance.PrimaryRecorder.TransmitEnabled = true;
                }
                else if (Kit_GameSettings.voiceChatTransmitMode == VoiceTransmitMode.Disabled)
                {
                    //Disable
                    PhotonVoiceNetwork.Instance.PrimaryRecorder.VoiceDetection = false;
                    PhotonVoiceNetwork.Instance.PrimaryRecorder.TransmitEnabled = false;
                }

                //Enable team switching
                if (MarsScreen.lockCursor)
                {   
                    if (main.currentPvPGameModeBehaviour && main.currentPvPGameModeBehaviour.isTeamGameMode)
                    {
                        if (main.assignedTeamID != 2)
                        {
                            if (Input.GetButtonDown("Voice Chat Global"))
                            {
                                //Check if we aren't already in global
                                if (PhotonVoiceNetwork.Instance.Client.GlobalInterestGroup != 0)
                                {
                                    //Set global
                                    PhotonVoiceNetwork.Instance.Client.GlobalInterestGroup = 0;
                                    //Inform player
                                    main.DisplayMessage("Voice chat set to global");
                                }
                            }
                            else if (Input.GetButtonDown("Voice Chat Team"))
                            {
                                //Check if we aren't already in the team's group
                                if (PhotonVoiceNetwork.Instance.Client.GlobalInterestGroup != main.assignedTeamID + 1)
                                {
                                    PhotonVoiceNetwork.Instance.Client.GlobalInterestGroup = (byte)(main.assignedTeamID + 1);
                                    main.DisplayMessage("Voice chat set to team only");
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns an unused UI, sets that one to used. Instantiates new one if needed
        /// </summary>
        /// <returns></returns>
        public int GetUnusedUI()
        {
            for (int i = 0; i < activeUis.Count; i++)
            {
                if (!activeUis[i].isUsed)
                {
                    activeUis[i].isUsed = true;
                    return i;
                }
            }

            //Instantiate new
            GameObject newUi = Instantiate(uiPrefab, uiGo, false);
            //Get UI component
            Kit_PhotonVoiceChatUI ui = newUi.GetComponent<Kit_PhotonVoiceChatUI>();
            ui.isUsed = true;
            ui.gameObject.SetActive(false);
            activeUis.Add(ui);
            return activeUis.Count - 1;
        }

        /// <summary>
        /// Releases the given element
        /// </summary>
        /// <param name="index"></param>
        public void ReleaseUI(int index)
        {
            activeUis[index].gameObject.SetActive(false);
            activeUis[index].isUsed = false;
        }
#endif
    }
}
