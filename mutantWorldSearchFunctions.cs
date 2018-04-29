using System;
using System.Collections;
using Pathfinding;
using TheForest.Utils;
using UnityEngine;


public class mutantWorldSearchFunctions : MonoBehaviour
{
	
	private void Awake()
	{
		this.sceneInfo = Scene.SceneTracker;
		this.setup = base.GetComponent<mutantScriptSetup>();
		this.ai = base.GetComponent<mutantAI>();
		this.searchFunctions = base.GetComponent<mutantSearchFunctions>();
		this.tr = base.transform;
	}

	
	private void OnEnable()
	{
		this.resetEncounterCoolDown();
	}

	
	private IEnumerator findClosestEntrance()
	{
		GameObject closestObj = null;
		float closestDist = float.PositiveInfinity;
		foreach (GameObject gameObject in this.sceneInfo.caveMarkers)
		{
			float sqrMagnitude = (this.tr.position - gameObject.transform.position).sqrMagnitude;
			if (sqrMagnitude < closestDist)
			{
				closestDist = sqrMagnitude;
				closestObj = gameObject;
			}
			this.setup.pmSleep.FsmVariables.GetFsmGameObject("closestEntranceGo").Value = closestObj;
			this.setup.homeGo = closestObj;
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator findClosestFeederToPlayer()
	{
		GameObject closestObj = null;
		float closestDist = float.PositiveInfinity;
		int count = 0;
		int size = this.sceneInfo.feedingEncounters.Count;
		if (size >= 1)
		{
			foreach (GameObject gameObject in this.sceneInfo.feedingEncounters)
			{
				float sqrMagnitude = (this.setup.sceneInfo.allPlayers[0].transform.position - gameObject.transform.position).sqrMagnitude;
				if (sqrMagnitude < closestDist)
				{
					closestDist = sqrMagnitude;
					closestObj = gameObject;
				}
				count++;
				this.setup.feederGo = closestObj;
			}
			yield return null;
		}
		yield break;
	}

	
	public bool findCloseGroupEncounter()
	{
		if (this.sceneInfo.encounters.Count < 1)
		{
			return false;
		}
		GameObject gameObject = this.sceneInfo.encounters[UnityEngine.Random.Range(0, this.sceneInfo.encounters.Count)];
		if (Vector3.Distance(this.tr.position, gameObject.transform.position) > 350f)
		{
			return false;
		}
		if (!gameObject.activeSelf)
		{
			return false;
		}
		this.getEncounterComponent(gameObject);
		if (!this.ge.occupied && !this.ge.encounterCoolDown)
		{
			this.setup.pmEncounter.enabled = true;
			this.encounterGo = gameObject;
			this.setup.pmEncounter.FsmVariables.GetFsmGameObject("encounterGo").Value = gameObject;
			this.setEncounterType();
			this.ge.occupied = true;
			this.ge.enableEncounterCoolDown();
			this.setup.search.currentTarget = gameObject;
			return true;
		}
		return false;
	}

	
	private IEnumerator findPointAroundEncounter(float dist)
	{
		this.search = true;
		while (this.search)
		{
			this.randomPoint = this.Circle2(UnityEngine.Random.Range(dist - 5f, dist + 7f));
			this.pos = new Vector3(this.encounterGo.transform.position.x + this.randomPoint.x, this.encounterGo.transform.position.y, this.encounterGo.transform.position.z + this.randomPoint.y);
			GraphNode node = AstarPath.active.GetNearest(this.pos).node;
			if (node.Walkable)
			{
				Vector3 newWaypoint = new Vector3((float)(node.position[0] / 1000), (float)(node.position[1] / 1000), (float)(node.position[2] / 1000));
				this.searchFunctions.updateCurrentWaypoint(newWaypoint);
				if (this.setup.pmEncounter)
				{
					this.setup.pmEncounter.SendEvent("doAction");
				}
				this.search = false;
				yield return null;
			}
			else if (this.setup.pmEncounter)
			{
				this.setup.pmEncounter.SendEvent("noValidTarget");
			}
			yield return null;
		}
		yield break;
	}

	
	public void getEncounterComponent(GameObject go)
	{
		this.ge = go.GetComponent<groupEncounterSetup>();
	}

	
	public void setEncounterType()
	{
		this.setup.pmEncounter.enabled = true;
		if (this.ge.typeRitual1)
		{
			this.setup.pmEncounter.FsmVariables.GetFsmBool("boolRitual1").Value = true;
		}
		if (this.ge.typeRitual2)
		{
			this.setup.pmEncounter.FsmVariables.GetFsmBool("boolRitual2").Value = true;
		}
		if (this.ge.typeFeeding1)
		{
			this.setup.pmEncounter.FsmVariables.GetFsmBool("boolFeeding1").Value = true;
		}
		if (this.ge.typeMourning1)
		{
			this.setup.pmEncounter.FsmVariables.GetFsmBool("boolMourning1").Value = true;
			this.ge.forceEnableAnimator();
		}
		this.ge.enableEncounterCoolDown();
		if (this.setup.pmSearchScript)
		{
			this.setup.pmSearchScript._encounterCoolDown = true;
		}
		base.Invoke("resetEncounterCoolDown", 300f);
		this.setup.pmEncounter.SendEvent("toActivateFSM");
	}

	
	private void resetEncounterCoolDown()
	{
		if (!this.setup)
		{
			return;
		}
		if (this.setup.pmSearchScript)
		{
			this.setup.pmSearchScript._encounterCoolDown = false;
		}
	}

	
	private void getLookatGo()
	{
		this.setup.pmEncounter.FsmVariables.GetFsmGameObject("lookatGo").Value = this.ge.lookatGo;
	}

	
	private IEnumerator getLeaderPos()
	{
		if (!this.ge.leaderPosFull)
		{
			this.ge.leaderPosFull = true;
			this.setup.pmEncounter.FsmVariables.GetFsmVector3("encounterPos").Value = this.ge.leaderPos.position;
			this.setup.pmEncounter.FsmVariables.GetFsmGameObject("encounterPosGo").Value = this.ge.leaderPos.gameObject;
			this.encounterPosGo = this.ge.leaderPos.gameObject;
			this.ai.target = this.ge.leaderPos;
		}
		else
		{
			this.setup.pmEncounter.SendEvent("busy");
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator getFollowerPos()
	{
		if (!this.ge.followerPosFull1)
		{
			this.ge.followerPosFull1 = true;
			this.setup.pmEncounter.FsmVariables.GetFsmVector3("encounterPos").Value = this.ge.followerPos1.position;
			this.setup.pmEncounter.FsmVariables.GetFsmGameObject("encounterPosGo").Value = this.ge.followerPos1.gameObject;
			this.ai.target = this.ge.followerPos1;
		}
		else if (!this.ge.followerPosFull2)
		{
			this.ge.followerPosFull2 = true;
			this.setup.pmEncounter.FsmVariables.GetFsmVector3("encounterPos").Value = this.ge.followerPos2.position;
			this.setup.pmEncounter.FsmVariables.GetFsmGameObject("encounterPosGo").Value = this.ge.followerPos2.gameObject;
			this.ai.target = this.ge.followerPos2;
		}
		else if (!this.ge.followerPosFull3)
		{
			this.ge.followerPosFull3 = true;
			this.setup.pmEncounter.FsmVariables.GetFsmVector3("encounterPos").Value = this.ge.followerPos3.position;
			this.setup.pmEncounter.FsmVariables.GetFsmGameObject("encounterPosGo").Value = this.ge.followerPos3.gameObject;
			this.ai.target = this.ge.followerPos3;
		}
		else if (!this.ge.followerPosFull4)
		{
			this.ge.followerPosFull4 = true;
			this.setup.pmEncounter.FsmVariables.GetFsmVector3("encounterPos").Value = this.ge.followerPos4.position;
			this.setup.pmEncounter.FsmVariables.GetFsmGameObject("encounterPosGo").Value = this.ge.followerPos4.gameObject;
			this.ai.target = this.ge.followerPos4;
		}
		yield return null;
		yield break;
	}

	
	private void removeEncounterPos()
	{
		if (this.ge)
		{
			this.ge.leaderPosFull = false;
			this.ge.followerPosFull1 = false;
			this.ge.followerPosFull2 = false;
			this.ge.followerPosFull3 = false;
			this.ge.followerPosFull4 = false;
			this.ge.occupied = false;
		}
	}

	
	private IEnumerator gibCurrentFeeder(GameObject go)
	{
		if (go)
		{
			chopEnemy componentInChildren = go.GetComponentInChildren<chopEnemy>();
			if (componentInChildren)
			{
				componentInChildren.triggerChop();
			}
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator gibCurrentMutant(GameObject go)
	{
		if (go)
		{
			explodeDummy componentInChildren = go.GetComponentInChildren<explodeDummy>();
			if (componentInChildren)
			{
				componentInChildren.Explosion(5f);
			}
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator sendFeedingEffect(GameObject go)
	{
		if (go)
		{
			go.SendMessage("enableFeedingEffect", SendMessageOptions.DontRequireReceiver);
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator stopFeedingEffect(GameObject go)
	{
		if (go)
		{
			go.SendMessage("disableFeedingEffect", SendMessageOptions.DontRequireReceiver);
		}
		yield return null;
		yield break;
	}

	
	public Vector2 Circle2(float radius)
	{
		Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
		insideUnitCircle.Normalize();
		return insideUnitCircle * radius;
	}

	
	private IEnumerator moveToEncounter()
	{
		this.setup.controller.enabled = false;
		Quaternion currRotation = this.setup.rotateTr.rotation;
		Vector3 desiredPos = base.transform.parent.position;
		while (true && this.encounterPosGo)
		{
			desiredPos = Vector3.Slerp(desiredPos, this.encounterPosGo.transform.position, Time.deltaTime * 0.8f);
			currRotation = Quaternion.Slerp(currRotation, this.encounterPosGo.transform.rotation, Time.deltaTime * 0.8f);
			base.transform.parent.position = desiredPos;
			this.setup.rotateTr.rotation = currRotation;
			yield return YieldPresets.WaitForFixedUpdate;
		}
		yield break;
	}

	
	private IEnumerator resetMoveToEncounter()
	{
		base.StopCoroutine("moveToEncounter");
		if (this.setup.controller)
		{
			this.setup.controller.enabled = true;
		}
		this.encounterPosGo = null;
		this.setup.rotateTr.localEulerAngles = new Vector3(0f, this.setup.rotateTr.localEulerAngles.y, 0f);
		yield return null;
		yield break;
	}

	
	private void resetMourningAnimator()
	{
		if (!this.setup)
		{
			return;
		}
		if (this.setup.ai.leader && this.ge && this.ge.lookatGo)
		{
			Animator component = this.ge.lookatGo.GetComponent<Animator>();
			if (component)
			{
				component.SetBoolReflected("encounterBOOL", false);
			}
		}
		this.setup.animControl.forceIkBool = false;
	}

	
	private void setIkToEncounter()
	{
		if (this.encounterGo)
		{
			this.setup.search.currentTarget = this.encounterGo;
		}
		this.setup.animControl.forceIkBool = true;
	}

	
	private sceneTracker sceneInfo;

	
	private mutantScriptSetup setup;

	
	private mutantSearchFunctions searchFunctions;

	
	public groupEncounterSetup ge;

	
	private mutantAI ai;

	
	private Transform tr;

	
	public GameObject encounterGo;

	
	public GameObject encounterPosGo;

	
	private RaycastHit hit;

	
	private bool search;

	
	private Vector3 pos;

	
	private Vector2 randomPoint;
}
