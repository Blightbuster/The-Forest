using System;
using TheForest.Utils;
using UnityEngine;


public class enemyAvoidance : MonoBehaviour
{
	
	private void Start()
	{
		this.setup = base.transform.root.GetComponentInChildren<mutantScriptSetup>();
		this.animControl = base.transform.root.GetComponentInChildren<mutantAnimatorControl>();
		this.animator = this.animControl.transform.GetComponent<Animator>();
		this.rootTr = base.transform.root;
		base.Invoke("setStartUp", 1f);
	}

	
	private void setStartUp()
	{
		this.startUp = true;
	}

	
	private void Update()
	{
		if (this.setup.ai.mainPlayerDist < 10f)
		{
			GameObject closestPlayerFromPos = Scene.SceneTracker.GetClosestPlayerFromPos(this.rootTr.position);
			if (closestPlayerFromPos)
			{
				Vector3 position = closestPlayerFromPos.transform.position;
				position.y = this.rootTr.position.y;
				float num = Vector3.Distance(this.rootTr.position, position);
			}
		}
	}

	
	private mutantScriptSetup setup;

	
	private mutantAnimatorControl animControl;

	
	private Transform rootTr;

	
	private Animator animator;

	
	private bool startUp;
}
