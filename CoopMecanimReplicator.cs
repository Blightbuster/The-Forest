using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Utils;
using UnityEngine;


public class CoopMecanimReplicator : EntityBehaviour<IWorldCharacter>
{
	
	private void UpdateOverrideLookupTable()
	{
		this.overrideLookup = new Dictionary<int, MecanimTransitionOverride>();
		foreach (MecanimTransitionOverride mecanimTransitionOverride in this.TransitionOverrides)
		{
			this.overrideLookup.Add(Animator.StringToHash(mecanimTransitionOverride.FullStateName), mecanimTransitionOverride);
		}
	}

	
	private void Awake()
	{
		this.setup = base.transform.GetComponentInChildren<mutantScriptSetup>();
		if (BoltNetwork.isRunning)
		{
			this.UpdateOverrideLookupTable();
		}
		else
		{
			base.enabled = false;
		}
	}

	
	private void Start()
	{
		if (this.isAnimal)
		{
		}
	}

	
	private void OnSpawned()
	{
		if (this.isAnimal && BoltNetwork.isServer)
		{
			this.forceMecanimSync();
		}
	}

	
	public override void Attached()
	{
		if (this.TargetAnimator == null && this.isPlayer && !this.isNetPlayer)
		{
			this.TargetAnimator = LocalPlayer.PlayerBase.GetComponent<Animator>();
		}
		for (int i = 0; i < this.LayersToSync.Length; i++)
		{
			this.LayersToSync[i].LayerIndex = this.TargetAnimator.GetLayerIndex(this.LayersToSync[i].Name);
			if (this.LayersToSync[i].LayerIndex == -1)
			{
			}
		}
		base.state.SetAnimator(this.TargetAnimator);
		if (!base.entity.IsOwner())
		{
			base.state.AddCallback("MecanimLayerHashes[]", new PropertyCallbackSimple(this.UpdateMecanimPropertiesFromState));
			base.state.AddCallback("MecanimLayerTimes[]", new PropertyCallbackSimple(this.UpdateMecanimPropertiesFromState));
		}
		if (this.TransitionData && !this.isDummy)
		{
			this.TransitionData.Init();
		}
		this.vis = base.transform.GetComponentInChildren<mutantVis>();
		if (!this.isDummy && this.useEventBasedSync && !base.entity.isOwner)
		{
			this.forceStateUpdate();
		}
	}

	
	public override void Detached()
	{
		base.StopAllCoroutines();
	}

	
	public void forceMecanimSync()
	{
		for (int i = 0; i < this.LayersToSync.Length; i++)
		{
			MecanimLayerConfig mecanimLayerConfig = this.LayersToSync[i];
			if (mecanimLayerConfig.Hash != 0)
			{
				this.ApplyHashToRemote(mecanimLayerConfig.LayerIndex, mecanimLayerConfig.Hash, false, 0.2f, false);
			}
		}
		for (int j = 0; j < this.LayersToSync.Length; j++)
		{
			if (this.LayersToSync[j].Hash_Recv != base.state.MecanimLayerHashes[j])
			{
				MecanimLayerConfig mecanimLayerConfig2 = this.LayersToSync[j];
				this.ApplyHashToRemote(mecanimLayerConfig2.LayerIndex, base.state.MecanimLayerHashes[j], false, 0.2f, false);
				Debug.Log("forced a SYNC on " + base.gameObject.name);
			}
		}
	}

	
	public void ForceStatelayerSync()
	{
		for (int i = 0; i < this.LayersToSync.Length; i++)
		{
			if (this.LayersToSync[i].Hash_Recv != base.state.MecanimLayerHashes[i])
			{
				MecanimLayerConfig mecanimLayerConfig = this.LayersToSync[i];
				this.ApplyHashToRemote(mecanimLayerConfig.LayerIndex, base.state.MecanimLayerHashes[i], false, base.state.MecanimLayerTimes[i], false);
			}
		}
	}

	
	public void ApplyHashToRemote(int layer, int hash, bool anyState, float normalizedTime, bool instantTransition)
	{
		MecanimLayerConfig mecanimLayerConfig = this.LayersToSync[layer];
		this.LayersToSync[layer].Hash = hash;
		mecanimLayerConfig.Hash_Recv = hash;
		this.UpdateRemoteEvent(null, new float?(normalizedTime), hash, layer, anyState, instantTransition);
	}

	
	private void UpdateMecanimPropertiesFromState()
	{
		for (int i = 0; i < this.LayersToSync.Length; i++)
		{
			if (this.LayersToSync[i].Hash_Recv != base.state.MecanimLayerHashes[i])
			{
				this.LayersToSync[i].Hash_Recv = (this.LayersToSync[i].Hash = base.state.MecanimLayerHashes[i]);
			}
			this.LayersToSync[i].NormalizedTime = base.state.MecanimLayerTimes[i];
		}
	}

	
	private void LateUpdate()
	{
		if (this.TargetAnimator == null && this.isPlayer && !this.isNetPlayer)
		{
			this.TargetAnimator = LocalPlayer.PlayerBase.GetComponent<Animator>();
		}
		if (!this.forceLayerSync)
		{
			this.forceLayerSync = true;
		}
		if (!this.TargetAnimator.enabled)
		{
			return;
		}
		if (base.entity.IsAttached())
		{
			if (base.entity.isOwner)
			{
				if (this.useEventBasedSync && !this.isDummy)
				{
					if (this.eventDistanceThreshold > 0f && this.vis.playerDist > 30f)
					{
						this.UpdateOwner();
					}
					else
					{
						this.UpdateOwnerWithEvents();
					}
				}
				else
				{
					this.UpdateOwner();
				}
			}
			else if (this.useEventBasedSync && !this.isDummy)
			{
				if (this.eventDistanceThreshold > 0f && this.vis.playerDist > 30f)
				{
					this.UpdateRemote(null, null);
				}
				else
				{
					this.UpdateRemoteLayers(null, null);
				}
			}
			else
			{
				this.UpdateRemote(null, null);
			}
			if (!base.entity.isOwner && !this.isPlayer && this.useEventBasedSync && Time.time > this.farUpdateTimer && LocalPlayer.Transform)
			{
				float sqrMagnitude = (LocalPlayer.Transform.position - base.transform.position).sqrMagnitude;
				AnimatorStateInfo currentAnimatorStateInfo = this.TargetAnimator.GetCurrentAnimatorStateInfo(0);
				if (sqrMagnitude > 900f && sqrMagnitude < 40000f && !this.TargetAnimator.IsInTransition(0) && (currentAnimatorStateInfo.tagHash == this.idleHash || currentAnimatorStateInfo.tagHash == this.walkHash || currentAnimatorStateInfo.tagHash == this.runHash))
				{
					this.forceMecanimSync();
					this.farUpdateTimer = Time.time + 2f;
				}
			}
			if (!base.entity.isOwner && this.isNetPlayer && Time.time > this.playerUpdateTimer)
			{
				if (this.TargetAnimator.GetLayerWeight(2) > 0.5f)
				{
					this.ForceStatelayerSync();
				}
				this.playerUpdateTimer = Time.time + 1f;
			}
		}
	}

	
	private void UpdateOwner()
	{
		if (base.entity.IsOwner())
		{
			base.state.MecanimSpeed = this.TargetAnimator.speed;
			for (int i = 0; i < this.LayersToSync.Length; i++)
			{
				MecanimLayerConfig mecanimLayerConfig = this.LayersToSync[i];
				AnimatorStateInfo animatorStateInfo = this.TargetAnimator.GetCurrentAnimatorStateInfo(mecanimLayerConfig.LayerIndex);
				if (this.TargetAnimator.IsInTransition(mecanimLayerConfig.LayerIndex))
				{
					animatorStateInfo = this.TargetAnimator.GetNextAnimatorStateInfo(mecanimLayerConfig.LayerIndex);
				}
				if (animatorStateInfo.fullPathHash != mecanimLayerConfig.Hash)
				{
					base.state.MecanimLayerHashes[i] = (mecanimLayerConfig.Hash = animatorStateInfo.fullPathHash);
					base.state.MecanimLayerTimes[i] = (mecanimLayerConfig.NormalizedTime = animatorStateInfo.normalizedTime);
				}
				if (mecanimLayerConfig.SyncWeight)
				{
					float layerWeight = this.TargetAnimator.GetLayerWeight(mecanimLayerConfig.LayerIndex);
					if (layerWeight != mecanimLayerConfig.Weight)
					{
						base.state.MecanimLayerWeights[i] = (mecanimLayerConfig.Weight = layerWeight);
					}
				}
			}
		}
	}

	
	private void forceLayerIndexSync()
	{
		for (int i = 0; i < this.LayersToSync.Length; i++)
		{
			this.LayersToSync[i].LayerIndex = this.TargetAnimator.GetLayerIndex(this.LayersToSync[i].Name);
			if (this.LayersToSync[i].LayerIndex == -1)
			{
			}
		}
	}

	
	public void forceStateUpdate()
	{
		if (base.entity.IsOwner())
		{
			for (int i = 0; i < this.LayersToSync.Length; i++)
			{
				MecanimLayerConfig mecanimLayerConfig = this.LayersToSync[i];
				AnimatorStateInfo currentAnimatorStateInfo = this.TargetAnimator.GetCurrentAnimatorStateInfo(mecanimLayerConfig.LayerIndex);
				int fullPathHash = currentAnimatorStateInfo.fullPathHash;
				base.StartCoroutine(this.sendTransitionEventDelayed(currentAnimatorStateInfo.fullPathHash, mecanimLayerConfig.LayerIndex, currentAnimatorStateInfo.normalizedTime, base.entity, false, 0));
			}
		}
	}

	
	private void UpdateOwnerWithEvents()
	{
		if (base.entity.IsOwner())
		{
			base.state.MecanimSpeed = this.TargetAnimator.speed;
			for (int i = 0; i < this.LayersToSync.Length; i++)
			{
				MecanimLayerConfig mecanimLayerConfig = this.LayersToSync[i];
				if (mecanimLayerConfig.LayerIndex == -1)
				{
					this.forceLayerIndexSync();
				}
				AnimatorStateInfo animatorStateInfo = this.TargetAnimator.GetCurrentAnimatorStateInfo(mecanimLayerConfig.LayerIndex);
				int fullPathHash = animatorStateInfo.fullPathHash;
				if (this.TargetAnimator.IsInTransition(mecanimLayerConfig.LayerIndex))
				{
					animatorStateInfo = this.TargetAnimator.GetNextAnimatorStateInfo(mecanimLayerConfig.LayerIndex);
					AnimatorTransitionInfo animatorTransitionInfo = this.TargetAnimator.GetAnimatorTransitionInfo(mecanimLayerConfig.LayerIndex);
					float normalizedTime = animatorTransitionInfo.normalizedTime;
					if (animatorStateInfo.fullPathHash != mecanimLayerConfig.Hash && this.TransitionData.Lookup != null)
					{
						int key = fullPathHash - animatorStateInfo.fullPathHash;
						if (animatorTransitionInfo.anyState)
						{
							key = 1111 - animatorStateInfo.fullPathHash;
						}
						Dictionary<bool, float> dictionary;
						if (this.TransitionData.Lookup.TryGetValue(key, out dictionary))
						{
							if ((animatorStateInfo.tagHash == this.runHash || animatorStateInfo.tagHash == this.walkHash) && !this.isPlayer)
							{
								base.StartCoroutine(this.sendTransitionEventDelayed(animatorStateInfo.fullPathHash, mecanimLayerConfig.LayerIndex, animatorStateInfo.normalizedTime, base.entity, animatorTransitionInfo.anyState, 7));
							}
							else
							{
								base.StartCoroutine(this.sendTransitionEventDelayed(animatorStateInfo.fullPathHash, mecanimLayerConfig.LayerIndex, animatorStateInfo.normalizedTime, base.entity, animatorTransitionInfo.anyState, 0));
							}
						}
						base.state.MecanimLayerHashes[i] = (mecanimLayerConfig.Hash = animatorStateInfo.fullPathHash);
						base.state.MecanimLayerTimes[i] = (mecanimLayerConfig.NormalizedTime = animatorStateInfo.normalizedTime);
					}
				}
				if (this.setup && this.setup.animControl && animatorStateInfo.tagHash == this.setup.animControl.deathHash)
				{
					base.state.MecanimLayerHashes[i] = (mecanimLayerConfig.Hash = animatorStateInfo.fullPathHash);
					base.state.MecanimLayerTimes[i] = (mecanimLayerConfig.NormalizedTime = animatorStateInfo.normalizedTime);
				}
				if (mecanimLayerConfig.SyncWeight)
				{
					float layerWeight = this.TargetAnimator.GetLayerWeight(mecanimLayerConfig.LayerIndex);
					if (layerWeight != mecanimLayerConfig.Weight)
					{
						base.state.MecanimLayerWeights[i] = (mecanimLayerConfig.Weight = layerWeight);
					}
				}
			}
		}
	}

	
	private IEnumerator sendTransitionEventDelayed(int getHash, int layer, float nTime, BoltEntity entity, bool anyState, int delay)
	{
		for (int count = 0; count < delay; count++)
		{
			yield return YieldPresets.WaitForFixedUpdate;
		}
		updateMecanimRemoteState ev = updateMecanimRemoteState.Create(GlobalTargets.Others);
		ev.hash = getHash;
		ev.layer = layer;
		ev.normalizedTime = nTime;
		ev.Target = entity;
		ev.anyState = anyState;
		ev.Send();
		yield break;
	}

	
	private void UpdateRemoteEvent(float? transitionDuration, float? stateNormalizedTime, int layerHash, int layer, bool anyState, bool instantTransition)
	{
		if (this.isAnimal)
		{
			base.entity = base.transform.parent.GetComponent<BoltEntity>();
		}
		if (base.entity.IsAttached())
		{
			this.TargetAnimator.speed = base.state.MecanimSpeed;
			AnimatorStateInfo currentAnimatorStateInfo = this.TargetAnimator.GetCurrentAnimatorStateInfo(layer);
			bool flag = false;
			if (this.TransitionData && transitionDuration == null && this.TransitionData.Lookup != null)
			{
				int key = currentAnimatorStateInfo.fullPathHash - layerHash;
				if (anyState)
				{
					key = 1111 - layerHash;
				}
				Dictionary<bool, float> dictionary;
				if (this.TransitionData.Lookup.TryGetValue(key, out dictionary))
				{
					float value;
					if (dictionary.TryGetValue(true, out value))
					{
						flag = true;
					}
					else if (dictionary.TryGetValue(false, out value))
					{
						flag = false;
					}
					transitionDuration = new float?(value);
				}
			}
			if (transitionDuration != null)
			{
				transitionDuration = new float?(transitionDuration.Value);
			}
			else
			{
				transitionDuration = new float?(0.05f);
			}
			if (instantTransition)
			{
				transitionDuration = new float?(0f);
			}
			if (flag)
			{
				this.TargetAnimator.CrossFadeInFixedTime(layerHash, Mathf.Max(0f, transitionDuration.Value), layer, stateNormalizedTime.Value);
			}
			else
			{
				this.TargetAnimator.CrossFade(layerHash, Mathf.Max(0f, transitionDuration.Value), layer, stateNormalizedTime.Value);
			}
		}
	}

	
	private void UpdateRemoteLayers(float? transitionDuration, float? stateNormalizedTime)
	{
		if (base.entity.IsAttached())
		{
			this.TargetAnimator.speed = base.state.MecanimSpeed;
			for (int i = 0; i < this.LayersToSync.Length; i++)
			{
				MecanimLayerConfig mecanimLayerConfig = this.LayersToSync[i];
				if (mecanimLayerConfig.SyncWeight && mecanimLayerConfig.Weight != base.state.MecanimLayerWeights[i])
				{
					float num = mecanimLayerConfig.Weight;
					num = Mathf.Lerp(num, base.state.MecanimLayerWeights[i], Time.deltaTime * 10f);
					this.TargetAnimator.SetLayerWeight(mecanimLayerConfig.LayerIndex, mecanimLayerConfig.Weight = num);
				}
			}
		}
	}

	
	private void UpdateRemote(float? transitionDuration, float? stateNormalizedTime)
	{
		if (base.entity.IsAttached())
		{
			this.TargetAnimator.speed = base.state.MecanimSpeed;
			for (int i = 0; i < this.LayersToSync.Length; i++)
			{
				MecanimLayerConfig mecanimLayerConfig = this.LayersToSync[i];
				if (mecanimLayerConfig.Hash != 0)
				{
					AnimatorStateInfo currentAnimatorStateInfo = this.TargetAnimator.GetCurrentAnimatorStateInfo(mecanimLayerConfig.LayerIndex);
					if (!this.TargetAnimator.IsInTransition(mecanimLayerConfig.LayerIndex))
					{
						if (currentAnimatorStateInfo.fullPathHash != mecanimLayerConfig.Hash)
						{
							MecanimTransitionOverride mecanimTransitionOverride;
							Dictionary<bool, float> dictionary;
							float value;
							if (this.overrideLookup.TryGetValue(mecanimLayerConfig.Hash, out mecanimTransitionOverride))
							{
								if (transitionDuration == null)
								{
									transitionDuration = new float?(mecanimLayerConfig.TransitionTime);
								}
								if (stateNormalizedTime == 0f)
								{
									stateNormalizedTime = new float?(mecanimTransitionOverride.TransitionOffset);
								}
							}
							else if (this.TransitionData && transitionDuration == null && this.TransitionData.Lookup != null && this.TransitionData.Lookup.TryGetValue(currentAnimatorStateInfo.fullPathHash, out dictionary) && dictionary.TryGetValue(true, out value))
							{
								transitionDuration = new float?(value);
							}
						}
						if (transitionDuration != null)
						{
							transitionDuration = new float?(transitionDuration.Value * this.CrossFadeMultiplier);
						}
						else
						{
							transitionDuration = new float?(mecanimLayerConfig.TransitionTime);
						}
						if (stateNormalizedTime == null)
						{
							stateNormalizedTime = new float?(mecanimLayerConfig.NormalizedTime);
						}
						this.TargetAnimator.CrossFade(mecanimLayerConfig.Hash, Mathf.Max(0f, transitionDuration.Value), mecanimLayerConfig.LayerIndex, stateNormalizedTime.Value);
						mecanimLayerConfig.Hash = 0;
					}
				}
				if (mecanimLayerConfig.SyncWeight && mecanimLayerConfig.Weight != base.state.MecanimLayerWeights[i])
				{
					float num = mecanimLayerConfig.Weight;
					num = Mathf.Lerp(num, base.state.MecanimLayerWeights[i], Time.deltaTime * 10f);
					this.TargetAnimator.SetLayerWeight(mecanimLayerConfig.LayerIndex, mecanimLayerConfig.Weight = num);
				}
			}
		}
	}

	
	private Dictionary<int, MecanimTransitionOverride> overrideLookup;

	
	[SerializeField]
	public Animator TargetAnimator;

	
	[SerializeField]
	public CoopMecanimReplicatorTransitionData TransitionData;

	
	[SerializeField]
	public MecanimLayerConfig[] LayersToSync;

	
	[SerializeField]
	public MecanimTransitionOverride[] TransitionOverrides;

	
	[Range(0f, 1f)]
	[SerializeField]
	public float CrossFadeMultiplier = 0.9f;

	
	public bool isDummy;

	
	public bool isPlayer;

	
	public bool isNetPlayer;

	
	public bool isAnimal;

	
	public bool isBird;

	
	public bool useEventBasedSync;

	
	public float eventDistanceThreshold;

	
	private int intoBlock = Animator.StringToHash("intoBlock");

	
	private float farUpdateTimer;

	
	private float playerUpdateTimer;

	
	private int runHash = Animator.StringToHash("running");

	
	private int walkHash = Animator.StringToHash("walking");

	
	private int idleHash = Animator.StringToHash("idle");

	
	private mutantVis vis;

	
	private mutantScriptSetup setup;

	
	private bool forceLayerSync;
}
