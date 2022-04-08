using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit
{
    public class DefaultInputData
    {
        /// <summary>
        /// When did we check for enemies the last time?
        /// </summary>
        public float lastScan;

        public List<Kit_PlayerBehaviour> enemyPlayersAwareOff = new List<Kit_PlayerBehaviour>();
    }

    [CreateAssetMenu(menuName = "MarsFPSKit/Input Manager/Default")]
    /// <summary>
    /// This is the kit's default input manager
    /// </summary>
    public class Kit_DefaultInputManager : Kit_InputManagerBase
    {
        /// <summary>
        /// How many seconds apart are our scans?
        /// </summary>
        public float scanFrequency = 1f;

        public string[] weaponSlotKeys;

        [Header("Spotting")]
        public LayerMask spottingLayer;
        public LayerMask spottingCheckLayers;
        public float spottingMaxDistance = 50f;
        public Vector2 spottingBoxExtents = new Vector2(30, 30);
        private Vector3 spottingBoxSize;
        public float spottingFov = 90f;
        public float spottingRayDistance = 200f;

        public override void InitializeControls(Kit_PlayerBehaviour pb)
        {
            DefaultInputData did = new DefaultInputData();
            pb.inputManagerData = did;
            pb.input.weaponSlotUses = new bool[weaponSlotKeys.Length];
            did.enemyPlayersAwareOff = new List<Kit_PlayerBehaviour>();
            spottingBoxSize = new Vector3(spottingBoxExtents.x, spottingBoxExtents.y, spottingMaxDistance / 2f);
        }

        public override void WriteToPlayerInput(Kit_PlayerBehaviour pb)
        {
            if (pb.inputManagerData != null && pb.inputManagerData.GetType() == typeof(DefaultInputData))
            {
                DefaultInputData did = pb.inputManagerData as DefaultInputData;
                //Get all input
                pb.input.hor = Input.GetAxis("Horizontal");
                pb.input.ver = Input.GetAxis("Vertical");
                pb.input.crouch = Input.GetButton("Crouch");
                pb.input.sprint = Input.GetButton("Sprint");
                pb.input.jump = Input.GetKey(KeyCode.Space);
                pb.input.dropWeapon = Input.GetKey(KeyCode.F);
                
                pb.input.rmb = Input.GetKey(KeyCode.Mouse1);
                pb.input.reload = Input.GetKey(KeyCode.R);
                pb.input.mouseX = Input.GetAxisRaw("Mouse X");
                pb.input.mouseY = Input.GetAxisRaw("Mouse Y");
                pb.input.leanLeft = Input.GetButton("Lean Left");
                pb.input.leanRight = Input.GetButton("Lean Right");
                pb.input.thirdPerson = Input.GetButton("Change Perspective");
                pb.input.flashlight = Input.GetButton("Flashlight");
                pb.input.laser = Input.GetButton("Laser");

                if (MarsScreen.lockCursor)
                {
                    pb.input.lmb = Input.GetMouseButton(0);
                }
                else
                {
                    pb.input.lmb = false;
                }

                if (pb.input.weaponSlotUses == null || pb.input.weaponSlotUses.Length != weaponSlotKeys.Length) pb.input.weaponSlotUses = new bool[weaponSlotKeys.Length];

                for (int i = 0; i < weaponSlotKeys.Length; i++)
                {
                    int id = i;
                    pb.input.weaponSlotUses[id] = Input.GetButton(weaponSlotKeys[id]);
                }

                //Scan
                if (Time.time > did.lastScan)
                {
                    did.lastScan = Time.time + scanFrequency;
                    ScanForEnemies(pb, did);
                }
            }
        }

        void ScanForEnemies(Kit_PlayerBehaviour pb, DefaultInputData did)
        {
            Collider[] possiblePlayers = Physics.OverlapBox(pb.playerCameraTransform.position + pb.playerCameraTransform.forward * (spottingMaxDistance / 2), spottingBoxSize, pb.playerCameraTransform.rotation, spottingLayer.value);

            //Clean
            did.enemyPlayersAwareOff.RemoveAll(item => item == null);

            //Loop
            for (int i = 0; i < possiblePlayers.Length; i++)
            {
                //Check if it is a player
                Kit_PlayerBehaviour pnb = possiblePlayers[i].transform.root.GetComponent<Kit_PlayerBehaviour>();
                if (pnb && pnb != pb)
                {
                    if (CanSeePlayer(pb, did, pnb))
                    {
                        if (isEnemyPlayer(pb, did, pnb))
                        {
                            if (!did.enemyPlayersAwareOff.Contains(pnb))
                            {
                                //Add to our known list
                                did.enemyPlayersAwareOff.Add(pnb);
                                //Call spotted
                                if (pb.voiceManager)
                                {
                                    pb.voiceManager.SpottedEnemy(pb, pnb);
                                }
                            }
                        }
                    }
                }
            }
        }

        bool CanSeePlayer(Kit_PlayerBehaviour pb, DefaultInputData did, Kit_PlayerBehaviour enemyPlayer)
        {
            if (enemyPlayer)
            {
                RaycastHit hit;
                Vector3 rayDirection = enemyPlayer.playerCameraTransform.position - new Vector3(0, 0.2f, 0f) - pb.playerCameraTransform.position;

                if ((Vector3.Angle(rayDirection, pb.playerCameraTransform.forward)) < spottingFov)
                {
                    if (Physics.Raycast(pb.playerCameraTransform.position, rayDirection, out hit, spottingRayDistance, spottingCheckLayers.value))
                    {
                        if (hit.collider.transform.root == enemyPlayer.transform.root)
                        {
                            return true;
                        }
                        else
                        {

                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool isEnemyPlayer(Kit_PlayerBehaviour pb, DefaultInputData did, Kit_PlayerBehaviour enemyPlayer)
        {
            if (pb)
            {
                if (pb.main.currentPvPGameModeBehaviour)
                {
                    if (!pb.main.currentPvPGameModeBehaviour.isTeamGameMode) return true;
                    else
                    {
                        if (pb.myTeam != enemyPlayer.myTeam) return true;
                        else return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }
}