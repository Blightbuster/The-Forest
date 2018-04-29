using System;
using TheForest.Utils;
using TheForest.Utils.WorkSchedulerInterfaces;
using UnityEngine;


public class LOD_SimpleToggleGO : MonoBehaviour, IThreadSafeTask
{
	
	private void Awake()
	{
	}

	
	private void Start()
	{
		this.RefreshVisibility(true);
	}

	
	private void OnDisable()
	{
		try
		{
			WorkScheduler.Unregister(this, this.wsToken);
			this.wsToken = -1;
		}
		catch
		{
		}
	}

	
	private void OnEnable()
	{
		if (!LevelSerializer.IsDeserializing)
		{
			this.WSRegistration();
		}
		else
		{
			base.Invoke("WSRegistration", 0.005f);
		}
	}

	
	private void WSRegistration()
	{
		this.position = base.transform.position;
		if (this.wsToken != -1)
		{
			WorkScheduler.Unregister(this, this.wsToken);
		}
		this.wsToken = WorkScheduler.Register(this, base.transform.position, false);
		this.RefreshVisibility(true);
	}

	
	private void RefreshVisibility(bool force)
	{
		bool flag = this.nextVisibility;
		if (flag != this.currentVisibility || force)
		{
			this.GO.SetActive(flag);
			this.currentVisibility = flag;
		}
	}

	
	
	
	public bool ShouldDoMainThreadRefresh { get; set; }

	
	public void ThreadedRefresh()
	{
		if (LocalPlayer.Transform)
		{
			this.position.y = PlayerCamLocation.PlayerLoc.y;
			float num = (this.position - PlayerCamLocation.PlayerLoc).sqrMagnitude * ((!this.currentVisibility) ? 1f : 0.9f);
			this.nextVisibility = (num < this.VisibleDistance * this.VisibleDistance);
		}
		this.ShouldDoMainThreadRefresh = (this.nextVisibility != this.currentVisibility);
	}

	
	public void MainThreadRefresh()
	{
		this.RefreshVisibility(false);
	}

	
	public GameObject GO;

	
	public float VisibleDistance = 75f;

	
	private int wsToken = -1;

	
	private bool nextVisibility = true;

	
	private bool currentVisibility = true;

	
	private Vector3 position;
}
