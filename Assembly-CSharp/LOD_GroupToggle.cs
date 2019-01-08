using System;
using TheForest.Utils;
using TheForest.Utils.WorkSchedulerInterfaces;
using UnityEngine;

public class LOD_GroupToggle : MonoBehaviour, IThreadSafeTask
{
	private void Start()
	{
		this._position = base.transform.position;
		this.ThreadedRefresh();
		this.ShouldDoMainThreadRefresh = false;
		this.RefreshVisibility(true);
	}

	private void OnDestroy()
	{
		try
		{
			this.OnDisable();
		}
		catch
		{
		}
	}

	private void OnEnable()
	{
		this._position = base.transform.position;
		if (!LevelSerializer.IsDeserializing && this._levels != null)
		{
			this.WSRegistration(this._doEnableRefresh);
		}
		else
		{
			base.Invoke("WSRegistration", 0.005f);
		}
	}

	private void OnDisable()
	{
		this.WSUnregister();
	}

	public virtual void CheckWsToken()
	{
		if (this._wsToken > -1 && !WorkScheduler.CheckTokenForPosition(this._position, this._wsToken))
		{
			this.WSRegistration(false);
			this.ShouldDoMainThreadRefresh = true;
		}
	}

	private void WSRegistration()
	{
		this.WSRegistration(this._doEnableRefresh);
	}

	protected virtual void WSRegistration(bool doEnableRefresh)
	{
		if (this._wsToken != -1)
		{
			WorkScheduler.Unregister(this, this._wsToken);
		}
		this._wsToken = WorkScheduler.Register(this, this._position, true);
		if (doEnableRefresh)
		{
			this.ThreadedRefresh();
			this.ShouldDoMainThreadRefresh = false;
			this.RefreshVisibility(true);
		}
	}

	protected virtual void WSUnregister()
	{
		if (this._wsToken > -1)
		{
			WorkScheduler.Unregister(this, this._wsToken);
			this._wsToken = -1;
		}
	}

	public void ForceVisibility(byte level)
	{
		this._nextVisibility = level;
		this.RefreshVisibility(true);
	}

	public virtual void RefreshVisibility(bool force)
	{
		this._position = base.transform.position;
		if (force)
		{
			this._currentVisibility = this._nextVisibility;
			for (int i = 0; i < this._levels.Length; i++)
			{
				this._levels[i].RefreshVisibility(i == (int)this._currentVisibility, i != (int)this._currentVisibility);
			}
		}
		else if (this._nextVisibility != this._currentVisibility)
		{
			bool forward = this._nextVisibility < this._currentVisibility;
			if ((int)this._currentVisibility < this._levels.Length)
			{
				this._levels[(int)this._currentVisibility].RefreshVisibility(false, forward);
			}
			if ((int)this._nextVisibility < this._levels.Length)
			{
				this._levels[(int)this._nextVisibility].RefreshVisibility(true, forward);
			}
			this._currentVisibility = this._nextVisibility;
		}
	}

	public bool ShouldDoMainThreadRefresh { get; set; }

	public virtual void ThreadedRefresh()
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
		this.ShouldDoMainThreadRefresh = (this._nextVisibility != this._currentVisibility);
		this.CheckWsToken();
	}

	public void MainThreadRefresh()
	{
		this.RefreshVisibility(false);
	}

	[NameFromEnumIndex(typeof(LOD_GroupToggle.Lods))]
	public LOD_GroupToggle.LodLevel[] _levels;

	public bool _mpCheckAllPlayers;

	public bool _doEnableRefresh = true;

	protected int _wsToken = -1;

	protected byte _nextVisibility;

	protected byte _currentVisibility;

	protected Vector3 _position;

	[Serializable]
	public class LodLevel
	{
		public void RefreshVisibility(bool newVisibility, bool forward)
		{
			this.RefreshRenderers(newVisibility, forward);
			this.RefreshComponents(newVisibility);
		}

		private void RefreshComponents(bool newVisibility)
		{
			if (this.Components == null)
			{
				return;
			}
			foreach (Component component in this.Components)
			{
				if (!(component == null))
				{
					Transform transform = component as Transform;
					if (transform != null)
					{
						transform.gameObject.SetActive(newVisibility);
					}
					else
					{
						MonoBehaviour monoBehaviour = component as MonoBehaviour;
						if (monoBehaviour != null)
						{
							monoBehaviour.enabled = newVisibility;
						}
						else
						{
							Light light = component as Light;
							if (light)
							{
								light.enabled = newVisibility;
							}
							else
							{
								Collider collider = component as Collider;
								if (collider != null)
								{
									collider.enabled = newVisibility;
								}
								else
								{
									ParticleEmitter particleEmitter = component as ParticleEmitter;
									if (particleEmitter != null)
									{
										particleEmitter.enabled = newVisibility;
									}
									else
									{
										ParticleSystem particleSystem = component as ParticleSystem;
										if (particleSystem != null)
										{
											particleSystem.emission.enabled = newVisibility;
										}
										else
										{
											Rigidbody rigidbody = component as Rigidbody;
											if (rigidbody)
											{
												rigidbody.isKinematic = !newVisibility;
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		private void RefreshRenderers(bool newVisibility, bool forward)
		{
			if (this.Renderers == null)
			{
				return;
			}
			foreach (Renderer renderer in this.Renderers)
			{
				if (!(renderer == null))
				{
					renderer.enabled = newVisibility;
					if (newVisibility && forward)
					{
						renderer.SendMessage("ResetStippling", SendMessageOptions.DontRequireReceiver);
					}
					else if (!newVisibility && !forward)
					{
						renderer.SendMessage("StippleOut", SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}

		public Renderer[] Renderers = new Renderer[0];

		public Component[] Components = new Component[0];

		[Range(1f, 10000f)]
		public float VisibleDistance = 75f;
	}

	public enum Lods
	{
		LOD0,
		LOD1,
		LOD2,
		LOD3
	}
}
