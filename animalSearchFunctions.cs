using System;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using Pathfinding;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Events;


public class animalSearchFunctions : MonoBehaviour
{
	
	private void OnEnable()
	{
	}

	
	private void Start()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		this.rg = AstarPath.active.astarData.recastGraph;
		this.tr = base.transform;
		this.info = Scene.SceneTracker;
		this.ai = base.gameObject.GetComponent<animalAI>();
		this.spawn = base.GetComponent<animalSpawnFunctions>();
		this.soundDetect = base.transform.GetComponentInChildren<animalSoundDetect>();
		PlayMakerFSM[] components = base.gameObject.GetComponents<PlayMakerFSM>();
		foreach (PlayMakerFSM playMakerFSM in components)
		{
			if (playMakerFSM.FsmName == "aiBaseFSM")
			{
				this.pmBase = playMakerFSM;
			}
		}
		Transform[] componentsInChildren = base.gameObject.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name == "currentWaypoint")
			{
				this.currentWaypoint = transform.gameObject;
			}
		}
		this.layer = 26;
		this.layerMask = 1 << this.layer;
		this.fsmTarget = this.pmBase.FsmVariables.GetFsmGameObject("targetGO");
		this.fsmObjectAngle = this.pmBase.FsmVariables.GetFsmFloat("objectAngle");
		this.fsmWaypointGO = this.pmBase.FsmVariables.GetFsmGameObject("waypointGO");
		this.fsmWaypointGO.Value = this.currentWaypoint.gameObject;
		this.currentWaypoint.transform.parent = null;
		this.pmBase.FsmVariables.GetFsmBool("drinkCheck").Value = false;
	}

	
	private void Update()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
		if (this.ai.isFollowing && this.closeAnimalGo && this.closeAnimalGo.activeSelf)
		{
			Vector3 vector;
			base.transform.position.y = vector.y + 2f;
			Vector3 vector2;
			this.closeAnimalGo.transform.position.y = vector2.y + 2f;
		}
	}

	
	private bool WaypointIsReachable(GraphNode waypointNode)
	{
		GraphNode node = this.rg.GetNearest(this.tr.position).node;
		return !node.Walkable || (waypointNode.Walkable && node.Area == waypointNode.Area);
	}

	
	private bool WaypointIsReachable(Vector3 waypoint)
	{
		GraphNode node = this.rg.GetNearest(this.tr.position).node;
		GraphNode node2 = this.rg.GetNearest(waypoint).node;
		return !node.Walkable || (node2.Walkable && node.Area == node2.Area);
	}

	
	private bool AnimalOnTerrain()
	{
		float num = Terrain.activeTerrain.SampleHeight(base.transform.position) + Terrain.activeTerrain.transform.position.y;
		if (this.tr.position.y - num > 2f || this.tr.position.y - num < -2f || base.transform.position.x > 2000f || base.transform.position.x < -2000f || base.transform.position.z > 2000f || base.transform.position.z < -2000f)
		{
			this.pmBase.SendEvent("noValidTarget");
			return false;
		}
		return true;
	}

	
	public IEnumerator findCloseWater()
	{
		for (int i = 0; i < this.info.drinkMarkers.Count; i++)
		{
			Transform temp = this.info.drinkMarkers[i];
			int randomIndex = UnityEngine.Random.Range(i, this.info.drinkMarkers.Count);
			this.info.drinkMarkers[i] = this.info.drinkMarkers[randomIndex];
			this.info.drinkMarkers[randomIndex] = temp;
		}
		this.currentDrinkMarker = null;
		for (int j = 0; j < this.info.drinkMarkers.Count; j++)
		{
			float dist = Vector3.Distance(this.tr.position, this.info.drinkMarkers[j].position);
			if (dist < 120f)
			{
				Vector3 leadPos = this.info.drinkMarkers[j].forward * -30f;
				leadPos = this.info.drinkMarkers[j].position + leadPos;
				leadPos.y = Terrain.activeTerrain.SampleHeight(leadPos) + Terrain.activeTerrain.transform.position.y;
				this.currentDrinkMarker = this.info.drinkMarkers[j];
				this.updateCurrentWaypoint(leadPos);
				this.setToWaypoint();
				this.pmBase.SendEvent("doAction");
			}
			yield return null;
			if (this.currentDrinkMarker)
			{
				yield break;
			}
		}
		if (this.currentDrinkMarker)
		{
			yield return null;
		}
		this.pmBase.SendEvent("noValidTarget");
		yield return null;
		yield break;
	}

	
	private IEnumerator findCloseBush()
	{
		this.layerMask = 4096;
		Collider[] allBush = Physics.OverlapSphere(this.tr.position, 40f, this.layerMask);
		Collider closestColl = null;
		foreach (Collider coll in allBush)
		{
			if (coll.CompareTag("SmallTree") && closestColl == null)
			{
				float dist = (coll.transform.position - this.tr.position).magnitude;
				if (dist > 12f)
				{
					closestColl = coll;
				}
			}
		}
		if (closestColl && this.WaypointIsReachable(closestColl.transform.position))
		{
			this.updateCurrentWaypoint(closestColl.transform.position);
			this.setToWaypoint();
			this.pmBase.FsmVariables.GetFsmGameObject("closeBushGo").Value = closestColl.gameObject;
			this.pmBase.SendEvent("doAction");
		}
		else
		{
			this.pmBase.SendEvent("noValidTarget");
		}
		yield return null;
		yield break;
	}

	
	public void findCloseAnimal()
	{
		GameObject gameObject = null;
		if (this.spawn.raccoon)
		{
			this.info.allRaccoons.RemoveAll((GameObject o) => o == null);
			for (int i = 0; i < this.info.allRaccoons.Count; i++)
			{
				float magnitude = (this.tr.position - this.info.allRaccoons[i].transform.position).magnitude;
				if (magnitude < 15f && this.info.allRaccoons[i].name != base.gameObject.name)
				{
					gameObject = this.info.allRaccoons[i];
				}
			}
		}
		if (this.spawn.deer)
		{
			this.info.allDeer.RemoveAll((GameObject o) => o == null);
			for (int j = 0; j < this.info.allDeer.Count; j++)
			{
				float magnitude2 = (this.tr.position - this.info.allDeer[j].transform.position).magnitude;
				if (magnitude2 < 15f && this.info.allDeer[j].name != base.gameObject.name)
				{
					gameObject = this.info.allDeer[j];
				}
			}
		}
		if (this.spawn.squirrel)
		{
			this.info.allSquirrel.RemoveAll((GameObject o) => o == null);
			for (int k = 0; k < this.info.allSquirrel.Count; k++)
			{
				float magnitude3 = (this.tr.position - this.info.allSquirrel[k].transform.position).magnitude;
				if (magnitude3 < 15f && this.info.allSquirrel[k].name != base.gameObject.name)
				{
					gameObject = this.info.allSquirrel[k];
				}
			}
		}
		if (this.spawn.boar)
		{
			this.info.allBoar.RemoveAll((GameObject o) => o == null);
			for (int l = 0; l < this.info.allBoar.Count; l++)
			{
				float magnitude4 = (this.tr.position - this.info.allBoar[l].transform.position).magnitude;
				if (magnitude4 < 15f && this.info.allBoar[l].name != base.gameObject.name)
				{
					gameObject = this.info.allBoar[l];
				}
			}
		}
		if (gameObject != null)
		{
			this.closeAnimalAi = gameObject.GetComponent<animalAI>();
			if (!this.closeAnimalAi.isFollowing && !this.closeAnimalAi.inTree)
			{
				this.pmBase.FsmVariables.GetFsmGameObject("closeAnimalGo").Value = gameObject;
				this.pmBase.SendEvent("doAction");
				this.pmBase.FsmVariables.GetFsmBool("followingBool").Value = true;
				this.ai.isFollowing = true;
				this.closeAnimalGo = gameObject;
			}
			else
			{
				this.pmBase.SendEvent("noValidTarget");
				this.pmBase.FsmVariables.GetFsmBool("followingBool").Value = false;
			}
		}
		else
		{
			this.pmBase.SendEvent("noValidTarget");
			this.pmBase.FsmVariables.GetFsmBool("followingBool").Value = false;
		}
	}

	
	public void findPositionNearAnimal()
	{
		GameObject gameObject = null;
		if (this.spawn.raccoon)
		{
			this.info.allRaccoons.RemoveAll((GameObject o) => o == null);
			for (int i = 0; i < this.info.allRaccoons.Count; i++)
			{
				float magnitude = (this.tr.position - this.info.allRaccoons[i].transform.position).magnitude;
				if (magnitude < 15f && this.info.allRaccoons[i].name != base.gameObject.name)
				{
					gameObject = this.info.allRaccoons[i];
				}
			}
		}
		if (this.spawn.deer)
		{
			this.info.allDeer.RemoveAll((GameObject o) => o == null);
			for (int j = 0; j < this.info.allDeer.Count; j++)
			{
				float magnitude2 = (this.tr.position - this.info.allDeer[j].transform.position).magnitude;
				if (magnitude2 < 15f && this.info.allDeer[j].name != base.gameObject.name)
				{
					gameObject = this.info.allDeer[j];
				}
			}
		}
		if (this.spawn.squirrel)
		{
			this.info.allSquirrel.RemoveAll((GameObject o) => o == null);
			for (int k = 0; k < this.info.allSquirrel.Count; k++)
			{
				float magnitude3 = (this.tr.position - this.info.allSquirrel[k].transform.position).magnitude;
				if (magnitude3 < 15f && this.info.allSquirrel[k].name != base.gameObject.name)
				{
					gameObject = this.info.allSquirrel[k];
				}
			}
		}
		if (this.spawn.boar)
		{
			this.info.allBoar.RemoveAll((GameObject o) => o == null);
			for (int l = 0; l < this.info.allBoar.Count; l++)
			{
				float magnitude4 = (this.tr.position - this.info.allBoar[l].transform.position).magnitude;
				if (magnitude4 < 15f && this.info.allBoar[l].name != base.gameObject.name)
				{
					gameObject = this.info.allBoar[l];
				}
			}
		}
		if (gameObject != null)
		{
			this.closeAnimalAi = gameObject.GetComponent<animalAI>();
			if (!this.closeAnimalAi.inTree)
			{
				Vector3 vector = (this.tr.position - gameObject.transform.position).normalized;
				vector = vector * 8f + gameObject.transform.position;
				float y = Terrain.activeTerrain.SampleHeight(vector) + Terrain.activeTerrain.transform.position.y;
				vector.y = y;
				this.updateCurrentWaypoint(vector);
				this.setToWaypoint();
				this.pmBase.SendEvent("doAction");
				return;
			}
			this.pmBase.SendEvent("noValidTarget");
		}
		this.pmBase.SendEvent("noValidTarget");
	}

	
	public void findCloseTrap()
	{
		if (!this.AnimalOnTerrain())
		{
			this.pmBase.SendEvent("noValidTarget");
			return;
		}
		GameObject gameObject = null;
		this.info.allRabbitTraps.RemoveAll((GameObject o) => o == null);
		for (int i = 0; i < this.info.allRabbitTraps.Count; i++)
		{
			float magnitude = (this.tr.position - this.info.allRabbitTraps[i].transform.position).magnitude;
			if (magnitude < 80f && magnitude > 12f && !this.info.allRabbitTraps[i].CompareTag("trapSprung"))
			{
				gameObject = this.info.allRabbitTraps[i];
			}
		}
		if (gameObject != null && this.WaypointIsReachable(gameObject.transform.position))
		{
			this.pmBase.FsmVariables.GetFsmGameObject("targetObjectGO").Value = gameObject;
			this.pmBase.SendEvent("doAction");
			this.updateCurrentWaypoint(gameObject.transform.position);
			this.setToWaypoint();
		}
		else
		{
			this.pmBase.SendEvent("noValidTarget");
		}
	}

	
	private IEnumerator findCloseTree(float dist)
	{
		if (!this.AnimalOnTerrain())
		{
			this.pmBase.SendEvent("toHome");
			yield break;
		}
		int layerMask = 2048;
		Collider[] allTrees = Physics.OverlapSphere(this.tr.position, dist, layerMask);
		for (int i = 0; i < allTrees.Length; i++)
		{
			Collider temp = allTrees[i];
			int randomIndex = UnityEngine.Random.Range(i, allTrees.Length);
			allTrees[i] = allTrees[randomIndex];
			allTrees[randomIndex] = temp;
		}
		float closestDist = 0f;
		for (int j = 0; j < allTrees.Length; j++)
		{
			closestDist = (allTrees[j].transform.position - this.tr.position).magnitude;
			if (closestDist > 15f && closestDist < 35f)
			{
				climbable climbScript = allTrees[j].transform.GetComponent<climbable>();
				if (climbScript)
				{
					Vector3 wpPos = new Vector3(allTrees[j].bounds.center.x, this.tr.position.y, allTrees[j].bounds.center.z);
					if (this.WaypointIsReachable(wpPos))
					{
						this.updateCurrentWaypoint(wpPos);
						this.pmBase.FsmVariables.GetFsmGameObject("treeGO").Value = this.currentWaypoint;
						this.pmBase.FsmVariables.GetFsmVector3("treePos").Value = wpPos;
						this.treeGirth = climbScript.climbDistance;
						this.nearestTree = this.currentWaypoint;
						this.activeTreeGo = allTrees[j].gameObject;
						base.StartCoroutine("validateCurrentTree", allTrees[j].gameObject);
						yield break;
					}
				}
			}
		}
		if (this.pmBase)
		{
			this.pmBase.SendEvent("toHome");
		}
		yield return null;
		yield break;
	}

	
	private void castPointAroundPlayer()
	{
		Vector2 vector = this.Circle2(UnityEngine.Random.Range(50f, 100f));
		Vector3 origin = new Vector3(this.info.allPlayers[0].transform.position.x + vector.x, this.info.allPlayers[0].transform.position.y + 50f, this.info.allPlayers[0].transform.position.z + vector.y);
		int num = 26;
		int num2 = 1 << num;
		if (!this.AnimalOnTerrain())
		{
			this.pmBase.SendEvent("noValidTarget");
			return;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(origin, Vector3.down, out raycastHit, 200f, num2))
		{
			if (raycastHit.collider.CompareTag("TerrainMain") && this.WaypointIsReachable(raycastHit.point))
			{
				this.updateCurrentWaypoint(raycastHit.point);
			}
			else
			{
				this.pmBase.SendEvent("noValidTarget");
			}
		}
	}

	
	private IEnumerator findClosestBeachMarker()
	{
		this.currentMarker = null;
		if (this.info.beachMarkers.Count > 0)
		{
			this.info.beachMarkers.Sort((GameObject c1, GameObject c2) => (this.<>f__this.tr.position - c1.transform.position).sqrMagnitude.CompareTo((this.<>f__this.tr.position - c2.transform.position).sqrMagnitude));
		}
		this.currentMarker = this.info.beachMarkers[UnityEngine.Random.Range(0, 3)];
		this.pmBase.FsmVariables.GetFsmGameObject("currentMarkerGo").Value = this.currentMarker;
		Vector3 pos;
		if (this.spawn.turtle)
		{
			Vector2 randomPoint = this.Circle2(UnityEngine.Random.Range(5f, 10f));
			pos = new Vector3(this.currentMarker.transform.position.x + randomPoint.x, this.currentMarker.transform.position.y, this.currentMarker.transform.position.z + randomPoint.y);
		}
		else
		{
			pos = this.currentMarker.transform.position;
		}
		GraphNode node = this.rg.GetNearest(pos).node;
		Vector3 newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
		if (this.WaypointIsReachable(newWaypoint) || this.spawn.crocodile)
		{
			this.updateCurrentWaypoint(newWaypoint);
			this.setToWaypoint();
			this.search = false;
			if (this.ai.swimming)
			{
				this.ai.StartCoroutine("enableForceTarget", this.currentWaypoint);
			}
		}
		else
		{
			this.pmBase.SendEvent("noValidTarget");
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator findClosestSwimMarker()
	{
		float closestDist = float.PositiveInfinity;
		this.currentMarker = null;
		for (int i = 0; i < this.info.swimMarkers.Count; i++)
		{
			float dist = (this.tr.position - this.info.swimMarkers[i].transform.position).sqrMagnitude;
			if (dist < closestDist)
			{
				closestDist = dist;
				this.currentMarker = this.info.swimMarkers[i];
			}
		}
		if (this.currentMarker)
		{
			Vector2 randomPoint = this.Circle2((float)UnityEngine.Random.Range(5, 20));
			Vector3 newPos = new Vector3(this.currentMarker.transform.position.x + randomPoint.x, this.currentMarker.transform.position.y, this.currentMarker.transform.position.z + randomPoint.y);
			if (this.WaypointIsReachable(newPos) || this.spawn.crocodile)
			{
				this.pmBase.FsmVariables.GetFsmGameObject("currentMarkerGo").Value = this.currentMarker;
				this.updateCurrentWaypoint(newPos);
				this.setToWaypoint();
				this.ai.StartCoroutine("enableForceTarget", this.currentWaypoint);
			}
			else
			{
				this.currentMarker = null;
			}
		}
		if (!this.currentMarker)
		{
			this.pmBase.SendEvent("noValidTarget");
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator findRandomSwimMarker()
	{
		float closestDist = float.PositiveInfinity;
		this.currentMarker = null;
		while (closestDist > 100f)
		{
			int rand = UnityEngine.Random.Range(0, this.info.swimMarkers.Count);
			float dist = (this.tr.position - this.info.swimMarkers[rand].transform.position).magnitude;
			if (dist < 100f)
			{
				closestDist = dist;
				this.currentMarker = this.info.swimMarkers[rand];
			}
		}
		if (this.currentMarker && (this.WaypointIsReachable(this.currentMarker.transform.position) || this.spawn.crocodile))
		{
			this.pmBase.FsmVariables.GetFsmGameObject("currentMarkerGo").Value = this.currentMarker;
			this.updateCurrentWaypoint(this.currentMarker.transform.position);
			this.ai.StartCoroutine("enableForceTarget", this.currentWaypoint);
			this.setToWaypoint();
		}
		else
		{
			this.pmBase.SendEvent("noValidTarget");
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator findRandomPoint(float dist)
	{
		this.search = true;
		if (AstarPath.active == null)
		{
			this.pmBase.SendEvent("noValidTarget");
			this.search = false;
			yield break;
		}
		if (!this.AnimalOnTerrain())
		{
			this.pmBase.SendEvent("noValidTarget");
			this.search = false;
			yield break;
		}
		this.layer = 26;
		this.layerMask = 1 << this.layer;
		for (int i = 0; i < 5; i++)
		{
			float attemptDistance = dist * (float)(5 - i) / 5f;
			Vector2 randomPoint = this.Circle2(UnityEngine.Random.Range(attemptDistance - 5f, 10f + attemptDistance));
			Vector3 pos = new Vector3(this.tr.position.x + randomPoint.x, this.tr.position.y, this.tr.position.z + randomPoint.y);
			pos.y = Terrain.activeTerrain.SampleHeight(pos) + Terrain.activeTerrain.transform.position.y;
			GraphNode node = this.rg.GetNearest(pos).node;
			if (this.WaypointIsReachable(node))
			{
				Vector3 newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
				this.updateCurrentWaypoint(newWaypoint);
				this.pmBase.SendEvent("doAction");
				this.search = false;
			}
		}
		if (this.search)
		{
			this.pmBase.SendEvent("noValidTarget");
			this.search = false;
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator findPointAwayFromPlayer(float dist)
	{
		if (this.info.allPlayers != null && this.info.allPlayers.Count > 0)
		{
			bool foundPath = false;
			if (!this.AnimalOnTerrain())
			{
				this.pmBase.SendEvent("noValidTarget");
				yield break;
			}
			for (int i = 0; i < 5; i++)
			{
				Vector3 playerDir = this.tr.position - this.info.allPlayers[0].transform.position;
				float attemptDistance = dist * (float)(5 - i) / 5f;
				Vector3 endPoint = this.tr.position + playerDir.normalized * UnityEngine.Random.Range(attemptDistance - 10f, 10f + attemptDistance);
				endPoint = new Vector3(endPoint.x + UnityEngine.Random.Range(-10f, 10f), endPoint.y, endPoint.z + UnityEngine.Random.Range(-10f, 10f));
				this.layer = 26;
				this.layerMask = 1 << this.layer;
				GraphNode node = this.rg.GetNearest(endPoint).node;
				if (this.WaypointIsReachable(node))
				{
					Vector3 newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
					this.updateCurrentWaypoint(newWaypoint);
					this.setToWaypoint();
					this.pmBase.SendEvent("doAction");
					foundPath = true;
					break;
				}
				this.pmBase.SendEvent("noValidTarget");
			}
			if (!foundPath)
			{
				this.pmBase.SendEvent("noValidTarget");
			}
			yield return null;
		}
		else
		{
			this.pmBase.SendEvent("noValidTarget");
		}
		yield break;
	}

	
	private IEnumerator findPointAwayFromSound(float dist)
	{
		if (!this.AnimalOnTerrain())
		{
			this.pmBase.SendEvent("noValidTarget");
			yield break;
		}
		Vector3 targetDir = this.tr.position - this.soundDetect.lastSoundPosition;
		Vector3 endPoint = this.tr.position + targetDir.normalized * UnityEngine.Random.Range(dist - 10f, 10f + dist);
		endPoint = new Vector3(endPoint.x + UnityEngine.Random.Range(-10f, 10f), endPoint.y, endPoint.z + UnityEngine.Random.Range(-10f, 10f));
		this.layer = 26;
		this.layerMask = 1 << this.layer;
		GraphNode node = this.rg.GetNearest(endPoint).node;
		if (this.WaypointIsReachable(node))
		{
			Vector3 newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
			this.updateCurrentWaypoint(newWaypoint);
			this.setToWaypoint();
			this.pmBase.SendEvent("doAction");
		}
		else
		{
			this.pmBase.SendEvent("noValidTarget");
		}
		yield return null;
		yield break;
	}

	
	private void updateCurrentTarget()
	{
		GameObject value = this.pmBase.FsmVariables.GetFsmGameObject("targetObjectGO").Value;
		this.ai.target = value.transform;
		this.fsmTarget.Value = value;
		this.fsmWaypointGO.Value.transform.position = value.transform.position;
	}

	
	private void updateCurrentColliderTarget()
	{
		GameObject value = this.pmBase.FsmVariables.GetFsmGameObject("targetObjectGO").Value;
		this.ai.target = value.transform;
		this.fsmTarget.Value = value;
		this.fsmWaypointGO.Value.transform.position = new Vector3(value.GetComponent<Collider>().bounds.center.x, value.transform.position.y, value.GetComponent<Collider>().bounds.center.z);
	}

	
	private void updateCurrentWaypoint(Vector3 vect)
	{
		this.currentWaypoint.transform.position = vect;
		this.ai.target = this.currentWaypoint.transform;
		this.fsmTarget.Value = this.currentWaypoint.gameObject;
	}

	
	public Vector2 Circle2(float radius)
	{
		Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
		insideUnitCircle.Normalize();
		return insideUnitCircle * radius;
	}

	
	private void setToWaypoint()
	{
		this.ai.target = this.currentWaypoint.transform;
		this.fsmTarget.Value = this.currentWaypoint.gameObject;
		this.ai.SearchPath();
	}

	
	private void setToCurrentWater()
	{
		this.currentWaypoint.transform.position = this.currentDrinkMarker.position;
		this.ai.target = this.currentWaypoint.transform;
		this.fsmTarget.Value = this.currentWaypoint.gameObject;
		this.ai.SearchPath();
	}

	
	private void setToPlayer()
	{
		if (this.info.allPlayers.Count > 0)
		{
			if (this.info.allPlayers[0] == null)
			{
				return;
			}
			this.ai.target = this.info.allPlayers[0].transform;
			this.fsmTarget.Value = this.info.allPlayers[0].gameObject;
			if (this.ai.swimming && this.ai.gameObject.activeSelf)
			{
				this.ai.StartCoroutine("enableForceTarget", this.info.allPlayers[0].gameObject);
			}
		}
	}

	
	private void setToCloseAnimal()
	{
		if (!this.followCoolDown)
		{
			this.followCoolDown = true;
			base.Invoke("setFollowTimeout", (float)UnityEngine.Random.Range(30, 60));
		}
		this.ai.target = this.closeAnimalGo.transform;
		this.fsmTarget.Value = this.closeAnimalGo;
	}

	
	private void setFollowTimeout()
	{
		this.pmBase.FsmVariables.GetFsmBool("followingBool").Value = false;
		this.ai.isFollowing = false;
		this.followCoolDown = false;
	}

	
	private IEnumerator findAngleToTarget(Vector3 target)
	{
		Vector3 localTarget = this.tr.InverseTransformPoint(target);
		float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * 57.29578f;
		this.fsmObjectAngle.Value = targetAngle;
		yield return null;
		yield break;
	}

	
	private IEnumerator setTreeAttachDist(float dist)
	{
		this.attachDist = dist;
		yield return null;
		yield break;
	}

	
	private IEnumerator findTreeAttachPos(Vector3 pos)
	{
		Vector3 otherPos = new Vector3(pos.x, this.tr.position.y, pos.z);
		Vector3 dir = (this.tr.position - otherPos).normalized;
		Vector3 jumpPos = pos + dir * this.attachDist * this.treeGirth;
		this.pmBase.FsmVariables.GetFsmVector3("attachPos").Value = jumpPos;
		yield return null;
		yield break;
	}

	
	private void checkActiveAnimal()
	{
		if (this.closeAnimalAi && this.closeAnimalAi.inTree)
		{
			this.pmBase.SendEvent("toStopFollowing");
		}
	}

	
	private void setDrinkCoolDown()
	{
		this.pmBase.FsmVariables.GetFsmBool("drinkCheck").Value = true;
		base.Invoke("resetDrinkCoolDown", 120f);
	}

	
	private void resetDrinkCoolDown()
	{
		if (base.enabled)
		{
			this.pmBase.FsmVariables.GetFsmBool("drinkCheck").Value = false;
		}
	}

	
	public void enableForceDirectionToTarget(Transform target)
	{
	}

	
	public void setupTreeListener()
	{
		this.onTree = true;
		TreeHealth.OnTreeCutDown.AddListener(new UnityAction<Vector3>(this.OnCutDownTree));
	}

	
	private void OnDisable()
	{
		base.StopAllCoroutines();
		if (this.onTree)
		{
			TreeHealth.OnTreeCutDown.RemoveListener(new UnityAction<Vector3>(this.OnCutDownTree));
			this.onTree = false;
		}
	}

	
	private void OnDestroy()
	{
		base.StopAllCoroutines();
		if (this.onTree)
		{
			TreeHealth.OnTreeCutDown.RemoveListener(new UnityAction<Vector3>(this.OnCutDownTree));
			this.onTree = false;
		}
	}

	
	public void OnCutDownTree(Vector3 treePos)
	{
		Vector3 position = base.transform.position;
		position.y = 0f;
		treePos.y = 0f;
		if (Vector3.Distance(position, treePos) < 5f && this.onTree)
		{
			this.ai.goRagdoll();
		}
	}

	
	private IEnumerator validateCurrentTree(GameObject tree)
	{
		TreeHealth th = tree.GetComponent<TreeHealth>();
		if (!th)
		{
			yield break;
		}
		if (th.LodTree)
		{
			LOD_Trees lt = th.LodTree.GetComponent<LOD_Trees>();
			while (lt.CurrentLodTransform != null && lt.enabled)
			{
				yield return null;
			}
			this.pmBase.SendEvent("cancelTreeClimb");
		}
		yield return null;
		yield break;
	}

	
	private void cancelValidateTree()
	{
		base.StopCoroutine("validateCurrentTree");
	}

	
	private const int MaxPathingAttempts = 5;

	
	public GameObject currentWaypoint;

	
	public GameObject nearestTree;

	
	public GameObject activeTreeGo;

	
	public GameObject currentMarker;

	
	public Transform currentDrinkMarker;

	
	private Transform tr;

	
	private float attachDist;

	
	private float treeGirth;

	
	public bool onTree;

	
	private PlayMakerFSM pmBase;

	
	private animalAI ai;

	
	public animalAI closeAnimalAi;

	
	private sceneTracker info;

	
	private animalSpawnFunctions spawn;

	
	private animalSoundDetect soundDetect;

	
	private List<GameObject> tempList;

	
	private RaycastHit hit;

	
	private int layer;

	
	private int layerMask;

	
	private bool search;

	
	private bool followCoolDown;

	
	private FsmFloat fsmObjectAngle;

	
	private FsmGameObject fsmTarget;

	
	private FsmGameObject fsmWaypointGO;

	
	public GameObject closeAnimalGo;

	
	private FsmFloat fsmPlayerDist;

	
	private RecastGraph rg;
}
