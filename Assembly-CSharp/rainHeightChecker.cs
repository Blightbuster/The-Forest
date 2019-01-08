using System;
using TheForest.Utils;
using UnityEngine;

public class rainHeightChecker : MonoBehaviour
{
	private void Start()
	{
		this.mask = 103948289;
		this.defaultHeight = base.transform.localPosition.y;
		this._controllers = base.transform.GetComponentsInChildren<rainParticleController>(true);
	}

	private void Update()
	{
		if (CoopPeerStarter.DedicatedHost || LocalPlayer.IsInCaves || LocalPlayer.IsInEndgame)
		{
			return;
		}
		if (Time.time > this._updateTimer && Scene.WeatherSystem.Raining)
		{
			Vector3 position = LocalPlayer.Transform.position;
			position.y = base.transform.position.y + 40f;
			if (Physics.SphereCast(position, 5f, Vector3.down, out this.hit, 50f, this.mask, QueryTriggerInteraction.Ignore))
			{
				if (this.hit.point.y > base.transform.position.y)
				{
					Vector3 position2 = base.transform.position;
					position2.y = this.hit.point.y;
					base.transform.position = position2;
					this.setHighRainParticles(true);
				}
			}
			else
			{
				base.transform.localPosition = Vector3.zero;
				this.setHighRainParticles(false);
			}
			this._updateTimer = Time.time + 1f;
		}
	}

	private void setHighRainParticles(bool high)
	{
		for (int i = 0; i < this._controllers.Length; i++)
		{
			if (this._controllers[i].gameObject.activeSelf)
			{
				if (high)
				{
					this._controllers[i].SetHighRain();
				}
				else
				{
					this._controllers[i].SetDefaultRain();
				}
			}
		}
	}

	private float defaultHeight;

	private float _updateTimer;

	private int mask;

	private RaycastHit hit;

	private rainParticleController[] _controllers;
}
