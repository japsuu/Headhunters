using System;
using System.Collections;
using Headhunters.Misc;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Headhunters.Networking.Interactions
{
    public class NetworkCarriableObject : NetworkCarriableBase
    {
        [Range(0f, 2f)]
        [SerializeField]
        private float playerCollisionEnableDelay = 0.2f;
        
        [SerializeField]
        private Vector2 dropForceRange = new Vector2(20f, 60f);

        [SerializeField]
        private Vector2 dropTorqueRange = new Vector2(-50f, 50f);

        protected Rigidbody mainRb;
        protected Rigidbody[] rigidbodies;
        protected Collider[] colliders;
        
        protected virtual void Awake()
        {
            rigidbodies = GetComponentsInChildren<Rigidbody>();
            colliders = GetComponentsInChildren<Collider>();
            mainRb = rigidbodies[0];
        }

        protected virtual void DisablePhysics()
        {
            // Disable colliders
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
            
            // Set a temporary layer
            gameObject.layer = LayerMask.NameToLayer("CarriedObject");

            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = true;
            }
        }

        protected virtual void EnablePhysics()
        {
            StartCoroutine(EnableCollisionsDelayed());

            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = false;
            }
        }

        [Server]
        private void ThrowForward()
        {
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = false;
            }

            if (CheatCodes.Hulk)
            {
                mainRb.AddForce(
                    transform.forward * 400, ForceMode.Impulse);
            
                mainRb.AddTorque(new Vector3(
                    Random.Range(dropTorqueRange.x, dropTorqueRange.y),
                    Random.Range(dropTorqueRange.x, dropTorqueRange.y),
                    Random.Range(dropTorqueRange.x, dropTorqueRange.y)), ForceMode.Impulse);
            }
            else
            {
                mainRb.AddForce(
                    transform.forward * Random.Range(dropForceRange.x, dropForceRange.y) +
                    transform.right * Random.Range(dropForceRange.x, dropForceRange.y), ForceMode.Impulse);
            
                mainRb.AddTorque(new Vector3(
                    Random.Range(dropTorqueRange.x, dropTorqueRange.y),
                    Random.Range(dropTorqueRange.x, dropTorqueRange.y),
                    Random.Range(dropTorqueRange.x, dropTorqueRange.y)), ForceMode.Impulse);
            }
        }

        public override void Server_AfterCarryStop()
        {
            ThrowForward();
        }

        public override void Owner_AfterDestroyed()
        {
            Destroy(gameObject);
        }

        public override void Client_AfterCarryStart()
        {
            DisablePhysics();

            if (!hasAuthority)
            {
                gameObject.transform.SetParent(null);
            }
        }

        public override void Client_AfterCarryStop()
        {
            EnablePhysics();
        }

        public override void Client_AfterDestroyed()
        {
            Destroy(gameObject);
        }

        private IEnumerator EnableCollisionsDelayed()
        {
            if (CheatCodes.Hulk) playerCollisionEnableDelay = 0.02f;
            
            // Enable all colliders
            foreach (Collider col in colliders)
            {
                col.enabled = true;
            }
            
            yield return new WaitForSecondsRealtime(playerCollisionEnableDelay);
            
            // Set back to default layer
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }
}