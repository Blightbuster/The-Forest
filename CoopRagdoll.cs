using System;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;


public class CoopRagdoll : CoopBase<IRagdollState>
{
	
	private void findClosestStoredToken()
	{
		float num = float.PositiveInfinity;
		this.closestStoredToken = null;
		for (int i = 0; i < Scene.SceneTracker.storedRagDollPrefabs.Count; i++)
		{
			if (Scene.SceneTracker.storedRagDollPrefabs[i] != null)
			{
				float num2 = Vector3.Distance(base.transform.position, Scene.SceneTracker.storedRagDollPrefabs[i].transform.position);
				if (num2 < num)
				{
					num = num2;
					this.closestStoredToken = Scene.SceneTracker.storedRagDollPrefabs[i];
				}
			}
		}
	}

	
	private void setupSnowSkin()
	{
		if (this.closestStoredToken == null)
		{
			return;
		}
		storeLocalMutantInfo2 component = this.closestStoredToken.transform.GetComponent<storeLocalMutantInfo2>();
		if (this.closestStoredToken && component.isSnow)
		{
			animalSkinSetup component2 = base.transform.GetComponent<animalSkinSetup>();
			if (component2 && this.snowMat)
			{
				component2.skin.sharedMaterial = this.snowMat;
			}
		}
	}

	
	private void setupSnowRabbitTypeTrigger()
	{
		AnimalTypeTrigger componentInChildren = base.transform.GetComponentInChildren<AnimalTypeTrigger>(true);
		if (componentInChildren && componentInChildren._type == AnimalType.EuropeanRabbit)
		{
			componentInChildren._type = AnimalType.snowRabbit;
		}
	}

	
	private void transferStuckArrows()
	{
		if (this.animalArrowSync)
		{
			if (this.closestStoredToken == null)
			{
				return;
			}
			storeLocalMutantInfo2 component = this.closestStoredToken.transform.GetComponent<storeLocalMutantInfo2>();
			if (this.closestStoredToken)
			{
				if (!this.ast)
				{
					this.ast = base.transform.GetComponent<arrowStickToTarget>();
				}
				if (!this.ast)
				{
					arrowStickToTarget[] componentsInChildren = base.transform.GetComponentsInChildren<arrowStickToTarget>(true);
					if (componentsInChildren[0])
					{
						this.ast = componentsInChildren[0];
					}
				}
				if (this.ast)
				{
					int num = 0;
					foreach (KeyValuePair<Transform, int> keyValuePair in component.stuckArrowsIndex)
					{
						if (keyValuePair.Key)
						{
							if (this.ast.singleJointMode)
							{
								keyValuePair.Key.parent = this.ast.baseJoint;
								keyValuePair.Key.localPosition = Vector3.zero;
								keyValuePair.Key.localRotation = component.stuckArrowRot[num];
							}
							else
							{
								keyValuePair.Key.parent = this.ast.stickToJoints[keyValuePair.Value];
								keyValuePair.Key.localPosition = component.stuckArrowPos[num];
								keyValuePair.Key.localRotation = component.stuckArrowRot[num];
							}
							fakeArrowSetup component2 = keyValuePair.Key.GetComponent<fakeArrowSetup>();
							if (component2 && BoltNetwork.isRunning)
							{
								component2.storedIndex = this.ast.stuckArrows.Count;
								component2.entityTarget = base.transform.root.GetComponent<BoltEntity>();
							}
							int count = this.ast.stuckArrows.Count;
							this.ast.stuckArrows.Add(keyValuePair.Key, count);
							num++;
						}
					}
				}
				UnityEngine.Object.Destroy(this.closestStoredToken);
				Scene.SceneTracker.storedRagDollPrefabs.RemoveAll((GameObject o) => o == null);
				Scene.SceneTracker.storedRagDollPrefabs.TrimExcess();
			}
		}
	}

	
	private void transferClientFire()
	{
		if (this.closestStoredToken == null)
		{
			return;
		}
		mutantTransferFire component = base.transform.GetComponent<mutantTransferFire>();
		if (component && component.allBones.Count > 0)
		{
			storeLocalMutantInfo2 component2 = this.closestStoredToken.transform.GetComponent<storeLocalMutantInfo2>();
			if (component2 == null || component2.fire.Count == 0)
			{
				return;
			}
			for (int i = 0; i < component2.fire.Count; i++)
			{
				if (component2.fire[i] && component2.fire[i].gameObject.activeSelf)
				{
					component2.fire[i].parent = component.allBones[i];
					component2.fire[i].localPosition = component2.firePos[i];
					component2.fire[i].localRotation = component2.fireRot[i];
				}
			}
		}
	}

	
	private void Start()
	{
		if (BoltNetwork.isRunning && BoltNetwork.isClient)
		{
			this.findClosestStoredToken();
			this.setupSnowSkin();
			if (this.animalArrowSync)
			{
				this.transferStuckArrows();
			}
			if (this.animalFireSync)
			{
				this.transferClientFire();
			}
		}
	}

	
	public override void Attached()
	{
		if (!base.entity.isOwner)
		{
			CoopMutantDummyToken coopMutantDummyToken = base.entity.attachToken as CoopMutantDummyToken;
			CoopRagdollToken coopRagdollToken = base.entity.attachToken as CoopRagdollToken;
			if (coopRagdollToken != null && coopRagdollToken.onFireApplied)
			{
				doBurn component = base.transform.GetComponent<doBurn>();
				if (component && component.fire)
				{
					component.fire.SetActive(true);
				}
			}
			if (coopMutantDummyToken != null)
			{
				animalSkinSetup component2 = base.transform.GetComponent<animalSkinSetup>();
				if (component2)
				{
					MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
					component2.skin.GetPropertyBlock(materialPropertyBlock);
					materialPropertyBlock.SetFloat("_Damage1", coopMutantDummyToken.skinDamage1);
					materialPropertyBlock.SetFloat("_Damage2", coopMutantDummyToken.skinDamage2);
					materialPropertyBlock.SetFloat("_Damage3", coopMutantDummyToken.skinDamage3);
					materialPropertyBlock.SetFloat("_Damage4", coopMutantDummyToken.skinDamage4);
					component2.skin.SetPropertyBlock(materialPropertyBlock);
				}
			}
		}
	}

	
	private void setSkinnedState()
	{
		SkinAnimal componentInChildren = base.transform.GetComponentInChildren<SkinAnimal>();
		if (componentInChildren)
		{
			componentInChildren.SetSkinnedMP();
		}
	}

	
	[SerializeField]
	private Rigidbody rigidbody;

	
	private arrowStickToTarget ast;

	
	private GameObject closestStoredToken;

	
	private CoopRagdollToken ragdollToken;

	
	public Material snowMat;

	
	public bool animalArrowSync;

	
	public bool animalFireSync;
}
