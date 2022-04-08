using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MarsFPSKit
{
    /// <summary>
    /// This custom scene manager makes sure that scenes are properly synced. In contrast to the default manager from Photon, reloading a scene is possible.
    /// </summary>
    public class Kit_SceneSyncer : MonoBehaviour, IOnEventCallback
    {
        public static Kit_SceneSyncer instance;
        /// <summary>
        /// Game Information
        /// </summary>
        public Kit_GameInformation information;
        /// <summary>
        /// The canvas that displays the loading screen
        /// </summary>
        public GameObject loadingCanvas;
        /// <summary>
        /// The image that displays the progress
        /// </summary>
        public Image loadingBar;
        /// <summary>
        /// Background image of map
        /// </summary>
        public Image backgroundImage;
        [HideInInspector]
        /// <summary>
        /// Is loading in progress?
        /// </summary>
        public bool isLoading;
        /// <summary>
        /// Scene we are currently loading
        /// </summary>
        private string sceneToLoad;

        [Header("Fade")]
        public Animator fadeAnim;
        /// <summary>
        /// How long does it take to fade in?
        /// </summary>
        public float fadeInLength = 0.25f;
        /// <summary>
        /// How long does it take to fade out?
        /// </summary>
        public float fadeOutLength = 0.25f;

        void Awake()
        {
            if (!instance)
            {
                //Setup callbacks
                SceneManager.activeSceneChanged += ActiveSceneChanged;
                //Assign instance
                instance = this;
                //Disable default manager
                PhotonNetwork.AutomaticallySyncScene = false;
                //Enable message queue by default so we can connect to photon
                PhotonNetwork.IsMessageQueueRunning = true;
                //Make sure it doens't get destroyed
                DontDestroyOnLoad(this);
                //Hide canvas
                loadingCanvas.SetActive(false);
            }
            else
            {
                //Only one of this instance may be active at a time
                Destroy(gameObject);
            }
        }

        void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void ActiveSceneChanged(Scene arg0, Scene arg1)
        {
            //We finished loading. Let Photon handle the things again
            PhotonNetwork.IsMessageQueueRunning = true;
            Debug.Log("[Scene Sync] Activating network queue!");
        }

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;
            object content = photonEvent.CustomData;
            int senderId = photonEvent.Sender;
            //Last code is reserved for the scene sync
            if (eventCode == (byte)199)
            {
                Kit_IngameMain main = FindObjectOfType<Kit_IngameMain>();
                if (main)
                {
                    main.ResetStats();
                }

                PhotonNetwork.IsMessageQueueRunning = false;
                Debug.Log("[Scene Sync] Deactivating network queue!");
                string scene = (string)content;
                if (scene != sceneToLoad)
                {
                    StartCoroutine(LoadSceneAsync(scene));
                }
            }
        }

        IEnumerator LoadSceneAsync(string scene)
        {
            sceneToLoad = scene;
            isLoading = true;
            Kit_MapInformation mapInfo = information.GetMapInformationFromSceneName(scene);
            if (mapInfo && mapInfo.loadingImage)
            {
                backgroundImage.sprite = mapInfo.loadingImage;
                backgroundImage.enabled = true;
            }
            else
            {
                backgroundImage.enabled = false;
            }
            //Reset progress
            loadingBar.fillAmount = 0f;
            //Show canvas
            loadingCanvas.SetActive(true);
            fadeAnim.Play("Fade In", 0, 0f);
            //Wait for anim
            yield return new WaitForSeconds(fadeInLength);
            AsyncOperation loading = SceneManager.LoadSceneAsync(scene);
            while (!loading.isDone)
            {
                loadingBar.fillAmount = loading.progress;
                yield return null;
            }
            fadeAnim.Play("Fade Out", 0, 0f);
            //Wait for anim
            yield return new WaitForSeconds(fadeOutLength);
            //Hide canvas again
            loadingCanvas.SetActive(false);
            //Unset bool
            isLoading = false;
            sceneToLoad = "";
        }

        /// <summary>
        /// Network loads a scene
        /// </summary>
        /// <param name="scene"></param>
        public void LoadScene(string scene)
        {
            if (!PhotonNetwork.IsConnected)
            {
                StartCoroutine(LoadSceneAsync(scene));
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                //First clean up!
                RaiseEventOptions options = new RaiseEventOptions();
                options.CachingOption = EventCaching.RemoveFromRoomCache;
                options.Receivers = ReceiverGroup.All;
                PhotonNetwork.RaiseEvent(199, scene, options, SendOptions.SendReliable);
                Debug.Log("[Scene Sync] Cleaning up room cache!");
                //Send event to load the new scene
                RaiseEventOptions optionsNew = new RaiseEventOptions();
                //Make sure it is in the GLOBAL cache so it will be there when the master client leaves
                optionsNew.CachingOption = EventCaching.AddToRoomCacheGlobal;
                optionsNew.Receivers = ReceiverGroup.All;
                PhotonNetwork.RaiseEvent(199, scene, optionsNew, SendOptions.SendReliable);
                Debug.Log("[Scene Sync] Sending scene load event!");
            }
        }
    }
}
