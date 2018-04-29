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
		for (int i = 0; i < Scene.SceneTracker.storedRagDollPrefabs.Count; i++)
		{
			if (Scene.SceneTracker.storedRagDollPrefabs[i] != null)
			{
				float dist = Vector3.Distance(base.transform.position, Scene.SceneTracker.storedRagDollPrefabs[i].transform.position);
				if (dist < closestDist)
				{
					closestDist = dist;
					closestStoredToken = Scene.SceneTracker.storedRagDollPrefabs[i];
				}
			}
		}
		if (closestStoredToken)
		{
			storeLocalMutantInfo2 component = closestStoredToken.transform.GetComponent<storeLocalMutantInfo2>();
			if (!this.ast)
			{
				this.ast = base.transform.GetComponent<arrowStickToTarget>();
			}
			if (!this.ast)
			{
				arrowStickToTarget[] componentsInChildren = base.transform.GetComponentsInChildren<arrowStickToTarget>(true);
				if (componentsInChildren != null && componentsInChildren.Length > 0 && componentsInChildren[0])
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
						keyValuePair.Key.parent = this.ast.stickToJoints[keyValuePair.Value];
						keyValuePair.Key.localPosition = component.stuckArrowPos[num];
						keyValuePair.Key.localRotation = component.stuckArrowRot[num];
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
			if (component.fireIndex.Count > 0)
			{
				base.StartCoroutine(this.transferClientFire(component));
			}
			if (this.ragDollJoints.Length == 0)
			{
				yield break;
			}
			CoopMutantMaterialSync component3 = base.GetComponent<CoopMutantMaterialSync>();
			if (component3)
			{
				component3.setSkinColor(component.matColor);
			}
			if (this.rootTr)
			{
				this.rootTr.rotation = component.rootRotation;
				this.rootTr.position = component.rootPosition;
				if (component.jointAngles.Count > 0)
				{
					for (int j = 0; j < this.ragDollJoints.Length; j++)
					{
						this.ragDollJoints[j].localRotation = component.jointAngles[j];
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
		if (!base.entity.isOwner)
		{
			CoopMutantDummyToken coopMutantDummyToken = base.entity.attachToken as CoopMutantDummyToken;
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
							if (component3.fireIndex.Count > 0)
							{
								base.StartCoroutine(this.transferClientFire(component3));
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

	
	private IEnumerator transferClientFire(storeLocalMutantInfo2 store)
	{
		mutantTransferFire mtf = base.transform.GetComponent<mutantTransferFire>();
		if (mtf && mtf.allBones.Count > 0)
		{
			if (store == null || store.fireIndex.Count == 0)
			{
				yield break;
			}
			float targetDist = float.PositiveInfinity;
			Transform key = null;
			foreach (KeyValuePair<Transform, int> keyValuePair in store.fireIndex)
			{
				if (key == null)
				{
					key = keyValuePair.Key;
				}
			}
			float timer = Time.time + 2f;
			while (targetDist > 10f)
			{
				if (Time.time > timer)
				{
					yield break;
				}
				targetDist = Vector3.Distance(key.position, base.transform.position);
				yield return null;
			}
			int indexCount = 0;
			foreach (KeyValuePair<Transform, int> keyValuePair2 in store.fireIndex)
			{
				if (keyValuePair2.Key && keyValuePair2.Key.gameObject.activeSelf)
				{
					keyValuePair2.Key.parent = mtf.allBones[keyValuePair2.Value];
					keyValuePair2.Key.localPosition = store.firePos[indexCount];
					keyValuePair2.Key.localRotation = store.fireRot[indexCount];
					indexCount++;
				}
			}
		}
		yield break;
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
