using System;
using System.Collections;
using FMOD.Studio;
using TheForest.Utils;
using UnityEngine;


public class secondArtifactController : MonoBehaviour
{
	
	private void Start()
	{
		this.startLocalPos = this.blueLight.transform.localPosition;
		this.animator = base.transform.GetComponent<Animator>();
		base.StartCoroutine("ArtifactGlowRoutine");
		this.random = UnityEngine.Random.Range(0f, 65535f);
	}

	
	private void Update()
	{
		this.startArtifactEvent();
	}

	
	private IEnumerator ArtifactGlowRoutine()
	{
		this.animator.enabled = true;
		MeshRenderer mr = this.artifactGo.GetComponent<MeshRenderer>();
		Material mat = mr.sharedMaterial;
		mat.EnableKeyword("_EMISSION");
		float emission = 0f;
		float speed = 1f;
		float light = 1f;
		float playerDist = float.PositiveInfinity;
		Color baseColor = Color.white;
		for (;;)
		{
			if (LocalPlayer.Transform)
			{
				playerDist = Vector3.Distance(LocalPlayer.Transform.position, base.transform.position);
				if (playerDist < 200f)
				{
					this.blueLight.gameObject.SetActive(true);
				}
				else
				{
					this.blueLight.gameObject.SetActive(false);
				}
			}
			speed = Mathf.Lerp(speed, this.speedTarget, Time.deltaTime / 5f);
			emission = Mathf.Lerp(emission, this.emissionTarget, Time.deltaTime / 2f);
			Color finalColor = baseColor * Mathf.LinearToGammaSpace(emission);
			mat.SetColor("_EmissionColor", finalColor);
			this.animator.SetFloat("speed", speed);
			float noise = Mathf.PerlinNoise(this.random, Time.time * this.timeMult);
			float noise2 = Mathf.PerlinNoise(this.random, Time.time * this.wobbleTimeMult);
			light = Mathf.Lerp(light, this.lightIntensityMult, Time.deltaTime / 5f);
			float lightIntensity = Mathf.Lerp(this.minIntensity, this.maxIntensity, noise) * light * this.flashAmount;
			if (this.additionalLights.Length > 0)
			{
				for (int i = 0; i < this.additionalLights.Length; i++)
				{
					this.additionalLights[i].intensity = lightIntensity / 2f;
				}
			}
			this.blueLight.intensity = lightIntensity;
			Vector3 localPos = this.startLocalPos;
			localPos.x = Mathf.Lerp(this.wobbleMin, this.wobbleMax, noise2);
			this.blueLight.transform.localPosition = localPos;
			yield return null;
		}
		yield break;
	}

	
	public void setArtifactOff()
	{
		this.emissionTarget = 0f;
		this.speedTarget = 0f;
		this.lightIntensityMult = 0f;
		base.Invoke("stopArtifactRoutine", 20f);
		UnityUtil.ERRCHECK(this.artifactLoop.stop(STOP_MODE.ALLOWFADEOUT));
	}

	
	public void setArtifactOn()
	{
		this.emissionTarget = 1.8f;
		this.speedTarget = 2f;
		this.lightIntensityMult = 2f;
	}

	
	private void stopArtifactRoutine()
	{
		base.StopCoroutine("ArtifactGlowRoutine");
	}

	
	private void enableFlashEffectGo()
	{
		if (this.explodeEffectGo)
		{
			this.explodeEffectGo.SetActive(true);
		}
		UnityUtil.ERRCHECK(this.artifactLoop.stop(STOP_MODE.ALLOWFADEOUT));
	}

	
	private void startArtifactEvent()
	{
		if (FMOD_StudioSystem.instance && this.artifactLoop == null && this.artifactLoopEvent != null)
		{
			this.artifactLoop = FMOD_StudioSystem.instance.GetEvent(this.artifactLoopEvent);
			UnityUtil.ERRCHECK(this.artifactLoop.set3DAttributes(UnityUtil.to3DAttributes(base.gameObject, null)));
			if (this.artifactLoop != null)
			{
				UnityUtil.ERRCHECK(this.artifactLoop.start());
			}
		}
	}

	
	public GameObject explodeEffectGo;

	
	public GameObject artifactGo;

	
	public Light blueLight;

	
	public Light[] additionalLights;

	
	public float emissionTarget = 0.5f;

	
	public float speedTarget = 0.75f;

	
	public float lightIntensityMult = 1f;

	
	private Animator animator;

	
	public float minIntensity = 0.6f;

	
	public float maxIntensity = 1.25f;

	
	public float timeMult = 2f;

	
	private float flashAmount = 1f;

	
	public float wobbleMin = -0.25f;

	
	public float wobbleMax = 0.25f;

	
	public float wobbleTimeMult = 2f;

	
	private Vector3 startLocalPos;

	
	private float random;

	
	public string artifactLoopEvent;

	
	private EventInstance artifactLoop;
}
