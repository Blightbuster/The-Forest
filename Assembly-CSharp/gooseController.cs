using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;

public class gooseController : MonoBehaviour
{
	private void Start()
	{
		this.Atmos = Scene.Atmosphere;
		this.currentCamera = Camera.main;
		if (this.spawnAmount > 0)
		{
			base.StartCoroutine("spawnGeese");
		}
		if (this.debugGeese)
		{
			base.Invoke("doTakeOff", this.debugStartFlyTime);
		}
		else if (!base.IsInvoking("doTakeOff"))
		{
			base.Invoke("doTakeOff", (float)UnityEngine.Random.Range(60, 120));
		}
		if (!base.IsInvoking("updateGeese"))
		{
			base.InvokeRepeating("updateGeese", 1f, 5f);
		}
	}

	public void initFlying()
	{
		if (!this.initStart)
		{
			if (this.debugGeese)
			{
				base.Invoke("doTakeOff", this.debugOnWaterTime);
			}
			else
			{
				base.Invoke("doTakeOff", UnityEngine.Random.Range(120f, 200f));
			}
			this.initStart = true;
		}
	}

	private void startCoolDown()
	{
		this.initStart = false;
	}

	private void updateGeese()
	{
		if (LocalPlayer.IsInCaves && !this.spawnDisabled)
		{
			this.disableGeese();
			this.spawnDisabled = true;
		}
		else if (!LocalPlayer.IsInCaves && this.spawnDisabled)
		{
			base.StartCoroutine("spawnGeese");
			this.spawnDisabled = false;
		}
		if (this.spawnedGeese.Count < this.spawnAmount && !this.replenish && !LocalPlayer.IsInCaves)
		{
			this.spawnedGeese.RemoveAll((GameObject o) => o == null);
			if (this.spawnedGeese.Count > 0 && this.spawnedGeese[0] && !this.spawnedGeese[0].GetComponent<newGooseAi>().flying)
			{
				base.StartCoroutine("spawnSingleGoose");
				base.Invoke("spawnCoolDown", 50f);
				this.replenish = true;
			}
		}
	}

	private void spawnCoolDown()
	{
		this.replenish = false;
	}

	public void disableGeese()
	{
		this.spawnedGeese.RemoveAll((GameObject o) => o == null);
		foreach (GameObject gameObject in this.spawnedGeese)
		{
			if (gameObject)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
		base.CancelInvoke("doTakeOff");
		this.spawnedGeese.Clear();
		this.leader = null;
	}

	public IEnumerator resetLeader()
	{
		yield return YieldPresets.WaitForFixedUpdate;
		this.spawnedGeese.RemoveAll((GameObject o) => o == null);
		this.leader = this.spawnedGeese[UnityEngine.Random.Range(0, this.spawnedGeese.Count)].transform;
		this.leader.SendMessage("setAsLeader");
		yield return YieldPresets.WaitForFixedUpdate;
		foreach (GameObject gameObject in this.spawnedGeese)
		{
			gameObject.SendMessage("setNewLeader");
		}
		yield break;
	}

	private void resetTakeOff()
	{
		this.takeOff = false;
	}

	private void doTakeOff()
	{
		for (int i = 0; i < this.spawnedGeese.Count; i++)
		{
			if (this.spawnedGeese[i])
			{
				this.spawnedGeese[i].SendMessage("startFlying");
			}
		}
		this.initStart = false;
	}

	private IEnumerator spawnGeese()
	{
		int totalGeese = 0;
		while (totalGeese < this.spawnAmount)
		{
			Vector3 spawnPos = this.RandomSpawnPos((float)UnityEngine.Random.Range(5, 20));
			spawnPos = new Vector3(spawnPos.x + base.transform.position.x, base.transform.position.y, spawnPos.y + base.transform.position.z);
			GameObject spawn = UnityEngine.Object.Instantiate<GameObject>(this.goose, spawnPos, base.transform.rotation);
			if (spawn)
			{
				spawn.transform.eulerAngles = new Vector3(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
				spawn.name += totalGeese;
				this.spawnedGeese.Add(spawn.gameObject);
				yield return YieldPresets.WaitForFixedUpdate;
				spawn.SendMessage("startGoose", base.gameObject, SendMessageOptions.DontRequireReceiver);
				totalGeese++;
			}
			if (!base.IsInvoking("doTakeOff"))
			{
				base.Invoke("doTakeOff", (float)UnityEngine.Random.Range(60, 120));
			}
		}
		yield return null;
		yield break;
	}

	private IEnumerator spawnSingleGoose()
	{
		Vector3 spawnPos = this.RandomSpawnPos((float)UnityEngine.Random.Range(5, 20));
		Transform spawnAt = null;
		if (this.spawnedGeese.Count > 0)
		{
			spawnAt = this.spawnedGeese[UnityEngine.Random.Range(0, this.spawnedGeese.Count)].transform;
			spawnPos = spawnAt.position;
		}
		else
		{
			spawnPos = new Vector3(spawnPos.x + base.transform.position.x, base.transform.position.y, spawnPos.y + base.transform.position.z);
		}
		GameObject spawn = UnityEngine.Object.Instantiate<GameObject>(this.goose, spawnPos, spawnAt.rotation);
		if (spawn)
		{
			spawn.name += this.spawnedGeese.Count + 2;
			this.spawnedGeese.Add(spawn.gameObject);
			yield return YieldPresets.WaitForFixedUpdate;
			spawn.SendMessage("startGoose", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
		yield return null;
		yield break;
	}

	private bool pointOffCamera(Vector3 pos)
	{
		this.screenPos = this.currentCamera.WorldToViewportPoint(pos);
		return this.screenPos.x < 0f || this.screenPos.x > 1f || this.screenPos.y < 0f || this.screenPos.y > 1f;
	}

	private Vector2 RandomSpawnPos(float radius)
	{
		Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
		insideUnitCircle.Normalize();
		return insideUnitCircle * radius;
	}

	public TheForestAtmosphere Atmos;

	public GameObject goose;

	public int spawnAmount;

	public Transform[] lakes;

	public Transform[] landingPoints;

	public Transform leader;

	public Transform currLandingPoint;

	public Vector3 landingPos;

	private bool takeOff;

	public bool initStart;

	public bool spawnDisabled;

	public bool replenish;

	private bool geeseOnWater;

	public List<GameObject> spawnedGeese = new List<GameObject>();

	public bool debugGeese;

	public float debugStartFlyTime;

	public float debugFlyTime;

	public float debugOnWaterTime;

	public GameObject forceLandingPoint;

	private Camera currentCamera;

	private Vector3 screenPos;
}
