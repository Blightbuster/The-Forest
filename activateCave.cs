using System;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Events;


public class activateCave : MonoBehaviour
{
	
	private void Start()
	{
		if (base.transform.parent)
		{
			this.caveManager = base.transform.parent.GetComponent<caveEntranceManager>();
		}
		base.enabled = false;
	}

	
	private void GrabEnter(GameObject grabber)
	{
		base.enabled = true;
		this.Sheen.SetActive(false);
		this.MyPickUp.SetActive(true);
	}

	
	private void GrabExit(GameObject grabber)
	{
		if (base.enabled)
		{
			base.enabled = false;
			this.MyPickUp.SetActive(false);
		}
	}

	
	private void Update()
	{
		if (!this.ignoreLightingSwitch)
		{
			if (this.entry && LocalPlayer.IsInCaves)
			{
				return;
			}
			if (!this.entry && !LocalPlayer.IsInCaves)
			{
				return;
			}
		}
		if (this.swimCave && !LocalPlayer.AnimControl.swimming)
		{
			this.Sheen.SetActive(true);
			this.MyPickUp.SetActive(false);
			return;
		}
		if (LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0).IsTag("enterCave") || LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2).IsTag("enterCave") || LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2).IsTag("explode") || LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2).IsTag("death") || LocalPlayer.AnimControl.knockedDown)
		{
			this.Sheen.SetActive(false);
			this.MyPickUp.SetActive(false);
			return;
		}
		if (LocalPlayer.FpCharacter.PushingSled)
		{
			return;
		}
		if (TheForest.Utils.Input.GetButtonDown("Take"))
		{
			LocalPlayer.SpecialActions.SendMessage("setLightingSwitch", this.ignoreLightingSwitch);
			LocalPlayer.SpecialActions.SendMessage("setSwimCave", this.swimCave);
			if (LocalPlayer.SavedData.ExitedEndgame)
			{
				this.goodbyeTimmyCutscene = false;
			}
			LocalPlayer.SpecialActions.SendMessage("setGoodbyeTimmyState", this.goodbyeTimmyCutscene);
			LocalPlayer.ActiveAreaInfo.SetCurrentCave(this.CaveNum);
			if (this.entry)
			{
				LocalPlayer.SpecialActions.SendMessage("enterCave", this.enterPos);
				if (!this.swimCave && this.caveManager)
				{
					base.StartCoroutine(this.caveManager.disableCaveBlackRoutine());
				}
			}
			else
			{
				LocalPlayer.SpecialActions.SendMessage("exitCave", this.exitPos);
				if (!this.swimCave && this.caveManager)
				{
					base.StartCoroutine(this.caveManager.enableCaveBlackRoutine());
				}
			}
			this.Sheen.SetActive(false);
			this.MyPickUp.SetActive(false);
			this.OnActivated.Invoke();
		}
	}

	
	private caveEntranceManager caveManager;

	
	public GameObject Sheen;

	
	public GameObject MyPickUp;

	
	public GameObject enterPos;

	
	public GameObject exitPos;

	
	public CaveNames CaveNum = CaveNames.NotInCaves;

	
	public bool entry;

	
	public bool ignoreLightingSwitch;

	
	public bool swimCave;

	
	public bool goodbyeTimmyCutscene;

	
	public UnityEvent OnActivated;
}
