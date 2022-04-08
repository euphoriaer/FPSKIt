using UnityEngine;
using System.Collections;

namespace MarsFPSKit
{

    /// <summary>
    /// This object contains a Region that is displayed to the user if assigned in <see cref="Kit_GameInformation"/>
    /// </summary>
    [CreateAssetMenu(menuName = "MarsFPSKit/Photon/Region")]
    public class Kit_RegionInformation : ScriptableObject
    {
        public string regionName; //Region name that is displayed to the user
        public string serverLocation; //Server location that is displayed to the user
        public string token; //The token to use for Photon
    }
}
