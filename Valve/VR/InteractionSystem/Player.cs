using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	
	public class Player : MonoBehaviour
	{
		
		
		public static Player instance
		{
			get
			{
				if (Player._instance == null)
				{
					Player._instance = UnityEngine.Object.FindObjectOfType<Player>();
				}
				return Player._instance;
			}
		}

		
		
		public int handCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < this.hands.Length; i++)
				{
					if (this.hands[i].gameObject.activeInHierarchy)
					{
						num++;
					}
				}
				return num;
			}
		}

		
		public Hand GetHand(int i)
		{
			for (int j = 0; j < this.hands.Length; j++)
			{
				if (this.hands[j].gameObject.activeInHierarchy)
				{
					if (i <= 0)
					{
						return this.hands[j];
					}
					i--;
				}
			}
			return null;
		}

		
		
		public Hand leftHand
		{
			get
			{
				for (int i = 0; i < this.hands.Length; i++)
				{
					if (this.hands[i].gameObject.activeInHierarchy)
					{
						if (this.hands[i].GuessCurrentHandType() == Hand.HandType.Left)
						{
							return this.hands[i];
						}
					}
				}
				return null;
			}
		}

		
		
		public Hand rightHand
		{
			get
			{
				for (int i = 0; i < this.hands.Length; i++)
				{
					if (this.hands[i].gameObject.activeInHierarchy)
					{
						if (this.hands[i].GuessCurrentHandType() == Hand.HandType.Right)
						{
							return this.hands[i];
						}
					}
				}
				return null;
			}
		}

		
		
		public SteamVR_Controller.Device leftController
		{
			get
			{
				Hand leftHand = this.leftHand;
				if (leftHand)
				{
					return leftHand.controller;
				}
				return null;
			}
		}

		
		
		public SteamVR_Controller.Device rightController
		{
			get
			{
				Hand rightHand = this.rightHand;
				if (rightHand)
				{
					return rightHand.controller;
				}
				return null;
			}
		}

		
		
		public Transform hmdTransform
		{
			get
			{
				for (int i = 0; i < this.hmdTransforms.Length; i++)
				{
					if (this.hmdTransforms[i].gameObject.activeInHierarchy)
					{
						return this.hmdTransforms[i];
					}
				}
				return null;
			}
		}

		
		
		public float eyeHeight
		{
			get
			{
				Transform hmdTransform = this.hmdTransform;
				if (hmdTransform)
				{
					return Vector3.Project(hmdTransform.position - this.trackingOriginTransform.position, this.trackingOriginTransform.up).magnitude / this.trackingOriginTransform.lossyScale.x;
				}
				return 0f;
			}
		}

		
		
		public Vector3 feetPositionGuess
		{
			get
			{
				Transform hmdTransform = this.hmdTransform;
				if (hmdTransform)
				{
					return this.trackingOriginTransform.position + Vector3.ProjectOnPlane(hmdTransform.position - this.trackingOriginTransform.position, this.trackingOriginTransform.up);
				}
				return this.trackingOriginTransform.position;
			}
		}

		
		
		public Vector3 bodyDirectionGuess
		{
			get
			{
				Transform hmdTransform = this.hmdTransform;
				if (hmdTransform)
				{
					Vector3 vector = Vector3.ProjectOnPlane(hmdTransform.forward, this.trackingOriginTransform.up);
					if (Vector3.Dot(hmdTransform.up, this.trackingOriginTransform.up) < 0f)
					{
						vector = -vector;
					}
					return vector;
				}
				return this.trackingOriginTransform.forward;
			}
		}

		
		private void Awake()
		{
			if (this.trackingOriginTransform == null)
			{
				this.trackingOriginTransform = base.transform;
			}
		}

		
		private void OnEnable()
		{
			Player._instance = this;
			if (SteamVR.instance != null)
			{
				this.ActivateRig(this.rigSteamVR);
			}
			else
			{
				this.ActivateRig(this.rig2DFallback);
			}
		}

		
		private void OnDrawGizmos()
		{
			if (this != Player.instance)
			{
				return;
			}
			Gizmos.color = Color.white;
			Gizmos.DrawIcon(this.feetPositionGuess, "vr_interaction_system_feet.png");
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(this.feetPositionGuess, this.feetPositionGuess + this.trackingOriginTransform.up * this.eyeHeight);
			Gizmos.color = Color.blue;
			Vector3 bodyDirectionGuess = this.bodyDirectionGuess;
			Vector3 b = Vector3.Cross(this.trackingOriginTransform.up, bodyDirectionGuess);
			Vector3 vector = this.feetPositionGuess + this.trackingOriginTransform.up * this.eyeHeight * 0.75f;
			Vector3 vector2 = vector + bodyDirectionGuess * 0.33f;
			Gizmos.DrawLine(vector, vector2);
			Gizmos.DrawLine(vector2, vector2 - 0.033f * (bodyDirectionGuess + b));
			Gizmos.DrawLine(vector2, vector2 - 0.033f * (bodyDirectionGuess - b));
			Gizmos.color = Color.red;
			int handCount = this.handCount;
			for (int i = 0; i < handCount; i++)
			{
				Hand hand = this.GetHand(i);
				if (hand.startingHandType == Hand.HandType.Left)
				{
					Gizmos.DrawIcon(hand.transform.position, "vr_interaction_system_left_hand.png");
				}
				else if (hand.startingHandType == Hand.HandType.Right)
				{
					Gizmos.DrawIcon(hand.transform.position, "vr_interaction_system_right_hand.png");
				}
				else
				{
					Hand.HandType handType = hand.GuessCurrentHandType();
					if (handType == Hand.HandType.Left)
					{
						Gizmos.DrawIcon(hand.transform.position, "vr_interaction_system_left_hand_question.png");
					}
					else if (handType == Hand.HandType.Right)
					{
						Gizmos.DrawIcon(hand.transform.position, "vr_interaction_system_right_hand_question.png");
					}
					else
					{
						Gizmos.DrawIcon(hand.transform.position, "vr_interaction_system_unknown_hand.png");
					}
				}
			}
		}

		
		public void Draw2DDebug()
		{
			if (!this.allowToggleTo2D)
			{
				return;
			}
			if (!SteamVR.active)
			{
				return;
			}
			int num = 100;
			int num2 = 25;
			int num3 = Screen.width / 2 - num / 2;
			int num4 = Screen.height - num2 - 10;
			string text = (!this.rigSteamVR.activeSelf) ? "VR" : "2D Debug";
			if (GUI.Button(new Rect((float)num3, (float)num4, (float)num, (float)num2), text))
			{
				if (this.rigSteamVR.activeSelf)
				{
					this.ActivateRig(this.rig2DFallback);
				}
				else
				{
					this.ActivateRig(this.rigSteamVR);
				}
			}
		}

		
		private void ActivateRig(GameObject rig)
		{
			if (rig == null)
			{
				return;
			}
			this.rigSteamVR.SetActive(rig == this.rigSteamVR);
			this.rig2DFallback.SetActive(rig == this.rig2DFallback);
			if (this.audioListener)
			{
				this.audioListener.transform.parent = this.hmdTransform;
				this.audioListener.transform.localPosition = Vector3.zero;
				this.audioListener.transform.localRotation = Quaternion.identity;
			}
		}

		
		public void PlayerShotSelf()
		{
		}

		
		[Tooltip("Virtual transform corresponding to the meatspace tracking origin. Devices are tracked relative to this.")]
		public Transform trackingOriginTransform;

		
		[Tooltip("List of possible transforms for the head/HMD, including the no-SteamVR fallback camera.")]
		public Transform[] hmdTransforms;

		
		[Tooltip("List of possible Hands, including no-SteamVR fallback Hands.")]
		public Hand[] hands;

		
		[Tooltip("Reference to the physics collider that follows the player's HMD position.")]
		public Collider headCollider;

		
		[Tooltip("These objects are enabled when SteamVR is available")]
		public GameObject rigSteamVR;

		
		[Tooltip("These objects are enabled when SteamVR is not available, or when the user toggles out of VR")]
		public GameObject rig2DFallback;

		
		[Tooltip("The audio listener for this player")]
		public Transform audioListener;

		
		public bool allowToggleTo2D = true;

		
		private static Player _instance;
	}
}
