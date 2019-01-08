using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

public class setupFeeding : MonoBehaviour
{
	private void Awake()
	{
	}

	private void Start()
	{
		this.mutantControl = Scene.MutantControler;
		if (UnityEngine.Random.value < 0.5f && !BoltNetwork.isClient)
		{
			base.InvokeRepeating("startEatMeEvent", UnityEngine.Random.Range(0f, 3f), 15f);
			base.Invoke("stopEatEvent", 160f);
		}
	}

	public void startEatMeEvent()
	{
		base.StartCoroutine("sendEatMeEvent");
	}

	private void stopEatEvent()
	{
		base.CancelInvoke("startEatMeEvent");
		base.StopCoroutine("sendEatMeEvent");
		this.cancelEatMeEvent();
	}

	public IEnumerator sendEatMeEvent()
	{
		if (Scene.SceneTracker.GetClosestPlayerDistanceFromPos(base.transform.position) < 150f)
		{
			foreach (GameObject go in this.mutantControl.activeSkinnyCannibals)
			{
				if (go && (go.transform.position - base.transform.position).sqrMagnitude < 5625f)
				{
					if (go)
					{
						go.SendMessage("switchToEatMe", base.gameObject, SendMessageOptions.DontRequireReceiver);
					}
					yield return YieldPresets.WaitForFixedUpdate;
				}
			}
			foreach (GameObject go2 in this.mutantControl.activeInstantSpawnedCannibals)
			{
				if (go2 && (go2.transform.position - base.transform.position).sqrMagnitude < 5625f)
				{
					if (go2)
					{
						go2.SendMessage("switchToEatMe", base.gameObject, SendMessageOptions.DontRequireReceiver);
					}
					yield return YieldPresets.WaitForFixedUpdate;
				}
			}
		}
		yield return null;
		yield break;
	}

	public void cancelEatMeEvent()
	{
		if (!this.mutantControl)
		{
			return;
		}
		foreach (GameObject gameObject in this.mutantControl.activeSkinnyCannibals)
		{
			if (gameObject)
			{
				gameObject.SendMessage("cancelEatMe", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private void OnDestroy()
	{
		this.cancelEatMeEvent();
		base.CancelInvoke("startEatMeEvent");
		base.StopAllCoroutines();
	}

	private void OnDisable()
	{
		this.cancelEatMeEvent();
		base.CancelInvoke("startEatMeEvent");
		base.StopAllCoroutines();
	}

	public void Explosion(float dist)
	{
		if (this.destroyGo)
		{
			UnityEngine.Object.Destroy(this.destroyGo);
		}
	}

	private mutantController mutantControl;

	public GameObject destroyGo;
}
