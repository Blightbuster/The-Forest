using System;
using UnityEngine;


public class enableRandomGo : MonoBehaviour
{
	
	private void Awake()
	{
		this.col = base.GetComponentInChildren<Collider>();
	}

	
	private void OnDeserialized()
	{
		this.DoRandomGo();
	}

	
	private void Start()
	{
		this.DoRandomGo();
	}

	
	private void OnEnable()
	{
		this.DoRandomGo();
	}

	
	private void DoRandomGo()
	{
		foreach (GameObject gameObject in this.goList)
		{
			if (gameObject)
			{
				gameObject.SetActive(false);
			}
		}
		if (!this.hideBodyMeshes)
		{
			this.goList[UnityEngine.Random.Range(0, this.goList.Length)].SetActive(true);
		}
	}

	
	public void hideAllGo()
	{
		this.col.enabled = false;
		foreach (GameObject gameObject in this.goList)
		{
			gameObject.SetActive(false);
		}
	}

	
	public bool hideBodyMeshes;

	
	public GameObject[] goList;

	
	private Collider col;
}
