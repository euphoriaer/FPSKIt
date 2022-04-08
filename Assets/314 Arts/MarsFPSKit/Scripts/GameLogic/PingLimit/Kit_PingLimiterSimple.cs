using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    [CreateAssetMenu(menuName = "MarsFPSKit/Ping Limiter/Basic Ping Limiter")]
    /// <summary>
    /// Implements a basic ping limiter that will display warnings and disconnects if the warnings have been ignored or the user was unable to improve his ping.
    /// </summary>
    public class Kit_PingLimiterSimple : Kit_PingLimitBase
    {
        [Tooltip("How many times is the user warned before he/she is kicked?")]
        /// <summary>
        /// How many times is the user warned before he/she is kicked?
        /// </summary>
        public int amountOfWarnings = 3;

        [Tooltip("How many seconds apart is the ping checked?")]
        /// <summary>
        /// How many seconds apart is the ping checked?
        /// </summary>
        public float pingCheckInterval = 10f;

        //RUNTIME DATA
        /// <summary>
        /// How many times has the user been warned in a row?
        /// </summary>
        private int currentNumberOfWarnings = 0;

        /// <summary>
        /// When was the ping checked for the last time?
        /// </summary>
        private float lastPingCheck;

        /// <summary>
        /// What is the current ping limit?
        /// </summary>
        private int currentPingLimit;
        //END

        public override void StartRelay(Kit_IngameMain main, bool enabled, int pingLimit = 0)
        {
            //Reset runtime data
            lastPingCheck = Time.time;
            currentNumberOfWarnings = 0;
            currentPingLimit = pingLimit;
        }

        public override void UpdateRelay(Kit_IngameMain main)
        {
            //Check if we need to check the ping
            if (Time.time - pingCheckInterval > lastPingCheck)
            {
                //Check if our ping is too high
                if (PhotonNetwork.GetPing() >= currentPingLimit)
                {
                    currentNumberOfWarnings++;
                    if (currentNumberOfWarnings > amountOfWarnings)
                    {
                        //Disconnect
                        main.Disconnect();
                    }
                    else
                    {
                        //Display warning
                        if (main.pingLimitUI)
                        {
                            main.pingLimitUI.DisplayWarning(PhotonNetwork.GetPing(), currentNumberOfWarnings);
                        }
                    }
                }
                else
                {
                    currentNumberOfWarnings = 0;
                }
                //Check last ping check
                lastPingCheck = Time.time;
            }
        }
    }
}
