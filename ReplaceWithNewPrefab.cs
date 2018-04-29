using System;
using System.Collections;
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
			Transform transform = UnityEngine.Object.Instantiate<Transform>(this._newPrefab, position, rotation);
			if (BoltNetwork.isServer && !this._doLocalOnlyCheck)
			{
				BoltNetwork.Attach(transform.gameObject);
			}
			if (this._transfertChildrenPrefabs)
			{
				IEnumerator enumerator = this._replaceTarget.transform.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform transform2 = (Transform)obj;
						if (transform2.GetComponent<PrefabIdentifier>())
						{
							transform2.parent = transform.transform;
						}
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
			}
			if (this._transferSkinProperties)
			{
				this.setupSkinProperties(transform);
			}
			coopDeadSharkCutHead component = base.transform.GetComponent<coopDeadSharkCutHead>();
			if (component)
			{
				component.syncRagDollForServer(transform.gameObject);
			}
			if (this._newPrefab2)
			{
				Transform transform3 = UnityEngine.Object.Instantiate<Transform>(this._newPrefab2, position, rotation);
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
			if (this._preserveScale)
			{
				transform.localScale = this._replaceTarget.transform.localScale;
			}
			BoltEntity component2 = this._replaceTarget.GetComponent<BoltEntity>();
			if (!BoltNetwork.isClient || !component2 || !component2.isAttached || component2.isOwner)
			{
				this._replaceTarget.transform.parent = null;
				if (!this._dontDisableReplaceTarget)
				{
					this._replaceTarget.SetActive(false);
				}
				UnityEngine.Object.Destroy(this._replaceTarget, this._destroyTime);
			}
		}
		else if (this._destroyIfClient)
		{
			BoltEntity component3 = this._replaceTarget.GetComponent<BoltEntity>();
			if (!this._doLocalOnlyCheck || !component3 || !component3.isAttached)
			{
				UnityEngine.Object.Destroy(this._replaceTarget, this._destroyTime);
			}
		}
	}

	
	private void setupSkinProperties(Transform target)
	{
		Renderer[] componentsInChildren = target.GetComponentsInChildren<Renderer>();
		Renderer renderer = null;
		foreach (Renderer renderer2 in componentsInChildren)
		{
			MeshFilter component = renderer2.GetComponent<MeshFilter>();
			if (component && component.mesh.name.Contains("Head"))
			{
				renderer = renderer2;
			}
		}
		if (renderer)
		{
			Material[] materials = renderer.materials;
			materials[0] = this._sourceSkinRenderer.material;
			renderer.materials = materials;
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			this._sourceSkinRenderer.GetPropertyBlock(materialPropertyBlock);
			MaterialPropertyBlock materialPropertyBlock2 = new MaterialPropertyBlock();
			renderer.GetPropertyBlock(materialPropertyBlock2);
			materialPropertyBlock2.SetFloat("_Damage1", materialPropertyBlock.GetFloat("_Damage1"));
			materialPropertyBlock2.SetFloat("_Damage2", materialPropertyBlock.GetFloat("_Damage2"));
			materialPropertyBlock2.SetFloat("_Damage3", materialPropertyBlock.GetFloat("_Damage3"));
			materialPropertyBlock2.SetFloat("_Damage4", materialPropertyBlock.GetFloat("_Damage4"));
			renderer.SetPropertyBlock(materialPropertyBlock2);
		}
	}

	
	public Transform _newPrefab;

	
	public Transform _newPrefab2;

	
	public Transform _newPrefabPositionTarget;

	
	public GameObject _replaceTarget;

	
	public Renderer _sourceSkinRenderer;

	
	public bool _destroyIfClient;

	
	public bool _replaceIfClient;

	
	public bool _preserveScale;

	
	public bool _doLocalOnlyCheck;

	
	public bool _dontDisableReplaceTarget;

	
	public bool _transfertChildrenPrefabs;

	
	public bool _parentToWorld;

	
	public bool _transferSkinProperties;

	
	public float _destroyTime;
}
