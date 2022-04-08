using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarsFPSKit
{
    namespace Weapons
    {
        public class WeaponControllerRuntimeData
        {
            public Kit_WeaponRenderer weaponRenderer;
            public Kit_ThirdPersonWeaponRenderer tpWeaponRenderer;
            public List<GameObject> instantiatedObjects = new List<GameObject>(); //Objects that were instantiated by this weapon. When it is replaced, they have to be cleaned up first.

            /// <summary>
            /// Is this weapon out and ready to shoot?
            /// </summary>
            public bool isSelectedAndReady = false;
            /// <summary>
            /// Is the weapon selected?
            /// </summary>
            public bool isSelected;

            public Animator genericAnimator;

            #region Stats
            /// <summary>
            /// Are we currently firing full auto?
            /// </summary>
            public bool isFiring;
            /// <summary>
            /// When did we fire for the last time? Compare with <see cref="Time.time"/>
            /// </summary>
            public float lastFire;
            /// <summary>
            /// How many bullets are left in the magazine?
            /// </summary>
            public int bulletsLeft;
            /// <summary>
            /// How many bullets do we have left to reload
            /// </summary>
            public int bulletsLeftToReload;

            /// <summary>
            /// When did we ran the last time (using this weapon)
            /// </summary>
            public float lastRun;
            #endregion

            #region Reload
            /// <summary>
            /// Are we currently reloading?
            /// </summary>
            public bool reloadInProgress;
            /// <summary>
            /// When is the next reloading phase over?
            /// </summary>
            public float reloadNextEnd; //This is only so we don't have to use a coroutine
            /// <summary>
            /// The current phase of reloading
            /// </summary>
            public int reloadPhase;
            #endregion

            #region Procedural Reload
            /// <summary>
            /// Set to true if the player attemps to fire during the reload
            /// </summary>
            public bool cancelProceduralReload = false;
            #endregion

            #region Aiming
            /// <summary>
            /// Are we currently aiming?
            /// </summary>
            public bool isAiming;

            /// <summary>
            /// So we can see if it changed, based on bools only
            /// </summary>
            public bool lastIsAiming;

            /// <summary>
            /// Float between 0 and 1 that indicates how far we have aimed in
            /// </summary>
            public float aimingProgress;

            /// <summary>
            /// Are we aimed in?
            /// </summary>
            public bool isAimedIn
            {
                get
                {
                    return Mathf.Approximately(aimingProgress, 1f);
                }
            }

            /// <summary>
            /// If we are using the sniper scope, this helps us to only hide the weapon once
            /// </summary>
            public bool sniperWeaponHidden;

            /// <summary>
            /// Transform used to move the weapon to aiming position
            /// </summary>
            public Transform aimingTransform;
            #endregion

            #region Weapon Delay
            /// <summary>
            /// The transform to apply our delay effect to
            /// </summary>
            public Transform weaponDelayTransform;
            /// <summary>
            /// Current weapon delay target
            /// </summary>
            public Vector3 weaponDelayCur;
            /// <summary>
            /// Current Mouse X input for weapon delay
            /// </summary>
            public float weaponDelayCurrentX;
            /// <summary>
            /// Current Mouse Y input for weapon delay
            /// </summary>
            public float weaponDelayCurrentY;
            #endregion

            #region Weapon Fall
            public Transform weaponFallTransform;
            #endregion

            #region Shell Ejection
            /// <summary>
            /// Should we check if we should eject a shell?
            /// </summary>
            public bool shellEjectEnabled = false;
            /// <summary>
            /// At which point in time are we going to eject the shell?
            /// </summary>
            public float shellEjectNext;
            #endregion

            #region  Bolt Action
            /// <summary>
            /// 0 = Nothing; 1 = Eject next; 2 = Eject next (last shot)
            /// </summary>
            public int boltActionState;
            /// <summary>
            /// When is the eject definied in <see cref="boltActionState"/> going to happen?
            /// </summary>
            public float boltActionTime;
            #endregion

            /// <summary>
            /// Which animation was played the last time? Used to only call CrossFade once so it transitions correctly.
            /// </summary>
            public int lastWeaponAnimationID;

            #region Spray Pattern
            /// <summary>
            /// Current state of the spray pattern
            /// </summary>
            public float sprayPatternState = 0f;
            #endregion

            #region Run Animation
            /// <summary>
            /// Is the running animation (using non generic mecanim) currently playing?
            /// </summary>
            public bool startedRunAnimation;
            #endregion

            #region Sound
            /// <summary>
            /// Audio source used for fire sounds
            /// </summary>
            public AudioSource soundFire;

            /// <summary>
            /// Audio Source used for reload sounds
            /// </summary>
            public AudioSource soundReload;

            /// <summary>
            /// Audio Source used for other sounds
            /// </summary>
            public AudioSource soundOther;
            #endregion

            #region Input
            public bool lastLmb;
            public bool lastRmb;
            public bool lastReload;
            #endregion

            #region Spring
            /// <summary>
            /// Positional spring
            /// </summary>
            public Kit_Spring springPos;
            /// <summary>
            /// Rotational spring
            /// </summary>
            public Kit_Spring springRot;
            #endregion

            //In order to modify the stats, we copy them here first
            #region Modifyable stats
            #region Settings
            [Header("Settings")]
            public FireMode fireMode = FireMode.Semi;
            public float fireRate = 0.1f;
            /// <summary>
            /// How many bullets do we have per mag
            /// </summary>
            public int bulletsPerMag = 30;
            /// <summary>
            /// With how many <see cref="WeaponControllerRuntimeData.bulletsLeftToReload"/> do we start?
            /// </summary>
            public int bulletsToReloadAtStart = 60;
            /// <summary>
            /// How much damage does this weapon deal?
            /// </summary>
            public float baseDamage = 30f;
            /// <summary>
            /// The max. range of this weapon
            /// </summary>
            public float range = 500f;
            /// <summary>
            /// This is the damage drop off relative to <see cref="range"/>. It should be a value between 0 and 1 that multiplies with <see cref="baseDamage"/>
            /// </summary>
            public AnimationCurve damageDropoff = AnimationCurve.Linear(0, 1, 500f, 0.8f);
            /// <summary>
            /// Should we shoot one bullet at a time or pellets?
            /// </summary>
            public FireTypeMode fireTypeMode = FireTypeMode.Simple;
            /// <summary>
            /// If we are firing using burst, how many bullets should be fired in one burst?
            /// </summary>
            public int burstBulletsPerShot = 3;
            /// <summary>
            /// How many seconds need to pass between individual shots in burst fire mode?
            /// </summary>
            public float burstTimeBetweenShots = 0.1f;
            /// <summary>
            /// If we should shoot pellets, how many should we shoot?
            /// </summary>
            public int amountOfPellets = 12;
            /// <summary>
            /// How much should the weapon slow us down (base)?  Other values are multiplier by this.
            /// </summary>
            public float speedMultiplierBase = 1f;
            /// <summary>
            /// How much force is applied to the ragdoll?
            /// </summary>
            public float ragdollForce = 500f;
            #endregion

            #region Bullets
            /// <summary>
            /// Determines whether we should use raycast or physical bullets
            /// </summary>
            public BulletMode bulletsMode = BulletMode.Raycast;
            /// <summary>
            /// Is bullet penetration enabled?
            /// </summary>
            public bool bulletsPenetrationEnabled = true;
            /// <summary>
            /// This is a value that determines how many objects can be penetrated with this bullet. Think of it as the money you have available to pay the penetration cost here <see cref="Kit_PenetrateableObject.cost"/>
            /// </summary>
            public int bulletsPenetrationForce = 3;
            /// <summary>
            /// If <see cref="bulletsMode"/> is set to <see cref="BulletMode.Physical"/>, this prefab will be instantiated instead of raycast
            /// </summary>
            public GameObject bulletPrefab;
            /// <summary>
            /// Speed of physical bullet in m/s
            /// </summary>
            public float bulletSpeed = 200;
            /// <summary>
            /// How many frames after shooting should the bullet be hidden?
            /// </summary>
            public int bulletHideForFrames = 3;
            /// <summary>
            /// Multiplier of gravity applied to bullet (1 = realistic). Multiplies with <see cref="Physics.gravity"/>
            /// </summary>
            public float bulletGravityMultiplier = 1f;
            /// <summary>
            /// How long will this weapon's bullets exist?
            /// </summary>
            public float bulletLifeTime = 10f;
            /// <summary>
            /// Should the bullet parent itself to its hit thing and stay alive? Useful for things like nails and arrows.
            /// </summary>
            public bool bulletStaysAliveAfterDeath = false;
            /// <summary>
            /// If bullet stays alive after death, this is how long
            /// </summary>
            public float bulletStaysAliveAfterDeathTime = 10f;
            #endregion

            #region Bullet Spread
            /// <summary>
            /// Which spray m ode does this weapon use?
            /// </summary>
            public SpreadMode bulletSpreadMode = SpreadMode.Simple;
            /// <summary>
            /// Base value for hip bullet spread
            /// </summary>
            public float bulletSpreadHipBase = 0.1f;
            /// <summary>
            /// How much spread should be added for our current velocity in reference to <see cref="bulletSpreadHipVelocityReference"/>
            /// </summary>
            public float bulletSpreadHipVelocityAdd = 0.1f;
            /// <summary>
            /// Reference velocity for hip spread
            /// </summary>
            public float bulletSpreadHipVelocityReference = 6f;

            /// <summary>
            /// Base value for aiming bullet spread
            /// </summary>
            public float bulletSpreadAimBase = 0.01f;
            /// <summary>
            /// How much spread should be added for our current velocity in reference to <see cref="bulletSpreadAimVelocityReference"/>
            /// </summary>
            public float bulletSpreadAimVelocityAdd = 0.02f;
            /// <summary>
            /// Reference velocity for aim spread
            /// </summary>
            public float bulletSpreadAimVelocityReference = 6f;

            //SPRAY PATTERN
            /// <summary>
            /// Offset values for the spray pattern mode
            /// </summary>
            public Vector3[] bulletSpreadSprayPattern = new Vector3[30];
            /// <summary>
            /// How fast does the spray pattern recover?
            /// </summary>
            public float bulletSpreadSprayPatternRecoverySpeed = 1f;
            //END
            #endregion

            #region Recoil
            /// <summary>
            /// Minimum amount of recoil per shot
            /// </summary>
            public Vector2 recoilPerShotMin = new Vector2 { x = -0.1f, y = 0.5f };
            /// <summary>
            /// Max amount of recoil per shot
            /// </summary>
            public Vector2 recoilPerShotMax = new Vector2 { x = 0.1f, y = 1f };
            /// <summary>
            /// How fast is our recoil going to be applied
            /// </summary>
            public float recoilApplyTime = 0.1f;
            /// <summary>
            /// How fast are we going to return to our normal position?
            /// </summary>
            public float recoilReturnSpeed = 6f;
            #endregion
            #endregion
        }

        #region enums
        public enum FireMode { Semi, Auto, Burst, BoltAction };
        public enum FireTypeMode { Simple, Pellets }
        public enum BulletMode { Raycast, Physical }
        public enum ReloadMode { Simple, FullEmpty, Chambered, Procedural, ProceduralChambered }
        public enum SpreadMode { Simple, SprayPattern }
        #endregion

        [CreateAssetMenu(menuName = ("MarsFPSKit/Weapons/Modern Weapon Script"))]
        public class Kit_ModernWeaponScript : Kit_WeaponBase
        {
            [Header("Spring")]
            /// <summary>
            /// Config for positional spring
            /// </summary>
            public Kit_Spring.SpringConfig springPosConfig;
            /// <summary>
            /// Config for rotational spring
            /// </summary>
            public Kit_Spring.SpringConfig springRotConfig;

            #region Settings
            [Header("Settings")]
            public FireMode fireMode = FireMode.Semi;
            /// <summary>
            /// How many rounds per minute does this weapon shoot?
            /// </summary>
            public int RPM = 600;
            private float fireRate = 0.1f;
            /// <summary>
            /// How many bullets do we have per mag
            /// </summary>
            public int bulletsPerMag = 30;
            /// <summary>
            /// With how many <see cref="WeaponControllerRuntimeData.bulletsLeftToReload"/> do we start?
            /// </summary>
            public int bulletsToReloadAtStart = 60;
            /// <summary>
            /// How much damage does this weapon deal?
            /// </summary>
            public float baseDamage = 30f;
            /// <summary>
            /// The max. range of this weapon
            /// </summary>
            public float range = 500f;
            /// <summary>
            /// This is the damage drop off relative to <see cref="range"/>. It should be a value between 0 and 1 that multiplies with <see cref="baseDamage"/>
            /// </summary>
            public AnimationCurve damageDropoff = AnimationCurve.Linear(0, 1, 500f, 0.8f);
            /// <summary>
            /// Should we shoot one bullet at a time or pellets?
            /// </summary>
            public FireTypeMode fireTypeMode = FireTypeMode.Simple;
            /// <summary>
            /// If we are firing using burst, how many bullets should be fired in one burst?
            /// </summary>
            public int burstBulletsPerShot = 3;
            /// <summary>
            /// How many seconds need to pass between individual shots in burst fire mode?
            /// </summary>
            public float burstTimeBetweenShots = 0.1f;
            /// <summary>
            /// If we should shoot pellets, how many should we shoot?
            /// </summary>
            public int amountOfPellets = 12;
            /// <summary>
            /// How much should the weapon slow us down (base)?  Other values are multiplier by this.
            /// </summary>
            public float speedMultiplierBase = 1f;
            /// <summary>
            /// How much force is applied to the ragdoll?
            /// </summary>
            public float ragdollForce = 500f;
            #endregion

            #region Attachments
            [FormerlySerializedAs("attachmentSlots")]
            /// <summary>
            /// Attachment slots available on this weapon
            /// </summary>
            public AttachmentSlot[] attachmentSlotsSpecific;

            /// <summary>
            /// If assigned, this list is used instead
            /// </summary>
            public Kit_AttachmentsUniversal attachmentSlotOverride;

            public AttachmentSlot[] attachmentSlots
            {
                get
                {
                    if (attachmentSlotOverride) return attachmentSlotOverride.attachmentSlots;
                    else return attachmentSlotsSpecific;
                }
            }
            #endregion

            #region Animation Settings
            /// <summary>
            /// Should the "empty" bool be set to the animator?
            /// </summary>
            public bool setEmptyBool;
            /// <summary>
            /// Should the "aiming" bool be set to the animator?
            /// </summary>
            public bool setAimingBool;
            /// <summary>
            /// Set "aiming" float to the animator?
            /// </summary>
            public bool setAimingProgress;
            /// <summary>
            /// Set actual movement direction to the animator? ("hor" / "ver")
            /// </summary>
            public bool setMovementDirection;
            #endregion

            #region Crosshair
            [Header("Crosshair")]
            /// <summary>
            /// Is the crosshair enabled for this weapon?
            /// </summary>
            public bool crosshairEnabled = true;
            /// <summary>
            /// By which factor should we multiply the crosshair's size?
            /// </summary>
            public float crosshairSizeMultiplier = 1f;
            #endregion

            #region Bullets
            [Header("Bullets")]
            /// <summary>
            /// Determines whether we should use raycast or physical bullets
            /// </summary>
            public BulletMode bulletsMode = BulletMode.Raycast;
            /// <summary>
            /// Is bullet penetration enabled?
            /// </summary>
            public bool bulletsPenetrationEnabled = true;
            /// <summary>
            /// This is a value that determines how many objects can be penetrated with this bullet. Think of it as the money you have available to pay the penetration cost here <see cref="Kit_PenetrateableObject.cost"/>
            /// </summary>
            public int bulletsPenetrationForce = 3;
            /// <summary>
            /// If <see cref="bulletsMode"/> is set to <see cref="BulletMode.Physical"/>, this prefab will be instantiated instead of raycast
            /// </summary>
            public GameObject bulletPrefab;
            /// <summary>
            /// Speed of physical bullet in m/s
            /// </summary>
            public float bulletSpeed = 200;
            /// <summary>
            /// How many frames after shooting should the bullet be hidden?
            /// </summary>
            public int bulletHideForFrames = 3;
            /// <summary>
            /// Multiplier of gravity applied to bullet (1 = realistic). Multiplies with <see cref="Physics.gravity"/>
            /// </summary>
            public float bulletGravityMultiplier = 1f;
            /// <summary>
            /// How long will this weapon's bullets exist?
            /// </summary>
            public float bulletLifeTime = 10f;
            /// <summary>
            /// Should the bullet parent itself to its hit thing and stay alive? Useful for things like nails and arrows.
            /// </summary>
            public bool bulletStaysAliveAfterDeath = false;
            /// <summary>
            /// If bullet stays alive after death, this is how long
            /// </summary>
            public float bulletStaysAliveAfterDeathTime = 10f;
            #endregion

            #region Bullet Spread
            [Header("Bullet Spread")]
            /// <summary>
            /// Which spray m ode does this weapon use?
            /// </summary>
            public SpreadMode bulletSpreadMode = SpreadMode.Simple;
            /// <summary>
            /// Base value for hip bullet spread
            /// </summary>
            public float bulletSpreadHipBase = 0.1f;
            /// <summary>
            /// How much spread should be added for our current velocity in reference to <see cref="bulletSpreadHipVelocityReference"/>
            /// </summary>
            public float bulletSpreadHipVelocityAdd = 0.1f;
            /// <summary>
            /// Reference velocity for hip spread
            /// </summary>
            public float bulletSpreadHipVelocityReference = 6f;

            /// <summary>
            /// Base value for aiming bullet spread
            /// </summary>
            public float bulletSpreadAimBase = 0.01f;
            /// <summary>
            /// How much spread should be added for our current velocity in reference to <see cref="bulletSpreadAimVelocityReference"/>
            /// </summary>
            public float bulletSpreadAimVelocityAdd = 0.02f;
            /// <summary>
            /// Reference velocity for aim spread
            /// </summary>
            public float bulletSpreadAimVelocityReference = 6f;

            //SPRAY PATTERN
            /// <summary>
            /// Offset values for the spray pattern mode
            /// </summary>
            public Vector3[] bulletSpreadSprayPattern = new Vector3[30];
            /// <summary>
            /// How fast does the spray pattern recover?
            /// </summary>
            public float bulletSpreadSprayPatternRecoverySpeed = 1f;
            //END
            #endregion

            #region Recoil
            [Header("Recoil")]
            /// <summary>
            /// Minimum amount of recoil per shot
            /// </summary>
            public Vector2 recoilPerShotMin = new Vector2 { x = -0.1f, y = 0.5f };
            /// <summary>
            /// Max amount of recoil per shot
            /// </summary>
            public Vector2 recoilPerShotMax = new Vector2 { x = 0.1f, y = 1f };
            /// <summary>
            /// How fast is our recoil going to be applied
            /// </summary>
            public float recoilApplyTime = 0.1f;
            /// <summary>
            /// How fast are we going to return to our normal position?
            /// </summary>
            public float recoilReturnSpeed = 6f;
            #endregion

            #region Reload
            [Header("Reload")]
            public ReloadMode reloadMode = ReloadMode.Chambered;
            public bool reloadProceduralAddBulletDuringStartEmpty;
            /// <summary>
            /// Reload time for normal reload (simple / full)
            /// </summary>
            public float reloadTimeOne, reloadTimeTwo; //Reload is split up in two parts, first is till bullets are updated, second is till you can shoot again
            /// <summary>
            /// Reload time for empty reload, if <see cref="reloadMode"/> is set to <see cref="ReloadMode.FullEmpty"/> or <see cref="ReloadMode.Chambered"/>
            /// </summary>
            public float reloadEmptyTimeOne, reloadEmptyTimeTwo;
            /// <summary>
            /// How much time does it take to start the procedural reload when the gun is still loaded?
            /// </summary>
            public float reloadProceduralStartTime;
            /// <summary>
            /// How much time does it take to start the reload when the gun is empty (and no bullet is added as set in <see cref="reloadProceduralAddBulletDuringStartEmpty"/>
            /// </summary>
            public float reloadProceduralEmptyStartTime;
            /// <summary>
            /// How much time until the bullet gets added (empty reload start + adding bullet during animation)
            /// </summary>
            public float reloadProceduralEmptyInsertStartTimeOne;
            /// <summary>
            /// How much time left until the loop can start?
            /// </summary>
            public float reloadProceduralEmptyInsertStartTimeTwo;
            /// <summary>
            /// How much time before the bullet gets added?
            /// </summary>
            public float reloadProceduralInsertTimeOne;
            /// <summary>
            /// How much time needs to pass after the bullet was added
            /// </summary>
            public float reloadProceduralInsertTimeTwo;
            /// <summary>
            /// How long does it take to end the procedural reload?
            /// </summary>
            public float reloadProceduralEndTime;

            /// <summary>
            /// Reloading full sound
            /// </summary>
            public AudioClip reloadSound;
            /// <summary>
            /// Reloading empty sound
            /// </summary>
            public AudioClip reloadSoundEmpty;
            /// <summary>
            /// Max sound distance for third person reload
            /// </summary>
            public float reloadSoundThirdPersonMaxRange = 20;
            /// <summary>
            /// Sound rolloff for third person reload
            /// </summary>
            public AnimationCurve reloadSoundThirdPersonRolloff = AnimationCurve.EaseInOut(0f, 1f, 20f, 0f);
            /// <summary>
            /// Sound that plays when the procedural reload starts while there is still ammo left in the gun
            /// </summary>
            public AudioClip reloadProceduralStartSound;
            /// <summary>
            /// Sound that plays when the procedural reload starts while the gun is completely empty
            /// </summary>
            public AudioClip reloadProceduralStartEmptySound;
            /// <summary>
            /// Sound that plays when a bullet is inserted during the procedural reload
            /// </summary>
            public AudioClip reloadProceduralInsertSound;
            /// <summary>
            /// Sound that plays when the procedural reload ends
            /// </summary>
            public AudioClip reloadProceduralEndSound;
            #endregion

            #region Aiming
            /// <summary>
            /// Can we aim?
            /// </summary>
            [Header("Aiming")]
            public bool aimEnabled = true;
            /// <summary>
            /// How fast (in seconds) should we go from idle position to aiming?
            /// </summary>
            public float aimInTime = 0.5f;
            /// <summary>
            /// How fast (in seconds) should we return from aiming back to idle position?
            /// </summary>
            public float aimOutTime = 0.5f;
            /// <summary>
            /// How much should aiming slow us down?
            /// </summary>
            public float aimSpeedMultiplier = 0.8f;
            #endregion

            #region Weapon Delay
            [Header("Weapon Delay")]
            /// <summary>
            /// Base amount for weapon delay
            /// </summary>
            public float weaponDelayBaseAmount = 1f;
            /// <summary>
            /// Max amount for weapon delay
            /// </summary>
            public float weaponDelayMaxAmount = 0.02f;
            /// <summary>
            /// Multiplier that is applied when we are aiming
            /// </summary>
            public float weaponDelayAimingMultiplier = 0.3f;
            /// <summary>
            /// How fast does the weapon delay update?
            /// </summary>
            public float weaponDelaySmooth = 3f;
            #endregion

            #region Weapon Tilt
            /// <summary>
            /// Should the weapon tilt sideways when we are walking sideways?
            /// </summary>
            public bool weaponTiltEnabled = true;
            /// <summary>
            /// Should the weapon also tilt when we are aiming?
            /// </summary>
            public bool weaponTiltEnabledWhileAiming;
            /// <summary>
            /// By how many degrees should the weapon tilt?
            /// </summary>
            public float weaponTiltIntensity = 5f;
            /// <summary>
            /// How fast should it return to 0,0,0 when weapon tilt is disabled?
            /// </summary>
            public float weaponTiltReturnSpeed = 3f;
            #endregion

            #region Weapon Fall
            [Header("Fall Down effect")]
            public float fallDownAmount = 10.0f;
            public float fallDownMinOffset = -6.0f;
            public float fallDownMaxoffset = 6.0f;
            public float fallDownTime = 0.1f;
            public float fallDownReturnSpeed = 1f;
            #endregion

            #region Shell Ejection
            [Header("Shell Ejection")]
            /// <summary>
            /// Our shell ejection prefab
            /// </summary>
            public GameObject shellEjectionPrefab;
            /// <summary>
            /// Should there be a delay between the shot and the shell ejection? Must be lower than <see cref="fireRate"/>.
            /// </summary>
            public float shellEjectionTime = 0f;
            /// <summary>
            /// The minimum force applied (relative to the ejection point)
            /// </summary>
            public Vector3 shellEjectionMinForce;
            /// <summary>
            /// The maximum force applied (relative to the ejection point)
            /// </summary>
            public Vector3 shellEjectionMaxForce;
            /// <summary>
            /// The minimum torque applied (relative to the ejection point)
            /// </summary>
            public Vector3 shellEjectionMinTorque;
            /// <summary>
            /// The maximum torque applied (relative to the ejection point)
            /// </summary>
            public Vector3 shellEjectionMaxTorque;
            #endregion

            #region Running
            [Header("Running")]
            /// <summary>
            /// Time in seconds that you cannot fire after you stopped running
            /// </summary>
            public float timeCannotFireAfterRun = 0.3f;
            #endregion

            #region Sounds
            [Header("Sounds")]
            /// <summary>
            /// Sound used for draw
            /// </summary>
            public AudioClip drawSound;
            /// <summary>
            /// Sound used for putaway
            /// </summary>
            public AudioClip putawaySound;
            /// <summary>
            /// Fire sound used for first person
            /// </summary>
            public AudioClip fireSound;
            /// <summary>
            /// Fire sound used for third person
            /// </summary>
            public AudioClip fireSoundThirdPerson;
            /// <summary>
            /// Delay for <see cref="boltActionSoundNormal"/> if this is a normal shot
            /// </summary>
            public float boltActionDelayNormal;
            /// <summary>
            /// Sound that plays for <see cref="FireMode.BoltAction"/> if this is a normal shot
            /// </summary>
            public AudioClip boltActionSoundNormal;
            /// <summary>
            /// Total time of bolt action if it is a normal shot
            /// </summary>
            public float boltActionTimeNormal = 0.3f;
            /// <summary>
            /// Delay for <see cref="boltActionSoundLast"/> if this is a normal shot
            /// </summary>
            public float boltActionDelayLast;
            /// <summary>
            /// Sound that plays for <see cref="FireMode.BoltAction"/> if this is the last shot
            /// </summary>
            public AudioClip boltActionSoundLast;
            /// <summary>
            /// Total time of bolt action if it is the last shot
            /// </summary>
            public float boltActionTimeLast = 0.3f;
            /// <summary>
            /// Max sound distance for third person bolt sound
            /// </summary>
            public float boltActionSoundThirdPersonMaxRange = 20f;
            /// <summary>
            /// Sound rolloff for third person bolt sound
            /// </summary>
            public AnimationCurve boltActionSoundThirdPersonRolloff = AnimationCurve.EaseInOut(0f, 1f, 20f, 0f);
            /// <summary>
            /// Max sound distance for third person fire
            /// </summary>
            public float fireSoundThirdPersonMaxRange = 300f;
            /// <summary>
            /// Sound rolloff for third person fire
            /// </summary>
            public AnimationCurve fireSoundThirdPersonRolloff = AnimationCurve.EaseInOut(0f, 1f, 300f, 0f);

            /// <summary>
            /// Dry fire sound (Shooting with 0 bullets left)
            /// </summary>
            public AudioClip dryFireSound;
            #endregion

            #region Walk Animations
            /// <summary>
            /// Should the walk animation still play while we are aiming?
            /// </summary>
            [Header("Walking")]
            public bool enableWalkAnimationWhileAiming;
            #endregion

            #region Generic Animations
            [Header("Generic Animations")]
            /// <summary>
            /// This animation controller holds the animations for generic gun movement (Idle, Walk, Run)
            /// </summary>
            public GameObject genericGunAnimatorControllerPrefab;

            /// <summary>
            /// Uses the generic walk anim if true
            /// </summary>
            public bool useGenericWalkAnim = true;

            /// <summary>
            /// Uses the generic run anim if true
            /// </summary>
            public bool useGenericRunAnim = true;
            #endregion

            //Information set at runtime
            /// <summary>
            /// The ID that this weapon has
            /// </summary>
            [HideInInspector]
            public int gameGunID;


            #region Overriden functions
            public override WeaponDisplayData GetWeaponDisplayData(Kit_PlayerBehaviour pb, object runtimeData)
            {
                WeaponDisplayData wdd = new WeaponDisplayData();
                wdd.sprite = weaponHudPicture;
                wdd.name = weaponName;
                return wdd;
            }

            public override void SetupValues(int id)
            {
                //Get our ID
                gameGunID = id;
                //Set RPM
                fireRate = 60f / RPM;
            }

            public override object SetupFirstPerson(Kit_PlayerBehaviour pb, int[] attachments)
            {
                WeaponControllerRuntimeData data = new WeaponControllerRuntimeData();

                //Setup root for this weapon
                GameObject root = new GameObject("Weapon root");
                root.transform.parent = pb.weaponsGo; //Set root
                root.transform.localPosition = Vector3.zero; //Reset position
                root.transform.localRotation = Quaternion.identity; //Reset rotation
                root.transform.localScale = Vector3.one; //Reset scale

                //Setup generic animations
                GameObject genericAnimations = Instantiate(genericGunAnimatorControllerPrefab);
                genericAnimations.transform.parent = root.transform;
                genericAnimations.transform.localPosition = Vector3.zero; //Reset position
                genericAnimations.transform.localRotation = Quaternion.identity; //Reset rotation
                genericAnimations.transform.localScale = Vector3.one; //Reset scale

                //Get animator
                Animator anim = genericAnimations.GetComponent<Animator>(); ;
                anim.Play("Idle");
                data.genericAnimator = anim;

                //Setup aiming transform
                GameObject aimTrans = new GameObject("Weapon aiming");
                aimTrans.transform.parent = genericAnimations.transform; //Set root
                aimTrans.transform.localPosition = Vector3.zero; //Reset position
                aimTrans.transform.localRotation = Quaternion.identity; //Reset rotation
                aimTrans.transform.localScale = Vector3.one; //Reset scale

                //Assign it
                data.aimingTransform = aimTrans.transform;

                //Delay transform
                GameObject delayTrans = new GameObject("Weapon delay");
                delayTrans.transform.parent = aimTrans.transform; //Set root
                delayTrans.transform.localPosition = Vector3.zero; //Reset position
                delayTrans.transform.localRotation = Quaternion.identity; //Reset rotation
                delayTrans.transform.localScale = Vector3.one; //Reset scale

                //Assign it
                data.weaponDelayTransform = delayTrans.transform;

                //Delay transform
                GameObject fallTrans = new GameObject("Weapon fall");
                fallTrans.transform.parent = delayTrans.transform; //Set root
                fallTrans.transform.localPosition = Vector3.zero; //Reset position
                fallTrans.transform.localRotation = Quaternion.identity; //Reset rotation
                fallTrans.transform.localScale = Vector3.one; //Reset scale

                //Assign it
                data.weaponFallTransform = fallTrans.transform;

                //Get Fire Audio (Needs to be consistent)
                if (pb.weaponsGo.GetComponent<AudioSource>()) data.soundFire = pb.weaponsGo.GetComponent<AudioSource>();
                else data.soundFire = pb.weaponsGo.gameObject.AddComponent<AudioSource>();

                //Setup reload sound
                GameObject soundReload = new GameObject("SoundReload"); //Create
                soundReload.transform.parent = root.transform;
                soundReload.transform.localPosition = Vector3.zero; //Reset position
                soundReload.transform.localRotation = Quaternion.identity; //Reset rotation
                soundReload.transform.localScale = Vector3.one; //Reset scale
                                                                //Add audio source
                data.soundReload = soundReload.AddComponent<AudioSource>();

                //Setup other sound
                GameObject soundOther = new GameObject("SoundOther"); //Create
                soundOther.transform.parent = root.transform;
                soundOther.transform.localPosition = Vector3.zero; //Reset position
                soundOther.transform.localRotation = Quaternion.identity; //Reset rotation
                soundOther.transform.localScale = Vector3.one; //Reset scale
                                                               //Add audio source
                data.soundOther = soundOther.AddComponent<AudioSource>();

                //Copy data first
                data.fireMode = fireMode;
                data.fireRate = 60f / RPM;
                data.bulletsPerMag = bulletsPerMag;
                data.bulletsToReloadAtStart = bulletsToReloadAtStart;
                data.baseDamage = baseDamage;
                data.range = range;
                data.damageDropoff = damageDropoff;
                data.fireTypeMode = fireTypeMode;
                data.burstBulletsPerShot = burstBulletsPerShot;
                data.burstTimeBetweenShots = burstTimeBetweenShots;
                data.amountOfPellets = amountOfPellets;
                data.speedMultiplierBase = speedMultiplierBase;
                data.ragdollForce = ragdollForce;

                data.bulletsMode = bulletsMode;
                data.bulletsPenetrationEnabled = bulletsPenetrationEnabled;
                data.bulletsPenetrationForce = bulletsPenetrationForce;
                data.bulletPrefab = bulletPrefab;
                data.bulletSpeed = bulletSpeed;
                data.bulletHideForFrames = bulletHideForFrames;
                data.bulletGravityMultiplier = bulletGravityMultiplier;
                data.bulletLifeTime = bulletLifeTime;
                data.bulletStaysAliveAfterDeath = bulletStaysAliveAfterDeath;
                data.bulletStaysAliveAfterDeathTime = bulletStaysAliveAfterDeathTime;

                data.bulletSpreadMode = bulletSpreadMode;
                data.bulletSpreadHipBase = bulletSpreadHipBase;
                data.bulletSpreadHipVelocityAdd = bulletSpreadHipVelocityAdd;
                data.bulletSpreadHipVelocityReference = bulletSpreadHipVelocityReference;
                data.bulletSpreadAimBase = bulletSpreadAimBase;
                data.bulletSpreadAimVelocityAdd = bulletSpreadAimVelocityAdd;
                data.bulletSpreadAimVelocityReference = bulletSpreadAimVelocityReference;

                data.bulletSpreadSprayPattern = bulletSpreadSprayPattern;
                data.bulletSpreadSprayPatternRecoverySpeed = bulletSpreadSprayPatternRecoverySpeed;

                data.recoilPerShotMin = recoilPerShotMin;
                data.recoilPerShotMax = recoilPerShotMax;
                data.recoilApplyTime = recoilApplyTime;
                data.recoilReturnSpeed = recoilReturnSpeed;

                //Setup the first person prefab
                GameObject fpRuntime = Instantiate(firstPersonPrefab, fallTrans.transform, false);
                fpRuntime.transform.localScale = Vector3.one; //Reset scale

                //Setup renderer
                data.weaponRenderer = fpRuntime.GetComponent<Kit_WeaponRenderer>();
                //Set Attachments
                data.weaponRenderer.SetAttachments(attachments, this, pb, data);

                //Play Dependent arms
                if (data.weaponRenderer.playerModelDependentArmsEnabled && pb.thirdPersonPlayerModel.firstPersonArmsPrefab.Contains(data.weaponRenderer.playerModelDependentArmsKey))
                {
                    //Create Prefab
                    GameObject fpArms = Instantiate(pb.thirdPersonPlayerModel.firstPersonArmsPrefab[data.weaponRenderer.playerModelDependentArmsKey], fallTrans.transform, false);
                    //Get Arms
                    Kit_FirstPersonArms fpa = fpArms.GetComponent<Kit_FirstPersonArms>();

                    if (fpa.cameraBoneOverride)
                    {
                        data.weaponRenderer.cameraAnimationBone = fpa.cameraBoneOverride;
                    }

                    if (fpa.cameraBoneTargetOverride)
                    {
                        data.weaponRenderer.cameraAnimationTarget = fpa.cameraBoneTargetOverride;
                    }

#if INTEGRATION_FPV2
                        //Set shaders
                        FirstPersonView.ShaderMaterialSolution.FPV_SM_Object armsObj = fpArms.GetComponent<FirstPersonView.ShaderMaterialSolution.FPV_SM_Object>();
                        if (armsObj)
                        {
                            //Set as first person object
                            armsObj.SetAsFirstPersonObject();
                        }
                        else
                        {
                            Debug.LogError("FPV2 is enabled but first person prefab does not have FPV_SM_Object assigned!");
                        }

#elif INTEGRATION_FPV3
                        //Set shaders
                        FirstPersonView.FPV_Object armsObj = fpArms.GetComponent<FirstPersonView.FPV_Object>();
                        if (armsObj)
                        {
                            //Set as first person object
                            armsObj.SetAsFirstPersonObject();
                        }
                        else
                        {
                            Debug.LogError("FPV3 is enabled but first person prefab does not have FPV_Object assigned!");
                        }
#endif

                    //Reparent
                    for (int i = 0; i < fpa.reparents.Length; i++)
                    {
                        if (fpa.reparents[i])
                        {
                            fpa.reparents[i].transform.parent = data.weaponRenderer.playerModelDependentArmsRoot;
                        }
                        else
                        {
                            Debug.LogWarning("First Person Arm Renderer at index " + i + " is not assigned", fpa);
                        }
                    }
                    //Merge Array
                    data.weaponRenderer.allWeaponRenderers = data.weaponRenderer.allWeaponRenderers.Concat(fpa.renderers).ToArray();
                    //Rebind so that the animator animates our freshly reparented transforms too!
                    if (data.weaponRenderer.anim)
                    {
                        data.weaponRenderer.anim.Rebind();

                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                        {
                            data.weaponRenderer.animAdditionals[i].Rebind();
                        }
                    }
                }

                //Hide
                data.weaponRenderer.visible = false;

#if INTEGRATION_FPV2
                    //Set shaders
                    FirstPersonView.ShaderMaterialSolution.FPV_SM_Object obj = fpRuntime.GetComponent<FirstPersonView.ShaderMaterialSolution.FPV_SM_Object>();
                    if (obj)
                    {
                        //Set as first person object
                        obj.SetAsFirstPersonObject();
                    }
                    else
                    {
                        Debug.LogError("FPV2 is enabled but first person prefab does not have FPV_SM_Object assigned!");
                    }
#elif INTEGRATION_FPV3
                    //Set shaders
                    FirstPersonView.FPV_Object obj = fpRuntime.GetComponent<FirstPersonView.FPV_Object>();
                    if (obj)
                    {
                        //Set as first person object
                        obj.SetAsFirstPersonObject();
                    }
                    else
                    {
                        Debug.LogError("FPV3 is enabled but first person prefab does not have FPV_Object assigned!");
                    }
#endif

                //Add to the list
                data.instantiatedObjects.Add(root);

                //Setup start values
                data.bulletsLeft = data.bulletsPerMag;
                data.bulletsLeftToReload = data.bulletsToReloadAtStart;

                //Object Pool the bullet
                pb.main.objectPooling.EnqueueInstantiateable(data.bulletPrefab, 50);
                //And shell
                if (shellEjectionPrefab)
                {
                    pb.main.objectPooling.EnqueueInstantiateable(shellEjectionPrefab, 50);
                }

                //Return runtime data
                return data;
            }

            public override void SetupThirdPerson(Kit_PlayerBehaviour pb, object runtimeData, int[] attachments)
            {
                WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;

                //Setup the third person prefab
                GameObject tpRuntime = Instantiate(thirdPersonPrefab, pb.thirdPersonPlayerModel.weaponsInHandsGo, false);
                //Set Scale
                tpRuntime.transform.localScale = Vector3.one;

                //Setup renderer
                data.tpWeaponRenderer = tpRuntime.GetComponent<Kit_ThirdPersonWeaponRenderer>();
                data.tpWeaponRenderer.visible = false;
                if (pb.isFirstPersonActive)
                {
                    //Make it shadows only
                    data.tpWeaponRenderer.shadowsOnly = true;
                }
                //Setup attachments
                data.tpWeaponRenderer.SetAttachments(attachments, this, pb, data);

                //Add to the list
                data.instantiatedObjects.Add(tpRuntime);

                //Object Pool the bullet
                pb.main.objectPooling.EnqueueInstantiateable(data.bulletPrefab, 50);
                //And shell
                if (shellEjectionPrefab)
                {
                    pb.main.objectPooling.EnqueueInstantiateable(shellEjectionPrefab, 50);
                }
            }

            public override void CalculateWeaponUpdate(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;

                    //Set this weapon to selected and ready (for other things)
                    data.isSelectedAndReady = true;

                    if (pb.photonView.IsMine)
                    {
                        //If we are not reloading
                        if (!data.reloadInProgress)
                        {
                            //Input
                            if (pb.enableInput)
                            {
                                //Check if movement and time allows firing
                                if (pb.movement.CanFire(pb) && Time.time - timeCannotFireAfterRun > data.lastRun)
                                {
                                    //Fire modes
                                    if (data.fireMode == FireMode.Semi)
                                    {
                                        //We cannot fire full auto, set value
                                        data.isFiring = false;
                                        //Check for input
                                        if (data.lastLmb != pb.input.lmb)
                                        {
                                            data.lastLmb = pb.input.lmb;
                                            if (pb.input.lmb)
                                            {
                                                //Compare with fire rate
                                                if (Time.time >= data.lastFire + data.fireRate)
                                                {
                                                    if (data.bulletsLeft > 0)
                                                    {
                                                        FireOneShot(pb, data);
                                                    }
                                                    else if (!pb.main.gameInformation.enableAutoReload || data.bulletsLeftToReload == 0) //No ammo, dry fire
                                                    {
                                                        DryFire(pb, data);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (data.fireMode == FireMode.BoltAction)
                                    {
                                        //We cannot fire full auto, set value
                                        data.isFiring = false;
                                        //Check for input
                                        if (data.lastLmb != pb.input.lmb)
                                        {
                                            data.lastLmb = pb.input.lmb;
                                            if (pb.input.lmb)
                                            {
                                                //In bolt action, lastFire already includes bolt time
                                                if (Time.time >= data.lastFire)
                                                {
                                                    if (data.bulletsLeft > 0)
                                                    {
                                                        FireOneShot(pb, data);
                                                    }
                                                    else if (!pb.main.gameInformation.enableAutoReload || data.bulletsLeftToReload == 0) //No ammo, dry fire
                                                    {
                                                        DryFire(pb, data);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (data.fireMode == FireMode.Auto)
                                    {
                                        //Check for input
                                        if (pb.input.lmb)
                                        {
                                            //Compare with fire rate
                                            if (Time.time >= data.lastFire + data.fireRate)
                                            {
                                                //Compare bullets left
                                                if (data.bulletsLeft > 0)
                                                {
                                                    FireOneShot(pb, data);
                                                    //We are firing full auto, set value
                                                    data.isFiring = true;
                                                }
                                                else if (!pb.main.gameInformation.enableAutoReload || data.bulletsLeftToReload == 0) //No ammo, dry fire
                                                {
                                                    DryFire(pb, data);
                                                    //We cannot fire, set value
                                                    data.isFiring = false;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //We cannot fire, set value
                                            data.isFiring = false;
                                        }
                                    }
                                    else if (data.fireMode == FireMode.Burst)
                                    {
                                        //We cannot fire full auto, set value
                                        data.isFiring = false;
                                        //Check for input
                                        if (data.lastLmb != pb.input.lmb)
                                        {
                                            data.lastLmb = pb.input.lmb;
                                            if (pb.input.lmb)
                                            {
                                                //Compare with fire rate
                                                if (Time.time >= data.lastFire + data.fireRate)
                                                {
                                                    if (data.bulletsLeft > 0)
                                                    {
                                                        FireBurst(pb, data);
                                                    }
                                                    else if (!pb.main.gameInformation.enableAutoReload || data.bulletsLeftToReload == 0) //No ammo, dry fire
                                                    {
                                                        DryFire(pb, data);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //We cannot fire, set value
                                    data.isFiring = false;
                                }

                                //Aiming input
                                //Hold
                                if (!Kit_GameSettings.isAimingToggle || pb.isBot)
                                {
                                    if (pb.movement.CanFire(pb) && aimEnabled)
                                    {
                                        //Check for input
                                        if (pb.input.rmb)
                                        {
                                            //Check if we can aim
                                            if (data.isSelectedAndReady && !data.reloadInProgress)
                                            {
                                                data.isAiming = true;
                                            }
                                            //Else we cannot aim, set to not aiming
                                            else
                                            {
                                                data.isAiming = false;
                                            }
                                        }
                                        //Else we cannot aim, set to not aiming
                                        else
                                        {
                                            data.isAiming = false;
                                        }
                                    }
                                    //Else we cannot aim, set to not aiming
                                    else
                                    {
                                        data.isAiming = false;
                                    }
                                }
                                //Toggle
                                else
                                {
                                    if (pb.movement.CanFire(pb) && aimEnabled)
                                    {
                                        //Check if we can aim
                                        if (data.isSelectedAndReady && !data.reloadInProgress)
                                        {
                                            //Check for input
                                            if (data.lastRmb != pb.input.rmb)
                                            {
                                                data.lastRmb = pb.input.rmb;
                                                if (pb.input.rmb)
                                                {
                                                    //Invert toggle
                                                    if (data.isAiming) data.isAiming = false;
                                                    else data.isAiming = true;
                                                }
                                            }
                                        }
                                        //Else we cannot aim, set to not aiming
                                        else
                                        {
                                            data.isAiming = false;
                                        }
                                    }
                                    //Else we cannot aim, set to not aiming
                                    else
                                    {
                                        data.isAiming = false;
                                    }
                                }

                                //Check for input
                                if (data.lastReload != pb.input.reload || pb.main.gameInformation.enableAutoReload && data.bulletsLeft == 0 && data.bulletsLeftToReload > 0 && Time.time > data.lastFire + 0.5f)
                                {
                                    data.lastReload = pb.input.reload;
                                    if (pb.input.reload || pb.main.gameInformation.enableAutoReload && data.bulletsLeft == 0 && data.bulletsLeftToReload > 0)
                                    {
                                        //Check if we can start reload
                                        if (data.fireMode != FireMode.BoltAction || data.fireMode == FireMode.BoltAction && Time.time > data.lastFire)
                                        {
                                            //Check if we have spare ammo
                                            if (data.bulletsLeftToReload > 0)
                                            {
                                                //Check if the weapon isn't already full (Chambered means we can have one in the chamber)
                                                if ((reloadMode == ReloadMode.Chambered || reloadMode == ReloadMode.ProceduralChambered) && data.bulletsLeft != data.bulletsPerMag + 1 || reloadMode != ReloadMode.Chambered && reloadMode != ReloadMode.ProceduralChambered && data.bulletsLeft != data.bulletsPerMag)
                                                {
                                                    data.reloadInProgress = true;
                                                    //Play reload voice
                                                    if (pb.voiceManager)
                                                    {
                                                        pb.voiceManager.Reloading(pb);
                                                    }
                                                    //Reset run animation
                                                    data.startedRunAnimation = false;
                                                    switch (reloadMode)
                                                    {
                                                        //Set reload time
                                                        case ReloadMode.Simple:
                                                            //Simple reload, set time
                                                            data.reloadNextEnd = Time.time + reloadTimeOne;
                                                            data.reloadPhase = 0;
                                                            if (pb.isFirstPersonActive)
                                                            {
                                                                if (data.weaponRenderer.anim)
                                                                {
                                                                    //Play animation
                                                                    data.weaponRenderer.anim.Play("Reload Full", data.weaponRenderer.animActionLayer, 0f);

                                                                    for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                                    {
                                                                        data.weaponRenderer.animAdditionals[i].Play("Reload Full", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                                    }
                                                                }
                                                                else if (data.weaponRenderer.legacyAnim)
                                                                {
                                                                    //Play animation
                                                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.reload, 0.15f, PlayMode.StopAll);
                                                                }
                                                                //Play sound
                                                                data.soundReload.PlayOneShot(reloadSound);
                                                            }
                                                            else
                                                            {
                                                                //Set clip
                                                                pb.thirdPersonPlayerModel.soundReload.clip = reloadSound;
                                                                //Set distance
                                                                pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                                //Set rolloff
                                                                pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                                //Play
                                                                pb.thirdPersonPlayerModel.soundReload.Play();
                                                            }
                                                            //Call network
                                                            pb.photonView.RPC("WeaponReloadNetwork", RpcTarget.Others, false);
                                                            //Play third person reload anim
                                                            pb.thirdPersonPlayerModel.PlayWeaponReloadAnimation(thirdPersonAnimType, 0);
                                                            break;

                                                        case ReloadMode.FullEmpty:
                                                            //Set time for full
                                                            if (data.bulletsLeft > 0)
                                                            {
                                                                //Set time
                                                                data.reloadNextEnd = Time.time + reloadTimeOne;
                                                                data.reloadPhase = 0;
                                                                if (pb.isFirstPersonActive)
                                                                {
                                                                    //Play animation
                                                                    if (data.weaponRenderer.anim)
                                                                    {
                                                                        //Play animation
                                                                        data.weaponRenderer.anim.Play("Reload Full", data.weaponRenderer.animActionLayer, 0f);

                                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                                        {
                                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Full", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                                        }
                                                                    }
                                                                    else if (data.weaponRenderer.legacyAnim)
                                                                    {
                                                                        //Play animation
                                                                        data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.reload, 0.15f, PlayMode.StopAll);
                                                                    }
                                                                    //Play sound
                                                                    data.soundReload.PlayOneShot(reloadSound);
                                                                }
                                                                else
                                                                {
                                                                    //Set clip
                                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadSound;
                                                                    //Set distance
                                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                                    //Set rolloff
                                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                                    //Play
                                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                                }
                                                                //Call network
                                                                pb.photonView.RPC("WeaponReloadNetwork", RpcTarget.Others, false);
                                                                //Play third person reload anim
                                                                pb.thirdPersonPlayerModel.PlayWeaponReloadAnimation(thirdPersonAnimType, 0);
                                                            }
                                                            //Empty
                                                            else
                                                            {
                                                                //Set time
                                                                data.reloadNextEnd = Time.time + reloadEmptyTimeOne;
                                                                data.reloadPhase = 0;
                                                                if (pb.isFirstPersonActive)
                                                                {
                                                                    //Play animation
                                                                    if (data.weaponRenderer.anim)
                                                                    {
                                                                        //Play animation
                                                                        data.weaponRenderer.anim.Play("Reload Empty", data.weaponRenderer.animActionLayer, 0f);

                                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                                        {
                                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Empty", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                                        }
                                                                    }
                                                                    else if (data.weaponRenderer.legacyAnim)
                                                                    {
                                                                        //Play animation
                                                                        data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.reloadEmpty, 0.15f, PlayMode.StopAll);
                                                                    }
                                                                    //Play sound
                                                                    data.soundReload.PlayOneShot(reloadSoundEmpty);
                                                                }
                                                                else
                                                                {
                                                                    //Play reload sound
                                                                    //Set clip
                                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadSoundEmpty;
                                                                    //Set distance
                                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                                    //Set rolloff
                                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                                    //Play
                                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                                }
                                                                //Call network
                                                                pb.photonView.RPC("WeaponReloadNetwork", RpcTarget.Others, true);
                                                                //Play third person reload anim
                                                                pb.thirdPersonPlayerModel.PlayWeaponReloadAnimation(thirdPersonAnimType, 0);
                                                            }
                                                            break;
                                                        case ReloadMode.Chambered:
                                                            //Set time for full
                                                            if (data.bulletsLeft > 0)
                                                            {
                                                                //Set time
                                                                data.reloadNextEnd = Time.time + reloadTimeOne;
                                                                data.reloadPhase = 0;
                                                                if (pb.isFirstPersonActive)
                                                                {
                                                                    //Play animation
                                                                    if (data.weaponRenderer.anim)
                                                                    {
                                                                        //Play animation
                                                                        data.weaponRenderer.anim.Play("Reload Full", data.weaponRenderer.animActionLayer, 0f);

                                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                                        {
                                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Full", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                                        }
                                                                    }
                                                                    else if (data.weaponRenderer.legacyAnim)
                                                                    {
                                                                        //Play animation
                                                                        data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.reload, 0.15f, PlayMode.StopAll);
                                                                    }
                                                                    //Play sound
                                                                    data.soundReload.PlayOneShot(reloadSound);
                                                                }
                                                                else
                                                                {
                                                                    //Set clip
                                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadSound;
                                                                    //Set distance
                                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                                    //Set rolloff
                                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                                    //Play
                                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                                }
                                                                //Call network
                                                                pb.photonView.RPC("WeaponReloadNetwork", RpcTarget.Others, false);
                                                                //Play third person reload anim
                                                                pb.thirdPersonPlayerModel.PlayWeaponReloadAnimation(thirdPersonAnimType, 0);
                                                            }
                                                            //Empty
                                                            else
                                                            {
                                                                //Set time
                                                                data.reloadNextEnd = Time.time + reloadEmptyTimeOne;
                                                                data.reloadPhase = 0;
                                                                if (pb.isFirstPersonActive)
                                                                {
                                                                    //Play animation
                                                                    if (data.weaponRenderer.anim)
                                                                    {
                                                                        //Play animation
                                                                        data.weaponRenderer.anim.Play("Reload Empty", data.weaponRenderer.animActionLayer, 0f);

                                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                                        {
                                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Empty", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                                        }
                                                                    }
                                                                    else if (data.weaponRenderer.legacyAnim)
                                                                    {
                                                                        //Play animation
                                                                        data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.reloadEmpty, 0.15f, PlayMode.StopAll);
                                                                    }
                                                                    //Play sound
                                                                    data.soundReload.PlayOneShot(reloadSoundEmpty);
                                                                }
                                                                else
                                                                {
                                                                    //Play reload sound
                                                                    //Set clip
                                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadSoundEmpty;
                                                                    //Set distance
                                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                                    //Set rolloff
                                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                                    //Play
                                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                                }
                                                                //Call network
                                                                pb.photonView.RPC("WeaponReloadNetwork", RpcTarget.Others, true);
                                                                //Play third person reload anim
                                                                pb.thirdPersonPlayerModel.PlayWeaponReloadAnimation(thirdPersonAnimType, 0);
                                                            }
                                                            break;
                                                        case ReloadMode.Procedural:
                                                            if (data.bulletsLeft > 0)
                                                            {
                                                                //Set phase
                                                                data.reloadPhase = 0;
                                                                //Set time
                                                                data.reloadNextEnd = Time.time + reloadProceduralStartTime;
                                                                if (pb.isFirstPersonActive)
                                                                {
                                                                    //Play animation
                                                                    if (data.weaponRenderer.anim)
                                                                    {
                                                                        //Play animation
                                                                        data.weaponRenderer.anim.Play("Reload Procedural Start", data.weaponRenderer.animActionLayer, 0f);

                                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                                        {
                                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Procedural Start", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                                        }
                                                                    }
                                                                    else if (data.weaponRenderer.legacyAnim)
                                                                    {
                                                                        //Play animation
                                                                        data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.reloadProceduralStart, 0.15f, PlayMode.StopAll);
                                                                    }
                                                                    //Play sound
                                                                    data.soundReload.PlayOneShot(reloadProceduralStartSound);
                                                                }
                                                                else
                                                                {
                                                                    //Play reload sound
                                                                    //Set clip
                                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralStartSound;
                                                                    //Set distance
                                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                                    //Set rolloff
                                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                                    //Play
                                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                                }
                                                                //Set cancel state
                                                                data.cancelProceduralReload = false;
                                                                //Call network
                                                                pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 0);
                                                                //Set cancel state
                                                                data.cancelProceduralReload = false;
                                                            }
                                                            else
                                                            {
                                                                //Set phase
                                                                data.reloadPhase = 0;
                                                                //Set time
                                                                if (reloadProceduralAddBulletDuringStartEmpty && data.bulletsLeft == 0)
                                                                {
                                                                    data.reloadNextEnd = Time.time + reloadProceduralEmptyInsertStartTimeOne;
                                                                }
                                                                else
                                                                {
                                                                    data.reloadNextEnd = Time.time + reloadProceduralEmptyStartTime;
                                                                }
                                                                if (pb.isFirstPersonActive)
                                                                {
                                                                    if (data.weaponRenderer.anim)
                                                                    {
                                                                        //Play animation
                                                                        data.weaponRenderer.anim.Play("Reload Procedural Start Empty", data.weaponRenderer.animActionLayer, 0f);

                                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                                        {
                                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Procedural Start Empty", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                                        }
                                                                    }
                                                                    else if (data.weaponRenderer.legacyAnim)
                                                                    {
                                                                        //Play animation
                                                                        data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.reloadProceduralStartEmpty, 0.15f, PlayMode.StopAll);
                                                                    }
                                                                    //Play sound
                                                                    data.soundReload.PlayOneShot(reloadProceduralStartEmptySound);
                                                                }
                                                                else
                                                                {
                                                                    //Play reload sound
                                                                    //Set clip
                                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralStartEmptySound;
                                                                    //Set distance
                                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                                    //Set rolloff
                                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                                    //Play
                                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                                }
                                                                //Set cancel state
                                                                data.cancelProceduralReload = false;
                                                                //Call network
                                                                pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 1);
                                                            }
                                                            break;
                                                        case ReloadMode.ProceduralChambered:
                                                            if (data.bulletsLeft > 0)
                                                            {
                                                                //Set phase
                                                                data.reloadPhase = 0;
                                                                //Set time
                                                                data.reloadNextEnd = Time.time + reloadProceduralStartTime;
                                                                if (pb.isFirstPersonActive)
                                                                {
                                                                    //Play animation
                                                                    if (data.weaponRenderer.anim)
                                                                    {
                                                                        //Play animation
                                                                        data.weaponRenderer.anim.Play("Reload Procedural Start", data.weaponRenderer.animActionLayer, 0f);

                                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                                        {
                                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Procedural Start", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                                        }
                                                                    }
                                                                    else if (data.weaponRenderer.legacyAnim)
                                                                    {
                                                                        //Play animation
                                                                        data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.reloadProceduralStart, 0.15f, PlayMode.StopAll);
                                                                    }
                                                                    //Play sound
                                                                    data.soundReload.PlayOneShot(reloadProceduralStartSound);
                                                                }
                                                                else
                                                                {
                                                                    //Play reload sound
                                                                    //Set clip
                                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralStartSound;
                                                                    //Set distance
                                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                                    //Set rolloff
                                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                                    //Play
                                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                                }
                                                                //Set cancel state
                                                                data.cancelProceduralReload = false;
                                                                //Call network
                                                                pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 0);
                                                                //Set cancel state
                                                                data.cancelProceduralReload = false;
                                                            }
                                                            else
                                                            {
                                                                //Set phase
                                                                data.reloadPhase = 0;
                                                                //Set time
                                                                if (reloadProceduralAddBulletDuringStartEmpty && data.bulletsLeft == 0)
                                                                {
                                                                    data.reloadNextEnd = Time.time + reloadProceduralEmptyInsertStartTimeOne;
                                                                }
                                                                else
                                                                {
                                                                    data.reloadNextEnd = Time.time + reloadProceduralEmptyStartTime;
                                                                }
                                                                if (pb.isFirstPersonActive)
                                                                {
                                                                    if (data.weaponRenderer.anim)
                                                                    {
                                                                        //Play animation
                                                                        data.weaponRenderer.anim.Play("Reload Procedural Start Empty", data.weaponRenderer.animActionLayer, 0f);

                                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                                        {
                                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Procedural Start Empty", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                                        }
                                                                    }
                                                                    else if (data.weaponRenderer.legacyAnim)
                                                                    {
                                                                        //Play animation
                                                                        data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.reloadProceduralStartEmpty, 0.15f, PlayMode.StopAll);
                                                                    }
                                                                    //Play sound
                                                                    data.soundReload.PlayOneShot(reloadProceduralStartEmptySound);
                                                                }
                                                                else
                                                                {
                                                                    //Play reload sound
                                                                    //Set clip
                                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralStartEmptySound;
                                                                    //Set distance
                                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                                    //Set rolloff
                                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                                    //Play
                                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                                }
                                                                //Set cancel state
                                                                data.cancelProceduralReload = false;
                                                                //Call network
                                                                pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 1);
                                                            }
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if (data != null && data.weaponRenderer && data.weaponRenderer.cachedInteractionAttachments != null)
                                {
                                    //Attachment interaction, only when we can control.
                                    for (int i = 0; i < data.weaponRenderer.cachedInteractionAttachments.Length; i++)
                                    {
                                        data.weaponRenderer.cachedInteractionAttachments[i].Interaction(pb);
                                    }
                                }
                            }
                            else
                            {
                                //So we don't fire anymore
                                data.isFiring = false;
                            }

                            if (setEmptyBool && pb.isFirstPersonActive && data.weaponRenderer.anim)
                            {
                                //Set empty bool
                                data.weaponRenderer.anim.SetBool("empty", data.bulletsLeft == 0);

                                for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                {
                                    data.weaponRenderer.animAdditionals[i].SetBool("empty", data.bulletsLeft == 0);
                                }
                            }
                        }
                        //Reloading
                        else
                        {
                            //When we are reloading, empty bool will be 'false'
                            if (setEmptyBool && pb.isFirstPersonActive && data.weaponRenderer.anim)
                            {
                                //Set empty bool
                                data.weaponRenderer.anim.SetBool("empty", false);

                                for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                {
                                    data.weaponRenderer.animAdditionals[i].SetBool("empty", false);
                                }
                            }
                            //Reset run animation
                            data.startedRunAnimation = false;
                            //Make sure we are not aiming
                            data.isAiming = false;
                            //Make sure we are not firing either
                            data.isFiring = false;
                            //Check for cancelling
                            if (reloadMode == ReloadMode.Procedural || reloadMode == ReloadMode.ProceduralChambered)
                            {
                                if (data.lastLmb != pb.input.lmb)
                                {
                                    data.lastLmb = pb.input.lmb;
                                    if (pb.input.lmb && data.bulletsLeft > 0)
                                    {
                                        //User wants to cancel
                                        data.cancelProceduralReload = true;
                                        data.reloadPhase = 0;
                                        data.reloadInProgress = false;
                                        FireOneShot(pb, data);
                                    }
                                }
                            }
                            else
                            {
                                data.cancelProceduralReload = false;
                            }

                            //Check if a reload phase is over
                            if (Time.time >= data.reloadNextEnd)
                            {
                                switch (reloadMode)
                                {
                                    //Simple reload
                                    case ReloadMode.Simple:
                                        if (data.reloadPhase == 0)
                                        {
                                            //Update bullets
                                            if (data.bulletsLeftToReload > data.bulletsPerMag)
                                            {
                                                if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                                    data.bulletsLeftToReload -= data.bulletsPerMag - data.bulletsLeft;
                                                data.bulletsLeft = data.bulletsPerMag;
                                            }
                                            else
                                            {
                                                int wepBullet = Mathf.Clamp(data.bulletsPerMag, data.bulletsLeftToReload, data.bulletsLeft + data.bulletsLeftToReload);
                                                if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                                    data.bulletsLeftToReload -= (wepBullet - data.bulletsLeft);
                                                data.bulletsLeft = wepBullet;
                                            }
                                            //Proceed
                                            data.reloadPhase = 1;
                                            data.reloadNextEnd = Time.time + reloadTimeTwo;
                                        }
                                        else if (data.reloadPhase == 1)
                                        {
                                            //Reload is over
                                            data.reloadInProgress = false;
                                            data.reloadPhase = 0;
                                        }
                                        break;

                                    //Reload with different anims / times for full / empty state
                                    case ReloadMode.FullEmpty:
                                        if (data.reloadPhase == 0)
                                        {
                                            //Set time
                                            if (data.bulletsLeft > 0)
                                            {
                                                //Full reload
                                                data.reloadNextEnd = Time.time + reloadTimeTwo;
                                            }
                                            else
                                            {
                                                //Empty reload
                                                data.reloadNextEnd = Time.time + reloadEmptyTimeTwo;
                                            }

                                            //Update bullets
                                            if (data.bulletsLeftToReload > data.bulletsPerMag)
                                            {
                                                if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                                    data.bulletsLeftToReload -= data.bulletsPerMag - data.bulletsLeft;
                                                data.bulletsLeft = data.bulletsPerMag;
                                            }
                                            else
                                            {
                                                int wepBullet = Mathf.Clamp(data.bulletsPerMag, data.bulletsLeftToReload, data.bulletsLeft + data.bulletsLeftToReload);
                                                if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                                    data.bulletsLeftToReload -= (wepBullet - data.bulletsLeft);
                                                data.bulletsLeft = wepBullet;
                                            }
                                            //Proceed
                                            data.reloadPhase = 1;
                                        }
                                        else if (data.reloadPhase == 1)
                                        {
                                            //Reload is over
                                            data.reloadInProgress = false;
                                            data.reloadPhase = 0;
                                        }
                                        break;
                                    //Reload with different anims / times for full / empty state and a bullet in the chamber
                                    case ReloadMode.Chambered:
                                        if (data.reloadPhase == 0)
                                        {
                                            //Set time
                                            if (data.bulletsLeft > 0)
                                            {
                                                //Full reload
                                                data.reloadNextEnd = Time.time + reloadTimeTwo;
                                            }
                                            else
                                            {
                                                //Empty reload
                                                data.reloadNextEnd = Time.time + reloadEmptyTimeTwo;
                                            }

                                            //Update bullets
                                            if (data.bulletsLeftToReload > data.bulletsPerMag)
                                            {
                                                if (data.bulletsLeft > 0)
                                                {
                                                    if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                                        data.bulletsLeftToReload -= (data.bulletsPerMag + 1) - data.bulletsLeft;
                                                    data.bulletsLeft = data.bulletsPerMag + 1;
                                                }
                                                else
                                                {
                                                    if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                                        data.bulletsLeftToReload -= data.bulletsPerMag - data.bulletsLeft;
                                                    data.bulletsLeft = data.bulletsPerMag;
                                                }
                                            }
                                            else
                                            {
                                                if (data.bulletsLeft > 0)
                                                {
                                                    int wepBullet = Mathf.Clamp(data.bulletsPerMag + 1, data.bulletsLeftToReload, data.bulletsLeft + data.bulletsLeftToReload);
                                                    if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                                        data.bulletsLeftToReload -= (wepBullet - data.bulletsLeft);
                                                    data.bulletsLeft = wepBullet;
                                                }
                                                else
                                                {
                                                    int wepBullet = Mathf.Clamp(data.bulletsPerMag, data.bulletsLeftToReload, data.bulletsLeft + data.bulletsLeftToReload);
                                                    if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                                        data.bulletsLeftToReload -= (wepBullet - data.bulletsLeft);
                                                    data.bulletsLeft = wepBullet;
                                                }
                                            }
                                            //Proceed
                                            data.reloadPhase = 1;
                                        }
                                        else if (data.reloadPhase == 1)
                                        {
                                            //Reload is over
                                            data.reloadInProgress = false;
                                            data.reloadPhase = 0;
                                        }
                                        break;
                                    case ReloadMode.Procedural:
                                        //Phase 0 = Start
                                        //Phase 1 = End of start (If bullet needs to be added)
                                        //Phase 2 = Loop
                                        //Phase 3 = Loop 2
                                        //Phase 4 = End
                                        if (data.reloadPhase == 0)
                                        {
                                            if (reloadProceduralAddBulletDuringStartEmpty && data.bulletsLeft == 0)
                                            {
                                                //Set phase
                                                data.reloadPhase = 1;
                                                //Set time
                                                data.reloadNextEnd = Time.time + reloadProceduralEmptyInsertStartTimeTwo;
                                                //Add bullet
                                                data.bulletsLeft++;
                                                //Substract
                                                if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                                    data.bulletsLeftToReload--;
                                                //Sounds and animation is already being played, so ignore that
                                            }
                                            else
                                            {
                                                //Go to Phase 2
                                                data.reloadPhase = 2;
                                                //Here we don't need to check if ammo is left because we already did that
                                                //Set time
                                                data.reloadNextEnd = Time.time + reloadProceduralInsertTimeOne;
                                                if (pb.isFirstPersonActive)
                                                {
                                                    //Play animation
                                                    if (data.weaponRenderer.anim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.anim.Play("Reload Procedural Insert", data.weaponRenderer.animActionLayer, 0f);

                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                        {
                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Procedural Insert", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                        }
                                                    }
                                                    else if (data.weaponRenderer.legacyAnim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.legacyAnim.Rewind(data.weaponRenderer.legacyAnimData.reloadProceduralInsert);
                                                        data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.reloadProceduralInsert, PlayMode.StopAll);
                                                    }
                                                    //Play sound
                                                    data.soundReload.PlayOneShot(reloadProceduralInsertSound);
                                                }
                                                else
                                                {
                                                    //Play reload sound
                                                    //Set clip
                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralInsertSound;
                                                    //Set distance
                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                    //Set rolloff
                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                    //Play
                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                }
                                                //Call network
                                                pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 2);
                                            }
                                        }
                                        else if (data.reloadPhase == 1)
                                        {
                                            //Insert is over, check if we have more bullets to reload
                                            if (data.bulletsLeftToReload > 0 && !data.cancelProceduralReload)
                                            {
                                                //Go to Phase 2
                                                data.reloadPhase = 2;
                                                //Here we don't need to check if ammo is left because we already did that
                                                //Set time
                                                data.reloadNextEnd = Time.time + reloadProceduralInsertTimeOne;
                                                if (pb.isFirstPersonActive)
                                                {
                                                    //Play animation
                                                    if (data.weaponRenderer.anim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.anim.Play("Reload Procedural Insert", data.weaponRenderer.animActionLayer, 0f);

                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                        {
                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Procedural Insert", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                        }
                                                    }
                                                    else if (data.weaponRenderer.legacyAnim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.legacyAnim.Rewind(data.weaponRenderer.legacyAnimData.reloadProceduralInsert);
                                                        data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.reloadProceduralInsert, PlayMode.StopAll);
                                                    }
                                                    //Play sound
                                                    data.soundReload.PlayOneShot(reloadProceduralInsertSound);
                                                }
                                                else
                                                {
                                                    //Play reload sound
                                                    //Set clip
                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralInsertSound;
                                                    //Set distance
                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                    //Set rolloff
                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                    //Play
                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                }
                                                //Call network
                                                pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 2);
                                            }
                                            else
                                            {
                                                //End
                                                data.reloadPhase = 4;
                                                //Set time
                                                data.reloadNextEnd = Time.time + reloadProceduralEndTime;
                                                if (pb.isFirstPersonActive)
                                                {
                                                    //Play animation
                                                    if (data.weaponRenderer.anim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.anim.Play("Reload Procedural End", data.weaponRenderer.animActionLayer, 0f);

                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                        {
                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Procedural End", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                        }
                                                    }
                                                    else if (data.weaponRenderer.legacyAnim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.reloadProceduralEnd, PlayMode.StopAll);
                                                    }
                                                    //Play sound
                                                    data.soundReload.PlayOneShot(reloadProceduralEndSound);
                                                }
                                                else
                                                {
                                                    //Play reload sound
                                                    //Set clip
                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralEndSound;
                                                    //Set distance
                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                    //Set rolloff
                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                    //Play
                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                }
                                                //Call network
                                                pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 4);
                                            }
                                        }
                                        else if (data.reloadPhase == 2)
                                        {
                                            //End insert
                                            data.bulletsLeft++;
                                            //Substract
                                            if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                                data.bulletsLeftToReload--;
                                            //Wait
                                            data.reloadPhase = 3;
                                            //Set time
                                            data.reloadNextEnd = Time.time + reloadProceduralInsertTimeTwo;
                                        }
                                        else if (data.reloadPhase == 3)
                                        {
                                            //Insert is over, check if we can insert some more
                                            if (data.bulletsLeftToReload > 0 && data.bulletsLeft < data.bulletsPerMag && !data.cancelProceduralReload)
                                            {
                                                //Go to Phase 2
                                                data.reloadPhase = 2;
                                                //Here we don't need to check if ammo is left because we already did that
                                                //Set time
                                                data.reloadNextEnd = Time.time + reloadProceduralInsertTimeOne;
                                                if (pb.isFirstPersonActive)
                                                {
                                                    //Play animation
                                                    if (data.weaponRenderer.anim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.anim.Play("Reload Procedural Insert", data.weaponRenderer.animActionLayer, 0f);

                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                        {
                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Procedural Insert", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                        }
                                                    }
                                                    else if (data.weaponRenderer.legacyAnim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.legacyAnim.Rewind(data.weaponRenderer.legacyAnimData.reloadProceduralInsert);
                                                        data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.reloadProceduralInsert, PlayMode.StopAll);
                                                    }
                                                    //Play sound
                                                    data.soundReload.PlayOneShot(reloadProceduralInsertSound);
                                                }
                                                else
                                                {
                                                    //Play reload sound
                                                    //Set clip
                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralInsertSound;
                                                    //Set distance
                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                    //Set rolloff
                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                    //Play
                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                }
                                                //Call network
                                                pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 2);
                                            }
                                            else
                                            {
                                                //End
                                                data.reloadPhase = 4;
                                                //Set time
                                                data.reloadNextEnd = Time.time + reloadProceduralEndTime;
                                                if (pb.isFirstPersonActive)
                                                {
                                                    //Play animation
                                                    if (data.weaponRenderer.anim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.anim.Play("Reload Procedural End", data.weaponRenderer.animActionLayer, 0f);

                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                        {
                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Procedural End", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                        }
                                                    }
                                                    else if (data.weaponRenderer.legacyAnim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.reloadProceduralEnd, PlayMode.StopAll);
                                                    }
                                                    //Play sound
                                                    data.soundReload.PlayOneShot(reloadProceduralEndSound);
                                                }
                                                else
                                                {
                                                    //Play reload sound
                                                    //Set clip
                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralEndSound;
                                                    //Set distance
                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                    //Set rolloff
                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                    //Play
                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                }
                                                //Call network
                                                pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 4);
                                            }
                                        }
                                        else if (data.reloadPhase == 4)
                                        {
                                            //It is over, end reload
                                            data.reloadInProgress = false;
                                            data.reloadPhase = 0;
                                        }
                                        break;
                                    case ReloadMode.ProceduralChambered:

                                        //Phase 0 = Start
                                        //Phase 1 = End of start (If bullet needs to be added)
                                        //Phase 2 = Loop
                                        //Phase 3 = Loop 2
                                        //Phase 4 = End
                                        if (data.reloadPhase == 0)
                                        {
                                            if (reloadProceduralAddBulletDuringStartEmpty && data.bulletsLeft == 0)
                                            {
                                                //Set phase
                                                data.reloadPhase = 1;
                                                //Set time
                                                data.reloadNextEnd = Time.time + reloadProceduralEmptyInsertStartTimeTwo;
                                                //Add bullet
                                                data.bulletsLeft++;
                                                //Substract
                                                if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                                    data.bulletsLeftToReload--;
                                                //Sounds and animation is already being played, so ignore that
                                            }
                                            else
                                            {
                                                //Go to Phase 2
                                                data.reloadPhase = 2;
                                                //Here we don't need to check if ammo is left because we already did that
                                                //Set time
                                                data.reloadNextEnd = Time.time + reloadProceduralInsertTimeOne;
                                                if (pb.isFirstPersonActive)
                                                {
                                                    //Play animation
                                                    if (data.weaponRenderer.anim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.anim.Play("Reload Procedural Insert", data.weaponRenderer.animActionLayer, 0f);

                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                        {
                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Procedural Insert", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                        }
                                                    }
                                                    else if (data.weaponRenderer.legacyAnim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.legacyAnim.Rewind(data.weaponRenderer.legacyAnimData.reloadProceduralInsert);
                                                        data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.reloadProceduralInsert, PlayMode.StopAll);
                                                    }
                                                    //Play sound
                                                    data.soundReload.PlayOneShot(reloadProceduralInsertSound);
                                                }
                                                else
                                                {
                                                    //Play reload sound
                                                    //Set clip
                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralInsertSound;
                                                    //Set distance
                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                    //Set rolloff
                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                    //Play
                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                }
                                                //Call network
                                                pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 2);
                                            }
                                        }
                                        else if (data.reloadPhase == 1)
                                        {
                                            //Insert is over, check if we have more bullets to reload
                                            if (data.bulletsLeftToReload > 0 && !data.cancelProceduralReload)
                                            {
                                                //Go to Phase 2
                                                data.reloadPhase = 2;
                                                //Here we don't need to check if ammo is left because we already did that
                                                //Set time
                                                data.reloadNextEnd = Time.time + reloadProceduralInsertTimeOne;
                                                if (pb.isFirstPersonActive)
                                                {
                                                    //Play animation
                                                    if (data.weaponRenderer.anim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.anim.Play("Reload Procedural Insert", data.weaponRenderer.animActionLayer, 0f);

                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                        {
                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Procedural Insert", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                        }
                                                    }
                                                    else if (data.weaponRenderer.legacyAnim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.legacyAnim.Rewind(data.weaponRenderer.legacyAnimData.reloadProceduralInsert);
                                                        data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.reloadProceduralInsert, PlayMode.StopAll);
                                                    }
                                                    //Play sound
                                                    data.soundReload.PlayOneShot(reloadProceduralInsertSound);
                                                }
                                                else
                                                {
                                                    //Play reload sound
                                                    //Set clip
                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralInsertSound;
                                                    //Set distance
                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                    //Set rolloff
                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                    //Play
                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                }
                                                //Call network
                                                pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 2);
                                            }
                                            else
                                            {
                                                //End
                                                data.reloadPhase = 4;
                                                //Set time
                                                data.reloadNextEnd = Time.time + reloadProceduralEndTime;
                                                if (pb.isFirstPersonActive)
                                                {
                                                    //Play animation
                                                    if (data.weaponRenderer.anim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.anim.Play("Reload Procedural End", data.weaponRenderer.animActionLayer, 0f);

                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                        {
                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Procedural End", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                        }
                                                    }
                                                    else if (data.weaponRenderer.legacyAnim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.reloadProceduralEnd, PlayMode.StopAll);
                                                    }
                                                    //Play sound
                                                    data.soundReload.PlayOneShot(reloadProceduralEndSound);
                                                }
                                                else
                                                {
                                                    //Play reload sound
                                                    //Set clip
                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralEndSound;
                                                    //Set distance
                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                    //Set rolloff
                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                    //Play
                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                }
                                                //Call network
                                                pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 4);
                                            }
                                        }
                                        else if (data.reloadPhase == 2)
                                        {
                                            //End insert
                                            data.bulletsLeft++;
                                            //Substract
                                            if (!pb.main.gameInformation.debugEnableUnlimitedReloads)
                                                data.bulletsLeftToReload--;
                                            //Wait
                                            data.reloadPhase = 3;
                                            //Set time
                                            data.reloadNextEnd = Time.time + reloadProceduralInsertTimeTwo;
                                        }
                                        else if (data.reloadPhase == 3)
                                        {
                                            //Insert is over, check if we can insert some more
                                            if (data.bulletsLeftToReload > 0 && data.bulletsLeft < data.bulletsPerMag + 1 && !data.cancelProceduralReload)
                                            {
                                                //Go to Phase 2
                                                data.reloadPhase = 2;
                                                //Here we don't need to check if ammo is left because we already did that
                                                //Set time
                                                data.reloadNextEnd = Time.time + reloadProceduralInsertTimeOne;
                                                if (pb.isFirstPersonActive)
                                                {
                                                    //Play animation
                                                    if (data.weaponRenderer.anim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.anim.Play("Reload Procedural Insert", data.weaponRenderer.animActionLayer, 0f);

                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                        {
                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Procedural Insert", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                        }
                                                    }
                                                    else if (data.weaponRenderer.legacyAnim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.legacyAnim.Rewind(data.weaponRenderer.legacyAnimData.reloadProceduralInsert);
                                                        data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.reloadProceduralInsert, PlayMode.StopAll);
                                                    }
                                                    //Play sound
                                                    data.soundReload.PlayOneShot(reloadProceduralInsertSound);
                                                }
                                                else
                                                {
                                                    //Play reload sound
                                                    //Set clip
                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralInsertSound;
                                                    //Set distance
                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                    //Set rolloff
                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                    //Play
                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                }
                                                //Call network
                                                pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 2);
                                            }
                                            else
                                            {
                                                //End
                                                data.reloadPhase = 4;
                                                //Set time
                                                data.reloadNextEnd = Time.time + reloadProceduralEndTime;
                                                if (pb.isFirstPersonActive)
                                                {
                                                    //Play animation
                                                    if (data.weaponRenderer.anim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.anim.Play("Reload Procedural End", data.weaponRenderer.animActionLayer, 0f);

                                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                        {
                                                            data.weaponRenderer.animAdditionals[i].Play("Reload Procedural End", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                                        }
                                                    }
                                                    else if (data.weaponRenderer.legacyAnim)
                                                    {
                                                        //Play animation
                                                        data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.reloadProceduralEnd, PlayMode.StopAll);
                                                    }
                                                    //Play sound
                                                    data.soundReload.PlayOneShot(reloadProceduralEndSound);
                                                }
                                                else
                                                {
                                                    //Play reload sound
                                                    //Set clip
                                                    pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralEndSound;
                                                    //Set distance
                                                    pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                                    //Set rolloff
                                                    pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                                    //Play
                                                    pb.thirdPersonPlayerModel.soundReload.Play();
                                                }
                                                //Call network
                                                pb.photonView.RPC("WeaponProceduralReloadNetwork", RpcTarget.Others, 4);
                                            }
                                        }
                                        else if (data.reloadPhase == 4)
                                        {
                                            //It is over, end reload
                                            data.reloadInProgress = false;
                                            data.reloadPhase = 0;
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        //Fire Replica
                        if (data.fireMode == FireMode.Auto)
                        {
                            if (data.isFiring)
                            {
                                //Compare with fire rate
                                if (Time.time >= data.lastFire + data.fireRate)
                                {
                                    FireOneShot(pb, data);
                                }
                            }
                        }
                    }

                    //Update aiming
                    if (data.isAiming)
                    {
                        //Increase progress
                        if (aimInTime > 0)
                        {
                            if (data.aimingProgress < 1) data.aimingProgress += Time.deltaTime / aimInTime;
                            //Clamp
                            data.aimingProgress = Mathf.Clamp01(data.aimingProgress);
                        }
                        else
                        {
                            //Instant mode
                            data.aimingProgress = 1f;
                        }
                    }
                    else
                    {
                        //Decrease progress
                        if (aimOutTime > 0)
                        {
                            if (data.aimingProgress > 0) data.aimingProgress -= Time.deltaTime / aimOutTime;
                            //Clamp
                            data.aimingProgress = Mathf.Clamp01(data.aimingProgress);
                        }
                        else
                        {
                            //Instant mode
                            data.aimingProgress = 0;
                        }
                    }

                    if (pb.isFirstPersonActive)
                    {
                        if (data.weaponRenderer.cachedUseFullscreenScope)
                        {
                            if (data.isAimedIn)
                            {
                                //We are aimed in, show scope
                                pb.main.hud.DisplaySniperScope(true);
                                //Snap fov
                                pb.main.mainCamera.fieldOfView = data.weaponRenderer.cachedAimingFov;
                                //Hide weapon
                                if (!data.sniperWeaponHidden)
                                {
                                    data.sniperWeaponHidden = true;
                                    data.weaponRenderer.visible = false;
                                }
                            }
                            else
                            {
                                //We are not aimed in, hide the scope
                                pb.main.hud.DisplaySniperScope(false);
                                //Smoothly fade FoV
                                pb.main.mainCamera.fieldOfView = Mathf.Lerp(Kit_GameSettings.baseFov, Kit_GameSettings.baseFov - 5f, data.aimingProgress);
                                //Show weapon
                                if (data.sniperWeaponHidden)
                                {
                                    data.sniperWeaponHidden = false;
                                    data.weaponRenderer.visible = true;
                                }
                            }
                        }
                        else
                        {
                            //Sniper scope is not active, just hide it
                            pb.main.hud.DisplaySniperScope(false);
                            //Smoothly fade FoV
                            pb.main.mainCamera.fieldOfView = Mathf.Lerp(Kit_GameSettings.baseFov, data.weaponRenderer.cachedAimingFov, data.aimingProgress);
                        }

                        //Update position and rotation
                        data.aimingTransform.localPosition = Vector3.Lerp(Vector3.zero, data.weaponRenderer.cachedAimingPos, data.aimingProgress);
                        data.aimingTransform.localRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(data.weaponRenderer.cachedAimingRot), data.aimingProgress);
                    }

                    if (data.bulletSpreadMode == SpreadMode.SprayPattern && !data.isFiring)
                    {
                        //Spray pattern recovery
                        if (data.sprayPatternState > 0f)
                        {
                            data.sprayPatternState -= Time.deltaTime * data.bulletSpreadSprayPatternRecoverySpeed;
                        }
                        else
                        {
                            data.sprayPatternState = 0f;
                        }
                    }

                    //Shell ejection
                    if (data.shellEjectEnabled) //Check if shell ejection is enabled
                    {
                        //Check if enough time has passed
                        if (Time.time >= data.shellEjectNext || shellEjectionTime <= 0)
                        {
                            //Set ejection bool
                            data.shellEjectEnabled = false;
                            //Eject shell
                            EjectShell(pb, data);
                        }
                    }

                    //Check for bolt action
                    if (data.boltActionState == 1 || data.boltActionState == 2)
                    {
                        //Check if the time has passed
                        if (Time.time > data.boltActionTime)
                        {
                            if (pb.isFirstPersonActive)
                            {
                                //Set values
                                data.soundOther.loop = false;


                                //Set correct clip
                                if (data.boltActionState == 1)
                                {
                                    data.soundOther.clip = boltActionSoundNormal;
                                }
                                else
                                {
                                    data.soundOther.clip = boltActionSoundLast;
                                }

                                //Play
                                data.soundOther.Play();
                            }

                            //Set back to 0
                            data.boltActionState = 0;
                        }
                    }

                    if (pb.isFirstPersonActive)
                    {
                        //Update HUD
                        pb.main.hud.DisplayAmmo(data.bulletsLeft, data.bulletsLeftToReload);
                        pb.main.hud.DisplayCrosshair(GetCrosshairSize(pb, data), pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.ThirdPerson);
                    }
                }
            }

            public override void AnimateWeapon(Kit_PlayerBehaviour pb, object runtimeData, int id, float speed)
            {
                if (pb.isFirstPersonActive)
                {
                    if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                    {
                        WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;

                        data.weaponRenderer.WeaponUpdate(this, data);

                        //Camera animation
                        if (data.weaponRenderer.cameraAnimationEnabled)
                        {
                            if (data.weaponRenderer.cameraAnimationType == CameraAnimationType.Copy)
                            {
                                if (data.weaponRenderer.cameraAnimationBone) pb.playerCameraAnimationTransform.localRotation = Quaternion.Euler(data.weaponRenderer.cameraAnimationReferenceRotation) * data.weaponRenderer.cameraAnimationBone.localRotation;
                                else pb.playerCameraAnimationTransform.localRotation = Quaternion.Slerp(pb.playerCameraAnimationTransform.localRotation, Quaternion.identity, Time.deltaTime * 5f);
                            }
                            else if (data.weaponRenderer.cameraAnimationType == CameraAnimationType.LookAt)
                            {
                                if (data.weaponRenderer.cameraAnimationBone && data.weaponRenderer.cameraAnimationTarget) pb.playerCameraAnimationTransform.localRotation = Quaternion.Euler(data.weaponRenderer.cameraAnimationReferenceRotation) * Quaternion.LookRotation(data.weaponRenderer.cameraAnimationTarget.localPosition - data.weaponRenderer.cameraAnimationBone.localPosition);
                                else pb.playerCameraAnimationTransform.localRotation = Quaternion.Slerp(pb.playerCameraAnimationTransform.localRotation, Quaternion.identity, Time.deltaTime * 5f);
                            }
                        }
                        else
                        {
                            //Go back to 0,0,0
                            pb.playerCameraAnimationTransform.localRotation = Quaternion.Slerp(pb.playerCameraAnimationTransform.localRotation, Quaternion.identity, Time.deltaTime * 5f);
                        }

                        //Weapon delay calculation
                        if ((MarsScreen.lockCursor || pb.isBot) && pb.canControlPlayer && !pb.isBeingSpectated)
                        {
                            //Get input from the mouse
                            data.weaponDelayCurrentX = -pb.input.mouseX * weaponDelayBaseAmount * Time.deltaTime;
                            if (!pb.looking.ReachedYMax(pb)) //Check if we should have delay on y looking
                                data.weaponDelayCurrentY = -pb.input.mouseY * weaponDelayBaseAmount * Time.deltaTime;
                            else //If not, just set it to zero
                                data.weaponDelayCurrentY = 0f;
                        }
                        else
                        {
                            //Cursor is not locked, set values to zero
                            data.weaponDelayCurrentX = 0f;
                            data.weaponDelayCurrentY = 0f;
                        }

                        //Clamp
                        data.weaponDelayCurrentX = Mathf.Clamp(data.weaponDelayCurrentX, -weaponDelayMaxAmount, weaponDelayMaxAmount);
                        data.weaponDelayCurrentY = Mathf.Clamp(data.weaponDelayCurrentY, -weaponDelayMaxAmount, weaponDelayMaxAmount);

                        //Aiming multiplier
                        if (data.isAiming)
                        {
                            data.weaponDelayCurrentX *= weaponDelayAimingMultiplier;
                            data.weaponDelayCurrentY *= weaponDelayAimingMultiplier;
                        }

                        //Update Vector
                        data.weaponDelayCur.x = data.weaponDelayCurrentX;
                        data.weaponDelayCur.y = data.weaponDelayCurrentY;
                        data.weaponDelayCur.z = 0f;

                        //Smooth move towards the target
                        data.weaponDelayTransform.localPosition = Vector3.Lerp(data.weaponDelayTransform.localPosition, data.weaponDelayCur, Time.deltaTime * weaponDelaySmooth);

                        //Weapon tilt
                        if (weaponTiltEnabled)
                        {
                            if (!weaponTiltEnabledWhileAiming && data.isAiming)
                            {
                                data.weaponDelayTransform.localRotation = Quaternion.Slerp(data.weaponDelayTransform.localRotation, Quaternion.identity, Time.deltaTime * weaponTiltReturnSpeed);
                            }
                            else
                            {
                                data.weaponDelayTransform.localRotation = Quaternion.Slerp(data.weaponDelayTransform.localRotation, Quaternion.Euler(0, 0, -pb.movement.GetMovementDirection(pb).x * weaponTiltIntensity), Time.deltaTime * weaponTiltReturnSpeed);
                            }
                        }
                        else
                        {
                            data.weaponDelayTransform.localRotation = Quaternion.Slerp(data.weaponDelayTransform.localRotation, Quaternion.identity, Time.deltaTime * weaponTiltReturnSpeed);
                        }

                        //Weapon Fall
                        data.weaponFallTransform.localRotation = Quaternion.Slerp(data.weaponFallTransform.localRotation, Quaternion.identity, Time.deltaTime * fallDownReturnSpeed);

                        //Recoil
                        //Apply to transform
                        pb.recoilApplyRotation = Quaternion.Slerp(pb.recoilApplyRotation, Quaternion.identity, Time.deltaTime * data.recoilReturnSpeed);

                        //Set speed
                        if (id != 0)
                        {
                            data.genericAnimator.SetFloat("speed", speed);
                        }
                        //If idle, set speed to 1
                        else
                        {
                            data.genericAnimator.SetFloat("speed", 1f);
                        }

                        //Run position and rotation
                        //Check state and if we can move (Selected, not reloading)
                        if (id == 2 && data.isSelectedAndReady && !data.reloadInProgress && data.weaponRenderer.useRunPosRot)
                        {
                            //Move to run pos
                            data.weaponRenderer.transform.localPosition = Vector3.Lerp(data.weaponRenderer.transform.localPosition, data.weaponRenderer.runPos, Time.deltaTime * data.weaponRenderer.runSmooth);
                            //Move to run rot
                            data.weaponRenderer.transform.localRotation = Quaternion.Slerp(data.weaponRenderer.transform.localRotation, Quaternion.Euler(data.weaponRenderer.runRot), Time.deltaTime * data.weaponRenderer.runSmooth);
                            //Set time
                            data.lastRun = Time.time;
                        }
                        else
                        {
                            //Move back to idle pos
                            data.weaponRenderer.transform.localPosition = Vector3.Lerp(data.weaponRenderer.transform.localPosition, Vector3.zero, Time.deltaTime * data.weaponRenderer.runSmooth * 2f);
                            //Move back to idle rot
                            data.weaponRenderer.transform.localRotation = Quaternion.Slerp(data.weaponRenderer.transform.localRotation, Quaternion.identity, Time.deltaTime * data.weaponRenderer.runSmooth * 2f);
                        }

                        //Make sure idle anim is playing if no anim is playing
                        if (data.weaponRenderer.legacyAnim && !data.weaponRenderer.legacyAnim.isPlaying && data.isSelectedAndReady)
                        {
                            if (data.bulletsLeft == 0)
                            {
                                data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idleEmpty].wrapMode = WrapMode.Loop;
                                data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idleEmpty].speed = data.weaponRenderer.legacyAnimData.idleAnimationSpeed;
                                data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.idleEmpty, 0.3f, PlayMode.StopAll);
                            }
                            else
                            {
                                data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idle].wrapMode = WrapMode.Loop;
                                data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idle].speed = data.weaponRenderer.legacyAnimData.idleAnimationSpeed;
                                data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.idle, 0.3f, PlayMode.StopAll);
                            }
                        }

                        //If we are aiming, force idle animation
                        if (data.isAiming && !enableWalkAnimationWhileAiming)
                        {
                            //Play idle animation
                            data.genericAnimator.CrossFade("Idle", 0.3f);
                            //Also set last id to 0, so it can be updated if we stop aiming
                            data.lastWeaponAnimationID = 0;

                            if (!useGenericRunAnim)
                            {
                                //End run animation on weapon animator
                                if (data.startedRunAnimation)
                                {
                                    data.startedRunAnimation = false;
                                    if (data.weaponRenderer.anim)
                                    {
                                        data.weaponRenderer.anim.SetTrigger("End Run");

                                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                        {
                                            data.weaponRenderer.animAdditionals[i].SetTrigger("End Run");
                                        }
                                    }
                                    else if (data.weaponRenderer.legacyAnim)
                                    {
                                        if (data.bulletsLeft == 0)
                                        {
                                            data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idleEmpty].wrapMode = WrapMode.Loop;
                                            data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idleEmpty].speed = data.weaponRenderer.legacyAnimData.idleAnimationSpeed;
                                            data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.idleEmpty, 0.3f, PlayMode.StopAll);
                                        }
                                        else
                                        {
                                            data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idle].wrapMode = WrapMode.Loop;
                                            data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idle].speed = data.weaponRenderer.legacyAnimData.idleAnimationSpeed;
                                            data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.idle, 0.3f, PlayMode.StopAll);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Check if state changed
                            if (id != data.lastWeaponAnimationID)
                            {
                                //Idle
                                if (id == 0)
                                {
                                    //Play idle animation
                                    data.genericAnimator.CrossFade("Idle", 0.3f);

                                    if (!useGenericRunAnim)
                                    {
                                        //End run animation on weapon animator
                                        if (data.startedRunAnimation)
                                        {
                                            data.startedRunAnimation = false;
                                            if (data.weaponRenderer.anim)
                                            {
                                                data.weaponRenderer.anim.SetTrigger("End Run");

                                                for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                {
                                                    data.weaponRenderer.animAdditionals[i].SetTrigger("End Run");
                                                }
                                            }
                                            else if (data.weaponRenderer.legacyAnim)
                                            {
                                                if (data.bulletsLeft == 0)
                                                {
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idleEmpty].wrapMode = WrapMode.Loop;
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idleEmpty].speed = data.weaponRenderer.legacyAnimData.idleAnimationSpeed;
                                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.idleEmpty, 0.3f, PlayMode.StopAll);
                                                }
                                                else
                                                {
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idle].wrapMode = WrapMode.Loop;
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idle].speed = data.weaponRenderer.legacyAnimData.idleAnimationSpeed;
                                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.idle, 0.3f, PlayMode.StopAll);
                                                }
                                            }
                                        }
                                    }
                                }
                                //Walk
                                else if (id == 1 && (!data.isAiming || enableWalkAnimationWhileAiming))
                                {
                                    //Check if we should use generic anim
                                    if (useGenericWalkAnim)
                                    {
                                        //Play run animation
                                        data.genericAnimator.CrossFade("Walk", 0.2f);
                                    }
                                    //If not continue to play Idle
                                    else
                                    {
                                        //Play idle animation
                                        data.genericAnimator.CrossFade("Idle", 0.3f);
                                    }

                                    if (!useGenericRunAnim)
                                    {
                                        //End run animation on weapon animator
                                        if (data.startedRunAnimation)
                                        {
                                            data.startedRunAnimation = false;
                                            if (data.weaponRenderer.anim)
                                            {
                                                data.weaponRenderer.anim.ResetTrigger("Start Run");
                                                data.weaponRenderer.anim.SetTrigger("End Run");

                                                for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                {
                                                    data.weaponRenderer.animAdditionals[i].ResetTrigger("Start Run");
                                                    data.weaponRenderer.animAdditionals[i].SetTrigger("End Run");
                                                }
                                            }
                                            else if (data.weaponRenderer.legacyAnim)
                                            {
                                                if (data.bulletsLeft == 0)
                                                {
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idleEmpty].wrapMode = WrapMode.Loop;
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idleEmpty].speed = data.weaponRenderer.legacyAnimData.idleAnimationSpeed;
                                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.idleEmpty, 0.3f, PlayMode.StopAll);
                                                }
                                                else
                                                {
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idle].wrapMode = WrapMode.Loop;
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idle].speed = data.weaponRenderer.legacyAnimData.idleAnimationSpeed;
                                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.idle, 0.3f, PlayMode.StopAll);
                                                }
                                            }
                                        }
                                    }
                                }
                                //Run
                                else if (id == 2 && (!data.isAiming || enableWalkAnimationWhileAiming))
                                {
                                    //Check if we should use generic anim
                                    if (useGenericRunAnim)
                                    {
                                        //Play run animation
                                        data.genericAnimator.CrossFade("Run", 0.2f);
                                    }
                                    //If not continue to play Idle
                                    else
                                    {
                                        //Play idle animation
                                        data.genericAnimator.CrossFade("Idle", 0.3f);
                                        //Start run animation on weapon animator
                                        if (!data.startedRunAnimation && !data.reloadInProgress && data.isSelectedAndReady)
                                        {
                                            data.startedRunAnimation = true;
                                            if (data.weaponRenderer.anim)
                                            {
                                                data.weaponRenderer.anim.ResetTrigger("End Run");
                                                data.weaponRenderer.anim.SetTrigger("Start Run");

                                                for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                {
                                                    data.weaponRenderer.animAdditionals[i].ResetTrigger("End Run");
                                                    data.weaponRenderer.animAdditionals[i].SetTrigger("Start Run");
                                                }
                                            }
                                            else if (data.weaponRenderer.legacyAnim)
                                            {
                                                if (data.bulletsLeft == 0)
                                                {
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.runEmpty].wrapMode = WrapMode.Loop;
                                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.runEmpty, 0.3f, PlayMode.StopAll);
                                                }
                                                else
                                                {
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.run].wrapMode = WrapMode.Loop;
                                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.run, 0.3f, PlayMode.StopAll);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //End run animation on weapon animator
                                    if (data.startedRunAnimation)
                                    {
                                        data.startedRunAnimation = false;
                                        if (data.weaponRenderer.anim)
                                        {
                                            data.weaponRenderer.anim.ResetTrigger("Start Run");
                                            data.weaponRenderer.anim.SetTrigger("End Run");

                                            for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                            {
                                                data.weaponRenderer.animAdditionals[i].ResetTrigger("Start Run");
                                                data.weaponRenderer.animAdditionals[i].SetTrigger("End Run");
                                            }
                                        }
                                        else if (data.weaponRenderer.legacyAnim)
                                        {
                                            if (data.bulletsLeft == 0)
                                            {
                                                data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idleEmpty].wrapMode = WrapMode.Loop;
                                                data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idleEmpty].speed = data.weaponRenderer.legacyAnimData.idleAnimationSpeed;
                                                data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.idleEmpty, 0.3f, PlayMode.StopAll);
                                            }
                                            else
                                            {
                                                data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idle].wrapMode = WrapMode.Loop;
                                                data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idle].speed = data.weaponRenderer.legacyAnimData.idleAnimationSpeed;
                                                data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.idle, 0.3f, PlayMode.StopAll);
                                            }
                                        }
                                    }
                                }
                                //Update last state
                                data.lastWeaponAnimationID = id;
                            }
                            else
                            {
                                if (!useGenericRunAnim)
                                {
                                    //Idle
                                    if (id == 0)
                                    {
                                        //End run animation on weapon animator
                                        if (data.startedRunAnimation)
                                        {
                                            data.startedRunAnimation = false;
                                            if (data.weaponRenderer.anim)
                                            {
                                                data.weaponRenderer.anim.ResetTrigger("Start Run");
                                                data.weaponRenderer.anim.SetTrigger("End Run");

                                                for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                {
                                                    data.weaponRenderer.animAdditionals[i].ResetTrigger("Start Run");
                                                    data.weaponRenderer.animAdditionals[i].SetTrigger("End Run");
                                                }
                                            }
                                            else if (data.weaponRenderer.legacyAnim)
                                            {
                                                if (data.bulletsLeft == 0)
                                                {
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idleEmpty].wrapMode = WrapMode.Loop;
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idleEmpty].speed = data.weaponRenderer.legacyAnimData.idleAnimationSpeed;
                                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.idleEmpty, 0.3f, PlayMode.StopAll);
                                                }
                                                else
                                                {
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idle].wrapMode = WrapMode.Loop;
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idle].speed = data.weaponRenderer.legacyAnimData.idleAnimationSpeed;
                                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.idle, 0.3f, PlayMode.StopAll);
                                                }
                                            }
                                        }
                                    }
                                    //Walk
                                    else if (id == 1 && (!data.isAiming || enableWalkAnimationWhileAiming))
                                    {
                                        //End run animation on weapon animator
                                        if (data.startedRunAnimation)
                                        {
                                            data.startedRunAnimation = false;
                                            if (data.weaponRenderer.anim)
                                            {
                                                data.weaponRenderer.anim.ResetTrigger("Start Run");
                                                data.weaponRenderer.anim.SetTrigger("End Run");

                                                for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                {
                                                    data.weaponRenderer.animAdditionals[i].ResetTrigger("Start Run");
                                                    data.weaponRenderer.animAdditionals[i].SetTrigger("End Run");
                                                }
                                            }
                                            else if (data.weaponRenderer.legacyAnim)
                                            {
                                                if (data.bulletsLeft == 0)
                                                {
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idleEmpty].wrapMode = WrapMode.Loop;
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idleEmpty].speed = data.weaponRenderer.legacyAnimData.idleAnimationSpeed;
                                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.idleEmpty, 0.3f, PlayMode.StopAll);
                                                }
                                                else
                                                {
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idle].wrapMode = WrapMode.Loop;
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idle].speed = data.weaponRenderer.legacyAnimData.idleAnimationSpeed;
                                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.idle, 0.3f, PlayMode.StopAll);
                                                }
                                            }
                                        }
                                    }
                                    //Run
                                    else if (id == 2 && !data.isAiming)
                                    {
                                        //Start run animation on weapon animator
                                        if (!data.startedRunAnimation && !data.reloadInProgress && data.isSelectedAndReady)
                                        {
                                            data.startedRunAnimation = true;
                                            if (data.weaponRenderer.anim)
                                            {
                                                data.weaponRenderer.anim.ResetTrigger("End Run");
                                                data.weaponRenderer.anim.SetTrigger("Start Run");

                                                for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                {
                                                    data.weaponRenderer.animAdditionals[i].ResetTrigger("End Run");
                                                    data.weaponRenderer.animAdditionals[i].SetTrigger("Start Run");
                                                }
                                            }
                                            else if (data.weaponRenderer.legacyAnim)
                                            {
                                                if (data.bulletsLeft == 0)
                                                {
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.runEmpty].wrapMode = WrapMode.Loop;
                                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.runEmpty, 0.3f, PlayMode.StopAll);
                                                }
                                                else
                                                {
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.run].wrapMode = WrapMode.Loop;
                                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.run, 0.3f, PlayMode.StopAll);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //End run animation on weapon animator
                                        if (data.startedRunAnimation)
                                        {
                                            data.startedRunAnimation = false;
                                            if (data.weaponRenderer.anim)
                                            {
                                                data.weaponRenderer.anim.ResetTrigger("Start Run");
                                                data.weaponRenderer.anim.SetTrigger("End Run");

                                                for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                                {
                                                    data.weaponRenderer.animAdditionals[i].ResetTrigger("Start Run");
                                                    data.weaponRenderer.animAdditionals[i].SetTrigger("End Run");
                                                }
                                            }
                                            else if (data.weaponRenderer.legacyAnim)
                                            {
                                                if (data.bulletsLeft == 0)
                                                {
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idleEmpty].wrapMode = WrapMode.Loop;
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idleEmpty].speed = data.weaponRenderer.legacyAnimData.idleAnimationSpeed;
                                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.idleEmpty, 0.3f, PlayMode.StopAll);
                                                }
                                                else
                                                {
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idle].wrapMode = WrapMode.Loop;
                                                    data.weaponRenderer.legacyAnim[data.weaponRenderer.legacyAnimData.idle].speed = data.weaponRenderer.legacyAnimData.idleAnimationSpeed;
                                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.idle, 0.3f, PlayMode.StopAll);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (setAimingProgress && pb.isFirstPersonActive && data.weaponRenderer.anim)
                        {
                            //Set empty bool
                            data.weaponRenderer.anim.SetFloat("aimingProgress", data.aimingProgress);

                            for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                            {
                                data.weaponRenderer.animAdditionals[i].SetFloat("aimingProgress", data.aimingProgress);
                            }
                        }

                        if (setAimingBool && pb.isFirstPersonActive && data.weaponRenderer.anim)
                        {
                            //Set empty bool
                            data.weaponRenderer.anim.SetBool("aiming", data.isAiming);

                            for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                            {
                                data.weaponRenderer.animAdditionals[i].SetBool("aiming", data.isAiming);
                            }
                        }

                        if (setMovementDirection && pb.isFirstPersonActive && data.weaponRenderer.anim)
                        {
                            Vector3 dir = pb.movement.GetMovementDirection(pb);
                            //Set empty bool
                            data.weaponRenderer.anim.SetFloat("ver", dir.z, 0.1f, Time.deltaTime);
                            data.weaponRenderer.anim.SetFloat("hor", dir.x, 0.1f, Time.deltaTime);
                            data.weaponRenderer.anim.SetFloat("walkSpeed", pb.movement.GetVelocity(pb).magnitude);

                            for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                            {
                                data.weaponRenderer.animAdditionals[i].SetFloat("ver", dir.z, 0.1f, Time.deltaTime);
                                data.weaponRenderer.animAdditionals[i].SetFloat("hor", dir.x, 0.1f, Time.deltaTime);
                                data.weaponRenderer.animAdditionals[i].SetFloat("walkSpeed", pb.movement.GetVelocity(pb).magnitude);
                            }
                        }
                    }
                }
            }

            public override void FallDownEffect(Kit_PlayerBehaviour pb, object runtimeData, bool wasFallDamageApplied)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    if (wasFallDamageApplied)
                    {
                        Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(data.weaponFallTransform, new Vector3(fallDownAmount, Random.Range(fallDownMinOffset, fallDownMaxoffset), 0), fallDownTime));
                    }
                    else
                    {
                        Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.Kick(data.weaponFallTransform, new Vector3(fallDownAmount / 3, Random.Range(fallDownMinOffset, fallDownMaxoffset) / 2, 0), fallDownTime));
                    }
                }
            }

            public override void DrawWeapon(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    //Reset firing
                    data.isFiring = false;
                    //Set selected
                    data.isSelected = true;
                    if (pb.isFirstPersonActive)
                    {
                        //Reset pos & rot of the renderer
                        data.weaponRenderer.transform.localPosition = Vector3.zero;
                        data.weaponRenderer.transform.localRotation = Quaternion.identity;
                        //Reset fov
                        pb.main.mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                        //Reset delay
                        data.weaponDelayCur = Vector3.zero;
                        //Call renderer
                        data.weaponRenderer.WeaponDraw();
                    }
                    //Reset aiming
                    data.aimingProgress = 0f;
                    if (pb.isFirstPersonActive)
                    {
                        data.aimingTransform.localPosition = Vector3.zero;
                        data.aimingTransform.localRotation = Quaternion.identity;
                    }
                    data.isAiming = false;
                    //Set default values
                    data.reloadInProgress = false;
                    //Reset spray pattern
                    data.sprayPatternState = 0f;
                    if (pb.isFirstPersonActive)
                    {
                        //Enable anim
                        if (data.weaponRenderer.anim)
                        {
                            //Disable anim
                            data.weaponRenderer.anim.enabled = true;
                            //Play animation
                            data.weaponRenderer.anim.Play("Draw", data.weaponRenderer.animActionLayer, 0f);

                            for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                            {
                                data.weaponRenderer.animAdditionals[i].enabled = true;
                                data.weaponRenderer.animAdditionals[i].Play("Draw", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                            }

                            if (setEmptyBool)
                            {
                                //Set empty bool
                                data.weaponRenderer.anim.SetBool("empty", data.bulletsLeft == 0);

                                for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                {
                                    data.weaponRenderer.animAdditionals[i].SetBool("empty", data.bulletsLeft == 0);
                                }
                            }
                        }
                        else if (data.weaponRenderer.legacyAnim)
                        {
                            data.weaponRenderer.legacyAnim.enabled = true;
                            if (data.bulletsLeft == 0)
                            {
                                //Play animation
                                data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.drawEmpty, PlayMode.StopAll);
                            }
                            else
                            {
                                //Play animation
                                data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.draw, PlayMode.StopAll);
                            }
                        }
                        //Reset shell ejection
                        data.shellEjectEnabled = false;
                        data.shellEjectNext = 0f;
                    }
                    //Reset bolt
                    data.boltActionState = 0;
                    data.boltActionTime = 0f;
                    if (pb.isFirstPersonActive)
                    {
                        //Play sound if it is assigned
                        if (drawSound) data.soundOther.PlayOneShot(drawSound);
                        if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson)
                        {
                            //Show weapon
                            data.weaponRenderer.visible = true;
                        }
                        else
                        {
                            data.weaponRenderer.visible = false;
                        }
                    }
                    if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson && pb.isFirstPersonActive)
                    {
                        //Show tp weapon and hide
                        data.tpWeaponRenderer.visible = true;
                        data.tpWeaponRenderer.shadowsOnly = true;
                    }
                    else
                    {
                        //Show tp weapon and show
                        data.tpWeaponRenderer.visible = true;
                        data.tpWeaponRenderer.shadowsOnly = false;
                    }
                    //Make sure it is not ready yet
                    data.isSelectedAndReady = false;
                    //Reset run animation
                    data.startedRunAnimation = false;
                    //Set third person anim type
                    pb.thirdPersonPlayerModel.SetAnimType(thirdPersonAnimType);
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override void PutawayWeapon(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    //Reset firing
                    data.isFiring = false;
                    //Reset aiming
                    data.aimingProgress = 0f;
                    //Reset reload
                    data.reloadInProgress = false;
                    if (pb.isFirstPersonActive)
                    {
                        data.aimingTransform.localPosition = Vector3.zero;
                        data.aimingTransform.localRotation = Quaternion.identity;
                        //Call renderer
                        data.weaponRenderer.WeaponPutaway();
                    }
                    data.isAiming = false;
                    if (pb.isFirstPersonActive)
                    {
                        //Check if we need to hide scope
                        if (data.weaponRenderer.cachedUseFullscreenScope)
                        {
                            //Hide
                            pb.main.hud.DisplaySniperScope(false);
                        }
                        //Reset fov
                        pb.main.mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                        //Enable anim
                        if (data.weaponRenderer.anim)
                        {
                            //Disable anim
                            data.weaponRenderer.anim.enabled = true;

                            for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                            {
                                data.weaponRenderer.animAdditionals[i].enabled = true;
                            }
                        }
                        else if (data.weaponRenderer.legacyAnim)
                        {
                            data.weaponRenderer.legacyAnim.enabled = true;
                        }
                        if (setEmptyBool)
                        {
                            //Set empty bool
                            data.weaponRenderer.anim.SetBool("empty", data.bulletsLeft == 0);

                            for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                            {
                                data.weaponRenderer.animAdditionals[i].SetBool("empty", data.bulletsLeft == 0);
                            }
                        }
                        //Play animation
                        if (data.weaponRenderer.anim)
                        {
                            //Play animation
                            data.weaponRenderer.anim.Play("Putaway", data.weaponRenderer.animActionLayer, 0f);

                            for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                            {
                                data.weaponRenderer.animAdditionals[i].Play("Putaway", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                            }
                        }
                        else if (data.weaponRenderer.legacyAnim)
                        {
                            //Play animation
                            if (data.bulletsLeft == 0)
                            {
                                data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.putawayEmpty, PlayMode.StopAll);
                            }
                            else
                            {
                                data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.putaway, PlayMode.StopAll);
                            }
                        }
                        //Stop reload sound
                        data.soundReload.Stop();
                        //Play sound if it is assigned
                        if (putawaySound) data.soundOther.PlayOneShot(putawaySound);
                        if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson)
                        {
                            //Show weapon
                            data.weaponRenderer.visible = true;
                        }
                        else
                        {
                            //Hide
                            data.weaponRenderer.visible = false;
                        }
                    }
                    if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson && pb.isFirstPersonActive)
                    {
                        //Show tp weapon
                        data.tpWeaponRenderer.visible = true;
                        data.tpWeaponRenderer.shadowsOnly = true;
                    }
                    else
                    {
                        data.tpWeaponRenderer.visible = true;
                        data.tpWeaponRenderer.shadowsOnly = false;
                    }
                    //Make sure it is not ready yet
                    data.isSelectedAndReady = false;
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override void PutawayWeaponHide(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    //Set selected
                    data.isSelected = false;
                    //Reset run animation
                    data.startedRunAnimation = false;
                    //Reset firing
                    data.isFiring = false;
                    //Reset aiming
                    data.aimingProgress = 0f;
                    if (pb.isFirstPersonActive)
                    {
                        data.aimingTransform.localPosition = Vector3.zero;
                        data.aimingTransform.localRotation = Quaternion.identity;
                        //Call renderer
                        data.weaponRenderer.WeaponPutawayHide();
                    }
                    data.isAiming = false;
                    if (pb.isFirstPersonActive)
                    {
                        //Check if we need to hide scope
                        if (data.weaponRenderer.cachedUseFullscreenScope)
                        {
                            //Hide
                            pb.main.hud.DisplaySniperScope(false);
                        }
                        //Reset fov
                        pb.main.mainCamera.fieldOfView = Kit_GameSettings.baseFov;
                        //Hide weapon
                        data.weaponRenderer.visible = false;
                        if (data.weaponRenderer.anim)
                        {
                            //Disable anim
                            data.weaponRenderer.anim.enabled = false;

                            for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                            {
                                data.weaponRenderer.animAdditionals[i].enabled = false;
                            }
                        }
                        else if (data.weaponRenderer.legacyAnim)
                        {
                            data.weaponRenderer.legacyAnim.enabled = false;
                        }
                        //Reset pos & rot of the renderer
                        data.weaponRenderer.transform.localPosition = Vector3.zero;
                        data.weaponRenderer.transform.localRotation = Quaternion.identity;
                        //Reset delay
                        data.weaponDelayCur = Vector3.zero;
                    }
                    //Hide tp weapon
                    data.tpWeaponRenderer.visible = false;
                    //Make sure it is not ready
                    data.isSelectedAndReady = false;
                    //Stop third person anims
                    pb.thirdPersonPlayerModel.AbortWeaponAnimations();
                }
            }

            public override void QuickUseOnOtherWeaponBegin(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    if (pb.isFirstPersonActive)
                    {
                        data.soundReload.Stop();
                        data.soundOther.Stop();
                    }

                    pb.thirdPersonPlayerModel.soundReload.Stop();
                    pb.thirdPersonPlayerModel.soundOther.Stop();
                }
            }

            public override void OnPhotonSerializeView(Kit_PlayerBehaviour pb, PhotonStream stream, PhotonMessageInfo info, object runtimeData)
            {
                if (stream.IsWriting)
                {
                    if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                    {
                        WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                        //Send firing
                        stream.SendNext(data.isFiring);
                        //Aiming
                        stream.SendNext(data.isAiming);
                        //Ammo
                        stream.SendNext(data.bulletsLeft);
                        stream.SendNext(data.bulletsLeftToReload);

                        try
                        {
                            for (int i = 0; i < data.weaponRenderer.cachedSyncAttachments.Length; i++)
                            {
                                int index = i;
                                data.weaponRenderer.cachedSyncAttachments[i].OnPhotonSerializeView(stream, info, pb, data, index);
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                    else
                    {
                        //Dummy send
                        stream.SendNext(false);
                        stream.SendNext(false);
                        stream.SendNext(0);
                        stream.SendNext(0);
                    }
                }
                else
                {
                    if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                    {
                        WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                        //Receive firing
                        data.isFiring = (bool)stream.ReceiveNext();
                        //Receive aiming
                        data.isAiming = (bool)stream.ReceiveNext();
                        //Receive ammo
                        data.bulletsLeft = (int)stream.ReceiveNext();
                        data.bulletsLeftToReload = (int)stream.ReceiveNext();

                        try
                        {
                            for (int i = 0; i < data.weaponRenderer.cachedSyncAttachments.Length; i++)
                            {
                                int index = i;
                                data.weaponRenderer.cachedSyncAttachments[i].OnPhotonSerializeView(stream, info, pb, data, index);
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                    else
                    {
                        //Dummy read
                        stream.ReceiveNext();
                        stream.ReceiveNext();
                        stream.ReceiveNext();
                        stream.ReceiveNext();
                    }
                }
            }

            public override void NetworkSemiRPCReceived(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (!pb.photonView.IsMine)
                {
                    if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                    {
                        WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                        FireOneShot(pb, data);
                    }
                }
            }

            public override void NetworkBoltActionRPCReceived(Kit_PlayerBehaviour pb, object runtimeData, int state)
            {
                if (!pb.photonView.IsMine)
                {
                    if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                    {
                        WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                        FireOneShot(pb, data);
                    }
                }
            }

            public override void NetworkBurstRPCReceived(Kit_PlayerBehaviour pb, object runtimeData, int burstLength)
            {
                if (!pb.photonView.IsMine)
                {
                    if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                    {
                        WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                        FireBurst(pb, data);
                    }
                }
            }

            public override void NetworkReloadRPCReceived(Kit_PlayerBehaviour pb, bool isEmpty, object runtimeData)
            {
                if (!pb.photonView.IsMine)
                {
                    if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                    {
                        WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                        //Play third person reload animation
                        pb.thirdPersonPlayerModel.PlayWeaponReloadAnimation(thirdPersonAnimType, isEmpty ? 1 : 0);
                        //Play reload sound
                        if (isEmpty)
                        {
                            if (pb.isFirstPersonActive)
                            {
                                //Set clip
                                data.soundReload.clip = reloadSoundEmpty;
                                //Play
                                data.soundReload.Play();

                                if (data.weaponRenderer.anim)
                                {
                                    //Play animation
                                    data.weaponRenderer.anim.Play("Reload Empty", data.weaponRenderer.animActionLayer, 0f);

                                    for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                    {
                                        data.weaponRenderer.animAdditionals[i].Play("Reload Empty", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                    }
                                }
                                else if (data.weaponRenderer.legacyAnim)
                                {
                                    //Play animation
                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.reloadEmpty, 0.15f, PlayMode.StopAll);
                                }
                            }
                            else
                            {
                                //Set clip
                                pb.thirdPersonPlayerModel.soundReload.clip = reloadSoundEmpty;
                                //Set distance
                                pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                //Set rolloff
                                pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                //Play
                                pb.thirdPersonPlayerModel.soundReload.Play();
                            }
                        }
                        else
                        {
                            if (pb.isFirstPersonActive)
                            {
                                //Set clip
                                data.soundReload.clip = reloadSound;
                                //Play
                                data.soundReload.Play();

                                if (data.weaponRenderer.anim)
                                {
                                    //Play animation
                                    data.weaponRenderer.anim.Play("Reload Full", data.weaponRenderer.animActionLayer, 0f);

                                    for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                    {
                                        data.weaponRenderer.animAdditionals[i].Play("Reload Full", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                    }
                                }
                                else if (data.weaponRenderer.legacyAnim)
                                {
                                    //Play animation
                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.reload, 0.15f, PlayMode.StopAll);
                                }
                            }
                            else
                            {
                                //Set clip
                                pb.thirdPersonPlayerModel.soundReload.clip = reloadSound;
                                //Set distance
                                pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                //Set rolloff
                                pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                //Play
                                pb.thirdPersonPlayerModel.soundReload.Play();
                            }
                        }
                    }
                }
            }

            public override void NetworkProceduralReloadRPCReceived(Kit_PlayerBehaviour pb, int stage, object runtimeData)
            {
                if (!pb.photonView.IsMine)
                {
                    if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                    {
                        WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                        //Play sounds
                        //0 = Start
                        if (stage == 0)
                        {
                            if (pb.isFirstPersonActive)
                            {
                                if (data.weaponRenderer.anim)
                                {
                                    //Play animation
                                    data.weaponRenderer.anim.Play("Reload Procedural Start", data.weaponRenderer.animActionLayer, 0f);

                                    for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                    {
                                        data.weaponRenderer.animAdditionals[i].Play("Reload Procedural Start", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                    }
                                }
                                else if (data.weaponRenderer.legacyAnim)
                                {
                                    //Play animation
                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.reloadProceduralStart, 0.15f, PlayMode.StopAll);
                                }
                                //Play sound
                                data.soundReload.PlayOneShot(reloadProceduralStartSound);
                            }
                            else
                            {
                                //Set clip
                                pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralStartSound;
                                //Set distance
                                pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                //Set rolloff
                                pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                //Play
                                pb.thirdPersonPlayerModel.soundReload.Play();
                            }
                        }
                        else if (stage == 1)
                        {
                            if (pb.isFirstPersonActive)
                            {
                                if (data.weaponRenderer.anim)
                                {
                                    //Play animation
                                    data.weaponRenderer.anim.Play("Reload Procedural Start Empty", data.weaponRenderer.animActionLayer, 0f);

                                    for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                    {
                                        data.weaponRenderer.animAdditionals[i].Play("Reload Procedural Start Empty", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                    }
                                }
                                else if (data.weaponRenderer.legacyAnim)
                                {
                                    //Play animation
                                    data.weaponRenderer.legacyAnim.CrossFade(data.weaponRenderer.legacyAnimData.reloadProceduralStartEmpty, 0.15f, PlayMode.StopAll);
                                }
                                //Play sound
                                data.soundReload.PlayOneShot(reloadProceduralStartEmptySound);
                            }
                            else
                            {
                                //Set clip
                                pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralStartEmptySound;
                                //Set distance
                                pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                //Set rolloff
                                pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                //Play
                                pb.thirdPersonPlayerModel.soundReload.Play();
                            }
                        }
                        else if (stage == 2)
                        {
                            if (pb.isFirstPersonActive)
                            {
                                //Play animation
                                if (data.weaponRenderer.anim)
                                {
                                    //Play animation
                                    data.weaponRenderer.anim.Play("Reload Procedural Insert", data.weaponRenderer.animActionLayer, 0f);

                                    for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                    {
                                        data.weaponRenderer.animAdditionals[i].Play("Reload Procedural Insert", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                    }
                                }
                                else if (data.weaponRenderer.legacyAnim)
                                {
                                    //Play animation
                                    data.weaponRenderer.legacyAnim.Rewind(data.weaponRenderer.legacyAnimData.reloadProceduralInsert);
                                    data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.reloadProceduralInsert, PlayMode.StopAll);
                                }
                                //Play sound
                                data.soundReload.PlayOneShot(reloadProceduralInsertSound);
                            }
                            else
                            {
                                //Set clip
                                pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralInsertSound;
                                //Set distance
                                pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                //Set rolloff
                                pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                //Play
                                pb.thirdPersonPlayerModel.soundReload.Play();
                            }
                        }
                        else if (stage == 4)
                        {
                            if (pb.isFirstPersonActive)
                            {
                                //Play animation
                                if (data.weaponRenderer.anim)
                                {
                                    //Play animation
                                    data.weaponRenderer.anim.Play("Reload Procedural End", data.weaponRenderer.animActionLayer, 0f);

                                    for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                    {
                                        data.weaponRenderer.animAdditionals[i].Play("Reload Procedural End", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                    }
                                }
                                else if (data.weaponRenderer.legacyAnim)
                                {
                                    //Play animation
                                    data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.reloadProceduralEnd, PlayMode.StopAll);
                                }
                                //Play sound
                                data.soundReload.PlayOneShot(reloadProceduralEndSound);
                            }
                            else
                            {
                                //Set clip
                                pb.thirdPersonPlayerModel.soundReload.clip = reloadProceduralEndSound;
                                //Set distance
                                pb.thirdPersonPlayerModel.soundReload.maxDistance = reloadSoundThirdPersonMaxRange;
                                //Set rolloff
                                pb.thirdPersonPlayerModel.soundReload.SetCustomCurve(AudioSourceCurveType.CustomRolloff, reloadSoundThirdPersonRolloff);
                                //Play
                                pb.thirdPersonPlayerModel.soundReload.Play();
                            }
                        }
                    }
                }
            }

            public override WeaponIKValues GetIK(Kit_PlayerBehaviour pb, Animator anim, object runtimeData)
            {
                WeaponIKValues toReturn = new WeaponIKValues();
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    if (data.tpWeaponRenderer.leftHandIK.Length > 0)
                    {
                        toReturn.leftHandIK = data.tpWeaponRenderer.leftHandIK[Mathf.Clamp(pb.thirdPersonPlayerModel.inverseKinematicID, 0, data.tpWeaponRenderer.leftHandIK.Length - 1)];

                        //Check if third person reload animation is being played
                        if (anim.GetCurrentAnimatorStateInfo(1).IsName(thirdPersonAnimType + " Reload") || anim.GetCurrentAnimatorStateInfo(1).IsName(thirdPersonAnimType + " Reload Empty"))
                        {
                            toReturn.canUseIK = false;
                        }
                        else
                        {
                            toReturn.canUseIK = true;
                        }
                    }
                    else
                    {
                        //Can't use ik if none is assigned
                        toReturn.canUseIK = false;
                    }
                }
                return toReturn;
            }

            public override WeaponStats GetStats()
            {
                WeaponStats toReturn = new WeaponStats();
                //Set damage
                toReturn.damage = baseDamage;
                //Set Fire Rate
                toReturn.fireRate = RPM;
                //Set reach
                toReturn.reach = range;
                //Set recoil
                toReturn.recoil = ((Mathf.Abs(recoilPerShotMax.x) + Mathf.Abs(recoilPerShotMax.y) + Mathf.Abs(recoilPerShotMin.x) + Mathf.Abs(recoilPerShotMax.y)) / (float)recoilReturnSpeed);
                return toReturn;
            }

            public override float Sensitivity(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (pb.isBot) return 1f;

                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    if (data.isAimedIn)
                    {
                        if (data.weaponRenderer.cachedUseFullscreenScope)
                        {
                            return Kit_GameSettings.fullScreenAimSensitivity;
                        }
                        else
                        {
                            return Kit_GameSettings.aimSensitivity;
                        }
                    }
                    else
                    {
                        return Kit_GameSettings.hipSensitivity;
                    }
                }
                return 1f;

            }

            public override int WeaponState(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    if (data.bulletsLeft > 0) return 0;
                    else if (data.bulletsLeftToReload > 0) return 1;
                    else return 2;
                }
                return 0;
            }

            public override int GetWeaponType(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    if (data.fireMode == FireMode.Auto)
                    {
                        return 0;
                    }
                    else if (data.fireMode == FireMode.Semi)
                    {
                        if (data.fireTypeMode == FireTypeMode.Simple) return 1;
                        else return 2;
                    }
                    else if (data.fireMode == FireMode.BoltAction)
                    {
                        if (data.fireTypeMode == FireTypeMode.Simple) return 1;
                        else return 2;
                    }
                }
                return 1;
            }

            public override void FirstThirdPersonChanged(Kit_PlayerBehaviour pb, Kit_GameInformation.Perspective perspective, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    //Get runtime data
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    //Activate or deactivate based on bool
                    if (perspective == Kit_GameInformation.Perspective.ThirdPerson)
                    {
                        if (data.weaponRenderer)
                        {
                            data.weaponRenderer.visible = false;
                        }
                        if (data.tpWeaponRenderer)
                        {
                            data.tpWeaponRenderer.visible = true;
                            data.tpWeaponRenderer.shadowsOnly = false;
                        }
                    }
                    else
                    {
                        if (data.weaponRenderer && !data.sniperWeaponHidden)
                        {
                            data.weaponRenderer.visible = true;
                        }
                        if (data.tpWeaponRenderer)
                        {
                            data.tpWeaponRenderer.visible = true;
                            data.tpWeaponRenderer.shadowsOnly = true;
                        }
                    }
                }
            }
            #endregion

            #region Unique functions
            /// <summary>
            /// Fires one shot locally
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="data"></param>
            public void FireOneShot(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                //Update spawn protection
                if (pb.spawnProtection)
                {
                    pb.spawnProtection.GunFired(pb);
                }

                if (pb.photonView.IsMine)
                {
                    //Call network (if semi fire)
                    if (data.fireMode == FireMode.Semi)
                    {
                        pb.photonView.RPC("WeaponSemiFireNetwork", RpcTarget.Others);
                    }
                    else if (data.fireMode == FireMode.BoltAction)
                    {
                        pb.photonView.RPC("WeaponBoltActionFireNetwork", RpcTarget.Others, (data.bulletsLeft == 1 ? 2 : 1)); //2 = Last, 1 = Normal; 0 would be none
                    }
                }

                //Set bolt action data
                if (data.fireMode == FireMode.BoltAction)
                {
                    if (data.bulletsLeft == 1)
                    {
                        //Set delay
                        data.boltActionTime = Time.time + boltActionDelayLast;
                        //Set state
                        data.boltActionState = 2;
                        //Set fire rate
                        data.lastFire = Time.time + boltActionTimeLast;
                    }
                    else
                    {
                        //Set delay
                        data.boltActionTime = Time.time + boltActionDelayNormal;
                        //Set state
                        data.boltActionState = 1;
                        //Set fire rate
                        data.lastFire = Time.time + boltActionTimeNormal;
                    }
                }
                else
                {
                    //Set firerate
                    data.lastFire = Time.time;
                }

                if (pb.isFirstPersonActive && pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson)
                {
                    //Play sound
                    data.soundFire.PlayOneShot(data.weaponRenderer.cachedFireSound);
                    //Play fire animation
                    if (data.bulletsLeft == 1)
                    {
                        //Last fire
                        if (data.weaponRenderer.anim)
                        {
                            //Play animation
                            data.weaponRenderer.anim.Play("Fire Last", data.weaponRenderer.animActionLayer, 0f);

                            for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                            {
                                data.weaponRenderer.animAdditionals[i].Play("Fire Last", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                            }
                        }
                        else if (data.weaponRenderer.legacyAnim)
                        {
                            //Play animation
                            data.weaponRenderer.legacyAnim.Rewind(data.weaponRenderer.legacyAnimData.fireLast);
                            data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.fireLast, PlayMode.StopAll);
                        }
                    }
                    else
                    {
                        if (data.isAiming)
                        {
                            //Normal fire (in aiming mode)
                            if (data.weaponRenderer.anim)
                            {
                                //Play animation
                                data.weaponRenderer.anim.Play("Fire Aim", data.weaponRenderer.animActionLayer, 0f);

                                for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                {
                                    data.weaponRenderer.animAdditionals[i].Play("Fire Aim", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                }
                            }
                            else if (data.weaponRenderer.legacyAnim)
                            {
                                //Play animation
                                data.weaponRenderer.legacyAnim.Rewind(data.weaponRenderer.legacyAnimData.fireAim);
                                data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.fireAim, PlayMode.StopAll);
                            }
                        }
                        else
                        {
                            //Normal fire
                            if (data.weaponRenderer.anim)
                            {
                                //Play animation
                                data.weaponRenderer.anim.Play("Fire", data.weaponRenderer.animActionLayer, 0f);

                                for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                                {
                                    data.weaponRenderer.animAdditionals[i].Play("Fire", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                                }
                            }
                            else if (data.weaponRenderer.legacyAnim)
                            {
                                //Play animation
                                data.weaponRenderer.legacyAnim.Rewind(data.weaponRenderer.legacyAnimData.fire);
                                data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.fire, PlayMode.StopAll);
                            }
                        }
                    }

                    //Play Muzzle Flash Particle System, if assigned
                    if (data.weaponRenderer.muzzleFlash && data.weaponRenderer.cachedMuzzleFlashEnabled)
                    {
                        data.weaponRenderer.muzzleFlash.Play(true);
                    }
                }
                else
                {
                    //Set clip
                    pb.thirdPersonPlayerModel.soundFire.clip = data.tpWeaponRenderer.cachedFireSoundThirdPerson;
                    //Update range
                    pb.thirdPersonPlayerModel.soundFire.maxDistance = data.tpWeaponRenderer.cachedFireSoundThirdPersonMaxRange;
                    //Update sound rolloff
                    pb.thirdPersonPlayerModel.soundFire.SetCustomCurve(AudioSourceCurveType.CustomRolloff, data.tpWeaponRenderer.cachedFireSoundThirdPersonRolloff);
                    //Play
                    pb.thirdPersonPlayerModel.soundFire.PlayOneShot(data.tpWeaponRenderer.cachedFireSoundThirdPerson);

                    //Play Muzzle Flash Particle System, if assigned
                    if (data.tpWeaponRenderer.muzzleFlash && data.tpWeaponRenderer.cachedMuzzleFlashEnabled)
                    {
                        data.tpWeaponRenderer.muzzleFlash.Play(true);
                    }
                }

                //Play third person fire animation
                pb.thirdPersonPlayerModel.PlayWeaponFireAnimation(thirdPersonAnimType);

                //Set shell ejection
                if (shellEjectionPrefab && pb.isFirstPersonActive)
                {
                    data.shellEjectEnabled = true;
                    data.shellEjectNext = Time.time + shellEjectionTime;
                    //The actual ejection is in the CustomUpdate part, so it is coroutine less
                }

                //Subtract bullets
                if (!pb.main.gameInformation.debugEnableUnlimitedBullets)
                    data.bulletsLeft--;

                //Simple fire
                if (data.fireTypeMode == FireTypeMode.Simple)
                {
                    if (data.bulletsMode == BulletMode.Raycast)
                    {
                        //Fire Raycast
                        FireRaycast(pb, data);
                    }
                    else if (data.bulletsMode == BulletMode.Physical)
                    {
                        //Fire Physical Bullet
                        FirePhysicalBullet(pb, data);
                    }
                }
                //Pellet fire
                else if (data.fireTypeMode == FireTypeMode.Pellets)
                {
                    //Count how many have been shot
                    int pelletsShot = 0;
                    while (pelletsShot < data.amountOfPellets)
                    {
                        //Increase amount of shot ones
                        pelletsShot++;
                        if (data.bulletsMode == BulletMode.Raycast)
                        {
                            //Fire Raycast
                            FireRaycast(pb, data);
                        }
                        else if (data.bulletsMode == BulletMode.Physical)
                        {
                            //Fire Physical Bullet
                            FirePhysicalBullet(pb, data);
                        }
                    }
                }

                //Increase spray pattern
                if (data.bulletSpreadMode == SpreadMode.SprayPattern)
                {
                    data.sprayPatternState++;
                }

                //Apply recoil using coroutine helper
                Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.WeaponApplyRecoil(this, data, pb, RandomExtensions.RandomBetweenVector2(data.recoilPerShotMin, data.recoilPerShotMax), data.recoilApplyTime));
            }

            /// <summary>
            /// Fire one burst (local)
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="data"></param>
            void FireBurst(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                //Update spawn protection
                if (pb.spawnProtection)
                {
                    pb.spawnProtection.GunFired(pb);
                }

                if (pb.photonView.IsMine)
                {
                    //Call network and already tell how many shots are going to be fired
                    pb.photonView.RPC("WeaponBurstFireNetwork", RpcTarget.Others, Mathf.Clamp(data.bulletsLeft, 0, data.burstBulletsPerShot));
                }

                //Start Coroutine
                Kit_ScriptableObjectCoroutineHelper.instance.StartCoroutine(Kit_ScriptableObjectCoroutineHelper.instance.WeaponBurstFire(this, data, pb));
            }

            public void FireRaycast(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                if (pb.main.gameInformation.fireShotsLocally || pb.photonView.IsMine)
                {
                    if (data.bulletsPenetrationEnabled)
                    {
                        //Will be set to true again if bullet penetrated through something
                        bool continueLoop = true;

                        //Bullet penetration ability
                        int bulletLifeLeft = data.bulletsPenetrationForce;

                        //So that we may not damage the same player twice
                        List<Kit_PlayerBehaviour> damagedPlayers = new List<Kit_PlayerBehaviour>();

                        RaycastHit hit;
                        Vector3 dir = pb.playerCameraTransform.forward + GetSpread(pb, data);
                        Vector3 pos = pb.playerCameraTransform.position;

                        //Override if third person is active
                        if (pb.isFirstPersonActive && pb.main.gameInformation.thirdPersonCameraShooting && pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.ThirdPerson)
                        {
                            dir = pb.main.mainCamera.transform.forward + GetSpread(pb, data);
                            pos = pb.main.mainCamera.transform.position;
                        }

                        float remainingDistance = data.range;

                        int tries = 0;

                        while (tries < 10 && Physics.Raycast(pos, dir, out hit, remainingDistance, pb.weaponHitLayers.value))
                        {
                            //Add
                            tries++;
                            //Distract distance
                            remainingDistance -= hit.distance;
                            //Set new pos
                            pos = hit.point;
                            if (continueLoop)
                            {
                                //Check if we hit ourselves
                                if (hit.transform.root != pb.transform.root)
                                {
                                    //Set Loop to false
                                    continueLoop = false;
                                    #region Damage
                                    //Check if we hit a player
                                    if (hit.transform.GetComponent<Kit_PlayerDamageMultiplier>() && pb.main.currentGameModeType == 2)
                                    {
                                        Kit_PlayerDamageMultiplier pdm = hit.transform.GetComponent<Kit_PlayerDamageMultiplier>();
                                        if (hit.transform.root.GetComponent<Kit_PlayerBehaviour>())
                                        {
                                            Kit_PlayerBehaviour hitPb = hit.transform.root.GetComponent<Kit_PlayerBehaviour>();
                                            //Check if we damaged this player already
                                            if (!damagedPlayers.Contains(hitPb))
                                            {
                                                //Add player
                                                damagedPlayers.Add(hitPb);
                                                //First check if we can actually damage that player
                                                if (pb.main.currentPvPGameModeBehaviour.ArePlayersEnemies(pb, hitPb))
                                                {
                                                    //Check if he has spawn protection
                                                    if (!hitPb.spawnProtection || hitPb.spawnProtection.CanTakeDamage(hitPb))
                                                    {
                                                        //Apply local damage, sample damage dropoff via distance
                                                        hitPb.LocalDamage(GetDamage(data, Vector3.Distance(pb.playerCameraTransform.position, hit.point)) * pdm.damageMultiplier, gameGunID, pb.transform.position, dir, data.ragdollForce, hit.point, pdm.ragdollId, pb.isBot, pb.id);
                                                        if (pb.isFirstPersonActive)
                                                        {
                                                            //Since we hit a player, show the hitmarker
                                                            pb.main.hud.DisplayHitmarker();
                                                        }
                                                    }
                                                    else if (pb.isFirstPersonActive)
                                                    {
                                                        //We hit a player but his spawn protection is active
                                                        pb.main.hud.DisplayHitmarkerSpawnProtected();
                                                    }
                                                }

                                                if (pb.photonView.IsMine)
                                                {
                                                    if (!pb.gameInformation.fireShotsLocally)
                                                    {
                                                        //Tell other players we hit something
                                                        pb.photonView.RPC("WeaponRaycastHit", RpcTarget.All, hit.point, hit.normal, hit.collider.tag);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    #region Particles and Interface
                                    else
                                    {
                                        if (pb.photonView.IsMine)
                                        {
                                            if (!pb.gameInformation.fireShotsLocally)
                                            {
                                                //Tell other players we hit something
                                                pb.photonView.RPC("WeaponRaycastHit", RpcTarget.All, hit.point, hit.normal, hit.collider.tag);
                                            }
                                        }

                                        if (hit.collider.GetComponentInParent<IKitDamageable>() != null)
                                        {
                                            if (hit.collider.GetComponentInParent<IKitDamageable>().LocalDamage(GetDamage(data, Vector3.Distance(pb.playerCameraTransform.position, hit.point)), gameGunID, pb.transform.position, dir, data.ragdollForce, hit.point, pb.isBot, pb.id))
                                            {

                                                if (pb.isFirstPersonActive)
                                                {
                                                    //Since we hit a player, show the hitmarker
                                                    pb.main.hud.DisplayHitmarker();
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    #region Bullet Penetration
                                    //Check if this object can be penetrated
                                    if (hit.collider.GetComponent<Kit_PenetrateableObject>())
                                    {
                                        Kit_PenetrateableObject penetration = hit.collider.GetComponent<Kit_PenetrateableObject>();
                                        if (bulletLifeLeft >= penetration.cost)
                                        {
                                            //We penetrated, subtract cost
                                            bulletLifeLeft -= penetration.cost;
                                            //Set bool
                                            continueLoop = true;
                                        }
                                    }

                                    //Abort loop if nothing was penetrated
                                    if (!continueLoop) break;
                                    #endregion
                                }
                            }
                        }
                    }
                    else
                    {
                        RaycastHit hit;
                        Vector3 dir = pb.playerCameraTransform.forward + GetSpread(pb, data);
                        Vector3 pos = pb.playerCameraTransform.position;

                        //Override if third person is active
                        if (pb.isFirstPersonActive && pb.main.gameInformation.thirdPersonCameraShooting && pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.ThirdPerson)
                        {
                            dir = pb.main.mainCamera.transform.forward + GetSpread(pb, data);
                            pos = pb.main.mainCamera.transform.position;
                        }

                        RaycastHit[] hits = Physics.RaycastAll(pos, dir, data.range, pb.weaponHitLayers.value).OrderBy(h => h.distance).ToArray();
                        for (int i = 0; i < hits.Length; i++)
                        {
                            hit = hits[i];
                            //Check if we hit ourselves
                            if (hit.transform.root != pb.transform.root)
                            {
                                //Check if we hit a player
                                if (hit.transform.GetComponent<Kit_PlayerDamageMultiplier>())
                                {
                                    Kit_PlayerDamageMultiplier pdm = hit.transform.GetComponent<Kit_PlayerDamageMultiplier>();
                                    if (hit.transform.root.GetComponent<Kit_PlayerBehaviour>())
                                    {
                                        Kit_PlayerBehaviour hitPb = hit.transform.root.GetComponent<Kit_PlayerBehaviour>();
                                        //First check if we can actually damage that player
                                        if (pb.main.currentPvPGameModeBehaviour.ArePlayersEnemies(pb, hitPb))
                                        {
                                            //Check if he has spawn protection
                                            if (!hitPb.spawnProtection || hitPb.spawnProtection.CanTakeDamage(hitPb))
                                            {
                                                //Apply local damage, sample damage dropoff via distance
                                                hitPb.LocalDamage(GetDamage(data, Vector3.Distance(pb.playerCameraTransform.position, hit.point)) * pdm.damageMultiplier, gameGunID, pb.transform.position, dir, data.ragdollForce, hit.point, pdm.ragdollId, pb.isBot, pb.id);
                                                if (pb.isFirstPersonActive)
                                                {
                                                    //Since we hit a player, show the hitmarker
                                                    pb.main.hud.DisplayHitmarker();
                                                }
                                            }
                                            else if (pb.isFirstPersonActive)
                                            {
                                                //We hit a player but his spawn protection is active
                                                pb.main.hud.DisplayHitmarkerSpawnProtected();
                                            }
                                        }
                                        if (pb.photonView.IsMine)
                                        {
                                            if (!pb.gameInformation.fireShotsLocally)
                                            {
                                                //Tell other players we hit something
                                                pb.photonView.RPC("WeaponRaycastHit", RpcTarget.All, hit.point, hit.normal, hit.collider.tag);
                                            }
                                        }
                                        break;
                                    }
                                }
                                else
                                {
                                    if (pb.photonView.IsMine)
                                    {
                                        if (!pb.gameInformation.fireShotsLocally)
                                        {
                                            //Tell other players we hit something
                                            pb.photonView.RPC("WeaponRaycastHit", RpcTarget.All, hit.point, hit.normal, hit.collider.tag);
                                        }
                                    }

                                    if (hits[i].collider.GetComponentInParent<IKitDamageable>() != null)
                                    {
                                        if (hits[i].collider.GetComponentInParent<IKitDamageable>().LocalDamage(GetDamage(data, Vector3.Distance(pb.playerCameraTransform.position, hit.point)), gameGunID, pb.transform.position, dir, data.ragdollForce, hit.point, pb.isBot, pb.id))
                                        {

                                            if (pb.isFirstPersonActive)
                                            {
                                                //Since we hit a player, show the hitmarker
                                                pb.main.hud.DisplayHitmarker();
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }


                    //Because these are calculated on the master client, we need to call shoot here too
                    if (pb.main.minimap)
                    {
                        pb.main.minimap.PlayerShoots(pb, data.tpWeaponRenderer.isWeaponSilenced);
                    }
                }
            }

            public override void NetworkPhysicalBulletFired(Kit_PlayerBehaviour pb, Vector3 pos, Vector3 dir, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    //Get runtime data
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    //Instantiate Game Object
                    GameObject bullet = pb.main.objectPooling.GetInstantiateable(data.bulletPrefab, pos, Quaternion.LookRotation(dir));
                    //Call setup
                    bullet.GetComponent<Kit_BulletBase>().Setup(pb.main, this, data, pb, dir);
                }
            }

            public void FirePhysicalBullet(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                if (pb.main.gameInformation.fireShotsLocally || pb.photonView.IsMine)
                {
                    Vector3 dir = pb.playerCameraTransform.forward + GetSpread(pb, data);
                    Vector3 pos = pb.playerCameraTransform.position;

                    //Override if third person is active
                    if (pb.isFirstPersonActive && pb.main.gameInformation.thirdPersonCameraShooting && pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.ThirdPerson)
                    {
                        dir = pb.main.mainCamera.transform.forward + GetSpread(pb, data);
                        pos = pb.main.mainCamera.transform.position;
                    }

                    if (pb.photonView.IsMine)
                    {
                        //Send RPC
                        pb.photonView.RPC("WeaponFirePhysicalBulletOthers", RpcTarget.Others, pos, dir);
                    }

                    //Instantiate Game Object
                    GameObject bullet = pb.main.objectPooling.GetInstantiateable(data.bulletPrefab, pos, Quaternion.LookRotation(dir));
                    //Call setup
                    bullet.GetComponent<Kit_BulletBase>().Setup(pb.main, this, data, pb, dir);

                    //Because these are calculated on the master client, we need to call shoot here too
                    if (pb.main.minimap)
                    {
                        pb.main.minimap.PlayerShoots(pb, data.tpWeaponRenderer.isWeaponSilenced);
                    }
                }
            }

            /// <summary>
            /// Returns the damage for given distance
            /// </summary>
            /// <param name="distance"></param>
            /// <returns></returns>
            float GetDamage(WeaponControllerRuntimeData data, float distance)
            {
                return data.baseDamage * data.damageDropoff.Evaluate(distance);
            }

            /// <summary>
            /// Returns a direction (for offset) based on data and this behaviour's stats
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="data"></param>
            Vector3 GetSpread(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                float velocity = pb.movement.GetVelocity(pb).magnitude;

                #region Hip Spread
                Vector3 spreadHip = Vector3.zero;
                spreadHip.x = Random.Range(-data.bulletSpreadHipBase, data.bulletSpreadHipBase);
                spreadHip.y = Random.Range(-data.bulletSpreadHipBase, data.bulletSpreadHipBase);
                spreadHip.z = Random.Range(-data.bulletSpreadHipBase, data.bulletSpreadHipBase);

                //Velocity add
                if (velocity > 0)
                {
                    spreadHip.x += RandomExtensions.RandomPosNeg() * data.bulletSpreadHipVelocityAdd * (velocity / data.bulletSpreadHipVelocityReference);
                    spreadHip.y += RandomExtensions.RandomPosNeg() * data.bulletSpreadHipVelocityAdd * (velocity / data.bulletSpreadHipVelocityReference);
                    spreadHip.z += RandomExtensions.RandomPosNeg() * data.bulletSpreadHipVelocityAdd * (velocity / data.bulletSpreadHipVelocityReference);
                }
                #endregion

                #region Aim Spread
                Vector3 spreadAim = Vector3.zero;
                spreadAim.x = Random.Range(-data.bulletSpreadAimBase, data.bulletSpreadAimBase);
                spreadAim.y = Random.Range(-data.bulletSpreadAimBase, data.bulletSpreadAimBase);
                spreadAim.z = Random.Range(-data.bulletSpreadAimBase, data.bulletSpreadAimBase);

                //Velocity add
                if (velocity > 0)
                {
                    spreadAim.x += RandomExtensions.RandomPosNeg() * data.bulletSpreadAimVelocityAdd * (velocity / data.bulletSpreadAimVelocityReference);
                    spreadAim.y += RandomExtensions.RandomPosNeg() * data.bulletSpreadAimVelocityAdd * (velocity / data.bulletSpreadAimVelocityReference);
                    spreadAim.z += RandomExtensions.RandomPosNeg() * data.bulletSpreadAimVelocityAdd * (velocity / data.bulletSpreadAimVelocityReference);
                }
                #endregion

                if (data.bulletSpreadMode == SpreadMode.Simple)
                {
                    //Interpolate between both based on aiming progress
                    return Vector3.Lerp(spreadHip, spreadAim, data.aimingProgress);
                }
                else if (data.bulletSpreadMode == SpreadMode.SprayPattern && data.bulletSpreadSprayPattern.Length > 0)
                {
                    //Interpolate between both based on aiming progress and add spray pattern
                    return Vector3.Lerp(spreadHip, spreadAim, data.aimingProgress) + data.bulletSpreadSprayPattern[Mathf.Clamp(Mathf.RoundToInt(data.sprayPatternState), 0, data.bulletSpreadSprayPattern.Length - 1)];
                }
                else
                {
                    //Interpolate between both based on aiming progress
                    return Vector3.Lerp(spreadHip, spreadAim, data.aimingProgress);
                }
            }

            float GetCrosshairSize(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                if (crosshairEnabled)
                {
                    //This is essentially the bullet spread part without the random factors.
                    float velocity = pb.movement.GetVelocity(pb).magnitude;
                    float spreadHip = data.bulletSpreadHipBase;

                    //Velocity add
                    if (velocity > 0)
                    {
                        spreadHip += data.bulletSpreadHipVelocityAdd * (velocity / data.bulletSpreadHipVelocityReference);
                    }

                    return Mathf.Lerp(spreadHip, 0f, data.aimingProgress) * crosshairSizeMultiplier;
                }
                //If crosshair is not enablewd
                else
                {
                    //Return size of 0 which will disable the crosshair
                    return 0f;
                }
            }

            void DryFire(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                if (pb.isFirstPersonActive)
                {
                    //Play sound (on Other channel so fire sound can still play)
                    data.soundOther.PlayOneShot(dryFireSound);
                    //Play Dry Fire animation
                    if (data.weaponRenderer.anim)
                    {
                        //Play animation
                        data.weaponRenderer.anim.Play("Dry Fire", data.weaponRenderer.animActionLayer, 0f);

                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                        {
                            data.weaponRenderer.animAdditionals[i].Play("Dry Fire", data.weaponRenderer.animAdditionalsActionLayer[i], 0f);
                        }
                    }
                    else if (data.weaponRenderer.legacyAnim && data.weaponRenderer.legacyAnimData.dryFire != "")
                    {
                        //Play animation
                        data.weaponRenderer.legacyAnim.Rewind(data.weaponRenderer.legacyAnimData.dryFire);
                        data.weaponRenderer.legacyAnim.Play(data.weaponRenderer.legacyAnimData.dryFire, PlayMode.StopAll);
                    }
                }
                //Set firerate
                data.lastFire = Time.time + 1f; //+1 so you don't dry fire too quick
            }

            /// <summary>
            /// Ejects a single shell
            /// </summary>
            /// <param name="pb"></param>
            /// <param name="data"></param>
            void EjectShell(Kit_PlayerBehaviour pb, WeaponControllerRuntimeData data)
            {
                if (shellEjectionPrefab & data.weaponRenderer.shellEjectTransform)
                {
                    GameObject shellObj = null;
                    if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson)
                    {
                        shellObj = pb.main.objectPooling.GetInstantiateable(shellEjectionPrefab, data.weaponRenderer.shellEjectTransform.position, data.weaponRenderer.shellEjectTransform.rotation);
                    }
                    else
                    {
                        shellObj = pb.main.objectPooling.GetInstantiateable(shellEjectionPrefab, data.tpWeaponRenderer.shellEjectTransform.position, data.weaponRenderer.shellEjectTransform.rotation);
                    }
                    if (!shellObj) return;
                    //Apply force
                    Rigidbody rb = shellObj.GetComponent<Rigidbody>();
                    rb.isKinematic = false;
                    rb.velocity = pb.movement.GetVelocity(pb);
                    //Add force
                    rb.AddRelativeForce(RandomExtensions.RandomBetweenVector3(shellEjectionMinForce, shellEjectionMaxForce));
                    //Add torque
                    rb.AddRelativeTorque(RandomExtensions.RandomBetweenVector3(shellEjectionMinTorque, shellEjectionMaxTorque));
                }
            }

            public override bool IsWeaponAiming(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    return data.isAiming; //Just relay
                }
                return false;
            }

            public override float GetAimingPercentage(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    return data.aimingProgress;
                }
                return base.GetAimingPercentage(pb, runtimeData);
            }

            public override bool ForceIntoFirstPerson(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    if (data.weaponRenderer)
                        return data.isAiming && data.weaponRenderer.cachedUseFullscreenScope; //Just relay
                }
                return false;
            }

            public override float AimInTime(Kit_PlayerBehaviour pb, object runtimeData)
            {
                return aimInTime;
            }

            public override float SpeedMultiplier(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    //Lerp from base multiplier to base multiplier multiplied with the aim speed multiplier
                    return Mathf.Lerp(data.speedMultiplierBase, data.speedMultiplierBase * aimSpeedMultiplier, data.aimingProgress);
                }
                return 1f;
            }

            public override void OnAmmoPickup(Kit_PlayerBehaviour pb, Kit_AmmoPickup pickup, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    //Add to bullets
                    data.bulletsLeftToReload += pickup.amountOfClipsToPickup * data.bulletsPerMag;
                }
            }

            public override bool IsWeaponFull(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    return data.bulletsLeftToReload >= data.bulletsToReloadAtStart;
                }

                return base.IsWeaponFull(pb, runtimeData);
            }

            public override void RestockAmmo(Kit_PlayerBehaviour pb, object runtimeData)
            {
                if (runtimeData != null && runtimeData.GetType() == typeof(WeaponControllerRuntimeData))
                {
                    WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;
                    //Reset ammo
                    data.bulletsLeftToReload = data.bulletsToReloadAtStart;
                }
            }
            #endregion


            public override void BeginSpectating(Kit_PlayerBehaviour pb, object runtimeData, int[] attachments)
            {
                WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;

                //Create First Person renderer if it doesn't exist already
                if (!data.weaponRenderer)
                {
                    //Setup root for this weapon
                    GameObject root = new GameObject("Weapon root");
                    root.transform.parent = pb.weaponsGo; //Set root
                    root.transform.localPosition = Vector3.zero; //Reset position
                    root.transform.localRotation = Quaternion.identity; //Reset rotation
                    root.transform.localScale = Vector3.one; //Reset scale

                    //Setup generic animations
                    GameObject genericAnimations = Instantiate(genericGunAnimatorControllerPrefab);
                    genericAnimations.transform.parent = root.transform;
                    genericAnimations.transform.localPosition = Vector3.zero; //Reset position
                    genericAnimations.transform.localRotation = Quaternion.identity; //Reset rotation
                    genericAnimations.transform.localScale = Vector3.one; //Reset scale

                    //Get animator
                    Animator anim = genericAnimations.GetComponent<Animator>(); ;
                    anim.Play("Idle");
                    data.genericAnimator = anim;

                    //Setup aiming transform
                    GameObject aimTrans = new GameObject("Weapon aiming");
                    aimTrans.transform.parent = genericAnimations.transform; //Set root
                    aimTrans.transform.localPosition = Vector3.zero; //Reset position
                    aimTrans.transform.localRotation = Quaternion.identity; //Reset rotation
                    aimTrans.transform.localScale = Vector3.one; //Reset scale

                    //Assign it
                    data.aimingTransform = aimTrans.transform;

                    //Delay transform
                    GameObject delayTrans = new GameObject("Weapon delay");
                    delayTrans.transform.parent = aimTrans.transform; //Set root
                    delayTrans.transform.localPosition = Vector3.zero; //Reset position
                    delayTrans.transform.localRotation = Quaternion.identity; //Reset rotation
                    delayTrans.transform.localScale = Vector3.one; //Reset scale

                    //Assign it
                    data.weaponDelayTransform = delayTrans.transform;

                    //Delay transform
                    GameObject fallTrans = new GameObject("Weapon fall");
                    fallTrans.transform.parent = delayTrans.transform; //Set root
                    fallTrans.transform.localPosition = Vector3.zero; //Reset position
                    fallTrans.transform.localRotation = Quaternion.identity; //Reset rotation
                    fallTrans.transform.localScale = Vector3.one; //Reset scale

                    //Assign it
                    data.weaponFallTransform = fallTrans.transform;

                    //Get Fire Audio (Needs to be consistent)
                    if (pb.weaponsGo.GetComponent<AudioSource>()) data.soundFire = pb.weaponsGo.GetComponent<AudioSource>();
                    else data.soundFire = pb.weaponsGo.gameObject.AddComponent<AudioSource>();

                    //Setup reload sound
                    GameObject soundReload = new GameObject("SoundReload"); //Create
                    soundReload.transform.parent = root.transform;
                    soundReload.transform.localPosition = Vector3.zero; //Reset position
                    soundReload.transform.localRotation = Quaternion.identity; //Reset rotation
                    soundReload.transform.localScale = Vector3.one; //Reset scale
                                                                    //Add audio source
                    data.soundReload = soundReload.AddComponent<AudioSource>();

                    //Setup other sound
                    GameObject soundOther = new GameObject("SoundOther"); //Create
                    soundOther.transform.parent = root.transform;
                    soundOther.transform.localPosition = Vector3.zero; //Reset position
                    soundOther.transform.localRotation = Quaternion.identity; //Reset rotation
                    soundOther.transform.localScale = Vector3.one; //Reset scale
                                                                   //Add audio source
                    data.soundOther = soundOther.AddComponent<AudioSource>();

                    //Setup the first person prefab
                    GameObject fpRuntime = Instantiate(firstPersonPrefab, fallTrans.transform, false);
                    fpRuntime.transform.localScale = Vector3.one; //Reset scale

                    //Setup renderer
                    data.weaponRenderer = fpRuntime.GetComponent<Kit_WeaponRenderer>();
                    //Set Attachments
                    data.weaponRenderer.SetAttachments(attachments, this, pb, null);

                    //Play Dependent arms
                    if (data.weaponRenderer.playerModelDependentArmsEnabled && pb.thirdPersonPlayerModel.firstPersonArmsPrefab.Contains(data.weaponRenderer.playerModelDependentArmsKey))
                    {
                        //Create Prefab
                        GameObject fpArms = Instantiate(pb.thirdPersonPlayerModel.firstPersonArmsPrefab[data.weaponRenderer.playerModelDependentArmsKey], fallTrans.transform, false);
                        //Get Arms
                        Kit_FirstPersonArms fpa = fpArms.GetComponent<Kit_FirstPersonArms>();

#if INTEGRATION_FPV2
                        //Set shaders
                        FirstPersonView.ShaderMaterialSolution.FPV_SM_Object armsObj = fpArms.GetComponent<FirstPersonView.ShaderMaterialSolution.FPV_SM_Object>();
                        if (armsObj)
                        {
                            //Set as first person object
                            armsObj.SetAsFirstPersonObject();
                        }
                        else
                        {
                            Debug.LogError("FPV2 is enabled but first person prefab does not have FPV_SM_Object assigned!");
                        }
#elif INTEGRATION_FPV3
                        //Set shaders
                        FirstPersonView.FPV_Object armsObj = fpArms.GetComponent<FirstPersonView.FPV_Object>();
                        if (armsObj)
                        {
                            //Set as first person object
                            armsObj.SetAsFirstPersonObject();
                        }
                        else
                        {
                            Debug.LogError("FPV3 is enabled but first person prefab does not have FPV_Object assigned!");
                        }
#endif

                        //Reparent
                        for (int i = 0; i < fpa.reparents.Length; i++)
                        {
                            if (fpa.reparents[i])
                            {
                                fpa.reparents[i].transform.parent = data.weaponRenderer.playerModelDependentArmsRoot;
                            }
                            else
                            {
                                Debug.LogWarning("First Person Arm Renderer at index " + i + " is not assigned", fpa);
                            }
                        }
                        //Merge Array
                        data.weaponRenderer.allWeaponRenderers = data.weaponRenderer.allWeaponRenderers.Concat(fpa.renderers).ToArray();
                        //Rebind so that the animator animates our freshly reparented transforms too!
                        if (data.weaponRenderer.anim)
                        {
                            data.weaponRenderer.anim.Rebind();
                        }

                        for (int i = 0; i < data.weaponRenderer.animAdditionals.Length; i++)
                        {
                            data.weaponRenderer.animAdditionals[i].Rebind();
                        }
                    }

                    //Hide
                    data.weaponRenderer.visible = false;

#if INTEGRATION_FPV2
                    //Set shaders
                    FirstPersonView.ShaderMaterialSolution.FPV_SM_Object obj = fpRuntime.GetComponent<FirstPersonView.ShaderMaterialSolution.FPV_SM_Object>();
                    if (obj)
                    {
                        //Set as first person object
                        obj.SetAsFirstPersonObject();
                    }
                    else
                    {
                        Debug.LogError("FPV2 is enabled but first person prefab does not have FPV_SM_Object assigned!");
                    }
#elif INTEGRATION_FPV3
                    //Set shaders
                    FirstPersonView.FPV_Object obj = fpRuntime.GetComponent<FirstPersonView.FPV_Object>();
                    if (obj)
                    {
                        //Set as first person object
                        obj.SetAsFirstPersonObject();
                    }
                    else
                    {
                        Debug.LogError("FPV3 is enabled but first person prefab does not have FPV_Object assigned!");
                    }
#endif


                    //Add to the list
                    data.instantiatedObjects.Add(root);
                }

                if (pb.looking.GetPerspective(pb) == Kit_GameInformation.Perspective.FirstPerson)
                {
                    //Show if selected
                    data.weaponRenderer.visible = data.isSelected;

                    //Hide tp
                    data.tpWeaponRenderer.shadowsOnly = true;
                }
                else
                {
                    //FP is definitely hidden, TP is already in the state that it should be in
                    data.weaponRenderer.visible = false;
                }
            }

            public override void EndSpectating(Kit_PlayerBehaviour pb, object runtimeData)
            {
                //Get data
                WeaponControllerRuntimeData data = runtimeData as WeaponControllerRuntimeData;

                //Hide weapon renderer if present
                if (data.weaponRenderer)
                {
                    data.weaponRenderer.visible = false;
                }

                data.tpWeaponRenderer.shadowsOnly = false;
            }
        }
    }
}
