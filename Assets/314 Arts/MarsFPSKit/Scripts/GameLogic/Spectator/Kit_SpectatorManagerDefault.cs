using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MarsFPSKit
{
    namespace Spectating
    {
        /// <summary>
        /// Runtime data for the default spectator manager
        /// </summary>
        public class SpectatorManagerRuntimeData
        {
            /// <summary>
            /// Whomst we are currently spectating
            /// </summary>
            public Kit_PlayerBehaviour currentSpectator;
            /// <summary>
            /// Are we currently in spectating mode?
            /// </summary>
            public bool isSpectating;
            /// <summary>
            /// UI reference
            /// </summary>
            public Kit_SpectatorUI ui;
        }

        [CreateAssetMenu(menuName = "MarsFPSKit/Spectator/New Default Manager")]
        public class Kit_SpectatorManagerDefault : Kit_SpectatorManagerBase
        {
            [Tooltip("Is spectating globally enabled?")]
            /// <summary>
            /// Is spectating globally enabled?
            /// </summary>
            public bool enableSpectating = true;

            /// <summary>
            /// Prefab for spectating ui
            /// </summary>
            public GameObject spectatingUi;

            public override bool IsSpectatingEnabled(Kit_IngameMain main)
            {
                return enableSpectating;
            }

            public override void Setup(Kit_IngameMain main)
            {
                //Create runtime data
                SpectatorManagerRuntimeData smrd = new SpectatorManagerRuntimeData();

                //Create UI
                if (spectatingUi)
                {
                    GameObject go = Instantiate(spectatingUi, main.ui_root.transform, false);
                    Kit_SpectatorUI ui = go.GetComponent<Kit_SpectatorUI>();

                    //Setup buttons
                    ui.previousPlayer.onClick.AddListener(delegate { PreviousPlayer(main); });
                    ui.nextPlayer.onClick.AddListener(delegate { NextPlayer(main); });

                    //Assign
                    smrd.ui = ui;
                    //Disable
                    ui.gameObject.SetActive(false);
                }

                //And assign
                main.spectatorManagerRuntimeData = smrd;
            }

            public override void BeginSpectating(Kit_IngameMain main, bool leaveTeam)
            {
                SpectatorManagerRuntimeData smrd = main.spectatorManagerRuntimeData as SpectatorManagerRuntimeData;

                if (!smrd.isSpectating)
                {
                    if (!main.currentPvPGameModeBehaviour || (main.currentPvPGameModeBehaviour && main.currentPvPGameModeBehaviour.GetSpectateable(main) != Spectateable.None))
                    {
                        Debug.Log("[Spectator Manager] Starting Spectating Mode");

                        //Destroy our player if necessary
                        if (main.myPlayer)
                        {
                            PhotonNetwork.Destroy(main.myPlayer.gameObject);
                        }

                        if (leaveTeam)
                        {
                            main.NoTeam();
                        }

                        //Close all menus
                        main.SwitchMenu(main.ingameFadeId);
                        //Proceed pause menu
                        main.pauseMenuState = PauseMenuState.main;

                        //Spectate first player
                        Kit_PlayerBehaviour[] spectateables = GetSpectateablePlayers(main);

                        if (spectateables.Length > 0)
                        {
                            SetSpectatingPlayer(main, spectateables[0]);
                        }
                        else
                        {
                            if (smrd.ui)
                            {
                                smrd.ui.currentPlayer.text = "Currently Spectating \n No one.";
                            }
                        }

                        //Enable UI
                        if (smrd.ui)
                        {
                            smrd.ui.gameObject.SetActive(true);
                        }

                        smrd.isSpectating = true;
                        
                    }
                }
                else
                {
                    //Close all menus
                    main.SwitchMenu(main.ingameFadeId);
                    //Proceed pause menu
                    main.pauseMenuState = PauseMenuState.main;
                }
            }

            public override void EndSpectating(Kit_IngameMain main)
            {
                SpectatorManagerRuntimeData smrd = main.spectatorManagerRuntimeData as SpectatorManagerRuntimeData;

                if (smrd.isSpectating)
                {
                    Debug.Log("[Spectator Manager] Ending Spectating Mode");

                    if (smrd.currentSpectator)
                    {
                        smrd.currentSpectator.OnSpectatingEnd();
                    }

                    smrd.currentSpectator = null;
                    smrd.isSpectating = false;

                    //Disable UI
                    if (smrd.ui)
                    {
                        smrd.ui.gameObject.SetActive(false);
                    }
                }
            }

            public override void PlayerWasSpawned(Kit_IngameMain main, Kit_PlayerBehaviour pb)
            {
                SpectatorManagerRuntimeData smrd = main.spectatorManagerRuntimeData as SpectatorManagerRuntimeData;

                if (smrd.isSpectating)
                {
                    if (!smrd.currentSpectator)
                    {
                        if (CanSpectatePlayer(main, pb))
                        {
                            SetSpectatingPlayer(main, pb);
                        }
                    }
                }
            }

            public override void PlayerWasKilled(Kit_IngameMain main, Kit_PlayerBehaviour pb)
            {
                SpectatorManagerRuntimeData smrd = main.spectatorManagerRuntimeData as SpectatorManagerRuntimeData;

                if (smrd.isSpectating)
                {
                    //End spectating on that guy
                    if (smrd.currentSpectator == pb)
                    {
                        Kit_PlayerBehaviour[] spectateables = GetSpectateablePlayers(main);

                        if (spectateables.Length > 1)
                        {
                            //Get index
                            int cur = System.Array.IndexOf(spectateables, pb);
                            //Increase
                            cur++;
                            //Clamp
                            if (cur >= spectateables.Length) cur = 0;

                            //Spectate new guy
                            SetSpectatingPlayer(main, spectateables[cur]);
                        }
                        else
                        {
                            pb.OnSpectatingEnd();

                            if (smrd.ui)
                            {
                                smrd.ui.currentPlayer.text = "Currently Spectating \n No one.";
                            }
                        }
                    }
                }
            }

            public void SetSpectatingPlayer(Kit_IngameMain main, Kit_PlayerBehaviour toSpectate)
            {
                SpectatorManagerRuntimeData smrd = main.spectatorManagerRuntimeData as SpectatorManagerRuntimeData;

                //Check if we are currently spectating someone
                if (smrd.currentSpectator)
                {
                    smrd.currentSpectator.OnSpectatingEnd();
                }

                //Assign new player
                smrd.currentSpectator = toSpectate;

                //Begin spectating that guy
                if (smrd.currentSpectator)
                {
                    Debug.Log("[Specator Manager] Now Spectating " + smrd.currentSpectator.name, smrd.currentSpectator);

                    smrd.currentSpectator.OnSpectatingStart();

                    //Set UI
                    if (smrd.ui)
                    {
                        smrd.ui.currentPlayer.text = "Currently Spectating \n" + smrd.currentSpectator.name;
                    }
                }
            }

            public void PreviousPlayer(Kit_IngameMain main)
            {
                SpectatorManagerRuntimeData smrd = main.spectatorManagerRuntimeData as SpectatorManagerRuntimeData;

                if (smrd.isSpectating)
                {
                    Kit_PlayerBehaviour[] spectateables = GetSpectateablePlayers(main);

                    if (spectateables.Length > 0)
                    {
                        if (smrd.currentSpectator)
                        {
                            //Get index
                            int cur = System.Array.IndexOf(spectateables, smrd.currentSpectator);
                            //Increase
                            cur--;
                            //Clamp
                            if (cur < 0) cur = spectateables.Length - 1;
                            if (cur < 0) cur = spectateables.Length - 1;

                            //Spectate new guy
                            SetSpectatingPlayer(main, spectateables[cur]);
                        }
                        else
                        {
                            //Spectate new guy
                            SetSpectatingPlayer(main, spectateables[0]);
                        }
                    }
                }
            }

            public void NextPlayer(Kit_IngameMain main)
            {
                SpectatorManagerRuntimeData smrd = main.spectatorManagerRuntimeData as SpectatorManagerRuntimeData;

                if (smrd.isSpectating)
                {
                    Kit_PlayerBehaviour[] spectateables = GetSpectateablePlayers(main);

                    if (spectateables.Length > 0)
                    {
                        if (smrd.currentSpectator)
                        {
                            //Get index
                            int cur = System.Array.IndexOf(spectateables, smrd.currentSpectator);
                            //Increase
                            cur++;
                            //Clamp
                            if (cur >= spectateables.Length) cur = 0;

                            //Spectate new guy
                            SetSpectatingPlayer(main, spectateables[cur]);
                        }
                        else
                        {
                            //Spectate new guy
                            SetSpectatingPlayer(main, spectateables[0]);
                        }
                    }
                }
            }

            public override bool IsCurrentlySpectating(Kit_IngameMain main)
            {
                SpectatorManagerRuntimeData smrd = main.spectatorManagerRuntimeData as SpectatorManagerRuntimeData;

                return smrd.isSpectating;
            }

            public Kit_PlayerBehaviour[] GetSpectateablePlayers(Kit_IngameMain main)
            {
                if (main.currentPvPGameModeBehaviour && main.currentPvPGameModeBehaviour.GetSpectateable(main) == Spectateable.Friendlies)
                {
                    return main.allActivePlayers.Where(x => !main.currentPvPGameModeBehaviour.AreWeEnemies(main, x.isBot, x.id)).ToArray();
                }
                else if (!main.currentPvPGameModeBehaviour || main.currentPvPGameModeBehaviour.GetSpectateable(main) == Spectateable.All)
                {
                    return main.allActivePlayers.ToArray();
                }

                return null;
            }

            public bool CanSpectatePlayer(Kit_IngameMain main, Kit_PlayerBehaviour player)
            {
                if (main.currentPvPGameModeBehaviour && main.currentPvPGameModeBehaviour.GetSpectateable(main) == Spectateable.Friendlies)
                {
                    return !main.currentPvPGameModeBehaviour.AreWeEnemies(main, player.isBot, player.id);
                }
                else if (!main.currentPvPGameModeBehaviour || main.currentPvPGameModeBehaviour.GetSpectateable(main) == Spectateable.All)
                {
                    return true;
                }

                return false;
            }
        }
    }
}