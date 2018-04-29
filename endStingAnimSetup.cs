using System;
using UnityEngine;


public class endStingAnimSetup : MonoBehaviour
{
	
	private void Start()
	{
		this.heldBottleGo.SetActive(false);
		this.heldToothBrush.SetActive(false);
		this.placedBottleGo.SetActive(true);
		this.placedToothBrush.SetActive(true);
		this.spitRenderer.enabled = false;
	}

	
	private void switchToothBrush()
	{
		this.heldToothBrush.SetActive(!this.heldToothBrush.activeSelf);
		this.placedToothBrush.SetActive(!this.placedToothBrush.activeSelf);
		Debug.Log("switching toothBrush");
	}

	
	private void switchBottle()
	{
		this.heldBottleGo.SetActive(!this.heldBottleGo.activeSelf);
		this.placedBottleGo.SetActive(!this.placedBottleGo.activeSelf);
	}

	
	private void dropBottle()
	{
		this.heldBottleGo.transform.parent = null;
	}

	
	private void enableSpit()
	{
		this.spitRenderer.enabled = true;
	}

	
	public GameObject heldBottleGo;

	
	public GameObject placedBottleGo;

	
	public GameObject heldToothBrush;

	
	public GameObject placedToothBrush;

	
	public Renderer spitRenderer;
}
