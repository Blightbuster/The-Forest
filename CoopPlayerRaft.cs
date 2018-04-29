using System;
using System.Collections.Generic;
using Bolt;
using TheForest.Buildings.Creation;
using TheForest.Utils;
using UnityEngine;


public class CoopPlayerRaft : EntityBehaviour<IPlayerState>
{
	
	private void Awake()
	{
		base.enabled = false;
	}

	
	public override void Attached()
	{
		this._proxyTransform = new GameObject("ProxyDynamicTransform").transform;
		base.state.RaftTransform.SetTransforms(this._proxyTransform);
		base.state.AddCallback("RaftEntity", new PropertyCallbackSimple(this.OnDynamicEntityChange));
		base.enabled = true;
	}

	
	private void OnDynamicEntityChange()
	{
		this._dynamicEntity = base.state.RaftEntity;
		this._hasDynamicEntity = this._dynamicEntity;
		if (this._hasDynamicEntity)
		{
			this._dynamicTransform = this.GetDynamicTransform(this._dynamicEntity);
		}
		else
		{
			this._dynamicTransform = null;
		}
	}

	
	public override void Detached()
	{
		if (this._proxyTransform)
		{
			UnityEngine.Object.Destroy(this._proxyTransform.gameObject);
			this._proxyTransform = null;
		}
		this._dynamicEntity = null;
		this._dynamicTransform = null;
	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (BoltNetwork.isRunning && this.entity.IsOwner() && other.gameObject.CompareTag("RaftMPTrigger"))
		{
			if (!this._dynamicMpTriggers.Contains(other))
			{
				this._dynamicMpTriggers.Add(other);
			}
			base.state.RaftEntity = other.gameObject.GetComponentInParent<BoltEntity>();
		}
		if (BoltNetwork.isRunning && !this.entity.IsOwner() && base.gameObject.CompareTag("PlayerNet") && other.gameObject.CompareTag("RaftMPTrigger"))
		{
			Collider component = base.gameObject.GetComponent<CapsuleCollider>();
			if (component && component.enabled && component.gameObject.activeSelf)
			{
				Collider[] componentsInChildren = other.transform.root.GetComponentsInChildren<Collider>();
				foreach (Collider collider in componentsInChildren)
				{
					if (!collider.isTrigger && collider.enabled && collider.gameObject.activeSelf)
					{
						Physics.IgnoreCollision(component, collider, true);
					}
				}
			}
		}
	}

	
	private void LateUpdate()
	{
		if (this.entity.isAttached)
		{
			if (this.entity.isOwner)
			{
				if (this._dynamicMpTriggers.Count > 0)
				{
					for (int i = this._dynamicMpTriggers.Count - 1; i >= 0; i--)
					{
						if (this._dynamicMpTriggers[i] == null || !this._dynamicMpTriggers[i].bounds.Intersects(LocalPlayer.FpCharacter.capsule.bounds))
						{
							this._dynamicMpTriggers.RemoveAt(i);
						}
					}
					if (this._dynamicMpTriggers.Count == 0)
					{
						this._hasDynamicEntity = false;
						base.state.RaftEntity = null;
					}
					else if (!this._hasDynamicEntity)
					{
						BoltEntity componentInParent = this._dynamicMpTriggers[0].GetComponentInParent<BoltEntity>();
						base.state.RaftEntity = componentInParent;
						this._dynamicEntity = componentInParent;
						this._dynamicTransform = this.GetDynamicTransform(this._dynamicEntity);
						this._hasDynamicEntity = this._dynamicEntity;
					}
				}
				if (this._hasDynamicEntity)
				{
					this._proxyTransform.position = this._dynamicTransform.InverseTransformPoint(base.transform.position);
				}
			}
			else if (this._hasDynamicEntity && this._dynamicTransform)
			{
				base.transform.position = this._dynamicTransform.TransformPoint(this._proxyTransform.position);
			}
		}
	}

	
	private Transform GetDynamicTransform(BoltEntity entity)
	{
		DynamicBuilding component = entity.GetComponent<DynamicBuilding>();
		if (component && component._parentOverride)
		{
			return component._parentOverride;
		}
		return entity.transform;
	}

	
	private BoltEntity _dynamicEntity;

	
	private Transform _dynamicTransform;

	
	private Transform _proxyTransform;

	
	private bool _hasDynamicEntity;

	
	private List<Collider> _dynamicMpTriggers = new List<Collider>();
}
