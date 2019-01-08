using System;
using UnityEngine;

public class DisableImpact : MonoBehaviour
{
	private void Awake()
	{
		this.body = base.gameObject.GetComponent<Rigidbody>();
		if (this.body == null)
		{
			Debug.LogError("No Rigidbody at " + base.gameObject.name);
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void disableImpact(float interval, bool multiplayer = false)
	{
		if (this.body == null)
		{
			this.body = base.gameObject.GetComponent<Rigidbody>();
		}
		if (this.body != null)
		{
			float time = (!multiplayer) ? interval : (1.5f * interval);
			this.body.isKinematic = true;
			base.Invoke("enableImpact", time);
		}
		else
		{
			Debug.LogError("No Rigidbody at " + base.gameObject.name);
		}
	}

	public void enableImpact()
	{
		this.body.isKinematic = false;
		UnityEngine.Object.Destroy(this);
	}

	private Rigidbody body;
}
