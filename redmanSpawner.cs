using System;
using System.Collections;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;


public class redmanSpawner : MonoBehaviour
{
	
	private void Start()
	{
		if (BoltNetwork.isRunning)
		{
			return;
		}
		base.StartCoroutine("doYachtSpawn");
		base.StartCoroutine("doCliffSpawn");
		this.canSeeRedDay = Clock.Day;
	}

	
	private void OnDeserialized()
	{
		this.canSeeRedDay = Clock.Day;
	}

	
	private IEnumerator doYachtSpawn()
	{
		if (this.doneYacht)
		{
			yield break;
		}
		while (Clock.planecrash)
		{
			yield return null;
		}
		yield return YieldPresets.WaitTwentySeconds;
		while (LocalPlayer.Transform)
		{
			while (Clock.Day != this.canSeeRedDay)
			{
				yield return null;
			}
			Vector3 playerPos = this.yachtSpawn.transform.InverseTransformPoint(LocalPlayer.Transform.position);
			float playerAngle = Mathf.Atan2(playerPos.x, playerPos.z) * 57.29578f;
			float dist = Vector3.Distance(LocalPlayer.Transform.position, this.yachtSpawn.transform.position);
			if (dist < 390f && dist > 150f && !this.doneYacht)
			{
				this.redMan = (GameObject)UnityEngine.Object.Instantiate((GameObject)Resources.Load("CutScene/redman_cutscene_prefab"), this.yachtSpawn.transform.position, this.yachtSpawn.transform.rotation);
				this.redMan.transform.parent = this.yachtSpawn.transform;
				redmanSetup setup = this.redMan.GetComponent<redmanSetup>();
				this.animator = this.redMan.GetComponent<Animator>();
				this.animator.SetBool("yachtScene", true);
				setup.useIk = true;
				this.leaveTime = Time.time;
				this.doneYacht = true;
			}
			if (this.doneYacht && (dist < 145f || dist > 440f || Time.time > this.leaveTime + 100f))
			{
				this.animator.SetBool("yachtSceneLeave", true);
				base.StartCoroutine(this.removeRedman(this.redMan, 10f));
				yield break;
			}
			yield return YieldPresets.WaitTwoSeconds;
		}
		yield break;
		yield break;
	}

	
	private IEnumerator doCave1Spawn()
	{
		if (this.doneCave1)
		{
			yield break;
		}
		while (Clock.planecrash)
		{
			yield return null;
		}
		yield return YieldPresets.WaitTwentySeconds;
		for (;;)
		{
			while (Clock.Day != this.canSeeRedDay)
			{
				yield return null;
			}
			Vector3 playerPos = this.caveSpawn1.transform.InverseTransformPoint(LocalPlayer.Transform.position);
			float angle = Mathf.Atan2(playerPos.x, playerPos.z) * 57.29578f;
			float dist = Vector3.Distance(LocalPlayer.Transform.position, this.caveSpawn1.transform.position);
			if (dist < 100f && dist > 80f && angle > -60f && angle < 60f && !this.doneCave1)
			{
				break;
			}
			yield return YieldPresets.WaitTwoSeconds;
			yield return YieldPresets.WaitPointOneSeconds;
		}
		this.redMan = (GameObject)UnityEngine.Object.Instantiate((GameObject)Resources.Load("CutScene/redman_cutscene_prefab"), this.caveSpawn1.transform.position, this.caveSpawn1.transform.rotation);
		redmanSetup setup = this.redMan.GetComponent<redmanSetup>();
		this.animator = this.redMan.GetComponent<Animator>();
		this.animator.SetBool("caveWalk1", true);
		base.StartCoroutine(this.removeRedman(this.redMan, 10f));
		this.doneCave1 = true;
		yield break;
		yield break;
	}

	
	private IEnumerator doCave2Spawn()
	{
		if (this.doneCave2)
		{
			yield break;
		}
		while (Clock.planecrash)
		{
			yield return null;
		}
		yield return YieldPresets.WaitTwentySeconds;
		for (;;)
		{
			while (Clock.Day != this.canSeeRedDay)
			{
				yield return null;
			}
			Vector3 playerPos = this.caveSpawn2.transform.InverseTransformPoint(LocalPlayer.Transform.position);
			float angle = Mathf.Atan2(playerPos.x, playerPos.z) * 57.29578f;
			float dist = Vector3.Distance(LocalPlayer.Transform.position, this.caveSpawn2.transform.position);
			if (dist < 160f && dist > 100f && angle < 60f && angle > -60f && !this.doneCave2)
			{
				this.redMan = (GameObject)UnityEngine.Object.Instantiate((GameObject)Resources.Load("CutScene/redman_cutscene_prefab"), this.caveSpawn2.transform.position, this.caveSpawn2.transform.rotation);
				redmanSetup setup = this.redMan.GetComponent<redmanSetup>();
				this.redMan.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
				this.animator = this.redMan.GetComponent<Animator>();
				this.animator.SetBool("caveWalk2", true);
				this.leaveTime = Time.time;
				this.doneCave2 = true;
			}
			if (this.doneCave2 && (dist < 110f || dist > 200f || angle > 70f || angle < -70f || Time.time > this.leaveTime + 60f))
			{
				break;
			}
			yield return YieldPresets.WaitTwoSeconds;
			yield return YieldPresets.WaitPointOneSeconds;
		}
		this.animator.SetBool("toWalk", true);
		base.StartCoroutine(this.removeRedman(this.redMan, 10f));
		yield break;
		yield break;
	}

	
	private IEnumerator doCliffSpawn()
	{
		while (Clock.planecrash)
		{
			yield return null;
		}
		yield return YieldPresets.WaitTwentySeconds;
		for (;;)
		{
			while (Clock.Day != this.canSeeRedDay)
			{
				yield return null;
			}
			foreach (GameObject go in this.cliffSpawn)
			{
				Vector3 playerPos = go.transform.InverseTransformPoint(LocalPlayer.Transform.position);
				float angle = Mathf.Atan2(playerPos.x, playerPos.z) * 57.29578f;
				float dist = Vector3.Distance(LocalPlayer.Transform.position, go.transform.position);
				if (dist < 160f && dist > 110f && angle < 60f && angle > -60f && !this.doneCliffSpawn)
				{
					base.StartCoroutine("doRedmanOnCliff", go.transform);
					this.doneCliffSpawn = true;
				}
				yield return null;
			}
			yield return YieldPresets.WaitTwoSeconds;
			yield return YieldPresets.WaitPointTwoSeconds;
		}
		yield break;
	}

	
	private IEnumerator doRedmanOnCliff(Transform pos)
	{
		GameObject red = (GameObject)UnityEngine.Object.Instantiate((GameObject)Resources.Load("CutScene/redman_cutscene_prefab"), pos.position, pos.rotation);
		redmanSetup setup = red.GetComponent<redmanSetup>();
		red.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
		this.animator = red.GetComponent<Animator>();
		setup.alignHeightToTerrain = true;
		setup.useIk = true;
		this.animator.SetBool("caveWalk3", true);
		this.leaveTime = Time.time;
		for (;;)
		{
			Vector3 playerPos = pos.InverseTransformPoint(LocalPlayer.Transform.position);
			float angle = Mathf.Atan2(playerPos.x, playerPos.z) * 57.29578f;
			float dist = Vector3.Distance(LocalPlayer.Transform.position, pos.position);
			if (dist < 90f || dist > 250f || angle > 70f || angle < -70f || Time.time > this.leaveTime + 40f)
			{
				break;
			}
			yield return YieldPresets.WaitOneSecond;
		}
		this.animator.SetBool("toRun", true);
		base.StartCoroutine(this.removeRedman(red, 3f));
		yield break;
		yield break;
	}

	
	private IEnumerator removeRedman(GameObject go, float t)
	{
		yield return new WaitForSeconds(t);
		if (go)
		{
			UnityEngine.Object.Destroy(go);
		}
		this.doneCliffSpawn = false;
		this.canSeeRedDay = Clock.Day + UnityEngine.Random.Range(1, 4);
		EventRegistry.Player.Publish(TfEvent.StoryProgress, GameStats.StoryElements.RedManOnYacht);
		yield break;
	}

	
	public GameObject redMan;

	
	public GameObject yachtSpawn;

	
	public GameObject caveSpawn1;

	
	public GameObject caveSpawn2;

	
	public GameObject caveSpawn3;

	
	public GameObject[] cliffSpawn;

	
	public int canSeeRedDay;

	
	private float leaveTime;

	
	private Animator animator;

	
	[SerializeThis]
	public bool doneYacht;

	
	[SerializeThis]
	public bool doneCave1;

	
	[SerializeThis]
	public bool doneCave2;

	
	[SerializeThis]
	public bool doneCave3;

	
	public bool doneCliffSpawn;
}
