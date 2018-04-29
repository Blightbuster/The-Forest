using System;
using System.Collections.Generic;
using PathologicalGames;
using TheForest.Buildings.Interfaces;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	public class ShowSupportingStructureAnchor : MonoBehaviour
	{
		
		private void Awake()
		{
			this._currentSupportsInfo = new List<ShowSupportingStructureAnchor.SupportInfo>();
			this._pool = new Queue<ShowSupportingStructureAnchor.SupportInfo>();
		}

		
		private void OnDisable()
		{
			foreach (ShowSupportingStructureAnchor.SupportInfo supportInfo in this._currentSupportsInfo)
			{
				if (supportInfo._icon)
				{
					PoolManager.Pools["misc"].Despawn(supportInfo._icon);
				}
				supportInfo._support = null;
				supportInfo._icon = null;
				this._pool.Enqueue(supportInfo);
			}
			this._currentSupportsInfo.Clear();
		}

		
		private void OnTriggerEnter(Collider other)
		{
			IStructureSupport support = other.GetComponentInParent<IStructureSupport>();
			if (support != null)
			{
				int num = this._currentSupportsInfo.FindIndex((ShowSupportingStructureAnchor.SupportInfo s) => s._support == support);
				if (num >= 0)
				{
					this.AddSupportCollider(this._currentSupportsInfo[num], other);
				}
				else
				{
					this.ShowSupportAnchor(support, other);
				}
			}
		}

		
		private void OnTriggerExit(Collider other)
		{
			IStructureSupport support = other.GetComponentInParent<IStructureSupport>();
			if (support != null)
			{
				int num = this._currentSupportsInfo.FindIndex((ShowSupportingStructureAnchor.SupportInfo s) => s._support == support);
				if (num >= 0)
				{
					this.RemoveSupportCollider(this._currentSupportsInfo[num], other);
				}
			}
		}

		
		private void OnDestroy()
		{
			foreach (ShowSupportingStructureAnchor.SupportInfo supportInfo in this._currentSupportsInfo)
			{
				supportInfo.Clear();
			}
			foreach (ShowSupportingStructureAnchor.SupportInfo supportInfo2 in this._pool)
			{
				supportInfo2.Clear();
			}
			this._currentSupportsInfo.Clear();
			this._currentSupportsInfo = null;
			this._pool.Clear();
			this._pool = null;
		}

		
		private void ShowSupportAnchor(IStructureSupport support, Collider c)
		{
			ShowSupportingStructureAnchor.SupportInfo supportInfo = (this._pool.Count <= 0) ? new ShowSupportingStructureAnchor.SupportInfo() : this._pool.Dequeue();
			supportInfo._support = support;
			supportInfo._icon = PoolManager.Pools["misc"].Spawn(this._anchorPrefab, support.SupportCenter + (support as MonoBehaviour).transform.TransformDirection(this._iconOffset), Quaternion.identity);
			if (supportInfo._colliders == null)
			{
				supportInfo._colliders = new List<Collider>();
			}
			supportInfo._colliders.Add(c);
			this._currentSupportsInfo.Add(supportInfo);
		}

		
		private void AddSupportCollider(ShowSupportingStructureAnchor.SupportInfo info, Collider c)
		{
			if (!info._colliders.Contains(c))
			{
				info._colliders.Add(c);
			}
		}

		
		private void RemoveSupportCollider(ShowSupportingStructureAnchor.SupportInfo info, Collider c)
		{
			if (info._colliders.Contains(c))
			{
				info._colliders.Remove(c);
				if (info._colliders.Count == 0)
				{
					PoolManager.Pools["misc"].Despawn(info._icon);
					this._currentSupportsInfo.Remove(info);
					info._support = null;
					info._icon = null;
					this._pool.Enqueue(info);
				}
			}
		}

		
		public Transform _anchorPrefab;

		
		public Vector3 _iconOffset = new Vector3(0f, 1f, 0f);

		
		private List<ShowSupportingStructureAnchor.SupportInfo> _currentSupportsInfo;

		
		private Queue<ShowSupportingStructureAnchor.SupportInfo> _pool;

		
		[Serializable]
		public class SupportInfo
		{
			
			public void Clear()
			{
				this._support = null;
				this._icon = null;
				if (this._colliders != null)
				{
					this._colliders.Clear();
					this._colliders = null;
				}
			}

			
			public IStructureSupport _support;

			
			public Transform _icon;

			
			public List<Collider> _colliders;
		}
	}
}
