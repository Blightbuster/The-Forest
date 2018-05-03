using System;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using Pathfinding;
using PathologicalGames;
using TheForest.Utils;
using TheForest.Utils.Settings;
using UnityEngine;


public class mutantSearchFunctions : MonoBehaviour
{
	
	
	
	public GameObject currentTarget
	{
		get
		{
			return this.privateCurrentTarget;
		}
		set
		{
			this.privateCurrentTarget = value;
			this.currentTargetCollider = base.GetComponent<Collider>();
		}
	}

	
	private void OnDeserialized()
	{
		base.StopAllCoroutines();
	}

	
	private void Awake()
	{
		this.tr = base.transform;
		this.rootTr = base.transform.parent;
		this.player = LocalPlayer.GameObject;
		this.ai = base.GetComponent<mutantAI>();
		this.sceneInfo = Scene.SceneTracker;
		this.setup = base.GetComponent<mutantScriptSetup>();
		this.animator = base.GetComponent<Animator>();
		this.collideDetect = base.transform.root.GetComponentInChildren<mutantCollisionDetect>();
		this.currentWaypoint = this.setup.currentWaypoint;
		this.lastSighting = this.setup.lastSighting;
		this.lookatTr = this.setup.lookatTr;
		this.familyFunctions = base.transform.root.GetComponent<mutantFamilyFunctions>();
	}

	
	private void Start()
	{
		if (this.setup.pmCombat)
		{
			this.currentStructureGo = this.setup.pmCombat.FsmVariables.GetFsmGameObject("structureGo");
			this.fsmClosestPlayerAngle = this.setup.pmCombat.FsmVariables.GetFsmFloat("closestPlayerAngle");
			this.fsmObjectAngle = this.setup.pmCombat.FsmVariables.GetFsmFloat("objectAngle");
			this.fsmCurrentTargetGo = this.setup.pmCombat.FsmVariables.GetFsmGameObject("currentTargetGo");
			this.fsmCombatClosestPlayerDist = this.setup.pmCombat.FsmVariables.GetFsmFloat("closestPlayerDist");
		}
		if (this.setup.pmSleep)
		{
			this.fsmCaveEntrance = this.setup.pmSleep.FsmVariables.GetFsmGameObject("closestEntranceGo");
			this.fsmInCave = this.setup.pmSleep.FsmVariables.GetFsmBool("inCaveBool");
		}
		if (this.setup.pmBrain)
		{
			this.fsmLeaderGo = this.setup.pmBrain.FsmVariables.GetFsmGameObject("leaderGo");
		}
		else
		{
			this.fsmLeaderGo = this.setup.pmCombat.FsmVariables.GetFsmGameObject("leaderGo");
		}
		if (this.ai.creepy_baby)
		{
			this.fsmLeaderGo = this.setup.pmCombat.FsmVariables.GetFsmGameObject("leaderGo");
		}
		if (this.ai.creepy || this.ai.creepy_baby || this.ai.creepy_male || this.ai.creepy_fat)
		{
			this.fsmInCave = this.setup.pmCombat.FsmVariables.GetFsmBool("inCaveBool");
		}
		this.currentWaypoint.transform.parent = null;
		this.lastSighting.transform.parent = null;
		this.layerMask = 67239936;
		this.visLayerMask = 104212480;
		this.rg = AstarPath.active.astarData.recastGraph;
		this.nmg = AstarPath.active.astarData.navmesh;
	}

	
	private void OnEnable()
	{
		if (!base.IsInvoking("updateSearchParams"))
		{
			base.InvokeRepeating("updateSearchParams", 1f, 1f);
		}
		if (!base.IsInvoking("getClosestPlayerDist"))
		{
			base.InvokeRepeating("getClosestPlayerDist", 1f, 1f);
		}
		this.setModEncounterRange(0f);
	}

	
	private void OnDisable()
	{
		this.screamCooldown = false;
		this.playerAware = false;
		if (LocalPlayer.ScriptSetup && LocalPlayer.ScriptSetup.targetFunctions.visibleEnemies.Contains(base.transform.parent.gameObject))
		{
			LocalPlayer.ScriptSetup.targetFunctions.visibleEnemies.Remove(base.transform.parent.gameObject);
		}
		base.CancelInvoke("updateSearchParams");
		base.CancelInvoke("getClosestPlayerDist");
		base.StopCoroutine("toLook");
		base.StopCoroutine("toTrack");
		base.StopCoroutine("enableAwareOfPlayer");
		this.trackCounter = 0;
		this.startedTrack = false;
		this.startedLook = false;
		if (Scene.SceneTracker)
		{
			Scene.SceneTracker.closeStructureTarget = null;
		}
	}

	
	public void setupCrouchedVisLayerMask()
	{
		this.visLayerMask = 104208384;
	}

	
	public void setupStandingVisLayerMask()
	{
		this.visLayerMask = 104204288;
	}

	
	public void setModEncounterRange(float amount)
	{
		this.modEncounterRange = amount;
	}

	
	private void updateSearchParams()
	{
		if (Clock.Dark || this.setup.typeSetup.inCave)
		{
			this.setCloseVisRange = this.closeVisRange;
			this.setLongVisRange = this.longVisRange;
			if (this.setCloseVisRange < 8f)
			{
				this.setCloseVisRange = 8f;
			}
			if (this.setLongVisRange < 25f)
			{
				this.setLongVisRange = 25f;
			}
		}
		else
		{
			this.setCloseVisRange = this.closeVisRange;
			this.setLongVisRange = this.longVisRange;
			if (this.setCloseVisRange < 12f)
			{
				this.setCloseVisRange = 12f;
			}
			if (this.setLongVisRange < 30f)
			{
				this.setLongVisRange = 30f;
			}
		}
		if (!this.ai.creepy && !this.ai.creepy_baby && !this.ai.creepy_male && !this.ai.creepy_fat)
		{
			if (this.fsmInCave.Value)
			{
				this.setup.pmSearchScript._targetStopDist = 6f;
			}
			else
			{
				this.setup.pmSearchScript._targetStopDist = 15f;
			}
		}
	}

	
	private void Update()
	{
		if (this.currentTarget)
		{
			this.currentTargetDist = Vector3.Distance(this.tr.position, this.currentTarget.transform.position);
		}
		if (!this.currentTarget)
		{
			this.currentTarget = this.currentWaypoint;
		}
	}

	
	private void setCeilingPos()
	{
		this.layerMask = 131072;
		this.pos = this.tr.position;
		if (Physics.Raycast(this.pos, Vector3.up, out this.hit, 90f, this.layerMask))
		{
			Vector3 point = this.hit.point;
			point.y -= 1.2f;
			this.rootTr.position = point;
			this.setup.pmSleep.FsmVariables.GetFsmVector3("sleepPos").Value = this.rootTr.position;
			this.setup.pmSleep.FsmVariables.GetFsmVector3("sleepAngle").Value = this.hit.normal * -1f;
			base.StartCoroutine(this.fixMutantPosition(this.rootTr, point));
		}
	}

	
	private IEnumerator fixMutantPosition(Transform m, Vector3 newPos)
	{
		float t = 0f;
		while (t < 1f)
		{
			if (m)
			{
				m.position = newPos;
				m.SendMessage("updateWorldTransformPosition", SendMessageOptions.DontRequireReceiver);
			}
			t += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	
	public void findCloseTrap()
	{
		GameObject gameObject = null;
		foreach (GameObject gameObject2 in this.sceneInfo.allRabbitTraps)
		{
			float magnitude = (this.tr.position - gameObject2.transform.position).magnitude;
			if (magnitude < 80f && magnitude > 8f && !gameObject2.CompareTag("trapSprung"))
			{
				gameObject = gameObject2;
			}
		}
		if (gameObject != null)
		{
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.FsmVariables.GetFsmGameObject("targetObjectGO").Value = gameObject;
				this.setup.pmCombat.SendEvent("doAction");
			}
			this.updateCurrentWaypoint(gameObject.transform.position);
			this.setToWaypoint();
		}
		else if (this.setup.pmCombat)
		{
			this.setup.pmCombat.SendEvent("noValidTarget");
		}
	}

	
	public bool findCloseTrapTrigger()
	{
		if (Time.time < this.trapTriggerTimout)
		{
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.SendEvent("noValidTarget");
			}
			return false;
		}
		GameObject gameObject = null;
		if (Scene.SceneTracker.allTrapTriggers.Count == 0)
		{
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.SendEvent("noValidTarget");
			}
			return false;
		}
		foreach (GameObject gameObject2 in this.sceneInfo.allTrapTriggers)
		{
			float magnitude = (this.tr.position - gameObject2.transform.position).magnitude;
			if (magnitude < 70f && magnitude > 12f)
			{
				gameObject = gameObject2;
			}
		}
		if (!(gameObject != null))
		{
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.SendEvent("noValidTarget");
			}
			return false;
		}
		if (!gameObject.activeSelf)
		{
			return false;
		}
		if (this.setup.pmCombat)
		{
			this.setup.pmCombatScript.structureGo = gameObject;
			this.setup.pmCombat.FsmVariables.GetFsmGameObject("structureGo").Value = gameObject;
			this.setup.pmCombat.SendEvent("doAction");
		}
		this.setup.pmSearchScript._structureGo = gameObject;
		this.trapTriggerTimout = Time.time + 10f;
		this.updateCurrentWaypoint(gameObject.transform.position);
		this.setToWaypoint();
		return true;
	}

	
	public IEnumerator castPointAroundPlayer(float dist)
	{
		float modDist = dist;
		if (Scene.SceneTracker.hasSwarmedByEnemies)
		{
			modDist = 400f;
		}
		if (this.fsmInCave.Value)
		{
			this.randomPoint = this.Circle2(8f);
			this.pos = new Vector3(this.currentTarget.transform.position.x + this.randomPoint.x, this.currentTarget.transform.position.y, this.currentTarget.transform.position.z + this.randomPoint.y);
		}
		else
		{
			this.randomPoint = this.Circle2(UnityEngine.Random.Range(modDist, modDist + 20f));
			this.pos = new Vector3(this.currentTarget.transform.position.x + this.randomPoint.x, this.currentTarget.transform.position.y, this.currentTarget.transform.position.z + this.randomPoint.y);
			this.pos.y = Terrain.activeTerrain.SampleHeight(this.pos) + Terrain.activeTerrain.transform.position.y;
		}
		GraphNode node = null;
		if (this.fsmInCave.Value)
		{
			node = this.nmg.GetNearest(this.pos).node;
		}
		else
		{
			node = this.rg.GetNearest(this.pos).node;
		}
		if (node == null)
		{
			this.search = false;
			yield break;
		}
		if (node.Walkable)
		{
			this.newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
			float height = Mathf.Abs(base.transform.position.y - this.newWaypoint.y);
			if (this.fsmInCave.Value && height > 15f)
			{
				if (this.setup.pmCombat)
				{
					this.setup.pmCombat.SendEvent("noValidTarget");
				}
				yield break;
			}
			this.updateCurrentWaypoint(this.newWaypoint);
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.SendEvent("doAction");
			}
			if (this.setup.pmCombatScript)
			{
				this.setup.pmCombatScript.doAction = true;
			}
			if (this.setup.pmSearchScript)
			{
				this.setup.pmSearchScript._doSearchAction = true;
			}
			this.search = false;
			this.searchCount = 0;
			yield return null;
		}
		else if (this.setup.pmCombat)
		{
			this.setup.pmCombat.SendEvent("noValidTarget");
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator castPointAroundSpawn(float dist)
	{
		if (this.setup.spawnGo == null)
		{
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.SendEvent("noValidTarget");
			}
			yield break;
		}
		if (!this.fsmInCave.Value)
		{
			this.randomPoint = this.Circle2(UnityEngine.Random.Range(dist, dist + 5f));
			this.pos = new Vector3(this.setup.spawnGo.transform.position.x + this.randomPoint.x, this.setup.spawnGo.transform.position.y, this.setup.spawnGo.transform.position.z + this.randomPoint.y);
			this.pos.y = Terrain.activeTerrain.SampleHeight(this.pos) + Terrain.activeTerrain.transform.position.y;
		}
		else
		{
			this.pos = new Vector3(this.setup.spawnGo.transform.position.x, this.setup.spawnGo.transform.position.y, this.setup.spawnGo.transform.position.z);
		}
		GraphNode node = null;
		if (this.fsmInCave.Value)
		{
			node = this.nmg.GetNearest(this.pos).node;
		}
		else
		{
			node = this.rg.GetNearest(this.pos).node;
		}
		bool valid = true;
		if (node == null)
		{
			valid = false;
		}
		if (node.Walkable && valid)
		{
			this.newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
			this.updateCurrentWaypoint(this.newWaypoint);
			if (this.setup.pmSearchScript)
			{
				this.setup.pmSearchScript._doSearchAction = true;
			}
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.SendEvent("doAction");
			}
			if (this.setup.pmCombatScript)
			{
				this.setup.pmCombatScript.doAction = true;
			}
			yield return null;
		}
		else if (this.setup.pmCombat)
		{
			this.setup.pmCombat.SendEvent("noValidTarget");
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator castPointAroundLastSighting(float dist)
	{
		if (this.fsmInCave.Value)
		{
			this.pos = this.lastSighting.transform.position;
		}
		else
		{
			this.randomPoint = this.Circle2(UnityEngine.Random.Range(dist - 5f, dist + 10f));
			this.pos = new Vector3(this.lastSighting.transform.position.x + this.randomPoint.x, this.lastSighting.transform.position.y, this.lastSighting.transform.position.z + this.randomPoint.y);
		}
		GraphNode node = null;
		if (this.fsmInCave.Value)
		{
			node = this.nmg.GetNearest(this.pos).node;
		}
		else
		{
			node = this.rg.GetNearest(this.pos).node;
		}
		if (node.Walkable)
		{
			this.newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
			this.updateCurrentWaypoint(this.newWaypoint);
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.SendEvent("doAction");
			}
			if (this.setup.pmSearchScript)
			{
				this.setup.pmSearchScript._doAction = true;
			}
			this.search = false;
		}
		else
		{
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.SendEvent("noValidTarget");
			}
			if (this.setup.pmSearchScript)
			{
				this.setup.pmSearchScript._doAction = false;
			}
			this.search = false;
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator findRandomPoint(float dist)
	{
		this.layerMask = 67248128;
		bool valid = false;
		if (this.setup.typeSetup.inCave)
		{
			this.randomPoint = this.Circle2((float)UnityEngine.Random.Range(10, 20));
			this.pos = new Vector3(this.tr.position.x + this.randomPoint.x, this.tr.position.y, this.tr.position.z + this.randomPoint.y);
		}
		else
		{
			this.randomPoint = this.Circle2(UnityEngine.Random.Range(dist - 5f, dist + 30f));
			this.pos = new Vector3(this.tr.position.x + this.randomPoint.x, this.tr.position.y, this.tr.position.z + this.randomPoint.y);
			this.pos.y = Terrain.activeTerrain.SampleHeight(this.pos) + Terrain.activeTerrain.transform.position.y;
		}
		GraphNode node = null;
		if (this.setup.typeSetup.inCave)
		{
			node = this.nmg.GetNearest(this.pos).node;
		}
		else
		{
			node = this.rg.GetNearest(this.pos).node;
		}
		valid = (node != null && node.Walkable);
		if (valid)
		{
			this.newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
			this.updateCurrentWaypoint(this.newWaypoint);
			this.setToWaypoint();
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.SendEvent("doAction");
			}
			if (this.setup.pmSearchScript)
			{
				this.setup.pmSearchScript._doSearchAction = true;
			}
		}
		else if (this.setup.pmCombat)
		{
			this.setup.pmCombat.SendEvent("noValidTarget");
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator findCloseWaypoint()
	{
		if (this.sceneInfo.allPlayers.Count == 0)
		{
			yield break;
		}
		if (this.sceneInfo.allPlayers[0] == null)
		{
			yield break;
		}
		if (this.sceneInfo.waypointMarkers.Count > 0)
		{
			this.sceneInfo.waypointMarkers.Sort((GameObject c1, GameObject c2) => Vector3.Distance(this.$this.sceneInfo.allPlayers[0].transform.position, c1.transform.position).CompareTo(Vector3.Distance(this.$this.sceneInfo.allPlayers[0].transform.position, c2.transform.position)));
			this.currentWaypointGo = this.sceneInfo.waypointMarkers[UnityEngine.Random.Range(2, 5)];
			this.updateCurrentWaypoint(this.currentWaypointGo.transform.position);
			if (this.setup.pmSearchScript)
			{
				this.setup.pmSearchScript._doSearchAction = true;
			}
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator findRandomWaypoint()
	{
		if (this.sceneInfo.waypointMarkers.Count > 0)
		{
			this.currentWaypointGo = this.sceneInfo.waypointMarkers[UnityEngine.Random.Range(0, this.sceneInfo.waypointMarkers.Count)];
			this.updateCurrentWaypoint(this.currentWaypointGo.transform.position);
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator findRandomStructure(float dist)
	{
		bool search = true;
		if (this.sceneInfo.recentlyBuilt.Count == 0)
		{
			yield break;
		}
		this.currentWaypointGo = this.sceneInfo.recentlyBuilt[UnityEngine.Random.Range(0, this.sceneInfo.recentlyBuilt.Count)];
		this.count = 0;
		while (search)
		{
			this.count++;
			this.randomPoint = this.Circle2(UnityEngine.Random.Range(dist - 10f, dist + 15f));
			if (this.currentWaypointGo)
			{
				this.pos = new Vector3(this.currentWaypointGo.transform.position.x + this.randomPoint.x, this.currentWaypointGo.transform.position.y, this.currentWaypointGo.transform.position.z + this.randomPoint.y);
			}
			GraphNode node = null;
			if (this.fsmInCave.Value)
			{
				node = this.nmg.GetNearest(this.pos).node;
			}
			else
			{
				node = this.rg.GetNearest(this.pos).node;
			}
			if (node.Walkable && this.count < 10)
			{
				Vector3 newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
				this.updateCurrentWaypoint(newWaypoint);
				search = false;
				yield return null;
			}
			else
			{
				search = false;
			}
			yield return null;
		}
		yield break;
	}

	
	public IEnumerator findDistantStructure(float dist)
	{
		bool search = true;
		if (this.sceneInfo.structuresBuilt.Count < 45)
		{
			yield break;
		}
		int randStruc = UnityEngine.Random.Range(0, this.sceneInfo.structuresBuilt.Count);
		int structureCount = 0;
		float closestDist = float.PositiveInfinity;
		GameObject closestStructure = null;
		while (structureCount < 30)
		{
			float sqrMagnitude = (this.sceneInfo.structuresBuilt[randStruc].transform.position - this.tr.position).sqrMagnitude;
			if (sqrMagnitude < closestDist && dist < 40000f)
			{
				closestDist = sqrMagnitude;
				closestStructure = this.sceneInfo.structuresBuilt[randStruc];
			}
			randStruc = UnityEngine.Random.Range(0, this.sceneInfo.structuresBuilt.Count);
			structureCount++;
		}
		if (closestStructure == null)
		{
			yield break;
		}
		this.count = 0;
		while (search)
		{
			this.randomPoint = this.Circle2(45f);
			if (closestStructure)
			{
				this.pos = new Vector3(closestStructure.transform.position.x + this.randomPoint.x, closestStructure.transform.position.y, closestStructure.transform.position.z + this.randomPoint.y);
			}
			GraphNode node = this.rg.GetNearest(this.pos, NNConstraint.Default).node;
			bool onValidArea = false;
			if (node != null)
			{
				bool flag = false;
				using (List<uint>.Enumerator enumerator = Scene.MutantControler.mostCommonArea.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						int num = (int)enumerator.Current;
						if ((long)num == (long)((ulong)node.Area))
						{
							flag = true;
						}
					}
				}
				onValidArea = flag;
			}
			if (node.Walkable && onValidArea)
			{
				Vector3 vect = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
				this.updateCurrentWaypoint(vect);
				if (this.setup.pmSearchScript)
				{
					this.setup.pmSearchScript._doSearchAction = true;
				}
				search = false;
				yield break;
			}
			if (this.count > 10)
			{
				search = false;
			}
			this.count++;
			yield return null;
		}
		yield break;
	}

	
	public IEnumerator findCloseStructure()
	{
		if (this.setup.ai.allPlayers.Count == 0)
		{
			yield break;
		}
		if (PlayerPreferences.NoDestruction)
		{
			if (this.setup.pmCombatScript)
			{
				this.setup.pmCombatScript.noValidTarget = true;
			}
			this.setup.pmCombat.SendEvent("noValidTarget");
			yield break;
		}
		if (this.setup.ai.allPlayers[0] != null && (this.setup.ai.allPlayers[0].transform.position - this.tr.position).sqrMagnitude > 40000f)
		{
			if (this.setup.pmCombatScript)
			{
				this.setup.pmCombatScript.noValidTarget = true;
			}
			this.setup.pmCombat.SendEvent("noValidTarget");
			yield break;
		}
		float closestDist = float.PositiveInfinity;
		Collider closestColl = null;
		int counter = 0;
		if (Scene.SceneTracker.closeStructureTarget && this.setup.pmCombat.enabled)
		{
			closestColl = Scene.SceneTracker.closeStructureTarget.GetComponent<Collider>();
		}
		else
		{
			this.layerMask = 102760448;
			Collider[] array = Physics.OverlapSphere(this.tr.position, 60f, this.layerMask);
			int num = array.Length;
			if (array.Length > 50)
			{
				num = 50;
			}
			float num2 = 50f;
			for (int i = 0; i < num; i++)
			{
				if (array[i].gameObject.CompareTag("structure") || array[i].gameObject.CompareTag("SLTier1") || array[i].gameObject.CompareTag("SLTier2") || array[i].gameObject.CompareTag("SLTier3"))
				{
					counter++;
					if (this.setup.ai.allPlayers[0] != null)
					{
						num2 = (this.setup.ai.allPlayers[0].transform.position - array[i].transform.position).sqrMagnitude;
					}
					float magnitude = (array[i].transform.position - this.tr.position).magnitude;
					float num3 = Terrain.activeTerrain.SampleHeight(array[i].bounds.center) + Terrain.activeTerrain.transform.position.y;
					float num4 = array[i].bounds.center.y - num3;
					if (magnitude > 16f && magnitude < closestDist && num4 < 6f && num4 > -1f && num2 > 144f && num2 < 40000f)
					{
						closestDist = magnitude;
						closestColl = array[i];
					}
				}
			}
		}
		if (closestColl)
		{
			getStructureStrength component = closestColl.gameObject.GetComponent<getStructureStrength>();
			if (component == null)
			{
				if (this.setup.pmCombatScript)
				{
					this.setup.pmCombatScript.noValidTarget = true;
				}
				this.setup.pmCombat.SendEvent("noValidTarget");
				yield break;
			}
			Vector3 normalized = (this.tr.position - closestColl.bounds.center).normalized;
			Vector3 vector = closestColl.bounds.center + normalized * 7.5f;
			vector.y = Terrain.activeTerrain.SampleHeight(vector) + Terrain.activeTerrain.transform.position.y;
			GraphNode node;
			if (this.fsmInCave.Value)
			{
				node = this.nmg.GetNearest(vector).node;
			}
			else
			{
				node = this.rg.GetNearest(vector).node;
			}
			if (node.Walkable)
			{
				vector = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
				this.updateCurrentWaypoint(vector);
				this.setup.pmCombat.FsmVariables.GetFsmGameObject("structureGo").Value = closestColl.gameObject;
				if (this.setup.pmCombatScript)
				{
					this.setup.pmCombatScript.structureGo = closestColl.gameObject;
				}
				this.setup.pmCombat.SendEvent("goToAction");
				Scene.SceneTracker.closeStructureTarget = closestColl.gameObject;
				if (this.setup.pmCombatScript)
				{
					this.setup.pmCombatScript.doAction = true;
				}
				if (this.setup.pmSearchScript)
				{
					this.setup.pmSearchScript._structureGo = closestColl.gameObject;
					this.setup.pmSearchScript._toStructure = true;
				}
				base.Invoke("resetClosestStructureTarget", 15f);
				yield break;
			}
		}
		this.setup.pmCombat.SendEvent("noValidTarget");
		if (this.setup.pmCombatScript)
		{
			this.setup.pmCombatScript.noValidTarget = true;
		}
		yield return null;
		yield break;
	}

	
	public void setToStructure()
	{
		GameObject value = this.setup.pmCombat.FsmVariables.GetFsmGameObject("structureGo").Value;
		if (value)
		{
			Collider component = value.GetComponent<Collider>();
			Vector3 vector;
			if (component)
			{
				vector = component.bounds.center;
			}
			else
			{
				vector = value.transform.position;
			}
			vector.y = Terrain.activeTerrain.SampleHeight(vector) + Terrain.activeTerrain.transform.position.y;
			this.updateCurrentWaypoint(vector);
		}
	}

	
	public IEnumerator findCloseBurnStructure()
	{
		if (PlayerPreferences.NoDestruction)
		{
			this.setup.pmCombatScript.noValidTarget = true;
			this.setup.pmCombat.SendEvent("noValidTarget");
			yield break;
		}
		if (this.sceneInfo.structuresBuilt.Count > 0)
		{
			this.sceneInfo.structuresBuilt.RemoveAll((GameObject o) => o == null);
		}
		for (int i = 0; i < this.sceneInfo.structuresBuilt.Count; i++)
		{
			GameObject value = this.sceneInfo.structuresBuilt[i];
			int index = UnityEngine.Random.Range(i, this.sceneInfo.structuresBuilt.Count);
			this.sceneInfo.structuresBuilt[i] = this.sceneInfo.structuresBuilt[index];
			this.sceneInfo.structuresBuilt[index] = value;
		}
		float num = float.PositiveInfinity;
		GameObject gameObject = null;
		int num2 = this.sceneInfo.structuresBuilt.Count;
		if (num2 > 60)
		{
			num2 = 60;
		}
		for (int j = 0; j < num2; j++)
		{
			if (this.sceneInfo.structuresBuilt[j] != null)
			{
				float magnitude = (this.sceneInfo.structuresBuilt[j].transform.position - this.tr.position).magnitude;
				if (magnitude < num && magnitude < 80f)
				{
					gameObject = this.sceneInfo.structuresBuilt[j];
					num = magnitude;
				}
			}
		}
		if (gameObject != null)
		{
			FireDamage component = gameObject.GetComponent<FireDamage>();
			if (component && !component.isBurning)
			{
				this.updateCurrentWaypoint(gameObject.transform.position);
				this.setToWaypoint();
				this.setup.pmCombatScript.doAction = true;
				this.setup.pmCombatScript.structureGo = gameObject;
				this.setup.pmCombat.SendEvent("toBurn");
				this.setup.pmCombat.FsmVariables.GetFsmGameObject("structureGo").Value = gameObject;
				yield break;
			}
		}
		this.setup.pmCombatScript.noValidTarget = true;
		this.setup.pmCombat.SendEvent("noValidTarget");
		yield break;
	}

	
	public IEnumerator findCloseFoodStructure()
	{
		if (PlayerPreferences.NoDestruction)
		{
			this.setup.pmCombatScript.noValidTarget = true;
			this.setup.pmSearchScript._noValidTarget = true;
			this.setup.pmCombat.SendEvent("noValidTarget");
			yield break;
		}
		if (this.sceneInfo.structuresBuilt.Count > 0)
		{
			this.sceneInfo.structuresBuilt.RemoveAll((GameObject o) => o == null);
		}
		float num = float.PositiveInfinity;
		GameObject gameObject = null;
		int num2 = this.sceneInfo.structuresBuilt.Count;
		if (num2 > 60)
		{
			num2 = 60;
		}
		for (int i = 0; i < num2; i++)
		{
			if (this.sceneInfo.structuresBuilt[i] != null)
			{
				float magnitude = (this.sceneInfo.structuresBuilt[i].transform.position - this.tr.position).magnitude;
				if (magnitude < num && magnitude < 100f)
				{
					getStructureStrength component = this.sceneInfo.structuresBuilt[i].GetComponent<getStructureStrength>();
					if (component && component._type == getStructureStrength.structureType.foodRack)
					{
						gameObject = this.sceneInfo.structuresBuilt[i];
						num = magnitude;
					}
				}
			}
		}
		if (gameObject != null)
		{
			this.updateCurrentWaypoint(gameObject.transform.position);
			this.setToWaypoint();
			this.setup.pmCombat.SendEvent("goToAction");
			this.setup.pmCombat.FsmVariables.GetFsmGameObject("structureGo").Value = gameObject;
			this.setup.pmSearchScript._structureGo = gameObject;
			this.setup.pmCombatScript.structureGo = gameObject;
			this.setup.pmCombatScript.doAction = true;
			this.setup.pmSearchScript._doSearchAction = true;
			yield break;
		}
		this.setup.pmCombat.SendEvent("noValidTarget");
		this.setup.pmCombatScript.noValidTarget = true;
		this.setup.pmSearchScript._noValidTarget = true;
		yield break;
	}

	
	public IEnumerator findClosePlayerFire()
	{
		if (Scene.SceneTracker.hasSearchedFire || Scene.SceneTracker.hasSwarmedByEnemies)
		{
			this.setup.pmCombat.SendEvent("noValidTarget");
			yield break;
		}
		if (this.ai.allPlayers.Count == 0)
		{
			this.setup.pmCombat.SendEvent("noValidTarget");
			yield break;
		}
		if (Scene.SceneTracker.allPlayerFires.Count > 0)
		{
			Scene.SceneTracker.allPlayerFires.RemoveAll((GameObject o) => o == null);
		}
		if (Scene.SceneTracker.allPlayerFires.Count == 0)
		{
			yield break;
		}
		for (int i = 0; i < Scene.SceneTracker.allPlayerFires.Count; i++)
		{
			GameObject value = Scene.SceneTracker.allPlayerFires[i];
			int index = UnityEngine.Random.Range(i, this.sceneInfo.allPlayerFires.Count);
			Scene.SceneTracker.allPlayerFires[i] = this.sceneInfo.allPlayerFires[index];
			Scene.SceneTracker.allPlayerFires[index] = value;
		}
		float num = 320f;
		if (!Clock.Dark)
		{
			num = 70f;
		}
		GameObject gameObject = null;
		foreach (GameObject gameObject2 in Scene.SceneTracker.allPlayerFires)
		{
			if (gameObject2 != null)
			{
				Fire2 componentInChildren = gameObject2.GetComponentInChildren<Fire2>();
				if (componentInChildren && componentInChildren.CurrentLit)
				{
					float magnitude = (gameObject2.transform.position - this.tr.position).magnitude;
					if (magnitude < num && this.ai.allPlayers[0] && Vector3.Distance(this.ai.allPlayers[0].transform.position, gameObject2.transform.position) < 170f)
					{
						gameObject = gameObject2;
					}
				}
			}
		}
		if (gameObject != null)
		{
			this.randomPoint = this.Circle2(40f);
			this.pos = new Vector3(gameObject.transform.position.x + this.randomPoint.x, gameObject.transform.position.y, gameObject.transform.position.z + this.randomPoint.y);
			GraphNode node;
			if (this.fsmInCave.Value)
			{
				node = this.nmg.GetNearest(this.pos).node;
			}
			else
			{
				node = this.rg.GetNearest(this.pos).node;
			}
			if (node.Walkable)
			{
				Vector3 vect = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
				this.updateCurrentWaypoint(vect);
				this.setToWaypoint();
				if (this.setup.pmSearchScript)
				{
					this.setup.pmSearchScript._doSearchAction = true;
				}
				Scene.SceneTracker.hasSearchedFire = true;
				Scene.SceneTracker.Invoke("resetSearchingFire", (float)UnityEngine.Random.Range(60, 90));
				yield break;
			}
		}
		this.setup.pmCombat.SendEvent("noValidTarget");
		yield break;
	}

	
	public IEnumerator findPointAwayFromPlayer(float dist)
	{
		this.searchCount = 0;
		this.search = true;
		while (this.search)
		{
			float angle = 0f;
			Vector3 testPoint = Vector3.zero;
			Vector3 relativePos;
			relativePos.z = 1f;
			while (angle < 110f && angle > -110f && this.currentTarget)
			{
				if (this.currentTarget)
				{
					this.randomPoint = this.Circle2(UnityEngine.Random.Range(dist, dist + 15f));
					testPoint = new Vector3(this.randomPoint.x + this.tr.position.x, this.tr.position.y, this.randomPoint.y + this.tr.position.z);
					Vector3 worldPosition = new Vector3(this.currentTarget.transform.position.x, this.lookatTr.position.y, this.currentTarget.transform.position.z);
					this.lookatTr.LookAt(worldPosition);
					relativePos = this.lookatTr.InverseTransformPoint(testPoint);
					angle = Mathf.Atan2(relativePos.x, relativePos.z) * 57.29578f;
					if (!this.fsmInCave.Value)
					{
						testPoint.y = Terrain.activeTerrain.SampleHeight(testPoint) + Terrain.activeTerrain.transform.position.y;
					}
				}
				else
				{
					this.setToClosestPlayer();
					angle = 150f;
				}
				yield return null;
			}
			Debug.DrawRay(testPoint, Vector3.up * 20f, Color.green, 1f);
			if (this.searchCount < 5)
			{
				GraphNode node;
				if (this.fsmInCave.Value)
				{
					node = this.nmg.GetNearest(testPoint, NNConstraint.Default).node;
				}
				else
				{
					node = this.rg.GetNearest(testPoint, NNConstraint.Default).node;
				}
				if (node == null)
				{
					this.setup.pmCombat.SendEvent("noValidTarget");
					if (this.setup.pmEncounter)
					{
						this.setup.pmEncounter.SendEvent("noValidTarget");
					}
					this.searchCount = 0;
					this.search = false;
					yield break;
				}
				if (node.Walkable)
				{
					if (this.setup.ai.groundNode == null)
					{
						this.newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
					}
					else if (node.Area != this.setup.ai.groundNode.Area)
					{
						NNConstraint nnconstraint = new NNConstraint();
						nnconstraint.constrainArea = true;
						int area = (int)this.setup.ai.groundNode.Area;
						nnconstraint.area = area;
						GraphNode node2;
						if (this.fsmInCave.Value)
						{
							node2 = this.nmg.GetNearest(testPoint, nnconstraint).node;
						}
						else
						{
							node2 = this.rg.GetNearest(testPoint, nnconstraint).node;
						}
						if (node2 != null)
						{
							this.newWaypoint = new Vector3((float)(node2.position[0] / 1000), (float)(node2.position[1] / 1000), (float)(node2.position[2] / 1000));
						}
						else
						{
							this.newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
						}
					}
					else
					{
						this.newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
					}
					this.updateCurrentWaypoint(this.newWaypoint);
					this.setup.pmCombat.SendEvent("doAction");
					if (this.setup.pmEncounter)
					{
						this.setup.pmEncounter.SendEvent("doAction");
					}
					this.setToWaypoint();
					if (this.setup.pmCombatScript)
					{
						this.setup.pmCombatScript.doAction = true;
					}
					this.search = false;
					this.searchCount = 0;
				}
				else
				{
					this.searchCount++;
				}
			}
			else
			{
				this.setup.pmCombat.SendEvent("noValidTarget");
				if (this.setup.pmEncounter)
				{
					this.setup.pmEncounter.SendEvent("noValidTarget");
				}
				this.searchCount = 0;
				this.search = false;
			}
			yield return null;
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator findPointInFrontOfPlayer(float dist)
	{
		float angle = 180f;
		Vector3 testPoint = Vector3.zero;
		int loopCount = 0;
		float newDist = dist;
		if (!Clock.Dark)
		{
			newDist = 500f;
		}
		if (Scene.SceneTracker.hasAttackedPlayer && !Clock.Dark)
		{
			newDist = 600f;
		}
		if (Clock.Dark)
		{
			newDist = 380f;
		}
		if (Scene.SceneTracker.hasSwarmedByEnemies)
		{
			newDist = 600f;
		}
		newDist *= GameSettings.Ai.playerSearchRadiusRadio;
		while (angle > 90f && loopCount < 20)
		{
			if (!LocalPlayer.Transform || this.ai.allPlayers.Count == 0)
			{
				yield break;
			}
			Vector3 dir = Quaternion.Euler(new Vector3(0f, UnityEngine.Random.Range(0f, 359f), 0f)) * Vector3.forward * (newDist + (float)UnityEngine.Random.Range(0, 25));
			Vector3 lookAtDir = this.tr.position;
			Vector3 target = LocalPlayer.Transform.position;
			if (BoltNetwork.isRunning)
			{
				GameObject gameObject = this.ai.allPlayers[UnityEngine.Random.Range(0, this.ai.allPlayers.Count)];
				if (gameObject)
				{
					target = gameObject.transform.position;
				}
			}
			lookAtDir.y = target.y;
			lookAtDir -= target;
			angle = Vector3.Angle(lookAtDir, dir);
			testPoint = dir + target;
			loopCount++;
			yield return null;
		}
		if (loopCount >= 20)
		{
			this.setup.pmCombat.SendEvent("noValidTarget");
			if (this.setup.pmEncounter)
			{
				this.setup.pmEncounter.SendEvent("noValidTarget");
			}
			yield break;
		}
		testPoint.y = Terrain.activeTerrain.SampleHeight(testPoint) + Terrain.activeTerrain.transform.position.y;
		GraphNode node = null;
		if (this.fsmInCave.Value)
		{
			node = this.nmg.GetNearest(testPoint).node;
		}
		else
		{
			node = this.rg.GetNearest(testPoint).node;
		}
		if (node == null)
		{
			yield break;
		}
		if (node.Walkable)
		{
			this.updateCurrentWaypoint(new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000)));
			if (this.setup.pmSearchScript)
			{
				this.setup.pmSearchScript._doSearchAction = true;
			}
		}
		else if (this.setup.pmEncounter)
		{
			this.setup.pmEncounter.SendEvent("noValidTarget");
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator findPointAwayFromExplosion(float dist)
	{
		if (this.collideDetect.currentTrigger)
		{
			this.tempTarget = this.collideDetect.currentTrigger.transform;
		}
		else if (this.setup.pmBrain.FsmVariables.GetFsmGameObject("fearTargetGo").Value)
		{
			this.tempTarget = this.setup.pmBrain.FsmVariables.GetFsmGameObject("fearTargetGo").Value.transform;
		}
		else
		{
			this.tempTarget = LocalPlayer.Transform;
		}
		this.search = true;
		while (this.search)
		{
			float angle = 0f;
			Vector3 testPoint = Vector3.zero;
			Vector3 relativePos;
			relativePos.z = 1f;
			while (angle < 100f && angle > -100f && this.tempTarget)
			{
				Vector2 randomPoint = this.Circle2(UnityEngine.Random.Range(dist, dist + 15f));
				testPoint = new Vector3(randomPoint.x + this.tr.position.x, this.tr.position.y, randomPoint.y + this.tr.position.z);
				Vector3 lookAtPos = new Vector3(this.tempTarget.position.x, this.lookatTr.position.y, this.tempTarget.position.z);
				this.lookatTr.LookAt(lookAtPos);
				relativePos = this.lookatTr.InverseTransformPoint(testPoint);
				angle = Mathf.Atan2(relativePos.x, relativePos.z) * 57.29578f;
				if (!this.fsmInCave.Value)
				{
					testPoint.y = Terrain.activeTerrain.SampleHeight(testPoint) + Terrain.activeTerrain.transform.position.y;
				}
				yield return null;
			}
			GraphNode node = null;
			if (this.fsmInCave.Value)
			{
				node = this.nmg.GetNearest(testPoint).node;
			}
			else
			{
				node = this.rg.GetNearest(testPoint).node;
			}
			if (node.Walkable)
			{
				this.newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
				this.updateCurrentWaypoint(this.newWaypoint);
				this.setup.pmCombat.SendEvent("doAction");
				if (this.setup.pmCombatScript)
				{
					this.setup.pmCombatScript.doAction = true;
				}
				if (this.setup.pmEncounter)
				{
					this.setup.pmEncounter.SendEvent("doAction");
				}
				this.search = false;
			}
			yield return null;
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator findPointAwayFromArtifact()
	{
		this.setup.pmCombat.FsmVariables.GetFsmBool("toAttractArtifact").Value = false;
		this.setup.pmCombat.FsmVariables.GetFsmBool("toRepelArtifact").Value = false;
		this.search = true;
		while (this.search)
		{
			float angle = 0f;
			Vector3 testPoint = Vector3.zero;
			Vector3 relativePos;
			relativePos.z = 1f;
			while (angle < 100f && angle > -100f)
			{
				Vector2 randomPoint = this.Circle2((float)UnityEngine.Random.Range(250, 500));
				testPoint = new Vector3(randomPoint.x + this.tr.position.x, this.tr.position.y, randomPoint.y + this.tr.position.z);
				Vector3 lookAtPos = new Vector3(this._lastArtifactPos.x, this.lookatTr.position.y, this._lastArtifactPos.z);
				this.lookatTr.LookAt(lookAtPos);
				relativePos = this.lookatTr.InverseTransformPoint(testPoint);
				angle = Mathf.Atan2(relativePos.x, relativePos.z) * 57.29578f;
				if (!this.fsmInCave.Value)
				{
					testPoint.y = Terrain.activeTerrain.SampleHeight(testPoint) + Terrain.activeTerrain.transform.position.y;
				}
				yield return null;
			}
			GraphNode node = null;
			if (this.fsmInCave.Value)
			{
				node = this.nmg.GetNearest(testPoint).node;
			}
			else
			{
				node = this.rg.GetNearest(testPoint).node;
			}
			if (node.Walkable)
			{
				this.newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
				this.updateCurrentWaypoint(this.newWaypoint);
				this.setup.pmCombat.SendEvent("doAction");
				if (this.setup.pmCombatScript)
				{
					this.setup.pmCombatScript.doAction = true;
				}
				if (this.setup.pmEncounter)
				{
					this.setup.pmEncounter.SendEvent("doAction");
				}
				this.search = false;
			}
			yield return null;
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator findAmbushPoint(float dist)
	{
		bool foundBush = false;
		if (this.ai.allPlayers.Count == 0)
		{
			yield break;
		}
		Vector3 getPlayerPos = this.ai.allPlayers[0].transform.position;
		for (int i = 0; i < PoolManager.Pools["Bushes"].Count; i++)
		{
			float distToBush = Vector3.Distance(this.tr.position, PoolManager.Pools["Bushes"][i].transform.position);
			float distBushToPlayer = Vector3.Distance(getPlayerPos, PoolManager.Pools["Bushes"][i].transform.position);
			if (distToBush > 20f && distToBush < 50f && distBushToPlayer > 18f)
			{
				foundBush = true;
				this.newWaypoint = PoolManager.Pools["Bushes"][i].transform.position;
				this.updateCurrentWaypoint(this.newWaypoint);
				this.setup.pmCombat.SendEvent("doHide");
				yield break;
			}
			yield return null;
		}
		if (!foundBush)
		{
		}
		yield break;
	}

	
	public IEnumerator findFlankPointToPlayer(float dist)
	{
		this.search = true;
		int count = 0;
		this.layerMask = 67248128;
		while (this.search && this.currentTarget)
		{
			float testDist = 0f;
			Vector3 testPoint = Vector3.zero;
			Vector3 relativePos;
			relativePos.z = 1f;
			float minFlankDist = this.currentTargetDist / 3f;
			while (testDist < minFlankDist || (testDist > this.currentTargetDist && this.currentTarget && count < 20))
			{
				Vector2 vector = this.Circle2(this.currentTargetDist);
				testPoint = new Vector3(vector.x + this.currentTarget.transform.position.x, this.tr.position.y, vector.y + this.currentTarget.transform.position.z);
				testDist = Vector3.Distance(this.tr.position, testPoint);
				count++;
			}
			GraphNode node = null;
			if (this.fsmInCave.Value)
			{
				node = this.nmg.GetNearest(testPoint).node;
			}
			else
			{
				node = this.rg.GetNearest(testPoint).node;
			}
			if (node.Walkable)
			{
				this.newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
				this.updateCurrentWaypoint(this.newWaypoint);
				this.setup.pmCombat.SendEvent("doAction");
				if (this.setup.pmCombatScript)
				{
					this.setup.pmCombatScript.doAction = true;
				}
				this.search = false;
			}
			yield return null;
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator findPerpToPlayer(int side)
	{
		this.search = true;
		this.layerMask = 67248128;
		while (this.search)
		{
			Vector3 dir = (this.currentTarget.transform.position - this.tr.position).normalized;
			Vector3 perp = Vector3.Cross(dir, this.currentTarget.transform.up);
			float dist = UnityEngine.Random.Range(20f, 30f);
			if (side == -1)
			{
				perp *= -dist;
			}
			else if (side == 1)
			{
				perp *= dist;
			}
			else if (UnityEngine.Random.Range(0, 2) == 0)
			{
				perp *= dist;
			}
			else
			{
				perp *= -dist;
			}
			Vector3 testPoint = this.currentTarget.transform.position + perp;
			GraphNode node = null;
			if (this.fsmInCave.Value)
			{
				node = this.nmg.GetNearest(testPoint).node;
			}
			else
			{
				node = this.rg.GetNearest(testPoint).node;
			}
			if (node.Walkable)
			{
				this.newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
				this.updateCurrentWaypoint(this.newWaypoint);
				this.setup.pmCombat.SendEvent("doAction");
				this.search = false;
			}
			else
			{
				this.setup.pmCombat.SendEvent("noValidTarget");
			}
			yield return null;
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator findPerpToEnemy(int side)
	{
		bool search = true;
		while (search)
		{
			Vector3 dir = (this.tr.position - this.currentTarget.transform.position).normalized;
			Vector3 perp = Vector3.Cross(dir, this.tr.up);
			float dist = UnityEngine.Random.Range(20f, 30f);
			if (side == -1)
			{
				perp *= dist;
			}
			else if (side == 1)
			{
				perp *= -dist;
			}
			else if (UnityEngine.Random.Range(0, 2) == 0)
			{
				perp *= dist;
			}
			else
			{
				perp *= -dist;
			}
			Vector3 testPoint = this.tr.position + perp;
			GraphNode node = null;
			if (this.fsmInCave.Value)
			{
				node = this.nmg.GetNearest(testPoint).node;
			}
			else
			{
				node = this.rg.GetNearest(testPoint).node;
			}
			if (node.Walkable)
			{
				this.newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
				this.updateCurrentWaypoint(this.newWaypoint);
				this.setup.pmCombat.SendEvent("doAction");
				search = false;
			}
			else
			{
				this.setup.pmCombat.SendEvent("noValidTarget");
			}
			yield return null;
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator findCloseBush(float dist)
	{
		int tempMask = 4096;
		Collider[] allBushes = Physics.OverlapSphere(this.tr.position, dist, tempMask);
		if (allBushes.Length > 0)
		{
			float angle = 0f;
			float closestDist = float.PositiveInfinity;
			GameObject tempBush = null;
			foreach (Collider collider in allBushes)
			{
				Vector3 relativePos = this.tr.InverseTransformPoint(collider.transform.position);
				angle = Mathf.Atan2(relativePos.x, relativePos.z) * 57.29578f;
				if (dist < closestDist && angle < 90f && angle > -90f)
				{
					tempBush = collider.gameObject;
					closestDist = dist;
				}
			}
			if (tempBush != null)
			{
				Vector3 pos = tempBush.GetComponent<Collider>().bounds.center;
				pos = new Vector3(pos.x, this.tr.position.y, pos.z);
				this.updateCurrentWaypoint(pos);
				yield return YieldPresets.WaitForFixedUpdate;
				if (this.setup.pmSearchScript)
				{
					this.setup.pmSearchScript._doAction = true;
				}
			}
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator findCloseTree(float dist)
	{
		this.layerMask = 2048;
		Collider[] allTrees = Physics.OverlapSphere(this.tr.position, dist, this.layerMask);
		this.setup.pmCombat.FsmVariables.GetFsmGameObject("treeGO").Value = null;
		for (int i = 0; i < allTrees.Length; i++)
		{
			Collider collider = allTrees[i];
			int num = UnityEngine.Random.Range(i, allTrees.Length);
			allTrees[i] = allTrees[num];
			allTrees[num] = collider;
		}
		float closestDist = 0f;
		for (int j = 0; j < allTrees.Length; j++)
		{
			closestDist = (allTrees[j].transform.position - this.tr.position).magnitude;
			if (closestDist > 25f && closestDist < 45f && allTrees[j].transform.GetComponent<climbable>())
			{
				this.updateCurrentWaypoint(allTrees[j].bounds.center);
				this.currentWaypoint.transform.position = new Vector3(this.currentWaypoint.transform.position.x, this.tr.position.y, this.currentWaypoint.transform.position.z);
				this.setup.pmCombat.FsmVariables.GetFsmGameObject("treeGO").Value = allTrees[j].gameObject;
				this.setup.pmCombat.FsmVariables.GetFsmVector3("treePos").Value = allTrees[j].bounds.center;
				this.nearestTree = this.currentWaypoint;
				yield break;
			}
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator creepyFindCloseTree(float dist)
	{
		this.layerMask = 33556480;
		Collider[] allTrees = Physics.OverlapSphere(this.tr.position, dist, this.layerMask);
		float closestDist = 0f;
		foreach (Collider collider in allTrees)
		{
			if (collider.gameObject.CompareTag("Tree"))
			{
				closestDist = (collider.transform.position - this.tr.position).magnitude;
				if (closestDist > 9f && closestDist < 30f && (collider.transform.GetComponent<climbable>() || collider.gameObject.CompareTag("structure")))
				{
					this.updateCurrentWaypoint(collider.bounds.center);
					this.currentWaypoint.transform.position = new Vector3(this.currentWaypoint.transform.position.x, this.tr.position.y, this.currentWaypoint.transform.position.z);
					this.setup.pmCombat.FsmVariables.GetFsmGameObject("treeGo").Value = collider.gameObject;
					this.setup.pmCombat.FsmVariables.GetFsmVector3("treePos").Value = collider.bounds.center;
					this.nearestTree = this.currentWaypoint;
					this.setup.pmCombat.SendEvent("goToTree");
					yield break;
				}
			}
		}
		this.setup.pmCombat.SendEvent("noValidTarget");
		yield return null;
		yield break;
	}

	
	public IEnumerator findJumpTree(bool towards)
	{
		this.layerMask = 2048;
		Collider[] allTrees = Physics.OverlapSphere(this.tr.position, 50f, this.layerMask);
		for (int i = 0; i < allTrees.Length; i++)
		{
			Collider collider = allTrees[i];
			int num = UnityEngine.Random.Range(i, allTrees.Length);
			allTrees[i] = allTrees[num];
			allTrees[num] = collider;
		}
		float closestDist = 0f;
		foreach (Collider collider2 in allTrees)
		{
			Vector3 vector = new Vector3(collider2.bounds.center.x, this.tr.position.y, collider2.bounds.center.z);
			closestDist = (vector - this.tr.position).magnitude;
			if (closestDist > 20f && closestDist < 32f && collider2.transform.GetComponent<climbable>())
			{
				float magnitude = (this.currentTarget.transform.position - collider2.bounds.center).magnitude;
				float num2 = this.currentTargetDist;
				if (magnitude < num2 && towards)
				{
					this.updateCurrentWaypoint(vector);
					this.setup.pmCombat.FsmVariables.GetFsmGameObject("nextTreeGO").Value = this.currentWaypoint;
					this.setup.pmCombat.FsmVariables.GetFsmVector3("nextTreePos").Value = vector;
					this.nearestTree = this.currentWaypoint;
				}
				else if (!towards)
				{
					this.updateCurrentWaypoint(vector);
					this.setup.pmCombat.FsmVariables.GetFsmGameObject("nextTreeGO").Value = this.currentWaypoint;
					this.setup.pmCombat.FsmVariables.GetFsmVector3("nextTreePos").Value = vector;
					this.nearestTree = this.currentWaypoint;
				}
			}
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator findJumpAttackObj()
	{
		bool searching = true;
		this.tempList = new List<Transform>(this.sceneInfo.jumpObjects);
		foreach (Transform objTr in this.tempList)
		{
			if (searching)
			{
				Vector3 tempObjPos = objTr.position;
				tempObjPos.y = this.tr.position.y;
				float dist = Vector3.Distance(this.tr.position, tempObjPos);
				if (dist < 50f && dist > 21f)
				{
					this.currentTarget.transform.position.y = this.tr.position.y;
					this.lookatTr.LookAt(this.currentTarget.transform.position);
					yield return YieldPresets.WaitForFixedUpdate;
					Vector3 localTarget = this.lookatTr.InverseTransformPoint(tempObjPos);
					float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * 57.29578f;
					if (targetAngle < 15f && targetAngle > -15f)
					{
						float playerDist = Vector3.Distance(this.currentTarget.transform.position, tempObjPos);
						float targetDist = Vector3.Distance(this.currentTarget.transform.position, this.tr.position);
						if (playerDist < 30f && playerDist > 7f && dist < targetDist)
						{
							jumpObject jo = objTr.GetComponent<jumpObject>();
							if (!jo.occupied)
							{
								jo.occupied = true;
								this.setup.pmCombat.SendEvent("toRockJumpAttack");
								this.currentWaypoint.transform.position = tempObjPos;
								this.ai.SearchPath();
								searching = false;
								yield return YieldPresets.WaitFourSeconds;
								if (jo)
								{
									jo.occupied = false;
								}
								yield return null;
							}
						}
					}
				}
			}
			yield return null;
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator findCloseJumpObj()
	{
		bool searching = true;
		if (this.sceneInfo.jumpObjects.Count == 0)
		{
			yield break;
		}
		for (int j = 0; j < this.sceneInfo.jumpObjects.Count; j++)
		{
			Transform value = this.sceneInfo.jumpObjects[j];
			int index = UnityEngine.Random.Range(j, this.sceneInfo.jumpObjects.Count);
			this.sceneInfo.jumpObjects[j] = this.sceneInfo.jumpObjects[index];
			this.sceneInfo.jumpObjects[index] = value;
		}
		for (int i = this.sceneInfo.jumpObjects.Count - 1; i >= 0; i = Mathf.Min(i - 1, this.sceneInfo.jumpObjects.Count - 1))
		{
			Transform objTr = this.sceneInfo.jumpObjects[i];
			if (searching && objTr != null)
			{
				Vector3 tempPos = objTr.position;
				tempPos.y = this.tr.position.y;
				float dist = Vector3.Distance(this.tr.position, tempPos);
				if (dist < 50f && dist > 21f)
				{
					float playerDist = Vector3.Distance(this.currentTarget.transform.position, tempPos);
					if (playerDist > 15f && playerDist < 70f)
					{
						this.currentJumpObj = objTr.GetComponent<jumpObject>();
						if (this.currentJumpObj && !this.currentJumpObj.occupied && this.currentJumpObj.jumpPos)
						{
							this.currentJumpObj.occupied = true;
							this.setup.pmCombat.FsmVariables.GetFsmVector3("closeRockPOS").Value = this.currentJumpObj.jumpPos.position;
							if (this.setup.pmCombatScript)
							{
								this.setup.pmCombatScript.toOnRock = true;
							}
							this.currentWaypoint.transform.position = tempPos;
							this.ai.SearchPath();
							searching = false;
							yield return null;
						}
					}
				}
			}
		}
		yield return null;
		yield break;
	}

	
	public void findPlaneCrash()
	{
		if (this.sceneInfo.planeCrash != null)
		{
			this.ai.target = this.sceneInfo.planeCrash.transform;
			this.ai.SearchPath();
		}
	}

	
	public void resetCurrentJumpObj()
	{
		if (this.currentJumpObj)
		{
			this.currentJumpObj.occupied = false;
		}
	}

	
	private void checkNullTarget()
	{
		if (this.setup.ai.allPlayers.Count == 0)
		{
			return;
		}
		if (this.currentTarget == null && this.sceneInfo.allPlayers[0])
		{
			this.currentTarget = this.sceneInfo.allPlayers[0];
		}
	}

	
	public void updateCurrentTarget()
	{
		if (this.setup.ai.creepy || this.setup.ai.creepy_fat || this.setup.ai.creepy_male || this.setup.ai.creepy_baby)
		{
			return;
		}
		this.newTarget = this.hitGo;
		this.ai.target = this.newTarget.transform;
		if (this.currentTarget != null)
		{
			this.lastTarget = this.currentTarget;
		}
		this.currentTarget = this.hitGo;
		this.fsmCurrentTargetGo.Value = this.hitGo;
		this.targetSetup = this.currentTarget.GetComponent<targetStats>();
		if (this.currentTarget.CompareTag("enemyRoot"))
		{
			this.setup.aiManager.setAggressiveCombat();
			this.setup.aiManager.Invoke("setDefaultCombat", 30f);
			this.setup.pmBrain.SendEvent("toSetAggressive");
		}
	}

	
	public void switchToNewTarget(GameObject go)
	{
		if (!go)
		{
			return;
		}
		if (this.currentTarget != go && !this.setup.ai.creepy_baby)
		{
			this.hitGo = go;
			this.fsmCurrentTargetGo.Value = go;
			this.ai.target = go.transform;
			if (this.currentTarget != null)
			{
				this.lastTarget = this.currentTarget;
			}
			this.currentTarget = go;
			this.targetSetup = go.GetComponent<targetStats>();
			base.StartCoroutine("toTrack");
		}
		if (this.ai.target != go.transform)
		{
			this.ai.target = go.transform;
		}
	}

	
	public void updateCurrentWaypoint(Vector3 vect)
	{
		this.currentWaypoint.transform.position = vect;
		this.ai.target = this.currentWaypoint.transform;
		this.ai.SearchPath();
	}

	
	private Vector2 Circle2(float radius)
	{
		Vector2 normalized = UnityEngine.Random.insideUnitCircle.normalized;
		return normalized * radius;
	}

	
	private void setToHome()
	{
		this.currentWaypoint.transform.position = this.setup.homeGo.transform.position;
		this.ai.target = this.currentWaypoint.transform;
		this.ai.SearchPath();
	}

	
	public void setToWaypoint()
	{
		this.ai.target = this.currentWaypoint.transform;
		this.ai.SearchPath();
	}

	
	private void setWaypointToTarget()
	{
		this.currentWaypoint.transform.position = this.ai.target.position;
	}

	
	public void setToLastSighting()
	{
		GraphNode node;
		if (this.fsmInCave.Value)
		{
			node = this.nmg.GetNearest(this.lastSighting.transform.position).node;
		}
		else
		{
			node = this.rg.GetNearest(this.lastSighting.transform.position).node;
		}
		if (node != null)
		{
			this.currentWaypoint.transform.position = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
		}
		else
		{
			this.currentWaypoint.transform.position = this.lastSighting.transform.position;
		}
		this.ai.target = this.currentWaypoint.transform;
		this.ai.SearchPath();
	}

	
	private void setToTree()
	{
		this.ai.target = this.nearestTree.transform;
		this.ai.SearchPath();
	}

	
	public void setToWallRunUp()
	{
		this.currentWallGo = this.setup.pmCombat.FsmVariables.GetFsmGameObject("wallGo").Value;
		if (this.currentWallGo)
		{
			Vector3 vector = this.currentWallGo.transform.position;
			climbableWallSetup component = this.currentWallGo.GetComponent<climbableWallSetup>();
			if (component)
			{
				vector = component.runToPosition;
			}
			vector.y = this.currentWallGo.transform.position.y;
			vector = (vector - this.currentWallGo.transform.position) * 6f;
			vector = this.currentWallGo.transform.position + vector;
			float y = Terrain.activeTerrain.SampleHeight(vector) + Terrain.activeTerrain.transform.position.y;
			vector.y = y;
			this.currentWaypoint.transform.position = vector;
			this.ai.target = this.currentWaypoint.transform;
			this.ai.SearchPath();
		}
		else
		{
			this.setup.pmCombat.SendEvent("noValidTarget");
		}
	}

	
	public void setToWall()
	{
		this.currentWallGo = this.setup.pmCombat.FsmVariables.GetFsmGameObject("wallGo").Value;
		if (this.currentWallGo)
		{
			Vector3 vector = this.currentWallGo.transform.position;
			climbableWallSetup component = this.currentWallGo.GetComponent<climbableWallSetup>();
			if (component)
			{
				vector = component.runToPosition;
			}
			if (!component)
			{
				float y = Terrain.activeTerrain.SampleHeight(vector) + Terrain.activeTerrain.transform.position.y;
				vector.y = y;
			}
			this.currentWaypoint.transform.position = vector;
			this.ai.target = this.currentWaypoint.transform;
			this.ai.SearchPath();
		}
		else
		{
			this.setup.pmCombat.SendEvent("noValidTarget");
		}
	}

	
	public bool checkWallHeight()
	{
		Vector3 vector = this.currentWallGo.transform.position;
		climbableWallSetup component = this.currentWallGo.GetComponent<climbableWallSetup>();
		if (component)
		{
			vector = component.runToPosition;
		}
		if (!component)
		{
			float y = Terrain.activeTerrain.SampleHeight(vector) + Terrain.activeTerrain.transform.position.y;
			vector.y = y;
		}
		Vector3 vector2 = vector;
		Vector3 position = this.currentWallGo.transform.position;
		vector2.y += 5.5f;
		position.y = vector2.y;
		RaycastHit raycastHit;
		if (Physics.Raycast(vector2, (position - vector2).normalized, out raycastHit, 10f, this.ai.wallLayerMask))
		{
			this.setup.pmCombatScript.doAction = true;
			this.setup.pmCombat.SendEvent("eventA");
			return true;
		}
		this.setup.pmCombat.SendEvent("eventB");
		return false;
	}

	
	public void resetWallOccupied()
	{
		GameObject value = this.setup.pmCombat.FsmVariables.GetFsmGameObject("wallGo").Value;
		if (value)
		{
			climbableWallSetup component = value.GetComponent<climbableWallSetup>();
			if (component)
			{
				component.occupied = false;
			}
			this.setup.pmCombat.FsmVariables.GetFsmGameObject("wallGo").Value = null;
		}
	}

	
	public void resetAndDisableWallOccupied()
	{
		GameObject value = this.setup.pmCombat.FsmVariables.GetFsmGameObject("wallGo").Value;
		if (value)
		{
			climbableWallSetup component = value.GetComponent<climbableWallSetup>();
			if (component)
			{
				component.occupied = false;
				component.invalid = true;
			}
			this.setup.pmCombat.FsmVariables.GetFsmGameObject("wallGo").Value = null;
		}
	}

	
	public void setToPlayer()
	{
		if (this.setup.ai.allPlayers.Count == 0)
		{
			return;
		}
		this.followingLeader = false;
		this.ai.targetOffset = Vector3.zero;
		if (this.currentTarget)
		{
			this.ai.target = this.currentTarget.transform;
		}
		else if (this.lastTarget)
		{
			this.ai.target = this.lastTarget.transform;
		}
		else if (this.ai.allPlayers.Count == 0)
		{
			if (LocalPlayer.Transform)
			{
				this.ai.target = LocalPlayer.Transform;
			}
		}
		else
		{
			this.ai.target = this.ai.allPlayers[0].transform;
		}
		this.ai.SearchPath();
	}

	
	public void setToClosestPlayer()
	{
		if (this.setup.ai.allPlayers.Count == 0)
		{
			return;
		}
		this.followingLeader = false;
		this.ai.targetOffset = Vector3.zero;
		if (this.ai.allPlayers[0])
		{
			this.currentWaypoint.transform.position = this.ai.allPlayers[0].transform.position;
			this.ai.target = this.ai.allPlayers[0].transform;
			this.ai.SearchPath();
		}
	}

	
	public void setToClosestArtifact()
	{
		this.followingLeader = false;
		this.ai.targetOffset = Vector3.zero;
		this.currentWaypoint.transform.position = this._lastArtifactPos;
		this.ai.target = this.currentWaypoint.transform;
		this.ai.SearchPath();
		this.setup.pmCombat.FsmVariables.GetFsmBool("toAttractArtifact").Value = false;
		this.setup.pmCombat.FsmVariables.GetFsmBool("toRepelArtifact").Value = false;
	}

	
	public void refreshCurrentTarget()
	{
		if (this.currentTarget == null)
		{
			this.ai.target = this.currentWaypoint.transform;
		}
	}

	
	private void setToCave()
	{
		this.ai.target = this.fsmCaveEntrance.Value.transform;
		this.ai.SearchPath();
	}

	
	private void setToHomeOffset()
	{
		if (this.setup.spawnGo)
		{
			this.currentWaypoint.transform.position = this.setup.spawnGo.transform.position;
		}
		if (base.gameObject.activeSelf)
		{
			this.ai.target = this.currentWaypoint.transform;
			base.StartCoroutine("getRandomTargetOffset", 1f);
			this.ai.SearchPath();
		}
	}

	
	public void setToLeader()
	{
		if (this.fsmLeaderGo.Value)
		{
			this.followingLeader = true;
			this.ai.target = this.fsmLeaderGo.Value.transform;
			this.ai.SearchPath();
		}
	}

	
	private void setToMember()
	{
		if (this.familyFunctions.currentMemberTarget)
		{
			this.ai.target = this.familyFunctions.currentMemberTarget.transform;
		}
		else
		{
			this.ai.target = this.currentWaypoint.transform;
		}
		this.ai.SearchPath();
	}

	
	public void setToCurrentMember()
	{
		if (this.familyFunctions.currentMemberTarget)
		{
			this.ai.target = this.familyFunctions.currentMemberTarget.transform;
			this.setup.pmCombat.FsmVariables.GetFsmGameObject("currentMemberGo").Value = this.familyFunctions.currentMemberTarget;
		}
		else
		{
			this.ai.target = this.currentWaypoint.transform;
			this.setup.pmCombat.FsmVariables.GetFsmGameObject("currentMemberGo").Value = null;
		}
		this.ai.SearchPath();
	}

	
	public void setToBody()
	{
		if (this.familyFunctions.currentMemberTarget && this.familyFunctions.currentMemberTarget.activeSelf)
		{
			this.ai.target = this.familyFunctions.currentMemberTarget.transform;
		}
		else if (this.setup.pmCombat)
		{
			this.setup.pmCombat.SendEvent("noValidPath");
		}
	}

	
	public IEnumerator getRandomTargetOffset(float dist)
	{
		Vector2 randPoint = this.Circle2((float)UnityEngine.Random.Range(6, 14));
		this.ai.targetOffset = this.ai.target.InverseTransformPoint(randPoint.x + this.ai.target.position.x, this.ai.target.position.y, randPoint.y + this.ai.target.position.z);
		yield return null;
		yield break;
	}

	
	public void resetTargetOffset()
	{
		this.ai.targetOffset = new Vector3(0f, 0f, 0f);
	}

	
	private IEnumerator findAngleToTarget(Vector3 target)
	{
		Vector3 localTarget = this.tr.InverseTransformPoint(target);
		float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * 57.29578f;
		this.fsmObjectAngle.Value = targetAngle;
		yield return null;
		yield break;
	}

	
	private IEnumerator findAngleFromPlayer()
	{
		Vector3 localTarget = this.player.transform.InverseTransformPoint(this.tr.position);
		float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * 57.29578f;
		this.fsmObjectAngle.Value = targetAngle;
		yield return null;
		yield break;
	}

	
	private IEnumerator findAngleToPlayer()
	{
		Vector3 localTarget = this.tr.InverseTransformPoint(this.player.transform.position);
		float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * 57.29578f;
		this.fsmObjectAngle.Value = targetAngle;
		yield return null;
		yield break;
	}

	
	private IEnumerator setTreeAttatchDist(float dist)
	{
		this.attachDist = dist;
		yield return null;
		yield break;
	}

	
	private IEnumerator setWallAttatchDist(float dist)
	{
		this.attachDist = dist;
		yield return null;
		yield break;
	}

	
	public Vector3 findTreeAttachPos(Vector3 pos, float dist)
	{
		Vector3 b = new Vector3(pos.x, this.tr.position.y, pos.z);
		Vector3 normalized = (this.tr.position - b).normalized;
		return pos + normalized * dist;
	}

	
	public Vector3 findWallAttachPos(GameObject go)
	{
		Vector3 position = this.tr.position;
		position = go.transform.InverseTransformPoint(position);
		Vector3 vector = go.transform.position;
		vector.y = this.tr.position.y;
		if (position.x < 0f)
		{
			vector -= go.transform.right * 2.05f;
		}
		else
		{
			vector += go.transform.right * 2.05f;
		}
		return vector;
	}

	
	public IEnumerator findEatingPos(Vector3 eatPos)
	{
		Vector3 otherPos = new Vector3(eatPos.x, this.tr.position.y, eatPos.z);
		Vector3 newDir = (this.tr.position - otherPos).normalized;
		this.setup.pmCombat.FsmVariables.GetFsmVector3("attachPos").Value = eatPos + newDir * 2.5f;
		if (this.setup.pmCombatScript)
		{
			this.setup.pmCombatScript.attachPos = eatPos + newDir * 2.5f;
		}
		if (this.setup.pmSearchScript)
		{
			this.setup.pmSearchScript._attachPos = eatPos + newDir * 2.5f;
		}
		yield return null;
		yield break;
	}

	
	public void setToGuardPosition(GameObject go)
	{
		Vector3 position = go.transform.position;
		Vector3 position2 = this.player.transform.position;
		float num = Vector2.Distance(position, position2);
		num /= 3f;
		Vector3 vector = (position - position2).normalized;
		vector = position + vector * num;
		this.currentWaypoint.transform.position = vector;
		this.ai.target = this.currentWaypoint.transform;
	}

	
	public void setToRescueRunPos()
	{
		Vector3 position = this.familyFunctions.currentMemberTarget.transform.GetChild(0).TransformPoint(Vector3.forward * -20f);
		this.currentWaypoint.transform.position = position;
		this.ai.target = this.currentWaypoint.transform;
	}

	
	private IEnumerator enableAwareOfPlayer()
	{
		if (!this.playerAware)
		{
			this.playerAware = true;
			if (LocalPlayer.ScriptSetup && !LocalPlayer.ScriptSetup.targetFunctions.visibleEnemies.Contains(base.transform.parent.gameObject))
			{
				LocalPlayer.ScriptSetup.targetFunctions.visibleEnemies.Add(base.transform.parent.gameObject);
			}
			yield return new WaitForSeconds(this.playerAwareReset);
			this.playerAware = false;
			if (LocalPlayer.ScriptSetup && LocalPlayer.ScriptSetup.targetFunctions.visibleEnemies.Contains(base.transform.parent.gameObject))
			{
				LocalPlayer.ScriptSetup.targetFunctions.visibleEnemies.Remove(base.transform.parent.gameObject);
			}
		}
		yield return null;
		yield break;
	}

	
	private void disableAwareOfPlayer()
	{
		if (this.ai.creepy || this.ai.creepy_baby || this.ai.creepy_fat || this.ai.creepy_male)
		{
			return;
		}
		if (LocalPlayer.ScriptSetup && LocalPlayer.ScriptSetup.targetFunctions.visibleEnemies.Contains(base.transform.parent.gameObject))
		{
			LocalPlayer.ScriptSetup.targetFunctions.visibleEnemies.Remove(base.transform.parent.gameObject);
		}
	}

	
	private IEnumerator enableSearchTimeout()
	{
		if (!this.enableSearch)
		{
			this.enableSearch = true;
			this.setup.pmSearchScript._CloseSearchResetBool = false;
			yield return YieldPresets.WaitTwentySeconds;
			this.setup.pmSearchScript._cautiousBool = false;
			yield return YieldPresets.WaitTenSeconds;
			if (this.animator)
			{
				float timer = Time.time + 2.4f;
				this.setup.animControl.StartCoroutine(this.setup.animControl.smoothChangeIdle(3f));
				while (Time.time < timer)
				{
					this.animator.SetFloat("walkTypeFloat1", 0f, 0.5f, Time.deltaTime);
					yield return null;
				}
				this.animator.SetFloat("walkTypeFloat1", 0f);
			}
			this.setup.pmSearchScript._CloseSearchResetBool = true;
			this.enableSearch = false;
		}
		yield break;
	}

	
	public void disableSearchTimeout()
	{
		base.StopCoroutine("enableSearchTimeout");
		this.enableSearch = false;
	}

	
	private void sortRecentlyBuiltStrctures()
	{
		this.sceneInfo.recentlyBuilt.Sort((GameObject c1, GameObject c2) => Vector3.Distance(this.tr.position, c1.transform.position).CompareTo(Vector3.Distance(this.tr.position, c2.transform.position)));
	}

	
	private void getClosestPlayerDist()
	{
		float num = float.PositiveInfinity;
		Transform transform = null;
		foreach (GameObject gameObject in this.setup.sceneInfo.allPlayers)
		{
			if (gameObject != null)
			{
				float num2 = Vector3.Distance(gameObject.transform.position, this.tr.position);
				if (num2 < num)
				{
					transform = gameObject.transform;
					num = num2;
				}
			}
		}
		if (this.setup.pmCombat)
		{
			this.fsmCombatClosestPlayerDist.Value = num;
		}
		if (transform)
		{
			Vector3 vector = base.transform.InverseTransformPoint(transform.position);
			this.fsmClosestPlayerAngle.Value = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
		}
	}

	
	private void getPlaneDistanceToWaypoint()
	{
		if (this.setup.planeCrashGo == null)
		{
			this.setup.planeCrashGo = GameObject.FindWithTag("planeCrash");
		}
	}

	
	public void playSearchScream()
	{
		if (!this.screamCooldown && UnityEngine.Random.value < 0.8f)
		{
			this.animator.SetIntegerReflected("randInt1", UnityEngine.Random.Range(4, 6));
			this.animator.SetBoolReflected("screamBOOL", true);
			this.screamCooldown = true;
			UnityEngine.Object.Instantiate<GameObject>(this.setup.soundGo, base.transform.position, base.transform.rotation);
			base.Invoke("resetScreamCooldown", 10f);
			base.Invoke("ForceDisableScream", 1f);
		}
	}

	
	private void ForceDisableScream()
	{
		this.animator.SetBoolReflected("screamBOOL", false);
	}

	
	private void resetScreamCooldown()
	{
		this.screamCooldown = false;
	}

	
	public bool findCloseCaveWayPoint()
	{
		this.sceneInfo.caveWayPoints.Sort((Transform c1, Transform c2) => (this.tr.position - c1.position).sqrMagnitude.CompareTo((this.tr.position - c2.position).sqrMagnitude));
		if (Vector3.Distance(this.sceneInfo.caveWayPoints[0].position, this.tr.position) < 65f)
		{
			this.currentWaypointGo = this.sceneInfo.caveWayPoints[0].gameObject;
			this.wpSetup = this.currentWaypointGo.GetComponent<wayPointSetup>();
			this.updateCurrentWaypoint(this.currentWaypointGo.transform.position);
			if (this.setup.pmSearchScript)
			{
				this.setup.pmSearchScript._doSearchAction = true;
			}
			return true;
		}
		return false;
	}

	
	public bool getNextWayPoint()
	{
		if (this.wpSetup)
		{
			this.currentWaypointGo = this.wpSetup.nextWaypoint.gameObject;
			this.updateCurrentWaypoint(this.currentWaypointGo.transform.position);
			this.wpSetup = this.currentWaypointGo.GetComponent<wayPointSetup>();
			return true;
		}
		return false;
	}

	
	public void setWayPointParams()
	{
		if (this.wpSetup)
		{
		}
	}

	
	private void findWalkableGround()
	{
		if (!this.setup.pmSleep.FsmVariables.GetFsmBool("inCaveBool").Value)
		{
			this.updateCurrentWaypoint(this.ai.lastWalkablePos);
			this.setToWaypoint();
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.SendEvent("doAction");
			}
		}
		else
		{
			this.updateCurrentWaypoint(this.setup.spawnGo.transform.position);
			this.setToWaypoint();
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.SendEvent("doAction");
			}
		}
	}

	
	public IEnumerator setVisRetry(int set)
	{
		this.visRetry = set;
		yield return null;
		yield break;
	}

	
	public IEnumerator toLook()
	{
		this.lookingForTarget = true;
		if (this.setup.pmBrain)
		{
			this.setup.pmBrain.FsmVariables.GetFsmBool("targetSeenBool").Value = false;
		}
		this.targetCounter = 0;
		base.StopCoroutine("toTrack");
		base.StopCoroutine("toDisableVis");
		if (this.startedLook)
		{
			yield break;
		}
		this.startedLook = true;
		this.startedTrack = false;
		yield return null;
		while (this.lookingForTarget)
		{
			this.disableAwareOfPlayer();
			if (this.checkTorchLightVisTargets() && this.lastTorchGo)
			{
				this.rootTr.SendMessage("onSoundAlert", this.lastTorchGo.GetComponent<Collider>(), SendMessageOptions.DontRequireReceiver);
			}
			if (this.raycastTargets())
			{
				if (this.checkValidTarget())
				{
					if (this.playerAware || this.ai.mainPlayerDist < 30f || this.targetCounter >= 3)
					{
						this.targetCounter = 0;
						this.setNewTarget();
						yield break;
					}
					this.targetCounter++;
					yield return YieldPresets.WaitPointTwoSeconds;
				}
				else
				{
					yield return YieldPresets.WaitPointTwoSeconds;
				}
			}
			else if (this.checkEnemyTargets() && this.checkValidTarget())
			{
				base.Invoke("setNewTarget", 0.5f);
				yield return YieldPresets.WaitPointFiveSeconds;
				yield break;
			}
			yield return YieldPresets.WaitPointTwoFiveSeconds;
		}
		yield break;
	}

	
	public IEnumerator toTrack()
	{
		base.StopCoroutine("toDisableVis");
		base.StopCoroutine("toLook");
		this.trackCounter = 0;
		if (this.startedTrack)
		{
			yield break;
		}
		this.startedTrack = true;
		this.startedLook = false;
		yield return null;
		while (this.checkCurrentTargetStatus())
		{
			if (this.checkForClosePlayers())
			{
				yield return YieldPresets.WaitPointFiveSeconds;
				if (this.checkForClosePlayers())
				{
					this.updateClosePlayerTarget();
					this.storeLastSighting();
					this.trackCounter = 0;
					if (this.setup.pmBrain)
					{
						this.setup.pmBrain.FsmVariables.GetFsmBool("targetSeenBool").Value = true;
					}
					yield return null;
				}
			}
			if (this.hitGo)
			{
				if (this.hitGo.CompareTag("enemyRoot"))
				{
					if (this.checkForPlayers())
					{
						yield return YieldPresets.WaitPointFiveSeconds;
						if (this.checkForPlayers())
						{
							this.updateClosePlayerTarget();
							this.storeLastSighting();
							this.trackCounter = 0;
						}
					}
					else if (Vector3.Distance(this.tr.position, this.hitGo.transform.position) < this.setLongVisRange && this.checkValidTarget())
					{
						this.storeLastSighting();
						this.trackCounter = 0;
					}
					else
					{
						this.targetCounter++;
					}
				}
				else if (this.rayCastActiveTarget())
				{
					if (this.checkValidTarget())
					{
						this.storeLastSighting();
						this.trackCounter = 0;
					}
					else
					{
						this.targetCounter++;
					}
				}
				else
				{
					this.targetCounter++;
				}
				if (this.targetCounter >= this.visRetry)
				{
					this.toLostTarget();
				}
			}
			yield return YieldPresets.WaitPointTwoSeconds;
		}
		this.toLostTarget();
		yield break;
		yield break;
	}

	
	public IEnumerator toDisableVis()
	{
		base.StopCoroutine("toTrack");
		base.StopCoroutine("toLook");
		this.startedLook = false;
		this.startedTrack = false;
		if (this.setup.pmBrain)
		{
			this.setup.pmBrain.FsmVariables.GetFsmBool("targetSeenBool").Value = false;
		}
		Scene.SceneTracker.removeFromVisible(this.rootTr.gameObject);
		yield return null;
		yield break;
	}

	
	private void storeLastSighting()
	{
		Scene.SceneTracker.addToVisible(this.rootTr.gameObject);
		if (this.hitGo.CompareTag("Player") || this.hitGo.CompareTag("PlayerNet"))
		{
			this.playerAware = true;
		}
		if (this.hitGo.CompareTag("Player") && LocalPlayer.ScriptSetup && !LocalPlayer.ScriptSetup.targetFunctions.visibleEnemies.Contains(base.transform.parent.gameObject))
		{
			LocalPlayer.ScriptSetup.targetFunctions.visibleEnemies.Add(base.transform.parent.gameObject);
		}
		if (this.setup.pmBrain)
		{
			this.setup.pmBrain.FsmVariables.GetFsmBool("targetSeenBool").Value = true;
		}
		this.targetCounter = 0;
		this.lastSighting.transform.position = this.hitGo.transform.position;
		this.lastVisibleTarget = this.hitGo;
	}

	
	private void toLostTarget()
	{
		base.StopCoroutine("toTrack");
		base.StopCoroutine("toDisable");
		base.StartCoroutine("toLook");
		base.StopCoroutine("enableAwareOfPlayer");
		this.playerAware = false;
		base.StartCoroutine("enableAwareOfPlayer");
		Scene.SceneTracker.removeFromVisible(this.rootTr.gameObject);
		this.trackCounter = 0;
		if (this.lastHitGo && !this.fsmInCave.Value && this.lastHitGo.GetComponent<getStructureStrength>())
		{
			bool flag = false;
			if (this.lastVisibleTarget)
			{
				GraphNode node = this.rg.GetNearest(this.lastVisibleTarget.transform.position, NNConstraint.Default).node;
				if (node != null && this.ai.groundNode != null && node.Area != this.ai.groundNode.Area)
				{
					flag = true;
				}
			}
			if (this.setup.pmCombat)
			{
				if (flag)
				{
					this.setup.pmCombat.FsmVariables.GetFsmBool("goToStructure").Value = true;
				}
				else
				{
					this.setup.pmCombat.FsmVariables.GetFsmBool("goToStructure").Value = false;
				}
			}
		}
		if (this.setup.animator.GetCurrentAnimatorStateInfo(0).tagHash != this.setup.hashs.deathTag && this.setup.pmCombat)
		{
			this.setup.pmCombat.SendEvent("toTargetLost");
		}
	}

	
	private void setNewTarget()
	{
		this.lookingForTarget = false;
		Scene.SceneTracker.addToVisible(this.rootTr.gameObject);
		this.lastSighting.transform.position = this.hitGo.transform.position;
		if (this.setup.pmBrain)
		{
			this.setup.pmBrain.FsmVariables.GetFsmBool("targetSeenBool").Value = true;
		}
		if (!this.ai.creepy && !this.ai.creepy_baby && !this.ai.creepy_fat && !this.ai.creepy_male)
		{
			this.disableAwareOfPlayer();
			if (this.setup.animator.GetCurrentAnimatorStateInfo(0).tagHash != this.setup.hashs.deathTag)
			{
				this.setup.enemyEvents.playPlayerSighted();
				this.setup.pmCombat.SendEvent("targetFound");
				this.setup.pmEncounter.SendEvent("targetFound");
				this.setup.pmSleep.SendEvent("targetFound");
				this.setup.pmSearchScript.toTargetFoundEvent();
			}
		}
		this.updateCurrentTarget();
		if (base.gameObject.activeSelf)
		{
			base.StopCoroutine("toLook");
			base.StartCoroutine("toTrack");
		}
	}

	
	private bool raycastTargets()
	{
		this.targetFound = false;
		int num = UnityEngine.Random.Range(0, Scene.SceneTracker.allPlayers.Count);
		if (this.setup.sceneInfo.allPlayers.Count > 0)
		{
			for (int i = 0; i < this.setup.sceneInfo.allPlayers.Count; i++)
			{
				if (num == Scene.SceneTracker.allPlayers.Count)
				{
					num -= Scene.SceneTracker.allPlayers.Count;
				}
				GameObject gameObject = Scene.SceneTracker.allPlayers[num];
				if (gameObject && !this.targetFound)
				{
					this.tempTargetDist = (this.tr.position - gameObject.transform.position).sqrMagnitude;
					if (this.tempTargetDist < 10000f)
					{
						Vector3 vector = this.tr.InverseTransformPoint(gameObject.transform.position);
						this.currTargetAngle = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
						if (this.currTargetAngle > -65f && this.currTargetAngle < 65f)
						{
							Vector3 direction = Vector3.zero;
							Collider component = gameObject.GetComponent<Collider>();
							if (this.setup.headJoint)
							{
								if (component)
								{
									Vector3 center = component.bounds.center;
									center.y += 1.2f;
									direction = center - this.setup.headJoint.transform.position;
								}
								else
								{
									direction = gameObject.transform.position - this.setup.headJoint.transform.position;
								}
							}
							else if (component)
							{
								Vector3 center2 = component.bounds.center;
								center2.y += 1.2f;
								direction = center2 - this.setup.animator.rootPosition;
							}
							else
							{
								direction = gameObject.transform.position - this.setup.animator.rootPosition;
							}
							if (this.setup.headJoint)
							{
								this.castPos = this.setup.headJoint.transform.position;
							}
							else
							{
								this.castPos = this.setup.animator.rootPosition;
							}
							if (Physics.Raycast(this.castPos, direction, out this.hit, 100f, this.visLayerMask))
							{
								if (!this.hit.collider.isTrigger)
								{
									this.hitGo = this.hit.transform.gameObject;
									this.targetFound = true;
									return true;
								}
								RaycastHit raycastHit;
								if (Physics.Raycast(this.hit.point, direction, out raycastHit, 100f - this.hit.distance, this.visLayerMask))
								{
									this.hitGo = raycastHit.transform.gameObject;
									this.targetFound = true;
									return true;
								}
							}
						}
					}
				}
				num++;
			}
		}
		this.targetFound = false;
		return false;
	}

	
	private bool rayCastActiveTarget()
	{
		if (!this.hitGo)
		{
			return false;
		}
		this.tempTargetDist = (this.tr.position - this.hitGo.transform.position).sqrMagnitude;
		if (this.tempTargetDist > this.setLongVisRange * this.setLongVisRange)
		{
			return false;
		}
		if (this.setup.headJoint)
		{
			this.castPos = this.setup.headJoint.transform.position;
		}
		else
		{
			this.castPos = this.setup.animator.rootPosition;
		}
		Collider component = this.hitGo.GetComponent<Collider>();
		Vector3 direction;
		if (component)
		{
			direction = this.hitGo.GetComponent<Collider>().bounds.center - this.castPos;
		}
		else
		{
			direction = this.hitGo.transform.position - this.castPos;
		}
		if (!Physics.Raycast(this.castPos, direction, out this.hit, this.setLongVisRange, this.visLayerMask))
		{
			return false;
		}
		if (!this.hit.collider.isTrigger)
		{
			this.hitGo = this.hit.transform.gameObject;
			return true;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(this.hit.point, direction, out raycastHit, this.setLongVisRange - this.hit.distance, this.visLayerMask))
		{
			this.hitGo = raycastHit.transform.gameObject;
			return true;
		}
		return false;
	}

	
	private bool checkTorchLightVisTargets()
	{
		int num = 104204288;
		for (int i = 0; i < Scene.SceneTracker.allVisTargets.Count; i++)
		{
			GameObject gameObject = Scene.SceneTracker.allVisTargets[i];
			if (gameObject == null || !gameObject.activeSelf)
			{
				return false;
			}
			float sqrMagnitude = (this.tr.position - gameObject.transform.position).sqrMagnitude;
			if (sqrMagnitude < 10000f)
			{
				Vector3 vector = this.tr.InverseTransformPoint(gameObject.transform.position);
				this.currTargetAngle = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
				Vector3 direction = Vector3.zero;
				if (this.setup.headJoint)
				{
					direction = gameObject.transform.position - this.setup.headJoint.transform.position;
				}
				if (this.setup.headJoint)
				{
					this.castPos = this.setup.headJoint.transform.position;
				}
				else
				{
					this.castPos = this.setup.animator.rootPosition;
				}
				if (Physics.Raycast(this.castPos, direction, out this.hit, 100f, num) && this.hit.collider.gameObject.CompareTag("torchLight"))
				{
					this.lastTorchGo = this.hit.transform.gameObject;
					if (sqrMagnitude < 625f)
					{
						torchLightSetup component = this.lastTorchGo.GetComponent<torchLightSetup>();
						if (component.distanceToPlayer < 55f)
						{
							this.lastTorchGo.transform.position = component.sourcePos;
						}
					}
					return true;
				}
			}
		}
		return false;
	}

	
	private bool checkEnemyTargets()
	{
		if (this.ai.allPlayers.Count == 0)
		{
			return false;
		}
		if (this.ai.allPlayers[0] == null)
		{
			return false;
		}
		if (this.ai.awayFromPlayer)
		{
			return false;
		}
		for (int i = 0; i < this.sceneInfo.closeEnemies.Count; i++)
		{
			GameObject value = this.sceneInfo.closeEnemies[i];
			int index = UnityEngine.Random.Range(i, this.sceneInfo.closeEnemies.Count);
			this.sceneInfo.closeEnemies[i] = this.sceneInfo.closeEnemies[index];
			this.sceneInfo.closeEnemies[index] = value;
		}
		if (this.setup.sceneInfo.closeEnemies.Count > 0 && !this.targetFound)
		{
			foreach (GameObject gameObject in this.setup.sceneInfo.closeEnemies)
			{
				if (gameObject && !this.targetFound && gameObject.name != base.transform.parent.gameObject.name && gameObject.activeSelf)
				{
					this.tempTargetDist = (this.tr.position - gameObject.transform.position).sqrMagnitude;
					if (this.tempTargetDist < this.setCloseVisRange * this.setCloseVisRange)
					{
						Vector3 vector = this.tr.InverseTransformPoint(gameObject.transform.position);
						this.currTargetAngle = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
						if (this.currTargetAngle > -90f && this.currTargetAngle < 90f)
						{
							this.hitGo = gameObject;
							return true;
						}
					}
				}
			}
		}
		this.targetFound = false;
		return false;
	}

	
	private bool checkForClosePlayers()
	{
		if (this.setup.sceneInfo.allPlayers.Count > 0)
		{
			int num = UnityEngine.Random.Range(0, Scene.SceneTracker.allPlayers.Count);
			for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
			{
				if (num == Scene.SceneTracker.allPlayers.Count)
				{
					num -= Scene.SceneTracker.allPlayers.Count;
				}
				GameObject gameObject = Scene.SceneTracker.allPlayers[num];
				if (gameObject)
				{
					this.tempTargetDist = (this.tr.position - gameObject.transform.position).sqrMagnitude;
					if (this.tempTargetDist < this.ai.playerDist * this.ai.playerDist && this.tempTargetDist < 1225f && gameObject.name != this.currentTarget.name)
					{
						Vector3 vector = this.tr.InverseTransformPoint(gameObject.transform.position);
						this.currTargetAngle = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
						if (this.currTargetAngle > -70f && this.currTargetAngle < 70f)
						{
							if (this.setup.headJoint)
							{
								this.castPos = this.setup.headJoint.transform.position;
							}
							else
							{
								this.castPos = this.setup.animator.rootPosition;
							}
							Vector3 direction = gameObject.GetComponent<Collider>().bounds.center - this.castPos;
							if (Physics.Raycast(this.castPos, direction, out this.hit, this.setCloseVisRange, this.visLayerMask))
							{
								this.hitGo = this.hit.transform.gameObject;
								this.closePlayerTarget = gameObject;
								return true;
							}
						}
					}
				}
				num++;
			}
		}
		return false;
	}

	
	private bool checkForPlayers()
	{
		if (this.setup.sceneInfo.allPlayers.Count > 0)
		{
			foreach (GameObject gameObject in this.setup.sceneInfo.allPlayers)
			{
				if (gameObject)
				{
					Vector3 vector = this.tr.InverseTransformPoint(gameObject.transform.position);
					this.currTargetAngle = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
					if (this.currTargetAngle > -70f && this.currTargetAngle < 70f)
					{
						if (this.setup.headJoint)
						{
							this.castPos = this.setup.headJoint.transform.position;
						}
						else
						{
							this.castPos = this.setup.animator.rootPosition;
						}
						Vector3 direction = gameObject.GetComponent<Collider>().bounds.center - this.castPos;
						if (Physics.Raycast(this.castPos, direction, out this.hit, this.setCloseVisRange, this.visLayerMask))
						{
							this.closePlayerTarget = gameObject;
							return true;
						}
					}
				}
			}
			return false;
		}
		return false;
	}

	
	public bool checkValidTarget()
	{
		if (!this.hitGo)
		{
			return false;
		}
		if (!this.hitGo.activeSelf)
		{
			return false;
		}
		this.lastHitGo = this.hitGo;
		if (this.hitGo.CompareTag("Player") || this.hitGo.CompareTag("Player_Remote") || this.hitGo.CompareTag("PlayerNet") || this.hitGo.CompareTag("PlayerRemote") || this.hitGo.CompareTag("playerFire"))
		{
			visRangeSetup component = this.hitGo.GetComponent<visRangeSetup>();
			targetStats component2 = this.hitGo.GetComponent<targetStats>();
			if (component2 && component2.targetDown)
			{
				return false;
			}
			if (!component)
			{
				return true;
			}
			if (this.startedTrack)
			{
				if (Vector3.Distance(this.rootTr.position, this.hitGo.transform.position) < this.setLongVisRange)
				{
					component.isTarget = true;
					return true;
				}
				return false;
			}
			else
			{
				if (Vector3.Distance(this.rootTr.position, this.hitGo.transform.position) < component.unscaledVisRange)
				{
					component.isTarget = true;
					return true;
				}
				return false;
			}
		}
		else
		{
			if (!this.hitGo.CompareTag("enemyRoot") || !(this.hitGo.name != base.transform.parent.gameObject.name))
			{
				return false;
			}
			mutantTypeSetup component3 = this.hitGo.GetComponent<mutantTypeSetup>();
			if (!component3)
			{
				return false;
			}
			if (component3.stats && !component3.stats.targetDown)
			{
				if (this.setup.ai.maleSkinny || this.setup.ai.femaleSkinny)
				{
					if (!component3.setup.ai.femaleSkinny && !component3.setup.ai.maleSkinny)
					{
						return true;
					}
				}
				else if (this.setup.ai.pale)
				{
					if (!component3.setup.ai.pale && !component3.setup.ai.creepy && !component3.setup.ai.creepy_male && component3.setup.ai.creepy_fat)
					{
						return true;
					}
				}
				else if (this.setup.ai.male || this.setup.ai.female)
				{
					if (component3.setup.ai.pale || component3.setup.ai.maleSkinny || component3.setup.ai.femaleSkinny || component3.setup.ai.creepy || component3.setup.ai.creepy_male || component3.setup.ai.creepy_fat)
					{
						return true;
					}
				}
				else
				{
					if (!this.setup.ai.creepy && !this.setup.ai.creepy_male && !component3.setup.ai.creepy_fat)
					{
						return false;
					}
					if (component3.setup.ai.maleSkinny || component3.setup.ai.femaleSkinny || component3.setup.ai.male || component3.setup.ai.female)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	
	private bool checkTargetStatus()
	{
		if (!this.hitGo)
		{
			return false;
		}
		if (!this.hitGo.activeSelf || this.hitGo.name == base.transform.parent.gameObject.name)
		{
			return false;
		}
		this.targetSetup = this.hitGo.GetComponent<targetStats>();
		return !this.targetSetup || !this.targetSetup.targetDown;
	}

	
	private bool checkCurrentTargetStatus()
	{
		if (!this.currentTarget)
		{
			return false;
		}
		if (!this.currentTarget.activeSelf || this.currentTarget.name == base.transform.parent.gameObject.name)
		{
			return false;
		}
		this.targetSetup = this.currentTarget.GetComponent<targetStats>();
		return !this.targetSetup || !this.targetSetup.targetDown;
	}

	
	public void updateClosePlayerTarget()
	{
		this.newTarget = this.closePlayerTarget;
		this.ai.target = this.closePlayerTarget.transform;
		this.hitGo = this.closePlayerTarget;
		if (this.currentTarget != null)
		{
			this.lastTarget = this.currentTarget;
		}
		this.currentTarget = this.closePlayerTarget;
		this.fsmCurrentTargetGo.Value = this.closePlayerTarget;
	}

	
	private Vector3 getTargetDir()
	{
		if (!(this.currentTarget != null))
		{
			return Vector3.zero;
		}
		Collider component = this.currentTarget.GetComponent<Collider>();
		if (component)
		{
			return component.bounds.center - this.setup.headJoint.transform.position;
		}
		return this.currentTarget.transform.position - this.setup.headJoint.transform.position;
	}

	
	private void setStructureTimeout()
	{
		this.setup.pmCombat.FsmVariables.GetFsmBool("structureTimeout").Value = true;
		base.Invoke("resetStructureTimeout", 25f);
	}

	
	private void resetStructureTimeout()
	{
		this.setup.pmCombat.FsmVariables.GetFsmBool("structureTimeout").Value = false;
	}

	
	private void resetClosestStructureTarget()
	{
		Scene.SceneTracker.closeStructureTarget = null;
	}

	
	public IEnumerator stealFoodRoutine()
	{
		float stealTimer = Time.time - 1f;
		if (this.currentStructureGo.Value == null)
		{
			this.setup.pmCombat.SendEvent("toReset");
			this.setup.pmCombatScript.cancelEvent = true;
			this.setup.pmSearchScript._cancelEvent = true;
			yield break;
		}
		Cook[] allFood = this.currentStructureGo.Value.transform.parent.GetComponentsInChildren<Cook>();
		if (allFood.Length == 0)
		{
			this.setup.pmCombat.SendEvent("toAttack");
			this.setup.pmCombatScript.doAction = true;
			this.setup.pmSearchScript._doAction = true;
			this.setup.pmSearchScript._cancelEvent = true;
			yield break;
		}
		int i = 0;
		while (this.currentStructureGo.Value)
		{
			if (i == allFood.Length)
			{
				this.setup.pmCombat.SendEvent("toAttack");
				this.setup.pmCombatScript.doAction = true;
				this.setup.pmSearchScript._doAction = true;
				yield break;
			}
			if (allFood[i] && Time.time > stealTimer)
			{
				UnityEngine.Object.Destroy(allFood[i].gameObject);
				stealTimer = Time.time + 15f;
				i++;
			}
			yield return null;
		}
		yield break;
	}

	
	private void checkAbovePlayer()
	{
		if (this.ai.lastPlayerTarget)
		{
			float num = this.tr.position.y - this.ai.lastPlayerTarget.position.y;
			if (this.setup.hitReactions.onStructure && num > 3f)
			{
				this.setup.pmCombat.SendEvent("eventA");
			}
			else
			{
				this.setup.pmCombat.SendEvent("toAction");
			}
		}
	}

	
	public void setToLastPlayerTarget()
	{
		Vector3 position;
		if (this.ai.lastPlayerTarget)
		{
			position = this.ai.lastPlayerTarget.transform.position;
			position.y = this.tr.position.y;
		}
		else
		{
			position = this.currentWaypoint.transform.position;
		}
		if (AstarPath.active == null)
		{
			return;
		}
		GraphNode node;
		if (this.fsmInCave.Value)
		{
			node = this.nmg.GetNearest(position).node;
		}
		else
		{
			node = this.rg.GetNearest(position).node;
		}
		if (node != null)
		{
			this.currentWaypoint.transform.position = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
			this.ai.target = this.currentWaypoint.transform;
			this.ai.SearchPath();
		}
	}

	
	public targetStats targetSetup;

	
	private GameObject player;

	
	private GameObject closePlayerTarget;

	
	public Transform worldPositionTr;

	
	public GameObject currentWaypoint;

	
	public GameObject lastTarget;

	
	public GameObject lastSighting;

	
	public GameObject nearestTree;

	
	public GameObject currentWallGo;

	
	public Collider currentTargetCollider;

	
	private GameObject privateCurrentTarget;

	
	private Transform tr;

	
	private Transform rootTr;

	
	public GameObject testSphere;

	
	private Transform lookatTr;

	
	private Transform tempTarget;

	
	private sceneTracker sceneInfo;

	
	private mutantFamilyFunctions familyFunctions;

	
	private mutantCollisionDetect collideDetect;

	
	public jumpObject currentJumpObj;

	
	private GameObject currentWaypointGo;

	
	private List<Transform> tempList;

	
	private GameObject newTarget;

	
	public wayPointSetup wpSetup;

	
	private float tempTargetDist;

	
	private float currentTargetDist;

	
	private float currTargetAngle;

	
	private float attachDist;

	
	public bool playerAware;

	
	private bool search;

	
	private int count;

	
	private int searchCount;

	
	private Vector2 randomPoint;

	
	private Vector3 pos;

	
	private RaycastHit hit;

	
	private Vector3 newWaypoint;

	
	private int layerMask;

	
	private int visLayerMask;

	
	private bool targetFound;

	
	private Vector3 castPos;

	
	private bool enableSearch;

	
	private bool screamCooldown;

	
	public bool followingLeader;

	
	public Vector3 _lastArtifactPos;

	
	private mutantAI ai;

	
	private Animator animator;

	
	private mutantScriptSetup setup;

	
	public float cautiousReset;

	
	public float searchReset;

	
	public float playerAwareReset;

	
	public float closeVisRange;

	
	public float longVisRange;

	
	public float setCloseVisRange;

	
	public float setLongVisRange;

	
	public float modifiedVisRange;

	
	public float modLighterRange;

	
	public float modMudRange;

	
	public float modEncounterRange;

	
	public float modCrouchRange;

	
	public float modBushRange;

	
	private float lighterRange;

	
	public GameObject lastHitGo;

	
	public GameObject lastVisibleTarget;

	
	public GameObject lastTorchGo;

	
	private FsmFloat fsmObjectAngle;

	
	private FsmGameObject fsmCaveEntrance;

	
	private FsmGameObject fsmLeaderGo;

	
	private FsmGameObject fsmCurrentTargetGo;

	
	public FsmBool fsmInCave;

	
	public FsmGameObject currentStructureGo;

	
	private FsmFloat fsmSearchClosestPlayerDist;

	
	private FsmFloat fsmCombatClosestPlayerDist;

	
	private FsmFloat fsmClosestPlayerAngle;

	
	private RecastGraph rg;

	
	private NavMeshGraph nmg;

	
	private float trapTriggerTimout;

	
	public bool lookingForTarget;

	
	private int targetCounter;

	
	private int trackCounter;

	
	public int visRetry = 6;

	
	public GameObject hitGo;

	
	public bool startedLook;

	
	public bool startedTrack;
}
