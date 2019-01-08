using System;
using TheForest.Utils;
using UnityEngine;

public class mikeHelpers : MonoBehaviour
{
	private void Start()
	{
		this.clock = GameObject.FindWithTag("Clock").GetComponent<Clock>();
		this.player = GameObject.FindWithTag("Player");
		if (this.walkman)
		{
			this.walkman.SetActive(false);
		}
	}

	private void Update()
	{
		if (TheForest.Utils.Input.GetButtonDown("killAllEnemy"))
		{
			foreach (GameObject gameObject in Scene.MutantControler.activeWorldCannibals)
			{
				if (gameObject)
				{
					gameObject.SendMessage("killThisEnemy", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		if (TheForest.Utils.Input.GetButtonDown("AltFire"))
		{
			Debug.Log("doing advance day");
			if (!Clock.Dark)
			{
				Scene.Atmosphere.TimeOfDay = 155f;
			}
			else
			{
				Scene.Atmosphere.TimeOfDay = 269f;
			}
			base.Invoke("checkDay", 1f);
		}
	}

	private void checkDay()
	{
		Debug.Log("current day = " + Clock.Day);
	}

	public GameObject player;

	public Clock clock;

	public GameObject walkman;
}
