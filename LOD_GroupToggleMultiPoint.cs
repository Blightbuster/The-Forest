using System;
using TheForest.Utils;
using TheForest.Utils.WorkSchedulerInterfaces;
using UnityEngine;


public class LOD_GroupToggleMultiPoint : LOD_GroupToggle
{
	
	private void Awake()
	{
		foreach (LOD_GroupToggleMultiPoint.LodPoint lodPoint in this._points)
		{
			lodPoint.Init(this);
		}
	}

	
	public override void CheckWsToken()
	{
		if (this._wsToken > -1 && !WorkScheduler.CheckTokenForPosition(this._position, this._wsToken))
		{
			this.WSRegistration(false);
			base.ShouldDoMainThreadRefresh = true;
		}
	}

	
	private void WSRegistration()
	{
		this.WSRegistration(this._doEnableRefresh);
	}

	
	protected override void WSRegistration(bool doEnableRefresh)
	{
		foreach (LOD_GroupToggleMultiPoint.LodPoint lodPoint in this._points)
		{
			lodPoint.WSRegistration(doEnableRefresh);
		}
		base.WSRegistration(doEnableRefresh);
	}

	
	protected override void WSUnregister()
	{
		foreach (LOD_GroupToggleMultiPoint.LodPoint lodPoint in this._points)
		{
			lodPoint.WSUnregister();
		}
		base.WSUnregister();
	}

	
	public override void RefreshVisibility(bool force)
	{
		byte b = this._nextVisibility;
		for (int i = 0; i < this._points.Length; i++)
		{
			if (this._points[i]._currentVisibility < b)
			{
				b = this._points[i]._currentVisibility;
			}
		}
		this._nextVisibility = b;
		base.RefreshVisibility(force);
	}

	
	public override void ThreadedRefresh()
	{
		float num = 0f;
		if (this._mpCheckAllPlayers)
		{
			num = Scene.SceneTracker.GetClosestPlayerDistanceFromPos(this._position);
			num *= num;
		}
		else if (LocalPlayer.Transform)
		{
			this._position.y = PlayerCamLocation.PlayerLoc.y;
			num = (this._position - PlayerCamLocation.PlayerLoc).sqrMagnitude;
		}
		byte b = 0;
		while ((int)b < this._levels.Length)
		{
			if (num + ((b != this._currentVisibility) ? 0f : -5f) < this._levels[(int)b].VisibleDistance * this._levels[(int)b].VisibleDistance)
			{
				break;
			}
			b += 1;
		}
		this._nextVisibility = b;
		base.ShouldDoMainThreadRefresh = (this._nextVisibility != this._currentVisibility);
		this.CheckWsToken();
	}

	
	public LOD_GroupToggleMultiPoint.LodPoint[] _points;

	
	[Serializable]
	public class LodPoint : IThreadSafeTask
	{
		
		
		
		public byte _currentVisibility { get; private set; }

		
		
		
		public LOD_GroupToggleMultiPoint Holder { get; private set; }

		
		
		
		public bool ShouldDoMainThreadRefresh { get; set; }

		
		public void Init(LOD_GroupToggleMultiPoint holder)
		{
			this.Holder = holder;
			this._position = this._target.position;
			this._wsToken = -1;
		}

		
		public void ThreadedRefresh()
		{
			float num = 0f;
			if (this.Holder._mpCheckAllPlayers)
			{
				num = Scene.SceneTracker.GetClosestPlayerDistanceFromPos(this._position);
				num *= num;
			}
			else if (LocalPlayer.Transform)
			{
				this._position.y = PlayerCamLocation.PlayerLoc.y;
				num = (this._position - PlayerCamLocation.PlayerLoc).sqrMagnitude;
			}
			byte b = 0;
			while ((int)b < this.Holder._levels.Length)
			{
				if (num + ((b != this._currentVisibility) ? 0f : -5f) < this.Holder._levels[(int)b].VisibleDistance * this.Holder._levels[(int)b].VisibleDistance)
				{
					break;
				}
				b += 1;
			}
			this._nextVisibility = b;
			this.ShouldDoMainThreadRefresh = (this._nextVisibility != this._currentVisibility);
		}

		
		public void MainThreadRefresh()
		{
			this._currentVisibility = this._nextVisibility;
			this.Holder.MainThreadRefresh();
		}

		
		public void WSRegistration(bool doEnableRefresh)
		{
			if (this._wsToken != -1)
			{
				WorkScheduler.Unregister(this, this._wsToken);
			}
			this._wsToken = WorkScheduler.Register(this, this._position, true);
		}

		
		public void WSUnregister()
		{
			if (this._wsToken > -1)
			{
				WorkScheduler.Unregister(this, this._wsToken);
				this._wsToken = -1;
			}
		}

		
		public Transform _target;

		
		private int _wsToken = -1;

		
		private byte _nextVisibility;

		
		private Vector3 _position;
	}
}
