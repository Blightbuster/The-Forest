using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CaveZone : MonoBehaviour
{
	private void Awake()
	{
		this._rigidbody = base.GetComponent<Rigidbody>();
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (collider.attachedRigidbody == this._rigidbody)
		{
			return;
		}
		if (!this.colliderTracker.ContainsKey(collider))
		{
			this.colliderTracker.Add(collider, 1);
		}
		else
		{
			Dictionary<Collider, int> dictionary;
			(dictionary = this.colliderTracker)[collider] = dictionary[collider] + 1;
		}
		if (this.colliderTracker[collider] == 1)
		{
			foreach (TerrainCollider collider2 in this.terrainColliders)
			{
				Physics.IgnoreCollision(collider2, collider, true);
			}
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (collider.attachedRigidbody == this._rigidbody)
		{
			return;
		}
		if (!this.colliderTracker.ContainsKey(collider))
		{
			return;
		}
		Dictionary<Collider, int> dictionary;
		(dictionary = this.colliderTracker)[collider] = dictionary[collider] - 1;
		if (this.colliderTracker[collider] <= 0)
		{
			foreach (TerrainCollider collider2 in this.terrainColliders)
			{
				Physics.IgnoreCollision(collider2, collider, false);
			}
			this.colliderTracker.Remove(collider);
		}
	}

	public TerrainCollider[] terrainColliders;

	private Dictionary<Collider, int> colliderTracker = new Dictionary<Collider, int>();

	private Rigidbody _rigidbody;
}
