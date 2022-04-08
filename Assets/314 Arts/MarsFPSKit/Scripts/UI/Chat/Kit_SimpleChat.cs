using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace MarsFPSKit
{
    public class Kit_SimpleChat : Kit_ChatBase
    {
        public Kit_IngameMain main;

        //Runtime info
        private int messageType = 0;
        private bool isChatOpen;
        private float lastMessageReceived = -100f;
        private float lastChatOpen = -100;
        private List<GameObject> activeChatEntries = new List<GameObject>();
        //End

        /// <summary>
        /// To hide the chat
        /// </summary>
        public CanvasGroup chatAlpha;

        /// <summary>
        /// Because scroll view is seperate from other parts of the chat, we use this
        /// </summary>
        public CanvasGroup chatScrollAlpha;

        /// <summary>
        /// To hide the messages
        /// </summary>
        public CanvasGroup messageAlpha;

        /// <summary>
        /// The placeholder that is displayed while there is nothing in chatInput
        /// </summary>
        public TextMeshProUGUI chatPlaceholder;
        public TMP_InputField chatInput;

        /// <summary>
        /// Prefab for a new chat entry
        /// </summary>
        public GameObject chatEntryPrefab;
        /// <summary>
        /// Where new chat entries are placed
        /// </summary>
        public RectTransform chatEntryGo;

        /// <summary>
        /// Used to scroll through the chat histroy
        /// </summary>
        public ScrollRect chatScroll;

        /// <summary>
        /// How long is the chat (background, input field) going to be visible after it has been used for the last time?
        /// </summary>
        public float fadeOutTimeNormal = 4f;
        /// <summary>
        /// How long are messages going to be visible after a message has been displayed for the last time?
        /// </summary>
        public float fadeOutTimeMessages = 8f;

        /// <summary>
        /// The highest amount of chat entries that can be active at the same time
        /// </summary>
        public int maxNumberOfActiveChatEntries = 32;

        /// <summary>
        /// Server prefix color
        /// </summary>
        public Color serverMessageColor;

        /// <summary>
        /// Color for team 1
        /// </summary>
        public Color teamOneColor = Color.blue;

        /// <summary>
        /// Color for team 2
        /// </summary>
        public Color teamTwoColor = Color.red;

        /// <summary>
        /// Color for team only messages
        /// </summary>
        public Color teamOnlyColor = Color.yellow;

        public override void DisplayChatMessage(Photon.Realtime.Player sender, string message, int type)
        {
            //Set time
            lastMessageReceived = Time.time;
            //Check if we exceeded number of active entries
            if (maxNumberOfActiveChatEntries > 0 && activeChatEntries.Count > maxNumberOfActiveChatEntries)
            {
                //Cache
                GameObject go = activeChatEntries[0];
                //Remove from list
                activeChatEntries.RemoveAt(0);
                //Destroy the game object
                Destroy(go);
            }

            //Instantiate new go
            GameObject newEntry = Instantiate(chatEntryPrefab, chatEntryGo, false);
            //Reset scale
            newEntry.transform.localScale = Vector3.one;
            //Determine Color
            Color finalCol = Color.white;
            //Color on team basis, if we are playing a team game mode
            if (main.currentPvPGameModeBehaviour && main.currentPvPGameModeBehaviour.isTeamGameMode)
            {
                //Set if the player has a team set
                if (sender.CustomProperties["team"] != null)
                {
                    //Team only message
                    if (type == 1)
                    {
                        finalCol = teamOnlyColor;
                    }
                    else
                    {
                        //Get team
                        int team = (int)sender.CustomProperties["team"];
                        if (team == 0)
                        {
                            //Set color
                            finalCol = teamOneColor;
                        }
                        else if (team == 1)
                        {
                            //Set color
                            finalCol = teamTwoColor;
                        }
                    }

                }
            }
            //Setup
            newEntry.GetComponent<Kit_SimpleChatEntry>().Setup("<color=#" + ColorUtility.ToHtmlStringRGB(finalCol) + ">" + sender.NickName + "</color>: " + message);
            //Add to list
            activeChatEntries.Add(newEntry);
            //Refresh entries
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatEntryGo); //Force layout update
            chatScroll.verticalScrollbar.value = 0f;
            Canvas.ForceUpdateCanvases();
        }

        public override void MasterClientSwitched(Photon.Realtime.Player player)
        {
            //Set time
            lastMessageReceived = Time.time;
            //Check if we exceeded number of active entries
            if (maxNumberOfActiveChatEntries > 0 && activeChatEntries.Count > maxNumberOfActiveChatEntries)
            {
                //Cache
                GameObject go = activeChatEntries[0];
                //Remove from list
                activeChatEntries.RemoveAt(0);
                //Destroy the game object
                Destroy(go);
            }

            //Instantiate new go
            GameObject newEntry = Instantiate(chatEntryPrefab, chatEntryGo, false);
            //Reset scale
            newEntry.transform.localScale = Vector3.one;
            //Setup
            newEntry.GetComponent<Kit_SimpleChatEntry>().Setup("<color=#" + ColorUtility.ToHtmlStringRGB(serverMessageColor) + ">Server: </color>" + player.NickName + " is the new master client");
            //Add to list
            activeChatEntries.Add(newEntry);
            //Refresh entries
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatEntryGo); //Force layout update
            chatScroll.verticalScrollbar.value = 0f;
            Canvas.ForceUpdateCanvases();
        }

        public override void PlayerJoined(Photon.Realtime.Player player)
        {
            //Set time
            lastMessageReceived = Time.time;
            //Check if we exceeded number of active entries
            if (maxNumberOfActiveChatEntries > 0 && activeChatEntries.Count > maxNumberOfActiveChatEntries)
            {
                //Cache
                GameObject go = activeChatEntries[0];
                //Remove from list
                activeChatEntries.RemoveAt(0);
                //Destroy the game object
                Destroy(go);
            }

            //Instantiate new go
            GameObject newEntry = Instantiate(chatEntryPrefab, chatEntryGo, false);
            //Reset scale
            newEntry.transform.localScale = Vector3.one;
            //Setup
            newEntry.GetComponent<Kit_SimpleChatEntry>().Setup("<color=#" + ColorUtility.ToHtmlStringRGB(serverMessageColor) + ">Server: </color>" + player.NickName + " joined");
            //Add to list
            activeChatEntries.Add(newEntry);
            //Refresh entries
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatEntryGo); //Force layout update
            chatScroll.verticalScrollbar.value = 0f;
            Canvas.ForceUpdateCanvases();
        }

        public override void PlayerLeft(Photon.Realtime.Player player)
        {
            //Set time
            lastMessageReceived = Time.time;
            //Check if we exceeded number of active entries
            if (maxNumberOfActiveChatEntries > 0 && activeChatEntries.Count > maxNumberOfActiveChatEntries)
            {
                //Cache
                GameObject go = activeChatEntries[0];
                //Remove from list
                activeChatEntries.RemoveAt(0);
                //Destroy the game object
                Destroy(go);
            }

            //Instantiate new go
            GameObject newEntry = Instantiate(chatEntryPrefab, chatEntryGo, false);
            //Reset scale
            newEntry.transform.localScale = Vector3.one;
            //Setup
            newEntry.GetComponent<Kit_SimpleChatEntry>().Setup("<color=#" + ColorUtility.ToHtmlStringRGB(serverMessageColor) + ">Server: </color>" + player.NickName + " left");
            //Add to list
            activeChatEntries.Add(newEntry);
            //Refresh entries
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatEntryGo); //Force layout update
            chatScroll.verticalScrollbar.value = 0f;
            Canvas.ForceUpdateCanvases();
        }

        public override void BotJoined(string botName)
        {
            //Set time
            lastMessageReceived = Time.time;
            //Check if we exceeded number of active entries
            if (maxNumberOfActiveChatEntries > 0 && activeChatEntries.Count > maxNumberOfActiveChatEntries)
            {
                //Cache
                GameObject go = activeChatEntries[0];
                //Remove from list
                activeChatEntries.RemoveAt(0);
                //Destroy the game object
                Destroy(go);
            }

            //Instantiate new go
            GameObject newEntry = Instantiate(chatEntryPrefab, chatEntryGo, false);
            //Reset scale
            newEntry.transform.localScale = Vector3.one;
            //Setup
            newEntry.GetComponent<Kit_SimpleChatEntry>().Setup("<color=#" + ColorUtility.ToHtmlStringRGB(serverMessageColor) + ">Server: </color>" + botName + " joined");
            //Add to list
            activeChatEntries.Add(newEntry);
            //Refresh entries
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatEntryGo); //Force layout update
            chatScroll.verticalScrollbar.value = 0f;
            Canvas.ForceUpdateCanvases();
        }

        public override void BotLeft(string botName)
        {
            //Set time
            lastMessageReceived = Time.time;
            //Check if we exceeded number of active entries
            if (maxNumberOfActiveChatEntries > 0 && activeChatEntries.Count > maxNumberOfActiveChatEntries)
            {
                //Cache
                GameObject go = activeChatEntries[0];
                //Remove from list
                activeChatEntries.RemoveAt(0);
                //Destroy the game object
                Destroy(go);
            }

            //Instantiate new go
            GameObject newEntry = Instantiate(chatEntryPrefab, chatEntryGo, false);
            //Reset scale
            newEntry.transform.localScale = Vector3.one;
            //Setup
            newEntry.GetComponent<Kit_SimpleChatEntry>().Setup("<color=#" + ColorUtility.ToHtmlStringRGB(serverMessageColor) + ">Server: </color>" + botName + " left");
            //Add to list
            activeChatEntries.Add(newEntry);
            //Refresh entries
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatEntryGo); //Force layout update
            chatScroll.verticalScrollbar.value = 0f;
            Canvas.ForceUpdateCanvases();
        }

        /// <summary>
        /// Call this if you press the button
        /// </summary>
        public void SendMessageButton()
        {
            //Check if we can send
            if (!chatInput.text.IsNullOrWhiteSpace())
            {
                //Send input
                SendChatMessage(chatInput.text, messageType);
                //Reset
                chatInput.text = "";
            }
            lastChatOpen = Time.time; //Set time
            lastMessageReceived = Time.time; //Also set this time
            //Close chat
            isChatOpen = false;
            RedrawChat(); //Redraw
        }

        public override void PauseMenuOpened()
        {
            //If chat is open
            if (isChatOpen)
            {
                //Set times
                lastChatOpen = Time.time;
                lastMessageReceived = Time.time;
            }

            //Make sure chat is closed
            isChatOpen = false;
            RedrawChat();
        }

        public override void PauseMenuClosed()
        {
            //If chat is open
            if (isChatOpen)
            {
                //Set times
                lastChatOpen = Time.time;
                lastMessageReceived = Time.time;
            }

            //Make sure chat is closed
            isChatOpen = false;
            RedrawChat();
        }

        void RedrawChat()
        {
            if (isChatOpen)
            {
                //If chat is open, select it
                EventSystem.current.SetSelectedGameObject(chatInput.gameObject, null);
                chatInput.OnPointerClick(new PointerEventData(EventSystem.current));
                //And unlock cursor
                MarsScreen.lockCursor = false;
            }
            else
            {
                if (main.myPlayer && !Kit_IngameMain.isPauseMenuOpen)
                {
                    //If we have a player and the pause menu is not open, lock the cursor again
                    MarsScreen.lockCursor = true;
                }
                else
                {
                    MarsScreen.lockCursor = false;
                }
            }
        }

        #region Unity Calls
        void Update()
        {
            #region Input
            //Only check for chat input if the pause menu isnt open
            if (!Kit_IngameMain.isPauseMenuOpen)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (!isChatOpen)
                    {
                        messageType = 0; //Message for everyone
                        chatPlaceholder.text = "Say to all"; //Display correct placeholder
                        chatInput.text = ""; //Make sure text is empty
                        isChatOpen = true; //Open
                        RedrawChat(); //Redraw
                        //Auto Spawn System
                        if (main.autoSpawnSystem && main.currentPvPGameModeBehaviour)
                        {
                            main.autoSpawnSystem.Interruption();
                        }
                    }
                    else
                    {
                        //Check if we can send
                        if (!chatInput.text.IsNullOrWhiteSpace())
                        {
                            //Send input
                            SendChatMessage(chatInput.text, messageType);
                            //Reset
                            chatInput.text = "";
                        }
                        lastChatOpen = Time.time; //Set time
                        lastMessageReceived = Time.time; //Also set this time
                        //Close chat
                        isChatOpen = false;
                        EventSystem.current.SetSelectedGameObject(null);
                        RedrawChat(); //Redraw
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Y))
                {
                    if (!isChatOpen)
                    {
                        messageType = 1; //Message for team only
                        //Check if we should display "team only"
                        if (main.currentPvPGameModeBehaviour && main.currentPvPGameModeBehaviour.isTeamGameMode)
                        {
                            chatPlaceholder.text = "Say to team"; //Display correct placeholder
                        }
                        else
                        {
                            chatPlaceholder.text = "Say to all"; //Display correct placeholder
                        }
                        chatInput.text = ""; //Make sure text is empty
                        isChatOpen = true;
                        RedrawChat(); //Redraw
                                      //Auto Spawn System
                        if (main.autoSpawnSystem && main.currentPvPGameModeBehaviour)
                        {
                            main.autoSpawnSystem.Interruption();
                        }
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (isChatOpen)
                    {
                        lastChatOpen = Time.time; //Set time
                        lastMessageReceived = Time.time; //Also set this time
                        isChatOpen = false; //Close
                        RedrawChat(); //Redraw
                    }
                }
            }
            #endregion

            #region UI
            if (isChatOpen)
            {
                chatAlpha.alpha = 1f;
                messageAlpha.alpha = 1f;
                chatScrollAlpha.alpha = 1f;
            }
            else
            {
                //Set alpha according to times
                chatAlpha.alpha = Mathf.Clamp01((lastChatOpen + fadeOutTimeNormal) - Time.time);
                messageAlpha.alpha = Mathf.Clamp01((lastMessageReceived + fadeOutTimeMessages) - Time.time);
                chatScrollAlpha.alpha = chatAlpha.alpha; //Just copy
            }
            #endregion
        }
        #endregion
    }
}