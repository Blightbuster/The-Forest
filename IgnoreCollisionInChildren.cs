using System;
using System.Collections.Generic;
using UnityEngine;


public class IgnoreCollisionInChildren : MonoBehaviour
{
	
	private void OnEnable()
	{
		if (this.DisableThisScript)
		{
			base.enabled = false;
			return;
		}
		if (this.m_Root == null)
		{
			this.m_Root = base.gameObject;
		}
		if (BoltNetwork.isRunning)
		{
			if (this.Raft)
			{
				this.RaftsDisableCollisionWithChildren();
			}
			else
			{
				this.DisableCollisionWithChildren();
			}
		}
	}

	
	private void Start()
	{
		if (this.DisableThisScript)
		{
			base.enabled = false;
			return;
		}
		if (this.m_Root == null)
		{
			this.m_Root = base.gameObject;
		}
		if (this.Raft)
		{
			Collider[] componentsInChildren = this.m_Root.GetComponentsInChildren<Collider>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				this.storedColliders.Add(componentsInChildren[i]);
			}
		}
		if (this.Raft)
		{
			this.RaftsDisableCollisionWithChildren();
		}
		else
		{
			this.DisableCollisionWithChildren();
		}
	}

	
	public void startDisableChildren()
	{
		if (this.Raft)
		{
			this.RaftsDisableCollisionWithChildren();
		}
		else
		{
			this.DisableCollisionWithChildren();
		}
	}

	
	public void DisableCollisionWithChildren()
	{
		Collider[] componentsInChildren = this.m_Root.GetComponentsInChildren<Collider>();
		Collider component = this.m_Root.GetComponent<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			if (this.m_IncludeRoot && component != null)
			{
				Physics.IgnoreCollision(collider, component);
			}
			foreach (Collider collider2 in componentsInChildren)
			{
				if (collider != collider2 && !collider.isTrigger && !collider2.isTrigger)
				{
					Physics.IgnoreCollision(collider, collider2);
				}
			}
		}
	}

	
	public void RaftsDisableCollisionWithChildren()
	{
		if (this.storedColliders.Count == 0)
		{
			Collider[] componentsInChildren = this.m_Root.GetComponentsInChildren<Collider>(true);
			if (componentsInChildren.Length != 0)
			{
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (componentsInChildren[i] != null && !componentsInChildren[i].isTrigger)
					{
						this.storedColliders.Add(componentsInChildren[i]);
					}
				}
			}
		}
		Collider component = this.m_Root.GetComponent<Collider>();
		for (int j = 0; j < this.storedColliders.Count; j++)
		{
			if (this.m_IncludeRoot && component != null && this.storedColliders[j] != null)
			{
				Physics.IgnoreCollision(this.storedColliders[j], component);
			}
		}
	}

	
	[SerializeField]
	private GameObject m_Root;

	
	[SerializeField]
	private bool m_IncludeRoot = true;

	
	public List<Collider> storedColliders = new List<Collider>();

	
	public bool Raft;

	
	public bool DisableThisScript;
}
