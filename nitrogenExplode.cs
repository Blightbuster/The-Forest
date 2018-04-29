using System;
using Bolt;
using UnityEngine;


public class nitrogenExplode : MonoBehaviour
{
	
	private void Start()
	{
		this.rb = base.transform.parent.GetComponent<Rigidbody>();
	}

	
	private void OnTriggerEnter(Collider other)
	{
		ArrowDamage component = other.GetComponent<ArrowDamage>();
		if (component || other.gameObject.CompareTag("Weapon"))
		{
			this.HitNitrogen(10);
		}
	}

	
	private void HitNitrogen(int Damage)
	{
		if (!this.nitrogenActive)
		{
			this.enableNitrogenExplosion();
		}
	}

	
	private void Explosion()
	{
		this.HitNitrogen(10);
	}

	
	private void Burn()
	{
		this.HitNitrogen(10);
	}

	
	public void enableNitrogenExplosion()
	{
		if (this.nitrogenActive)
		{
			return;
		}
		this.nitrogenActive = true;
		if (BoltNetwork.isRunning && BoltNetwork.isClient)
		{
			BoltEntity component = this.parentGo.GetComponent<BoltEntity>();
			if (component && component.isAttached)
			{
				SendMessageEvent sendMessageEvent = SendMessageEvent.Create(GlobalTargets.OnlyServer);
				sendMessageEvent.Target = component;
				sendMessageEvent.Message = "enableNitrogenExplosion";
				sendMessageEvent.Send();
			}
		}
		this.flameGo.SetActive(true);
		if (!BoltNetwork.isClient)
		{
			Rigidbody component2 = this.parentGo.GetComponent<Rigidbody>();
			if (component2)
			{
				component2.isKinematic = false;
				component2.useGravity = true;
			}
			this.spawnedBomb = UnityEngine.Object.Instantiate<GameObject>(this.ExplodeGo, base.transform.position, base.transform.rotation);
			this.spawnedBomb.SendMessage("setWaitTime", this.waitTime, SendMessageOptions.DontRequireReceiver);
			base.Invoke("CleanUp", (float)this.waitTime);
		}
		this.nitrogenActive = true;
	}

	
	private void CleanUp()
	{
		if (BoltNetwork.isRunning && BoltNetwork.isServer)
		{
			BoltEntity componentInParent = base.transform.GetComponentInParent<BoltEntity>();
			if (componentInParent && componentInParent.isAttached && componentInParent.IsOwner())
			{
				UnityEngine.Object.Destroy(componentInParent.gameObject);
			}
		}
		else
		{
			UnityEngine.Object.Destroy(this.parentGo);
		}
	}

	
	private void Update()
	{
		if (this.spawnedBomb)
		{
			this.spawnedBomb.transform.position = base.transform.position;
			if (this.rb)
			{
				Vector3 force = (-base.transform.right + -base.transform.up) * 70f;
				this.rb.AddForceAtPosition(force, base.transform.position, ForceMode.Force);
			}
		}
	}

	
	public GameObject parentGo;

	
	public GameObject ExplodeGo;

	
	public GameObject flameGo;

	
	private GameObject spawnedBomb;

	
	private Rigidbody rb;

	
	private bool nitrogenActive;

	
	public int waitTime = 4;
}
