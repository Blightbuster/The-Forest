using System;
using System.Collections;
using UnityEngine;

public class setupDummyFire : MonoBehaviour
{
	private void Start()
	{
	}

	private void OnDisable()
	{
		this.disableFire();
	}

	private void startDummyFire()
	{
		foreach (GameObject gameObject in this.fire)
		{
			if (gameObject)
			{
				gameObject.SetActive(true);
				base.StartCoroutine("burnDown", gameObject.transform);
			}
		}
	}

	private IEnumerator burnDown(Transform tr)
	{
		yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 3f));
		Vector3 startScale = tr.localScale;
		Vector3 setScale = Vector3.zero;
		float t = 0f;
		while (t < 1f * this.rate)
		{
			t += Time.deltaTime * this.rate;
			setScale = Vector3.Lerp(startScale, Vector3.zero, t);
			tr.localScale = setScale;
			yield return null;
		}
		tr.gameObject.SetActive(false);
		yield break;
	}

	private void disableFire()
	{
		foreach (GameObject gameObject in this.fire)
		{
			if (gameObject)
			{
				gameObject.SetActive(false);
			}
		}
	}

	public GameObject[] fire;

	public float rate = 0.2f;
}
