using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace MarsFPSKit
{
    /// <summary>
    /// This script is the actual smoke grenade
    /// </summary>
    public class Kit_GrenadeSmoke : Photon.Pun.MonoBehaviourPun, IPunObservable
    {
        /// <summary>
        /// Time until the grenade will throw smoke
        /// </summary>
        public float timeUntilSmoke = 5f;

        /// <summary>
        /// Time until the grenade is destroyed
        /// </summary>
        public float timeUntilDestroy = 20f;

        /// <summary>
        /// Rigidbody of this grenade!
        /// </summary>
        public Rigidbody rb;

        /// <summary>
        /// The smoke!!!
        /// </summary>
        public ParticleSystem smoke;
        /// <summary>
        /// Master said to smoke?
        /// </summary>
        private bool smokeFired;
        /// <summary>
        /// Was the particle system set to play?
        /// </summary>
        private bool wasSmokeFired;

        IEnumerator Start()
        {
            if (photonView.IsMine)
            {
                //Should be at start, but just to make sure!
                rb.isKinematic = false;
                //Wait
                yield return new WaitForSeconds(timeUntilSmoke);
                smokeFired = true;
                yield return new WaitForSeconds(timeUntilDestroy);
                //Then just destroy
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                rb.isKinematic = true;
            }
        }

        void Update()
        {
            if (smokeFired && smokeFired != wasSmokeFired)
            {
                smoke.transform.up = Vector3.up;
                wasSmokeFired = smokeFired;
                smoke.Play(true);
            }
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(smokeFired);
            }
            else
            {
                smokeFired = (bool)stream.ReceiveNext();
            }
        }
    }
}