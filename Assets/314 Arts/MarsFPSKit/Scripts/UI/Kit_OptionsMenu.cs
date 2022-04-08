using Photon.Voice.PUN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    public class Kit_OptionsMenu : MonoBehaviour
    {
        [Header("Graphics")]
        /// <summary>
        /// Dropdown 
        /// </summary>
        public Dropdown resolution;
        /// <summary>
        /// Dropdown for <see cref="QualitySettings.anisotropicFiltering"/>
        /// </summary>
        public Dropdown anisotropicFiltering;
        /// <summary>
        /// Dropdown for <see cref="QualitySettings.masterTextureLimit"/>
        /// </summary>
        public Dropdown textureQuality;
        /// <summary>
        /// Toggle for <see cref="QualitySettings.vSyncCount"/>
        /// </summary>
        public Toggle vSync;
        /// <summary>
        /// Dropdown for <see cref="QualitySettings.shadows"/>
        /// </summary>
        public Dropdown shadows;
        /// <summary>
        /// Dropdown for <see cref="QualitySettings.shadowResolution"/>
        /// </summary>
        public Dropdown shadowQuality;
        /// <summary>
        /// Slider for <see cref="QualitySettings.shadowDistance"/>
        /// </summary>
        public Slider shadowDistance;
        /// <summary>
        /// Shadow projection mode
        /// </summary>
        public Dropdown shadowProjectionMode;
        /// <summary>
        /// Shadowmask mode
        /// </summary>
        public Dropdown shadowmaskMode;
        /// <summary>
        /// Shadow cascades
        /// </summary>
        public Dropdown shadowCascades;
        /// <summary>
        /// Toggle for <see cref="Screen.fullScreen"/>
        /// </summary>
        public Toggle fullscreen;
        /// <summary>
        /// Soft particles enabled?
        /// </summary>
        public Toggle softParticles;
        /// <summary>
        /// Realtime reflection probes enabled?
        /// </summary>
        public Toggle realtimeReflectionProbes;
        /// <summary>
        /// LOD Bias
        /// </summary>
        public Slider lodBias;

        [Header("Audio")]
        /// <summary>
        /// The slider for the audio listener volume
        /// </summary>
        public Slider masterVolume;
        /// <summary>
        /// The audio mode slider
        /// </summary>
        public Dropdown audioMode;

        [Header("Voice Chat")]
        /// <summary>
        /// This dropdown chooses the voice chat mode
        /// </summary>
        public Dropdown voiceChatMode;
        /// <summary>
        /// This dropdown chooses the voice chat device
        /// </summary>
        public Dropdown voiceChatDevices;

        [Header("Controls")]
        /// <summary>
        /// Controls hip sensitivity
        /// </summary>
        public Slider controlsHipSensitivity;
        /// <summary>
        /// Controls aim sensitivity
        /// </summary>
        public Slider controlsAimSensitivity;
        /// <summary>
        /// Controls scope sensitivity
        /// </summary>
        public Slider controlsScopeSensitivity;
        /// <summary>
        /// Controls whether aiming is toggle or hold
        /// </summary>
        public Toggle controlsAimingToggle;
        /// <summary>
        /// Controls whether crouching is toggle or hold
        /// </summary>
        public Toggle controlsCrouchToggle;

        [Header("Gameplay")]
        /// <summary>
        /// Gameplay field of view
        /// </summary>
        public Slider gameplayFieldOfView;
        /// <summary>
        /// Gameplay field of view texxt
        /// </summary>
        public Text gameplayFieldOfViewText;

        void Start()
        {
            //Setup resolutions
            resolution.ClearOptions();
            List<Dropdown.OptionData> screenOptions = new List<Dropdown.OptionData>();
            for (int i = 0; i < Screen.resolutions.Length; i++)
            {
                screenOptions.Add(new Dropdown.OptionData { text = Screen.resolutions[i].width + "x" + Screen.resolutions[i].height + "@" + Screen.resolutions[i].refreshRate + "Hz" });
            }
            //Set
            resolution.AddOptions(screenOptions);

            //Setup callbacks
            resolution.onValueChanged.AddListener(delegate { ResolutionValueChanged(resolution.value); });
            anisotropicFiltering.onValueChanged.AddListener(delegate { AnisotropicValueChanged(anisotropicFiltering.value); });
            textureQuality.onValueChanged.AddListener(delegate { TextureQualityValueChanged(textureQuality.value); });
            vSync.onValueChanged.AddListener(delegate { VSyncValueChanged(vSync.isOn); });
            shadows.onValueChanged.AddListener(delegate { ShadowsValueChanged(shadows.value); });
            shadowQuality.onValueChanged.AddListener(delegate { ShadowQualityValueChanged(shadowQuality.value); });
            shadowDistance.onValueChanged.AddListener(delegate { ShadowDistanceValueChanged(shadowDistance.value); });
            shadowCascades.onValueChanged.AddListener(delegate { ShadowCascadeModeChanged(shadowCascades.value); });
            shadowmaskMode.onValueChanged.AddListener(delegate { ShadowmaskModeChanged(shadowmaskMode.value); });
            shadowProjectionMode.onValueChanged.AddListener(delegate { ShadowProjectionModeChanged(shadowProjectionMode.value); });
            fullscreen.onValueChanged.AddListener(delegate { OnFullscreenValueChanged(fullscreen.isOn); });
            softParticles.onValueChanged.AddListener(delegate { SoftParticlesChanged(softParticles.isOn); });
            realtimeReflectionProbes.onValueChanged.AddListener(delegate { RealtimeReflectionProbesChanged(realtimeReflectionProbes.isOn); });
            lodBias.onValueChanged.AddListener(delegate { LODBiasChanged(lodBias.value); });

            masterVolume.onValueChanged.AddListener(delegate { MasterVolumeValueChanged(masterVolume.value); });
            audioMode.onValueChanged.AddListener(delegate { AudioSpeakerModeChanged(audioMode.value); });
#if UNITY_WEBGL
            voiceChatDevices.transform.parent.gameObject.SetActive(false);
            voiceChatMode.transform.parent.gameObject.SetActive(false);
#else
            voiceChatMode.onValueChanged.AddListener(delegate { VoiceChatModeChanged(voiceChatMode.value); });
            voiceChatDevices.onValueChanged.AddListener(delegate { VoiceChatDeviceChanged(voiceChatDevices.value); });
#endif
            controlsHipSensitivity.onValueChanged.AddListener(delegate { ControlsHipSensitivityChanged(controlsHipSensitivity.value); });
            controlsAimSensitivity.onValueChanged.AddListener(delegate { ControlsAimSensitivityChanged(controlsAimSensitivity.value); });
            controlsScopeSensitivity.onValueChanged.AddListener(delegate { ControlsScopeSensitivityChanged(controlsScopeSensitivity.value); });
            controlsAimingToggle.onValueChanged.AddListener(delegate { ControlsAimingToggleChanged(controlsAimingToggle.isOn); });
            controlsCrouchToggle.onValueChanged.AddListener(delegate { ControlsCrouchingToggleChanged(controlsCrouchToggle.isOn); });

            gameplayFieldOfView.onValueChanged.AddListener(delegate { GameplayFieldOfViewChanged(gameplayFieldOfView.value); });

            //Load
            Load();
        }

        private void LODBiasChanged(float value)
        {
            QualitySettings.lodBias = value;
            Save();
        }

        private void RealtimeReflectionProbesChanged(bool isOn)
        {
            QualitySettings.realtimeReflectionProbes = isOn;
            Save();
        }

        private void SoftParticlesChanged(bool isOn)
        {
            QualitySettings.softParticles = isOn;
            Save();
        }

        private void ShadowProjectionModeChanged(int value)
        {
            QualitySettings.shadowProjection = (ShadowProjection)value;
            Save();
        }

        private void ShadowmaskModeChanged(int value)
        {
            QualitySettings.shadowmaskMode = (ShadowmaskMode)value;
            Save();
        }

        private void ShadowCascadeModeChanged(int value)
        {
            if (value == 0) value = 0;
            else if (value == 1) value = 2;
            else if (value == 2) value = 4;
            QualitySettings.shadowCascades = value;
            Save();
        }

        private void OnFullscreenValueChanged(bool isOn)
        {
            Screen.fullScreen = isOn;
            Save();
        }

        private void MasterVolumeValueChanged(float value)
        {
            AudioListener.volume = value / 100f;
            Save();
        }

        private void ShadowDistanceValueChanged(float value)
        {
            QualitySettings.shadowDistance = value;
            Save();
        }

        private void ShadowQualityValueChanged(int value)
        {
            QualitySettings.shadowResolution = (ShadowResolution)value;
            Save();
        }

        private void ShadowsValueChanged(int value)
        {
            QualitySettings.shadows = (ShadowQuality)value;
            Save();
        }

        private void VSyncValueChanged(bool isOn)
        {
            QualitySettings.vSyncCount = isOn ? 1 : 0;
            Save();
        }

        private void TextureQualityValueChanged(int value)
        {
            QualitySettings.masterTextureLimit = value;
            Save();
        }

        private void AnisotropicValueChanged(int value)
        {
            QualitySettings.anisotropicFiltering = (AnisotropicFiltering)value;
            Save();
        }

        private void ResolutionValueChanged(int value)
        {
            if (value >= Screen.resolutions.Length) value = Screen.resolutions.Length - 1;
            Screen.SetResolution(Screen.resolutions[value].width, Screen.resolutions[value].height, fullscreen.isOn, Screen.resolutions[value].refreshRate);
            Save();
        }

        private void AudioSpeakerModeChanged(int value)
        {
            //Raw is skipped, so add + 1
            AudioSpeakerMode mode = (AudioSpeakerMode)(value + 1);
            //Get current config
            AudioConfiguration config = AudioSettings.GetConfiguration();
            //Check if the mode changed
            if (config.speakerMode != mode)
            {
                config.speakerMode = mode;
                AudioSettings.Reset(config);
            }
            Save();
        }

#if !UNITY_WEBGL && !DISABLE_VOICECHAT
        private void VoiceChatModeChanged(int value)
        {
            Kit_GameSettings.voiceChatTransmitMode = (VoiceTransmitMode)value;
            Save();
        }

        private void VoiceChatDeviceChanged(int value)
        {
            //Check if its still up to date
            if (value < Microphone.devices.Length)
            {
                if (PhotonVoiceNetwork.Instance && PhotonVoiceNetwork.Instance.PrimaryRecorder)
                {
                    if (PhotonVoiceNetwork.Instance.PrimaryRecorder.UnityMicrophoneDevice != Microphone.devices[value])
                    {
                        //Check if local change is needed
                        if (PhotonVoiceNetwork.Instance && PhotonVoiceNetwork.Instance.PrimaryRecorder)
                        {
                            PhotonVoiceNetwork.Instance.PrimaryRecorder.UnityMicrophoneDevice = Microphone.devices[value];
                            PhotonVoiceNetwork.Instance.PrimaryRecorder.RestartRecording();
                        }
                    }
                }
            }
            Save();
        }

        public void RedrawVoiceChatDevices()
        {
            List<Dropdown.OptionData> devices = new List<Dropdown.OptionData>();
            for (int i = 0; i < Microphone.devices.Length; i++)
            {
                devices.Add(new Dropdown.OptionData { text = Microphone.devices[i] });
            }
            voiceChatDevices.ClearOptions();
            voiceChatDevices.AddOptions(devices);
        }
#endif

        private void ControlsScopeSensitivityChanged(float value)
        {
            Kit_GameSettings.fullScreenAimSensitivity = value;
            Save();
        }

        private void ControlsAimSensitivityChanged(float value)
        {
            Kit_GameSettings.aimSensitivity = value;
            Save();
        }

        private void ControlsHipSensitivityChanged(float value)
        {
            Kit_GameSettings.hipSensitivity = value;
            Save();
        }

        private void ControlsAimingToggleChanged(bool value)
        {
            Kit_GameSettings.isAimingToggle = value;
            Save();
        }

        private void ControlsCrouchingToggleChanged(bool value)
        {
            Kit_GameSettings.isCrouchToggle = value;
            Save();
        }

        private void GameplayFieldOfViewChanged(float value)
        {
            Kit_GameSettings.baseFov = value;
            //Set text
            gameplayFieldOfViewText.text = "Field of View: " + Kit_GameSettings.baseFov.ToString("F0") + "° ";
            Save();
        }

        bool loadDone = false;

        void Load()
        {
            fullscreen.isOn = PlayerPrefsExtended.GetBool("optionsFullscreen", Screen.fullScreen);
            resolution.value = PlayerPrefs.GetInt("optionsResolution", Screen.resolutions.Length - 1);
            anisotropicFiltering.value = PlayerPrefs.GetInt("optionsAnisotropicFiltering", 2);
            textureQuality.value = PlayerPrefs.GetInt("optionsTextureQuality", 0);
            vSync.isOn = PlayerPrefsExtended.GetBool("optionsVerticalSync", true);
            shadows.value = PlayerPrefs.GetInt("optionsShadows", 2);
            shadowQuality.value = PlayerPrefs.GetInt("optionsShadowsQuality", 3);
            shadowDistance.value = PlayerPrefs.GetFloat("optionsShadowDistance", 50);

            shadowProjectionMode.value = PlayerPrefs.GetInt("optionsShadowProjectionMode", 1);
            shadowmaskMode.value = PlayerPrefs.GetInt("optionsShadowmaskMode", 1);
            shadowCascades.value = PlayerPrefs.GetInt("optionsShadowCascades", 2);
            lodBias.value = PlayerPrefs.GetFloat("optionsLodBias", 1);
            realtimeReflectionProbes.isOn = PlayerPrefsExtended.GetBool("optionsRealtimeReflectionProbes", true);
            softParticles.isOn = PlayerPrefsExtended.GetBool("optionsSoftParticles", true);

            masterVolume.value = PlayerPrefs.GetFloat("optionsVolume", 100f);
            audioMode.value = PlayerPrefs.GetInt("optionsAudioMode", 1);
            //Manually redraw audio
            audioMode.RefreshShownValue();
#if !UNITY_WEBGL && !DISABLE_VOICECHAT
            voiceChatMode.value = PlayerPrefs.GetInt("optionsVoiceChatMode", 0);
            //Manually redraw voice chat
            voiceChatMode.RefreshShownValue();
            string defMic = "";
            if (Microphone.devices.Length > 0) defMic = Microphone.devices[0];
            string microphone = PlayerPrefs.GetString("optionsVoiceChatMicrophone", defMic);
            if (microphone != "" && microphone != null && Microphone.devices.Contains(microphone))
            {
                if (PhotonVoiceNetwork.Instance && PhotonVoiceNetwork.Instance.PrimaryRecorder)
                {
                    PhotonVoiceNetwork.Instance.PrimaryRecorder.UnityMicrophoneDevice = microphone;
                    PhotonVoiceNetwork.Instance.PrimaryRecorder.RestartRecording();
                }
                //Redraw
                RedrawVoiceChatDevices();
                int index = 0;
                for (int i = 0; i < voiceChatDevices.options.Count; i++)
                {
                    if (voiceChatDevices.options[i].text == microphone)
                    {
                        index = i;
                        break;
                    }
                }
                //Set active
                voiceChatDevices.value = index;
            }
            else
            {
                RedrawVoiceChatDevices();
            }
#endif
            //Controls
            controlsHipSensitivity.value = PlayerPrefs.GetFloat("optionsHipSensitivity", 1f);
            controlsAimSensitivity.value = PlayerPrefs.GetFloat("optionsAimSensitivity", 0.8f);
            controlsScopeSensitivity.value = PlayerPrefs.GetFloat("optionsScopeSensitivity", 0.2f);
            controlsAimingToggle.isOn = PlayerPrefsExtended.GetBool("optionsAimingToggle", true);
            controlsCrouchToggle.isOn = PlayerPrefsExtended.GetBool("optionsCrouchToggle", true);

            gameplayFieldOfView.value = PlayerPrefs.GetFloat("optionsFieldOfView", 60f);

            loadDone = true;
        }

        void Save()
        {
            if (loadDone)
            {
                PlayerPrefsExtended.SetBool("optionsFullscreen", Screen.fullScreen);
                PlayerPrefs.SetInt("optionsResolution", resolution.value);
                PlayerPrefs.SetInt("optionsAnisotropicFiltering", anisotropicFiltering.value);
                PlayerPrefs.SetInt("optionsTextureQuality", textureQuality.value);
                PlayerPrefsExtended.SetBool("optionsVerticalSync", vSync.isOn);
                PlayerPrefs.SetInt("optionsShadows", shadows.value);
                PlayerPrefs.SetInt("optionsShadowsQuality", shadowQuality.value);
                PlayerPrefs.SetFloat("optionsShadowDistance", shadowDistance.value);
                PlayerPrefs.SetInt("optionsShadowProjectionMode", (int)QualitySettings.shadowProjection);
                PlayerPrefs.SetInt("optionsShadowmaskMode", (int)QualitySettings.shadowmaskMode);
                PlayerPrefs.SetInt("optionsShadowCascades", shadowCascades.value);
                PlayerPrefs.SetFloat("optionsLodBias", QualitySettings.lodBias);
                PlayerPrefsExtended.SetBool("optionsRealtimeReflectionProbes", QualitySettings.realtimeReflectionProbes);
                PlayerPrefsExtended.SetBool("optionsSoftParticles", QualitySettings.softParticles);
                PlayerPrefs.SetFloat("optionsVolume", masterVolume.value);
                PlayerPrefs.SetInt("optionsAudioMode", audioMode.value);
                PlayerPrefs.SetInt("optionsVoiceChatMode", voiceChatMode.value);
#if !UNITY_WEBGL && !DISABLE_VOICECHAT
                if (PhotonVoiceNetwork.Instance && PhotonVoiceNetwork.Instance.PrimaryRecorder)
                {
                    PlayerPrefs.SetString("optionsVoiceChatMicrophone", PhotonVoiceNetwork.Instance.PrimaryRecorder.UnityMicrophoneDevice);
                }
#endif
                PlayerPrefs.SetFloat("optionsHipSensitivity", Kit_GameSettings.hipSensitivity);
                PlayerPrefs.SetFloat("optionsAimSensitivity", Kit_GameSettings.aimSensitivity);
                PlayerPrefs.SetFloat("optionsScopeSensitivity", Kit_GameSettings.fullScreenAimSensitivity);
                PlayerPrefsExtended.SetBool("optionsAimingToggle", Kit_GameSettings.isAimingToggle);
                PlayerPrefsExtended.SetBool("optionsCrouchToggle", Kit_GameSettings.isCrouchToggle);
                PlayerPrefs.SetFloat("optionsFieldOfView", Kit_GameSettings.baseFov);
            }
        }
    }
}