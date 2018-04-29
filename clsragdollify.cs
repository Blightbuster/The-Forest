using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class clsragdollify : MonoBehaviour
{
	
	private void Awake()
	{
		this.bloodPropertyBlock = new MaterialPropertyBlock();
		if (this.fish)
		{
			this.fishScript = base.transform.GetComponent<Fish>();
		}
		if (this.doCreepySkin)
		{
			this.enemyHealth = base.transform.GetComponent<EnemyHealth>();
		}
		this.animalHealth = base.GetComponentInParent<animalHealth>();
		this.ca = base.GetComponentInParent<CoopAnimal>();
		this.ast = base.transform.GetComponentInChildren<arrowStickToTarget>();
		if (!this.ast && base.transform.parent)
		{
			this.ast = base.transform.parent.GetComponentInChildren<arrowStickToTarget>();
		}
	}

	
	private void metcopytransforms(Transform varpsource, Transform varpdestination, Vector3 varpvelocity = default(Vector3))
	{
		varpdestination.position = varpsource.position;
		varpdestination.rotation = varpsource.rotation;
		if (varpvelocity != Vector3.zero)
		{
			Rigidbody component = varpdestination.GetComponent<Rigidbody>();
			if (component != null && this.hitTr != null)
			{
				component.velocity = varpvelocity * 1.5f + this.hitTr.right * UnityEngine.Random.Range(-5f, 5f);
				if (component.GetComponent<ConstantForce>() && this.spinRagdoll)
				{
					component.GetComponent<ConstantForce>().torque = Vector3.up * 1E+08f + base.transform.right * 1000000f;
					this.spinRagdoll = false;
				}
			}
		}
		if (!this.hackVelocity)
		{
			IEnumerator enumerator = varpdestination.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					Transform transform2 = varpsource.Find(transform.name);
					if (transform2)
					{
						this.metcopytransforms(transform2, transform, varpvelocity * 1f);
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}
	}

	
	public Transform metgoragdoll(Vector3 varpvelocity = default(Vector3))
	{
		Transform transform = UnityEngine.Object.Instantiate<Transform>(this.vargamragdoll, base.transform.position, base.transform.rotation);
		if (!this.ignoreScale)
		{
			transform.localScale = base.transform.localScale;
		}
		this.metcopytransforms(base.transform, transform, varpvelocity * 1f);
		if (this.doCreepySkin && this.enemyHealth)
		{
			transform.gameObject.SendMessage("setSkin", this.enemyHealth.MySkin.sharedMaterial, SendMessageOptions.DontRequireReceiver);
			this.enemyHealth.MySkin.GetPropertyBlock(this.bloodPropertyBlock);
			transform.gameObject.SendMessage("setSkinDamageProperty", this.bloodPropertyBlock, SendMessageOptions.DontRequireReceiver);
			if (this.enemyHealth.Fire.Length > 0)
			{
				mutantTransferFire component = base.transform.parent.GetComponent<mutantTransferFire>();
				foreach (GameObject gameObject in this.enemyHealth.Fire)
				{
					if (gameObject.activeSelf && component)
					{
						component.transferFireToTarget(gameObject, transform.gameObject);
					}
				}
			}
			if (BoltNetwork.isServer)
			{
				BoltEntity component2 = base.transform.parent.GetComponent<BoltEntity>();
				if (component2)
				{
					IMutantState state = component2.GetState<IMutantState>();
					CoopMutantDummyToken coopMutantDummyToken = new CoopMutantDummyToken();
					coopMutantDummyToken.Scale = base.transform.localScale;
					coopMutantDummyToken.skinDamage1 = this.bloodPropertyBlock.GetFloat("_Damage1");
					coopMutantDummyToken.skinDamage2 = this.bloodPropertyBlock.GetFloat("_Damage2");
					coopMutantDummyToken.skinDamage3 = this.bloodPropertyBlock.GetFloat("_Damage3");
					coopMutantDummyToken.skinDamage4 = this.bloodPropertyBlock.GetFloat("_Damage4");
					coopMutantDummyToken.skinColor = this.enemyHealth.MySkin.sharedMaterial.GetColor("_Color");
					mutantTypeSetup component3 = base.transform.parent.GetComponent<mutantTypeSetup>();
					if (component3)
					{
					}
					BoltNetwork.Attach(transform.gameObject, coopMutantDummyToken);
				}
			}
		}
		if (this.animalHealth && this.animalHealth.mySkin)
		{
			this.animalHealth.mySkin.GetPropertyBlock(this.bloodPropertyBlock);
			float @float = this.bloodPropertyBlock.GetFloat("_Damage1");
			transform.gameObject.SendMessage("setSkinDamageProperty", this.bloodPropertyBlock, SendMessageOptions.DontRequireReceiver);
		}
		if (this.animalHealth && this.animalHealth.Fire)
		{
			mutantTransferFire component4;
			if (base.transform.parent)
			{
				component4 = base.transform.parent.GetComponent<mutantTransferFire>();
			}
			else
			{
				component4 = base.transform.GetComponent<mutantTransferFire>();
			}
			if (this.animalHealth.Fire.activeSelf && component4)
			{
				component4.transferFireToTarget(this.animalHealth.Fire, transform.gameObject);
			}
		}
		if (this.bat && this.burning)
		{
			transform.gameObject.SendMessage("enableFire", SendMessageOptions.DontRequireReceiver);
		}
		if (this.animal)
		{
			animalSpawnFunctions component5 = base.transform.root.GetComponent<animalSpawnFunctions>();
			if (component5)
			{
				transform.gameObject.SendMessage("setSkin", component5.meshRenderer.sharedMaterial, SendMessageOptions.DontRequireReceiver);
			}
			if (this.ca && this.ca.isSnow)
			{
				transform.gameObject.SendMessage("setupSnowRabbitTypeTrigger", SendMessageOptions.DontRequireReceiver);
			}
		}
		if (this.bird)
		{
			if (this.burning)
			{
				transform.gameObject.SendMessage("enableFire", SendMessageOptions.DontRequireReceiver);
			}
			lb_Bird component6 = base.transform.GetComponent<lb_Bird>();
			transform.gameObject.SendMessage("setSkin", component6.skin.sharedMaterial, SendMessageOptions.DontRequireReceiver);
		}
		if (this.fish)
		{
			transform.gameObject.SendMessage("doSkinSetup", this.fishScript.fishTypeInt, SendMessageOptions.DontRequireReceiver);
			transform.gameObject.SendMessage("setupFishType", this.fishScript.fishNatureGuideValue, SendMessageOptions.DontRequireReceiver);
		}
		if (this.alreadyBurnt)
		{
			transform.gameObject.SendMessage("enableBurntSkin", SendMessageOptions.DontRequireReceiver);
		}
		if (this.ast)
		{
			arrowStickToTarget component7 = transform.GetComponent<arrowStickToTarget>();
			if (component7)
			{
				foreach (KeyValuePair<Transform, int> keyValuePair in this.ast.stuckArrows)
				{
					if (keyValuePair.Key)
					{
						component7.CreatureType(this.ast.IsAnimal, this.ast.IsBird, this.ast.IsFish);
						component7.applyStuckArrowToDummy(keyValuePair.Key, keyValuePair.Key.localPosition, keyValuePair.Key.localRotation, keyValuePair.Value);
					}
				}
			}
		}
		this.burning = false;
		this.alreadyBurnt = false;
		return transform;
	}

	
	private void setSkinDamageMP(GameObject ragdoll)
	{
		if (BoltNetwork.isServer)
		{
		}
	}

	
	public Transform vargamragdoll;

	
	public bool doCreepySkin;

	
	public bool animal;

	
	public bool fish;

	
	public bool bird;

	
	public bool bat;

	
	public bool burning;

	
	public bool alreadyBurnt;

	
	public bool spinRagdoll;

	
	public bool hackVelocity;

	
	public Transform hitTr;

	
	public Vector3 getVelocity;

	
	public bool ignoreScale;

	
	private MaterialPropertyBlock bloodPropertyBlock;

	
	private Fish fishScript;

	
	private EnemyHealth enemyHealth;

	
	private animalHealth animalHealth;

	
	private arrowStickToTarget ast;

	
	public CoopAnimal ca;
}
