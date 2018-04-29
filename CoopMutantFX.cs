using System;
using Bolt;
using FMOD.Studio;
using UnityEngine;


public class CoopMutantFX : EntityBehaviour<IMutantState>
{
	
	private void Awake()
	{
		if (!BoltNetwork.isRunning)
		{
			base.enabled = false;
		}
	}

	
	private void OnDisable()
	{
		this.StopOnFireEvent();
	}

	
	public override void Attached()
	{
		base.state.AddCallback("fx_mask", delegate
		{
			int fx_mask = base.state.fx_mask;
			this.FX_Fire1.SetActive((fx_mask & 1) == 1);
			if (!base.entity.isOwner)
			{
				bool flag = this.isBurning;
				this.isBurning = (fx_mask != 0);
				if (this.isBurning != flag)
				{
					if (this.isBurning)
					{
						this.StartOnFireEvent();
					}
					else
					{
						this.StopOnFireEvent();
					}
				}
			}
		});
	}

	
	private void StartOnFireEvent()
	{
		if (SteamDSConfig.isDedicatedServer)
		{
			return;
		}
		if (!FMOD_StudioSystem.ForceFmodOff && FMOD_StudioSystem.instance && this.onFireEventInstance == null && !string.IsNullOrEmpty(this.OnFireEvent))
		{
			this.onFireEventInstance = FMOD_StudioSystem.instance.GetEvent(this.OnFireEvent);
			if (this.onFireEventInstance != null)
			{
				this.onFireEventInstance.setParameterValue("health", 50f);
				this.UpdateOnFireEvent();
				UnityUtil.ERRCHECK(this.onFireEventInstance.start());
			}
		}
	}

	
	private void StopOnFireEvent()
	{
		if (this.onFireEventInstance != null)
		{
			UnityUtil.ERRCHECK(this.onFireEventInstance.stop(STOP_MODE.ALLOWFADEOUT));
			UnityUtil.ERRCHECK(this.onFireEventInstance.release());
			this.onFireEventInstance = null;
		}
	}

	
	private void UpdateOnFireEvent()
	{
		if (this.onFireEventInstance != null)
		{
			UnityUtil.ERRCHECK(this.onFireEventInstance.set3DAttributes(base.transform.to3DAttributes()));
		}
	}

	
	public void Update()
	{
		if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner)
		{
			int num = 0;
			if (this.FX_Fire1 && this.FX_Fire1.activeSelf)
			{
				num |= 1;
			}
			if (base.state.fx_mask != num)
			{
				base.state.fx_mask = num;
			}
		}
		if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && !base.entity.isOwner)
		{
			if (this.FX_Fire1 && this.FX_Fire1.activeSelf && !this.particlesActive)
			{
				if (this.FX_Fire2)
				{
					this.FX_Fire2.SetActive(true);
				}
				if (this.FX_Fire3)
				{
					this.FX_Fire3.SetActive(true);
				}
				if (this.FX_Fire4)
				{
					this.FX_Fire4.SetActive(true);
				}
				this.particlesActive = true;
			}
			else if (this.FX_Fire1 && !this.FX_Fire1.activeSelf && this.particlesActive)
			{
				this.resetFireDurations();
				if (this.FX_Fire2)
				{
					this.FX_Fire2.SetActive(false);
				}
				if (this.FX_Fire3)
				{
					this.FX_Fire3.SetActive(false);
				}
				if (this.FX_Fire4)
				{
					this.FX_Fire4.SetActive(false);
				}
				this.particlesActive = false;
			}
		}
		this.UpdateOnFireEvent();
	}

	
	private void resetFireDurations()
	{
		if (this.FX_Fire1)
		{
			spawnParticleController component = this.FX_Fire1.GetComponent<spawnParticleController>();
			if (component)
			{
				component.resetParticleDuration();
			}
		}
		if (this.FX_Fire2)
		{
			spawnParticleController component2 = this.FX_Fire2.GetComponent<spawnParticleController>();
			if (component2)
			{
				component2.resetParticleDuration();
			}
		}
		if (this.FX_Fire3)
		{
			spawnParticleController component3 = this.FX_Fire3.GetComponent<spawnParticleController>();
			if (component3)
			{
				component3.resetParticleDuration();
			}
		}
		if (this.FX_Fire4)
		{
			spawnParticleController component4 = this.FX_Fire4.GetComponent<spawnParticleController>();
			if (component4)
			{
				component4.resetParticleDuration();
			}
		}
	}

	
	public GameObject FX_Fire1;

	
	public GameObject FX_Fire2;

	
	public GameObject FX_Fire3;

	
	public GameObject FX_Fire4;

	
	[Header("FMOD Events (played on creepy mutants for multiplayer clients)")]
	public string OnFireEvent;

	
	private bool isBurning;

	
	private bool particlesActive;

	
	private EventInstance onFireEventInstance;
}
