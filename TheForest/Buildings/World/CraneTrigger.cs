using System;
using System.Collections;
using Bolt;
using TheForest.Audio;
using TheForest.Items.Inventory;
using TheForest.Utils;
using TheForest.World;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	[DoNotSerializePublic]
	public class CraneTrigger : EntityBehaviour<ICraneState>
	{
		
		private void Awake()
		{
			if (!this._mainTrigger)
			{
				this._mainTrigger = this;
			}
			base.enabled = false;
			this._state = CraneTrigger.States.Idle;
			this._climbHash = Animator.StringToHash("climbing");
			this._climbIdleHash = Animator.StringToHash("climbIdle");
			base.StartCoroutine(this.DelayedAwake(LevelSerializer.IsDeserializing));
		}

		
		private IEnumerator DelayedAwake(bool deserializing)
		{
			yield return null;
			if (!deserializing && !BoltNetwork.isClient && this._mainTrigger == this)
			{
				float num = this._platformTr.position.y - this._maxHeight;
				Vector3 vector = new Vector3(0f, -this._maxHeight, 0f);
				Vector3 b = new Vector3(0f, -6.25f + this._platformTr.localPosition.y, 0f);
				for (int i = 0; i < this._ropes.Length; i++)
				{
					RaycastHit raycastHit;
					if (Physics.SphereCast(this._ropes[i].position + b, 0.2f, vector.normalized, out raycastHit, Mathf.Abs(vector.y) * 1.2f, Scene.ValidateFloorLayers(this._ropes[i].position, this._downwardsCollisionMask.value)) && raycastHit.point.y > num)
					{
						num = Mathf.Min(raycastHit.point.y, this._platformTr.position.y);
					}
				}
				this._mainTrigger._platformTr.position = new Vector3(this._platformTr.position.x, num + 7f, this._platformTr.position.z);
				this._mainTrigger.UpdateHeightMp();
			}
			this.ScaleRopes();
			yield break;
		}

		
		private void Update()
		{
			if (LocalPlayer.FpCharacter.PushingSled)
			{
				return;
			}
			this.RefreshIcons();
			if (TheForest.Utils.Input.GetButtonDown("Take"))
			{
				if (this._mainTrigger._state == CraneTrigger.States.Locked)
				{
					this.UnlockPlayer();
				}
				else if (this.CanLock)
				{
					this.LockPlayer();
				}
			}
			else if (this._mainTrigger._state == CraneTrigger.States.Locked)
			{
				if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.World && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.PlayerList)
				{
					this.UnlockPlayer();
				}
				else
				{
					float axis = TheForest.Utils.Input.GetAxis("Vertical");
					if (!Mathf.Approximately(axis, 0f))
					{
						LocalPlayer.Sfx.SetCraneLoop(true, base.gameObject);
						if (!BoltNetwork.isClient)
						{
							this.TryMove(Mathf.RoundToInt(axis));
						}
						else
						{
							this.TryMoveMp(axis, false);
						}
					}
					else
					{
						LocalPlayer.Sfx.SetCraneLoop(false, null);
					}
				}
			}
		}

		
		private void TryMove(int direction)
		{
			if (direction == 0)
			{
				return;
			}
			Vector3 b = new Vector3(0f, -6.25f + this._platformTr.localPosition.y, 0f);
			Vector3 vector = new Vector3(0f, ((direction >= 0) ? this._moveSpeed : (-this._moveSpeed)) * Time.deltaTime, 0f);
			this._outMovement = vector;
			int layers = (direction >= 0) ? this._upwardsCollisionMask : this._downwardsCollisionMask;
			for (int i = 0; i < this._mainTrigger._ropes.Length; i++)
			{
				Transform transform = this._mainTrigger._ropes[i];
				RaycastHit raycastHit;
				if (Physics.SphereCast(transform.position + b, 0.1f, vector.normalized, out raycastHit, Mathf.Abs(vector.y) * 1.2f, Scene.ValidateFloorLayers(transform.position, layers)))
				{
					Sfx.Play(SfxInfo.SfxTypes.HitWood, base.transform, true);
					if (this._mainTrigger._state == CraneTrigger.States.Locked)
					{
						this.UnlockPlayer();
					}
					else
					{
						this.UnsetGrabbedBy();
					}
					return;
				}
			}
			if (direction < 0)
			{
				for (int j = 0; j < Scene.SceneTracker.allPlayers.Count; j++)
				{
					GameObject gameObject = Scene.SceneTracker.allPlayers[j];
					if (gameObject)
					{
						Vector3 vector2 = this._floor.transform.InverseTransformPoint(gameObject.transform.position);
						if (Mathf.Abs(vector2.x) < 5.75f && Mathf.Abs(vector2.z) < 5.75f && vector2.y < -5f && vector2.y > -11f)
						{
							if (this._mainTrigger._state == CraneTrigger.States.Locked)
							{
								this.UnlockPlayer();
							}
							else
							{
								this.UnsetGrabbedBy();
							}
							return;
						}
					}
				}
			}
			this._platformTr.localPosition += vector;
			if (this._platformTr.localPosition.y < -this._maxHeight)
			{
				this._platformTr.localPosition = new Vector3(this._platformTr.localPosition.x, -this._maxHeight, this._platformTr.localPosition.z);
				if (this._mainTrigger._state == CraneTrigger.States.Locked)
				{
					this.UnlockPlayer();
				}
				else
				{
					this.UnsetGrabbedBy();
				}
				Sfx.Play(SfxInfo.SfxTypes.HitWood, base.transform, true);
			}
			else if (this._platformTr.localPosition.y > 0f)
			{
				this._platformTr.localPosition = new Vector3(this._platformTr.localPosition.x, 0f, this._platformTr.localPosition.z);
				if (this._mainTrigger._state == CraneTrigger.States.Locked)
				{
					this.UnlockPlayer();
				}
				else
				{
					this.UnsetGrabbedBy();
				}
				Sfx.Play(SfxInfo.SfxTypes.HitWood, base.transform, true);
			}
			this.UpdateHeightMp();
			this.ScaleRopes();
		}

		
		private void GrabEnter()
		{
			this._grabbed = true;
			base.enabled = true;
			this.RefreshIcons();
		}

		
		private void GrabExit()
		{
			this._grabbed = false;
			if (base.enabled && this._mainTrigger._state == CraneTrigger.States.Idle)
			{
				base.enabled = false;
			}
			this.RefreshIcons();
		}

		
		private void RefreshIcons()
		{
			bool canLock = this.CanLock;
			if (this._iconSheen.activeSelf != !canLock)
			{
				this._iconSheen.SetActive(!canLock);
			}
			if (this._iconPickUp.activeSelf != canLock)
			{
				this._iconPickUp.SetActive(canLock);
			}
		}

		
		private void LockPlayer()
		{
			Grabber.Filter = base.gameObject;
			this._mainTrigger._state = CraneTrigger.States.Locked;
			LocalPlayer.SpecialActions.SendMessage("EnterCraneClimb", base.transform);
			this.TryMoveMp(0f, false);
		}

		
		private void UnlockPlayer()
		{
			Grabber.Filter = null;
			this._mainTrigger._state = CraneTrigger.States.Idle;
			if (!this._grabbed)
			{
				this.GrabExit();
			}
			LocalPlayer.SpecialActions.SendMessage("ExitCraneClimb");
			LocalPlayer.Sfx.SetCraneLoop(false, null);
			this.TryMoveMp(0f, true);
		}

		
		private void ScaleRopes()
		{
			if (this._mainTrigger == this)
			{
				Vector3 localScale = new Vector3(1f, Mathf.Abs(this._platformTr.localPosition.y) + 6f, 1f);
				for (int i = 0; i < this._ropes.Length; i++)
				{
					this._ropes[i].localScale = localScale;
				}
			}
			else
			{
				this._mainTrigger.ScaleRopes();
				Vector3 localScale2 = new Vector3(1f, base.transform.parent.position.y - base.transform.position.y, 1f);
				for (int j = 0; j < this._ropes.Length; j++)
				{
					this._ropes[j].localScale = localScale2;
				}
			}
		}

		
		
		private bool CanLock
		{
			get
			{
				return base.enabled && this._grabbed && this._floor.enabled == this._driveOnPlatform && (!BoltNetwork.isRunning || !base.state.GrabbedBy) && !LocalPlayer.AnimControl.swimming && !LocalPlayer.FpCharacter.jumping && !LocalPlayer.FpCharacter.crouching;
			}
		}

		
		public override void Attached()
		{
			if (!base.entity.isOwner)
			{
				base.state.AddCallback("Height", new PropertyCallbackSimple(this.OnHeightUpdated));
			}
			else
			{
				this.UpdateHeightMp();
			}
			base.state.AddCallback("GrabbedBy", new PropertyCallbackSimple(this.OnGrabbedByUpdated));
		}

		
		private void TryMoveMp(float direction, bool unlocking = false)
		{
			if (BoltNetwork.isRunning)
			{
				CraneCommand craneCommand = CraneCommand.Create(GlobalTargets.OnlyServer);
				craneCommand.Direction = Mathf.RoundToInt(direction);
				craneCommand.Sender = ((!unlocking) ? LocalPlayer.Entity : null);
				craneCommand.Crane = base.entity;
				craneCommand.Send();
			}
		}

		
		private void UnsetGrabbedBy()
		{
			if (BoltNetwork.isRunning)
			{
				base.state.GrabbedBy = null;
			}
		}

		
		private void UpdateHeightMp()
		{
			if (BoltNetwork.isServer)
			{
				base.state.Height = this._platformTr.localPosition.y;
			}
		}

		
		private void OnGrabbedByUpdated()
		{
			if (this._mainTrigger._state == CraneTrigger.States.Locked && base.state.GrabbedBy == null)
			{
				this.UnlockPlayer();
			}
		}

		
		private void OnHeightUpdated()
		{
			this._platformTr.localPosition = new Vector3(0f, base.state.Height, 0f);
			this.ScaleRopes();
		}

		
		public void AddRemoteInput(int direction)
		{
			this._remoteInputEndTime = Time.realtimeSinceStartup + 0.15f;
			this._remoteInputDirection = direction;
			if (this._remoteInputCoroutine == null)
			{
				this._remoteInputCoroutine = base.StartCoroutine(this.RemoteInputCoroutine());
			}
		}

		
		private IEnumerator RemoteInputCoroutine()
		{
			while (this._remoteInputEndTime > Time.realtimeSinceStartup && base.state.GrabbedBy)
			{
				this.TryMove(this._remoteInputDirection);
				yield return null;
			}
			this._remoteInputCoroutine = null;
			yield break;
		}

		
		private IEnumerator refreshPlatformTriggerRoutine()
		{
			Collider platformCollider = this._platformTr.GetComponent<Collider>();
			if (platformCollider)
			{
				platformCollider.enabled = false;
			}
			yield return YieldPresets.WaitForFixedUpdate;
			if (platformCollider)
			{
				platformCollider.enabled = true;
			}
			yield break;
		}

		
		public float _onRopeOffset;

		
		public float _maxHeight = 50f;

		
		public float _moveSpeed = 1f;

		
		public Transform _platformTr;

		
		public Transform[] _ropes;

		
		public GameObject _iconSheen;

		
		public GameObject _iconPickUp;

		
		public LayerMask _upwardsCollisionMask;

		
		public LayerMask _downwardsCollisionMask;

		
		public DynamicFloor _floor;

		
		public CraneTrigger _mainTrigger;

		
		public Collider _buildBlocker;

		
		public bool _driveOnPlatform = true;

		
		public Vector3 _outMovement;

		
		private bool _grabbed;

		
		private CraneTrigger.States _state;

		
		private int _climbHash;

		
		private int _climbIdleHash;

		
		private float _remoteInputEndTime;

		
		private int _remoteInputDirection;

		
		private Coroutine _remoteInputCoroutine;

		
		private enum States
		{
			
			Idle,
			
			Locked
		}
	}
}
