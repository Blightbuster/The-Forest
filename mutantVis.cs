﻿using System;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using TheForest.Utils;
using UnityEngine;


public class mutantVis : MonoBehaviour
{
	
	private void Awake()
	{
	}

	
	private void Start()
	{
		this.thisTr = base.transform;
		this.animator = base.transform.GetComponent<Animator>();
		if (this.mainRendererGo)
		{
			this.thisRenderer = this.mainRendererGo.GetComponent<SkinnedMeshRenderer>();
		}
		if (this.lowSkinnyGo)
		{
			this.thisSkinnyRenderer = this.lowSkinnyGo.GetComponent<SkinnedMeshRenderer>();
		}
		if (this.lowRendererGo)
		{
			this.thisLowRenderer = this.lowRendererGo.GetComponent<SkinnedMeshRenderer>();
		}
		if (this.lowSkinnyGo)
		{
			this.thisLowSkinnyRenderer = this.lowSkinnyGo.GetComponent<SkinnedMeshRenderer>();
		}
		this.rootGo = base.transform.root.gameObject;
		if (!this.netPrefab)
		{
			this.setup = base.transform.GetComponent<mutantScriptSetup>();
			this.fsmDist = this.setup.pmCombat.FsmVariables.GetFsmFloat("playerDist");
			if (this.setup.pmSleep)
			{
				this.setup.pmSleep.FsmVariables.GetFsmGameObject("meshGo").Value = base.gameObject;
			}
			if (this.setup.pmEncounter)
			{
				this.fsmDoingEncounter = this.setup.pmEncounter.FsmVariables.GetFsmBool("doingEncounter");
			}
			if (this.setup.pmBrain)
			{
				this.fsmSleepBool = this.setup.pmBrain.FsmVariables.GetFsmBool("sleepBool");
			}
			else if (this.setup.pmCombat)
			{
				this.fsmSleepBool = this.setup.pmCombat.FsmVariables.GetFsmBool("sleepBool");
			}
			if (this.setup.pmCombat)
			{
				this.fsmAnimatorActive = this.setup.pmCombat.FsmVariables.GetFsmBool("animatorActive");
			}
		}
		if (this.netPrefab)
		{
			base.InvokeRepeating("updatePlayerTargets", 0.5f, 0.2f);
		}
		this.initBool = false;
		base.Invoke("initMe", 3f);
	}

	
	private void OnEnable()
	{
		base.Invoke("initMe", 1f);
		base.Invoke("initDisable", 1f);
	}

	
	private void initDisable()
	{
		this.disableAnimation();
		this.animEnabled = false;
		this.animReduced = false;
		this.animDisabled = true;
		this.trigger = true;
	}

	
	private void OnDisable()
	{
		this.disableVis = false;
		this.initBool = false;
		this.animDisabled = false;
		this.animReduced = false;
		this.animEnabled = false;
		this.encounterCheck = false;
		this.refreshAmplify = false;
	}

	
	private void initMe()
	{
		this.initBool = true;
	}

	
	private void updatePlayerTargets()
	{
		if (!Scene.SceneTracker)
		{
			return;
		}
		this.allPlayers = new List<GameObject>(Scene.SceneTracker.allPlayers);
		this.allPlayers.RemoveAll((GameObject o) => o == null);
		if (this.allPlayers.Count > 1)
		{
			this.allPlayers.Sort((GameObject c1, GameObject c2) => (this.thisTr.position - c1.transform.position).sqrMagnitude.CompareTo((this.thisTr.position - c2.transform.position).sqrMagnitude));
		}
		if (this.allPlayers.Count == 0)
		{
			return;
		}
		if (this.allPlayers[0] && this.allPlayers[0] != null)
		{
			this.playerTr = this.allPlayers[0].transform;
		}
	}

	
	private void Update()
	{
		if (!this.initBool)
		{
			return;
		}
		if (this.disableVis)
		{
			return;
		}
		if (this.netPrefab)
		{
			this.encounterCheck = false;
		}
		if (!this.netPrefab)
		{
			this.playerDist = this.setup.ai.mainPlayerDist;
		}
		if (CoopPeerStarter.DedicatedHost)
		{
			return;
		}
		if (this.rootGo.activeSelf)
		{
			if (!this.netPrefab)
			{
				if (this.setup.ai.creepy || this.setup.ai.creepy_baby || this.setup.ai.creepy_male || this.setup.ai.creepy_fat)
				{
					this.encounterCheck = false;
				}
				else
				{
					this.encounterCheck = this.fsmDoingEncounter.Value;
					if (this.playerDist > 30f)
					{
						if (this.setup.ai.maleSkinny)
						{
							if (!this.lowSkinnyGo.activeSelf)
							{
								this.lowSkinnyGo.SetActive(true);
							}
							if (this.lowRendererGo.activeSelf)
							{
								this.lowRendererGo.SetActive(false);
							}
							if (this.thisRenderer.enabled)
							{
								this.thisRenderer.enabled = false;
							}
						}
						else
						{
							if (!this.lowRendererGo.active)
							{
								this.lowRendererGo.SetActive(true);
							}
							if (this.lowSkinnyGo.activeSelf)
							{
								this.lowSkinnyGo.SetActive(false);
							}
							if (this.thisRenderer.enabled)
							{
								this.thisRenderer.enabled = false;
							}
						}
					}
					else
					{
						if (this.lowSkinnyGo.activeSelf)
						{
							this.lowSkinnyGo.SetActive(false);
						}
						if (this.lowRendererGo.activeSelf)
						{
							this.lowRendererGo.SetActive(false);
						}
						if (!this.thisRenderer.enabled)
						{
							this.thisRenderer.enabled = true;
						}
					}
				}
			}
			if (this.renderCam == null && Camera.main)
			{
				this.renderCam = Camera.main;
			}
			if (this.renderCam != null)
			{
				if (this.mainRendererGo && this.mainRendererGo.activeSelf)
				{
					if (this.thisRenderer.IsVisibleFrom(this.renderCam))
					{
						this.isVisible = true;
					}
					else
					{
						this.isVisible = false;
					}
				}
				if (this.lowSkinnyGo && this.lowSkinnyGo.activeSelf)
				{
					if (this.thisSkinnyRenderer.IsVisibleFrom(this.renderCam))
					{
						this.isVisible = true;
					}
					else
					{
						this.isVisible = false;
					}
				}
				if (this.lowRendererGo && this.lowRendererGo.activeSelf)
				{
					if (this.thisLowRenderer.IsVisibleFrom(this.renderCam))
					{
						this.isVisible = true;
					}
					else
					{
						this.isVisible = false;
					}
				}
			}
			if (this.encounterCheck)
			{
				if (this.trigger)
				{
					base.StartCoroutine(this.enableAnimation());
					this.trigger = false;
				}
			}
			else if (this.playerDist > 220f)
			{
				if (!this.trigger)
				{
					base.StopCoroutine("enableAnimation");
					if (BoltNetwork.isRunning)
					{
						this.reduceAnimation();
					}
					else
					{
						this.disableAnimation();
					}
					this.trigger = true;
				}
			}
			else if (this.isVisible)
			{
				if (this.trigger)
				{
					base.StartCoroutine("enableAnimation");
					this.trigger = false;
				}
			}
			else if ((this.isVisible && this.playerDist < 180f) || this.playerDist < 30f)
			{
				if (this.trigger)
				{
					base.StartCoroutine("enableAnimation");
					this.trigger = false;
				}
			}
			else if (this.playerDist > 180f)
			{
				if (!this.trigger)
				{
					base.StopCoroutine("enableAnimation");
					if (BoltNetwork.isRunning)
					{
						this.reduceAnimation();
					}
					else
					{
						this.disableAnimation();
					}
					this.trigger = true;
				}
			}
			else if (!this.trigger)
			{
				base.StopCoroutine("enableAnimation");
				this.reduceAnimation();
				this.trigger = true;
			}
		}
		if (this.mecanimEmitter)
		{
			if (this.playerDist > this.mecanimEventsDisableDistance)
			{
				if (this.mecanimEmitter.enabled)
				{
					this.mecanimEmitter.enabled = false;
				}
			}
			else if (!this.mecanimEmitter.enabled)
			{
				this.mecanimEmitter.enabled = true;
			}
		}
		if (this.displacementGo)
		{
			if (this.playerDist > this.displacementDisableDistance)
			{
				if (this.displacementGo.activeSelf)
				{
					this.displacementGo.SetActive(false);
				}
			}
			else if (!this.displacementGo.activeSelf)
			{
				this.displacementGo.SetActive(true);
			}
		}
	}

	
	private void disableAnimation()
	{
		if (BoltNetwork.isServer)
		{
			return;
		}
		if (!this.animDisabled)
		{
			if (!this.netPrefab && this.setup)
			{
				this.setup.typeSetup.updateWorldTransformPosition();
				if (this.setup.ai.male || this.setup.ai.female)
				{
					this.setup.typeSetup.storeDefaultParams();
				}
				this.fsmAnimatorActive.Value = false;
			}
			this.isActive = false;
			if (this.animator)
			{
				this.animator.enabled = false;
			}
			if (this.props && this.props.activeSelf)
			{
				this.props.SetActive(false);
			}
			for (int i = 0; i < this.hideGo.Length; i++)
			{
				if (this.hideGo[i].activeSelf)
				{
					this.hideGo[i].SetActive(false);
				}
			}
			for (int j = 0; j < this.hideRenderers.Length; j++)
			{
				if (this.hideRenderers[j].enabled)
				{
					this.hideRenderers[j].enabled = false;
				}
			}
			for (int k = 0; k < this.joints.Length; k++)
			{
				if (this.joints[k].activeSelf)
				{
					this.joints[k].SetActive(false);
				}
			}
			if (this.thisRenderer && this.thisRenderer.enabled)
			{
				this.thisRenderer.enabled = false;
			}
			if (this.collisionGo && this.collisionGo.activeSelf)
			{
				this.collisionGo.SetActive(false);
			}
			this.animEnabled = false;
			this.animReduced = false;
			this.animDisabled = true;
		}
	}

	
	private void reduceAnimation()
	{
		if (!this.animReduced)
		{
			if (!this.netPrefab)
			{
				this.setup.typeSetup.updateWorldTransformPosition();
				if (this.setup.ai.male || this.setup.ai.female)
				{
					this.setup.typeSetup.storeDefaultParams();
				}
			}
			this.isActive = false;
			if (this.thisRenderer && this.thisRenderer.enabled)
			{
				this.thisRenderer.enabled = false;
			}
			if (this.props && this.props.activeSelf)
			{
				this.props.SetActive(false);
			}
			for (int i = 0; i < this.hideGo.Length; i++)
			{
				if (this.hideGo[i].activeSelf)
				{
					this.hideGo[i].SetActive(false);
				}
			}
			for (int j = 0; j < this.hideRenderers.Length; j++)
			{
				if (this.hideRenderers[j].enabled)
				{
					this.hideRenderers[j].enabled = false;
				}
			}
			for (int k = 0; k < this.joints.Length; k++)
			{
				if (this.joints[k].activeSelf)
				{
					this.joints[k].SetActive(false);
				}
			}
			this.animDisabled = false;
			this.animEnabled = false;
			this.animReduced = true;
		}
	}

	
	private IEnumerator enableAnimation()
	{
		if (!this.animEnabled)
		{
			this.isActive = true;
			this.animator.enabled = true;
			yield return null;
			if (this.thisRenderer && !this.thisRenderer.enabled)
			{
				this.thisRenderer.enabled = true;
			}
			if (this.props && !this.props.activeSelf)
			{
				this.props.SetActive(true);
			}
			for (int i = 0; i < this.hideGo.Length; i++)
			{
				if (!this.hideGo[i].activeSelf)
				{
					this.hideGo[i].SetActive(true);
				}
			}
			for (int j = 0; j < this.hideRenderers.Length; j++)
			{
				if (!this.hideRenderers[j].enabled)
				{
					this.hideRenderers[j].enabled = true;
				}
			}
			for (int k = 0; k < this.joints.Length; k++)
			{
				if (!this.joints[k].activeSelf)
				{
					this.joints[k].SetActive(true);
				}
			}
			if (!this.netPrefab)
			{
				this.fsmAnimatorActive.Value = true;
				if (this.setup.ai.male || this.setup.ai.female)
				{
					this.setup.typeSetup.setDefaultParams();
				}
			}
			if (this.collisionGo && !this.collisionGo.activeSelf)
			{
				this.collisionGo.SetActive(true);
			}
			this.animReduced = false;
			this.animEnabled = true;
			if (this.animDisabled)
			{
				this.animator.SetTriggerReflected("resetTrigger");
				this.animDisabled = false;
			}
		}
		yield break;
	}

	
	private IEnumerator fixMotionBlur(SkinnedMeshRenderer sk)
	{
		yield return YieldPresets.WaitForEndOfFrame;
		yield break;
	}

	
	private PlayMakerFSM pmControl;

	
	public Animator animator;

	
	public Renderer worldPosRenderer;

	
	private SkinnedMeshRenderer thisRenderer;

	
	private SkinnedMeshRenderer thisLowRenderer;

	
	private SkinnedMeshRenderer thisSkinnyRenderer;

	
	private SkinnedMeshRenderer thisLowSkinnyRenderer;

	
	public GameObject mainRendererGo;

	
	public GameObject lowRendererGo;

	
	public GameObject lowSkinnyGo;

	
	public Camera renderCam;

	
	private mutantScriptSetup setup;

	
	public bool trigger;

	
	private bool amplifyTrigger;

	
	public bool doAmplify;

	
	public bool animEnabled;

	
	public bool animDisabled;

	
	public bool animReduced;

	
	public GameObject[] joints;

	
	public GameObject[] hideGo;

	
	public Renderer[] hideRenderers;

	
	public GameObject displacementGo;

	
	public MecanimEventEmitter mecanimEmitter;

	
	public GameObject props;

	
	public GameObject collisionGo;

	
	public GameObject pushGo;

	
	private GameObject rootGo;

	
	public float playerDist;

	
	private Transform playerTr;

	
	private Transform thisTr;

	
	private Vector3 playerLocalTarget;

	
	private bool encounterCheck;

	
	private bool refreshAmplify;

	
	public bool disableVis;

	
	private FsmBool fsmSleepBool;

	
	private FsmBool fsmDoingEncounter;

	
	private FsmBool fsmAnimatorActive;

	
	public FsmFloat fsmDist;

	
	public float mecanimEventsDisableDistance = 75f;

	
	public float displacementDisableDistance = 35f;

	
	public bool isVisible;

	
	public bool isActive;

	
	public bool netPrefab;

	
	private bool initBool;

	
	public List<GameObject> allPlayers = new List<GameObject>();
}
