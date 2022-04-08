using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MarsFPSKit
{
    public class Kit_Domination_FlagRuntime : MonoBehaviour
    {
        /// <summary>
        /// Position for the UI
        /// </summary>
        public Transform uiPosition;

        /// <summary>
        /// This is the renderer of the flag, where ownership materials will be applied
        /// </summary>
        public Renderer flagRenderer;

        /// <summary>
        /// Cloth object of flag
        /// </summary>
        public Cloth flagCloth;

        /// <summary>
        /// Minimap
        /// </summary>
        public SpriteRenderer minimapIcon;

        #region Runtime
        public List<Kit_PlayerBehaviour> playersInTrigger = new List<Kit_PlayerBehaviour>();

        /// <summary>
        /// Main
        /// </summary>
        public Kit_IngameMain main;
        #endregion

        public void Setup(Kit_Domination_Flag flag)
        {
            //Copy acceleration
            flagCloth.externalAcceleration = flag.externalAcceleration;
            flagCloth.randomAcceleration = flag.randomAcceleration;
        }

        /// <summary>
        /// The owner of this flag changed
        /// </summary>
        /// <param name="newOwner"></param>
        public void UpdateFlag(int owner, Kit_PvP_GMB_Domination gameMode)
        {
            //Change material accordingly
            if (owner == 0)
            {
                flagRenderer.sharedMaterial = gameMode.flagMaterialNeutral;
            }
            else
            {
                flagRenderer.sharedMaterial = gameMode.flagMaterialTeams[owner - 1];
            }
        }

        /// <summary>
        /// The master client switched
        /// </summary>
        /// <param name="player"></param>
        public void OnMasterClientSwitched(Photon.Realtime.Player player)
        {

        }

        #region Unity Calls
        void Start()
        {
            //Find main
            main = FindObjectOfType<Kit_IngameMain>();
        }

        void OnTriggerEnter(Collider other)
        {
            //First clean list
            playersInTrigger = playersInTrigger.Where(item => item != null).ToList();

            Kit_PlayerBehaviour pb = other.transform.root.GetComponent<Kit_PlayerBehaviour>();
            if (pb)
            {
                if (!playersInTrigger.Contains(pb)) playersInTrigger.Add(pb);
                //Tell game mode something changed
                (main.currentPvPGameModeBehaviour as Kit_PvP_GMB_Domination).FlagStateChanged(main, this);
            }
        }

        public void PlayerDied()
        {
            //First clean list
            playersInTrigger = playersInTrigger.Where(item => item != null).ToList();
            //Tell game mode something changed
            (main.currentPvPGameModeBehaviour as Kit_PvP_GMB_Domination).FlagStateChanged(main, this);
        }

        void OnTriggerExit(Collider other)
        {
            //First clean list
            playersInTrigger = playersInTrigger.Where(item => item != null).ToList();

            Kit_PlayerBehaviour pb = other.transform.root.GetComponent<Kit_PlayerBehaviour>();
            if (pb)
            {
                if (playersInTrigger.Contains(pb)) playersInTrigger.Remove(pb);
                //Tell game mode something changed
                (main.currentPvPGameModeBehaviour as Kit_PvP_GMB_Domination).FlagStateChanged(main, this);
            }
        }
        #endregion
    }
}
