using System;
using UnityEngine;

public class setupBodyDamage : MonoBehaviour
{
	private void Start()
	{
		this.setupBody();
	}

	private void setupBody()
	{
		if (this.bloodyMat.Length > 0 && (double)UnityEngine.Random.value < 0.65)
		{
			this.SetSkin(this.bloodyMat[UnityEngine.Random.Range(0, this.bloodyMat.Length)]);
		}
		else
		{
			this.SetSkin(this.defaultMat[UnityEngine.Random.Range(0, this.defaultMat.Length)]);
		}
		if (this.burntMat && UnityEngine.Random.value < 0.1f)
		{
			this.SetSkin(this.burntMat);
		}
		if (UnityEngine.Random.value < 0.5f)
		{
			int num = UnityEngine.Random.Range(0, this.choppedParts.Length);
			this.choppedParts[num].SetActive(false);
			this.goreParts[num].SetActive(true);
			this.damageTriggers[num].SetActive(false);
		}
		if (UnityEngine.Random.value < 0.5f)
		{
			int num2 = UnityEngine.Random.Range(0, this.choppedParts.Length);
			this.choppedParts[num2].SetActive(false);
			this.goreParts[num2].SetActive(true);
			this.damageTriggers[num2].SetActive(false);
		}
		if (UnityEngine.Random.value < 0.75f)
		{
			this.necklace[0].SetActive(false);
		}
		else
		{
			this.necklace[0].SetActive(true);
		}
	}

	public void SetSkin(Material mat)
	{
		if (this.MyBodyParts.Length > 0)
		{
			foreach (GameObject gameObject in this.MyBodyParts)
			{
				gameObject.GetComponent<Renderer>().material = mat;
			}
		}
		if (this.MySkinnyParts.Length > 0)
		{
			foreach (GameObject gameObject2 in this.MySkinnyParts)
			{
				gameObject2.GetComponent<Renderer>().material = mat;
			}
		}
	}

	public Material[] defaultMat;

	public Material[] bloodyMat;

	public Material burntMat;

	public GameObject[] MyBodyParts;

	public GameObject[] MySkinnyParts;

	public GameObject[] choppedParts;

	public GameObject[] goreParts;

	public GameObject[] damageTriggers;

	public GameObject[] necklace;
}
