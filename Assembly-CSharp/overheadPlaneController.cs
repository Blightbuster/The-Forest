using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

public class overheadPlaneController : MonoBehaviour
{
	private void Start()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		this.dayCounter = Clock.Day;
		base.Invoke("setupOverheadPlane", UnityEngine.Random.Range(this.spawnRateMin, this.spawnRateMax));
	}

	private void Update()
	{
		if (!LocalPlayer.GameObject)
		{
			return;
		}
		Vector3 position = base.transform.position;
		position.x = LocalPlayer.Transform.position.x;
		position.z = LocalPlayer.Transform.position.z;
		base.transform.position = position;
	}

	private void setupOverheadPlane()
	{
		if (this.dayCounter == Clock.Day)
		{
			this.dayCounter++;
			if (UnityEngine.Random.value > 0.6f)
			{
				this.canFlyPlaneDay = true;
			}
			else
			{
				this.canFlyPlaneDay = false;
			}
		}
		if (this.ignoreDayCheck)
		{
			this.canFlyPlaneDay = true;
		}
		if (!LocalPlayer.IsInCaves && !Clock.planecrash && this.canFlyPlaneDay && !LocalPlayer.IsInEndgame && !LocalPlayer.AnimControl.endGameCutScene)
		{
			Vector2 vector = this.pointOnCircle(2500f);
			Vector3 vector2 = new Vector3(vector.x, 1700f, vector.y);
			Vector2 vector3 = this.pointOnCircle(350f);
			Vector3 vector4 = LocalPlayer.Transform.position;
			vector4.x += vector3.x;
			vector4.z += vector3.y;
			vector4.y = vector2.y;
			vector4 = (vector4 - vector2).normalized;
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.planeGo, vector2, Quaternion.LookRotation(vector4));
			gameObject.transform.parent = base.transform;
			base.StartCoroutine(this.overheadPlaneMove(gameObject, vector4));
		}
		base.Invoke("setupOverheadPlane", UnityEngine.Random.Range(this.spawnRateMin, this.spawnRateMax));
	}

	private IEnumerator overheadPlaneMove(GameObject plane, Vector3 dir)
	{
		Vector3 midPoint = new Vector3(0f, 1700f, 0f);
		for (;;)
		{
			if (LocalPlayer.GameObject)
			{
				plane.transform.position = plane.transform.position + dir * this.speed;
				if (Vector3.Distance(midPoint, plane.transform.position) > 5500f)
				{
					break;
				}
			}
			yield return new WaitForFixedUpdate();
		}
		UnityEngine.Object.Destroy(plane);
		yield break;
		yield break;
	}

	private Vector2 pointOnCircle(float radius)
	{
		Vector2 normalized = UnityEngine.Random.insideUnitCircle.normalized;
		return normalized * radius;
	}

	public GameObject planeGo;

	public float speed;

	public float spawnRateMin;

	public float spawnRateMax;

	public bool ignoreDayCheck;

	private int dayCounter;

	private bool canFlyPlaneDay;
}
