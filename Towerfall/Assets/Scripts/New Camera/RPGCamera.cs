using UnityEngine;
using System.Collections;

public class RPGCamera : MonoBehaviour {
	
	public Vector3 CameraPivotLocalPosition = new Vector3(0, 0.6f, 0);
	public bool InvertMouse = true;
	public bool AlignCameraWhenMoving = true;
	public float MouseXSensitivity = 8.0f;
	public float MouseYSensitivity = 8.0f;
	public float MouseYMin = -89.5f;
	public float MouseYMax = 89.5f;
	public float MouseScrollSensitivity = 15.0f;
	public float MouseSmoothTime = 0.08f;
	public float MaxDistance = 30.0f;
	public float DistanceSmoothTime = 0.7f;
	public float FadingStartDistance = 1.2f;
	public float FadingEndDistance = 0.8f;
	public KeyCode FirstPersonSwitch = KeyCode.PageUp;
	public KeyCode ThirdPersonSwitch = KeyCode.PageDown;
	public float StartMouseX = 0;
	public float StartMouseY = 15.0f;
	public float StartDistance = 10.0f;

	private Transform mainCameraTransform;
	private float cameraNearClip;
	private Vector3 _cameraPivotPosition;
	private Vector3 _desiredPosition;
	private float _desiredDistance;
	private float _distanceSmooth = 0;
	private float _distanceCurrentVelocity;
	private bool _alignCameraWithCharacter = false;
	private float _mouseX = 0;
	private float _mouseXSmooth = 0;
	private float _mouseXCurrentVelocity;
	private float _mouseY = 0;
	private float _mouseYSmooth = 0;
	private float _mouseYCurrentVelocity;
	private float _desiredMouseY = 0;
	private Renderer[] _renderersToFade;

	private void Awake() {

		ResetView();

		_renderersToFade = GetComponentsInChildren<Renderer>();
	}

    private void Start () {
		mainCameraTransform = Camera.main.transform;
		cameraNearClip = Camera.main.nearClipPlane;
	}

    /// <summary>
    /// Runs through all of the different conditions that should prevent the camera from rotating
    /// </summary>
    private bool CanCameraRotate () {
		return true;
	}

	private void LateUpdate() {

		if (CanCameraRotate () != true)
			return;

		// Set the camera's pivot position in world coordinates
		_cameraPivotPosition = transform.position + CameraPivotLocalPosition;

		// Check if the camera lies on the ground
		bool mouseYConstrained = Physics.Linecast(mainCameraTransform.position,
												  mainCameraTransform.position + Vector3.down);
		mouseYConstrained = mouseYConstrained && mainCameraTransform.position.y < _cameraPivotPosition.y;

		#region Get inputs

		float mouseYMinLimit = _mouseY;
		// Get mouse movement
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
			// Lock the cursor and hide it
			Cursor.lockState = CursorLockMode.Confined;
			Cursor.visible = false;

			_mouseX += Input.GetAxis("Mouse X") * MouseXSensitivity;

			if (InvertMouse)
				_desiredMouseY -= Input.GetAxis("Mouse Y") * MouseYSensitivity;
			else
				_desiredMouseY += Input.GetAxis("Mouse Y") * MouseYSensitivity;

			if (mouseYConstrained) {
				_mouseY = Mathf.Clamp(_desiredMouseY, Mathf.Max(mouseYMinLimit, MouseYMin), MouseYMax);
				_desiredMouseY = Mathf.Max(_desiredMouseY, _mouseY - 90.0f);
			} else
				_mouseY = Mathf.Clamp(_desiredMouseY, MouseYMin, MouseYMax);

			_desiredMouseY = Mathf.Min(_desiredMouseY, MouseYMax);
		} else {
			// Unlock the cursor and make it visible again
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		// Get the scroll wheel input
		_desiredDistance = _desiredDistance - Input.GetAxis("Mouse ScrollWheel") * MouseScrollSensitivity;
		_desiredDistance = Mathf.Clamp(_desiredDistance, 0, MaxDistance);
		// Check if one of the switch buttons got pressed down
		if (Input.GetKeyDown(FirstPersonSwitch)) {
			_desiredDistance = 0;
		} else if (Input.GetKeyDown(ThirdPersonSwitch)) {
			_desiredDistance = MaxDistance;
		}
		// Align camera when moving forward or backwards
		if (AlignCameraWhenMoving && _alignCameraWithCharacter)
			AlignCameraWithCharacter();

		#endregion

		#region Smooth the inputs
		
		_mouseXSmooth = Mathf.SmoothDamp(_mouseXSmooth, _mouseX, ref _mouseXCurrentVelocity, MouseSmoothTime);
		_mouseYSmooth = Mathf.SmoothDamp(_mouseYSmooth, _mouseY, ref _mouseYCurrentVelocity, MouseSmoothTime);

		#endregion

		#region Compute the new camera position
		Vector3 newCameraPosition;
		// Compute the desired position
		_desiredPosition = GetCameraPosition(_mouseYSmooth, _mouseXSmooth, _desiredDistance);

		// Compute the closest possible camera distance by checking if there is something inside the view frustum
		float closestDistance = (new RPGClipPlane(_desiredPosition, _cameraPivotPosition)).CheckViewFrustum();
		
		if (closestDistance != -1) {
			// Set the camera distance to the closest distance because the camera is constricted
			closestDistance -= cameraNearClip;
			if (_distanceSmooth < closestDistance)
				// Smooth the distance if we move from a smaller constricted distance to a bigger constricted distance
				_distanceSmooth = Mathf.SmoothDamp(_distanceSmooth, closestDistance, ref _distanceCurrentVelocity, DistanceSmoothTime);
			else
				// Do not smooth if the new closest distance is smaller than the current distance
				_distanceSmooth = closestDistance;
		
		} else {
			// The camera is not constricted (anymore) so smooth the distance change
			_distanceSmooth = Mathf.SmoothDamp(_distanceSmooth, _desiredDistance, ref _distanceCurrentVelocity, DistanceSmoothTime);
		}
		// Compute the new camera position
		newCameraPosition = GetCameraPosition(_mouseYSmooth, _mouseXSmooth, _distanceSmooth);

		#endregion

		#region Update the camera transform

		mainCameraTransform.position = newCameraPosition;
		// Check if we are in third or first person and adjust the camera rotation behavior
		if (_distanceSmooth > 0.1f)
			mainCameraTransform.LookAt(_cameraPivotPosition);
		else
			mainCameraTransform.eulerAngles = new Vector3(_mouseYSmooth, _mouseXSmooth, 0);

		if (mouseYConstrained) {			
			float lookUpDegrees = _desiredMouseY - _mouseY;
			mainCameraTransform.Rotate(Vector3.right, lookUpDegrees);
		}

		#endregion

		// Let the character fade
		//CharacterFade();
	}

	private Vector3 GetCameraPosition(float xAxisDegrees, float yAxisDegrees, float distance) {
		Vector3 offset = new Vector3(0, 0, -distance);
		Quaternion rotation = Quaternion.Euler(xAxisDegrees, yAxisDegrees, 0);

		return _cameraPivotPosition + rotation * offset;
	}

	public void ResetView() {
		_mouseX = transform.eulerAngles.y + StartMouseX;
		_mouseY = _desiredMouseY = StartMouseY;
		_desiredDistance = StartDistance;
	}

	public void Rotate(float degree) {
		_mouseX += degree;
	}

	public void SetAlignCameraWithCharacter(bool on) {
		_alignCameraWithCharacter = on && !Input.GetMouseButton(0) && !Input.GetMouseButton(1);
	}

	private void AlignCameraWithCharacter() {
		float characterRotation = transform.eulerAngles.y;
		// Shift the character rotation offset so it fits the interval (-180,180]
		if (characterRotation > 180f) {
			characterRotation = characterRotation - 360f;
		}
		// Compute how many full rotations we have done with the camera and the offset to being behind the character
		float offsetToCameraRotation = CustomModulo(_mouseX, 360);
		float numberOfFullRotations = (_mouseX - offsetToCameraRotation) / 360;
		
		if (_mouseX < 0) {
			if (offsetToCameraRotation < -180 + characterRotation)
				numberOfFullRotations--;
		} else {
			if (offsetToCameraRotation > 180 + characterRotation)
				// The shortest way to rotate behind the character is to fulfill the current rotation
				numberOfFullRotations++;
		}

		_mouseX = numberOfFullRotations * 360 + characterRotation;
	}

	private float CustomModulo(float dividend, float divisor) {
		if (dividend < 0)
			return dividend - divisor * Mathf.Ceil(dividend / divisor);	
		else
			return dividend - divisor * Mathf.Floor(dividend / divisor);
	}

	private void CharacterFade() {
		// Get the actual distance
		float actualDistance = Vector3.Distance(transform.position, mainCameraTransform.position);
		// Compute the new alpha value depending on the fading start und end distance
		float characterAlpha = Mathf.Floor(Mathf.Clamp(actualDistance / FadingStartDistance - FadingEndDistance, 0, 1) * 100) / 100;
		// Set the computed alpha value for all renderers 
		foreach (Renderer r in _renderersToFade) {
			r.material.color = new Color(r.material.color.r, r.material.color.g, r.material.color.b, characterAlpha);
		}
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(transform.position + CameraPivotLocalPosition, 0.1f);
	}
}
