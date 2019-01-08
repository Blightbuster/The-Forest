using System;
using UnityEngine;

public class FeatherPhysics : MonoBehaviour
{
	private void Start()
	{
		this.torque.x = (float)UnityEngine.Random.Range(-200, 200);
		this.torque.y = (float)UnityEngine.Random.Range(-200, 200);
		this.torque.z = (float)UnityEngine.Random.Range(-200, 200);
		base.GetComponent<ConstantForce>().torque = this.torque;
		base.GetComponent<Rigidbody>().AddForce(Vector3.up * 2.5f, ForceMode.Impulse);
		if (this.unparent)
		{
			base.transform.parent = null;
		}
		base.Invoke("FloatDown", 2f);
		base.Invoke("CleanUp", 30f);
	}

	private void FloatDown()
	{
		this.torque = new Vector3(0f, 0f, 0f);
		base.GetComponent<ConstantForce>().torque = this.torque;
		base.GetComponent<Rigidbody>().AddForce(Vector3.down * 3f, ForceMode.Impulse);
	}

	private void CleanUp()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Renderer component = base.GetComponent<Renderer>();
		if (!component || !component.IsVisibleFrom(Camera.main))
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			base.Invoke("CleanUp", 4f);
		}
	}

	private Vector3 torque;

	public bool unparent;
}
