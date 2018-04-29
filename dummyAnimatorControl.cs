using System;
using Bolt;
using TheForest.Utils;
using UnityEngine;


public class dummyAnimatorControl : MonoBehaviour
{
	
	private void Awake()
	{
		this.cmd = base.transform.GetComponent<CoopMutantDummy>();
		this.mrs = base.transform.GetComponent<mutantRagdollSetup>();
		this.dropCollider = base.transform.GetComponent<BoxCollider>();
		this.dropRb = base.transform.GetComponent<Rigidbody>();
		this.animator = base.GetComponent<Animator>();
		this.control = base.GetComponent<dummyAnimatorControl>();
		this.Tr = base.transform;
		this.layer = 26;
		this.layerMask = 1 << this.layer;
	}

	
	private void Start()
	{
		this.refreshRagDollDelay = Time.time + 0.6f;
		this.neckLayerMask = 36839424;
		base.Invoke("disableControl", 5f);
		if (this.setupFeedingEncounter)
		{
			this.setFeedingEncounter();
		}
	}

	
	private void OnEnable()
	{
		this.refreshRagDollDelay = Time.time + 0.6f;
		this.neckRayCastDelay = Time.time + 0.25f;
		this.groundCheckTimer = Time.time + 4f;
		this.testNeck = false;
		this.animator.enabled = true;
		this.control.enabled = true;
		base.Invoke("disableControl", 5f);
		if (this.setupFeedingEncounter)
		{
			this.setFeedingEncounter();
		}
	}

	
	private void OnDisable()
	{
		base.CancelInvoke("disableControl");
		this.testNeck = false;
		this.doPickupDummy();
		this.bodyOnSled = false;
	}

	
	private void doPickupDummy()
	{
		this.testNeck = false;
		if ((this.setupFeedingEncounter || this.setupMourningEncounter) && this.Tr)
		{
			this.Tr.localPosition = Vector3.zero;
		}
		if (!this.setupFeedingEncounter && !this.setupMourningEncounter && !base.transform.root.CompareTag("Multisled"))
		{
			if (base.transform.parent != null)
			{
				base.Invoke("setParentNull", 0.25f);
			}
			if (BoltNetwork.isRunning)
			{
				base.transform.SendMessageUpwards("releaseNooseTrapMP", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				base.transform.SendMessageUpwards("releaseNooseTrap", SendMessageOptions.DontRequireReceiver);
			}
		}
		if (this.trapGo)
		{
			this.trapGo.SendMessage("removeMutant", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
		base.CancelInvoke("disableControl");
		this.calledFromDeath = false;
		if (!BoltNetwork.isClient)
		{
		}
		if (BoltNetwork.isClient)
		{
			this.Tr.position = new Vector3(-98f, -98f, -98f);
			this.timerOffsetPosition = Time.time + 0.6f;
		}
	}

	
	private void OnDestroy()
	{
		if (this.hips)
		{
			this.hips.position = base.transform.position;
			this.hips.rotation = base.transform.rotation;
		}
	}

	
	private void setParentNull()
	{
		if (base.transform.parent != null)
		{
			base.transform.parent = null;
		}
	}

	
	private void OnAnimatorMove()
	{
		if (this.bodyOnSled)
		{
			return;
		}
		if (this.calledFromDeath)
		{
			return;
		}
		this.animator.SetBool("deathfinalBOOL", true);
		if (base.enabled && this.animator.enabled && !this.setupMourningEncounter && !this.animator.GetBool("trapBool"))
		{
			this.pos = new Vector3(this.Tr.position.x, this.Tr.position.y + 3f, this.Tr.position.z);
			if (this.currState0.normalizedTime == 1f)
			{
				return;
			}
			this.allHit = Physics.RaycastAll(this.pos, Vector3.down, 7f, this.groundMask);
			float num = float.PositiveInfinity;
			Collider exists = null;
			for (int i = 0; i < this.allHit.Length; i++)
			{
				if (!this.allHit[i].collider.isTrigger)
				{
					float distance = this.allHit[i].distance;
					if (distance < num)
					{
						num = distance;
						exists = this.allHit[i].collider;
						this.hit = this.allHit[i];
					}
				}
			}
			Vector3 deltaPosition = this.animator.deltaPosition;
			if (!BoltNetwork.isClient && exists)
			{
				this.Tr.Translate(deltaPosition, Space.World);
				this.Tr.position = new Vector3(this.Tr.position.x, Mathf.Lerp(this.Tr.position.y, this.hit.point.y, Time.deltaTime * 5f), this.Tr.position.z);
			}
			else if (!BoltNetwork.isClient)
			{
				deltaPosition = this.animator.deltaPosition;
				deltaPosition.y -= 0.25f;
				this.Tr.Translate(deltaPosition, Space.World);
			}
			if (exists)
			{
				if (!this.testNeck)
				{
					this.setNormal = this.hit.normal;
				}
				this.Tr.rotation = this.animator.rootRotation;
				this.Tr.rotation = Quaternion.Lerp(this.Tr.rotation, Quaternion.LookRotation(Vector3.Cross(this.Tr.right, this.setNormal), this.setNormal), Time.deltaTime * 8f);
			}
			if (!this.testNeck && Time.time > this.neckRayCastDelay)
			{
				Vector3 position = this.neck.position;
				position.y += 3f;
				this.allHit2 = Physics.RaycastAll(position, Vector3.down, 4f, this.groundMask);
				float num2 = float.PositiveInfinity;
				Collider exists2 = null;
				for (int j = 0; j < this.allHit2.Length; j++)
				{
					if (!this.allHit2[j].collider.isTrigger)
					{
						float distance2 = this.allHit2[j].distance;
						if (distance2 < num2)
						{
							num2 = distance2;
							exists2 = this.allHit2[j].collider;
							this.hit2 = this.allHit2[j];
						}
					}
				}
				if (exists2 && this.neck.position.y - this.hit2.point.y < 0.1f)
				{
					Vector3 point = this.hit2.point;
					point.y += 0.1f;
					Vector3 normalized = (point - this.Tr.position).normalized;
					Vector3 rhs = Vector3.Cross(normalized, this.setNormal * -1f);
					this.setNormal = Vector3.Cross(normalized, rhs);
					this.testNeck = true;
				}
			}
		}
	}

	
	public void disableControl()
	{
		this.animator.enabled = false;
		this.calledFromDeath = false;
		for (int i = 0; i < this.ragDollParts.Length; i++)
		{
			this.ragDollParts[i].SetActive(false);
		}
	}

	
	private void disableControlAndSync()
	{
		this.animator.enabled = false;
		this.calledFromDeath = false;
		this.cmd.doSync = false;
		for (int i = 0; i < this.ragDollParts.Length; i++)
		{
			this.ragDollParts[i].SetActive(false);
		}
	}

	
	private void refreshRagDollParts()
	{
		if (base.transform.GetComponentInParent<limitSledBlur>())
		{
			return;
		}
		if (this.animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Scene.animHash.nooseTrapDeathHash)
		{
			return;
		}
		for (int i = 0; i < this.ragDollParts.Length; i++)
		{
			this.ragDollParts[i].SetActive(true);
			this.ragDollParts[i].SendMessage("setDropped", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	private void setTrapGo(GameObject trap)
	{
		this.trapGo = trap;
	}

	
	private void setTrapFall()
	{
		this.animator.enabled = true;
		base.CancelInvoke("disableControl");
		base.Invoke("disableControl", 5f);
		this.animator.SetBoolReflected("deathfinalBOOL", true);
		this.animator.SetBoolReflected("trapBool", false);
		this.animator.SetBoolReflected("dropFromTrap", true);
		this.setParentNull();
		for (int i = 0; i < this.ragDollParts.Length; i++)
		{
			this.ragDollParts[i].SetActive(true);
		}
	}

	
	private void releaseFromSpikeTrap()
	{
		if (BoltNetwork.isServer)
		{
			DroppedBody droppedBody = DroppedBody.Create(GlobalTargets.Others);
			droppedBody.Target = base.transform.GetComponent<BoltEntity>();
			droppedBody.dropFromTrap = true;
			droppedBody.Send();
		}
		if (BoltNetwork.isClient)
		{
			this.ignoreFarAwayCheck = Time.time + 4f;
			this.timerAnimatorOffset = Time.time + 5f;
			base.CancelInvoke("disableControlAndSync");
		}
		this.animator.enabled = true;
		base.CancelInvoke("disableControl");
		base.Invoke("disableControl", 5f);
		this.animator.CrossFade("Base Layer.deathStealth1", 0f, 0, 0.4f);
		for (int i = 0; i < this.ragDollParts.Length; i++)
		{
			this.ragDollParts[i].SetActive(true);
		}
	}

	
	public void dropFromCarry(bool ignoreAnim)
	{
		this.ignoreFarAwayCheck = Time.time + 4f;
		if (this.setupMourningEncounter || this.setupFeedingEncounter)
		{
			this.Tr.position = this.parentGo.transform.position;
		}
		if (this.setupMourningEncounter)
		{
			this.parentGo.SendMessage("doAlignForEncounter", SendMessageOptions.DontRequireReceiver);
		}
		if (BoltNetwork.isRunning)
		{
			base.CancelInvoke("disableControl");
			this.animator.enabled = true;
			this.control.enabled = true;
			base.Invoke("disableControl", 5f);
		}
		limitSledBlur componentInParent = base.transform.GetComponentInParent<limitSledBlur>();
		if (!componentInParent)
		{
			for (int i = 0; i < this.ragDollParts.Length; i++)
			{
				this.ragDollParts[i].SetActive(true);
			}
		}
		if (componentInParent)
		{
			base.Invoke("disableControl", 2.5f);
		}
		if (!ignoreAnim)
		{
			this.hips.localPosition = new Vector3(0f, 0f, 0f);
			this.animator.CrossFade("Base Layer.deathStealth1", 0f, 0, 0.4f);
		}
		if (BoltNetwork.isClient)
		{
			this.dropTimeOffset = Time.time + 1.1f;
		}
	}

	
	private void clientDrop(Quaternion r)
	{
		this.ignoreFarAwayCheck = Time.time + 4f;
		base.CancelInvoke("disableControl");
		base.CancelInvoke("disableControlAndSync");
		this.neckRayCastDelay = Time.time + 0.25f;
		this.testNeck = false;
		this.Tr.rotation = r;
		this.animator.CrossFade("Base Layer.deathStealth1", 0f, 0, 0.28f);
	}

	
	private void Update()
	{
		this.currState0 = this.animator.GetCurrentAnimatorStateInfo(0);
		if (this.bodyOnSled)
		{
			return;
		}
		if (this.burnTrigger)
		{
			this.burnTrigger.position = this.hips.position;
		}
		if (Time.time < this.ignoreFarAwayCheck)
		{
			this.playerMovedFarAway = false;
		}
		if (BoltNetwork.isRunning && base.transform.position.x > 4000f)
		{
			this.animator.enabled = false;
			return;
		}
		if (Time.time > this.refreshRagDollDelay && Time.time > this.ignoreFarAwayCheck)
		{
			float sqrMagnitude = (LocalPlayer.Transform.position - this.Tr.position).sqrMagnitude;
			if (sqrMagnitude > 4225f)
			{
				this.playerMovedFarAway = true;
			}
			if (this.playerMovedFarAway && sqrMagnitude < 4225f && !this.animator.GetBool("trapBool") && !this.animationInTrap())
			{
				base.CancelInvoke("disableControlAndSync");
				this.refreshRagDollParts();
				this.cmd.doSync = false;
				base.Invoke("disableControlAndSync", 6.5f);
				this.playerMovedFarAway = false;
			}
			this.refreshRagDollDelay = Time.time + 0.6f;
		}
		if (BoltNetwork.isClient)
		{
			if (this.timerOffsetPosition > Time.time)
			{
				this.Tr.position = new Vector3(-98f, -98f, -98f);
			}
			this.currPos = this.Tr.position;
			if ((this.currPos - this.lastPos).sqrMagnitude * 1000f > 0.1f)
			{
				this.animator.enabled = true;
				this.timerAnimatorOffset = Time.time + 5f;
			}
			else if (Time.time > this.timerAnimatorOffset)
			{
				this.animator.enabled = false;
			}
		}
		this.lastPos = this.Tr.position;
		if (Time.time < this.dropTimeOffset && LocalPlayer.Transform)
		{
			if ((this.Tr.position - LocalPlayer.Transform.position).sqrMagnitude < 100f && !this.doneDropCheck)
			{
				this.animator.CrossFade("Base Layer.deathStealth1", 0f, 0, 0.3f);
				Vector3 vector = LocalPlayer.Transform.position + LocalPlayer.Transform.forward * 2f;
				this.doneDropCheck = true;
				for (int i = 0; i < this.ragDollParts.Length; i++)
				{
					this.ragDollParts[i].SetActive(true);
				}
			}
		}
		else
		{
			this.doneDropCheck = false;
		}
	}

	
	private void LateUpdate()
	{
		if (this.bodyOnSled)
		{
			return;
		}
		if (BoltNetwork.isClient && this.noosePivot && this.currState0.shortNameHash == Scene.animHash.nooseTrapDeathHash)
		{
			Vector3 position = this.Tr.position;
			position.x = this.noosePivot.position.x;
			position.z = this.noosePivot.position.z;
			this.Tr.position = position;
		}
		if (Time.time > this.groundCheckTimer && !this.animator.GetBool("trapBool") && !this.animationInTrap() && !BoltNetwork.isClient)
		{
			RaycastHit raycastHit;
			if (Physics.Raycast(this.hips.position, Vector3.down, out raycastHit, 2f, this.groundMask))
			{
				this.groundCheckTimer = Time.time + 1.5f;
			}
			else
			{
				this.control.enabled = true;
				this.animator.enabled = true;
				this.dropFromCarry(true);
				base.transform.SendMessage("sendResetRagDoll", SendMessageOptions.DontRequireReceiver);
				base.transform.SendMessage("sendSetupRagDoll", SendMessageOptions.DontRequireReceiver);
				base.transform.SendMessage("setRagDollDrop", SendMessageOptions.DontRequireReceiver);
				base.Invoke("disableControl", 5f);
				this.refreshRagDollParts();
				this.groundCheckTimer = Time.time + 6f;
				this.refreshRagDollDelay = Time.time + 0.6f;
				this.neckRayCastDelay = Time.time + 0.25f;
			}
		}
	}

	
	public void setFeedingEncounter()
	{
		if (!this.randomSeed)
		{
			this.rVal = UnityEngine.Random.value;
			this.randomSeed = true;
		}
		if (this.animator)
		{
			if (this.rVal < 0.33f)
			{
				this.animator.CrossFade("Base Layer.deathStealth1", 0f, 0, 1f);
			}
			else if (this.rVal < 0.66f)
			{
				this.animator.CrossFade("Base Layer.dyingToDead", 0f, 0, 1f);
			}
			else
			{
				this.animator.CrossFade("Base Layer.deathMir1", 0f, 0, 1f);
			}
		}
	}

	
	public void hideAllGo()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	
	private void setCalledFromDeath()
	{
		this.calledFromDeath = true;
	}

	
	private Vector3 getDropPosition(Vector3 dropPos)
	{
		Vector3 origin = dropPos;
		origin.y += 5f;
		RaycastHit[] array = Physics.RaycastAll(origin, Vector3.down, 20f, this.groundMask);
		float num = float.PositiveInfinity;
		Collider exists = null;
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].collider.isTrigger)
			{
				float distance = array[i].distance;
				if (distance < num)
				{
					num = distance;
					exists = array[i].collider;
					this.finalHit = array[i];
				}
			}
		}
		if (exists)
		{
			return this.finalHit.point;
		}
		return dropPos;
	}

	
	private void delayedDisableAnimator()
	{
		this.animator.enabled = false;
	}

	
	private bool animationInTrap()
	{
		AnimatorStateInfo currentAnimatorStateInfo = this.animator.GetCurrentAnimatorStateInfo(0);
		return currentAnimatorStateInfo.shortNameHash == Scene.animHash.nooseTrapDeathHash || currentAnimatorStateInfo.shortNameHash == Scene.animHash.spikeWallDeathHash;
	}

	
	private void setClientDummyInNooseTrap(Transform pivot)
	{
		this.noosePivot = pivot;
	}

	
	private CoopMutantDummy cmd;

	
	private mutantRagdollSetup mrs;

	
	private Animator animator;

	
	private dummyAnimatorControl control;

	
	public GameObject parentGo;

	
	public chopEnemy chop;

	
	private Transform Tr;

	
	public Transform burnTrigger;

	
	private Vector3 pos;

	
	private int layer;

	
	private int layerMask;

	
	private int neckLayerMask;

	
	public LayerMask groundMask;

	
	public bool setupFeedingEncounter;

	
	public bool setupMourningEncounter;

	
	public Transform hips;

	
	public Transform neck;

	
	private float rVal;

	
	private bool randomSeed;

	
	public GameObject trapGo;

	
	private bool aligning;

	
	public bool calledFromDeath;

	
	private float dropTimeOffset;

	
	public bool doneDropCheck;

	
	private Vector3 currPos;

	
	private Vector3 lastPos;

	
	private bool testNeck;

	
	private bool doUpperBodyAlign;

	
	private float neckPos;

	
	private Vector3 setNormal;

	
	private float neckRayCastDelay;

	
	private float refreshRagDollDelay;

	
	private bool playerMovedFarAway;

	
	private float ignoreFarAwayCheck;

	
	public bool bodyOnSled;

	
	private AnimatorStateInfo currState0;

	
	public GameObject[] ragDollParts;

	
	private BoxCollider dropCollider;

	
	private Rigidbody dropRb;

	
	private RaycastHit hit;

	
	private RaycastHit[] allHit;

	
	private RaycastHit[] allHit2;

	
	private RaycastHit hit2;

	
	private float timerAnimatorOffset;

	
	private float timerOffsetPosition;

	
	private float groundCheckTimer;

	
	private RaycastHit finalHit;

	
	private Transform noosePivot;
}
