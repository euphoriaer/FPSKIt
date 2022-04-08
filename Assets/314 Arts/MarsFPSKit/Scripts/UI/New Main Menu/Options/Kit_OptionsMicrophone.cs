using Photon.Voice.PUN;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace MarsFPSKit
{
    namespace UI
    {
        [CreateAssetMenu(menuName = "MarsFPSKit/Options/Audio/Photon Voice Microphone")]
        public class Kit_OptionsMicrophone : Kit_OptionBase
        {
            public override string GetDisplayName()
            {
                return "Voice Chat Microphone";
            }

            public override string GetHoverText()
            {
                return "The Microphone you want to use for the in-game voice chat.";
            }

            public override OptionType GetOptionType()
            {
                return OptionType.Dropdown;
            }

            public override void OnDropdownStart(TextMeshProUGUI txt, TMP_Dropdown dropdown)
            {
#if !UNITY_WEBGL && !DISABLE_VOICECHAT
                string defMic = "";
                if (Microphone.devices.Length > 0) defMic = Microphone.devices[0];
                string microphone = PlayerPrefs.GetString("voiceChatMicrophone", defMic);
                if (microphone != "" && microphone != null && Microphone.devices.Contains(microphone))
                {
                    if (PhotonVoiceNetwork.Instance && PhotonVoiceNetwork.Instance.PrimaryRecorder)
                    {
                        PhotonVoiceNetwork.Instance.PrimaryRecorder.UnityMicrophoneDevice = microphone;
                        if (PhotonVoiceNetwork.Instance.PrimaryRecorder.RequiresRestart)
                        {
                            PhotonVoiceNetwork.Instance.PrimaryRecorder.RestartRecording();
                        }
                    }

                    List<TMP_Dropdown.OptionData> devices = new List<TMP_Dropdown.OptionData>();
                    for (int i = 0; i < Microphone.devices.Length; i++)
                    {
                        devices.Add(new TMP_Dropdown.OptionData { text = Microphone.devices[i] });
                    }
                    dropdown.ClearOptions();
                    dropdown.AddOptions(devices);

                    int index = 0;
                    for (int i = 0; i < dropdown.options.Count; i++)
                    {
                        if (dropdown.options[i].text == microphone)
                        {
                            index = i;
                            break;
                        }
                    }
                    //Set active
                    dropdown.value = index;
                }
#endif
            }

            public override void OnDropdowChange(TextMeshProUGUI txt, int newValue)
            {
#if !UNITY_WEBGL && !DISABLE_VOICECHAT
                if (PhotonVoiceNetwork.Instance && PhotonVoiceNetwork.Instance.PrimaryRecorder)
                {
                    PhotonVoiceNetwork.Instance.PrimaryRecorder.UnityMicrophoneDevice = Microphone.devices[newValue];
                    PhotonVoiceNetwork.Instance.PrimaryRecorder.RestartRecording();
                }

                PlayerPrefs.SetString("voiceChatMicrophone", Microphone.devices[newValue]);
#endif
            }
        }
    }
}