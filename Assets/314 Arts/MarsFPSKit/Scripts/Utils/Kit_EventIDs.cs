using UnityEngine;

namespace MarsFPSKit
{
    public class Kit_EventIDs
    {
        public static byte killEvent
        {
            get
            {
                return 0;
            }
        }

        public static byte requestChatMessage
        {
            get
            {
                return 1;
            }
        }

        public static byte chatMessageReceived
        {
            get
            {
                return 2;
            }
        }

        public static byte resetRequest
        {
            get
            {
                return 3;
            }
        }

        public static byte startVote
        {
            get
            {
                return 4;
            }
        }

        public static byte playerJoinedTeam
        {
            get
            {
                return 5;
            }
        }

        public static byte spawnSceneObject
        {
            get
            {
                return 6;
            }
        }

        public static byte hitMarkerEvent
        {
            get
            {
                return 7;
            }
        }

        public static byte xpEvent
        {
            get
            {
                return 8;
            }
        }

        public static byte assistEvent
        {
            get
            {
                return 9;
            }
        }

        public static byte respawnEvent
        {
            get
            {
                return 10;
            }
        }
    }
}