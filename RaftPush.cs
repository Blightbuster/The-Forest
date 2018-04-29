using System;
using System.Collections;
using Bolt;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using TheForest.World;
using UnityEngine;


[DoNotSerializePublic]
public class RaftPush : EntityBehaviour<IRaftState>
{
	
	private void Awake()
	{
		base.enabled = false;
		this._state = RaftPush.States.Idle;
		this._direction = Vector3.zero;
		this._canLockIcon.gameObject.SetActive(false);
		if (!this._rb)
		{
			this._rb = base.transform.parent.GetComponent<Rigidbody>();
		}
	}

	
	private void Start()
	{
		if (!this._buoyancy)
		{
			this._buoyancy = base.GetComponentInParent<Buoyancy>();
			this._rb = base.GetComponentInParent<Rigidbody>();
		}
		this._raftOnLand = base.GetComponentInParent<raftOnLand>();
		this._floor = base.transform.GetComponentInParent<DynamicFloor>();
	}

	
	private void Update()
	{
		if ((this._state == RaftPush.States.DriverStanding || this._state == RaftPush.States.Idle) && !this._doingOutOfWorld)
		{
			this.allowDirection = false;
		}
		if (this._state == RaftPush.States.DriverStanding)
		{
			LocalPlayer.AnimControl.standingOnRaft = true;
		}
		else
		{
			LocalPlayer.AnimControl.standingOnRaft = false;
		}
		bool flag = this._isGrabbed && this._state == RaftPush.States.DriverStanding;
		if (flag && BoltNetwork.isRunning && base.state.GrabbedBy[this._oarId] != null)
		{
			flag = false;
		}
		if (!this._canLockIcon.gameObject.activeSelf.Equals(flag))
		{
			this._canLockIcon.gameObject.SetActive(flag);
		}
		if (this._shouldUnlock)
		{
			this._shouldUnlock = false;
			if (BoltNetwork.isRunning)
			{
				RaftGrab raftGrab = RaftGrab.Create(GlobalTargets.OnlyServer);
				raftGrab.OarId = this._oarId;
				raftGrab.Raft = base.GetComponentInParent<BoltEntity>();
				raftGrab.Player = null;
				raftGrab.Send();
			}
			else
			{
				this.offRaft();
			}
			return;
		}
		this._shouldUnlock = false;
		if (this.stickToRaft)
		{
			LocalPlayer.FpCharacter.enabled = false;
			LocalPlayer.AnimControl.controller.useGravity = false;
			LocalPlayer.AnimControl.controller.isKinematic = true;
			Vector3 position = this._driverPos.position;
			position.y += LocalPlayer.AnimControl.playerCollider.height / 2f - LocalPlayer.AnimControl.playerCollider.center.y;
			LocalPlayer.Transform.position = position;
			LocalPlayer.Transform.rotation = this._driverPos.rotation;
			LocalPlayer.Animator.SetLayerWeightReflected(2, 1f);
		}
		if (TheForest.Utils.Input.GetButtonDown("Take") && !this._doingOutOfWorld)
		{
			if (flag)
			{
				if (BoltNetwork.isRunning)
				{
					RaftGrab raftGrab2 = RaftGrab.Create(GlobalTargets.OnlyServer);
					raftGrab2.OarId = this._oarId;
					raftGrab2.Raft = base.GetComponentInParent<BoltEntity>();
					raftGrab2.Player = LocalPlayer.Entity;
					raftGrab2.Send();
				}
				else
				{
					this.onRaft();
				}
			}
			else if (this._state == RaftPush.States.DriverLocked)
			{
				if (BoltNetwork.isRunning)
				{
					RaftGrab raftGrab3 = RaftGrab.Create(GlobalTargets.OnlyServer);
					raftGrab3.OarId = this._oarId;
					raftGrab3.Raft = base.GetComponentInParent<BoltEntity>();
					raftGrab3.Player = null;
					raftGrab3.Send();
				}
				else
				{
					this.offRaft();
				}
			}
		}
		else if (this._state == RaftPush.States.DriverLocked && !this._doingOutOfWorld)
		{
			RaftPush.MoveDirection moveDirection = RaftPush.MoveDirection.None;
			bool flag2 = false;
			if (Vector3.Angle(this._raft.transform.up, Vector3.up) > 45f)
			{
				flag2 = true;
			}
			float num = TheForest.Utils.Input.GetAxis("Horizontal");
			if (flag2)
			{
				num = 0f;
			}
			this.axisDirection = num;
			this.moveDirection = moveDirection;
			if ((TheForest.Utils.Input.GetButton("Fire1") || TheForest.Utils.Input.GetButton("AltFire")) && !flag2)
			{
				if (this.CheckDistanceFromOceanCollision() || LocalPlayer.AnimControl.doneOutOfWorldRoutine || !LocalPlayer.Inventory.Owns(LocalPlayer.AnimControl._timmyPhotoId, true))
				{
					moveDirection = ((!TheForest.Utils.Input.GetButton("Fire1")) ? RaftPush.MoveDirection.Backward : RaftPush.MoveDirection.Forward);
					this.allowDirection = true;
					this.moveDirection = moveDirection;
					this._driver.enablePaddleOnRaft(true);
				}
				else
				{
					this.allowDirection = false;
					this._driver.enablePaddleOnRaft(false);
					if (!this._doingOutOfWorld)
					{
						base.StartCoroutine(this.outOfWorldRoutine());
					}
				}
			}
			else
			{
				this.allowDirection = false;
				this._driver.enablePaddleOnRaft(false);
			}
		}
		else if (this._state == RaftPush.States.Auto)
		{
			this._direction = Scene.OceanCeto.transform.position - base.transform.position;
			this._direction.Normalize();
			this._raft.GetComponent<Rigidbody>().AddForce(this._direction * this._speed * 5f, ForceMode.Impulse);
			if (this.CheckDistanceFromOceanCollision())
			{
				this._state = RaftPush.States.DriverLocked;
			}
		}
	}

	
	private void FixedUpdate()
	{
		if (PlayerPreferences.LowQualityPhysics)
		{
			this.flip = !this.flip;
			if (this.flip)
			{
				return;
			}
		}
		if (!BoltNetwork.isRunning)
		{
			if (this.allowDirection)
			{
				this.PushRaft(this.moveDirection);
			}
			if (this._state == RaftPush.States.DriverLocked && !BoltNetwork.isRunning && !this._doingOutOfWorld)
			{
				this.TurnRaft(this.axisDirection);
			}
		}
		else if (this._state == RaftPush.States.DriverLocked && !this._doingOutOfWorld)
		{
			this.UpdateDirectionMp(this.axisDirection, this.moveDirection);
		}
	}

	
	private void UpdateDirectionMp(float axisD, RaftPush.MoveDirection dir)
	{
		if (BoltNetwork.isRunning && (!Mathf.Approximately(axisD, 0f) || this.allowDirection))
		{
			RaftControl raftControl = RaftControl.Create(GlobalTargets.OnlyServer);
			raftControl.OarId = this._oarId;
			raftControl.Rotation = axisD;
			raftControl.Movement = (int)dir;
			raftControl.Raft = base.GetComponentInParent<BoltEntity>();
			raftControl.Send();
		}
	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player") && this._state == RaftPush.States.Idle)
		{
			base.enabled = true;
			this._driver = LocalPlayer.FpCharacter;
			this._state = RaftPush.States.DriverStanding;
			this._canLockIcon.gameObject.SetActive(true);
		}
	}

	
	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Player") && (this._state == RaftPush.States.DriverStanding || this._state == RaftPush.States.Idle))
		{
			if (this._driver == LocalPlayer.FpCharacter)
			{
			}
			base.enabled = false;
			this._state = RaftPush.States.Idle;
			this._canLockIcon.gameObject.SetActive(false);
		}
	}

	
	private void OnDestroy()
	{
		if (this._driver == LocalPlayer.FpCharacter)
		{
			this.offRaft();
		}
	}

	
	private void GrabEnter()
	{
		this._isGrabbed = true;
	}

	
	private void GrabExit()
	{
		this._isGrabbed = false;
	}

	
	private void onRaft()
	{
		if (Vector3.Angle(this._raft.transform.up, Vector3.up) > 90f)
		{
			this._raft.transform.localEulerAngles = new Vector3(0f, this._raft.transform.localEulerAngles.y, 0f);
		}
		PlayerInventory component = this._driver.GetComponent<PlayerInventory>();
		LocalPlayer.AnimControl.oarHeld.SetActive(true);
		LocalPlayer.AnimControl.currRaft = this;
		this._oar.SetActive(false);
		LocalPlayer.Inventory.HideAllEquiped(false, false);
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.AnimControl.controller.useGravity = false;
		LocalPlayer.AnimControl.controller.isKinematic = true;
		LocalPlayer.AnimControl.blockInventoryOpen = true;
		LocalPlayer.AnimControl.controller.Sleep();
		LocalPlayer.AnimControl.playerCollider.enabled = false;
		LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
		LocalPlayer.Transform.parent = this._raft.transform;
		Vector3 position = this._driverPos.position;
		position.y += LocalPlayer.AnimControl.playerCollider.height / 2f - LocalPlayer.AnimControl.playerCollider.center.y;
		LocalPlayer.Transform.position = position;
		LocalPlayer.Transform.rotation = this._driverPos.rotation;
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.CamRotator.rotationRange = new Vector2(100f, 130f);
		this._driver.OnRaft();
		this._canLockIcon.gameObject.SetActive(false);
		this._state = RaftPush.States.DriverLocked;
		this._buoyancy.ForceValidateTriggers = true;
		LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("paddleBool").Value = true;
		this.stickToRaft = true;
	}

	
	private void offRaft()
	{
		this._buoyancy.ForceValidateTriggers = false;
		if (LocalPlayer.AnimControl != null)
		{
			LocalPlayer.AnimControl.oarHeld.SetActive(false);
			LocalPlayer.AnimControl.currRaft = null;
		}
		base.Invoke("turnOffFoam", 3f);
		this._oar.SetActive(true);
		if (this._driver)
		{
			PlayerInventory component = this._driver.GetComponent<PlayerInventory>();
			component.EquipPreviousWeaponDelayed();
		}
		if (this.stickToRaft)
		{
			this.ShutDown();
		}
		base.GetComponent<Collider>().enabled = false;
		base.GetComponent<Collider>().enabled = true;
		if (this._floor)
		{
			this._floor.refreshPlayerOffset();
			this._floor.enabled = true;
		}
		if (BoltNetwork.isRunning && base.entity.isAttached && base.state.GrabbedBy[this._oarId] == LocalPlayer.Entity)
		{
			RaftGrab raftGrab = RaftGrab.Create(GlobalTargets.OnlyServer);
			raftGrab.OarId = this._oarId;
			raftGrab.Raft = base.GetComponentInParent<BoltEntity>();
			raftGrab.Player = null;
			raftGrab.Send();
		}
	}

	
	private void turnOffFoam()
	{
	}

	
	private void turnOnFoam()
	{
	}

	
	private void ShutDown()
	{
		this._driver.OffRaft();
		this._driver = null;
		this._state = RaftPush.States.Idle;
		LocalPlayer.Transform.parent = Scene.SceneTracker.transform;
		LocalPlayer.Transform.parent = null;
		LocalPlayer.AnimControl.controller.useGravity = true;
		LocalPlayer.AnimControl.controller.isKinematic = false;
		LocalPlayer.AnimControl.controller.WakeUp();
		LocalPlayer.FpCharacter.enabled = true;
		LocalPlayer.FpCharacter.Locked = false;
		LocalPlayer.MainRotator.resetOriginalRotation = true;
		LocalPlayer.MainRotator.enabled = true;
		LocalPlayer.AnimControl.playerCollider.enabled = true;
		LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
		LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
		LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("paddleBool").Value = false;
		LocalPlayer.Animator.SetBoolReflected("paddleBool", false);
		LocalPlayer.AnimControl.blockInventoryOpen = false;
		this.stickToRaft = false;
		LocalPlayer.Transform.localEulerAngles = new Vector3(0f, LocalPlayer.Transform.localEulerAngles.y, 0f);
		this._canLockIcon.gameObject.SetActive(false);
		base.Invoke("turnOffFoam", 3f);
		base.enabled = false;
	}

	
	public void PushRaft(RaftPush.MoveDirection dir)
	{
		if (dir == RaftPush.MoveDirection.Forward)
		{
			this._direction = ((!this._relativeForce) ? this._raft.transform.forward : base.transform.forward);
		}
		else
		{
			if (dir != RaftPush.MoveDirection.Backward)
			{
				return;
			}
			this._direction = ((!this._relativeForce) ? this._raft.transform.forward : base.transform.forward) * -1f;
		}
		this._direction.y = 0f;
		float num = 1f;
		if (!this._buoyancy.InWater)
		{
			num = 0f;
		}
		this._direction *= this._speed * 2f * num;
		Vector3 velocity = this._rb.velocity;
		Vector3 target = this._direction - velocity;
		Vector3 vector = Vector3.zero;
		Vector3 zero = Vector3.zero;
		vector = Vector3.SmoothDamp(vector, target, ref zero, this._dampSpeed);
		if (this.UseRelativeForce)
		{
			Vector3 position = base.transform.position;
			if (this._buoyancy.OverrideCenterOfMass != null)
			{
				position.y = this._buoyancy.OverrideCenterOfMass.position.y;
			}
			this._rb.AddForceAtPosition(vector * (0.016666f / Time.fixedDeltaTime), position, this._forceMode);
		}
		else
		{
			this._rb.AddForce(vector * (0.016666f / Time.fixedDeltaTime), this._forceMode);
		}
		if (this._rb.velocity.magnitude > this._maxVelocity)
		{
			Vector3 vector2 = this._rb.velocity.normalized;
			vector2 *= this._maxVelocity;
			this._rb.velocity = vector2;
		}
	}

	
	public void TurnRaft(float axis)
	{
		if (!this._relativeForce || Scene.SceneTracker.allPlayers.Count < 2)
		{
			float target = axis * (this._rotateForce * 2f) * Mathf.Clamp(this._rb.velocity.normalized.magnitude, 0.1f, 1f);
			float num = 0f;
			float num2 = 0f;
			num2 = Mathf.SmoothDamp(num2, target, ref num, this._torqueDamp);
			this._rb.AddTorque(0f, num2 * (0.016666f / Time.fixedDeltaTime), 0f, ForceMode.Force);
		}
	}

	
	private bool CheckDistanceFromOceanCenter()
	{
		float num = Vector3.Distance(base.transform.position, Scene.OceanCeto.transform.position);
		Debug.Log("ocean dist = " + num);
		return num < this._distanceFromOceanCenter;
	}

	
	private bool CheckDistanceFromOceanCollision()
	{
		if (!Scene.SceneTracker)
		{
			return true;
		}
		for (int i = 0; i < Scene.SceneTracker.oceanCollision.Length; i++)
		{
			if (Scene.SceneTracker.oceanCollision[i].transform.InverseTransformPoint(base.transform.position).x > -5f)
			{
				return false;
			}
		}
		return true;
	}

	
	private IEnumerator outOfWorldRoutine()
	{
		this._doingOutOfWorld = true;
		LocalPlayer.AnimControl.endGameCutScene = true;
		LocalPlayer.Create.Grabber.gameObject.SetActive(false);
		LocalPlayer.AnimControl.doneOutOfWorldRoutine = true;
		yield return YieldPresets.WaitTwoSeconds;
		Vector3 oarPos = LocalPlayer.AnimControl.oarHeld.transform.localPosition;
		Quaternion oarRot = LocalPlayer.AnimControl.oarHeld.transform.localRotation;
		Transform oarParent = LocalPlayer.AnimControl.oarHeld.transform.parent;
		LocalPlayer.AnimControl.oarHeld.transform.parent = LocalPlayer.Transform;
		LocalPlayer.Animator.SetBool("lookAtPhoto", true);
		yield return YieldPresets.WaitPointFiveSeconds;
		LocalPlayer.AnimControl.heldTimmyPhotoGo.SetActive(true);
		yield return YieldPresets.WaitFiveSeconds;
		LocalPlayer.Animator.SetBool("lookAtPhoto", false);
		yield return YieldPresets.WaitPointFiveSeconds;
		LocalPlayer.AnimControl.heldTimmyPhotoGo.SetActive(false);
		yield return YieldPresets.WaitPointFiveSeconds;
		LocalPlayer.AnimControl.oarHeld.transform.parent = oarParent;
		LocalPlayer.AnimControl.oarHeld.transform.localPosition = oarPos;
		LocalPlayer.AnimControl.oarHeld.transform.localRotation = oarRot;
		LocalPlayer.Animator.SetBool("paddleIdleBool", true);
		this._driver.enablePaddleOnRaft(true);
		yield return YieldPresets.WaitPointFiveSeconds;
		float timer = Time.time + 2.5f;
		while (Time.time < timer)
		{
			this.allowDirection = true;
			this.moveDirection = RaftPush.MoveDirection.Backward;
			this._driver.enablePaddleOnRaft(true);
			yield return null;
		}
		this.moveDirection = RaftPush.MoveDirection.None;
		this.allowDirection = false;
		this._driver.enablePaddleOnRaft(false);
		this._doingOutOfWorld = false;
		LocalPlayer.AnimControl.endGameCutScene = false;
		LocalPlayer.Create.Grabber.gameObject.SetActive(true);
		yield return null;
		yield break;
	}

	
	
	public bool InWater
	{
		get
		{
			return (!BoltNetwork.isRunning) ? this._buoyancy.InWater : base.state.InWater;
		}
	}

	
	
	private bool UseRelativeForce
	{
		get
		{
			return this._relativeForce;
		}
	}

	
	public override void Attached()
	{
		base.state.Transform.SetTransforms(this._rb.transform);
		base.state.AddCallback("GrabbedBy[]", new PropertyCallbackSimple(this.OnGrabbed));
	}

	
	private void OnGrabbed()
	{
		if (base.state.GrabbedBy[this._oarId] == LocalPlayer.Entity)
		{
			if (this._state >= RaftPush.States.DriverStanding)
			{
				if (this._driver && this._driver == LocalPlayer.FpCharacter && this._state == RaftPush.States.DriverStanding)
				{
					this.onRaft();
				}
			}
			else
			{
				this.offRaft();
			}
		}
		else
		{
			if (this._state == RaftPush.States.DriverLocked)
			{
				this.offRaft();
			}
			this._oar.SetActive(base.state.GrabbedBy[this._oarId] == null);
		}
	}

	
	public int _oarId;

	
	public Rigidbody _rb;

	
	public raftOnLand _raftOnLand;

	
	public Buoyancy _buoyancy;

	
	public GameObject _raft;

	
	public GameObject _oar;

	
	public Transform _driverPos;

	
	public SheenBillboard _canLockIcon;

	
	public float _distanceFromOceanCenter;

	
	public float _speed = 10f;

	
	public float _paddlingDelay = 1.8f;

	
	public float _maxVelocity = 10f;

	
	public float _dampSpeed = 5f;

	
	public float _rotateForce = 5000f;

	
	public float _torqueDamp = 0.15f;

	
	public bool _relativeForce;

	
	public ForceMode _forceMode;

	
	[ItemIdPicker]
	public int _photoId;

	
	public RaftPush.States _state;

	
	private Vector3 _direction;

	
	private FirstPersonCharacter _driver;

	
	private DynamicFloor _floor;

	
	private float _prevMass;

	
	private bool _shouldUnlock;

	
	private bool _isGrabbed;

	
	private bool stickToRaft;

	
	private bool _doingOutOfWorld;

	
	private bool allowDirection;

	
	private bool allowAxis;

	
	private bool flip;

	
	private float axisDirection;

	
	private RaftPush.MoveDirection moveDirection;

	
	public enum States
	{
		
		Idle,
		
		Auto,
		
		DriverStanding,
		
		DriverLocked
	}

	
	public enum MoveDirection
	{
		
		None,
		
		Forward,
		
		Backward
	}
}
