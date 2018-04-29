using System;
using System.Collections.Generic;
using Bolt;
using UnityEngine;


internal class CoopComponentDisabler : EntityBehaviour
{
	
	public void EnableComponents()
	{
		this.Apply(true);
	}

	
	public void DisableComponents()
	{
		this.Apply(false);
	}

	
	private void Apply(bool enabled)
	{
		for (int i = 0; i < this.Renderers.Count; i++)
		{
			if (this.Renderers[i])
			{
				this.Renderers[i].enabled = enabled;
			}
		}
		for (int j = 0; j < this.Colliders.Count; j++)
		{
			if (this.Colliders[j])
			{
				this.Colliders[j].enabled = enabled;
			}
		}
		if (this.animator && !this.ignoreAnimator)
		{
			this.animator.enabled = enabled;
		}
	}

	
	private void Awake()
	{
		mutantAI_net component = base.transform.GetComponent<mutantAI_net>();
		if (component && component.creepy_boss)
		{
			base.enabled = false;
			return;
		}
		if (base.entity && base.entity.isAttached)
		{
			CoopAnimal componentInChildren = base.transform.GetComponentInChildren<CoopAnimal>();
			if (componentInChildren && componentInChildren.birdType)
			{
				UnityEngine.Object.Destroy(this);
			}
			foreach (Renderer item in base.entity.GetComponentsInChildren<Renderer>())
			{
				this.Renderers.Add(item);
			}
			foreach (Collider item2 in base.entity.GetComponentsInChildren<Collider>())
			{
				this.Colliders.Add(item2);
			}
			if (!this.animator)
			{
				this.animator = base.entity.GetComponentInChildren<Animator>();
			}
			if (base.entity.isFrozen)
			{
				this.DisableComponents();
			}
			else
			{
				this.EnableComponents();
			}
		}
	}

	
	public List<Renderer> Renderers = new List<Renderer>();

	
	public List<Collider> Colliders = new List<Collider>();

	
	public Animator animator;

	
	public bool ignoreAnimator;
}
