﻿using System;
using UnityEngine;

public class LoadLevel_Flamethrower : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	public void NextLevelButton(string levelName)
	{
		Application.LoadLevel("Ground_Explosion");
	}

	public void OnGUI()
	{
		if (GUI.Button(new Rect(70f, 70f, 100f, 30f), "Next Effects"))
		{
			Application.LoadLevel("Ground_Explosion");
		}
	}
}
