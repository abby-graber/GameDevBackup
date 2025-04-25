using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]

public class CharacterMovementManager : MonoBehaviour {

	public float WalkSpeed = 10.0f;
	public float StrafeSpeed = 10.0f;
	public float WalkBackSpeed = -2.5f;
	public float AirborneSpeed = 2.0f;
	public float RotatingSpeed = 2.5f;
	public float SprintSpeedMultiplier = 2.0f;
	public float JumpHeight = 10.0f;
	public int AllowedAirborneMoves = 1;
	public float SlidingThreshold = 40.0f;
	public float FallingThreshold = 6.0f;
	public float Gravity = 20.0f;

	private float zSpeedVelocity = 0f;
	private float xSpeedVelocity = 0f;
	public float speedSmoothTime = 0.1f; 

	private CharacterController _characterController;
	private RPGCamera _rpgCamera;
	private Vector3 _playerDirection;
	private Vector3 _playerDirectionWorld;
	private float _localRotation;
	private bool _jump = false;
	private bool _sprinting = false;
	private bool _allowAirborneMovement = false;
	private int _airborneMovesCount = 0;

	public KeyCode SprintKey = KeyCode.LeftShift;

	[SerializeField] private AudioClip jumpSound;

	private Animator animator;

	private Rigidbody rb;

	private bool _autorun = false;

	private void Awake() {
		_characterController = GetComponent<CharacterController>();
		_rpgCamera = GetComponent<RPGCamera>();
		_characterController.slopeLimit = SlidingThreshold + 0.2f;
		animator = GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;

		try {
			Input.GetButton ("Horizontal Strafe");
			Input.GetButton ("Autorun");
		}
		catch (UnityException e) {
			Debug.LogWarning (e.Message);
		}
	}

	private void Update () {

		#region Check inputs
		// Create the local movement direction
		float vertical = Input.GetAxisRaw ("Vertical");
		// Check the autorun input
		if (_autorun &&
			(Input.GetButtonDown ("Vertical") || Input.GetButtonDown ("Autorun")))
			_autorun = false;
		else if (Input.GetButtonDown ("Autorun"))
			_autorun = true;

		if (Input.GetMouseButton (0) && Input.GetMouseButton (1)
			|| _autorun)
			vertical = 1.0f;

		float horizontal = Input.GetAxisRaw ("Horizontal");
		float horizontalStrafe = Input.GetAxisRaw ("Horizontal Strafe");

		// Strafe if the right mouse button and the "Horizontal" input are pressed at once 
		if (Input.GetMouseButton (1) && Input.GetAxisRaw ("Horizontal") != 0) {
			horizontalStrafe = horizontal;
			horizontal = 0f;
			Debug.Log("strafe");
		}
		// Create and set the player's direction inside the motor
		Vector3 playerDirection = new Vector3 (horizontalStrafe, 0, vertical);
		SetPlayerDirection (playerDirection);
		// Allow movement while airborne
		AllowAirborneMovement (Input.GetButtonDown ("Vertical") || Input.GetButtonDown ("Horizontal Strafe"));

		// Set the y-axis rotation and if the camera also rotates inside the motor
		SetLocalRotation (horizontal, !Input.GetMouseButton (0));
		// Check if the player wants the character to look in the direction of the camera
		if (Input.GetMouseButton (1))
			AlignCharacterWithCamera ();

		// Check if the sprint modifier is pressed down
		_sprinting = Input.GetKey(SprintKey);

		// Check if the jump button is pressed down
		if (Input.GetButtonDown ("Jump")) {
			Jump ();
		}
		#endregion

		StartMotor ();
		
		Vector3 localVelocity = transform.InverseTransformDirection(_characterController.velocity);

		//animator.SetFloat("zSpeed", localVelocity.z/ (WalkSpeed * SprintSpeedMultiplier));
        //animator.SetFloat("xSpeed", localVelocity.x/ StrafeSpeed);

		float targetZSpeed = localVelocity.z / (WalkSpeed * SprintSpeedMultiplier);
		float targetXSpeed = localVelocity.x / StrafeSpeed;

		// Interpolate speeds smoothly
		float smoothedZSpeed = Mathf.SmoothDamp(animator.GetFloat("zSpeed"), targetZSpeed, ref zSpeedVelocity, speedSmoothTime);
		float smoothedXSpeed = Mathf.SmoothDamp(animator.GetFloat("xSpeed"), targetXSpeed, ref xSpeedVelocity, speedSmoothTime);

		// Apply to Animator
		animator.SetFloat("zSpeed", smoothedZSpeed);
		animator.SetFloat("xSpeed", smoothedXSpeed);

	}

	public void StartMotor() {

		if (_characterController.isGrounded) {
			// Reset the counter for the number of allowed moves while airborne
			_airborneMovesCount = 0;
			// Transform the local movement direction to world space
			_playerDirectionWorld = transform.TransformDirection(_playerDirection);
			// Normalize the player's movement direction
			if (_playerDirectionWorld.magnitude > 1)
				_playerDirectionWorld = Vector3.Normalize(_playerDirectionWorld);

			// Compute the combined speed
			float combinedSpeed = 0f;

			if (_playerDirection.x != 0 || _playerDirection.z != 0) {
				if(_playerDirection.z < 0f){		//walking backwards
					combinedSpeed = (StrafeSpeed * Mathf.Abs(_playerDirection.x) + (WalkSpeed / 2f) * Mathf.Abs(_playerDirection.z))
						/ (Mathf.Abs(_playerDirection.x) + Mathf.Abs(_playerDirection.z));
					//Debug.Log("Walking back");
				}else{
					combinedSpeed = (StrafeSpeed * Mathf.Abs(_playerDirection.x) + WalkSpeed * Mathf.Abs(_playerDirection.z))
						/ (Mathf.Abs(_playerDirection.x) + Mathf.Abs(_playerDirection.z));
					//Debug.Log("walking forward");
				}
			}


			// Apply the combined speed
			_playerDirectionWorld *= combinedSpeed;
			// Multiply with the sprint multiplier if sprinting
			if (_sprinting)
				_playerDirectionWorld *= SprintSpeedMultiplier;

			// Apply the falling threshold
			_playerDirectionWorld.y = -FallingThreshold;

			ApplySliding();
			// Check if the character pressed the jump button this frame
			if (_jump) {
				_jump = false;
				_playerDirectionWorld.y = JumpHeight;
			}

		} else if (_allowAirborneMovement 
		           && _airborneMovesCount <= AllowedAirborneMoves
		           && transform.InverseTransformDirection(_playerDirectionWorld).z == 0) {
			// Allow slight movement while airborne after a standing jump
			Vector3 playerDirectionWorld = transform.TransformDirection(_playerDirection);
			// Normalize the player's movement direction
			if (_playerDirectionWorld.magnitude > 1)
				playerDirectionWorld = Vector3.Normalize(playerDirectionWorld);
			// Apply the airborne speed
			playerDirectionWorld *= AirborneSpeed;
			// Set the x and z direction in order to move the character constantly
			_playerDirectionWorld.x = playerDirectionWorld.x;
			_playerDirectionWorld.z = playerDirectionWorld.z;
		}

		// Apply the gravity
		_playerDirectionWorld.y -= Gravity * Time.deltaTime;
		// Move the character
		_characterController.Move(_playerDirectionWorld * Time.deltaTime);
		transform.Rotate(Vector3.up * _localRotation);
	}

	private void ApplySliding() {
		RaycastHit hitInfo;

		// Cast a ray down to the ground in order to get the ground's normal vector
		if (Physics.Raycast(transform.position, Vector3.down, out hitInfo)) {
			//Debug.DrawLine(transform.position, transform.position + hitInfo.normal);
			//Debug.DrawLine(transform.position, transform.position + slopeDirection);

			Vector3 hitNormal = hitInfo.normal;
			// Compute the slope in degrees
			float slope = Vector3.Angle(hitNormal, Vector3.up);
			// Compute the sliding direction
			Vector3 slidingDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);
			// Normalize the sliding direction and make it orthogonal to the hit normal
			Vector3.OrthoNormalize(ref hitNormal, ref slidingDirection);
			// Check if the slope is too steep
			if (slope > SlidingThreshold)
				_playerDirectionWorld += slidingDirection * slope * 0.5f;
		}
	}
		
	public void Jump() {
		if (_characterController.isGrounded)
			animator.SetTrigger("jumpTrigger");
			_jump = true;
			SoundFXManager.instance.PlaySoundFXClip(jumpSound, transform, 1f);
	}

	public void Sprint(bool on) {
		_sprinting = on;
	}

	public void Sprint(bool on, float speed) {
		_sprinting = on;
		SprintSpeedMultiplier = speed;
	}

	public void SetPlayerDirection(Vector3 direction) {
		_playerDirection = direction;

		if (_rpgCamera)
			_rpgCamera.SetAlignCameraWithCharacter(_playerDirection.z != 0 || _playerDirection.x != 0);
	}

	public void AllowAirborneMovement(bool on) {
		_allowAirborneMovement = false;
		// Allow airborne movement and increase the airborne moves counter if we are not grounded
		if (!_characterController.isGrounded && on) {
			_allowAirborneMovement = true;
			_airborneMovesCount++;
		}
	}

	public void SetLocalRotation(float rotation, bool withCamera = false) {
		_localRotation = rotation * RotatingSpeed;

		if (_rpgCamera && withCamera)
			_rpgCamera.Rotate(rotation * RotatingSpeed);
	}

	public void AlignCharacterWithCamera() {
		transform.eulerAngles = new Vector3(transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, transform.eulerAngles.z);
	}
}
