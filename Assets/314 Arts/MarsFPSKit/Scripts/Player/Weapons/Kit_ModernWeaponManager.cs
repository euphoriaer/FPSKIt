using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        /// <summary>
        /// Input that is allowed for the given weapon slot
        /// </summary>
        [System.Serializable]
        public class WeaponManagerSlot
        {
            /// <summary>
            /// Can weapons in this slot be equipped?
            /// </summary>
            public bool enableEquipping = true;
            /// <summary>
            /// ID of <see cref="Kit_PlayerInput.weaponSlotUses"/>
            /// </summary>
            public int equippingInputID;
            /// <summary>
            /// Can weapons in this slot be quick used (e.g. quick grenades, quick knife)
            /// </summary>
            public bool enableQuickUse;
            /// <summary>
            /// ID of <see cref="Kit_PlayerInput.weaponSlotUses"/>
            /// </summary>
            public int quickUseInputID;
            /// <summary>
            /// When <see cref="maxAmountOfWeaponsInSlot"/> is > 1 and <see cref="enableQuickUse"/> is set to true, this key can be used to iterate through them
            /// </summary>
            public int quickUseIterationKey;
        }

        /// <summary>
        /// This will store runtime data for the controlling player
        /// </summary>
        public class WeaponManagerControllerRuntimeData
        {
            /// <summary>
            /// Our currently selected weapon; [0] = slot; [1] = weapon In Slot
            /// </summary>
            public int[] currentWeapon = new int[2];

            /// <summary>
            /// The weapon we want to select
            /// </summary>
            public int[] desiredWeapon = new int[2];

            /// <summary>
            /// Desired weapon is locked (by plugin?)
            /// </summary>
            public bool isDesiredWeaponLocked;

            /// <summary>
            /// Is a quick use in progress?
            /// </summary>
            public bool quickUseInProgress;
            /// <summary>
            /// Quick use that we want to do!
            /// </summary>
            public int[] desiredQuickUse = new int[2];
            /// <summary>
            /// Current state of quick use.
            /// </summary>
            public int quickUseState;
            /// <summary>
            /// When is the next quick use state over?
            /// </summary>
            public float quickUseOverAt;
            /// <summary>
            /// Sync!
            /// </summary>
            public bool quickUseSyncButtonWaitOver;

            /// <summary>
            /// The data of our two weapons that are in use. None of these should ever be null.
            /// </summary>
            public WeaponSlotReference[] weaponsInUse = new WeaponSlotReference[2];

            /// <summary>
            /// Last states for the slot buttons!
            /// </summary>
            public bool[] lastInputIDs;
            /// <summary>
            /// Last state for the drop weapon
            /// </summary>
            public bool lastDropWeapon;

            /// <summary>
            /// Are we currently switching weapons?
            /// </summary>
            public bool switchInProgress;
            /// <summary>
            /// When is the next switching phase over?
            /// </summary>
            public float switchNextEnd; //This is only so we don't have to use a coroutine
            /// <summary>
            /// The current phase of switching
            /// </summary>
            public int switchPhase;
            /// <summary>
            /// Raycast hit for the pickup process
            /// </summary>
            public RaycastHit hit;

            /// <summary>
            /// To fire the interaction end trigger
            /// We store the object we are currently interacting with
            /// </summary>
            public Kit_InteractableObject holdingInteractableObject;

            #region IK
            /// <summary>
            /// Weight of the left hand IK
            /// </summary>
            public float leftHandIKWeight;
            #endregion
        }

        /// <summary>
        /// This contains the reference to a generic weapon.
        /// </summary>
        public class WeaponSlotReference
        {
            public int selectedSlot;
            public int selectedQuickUse;
            public WeaponReference[] weaponsInSlot;
            /// <summary>
            /// If this is true, those were injected from a plugin and cannot be manually selected.
            /// </summary>
            public bool isInjectedFromPlugin;
        }

        public class WeaponReference
        {
            /// <summary>
            /// Assigned weapon behaviour
            /// </summary>
            public Kit_WeaponBase behaviour;
            /// <summary>
            /// Runtime data of this weapon
            /// </summary>
            public object runtimeData;
            /// <summary>
            /// Attachments for this weapon
            /// </summary>
            public int[] attachments;
            /// <summary>
            /// ID of this weapon
            /// </summary>
            public int id;
        }

        public enum DeadDrop { None, Selected, All }

        /// <summary>
        /// This is a modern, generic weapon manager. Weapons are put away and then taken out, like in COD or Battlefield. Supports an "infinite" amount of weapons
        /// </summary>
        [CreateAssetMenu(menuName = ("MarsFPSKit/Weapons/Modern Weapon Manager"))]
        public class Kit_ModernWeaponManager : Kit_WeaponManagerBase
        {
            public WeaponManagerSlot[] slotConfiguration;
            /// <summary>
            /// Main drop prefab!
            /// </summary>
            public GameObject dropPrefab;
            /// <summary>
            /// Layers that will be hit by the pickup raycast
            /// </summary>
            public LayerMask pickupLayers;
            /// <summary>
            /// Distance for the pickup raycast
            /// </summary>
            public float pickupDistance = 3f;
            /// <summary>
            /// Which weapons should be dropped upon death?
            /// </summary>
            public DeadDrop uponDeathDrop;
            /// <summary>
            /// How fast does the weapon position change?
            /// </summary>
            public float weaponPositionChangeSpeed = 5f;
            /// <summary>
            /// Can we switch while we are running?
            /// </summary>
            public bool allowSwitchingWhileRunning;
            /// <summary>
            /// Can we do quick use while running?
            /// </summary>
            public bool allowQuickUseWhileRunning;

            public override void SetupManager(Kit_PlayerBehaviour pb, object[] instantiationData)
            {
                //Setup runtime data
                WeaponManagerControllerRuntimeData runtimeData = new WeaponManagerControllerRuntimeData();
                pb.customWeaponManagerData = runtimeData; //Assign

                if (pb.isFirstPersonActive)
                {
                    //Hide crosshair
                    pb.main.hud.DisplayCrosshair(0f, false);
                }

                if (pb.isController || pb.isBot)
                {
                    //Setup input IDs
                    runtimeData.lastInputIDs = new bool[pb.input.weaponSlotUses.Length];
                }

                int amountOfWeapons = (int)instantiationData[1];
                Debug.Log("[Weapon Manager] Setup Begins With " + amountOfWeapons + " Weapons");

                //Determine how many slots are going to be used!
                int highestSlot = 0;

                for (int i = 0; i < amountOfWeapons; i++)
                {
                    Hashtable table = (Hashtable)instantiationData[2 + i];
                    if ((int)table["slot"] > highestSlot)
                    {
                        highestSlot = (int)table["slot"];
                    }
                }

                //Increase by one (is length, so highest slot is Length - 1)
                highestSlot++;

                //PLUGIN INJECTION
                //KEY = WEAPON; VALUE = ID OF PLUGIN
                List<WeaponsFromPlugin> weaponsFromPlugins = new List<WeaponsFromPlugin>();
                List<Kit_WeaponInjection> pluginWeaponsCallback = new List<Kit_WeaponInjection>();

                for (int i = 0; i < pb.main.gameInformation.plugins.Length; i++)
                {
                    WeaponsFromPlugin weapons = pb.main.gameInformation.plugins[i].WeaponsToInjectIntoWeaponManager(pb);
                    if (weapons.weaponsInSlot.Length > 0)
                    {
                        int id = i;
                        pluginWeaponsCallback.Add(pb.main.gameInformation.plugins[id]);
                        weaponsFromPlugins.Add(weapons);
                    }
                }

                WeaponsFromPlugin movementWeapons = pb.movement.WeaponsToInjectIntoWeaponManager(pb);
                if (movementWeapons != null && movementWeapons.weaponsInSlot.Length > 0)
                {
                    pluginWeaponsCallback.Add(pb.movement);
                    weaponsFromPlugins.Add(movementWeapons);
                }

                if (pb.main.currentPvPGameModeBehaviour)
                {
                    WeaponsFromPlugin gameModeWeapons = pb.main.currentPvPGameModeBehaviour.WeaponsToInjectIntoWeaponManager(pb);
                    if (gameModeWeapons != null && gameModeWeapons.weaponsInSlot.Length > 0)
                    {
                        pluginWeaponsCallback.Add(pb.main.currentPvPGameModeBehaviour);
                        weaponsFromPlugins.Add(gameModeWeapons);
                    }
                }

                if (pb.main.currentPvEGameModeBehaviour)
                {
                    WeaponsFromPlugin gameModeWeapons = pb.main.currentPvEGameModeBehaviour.WeaponsToInjectIntoWeaponManager(pb);
                    if (gameModeWeapons != null && gameModeWeapons.weaponsInSlot.Length > 0)
                    {
                        pluginWeaponsCallback.Add(pb.main.currentPvEGameModeBehaviour);
                        weaponsFromPlugins.Add(gameModeWeapons);
                    }
                }

                //Setup Slot Length!
                runtimeData.weaponsInUse = new WeaponSlotReference[highestSlot + weaponsFromPlugins.Count];
                //PLUGIN INJECTION END

                //Setup
                for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                {
                    runtimeData.weaponsInUse[i] = new WeaponSlotReference();
                }

                //Now determine how many weapons go in each slot
                int[] weaponsInEachSlot = new int[highestSlot];

                for (int i = 0; i < amountOfWeapons; i++)
                {
                    Hashtable table = (Hashtable)instantiationData[2 + i];
                    int slot = (int)table["slot"];
                    //Add!
                    weaponsInEachSlot[slot]++;
                }

                //Setup length of slots!
                for (int i = 0; i < highestSlot; i++)
                {
                    runtimeData.weaponsInUse[i].weaponsInSlot = new WeaponReference[weaponsInEachSlot[i]];
                }

                //Now, setup weapons!

                int[] slotsUsed = new int[highestSlot];

                for (int i = 0; i < amountOfWeapons; i++)
                {
                    Hashtable table = (Hashtable)instantiationData[2 + i];
                    int slot = (int)table["slot"];
                    int id = (int)table["id"];
                    int[] attachments = (int[])table["attachments"];

                    //Get their behaviour modules
                    Kit_WeaponBase weaponBehaviour = pb.gameInformation.allWeapons[id];
                    //Setup values
                    weaponBehaviour.SetupValues(id);
                    //Setup Reference
                    runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]] = new WeaponReference();
                    //Assign Behaviour
                    runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].behaviour = weaponBehaviour;
                    //Assign id
                    runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].id = id;
                    //Setup FP
                    runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].runtimeData = weaponBehaviour.SetupFirstPerson(pb, attachments);
                    //Assign attachments
                    runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].attachments = attachments;
                    //Setup TP
                    weaponBehaviour.SetupThirdPerson(pb, runtimeData.weaponsInUse[slot].weaponsInSlot[slotsUsed[slot]].runtimeData, attachments);
                    //Increase Slot!
                    slotsUsed[slot]++;
                }

                //PLUGIN INJECTION
                for (int i = 0; i < weaponsFromPlugins.Count; i++)
                {
                    int slot = highestSlot + i;
                    runtimeData.weaponsInUse[slot].isInjectedFromPlugin = true;

                    runtimeData.weaponsInUse[slot].weaponsInSlot = new WeaponReference[weaponsFromPlugins[i].weaponsInSlot.Length];

                    for (int p = 0; p < weaponsFromPlugins[i].weaponsInSlot.Length; p++)
                    {
                        Kit_WeaponBase weaponBehaviour = weaponsFromPlugins[i].weaponsInSlot[p].weapon;
                        runtimeData.weaponsInUse[slot].weaponsInSlot[p] = new WeaponReference();
                        //Assign Behaviour
                        runtimeData.weaponsInUse[slot].weaponsInSlot[p].behaviour = weaponBehaviour;
                        //These weapons have no id
                        runtimeData.weaponsInUse[slot].weaponsInSlot[p].id = -1;
                        //Setup FP
                        runtimeData.weaponsInUse[slot].weaponsInSlot[p].runtimeData = weaponBehaviour.SetupFirstPerson(pb, weaponsFromPlugins[i].weaponsInSlot[p].attachments);
                        //Assign attachments
                        runtimeData.weaponsInUse[slot].weaponsInSlot[p].attachments = weaponsFromPlugins[i].weaponsInSlot[p].attachments;
                        //Setup TP
                        weaponBehaviour.SetupThirdPerson(pb, runtimeData.weaponsInUse[slot].weaponsInSlot[p].runtimeData, weaponsFromPlugins[i].weaponsInSlot[p].attachments);
                    }

                    //Call plugin
                    pluginWeaponsCallback[i].ReportSlotOfInjectedWeapons(pb, slot);
                }
                //END
                SelectDefaultWeapon(pb, runtimeData);

                //Set time
                runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.drawTime;
                //Set phase
                runtimeData.switchPhase = 1;
                //Set switching
                runtimeData.switchInProgress = true;
            }

            /// <summary>
            /// Selects the first weapon
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="runtimeData"></param>
            void SelectDefaultWeapon(Kit_PlayerBehaviour pb, WeaponManagerControllerRuntimeData runtimeData)
            {
                //Select default weapon
                for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                {
                    if (i < slotConfiguration.Length && slotConfiguration[i].enableEquipping)
                    {
                        for (int o = 0; o < runtimeData.weaponsInUse[i].weaponsInSlot.Length; o++)
                        {
                            if (runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour.CanBeSelected(pb, runtimeData.weaponsInUse[i].weaponsInSlot[o].runtimeData))
                            {
                                int id = i;
                                int od = o;
                                //Select current weapon
                                runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour.DrawWeapon(pb, runtimeData.weaponsInUse[i].weaponsInSlot[o].runtimeData);
                                //Play Third person animation
                                pb.thirdPersonPlayerModel.PlayWeaponChangeAnimation(runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour.thirdPersonAnimType, true, runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour.drawTime);
                                //Set current weapon
                                runtimeData.desiredWeapon[0] = runtimeData.currentWeapon[0] = id;
                                runtimeData.desiredWeapon[1] = runtimeData.currentWeapon[1] = od;
                                return;
                            }
                        }
                    }
                }
            }

            public override void ForceUnselectCurrentWeapon(Kit_PlayerBehaviour pb)
            {
                Debug.Log("[Weapon Manager] Forcing unselect of current weapon!");
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    bool foundWeapon = false;
                    //Try to find next weapon
                    int[] next = new int[2] { -1, -1 };
                    for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                    {
                        if (foundWeapon) break;

                        for (int o = 0; o < runtimeData.weaponsInUse[i].weaponsInSlot.Length; o++)
                        {
                            if (!runtimeData.weaponsInUse[i].isInjectedFromPlugin)
                            {
                                //Check if this one works!
                                if (runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour.CanBeSelected(pb, runtimeData.weaponsInUse[i].weaponsInSlot[o].runtimeData))
                                {
                                    int id = i;
                                    int idTwo = o;
                                    next[0] = id;
                                    next[1] = idTwo;
                                    //We found one
                                    foundWeapon = true;
                                    break;
                                }
                            }
                        }
                    }

                    //This should ALWAYS be true!
                    if (next[0] >= 0 && next[1] >= 0)
                    {
                        runtimeData.desiredWeapon[0] = next[0];
                        runtimeData.desiredWeapon[1] = next[1];
                        //Begin switch and skip putaway
                        runtimeData.switchInProgress = true;
                        //Set time (Because here we cannot use a coroutine)
                        runtimeData.switchNextEnd = 0f;
                        //Set phase
                        runtimeData.switchPhase = 0;
                        if (pb.isFirstPersonActive)
                        {
                            //Hide crosshair
                            pb.main.hud.DisplayCrosshair(0f, false);
                        }
                        //Set current one too!
                        runtimeData.currentWeapon[0] = next[0];
                        runtimeData.currentWeapon[1] = next[1];
                    }
                    else
                    {
                        Debug.LogError("Could not find next weapon! This is not allowed!");
                    }
                }
            }

            public override void CustomUpdate(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;

                    if (pb.enableInput)
                    {
                        if (!runtimeData.isDesiredWeaponLocked)
                        {
                            for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                            {
                                if (!runtimeData.weaponsInUse[i].isInjectedFromPlugin)
                                {
                                    if (slotConfiguration[i].enableEquipping && slotConfiguration[i].equippingInputID >= 0 && !runtimeData.quickUseInProgress)
                                    {
                                        if (runtimeData.lastInputIDs[slotConfiguration[i].equippingInputID] != pb.input.weaponSlotUses[slotConfiguration[i].equippingInputID])
                                        {
                                            runtimeData.lastInputIDs[slotConfiguration[i].equippingInputID] = pb.input.weaponSlotUses[slotConfiguration[i].equippingInputID];
                                            //Check for input
                                            if (pb.input.weaponSlotUses[slotConfiguration[i].equippingInputID] && (allowSwitchingWhileRunning || !pb.movement.IsRunning(pb)))
                                            {
                                                int id = i;
                                                if (runtimeData.desiredWeapon[0] != id)
                                                {
                                                    if (runtimeData.weaponsInUse[i].weaponsInSlot[0].behaviour.CanBeSelected(pb, runtimeData.weaponsInUse[i].weaponsInSlot[0].runtimeData))
                                                    {
                                                        runtimeData.desiredWeapon[0] = id;
                                                        runtimeData.desiredWeapon[1] = 0;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    int next = runtimeData.desiredWeapon[1] + 1;
                                                    if (next >= runtimeData.weaponsInUse[id].weaponsInSlot.Length)
                                                    {
                                                        next = 0;
                                                    }
                                                    if (runtimeData.weaponsInUse[i].weaponsInSlot[next].behaviour.CanBeSelected(pb, runtimeData.weaponsInUse[i].weaponsInSlot[next].runtimeData))
                                                    {
                                                        runtimeData.desiredWeapon[1] = next;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            //Check if we can do a quick use!
                            if (runtimeData.currentWeapon[0] == runtimeData.desiredWeapon[0] && runtimeData.currentWeapon[1] == runtimeData.currentWeapon[1] && !runtimeData.quickUseInProgress && !runtimeData.switchInProgress)
                            {
                                for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                                {
                                    if (!runtimeData.weaponsInUse[i].isInjectedFromPlugin)
                                    {
                                        int slot = i;
                                        if (slotConfiguration[slot].enableQuickUse)
                                        {
                                            if (slotConfiguration[slot].quickUseIterationKey >= 0)
                                            {
                                                if (runtimeData.lastInputIDs[slotConfiguration[slot].quickUseIterationKey] != pb.input.weaponSlotUses[slotConfiguration[slot].quickUseIterationKey])
                                                {
                                                    runtimeData.lastInputIDs[slotConfiguration[slot].quickUseIterationKey] = pb.input.weaponSlotUses[slotConfiguration[slot].quickUseIterationKey];

                                                    int id = i;
                                                    if (pb.input.weaponSlotUses[slotConfiguration[slot].quickUseIterationKey])
                                                    {
                                                        runtimeData.weaponsInUse[id].selectedQuickUse++;
                                                        if (runtimeData.weaponsInUse[id].selectedQuickUse >= runtimeData.weaponsInUse[id].weaponsInSlot.Length)
                                                        {
                                                            runtimeData.weaponsInUse[id].selectedQuickUse = 0;
                                                        }
                                                    }
                                                }
                                            }

                                            if (slotConfiguration[slot].quickUseInputID >= 0)
                                            {
                                                if (runtimeData.lastInputIDs[slotConfiguration[slot].quickUseInputID] != pb.input.weaponSlotUses[slotConfiguration[slot].quickUseInputID])
                                                {
                                                    runtimeData.lastInputIDs[slotConfiguration[slot].quickUseInputID] = pb.input.weaponSlotUses[slotConfiguration[slot].quickUseInputID];
                                                    //Check for input
                                                    if (pb.input.weaponSlotUses[slotConfiguration[slot].quickUseInputID] && (allowQuickUseWhileRunning || !pb.movement.IsRunning(pb)))
                                                    {
                                                        if (runtimeData.weaponsInUse[slot].weaponsInSlot[runtimeData.weaponsInUse[slot].selectedQuickUse].behaviour.SupportsQuickUse(pb, runtimeData.weaponsInUse[slot].weaponsInSlot[runtimeData.weaponsInUse[slot].selectedQuickUse].runtimeData))
                                                        {
                                                            runtimeData.desiredQuickUse[0] = slot;
                                                            runtimeData.desiredQuickUse[1] = runtimeData.weaponsInUse[slot].selectedQuickUse;
                                                            runtimeData.quickUseInProgress = true;
                                                            //Also reset these!
                                                            runtimeData.quickUseState = 0;
                                                            runtimeData.quickUseOverAt = Time.time;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (Physics.Raycast(pb.playerCameraTransform.position, pb.playerCameraTransform.forward, out runtimeData.hit, pickupDistance, pickupLayers.value))
                        {
                            if (runtimeData.hit.transform.root.GetComponent<Kit_DropBehaviour>())
                            {
                                Kit_DropBehaviour drop = runtimeData.hit.transform.root.GetComponent<Kit_DropBehaviour>();
                                if (pb.isFirstPersonActive)
                                {
                                    pb.main.hud.DisplayWeaponPickup(true, drop.weaponID);
                                    pb.main.hud.DisplayInteraction(false);
                                }

                                if (runtimeData.lastDropWeapon != pb.input.dropWeapon)
                                {
                                    runtimeData.lastDropWeapon = pb.input.dropWeapon;
                                    if (pb.input.dropWeapon && (allowSwitchingWhileRunning || !pb.movement.IsRunning(pb)))
                                    {
                                        int[] slots = new int[2];

                                        if (pb.main.gameInformation.allWeapons[drop.weaponID].canFitIntoSlots.Contains(runtimeData.currentWeapon[0]))
                                        {
                                            slots[0] = runtimeData.currentWeapon[0];
                                            slots[1] = runtimeData.currentWeapon[1];
                                        }
                                        else
                                        {
                                            slots[0] = pb.main.gameInformation.allWeapons[drop.weaponID].canFitIntoSlots[0];
                                            slots[1] = 0;
                                        }

                                        //Check if we can drop
                                        if (!drop.isSceneOwned || drop.isSceneOwned && pb.main.gameInformation.enableDropWeaponOnSceneSpawnedWeapons)
                                        {
                                            //First drop our weapon
                                            DropWeapon(pb, slots[0], slots[1], drop.transform);
                                        }

                                        //Pickup new weapon
                                        pb.photonView.RPC("ReplaceWeapon", RpcTarget.AllBuffered, slots, drop.weaponID, drop.bulletsLeft, drop.bulletsLeftToReload, drop.attachments);
                                        //First hide
                                        drop.rendererRoot.SetActive(false);
                                        if (drop.isSceneOwned)
                                        {
                                            //Delete object
                                            drop.photonView.RPC("PickedUp", PhotonNetwork.MasterClient);
                                        }
                                        else
                                        {
                                            //Delete object
                                            drop.photonView.RPC("PickedUp", drop.photonView.Owner);
                                        }
                                    }
                                }
                            }
                            else if (runtimeData.hit.transform.GetComponentInParent<Kit_InteractableObject>())
                            {
                                Kit_InteractableObject io = runtimeData.hit.transform.GetComponentInParent<Kit_InteractableObject>();

                                if (io.CanInteract(pb))
                                {
                                    if (pb.isFirstPersonActive)
                                    {
                                        pb.main.hud.DisplayWeaponPickup(false);
                                        pb.main.hud.DisplayInteraction(true, io.interactionText);
                                    }

                                    if (io.IsHold())
                                    {
                                        if (pb.input.dropWeapon)
                                        {
                                            //Store object
                                            runtimeData.holdingInteractableObject = io;

                                            //Tell object we want to interact
                                            io.Interact(pb);
                                        }
                                        else{
                                            if (runtimeData.holdingInteractableObject)
                                            {
                                                runtimeData.holdingInteractableObject.InteractHoldEnd(pb);
                                                runtimeData.holdingInteractableObject = null;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //TODO: Should maybe change name later to avoid confusion
                                        if (runtimeData.lastDropWeapon != pb.input.dropWeapon)
                                        {
                                            runtimeData.lastDropWeapon = pb.input.dropWeapon;
                                            if (pb.input.dropWeapon)
                                            {
                                                //Tell object we want to interact
                                                io.Interact(pb);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (pb.isFirstPersonActive)
                                    {
                                        pb.main.hud.DisplayWeaponPickup(false);
                                        pb.main.hud.DisplayInteraction(false);
                                    }

                                    if (runtimeData.holdingInteractableObject)
                                    {
                                        runtimeData.holdingInteractableObject.InteractHoldEnd(pb);
                                        runtimeData.holdingInteractableObject = null;
                                    }
                                }
                            }
                            else
                            {
                                if (pb.isFirstPersonActive)
                                {
                                    pb.main.hud.DisplayWeaponPickup(false);
                                    pb.main.hud.DisplayInteraction(false);
                                }

                                if (runtimeData.holdingInteractableObject)
                                {
                                    runtimeData.holdingInteractableObject.InteractHoldEnd(pb);
                                    runtimeData.holdingInteractableObject = null;
                                }
                            }
                        }
                        else
                        {
                            if (pb.isFirstPersonActive)
                            {
                                pb.main.hud.DisplayWeaponPickup(false);
                                pb.main.hud.DisplayInteraction(false);
                            }

                            if (runtimeData.holdingInteractableObject)
                            {
                                runtimeData.holdingInteractableObject.InteractHoldEnd(pb);
                                runtimeData.holdingInteractableObject = null;
                            }
                        }
                    }

                    //Quick use has priority!
                    if (runtimeData.quickUseInProgress || runtimeData.quickUseState > 0)
                    {
                        //Update weapon animation
                        runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.AnimateWeapon(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData, pb.movement.GetCurrentWeaponMoveAnimation(pb), pb.movement.GetCurrentWalkAnimationSpeed(pb));

                        if (Time.time >= runtimeData.quickUseOverAt)
                        {
                            //First, put away current weapon!
                            if (runtimeData.quickUseState == 0)
                            {
                                if (!runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.QuickUseSkipsPutaway(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData))
                                {
                                    //Set time (Because here we cannot use a coroutine)
                                    runtimeData.quickUseOverAt = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.putawayTime;
                                    //Set phase
                                    runtimeData.quickUseState = 1;
                                    //Start putaway
                                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.PutawayWeapon(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                                    //Play Third person animation
                                    pb.thirdPersonPlayerModel.PlayWeaponChangeAnimation(runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.thirdPersonAnimType, false, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.putawayTime);
                                    if (pb.isFirstPersonActive)
                                    {
                                        //Hide crosshair
                                        pb.main.hud.DisplayCrosshair(0f, false);
                                    }
                                }
                                else
                                {
                                    //Set phase
                                    runtimeData.quickUseState = 1;
                                    if (pb.isFirstPersonActive)
                                    {
                                        //Hide crosshair
                                        pb.main.hud.DisplayCrosshair(0f, false);
                                    }
                                }
                            }
                            else if (runtimeData.quickUseState == 1)
                            {
                                //Weapon has been put away, hide weapon
                                runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.PutawayWeaponHide(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                                runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.QuickUseOnOtherWeaponBegin(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);

                                //Set state
                                runtimeData.quickUseState = 2;

                                //Begin quick use....
                                runtimeData.quickUseOverAt = Time.time + runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.BeginQuickUse(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                            }
                            else if (runtimeData.quickUseState == 2)
                            {
                                //Check if weapon wants to abort quick use
                                if (!runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.SupportsQuickUse(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData) ||
                                    //Check if we don't need to wait for the button
                                    (!runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.WaitForQuickUseButtonRelease(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData)) ||
                                    //Or released the button locally
                                    ((pb.photonView.IsMine && !pb.input.weaponSlotUses[slotConfiguration[runtimeData.desiredQuickUse[0]].quickUseInputID]) ||
                                    //Or button was released via sync
                                    (!pb.photonView.IsMine && runtimeData.quickUseSyncButtonWaitOver)))
                                {
                                    runtimeData.quickUseSyncButtonWaitOver = true;
                                    //Set State
                                    runtimeData.quickUseState = 3;
                                    //End quick use...
                                    runtimeData.quickUseOverAt = Time.time + runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.EndQuickUse(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                                }
                            }
                            else if (runtimeData.quickUseState == 3)
                            {
                                //Hide Quick Use!
                                runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.EndQuickUseAfter(pb, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                                //Check if currently selected  weapon is valid.
                                if (runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.CanBeSelected(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData))
                                {
                                    //Set weapon
                                    if (runtimeData.currentWeapon[0] == runtimeData.desiredWeapon[0])
                                    {
                                        runtimeData.currentWeapon[0] = runtimeData.desiredWeapon[0];
                                        runtimeData.currentWeapon[1] = runtimeData.desiredWeapon[1];
                                    }
                                    else
                                    {
                                        runtimeData.currentWeapon[0] = runtimeData.desiredWeapon[0];
                                        runtimeData.currentWeapon[1] = 0;
                                    }
                                }
                                else
                                {
                                    //Its not, find a new one
                                    int[] next = new int[2] { -1, -1 };
                                    for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                                    {
                                        for (int o = 0; o < runtimeData.weaponsInUse[i].weaponsInSlot.Length; o++)
                                        {
                                            //Check if this one works!
                                            if (runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour.CanBeSelected(pb, runtimeData.weaponsInUse[i].weaponsInSlot[o].runtimeData))
                                            {
                                                int id = i;
                                                int idTwo = o;
                                                next[0] = id;
                                                next[1] = idTwo;
                                                //We found one
                                                break;
                                            }
                                        }
                                    }

                                    //This should ALWAYS be true!
                                    if (next[0] >= 0 && next[1] >= 0)
                                    {
                                        runtimeData.desiredWeapon[0] = next[0];
                                        runtimeData.desiredWeapon[1] = next[1];
                                        //Set current one too!
                                        runtimeData.currentWeapon[0] = next[0];
                                        runtimeData.currentWeapon[1] = next[1];
                                    }
                                    else
                                    {
                                        Debug.LogError("Could not find next weapon! This is not allowed!");
                                    }
                                }

                                //Draw that weapon
                                runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.DrawWeapon(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                                //Play Third person animation
                                pb.thirdPersonPlayerModel.PlayWeaponChangeAnimation(runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.thirdPersonAnimType, true, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.drawTime);
                                //Set phase
                                runtimeData.quickUseState = 4;
                                //Set time
                                runtimeData.quickUseOverAt = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.drawTime;
                                //Done, now wait
                            }
                            else if (runtimeData.quickUseState == 4)
                            {
                                //End quick use
                                runtimeData.quickUseInProgress = false;
                                runtimeData.desiredQuickUse[0] = -1;
                                runtimeData.desiredQuickUse[1] = -1;
                                runtimeData.quickUseSyncButtonWaitOver = false;
                                runtimeData.quickUseState = 0;
                                runtimeData.quickUseOverAt = Time.time;

                                //Also reset switching just to be sure!
                                runtimeData.switchPhase = 0;
                                runtimeData.switchNextEnd = 0f;
                                runtimeData.switchInProgress = false;
                            }
                        }
                    }
                    else
                    {
                        //Update weapon animation
                        runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.AnimateWeapon(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, pb.movement.GetCurrentWeaponMoveAnimation(pb), pb.movement.GetCurrentWalkAnimationSpeed(pb));

                        if (!runtimeData.switchInProgress)
                        {
                            //If we aren't switching weapons, update weapon behaviour
                            runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.CalculateWeaponUpdate(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);

                            //Check if we want to select a different weapon
                            if (runtimeData.desiredWeapon[0] != runtimeData.currentWeapon[0] || runtimeData.desiredWeapon[0] == runtimeData.currentWeapon[0] && runtimeData.desiredWeapon[1] != runtimeData.currentWeapon[1])
                            {
                                //If not, start to switch
                                runtimeData.switchInProgress = true;
                                //Set time (Because here we cannot use a coroutine)
                                runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.putawayTime;
                                //Set phase
                                runtimeData.switchPhase = 0;
                                //Start putaway
                                runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.PutawayWeapon(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                                //Play Third person animation
                                pb.thirdPersonPlayerModel.PlayWeaponChangeAnimation(runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.thirdPersonAnimType, false, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.putawayTime);
                                if (pb.isFirstPersonActive)
                                {
                                    //Hide crosshair
                                    pb.main.hud.DisplayCrosshair(0f, false);
                                }
                            }
                        }
                        else
                        {
                            //Switching, courtine less
                            #region Switching
                            //Check for time
                            if (Time.time >= runtimeData.switchNextEnd)
                            {
                                //Time is over, check which phase is next
                                if (runtimeData.switchPhase == 0)
                                {
                                    //Weapon has been put away, hide weapon
                                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.PutawayWeaponHide(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);

                                    runtimeData.currentWeapon[0] = runtimeData.desiredWeapon[0];
                                    runtimeData.currentWeapon[1] = runtimeData.desiredWeapon[1];

                                    //Draw that weapon
                                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.DrawWeapon(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                                    //Set phase
                                    runtimeData.switchPhase = 1;
                                    //Set time
                                    runtimeData.switchNextEnd = Time.time + runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.drawTime;
                                    //Play Third person animation
                                    pb.thirdPersonPlayerModel.PlayWeaponChangeAnimation(runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.thirdPersonAnimType, true, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.drawTime);
                                    //Done, now wait
                                }
                                else if (runtimeData.switchPhase == 1)
                                {
                                    //Switching is over
                                    runtimeData.switchPhase = 0;
                                    runtimeData.switchNextEnd = 0f;
                                    runtimeData.switchInProgress = false;
                                }
                            }
                            #endregion
                        }
                    }

                    //Move weapons transform
                    pb.weaponsGo.localPosition = Vector3.Lerp(pb.weaponsGo.localPosition, Vector3.zero + pb.looking.GetWeaponOffset(pb), Time.deltaTime * weaponPositionChangeSpeed);

                    //Move weapons transform
                    pb.weaponsGo.localRotation = Quaternion.Slerp(pb.weaponsGo.localRotation, pb.looking.GetWeaponRotationOffset(pb), Time.deltaTime * weaponPositionChangeSpeed);

                    if (pb.isFirstPersonActive)
                    {
                        pb.main.hud.DisplayWeaponsAndQuickUses(pb, runtimeData);
                    }
                }
            }

            public override void PlayerDead(Kit_PlayerBehaviour pb)
            {
                if ((pb.main.currentPvPGameModeBehaviour && pb.main.currentPvPGameModeBehaviour.CanDropWeapons(pb.main)) || (pb.main.currentPvEGameModeBehaviour && pb.main.currentPvEGameModeBehaviour.CanDropWeapons(pb.main)))
                {
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        if (uponDeathDrop == DeadDrop.Selected)
                        {
                            DropWeaponDead(pb, runtimeData.currentWeapon[0], runtimeData.currentWeapon[1]);
                        }
                        else if (uponDeathDrop == DeadDrop.All)
                        {
                            for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                            {
                                if (!runtimeData.weaponsInUse[i].isInjectedFromPlugin)
                                {
                                    for (int o = 0; o < runtimeData.weaponsInUse[i].weaponsInSlot.Length; o++)
                                    {
                                        DropWeaponDead(pb, i, o);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public override void OnAnimatorIKCallback(Kit_PlayerBehaviour pb, Animator anim)
            {
                //Get runtime data
                if (pb.isController)
                {
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        if (anim)
                        {
                            //Get Weapon IK
                            WeaponIKValues ikv = null;

                            if (runtimeData.quickUseInProgress && runtimeData.quickUseState > 0 && runtimeData.quickUseState < 4)
                            {
                                ikv = runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.GetIK(pb, anim, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                            }
                            else
                            {
                                ikv = runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.GetIK(pb, anim, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                            }

                            if (ikv != null)
                            {
                                if (ikv.leftHandIK)
                                {
                                    anim.SetIKPosition(AvatarIKGoal.LeftHand, ikv.leftHandIK.position);
                                    anim.SetIKRotation(AvatarIKGoal.LeftHand, ikv.leftHandIK.rotation);
                                }
                                if (!runtimeData.switchInProgress && ikv.canUseIK && ikv.leftHandIK)
                                {
                                    runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 1f, Time.deltaTime * 3);
                                }
                                else
                                {
                                    runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 0f, Time.deltaTime * 20);
                                }
                                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                            }
                            else
                            {
                                runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 0f, Time.deltaTime * 20);
                                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                            }
                        }
                    }
                }
                else
                {
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        if (anim)
                        {
                            //Get Weapon IK
                            WeaponIKValues ikv = null;

                            if (runtimeData.quickUseInProgress && runtimeData.quickUseState > 0 && runtimeData.quickUseState < 4)
                            {
                                ikv = runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.GetIK(pb, anim, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                            }
                            else
                            {
                                ikv = runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.GetIK(pb, anim, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                            }

                            if (ikv != null)
                            {
                                if (ikv.leftHandIK)
                                {
                                    anim.SetIKPosition(AvatarIKGoal.LeftHand, ikv.leftHandIK.position);
                                    anim.SetIKRotation(AvatarIKGoal.LeftHand, ikv.leftHandIK.rotation);
                                }
                                if (!runtimeData.switchInProgress && ikv.canUseIK && ikv.leftHandIK)
                                {
                                    runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 1f, Time.deltaTime * 3);
                                }
                                else
                                {
                                    runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 0f, Time.deltaTime * 20);
                                }
                                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                            }
                            else
                            {
                                runtimeData.leftHandIKWeight = Mathf.Lerp(runtimeData.leftHandIKWeight, 0f, Time.deltaTime * 20);
                                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, runtimeData.leftHandIKWeight);
                            }
                        }
                    }
                }
            }

            public override void FallDownEffect(Kit_PlayerBehaviour pb, bool wasFallDamageApplied)
            {
                if (pb.isBot) return;
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.FallDownEffect(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, wasFallDamageApplied);
                }
            }

            public override void OnControllerColliderHitRelay(Kit_PlayerBehaviour pb, ControllerColliderHit hit)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;

                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.OnControllerColliderHitCallback(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, hit);
                }
            }

            public override void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info)
            {
                if (stream.IsWriting)
                {
                    //Get runtime data
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        //Send runtime data
                        stream.SendNext(runtimeData.desiredWeapon[0]);
                        stream.SendNext(runtimeData.desiredWeapon[1]);

                        stream.SendNext(runtimeData.desiredQuickUse[0]);
                        stream.SendNext(runtimeData.desiredQuickUse[1]);
                        stream.SendNext(runtimeData.quickUseInProgress);
                        stream.SendNext(runtimeData.quickUseSyncButtonWaitOver);

                        //Callback for weapon
                        runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.OnPhotonSerializeView(pb, stream, info, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                    }
                    //Send dummy data
                    else
                    {
                        stream.SendNext(0);
                        stream.SendNext(0);

                        stream.SendNext(0);
                        stream.SendNext(0);
                        stream.SendNext(false);
                        stream.SendNext(false);
                    }
                }
                else
                {
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        runtimeData.desiredWeapon[0] = (int)stream.ReceiveNext();
                        runtimeData.desiredWeapon[1] = (int)stream.ReceiveNext();

                        runtimeData.desiredQuickUse[0] = (int)stream.ReceiveNext();
                        runtimeData.desiredQuickUse[1] = (int)stream.ReceiveNext();
                        runtimeData.quickUseInProgress = (bool)stream.ReceiveNext();
                        runtimeData.quickUseSyncButtonWaitOver = (bool)stream.ReceiveNext();

                        //Callback for weapon
                        runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.OnPhotonSerializeView(pb, stream, info, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                    }
                    else
                    {
                        //Dummy reading
                        stream.ReceiveNext();
                        stream.ReceiveNext();
                        stream.ReceiveNext();
                        stream.ReceiveNext();
                        stream.ReceiveNext();
                        stream.ReceiveNext();
                    }
                }
            }

            public override void NetworkSemiRPCReceived(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkSemiRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
            }

            public override void NetworkBoltActionRPCReceived(Kit_PlayerBehaviour pb, int state)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkBoltActionRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, state);
                }
            }

            public override void NetworkBurstRPCReceived(Kit_PlayerBehaviour pb, int burstLength)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkBurstRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, burstLength);
                }
            }

            public override void NetworkReloadRPCReceived(Kit_PlayerBehaviour pb, bool isEmpty)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkReloadRPCReceived(pb, isEmpty, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
            }

            public override void NetworkProceduralReloadRPCReceived(Kit_PlayerBehaviour pb, int stage)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkProceduralReloadRPCReceived(pb, stage, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
            }

            public override void NetworkMeleeChargeRPCReceived(Kit_PlayerBehaviour pb, int state, int slot)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkMeleeChargeRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, state, slot);
                }
            }

            public override void NetworkMeleeHealRPCReceived(Kit_PlayerBehaviour pb, int slot)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkMeleeHealRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, slot);
                }
            }

            public override void NetworkMeleeStabRPCReceived(Kit_PlayerBehaviour pb, int state, int slot)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkMeleeStabRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData, state, slot);
                }
            }

            public override void NetworkGrenadePullPinRPCReceived(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkGrenadePullPinRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
            }

            public override void NetworkGrenadeThrowRPCReceived(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkGrenadeThrowRPCReceived(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
            }

            public override float GetAimingPercentage(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.GetAimingPercentage(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData); //Relay to weapon script
                }
                return 0;
            }

            public override bool IsAiming(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.IsWeaponAiming(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData); //Relay to weapon script
                }
                return false;
            }

            public override float AimInTime(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.AimInTime(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData); //Relay to weapon script
                }
                return 0.5f;
            }

            public override bool ForceIntoFirstPerson(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.ForceIntoFirstPerson(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData); //Relay to weapon script
                }
                return false;
            }

            public override bool CanRun(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    if (allowSwitchingWhileRunning) return true;
                    else return !runtimeData.switchInProgress;
                }
                return true;
            }

            public override float CurrentMovementMultiplier(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.SpeedMultiplier(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData); //Relay to weapon script
                }
                return 1f;
            }

            public override float CurrentSensitivity(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.Sensitivity(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData); //Relay to weapon script
                }
                return 1f;
            }

            public override int GetCurrentWeapon(Kit_PlayerBehaviour pb)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Just return ID
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].id;
                }
                return -1;
            }

            public override bool CanBuyWeapon(Kit_PlayerBehaviour pb, int id)
            {
                //Get runtime data
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;

                    for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                    {
                        for (int o = 0; o < runtimeData.weaponsInUse[i].weaponsInSlot.Length; o++)
                        {
                            if (runtimeData.weaponsInUse[i].weaponsInSlot[o].id == id)
                            {
                                //Special case for grenades - let us buy more grenades if we are below that #
                                if (runtimeData.weaponsInUse[i].weaponsInSlot[o].runtimeData != null && runtimeData.weaponsInUse[i].weaponsInSlot[o].runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                                {
                                    GrenadeControllerRuntimeData gcrd = runtimeData.weaponsInUse[i].weaponsInSlot[o].runtimeData as GrenadeControllerRuntimeData;

                                    Kit_ModernGrenadeScript grenadeData = runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour as Kit_ModernGrenadeScript;


                                    if (gcrd.amountOfGrenadesLeft < grenadeData.amountOfGrenadesAtStart)
                                    {
                                        return true;
                                    }

                                }

                                return false;
                            }
                        }
                    }

                    //Can buy
                    return true;
                }
                return false;
            }

            public override void NetworkReplaceWeapon(Kit_PlayerBehaviour pb, int[] slot, int weapon, int bulletsLeft, int bulletsLeftToReload, int[] attachments)
            {
                Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.NetworkReplaceWeaponWait(pb, slot, weapon, bulletsLeft, bulletsLeftToReload, attachments));
            }

            public override void NetworkPhysicalBulletFired(Kit_PlayerBehaviour pb, Vector3 pos, Vector3 dir)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Relay to weapon script
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.NetworkPhysicalBulletFired(pb, pos, dir, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
            }

            public void DropWeapon(Kit_PlayerBehaviour pb, int slot, int weaponInSlot)
            {
                if ((pb.main.currentPvPGameModeBehaviour && pb.main.currentPvPGameModeBehaviour.CanDropWeapons(pb.main)) || (pb.main.currentPvEGameModeBehaviour && pb.main.currentPvEGameModeBehaviour.CanDropWeapons(pb.main)))
                {
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        //Get the manager's runtime data
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        //Setup instantiation data
                        object[] instData = new object[4 + runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length];
                        if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                        {
                            //Get the weapon's runtime data
                            WeaponControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData as WeaponControllerRuntimeData;
                            //Get the Scriptable object
                            Kit_ModernWeaponScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernWeaponScript;
                            //ID
                            instData[0] = wepScript.gameGunID;
                            //Bullets left
                            instData[1] = wepData.bulletsLeft;
                            //Bullets Left To Reload
                            instData[2] = wepData.bulletsLeftToReload;
                            //Attachments length
                            instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                            for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                            {
                                instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                            }
                            //Instantiate
                            PhotonNetwork.Instantiate(dropPrefab.name, pb.playerCameraTransform.position, pb.playerCameraTransform.rotation, 0, instData);
                        }
                        else if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                        {
                            //Get the weapon's runtime data
                            //MeleeControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData as MeleeControllerRuntimeData;
                            //Get the Scriptable object
                            Kit_ModernMeleeScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernMeleeScript;
                            //ID
                            instData[0] = wepScript.gameGunID;
                            //Bullets left (nothing)
                            instData[1] = 0;
                            //Bullets Left To Reload (nothing;
                            instData[2] = 0;
                            //Attachments length (melee doesnt support that yet but well do it anyway)
                            instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                            for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                            {
                                instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                            }
                            //Instantiate
                            PhotonNetwork.Instantiate(dropPrefab.name, pb.playerCameraTransform.position, pb.playerCameraTransform.rotation, 0, instData);
                        }
                        else if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                        {
                            //Get the weapon's runtime data
                            GrenadeControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData as GrenadeControllerRuntimeData;
                            if (wepData.amountOfGrenadesLeft <= 0) return;
                            //Get the Scriptable object
                            Kit_ModernGrenadeScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernGrenadeScript;
                            //ID
                            instData[0] = wepScript.gameGunID;
                            //Bullets left (grenades left)
                            instData[1] = wepData.amountOfGrenadesLeft;
                            //Bullets Left To Reload (nothing;
                            instData[2] = 0;
                            //Attachments length (melee doesnt support that yet but well do it anyway)
                            instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                            for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                            {
                                instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                            }
                            //Instantiate
                            PhotonNetwork.Instantiate(dropPrefab.name, pb.playerCameraTransform.position, pb.playerCameraTransform.rotation, 0, instData);
                        }
                    }
                }
            }

            /// <summary>
            /// Drops a weapon and applies the ragdoll force!
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="slot"></param>
            public void DropWeaponDead(Kit_PlayerBehaviour pb, int slot, int weaponInSlot)
            {
                if ((pb.main.currentPvPGameModeBehaviour && pb.main.currentPvPGameModeBehaviour.CanDropWeapons(pb.main)) || (pb.main.currentPvEGameModeBehaviour && pb.main.currentPvEGameModeBehaviour.CanDropWeapons(pb.main)))
                {
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        //Get the manager's runtime data
                        WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                        //Setup instantiation data
                        object[] instData = new object[4 + runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length];
                        if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour.dropPrefab)
                        {
                            if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                            {
                                //Get the weapon's runtime data
                                WeaponControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData as WeaponControllerRuntimeData;
                                //Get the Scriptable object
                                Kit_ModernWeaponScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernWeaponScript;
                                //ID
                                instData[0] = wepScript.gameGunID;
                                //Bullets left
                                instData[1] = wepData.bulletsLeft;
                                //Bullets Left To Reload
                                instData[2] = wepData.bulletsLeftToReload;
                                //Attachments length
                                instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                                for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                                {
                                    instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                                }
                                //Instantiate
                                GameObject go = PhotonNetwork.Instantiate(dropPrefab.name, pb.playerCameraTransform.position + Random.insideUnitSphere, pb.playerCameraTransform.rotation, 0, instData);
                                Rigidbody body = go.GetComponent<Rigidbody>();
                                body.velocity = pb.movement.GetVelocity(pb);
                                body.AddForceNextFrame(pb.ragdollForward * pb.ragdollForce / 10);
                            }
                            else if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                            {
                                //Get the weapon's runtime data
                                //MeleeControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData as MeleeControllerRuntimeData;
                                //Get the Scriptable object
                                Kit_ModernMeleeScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernMeleeScript;
                                //ID
                                instData[0] = wepScript.gameGunID;
                                //Bullets left
                                instData[1] = 0;
                                //Bullets Left To Reload
                                instData[2] = 0;
                                //Attachments length
                                instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                                for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                                {
                                    instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                                }
                                //Instantiate
                                GameObject go = PhotonNetwork.Instantiate(dropPrefab.name, pb.playerCameraTransform.position + Random.insideUnitSphere, pb.playerCameraTransform.rotation, 0, instData);
                                Rigidbody body = go.GetComponent<Rigidbody>();
                                body.velocity = pb.movement.GetVelocity(pb);
                                body.AddForceNextFrame(pb.ragdollForward * pb.ragdollForce / 10);
                            }
                            else if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                            {
                                //Get the weapon's runtime data
                                GrenadeControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData as GrenadeControllerRuntimeData;
                                if (wepData.amountOfGrenadesLeft <= 0) return;
                                //Get the Scriptable object
                                Kit_ModernGrenadeScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernGrenadeScript;
                                //ID
                                instData[0] = wepScript.gameGunID;
                                //Bullets left
                                instData[1] = wepData.amountOfGrenadesLeft;
                                //Bullets Left To Reload
                                instData[2] = 0;
                                //Attachments length
                                instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                                for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                                {
                                    instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                                }
                                //Instantiate
                                GameObject go = PhotonNetwork.Instantiate(dropPrefab.name, pb.playerCameraTransform.position + Random.insideUnitSphere, pb.playerCameraTransform.rotation, 0, instData);
                                Rigidbody body = go.GetComponent<Rigidbody>();
                                body.velocity = pb.movement.GetVelocity(pb);
                                body.AddForceNextFrame(pb.ragdollForward * pb.ragdollForce / 10);
                            }
                        }
                    }
                }
            }

            public void DropWeapon(Kit_PlayerBehaviour pb, int slot, int weaponInSlot, Transform replace)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    //Get the manager's runtime data
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour.dropPrefab)
                    {
                        //Setup instantiation data
                        object[] instData = new object[4 + runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length];
                        if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                        {
                            //Get the weapon's runtime data
                            WeaponControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData as WeaponControllerRuntimeData;
                            //Get the Scriptable object
                            Kit_ModernWeaponScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernWeaponScript;
                            //ID
                            instData[0] = wepScript.gameGunID;
                            //Bullets left
                            instData[1] = wepData.bulletsLeft;
                            //Bullets Left To Reload
                            instData[2] = wepData.bulletsLeftToReload;
                            //Attachments length
                            instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                            for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                            {
                                instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                            }
                            //Instantiate
                            PhotonNetwork.Instantiate(dropPrefab.name, replace.position, replace.rotation, 0, instData);
                        }
                        else if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(MeleeControllerRuntimeData))
                        {
                            //Get the weapon's runtime data
                            //MeleeControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData as MeleeControllerRuntimeData;
                            //Get the Scriptable object
                            Kit_ModernMeleeScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernMeleeScript;
                            //ID
                            instData[0] = wepScript.gameGunID;
                            //Bullets left
                            instData[1] = 0;
                            //Bullets Left To Reload
                            instData[2] = 0;
                            //Attachments length
                            instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                            for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                            {
                                instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                            }
                            //Instantiate
                            PhotonNetwork.Instantiate(dropPrefab.name, replace.position, replace.rotation, 0, instData);
                        }
                        else if (runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData != null && runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData.GetType() == typeof(GrenadeControllerRuntimeData))
                        {
                            //Get the weapon's runtime data
                            GrenadeControllerRuntimeData wepData = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].runtimeData as GrenadeControllerRuntimeData;
                            if (wepData.amountOfGrenadesLeft <= 0) return;
                            //Get the Scriptable object
                            Kit_ModernGrenadeScript wepScript = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].behaviour as Kit_ModernGrenadeScript;
                            //ID
                            instData[0] = wepScript.gameGunID;
                            //Bullets left
                            instData[1] = wepData.amountOfGrenadesLeft;
                            //Bullets Left To Reload
                            instData[2] = 0;
                            //Attachments length
                            instData[3] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length;
                            for (int i = 0; i < runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments.Length; i++)
                            {
                                instData[4 + i] = runtimeData.weaponsInUse[slot].weaponsInSlot[weaponInSlot].attachments[i];
                            }
                            //Instantiate
                            PhotonNetwork.Instantiate(dropPrefab.name, replace.position, replace.rotation, 0, instData);
                        }
                    }
                }
            }

            public override int WeaponState(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    //Get the manager's runtime data
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.WeaponState(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
                return 0;
            }

            public override int WeaponType(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    //Get the manager's runtime data
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.GetWeaponType(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
                return 0;
            }

            public override void FirstThirdPersonChanged(Kit_PlayerBehaviour pb, Kit_GameInformation.Perspective perspective)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    //Forward to currently selected weapon
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    if (runtimeData.quickUseInProgress && runtimeData.quickUseState > 0 && runtimeData.quickUseState < 4)
                    {
                        runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].behaviour.FirstThirdPersonChanged(pb, perspective, runtimeData.weaponsInUse[runtimeData.desiredQuickUse[0]].weaponsInSlot[runtimeData.desiredQuickUse[1]].runtimeData);
                    }
                    else
                    {
                        runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.FirstThirdPersonChanged(pb, perspective, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                    }
                }
            }

            public override void OnAmmoPickup(Kit_PlayerBehaviour pb, Kit_AmmoPickup pickup)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    //Forward to currently selected weapon
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.OnAmmoPickup(pb, pickup, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }
            }

            public override void RestockAmmo(Kit_PlayerBehaviour pb, bool allWeapons)
            {
                if (pb.photonView.IsMine)
                {
                    if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                    {
                        if (allWeapons)
                        {
                            WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                            for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                            {
                                for (int o = 0; o < runtimeData.weaponsInUse[i].weaponsInSlot.Length; o++)
                                {
                                    runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour.RestockAmmo(pb, runtimeData.weaponsInUse[i].weaponsInSlot[o].runtimeData);
                                }
                            }
                        }
                        else
                        {
                            //Forward to currently selected weapon
                            WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                            runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.RestockAmmo(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                        }
                    }
                }
                else if (PhotonNetwork.IsMasterClient)
                {
                    //Send RPC to do this
                    pb.photonView.RPC("WeaponRestockAll", pb.photonView.Owner, allWeapons);
                }
            }

            public override bool IsCurrentWeaponFull(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    //Forward to currently selected weapon
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    return runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].behaviour.IsWeaponFull(pb, runtimeData.weaponsInUse[runtimeData.currentWeapon[0]].weaponsInSlot[runtimeData.currentWeapon[1]].runtimeData);
                }

                return true;
            }

            public override void PluginSelectWeapon(Kit_PlayerBehaviour pb, int slot, int id, bool locked = true)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    //Just set, thats all!
                    runtimeData.desiredWeapon[1] = id;
                    runtimeData.desiredWeapon[0] = slot;
                    runtimeData.isDesiredWeaponLocked = locked;
                }
            }

            public override int[] GetCurrentlyDesiredWeapon(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    int[] toReturn = new int[2];
                    toReturn[0] = runtimeData.desiredWeapon[0];
                    toReturn[1] = runtimeData.desiredWeapon[1];
                    return toReturn;
                }
                return null;
            }

            public override int[] GetCurrentlyDesiredQuickUse(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    int[] toReturn = new int[2];
                    toReturn[0] = runtimeData.desiredQuickUse[0];
                    toReturn[1] = runtimeData.desiredQuickUse[1];
                    return toReturn;
                }
                return null;
            }

            public override int[] GetCurrentlySelectedWeapon(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    int[] toReturn = new int[2];
                    toReturn[0] = runtimeData.currentWeapon[0];
                    toReturn[1] = runtimeData.currentWeapon[1];
                    return toReturn;
                }
                return null;
            }

            public override void SetDesiredWeapon(Kit_PlayerBehaviour pb, int[] desiredWeapon)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;
                    if (runtimeData.weaponsInUse[desiredWeapon[0]].weaponsInSlot[desiredWeapon[1]].behaviour.CanBeSelected(pb, runtimeData.weaponsInUse[desiredWeapon[0]].weaponsInSlot[desiredWeapon[1]].runtimeData))
                    {
                        runtimeData.desiredWeapon = desiredWeapon;
                    }
                }
            }

            public override int[][] GetSlotsWithEmptyWeapon(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;

                    List<int[]> emptySlots = new List<int[]>();

                    for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                    {

                        for (int o = 0; o < runtimeData.weaponsInUse[i].weaponsInSlot.Length; o++)
                        {
                            //Check if that slot contains a placehodler weapon
                            if (runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour.GetType() == typeof(Kit_WeaponUnselectable))
                            {
                                int id = i;
                                int od = o;
                                //Add it to list of empty slots
                                emptySlots.Add(new int[] { id, od });
                            }
                        }
                    }

                    return emptySlots.ToArray();
                }
                return null;
            }

            public override void BeginSpectating(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;

                    for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                    {
                        for (int o = 0; o < runtimeData.weaponsInUse[i].weaponsInSlot.Length; o++)
                        {
                            //Relay
                            runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour.BeginSpectating(pb, runtimeData.weaponsInUse[i].weaponsInSlot[o].runtimeData, runtimeData.weaponsInUse[i].weaponsInSlot[o].attachments);
                        }
                    }
                }
            }

            public override void EndSpectating(Kit_PlayerBehaviour pb)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;

                    for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                    {
                        for (int o = 0; o < runtimeData.weaponsInUse[i].weaponsInSlot.Length; o++)
                        {
                            //Relay
                            runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour.EndSpectating(pb, runtimeData.weaponsInUse[i].weaponsInSlot[o].runtimeData);
                        }
                    }
                }
            }

            public override void OnTriggerEnterRelay(Kit_PlayerBehaviour pb, Collider col)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;

                    for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                    {
                        for (int o = 0; o < runtimeData.weaponsInUse[i].weaponsInSlot.Length; o++)
                        {
                            //Relay
                            runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour.OnTriggerEnterRelay(pb, runtimeData.weaponsInUse[i].weaponsInSlot[o].runtimeData, col);
                        }
                    }
                }
            }

            public override void OnTriggerExitRelay(Kit_PlayerBehaviour pb, Collider col)
            {
                if (pb.customWeaponManagerData != null && pb.customWeaponManagerData.GetType() == typeof(WeaponManagerControllerRuntimeData))
                {
                    WeaponManagerControllerRuntimeData runtimeData = pb.customWeaponManagerData as WeaponManagerControllerRuntimeData;

                    for (int i = 0; i < runtimeData.weaponsInUse.Length; i++)
                    {
                        for (int o = 0; o < runtimeData.weaponsInUse[i].weaponsInSlot.Length; o++)
                        {
                            //Relay
                            runtimeData.weaponsInUse[i].weaponsInSlot[o].behaviour.OnTriggerExitRelay(pb, runtimeData.weaponsInUse[i].weaponsInSlot[o].runtimeData, col);
                        }
                    }
                }
            }
        }
    }
}
