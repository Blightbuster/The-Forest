using System;
using PathologicalGames;
using TheForest.Utils;
using TheForest.Utils.WorkSchedulerInterfaces;
using UnityEngine;

public abstract class LOD_Base : MonoBehaviour, IThreadSafeTask
{
	public abstract LOD_Settings LodSettings { get; }

	public abstract SpawnPool Pool { get; }

	public virtual bool DestroyInsteadOfDisable
	{
		get
		{
			return false;
		}
	}

	public virtual bool UseLow
	{
		get
		{
			return true;
		}
	}

	public int CurrentLOD
	{
		get
		{
			return this.currentLOD;
		}
	}

	private void Awake()
	{
		this.maxDrawDistanceSetting = this.LodSettings.GetNewObjectMaxDrawDistance;
	}

	protected virtual void OnEnable()
	{
		this.CurrentLodTransform = null;
		this._position = base.transform.position;
		if (this.cb == null)
		{
			if (this.billboard != null)
			{
				this.cb = this.billboard.GetComponent<CustomBillboard>();
			}
			if (this.BillboardId == -1 && this.cb != null)
			{
				this.BillboardId = this.cb.Register(this._position, base.transform.eulerAngles.y);
			}
		}
		else
		{
			this.cb.SetAlive(this.BillboardId, true);
		}
		this.wsToken = WorkScheduler.Register(this, this._position, this.canForceWorkScheduler);
		this.canForceWorkScheduler = false;
	}

	protected virtual void OnDisable()
	{
		if (!MenuMain.exitingToMenu)
		{
			WorkScheduler.Unregister(this, this.wsToken);
			this.canForceWorkScheduler = true;
			if (this.CurrentLodTransform)
			{
				if (this.lodWasDestroyed && this.Pool)
				{
					this.Pool.KillInstance(this.CurrentLodTransform);
				}
				else
				{
					this.DespawnCurrent();
				}
			}
			this.lodWasDestroyed = false;
			this.CurrentLodTransform = null;
			this.isSpawned = false;
			this.currentLOD = -1;
			if (this.BillboardId >= 0 && this.cb != null)
			{
				this.cb.SetAlive(this.BillboardId, false);
			}
			if (!base.gameObject.activeSelf)
			{
				base.enabled = true;
			}
		}
	}

	public void DespawnCurrent()
	{
		if (this.CurrentLodTransform && this.CurrentLodTransform.gameObject.activeSelf && !this.stats.StopPooling && Scene.ActiveMB)
		{
			LOD_Stats.Current = this.stats;
			if (this.Pool)
			{
				this.Pool.Despawn(this.CurrentLodTransform);
			}
			else
			{
				UnityEngine.Object.Destroy(this.CurrentLodTransform.gameObject);
			}
			LOD_Stats.Current = null;
			this.CurrentLodTransform = null;
			this.isSpawned = false;
		}
	}

	private int GetLOD()
	{
		return this.LodSettings.GetLOD(this._position, this.currentLOD);
	}

	public virtual void SetLOD(int lod)
	{
		if (this.stats.StopPooling)
		{
			return;
		}
		this.DespawnCurrent();
		if (this.DontSpawn)
		{
			return;
		}
		Transform transform = null;
		if (lod != 0)
		{
			if (lod != 1)
			{
				if (lod == 2)
				{
					if (this.UseLow)
					{
						transform = this.Low;
					}
				}
			}
			else
			{
				transform = this.Mid;
			}
		}
		else
		{
			transform = this.High;
		}
		if (transform != null)
		{
			LOD_Stats.Current = this.stats;
			if (this.Pool)
			{
				this.CurrentLodTransform = this.Pool.Spawn(transform, this._position, base.transform.rotation);
			}
			else
			{
				this.CurrentLodTransform = UnityEngine.Object.Instantiate<Transform>(transform, this._position, base.transform.rotation);
			}
			LOD_Stats.Current = null;
			if (this.CurrentLodTransform)
			{
				this.CurrentLodTransform.SendMessage("SetLodBase", this, SendMessageOptions.DontRequireReceiver);
				this.isSpawned = true;
			}
		}
		this.currentLOD = lod;
		base.SendMessage("LodChanged", this.currentLOD, SendMessageOptions.DontRequireReceiver);
	}

	public void RefreshLODs()
	{
		if (this.DontSpawn || this.maxDrawDistanceSetting < TheForestQualitySettings.UserSettings.DrawDistance)
		{
			this.DespawnCurrent();
			if (this.currentLOD != -2)
			{
				if (this.BillboardId >= 0 && this.cb != null)
				{
					this.cb.SetAlive(this.BillboardId, false);
				}
				this.currentLOD = -2;
			}
			return;
		}
		if (this.currentLOD == -2 && this.BillboardId >= 0 && this.cb != null)
		{
			this.cb.SetAlive(this.BillboardId, true);
		}
		bool flag = this.CurrentLodTransform && !this.CurrentLodTransform.gameObject.activeSelf;
		if (this.isSpawned && (!this.CurrentLodTransform || flag))
		{
			this.CurrentLodTransform = null;
			this.lodWasDestroyed = !flag;
			base.enabled = false;
			if (this.DestroyInsteadOfDisable)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			return;
		}
		int num = this.nextLod;
		if (num != this.currentLOD)
		{
			this.SetLOD(num);
		}
	}

	public void ForceLOD()
	{
	}

	public bool Burn()
	{
		bool result = false;
		if (this.CurrentLodTransform)
		{
			FirePoint[] componentsInChildren = this.CurrentLodTransform.GetComponentsInChildren<FirePoint>();
			foreach (FirePoint firePoint in componentsInChildren)
			{
				firePoint.startFire();
				result = true;
			}
		}
		return result;
	}

	public bool ShouldDoMainThreadRefresh { get; set; }

	public void ThreadedRefresh()
	{
		if (this.currentLOD == -2 != this.maxDrawDistanceSetting < TheForestQualitySettings.UserSettings.DrawDistance)
		{
			this.ShouldDoMainThreadRefresh = true;
		}
		else if (this.currentLOD > -2)
		{
			this.nextLod = this.GetLOD();
			if (this.nextLod != this.currentLOD || (this.isSpawned && !this.CurrentLodTransform))
			{
				this.ShouldDoMainThreadRefresh = true;
			}
			else if (this.ShouldDoMainThreadRefresh)
			{
				this.ShouldDoMainThreadRefresh = false;
			}
		}
	}

	public void MainThreadRefresh()
	{
		this.RefreshLODs();
	}

	public Transform High;

	public Transform Mid;

	public Transform Low;

	public GameObject billboard;

	private bool isSpawned;

	private bool canForceWorkScheduler = true;

	[HideInInspector]
	public bool DontSpawn;

	public Transform CurrentLodTransform;

	protected CustomBillboard cb;

	protected int nextLod = -1;

	protected int currentLOD = -1;

	protected int BillboardId = -1;

	private TheForestQualitySettings.DrawDistances maxDrawDistanceSetting;

	private LOD_Stats stats = new LOD_Stats();

	protected Vector3 _position;

	private int wsToken;

	private bool lodWasDestroyed;
}
