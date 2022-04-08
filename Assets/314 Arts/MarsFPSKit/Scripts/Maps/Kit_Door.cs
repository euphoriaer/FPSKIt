using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    public enum DoorType { Rotate, Animated }

    [RequireComponent(typeof(PhotonView))]
    /// <summary>
    /// Implements a collision detected door that can be opened by curve or animation
    /// </summary>
    public class Kit_Door : Kit_InteractableObject, IPunObservable
    {
        /// <summary>
        /// How will this door be opened (technically)?
        /// </summary>
        public DoorType typeOfDoor;
        /// <summary>
        /// Transform that will be rotated
        /// </summary>
        public Transform transformToRotate;
        /// <summary>
        /// Rotation of door when closed
        /// </summary>
        public Vector3 rotationClosed;
        /// <summary>
        /// Rotation of door when opened
        /// </summary>
        public Vector3 rotationOpened;

        public Animator anim;
        /// <summary>
        /// Name of animation that is played. Needs to be open before animationSplitTime and close after that
        /// </summary>
        public string animationName;
        /// <summary>
        /// Time at which animation is split
        /// </summary>
        public float animationSplitTime = 0.5f;
        /// <summary>
        /// So that door may be closed / opened at any time, we need to play correct animation back
        /// </summary>
        public bool animationIsOverSplitTime;

        /// <summary>
        /// How long does it take to open door?
        /// </summary>
        public float openTime = 1f;
        /// <summary>
        /// How long does it take to close door?
        /// </summary>
        public float closeTime = 1f;
        /// <summary>
        /// Door state (0 = Closed 1 = Open)
        /// </summary>
        private float progress;

        /// <summary>
        /// Collider used for opening. Can be null but door will then move / clip through things
        /// </summary>
        public Kit_DoorCollider doorColliderOpening;
        /// <summary>
        /// Collider used for closing. Can be null but door will then move / clip through things
        /// </summary>
        public Kit_DoorCollider doorColliderClosing;

        /// <summary>
        /// Audio Source to 
        /// </summary>
        public AudioSource doorSoundSource;
        /// <summary>
        /// Sound of door openin
        /// </summary>
        public AudioClip doorOpeningSound;
        /// <summary>
        /// Sound of door closing
        /// </summary>
        public AudioClip doorClosingSound;

        /// <summary>
        /// Is this door open?
        /// </summary>
        public bool isOpen;

        /// <summary>
        /// Progress used to smooth sync
        /// </summary>
        private float syncProgress;

        void Awake()
        {
            //Make sure this is unparented. It should always be on the root.
            transform.parent = null;
        }

        void Start()
        {
            if (typeOfDoor == DoorType.Animated)
            {
                //Set animator speed to 0
                anim.speed = 0f;
            }

            //Set close sound
            if (doorSoundSource)
            {
                doorSoundSource.clip = doorClosingSound;
            }
        }

        void Update()
        {
            //Sync..
            if (!photonView.IsMine)
            {
                //progress = Mathf.MoveTowards(progress, syncProgress, Time.deltaTime * 10f);

                if (syncProgress > progress)
                {
                    progress += Time.deltaTime / openTime;
                    progress = Mathf.Clamp(progress, 0, syncProgress);
                }
                else
                {
                    progress -= Time.deltaTime / closeTime;
                    progress = Mathf.Clamp(progress, syncProgress, 1f);
                }
            }

            if (typeOfDoor == DoorType.Rotate)
            {
                if (isOpen)
                {
                    if (photonView.IsMine)
                    {
                        if (doorColliderOpening)
                        {
                            //Rotate to open position
                            if (!doorColliderOpening.somethingInWay && progress < 1f) progress += Time.deltaTime / openTime;
                        }
                        else
                        {
                            //Rotate to open position
                            if (progress < 1f) progress += Time.deltaTime / openTime;
                        }
                    }

                    //Check for sound
                    if (doorSoundSource)
                    {
                        if (doorSoundSource.clip != doorOpeningSound)
                        {
                            doorSoundSource.clip = doorOpeningSound;
                            doorSoundSource.Play();
                        }
                    }
                }
                else
                {
                    if (photonView.IsMine)
                    {
                        if (doorColliderClosing)
                        {
                            //Rotate to close position
                            if (!doorColliderClosing.somethingInWay && progress > 0f) progress -= Time.deltaTime / closeTime;
                        }
                        else
                        {
                            //Rotate to close position
                            if (progress > 0f) progress -= Time.deltaTime / closeTime;
                        }
                    }

                    //Check for sound
                    if (doorSoundSource)
                    {
                        if (doorSoundSource.clip != doorClosingSound)
                        {
                            doorSoundSource.clip = doorClosingSound;
                            doorSoundSource.Play();
                        }
                    }
                }

                progress = Mathf.Clamp01(progress);

                //Slerp
                transformToRotate.localRotation = Quaternion.Slerp(Quaternion.Euler(rotationClosed), Quaternion.Euler(rotationOpened), progress);
            }
            else if (typeOfDoor == DoorType.Animated)
            {
                if (isOpen)
                {
                    if (photonView.IsMine)
                    {
                        if (doorColliderOpening)
                        {
                            //Rotate to open position
                            if (!doorColliderOpening.somethingInWay && progress < 1f) progress += Time.deltaTime / openTime;
                        }
                        else
                        {
                            //Rotate to open position
                            if (progress < 1f) progress += Time.deltaTime / openTime;
                        }
                    }

                    if (animationIsOverSplitTime)
                    {
                        anim.Play(animationName, 0, Mathf.Lerp(1f, animationSplitTime, progress));
                    }
                    else
                    {
                        anim.Play(animationName, 0, Mathf.Lerp(0f, animationSplitTime, progress));
                    }

                    if (photonView.IsMine)
                    {
                        if (Mathf.Approximately(progress, 1f)) animationIsOverSplitTime = true;
                    }

                    //Check for sound
                    if (doorSoundSource)
                    {
                        if (doorSoundSource.clip != doorOpeningSound)
                        {
                            doorSoundSource.clip = doorOpeningSound;
                            doorSoundSource.Play();
                        }
                    }
                }
                else
                {
                    if (photonView.IsMine)
                    {
                        if (doorColliderClosing)
                        {
                            //Rotate to close position
                            if (!doorColliderClosing.somethingInWay && progress > 0f) progress -= Time.deltaTime / closeTime;
                        }
                        else
                        {
                            //Rotate to close position
                            if (progress > 0f) progress -= Time.deltaTime / closeTime;
                        }
                    }

                    if (animationIsOverSplitTime)
                    {
                        anim.Play(animationName, 0, Mathf.Lerp(1f, animationSplitTime, progress));
                    }
                    else
                    {
                        anim.Play(animationName, 0, Mathf.Lerp(0f, animationSplitTime, progress));
                    }

                    if (photonView.IsMine)
                    {
                        if (Mathf.Approximately(progress, 0f)) animationIsOverSplitTime = false;
                    }

                    //Check for sound
                    if (doorSoundSource)
                    {
                        if (doorSoundSource.clip != doorClosingSound)
                        {
                            doorSoundSource.clip = doorClosingSound;
                            doorSoundSource.Play();
                        }
                    }
                }

                progress = Mathf.Clamp01(progress);
            }
        }

        public override void Interact(Kit_PlayerBehaviour who)
        {
            //Send state change rpc!
            photonView.RPC("ChangeState", RpcTarget.MasterClient);
        }

        [PunRPC]
        public void ChangeState()
        {
            //Someone requested we change state
            if (PhotonNetwork.IsMasterClient)
            {
                isOpen = !isOpen;
            }
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(isOpen);
                stream.SendNext(progress);
                stream.SendNext(animationIsOverSplitTime);
            }
            else
            {
                isOpen = (bool)stream.ReceiveNext();
                syncProgress = (float)stream.ReceiveNext();
                animationIsOverSplitTime = (bool)stream.ReceiveNext();
            }
        }
    }
}