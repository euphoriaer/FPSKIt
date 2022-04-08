using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    public class Kit_ExplodeableBarrel : MonoBehaviourPun, IKitDamageable, IPunObservable
    {
        /// <summary>
        /// Explosion prefab
        /// </summary>
        public GameObject explosion;
        /// <summary>
        /// Hitpoints at start.
        /// </summary>
        public float startHitPoints = 100f;
        /// <summary>
        /// Current hitpoints
        /// </summary>
        private float hitPoints = 100f;
        /// <summary>
        /// If set to > 0, the hitpoints will decrease by this factor after they have been damaged once
        /// </summary>
        public float decreaseHitPointsAfterDamaged = 10f;
        /// <summary>
        /// This particle system will be played when it was damaged
        /// </summary>
        public ParticleSystem playWhenDamaged;

        /// <summary>
        /// Was the player who destroyed this barrel a bot?
        /// </summary>
        private bool destroyedByBot;
        /// <summary>
        /// The id of the player that destroyed the barrel
        /// </summary>
        private int destroyedById = -1;

        void Start()
        {
            if (photonView.IsMine)
            {
                hitPoints = startHitPoints;
            }
        }

        void Update()
        {
            if (hitPoints < startHitPoints)
            {
                if (photonView.IsMine)
                {
                    if (decreaseHitPointsAfterDamaged > 0)
                    {
                        hitPoints -= Time.deltaTime * decreaseHitPointsAfterDamaged;

                        if (hitPoints <= 0)
                        {
                            PhotonNetwork.Destroy(gameObject);
                        }
                    }
                }

                if (playWhenDamaged)
                {
                    if (!playWhenDamaged.isPlaying)
                    {
                        playWhenDamaged.Play(true);
                    }
                }
            }
        }

        bool IKitDamageable.LocalDamage(float dmg, int gunID, Vector3 shotPos, Vector3 forward, float force, Vector3 hitPos, bool shotBot, int shotId)
        {
            if (photonView.Owner == null)
                photonView.RPC("DamageBarrel", RpcTarget.MasterClient, dmg, shotBot, shotId);
            else
                photonView.RPC("DamageBarrel", photonView.Owner, dmg, shotBot, shotId);

            return true;
        }


        [PunRPC]
        public void DamageBarrel(float dmg, bool shotBot, int shotId)
        {
            if (photonView.IsMine)
            {
                hitPoints -= dmg;
                destroyedByBot = shotBot;
                destroyedById = shotId;

                if (hitPoints <= 0)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }

        void OnDestroy()
        {
            if (photonView && explosion && !Kit_SceneSyncer.instance.isLoading && PhotonNetwork.InRoom)
            {
                GameObject go = Instantiate(explosion, transform.position, transform.rotation);

                if (go.GetComponent<Kit_Explosion>())
                {
                    go.GetComponent<Kit_Explosion>().Explode(photonView.IsMine, destroyedByBot, destroyedById, "Barrel");
                }
            }
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(hitPoints);
            }
            else
            {
                hitPoints = (float)stream.ReceiveNext();
            }
        }
    }
}