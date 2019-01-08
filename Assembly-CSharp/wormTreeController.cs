using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

[Serializable]
public class wormTreeController : MonoBehaviour
{
	private void Start()
	{
		if (this.thisSkin)
		{
			this.thisSkin.enabled = false;
		}
		this.animator = base.transform.GetComponent<Animator>();
		this.attachControl = base.transform.GetComponent<wormAttachController>();
		if (UnityEngine.Random.value > 0.4f)
		{
			this.findCloseTree();
		}
		this.detachCoolDown = Time.time + 14f;
	}

	private void findCloseTree()
	{
		foreach (GameObject gameObject in Scene.SceneTracker.closeTrees)
		{
			if (Vector3.Distance(base.transform.position, gameObject.transform.position) < 15f && Vector3.Distance(gameObject.transform.position, LocalPlayer.Transform.position) < 20f)
			{
				this.closeTreeGo = gameObject;
				this.closeTreePos = this.closeTreeGo.transform.position;
				break;
			}
		}
	}

	private void setHiveController(wormHiveController set)
	{
		this.hiveController = set;
	}

	private void Update()
	{
		this.UpdateAttachPoints();
		float closestPlayerDistanceFromPos = Scene.SceneTracker.GetClosestPlayerDistanceFromPos(base.transform.position);
		if (this.closeTreeGo != null && !this.delayedDetach)
		{
			this.doTreeAttack();
		}
		if (this.currentAttachedWorms > 12 && closestPlayerDistanceFromPos < 35f && this.closeTreeGo == null)
		{
			this.doCollapseAttack();
		}
		else
		{
			this.animator.SetBool("whipBool", false);
			this.animator.SetBool("collapseBool", false);
		}
		if (Time.time > this.detachCoolDown)
		{
			this.DetachAllWorms();
		}
		if (closestPlayerDistanceFromPos > 40f && !this.delayedDetach && this.currentAttachedWorms > 4)
		{
			base.StartCoroutine(this.DetachAllWormsDelayedUp(1));
			base.StartCoroutine(this.DetachAllWormsDelayedDown(1));
			this.delayedDetach = true;
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
			this.attachControl.numAttachedWorms = this.currentAttachedWorms;
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
				if (!flag2)
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

	private void doTreeAttack()
	{
		if (this.closeTreeGo == null)
		{
			return;
		}
		float y = Terrain.activeTerrain.SampleHeight(this.closeTreePos) + Terrain.activeTerrain.transform.position.y;
		Vector3 position = this.closeTreePos;
		position.y = y;
		base.transform.position = position;
		this.animator.SetBool("treeIdle", true);
		if (this.currentAttachedWorms > 16)
		{
			this.animator.SetBool("treeAttack", true);
			Vector3 position2 = Scene.SceneTracker.GetClosestPlayerFromPos(base.transform.position).transform.position;
			this.doSmoothLookAtDir(position2, 2.5f);
		}
		else
		{
			this.animator.SetBool("treeAttack", false);
		}
	}

	private void doCollapseAttack()
	{
		Vector3 position = Scene.SceneTracker.GetClosestPlayerFromPos(base.transform.position).transform.position;
		this.doSmoothLookAtDir(position, 2.5f);
		if (UnityEngine.Random.value > 0.5f)
		{
			this.animator.SetBool("whipBool", true);
		}
		else
		{
			this.animator.SetBool("collapseBool", true);
		}
	}

	private void DetachAllWorms()
	{
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
		this.enableFootShake();
		if (this.hiveController)
		{
			this.hiveController.spawnFormCoolDown = Time.time + 4f;
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

	private void enableFootShake()
	{
		float num = Vector3.Distance(LocalPlayer.Transform.position, base.transform.position);
		if (num < 26f)
		{
			LocalPlayer.HitReactions.enableFootShake(num, 0.3f);
		}
	}

	private void doSmoothLookAtDir(Vector3 lookAtPos, float speed)
	{
		lookAtPos.y = base.transform.position.y;
		Vector3 vector = lookAtPos - base.transform.position;
		Quaternion quaternion = base.transform.rotation;
		if (vector != Vector3.zero && vector.sqrMagnitude > 0f)
		{
			this.desiredRotation = Quaternion.LookRotation(vector, Vector3.up);
		}
		quaternion = Quaternion.Slerp(quaternion, this.desiredRotation, speed * Time.deltaTime);
		base.transform.rotation = quaternion;
	}

	public wormHiveController hiveController;

	private wormAttachController attachControl;

	private Animator animator;

	public GameObject closeTreeGo;

	public SkinnedMeshRenderer thisSkin;

	public int currentAttachedWorms;

	private float detachCoolDown;

	private Quaternion desiredRotation;

	private Vector3 closeTreePos;

	public bool delayedDetach;
}
