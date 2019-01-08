using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
	public class Hand : MonoBehaviour
	{
		public ReadOnlyCollection<Hand.AttachedObject> AttachedObjects
		{
			get
			{
				return this.attachedObjects.AsReadOnly();
			}
		}

		public bool hoverLocked { get; private set; }

		public Interactable hoveringInteractable
		{
			get
			{
				return this._hoveringInteractable;
			}
			set
			{
				if (this._hoveringInteractable != value)
				{
					if (this._hoveringInteractable != null)
					{
						this.HandDebugLog("HoverEnd " + this._hoveringInteractable.gameObject);
						this._hoveringInteractable.SendMessage("OnHandHoverEnd", this, SendMessageOptions.DontRequireReceiver);
						if (this._hoveringInteractable != null)
						{
							base.BroadcastMessage("OnParentHandHoverEnd", this._hoveringInteractable, SendMessageOptions.DontRequireReceiver);
						}
					}
					this._hoveringInteractable = value;
					if (this._hoveringInteractable != null)
					{
						this.HandDebugLog("HoverBegin " + this._hoveringInteractable.gameObject);
						this._hoveringInteractable.SendMessage("OnHandHoverBegin", this, SendMessageOptions.DontRequireReceiver);
						if (this._hoveringInteractable != null)
						{
							base.BroadcastMessage("OnParentHandHoverBegin", this._hoveringInteractable, SendMessageOptions.DontRequireReceiver);
						}
					}
				}
			}
		}

		public GameObject currentAttachedObject
		{
			get
			{
				this.CleanUpAttachedObjectStack();
				if (this.attachedObjects.Count > 0)
				{
					return this.attachedObjects[this.attachedObjects.Count - 1].attachedObject;
				}
				return null;
			}
		}

		public Transform GetAttachmentTransform(string attachmentPoint = "")
		{
			Transform transform = null;
			if (!string.IsNullOrEmpty(attachmentPoint))
			{
				transform = base.transform.Find(attachmentPoint);
			}
			if (!transform)
			{
				transform = base.transform;
			}
			return transform;
		}

		public Hand.HandType GuessCurrentHandType()
		{
			if (this.startingHandType == Hand.HandType.Left || this.startingHandType == Hand.HandType.Right)
			{
				return this.startingHandType;
			}
			if (this.startingHandType == Hand.HandType.Any && this.otherHand != null && this.otherHand.controller == null)
			{
				return Hand.HandType.Right;
			}
			if (this.controller == null || this.otherHand == null || this.otherHand.controller == null)
			{
				return this.startingHandType;
			}
			if ((ulong)this.controller.index == (ulong)((long)SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost, ETrackedDeviceClass.Controller, 0)))
			{
				return Hand.HandType.Left;
			}
			return Hand.HandType.Right;
		}

		public void AttachObject(GameObject objectToAttach, Hand.AttachmentFlags flags = Hand.AttachmentFlags.SnapOnAttach | Hand.AttachmentFlags.DetachOthers | Hand.AttachmentFlags.DetachFromOtherHand | Hand.AttachmentFlags.ParentToHand, string attachmentPoint = "")
		{
			if (flags == (Hand.AttachmentFlags)0)
			{
				flags = (Hand.AttachmentFlags.SnapOnAttach | Hand.AttachmentFlags.DetachOthers | Hand.AttachmentFlags.DetachFromOtherHand | Hand.AttachmentFlags.ParentToHand);
			}
			this.CleanUpAttachedObjectStack();
			this.DetachObject(objectToAttach, true);
			if ((flags & Hand.AttachmentFlags.DetachFromOtherHand) == Hand.AttachmentFlags.DetachFromOtherHand && this.otherHand)
			{
				this.otherHand.DetachObject(objectToAttach, true);
			}
			if ((flags & Hand.AttachmentFlags.DetachOthers) == Hand.AttachmentFlags.DetachOthers)
			{
				while (this.attachedObjects.Count > 0)
				{
					this.DetachObject(this.attachedObjects[0].attachedObject, true);
				}
			}
			if (this.currentAttachedObject)
			{
				this.currentAttachedObject.SendMessage("OnHandFocusLost", this, SendMessageOptions.DontRequireReceiver);
			}
			Hand.AttachedObject item = default(Hand.AttachedObject);
			item.attachedObject = objectToAttach;
			item.originalParent = ((!(objectToAttach.transform.parent != null)) ? null : objectToAttach.transform.parent.gameObject);
			if ((flags & Hand.AttachmentFlags.ParentToHand) == Hand.AttachmentFlags.ParentToHand)
			{
				objectToAttach.transform.parent = this.GetAttachmentTransform(attachmentPoint);
				item.isParentedToHand = true;
			}
			else
			{
				item.isParentedToHand = false;
			}
			this.attachedObjects.Add(item);
			if ((flags & Hand.AttachmentFlags.SnapOnAttach) == Hand.AttachmentFlags.SnapOnAttach)
			{
				objectToAttach.transform.localPosition = Vector3.zero;
				objectToAttach.transform.localRotation = Quaternion.identity;
			}
			this.HandDebugLog("AttachObject " + objectToAttach);
			objectToAttach.SendMessage("OnAttachedToHand", this, SendMessageOptions.DontRequireReceiver);
			this.UpdateHovering();
		}

		public void DetachObject(GameObject objectToDetach, bool restoreOriginalParent = true)
		{
			int num = this.attachedObjects.FindIndex((Hand.AttachedObject l) => l.attachedObject == objectToDetach);
			if (num != -1)
			{
				this.HandDebugLog("DetachObject " + objectToDetach);
				GameObject currentAttachedObject = this.currentAttachedObject;
				Transform parent = null;
				if (this.attachedObjects[num].isParentedToHand)
				{
					if (restoreOriginalParent && this.attachedObjects[num].originalParent != null)
					{
						parent = this.attachedObjects[num].originalParent.transform;
					}
					this.attachedObjects[num].attachedObject.transform.parent = parent;
				}
				this.attachedObjects[num].attachedObject.SetActive(true);
				this.attachedObjects[num].attachedObject.SendMessage("OnDetachedFromHand", this, SendMessageOptions.DontRequireReceiver);
				this.attachedObjects.RemoveAt(num);
				GameObject currentAttachedObject2 = this.currentAttachedObject;
				if (currentAttachedObject2 != null && currentAttachedObject2 != currentAttachedObject)
				{
					currentAttachedObject2.SetActive(true);
					currentAttachedObject2.SendMessage("OnHandFocusAcquired", this, SendMessageOptions.DontRequireReceiver);
				}
			}
			this.CleanUpAttachedObjectStack();
		}

		public Vector3 GetTrackedObjectVelocity()
		{
			if (this.controller != null)
			{
				return base.transform.parent.TransformVector(this.controller.velocity);
			}
			return Vector3.zero;
		}

		public Vector3 GetTrackedObjectAngularVelocity()
		{
			if (this.controller != null)
			{
				return base.transform.parent.TransformVector(this.controller.angularVelocity);
			}
			return Vector3.zero;
		}

		private void CleanUpAttachedObjectStack()
		{
			this.attachedObjects.RemoveAll((Hand.AttachedObject l) => l.attachedObject == null);
		}

		private void Awake()
		{
			this.trackedDeviceRoleChangedAction = SteamVR_Events.SystemAction(EVREventType.VREvent_TrackedDeviceRoleChanged, new UnityAction<VREvent_t>(this.OnTrackedDeviceRoleChanged));
			this.inputFocusAction = SteamVR_Events.InputFocusAction(new UnityAction<bool>(this.OnInputFocus));
			if (this.hoverSphereTransform == null)
			{
				this.hoverSphereTransform = base.transform;
			}
			this.applicationLostFocusObject = new GameObject("_application_lost_focus");
			this.applicationLostFocusObject.transform.parent = base.transform;
			this.applicationLostFocusObject.SetActive(false);
		}

		private IEnumerator Start()
		{
			this.playerInstance = Player.instance;
			if (!this.playerInstance)
			{
				Debug.LogError("No player instance found in Hand Start()");
			}
			this.overlappingColliders = new Collider[16];
			if (this.noSteamVRFallbackCamera)
			{
				yield break;
			}
			SteamVR vr = SteamVR.instance;
			for (;;)
			{
				yield return new WaitForSeconds(1f);
				if (this.controller != null)
				{
					break;
				}
				if (this.startingHandType == Hand.HandType.Left || this.startingHandType == Hand.HandType.Right)
				{
					CVRSystem system = OpenVR.System;
					uint trackedDeviceIndexForControllerRole = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
					uint trackedDeviceIndexForControllerRole2 = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
					int num;
					if (trackedDeviceIndexForControllerRole != 4294967295u)
					{
						num = (int)trackedDeviceIndexForControllerRole;
					}
					else
					{
						num = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost, ETrackedDeviceClass.Controller, 0);
					}
					int num2;
					if (trackedDeviceIndexForControllerRole2 != 4294967295u)
					{
						num2 = (int)trackedDeviceIndexForControllerRole2;
					}
					else
					{
						num2 = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost, ETrackedDeviceClass.Controller, 0);
					}
					if (num != -1 && num2 != -1 && num != num2)
					{
						int index = (this.startingHandType != Hand.HandType.Right) ? num : num2;
						int index2 = (this.startingHandType != Hand.HandType.Right) ? num2 : num;
						this.InitController(index);
						if (this.otherHand)
						{
							this.otherHand.InitController(index2);
						}
					}
				}
				else
				{
					int num3 = 0;
					while ((long)num3 < 64L)
					{
						if (vr.hmd.GetTrackedDeviceClass((uint)num3) == ETrackedDeviceClass.Controller)
						{
							SteamVR_Controller.Device device = SteamVR_Controller.Input(num3);
							if (device.valid)
							{
								if (!(this.otherHand != null) || this.otherHand.controller == null || num3 != (int)this.otherHand.controller.index)
								{
									this.InitController(num3);
								}
							}
						}
						num3++;
					}
				}
			}
			yield break;
		}

		private void OnTrackedDeviceRoleChanged(VREvent_t vrEvent)
		{
			this.RefreshAssignedRoles();
		}

		private void RefreshAssignedRoles()
		{
			CVRSystem system = OpenVR.System;
			SteamVR_Controller.Device device = this.controller;
			uint trackedDeviceIndexForControllerRole = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
			uint trackedDeviceIndexForControllerRole2 = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
			uint num = (this.controller != null) ? this.controller.index : uint.MaxValue;
			uint num2 = uint.MaxValue;
			if (this.startingHandType == Hand.HandType.Left && num != trackedDeviceIndexForControllerRole)
			{
				num2 = trackedDeviceIndexForControllerRole;
			}
			else if (this.startingHandType == Hand.HandType.Right && num != trackedDeviceIndexForControllerRole2)
			{
				num2 = trackedDeviceIndexForControllerRole2;
			}
			if (num2 == 4294967295u)
			{
				return;
			}
			try
			{
				this.controller = SteamVR_Controller.Input((int)num2);
				if (this.controller != device)
				{
					if (this.otherHand != null)
					{
						this.otherHand.controller = device;
					}
				}
			}
			catch
			{
			}
		}

		private void UpdateHovering()
		{
			if (this.playerInstance == null)
			{
				return;
			}
			if (this.noSteamVRFallbackCamera == null && this.controller == null)
			{
				return;
			}
			if (this.hoverLocked)
			{
				return;
			}
			if (this.applicationLostFocusObject.activeSelf)
			{
				return;
			}
			float num = float.MaxValue;
			Interactable hoveringInteractable = null;
			float num2 = 1f;
			if (this.playerInstance)
			{
				num2 = this.playerInstance.trackingOriginTransform.lossyScale.x;
			}
			float num3 = this.hoverSphereRadius * num2;
			float num4 = Mathf.Abs(base.transform.position.y - this.playerInstance.trackingOriginTransform.position.y);
			float num5 = Util.RemapNumberClamped(num4, 0f, 0.5f * num2, 5f, 1f) * num2;
			for (int i = 0; i < this.overlappingColliders.Length; i++)
			{
				this.overlappingColliders[i] = null;
			}
			Physics.OverlapBoxNonAlloc(this.hoverSphereTransform.position - new Vector3(0f, num3 * num5 - num3, 0f), new Vector3(num3, num3 * num5 * 2f, num3), this.overlappingColliders, Quaternion.identity, this.hoverLayerMask.value);
			int num6 = 0;
			Collider[] array = this.overlappingColliders;
			for (int j = 0; j < array.Length; j++)
			{
				Collider collider = array[j];
				if (!(collider == null))
				{
					Interactable contacting = collider.GetComponentInParent<Interactable>();
					if (!(contacting == null))
					{
						IgnoreHovering component = collider.GetComponent<IgnoreHovering>();
						if (!(component != null) || (!(component.onlyIgnoreHand == null) && !(component.onlyIgnoreHand == this)))
						{
							if (this.attachedObjects.FindIndex((Hand.AttachedObject l) => l.attachedObject == contacting.gameObject) == -1)
							{
								if (!this.otherHand || !(this.otherHand.hoveringInteractable == contacting))
								{
									float num7 = Vector3.Distance(contacting.transform.position, this.hoverSphereTransform.position);
									if (num7 < num)
									{
										num = num7;
										hoveringInteractable = contacting;
									}
									num6++;
								}
							}
						}
					}
				}
			}
			this.hoveringInteractable = hoveringInteractable;
			if (num6 > 0 && num6 != this.prevOverlappingColliders)
			{
				this.prevOverlappingColliders = num6;
				this.HandDebugLog("Found " + num6 + " overlapping colliders.");
			}
		}

		private void UpdateNoSteamVRFallback()
		{
			if (this.noSteamVRFallbackCamera)
			{
				Ray ray = this.noSteamVRFallbackCamera.ScreenPointToRay(Input.mousePosition);
				if (this.attachedObjects.Count > 0)
				{
					base.transform.position = ray.origin + this.noSteamVRFallbackInteractorDistance * ray.direction;
				}
				else
				{
					Vector3 position = base.transform.position;
					base.transform.position = this.noSteamVRFallbackCamera.transform.forward * -1000f;
					RaycastHit raycastHit;
					if (Physics.Raycast(ray, out raycastHit, this.noSteamVRFallbackMaxDistanceNoItem))
					{
						base.transform.position = raycastHit.point;
						this.noSteamVRFallbackInteractorDistance = Mathf.Min(this.noSteamVRFallbackMaxDistanceNoItem, raycastHit.distance);
					}
					else if (this.noSteamVRFallbackInteractorDistance > 0f)
					{
						base.transform.position = ray.origin + Mathf.Min(this.noSteamVRFallbackMaxDistanceNoItem, this.noSteamVRFallbackInteractorDistance) * ray.direction;
					}
					else
					{
						base.transform.position = position;
					}
				}
			}
		}

		private void UpdateDebugText()
		{
			if (this.showDebugText)
			{
				if (this.debugText == null)
				{
					this.debugText = new GameObject("_debug_text").AddComponent<TextMesh>();
					this.debugText.fontSize = 120;
					this.debugText.characterSize = 0.001f;
					this.debugText.transform.parent = base.transform;
					this.debugText.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
				}
				if (this.GuessCurrentHandType() == Hand.HandType.Right)
				{
					this.debugText.transform.localPosition = new Vector3(-0.05f, 0f, 0f);
					this.debugText.alignment = TextAlignment.Right;
					this.debugText.anchor = TextAnchor.UpperRight;
				}
				else
				{
					this.debugText.transform.localPosition = new Vector3(0.05f, 0f, 0f);
					this.debugText.alignment = TextAlignment.Left;
					this.debugText.anchor = TextAnchor.UpperLeft;
				}
				this.debugText.text = string.Format("Hovering: {0}\nHover Lock: {1}\nAttached: {2}\nTotal Attached: {3}\nType: {4}\n", new object[]
				{
					(!this.hoveringInteractable) ? "null" : this.hoveringInteractable.gameObject.name,
					this.hoverLocked,
					(!this.currentAttachedObject) ? "null" : this.currentAttachedObject.name,
					this.attachedObjects.Count,
					this.GuessCurrentHandType().ToString()
				});
			}
			else if (this.debugText != null)
			{
				UnityEngine.Object.Destroy(this.debugText.gameObject);
			}
		}

		private void OnEnable()
		{
			this.inputFocusAction.enabled = true;
			float time = (!(this.otherHand != null) || this.otherHand.GetInstanceID() >= base.GetInstanceID()) ? 0f : (0.5f * this.hoverUpdateInterval);
			base.InvokeRepeating("UpdateHovering", time, this.hoverUpdateInterval);
			base.InvokeRepeating("UpdateDebugText", time, this.hoverUpdateInterval);
		}

		private void OnDisable()
		{
			this.inputFocusAction.enabled = false;
			base.CancelInvoke();
		}

		private void Update()
		{
			if (this.controller != null)
			{
				CVRSystem system = OpenVR.System;
				this.ETrackedControllerRole = system.GetControllerRoleForTrackedDeviceIndex(this.controller.index);
				if (!this.TrackedHandMatchesStarting())
				{
					this.RefreshAssignedRoles();
				}
			}
			this.UpdateHandPoses();
			this.UpdateNoSteamVRFallback();
			GameObject currentAttachedObject = this.currentAttachedObject;
			if (currentAttachedObject)
			{
				currentAttachedObject.SendMessage("HandAttachedUpdate", this, SendMessageOptions.DontRequireReceiver);
			}
			if (this.hoveringInteractable)
			{
				this.hoveringInteractable.SendMessage("HandHoverUpdate", this, SendMessageOptions.DontRequireReceiver);
			}
			if (Time.timeScale == 0f)
			{
				this.UpdateHandPoses();
			}
		}

		private bool TrackedHandMatchesStarting()
		{
			return (this.ETrackedControllerRole == ETrackedControllerRole.LeftHand && this.startingHandType == Hand.HandType.Left) || (this.ETrackedControllerRole == ETrackedControllerRole.RightHand && this.startingHandType == Hand.HandType.Right);
		}

		private void LateUpdate()
		{
			if (this.controllerObject != null && this.attachedObjects.Count == 0)
			{
				this.AttachObject(this.controllerObject, Hand.AttachmentFlags.SnapOnAttach | Hand.AttachmentFlags.DetachOthers | Hand.AttachmentFlags.DetachFromOtherHand | Hand.AttachmentFlags.ParentToHand, string.Empty);
			}
		}

		private void OnInputFocus(bool hasFocus)
		{
			if (hasFocus)
			{
				this.DetachObject(this.applicationLostFocusObject, true);
				this.applicationLostFocusObject.SetActive(false);
				this.UpdateHandPoses();
				this.UpdateHovering();
				base.BroadcastMessage("OnParentHandInputFocusAcquired", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				this.applicationLostFocusObject.SetActive(true);
				this.AttachObject(this.applicationLostFocusObject, Hand.AttachmentFlags.ParentToHand, string.Empty);
				base.BroadcastMessage("OnParentHandInputFocusLost", SendMessageOptions.DontRequireReceiver);
			}
		}

		private void FixUpedUpdate()
		{
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(0.5f, 1f, 0.5f, 0.9f);
			Transform transform = (!this.hoverSphereTransform) ? base.transform : this.hoverSphereTransform;
			Gizmos.DrawWireSphere(transform.position, this.hoverSphereRadius);
		}

		private void HandDebugLog(string msg)
		{
			if (this.spewDebugText)
			{
				Debug.Log("Hand (" + base.name + "): " + msg);
			}
		}

		private void UpdateHandPoses()
		{
			if (this.controller != null)
			{
				SteamVR instance = SteamVR.instance;
				if (instance != null)
				{
					TrackedDevicePose_t trackedDevicePose_t = default(TrackedDevicePose_t);
					TrackedDevicePose_t trackedDevicePose_t2 = default(TrackedDevicePose_t);
					if (instance.compositor.GetLastPoseForTrackedDeviceIndex(this.controller.index, ref trackedDevicePose_t, ref trackedDevicePose_t2) == EVRCompositorError.None)
					{
						SteamVR_Utils.RigidTransform rigidTransform = new SteamVR_Utils.RigidTransform(trackedDevicePose_t2.mDeviceToAbsoluteTracking);
						base.transform.localPosition = rigidTransform.pos;
						base.transform.localRotation = rigidTransform.rot;
					}
				}
			}
		}

		public void HoverLock(Interactable interactable)
		{
			this.HandDebugLog("HoverLock " + interactable);
			this.hoverLocked = true;
			this.hoveringInteractable = interactable;
		}

		public void HoverUnlock(Interactable interactable)
		{
			this.HandDebugLog("HoverUnlock " + interactable);
			if (this.hoveringInteractable == interactable)
			{
				this.hoverLocked = false;
			}
		}

		public bool GetStandardInteractionButtonDown()
		{
			if (this.noSteamVRFallbackCamera)
			{
				return Input.GetMouseButtonDown(0);
			}
			return this.controller != null && this.controller.GetHairTriggerDown();
		}

		public bool GetStandardInteractionButtonUp()
		{
			if (this.noSteamVRFallbackCamera)
			{
				return Input.GetMouseButtonUp(0);
			}
			return this.controller != null && this.controller.GetHairTriggerUp();
		}

		public bool GetStandardInteractionButton()
		{
			if (this.noSteamVRFallbackCamera)
			{
				return Input.GetMouseButton(0);
			}
			return this.controller != null && this.controller.GetHairTrigger();
		}

		private void InitController(int index)
		{
			if (this.controller == null)
			{
				this.controller = SteamVR_Controller.Input(index);
				this.HandDebugLog(string.Concat(new object[]
				{
					"Hand ",
					base.name,
					" connected with device index ",
					this.controller.index
				}));
				this.controllerObject = UnityEngine.Object.Instantiate<GameObject>(this.controllerPrefab);
				this.controllerObject.SetActive(true);
				this.controllerObject.name = this.controllerPrefab.name + "_" + base.name;
				this.controllerObject.layer = base.gameObject.layer;
				this.controllerObject.tag = base.gameObject.tag;
				this.AttachObject(this.controllerObject, Hand.AttachmentFlags.SnapOnAttach | Hand.AttachmentFlags.DetachOthers | Hand.AttachmentFlags.DetachFromOtherHand | Hand.AttachmentFlags.ParentToHand, string.Empty);
				this.controller.TriggerHapticPulse(800, EVRButtonId.k_EButton_Axis0);
				this.controllerObject.transform.localScale = this.controllerPrefab.transform.localScale;
				base.BroadcastMessage("OnHandInitialized", index, SendMessageOptions.DontRequireReceiver);
			}
		}

		public const Hand.AttachmentFlags defaultAttachmentFlags = Hand.AttachmentFlags.SnapOnAttach | Hand.AttachmentFlags.DetachOthers | Hand.AttachmentFlags.DetachFromOtherHand | Hand.AttachmentFlags.ParentToHand;

		public Hand otherHand;

		public Hand.HandType startingHandType;

		public Transform hoverSphereTransform;

		public float hoverSphereRadius = 0.05f;

		public LayerMask hoverLayerMask = -1;

		public float hoverUpdateInterval = 0.1f;

		public Camera noSteamVRFallbackCamera;

		public float noSteamVRFallbackMaxDistanceNoItem = 10f;

		public float noSteamVRFallbackMaxDistanceWithItem = 0.5f;

		private float noSteamVRFallbackInteractorDistance = -1f;

		public SteamVR_Controller.Device controller;

		public GameObject controllerPrefab;

		private GameObject controllerObject;

		public bool showDebugText;

		public bool spewDebugText;

		private List<Hand.AttachedObject> attachedObjects = new List<Hand.AttachedObject>();

		private Interactable _hoveringInteractable;

		private TextMesh debugText;

		private int prevOverlappingColliders;

		private const int ColliderArraySize = 16;

		private Collider[] overlappingColliders;

		private Player playerInstance;

		private GameObject applicationLostFocusObject;

		private SteamVR_Events.Action inputFocusAction;

		private SteamVR_Events.Action trackedDeviceRoleChangedAction;

		public ETrackedControllerRole ETrackedControllerRole;

		public enum HandType
		{
			Left,
			Right,
			Any
		}

		[Flags]
		public enum AttachmentFlags
		{
			SnapOnAttach = 1,
			DetachOthers = 2,
			DetachFromOtherHand = 4,
			ParentToHand = 8
		}

		public struct AttachedObject
		{
			public GameObject attachedObject;

			public GameObject originalParent;

			public bool isParentedToHand;
		}
	}
}
