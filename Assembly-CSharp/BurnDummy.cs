using System;
using System.Collections;
using Bolt;
using TheForest.Items;
using UnityEngine;

public class BurnDummy : EntityBehaviour
{
	private void Burn()
	{
		if (!BoltNetwork.isClient && !this._isBurning)
		{
			base.StartCoroutine(this.BurnRoutine(false));
			if (BoltNetwork.isServer && base.entity.isAttached)
			{
				if (base.entity.StateIs<IMutantMaleDummyState>())
				{
					base.entity.GetState<IMutantMaleDummyState>().IsBurning = true;
				}
				else if (base.entity.StateIs<IMutantFemaleDummyState>())
				{
					base.entity.GetState<IMutantFemaleDummyState>().IsBurning = true;
				}
			}
		}
	}

	private void OnEnable()
	{
		if (this._isBurning && !BoltNetwork.isClient)
		{
			base.StartCoroutine(this.BurnRoutine(true));
		}
	}

	private IEnumerator BurnRoutine(bool burnShort = false)
	{
		this._isBurning = true;
		this.BurnSFX();
		if (burnShort)
		{
			yield return YieldPresets.WaitTenSeconds;
		}
		else
		{
			yield return YieldPresets.WaitFourtySeconds;
		}
		Transform bonePrefab = BoltNetwork.isRunning ? ItemDatabase.ItemById(this._boneItemId)._pickupPrefabMP : ItemDatabase.ItemById(this._boneItemId)._pickupPrefab;
		Transform skullPrefab = BoltNetwork.isRunning ? ItemDatabase.ItemById(this._skullItemId)._pickupPrefabMP : ItemDatabase.ItemById(this._skullItemId)._pickupPrefab;
		Transform pickup;
		for (int i = 0; i < this._bonesSpawn.Length; i++)
		{
			if (this._bonesSpawn[i])
			{
				pickup = (BoltNetwork.isRunning ? BoltNetwork.Instantiate(bonePrefab.gameObject).transform : UnityEngine.Object.Instantiate<Transform>(bonePrefab));
				pickup.position = this._bonesSpawn[i].position;
				pickup.rotation = this._bonesSpawn[i].rotation;
			}
		}
		pickup = (BoltNetwork.isRunning ? BoltNetwork.Instantiate(bonePrefab.gameObject).transform : UnityEngine.Object.Instantiate<Transform>(bonePrefab));
		pickup.position = base.transform.position + Vector3.left;
		pickup.rotation = base.transform.rotation;
		pickup = (BoltNetwork.isRunning ? BoltNetwork.Instantiate(bonePrefab.gameObject).transform : UnityEngine.Object.Instantiate<Transform>(bonePrefab));
		pickup.position = base.transform.position + Vector3.right;
		pickup.rotation = base.transform.rotation;
		if (this._skullSpawn)
		{
			pickup = (BoltNetwork.isRunning ? BoltNetwork.Instantiate(skullPrefab.gameObject).transform : UnityEngine.Object.Instantiate<Transform>(skullPrefab));
			pickup.position = this._skullSpawn.position;
			pickup.rotation = this._skullSpawn.rotation;
		}
		FMODCommon.PlayOneshotNetworked("event:/fire/bones_explode", base.transform, FMODCommon.NetworkRole.Any);
		limitSledBlur lsb = base.transform.GetComponentInParent<limitSledBlur>();
		if (lsb && lsb.mh)
		{
			lsb.mh.PickUpBody();
		}
		if (BoltNetwork.isRunning && base.entity.isAttached)
		{
			BoltNetwork.Destroy(base.transform.parent.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.transform.parent.gameObject);
		}
		yield break;
	}

	private void BurnSFX()
	{
		if (this._infectionTrigger)
		{
			UnityEngine.Object.Destroy(this._infectionTrigger);
			this._infectionTrigger = null;
		}
		for (int i = 0; i < this._renderers.Length; i++)
		{
			if (this._renderers[i])
			{
				this._renderers[i].sharedMaterial = this._burntMat;
			}
		}
		this._fire.SetActive(true);
		this._pickupTrigger.SetActive(false);
	}

	public override void Attached()
	{
		if (base.entity.StateIs<IMutantMaleDummyState>())
		{
			base.entity.GetState<IMutantMaleDummyState>().AddCallback("IsBurning", new PropertyCallbackSimple(this.BurnSFX));
		}
		else if (base.entity.StateIs<IMutantFemaleDummyState>())
		{
			base.entity.GetState<IMutantFemaleDummyState>().AddCallback("IsBurning", new PropertyCallbackSimple(this.BurnSFX));
		}
	}

	public GameObject _infectionTrigger;

	public Transform[] _bonesSpawn;

	public Transform _skullSpawn;

	public GameObject _pickupTrigger;

	public GameObject _fire;

	public Renderer[] _renderers;

	public Material _burntMat;

	[ItemIdPicker]
	public int _boneItemId;

	[ItemIdPicker]
	public int _skullItemId;

	public bool _isBurning;
}
