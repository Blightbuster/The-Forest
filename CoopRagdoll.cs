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
		}
	}

	
	public override void Attached()
	{
		if (!this.entity.isOwner)
		{
			CoopMutantDummyToken coopMutantDummyToken = this.entity.attachToken as CoopMutantDummyToken;
			if (coopMutantDummyToken != null)
			{
				animalSkinSetup component = base.transform.GetComponent<animalSkinSetup>();
				if (component)
				{
					MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
					component.skin.GetPropertyBlock(materialPropertyBlock);
					materialPropertyBlock.SetFloat("_Damage1", coopMutantDummyToken.skinDamage1);
					materialPropertyBlock.SetFloat("_Damage2", coopMutantDummyToken.skinDamage2);
					materialPropertyBlock.SetFloat("_Damage3", coopMutantDummyToken.skinDamage3);
					materialPropertyBlock.SetFloat("_Damage4", coopMutantDummyToken.skinDamage4);
					component.skin.SetPropertyBlock(materialPropertyBlock);
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

	
	public Material snowMat;

	
	public bool animalArrowSync;
}
