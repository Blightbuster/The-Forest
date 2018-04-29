using System;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;


public class activateTimmyPickup : MonoBehaviour
{
	
	private void Start()
	{
		base.enabled = false;
	}

	
	private void GrabEnter(GameObject grabber)
	{
		if (this.pickup)
		{
			base.enabled = false;
			return;
		}
		base.enabled = true;
		if (this.Sheen)
		{
			this.Sheen.SetActive(false);
		}
		if (this.MyPickUp)
		{
			this.MyPickUp.SetActive(true);
		}
	}

	
	private void GrabExit(GameObject grabber)
	{
		if (this.pickup)
		{
			base.enabled = false;
			return;
		}
		if (this.Sheen)
		{
			this.Sheen.SetActive(true);
		}
		if (this.MyPickUp)
		{
			this.MyPickUp.SetActive(false);
		}
		base.enabled = false;
	}

	
	private void Update()
	{
		if (TheForest.Utils.Input.GetButtonAfterDelay("Take", this.delay, false) && !this.pickup && !this.pickup)
		{
			LocalPlayer.SpecialActions.SendMessage("setSpectator", false, SendMessageOptions.DontRequireReceiver);
			if (this.sequence)
			{
				this.sequence.BeginStage(0);
			}
			else
			{
				this.DoActorAnimation();
			}
		}
	}

	
	public void DoActorAnimation()
	{
		LocalPlayer.SpecialActions.SendMessage("setSpectator", false, SendMessageOptions.DontRequireReceiver);
		this.InitAnim();
	}

	
	public void DoSpectatorAnimation()
	{
		if (this.sequence.Proxy.state.Actor)
		{
			Animator componentInChildren = this.sequence.Proxy.state.Actor.GetComponentInChildren<Animator>();
			LocalPlayer.SpecialActions.SendMessage("setActorAnimator", componentInChildren, SendMessageOptions.DontRequireReceiver);
		}
		if (LocalPlayer.SpecialActions)
		{
			LocalPlayer.SpecialActions.SendMessage("setSpectator", true, SendMessageOptions.DontRequireReceiver);
		}
		this.InitAnim();
	}

	
	private void InitAnim()
	{
		if (LocalPlayer.SpecialActions)
		{
			LocalPlayer.SpecialActions.SendMessage("sendTimmyPickup", this.timmyGo);
			LocalPlayer.SpecialActions.SendMessage("sendCablesAnimator", this.cablesController);
			LocalPlayer.SpecialActions.SendMessage("sendMachineAnimator", this.machineController);
			LocalPlayer.SpecialActions.SendMessage("setToy", this.placedToyGo);
			LocalPlayer.SpecialActions.SendMessage("setArtifactAudioState", this.audioState);
			LocalPlayer.SpecialActions.SendMessage("pickupTimmyRoutine", this.markTr);
			LocalPlayer.SpecialActions.SendMessage("setScreensGo", this.screensGo);
			LocalPlayer.SpecialActions.SendMessage("setGirlTrigger", this.girlTrigger);
			LocalPlayer.SpecialActions.SendMessage("setCurrentSequence", this.sequence);
		}
		this.pickup = true;
		if (this.Sheen)
		{
			this.Sheen.SetActive(false);
		}
		if (this.MyPickUp)
		{
			this.MyPickUp.SetActive(false);
		}
		base.gameObject.SetActive(false);
	}

	
	public void DoProgressTick()
	{
		this.doneProgressTicks++;
	}

	
	public void DoLateCompletion()
	{
		Animator component = this.timmyGo.GetComponent<Animator>();
		component.CrossFade("pickupToMachine", 0f, 0, 0.87f);
		this.cablesController.CrossFade("cableSequence", 0f, 0, 1f);
		this.machineController.CrossFade("Base Layer.endMachineSequence", 0f, 0, 1f);
		component.enabled = true;
		this.cablesController.enabled = true;
		this.machineController.enabled = true;
		this.pickup = true;
		if (this.Sheen)
		{
			this.Sheen.SetActive(false);
		}
		if (this.MyPickUp)
		{
			this.MyPickUp.SetActive(false);
		}
		base.gameObject.SetActive(false);
		this.girlTrigger.SetActive(true);
		this.placedToyGo.SetActive(true);
		for (int i = this.doneProgressTicks; i < 5; i++)
		{
			foreach (object obj in this.screensGo.transform)
			{
				Transform transform = (Transform)obj;
				transform.SendMessage("CheckNextImage", SendMessageOptions.DontRequireReceiver);
			}
		}
		base.Invoke("PublishTimmyFoundEvent", 1f);
	}

	
	private void PublishTimmyFoundEvent()
	{
		EventRegistry.Player.Publish(TfEvent.StoryProgress, GameStats.StoryElements.TimmyFound);
	}

	
	public GameObject Sheen;

	
	public GameObject MyPickUp;

	
	public Transform markTr;

	
	public GameObject timmyGo;

	
	public GameObject placedToyGo;

	
	public GameObject screensGo;

	
	public GameObject girlTrigger;

	
	public ArtifactAudioState audioState;

	
	public Animator machineController;

	
	public Animator cablesController;

	
	public bool pickup;

	
	public float delay = 0.5f;

	
	public AnimationSequence sequence;

	
	private int doneProgressTicks;
}
