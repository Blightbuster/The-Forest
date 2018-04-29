using System;
using System.Collections;
using TheForest.Utils.Physics;
using TheForest.World;
using UnityEngine;


public class CoopLogStopper : MonoBehaviour, IOnCollisionEnterProxy, IOnCollisionExitProxy
{
	
	private void OnSpawned()
	{
		this.OnEnable();
	}

	
	private void OnEnable()
	{
		if (BoltNetwork.isClient)
		{
			this.rb.isKinematic = true;
			this.rb.useGravity = false;
			this.rbCollider.radius = 0.35f;
			this.rbCollider.height = 4.5f;
			this.rb = base.transform.GetComponent<Rigidbody>();
			if (this.rb)
			{
				UnityEngine.Object.Destroy(this.rb);
			}
			UnityEngine.Object.Destroy(base.GetComponent<Buoyancy>());
			UnityEngine.Object.Destroy(base.GetComponent<EnableInWaterProxy>());
			base.StartCoroutine(this.fixClientCollision());
		}
		else
		{
			this.rb.isKinematic = false;
			this.stopping = false;
		}
		if (!BoltNetwork.isClient)
		{
			base.StartCoroutine(this.fixHostCollision());
		}
	}

	
	public void OnCollisionEnterProxied(Collision collision)
	{
		if (BoltNetwork.isServer && !this.stopping && collision.transform.GetComponentInChildren<Terrain>())
		{
			base.StartCoroutine(this.Stop(3));
		}
	}

	
	public void OnCollisionExitProxied(Collision collision)
	{
		if (BoltNetwork.isServer && !this.stopping && collision.transform.GetComponentInChildren<Terrain>())
		{
			Vector3 velocity = this.rb.velocity;
			if (velocity.y > 0f)
			{
				velocity.y = 0f;
			}
			this.rb.velocity = velocity;
		}
	}

	
	private IEnumerator Stop(int time)
	{
		this.stopping = true;
		yield return YieldPresets.WaitOneSecond;
		while (base.transform.position.y - Terrain.activeTerrain.SampleHeight(base.transform.position) > 1f)
		{
			yield return YieldPresets.WaitOneSecond;
		}
		this.rb.velocity = Vector3.zero;
		this.rb.angularVelocity = Vector3.zero;
		this.rb.isKinematic = true;
		yield break;
	}

	
	private IEnumerator fixClientCollision()
	{
		this.rbCollider.radius = 0.01f;
		this.rbCollider.height = 0.01f;
		yield return YieldPresets.WaitOneSecond;
		if (this.rbCollider)
		{
			this.rbCollider.radius = 0.35f;
			this.rbCollider.height = 4.5f;
		}
		yield break;
	}

	
	private IEnumerator fixHostCollision()
	{
		float d = this.rb.drag;
		float ad = this.rb.angularDrag;
		this.rb.drag = 35f;
		this.rb.angularDrag = 35f;
		yield return YieldPresets.WaitPointTwoFiveSeconds;
		if (this.rb)
		{
			this.rb.drag = d;
			this.rb.angularDrag = ad;
		}
		yield break;
	}

	
	[SerializeField]
	private Rigidbody rb;

	
	[SerializeField]
	private CapsuleCollider rbCollider;

	
	private bool stopping;
}
