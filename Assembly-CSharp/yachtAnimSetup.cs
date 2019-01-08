using System;
using UnityEngine;

public class yachtAnimSetup : MonoBehaviour
{
	private void Start()
	{
		if (this.plankType)
		{
			base.Invoke("animSetupPlank", 2f);
		}
		else
		{
			base.Invoke("animSetup", 2f);
		}
	}

	private void OnEnable()
	{
		if (this.plankType)
		{
			this.animSetupPlank();
		}
		else
		{
			this.animSetup();
		}
	}

	private void animSetup()
	{
		base.transform.GetComponent<Animation>()["yachtWobble"].wrapMode = WrapMode.Loop;
		base.transform.GetComponent<Animation>()["yachtWobble"].speed = 1f;
		base.transform.GetComponent<Animation>().Play("yachtWobble", PlayMode.StopAll);
	}

	private void animSetupPlank()
	{
		base.transform.GetComponent<Animation>()["woodenPlankWobble"].wrapMode = WrapMode.Loop;
		base.transform.GetComponent<Animation>()["woodenPlankWobble"].speed = 2f;
		base.transform.GetComponent<Animation>().Play("woodenPlankWobble", PlayMode.StopAll);
	}

	public bool plankType;
}
