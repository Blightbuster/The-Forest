﻿using System;
using UnityEngine;

public class ISeeDeadPeople : MonoBehaviour
{
	private void SawBody()
	{
		ISeeDeadPeople.DeadPeople++;
		base.CancelInvoke("TurnOffMissionText");
		this.MissionText.SetActive(true);
		base.GetComponent<AudioSource>().Play();
		base.Invoke("TurnOffMissionText", 5f);
	}

	private void TurnOffMissionText()
	{
		this.MissionText.SetActive(false);
	}

	public GameObject MissionText;

	public static int DeadPeople;
}
