using System;
using TheForest.Utils;
using TheForest.Utils.WorkSchedulerInterfaces;
using UnityEngine;


public class LOD_SimpleToggle : MonoBehaviour, IThreadSafeTask
{
	
	private void Start()
	{
		this.position = base.transform.position;
		this.ThreadedRefresh();
		this.ShouldDoMainThreadRefresh = false;
		this.RefreshVisibility(true);
	}

	
	private void OnDestroy()
	{
		try
		{
			WorkScheduler.Unregister(this, this.wsToken);
		}
		catch
		{
		}
	}

	
	private void OnEnable()
	{
		this.position = base.transform.position;
		if (!LevelSerializer.IsDeserializing)
		{
			this.WSRegistration(this.DoEnableRefresh);
		}
		else
		{
			base.Invoke("WSRegistration", 0.005f);
		}
	}

	
	private void OnDisable()
	{
		if (this.wsToken > -1)
		{
			WorkScheduler.Unregister(this, this.wsToken);
			this.wsToken = -1;
		}
	}

	
	public void CheckWsToken()
	{
		if (this.wsToken > -1 && !WorkScheduler.CheckTokenForPosition(this.position, this.wsToken))
		{
			this.WSRegistration(false);
			this.ShouldDoMainThreadRefresh = true;
		}
	}

	
	private void WSRegistration()
	{
		this.WSRegistration(this.DoEnableRefresh);
	}

	
	private void WSRegistration(bool doEnableRefresh)
	{
		if (this.wsToken != -1)
		{
			WorkScheduler.Unregister(this, this.wsToken);
		}
		this.wsToken = WorkScheduler.Register(this, this.position, true);
		if (doEnableRefresh)
		{
			this.ThreadedRefresh();
			this.ShouldDoMainThreadRefresh = false;
			this.RefreshVisibility(true);
		}
	}

	
	private void RefreshVisibility(bool force)
	{
		this.position = base.transform.position;
		bool flag = this.nextVisibility;
		if (flag != this.currentVisibility || force)
		{
			for (int i = 0; i < this.Renderers.Length; i++)
			{
				if (!this.Renderers[i].IsNull())
				{
					this.Renderers[i].enabled = flag;
					if (flag)
					{
						this.Renderers[i].SendMessage("OnSpawned", SendMessageOptions.DontRequireReceiver);
					}
				}
			}
			for (int j = 0; j < this.Components.Length; j++)
			{
				Component component = this.Components[j];
				Transform transform = component as Transform;
				if (transform != null)
				{
					transform.gameObject.SetActive(flag);
				}
				else
				{
					MonoBehaviour monoBehaviour = component as MonoBehaviour;
					if (monoBehaviour != null)
					{
						monoBehaviour.enabled = flag;
					}
					else
					{
						Collider collider = component as Collider;
						if (collider != null)
						{
							collider.enabled = flag;
						}
						else
						{
							ParticleEmitter particleEmitter = component as ParticleEmitter;
							if (particleEmitter != null)
							{
								particleEmitter.enabled = flag;
							}
							else
							{
								ParticleSystem particleSystem = component as ParticleSystem;
								if (particleSystem != null)
								{
									particleSystem.enableEmission = flag;
								}
								else
								{
									AudioSource audioSource = component as AudioSource;
									if (audioSource != null)
									{
										audioSource.enabled = flag;
									}
									else
									{
										Light light = component as Light;
										if (light)
										{
											light.enabled = flag;
										}
										else
										{
											Rigidbody rigidbody = component as Rigidbody;
											if (rigidbody)
											{
												rigidbody.isKinematic = !flag;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			this.currentVisibility = flag;
		}
	}

	
	
	
	public bool ShouldDoMainThreadRefresh { get; set; }

	
	public void ThreadedRefresh()
	{
		if (this.MpCheckAllPlayers)
		{
			this.nextVisibility = (Scene.SceneTracker.GetClosestPlayerDistanceFromPos(this.position) * ((!this.currentVisibility) ? 1f : 0.9f) < this.VisibleDistance);
		}
		else if (LocalPlayer.Transform)
		{
			this.position.y = PlayerCamLocation.PlayerLoc.y;
			float num = (this.position - PlayerCamLocation.PlayerLoc).sqrMagnitude * ((!this.currentVisibility) ? 1f : 0.9f);
			this.nextVisibility = (num < this.VisibleDistance * this.VisibleDistance);
		}
		this.ShouldDoMainThreadRefresh = (this.nextVisibility != this.currentVisibility);
		this.CheckWsToken();
	}

	
	public void MainThreadRefresh()
	{
		this.RefreshVisibility(false);
	}

	
	public Renderer[] Renderers = new Renderer[0];

	
	public Component[] Components = new Component[0];

	
	[Range(1f, 1000f)]
	public float VisibleDistance = 75f;

	
	public bool MpCheckAllPlayers;

	
	public bool DoEnableRefresh = true;

	
	private int wsToken = -1;

	
	private bool nextVisibility = true;

	
	private bool currentVisibility = true;

	
	private Vector3 position;
}
