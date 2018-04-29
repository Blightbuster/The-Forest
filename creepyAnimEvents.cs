using System;
using FMOD.Studio;
using PathologicalGames;
using TheForest.Utils;
using UnityEngine;


public class creepyAnimEvents : MonoBehaviour
{
	
	private void OnDeserialized()
	{
		this.doStart();
	}

	
	private void Start()
	{
		this.doStart();
	}

	
	private void doStart()
	{
		this.vis = base.transform.GetComponent<mutantVis>();
		if (base.transform.parent)
		{
			this.waterDetect = base.transform.parent.GetComponentInChildren<mutantWaterDetect>();
		}
		this.ragDollSetup = base.transform.GetComponent<clsragdollify>();
		this.girlAi = base.transform.GetComponent<girlMutantAiManager>();
		this.animator = base.gameObject.GetComponent<Animator>();
		this.setup = base.transform.GetComponent<mutantScriptSetup>();
		Transform[] componentsInChildren = base.transform.root.GetComponentsInChildren<Transform>(true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name == "weaponLeftGO")
			{
				this.weaponLeft = transform.gameObject;
			}
			if (transform.name == "weaponLeftGO1")
			{
				this.weaponLeft1 = transform.gameObject;
			}
			if (transform.name == "weaponRightGO")
			{
				this.weaponRight = transform.gameObject;
			}
			if (transform.name == "weaponGO")
			{
				this.weaponMain = transform.gameObject;
				this.weaponMainCollider = transform.GetComponent<Collider>();
			}
			if (transform.name == "weaponHead")
			{
				this.girlWeaponHead = transform.gameObject;
			}
			if (transform.name == "weaponLeftLeg")
			{
				this.girlWeaponLeftLeg = transform.gameObject;
			}
			if (transform.name == "weaponLeftLegUpper")
			{
				this.girlWeaponLeftLegUpper = transform.gameObject;
			}
			if (transform.name == "weaponLeftLegStomper")
			{
				this.girlWeaponLeftLegStomp = transform.gameObject;
			}
			if (transform.name == "weaponRightLeg")
			{
				this.girlWeaponRightLeg = transform.gameObject;
			}
			if (transform.name == "weaponRightLegUpper")
			{
				this.girlWeaponRightLegUpper = transform.gameObject;
			}
			if (transform.name == "weaponRightLegStomper")
			{
				this.girlWeaponRightStomp = transform.gameObject;
			}
			if (transform.name == "weaponRightArm")
			{
				this.girlWeaponRightArm = transform.gameObject;
			}
			if (transform.name == "weaponHeadStomper")
			{
				this.girlWeaponHeadStomp = transform.gameObject;
			}
			if (transform.name == "playerCollisionHead")
			{
				this.headCol = transform.GetComponent<Collider>();
			}
			if (transform.name == "playerCollisionLeftLeg1")
			{
				this.leftLeg1Col = transform.GetComponent<Collider>();
			}
			if (transform.name == "playerCollisionLeftLeg2")
			{
				this.leftLeg2Col = transform.GetComponent<Collider>();
			}
			if (transform.name == "playerCollisionRightLeg1")
			{
				this.rightLeg1Col = transform.GetComponent<Collider>();
			}
			if (transform.name == "playerCollisionRightLeg2")
			{
				this.rightLeg2Col = transform.GetComponent<Collider>();
			}
		}
	}

	
	private void OnDisable()
	{
		this.stopBossFightMusic();
	}

	
	public void doWalkSplash()
	{
		if (this.waterDetect == null)
		{
			return;
		}
		if (this.waterDetect.currentWaterCollider)
		{
			Vector3 position = this.feetAudioGo.transform.position;
			position.x += UnityEngine.Random.Range(-0.3f, 0.3f);
			position.z += UnityEngine.Random.Range(-0.3f, 0.3f);
			position.y = this.waterDetect.currentWaterCollider.bounds.center.y * this.waterDetect.currentWaterCollider.transform.localScale.y + this.waterDetect.currentWaterCollider.bounds.extents.y + 0.4f;
			PoolManager.Pools["Particles"].Spawn(this.waterSplash.transform, position, Quaternion.identity);
		}
	}

	
	private void disableDamageBool(bool setDamage)
	{
		if (this.netPrefab)
		{
			return;
		}
		this.animator.SetBoolReflected("damageBOOL", false);
		if (UnityEngine.Random.Range(0, 5) < 2)
		{
		}
	}

	
	private void enableWeapon()
	{
		if (this.weaponLeft)
		{
			this.weaponLeft.GetComponent<Collider>().enabled = true;
		}
		if (this.weaponLeft1)
		{
			this.weaponLeft1.GetComponent<Collider>().enabled = true;
		}
		if (this.weaponRight)
		{
			this.weaponRight.GetComponent<Collider>().enabled = true;
		}
	}

	
	public void disableWeapon()
	{
		if (this.weaponLeft)
		{
			this.weaponLeft.GetComponent<Collider>().enabled = false;
		}
		if (this.weaponLeft1)
		{
			this.weaponLeft1.GetComponent<Collider>().enabled = false;
		}
		if (this.weaponRight)
		{
			this.weaponRight.GetComponent<Collider>().enabled = false;
		}
	}

	
	private void enableLeftWeapon()
	{
		if (this.weaponLeft)
		{
			this.weaponLeft.GetComponent<Collider>().enabled = true;
		}
		if (this.weaponLeft1)
		{
			this.weaponLeft1.GetComponent<Collider>().enabled = true;
		}
	}

	
	private void disableLeftWeapon()
	{
		if (this.weaponLeft)
		{
			this.weaponLeft.GetComponent<Collider>().enabled = false;
		}
		if (this.weaponLeft1)
		{
			this.weaponLeft1.GetComponent<Collider>().enabled = false;
		}
	}

	
	private void enableRightWeapon()
	{
		if (this.weaponRight)
		{
			this.weaponRight.GetComponent<Collider>().enabled = true;
		}
	}

	
	private void disableRightWeapon()
	{
		if (this.weaponRight)
		{
			this.weaponRight.GetComponent<Collider>().enabled = false;
		}
	}

	
	private void enableMainWeapon()
	{
		if (this.weaponMainCollider)
		{
			this.weaponMainCollider.enabled = true;
		}
		base.Invoke("disableMainWeapon", 2f);
	}

	
	private void disableMainWeapon()
	{
		if (this.weaponMainCollider)
		{
			this.weaponMainCollider.enabled = false;
		}
	}

	
	private void disableAllWeapons()
	{
		if (this.weaponMainCollider)
		{
			this.weaponMainCollider.enabled = false;
		}
		if (this.weaponLeft)
		{
			this.weaponLeft.GetComponent<Collider>().enabled = false;
		}
		if (this.weaponLeft1)
		{
			this.weaponLeft1.GetComponent<Collider>().enabled = false;
		}
		if (this.weaponRight)
		{
			this.weaponRight.GetComponent<Collider>().enabled = false;
		}
	}

	
	private void footStomp()
	{
		if (!LocalPlayer.Transform)
		{
			return;
		}
		if (this.netPrefab)
		{
			float num = Vector3.Distance(LocalPlayer.Transform.position, base.transform.position);
			if (num < 26f)
			{
				LocalPlayer.HitReactions.enableFootShake(num, 0.3f);
			}
			return;
		}
		if (Vector3.Distance(LocalPlayer.Transform.position, base.transform.position) < 26f)
		{
			LocalPlayer.HitReactions.enableFootShake(this.setup.ai.playerDist, 0.3f);
		}
	}

	
	private void footStompFast()
	{
		if (!LocalPlayer.Transform)
		{
			return;
		}
		if (this.netPrefab)
		{
			float num = Vector3.Distance(LocalPlayer.Transform.position, base.transform.position);
			if (num < 26f)
			{
				LocalPlayer.HitReactions.enableFootShake(num, 0.15f);
			}
			return;
		}
		if (Vector3.Distance(LocalPlayer.Transform.position, base.transform.position) < 26f)
		{
			LocalPlayer.HitReactions.enableFootShake(this.setup.ai.playerDist, 0.15f);
		}
	}

	
	private void disableHeadPlayerColliders(float t)
	{
		this.headCol.enabled = false;
		base.Invoke("enableHeadPlayerColliders", t);
	}

	
	private void enableHeadPlayerColliders()
	{
		this.headCol.enabled = true;
	}

	
	private void disableLegPlayerColliders(float t)
	{
		this.leftLeg1Col.enabled = false;
		this.leftLeg2Col.enabled = false;
		this.rightLeg1Col.enabled = false;
		this.rightLeg2Col.enabled = false;
		base.Invoke("enableLegPlayerColliders", t);
	}

	
	private void enableLegPlayerColliders()
	{
		this.leftLeg1Col.enabled = true;
		this.leftLeg2Col.enabled = true;
		this.rightLeg1Col.enabled = true;
		this.rightLeg2Col.enabled = true;
	}

	
	private void enableWeaponLeftLeg()
	{
		base.CancelInvoke("enableLegPlayerColliders");
		if (this.animator.GetCurrentAnimatorStateInfo(0).shortNameHash == this.longSpinAttackHash)
		{
			this.disableLegPlayerColliders(2f);
		}
		else
		{
			this.disableLegPlayerColliders(0.75f);
		}
		this.girlWeaponLeftLegUpper.GetComponent<Collider>().enabled = true;
		this.girlWeaponLeftLeg.GetComponent<Collider>().enabled = true;
	}

	
	private void disableWeaponLeftLeg()
	{
		this.girlWeaponLeftLegUpper.GetComponent<Collider>().enabled = false;
		this.girlWeaponLeftLeg.GetComponent<Collider>().enabled = false;
	}

	
	private void enableWeaponRightLeg()
	{
		base.CancelInvoke("enableLegPlayerColliders");
		if (this.animator.GetCurrentAnimatorStateInfo(0).shortNameHash == this.longSpinAttackHash)
		{
			this.disableLegPlayerColliders(2f);
		}
		else
		{
			this.disableLegPlayerColliders(0.75f);
		}
		this.girlWeaponRightLegUpper.GetComponent<Collider>().enabled = true;
		this.girlWeaponRightLeg.GetComponent<Collider>().enabled = true;
	}

	
	private void disableWeaponRightLeg()
	{
		this.girlWeaponRightLegUpper.GetComponent<Collider>().enabled = false;
		this.girlWeaponRightLeg.GetComponent<Collider>().enabled = false;
	}

	
	private void enableWeaponHead()
	{
		this.disableHeadPlayerColliders(0.6f);
		this.girlWeaponHead.GetComponent<Collider>().enabled = true;
	}

	
	private void disableWeaponHead()
	{
		this.girlWeaponHead.GetComponent<Collider>().enabled = false;
	}

	
	private void enableStompLeftLeg()
	{
		base.CancelInvoke("enableLegPlayerColliders");
		this.disableLegPlayerColliders(0.75f);
		this.girlWeaponLeftLegStomp.GetComponent<Collider>().enabled = true;
		base.Invoke("disableStompLeftLeg", 0.5f);
	}

	
	private void disableStompLeftLeg()
	{
		this.girlWeaponLeftLegStomp.GetComponent<Collider>().enabled = false;
	}

	
	private void enableStompRightLeg()
	{
		base.CancelInvoke("enableLegPlayerColliders");
		this.disableLegPlayerColliders(0.75f);
		this.girlWeaponRightStomp.GetComponent<Collider>().enabled = true;
		base.Invoke("disableStompRightLeg", 0.5f);
	}

	
	private void disableStompRightLeg()
	{
		this.girlWeaponRightStomp.GetComponent<Collider>().enabled = false;
	}

	
	private void enableStompHead()
	{
		base.Invoke("disableStompHead", 0.5f);
		this.girlWeaponHeadStomp.GetComponent<Collider>().enabled = true;
	}

	
	private void disableStompHead()
	{
		this.girlWeaponHeadStomp.GetComponent<Collider>().enabled = false;
	}

	
	private void enableRagDoll()
	{
		if (this.netPrefab)
		{
			return;
		}
		this.ragDollSetup.metgoragdoll(default(Vector3));
		if (PoolManager.Pools["enemies"].IsSpawned(base.transform.parent))
		{
			PoolManager.Pools["enemies"].Despawn(base.transform.parent);
		}
		else
		{
			UnityEngine.Object.Destroy(base.transform.root.gameObject);
		}
	}

	
	private void playerSighted()
	{
		FMODCommon.PlayOneshot(this.playerSightedSound, base.transform);
	}

	
	private void birthLeft()
	{
		if (this.netPrefab)
		{
			return;
		}
		GameObject item = UnityEngine.Object.Instantiate<GameObject>(this.babySpawnPrefab, this.leftBirthTr.position, Quaternion.identity);
		if (this.girlAi)
		{
			this.girlAi.spawnedBabies.Add(item);
		}
	}

	
	private void birthRight()
	{
		if (this.netPrefab)
		{
			return;
		}
		GameObject item = UnityEngine.Object.Instantiate<GameObject>(this.babySpawnPrefab, this.rightBirthTr.position, Quaternion.identity);
		if (this.girlAi)
		{
			this.girlAi.spawnedBabies.Add(item);
		}
	}

	
	private void dropToy()
	{
		if (this.toyPlane)
		{
			this.toyPlane.transform.parent = null;
		}
	}

	
	private void startGirlIdleSound()
	{
		if (this.girlIdleEmitter != null)
		{
			this.girlIdleEmitter.enabled = true;
		}
	}

	
	private void stopGirlIdleSound()
	{
		if (this.girlIdleEmitter != null)
		{
			this.girlIdleEmitter.enabled = false;
		}
	}

	
	private void startBossFightMusic()
	{
		if (this.bossFightEvent == null && !string.IsNullOrEmpty(this.bossFightMusic))
		{
			this.bossFightEvent = FMODCommon.PlayOneshot(this.bossFightMusic, base.transform);
		}
	}

	
	private void stopBossFightMusic()
	{
		if (this.bossFightEvent != null)
		{
			UnityUtil.ERRCHECK(this.bossFightEvent.stop(STOP_MODE.ALLOWFADEOUT));
			UnityUtil.ERRCHECK(this.bossFightEvent.release());
			this.bossFightEvent = null;
		}
	}

	
	private mutantVis vis;

	
	private mutantWaterDetect waterDetect;

	
	private Animator animator;

	
	private mutantScriptSetup setup;

	
	private clsragdollify ragDollSetup;

	
	private girlMutantAiManager girlAi;

	
	private GameObject weaponLeft;

	
	private GameObject weaponLeft1;

	
	private GameObject weaponRight;

	
	public GameObject toyPlane;

	
	public Transform leftBirthTr;

	
	public Transform rightBirthTr;

	
	public GameObject babySpawnPrefab;

	
	private GameObject girlWeaponHead;

	
	private GameObject girlWeaponRightArm;

	
	private GameObject girlWeaponLeftLeg;

	
	private GameObject girlWeaponLeftLegUpper;

	
	private GameObject girlWeaponLeftLegStomp;

	
	private GameObject girlWeaponRightLeg;

	
	private GameObject girlWeaponRightLegUpper;

	
	private GameObject girlWeaponRightStomp;

	
	private GameObject girlWeaponHeadStomp;

	
	private GameObject girlCollisionHead;

	
	private GameObject girlCollisionLeftLeg;

	
	private GameObject girlCollisionRightLeg;

	
	private GameObject girlCollisionLeftLegUpper;

	
	private GameObject girlCollisionRightLegUpper;

	
	private Collider headCol;

	
	private Collider leftLeg1Col;

	
	private Collider leftLeg2Col;

	
	private Collider rightLeg1Col;

	
	private Collider rightLeg2Col;

	
	public GameObject weaponMain;

	
	public Collider weaponMainCollider;

	
	public GameObject waterSplash;

	
	public GameObject feetAudioGo;

	
	public bool netPrefab;

	
	private int longSpinAttackHash = Animator.StringToHash("attackSpinLong");

	
	public bool parry;

	
	public FMOD_StudioEventEmitter girlIdleEmitter;

	
	[Header("FMOD Events")]
	public string playerSightedSound;

	
	public string bossFightMusic;

	
	private EventInstance bossFightEvent;
}
