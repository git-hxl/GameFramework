using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class PlayrController : MonoBehaviour
    {
        private CharacterController controller;
        private Vector3 playerVelocity;
        private bool groundedPlayer;
        public float playerSpeed = 2.0f;
        public float jumpHeight = 1.0f;
        public float gravityValue = -9.81f;

        private Animator animator;
        private void Start()
        {
            controller = gameObject.GetComponent<CharacterController>();
            groundedPlayer = true;

            animator = gameObject.GetComponent<Animator>();
        }

        void Update()
        {
            groundedPlayer = controller.isGrounded;
            if (groundedPlayer && playerVelocity.y < 0)
            {
                playerVelocity.y = 0f;
            }

            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;


            if (move != Vector3.zero)
            {
                gameObject.transform.forward = move;
            }

            playerVelocity.x = move.x;
            playerVelocity.z = move.z;

            // Changes the height position of the player..
            if (Input.GetKeyDown(KeyCode.Space) && groundedPlayer)
            {
                playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            }

            playerVelocity.y += gravityValue * Time.deltaTime;

            CollisionFlags collisionFlags = controller.Move(playerVelocity * Time.deltaTime * playerSpeed);

            //groundedPlayer = collisionFlags == CollisionFlags.Below;

            if (animator != null)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    animator.SetTrigger("Fire");
                }
            }

        }
    }
}
