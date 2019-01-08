using System;
using System.Collections;
using UnityEngine;

public class soundDetect : MonoBehaviour
{
	private void Start()
	{
		this.breakBool = false;
	}

	private void OnCollisionEnter(Collision other)
	{
		if ((other.gameObject.CompareTag("Tree") || other.gameObject.CompareTag("TerrainMain") || other.gameObject.layer == 17 || other.gameObject.layer == 20 || other.gameObject.layer == 25 || other.gameObject.layer == 26) && !this.breakBool && base.GetComponent<Rigidbody>().velocity.magnitude > 2f)
		{
			base.StartCoroutine("setupSoundEvent");
			this.breakBool = true;
			base.Invoke("resetBreakBool", 2f);
		}
	}

	private IEnumerator setupSoundEvent()
	{
		this.spawnedSound = UnityEngine.Object.Instantiate<Transform>(this.soundDetectGo.transform, base.transform.position, base.transform.rotation);
		this.soundCollider = this.spawnedSound.GetComponent<SphereCollider>();
		this.spawnedSound.SendMessage("setRange", this.soundRange, SendMessageOptions.DontRequireReceiver);
		if (this.followParent)
		{
			this.spawnedSound.parent = base.transform;
		}
		base.Invoke("removeGo", this.lifetime);
		if (this.pulse)
		{
			base.InvokeRepeating("pulseCollider", 1f, this.pulseRate);
		}
		yield return null;
		yield break;
	}

	private void removeGo()
	{
		if (this.spawnedSound)
		{
			UnityEngine.Object.Destroy(this.spawnedSound.gameObject);
		}
	}

	private void pulseCollider()
	{
		if (!this.soundCollider)
		{
			return;
		}
		if (this.soundCollider.enabled)
		{
			this.soundCollider.enabled = false;
		}
		else
		{
			this.soundCollider.enabled = true;
		}
	}

	private void resetBreakBool()
	{
		this.breakBool = false;
	}

	private void OnDisable()
	{
		if (this.spawnedSound)
		{
			UnityEngine.Object.Destroy(this.spawnedSound.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (this.spawnedSound)
		{
			UnityEngine.Object.Destroy(this.spawnedSound.gameObject);
		}
	}

	public GameObject soundDetectGo;

	public float soundRange;

	public float lifetime;

	public bool pulse;

	public float pulseRate;

	public bool followParent;

	private Transform spawnedSound;

	private SphereCollider soundCollider;

	private bool breakBool;
}
