﻿using System;
using TheForest.Utils;
using UnityEngine;

namespace AdvancedTerrainGrass
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(WindZone))]
	public class Wind : MonoBehaviour
	{
		private void OnEnable()
		{
			if (this.WindCompositeShader == null)
			{
				this.WindCompositeShader = Shader.Find("WindComposite");
			}
			if (this.WindBaseTex == null)
			{
				this.WindBaseTex = (Resources.Load("Default wind base texture") as Texture);
			}
			this.SetupRT();
			this.GetPIDs();
			this.trans = base.transform;
			this.currentEulerAngle = this.trans.rotation.eulerAngles.y;
			this.windZone = this.trans.GetComponent<WindZone>();
			this.StartWind = this.windZone.windMain;
		}

		private void SetupRT()
		{
			if (this.WindRenderTexture == null || this.m_material == null)
			{
				this.WindRenderTexture = new RenderTexture((int)this.Resolution, (int)this.Resolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
				this.WindRenderTexture.useMipMap = true;
				this.WindRenderTexture.wrapMode = TextureWrapMode.Repeat;
				this.m_material = new Material(this.WindCompositeShader);
			}
		}

		private void GetPIDs()
		{
			this.WindRTPID = Shader.PropertyToID("_AtgWindRT");
			this.AtgWindDirSizePID = Shader.PropertyToID("_AtgWindDirSize");
			this.AtgWindStrengthMultipliersPID = Shader.PropertyToID("_AtgWindStrengthMultipliers");
			this.AtgSinTimePID = Shader.PropertyToID("_AtgSinTime");
			this.AtgGustPID = Shader.PropertyToID("_AtgGust");
			this.AtgWindUVsPID = Shader.PropertyToID("_AtgWindUVs");
			this.AtgWindUVs1PID = Shader.PropertyToID("_AtgWindUVs1");
			this.AtgWindUVs2PID = Shader.PropertyToID("_AtgWindUVs2");
			this.AtgWindUVs3PID = Shader.PropertyToID("_AtgWindUVs3");
		}

		private void OnValidate()
		{
			if (this.WindCompositeShader == null)
			{
				this.WindCompositeShader = Shader.Find("WindComposite");
			}
			if (this.WindBaseTex == null)
			{
				this.WindBaseTex = (Resources.Load("Default wind base texture") as Texture);
			}
		}

		private void Update()
		{
			if (Scene.Atmosphere)
			{
				this.windZone.windMain = this.StartWind * (Scene.Atmosphere.WindIntensity + 0.8f) * 1.1f;
			}
			this.mainWind = this.windZone.windMain;
			this.turbulence = this.windZone.windTurbulence;
			this.UpdateWindOrientation();
			float deltaTime = Time.deltaTime;
			this.WindDirectionSize.x = this.trans.forward.x;
			this.WindDirectionSize.y = this.trans.forward.y;
			this.WindDirectionSize.z = this.trans.forward.z;
			this.WindDirectionSize.w = this.size;
			Vector2 a = new Vector2(this.WindDirectionSize.x, this.WindDirectionSize.z) * deltaTime * this.speed;
			this.uvs -= a * this.speedLayer0;
			this.uvs.x = this.uvs.x - (float)((int)this.uvs.x);
			this.uvs.y = this.uvs.y - (float)((int)this.uvs.y);
			this.uvs1 -= a * this.speedLayer1;
			this.uvs1.x = this.uvs1.x - (float)((int)this.uvs1.x);
			this.uvs1.y = this.uvs1.y - (float)((int)this.uvs1.y);
			this.uvs2 -= a * this.speedLayer2;
			this.uvs2.x = this.uvs2.x - (float)((int)this.uvs2.x);
			this.uvs2.y = this.uvs2.y - (float)((int)this.uvs2.y);
			this.uvs3 -= a * this.GrassGustSpeed;
			this.uvs3.x = this.uvs3.x - (float)((int)this.uvs3.x);
			this.uvs3.y = this.uvs3.y - (float)((int)this.uvs3.y);
			Shader.SetGlobalVector(this.AtgWindDirSizePID, this.WindDirectionSize);
			Vector2 v;
			v.x = this.Grass * this.mainWind;
			v.y = this.Foliage * this.mainWind;
			Shader.SetGlobalVector(this.AtgWindStrengthMultipliersPID, v);
			Shader.SetGlobalVector(this.AtgGustPID, new Vector2((float)this.GrassGustTiling, this.turbulence + 0.5f));
			float time = Time.time;
			Shader.SetGlobalVector(this.AtgSinTimePID, new Vector4(Mathf.Sin(time * this.JitterFrequency), Mathf.Sin(time * this.JitterFrequency * 0.2317f + 6.28318548f), Mathf.Sin(time * this.JitterHighFrequency), this.turbulence * 0.1f));
			Shader.SetGlobalVector(this.AtgWindUVsPID, this.uvs);
			Shader.SetGlobalVector(this.AtgWindUVs1PID, this.uvs1);
			Shader.SetGlobalVector(this.AtgWindUVs2PID, this.uvs2);
			Shader.SetGlobalVector(this.AtgWindUVs3PID, this.uvs3);
			Graphics.Blit(this.WindBaseTex, this.WindRenderTexture, this.m_material);
			this.WindRenderTexture.SetGlobalShaderProperty("_AtgWindRT");
		}

		private void UpdateWindOrientation()
		{
			this.nextChangeWindDirectionCounter -= Time.deltaTime;
			if (this.nextChangeWindDirectionCounter < 0f)
			{
				this.nextChangeWindDirectionCounter = UnityEngine.Random.Range(this.MIN_TIME_CHANGE_WIND_DIRECTION, this.MAX_TIME_CHANGE_WIND_DIRECTION);
				this.currentWindOrientationChange = (float)((UnityEngine.Random.Range(0, 2) != 0) ? 1 : -1) * UnityEngine.Random.Range(this.MIN_ORIENTATION_CHANGE_WIND_DIRECTION, this.MAX_ORIENTATION_CHANGE_WIND_DIRECTION);
				this.targetEulerAngle = this.currentEulerAngle + this.currentWindOrientationChange;
				this.currentWindOrientationSpeed = UnityEngine.Random.Range(this.MIN_SPEED_CHANGE_WIND_DIRECTION, this.MAX_SPEED_CHANGE_WIND_DIRECTION);
				this.deltaEulerAngle = this.currentWindOrientationChange / this.currentWindOrientationSpeed;
			}
			if (Mathf.Abs(this.targetEulerAngle - this.currentEulerAngle) > 1f)
			{
				this.currentEulerAngle += this.deltaEulerAngle * Time.deltaTime;
				if (this.currentEulerAngle > 360f)
				{
					this.currentEulerAngle -= 360f;
				}
				else if (this.currentEulerAngle < -360f)
				{
					this.currentEulerAngle += 360f;
				}
				this.trans.localEulerAngles = new Vector3(0f, this.currentEulerAngle, 0f);
			}
		}

		[Header("Render Texture Settings")]
		[Space(4f)]
		public RTSize Resolution = RTSize._512;

		public Texture WindBaseTex;

		public Shader WindCompositeShader;

		[Header("Wind Multipliers")]
		[Space(4f)]
		public float Grass = 1f;

		public float Foliage = 1f;

		[Header("Size and Speed")]
		[Space(4f)]
		[Range(0.001f, 0.1f)]
		public float size = 0.01f;

		[Range(0.0001f, 0.2f)]
		[Space(5f)]
		public float speed = 0.02f;

		public float speedLayer0 = 0.476f;

		public float speedLayer1 = 1.23f;

		public float speedLayer2 = 2.93f;

		[Header("Noise")]
		[Space(4f)]
		public int GrassGustTiling = 4;

		public float GrassGustSpeed = 0.278f;

		[Header("Jitter")]
		[Space(4f)]
		public float JitterFrequency = 3.127f;

		public float JitterHighFrequency = 21f;

		private RenderTexture WindRenderTexture;

		private Material m_material;

		private Vector2 uvs = new Vector2(0f, 0f);

		private Vector2 uvs1 = new Vector2(0f, 0f);

		private Vector2 uvs2 = new Vector2(0f, 0f);

		private Vector2 uvs3 = new Vector2(0f, 0f);

		private int WindRTPID;

		private Transform trans;

		private WindZone windZone;

		private float mainWind;

		private float turbulence;

		private int AtgWindDirSizePID;

		private int AtgWindStrengthMultipliersPID;

		private int AtgSinTimePID;

		private int AtgGustPID;

		private int AtgWindUVsPID;

		private int AtgWindUVs1PID;

		private int AtgWindUVs2PID;

		private int AtgWindUVs3PID;

		private float StartWind;

		private Vector4 WindDirectionSize = Vector4.zero;

		private float MIN_TIME_CHANGE_WIND_DIRECTION = 10f;

		private float MAX_TIME_CHANGE_WIND_DIRECTION = 25f;

		private float nextChangeWindDirectionCounter;

		private float MIN_ORIENTATION_CHANGE_WIND_DIRECTION = 5f;

		private float MAX_ORIENTATION_CHANGE_WIND_DIRECTION = 20f;

		private float currentWindOrientationChange;

		private float MIN_SPEED_CHANGE_WIND_DIRECTION = 7f;

		private float MAX_SPEED_CHANGE_WIND_DIRECTION = 15f;

		private float currentWindOrientationSpeed;

		private float currentEulerAngle;

		private float targetEulerAngle;

		private float deltaEulerAngle;
	}
}
