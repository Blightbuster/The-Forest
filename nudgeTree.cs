using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;


public class nudgeTree : MonoBehaviour
{
	
	private void Start()
	{
		this.rb = base.transform.GetComponent<Rigidbody>();
		Vector3 centerOfMass = this.rb.centerOfMass;
		centerOfMass.y += 1f;
		this.rb.centerOfMass = centerOfMass;
	}

	
	private void OnEnable()
	{
		base.StartCoroutine(this.fixRigidBodyCollision());
	}

	
	private void addRandomForce()
	{
		Vector3 position = base.transform.position;
		Vector2 vector = this.Circle(this.nudgeForce);
		Vector3 a = new Vector3(vector.x, 0f, vector.y);
		position.y += 15f;
		this.rb.AddForceAtPosition(a * (0.016666f / Time.fixedDeltaTime), position);
	}

	
	private void addPerpForce()
	{
		Vector3 a = LocalPlayer.Transform.forward * this.nudgeForce;
		if (BoltNetwork.isRunning)
		{
			GameObject gameObject = null;
			float num = float.PositiveInfinity;
			foreach (GameObject gameObject2 in Scene.SceneTracker.allPlayers)
			{
				if (gameObject2)
				{
					float sqrMagnitude = (base.transform.position - gameObject2.transform.position).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						gameObject = gameObject2;
						num = sqrMagnitude;
					}
				}
			}
			if (gameObject)
			{
				a = gameObject.transform.forward * this.nudgeForce;
			}
		}
		Vector3 position = base.transform.position;
		position.y += 15f;
		this.rb.AddForceAtPosition(a * (0.016666f / Time.fixedDeltaTime), position);
	}

	
	private Vector2 Circle(float radius)
	{
		Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
		insideUnitCircle.Normalize();
		return insideUnitCircle * radius;
	}

	
	private IEnumerator fixRigidBodyCollision()
	{
		if (!this.rb)
		{
			this.rb = base.transform.GetComponent<Rigidbody>();
		}
		if (!this.rb)
		{
			yield break;
		}
		float storeDrag = this.rb.drag;
		float storeAngDrag = this.rb.angularDrag;
		this.rb.drag = 25f;
		this.rb.angularDrag = 25f;
		yield return YieldPresets.WaitPointOneSeconds;
		this.rb.drag = storeDrag;
		this.rb.angularDrag = storeAngDrag;
		this.addPerpForce();
		yield break;
	}

	
	private void OnDestroy()
	{
		this.rb = null;
	}

	
	private Rigidbody rb;

	
	public float nudgeForce;
}
