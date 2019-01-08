using System;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using Pathfinding;
using TheForest.Utils;
using UnityEngine;

public class mutantAI : MonoBehaviour
{
	public void SyncAIConfiguration()
	{
		if (BoltNetwork.isRunning && this.ai_net)
		{
			this.ai_net.TriggerSync(this);
		}
	}

	private void Start()
	{
		this.rg = AstarPath.active.astarData.recastGraph;
		this.layerMask = 103948289;
		this.wallLayerMask = 103948289;
		this.avatar = base.transform.GetComponentInChildren<Animator>();
		this.setup = base.GetComponent<mutantScriptSetup>();
		this.controller = base.transform.root.GetComponent<CharacterController>();
		this.seeker = base.GetComponent<Seeker>();
		this.thisTr = base.transform;
		this.rootTr = base.transform.root;
		this.deadZone *= 0.0174532924f;
		this.storeNextWaypointDistance = this.nextWaypointDistance;
		if (this.setup.pmMotor)
		{
			this.fsmRotateSpeed = this.setup.pmMotor.FsmVariables.GetFsmFloat("rotateSpeed");
		}
		this.fsmSmoothPathDir = this.setup.pmCombat.FsmVariables.GetFsmFloat("smoothedPathDir");
		this.fsmTargetDir = this.setup.pmCombat.FsmVariables.GetFsmFloat("targetDir");
		this.fsmTargetDist = this.setup.pmCombat.FsmVariables.GetFsmFloat("targetDist");
		if (this.creepy_male || this.creepy || this.creepy_fat || this.creepy_baby)
		{
			this.setup.pmCombat.FsmVariables.GetFsmFloat("closestPlayerDist");
			this.setup.pmCombat.FsmVariables.GetFsmBool("animatorEnabled");
		}
		this.fsmClosestPlayerHeight = this.setup.pmCombat.FsmVariables.GetFsmFloat("closestPlayerHeight");
		this.fsmOnWallBool = this.setup.pmCombat.FsmVariables.GetFsmBool("toClimbWall");
		this.fsmTarget = this.setup.pmCombat.FsmVariables.GetFsmGameObject("target");
		this.fsmPlayerDist = this.setup.pmCombat.FsmVariables.GetFsmFloat("playerDist");
		this.fsmClosestPlayerDist = this.setup.pmCombat.FsmVariables.GetFsmFloat("closestPlayerDist");
		this.fsmPlayerDir = this.setup.pmCombat.FsmVariables.GetFsmVector3("playerDir");
		this.fsmPlayerAngle = this.setup.pmCombat.FsmVariables.GetFsmFloat("playerAngle");
		if (this.setup.pmBrain)
		{
			this.fsmPathVector = this.setup.pmBrain.FsmVariables.GetFsmVector3("pathVector");
			this.fsmLeaderGo = this.setup.pmBrain.FsmVariables.GetFsmGameObject("leaderGo");
		}
		else
		{
			this.fsmLeaderGo = this.setup.pmCombat.FsmVariables.GetFsmGameObject("leaderGo");
		}
		if (this.setup.pmMotor)
		{
			this.fsmWantedDir = this.setup.pmMotor.FsmVariables.GetFsmVector3("wantedDir");
			this.fsmMoving = this.setup.pmMotor.FsmVariables.GetFsmBool("movingBool");
		}
		this.fsmCurrentAttackerGo = this.setup.pmCombat.FsmVariables.GetFsmGameObject("currentAttackerGo");
		if (this.setup.pmBrain)
		{
			this.fsmBrainTarget = this.setup.pmBrain.FsmVariables.GetFsmGameObject("target");
			this.fsmBrainPlayerDist = this.setup.pmBrain.FsmVariables.GetFsmFloat("playerDist");
			this.fsmBrainPlayerDist2D = this.setup.pmBrain.FsmVariables.GetFsmFloat("playerDist2D");
			this.fsmBrainPlayerAngle = this.setup.pmBrain.FsmVariables.GetFsmFloat("playerAngle");
			this.fsmBrainTargetDist = this.setup.pmBrain.FsmVariables.GetFsmFloat("targetDist");
			this.fsmBrainTargetDir = this.setup.pmBrain.FsmVariables.GetFsmFloat("targetDir");
			this.fsmBrainGroundTag = this.setup.pmBrain.FsmVariables.GetFsmInt("groundTag");
			this.fsmPlayerGroundTag = this.setup.pmBrain.FsmVariables.GetFsmInt("playerGroundTag");
			this.fsmBrainGroundWalkable = this.setup.pmBrain.FsmVariables.GetFsmBool("groundWalkable");
			this.fsmBrainPlayerRedBool = this.setup.pmBrain.FsmVariables.GetFsmBool("playerIsRed");
		}
		this.target = LocalPlayer.Transform;
		this.fsmTarget.Value = LocalPlayer.GameObject;
		if (this.setup.pmBrain)
		{
			this.fsmBrainTarget.Value = this.target.gameObject;
		}
		if (this.creepy_boss)
		{
			this.avatar.speed = 1f;
			this.animSpeed = 1f;
			Scene.SceneTracker.EndgameBoss = base.gameObject;
			Scene.mecanimEvents.GetComponent<MecanimEventSetupHelper>().dataSources[19] = this._mecanimEventPrefab;
			Scene.mecanimEvents.SendMessage("refreshDataSources");
			base.transform.parent.GetComponentInChildren<arrowStickToTarget>().enabled = false;
		}
		this.getRotSpeed = this.rotationSpeed;
		this.allPlayers = new List<GameObject>(this.setup.sceneInfo.allPlayers);
		if (this.creepy_boss)
		{
			this.OnSpawned();
		}
	}

	private void OnSpawned()
	{
		float time = UnityEngine.Random.Range(0.1f, 3f);
		if (!base.IsInvoking("checkTags"))
		{
			base.InvokeRepeating("checkTags", time, 1f);
		}
		if (!base.IsInvoking("checkCloseTargets"))
		{
			base.InvokeRepeating("checkCloseTargets", time, 1.5f);
		}
		if (!base.IsInvoking("updatePlayerTargets"))
		{
			base.InvokeRepeating("updatePlayerTargets", time, 1.5f);
		}
		base.Invoke("SyncAIConfiguration", 0.5f);
		this.resetInsideBase();
		if (this.avatar)
		{
			this.avatar.SetInteger("sharpTurnDir", 0);
		}
		this.resetPlayerRedReaction();
		this.doneClimbCoolDown = false;
		this.climbingWallCooDown = false;
		this.prevPos = base.transform.position;
	}

	private void OnDespawned()
	{
		if (!this.creepy && !this.creepy_baby && !this.creepy_male && !this.creepy_fat)
		{
			this.resetPlayerRedReaction();
		}
		base.CancelInvoke("resetClimbWallCoolDown");
		base.CancelInvoke("checkTags");
		base.CancelInvoke("checkCloseTargets");
		base.CancelInvoke("updatePlayerTargets");
		if (this.setup.sceneInfo.closeEnemies.Contains(this.rootTr.gameObject))
		{
			this.setup.sceneInfo.closeEnemies.Remove(this.rootTr.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (this.path != null)
		{
			this.path.Release(this, false);
		}
		this.path = null;
	}

	private void updatePlayerTargets()
	{
		this.allPlayers = new List<GameObject>(this.setup.sceneInfo.allPlayers);
		this.allPlayers.RemoveAll((GameObject o) => o == null);
		if (this.allPlayers.Count > 1)
		{
			this.allPlayers.Sort((GameObject c1, GameObject c2) => (this.thisTr.position - c1.transform.position).sqrMagnitude.CompareTo((this.thisTr.position - c2.transform.position).sqrMagnitude));
		}
	}

	private void checkCloseTargets()
	{
		if (this.rootTr.gameObject.activeSelf)
		{
			if (this.mainPlayerDist < 65f)
			{
				if (!this.setup.sceneInfo.closeEnemies.Contains(this.rootTr.gameObject))
				{
					this.setup.sceneInfo.closeEnemies.Add(this.rootTr.gameObject);
				}
			}
			else if (this.setup.sceneInfo.closeEnemies.Contains(this.rootTr.gameObject))
			{
				this.setup.sceneInfo.closeEnemies.Remove(this.rootTr.gameObject);
			}
		}
		else if (this.setup.sceneInfo.closeEnemies.Contains(this.rootTr.gameObject))
		{
			this.setup.sceneInfo.closeEnemies.Remove(this.rootTr.gameObject);
		}
		if (!BoltNetwork.isRunning)
		{
			if (this.mainPlayerDist > 75f)
			{
				this.awayFromPlayer = true;
			}
			else
			{
				this.awayFromPlayer = false;
			}
		}
		else
		{
			this.awayFromPlayer = false;
		}
	}

	private void checkTags()
	{
		if (!this.avatar.enabled)
		{
			return;
		}
		if (!this.rootTr.gameObject.activeSelf)
		{
			return;
		}
		if (Scene.SceneTracker.graphsBeingUpdated)
		{
			return;
		}
		if (this.setup.search.fsmInCave.Value)
		{
			if (this.setup.pmBrain)
			{
				this.fsmBrainGroundWalkable.Value = true;
			}
			return;
		}
		if (this.creepy || this.creepy_male || this.creepy_baby || this.creepy_fat)
		{
			return;
		}
		float num = this.thisTr.position.y - this.setup.animControl.terrainPosY;
		if (num < -2f || num > 25f)
		{
			if (this.setup.pmBrain)
			{
				this.fsmBrainGroundWalkable.Value = true;
			}
			return;
		}
		if (!AstarPath.active)
		{
			return;
		}
		this.groundNode = this.rg.GetNearest(this.thisTr.position).node;
		this.ugroundTag = this.groundNode.Tag;
		this.groundTag = (int)this.ugroundTag;
		if (this.groundNode != null)
		{
			if (this.groundNode.Walkable)
			{
				this.lastWalkablePos = new Vector3((float)(this.groundNode.position[0] / 1000), (float)(this.groundNode.position[1] / 1000), (float)(this.groundNode.position[2] / 1000));
			}
			if (this.setup.pmBrain)
			{
				this.fsmBrainGroundTag.Value = this.groundTag;
			}
			if (!this.groundNode.Walkable)
			{
				this.stuckCount++;
				if (this.stuckCount > 10 && this.allPlayers.Count > 0 && this.allPlayers[0] && this.mainPlayerDist > 250f)
				{
					Vector2 vector = this.randomCircle(50f);
					Vector3 position = new Vector3(this.thisTr.position.x + vector.x, this.thisTr.position.y, this.thisTr.position.z + vector.y);
					GraphNode node = this.rg.GetNearest(position, NNConstraint.Default).node;
					bool flag = false;
					using (List<uint>.Enumerator enumerator = Scene.MutantControler.mostCommonArea.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							int num2 = (int)enumerator.Current;
							if ((long)num2 == (long)((ulong)node.Area))
							{
								flag = true;
							}
						}
					}
					if (node.Walkable && flag)
					{
						this.setup.search.worldPositionTr.position = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
						this.rootTr.position = this.setup.search.worldPositionTr.position;
						this.stuckCount = 0;
					}
				}
			}
			else
			{
				this.stuckCount = 0;
			}
		}
		else if (this.setup.pmBrain)
		{
			this.fsmBrainGroundWalkable.Value = false;
		}
	}

	protected float XZSqrMagnitude(Vector3 a, Vector3 b)
	{
		float num = b.x - a.x;
		float num2 = b.z - a.z;
		return num * num + num2 * num2;
	}

	public virtual void SearchPath()
	{
		if (this.canSearch && base.gameObject.activeInHierarchy)
		{
			if (this.target == null)
			{
				return;
			}
			if (this.controller && !BoltNetwork.isRunning && (!this.avatar.enabled || !this.controller.enabled))
			{
				this.seeker.StartPath(this.setup.search.worldPositionTr.position, this.target.position + this.targetOffset, new OnPathDelegate(this.OnPathComplete));
			}
			else
			{
				this.seeker.StartPath(this.thisTr.position, this.target.position + this.targetOffset, new OnPathDelegate(this.OnPathComplete));
			}
		}
	}

	public void OnPathComplete(Path p)
	{
		if (!p.error)
		{
			if (this.path != null)
			{
				this.path.Release(this, false);
			}
			this.path = p;
			this.path.Claim(this);
			if (this.groundNode != null && this.target != null && this.groundNode.Walkable && !this.setup.search.fsmInCave.Value && !this.creepy && !this.creepy_baby && !this.creepy_male && !this.creepy_fat)
			{
				uint area = this.groundNode.Area;
				NNConstraint nnconstraint = new NNConstraint();
				nnconstraint.constrainArea = true;
				int area2 = (int)area;
				nnconstraint.area = area2;
				this.targetNode = this.rg.GetNearest(this.target.position, NNConstraint.Default).node;
				bool flag = false;
				targetStats targetStats = null;
				if (this.target.gameObject.CompareTag("Player") || this.target.gameObject.CompareTag("PlayerNet"))
				{
					targetStats = this.target.GetComponent<targetStats>();
					if (targetStats.inWater || targetStats.onRaft)
					{
						flag = true;
					}
				}
				if (this.targetNode != null || flag)
				{
					if (flag)
					{
						GraphNode node = this.rg.GetNearest(this.target.position, nnconstraint).node;
						Vector3 position = this.target.position;
						if (node == null)
						{
							node = this.groundNode;
						}
						this.lastPlayerTarget = this.target;
						position = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
						this.setup.search.updateCurrentWaypoint(position);
						this.setup.search.setToWaypoint();
						if (this.setup.pmCombat)
						{
							this.setup.pmCombat.SendEvent("noValidPath");
						}
						if (this.setup.pmEncounter)
						{
							this.setup.pmEncounter.SendEvent("noValidPath");
						}
						if (this.setup.pmSearchScript)
						{
							this.setup.pmSearchScript._validPath = false;
						}
						if (this.setup.pmCombat)
						{
							this.setup.pmCombat.FsmVariables.GetFsmBool("playerWasOffNav").Value = true;
						}
					}
					else if (this.targetNode.Area != this.groundNode.Area)
					{
						GraphNode node2 = this.rg.GetNearest(this.target.position, nnconstraint).node;
						Vector3 position2 = this.target.position;
						if (node2 != null)
						{
							position2 = new Vector3((float)(node2.position[0] / 1000), (float)(node2.position[1] / 1000), (float)(node2.position[2] / 1000));
						}
						float num = this.target.position.y - position2.y;
						if (!this.target.gameObject.CompareTag("Player") && !this.target.gameObject.CompareTag("PlayerNet"))
						{
							this.setup.search.updateCurrentWaypoint(base.transform.position);
						}
						if (Vector3.Distance(position2, this.target.position) > 10f)
						{
							if (this.target.gameObject.CompareTag("Player") || (this.target.gameObject.CompareTag("PlayerNet") && num > 2f))
							{
								if (!targetStats || targetStats.onStructure)
								{
								}
								if (!this.climbingWallCooDown && !this.doneClimbCoolDown && !this.setup.hitReactions.onStructure)
								{
									base.StartCoroutine(this.findCloseClimbingWall(this.target, this.targetNode));
									this.lastPlayerTarget = this.target;
									this.climbingWallCooDown = true;
								}
								if (this.setup.pmBrain && !this.doingClimbWall)
								{
									this.setup.pmBrain.SendEvent("toSetPassive");
								}
							}
							this.setup.search.updateCurrentWaypoint(position2);
							if (!this.setup.hitReactions.onStructure && !this.fsmOnWallBool.Value)
							{
								if (this.setup.pmCombat)
								{
									this.setup.pmCombat.SendEvent("noValidPath");
								}
								if (this.setup.pmEncounter)
								{
									this.setup.pmEncounter.SendEvent("noValidPath");
								}
								if (this.setup.pmSearchScript)
								{
									this.setup.pmSearchScript._validPath = false;
								}
							}
						}
					}
					if (this.target.gameObject.CompareTag("Player") || this.target.gameObject.CompareTag("PlayerNet"))
					{
						if (this.setup.hitReactions.onStructure && this.thisTr.position.y - this.target.position.y > 3f)
						{
							if (this.setup.pmCombat)
							{
								this.setup.pmCombat.FsmVariables.GetFsmBool("attackBelowBool").Value = true;
							}
						}
						else if (this.setup.pmCombat)
						{
							this.setup.pmCombat.FsmVariables.GetFsmBool("attackBelowBool").Value = false;
						}
					}
				}
			}
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.SendEvent("validPath");
			}
			if (this.setup.pmEncounter)
			{
				this.setup.pmEncounter.SendEvent("validPath");
			}
			if (this.setup.pmSearchScript)
			{
				this.setup.pmSearchScript._validPath = true;
			}
			this.currentWaypoint = 1;
			return;
		}
		if (this.setup.pmCombat)
		{
			this.setup.pmCombat.SendEvent("noValidPath");
		}
		if (this.setup.pmEncounter)
		{
			this.setup.pmEncounter.SendEvent("noValidPath");
		}
		if (this.setup.pmSearchScript)
		{
			this.setup.pmSearchScript._validPath = false;
		}
	}

	private void resetInsideBase()
	{
		this.insideBase = false;
		if (this.setup && this.setup.pmCombat)
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("fsmInsideBase").Value = false;
		}
	}

	public IEnumerator setRepathRate(float rate)
	{
		this.repathRate = rate;
		yield return null;
		yield break;
	}

	private void startMovement()
	{
		this.doMove = true;
		this.canSearch = true;
		if (!base.IsInvoking("SearchPath"))
		{
			if (this.mainPlayerDist > 80f)
			{
				base.InvokeRepeating("SearchPath", 0f, this.distantRepathRate);
			}
			else
			{
				base.InvokeRepeating("SearchPath", 0f, this.repathRate);
			}
		}
		base.StartCoroutine("doMovement");
	}

	private void stopMovement()
	{
		this.doMove = false;
		this.canSearch = false;
		base.StopCoroutine("doMovement");
		base.CancelInvoke("SearchPath");
	}

	public void enablePathSearch()
	{
		this.canSearch = true;
		this.SearchPath();
	}

	private void Update()
	{
		if (this.allPlayers.Count == 0)
		{
			return;
		}
		if (this.target == null && this.allPlayers.Count > 0 && this.allPlayers[0] != null)
		{
			this.target = this.allPlayers[0].transform;
		}
		if (this.setup.hitReactions && !this.setup.hitReactions.onStructure && this.setup.pmCombat)
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("attackBelowBool").Value = false;
		}
		if (this.target)
		{
			this.fsmTarget.Value = this.target.gameObject;
			if (this.setup.pmCombat && this.setup.search.currentTarget)
			{
				this.fsmCurrentAttackerGo.Value = this.setup.search.currentTarget;
			}
			if ((this.target.gameObject.CompareTag("Player") || this.target.gameObject.CompareTag("PlayerNet")) && this.setup.pmBrain)
			{
				targetStats component = this.target.GetComponent<targetStats>();
				if (component && this.targetDist < 40f)
				{
					if (component.isRed && !this.setup.collisionDetect.fsmFearOverrideBool.Value && !this.fsmBrainPlayerRedBool.Value)
					{
						this.setup.pmBrain.SendEvent("toSetFearful");
						this.setup.aiManager.flee = true;
						this.setup.pmBrain.FsmVariables.GetFsmGameObject("fearTargetGo").Value = this.target.gameObject;
						this.fsmBrainPlayerRedBool.Value = true;
						base.Invoke("resetPlayerRedReaction", 9f);
					}
					else if (!component.isRed && this.setup.pmBrain.FsmVariables.GetFsmBool("playerIsRed").Value)
					{
						this.resetPlayerRedReaction();
					}
				}
			}
		}
		if (LocalPlayer.Transform != null)
		{
			LocalPlayer.Transform.position.y = this.thisTr.position.y;
			if (this.allPlayers[0])
			{
				if (this.allPlayers[0] != null)
				{
					this.mainPlayerHeight = this.thisTr.position.y - this.allPlayers[0].transform.position.y;
					this.mainPlayerDist = Vector3.Distance(this.thisTr.position, this.allPlayers[0].transform.position);
					this.tempTarget = this.thisTr.InverseTransformPoint(this.allPlayers[0].transform.position);
					this.mainPlayerAngle = Mathf.Atan2(this.tempTarget.x, this.tempTarget.z) * 57.29578f;
				}
			}
			else
			{
				this.mainPlayerHeight = this.thisTr.position.y - LocalPlayer.Transform.position.y;
				this.mainPlayerDist = Vector3.Distance(this.thisTr.position, LocalPlayer.Transform.position);
				this.tempTarget = this.thisTr.InverseTransformPoint(LocalPlayer.Transform.position);
				this.mainPlayerAngle = Mathf.Atan2(this.tempTarget.x, this.tempTarget.z) * 57.29578f;
			}
		}
		if (this.creepy_boss)
		{
			this.avatar.SetFloat("playerDist", this.mainPlayerDist);
			this.avatar.SetFloat("playerAngle", this.mainPlayerAngle);
		}
		if (this.setup.search.currentTarget)
		{
			this.playerDist = Vector3.Distance(this.setup.search.currentTarget.transform.position, this.thisTr.position);
			this.playerTarget = this.thisTr.InverseTransformPoint(this.setup.search.currentTarget.transform.position);
			if (this.avatar.enabled)
			{
				this.playerAngle = Mathf.Atan2(this.playerTarget.x, this.playerTarget.z) * 57.29578f;
				if (this.creepy_boss)
				{
					this.fsmPlayerAngle.Value = this.playerAngle;
				}
				else
				{
					this.fsmPlayerAngle.Value = this.playerAngle;
				}
			}
			else
			{
				this.playerAngle = 0f;
				this.fsmPlayerAngle.Value = this.playerAngle;
			}
			if (this.avatar.enabled)
			{
				if (this.creepy_fat || this.creepy_boss)
				{
					if (this.creepy_boss)
					{
						this.avatar.SetFloatReflected("playerAngle", this.mainPlayerAngle);
					}
					else
					{
						this.avatar.SetFloatReflected("playerAngle", this.mainPlayerAngle);
					}
				}
				else
				{
					this.avatar.SetFloatReflected("playerAngle", this.playerAngle);
				}
			}
		}
		if (this.creepy || this.creepy_male || this.creepy_baby || this.creepy_fat)
		{
			this.fsmPlayerDist.Value = this.mainPlayerDist;
		}
		else
		{
			this.fsmClosestPlayerHeight.Value = this.mainPlayerHeight;
			this.fsmClosestPlayerDist.Value = this.mainPlayerDist;
			this.fsmPlayerDist.Value = this.playerDist;
		}
		if (this.setup.search.currentTarget && !this.awayFromPlayer)
		{
			Collider currentTargetCollider = this.setup.search.currentTargetCollider;
			if (this.setup.headJoint)
			{
				if (currentTargetCollider)
				{
					this.playerDir = currentTargetCollider.bounds.center - this.setup.headJoint.transform.position;
				}
				else
				{
					this.playerDir = this.setup.search.currentTarget.transform.position - this.setup.headJoint.transform.position;
				}
				this.fsmPlayerDir.Value = this.playerDir;
			}
			else if (currentTargetCollider)
			{
				this.playerDir = currentTargetCollider.bounds.center - this.thisTr.position;
				this.fsmPlayerDir.Value = this.playerDir;
			}
		}
		float num = this.targetAngle;
		if (this.target)
		{
			this.localTarget = this.thisTr.InverseTransformPoint(this.target.position + this.targetOffset);
			this.targetAngle = Mathf.Atan2(this.localTarget.x, this.localTarget.z) * 57.29578f;
		}
		if (this.avatar.enabled)
		{
			this.avatar.SetFloatReflected("TargetDir", this.targetAngle);
			this.fsmTargetDir.Value = this.targetAngle;
		}
		else
		{
			this.avatar.SetFloatReflected("TargetDir", 0f);
			this.fsmTargetDir.Value = 0f;
		}
		if (this.target)
		{
			this.targetDist = Vector3.Distance(this.target.position + this.targetOffset, this.thisTr.position);
		}
		this.fsmTargetDist.Value = this.targetDist;
		if (this.setup.pmBrain)
		{
			this.fsmBrainPlayerAngle.Value = this.playerAngle;
			this.fsmBrainPlayerDist.Value = this.mainPlayerDist;
			this.fsmBrainPlayerDist2D.Value = this.mainPlayerDist;
			this.fsmBrainTargetDist.Value = this.targetDist;
			this.fsmBrainTargetDir.Value = this.targetAngle;
		}
	}

	private IEnumerator doMovement()
	{
		IL_8AF:
		while (this.doMove)
		{
			while (this.path == null || this.avatar == null)
			{
				yield return null;
			}
			if ((this.creepy || this.creepy_fat || this.creepy_male) && this.fsmLeaderGo.Value)
			{
				if (this.fsmLeaderGo.Value.activeSelf && this.setup.search.followingLeader)
				{
					if (this.targetDist > 35f)
					{
						this.avatar.SetFloat("Speed", 1f);
						if (this.creepy_fat)
						{
							this.avatar.SetBool("charge", true);
						}
					}
					else
					{
						this.avatar.SetFloat("Speed", 0.3f);
						if (this.creepy_fat)
						{
							this.avatar.SetBool("charge", false);
						}
					}
				}
				else
				{
					this.targetOffset = Vector3.zero;
				}
			}
			this.currentDir = this.avatar.rootRotation * Vector3.forward;
			List<Vector3> vPath = this.path.vectorPath;
			if (this.currentWaypoint < 3)
			{
				this.nextWaypointDistance = 4.2f;
			}
			else
			{
				this.nextWaypointDistance = this.storeNextWaypointDistance;
			}
			if (this.currentWaypoint >= vPath.Count)
			{
				this.currentWaypoint = vPath.Count - 1;
			}
			if (this.currentWaypoint <= 1)
			{
				this.currentWaypoint = 1;
			}
			while (this.currentWaypoint < this.path.vectorPath.Count - 1)
			{
				Vector3 position = this.thisTr.position;
				if (this.controller && !BoltNetwork.isRunning && (!this.avatar.enabled || !this.controller.enabled))
				{
					position = this.setup.search.worldPositionTr.position;
				}
				float num = this.XZSqrMagnitude(this.path.vectorPath[this.currentWaypoint], position);
				if (num >= this.nextWaypointDistance * this.nextWaypointDistance)
				{
					IL_376:
					if (this.path.vectorPath.Count > 1)
					{
						this.targetDirection = this.path.vectorPath[this.currentWaypoint] - this.avatar.rootPosition;
						this.wantedDir = this.targetDirection.normalized;
						this.wantedPos = vPath[this.currentWaypoint];
					}
					if (this.setup.pmMotor)
					{
						this.fsmWantedDir.Value = this.wantedDir;
					}
					if (this.currentWaypoint < vPath.Count - 1)
					{
						Vector3 position2 = this.thisTr.position;
						if (this.controller && !BoltNetwork.isRunning && (!this.avatar.enabled || !this.controller.enabled))
						{
							position2 = this.setup.search.worldPositionTr.position;
						}
						if (Vector3.Distance(position2, this.path.vectorPath[this.currentWaypoint]) < this.nextWaypointDistance)
						{
							this.currentWaypoint++;
						}
					}
					if (!this.creepy && !this.creepy_male)
					{
						float f = this.FindAngle(this.thisTr.forward, this.wantedDir, this.thisTr.up);
						if (Mathf.Abs(f) < this.deadZone)
						{
							this.absDir = 0f;
						}
						else if (Vector3.Dot(this.currentDir, this.wantedDir) > 0f)
						{
							this.absDir = Vector3.Cross(this.currentDir, this.wantedDir).y;
						}
						else
						{
							this.absDir = (float)((Vector3.Cross(this.currentDir, this.wantedDir).y <= 0f) ? -1 : 1);
						}
						if (this.mainPlayerDist > 65f)
						{
							this.rotationSpeed = 20f;
						}
						else
						{
							this.rotationSpeed = this.getRotSpeed;
						}
						this.smoothedDir = Mathf.Lerp(this.fsmSmoothPathDir.Value, this.absDir, Time.deltaTime * this.rotationSpeed);
						this.fsmSmoothPathDir.Value = this.smoothedDir;
						if (this.avatar.enabled)
						{
							this.avatar.SetFloatReflected("Direction", this.smoothedDir * 10f);
						}
					}
					if (!this.maleSkinny && !this.femaleSkinny && !this.creepy && !this.creepy_baby && !this.creepy_fat && !this.creepy_male)
					{
						Vector3 vector = this.thisTr.InverseTransformPoint(this.wantedPos);
						this.waypointAngle = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
						if (this.waypointAngle > 80f && this.waypointAngle < 135f)
						{
							this.avatar.SetInteger("sharpTurnDir", 1);
						}
						else if (this.waypointAngle < -80f && this.waypointAngle > -135f)
						{
							this.avatar.SetInteger("sharpTurnDir", -1);
						}
						else if (this.waypointAngle > 175f || this.waypointAngle < -175f)
						{
							this.avatar.SetInteger("sharpTurnDir", -2);
						}
						else
						{
							this.avatar.SetInteger("sharpTurnDir", 0);
						}
					}
					yield return null;
					goto IL_8AF;
				}
				this.currentWaypoint++;
			}
			goto IL_376;
		}
		yield break;
	}

	private void disableController()
	{
		this.controller.GetComponent<Collider>().enabled = false;
	}

	private float FindAngle(Vector3 fromVector, Vector3 toVector, Vector3 upVector)
	{
		if (toVector == Vector3.zero)
		{
			return 0f;
		}
		float num = Vector3.Angle(fromVector, toVector);
		Vector3 lhs = Vector3.Cross(fromVector, toVector);
		num *= Mathf.Sign(Vector3.Dot(lhs, upVector));
		return num * 0.0174532924f;
	}

	private void restartAI()
	{
		if (!this.creepy && !this.creepy_male && !this.cutScene && !this.creepy_fat)
		{
			string activeStateName = this.setup.pmCombat.ActiveStateName;
			string activeStateName2 = this.setup.pmEncounter.ActiveStateName;
			string activeStateName3 = this.setup.pmSleep.ActiveStateName;
			if (!(activeStateName == "disabled") || !(activeStateName2 == "disabled") || activeStateName3 == "disabled")
			{
			}
		}
	}

	public void cancelDefaultActions()
	{
		if (!this.avatar.enabled)
		{
			return;
		}
		this.avatar.SetBoolReflected("onRockBOOL", false);
		this.avatar.SetBoolReflected("dodgeBOOL", false);
		this.avatar.SetBoolReflected("fearBOOL", false);
		this.avatar.SetBoolReflected("backAwayBOOL", false);
		this.avatar.SetBoolReflected("crouchBOOL", false);
		this.avatar.SetBoolReflected("sideWalkBOOL", false);
		this.avatar.SetBoolReflected("staggerBOOL", false);
		this.avatar.SetBoolReflected("freakoutBOOL", false);
		this.avatar.SetBool("ritualBOOL", false);
	}

	public void resetCombatParams()
	{
		if (!this.avatar.enabled)
		{
			return;
		}
		this.avatar.SetBoolReflected("callBool", false);
		this.avatar.SetBoolReflected("onRockBOOL", false);
		this.avatar.SetBoolReflected("dodgeBOOL", false);
		this.avatar.SetBoolReflected("idleWaryBOOL", false);
		this.avatar.SetBoolReflected("fearBOOL", false);
		this.avatar.SetBoolReflected("backAwayBOOL", false);
		this.avatar.SetBoolReflected("crouchBOOL", false);
		this.avatar.SetBoolReflected("sideWalkBOOL", false);
		this.avatar.SetBoolReflected("staggerBOOL", false);
		this.avatar.SetBoolReflected("freakoutBOOL", false);
		this.avatar.SetBoolReflected("jumpBlockBool", false);
		this.avatar.SetBoolReflected("recoverBool", false);
		this.avatar.SetBoolReflected("rescueBool1", false);
		this.avatar.SetBoolReflected("walkBOOL", false);
		this.avatar.SetBoolReflected("sideWalkBOOL", false);
		this.avatar.SetBoolReflected("actionBOOL1", false);
		this.avatar.SetBoolReflected("damageBOOL", false);
		this.avatar.SetBoolReflected("damageBehindBOOL", false);
		this.avatar.SetBoolReflected("screamBOOL", false);
		this.avatar.SetBoolReflected("runAwayBOOL", false);
		this.avatar.SetBoolReflected("crouchBOOL", false);
		this.avatar.SetBoolReflected("attackBOOL", false);
		this.avatar.SetBoolReflected("attackRightBOOL", false);
		this.avatar.SetBoolReflected("attackLeftBOOL", false);
		this.avatar.SetBoolReflected("attackJumpBOOL", false);
		this.avatar.SetBoolReflected("attackMovingBOOL", false);
		this.avatar.SetBoolReflected("turnLeftBOOL", false);
		this.avatar.SetBoolReflected("turnRightBOOL", false);
		this.avatar.SetBoolReflected("turnAroundBOOL", false);
		this.avatar.SetBoolReflected("sideStepBOOL", false);
		this.avatar.SetBoolReflected("dodgeBOOL", false);
		this.avatar.SetBoolReflected("treeJumpBOOL", false);
		this.avatar.SetBoolReflected("attackJumpBOOL", false);
		this.avatar.SetBoolReflected("backAwayBOOL", false);
		this.avatar.SetBoolReflected("soundBool1", false);
		this.avatar.SetBool("feedingBOOL", false);
		this.avatar.SetInteger("climbDirInt", 0);
		this.avatar.SetBool("ritualBOOL", false);
	}

	public void forceTreeDown()
	{
		if (!this.avatar.enabled)
		{
			return;
		}
		if (this.setup.pmCombat.FsmVariables.GetFsmBool("toClimbWall").Value)
		{
			return;
		}
		if (this.avatar.GetBool("treeBOOL"))
		{
			this.avatar.SetIntegerReflected("randInt1", UnityEngine.Random.Range(0, 2));
			this.avatar.SetBoolReflected("treeBOOL", false);
			this.avatar.SetBoolReflected("treeJumpBOOL", false);
			this.avatar.SetBoolReflected("attackJumpBOOL", false);
			this.setup.pmCombat.FsmVariables.GetFsmBool("inTreeBool").Value = false;
		}
	}

	public IEnumerator toWalk()
	{
		if (this.startedMove)
		{
			yield break;
		}
		base.StopCoroutine("toRun");
		base.StopCoroutine("toStop");
		base.StopCoroutine("toSearch");
		this.startMovement();
		this.movingBool = true;
		this.startedMove = true;
		this.startedRun = false;
		this.startedSearch = false;
		for (;;)
		{
			this.avatar.SetFloatReflected("Speed", 0.3f);
			yield return YieldPresets.WaitPointThreeSeconds;
		}
		yield break;
	}

	public IEnumerator toRun()
	{
		if (this.startedRun)
		{
			yield break;
		}
		base.StopCoroutine("toWalk");
		base.StopCoroutine("toStop");
		base.StopCoroutine("toSearch");
		this.startMovement();
		this.movingBool = true;
		this.startedRun = true;
		this.startedMove = false;
		this.startedSearch = false;
		for (;;)
		{
			this.avatar.SetFloat("Speed", 1f);
			yield return YieldPresets.WaitPointThreeSeconds;
		}
		yield break;
	}

	public IEnumerator toSearch()
	{
		if (this.startedSearch)
		{
			yield break;
		}
		base.StopCoroutine("toWalk");
		base.StopCoroutine("toRun");
		base.StopCoroutine("toStop");
		this.startMovement();
		this.startedSearch = true;
		this.startedRun = false;
		this.startedMove = false;
		yield break;
	}

	public IEnumerator toStop()
	{
		base.StopCoroutine("toWalk");
		base.StopCoroutine("toRun");
		base.StopCoroutine("toSearch");
		this.stopMovement();
		this.avatar.SetFloatReflected("Speed", 0f);
		this.movingBool = false;
		this.startedRun = false;
		this.startedMove = false;
		this.startedSearch = false;
		yield return null;
		yield break;
	}

	private Vector2 randomCircle(float radius)
	{
		Vector2 normalized = UnityEngine.Random.insideUnitCircle.normalized;
		return normalized * radius;
	}

	private IEnumerator findCloseClimbingWall(Transform getTarget, GraphNode targetNode)
	{
		bool foundValidWall = false;
		int randVal = UnityEngine.Random.Range(0, Scene.SceneTracker.climbableStructures.Count);
		for (int i = 0; i < Scene.SceneTracker.climbableStructures.Count; i++)
		{
			if (randVal == Scene.SceneTracker.climbableStructures.Count)
			{
				randVal = 0;
			}
			if (!foundValidWall && getTarget && Scene.SceneTracker.climbableStructures[randVal] != null)
			{
				Vector3 position = getTarget.position;
				position.y = this.thisTr.position.y;
				Vector3 position2 = Scene.SceneTracker.climbableStructures[randVal].transform.position;
				position2.y = this.thisTr.position.y;
				Vector3 vector = position - position2;
				Vector3 from = this.thisTr.position - position2;
				Vector3 to = this.thisTr.position - position;
				float num = Vector3.Angle(from, to);
				float sqrMagnitude = vector.sqrMagnitude;
				float sqrMagnitude2 = from.sqrMagnitude;
				float sqrMagnitude3 = to.sqrMagnitude;
				if (sqrMagnitude < sqrMagnitude3 && sqrMagnitude < 2500f && sqrMagnitude2 > 225f && num < 70f)
				{
					Vector3 position3 = Scene.SceneTracker.climbableStructures[randVal].transform.position;
					position3.y = getTarget.position.y;
					uint area = targetNode.Area;
					NNConstraint nnconstraint = new NNConstraint();
					nnconstraint.constrainArea = true;
					int area2 = (int)area;
					nnconstraint.area = area2;
					bool flag = false;
					GraphNode node = this.rg.GetNearest(position3, nnconstraint).node;
					if (node != null)
					{
						Vector3 b = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
						if (Vector3.Distance(position3, b) < 8f)
						{
							flag = true;
						}
					}
					climbableWallSetup component = Scene.SceneTracker.climbableStructures[randVal].GetComponent<climbableWallSetup>();
					if (component && !component.occupied && !component.invalid && flag)
					{
						this.setup.pmCombat.FsmVariables.GetFsmGameObject("wallGo").Value = Scene.SceneTracker.climbableStructures[randVal];
						this.setup.pmCombatScript.wallGo = Scene.SceneTracker.climbableStructures[randVal];
						this.setup.pmCombat.FsmVariables.GetFsmBool("toClimbWall").Value = true;
						foundValidWall = true;
						if (component)
						{
							component.occupied = true;
						}
					}
				}
			}
			randVal++;
			yield return null;
		}
		yield return YieldPresets.WaitTenSeconds;
		this.climbingWallCooDown = false;
		yield break;
	}

	public void setClimbWallCoolDown()
	{
		this.doneClimbCoolDown = true;
		base.Invoke("resetClimbWallCoolDown", 25f);
	}

	private void resetClimbWallCoolDown()
	{
		this.doneClimbCoolDown = false;
	}

	public void resetPlayerRedReaction()
	{
		if (!this.setup)
		{
			return;
		}
		this.setup.pmCombat.FsmVariables.GetFsmBool("fearBOOL").Value = false;
		if (this.setup.aiManager)
		{
			this.setup.aiManager.flee = false;
		}
		if (this.setup.pmBrain)
		{
			this.setup.pmBrain.SendEvent("toActivateFSM");
			this.setup.pmBrain.FsmVariables.GetFsmBool("playerIsRed").Value = false;
		}
	}

	public bool leader;

	public bool maleSkinny;

	public bool femaleSkinny;

	public bool male;

	public bool female;

	public bool creepy;

	public bool creepy_male;

	public bool creepy_baby;

	public bool creepy_fat;

	public bool fireman;

	public bool fireman_dynamite;

	public bool pale;

	public bool cutScene;

	public bool creepy_boss;

	public bool painted;

	public bool skinned;

	public MecanimEventData _mecanimEventPrefab;

	private Transform testSphere;

	protected CharacterController controller;

	protected Animator avatar;

	protected Seeker seeker;

	public Transform target;

	public Vector3 targetOffset;

	public float nextWaypointDistance;

	private float storeNextWaypointDistance;

	public int currentWaypoint;

	public float repathRate;

	public float distantRepathRate = 4f;

	private Transform thisTr;

	private Transform rootTr;

	public Path path;

	private mutantScriptSetup setup;

	public float animSpeed;

	public bool awayFromPlayer;

	public Vector3 lastWalkablePos;

	public bool movingBool;

	public Transform lastPlayerTarget;

	public bool girlFullyTransformed;

	private AnimatorStateInfo currentState;

	private AnimatorStateInfo nextState;

	public PlayMakerFSM playMaker;

	public PlayMakerFSM pmSearch;

	public PlayMakerFSM pmVision;

	public PlayMakerFSM pmTree;

	public PlayMakerFSM pmMotor;

	public PlayMakerFSM pmSleep;

	private Vector3 targetDirection;

	public float smoothedDir;

	public float absDir;

	public bool doMove;

	public bool canSearch;

	public List<GameObject> allPlayers = new List<GameObject>();

	public mutantAI_net ai_net;

	public Transform playerHead;

	public Transform playerHips;

	public GameObject headJoint;

	public float approachDist;

	public float deadZone;

	public float rotationSpeed;

	public Vector3 wantedDir;

	public Vector3 wantedPos;

	private Vector3 playerDir;

	private Vector3 localTarget;

	private Vector3 currentDir;

	public float targetAngle;

	public float targetDist;

	private Vector3 playerTarget;

	private Vector3 tempTarget;

	public float mainPlayerAngle;

	private float playerAngle;

	public float mainPlayerDist;

	private float getRotSpeed;

	public bool insideBase;

	public float mainPlayerHeight;

	private uint ugroundTag;

	private int groundTag;

	public GraphNode groundNode;

	public GraphNode targetNode;

	private int stuckCount;

	private int offNavMeshCount;

	private FsmFloat fsmPathDir;

	private FsmFloat fsmSmoothPathDir;

	private FsmFloat fsmSpeed;

	private FsmFloat fsmTargetDir;

	public FsmFloat fsmTargetDist;

	private FsmFloat fsmSearchTargetDist;

	private FsmFloat fsmIkBlend;

	private FsmGameObject fsmTarget;

	private FsmGameObject fsmSearchTarget;

	private FsmFloat fsmPlayerDist;

	private FsmFloat fsmClosestPlayerHeight;

	private FsmFloat fsmStalkPlayerDist;

	private FsmVector3 fsmPlayerDir;

	private FsmFloat fsmPlayerAngle;

	private FsmVector3 fsmPathVector;

	private FsmVector3 fsmWantedDir;

	private FsmBool fsmMoving;

	private FsmGameObject fsmBrainTarget;

	private FsmFloat fsmBrainPlayerDist;

	private FsmFloat fsmBrainPlayerDist2D;

	private FsmFloat fsmBrainPlayerAngle;

	private FsmFloat fsmBrainTargetDist;

	private FsmFloat fsmBrainTargetDir;

	private FsmInt fsmBrainGroundTag;

	private FsmBool fsmBrainGroundWalkable;

	private FsmInt fsmPlayerGroundTag;

	private FsmGameObject fsmCurrentAttackerGo;

	public FsmFloat fsmRotateSpeed;

	private FsmFloat fsmClosestPlayerDist;

	public FsmBool fsmOnWallBool;

	private FsmBool fsmBrainPlayerRedBool;

	private FsmGameObject fsmLeaderGo;

	private FsmBool fsmAnimatorEnabled;

	public float playerDist;

	private int layerMask;

	public int wallLayerMask;

	private bool doingClimbWall;

	private bool climbingWallCooDown;

	private bool doneClimbCoolDown;

	private Vector3 prevPos;

	public RecastGraph rg;

	public float waypointAngle;

	public bool startedRun;

	public bool startedMove;

	public bool startedSearch;

	private float followDist;
}
