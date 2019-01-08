using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
	[RequireComponent(typeof(Interactable))]
	public class CircularDrive : MonoBehaviour
	{
		private void Freeze(Hand hand)
		{
			this.frozen = true;
			this.frozenAngle = this.outAngle;
			this.frozenHandWorldPos = hand.hoverSphereTransform.position;
			this.frozenSqDistanceMinMaxThreshold.x = this.frozenDistanceMinMaxThreshold.x * this.frozenDistanceMinMaxThreshold.x;
			this.frozenSqDistanceMinMaxThreshold.y = this.frozenDistanceMinMaxThreshold.y * this.frozenDistanceMinMaxThreshold.y;
		}

		private void UnFreeze()
		{
			this.frozen = false;
			this.frozenHandWorldPos.Set(0f, 0f, 0f);
		}

		private void Start()
		{
			if (this.childCollider == null)
			{
				this.childCollider = base.GetComponentInChildren<Collider>();
			}
			if (this.linearMapping == null)
			{
				this.linearMapping = base.GetComponent<LinearMapping>();
			}
			if (this.linearMapping == null)
			{
				this.linearMapping = base.gameObject.AddComponent<LinearMapping>();
			}
			this.worldPlaneNormal = new Vector3(0f, 0f, 0f);
			this.worldPlaneNormal[(int)this.axisOfRotation] = 1f;
			this.localPlaneNormal = this.worldPlaneNormal;
			if (base.transform.parent)
			{
				this.worldPlaneNormal = base.transform.parent.localToWorldMatrix.MultiplyVector(this.worldPlaneNormal).normalized;
			}
			if (this.limited)
			{
				this.start = Quaternion.identity;
				this.outAngle = base.transform.localEulerAngles[(int)this.axisOfRotation];
				if (this.forceStart)
				{
					this.outAngle = Mathf.Clamp(this.startAngle, this.minAngle, this.maxAngle);
				}
			}
			else
			{
				this.start = Quaternion.AngleAxis(base.transform.localEulerAngles[(int)this.axisOfRotation], this.localPlaneNormal);
				this.outAngle = 0f;
			}
			if (this.debugText)
			{
				this.debugText.alignment = TextAlignment.Left;
				this.debugText.anchor = TextAnchor.UpperLeft;
			}
			this.UpdateAll();
		}

		private void OnDisable()
		{
			if (this.handHoverLocked)
			{
				ControllerButtonHints.HideButtonHint(this.handHoverLocked, new EVRButtonId[]
				{
					EVRButtonId.k_EButton_Axis1
				});
				this.handHoverLocked.HoverUnlock(base.GetComponent<Interactable>());
				this.handHoverLocked = null;
			}
		}

		private IEnumerator HapticPulses(SteamVR_Controller.Device controller, float flMagnitude, int nCount)
		{
			if (controller != null)
			{
				int nRangeMax = (int)Util.RemapNumberClamped(flMagnitude, 0f, 1f, 100f, 900f);
				nCount = Mathf.Clamp(nCount, 1, 10);
				ushort i = 0;
				while ((int)i < nCount)
				{
					ushort duration = (ushort)UnityEngine.Random.Range(100, nRangeMax);
					controller.TriggerHapticPulse(duration, EVRButtonId.k_EButton_Axis0);
					yield return new WaitForSeconds(0.01f);
					i += 1;
				}
			}
			yield break;
		}

		private void OnHandHoverBegin(Hand hand)
		{
			ControllerButtonHints.ShowButtonHint(hand, new EVRButtonId[]
			{
				EVRButtonId.k_EButton_Axis1
			});
		}

		private void OnHandHoverEnd(Hand hand)
		{
			ControllerButtonHints.HideButtonHint(hand, new EVRButtonId[]
			{
				EVRButtonId.k_EButton_Axis1
			});
			if (this.driving && hand.GetStandardInteractionButton())
			{
				base.StartCoroutine(this.HapticPulses(hand.controller, 1f, 10));
			}
			this.driving = false;
			this.handHoverLocked = null;
		}

		private void HandHoverUpdate(Hand hand)
		{
			if (hand.GetStandardInteractionButtonDown())
			{
				this.lastHandProjected = this.ComputeToTransformProjected(hand.hoverSphereTransform);
				if (this.hoverLock)
				{
					hand.HoverLock(base.GetComponent<Interactable>());
					this.handHoverLocked = hand;
				}
				this.driving = true;
				this.ComputeAngle(hand);
				this.UpdateAll();
				ControllerButtonHints.HideButtonHint(hand, new EVRButtonId[]
				{
					EVRButtonId.k_EButton_Axis1
				});
			}
			else if (hand.GetStandardInteractionButtonUp())
			{
				if (this.hoverLock)
				{
					hand.HoverUnlock(base.GetComponent<Interactable>());
					this.handHoverLocked = null;
				}
			}
			else if (this.driving && hand.GetStandardInteractionButton() && hand.hoveringInteractable == base.GetComponent<Interactable>())
			{
				this.ComputeAngle(hand);
				this.UpdateAll();
			}
		}

		private Vector3 ComputeToTransformProjected(Transform xForm)
		{
			Vector3 normalized = (xForm.position - base.transform.position).normalized;
			Vector3 normalized2 = new Vector3(0f, 0f, 0f);
			if (normalized.sqrMagnitude > 0f)
			{
				normalized2 = Vector3.ProjectOnPlane(normalized, this.worldPlaneNormal).normalized;
			}
			else
			{
				Debug.LogFormat("The collider needs to be a minimum distance away from the CircularDrive GameObject {0}", new object[]
				{
					base.gameObject.ToString()
				});
			}
			if (this.debugPath && this.dbgPathLimit > 0)
			{
				this.DrawDebugPath(xForm, normalized2);
			}
			return normalized2;
		}

		private void DrawDebugPath(Transform xForm, Vector3 toTransformProjected)
		{
			if (this.dbgObjectCount == 0)
			{
				this.dbgObjectsParent = new GameObject("Circular Drive Debug");
				this.dbgHandObjects = new GameObject[this.dbgPathLimit];
				this.dbgProjObjects = new GameObject[this.dbgPathLimit];
				this.dbgObjectCount = this.dbgPathLimit;
				this.dbgObjectIndex = 0;
			}
			GameObject gameObject;
			if (this.dbgHandObjects[this.dbgObjectIndex])
			{
				gameObject = this.dbgHandObjects[this.dbgObjectIndex];
			}
			else
			{
				gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				gameObject.transform.SetParent(this.dbgObjectsParent.transform);
				this.dbgHandObjects[this.dbgObjectIndex] = gameObject;
			}
			gameObject.name = string.Format("actual_{0}", (int)((1f - this.red.r) * 10f));
			gameObject.transform.position = xForm.position;
			gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
			gameObject.transform.localScale = new Vector3(0.004f, 0.004f, 0.004f);
			gameObject.gameObject.GetComponent<Renderer>().material.color = this.red;
			if (this.red.r > 0.1f)
			{
				this.red.r = this.red.r - 0.1f;
			}
			else
			{
				this.red.r = 1f;
			}
			if (this.dbgProjObjects[this.dbgObjectIndex])
			{
				gameObject = this.dbgProjObjects[this.dbgObjectIndex];
			}
			else
			{
				gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				gameObject.transform.SetParent(this.dbgObjectsParent.transform);
				this.dbgProjObjects[this.dbgObjectIndex] = gameObject;
			}
			gameObject.name = string.Format("projed_{0}", (int)((1f - this.green.g) * 10f));
			gameObject.transform.position = base.transform.position + toTransformProjected * 0.25f;
			gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
			gameObject.transform.localScale = new Vector3(0.004f, 0.004f, 0.004f);
			gameObject.gameObject.GetComponent<Renderer>().material.color = this.green;
			if (this.green.g > 0.1f)
			{
				this.green.g = this.green.g - 0.1f;
			}
			else
			{
				this.green.g = 1f;
			}
			this.dbgObjectIndex = (this.dbgObjectIndex + 1) % this.dbgObjectCount;
		}

		private void UpdateLinearMapping()
		{
			if (this.limited)
			{
				this.linearMapping.value = (this.outAngle - this.minAngle) / (this.maxAngle - this.minAngle);
			}
			else
			{
				float num = this.outAngle / 360f;
				this.linearMapping.value = num - Mathf.Floor(num);
			}
			this.UpdateDebugText();
		}

		private void UpdateGameObject()
		{
			if (this.rotateGameObject)
			{
				base.transform.localRotation = this.start * Quaternion.AngleAxis(this.outAngle, this.localPlaneNormal);
			}
		}

		private void UpdateDebugText()
		{
			if (this.debugText)
			{
				this.debugText.text = string.Format("Linear: {0}\nAngle:  {1}\n", this.linearMapping.value, this.outAngle);
			}
		}

		private void UpdateAll()
		{
			this.UpdateLinearMapping();
			this.UpdateGameObject();
			this.UpdateDebugText();
		}

		private void ComputeAngle(Hand hand)
		{
			Vector3 vector = this.ComputeToTransformProjected(hand.hoverSphereTransform);
			if (!vector.Equals(this.lastHandProjected))
			{
				float num = Vector3.Angle(this.lastHandProjected, vector);
				if (num > 0f)
				{
					if (this.frozen)
					{
						float sqrMagnitude = (hand.hoverSphereTransform.position - this.frozenHandWorldPos).sqrMagnitude;
						if (sqrMagnitude > this.frozenSqDistanceMinMaxThreshold.x)
						{
							this.outAngle = this.frozenAngle + UnityEngine.Random.Range(-1f, 1f);
							float num2 = Util.RemapNumberClamped(sqrMagnitude, this.frozenSqDistanceMinMaxThreshold.x, this.frozenSqDistanceMinMaxThreshold.y, 0f, 1f);
							if (num2 > 0f)
							{
								base.StartCoroutine(this.HapticPulses(hand.controller, num2, 10));
							}
							else
							{
								base.StartCoroutine(this.HapticPulses(hand.controller, 0.5f, 10));
							}
							if (sqrMagnitude >= this.frozenSqDistanceMinMaxThreshold.y)
							{
								this.onFrozenDistanceThreshold.Invoke();
							}
						}
					}
					else
					{
						Vector3 normalized = Vector3.Cross(this.lastHandProjected, vector).normalized;
						float num3 = Vector3.Dot(this.worldPlaneNormal, normalized);
						float num4 = num;
						if (num3 < 0f)
						{
							num4 = -num4;
						}
						if (this.limited)
						{
							float num5 = Mathf.Clamp(this.outAngle + num4, this.minAngle, this.maxAngle);
							if (this.outAngle == this.minAngle)
							{
								if (num5 > this.minAngle && num < this.minMaxAngularThreshold)
								{
									this.outAngle = num5;
									this.lastHandProjected = vector;
								}
							}
							else if (this.outAngle == this.maxAngle)
							{
								if (num5 < this.maxAngle && num < this.minMaxAngularThreshold)
								{
									this.outAngle = num5;
									this.lastHandProjected = vector;
								}
							}
							else if (num5 == this.minAngle)
							{
								this.outAngle = num5;
								this.lastHandProjected = vector;
								this.onMinAngle.Invoke();
								if (this.freezeOnMin)
								{
									this.Freeze(hand);
								}
							}
							else if (num5 == this.maxAngle)
							{
								this.outAngle = num5;
								this.lastHandProjected = vector;
								this.onMaxAngle.Invoke();
								if (this.freezeOnMax)
								{
									this.Freeze(hand);
								}
							}
							else
							{
								this.outAngle = num5;
								this.lastHandProjected = vector;
							}
						}
						else
						{
							this.outAngle += num4;
							this.lastHandProjected = vector;
						}
					}
				}
			}
		}

		[Tooltip("The axis around which the circular drive will rotate in local space")]
		public CircularDrive.Axis_t axisOfRotation;

		[Tooltip("Child GameObject which has the Collider component to initiate interaction, only needs to be set if there is more than one Collider child")]
		public Collider childCollider;

		[Tooltip("A LinearMapping component to drive, if not specified one will be dynamically added to this GameObject")]
		public LinearMapping linearMapping;

		[Tooltip("If true, the drive will stay manipulating as long as the button is held down, if false, it will stop if the controller moves out of the collider")]
		public bool hoverLock;

		[Header("Limited Rotation")]
		[Tooltip("If true, the rotation will be limited to [minAngle, maxAngle], if false, the rotation is unlimited")]
		public bool limited;

		public Vector2 frozenDistanceMinMaxThreshold = new Vector2(0.1f, 0.2f);

		public UnityEvent onFrozenDistanceThreshold;

		[Header("Limited Rotation Min")]
		[Tooltip("If limited is true, the specifies the lower limit, otherwise value is unused")]
		public float minAngle = -45f;

		[Tooltip("If limited, set whether drive will freeze its angle when the min angle is reached")]
		public bool freezeOnMin;

		[Tooltip("If limited, event invoked when minAngle is reached")]
		public UnityEvent onMinAngle;

		[Header("Limited Rotation Max")]
		[Tooltip("If limited is true, the specifies the upper limit, otherwise value is unused")]
		public float maxAngle = 45f;

		[Tooltip("If limited, set whether drive will freeze its angle when the max angle is reached")]
		public bool freezeOnMax;

		[Tooltip("If limited, event invoked when maxAngle is reached")]
		public UnityEvent onMaxAngle;

		[Tooltip("If limited is true, this forces the starting angle to be startAngle, clamped to [minAngle, maxAngle]")]
		public bool forceStart;

		[Tooltip("If limited is true and forceStart is true, the starting angle will be this, clamped to [minAngle, maxAngle]")]
		public float startAngle;

		[Tooltip("If true, the transform of the GameObject this component is on will be rotated accordingly")]
		public bool rotateGameObject = true;

		[Tooltip("If true, the path of the Hand (red) and the projected value (green) will be drawn")]
		public bool debugPath;

		[Tooltip("If debugPath is true, this is the maximum number of GameObjects to create to draw the path")]
		public int dbgPathLimit = 50;

		[Tooltip("If not null, the TextMesh will display the linear value and the angular value of this circular drive")]
		public TextMesh debugText;

		[Tooltip("The output angle value of the drive in degrees, unlimited will increase or decrease without bound, take the 360 modulus to find number of rotations")]
		public float outAngle;

		private Quaternion start;

		private Vector3 worldPlaneNormal = new Vector3(1f, 0f, 0f);

		private Vector3 localPlaneNormal = new Vector3(1f, 0f, 0f);

		private Vector3 lastHandProjected;

		private Color red = new Color(1f, 0f, 0f);

		private Color green = new Color(0f, 1f, 0f);

		private GameObject[] dbgHandObjects;

		private GameObject[] dbgProjObjects;

		private GameObject dbgObjectsParent;

		private int dbgObjectCount;

		private int dbgObjectIndex;

		private bool driving;

		private float minMaxAngularThreshold = 1f;

		private bool frozen;

		private float frozenAngle;

		private Vector3 frozenHandWorldPos = new Vector3(0f, 0f, 0f);

		private Vector2 frozenSqDistanceMinMaxThreshold = new Vector2(0f, 0f);

		private Hand handHoverLocked;

		public enum Axis_t
		{
			XAxis,
			YAxis,
			ZAxis
		}
	}
}
