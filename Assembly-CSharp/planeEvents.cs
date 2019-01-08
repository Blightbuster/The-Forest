using System;
using FMOD.Studio;
using UnityEngine;

public class planeEvents : MonoBehaviour
{
	private void Awake()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	private void Start()
	{
		this.cutScene = this.pmCutScene.transform.GetComponent<TriggerCutScene>();
	}

	public void fallForward1()
	{
		if (this.seatPivot)
		{
			this.seatPivot.GetComponent<Animation>().Play("planeFallForward1");
		}
	}

	private void playHitGroundSFX()
	{
		if (FMOD_StudioSystem.instance)
		{
			if (this.cutScene.skipOpening)
			{
				this.hitGroundEvent = FMOD_StudioSystem.instance.PlayOneShot("event:/ambient/plane_start/hit_ground_skipped", Vector3.zero, null);
			}
			else
			{
				this.hitGroundEvent = FMOD_StudioSystem.instance.PlayOneShot("event:/ambient/plane_start/hit_ground", Vector3.zero, null);
			}
		}
	}

	private void hitGround()
	{
		if (this.cutScene.timmySleepGo)
		{
			this.cutScene.timmySleepGo.GetComponent<Animator>().SetBool("hitGround", true);
		}
		if (this.cutScene.playerSeatGo)
		{
			this.cutScene.playerSeatGo.GetComponent<Animator>().SetBool("hitGround", true);
		}
		if (BoltNetwork.isClient)
		{
			this.cutScene.clientPlayerOnPlaneGo.GetComponent<Animator>().SetBool("hitGround", true);
		}
	}

	private void crashStop()
	{
		if (this.cutScene.timmySleepGo)
		{
			this.cutScene.timmySleepGo.GetComponent<Animator>().SetBool("crashStop", true);
		}
		if (this.cutScene.playerSeatGo)
		{
			this.cutScene.playerSeatGo.GetComponent<Animator>().SetBool("crashStop", true);
		}
		if (BoltNetwork.isClient)
		{
			this.cutScene.clientPlayerOnPlaneGo.GetComponent<Animator>().SetBool("crashStop", true);
		}
		base.Invoke("goBlack", 0.4f);
	}

	private void enableGroundDebris()
	{
		if (this.cutScene.debrisInterior2)
		{
			this.cutScene.debrisInterior2.SetActive(true);
		}
	}

	private void fallForward2()
	{
	}

	private void goBlack()
	{
		this.pmCutScene.SendEvent("doAction");
	}

	public void stopFMODEvents()
	{
		FMODCommon.ReleaseIfValid(this.hitGroundEvent, STOP_MODE.ALLOWFADEOUT);
		this.hitGroundEvent = null;
	}

	private TriggerCutScene cutScene;

	public GameObject seatPivot;

	public GameObject timmyCrashGo;

	private Animator timmyAnim;

	public PlayMakerFSM pmCutScene;

	private EventInstance hitGroundEvent;
}
