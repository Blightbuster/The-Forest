using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

public class wormAngelController : MonoBehaviour
{
	private void Start()
	{
		this.thisSkin = base.transform.GetComponentInChildren<SkinnedMeshRenderer>();
		if (this.thisSkin && !this.DebugMovement)
		{
			this.thisSkin.enabled = false;
		}
		this.animator = base.transform.GetComponentInChildren<Animator>();
		this.rootTr = base.transform;
		this.controller = base.transform.GetComponent<CharacterController>();
		this.attachControl = base.transform.GetComponent<wormAttachController>();
		this.formCollapseTimer = Time.time + 15f;
		if (UnityEngine.Random.value > 0.5f)
		{
			this.doSwoopAttack = true;
		}
	}

	private void setHiveController(wormHiveController set)
	{
		this.hiveController = set;
	}

	private void Update()
	{
		if (!this.DebugMovement)
		{
			this.UpdateAttachPoints();
		}
		if (Time.time > this.formCollapseTimer && this.currentAttachedWorms < 30 && !this.animator.GetBool("wakeUp"))
		{
			this.DetachAllWorms();
		}
		if (this.dripWormTimer > Time.time)
		{
			this.DetachRandomWorm();
			this.dripWormTimer = Time.time + 5f;
		}
		this.playerDist = Scene.SceneTracker.GetClosestPlayerDistanceFromPos(base.transform.position);
		if (this.currentAttachedWorms > 26 || this.DebugMovement)
		{
			this.animator.SetBool("wakeUp", true);
			if (this.doSwoopAttack)
			{
				this.animator.SetBool("swoopAttack", true);
			}
		}
		this.MoveTowardTarget();
		if (this.animator.GetBool("wakeUp") && this.currentAttachedWorms == 0)
		{
			this.DetachAllWorms();
		}
	}

	private void UpdateAttachPoints()
	{
		this.currentAttachedWorms = 0;
		for (int i = 0; i < this.attachControl.AttachPoints.Length; i++)
		{
			bool flag = false;
			wormAttachPoints wormAttachPoints = this.attachControl.AttachPoints[i];
			foreach (Transform x in wormAttachPoints.AttachedWorm)
			{
				if (x != null)
				{
					this.currentAttachedWorms++;
					flag = true;
				}
			}
			if (i > 0 && flag)
			{
				bool flag2 = false;
				int num = i - 1;
				foreach (Transform x2 in this.attachControl.AttachPoints[num].AttachedWorm)
				{
					if (x2 != null)
					{
						flag2 = true;
					}
				}
				if (!flag2 && !this.delayedDetach)
				{
					base.StartCoroutine(this.DetachAllWormsDelayedUp(i));
					base.StartCoroutine(this.DetachAllWormsDelayedDown(i));
					this.delayedDetach = true;
					this.attachControl.canAttach = false;
					break;
				}
			}
		}
		if (this.currentAttachedWorms == 0 && this.delayedDetach)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void MoveTowardTarget()
	{
		this.target = Scene.SceneTracker.GetClosestPlayerFromPos(base.transform.position).transform.position;
		this.moveDir = this.animator.deltaPosition;
		this.moveDir.y = this.moveDir.y - this.gravity * Time.deltaTime;
		this.controller.Move(this.moveDir);
		this.rootSmoothLookAtDir(this.target, 2f);
	}

	private void DetachRandomWorm()
	{
		bool flag = false;
		for (int i = 0; i < this.attachControl.AttachPoints.Length; i++)
		{
			i = UnityEngine.Random.Range(0, this.attachControl.AttachPoints.Length);
			wormAttachPoints wormAttachPoints = this.attachControl.AttachPoints[i];
			for (int j = 0; j < wormAttachPoints.AttachedWorm.Length; j++)
			{
				if (wormAttachPoints.AttachedWorm[j] != null)
				{
					wormAttachPoints.AttachedWorm[j].SendMessage("Detach", SendMessageOptions.DontRequireReceiver);
					wormAttachPoints.AttachedWorm[j] = null;
					if (wormAttachPoints.currentEmptySlot > 0)
					{
						wormAttachPoints.currentEmptySlot--;
					}
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
	}

	private void DetachAllWorms()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		foreach (wormAttachPoints wormAttachPoints in this.attachControl.AttachPoints)
		{
			for (int j = 0; j < wormAttachPoints.AttachedWorm.Length; j++)
			{
				if (wormAttachPoints.AttachedWorm[j] != null)
				{
					wormAttachPoints.AttachedWorm[j].SendMessage("Detach", SendMessageOptions.DontRequireReceiver);
					wormAttachPoints.AttachedWorm[j] = null;
				}
			}
			wormAttachPoints.currentEmptySlot = 0;
		}
		float num = Vector3.Distance(LocalPlayer.Transform.position, base.transform.position);
		if (num < 26f)
		{
			LocalPlayer.HitReactions.enableFootShake(num, 0.3f);
		}
		if (this.hiveController)
		{
			this.hiveController.spawnFormCoolDown = Time.time + 5f;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private IEnumerator DetachAllWormsDelayedUp(int startSegment)
	{
		for (int index = startSegment; index < this.attachControl.AttachPoints.Length; index++)
		{
			wormAttachPoints t = this.attachControl.AttachPoints[index];
			for (int i = 0; i < t.AttachedWorm.Length; i++)
			{
				if (t.AttachedWorm[i] != null)
				{
					t.AttachedWorm[i].SendMessage("Detach", SendMessageOptions.DontRequireReceiver);
					t.AttachedWorm[i] = null;
				}
			}
			t.currentEmptySlot = 0;
			yield return YieldPresets.WaitPointOneSeconds;
		}
		yield break;
	}

	private IEnumerator DetachAllWormsDelayedDown(int startSegment)
	{
		for (int index = startSegment; index > -1; index--)
		{
			wormAttachPoints t = this.attachControl.AttachPoints[index];
			for (int i = 0; i < t.AttachedWorm.Length; i++)
			{
				if (t.AttachedWorm[i] != null)
				{
					t.AttachedWorm[i].SendMessage("Detach", SendMessageOptions.DontRequireReceiver);
					t.AttachedWorm[i] = null;
				}
			}
			t.currentEmptySlot = 0;
			yield return YieldPresets.WaitPointOneSeconds;
		}
		yield break;
	}

	private void rootSmoothLookAtDir(Vector3 lookAtPos, float speed)
	{
		lookAtPos.y = base.transform.position.y;
		Vector3 vector = lookAtPos - base.transform.position;
		Quaternion quaternion = base.transform.rotation;
		if (vector != Vector3.zero && vector.sqrMagnitude > 0f)
		{
			this.desiredRotation = Quaternion.LookRotation(vector, Vector3.up);
		}
		quaternion = Quaternion.Slerp(quaternion, this.desiredRotation, speed * Time.deltaTime);
		this.rootTr.rotation = quaternion;
	}

	public wormHiveController hiveController;

	private wormAttachController attachControl;

	private Animator animator;

	private SkinnedMeshRenderer thisSkin;

	public int currentAttachedWorms;

	private Quaternion desiredRotation;

	private float dripWormTimer;

	private float formCollapseTimer;

	private CharacterController controller;

	private Transform rootTr;

	public float playerDist;

	public Vector3 target;

	private Vector3 moveDir;

	public float gravity = 10f;

	public bool delayedDetach;

	private bool doSwoopAttack;

	public bool DebugMovement;
}
