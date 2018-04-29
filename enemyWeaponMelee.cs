using System;
using Bolt;
using HutongGames.PlayMaker;
using TheForest.Utils;
using TheForest.Utils.Settings;
using TheForest.World;
using UnityEngine;


public class enemyWeaponMelee : MonoBehaviour
{
	
	private void OnDeserialized()
	{
		this.doAwake();
	}

	
	private void Awake()
	{
		this.doAwake();
	}

	
	private void doAwake()
	{
		this.enemyHitMask = 36841472;
		this.rootTr = base.transform.root;
		this.thisCollider = base.transform.GetComponent<Collider>();
		if (!this.netPrefab)
		{
			this.setup = base.transform.root.GetComponentInChildren<mutantScriptSetup>();
			this.ai = base.transform.root.GetComponentInChildren<mutantAI>();
		}
		if (this.netPrefab)
		{
			this.hitPrediction = base.transform.root.GetComponent<CoopMutantClientHitPrediction>();
			this.ai_net = base.transform.root.GetComponentInChildren<mutantAI_net>();
		}
		this.props = base.transform.root.GetComponentInChildren<mutantPropManager>();
		this.animator = base.transform.root.GetComponentInChildren<Animator>();
		this.events = base.transform.root.GetComponentInChildren<enemyAnimEvents>();
		this.blockHash = Animator.StringToHash("block");
		FMODCommon.PreloadEvents(new string[]
		{
			this.weaponHitEvent,
			this.parryEvent,
			this.blockEvent,
			this.shellBlockEvent
		});
		this.hasPreloaded = true;
	}

	
	private void OnDisable()
	{
		if (this.hasPreloaded)
		{
			FMODCommon.UnloadEvents(new string[]
			{
				this.weaponHitEvent,
				this.parryEvent,
				this.blockEvent,
				this.shellBlockEvent
			});
			this.hasPreloaded = false;
		}
	}

	
	private void Start()
	{
		if (!this.netPrefab && this.mainTrigger)
		{
			this.fsmAttackStructure = this.setup.pmCombat.FsmVariables.GetFsmBool("attackStructure");
		}
		base.Invoke("setupAttackerType", 1f);
	}

	
	private void setupAttackerType()
	{
		if (this.netPrefab)
		{
			this.leader = this.ai_net.leader;
			this.maleSkinny = this.ai_net.maleSkinny;
			this.femaleSkinny = this.ai_net.femaleSkinny;
			this.male = this.ai_net.male;
			this.female = this.ai_net.female;
			this.creepy = this.ai_net.creepy;
			this.creepy_male = this.ai_net.creepy_male;
			this.creepy_baby = this.ai_net.creepy_baby;
			this.creepy_fat = this.ai_net.creepy_fat;
			this.firemanMain = this.ai_net.fireman;
			this.pale = this.ai_net.pale;
			this.painted = this.ai_net.painted;
			this.skinned = this.ai_net.skinned;
		}
		else
		{
			this.leader = this.ai.leader;
			this.maleSkinny = this.ai.maleSkinny;
			this.femaleSkinny = this.ai.femaleSkinny;
			this.male = this.ai.male;
			this.female = this.ai.female;
			this.creepy = this.ai.creepy;
			this.creepy_male = this.ai.creepy_male;
			this.creepy_baby = this.ai.creepy_baby;
			this.creepy_fat = this.ai.creepy_fat;
			this.firemanMain = this.ai.fireman;
			this.pale = this.ai.pale;
			this.painted = this.ai.painted;
			this.skinned = this.ai.skinned;
			this.bodyCollider = this.setup.bodyCollider;
		}
		if (this.maleSkinny || this.femaleSkinny)
		{
			this.attackerType = 1;
		}
		else if (this.pale && !this.skinned)
		{
			this.attackerType = 2;
		}
		else if (this.male || this.female)
		{
			this.attackerType = 0;
		}
		if (this.creepy || this.creepy_male)
		{
			this.attackerType = 3;
		}
	}

	
	private void Update()
	{
		if (this.thisCollider && !this.thisCollider.enabled)
		{
			this.enemyAtStructure = false;
		}
	}

	
	private void OnTriggerEnter(Collider other)
	{
		this.currState = this.animator.GetCurrentAnimatorStateInfo(0);
		this.nextState = this.animator.GetNextAnimatorStateInfo(0);
		if (this.currState.tagHash == this.damagedHash || this.currState.tagHash == this.staggerHash || this.currState.tagHash == this.hitStaggerHash || this.currState.tagHash == this.deathHash || this.nextState.tagHash == this.damagedHash || this.nextState.tagHash == this.staggerHash || this.nextState.tagHash == this.hitStaggerHash || this.nextState.tagHash == this.deathHash)
		{
			return;
		}
		if (other.gameObject.CompareTag("trapTrigger"))
		{
			other.gameObject.SendMessage("CutRope", SendMessageOptions.DontRequireReceiver);
		}
		if (!this.netPrefab && LocalPlayer.Animator && LocalPlayer.Animator.GetBool("deathBool"))
		{
			return;
		}
		if (other.gameObject.CompareTag("playerHitDetect") && this.mainTrigger)
		{
			if (!Scene.SceneTracker.hasAttackedPlayer)
			{
				Scene.SceneTracker.hasAttackedPlayer = true;
				Scene.SceneTracker.Invoke("resetHasAttackedPlayer", (float)UnityEngine.Random.Range(120, 240));
			}
			targetStats component = other.transform.root.GetComponent<targetStats>();
			if (component && component.targetDown)
			{
				return;
			}
			Animator componentInParent = other.gameObject.GetComponentInParent<Animator>();
			Vector3 position = this.rootTr.position;
			position.y += 3.3f;
			Vector3 direction = other.transform.position - position;
			RaycastHit[] array = Physics.RaycastAll(position, direction, direction.magnitude, this.enemyHitMask);
			for (int i = 0; i < array.Length; i++)
			{
				if ((array[i].transform.gameObject.layer == 11 || array[i].transform.gameObject.layer == 13 || array[i].transform.gameObject.layer == 17 || array[i].transform.gameObject.layer == 20 || array[i].transform.gameObject.layer == 21 || array[i].transform.gameObject.layer == 25) && !array[i].collider.isTrigger)
				{
					return;
				}
			}
			if (!this.creepy_male && !this.creepy && !this.creepy_baby && !this.creepy_fat && this.events && componentInParent)
			{
				bool flag = this.InFront(other.gameObject);
				if (!BoltNetwork.isServer || !this.netPrefab)
				{
					if (flag && this.events.parryBool && (componentInParent.GetNextAnimatorStateInfo(1).tagHash == this.blockHash || componentInParent.GetCurrentAnimatorStateInfo(1).tagHash == this.blockHash))
					{
						int parryDir = this.events.parryDir;
						this.animator.SetIntegerReflected("parryDirInt", parryDir);
						if (BoltNetwork.isClient && this.netPrefab)
						{
							this.animator.SetTriggerReflected("ClientParryTrigger");
							this.hitPrediction.StartParryPrediction();
							parryEnemy parryEnemy = parryEnemy.Create(GlobalTargets.OnlyServer);
							parryEnemy.Target = base.transform.root.GetComponent<BoltEntity>();
							parryEnemy.Send();
							FMODCommon.PlayOneshot(this.parryEvent, base.transform);
						}
						else
						{
							this.animator.SetTriggerReflected("parryTrigger");
						}
						this.events.StartCoroutine("disableAllWeapons");
						playerHitReactions componentInParent2 = other.gameObject.GetComponentInParent<playerHitReactions>();
						if (componentInParent2 != null)
						{
							componentInParent2.enableParryState();
						}
						FMODCommon.PlayOneshotNetworked(this.parryEvent, base.transform, FMODCommon.NetworkRole.Server);
						this.events.parryBool = false;
						return;
					}
				}
			}
			if (this.events)
			{
				this.events.parryBool = false;
			}
			other.transform.root.SendMessage("getHitDirection", this.rootTr.position, SendMessageOptions.DontRequireReceiver);
			int num = 0;
			if (this.maleSkinny || this.femaleSkinny)
			{
				if (this.pale)
				{
					if (this.skinned)
					{
						num = Mathf.FloorToInt(10f * GameSettings.Ai.skinnyDamageRatio * GameSettings.Ai.skinMaskDamageRatio);
					}
					else
					{
						num = Mathf.FloorToInt(10f * GameSettings.Ai.skinnyDamageRatio);
					}
				}
				else
				{
					num = Mathf.FloorToInt(13f * GameSettings.Ai.skinnyDamageRatio);
					if (this.maleSkinny && this.props.regularStick.activeSelf && this.events.leftHandWeapon)
					{
						num = Mathf.FloorToInt((float)num * 1.35f);
					}
				}
			}
			else if (this.male && this.pale)
			{
				if (this.skinned)
				{
					num = Mathf.FloorToInt(22f * GameSettings.Ai.largePaleDamageRatio * GameSettings.Ai.skinMaskDamageRatio);
				}
				else
				{
					num = Mathf.FloorToInt(22f * GameSettings.Ai.largePaleDamageRatio);
				}
			}
			else if (this.male && !this.firemanMain)
			{
				if (this.painted)
				{
					num = Mathf.FloorToInt(20f * GameSettings.Ai.regularMaleDamageRatio * GameSettings.Ai.paintedDamageRatio);
				}
				else
				{
					num = Mathf.FloorToInt(20f * GameSettings.Ai.regularMaleDamageRatio);
				}
			}
			else if (this.female)
			{
				num = Mathf.FloorToInt(17f * GameSettings.Ai.regularFemaleDamageRatio);
			}
			else if (this.creepy)
			{
				if (this.pale)
				{
					num = Mathf.FloorToInt(35f * GameSettings.Ai.creepyDamageRatio);
				}
				else
				{
					num = Mathf.FloorToInt(28f * GameSettings.Ai.creepyDamageRatio);
				}
			}
			else if (this.creepy_male)
			{
				if (this.pale)
				{
					num = Mathf.FloorToInt(40f * GameSettings.Ai.creepyDamageRatio);
				}
				else
				{
					num = Mathf.FloorToInt(30f * GameSettings.Ai.creepyDamageRatio);
				}
			}
			else if (this.creepy_baby)
			{
				num = Mathf.FloorToInt(26f * GameSettings.Ai.creepyBabyDamageRatio);
			}
			else if (this.firemanMain)
			{
				num = Mathf.FloorToInt(12f * GameSettings.Ai.regularMaleDamageRatio);
				if (this.events && !this.enemyAtStructure && !this.events.noFireAttack)
				{
					if (BoltNetwork.isRunning && this.netPrefab)
					{
						other.gameObject.SendMessageUpwards("Burn", SendMessageOptions.DontRequireReceiver);
					}
					else
					{
						other.gameObject.SendMessageUpwards("Burn", SendMessageOptions.DontRequireReceiver);
					}
				}
			}
			if (!this.female && this.male)
			{
				if (this.holdingRegularWeapon() && this.events.leftHandWeapon)
				{
					num += 7;
				}
				else if (this.holdingAdvancedWeapon() && this.events.leftHandWeapon)
				{
					num += 15;
				}
			}
			if (this.setup && this.setup.health.poisoned)
			{
				num = Mathf.FloorToInt((float)num / 1.6f);
			}
			PlayerStats component2 = other.transform.root.GetComponent<PlayerStats>();
			if (this.male || this.female || this.creepy_male || this.creepy_fat || this.creepy || this.creepy_baby)
			{
				netId component3 = other.transform.GetComponent<netId>();
				if (BoltNetwork.isServer && component3)
				{
					other.transform.root.SendMessage("StartPrediction", SendMessageOptions.DontRequireReceiver);
					return;
				}
				if (BoltNetwork.isClient && this.netPrefab && !component3)
				{
					other.transform.root.SendMessage("setCurrentAttacker", this, SendMessageOptions.DontRequireReceiver);
					other.transform.root.SendMessage("hitFromEnemy", num, SendMessageOptions.DontRequireReceiver);
					other.transform.root.SendMessage("StartPrediction", SendMessageOptions.DontRequireReceiver);
				}
				else if (BoltNetwork.isServer)
				{
					if (!component3)
					{
						other.transform.root.SendMessage("setCurrentAttacker", this, SendMessageOptions.DontRequireReceiver);
						other.transform.root.SendMessage("hitFromEnemy", num, SendMessageOptions.DontRequireReceiver);
					}
				}
				else if (!BoltNetwork.isRunning && component2)
				{
					component2.setCurrentAttacker(this);
					component2.hitFromEnemy(num);
				}
			}
			else if (!this.netPrefab && component2)
			{
				component2.setCurrentAttacker(this);
				component2.hitFromEnemy(num);
			}
		}
		if (other.gameObject.CompareTag("enemyCollide") && this.mainTrigger && this.bodyCollider && !this.enemyAtStructure)
		{
			this.setupAttackerType();
			if (other.gameObject != this.bodyCollider)
			{
				other.transform.SendMessageUpwards("getAttackDirection", UnityEngine.Random.Range(0, 2), SendMessageOptions.DontRequireReceiver);
				other.transform.SendMessageUpwards("getCombo", UnityEngine.Random.Range(1, 4), SendMessageOptions.DontRequireReceiver);
				other.transform.SendMessage("getAttackerType", this.attackerType, SendMessageOptions.DontRequireReceiver);
				other.transform.SendMessage("getAttacker", this.rootTr.gameObject, SendMessageOptions.DontRequireReceiver);
				other.transform.SendMessageUpwards("Hit", 6, SendMessageOptions.DontRequireReceiver);
				FMODCommon.PlayOneshotNetworked(this.weaponHitEvent, base.transform, FMODCommon.NetworkRole.Server);
			}
		}
		if (other.gameObject.CompareTag("BreakableWood") || (other.gameObject.CompareTag("BreakableRock") && this.mainTrigger))
		{
			other.transform.SendMessage("Hit", 50, SendMessageOptions.DontRequireReceiver);
			other.SendMessage("LocalizedHit", new LocalizedHitData(base.transform.position, 50f), SendMessageOptions.DontRequireReceiver);
			FMODCommon.PlayOneshotNetworked(this.weaponHitEvent, base.transform, FMODCommon.NetworkRole.Server);
		}
		if (other.gameObject.CompareTag("SmallTree") && !this.mainTrigger)
		{
			other.SendMessage("Hit", 2, SendMessageOptions.DontRequireReceiver);
		}
		if (other.gameObject.CompareTag("Fire") && this.mainTrigger && this.firemanMain && !this.events.noFireAttack)
		{
			other.SendMessage("Burn", SendMessageOptions.DontRequireReceiver);
		}
		if (other.gameObject.CompareTag("Tree") && this.mainTrigger && this.creepy_male)
		{
			other.SendMessage("Explosion", 5f, SendMessageOptions.DontRequireReceiver);
			FMODCommon.PlayOneshotNetworked(this.weaponHitEvent, base.transform, FMODCommon.NetworkRole.Server);
		}
		if ((other.gameObject.CompareTag("structure") || other.gameObject.CompareTag("SLTier1") || other.gameObject.CompareTag("SLTier2") || other.gameObject.CompareTag("SLTier3") || other.gameObject.CompareTag("jumpObject") || other.gameObject.CompareTag("UnderfootWood")) && this.mainTrigger)
		{
			getStructureStrength component4 = other.gameObject.GetComponent<getStructureStrength>();
			bool flag2 = false;
			if (component4 == null)
			{
				flag2 = true;
			}
			this.enemyAtStructure = true;
			int num2;
			if (this.creepy_male || this.creepy || this.creepy_fat || this.creepy_baby)
			{
				if (this.creepy_baby)
				{
					num2 = Mathf.FloorToInt(10f * GameSettings.Ai.creepyStructureDamageRatio);
				}
				else
				{
					num2 = Mathf.FloorToInt(30f * GameSettings.Ai.creepyStructureDamageRatio);
				}
			}
			else
			{
				if (flag2)
				{
					return;
				}
				if (this.maleSkinny || this.femaleSkinny)
				{
					if (component4._strength == getStructureStrength.strength.weak)
					{
						num2 = Mathf.FloorToInt(8f * GameSettings.Ai.regularStructureDamageRatio);
					}
					else
					{
						num2 = 0;
					}
				}
				else if (this.pale || this.painted || this.skinned)
				{
					if (component4._strength == getStructureStrength.strength.veryStrong)
					{
						num2 = 0;
					}
					else
					{
						num2 = Mathf.FloorToInt(16f * GameSettings.Ai.regularStructureDamageRatio);
					}
				}
				else if (component4._strength == getStructureStrength.strength.veryStrong)
				{
					num2 = 0;
				}
				else
				{
					num2 = Mathf.FloorToInt(12f * GameSettings.Ai.regularStructureDamageRatio);
				}
			}
			if (this.setup && this.setup.health.poisoned)
			{
				num2 /= 2;
			}
			other.SendMessage("Hit", num2, SendMessageOptions.DontRequireReceiver);
			other.SendMessage("LocalizedHit", new LocalizedHitData(base.transform.position, (float)num2), SendMessageOptions.DontRequireReceiver);
			FMODCommon.PlayOneshotNetworked(this.weaponHitEvent, base.transform, FMODCommon.NetworkRole.Server);
		}
	}

	
	private bool holdingRegularWeapon()
	{
		for (int i = 0; i < this.props.regularWeapons.Length; i++)
		{
			if (this.props.regularWeapons[i].activeSelf)
			{
				return true;
			}
		}
		return false;
	}

	
	private bool holdingAdvancedWeapon()
	{
		for (int i = 0; i < this.props.advancedWeapons.Length; i++)
		{
			if (this.props.advancedWeapons[i].activeSelf)
			{
				return true;
			}
		}
		return false;
	}

	
	private bool InFront(GameObject other)
	{
		if (other.GetComponentInParent<FirstPersonCharacter>() == null)
		{
			return true;
		}
		Vector3 forward = other.GetComponentInParent<FirstPersonCharacter>().transform.forward;
		Vector3 vector = this.rootTr.position - other.GetComponentInParent<FirstPersonCharacter>().transform.position;
		Vector2 v = new Vector2(forward.x, forward.z);
		Vector2 v2 = new Vector2(vector.x, vector.z);
		return Vector3.Angle(v, v2) <= 90f;
	}

	
	public bool netPrefab;

	
	public bool mainTrigger;

	
	private mutantScriptSetup setup;

	
	private mutantAI ai;

	
	private mutantAI_net ai_net;

	
	private enemyAnimEvents events;

	
	private Animator animator;

	
	private Transform rootTr;

	
	private CoopMutantClientHitPrediction hitPrediction;

	
	private mutantPropManager props;

	
	private Collider thisCollider;

	
	public GameObject bodyCollider;

	
	private int layerMask;

	
	private int blockHash;

	
	public int attackerType;

	
	public bool behindWallCheck;

	
	private AnimatorStateInfo currState;

	
	private AnimatorStateInfo nextState;

	
	[Header("FMOD")]
	public string weaponHitEvent;

	
	public string parryEvent;

	
	public string blockEvent;

	
	public string shellBlockEvent;

	
	private FsmBool fsmAttackStructure;

	
	private bool hasPreloaded;

	
	public bool enemyAtStructure;

	
	public bool leader;

	
	public bool maleSkinny;

	
	public bool femaleSkinny;

	
	public bool male;

	
	public bool female;

	
	public bool creepy;

	
	public bool creepy_male;

	
	public bool creepy_baby;

	
	public bool creepy_fat;

	
	public bool firemanMain;

	
	public bool pale;

	
	public bool painted;

	
	public bool skinned;

	
	private int enemyHitMask;

	
	private int damagedHash = Animator.StringToHash("damaged");

	
	private int hitStaggerHash = Animator.StringToHash("hitStagger");

	
	private int staggerHash = Animator.StringToHash("stagger");

	
	private int deathHash = Animator.StringToHash("death");
}
