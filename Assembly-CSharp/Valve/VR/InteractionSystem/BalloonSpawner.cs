using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class BalloonSpawner : MonoBehaviour
	{
		private void Start()
		{
			if (this.balloonPrefab == null)
			{
				return;
			}
			if (this.autoSpawn && this.spawnAtStartup)
			{
				this.SpawnBalloon(this.color);
				this.nextSpawnTime = UnityEngine.Random.Range(this.minSpawnTime, this.maxSpawnTime) + Time.time;
			}
		}

		private void Update()
		{
			if (this.balloonPrefab == null)
			{
				return;
			}
			if (Time.time > this.nextSpawnTime && this.autoSpawn)
			{
				this.SpawnBalloon(this.color);
				this.nextSpawnTime = UnityEngine.Random.Range(this.minSpawnTime, this.maxSpawnTime) + Time.time;
			}
		}

		public GameObject SpawnBalloon(Balloon.BalloonColor color = Balloon.BalloonColor.Red)
		{
			if (this.balloonPrefab == null)
			{
				return null;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.balloonPrefab, base.transform.position, base.transform.rotation);
			gameObject.transform.localScale = new Vector3(this.scale, this.scale, this.scale);
			if (this.attachBalloon)
			{
				gameObject.transform.parent = base.transform;
			}
			if (this.sendSpawnMessageToParent && base.transform.parent != null)
			{
				base.transform.parent.SendMessage("OnBalloonSpawned", gameObject, SendMessageOptions.DontRequireReceiver);
			}
			if (this.playSounds)
			{
				if (this.inflateSound != null)
				{
					this.inflateSound.Play();
				}
				if (this.stretchSound != null)
				{
					this.stretchSound.Play();
				}
			}
			gameObject.GetComponentInChildren<Balloon>().SetColor(color);
			if (this.spawnDirectionTransform != null)
			{
				gameObject.GetComponentInChildren<Rigidbody>().AddForce(this.spawnDirectionTransform.forward * this.spawnForce);
			}
			return gameObject;
		}

		public void SpawnBalloonFromEvent(int color)
		{
			this.SpawnBalloon((Balloon.BalloonColor)color);
		}

		public float minSpawnTime = 5f;

		public float maxSpawnTime = 15f;

		private float nextSpawnTime;

		public GameObject balloonPrefab;

		public bool autoSpawn = true;

		public bool spawnAtStartup = true;

		public bool playSounds = true;

		public SoundPlayOneshot inflateSound;

		public SoundPlayOneshot stretchSound;

		public bool sendSpawnMessageToParent;

		public float scale = 1f;

		public Transform spawnDirectionTransform;

		public float spawnForce;

		public bool attachBalloon;

		public Balloon.BalloonColor color = Balloon.BalloonColor.Random;
	}
}
