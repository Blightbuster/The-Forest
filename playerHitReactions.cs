using System;
using System.Collections;
using Bolt;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;


public class playerHitReactions : MonoBehaviour
{
	
	private void Awake()
	{
		this.camRotator = GameObject.FindWithTag("MainCamera").GetComponent<SimpleMouseRotator>();
		this.fps = LocalPlayer.FpCharacter;
		this.animator = base.transform.GetComponentInChildren<Animator>();
		this.setup = base.GetComponentInChildren<playerScriptSetup>();
		this.walkSpeed = this.fps.walkSpeed;
		this.runSpeed = this.fps.runSpeed;
		this.strafeSpeed = this.fps.strafeSpeed;
		this.camAnimation = this.camAnim.GetComponent<Animation>();
		this._rb = base.GetComponent<Rigidbody>();
	}

	
	public void doBirdOnHand()
	{
		if (LocalPlayer.Inventory.IsLeftHandEmpty() && !LocalPlayer.IsInCaves)
		{
			this.setup.pmControl.SendEvent("toOnHand");
		}
	}

	
	public void enableParryState()
	{
		this.kingHitBool = true;
		this.animator.SetBoolReflected("parryBool", true);
		if (this.camAnim)
		{
			this.camAnimation.Play("camShake1", PlayMode.StopAll);
		}
		base.Invoke("cancelParry", 0.5f);
		base.Invoke("disableParryState", 2.5f);
	}

	
	private void cancelParry()
	{
		this.animator.SetBoolReflected("parryBool", false);
	}

	
	private void disableParryState()
	{
		this.kingHitBool = false;
	}

	
	public void enableHitState()
	{
		if (ForestVR.Enabled)
		{
			return;
		}
		this.camAnimation.Play("camShake1", PlayMode.StopAll);
	}

	
	public void explodeForce(Transform pos)
	{
	}

	
	public IEnumerator enableExplodeCamera()
	{
		LocalPlayer.Transform.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.CamRotator.enabled = false;
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.AnimControl.useRootMotion = true;
		LocalPlayer.AnimControl.knockedDown = true;
		LocalPlayer.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
		LocalPlayer.CamFollowHead.lockYCam = true;
		float timer = 0f;
		while (timer < 0.5f)
		{
			LocalPlayer.CamFollowHead.transform.localEulerAngles = Vector3.zero;
			LocalPlayer.AnimControl.useRootMotion = true;
			LocalPlayer.MainRotator.enabled = false;
			LocalPlayer.CamRotator.enabled = false;
			LocalPlayer.Animator.SetLayerWeightReflected(2, 1f);
			timer += Time.deltaTime;
			LocalPlayer.Rigidbody.AddForce(LocalPlayer.Transform.forward * -8f, ForceMode.VelocityChange);
			yield return null;
		}
		AnimatorStateInfo currState2 = this.animator.GetCurrentAnimatorStateInfo(2);
		timer = 0f;
		while (currState2.tagHash == this.explodeHash)
		{
			if (timer < 0.25f)
			{
				LocalPlayer.Rigidbody.AddForce(LocalPlayer.Transform.forward * -8f, ForceMode.VelocityChange);
			}
			LocalPlayer.MainRotator.enabled = false;
			LocalPlayer.CamRotator.enabled = false;
			LocalPlayer.AnimControl.useRootMotion = true;
			currState2 = this.animator.GetCurrentAnimatorStateInfo(2);
			this.animator.SetLayerWeightReflected(4, 0f);
			timer += Time.deltaTime;
			yield return null;
		}
		LocalPlayer.AnimControl.StartCoroutine("smoothEnableLayerNew", 4);
		LocalPlayer.AnimControl.StartCoroutine("smoothDisableLayerNew", 2);
		LocalPlayer.AnimControl.knockedDown = false;
		LocalPlayer.CamFollowHead.lockYCam = false;
		LocalPlayer.CamRotator.resetOriginalRotation = true;
		LocalPlayer.MainRotator.resetOriginalRotation = true;
		LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
		LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
		LocalPlayer.MainRotator.enabled = true;
		LocalPlayer.FpCharacter.enabled = true;
		LocalPlayer.CamRotator.enabled = true;
		LocalPlayer.FpCharacter.CanJump = true;
		LocalPlayer.AnimControl.useRootMotion = false;
		LocalPlayer.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
		LocalPlayer.AnimControl.onRope = false;
		LocalPlayer.CamRotator.stopInput = false;
		if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Pause)
		{
			LocalPlayer.FpCharacter.Locked = false;
		}
		LocalPlayer.CamFollowHead.smoothLock = false;
		LocalPlayer.CamFollowHead.transform.localEulerAngles = Vector3.zero;
		LocalPlayer.Transform.localEulerAngles = new Vector3(0f, LocalPlayer.Transform.localEulerAngles.y, 0f);
		yield break;
	}

	
	public void disableExplodeCamera()
	{
	}

	
	public void enableExplodeShake(float dist)
	{
		if (ForestVR.Enabled)
		{
			return;
		}
		base.CancelInvoke("resetExplodeShake");
		this.doingExplodeShake = true;
		if (dist > 100f)
		{
			return;
		}
		float weight;
		if (dist < 20f)
		{
			weight = 1f;
		}
		else
		{
			weight = 20f / dist;
		}
		this.camAnimation["explodeShake"].layer = 1;
		this.camAnimation["noShake"].layer = 0;
		this.camAnimation.Play("noShake");
		this.camAnimation.Play("explodeShake", PlayMode.StopSameLayer);
		this.camAnimation["explodeShake"].weight = weight;
		base.Invoke("resetExplodeShake", 1.2f);
	}

	
	public void enableFootShake(float dist, float mag)
	{
		if (ForestVR.Enabled)
		{
			return;
		}
		base.CancelInvoke("resetExplodeShake");
		if (LocalPlayer.AnimControl.swimming)
		{
			return;
		}
		if (dist < 30f)
		{
			float weight = (1f - dist / 30f) * mag;
			this.camAnimation["explodeShake"].layer = 1;
			this.camAnimation["noShake"].layer = 0;
			this.camAnimation.Play("noShake");
			this.camAnimation.Play("explodeShake", PlayMode.StopSameLayer);
			this.camAnimation["explodeShake"].weight = weight;
			base.Invoke("resetExplodeShake", 1.2f);
			return;
		}
	}

	
	private void resetExplodeShake()
	{
		this.doingExplodeShake = false;
		this.camAnimation.Stop("noShake");
	}

	
	public void enableWeaponHitState()
	{
		if (ForestVR.Enabled)
		{
			return;
		}
		this.camAnimation.Play("camShake2", PlayMode.StopAll);
	}

	
	private void enableBlockState()
	{
	}

	
	public void disableBlockState()
	{
	}

	
	private void enableBlockHitState()
	{
	}

	
	private void disableBlockHitState()
	{
	}

	
	public void enableControllerFreeze()
	{
		this.fps.walkSpeed = 1.2f;
		this.fps.runSpeed = 1.2f;
		this.fps.strafeSpeed = 1.2f;
	}

	
	public IEnumerator doHardfallRoutine()
	{
		float timer = 0f;
		while (timer < 1f)
		{
			LocalPlayer.FpCharacter.clampInputVal = 0f;
			LocalPlayer.Rigidbody.velocity = Vector3.zero;
			timer += Time.deltaTime;
			yield return null;
		}
		LocalPlayer.FpCharacter.clampInputVal = 1f;
		yield break;
	}

	
	public IEnumerator setControllerSpeed(float speed)
	{
		this.fps.walkSpeed = speed;
		this.fps.runSpeed = speed;
		this.fps.strafeSpeed = speed;
		yield return null;
		yield break;
	}

	
	public void disableControllerFreeze()
	{
		this.fps.walkSpeed = this.walkSpeed;
		this.fps.runSpeed = this.runSpeed;
		this.fps.strafeSpeed = this.strafeSpeed;
		this.fps.hitByEnemy = false;
		LocalPlayer.Rigidbody.drag = 0f;
	}

	
	public void disableImpact(float interval)
	{
		LocalPlayer.Rigidbody.isKinematic = true;
		base.Invoke("enableImpact", interval);
	}

	
	public void enableImpact()
	{
		LocalPlayer.Rigidbody.isKinematic = false;
	}

	
	public IEnumerator enablePushBack(float strength)
	{
		float t = 0f;
		float max = 0.5f;
		float dropoff = 1f;
		while (t < max && this.fps.Grounded)
		{
			dropoff *= 1f - t * 2f;
			t += Time.deltaTime;
			yield return YieldPresets.WaitForFixedUpdate;
		}
		yield break;
	}

	
	private IEnumerator enableWeaponImpactState()
	{
		yield return YieldPresets.WaitPointFiveSeconds;
		yield break;
	}

	
	private IEnumerator enableTreeImpactState()
	{
		yield return YieldPresets.WaitPointThreeSeconds;
		yield break;
	}

	
	private IEnumerator enableWeaponBreakState()
	{
		yield return YieldPresets.WaitPointFiveSeconds;
		yield break;
	}

	
	private void setSeatedCam()
	{
		if (this.camRotator)
		{
			this.camRotator.enabled = true;
		}
		LocalPlayer.AnimControl.sitting = true;
	}

	
	public void lookAtExplosion(Vector3 pos)
	{
		if (LocalPlayer.AnimControl.onRope)
		{
			return;
		}
		Vector3 worldPosition = pos;
		worldPosition.y = base.transform.position.y;
		base.transform.LookAt(worldPosition, base.transform.up);
	}

	
	private IEnumerator enableMpRenderers()
	{
		yield return YieldPresets.WaitTwoSeconds;
		if (LocalPlayer.CamFollowHead.flying)
		{
			yield break;
		}
		syncPlayerRenderers ev = syncPlayerRenderers.Create(GlobalTargets.Everyone);
		ev.target = base.transform.GetComponent<BoltEntity>();
		ev.Send();
		yield return null;
		yield break;
	}

	
	private IEnumerator disableMpRenderers()
	{
		yield break;
	}

	
	private FirstPersonCharacter fps;

	
	private Animator animator;

	
	private playerScriptSetup setup;

	
	public GameObject camAnim;

	
	private Animation camAnimation;

	
	public Vector3 hitDir;

	
	public bool kingHitBool;

	
	private SimpleMouseRotator camRotator;

	
	private Rigidbody _rb;

	
	private int explodeHash = Animator.StringToHash("explode");

	
	private float walkSpeed;

	
	private float runSpeed;

	
	private float strafeSpeed;

	
	private bool doingExplodeShake;
}
