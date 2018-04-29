using System;
using UnityEngine;


public class ReplaceWithNewPrefab : MonoBehaviour
{
	
	private void Start()
	{
		if (!BoltNetwork.isClient || this._replaceIfClient)
		{
			if (!this._replaceTarget)
			{
				this._replaceTarget = base.gameObject;
			}
			Vector3 position;
			Quaternion rotation;
			if (this._newPrefabPositionTarget)
			{
				position = this._newPrefabPositionTarget.position;
				rotation = this._newPrefabPositionTarget.rotation;
			}
			else
			{
				position = this._replaceTarget.transform.position;
				rotation = this._replaceTarget.transform.rotation;
			}
			Transform transform = (Transform)UnityEngine.Object.Instantiate(this._newPrefab, position, rotation);
			if (BoltNetwork.isServer && !this._doLocalOnlyCheck)
			{
				BoltNetwork.Attach(transform.gameObject);
			}
			if (this._transfertChildrenPrefabs)
			{
				foreach (object obj in this._replaceTarget.transform)
				{
					Transform transform2 = (Transform)obj;
					if (transform2.GetComponent<PrefabIdentifier>())
					{
						transform2.parent = transform.transform;
					}
				}
			}
			coopDeadSharkCutHead component = base.transform.GetComponent<coopDeadSharkCutHead>();
			if (component)
			{
				component.syncRagDollForServer(transform.gameObject);
			}
			if (this._newPrefab2)
			{
				Transform transform3 = (Transform)UnityEngine.Object.Instantiate(this._newPrefab2, position, rotation);
				if (BoltNetwork.isServer && !this._doLocalOnlyCheck)
				{
					BoltNetwork.Attach(transform3.gameObject);
				}
				if (!this._parentToWorld)
				{
					transform3.parent = this._replaceTarget.transform.parent;
				}
			}
			if (!this._parentToWorld)
			{
				transform.parent = this._replaceTarget.transform.parent;
			}
			this._replaceTarget.transform.parent = null;
			if (!this._dontDisableReplaceTarget)
			{
				this._replaceTarget.SetActive(false);
			}
			UnityEngine.Object.Destroy(this._replaceTarget, this._destroyTime);
		}
		else if (this._destroyIfClient)
		{
			BoltEntity component2 = this._replaceTarget.GetComponent<BoltEntity>();
			if (!this._doLocalOnlyCheck || !component2 || !component2.isAttached)
			{
				UnityEngine.Object.Destroy(this._replaceTarget, this._destroyTime);
			}
		}
	}

	
	public Transform _newPrefab;

	
	public Transform _newPrefab2;

	
	public Transform _newPrefabPositionTarget;

	
	public GameObject _replaceTarget;

	
	public bool _destroyIfClient;

	
	public bool _replaceIfClient;

	
	public bool _doLocalOnlyCheck;

	
	public bool _dontDisableReplaceTarget;

	
	public bool _transfertChildrenPrefabs;

	
	public bool _parentToWorld;

	
	public float _destroyTime;
}
