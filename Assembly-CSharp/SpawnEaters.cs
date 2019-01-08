using System;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;

public class SpawnEaters : MonoBehaviour
{
	private void Start()
	{
		int num = UnityEngine.Random.Range(1, this.spots.Length + 1);
		if (LocalPlayer.AnimControl.swimming)
		{
			LocalPlayer.Animator.CrossFade("fullBodyActions.deathFallForward", 0f, 2, 1f);
			return;
		}
		int num2 = 0;
		while (num-- > 0)
		{
			if (this.spots[num2].childCount == 0)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.enemyPrefab);
				gameObject.transform.parent = this.spots[num2];
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				Animator component = gameObject.GetComponent<Animator>();
				component.CrossFade("Base Layer.feeding1", 0f, 0, UnityEngine.Random.Range(0f, 1f));
				gameObject.transform.parent = null;
				this.enemies.Add(gameObject);
				num2++;
			}
		}
	}

	private void OnDestroy()
	{
		foreach (GameObject obj in this.enemies)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.enemies.Clear();
	}

	public Transform[] spots;

	public GameObject enemyPrefab;

	public List<GameObject> enemies = new List<GameObject>();

	private int deadHash = Animator.StringToHash("deathFallForward");
}
