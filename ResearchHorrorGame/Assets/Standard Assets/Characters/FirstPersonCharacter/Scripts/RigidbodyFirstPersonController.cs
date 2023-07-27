using System;
using UnityEngine;
//using UnityStandardAssets.CrossPlatformInput;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (Rigidbody))]
    [RequireComponent(typeof (CapsuleCollider))]
    public class RigidbodyFirstPersonController : MonoBehaviour
    {
        [Serializable]
        public class MovementSettings
        {
            public float ForwardSpeed = 8.0f;   // Speed when walking forward
            public float BackwardSpeed = 4.0f;  // Speed when walking backwards
            public float StrafeSpeed = 4.0f;    // Speed when walking sideways
            public float RunMultiplier = 2.0f;   // Speed when sprinting
	        public KeyCode RunKey = KeyCode.LeftShift;
            public float JumpForce = 30f;
            public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            [HideInInspector] public float CurrentTargetSpeed = 8f;
            public bool CanRun { get; set; }
            public bool CanJump { get; set; }

#if !MOBILE_INPUT
            private bool m_Running;
#endif

            public void UpdateDesiredTargetSpeed(Vector2 input)
            {
	            if (input == Vector2.zero) return;
				if (input.x > 0 || input.x < 0)
				{
					//strafe
					CurrentTargetSpeed = StrafeSpeed;
				}
				if (input.y < 0)
				{
					//backwards
					CurrentTargetSpeed = BackwardSpeed;
				}
				if (input.y > 0)
				{
					//forwards
					//handled last as if strafing and moving forward at the same time forwards speed should take precedence
					CurrentTargetSpeed = ForwardSpeed;
				}
#if !MOBILE_INPUT
	            if (CanRun && Input.GetKey(RunKey))
	            {
		            CurrentTargetSpeed *= RunMultiplier;
		            m_Running = true;
	            }
	            else
	            {
		            m_Running = false;
	            }
#endif
            }

#if !MOBILE_INPUT
            public bool Running
            {
                get { return m_Running; }
            }
#endif
        }


        [Serializable]
        public class AdvancedSettings
        {
            public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
            public float stickToGroundHelperDistance = 0.5f; // stops the character
            public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
            public bool airControl; // can the user control the direction that is being moved in the air
            [Tooltip("set it to 0.1 or more if you get stuck in wall")]
            public float shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
        }


        public Camera cam;
        public bool lockMovementDirection { get; set; }
        public Vector3 lockedForwardDirection { get; set; }
        public Vector3 lockedRightDirection { get; set; }
        private Vector3 forwardMovementBasis { get => lockMovementDirection ? lockedForwardDirection : cam.transform.forward; }
        private Vector3 rightMovementBasis { get => lockMovementDirection ? lockedRightDirection : cam.transform.right; }
        public MovementSettings movementSettings = new MovementSettings();
        public MouseLook mouseLook = new MouseLook();
        public AdvancedSettings advancedSettings = new AdvancedSettings();

        public new Rigidbody rigidbody { get => m_RigidBody; }
        private Rigidbody m_RigidBody;
        private CapsuleCollider m_Capsule;
        private float m_YRotation;
        private Vector3 m_GroundContactNormal;
        private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;
        public LayerMask platformLayers = Physics.AllLayers;
        private AudioSource m_AudioSource;

        [SerializeField] private AudioClip jumpSound, landSound;
        [SerializeField] private AudioClip[] footsteps;
        [SerializeField] private float m_StepInterval;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        private float m_StepCycle;
        private float m_NextStep;


        public Vector3 Velocity
        {
            get { return m_RigidBody.velocity; }
        }

        public bool Grounded
        {
            get { return m_IsGrounded; }
        }

        public bool Jumping
        {
            get { return m_Jumping; }
        }

        public bool Running
        {
            get
            {
 #if !MOBILE_INPUT
				return movementSettings.Running;
#else
	            return false;
#endif
            }
        }


        private void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
            m_AudioSource = GetComponent<AudioSource>();

            mouseLook.Init (transform, cam.transform);
        }


        private void Update()
        {
            RotateView();

            //Fix this because they can repeatedly press jump and make the jump sound
            if (movementSettings.CanJump && Input.GetButtonDown("Jump") && !m_Jump)
            {
                m_Jump = true;
                PlayJumpSound();
            }
        }


        private void FixedUpdate()
        {
            GroundCheck();
            Vector2 input = GetInput();

            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded))
            {
                // always move along the camera forward as it is the direction that it being aimed at
                //Vector3 desiredMove = cam.transform.forward*input.y + cam.transform.right*input.x;

                //In order to implement a look back feature while player is running, we must be able to let the player continue to run forward while temporarily looking back
                //over their shoulder. When lookBack is true, just use the global Vector3.forward and Vector3.right as the basis for the desired movement direction,
                //otherwise under normal movement, just use the camera forward and right directions as the basis (this decision is handled in the forwardMovementBasis and
                //rightMovementBasis properties)
                Vector3 desiredMove = forwardMovementBasis*input.y + rightMovementBasis*input.x;
                desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

                desiredMove.x = desiredMove.x*movementSettings.CurrentTargetSpeed;
                desiredMove.z = desiredMove.z*movementSettings.CurrentTargetSpeed;
                desiredMove.y = desiredMove.y*movementSettings.CurrentTargetSpeed;
                if (m_RigidBody.velocity.sqrMagnitude <
                    (movementSettings.CurrentTargetSpeed*movementSettings.CurrentTargetSpeed))
                {
                    m_RigidBody.AddForce(desiredMove*SlopeMultiplier(), ForceMode.Impulse);
                }
            }

            if (m_IsGrounded)
            {
                m_RigidBody.drag = 5f;

                if (m_Jump)
                {
                    m_RigidBody.drag = 0f;
                    m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                    m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                    m_Jumping = true;
                }

                if (!m_Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f)
                {
                    m_RigidBody.Sleep();
                }
            }
            else
            {
                m_RigidBody.drag = 0f;
                if (m_PreviouslyGrounded && !m_Jumping)
                {
                    StickToGroundHelper();
                }
            }

            ProgressStepCycle(movementSettings.CurrentTargetSpeed);

            m_Jump = false;
        }


        private float SlopeMultiplier()
        {
            float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
            return movementSettings.SlopeCurveModifier.Evaluate(angle);
        }


        private void StickToGroundHelper()
        {
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height/2f) - m_Capsule.radius) +
                                   //advancedSettings.stickToGroundHelperDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                                   advancedSettings.stickToGroundHelperDistance, platformLayers, QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
                {
                    m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
                }
            }
        }


        private Vector2 GetInput()
        {
            
            Vector2 input = new Vector2
                {
                    x = Input.GetAxis("Horizontal"),
                    y = Input.GetAxis("Vertical")
                };
			movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }



        private void RotateView()
        {
            //avoids the mouse looking if the game is effectively paused
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            // get the rotation before it's changed
            float oldYRotation = transform.eulerAngles.y;

            if(!lockMovementDirection)
                mouseLook.LookRotation (transform, cam.transform);

            if (m_IsGrounded || advancedSettings.airControl)
            {
                // Rotate the rigidbody velocity to match the new direction that the character is looking
                Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                m_RigidBody.velocity = velRotation*m_RigidBody.velocity;
            }
        }

        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        private void GroundCheck()
        {
            m_PreviouslyGrounded = m_IsGrounded;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   //((m_Capsule.height/2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                                   ((m_Capsule.height/2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance, platformLayers, QueryTriggerInteraction.Ignore))
            {
                m_IsGrounded = true;
                m_GroundContactNormal = hitInfo.normal;
            }
            else
            {
                m_IsGrounded = false;
                m_GroundContactNormal = Vector3.up;
            }
            if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
            {
                m_Jumping = false;
                PlayLandSound();
            }
        }

        // Taken from FirstPersonController.cs in Unity Standard Assets
        private void ProgressStepCycle(float speed)
        {
            if (m_RigidBody.velocity.sqrMagnitude > 0 && (/*m_Input.x*/ Input.GetAxis("Horizontal") != 0 || /*m_Input.y*/ Input.GetAxis("Vertical") != 0))
            {
                m_StepCycle += (m_RigidBody.velocity.magnitude + (speed * (/*m_IsWalking*/ !movementSettings.Running ? 1f : m_RunstepLenghten))) *
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepSound();
        }

        // Taken from FirstPersonController.cs in Unity Standard Assets
        private void PlayFootStepSound()
        {
            if (!m_IsGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, footsteps.Length);
            m_AudioSource.clip = footsteps[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            footsteps[n] = footsteps[0];
            footsteps[0] = m_AudioSource.clip;
        }

        private void PlayJumpSound()
        {
            m_AudioSource.PlayOneShot(jumpSound);
        }

        private void PlayLandSound()
        {
            m_AudioSource.PlayOneShot(landSound);
        }
    }
}
