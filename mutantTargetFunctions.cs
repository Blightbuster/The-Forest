using System;
using System.Collections;
using HutongGames.PlayMaker;
using PathologicalGames;
using TheForest.Utils;
using UnityEngine;


public class mutantTargetFunctions : MonoBehaviour
{
	
	private void Start()
	{
		this.animator = base.GetComponent<Animator>();
		this.setup = base.GetComponent<mutantScriptSetup>();
		this.thisTr = base.transform;
		if (this.setup.pmCombat)
		{
			this.fsmFearBool = this.setup.pmCombat.FsmVariables.GetFsmBool("fearBOOL");
		}
		if (this.setup.pmBrain)
		{
			this.fsmDeadBool = this.setup.pmBrain.FsmVariables.GetFsmBool("deadBool");
		}
		if (this.setup.pmCombat)
		{
			this.fsmDownBool = this.setup.pmCombat.FsmVariables.GetFsmBool("deathBool");
		}
		base.InvokeRepeating("sendRemoveAttacker", 1f, 4f);
		base.InvokeRepeating("sendAddVisibleTarget", 1f, 3f);
	}

	
	private void OnDespawned()
	{
		this.forceRemoveAttacker();
	}

	
	private void sendAddVisibleTarget()
	{
		if (this.setup.disableAiForDebug)
		{
			return;
		}
		if (!this.setup.ai.creepy && !this.setup.ai.creepy_male && !this.setup.ai.creepy_fat && this.setup.search.currentTarget)
		{
			if (this.setup.search.currentTarget.CompareTag("Player") && this.setup.ai.playerDist < 90f && !this.setup.search.lookingForTarget)
			{
				this.setup.sceneInfo.addToVisible(base.transform.parent.gameObject);
			}
			else if (!this.setup.search.currentTarget.CompareTag("Player") || this.setup.search.lookingForTarget)
			{
				this.setup.sceneInfo.removeFromVisible(base.transform.parent.gameObject);
			}
		}
	}

	
	public void sendAddAttacker()
	{
		if (this.setup.search.currentTarget && (this.setup.search.currentTarget.CompareTag("Player") || this.setup.search.currentTarget.CompareTag("PlayerNet") || this.setup.search.currentTarget.CompareTag("PlayerRemote")) && !this.setup.ai.creepy && !this.setup.ai.creepy_male && !this.setup.ai.creepy_baby && !this.setup.ai.creepy_fat && Scene.SceneTracker)
		{
			Scene.SceneTracker.addAttacker(this.setup.hashName);
		}
	}

	
	public void sendRemoveAttacker()
	{
		if (!this.setup.ai.creepy && !this.setup.ai.creepy_male && !this.setup.ai.creepy_fat && (this.setup.ai.playerDist > this.threatRemoveDist || this.fsmDeadBool.Value || this.fsmDownBool.Value) && Scene.SceneTracker)
		{
			Scene.SceneTracker.removeAttacker(this.setup.hashName);
		}
	}

	
	public void forceRemoveAttacker()
	{
		if (!this.setup.ai.creepy && !this.setup.ai.creepy_male && !this.setup.ai.creepy_fat && Scene.SceneTracker)
		{
			Scene.SceneTracker.removeAttacker(this.setup.hashName);
		}
	}

	
	public void EnemyInLight(TargetTracker source)
	{
		if (source.gameObject.name == "Light")
		{
			this.animator.SetBoolReflected("fearBOOL", true);
			this.fsmFearBool.Value = true;
		}
	}

	
	private void EnemyOutOfLight(TargetTracker source)
	{
		if (source.gameObject.name == "Light")
		{
			this.fsmFearBool.Value = false;
			this.animator.SetBoolReflected("fearBOOL", false);
		}
	}

	
	private void PlayerNoiseDetected(TargetTracker source)
	{
		if (source.gameObject.CompareTag("playerBase"))
		{
			base.StartCoroutine("sendPlayerNoise");
			this.setup.lastSighting.transform.position = source.transform.position;
		}
	}

	
	private void PlayerNoiseStop(TargetTracker source)
	{
		if (source.gameObject.CompareTag("playerBase"))
		{
			base.StopCoroutine("sendPlayerNoise");
		}
	}

	
	private IEnumerator sendPlayerNoise()
	{
		if (this.setup.pmCombat)
		{
			this.setup.pmCombat.SendEvent("toPlayerNoise");
		}
		if (this.setup.pmSleep)
		{
			this.setup.pmSleep.SendEvent("toNoise");
		}
		if (this.setup.pmEncounter)
		{
			this.setup.pmEncounter.SendEvent("toNoise");
		}
		if (this.setup.pmSearchScript)
		{
			this.setup.pmSearchScript._toPlayerNoise = true;
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator returnObjectAngle(Vector3 pos)
	{
		Vector3 localTarget = this.thisTr.InverseTransformPoint(pos);
		float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * 57.29578f;
		this.setup.pmCombat.FsmVariables.GetFsmFloat("objectAngle").Value = targetAngle;
		this.setup.pmBrain.FsmVariables.GetFsmFloat("objectAngle").Value = targetAngle;
		yield return null;
		yield break;
	}

	
	private IEnumerator returnTargetObjectAngle(GameObject go)
	{
		Vector3 localTarget = go.transform.InverseTransformPoint(this.thisTr.position);
		float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * 57.29578f;
		this.setup.pmCombat.FsmVariables.GetFsmFloat("objectAngle").Value = targetAngle;
		this.setup.pmBrain.FsmVariables.GetFsmFloat("objectAngle").Value = targetAngle;
		yield return null;
		yield break;
	}

	
	public IEnumerator getTargetRunningAway()
	{
		Vector3 lastPos = this.setup.ai.target.position;
		yield return YieldPresets.WaitForFixedUpdate;
		yield return YieldPresets.WaitForFixedUpdate;
		Vector3 dir = lastPos - this.setup.ai.target.position;
		Vector3 relativePos = this.thisTr.position - dir;
		if (dir.magnitude <= 0.1f)
		{
			this.setup.pmCombat.SendEvent("toRunNormalAttack");
			this.setup.pmCombatScript.runLongAttack = false;
			yield break;
		}
		relativePos = new Vector3(relativePos.x, this.thisTr.position.y, relativePos.z);
		Vector3 vector = this.thisTr.InverseTransformPoint(relativePos);
		float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
		if (num < 80f && num > -80f)
		{
			this.setup.pmCombat.SendEvent("toRunBackAttack");
			this.setup.pmCombatScript.runLongAttack = true;
			yield break;
		}
		this.setup.pmCombat.SendEvent("toRunNormalAttack");
		this.setup.pmCombatScript.runLongAttack = false;
		yield break;
	}

	
	private Animator animator;

	
	private mutantScriptSetup setup;

	
	private Transform thisTr;

	
	private FsmBool fsmFearBool;

	
	private FsmBool fsmDeadBool;

	
	private FsmBool fsmDownBool;

	
	public float threatRemoveDist = 20f;

	
	public int defaultVisionRange = 50;
}
