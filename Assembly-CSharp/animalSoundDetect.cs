using System;
using TheForest.Utils;
using UnityEngine;

public class animalSoundDetect : MonoBehaviour
{
	private void Start()
	{
		this.checkEnemyTimer = Time.time + 2f;
		this.animControl = base.transform.root.GetComponentInChildren<animalAnimatorControl>();
		PlayMakerFSM[] components = base.transform.root.GetComponents<PlayMakerFSM>();
		foreach (PlayMakerFSM playMakerFSM in components)
		{
			if (playMakerFSM.FsmName == "aiBaseFSM")
			{
				this.pmBase = playMakerFSM;
			}
		}
	}

	private void Update()
	{
		if (Time.time > this.checkEnemyTimer)
		{
			for (int i = 0; i < Scene.MutantControler.ActiveWorldCannibals.Count; i++)
			{
				if (Scene.MutantControler.ActiveWorldCannibals[i] && Scene.MutantControler.ActiveWorldCannibals[i].activeSelf && (Scene.MutantControler.ActiveWorldCannibals[i].transform.position - base.transform.position).sqrMagnitude < 625f)
				{
					this.checkSoundAlert(Scene.MutantControler.ActiveWorldCannibals[i].transform.position);
				}
			}
			for (int j = 0; j < Scene.MutantControler.activeBabies.Count; j++)
			{
				if (Scene.MutantControler.activeBabies[j] && Scene.MutantControler.activeBabies[j].activeSelf && (Scene.MutantControler.activeBabies[j].transform.position - base.transform.position).sqrMagnitude < 625f)
				{
					this.checkSoundAlert(Scene.MutantControler.activeBabies[j].transform.position);
				}
			}
			this.checkEnemyTimer = Time.time + 2f;
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (!this.soundCoolDown && (other.gameObject.CompareTag("soundAlert") || other.gameObject.CompareTag("torchLight")))
		{
			this.checkSoundAlert(other.transform.position);
		}
	}

	private void checkSoundAlert(Vector3 pos)
	{
		if (!this.soundCoolDown)
		{
			if (this.animControl)
			{
				if ((this.animControl.typeLizard || this.animControl.typeRaccoon) && this.animControl.animator.GetBool("treeClimb"))
				{
					return;
				}
				if (this.animControl.animator.GetBool("trapped"))
				{
					return;
				}
			}
			this.lastSoundPosition = pos;
			this.pmBase.SendEvent("toSoundAlert");
			if (!this.soundCoolDown)
			{
				base.Invoke("resetCoolDown", 3f);
			}
			this.soundCoolDown = true;
		}
	}

	private void resetCoolDown()
	{
		this.soundCoolDown = false;
	}

	private animalAnimatorControl animControl;

	private bool soundCoolDown;

	public Vector3 lastSoundPosition;

	private PlayMakerFSM pmBase;

	private float checkEnemyTimer;
}
