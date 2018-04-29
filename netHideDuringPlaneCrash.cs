using System;
using TheForest.Utils;
using UnityEngine;


public class netHideDuringPlaneCrash : MonoBehaviour
{
	
	private void Start()
	{
		this.animator = base.transform.GetComponent<Animator>();
	}

	
	private void Update()
	{
		if (this.startedInPlane && Scene.TriggerCutScene.Hull && Vector3.Distance(Scene.TriggerCutScene.Hull.transform.position, base.transform.position) < 10f)
		{
			if (!this.timerBlock)
			{
				this.enableTime = Time.time + 5f;
				this.timerBlock = true;
			}
			this.setPlayerRenderers(false);
			this.currState2 = this.animator.GetCurrentAnimatorStateInfo(2);
			if (this.currState2.tagHash == this.getupHash || Time.time > this.enableTime)
			{
				this.setPlayerRenderers(true);
				this.startedInPlane = false;
				base.enabled = false;
			}
		}
	}

	
	public void setPlayerRenderers(bool onoff)
	{
		if (!onoff)
		{
			for (int i = 0; i < this.hideRenderers.Length; i++)
			{
				this.hideRenderers[i].enabled = false;
			}
			for (int j = 0; j < this.hideGo.Length; j++)
			{
				this.hideGo[j].SetActive(false);
			}
		}
		else
		{
			for (int k = 0; k < this.hideRenderers.Length; k++)
			{
				this.hideRenderers[k].enabled = true;
			}
			for (int l = 0; l < this.hideGo.Length; l++)
			{
				this.hideGo[l].SetActive(true);
			}
		}
	}

	
	public Renderer[] hideRenderers;

	
	public GameObject[] hideGo;

	
	private Animator animator;

	
	private AnimatorStateInfo currState2;

	
	private int getupHash = Animator.StringToHash("getup");

	
	private bool getupStarted;

	
	private bool timeout;

	
	public bool saveCheck;

	
	private float destroyTime = 4f;

	
	private float enableTime;

	
	private bool timerBlock;

	
	public bool startedInPlane;
}
