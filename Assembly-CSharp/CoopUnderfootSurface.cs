using System;
using Bolt;

public class CoopUnderfootSurface : EntityBehaviour<IPlayerState>
{
	public UnderfootSurfaceDetector.SurfaceType Surface { get; private set; }

	public bool IsOnGore { get; private set; }

	private void Start()
	{
		this.surfaceDetector = base.GetComponentInChildren<UnderfootSurfaceDetector>();
		this.Surface = UnderfootSurfaceDetector.SurfaceType.None;
		this.IsOnGore = false;
	}

	public override void Attached()
	{
		if (!base.entity.isOwner)
		{
			base.state.AddCallback("UnderfootSurface", new PropertyCallbackSimple(this.OnUnderfootSurface));
			base.state.AddCallback("UnderfootGore", new PropertyCallbackSimple(this.OnUnderfootGore));
		}
	}

	private void OnUnderfootSurface()
	{
		this.Surface = (UnderfootSurfaceDetector.SurfaceType)base.state.UnderfootSurface;
	}

	private void OnUnderfootGore()
	{
		this.IsOnGore = base.state.UnderfootGore;
	}

	private void Update()
	{
		if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner && this.surfaceDetector)
		{
			base.state.UnderfootSurface = (int)this.surfaceDetector.Surface;
			base.state.UnderfootGore = this.surfaceDetector.IsOnGore;
		}
	}

	private UnderfootSurfaceDetector surfaceDetector;
}
