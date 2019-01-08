using System;
using System.Collections;
using UnityEngine;

public class setupCreepyFire : MonoBehaviour
{
	private void Start()
	{
	}

	private void OnDisable()
	{
		this.disableAllFire();
	}

	private void enableBurntSkin()
	{
		if (this.burntMat && this.skin)
		{
			this.skin.material = this.burntMat;
		}
	}

	private void enableFire()
	{
		foreach (GameObject gameObject in this.fire)
		{
			if (gameObject)
			{
				gameObject.SetActive(true);
				base.StartCoroutine("disableFire", gameObject);
			}
		}
	}

	private void disableAllFire()
	{
		foreach (GameObject gameObject in this.fire)
		{
			if (gameObject)
			{
				gameObject.SetActive(false);
			}
		}
	}

	private IEnumerator disableFire(GameObject go)
	{
		yield return new WaitForSeconds(UnityEngine.Random.Range(7f, 12f));
		if (go)
		{
			go.SetActive(false);
		}
		yield break;
	}

	public GameObject[] fire;

	public Material burntMat;

	public SkinnedMeshRenderer skin;
}
