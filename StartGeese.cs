using System;
using UnityEngine;


public class StartGeese : MonoBehaviour
{
	
	private void Start()
	{
		this.emitter = base.GetComponent<FMOD_StudioEventEmitter>();
	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Grabber"))
		{
			this.emitter.Play();
			foreach (GameObject gameObject in this.geese)
			{
				gameObject.SendMessage("Fly", SendMessageOptions.DontRequireReceiver);
			}
			base.Invoke("resetGeese", 600f);
			base.gameObject.GetComponent<Collider>().enabled = false;
		}
	}

	
	private void resetGeese()
	{
		foreach (GameObject gameObject in this.geese)
		{
			gameObject.SetActive(true);
			base.gameObject.GetComponent<Collider>().enabled = true;
		}
	}

	
	public GameObject[] geese;

	
	private FMOD_StudioEventEmitter emitter;
}
