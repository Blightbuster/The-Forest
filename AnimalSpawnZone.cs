﻿using System;
using System.Collections.Generic;
using TheForest.Utils.WorkSchedulerInterfaces;
using UnityEngine;


public class AnimalSpawnZone : MonoBehaviour, IThreadSafeTask
{
	
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1f, 0.498039216f, 0.152941182f, 0.5f);
		Gizmos.DrawSphere(base.transform.position, this.ZoneRadius);
		Gizmos.color = Color.white;
	}

	
	private void Awake()
	{
		if (this.WsToken == -1)
		{
			this.WsToken = WorkScheduler.Register(this, base.transform.position, true);
		}
	}

	
	private void OnEnable()
	{
		if (this.WsToken == -1)
		{
			this.WsToken = WorkScheduler.Register(this, base.transform.position, false);
		}
	}

	
	private void OnDisable()
	{
		if (this.WsToken != -1)
		{
			WorkScheduler.Unregister(this, this.WsToken);
			this.WsToken = -1;
		}
		this.SpawnedAnimals.Clear();
	}

	
	
	
	public bool ShouldDoMainThreadRefresh { get; set; }

	
	public void ThreadedRefresh()
	{
		this.ShouldDoMainThreadRefresh = (this.AddSpawnBackTimes.Count > 0 && this.AddSpawnBackTimes.Peek() < WorkScheduler.CurrentTime);
	}

	
	public void MainThreadRefresh()
	{
		while (this.AddSpawnBackTimes.Count > 0 && this.AddSpawnBackTimes.Peek() < Time.time)
		{
			this.AddSpawnBackTimes.Dequeue();
			this.MaxAnimalsTotal++;
		}
	}

	
	[HideInInspector]
	public float DelayUntil;

	
	[SerializeField]
	public bool snowType;

	
	[SerializeField]
	public float ZoneRadius = 128f;

	
	[HideInInspector]
	public float TotalWeight;

	
	[SerializeField]
	public float DelayAfterKillTime = 300f;

	
	[HideInInspector]
	public int TotalAnimalsAlive;

	
	[SerializeField]
	public int MaxAnimalsTotal = 32;

	
	[SerializeField]
	public int MaxAnimalsPerUpdate = 2;

	
	[SerializeField]
	public AnimalSpawnConfig[] Spawns = new AnimalSpawnConfig[0];

	
	[SerializeField]
	public List<GameObject> SpawnedAnimals = new List<GameObject>();

	
	[HideInInspector]
	public Queue<float> AddSpawnBackTimes = new Queue<float>();

	
	private int WsToken = -1;
}
