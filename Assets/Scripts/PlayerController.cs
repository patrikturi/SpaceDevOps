using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public GameObject PlatformPrefab;

	private const string THRUST_AXIS = "Thrust";
	private const string BRAKE_AXIS = "Brake";
	private const string VERTICAL_AXIS = "Vertical";
	private const string HORIZONTAL_AXIS = "Horizontal";
	private const string SIDE_AXIS = "Side";

	private const float MAX_SPEED = 25;
	private const float FWD_ACC_RATIO = 0.75f;
	private const float MIN_FWD_DECC = 1.5f;
	private const float FWD_DAMPING_RATIO = 0.075f;
	private const float MIN_FWD_DAMPING = 0.75f;
	private const float MAX_ORTHO_SPEED = 12.5f;
	private const float ORTHO_ACC_RATIO = 0.4f;
	private const float MAX_ORTHO_ACC = MAX_ORTHO_SPEED * ORTHO_ACC_RATIO * 1.25f;
	private const float ORTHO_DAMPING_RATIO = 0.075f;
	private const float MIN_ORTHO_DAMPING = 0.75f;
	private const float ORTHO_SCALE_DOWN_SPEED = 0.6f * MAX_SPEED;

	private const float MAX_ROT_ANG_SPEED = 2.5f;
	private const float ROT_ANG_ACC_RATIO = 4.5f;
	private const float ROT_ANG_DECC_RATIO = 9f;
	private const float ROT_SCALE_DOWN_SPEED = 0.2f * MAX_SPEED;

	private const float MAX_STEER_ANG_SPEED = 1.75f;
	private const float STEER_ANG_ACC_RATIO = 3.5f;
	private const float STEER_LANDED_ANG_ACC_RATIO = 14f;
	private const float STEER_ANG_DECC_RATIO = 9f;
	private const float STEER_SCALE_DOWN_SPEED = 0.2f * MAX_SPEED;

	private static float STEER_FORCE_DIFF_MULT;
	private static float ORTHO_ACC_DIFF_MULT;

	private static float GRAVITY_QUERY_RANGE;
	private const float GRAVITY_RADIUS = 75.0f;
	private const float MIN_GRAVITY_RADIUS = 50f;
	private static float DRAG_COEFF;
	private const float GRAVITY_ACC = 7.5f;

	private const string SPEED_UI_KEY = "Speed";

	// Related to gravity
	private const float SCAN_INTERVAL = 0.2f;
	private static int SCAN_FRAME_SKIP;
	private int scan_cur_frame = 0;

	private Transform boundsFront;

	private float thrustInput;
	private float brakeInput;
	private float verticalInput;
	private float horizontalInput;
	private float orthoInput;

	private float fwdSpeedAbs;

	private DebugUI debugUI;
	private Rigidbody body;

	private PController fwdMotor;
	private PController fwdDamping;
	private PController orthoMotor;
	private PController orthoDamping;

	private static float BOUNDS_SIZE;
	private const float BOUNDS_PUSH_RANGE = 10f;
	private const float BOUNDS_PUSH_MAG = 5f;

	private const float PLATFORM_GRAVITY_MAX_HEIGHT = 35f;
	private const float PLATFORM_GRAVITY_MIN_HEIGHT = -25f;
	// Max height from the platform to the ship when the ship is still considered as landed
	private static float MAX_LANDING_OFFSET = 0.5f;
	private static float MAX_PLATFORM_LANDING_HEIGHT;
	private BoxCollider platformCollider;
	private bool landed = false;

	void Awake() {
		body = GetComponent<Rigidbody> ();
		body.inertiaTensorRotation = Quaternion.identity;
		DRAG_COEFF = body.mass * 0.5f;
		STEER_FORCE_DIFF_MULT = 20f * body.mass;
		ORTHO_ACC_DIFF_MULT = 5f;
		SCAN_FRAME_SKIP = (int)Mathf.Round(SCAN_INTERVAL / Time.fixedDeltaTime);

		fwdMotor = new PController (FWD_ACC_RATIO);
		fwdDamping = new PController (FWD_DAMPING_RATIO);
		fwdDamping.SetMinOutput(MIN_FWD_DAMPING);
		orthoMotor = new PController (ORTHO_ACC_RATIO);
		orthoDamping = new PController (ORTHO_DAMPING_RATIO);
		orthoDamping.SetMinOutput (MIN_ORTHO_DAMPING);

		platformCollider = PlatformPrefab.GetComponent<BoxCollider> ();

		float platformSize = platformCollider.size.y * PlatformPrefab.transform.localScale.y / 2f;
		CapsuleCollider shipCollider = GetComponent<CapsuleCollider> ();
		float shipSize = shipCollider.height * shipCollider.transform.localScale.y / 2f;
		MAX_PLATFORM_LANDING_HEIGHT = platformSize + shipSize + MAX_LANDING_OFFSET;
	}

	void Start() {
		debugUI = DebugUI.Instance;
		boundsFront = SceneManager.Instance.BoundsFront;
		BOUNDS_SIZE = boundsFront.localScale.x / 2f;
		GRAVITY_QUERY_RANGE = SceneManager.Instance.PLANET_MAX_SIZE + GRAVITY_RADIUS * SceneManager.PLANET_SIZE_VAR_MAX;
	}

	void FixedUpdate()
	{
		thrustInput = Input.GetAxis (THRUST_AXIS);
		brakeInput = Input.GetAxis (BRAKE_AXIS);
		verticalInput = Input.GetAxis (VERTICAL_AXIS);
		horizontalInput = Input.GetAxis (HORIZONTAL_AXIS);
		orthoInput = Input.GetAxis (SIDE_AXIS);

		fwdSpeedAbs = Mathf.Abs(Vector3.Dot(body.transform.forward, body.velocity));

		ApplyFwdForce ();
		ApplyOrthoForce ();
		ApplySteeringForce ();

		scan_cur_frame++;
		if (scan_cur_frame >= SCAN_FRAME_SKIP) {
			UpdateGravityObjects();
			scan_cur_frame = 0;
		}
		gravCnt = 0;
		ApplyGravity ();
		debugUI.UpdateVar ("Grav", gravCnt.ToString());
		ApplyBoundsForce ();

		ApplyRotation ();
		ApplySteering ();
		ApplyYRotation ();
	}

	void ApplyFwdForce()
	{
		float targetSpeed = GetTargetSpeed ();
		bool engineOn = IsEngineOn ();
		Vector3 fwd = body.transform.forward;
		float fwdSpeed = Vector3.Dot (fwd, body.velocity);
		debugUI.UpdateVar (SPEED_UI_KEY, fwdSpeed.ToString ("0.0"));

		PController controller;
		if (engineOn || landed) {
			controller = fwdMotor;

			// Braking or (landed and no controls)
			if (targetSpeed < 0.1f) {
				fwdMotor.SetMinOutput (MIN_FWD_DECC);
			} else { // Throttling
				fwdMotor.ClearMinOutput ();
			}
		} else {
			controller = fwdDamping;
		}

		float fwdSpeedDiff = targetSpeed - fwdSpeed;
		controller.SetMaxOutput (fwdSpeedDiff / Time.fixedDeltaTime);
		float fwdAcc = controller.GetOutput(fwdSpeed, targetSpeed);
		// F = m * a
		Vector3 fwdForce = body.mass * fwdAcc * fwd;
		body.AddForce (fwdForce);
	}

	private float GetTargetSpeed() {
		if (brakeInput > 0.1f) {
			return 0f;
		} else if (thrustInput > 0.1f) {
			return MAX_SPEED;
		}
		// Engine turned off
		Debug.Assert(IsEngineOn() == false);
		return 0f;
	}

	private bool IsEngineOn() {
		return (thrustInput > 0.1f) || (brakeInput > 0.1f);
	}

	void ApplyOrthoForce() {
		float targetSpeed;
		Vector3 right = body.transform.right;
		float orthoSpeed = Vector3.Dot (right, body.velocity);
		float yAngSpeed = Vector3.Dot (body.transform.forward, body.angularVelocity);

		PController controller;
		if (Mathf.Abs(orthoInput) > 0.1f) {
			controller = orthoMotor;
			targetSpeed = Mathf.Sign (orthoInput) * MAX_ORTHO_SPEED;

			// Make max ortho speed slower at a slow linear velocity
			float scaleDownRatio = Mathf.Min (fwdSpeedAbs / ORTHO_SCALE_DOWN_SPEED, 1f);
			targetSpeed *= scaleDownRatio;
		} else {
			controller = orthoDamping;
			targetSpeed = 0f;
		}

		float orthoSpeedDiff = targetSpeed - orthoSpeed;

		float accToTargetAbs = Mathf.Abs(orthoSpeedDiff / Time.fixedDeltaTime);
		controller.SetMaxOutput (Mathf.Min(accToTargetAbs, MAX_ORTHO_ACC));
		float orthoAcc = controller.GetOutput(orthoSpeed, targetSpeed);
		// Apply additional force to stabilize after a turn or a collision
		float diffAcc = -Mathf.Sign(orthoSpeed) * Mathf.Abs (ORTHO_ACC_DIFF_MULT * yAngSpeed);
		orthoAcc += diffAcc;
		orthoAcc = controller.ApplyLimits (orthoAcc);

		// F = m * a
		Vector3 orthoForce = body.mass * orthoAcc * right;
		body.AddForce (orthoForce);
	}

	void ApplyRotation()
	{
		float targetAngSpeed;
		float angAccRatio;
		if (Mathf.Abs (horizontalInput) > 0.1f) {
			targetAngSpeed = -Mathf.Sign (horizontalInput) * MAX_ROT_ANG_SPEED;
			// Make rotation slower at a slow linear velocity
			float scaleDownRatio = Mathf.Min (fwdSpeedAbs / ROT_SCALE_DOWN_SPEED, 1f);
			targetAngSpeed *= scaleDownRatio;
			angAccRatio = ROT_ANG_ACC_RATIO;
		} else {
			targetAngSpeed = 0f;
			angAccRatio = ROT_ANG_DECC_RATIO;
		}

		Vector3 fwd = body.transform.forward;
		float fwdAngSpeed = Vector3.Dot (fwd, body.angularVelocity);

		float fwdAngSpeedDiff = targetAngSpeed - fwdAngSpeed;
		float angAcc = fwdAngSpeedDiff * angAccRatio;
		float torqueMag = body.inertiaTensor.z * angAcc;
		Vector3 relTorque = new Vector3(0, 0, torqueMag);
		body.AddRelativeTorque (relTorque);
	}

	void ApplySteering()
	{
		float targetAngSpeed;
		float angAccRatio;

		if (Mathf.Abs (verticalInput) > 0.1f) {
			targetAngSpeed = Mathf.Sign (verticalInput) * MAX_STEER_ANG_SPEED;
			// Make steering slower at a slow linear velocity
			float scaleDownRatio = Mathf.Min (fwdSpeedAbs / STEER_SCALE_DOWN_SPEED, 1f);
			targetAngSpeed *= scaleDownRatio;

			// Increase steering strength when landed: otherwise the ship
			// is unable to take off from a platform
			angAccRatio = landed ? STEER_LANDED_ANG_ACC_RATIO : STEER_ANG_ACC_RATIO;
		} else {
			targetAngSpeed = 0f;
			angAccRatio = STEER_ANG_DECC_RATIO;
		}

		Vector3 right = body.transform.right;
		float xAngSpeed = Vector3.Dot (right, body.angularVelocity);

		float xAngSpeedDiff = targetAngSpeed - xAngSpeed;
		float angAcc = xAngSpeedDiff * angAccRatio;
		float torqueMag = body.inertiaTensor.x * angAcc;
		Vector3 relTorque = new Vector3(torqueMag, 0, 0);
		body.AddRelativeTorque (relTorque);
	}

	// Damping only
	void ApplyYRotation() {
		Vector3 up = body.transform.up;
		float upAngSpeed = Vector3.Dot (up, body.angularVelocity);

		float angAcc = -upAngSpeed / Time.fixedDeltaTime;
		float torqueMag = body.inertiaTensor.y * angAcc;
		Vector3 relTorque = new Vector3(0, torqueMag, 0);
		body.AddRelativeTorque (relTorque);
	}

	void ApplySteeringForce ()
	{
		// Keep Y velocity zero
		Vector3 up = body.transform.up;
		float upSpeed = Vector3.Dot (up, body.velocity);

		debugUI.UpdateVar ("UpSpeed", upSpeed.ToString ("0.0"));

		// F = m * dv/dt
		float maxForceMag = body.mass * Mathf.Abs(upSpeed)/Time.fixedDeltaTime;

		Vector3 right = body.transform.right;
		float xAngSpeed = Vector3.Dot (right, body.angularVelocity);

		float dragForceMag = Mathf.Abs (upSpeed) * DRAG_COEFF;
		float diffForceMag = Mathf.Abs(STEER_FORCE_DIFF_MULT * xAngSpeed);

		float steeringForceMag = dragForceMag + diffForceMag;

		steeringForceMag = Mathf.Min (steeringForceMag, maxForceMag);
		Vector3 dragForce = -Mathf.Sign(upSpeed) * steeringForceMag * up;
		body.AddForce (dragForce);
	}

	// Gravity towards the center of the sphere
	private List<GameObject> sphereGravityObjects = new List<GameObject> ();
	// Gravity in a single direction
	private List<GameObject> directionalGravityObjects = new List<GameObject> ();
	// Debug variable for number of gravity objects affecting the ship
	private int gravCnt = 0;

	void UpdateGravityObjects() {
		// TODO: delay by one frame per player number
		Collider[] hitColliders = Physics.OverlapSphere(body.transform.position, GRAVITY_QUERY_RANGE);

		sphereGravityObjects.Clear ();
		// TODO: filter duplicates
		foreach (Collider collider in hitColliders) {
			GameObject obj = collider.gameObject;
			if (obj.tag.Contains ("SPHERE_GRAVITY")) {
				sphereGravityObjects.Add (obj);
			}
		}

		directionalGravityObjects.Clear ();
		foreach (Collider collider in hitColliders) {
			GameObject obj = collider.gameObject;
			if (obj.tag.Contains ("DIR_GRAVITY")) {
				directionalGravityObjects.Add (obj);
			}
		}
	}

	void ApplyGravity() {

		bool hasPlatformGravity = false;
		landed = false;

		// Warning: The platform is expected to have Y up rotation
		foreach (GameObject obj in directionalGravityObjects) {

			Vector3 platformGravityArea = new Vector3 (platformCollider.size.x, PLATFORM_GRAVITY_MAX_HEIGHT*2f, platformCollider.size.z);
			float rotation = obj.transform.rotation.eulerAngles.y;
			Vector3 rotatedArea = Quaternion.AngleAxis (rotation, Vector3.up) * platformGravityArea;
			platformGravityArea = new Vector3 (Mathf.Abs (rotatedArea.x), rotatedArea.y, Mathf.Abs (rotatedArea.z));

			Bounds platformBounds = new Bounds(obj.transform.position, platformGravityArea);

			if (!platformBounds.Contains (transform.position)) {
				continue;
			}

			float height = transform.position.y - obj.transform.position.y;
			if (height < PLATFORM_GRAVITY_MIN_HEIGHT) {
				continue;
			}

			float platformToShip = body.position.y - obj.transform.position.y;

			if (platformToShip > 0f && platformToShip < MAX_PLATFORM_LANDING_HEIGHT) {
				landed = true;
			}

			gravCnt++;
			hasPlatformGravity = true;

			Vector3 gravityDir = Vector3.down;

			float forceMag = body.mass * GRAVITY_ACC;
			body.AddForce (forceMag * gravityDir);
		}

		// Do not apply planet gravity when above a platform
		if (hasPlatformGravity) {
			return;
		}

		foreach(GameObject obj in sphereGravityObjects) {
			Vector3 toObj = obj.transform.position - body.transform.position;
			float planetToShip = toObj.magnitude;
			float planetSize = getPlanetSize(obj);

			float gravityRadius = GRAVITY_RADIUS * obj.transform.localScale.x;
			gravityRadius = Mathf.Max (gravityRadius, MIN_GRAVITY_RADIUS);
			if (planetToShip > planetSize + gravityRadius) {
				continue;
			}

			gravCnt++;

			toObj.Normalize ();

			float forceMag = body.mass * GRAVITY_ACC;
			body.AddForce (forceMag * toObj);

			if (planetToShip < planetSize + MAX_LANDING_OFFSET) {
				landed = true;
			}
		}
	}

	private float getPlanetSize(GameObject planet) {
		// x,y,z scale are expected to be equal
		return SceneManager.Instance.PLANET_COLLIDER_SIZE * planet.transform.localScale.x;
	}

	// Toss away from the game bounds if very close
	// This will prevent getting stuck eg. in a corner
	void ApplyBoundsForce() {
		ApplyBoundsForceAxis (0, body.transform.position.x);
		ApplyBoundsForceAxis (1, body.transform.position.y);
		ApplyBoundsForceAxis (2, body.transform.position.z);
	}

	private void ApplyBoundsForceAxis(int axisIndex, float position) {
		if (BOUNDS_SIZE - Mathf.Abs (position) < BOUNDS_PUSH_RANGE) {
			float sign = -Mathf.Sign (position);
			Vector3 dir;
			if (axisIndex == 0) {
				dir = new Vector3 (sign, 0, 0);
			}
			else if (axisIndex == 1) {
				dir = new Vector3 (0, sign, 0);
			}
			else if (axisIndex == 2) {
				dir = new Vector3 (0, 0, sign);
			} else {
				throw new Exception ("Unhandled state");
			}
			// F = m*a
			body.AddForce (dir*BOUNDS_PUSH_MAG*body.mass);
		}
	}
}
