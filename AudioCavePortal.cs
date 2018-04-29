using System;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;


public class AudioCavePortal : MonoBehaviour
{
	
	private void Start()
	{
		PlayerStats.AddEndgameCavePortal(base.gameObject);
		this.collider = base.GetComponent<Collider>();
		this.playerColliders = new List<Collider>();
		base.InvokeRepeating("ValidateColliders", 1f, 1f);
	}

	
	private void OnDisable()
	{
		PlayerStats.RemoveEndgameCavePortal(base.gameObject);
	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			LocalPlayer.Stats.EnableCaveAudio();
			if (!this.playerColliders.Contains(other))
			{
				this.playerColliders.Add(other);
			}
		}
	}

	
	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			this.playerColliders.Remove(other);
			this.PostColliderRemovalCheck(other);
		}
	}

	
	private void ValidateColliders()
	{
		if (this.playerColliders.Count > 0)
		{
			Collider collider = null;
			int i = 0;
			while (i < this.playerColliders.Count)
			{
				Collider collider2 = this.playerColliders[i];
				if (collider2 == null)
				{
					this.playerColliders.RemoveAt(i);
				}
				else if (!collider2.bounds.Intersects(this.collider.bounds))
				{
					collider = collider2;
					this.playerColliders.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
			if (collider != null)
			{
				this.PostColliderRemovalCheck(collider);
			}
		}
	}

	
	private void PostColliderRemovalCheck(Collider colliderRemoved)
	{
		if (this.playerColliders.Count == 0)
		{
			float z = base.transform.InverseTransformPoint(colliderRemoved.transform.position).z;
			if (z < 0f)
			{
				LocalPlayer.Stats.DisableCaveAudio();
			}
		}
	}

	
	private Collider collider;

	
	private List<Collider> playerColliders;
}
