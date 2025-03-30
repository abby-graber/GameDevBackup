using UnityEngine;
using System.Collections;

public class RPGClipPlane {

	public Vector3 Position;
	public Vector3 TargetPosition;
	public Vector3 ShiftUpperLeft;
	public Vector3 ShiftUpperRight;
	public Vector3 ShiftLowerLeft;
	public Vector3 ShiftLowerRight;

	private float _halfWidth;
	private float _halfHeight;

	public RPGClipPlane(Vector3 atPosition, Vector3 target) {
		Position = atPosition;
		TargetPosition = target;

		float halfFieldOfView = Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad;
		_halfHeight = Camera.main.nearClipPlane * Mathf.Tan(halfFieldOfView);
		_halfWidth = _halfHeight * Camera.main.aspect;

		Vector3 targetDirection = target - atPosition;
		targetDirection.Normalize();

		Vector3 localRight = Camera.main.transform.right;
		Vector3 localUp = Vector3.Cross(targetDirection, localRight);
		localUp.Normalize();

		float offset = Camera.main.nearClipPlane;

		ShiftUpperLeft = -localRight * _halfWidth;
		ShiftUpperLeft += localUp * _halfHeight;
		ShiftUpperLeft += targetDirection * offset;
		
		ShiftUpperRight = localRight * _halfWidth;
		ShiftUpperRight += localUp * _halfHeight;
		ShiftUpperRight += targetDirection * offset;
		
		ShiftLowerLeft = -localRight * _halfWidth;
		ShiftLowerLeft -= localUp * _halfHeight;
		ShiftLowerLeft += targetDirection * offset;

		ShiftLowerRight = localRight * _halfWidth;
		ShiftLowerRight -= localUp * _halfHeight;
		ShiftLowerRight += targetDirection * offset;
	}

	public float CheckViewFrustum() {
		// Return -1 if there was no collision with the view frustum
		float closestDistance = -1;
		RaycastHit hitInfo;

		Vector3 UpperLeft = Position + ShiftUpperLeft;
		Vector3 UpperRight = Position + ShiftUpperRight;
		Vector3 LowerLeft = Position + ShiftLowerLeft;
		Vector3 LowerRight = Position + ShiftLowerRight;
		
		/*// Debugging lines
		// Draw the camera's clip plane at the desired position
		Debug.DrawLine(UpperLeft, UpperRight, Color.red);
		Debug.DrawLine(UpperLeft, LowerLeft, Color.red);
		Debug.DrawLine(UpperRight, LowerRight, Color.red);
		Debug.DrawLine(LowerLeft, LowerRight, Color.red);

		// Draw the clip plane of the actual camera position
		Vector3 cameraPosition = Camera.main.transform.position;
		Debug.DrawLine(cameraPosition + ShiftUpperLeft, cameraPosition + ShiftUpperRight, Color.red);
		Debug.DrawLine(cameraPosition + ShiftUpperLeft, cameraPosition + ShiftLowerLeft, Color.red);
		Debug.DrawLine(cameraPosition + ShiftUpperRight, cameraPosition + ShiftLowerRight, Color.red);
		Debug.DrawLine(cameraPosition + ShiftLowerLeft, cameraPosition + ShiftLowerRight, Color.red);

		// Draw the corresponding clip plane at the target
		Debug.DrawLine(TargetPosition + ShiftUpperLeft, TargetPosition + ShiftUpperRight, Color.red);
		Debug.DrawLine(TargetPosition + ShiftUpperLeft, TargetPosition + ShiftLowerLeft, Color.red);
		Debug.DrawLine(TargetPosition + ShiftUpperRight, TargetPosition + ShiftLowerRight, Color.red);
		Debug.DrawLine(TargetPosition + ShiftLowerLeft, TargetPosition + ShiftLowerRight, Color.red);

		// Draw the lines which are tested for collisions 
		Debug.DrawLine(TargetPosition, cameraPosition, Color.cyan);
		Debug.DrawLine(TargetPosition + ShiftUpperLeft, UpperLeft);
		Debug.DrawLine(TargetPosition + ShiftUpperRight, UpperRight);
		Debug.DrawLine(TargetPosition + ShiftLowerLeft, LowerLeft);
		Debug.DrawLine(TargetPosition + ShiftLowerRight, LowerRight);
		*/

		LayerMask ignorePlayer = ~((1 << LayerMask.NameToLayer("Player")) + 4);

		// Check the line from the target to the clip plane
		if (Physics.Linecast(TargetPosition, Position, out hitInfo, ignorePlayer))
			closestDistance = hitInfo.distance;

		// Check the line from the target's upper left to the upper left vertex of the clip plane
		if (Physics.Linecast(TargetPosition + ShiftUpperLeft, UpperLeft, out hitInfo, ignorePlayer))
			if (hitInfo.distance < closestDistance || closestDistance == -1)
				closestDistance = Vector3.Distance(TargetPosition, hitInfo.point - ShiftUpperLeft);

		// Check the line from the target's upper right to the upper right vertex of the clip plane
		 if (Physics.Linecast(TargetPosition + ShiftUpperRight, UpperRight, out hitInfo, ignorePlayer))
			if (hitInfo.distance < closestDistance || closestDistance == -1)
				closestDistance = Vector3.Distance(TargetPosition, hitInfo.point - ShiftUpperRight);

		// Check the line from the target's lower left to the lower left vertex of the clip plane
		if (Physics.Linecast(TargetPosition + ShiftLowerLeft, LowerLeft, out hitInfo, ignorePlayer))
			if (hitInfo.distance < closestDistance || closestDistance == -1)
					closestDistance = Vector3.Distance(TargetPosition, hitInfo.point - ShiftLowerLeft);

		// Check the line from the target's lower right to the lower right vertex of the clip plane
		if (Physics.Linecast(TargetPosition + ShiftLowerRight, LowerRight, out hitInfo, ignorePlayer))
			if (hitInfo.distance < closestDistance || closestDistance == -1)
					closestDistance = Vector3.Distance(TargetPosition, hitInfo.point - ShiftLowerRight);

		return closestDistance;
	}
}
