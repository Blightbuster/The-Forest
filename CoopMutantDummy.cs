using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Utils;
using UnityEngine;


public class CoopMutantDummy : EntityBehaviour<IMutantState>
{
	
	private void Start()
	{
		if (this.debugRagdoll)
		{
			this.updateJointRotations();
		}
		this.mrf = base.transform.GetComponentsInChildren<mutantRagdollFollow>();
		arrowStickToTarget[] componentsInChildren = base.transform.GetComponentsInChildren<arrowStickToTarget>(true);
		if (componentsInChildren != null && componentsInChildren.Length > 0)
		{
			this.ast = componentsInChildren[0];
		}
	}

	
	private void OnDisable()
	{
		this.disableSync();
	}

	
	private void disableSync()
	{
		this.doSync = false;
		this.jointRotations.Clear();
	}

	
	public void updateJointRotations()
	{
		this.jointRotations.Clear();
		for (int i = 0; i < this.ragDollJoints.Length; i++)
		{
			this.jointRotations.Add(this.ragDollJoints[i].localRotation);
			this.doSync = true;
		}
	}

	
	public void syncRagDollJoints(List<Quaternion> joints)
	{
		for (int i = 0; i < joints.Count; i++)
		{
			this.jointRotations.Add(joints[i]);
			this.doSync = true;
		}
	}

	
	private void setRagDollDrop()
	{
		foreach (mutantRagdollFollow mutantRagdollFollow in this.mrf)
		{
			mutantRagdollFollow.setDropped();
		}
	}

	
	public void sendResetRagDoll()
	{
		this.doSync = false;
		this.jointRotations.Clear();
		foreach (mutantRagdollFollow mutantRagdollFollow in this.mrf)
		{
			mutantRagdollFollow.resetRagDollParams();
		}
	}

	
	public void sendSetupRagDoll()
	{
		foreach (mutantRagdollFollow mutantRagdollFollow in this.mrf)
		{
			mutantRagdollFollow.setupRagdollParams();
		}
	}

	
	private IEnumerator syncRagDollPositions()
	{
		yield return new WaitForFixedUpdate();
		float closestDist = float.PositiveInfinity;
		GameObject closestStoredToken = null;
		for (int z = 0; z < Scene.SceneTracker.storedRagDollPrefabs.Count; z++)
		{
			if (Scene.SceneTracker.storedRagDollPrefabs[z] != null)
			{
				float dist = Vector3.Distance(base.transform.position, Scene.SceneTracker.storedRagDollPrefabs[z].transform.position);
				if (dist < closestDist)
				{
					closestDist = dist;
					closestStoredToken = Scene.SceneTracker.storedRagDollPrefabs[z];
				}
			}
		}
		if (closestStoredToken)
		{
			storeLocalMutantInfo2 slmi = closestStoredToken.transform.GetComponent<storeLocalMutantInfo2>();
			if (!this.ast)
			{
				this.ast = base.transform.GetComponent<arrowStickToTarget>();
			}
			if (!this.ast)
			{
				arrowStickToTarget[] allAst = base.transform.GetComponentsInChildren<arrowStickToTarget>(true);
				if (allAst != null && allAst.Length > 0 && allAst[0])
				{
					this.ast = allAst[0];
				}
			}
			if (this.ast)
			{
				int index = 0;
				foreach (KeyValuePair<Transform, int> attachStat in slmi.stuckArrowsIndex)
				{
					if (attachStat.Key)
					{
						attachStat.Key.parent = this.ast.stickToJoints[attachStat.Value];
						attachStat.Key.localPosition = slmi.stuckArrowPos[index];
						attachStat.Key.localRotation = slmi.stuckArrowRot[index];
						fakeArrowSetup fas = attachStat.Key.GetComponent<fakeArrowSetup>();
						if (fas && BoltNetwork.isRunning)
						{
							fas.storedIndex = this.ast.stuckArrows.Count;
							fas.entityTarget = base.transform.root.GetComponent<BoltEntity>();
						}
						int newIndex = this.ast.stuckArrows.Count;
						this.ast.stuckArrows.Add(attachStat.Key, newIndex);
						index++;
					}
				}
			}
			if (slmi.onFire)
			{
				base.transform.SendMessage("enableFire", SendMessageOptions.DontRequireReceiver);
			}
			if (this.ragDollJoints.Length == 0)
			{
				yield break;
			}
			CoopMutantMaterialSync cmms = base.GetComponent<CoopMutantMaterialSync>();
			if (cmms)
			{
				cmms.setSkinColor(slmi.matColor);
			}
			if (this.rootTr)
			{
				this.rootTr.rotation = slmi.rootRotation;
				this.rootTr.position = slmi.rootPosition;
				if (slmi.jointAngles.Count > 0)
				{
					for (int i = 0; i < this.ragDollJoints.Length; i++)
					{
						this.ragDollJoints[i].localRotation = slmi.jointAngles[i];
					}
				}
			}
		}
		yield return null;
		yield break;
	}

	
	private void LateUpdate()
	{
		if (this.doSync)
		{
			for (int i = 0; i < this.ragDollJoints.Length; i++)
			{
				if (this.ragDollJoints.Length == this.jointRotations.Count)
				{
					this.ragDollJoints[i].localRotation = this.jointRotations[i];
				}
			}
		}
	}

	
	public override void Attached()
	{
		if (!this.Creepy)
		{
			base.state.Transform.SetTransforms(base.transform);
		}
		if (!this.entity.isOwner)
		{
			CoopMutantDummyToken coopMutantDummyToken = this.entity.attachToken as CoopMutantDummyToken;
			if (coopMutantDummyToken != null)
			{
				base.transform.localScale = coopMutantDummyToken.Scale;
				CoopMutantMaterialSync component = base.GetComponent<CoopMutantMaterialSync>();
				if (component && coopMutantDummyToken.MaterialIndex >= 0)
				{
					component.ApplyMaterial(coopMutantDummyToken.MaterialIndex);
					component.Disabled = true;
					if (!this.Creepy)
					{
						if (coopMutantDummyToken.OriginalMutant)
						{
							Animator componentInChildren = coopMutantDummyToken.OriginalMutant.GetComponentInChildren<Animator>();
							AnimatorStateInfo currentAnimatorStateInfo = componentInChildren.GetCurrentAnimatorStateInfo(0);
							if (this.Replicator)
							{
								this.Replicator.ApplyHashToRemote(0, currentAnimatorStateInfo.fullPathHash, false, currentAnimatorStateInfo.normalizedTime, true);
							}
						}
						dummyAnimatorControl component2 = base.GetComponent<dummyAnimatorControl>();
						if (component2)
						{
							component2.hips.position = coopMutantDummyToken.HipPosition;
							component2.hips.rotation = coopMutantDummyToken.HipRotation;
						}
						float num = float.PositiveInfinity;
						GameObject gameObject = null;
						for (int i = 0; i < Scene.SceneTracker.storedRagDollPrefabs.Count; i++)
						{
							if (Scene.SceneTracker.storedRagDollPrefabs[i] != null)
							{
								float num2 = Vector3.Distance(base.transform.position, Scene.SceneTracker.storedRagDollPrefabs[i].transform.position);
								if (num2 < num)
								{
									num = num2;
									gameObject = Scene.SceneTracker.storedRagDollPrefabs[i];
								}
							}
						}
						if (gameObject)
						{
							storeLocalMutantInfo2 component3 = gameObject.transform.GetComponent<storeLocalMutantInfo2>();
							this.jointRotations.Clear();
							for (int j = 0; j < component3.jointAngles.Count; j++)
							{
								this.ragDollJoints[j].localRotation = component3.jointAngles[j];
								this.jointRotations.Add(component3.jointAngles[j]);
							}
							if (component)
							{
								component.setSkinColor(component3.matColor);
							}
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
								int num3 = 0;
								foreach (KeyValuePair<Transform, int> keyValuePair in component3.stuckArrowsIndex)
								{
									if (keyValuePair.Key)
									{
										keyValuePair.Key.parent = this.ast.stickToJoints[keyValuePair.Value];
										keyValuePair.Key.localPosition = component3.stuckArrowPos[num3];
										keyValuePair.Key.localRotation = component3.stuckArrowRot[num3];
										fakeArrowSetup component4 = keyValuePair.Key.GetComponent<fakeArrowSetup>();
										if (component4 && BoltNetwork.isRunning)
										{
											component4.storedIndex = this.ast.stuckArrows.Count;
											component4.entityTarget = base.transform.root.GetComponent<BoltEntity>();
										}
										int count = this.ast.stuckArrows.Count;
										this.ast.stuckArrows.Add(keyValuePair.Key, count);
										num3++;
									}
								}
							}
							this.doSync = true;
						}
					}
				}
				if (this.Creepy)
				{
					base.StartCoroutine(this.syncRagDollPositions());
				}
				if (!this.Creepy)
				{
					CoopMutantPropSync component5 = base.GetComponent<CoopMutantPropSync>();
					if (component5)
					{
						component5.ApplyPropMask(coopMutantDummyToken.Props);
					}
					if (this.RegularParts && this.SkinnyParts)
					{
						if (coopMutantDummyToken.Skinny)
						{
							this.RegularParts.SetActive(false);
							this.SkinnyParts.SetActive(true);
						}
						else
						{
							this.RegularParts.SetActive(true);
							this.SkinnyParts.SetActive(false);
						}
					}
				}
			}
		}
	}

	
	public bool Creepy;

	
	public GameObject PickupTrigger;

	
	public GameObject RegularParts;

	
	public GameObject SkinnyParts;

	
	public Transform rootTr;

	
	public CoopMecanimReplicator Replicator;

	
	private mutantRagdollFollow[] mrf;

	
	private arrowStickToTarget ast;

	
	public Transform[] ragDollJoints;

	
	public List<Quaternion> jointRotations = new List<Quaternion>();

	
	public bool doSync;

	
	public bool debugRagdoll;
}
