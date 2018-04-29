using System;
using UnityEngine;


public class TurnOffTutorial : MonoBehaviour
{
	
	private void OnEnable()
	{
		this.IndexPage = GameObject.FindGameObjectWithTag("Index");
		if (this.IndexPage == null || !this.IndexPage.activeSelf)
		{
			base.gameObject.SetActive(false);
		}
	}

	
	private void Update()
	{
		if (this.IndexPage == null || !this.IndexPage.activeSelf)
		{
			base.gameObject.SetActive(false);
		}
	}

	
	public GameObject IndexPage;
}
