using System;
using UnityEngine;
using UnityEngine.VR;


[ExecuteInEditMode]
public class OVRCameraRig : MonoBehaviour
{
	
	
	public Camera leftEyeCamera
	{
		get
		{
			return (!this.usePerEyeCameras) ? this._centerEyeCamera : this._leftEyeCamera;
		}
	}

	
	
	public Camera rightEyeCamera
	{
		get
		{
			return (!this.usePerEyeCameras) ? this._centerEyeCamera : this._rightEyeCamera;
		}
	}

	
	
	
	public Transform trackingSpace { get; private set; }

	
	
	
	public Transform leftEyeAnchor { get; private set; }

	
	
	
	public Transform centerEyeAnchor { get; private set; }

	
	
	
	public Transform rightEyeAnchor { get; private set; }

	
	
	
	public Transform leftHandAnchor { get; private set; }

	
	
	
	public Transform rightHandAnchor { get; private set; }

	
	
	
	public Transform trackerAnchor { get; private set; }

	
	
	
	public event Action<OVRCameraRig> UpdatedAnchors;

	
	protected virtual void Awake()
	{
		this._skipUpdate = true;
		this.EnsureGameObjectIntegrity();
	}

	
	protected virtual void Start()
	{
		this.UpdateAnchors();
	}

	
	protected virtual void FixedUpdate()
	{
		if (this.useFixedUpdateForTracking)
		{
			this.UpdateAnchors();
		}
	}

	
	protected virtual void Update()
	{
		this._skipUpdate = false;
		if (!this.useFixedUpdateForTracking)
		{
			this.UpdateAnchors();
		}
	}

	
	protected virtual void UpdateAnchors()
	{
		this.EnsureGameObjectIntegrity();
		if (!Application.isPlaying)
		{
			return;
		}
		if (this._skipUpdate)
		{
			this.centerEyeAnchor.FromOVRPose(OVRPose.identity, true);
			this.leftEyeAnchor.FromOVRPose(OVRPose.identity, true);
			this.rightEyeAnchor.FromOVRPose(OVRPose.identity, true);
			return;
		}
		bool monoscopic = OVRManager.instance.monoscopic;
		OVRPose pose = OVRManager.tracker.GetPose(0);
		this.trackerAnchor.localRotation = pose.orientation;
		this.centerEyeAnchor.localRotation = InputTracking.GetLocalRotation(VRNode.CenterEye);
		this.leftEyeAnchor.localRotation = ((!monoscopic) ? InputTracking.GetLocalRotation(VRNode.LeftEye) : this.centerEyeAnchor.localRotation);
		this.rightEyeAnchor.localRotation = ((!monoscopic) ? InputTracking.GetLocalRotation(VRNode.RightEye) : this.centerEyeAnchor.localRotation);
		this.leftHandAnchor.localRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
		this.rightHandAnchor.localRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
		this.trackerAnchor.localPosition = pose.position;
		this.centerEyeAnchor.localPosition = InputTracking.GetLocalPosition(VRNode.CenterEye);
		this.leftEyeAnchor.localPosition = ((!monoscopic) ? InputTracking.GetLocalPosition(VRNode.LeftEye) : this.centerEyeAnchor.localPosition);
		this.rightEyeAnchor.localPosition = ((!monoscopic) ? InputTracking.GetLocalPosition(VRNode.RightEye) : this.centerEyeAnchor.localPosition);
		this.leftHandAnchor.localPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
		this.rightHandAnchor.localPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
		this.RaiseUpdatedAnchorsEvent();
	}

	
	protected virtual void RaiseUpdatedAnchorsEvent()
	{
		if (this.UpdatedAnchors != null)
		{
			this.UpdatedAnchors(this);
		}
	}

	
	public virtual void EnsureGameObjectIntegrity()
	{
		if (this.trackingSpace == null)
		{
			this.trackingSpace = this.ConfigureAnchor(null, this.trackingSpaceName);
		}
		if (this.leftEyeAnchor == null)
		{
			this.leftEyeAnchor = this.ConfigureAnchor(this.trackingSpace, this.leftEyeAnchorName);
		}
		if (this.centerEyeAnchor == null)
		{
			this.centerEyeAnchor = this.ConfigureAnchor(this.trackingSpace, this.centerEyeAnchorName);
		}
		if (this.rightEyeAnchor == null)
		{
			this.rightEyeAnchor = this.ConfigureAnchor(this.trackingSpace, this.rightEyeAnchorName);
		}
		if (this.leftHandAnchor == null)
		{
			this.leftHandAnchor = this.ConfigureAnchor(this.trackingSpace, this.leftHandAnchorName);
		}
		if (this.rightHandAnchor == null)
		{
			this.rightHandAnchor = this.ConfigureAnchor(this.trackingSpace, this.rightHandAnchorName);
		}
		if (this.trackerAnchor == null)
		{
			this.trackerAnchor = this.ConfigureAnchor(this.trackingSpace, this.trackerAnchorName);
		}
		if (this._centerEyeCamera == null || this._leftEyeCamera == null || this._rightEyeCamera == null)
		{
			this._centerEyeCamera = this.centerEyeAnchor.GetComponent<Camera>();
			this._leftEyeCamera = this.leftEyeAnchor.GetComponent<Camera>();
			this._rightEyeCamera = this.rightEyeAnchor.GetComponent<Camera>();
			if (this._centerEyeCamera == null)
			{
				this._centerEyeCamera = this.centerEyeAnchor.gameObject.AddComponent<Camera>();
				this._centerEyeCamera.tag = "MainCamera";
			}
			if (this._leftEyeCamera == null)
			{
				this._leftEyeCamera = this.leftEyeAnchor.gameObject.AddComponent<Camera>();
				this._leftEyeCamera.tag = "MainCamera";
			}
			if (this._rightEyeCamera == null)
			{
				this._rightEyeCamera = this.rightEyeAnchor.gameObject.AddComponent<Camera>();
				this._rightEyeCamera.tag = "MainCamera";
			}
			this._centerEyeCamera.stereoTargetEye = StereoTargetEyeMask.Both;
			this._leftEyeCamera.stereoTargetEye = StereoTargetEyeMask.Left;
			this._rightEyeCamera.stereoTargetEye = StereoTargetEyeMask.Right;
		}
		if (this._centerEyeCamera.enabled == this.usePerEyeCameras || this._leftEyeCamera.enabled == !this.usePerEyeCameras || this._rightEyeCamera.enabled == !this.usePerEyeCameras)
		{
			this._skipUpdate = true;
		}
		this._centerEyeCamera.enabled = !this.usePerEyeCameras;
		this._leftEyeCamera.enabled = this.usePerEyeCameras;
		this._rightEyeCamera.enabled = this.usePerEyeCameras;
	}

	
	protected virtual Transform ConfigureAnchor(Transform root, string name)
	{
		Transform transform = (!(root != null)) ? null : base.transform.Find(root.name + "/" + name);
		if (transform == null)
		{
			transform = base.transform.Find(name);
		}
		if (transform == null)
		{
			transform = new GameObject(name).transform;
		}
		transform.name = name;
		transform.parent = ((!(root != null)) ? base.transform : root);
		transform.localScale = Vector3.one;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		return transform;
	}

	
	public virtual Matrix4x4 ComputeTrackReferenceMatrix()
	{
		if (this.centerEyeAnchor == null)
		{
			Debug.LogError("centerEyeAnchor is required");
			return Matrix4x4.identity;
		}
		OVRPose ovrpose;
		ovrpose.position = InputTracking.GetLocalPosition(VRNode.Head);
		ovrpose.orientation = InputTracking.GetLocalRotation(VRNode.Head);
		OVRPose ovrpose2 = ovrpose.Inverse();
		Matrix4x4 rhs = Matrix4x4.TRS(ovrpose2.position, ovrpose2.orientation, Vector3.one);
		return this.centerEyeAnchor.localToWorldMatrix * rhs;
	}

	
	public bool usePerEyeCameras;

	
	public bool useFixedUpdateForTracking;

	
	protected bool _skipUpdate;

	
	protected readonly string trackingSpaceName = "TrackingSpace";

	
	protected readonly string trackerAnchorName = "TrackerAnchor";

	
	protected readonly string leftEyeAnchorName = "LeftEyeAnchor";

	
	protected readonly string centerEyeAnchorName = "CenterEyeAnchor";

	
	protected readonly string rightEyeAnchorName = "RightEyeAnchor";

	
	protected readonly string leftHandAnchorName = "LeftHandAnchor";

	
	protected readonly string rightHandAnchorName = "RightHandAnchor";

	
	protected Camera _centerEyeCamera;

	
	protected Camera _leftEyeCamera;

	
	protected Camera _rightEyeCamera;
}
