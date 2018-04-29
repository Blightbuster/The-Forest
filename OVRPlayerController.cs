using System;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class OVRPlayerController : MonoBehaviour
{
	
	
	
	public event Action<Transform> TransformUpdated;

	
	
	
	public event Action CameraUpdated;

	
	
	
	public event Action PreCharacterMove;

	
	
	
	public float InitialYRotation { get; private set; }

	
	private void Start()
	{
		Vector3 localPosition = this.CameraRig.transform.localPosition;
		localPosition.z = OVRManager.profile.eyeDepth;
		this.CameraRig.transform.localPosition = localPosition;
	}

	
	private void Awake()
	{
		this.Controller = base.gameObject.GetComponent<CharacterController>();
		if (this.Controller == null)
		{
			Debug.LogWarning("OVRPlayerController: No CharacterController attached.");
		}
		OVRCameraRig[] componentsInChildren = base.gameObject.GetComponentsInChildren<OVRCameraRig>();
		if (componentsInChildren.Length == 0)
		{
			Debug.LogWarning("OVRPlayerController: No OVRCameraRig attached.");
		}
		else if (componentsInChildren.Length > 1)
		{
			Debug.LogWarning("OVRPlayerController: More then 1 OVRCameraRig attached.");
		}
		else
		{
			this.CameraRig = componentsInChildren[0];
		}
		this.InitialYRotation = base.transform.rotation.eulerAngles.y;
	}

	
	private void OnEnable()
	{
		OVRManager.display.RecenteredPose += this.ResetOrientation;
		if (this.CameraRig != null)
		{
			this.CameraRig.UpdatedAnchors += this.UpdateTransform;
		}
	}

	
	private void OnDisable()
	{
		OVRManager.display.RecenteredPose -= this.ResetOrientation;
		if (this.CameraRig != null)
		{
			this.CameraRig.UpdatedAnchors -= this.UpdateTransform;
		}
	}

	
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Q))
		{
			this.buttonRotation -= this.RotationRatchet;
		}
		if (Input.GetKeyDown(KeyCode.E))
		{
			this.buttonRotation += this.RotationRatchet;
		}
	}

	
	protected virtual void UpdateController()
	{
		if (this.useProfileData)
		{
			OVRPose? initialPose = this.InitialPose;
			if (initialPose == null)
			{
				this.InitialPose = new OVRPose?(new OVRPose
				{
					position = this.CameraRig.transform.localPosition,
					orientation = this.CameraRig.transform.localRotation
				});
			}
			Vector3 localPosition = this.CameraRig.transform.localPosition;
			if (OVRManager.instance.trackingOriginType == OVRManager.TrackingOrigin.EyeLevel)
			{
				localPosition.y = OVRManager.profile.eyeHeight - 0.5f * this.Controller.height + this.Controller.center.y;
			}
			else if (OVRManager.instance.trackingOriginType == OVRManager.TrackingOrigin.FloorLevel)
			{
				localPosition.y = -(0.5f * this.Controller.height) + this.Controller.center.y;
			}
			this.CameraRig.transform.localPosition = localPosition;
		}
		else
		{
			OVRPose? initialPose2 = this.InitialPose;
			if (initialPose2 != null)
			{
				this.CameraRig.transform.localPosition = this.InitialPose.Value.position;
				this.CameraRig.transform.localRotation = this.InitialPose.Value.orientation;
				this.InitialPose = null;
			}
		}
		this.CameraHeight = this.CameraRig.centerEyeAnchor.localPosition.y;
		if (this.CameraUpdated != null)
		{
			this.CameraUpdated();
		}
		this.UpdateMovement();
		Vector3 vector = Vector3.zero;
		float num = 1f + this.Damping * this.SimulationRate * Time.deltaTime;
		this.MoveThrottle.x = this.MoveThrottle.x / num;
		this.MoveThrottle.y = ((this.MoveThrottle.y <= 0f) ? this.MoveThrottle.y : (this.MoveThrottle.y / num));
		this.MoveThrottle.z = this.MoveThrottle.z / num;
		vector += this.MoveThrottle * this.SimulationRate * Time.deltaTime;
		if (this.Controller.isGrounded && this.FallSpeed <= 0f)
		{
			this.FallSpeed = Physics.gravity.y * (this.GravityModifier * 0.002f);
		}
		else
		{
			this.FallSpeed += Physics.gravity.y * (this.GravityModifier * 0.002f) * this.SimulationRate * Time.deltaTime;
		}
		vector.y += this.FallSpeed * this.SimulationRate * Time.deltaTime;
		if (this.Controller.isGrounded && this.MoveThrottle.y <= base.transform.lossyScale.y * 0.001f)
		{
			float stepOffset = this.Controller.stepOffset;
			Vector3 vector2 = new Vector3(vector.x, 0f, vector.z);
			float d = Mathf.Max(stepOffset, vector2.magnitude);
			vector -= d * Vector3.up;
		}
		if (this.PreCharacterMove != null)
		{
			this.PreCharacterMove();
			this.Teleported = false;
		}
		Vector3 vector3 = Vector3.Scale(this.Controller.transform.localPosition + vector, new Vector3(1f, 0f, 1f));
		this.Controller.Move(vector);
		Vector3 vector4 = Vector3.Scale(this.Controller.transform.localPosition, new Vector3(1f, 0f, 1f));
		if (vector3 != vector4)
		{
			this.MoveThrottle += (vector4 - vector3) / (this.SimulationRate * Time.deltaTime);
		}
	}

	
	public virtual void UpdateMovement()
	{
		if (this.HaltUpdateMovement)
		{
			return;
		}
		if (this.EnableLinearMovement)
		{
			bool flag = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
			bool flag2 = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
			bool flag3 = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
			bool flag4 = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
			bool flag5 = false;
			if (OVRInput.Get(OVRInput.Button.DpadUp, OVRInput.Controller.Active))
			{
				flag = true;
				flag5 = true;
			}
			if (OVRInput.Get(OVRInput.Button.DpadDown, OVRInput.Controller.Active))
			{
				flag4 = true;
				flag5 = true;
			}
			this.MoveScale = 1f;
			if ((flag && flag2) || (flag && flag3) || (flag4 && flag2) || (flag4 && flag3))
			{
				this.MoveScale = 0.707106769f;
			}
			if (!this.Controller.isGrounded)
			{
				this.MoveScale = 0f;
			}
			this.MoveScale *= this.SimulationRate * Time.deltaTime;
			float num = this.Acceleration * 0.1f * this.MoveScale * this.MoveScaleMultiplier;
			if (flag5 || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				num *= 2f;
			}
			Vector3 eulerAngles = base.transform.rotation.eulerAngles;
			eulerAngles.z = (eulerAngles.x = 0f);
			Quaternion rotation = Quaternion.Euler(eulerAngles);
			if (flag)
			{
				this.MoveThrottle += rotation * (base.transform.lossyScale.z * num * Vector3.forward);
			}
			if (flag4)
			{
				this.MoveThrottle += rotation * (base.transform.lossyScale.z * num * this.BackAndSideDampen * Vector3.back);
			}
			if (flag2)
			{
				this.MoveThrottle += rotation * (base.transform.lossyScale.x * num * this.BackAndSideDampen * Vector3.left);
			}
			if (flag3)
			{
				this.MoveThrottle += rotation * (base.transform.lossyScale.x * num * this.BackAndSideDampen * Vector3.right);
			}
			num = this.Acceleration * 0.1f * this.MoveScale * this.MoveScaleMultiplier;
			num *= 1f + OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.Active);
			Vector2 vector = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.Active);
			if (this.FixedSpeedSteps > 0)
			{
				vector.y = Mathf.Round(vector.y * (float)this.FixedSpeedSteps) / (float)this.FixedSpeedSteps;
				vector.x = Mathf.Round(vector.x * (float)this.FixedSpeedSteps) / (float)this.FixedSpeedSteps;
			}
			if (vector.y > 0f)
			{
				this.MoveThrottle += rotation * (vector.y * base.transform.lossyScale.z * num * Vector3.forward);
			}
			if (vector.y < 0f)
			{
				this.MoveThrottle += rotation * (Mathf.Abs(vector.y) * base.transform.lossyScale.z * num * this.BackAndSideDampen * Vector3.back);
			}
			if (vector.x < 0f)
			{
				this.MoveThrottle += rotation * (Mathf.Abs(vector.x) * base.transform.lossyScale.x * num * this.BackAndSideDampen * Vector3.left);
			}
			if (vector.x > 0f)
			{
				this.MoveThrottle += rotation * (vector.x * base.transform.lossyScale.x * num * this.BackAndSideDampen * Vector3.right);
			}
		}
		if (this.EnableRotation)
		{
			Vector3 eulerAngles2 = base.transform.rotation.eulerAngles;
			float num2 = this.SimulationRate * Time.deltaTime * this.RotationAmount * this.RotationScaleMultiplier;
			bool flag6 = OVRInput.Get(OVRInput.Button.PrimaryShoulder, OVRInput.Controller.Active);
			if (flag6 && !this.prevHatLeft)
			{
				eulerAngles2.y -= this.RotationRatchet;
			}
			this.prevHatLeft = flag6;
			bool flag7 = OVRInput.Get(OVRInput.Button.SecondaryShoulder, OVRInput.Controller.Active);
			if (flag7 && !this.prevHatRight)
			{
				eulerAngles2.y += this.RotationRatchet;
			}
			this.prevHatRight = flag7;
			eulerAngles2.y += this.buttonRotation;
			this.buttonRotation = 0f;
			if (!this.SkipMouseRotation)
			{
				eulerAngles2.y += Input.GetAxis("Mouse X") * num2 * 3.25f;
			}
			if (this.SnapRotation)
			{
				if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft, OVRInput.Controller.Active))
				{
					if (this.ReadyToSnapTurn)
					{
						eulerAngles2.y -= this.RotationRatchet;
						this.ReadyToSnapTurn = false;
					}
				}
				else if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight, OVRInput.Controller.Active))
				{
					if (this.ReadyToSnapTurn)
					{
						eulerAngles2.y += this.RotationRatchet;
						this.ReadyToSnapTurn = false;
					}
				}
				else
				{
					this.ReadyToSnapTurn = true;
				}
			}
			else
			{
				Vector2 vector2 = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick, OVRInput.Controller.Active);
				eulerAngles2.y += vector2.x * num2;
			}
			base.transform.rotation = Quaternion.Euler(eulerAngles2);
		}
	}

	
	public void UpdateTransform(OVRCameraRig rig)
	{
		Transform trackingSpace = this.CameraRig.trackingSpace;
		Transform centerEyeAnchor = this.CameraRig.centerEyeAnchor;
		if (this.HmdRotatesY && !this.Teleported)
		{
			Vector3 position = trackingSpace.position;
			Quaternion rotation = trackingSpace.rotation;
			base.transform.rotation = Quaternion.Euler(0f, centerEyeAnchor.rotation.eulerAngles.y, 0f);
			trackingSpace.position = position;
			trackingSpace.rotation = rotation;
		}
		this.UpdateController();
		if (this.TransformUpdated != null)
		{
			this.TransformUpdated(trackingSpace);
		}
	}

	
	public bool Jump()
	{
		if (!this.Controller.isGrounded)
		{
			return false;
		}
		this.MoveThrottle += new Vector3(0f, base.transform.lossyScale.y * this.JumpForce, 0f);
		return true;
	}

	
	public void Stop()
	{
		this.Controller.Move(Vector3.zero);
		this.MoveThrottle = Vector3.zero;
		this.FallSpeed = 0f;
	}

	
	public void GetMoveScaleMultiplier(ref float moveScaleMultiplier)
	{
		moveScaleMultiplier = this.MoveScaleMultiplier;
	}

	
	public void SetMoveScaleMultiplier(float moveScaleMultiplier)
	{
		this.MoveScaleMultiplier = moveScaleMultiplier;
	}

	
	public void GetRotationScaleMultiplier(ref float rotationScaleMultiplier)
	{
		rotationScaleMultiplier = this.RotationScaleMultiplier;
	}

	
	public void SetRotationScaleMultiplier(float rotationScaleMultiplier)
	{
		this.RotationScaleMultiplier = rotationScaleMultiplier;
	}

	
	public void GetSkipMouseRotation(ref bool skipMouseRotation)
	{
		skipMouseRotation = this.SkipMouseRotation;
	}

	
	public void SetSkipMouseRotation(bool skipMouseRotation)
	{
		this.SkipMouseRotation = skipMouseRotation;
	}

	
	public void GetHaltUpdateMovement(ref bool haltUpdateMovement)
	{
		haltUpdateMovement = this.HaltUpdateMovement;
	}

	
	public void SetHaltUpdateMovement(bool haltUpdateMovement)
	{
		this.HaltUpdateMovement = haltUpdateMovement;
	}

	
	public void ResetOrientation()
	{
		if (this.HmdResetsY && !this.HmdRotatesY)
		{
			Vector3 eulerAngles = base.transform.rotation.eulerAngles;
			eulerAngles.y = this.InitialYRotation;
			base.transform.rotation = Quaternion.Euler(eulerAngles);
		}
	}

	
	public float Acceleration = 0.1f;

	
	public float Damping = 0.3f;

	
	public float BackAndSideDampen = 0.5f;

	
	public float JumpForce = 0.3f;

	
	public float RotationAmount = 1.5f;

	
	public float RotationRatchet = 45f;

	
	[Tooltip("The player will rotate in fixed steps if Snap Rotation is enabled.")]
	public bool SnapRotation = true;

	
	[Tooltip("How many fixed speeds to use with linear movement? 0=linear control")]
	public int FixedSpeedSteps;

	
	public bool HmdResetsY = true;

	
	public bool HmdRotatesY = true;

	
	public float GravityModifier = 0.379f;

	
	public bool useProfileData = true;

	
	[NonSerialized]
	public float CameraHeight;

	
	[NonSerialized]
	public bool Teleported;

	
	public bool EnableLinearMovement = true;

	
	public bool EnableRotation = true;

	
	protected CharacterController Controller;

	
	protected OVRCameraRig CameraRig;

	
	private float MoveScale = 1f;

	
	private Vector3 MoveThrottle = Vector3.zero;

	
	private float FallSpeed;

	
	private OVRPose? InitialPose;

	
	private float MoveScaleMultiplier = 1f;

	
	private float RotationScaleMultiplier = 1f;

	
	private bool SkipMouseRotation = true;

	
	private bool HaltUpdateMovement;

	
	private bool prevHatLeft;

	
	private bool prevHatRight;

	
	private float SimulationRate = 60f;

	
	private float buttonRotation;

	
	private bool ReadyToSnapTurn;
}
