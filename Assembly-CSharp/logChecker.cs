using System;
using System.Collections;
using TheForest.Utils;
using TheForest.Utils.Physics;
using UnityEngine;

public class logChecker : MonoBehaviour, IOnCollisionEnterProxy, IOnCollisionExitProxy
{
	private void Start()
	{
		this.rb = base.GetComponent<Rigidbody>();
		this.grounded = false;
		this.rb.angularDrag = 0.05f;
		this.rb.drag = 0.05f;
	}

	private void enableSpawnedfromTree()
	{
		base.StartCoroutine(this.setDrag());
	}

	private void OnEnable()
	{
		if (this.ignoringTerrainCollision)
		{
			Physics.IgnoreCollision(base.GetComponent<Collider>(), Terrain.activeTerrain.GetComponent<Collider>(), false);
			this.ignoringTerrainCollision = false;
		}
	}

	private void OnDisable()
	{
		base.StopAllCoroutines();
		if (this.rb)
		{
			this.rb.drag = 0.05f;
			this.rb.angularDrag = 0.05f;
		}
	}

	private IEnumerator setDrag()
	{
		if (!this.rb)
		{
			this.rb = base.transform.GetComponent<Rigidbody>();
		}
		if (this.rb)
		{
			this.rb.drag = 20f;
			float timer = Time.time + 0.4f;
			while (Time.time < timer)
			{
				this.rb.drag = 15f;
				this.rb.angularDrag = 15f;
				yield return null;
			}
			this.rb.drag = 0.05f;
		}
		yield break;
	}

	public void OnCollisionEnterProxied(Collision col)
	{
		if (col == null)
		{
			return;
		}
		if (this.rb == null)
		{
			return;
		}
		if (col.gameObject == null)
		{
			return;
		}
		if (col.gameObject.CompareTag("Player"))
		{
			this.grounded = false;
			this.rb.drag = 0.05f;
			this.rb.angularDrag = 0.05f;
		}
		else if (col.gameObject.CompareTag("TerrainMain"))
		{
			if (Scene.IsInSinkhole(base.transform.position))
			{
				this.ignoringTerrainCollision = true;
				Physics.IgnoreCollision(base.GetComponent<Collider>(), col.collider, true);
			}
			else
			{
				this.grounded = true;
				this.rb.drag = 0.05f;
				this.rb.angularDrag = 8f;
			}
		}
	}

	public void OnCollisionExitProxied(Collision col)
	{
		if (col == null)
		{
			return;
		}
		if (col.gameObject == null)
		{
			return;
		}
		if (this.rb == null)
		{
			return;
		}
		if (col.gameObject.CompareTag("Player"))
		{
			this.grounded = false;
			this.rb.drag = 0.05f;
			this.rb.angularDrag = 8f;
		}
		if (col.gameObject.CompareTag("TerrainMain"))
		{
			this.rb.drag = 0.05f;
			this.rb.angularDrag = 0.05f;
			this.grounded = false;
		}
	}

	public bool grounded;

	private Rigidbody rb;

	private bool ignoringTerrainCollision;
}
