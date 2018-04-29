using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Items.Craft;
using TheForest.Items.Inventory;
using TheForest.Items.World;
using UnityEngine;


public class arrowStickToTarget : MonoBehaviour
{
	
	private void Awake()
	{
		if (this.stickToJoints.Length != 0 || !this.singleJointMode)
		{
		}
		this.fakeArrowBonePickup = (Resources.Load("fakeArrowBonePickup") as GameObject);
		this.fakeArrowModernPickup = (Resources.Load("fakeArrowModernPickup") as GameObject);
		this.fakeArrowPickup = (Resources.Load("fakeArrowPickup") as GameObject);
	}

	
	private void OnSpawned()
	{
		this.cleanUpStuckArrows();
	}

	
	private void cleanUpStuckArrows()
	{
		foreach (KeyValuePair<Transform, int> keyValuePair in this.stuckArrows)
		{
			if (keyValuePair.Key)
			{
				UnityEngine.Object.Destroy(keyValuePair.Key.gameObject);
			}
		}
		this.stuckArrows.Clear();
	}

	
	public void CreatureType(bool animal, bool bird, bool fish)
	{
		this._animal = animal;
		this._bird = bird;
		this._fish = fish;
	}

	
	
	public bool IsAnimal
	{
		get
		{
			return this._animal;
		}
	}

	
	
	public bool IsBird
	{
		get
		{
			return this._bird;
		}
	}

	
	
	public bool IsFish
	{
		get
		{
			return this._fish;
		}
	}

	
	
	public bool IsCreature
	{
		get
		{
			return this._animal || this._bird || this._fish;
		}
	}

	
	public bool stickArrowToNearestBone(Transform arrow)
	{
		Transform parent = arrow.parent;
		WeaponStatUpgrade.Types arrowBonus = (WeaponStatUpgrade.Types)(-1);
		ItemProperties properties = parent.GetComponent<arrowTrajectory>()._pickup.GetComponent<PickUp>()._properties;
		if (properties != null && properties.ActiveBonus != (WeaponStatUpgrade.Types)(-1))
		{
			arrowBonus = properties.ActiveBonus;
		}
		GameObject gameObject;
		switch (arrowBonus)
		{
		case WeaponStatUpgrade.Types.BoneAmmo:
			gameObject = (GameObject)UnityEngine.Object.Instantiate(this.fakeArrowBonePickup, parent.transform.position, parent.transform.rotation);
			goto IL_DE;
		case WeaponStatUpgrade.Types.ModernAmmo:
			gameObject = (GameObject)UnityEngine.Object.Instantiate(this.fakeArrowModernPickup, parent.transform.position, parent.transform.rotation);
			goto IL_DE;
		}
		gameObject = (GameObject)UnityEngine.Object.Instantiate(this.fakeArrowPickup, parent.transform.position, parent.transform.rotation);
		IL_DE:
		Collider component = gameObject.GetComponent<Collider>();
		if (component)
		{
			component.enabled = false;
		}
		Transform tip = gameObject.GetComponent<fakeArrowSetup>().tip;
		int num = this.returnNearestJointMidPoint(tip);
		if (this.singleJointMode)
		{
			num = 0;
			Vector3 vector = (gameObject.transform.position - this.baseJoint.position).normalized;
			vector = this.baseJoint.position + vector * 0.2f;
			gameObject.transform.parent = this.baseJoint;
			gameObject.transform.position = vector;
			gameObject.transform.rotation = Quaternion.LookRotation(gameObject.transform.position - this.baseJoint.position) * Quaternion.Euler(-90f, 0f, 0f);
		}
		else
		{
			Transform transform = this.stickToJoints[num];
			foreach (object obj in this.stickToJoints[num])
			{
				Transform transform2 = (Transform)obj;
				if (!transform2.GetComponent<MonoBehaviour>())
				{
					transform = transform2;
					break;
				}
			}
			Vector3 vector2 = (this.stickToJoints[num].position + transform.position) / 2f;
			Vector3 vector3 = (gameObject.transform.position - vector2).normalized;
			vector3 = vector2 + vector3 * 0.35f;
			gameObject.transform.parent = this.stickToJoints[num];
			gameObject.transform.position = vector3;
			gameObject.transform.rotation = Quaternion.LookRotation(gameObject.transform.position - vector2) * Quaternion.Euler(-90f, 0f, 0f);
		}
		bool result = false;
		if (this.stickToJoints.Length > 0 && this.stickToJoints[num] && this.stickToJoints[num].GetComponent<headShotObject>())
		{
			result = true;
		}
		if (!this.stuckArrows.ContainsKey(gameObject.transform))
		{
			this.stuckArrows.Add(gameObject.transform, num);
			fakeArrowSetup component2 = gameObject.GetComponent<fakeArrowSetup>();
			if (component2 && BoltNetwork.isRunning)
			{
				component2.storedIndex = this.stuckArrows.Count - 1;
				component2.entityTarget = base.transform.root.GetComponent<BoltEntity>();
			}
			this.numStuckArrows++;
		}
		if (BoltNetwork.isRunning)
		{
			BoltEntity component3 = parent.GetComponent<BoltEntity>();
			if (component3.isAttached && component3.isOwner)
			{
				if (this.IsCreature && BoltNetwork.isServer)
				{
					base.StartCoroutine(this.SendArrowMPDelayed(gameObject, num, arrowBonus));
				}
				else
				{
					this.sendArrowMP(gameObject, num, arrowBonus);
				}
			}
		}
		return result;
	}

	
	public void sendArrowMP(GameObject fakeArrow, int closestJointIndex, WeaponStatUpgrade.Types arrowBonus = (WeaponStatUpgrade.Types)(-1))
	{
		stuckArrowsSync stuckArrowsSync = stuckArrowsSync.Create(GlobalTargets.Others);
		BoltEntity component = base.transform.root.GetComponent<BoltEntity>();
		if (component)
		{
		}
		stuckArrowsSync.target = component;
		stuckArrowsSync.pos = fakeArrow.transform.localPosition;
		stuckArrowsSync.rot = fakeArrow.transform.localRotation;
		if (arrowBonus == WeaponStatUpgrade.Types.BoneAmmo)
		{
			stuckArrowsSync.type = 1;
		}
		stuckArrowsSync.index = closestJointIndex;
		if (stuckArrowsSync.target)
		{
			stuckArrowsSync.Send();
		}
	}

	
	public IEnumerator SendArrowMPDelayed(GameObject fakeArrow, int closestJointIndex, WeaponStatUpgrade.Types arrowBonus = (WeaponStatUpgrade.Types)(-1))
	{
		yield return new WaitForSeconds(1f);
		this.sendArrowMP(fakeArrow, closestJointIndex, arrowBonus);
		yield break;
	}

	
	public Transform returnNearestJoint(Transform target)
	{
		int num = this.returnNearestJointMidPoint(target);
		if (this.singleJointMode)
		{
			return this.baseJoint;
		}
		return this.stickToJoints[num];
	}

	
	public bool isHeadTransform(Transform target)
	{
		int num = this.returnNearestJointMidPoint(target);
		return this.stickToJoints.Length > 0 && this.stickToJoints[num] && this.stickToJoints[num].GetComponent<headShotObject>();
	}

	
	private int returnNearestJointMidPoint(Transform arrow)
	{
		float num = float.PositiveInfinity;
		int result = 0;
		Vector3 vector = arrow.position;
		vector += arrow.forward * 2f;
		for (int i = 0; i < this.stickToJoints.Length; i++)
		{
			float sqrMagnitude = (this.stickToJoints[i].position - vector).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = i;
			}
		}
		return result;
	}

	
	public void applyStuckArrowToDummy(Transform arrow, Vector3 pos, Quaternion rot, int index)
	{
		if (this.nearestJointOnRagdollMode)
		{
			int num = this.returnNearestJointMidPoint(arrow);
			Vector3 vector = (arrow.position - this.stickToJoints[num].position).normalized;
			vector = this.stickToJoints[num].position + vector * 0.35f;
			arrow.parent = this.stickToJoints[num];
			arrow.position = vector;
			arrow.rotation = Quaternion.LookRotation(arrow.position - this.stickToJoints[num].position) * Quaternion.Euler(-90f, 0f, 0f);
		}
		else if (this.singleJointMode)
		{
			arrow.parent = this.baseJoint;
		}
		else
		{
			arrow.parent = this.stickToJoints[index];
		}
		if (!this.nearestJointOnRagdollMode)
		{
			arrow.localPosition = pos;
			arrow.localRotation = rot;
		}
		if (this.IsCreature)
		{
			arrow.localPosition = pos;
			arrow.localRotation = rot;
		}
		int count;
		if (this.IsCreature)
		{
			this.stuckArrows.Add(arrow, index);
			count = this.stuckArrows.Count;
		}
		else
		{
			count = this.stuckArrows.Count;
			this.stuckArrows.Add(arrow, index);
		}
		fakeArrowSetup component = arrow.GetComponent<fakeArrowSetup>();
		if (component && BoltNetwork.isRunning)
		{
			component.storedIndex = count - 1;
			component.entityTarget = base.transform.root.GetComponent<BoltEntity>();
		}
		if (BoltNetwork.isRunning && BoltNetwork.isServer && this.IsCreature)
		{
			base.StartCoroutine(this.SendArrowMPDelayed(arrow.gameObject, index, (WeaponStatUpgrade.Types)(-1)));
		}
	}

	
	public Transform[] stickToJoints;

	
	public Transform baseJoint;

	
	public bool singleJointMode;

	
	public bool nearestJointOnRagdollMode;

	
	public Dictionary<Transform, int> stuckArrows = new Dictionary<Transform, int>();

	
	public int numStuckArrows;

	
	public GameObject fakeArrowBonePickup;

	
	public GameObject fakeArrowModernPickup;

	
	public GameObject fakeArrowPickup;

	
	private bool _animal;

	
	private bool _bird;

	
	private bool _fish;
}
