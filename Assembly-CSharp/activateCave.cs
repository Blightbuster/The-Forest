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
		if (this.caveManager == null)
		{
			this.caveManager = base.transform.parent.GetComponentInChildren<caveEntranceManager>();
		}
		base.enabled = false;
	}

	private void GrabEnter(GameObject grabber)
	{
		base.enabled = true;
		this.ShowIcon(true, false);
	}

	private void ShowIcon(bool value, bool sheenValue)
	{
		if (Scene.HudGui.EnterCavesIcon != null)
		{
			Scene.HudGui.EnterCavesIcon.gameObject.SetActive(value);
			Scene.HudGui.EnterCavesIcon._target = ((!value) ? null : base.transform);
		}
	}

	private void GrabExit(GameObject grabber)
	{
		if (base.enabled)
		{
			base.enabled = false;
			this.ShowIcon(false, false);
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
			this.ShowIcon(false, true);
			return;
		}
		if (LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0).IsTag("enterCave") || LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2).IsTag("enterCave") || LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2).IsTag("explode") || LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2).IsTag("death") || LocalPlayer.AnimControl.knockedDown)
		{
			this.ShowIcon(false, false);
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
			LocalPlayer.SpecialActions.SendMessage("setIgnoreOcean", this.ignoreOceanTriggers);
			if (LocalPlayer.SavedData.ExitedEndgame)
			{
				this.goodbyeTimmyCutscene = false;
			}
			LocalPlayer.SpecialActions.SendMessage("setGoodbyeTimmyState", this.goodbyeTimmyCutscene);
			LocalPlayer.ActiveAreaInfo.SetCurrentCave(this.CaveNum);
			if (this.entry)
			{
				LocalPlayer.SpecialActions.SendMessage("enterCave", this.enterPos);
				if (this.caveManager)
				{
					base.StartCoroutine(this.caveManager.disableCaveBlackRoutine());
				}
			}
			else
			{
				LocalPlayer.SpecialActions.SendMessage("exitCave", this.exitPos);
				if (this.caveManager)
				{
					base.StartCoroutine(this.caveManager.enableCaveBlackRoutine());
				}
			}
			this.ShowIcon(false, false);
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

	public bool ignoreOceanTriggers;

	public bool goodbyeTimmyCutscene;

	public UnityEvent OnActivated;
}
