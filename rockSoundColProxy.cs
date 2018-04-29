using System;
using Bolt;
using PathologicalGames;
using TheForest.Utils.Physics;
using UnityEngine;


public class rockSoundColProxy : MonoBehaviour, IOnCollisionEnterProxy
{
	
	private void Awake()
	{
		this.soundDelay = false;
		base.Invoke("disableSoundDelay", 2f);
		this.waterLayer = LayerMask.NameToLayer("Water");
		this.rigidBody = base.GetComponent<Rigidbody>();
	}

	
	private void Start()
	{
		if (!CoopPeerStarter.DedicatedHost)
		{
			FMOD_StudioSystem.PreloadEvent(this.rockOnTree);
			FMOD_StudioSystem.PreloadEvent(this.rockBush);
			FMOD_StudioSystem.PreloadEvent(this.rockWater);
			FMOD_StudioSystem.PreloadEvent(this.rockOnGround);
		}
	}

	
	private void disableSoundDelay()
	{
		this.soundDelay = false;
	}

	
	private void PlayEvent(string path)
	{
		if (!this.rigidBody || CoopPeerStarter.DedicatedHost)
		{
			return;
		}
		if (path.Length > 0)
		{
			if (this.maximumSpeed > 0f)
			{
				FMODCommon.PlayOneshot(path, base.transform.position, FMODCommon.NetworkRole.Any, new object[]
				{
					"speed",
					this.rigidBody.velocity.magnitude / this.maximumSpeed
				});
			}
			else
			{
				FMODCommon.PlayOneshot(path, base.transform.position, FMODCommon.NetworkRole.Any, new object[0]);
			}
		}
		this.effectDelay = true;
		base.Invoke("disableEffectDelay", 1f);
	}

	
	public void OnCollisionEnterProxied(Collision collision)
	{
		if (!this.rigidBody)
		{
			return;
		}
		if (this.flintLock)
		{
			ArrowDamage componentInChildren = base.transform.GetComponentInChildren<ArrowDamage>();
			if (componentInChildren && componentInChildren.Live)
			{
				componentInChildren.CheckHit(collision.transform.position, collision.transform, collision.collider.isTrigger, collision.collider);
			}
		}
		this.calculateDamage(collision.gameObject);
		if ((collision.gameObject.CompareTag("Tree") || collision.gameObject.CompareTag("Stick") || collision.gameObject.layer == 17 || collision.gameObject.layer == 20 || collision.gameObject.layer == 25 || UnderfootSurfaceDetector.GetSurfaceType(collision.collider) != UnderfootSurfaceDetector.SurfaceType.None) && this.rigidBody.velocity.magnitude > this.speedThreshold)
		{
			if (!this.InWater && !this.effectDelay)
			{
				this.PlayEvent(this.rockOnTree);
			}
			if (!this.soundDelay)
			{
				this.enableSound();
				this.soundDelay = true;
			}
		}
		if ((collision.gameObject.CompareTag("TerrainMain") || collision.gameObject.layer == 26) && this.rigidBody.velocity.magnitude > this.speedThreshold)
		{
			if (!this.InWater && !this.effectDelay)
			{
				if (this.rockOnGround.Length > 0)
				{
					this.PlayEvent(this.rockOnGround);
				}
				else
				{
					this.PlayEvent(this.rockOnTree);
				}
			}
			if (!this.soundDelay)
			{
				this.enableSound();
				this.soundDelay = true;
			}
		}
		if (collision.gameObject.CompareTag("Float") && this.rigidBody.velocity.magnitude > this.speedThreshold)
		{
			if (!this.InWater && !this.effectDelay)
			{
				if (this.rockOnGround.Length > 0)
				{
					this.PlayEvent(this.stickHitTree);
				}
				else
				{
					this.PlayEvent(this.rockOnTree);
				}
			}
			if (!this.soundDelay)
			{
				this.enableSound();
				this.soundDelay = true;
			}
		}
	}

	
	
	private bool InWater
	{
		get
		{
			return this.inWaterCount > 0;
		}
	}

	
	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("SmallTree"))
		{
		}
		if (!this.rigidBody)
		{
			return;
		}
		this.calculateDamage(other.gameObject);
		if (other.gameObject.layer == this.waterLayer)
		{
			if (!this.InWater && !this.effectDelay && this.rigidBody.velocity.magnitude > this.speedThreshold)
			{
				if (this.waterSplashGo)
				{
					Vector3 position = base.transform.position;
					position.y = other.bounds.center.y + other.bounds.extents.y + 0.2f;
					PoolManager.Pools["Particles"].Spawn(this.waterSplashGo.transform, position, Quaternion.identity);
				}
				this.PlayEvent(this.rockWater);
			}
			this.inWaterCount++;
		}
	}

	
	public void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == this.waterLayer)
		{
			this.inWaterCount--;
		}
	}

	
	private void calculateDamage(GameObject go)
	{
		if (this.allowDamage && this.rigidBody.velocity.magnitude > Mathf.Max(this.speedThreshold * 2f, 15f))
		{
			if (go.CompareTag("PlayerNet") && BoltNetwork.isRunning)
			{
				BoltEntity componentInParent = go.GetComponentInParent<BoltEntity>();
				if (componentInParent)
				{
					HitPlayer hitPlayer = HitPlayer.Create(componentInParent, EntityTargets.OnlyOwner);
					if (this.flintLock)
					{
						hitPlayer.damage = 45;
					}
					else
					{
						hitPlayer.damage = 5;
					}
					hitPlayer.Send();
				}
			}
			if (go.CompareTag("enemyCollide") || go.CompareTag("enemyRoot") || go.tag == "lb_bird" || go.CompareTag("animalCollide") || go.CompareTag("Fish"))
			{
				if (BoltNetwork.isClient)
				{
					PlayerHitEnemy playerHitEnemy = PlayerHitEnemy.Raise(GlobalTargets.OnlyServer);
					playerHitEnemy.Target = go.GetComponentInParent<BoltEntity>();
					playerHitEnemy.getAttackDirection = 3;
					playerHitEnemy.getAttackerType = 4;
					if (this.flintLock)
					{
						playerHitEnemy.Hit = 100;
					}
					else
					{
						playerHitEnemy.Hit = 1;
					}
					playerHitEnemy.Send();
				}
				else if (!go.CompareTag("MidTree") && !go.CompareTag("Tree") && go.layer != 11)
				{
					go.gameObject.SendMessageUpwards("getAttackDirection", 3, SendMessageOptions.DontRequireReceiver);
					go.gameObject.SendMessageUpwards("getAttackerType", 4, SendMessageOptions.DontRequireReceiver);
					int num = 1;
					if (this.flintLock)
					{
						num = 100;
					}
					go.gameObject.SendMessageUpwards("Hit", num, SendMessageOptions.DontRequireReceiver);
					go.SendMessage("Hit", num, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}

	
	private void enableSound()
	{
		this.spawnedSound = UnityEngine.Object.Instantiate<Transform>(this.soundDetect.transform, base.transform.position, base.transform.rotation);
		this.spawnedSound.SendMessage("setRange", this.soundRange, SendMessageOptions.DontRequireReceiver);
		base.Invoke("disableSound", 0.5f);
	}

	
	private void disableSound()
	{
		if (this.spawnedSound)
		{
			UnityEngine.Object.Destroy(this.spawnedSound.gameObject);
		}
		this.soundDelay = false;
	}

	
	private void disableEffectDelay()
	{
		this.effectDelay = false;
	}

	
	private void OnDisable()
	{
		this.disableSound();
	}

	
	private void OnDestroy()
	{
		this.disableSound();
	}

	
	public float maximumSpeed;

	
	public float speedThreshold = 2.5f;

	
	public string rockOnTree;

	
	public string rockBush;

	
	public string rockWater;

	
	public string rockOnGround;

	
	public string stickHitTree;

	
	public GameObject soundDetect;

	
	public GameObject waterSplashGo;

	
	public float soundRange;

	
	private GameObject soundGo;

	
	private Transform spawnedSound;

	
	private int waterLayer;

	
	private bool soundDelay;

	
	private bool effectDelay;

	
	public Rigidbody rigidBody;

	
	public bool allowDamage;

	
	public bool flintLock;

	
	private int inWaterCount;
}
