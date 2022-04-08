﻿using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    /// <summary>
    /// This is used for <see cref="Kit_PvP_GMB_Deathmatch"/>
    /// </summary>
    public class Kit_DeathmatchHUD : Kit_GameModeHUDBase
    {
        public Text timer;

        private int roundedRestSeconds;
        private int displaySeconds;
        private int displayMinutes;

        public override void HUDUpdate(Kit_IngameMain main)
        {
            if (main.currentPvPGameModeBehaviour.AreEnoughPlayersThere(main) || main.hasGameModeStarted)
            {
                roundedRestSeconds = Mathf.CeilToInt(main.timer);
                displaySeconds = roundedRestSeconds % 60; //Get seconds
                displayMinutes = roundedRestSeconds / 60; //Get minutes
                                                          //Update text
                timer.text = string.Format("{0:00} : {1:00}", displayMinutes, displaySeconds);
                timer.enabled = true;
            }
            else
            {
                timer.enabled = false;
            }
        }
    }
}
