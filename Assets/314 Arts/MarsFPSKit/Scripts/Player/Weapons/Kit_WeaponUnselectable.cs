using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    namespace Weapons
    {
        public class UnselectableWeaponRuntimeData
        {

        }

        [CreateAssetMenu(menuName = ("MarsFPSKit/Weapons/Unselectable (Used to hold a slot free)"))]
        /// <summary>
        /// This implements an unselectable weapon to hold a slot free
        /// </summary>
        public class Kit_WeaponUnselectable : Kit_WeaponBase
        {
            public override float AimInTime(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return 1f;
            }

            public override void AnimateWeapon(Kit_PlayerBehaviour pb, object runtimeData, int id, float speed)
            {

            }

            public override void BeginSpectating(Kit_PlayerBehaviour pb, object runtimeData, int[] attachments)
            {

            }

            public override void CalculateWeaponUpdate(Kit_PlayerBehaviour pb, object runtimeData)
            {

            }

            public override void DrawWeapon(Kit_PlayerBehaviour pb, object runtimeData)
            {

            }

            public override void EndSpectating(Kit_PlayerBehaviour pb, object runtimeData)
            {

            }

            public override void FallDownEffect(Kit_PlayerBehaviour pb, object runtimeData, bool wasFallDamageApplied)
            {

            }

            public override void FirstThirdPersonChanged(Kit_PlayerBehaviour pb, Kit_GameInformation.Perspective perspective, object runtimeData)
            {

            }

            public override bool ForceIntoFirstPerson(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return false;
            }

            public override WeaponIKValues GetIK(Kit_PlayerBehaviour pb, Animator anim, object runtimeData)
            {
                return new WeaponIKValues();
            }

            public override WeaponStats GetStats()
            {
                return new WeaponStats();
            }

            public override int GetWeaponType(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return -1;
            }

            public override bool IsWeaponAiming(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return false;
            }

            public override void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info, object runtimeData)
            {

            }

            public override void PutawayWeapon(Kit_PlayerBehaviour pb, object runtimeData)
            {

            }

            public override void PutawayWeaponHide(Kit_PlayerBehaviour pb, object runtimeData)
            {

            }

            public override float Sensitivity(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return Kit_GameSettings.hipSensitivity;
            }

            public override object SetupFirstPerson(Kit_PlayerBehaviour pb, int[] attachments)
            {
                return new UnselectableWeaponRuntimeData();
            }

            public override void SetupThirdPerson(Kit_PlayerBehaviour pb, object runtimeData, int[] attachments)
            {

            }

            public override void SetupValues(int id)
            {

            }

            public override float SpeedMultiplier(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return 1f;
            }

            public override int WeaponState(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return 2;
            }

            public override bool CanBeSelected(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return false;
            }

            public override bool CanBeSelectedInLoadout()
            {
                return false;
            }
        }
    }
}