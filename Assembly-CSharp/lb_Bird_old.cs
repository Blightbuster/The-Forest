using System;
using System.Collections;
using UnityEngine;

public class lb_Bird_old : MonoBehaviour
{
	private void Awake()
	{
		this.anim = base.gameObject.GetComponent<Animator>();
		this.ragDoll = base.GetComponent<clsragdollify>();
	}

	private void OnEnable()
	{
		this.idleAnimationHash = Animator.StringToHash("Base Layer.Idle");
		this.flyTagHash = Animator.StringToHash("flying");
		this.hopIntHash = Animator.StringToHash("hop");
		this.flyingBoolHash = Animator.StringToHash("flying");
		this.peckBoolHash = Animator.StringToHash("peck");
		this.ruffleBoolHash = Animator.StringToHash("ruffle");
		this.preenBoolHash = Animator.StringToHash("preen");
		this.landingBoolHash = Animator.StringToHash("landing");
		this.singTriggerHash = Animator.StringToHash("sing");
		this.flyingDirectionHash = Animator.StringToHash("flyingDirectionX");
		this.anim.SetFloatReflected("IdleAgitated", this.agitationLevel);
	}

	private void PauseBird()
	{
		this.originalAnimSpeed = this.anim.speed;
		this.anim.speed = 0f;
		if (!base.GetComponent<Rigidbody>().isKinematic)
		{
			this.originalVelocity = base.GetComponent<Rigidbody>().velocity;
		}
		base.GetComponent<Rigidbody>().isKinematic = true;
		base.GetComponent<AudioSource>().Stop();
		this.paused = true;
	}

	private void UnPauseBird()
	{
		this.anim.speed = this.originalAnimSpeed;
		base.GetComponent<Rigidbody>().isKinematic = false;
		base.GetComponent<Rigidbody>().velocity = this.originalVelocity;
		this.paused = false;
	}

	private void DoSmoothLookAt(Vector3 target)
	{
		Vector3 normalized = (target - base.transform.position).normalized;
		if (normalized != Vector3.zero && normalized.sqrMagnitude > 0f)
		{
			this.desiredRotation = Quaternion.LookRotation(normalized, Vector3.up);
		}
		this.lastRotation = Quaternion.Slerp(this.lastRotation, this.desiredRotation, 3f * Time.deltaTime);
		base.transform.rotation = this.lastRotation;
	}

	private IEnumerator FlyToTarget(Vector3 target)
	{
		if ((double)UnityEngine.Random.value < 0.5)
		{
			base.GetComponent<AudioSource>().volume = 0.2f;
			base.GetComponent<AudioSource>().PlayOneShot(this.flyAway1, 0.3f);
		}
		else
		{
			base.GetComponent<AudioSource>().volume = 0.2f;
			base.GetComponent<AudioSource>().PlayOneShot(this.flyAway2, 0.3f);
		}
		this.flying = true;
		this.landing = false;
		this.onGround = false;
		base.GetComponent<Rigidbody>().isKinematic = false;
		base.GetComponent<Rigidbody>().velocity = Vector3.zero;
		base.GetComponent<Rigidbody>().drag = this.controller.drag;
		this.anim.applyRootMotion = false;
		this.anim.SetBool(this.flyingBoolHash, true);
		this.anim.SetBool(this.landingBoolHash, false);
		while (this.anim.GetCurrentAnimatorStateInfo(0).tagHash != this.flyTagHash)
		{
			yield return null;
		}
		base.GetComponent<Rigidbody>().AddForce(base.transform.forward * this.controller.flySpeed + base.transform.up * this.controller.upSpeed);
		float t = 0f;
		while (t < 1f)
		{
			if (!this.paused)
			{
				t += Time.deltaTime;
			}
			yield return null;
		}
		Quaternion finalRotation = Quaternion.identity;
		Quaternion startingRotation = base.transform.rotation;
		this.distanceToTarget = Vector3.Distance(base.transform.position, target);
		t = 0f;
		while (base.transform.rotation != finalRotation || this.distanceToTarget >= 1.5f)
		{
			if (!this.paused)
			{
				this.distanceToTarget = Vector3.Distance(base.transform.position, target);
				Vector3 vectorDirectionToTarget = (target - base.transform.position).normalized;
				if (vectorDirectionToTarget != Vector3.zero && vectorDirectionToTarget.sqrMagnitude > 0f)
				{
					finalRotation = Quaternion.LookRotation(vectorDirectionToTarget);
					base.transform.rotation = Quaternion.Slerp(startingRotation, finalRotation, t);
				}
				this.anim.SetFloatReflected(this.flyingDirectionHash, this.FindBankingAngle(base.transform.forward, vectorDirectionToTarget));
				t += Time.deltaTime * 0.5f;
				base.GetComponent<Rigidbody>().AddForce(base.transform.forward * this.controller.flySpeed * Time.deltaTime);
				if (Physics.Raycast(base.transform.position, -Vector3.up, 0.15f) && base.GetComponent<Rigidbody>().velocity.y < 0f)
				{
					base.GetComponent<Rigidbody>().velocity = new Vector3(base.GetComponent<Rigidbody>().velocity.x, 0f, base.GetComponent<Rigidbody>().velocity.z);
				}
			}
			yield return null;
		}
		float flyingForce = this.controller.flySpeed;
		for (;;)
		{
			if (!this.paused)
			{
				if (Physics.Raycast(base.transform.position, -Vector3.up, 0.15f) && base.GetComponent<Rigidbody>().velocity.y < 0f)
				{
					base.GetComponent<Rigidbody>().velocity = new Vector3(base.GetComponent<Rigidbody>().velocity.x, 0f, base.GetComponent<Rigidbody>().velocity.z);
				}
				Vector3 vectorDirectionToTarget = (target - base.transform.position).normalized;
				finalRotation = Quaternion.LookRotation(vectorDirectionToTarget);
				this.anim.SetFloatReflected(this.flyingDirectionHash, this.FindBankingAngle(base.transform.forward, vectorDirectionToTarget));
				base.transform.rotation = finalRotation;
				base.GetComponent<Rigidbody>().AddForce(base.transform.forward * flyingForce * Time.deltaTime);
				this.distanceToTarget = Vector3.Distance(base.transform.position, target);
				if (this.distanceToTarget <= 15f)
				{
					if (this.distanceToTarget < 6f)
					{
						break;
					}
					base.GetComponent<Rigidbody>().drag = this.controller.landDrag;
					flyingForce = this.controller.landSpeed;
				}
			}
			yield return null;
		}
		this.anim.SetFloatReflected(this.flyingDirectionHash, 0f);
		Vector3 vel = Vector3.zero;
		this.flying = false;
		this.landing = true;
		this.anim.SetBool(this.landingBoolHash, true);
		this.anim.SetBool(this.flyingBoolHash, false);
		t = 0f;
		base.GetComponent<Rigidbody>().velocity = Vector3.zero;
		Collider[] hitColliders = Physics.OverlapSphere(target, 0.2f);
		for (int i = 0; i < hitColliders.Length; i++)
		{
			if (hitColliders[i].tag == "lb_bird" && hitColliders[i].transform != base.transform)
			{
				hitColliders[i].SendMessage("FlyAway");
			}
		}
		startingRotation = base.transform.rotation;
		base.transform.localEulerAngles = new Vector3(0f, base.transform.localEulerAngles.y, 0f);
		finalRotation = base.transform.rotation;
		base.transform.rotation = startingRotation;
		while (this.distanceToTarget > 0.05f)
		{
			this.distanceToTarget = Vector3.Distance(base.transform.position, target);
			if (!this.paused)
			{
				base.transform.rotation = Quaternion.Slerp(startingRotation, finalRotation, t);
				base.transform.position = Vector3.SmoothDamp(base.transform.position, target, ref vel, 0.5f);
				t += Time.deltaTime * 2f;
			}
			base.transform.rotation = finalRotation;
			yield return null;
		}
		base.GetComponent<Rigidbody>().drag = this.controller.drag;
		base.GetComponent<Rigidbody>().velocity = Vector3.zero;
		this.anim.SetBool(this.landingBoolHash, false);
		this.landing = false;
		base.transform.localEulerAngles = new Vector3(0f, base.transform.localEulerAngles.y, 0f);
		base.transform.position = target;
		this.anim.applyRootMotion = true;
		this.onGround = true;
		yield break;
	}

	private float FindBankingAngle(Vector3 birdForward, Vector3 dirToTarget)
	{
		Vector3 lhs = Vector3.Cross(birdForward, dirToTarget);
		return Vector3.Dot(lhs, Vector3.up);
	}

	private void OnGroundBehaviors()
	{
		this.idle = (this.anim.GetCurrentAnimatorStateInfo(0).nameHash == this.idleAnimationHash);
		if (!base.GetComponent<Rigidbody>().isKinematic)
		{
			base.GetComponent<Rigidbody>().isKinematic = true;
		}
		if (this.idle)
		{
			if (!this.songBreak && this.song1.Length > 0)
			{
				this.PlaySong();
			}
			base.GetComponent<AudioSource>().volume = 0.7f;
			if ((double)UnityEngine.Random.value < (double)Time.deltaTime * 0.33)
			{
				float value = UnityEngine.Random.value;
				if ((double)value < 0.3)
				{
					this.DisplayBehavior(lb_Bird_old.birdBehaviors.sing);
				}
				else if ((double)value < 0.5)
				{
					this.DisplayBehavior(lb_Bird_old.birdBehaviors.peck);
				}
				else if ((double)value < 0.6)
				{
					this.DisplayBehavior(lb_Bird_old.birdBehaviors.preen);
				}
				else if (!this.perched && (double)value < 0.7)
				{
					this.DisplayBehavior(lb_Bird_old.birdBehaviors.ruffle);
				}
				else if (!this.perched && (double)value < 0.85)
				{
					this.DisplayBehavior(lb_Bird_old.birdBehaviors.hopForward);
				}
				else if (!this.perched && (double)value < 0.9)
				{
					this.DisplayBehavior(lb_Bird_old.birdBehaviors.hopLeft);
				}
				else if (!this.perched && (double)value < 0.95)
				{
					this.DisplayBehavior(lb_Bird_old.birdBehaviors.hopRight);
				}
				else if (!this.perched && value <= 1f)
				{
					this.DisplayBehavior(lb_Bird_old.birdBehaviors.hopBackward);
				}
				else
				{
					this.DisplayBehavior(lb_Bird_old.birdBehaviors.sing);
				}
				this.anim.SetFloatReflected("IdleAgitated", UnityEngine.Random.value);
			}
			if ((double)UnityEngine.Random.value < (double)Time.deltaTime * 0.1)
			{
				this.FlyAway();
			}
		}
	}

	private void DisplayBehavior(lb_Bird_old.birdBehaviors behavior)
	{
		this.idle = false;
		switch (behavior)
		{
		case lb_Bird_old.birdBehaviors.sing:
			this.ResetHopInt();
			this.anim.SetTrigger(this.singTriggerHash);
			break;
		case lb_Bird_old.birdBehaviors.preen:
			this.ResetHopInt();
			this.anim.SetTrigger(this.preenBoolHash);
			break;
		case lb_Bird_old.birdBehaviors.ruffle:
			this.ResetHopInt();
			this.anim.SetTrigger(this.ruffleBoolHash);
			break;
		case lb_Bird_old.birdBehaviors.peck:
			this.ResetHopInt();
			this.anim.SetTrigger(this.peckBoolHash);
			break;
		case lb_Bird_old.birdBehaviors.hopForward:
			this.anim.SetInteger(this.hopIntHash, 1);
			base.Invoke("ResetHopInt", UnityEngine.Random.Range(0.5f, 1f));
			break;
		case lb_Bird_old.birdBehaviors.hopBackward:
			this.anim.SetInteger(this.hopIntHash, -1);
			base.Invoke("ResetHopInt", UnityEngine.Random.Range(0.5f, 1f));
			break;
		case lb_Bird_old.birdBehaviors.hopLeft:
			this.anim.SetInteger(this.hopIntHash, -2);
			base.Invoke("ResetHopInt", UnityEngine.Random.Range(0.5f, 1f));
			break;
		case lb_Bird_old.birdBehaviors.hopRight:
			this.anim.SetInteger(this.hopIntHash, 2);
			base.Invoke("ResetHopInt", UnityEngine.Random.Range(0.5f, 1f));
			break;
		}
	}

	private void OnTriggerEnter(Collider col)
	{
		if (col.tag == "lb_bird")
		{
			this.FlyAway();
		}
		if ((col.CompareTag("soundAlert") || col.CompareTag("enemyRoot")) && Vector3.Distance(col.transform.position, base.transform.position) < 12f)
		{
			this.Flee();
			base.Invoke("AbortFlyToTarget", UnityEngine.Random.Range(5f, 7f));
		}
	}

	private void OnTriggerExit(Collider col)
	{
		if (this.onGround && (col.tag == "lb_groundTarget" || col.tag == "lb_perchTarget"))
		{
			this.FlyAway();
		}
	}

	public void Burn()
	{
		this.DieFire();
	}

	private void AbortFlyToTarget()
	{
		base.StopCoroutine("FlyToTarget");
		this.anim.SetBool(this.landingBoolHash, false);
		this.anim.SetFloatReflected(this.flyingDirectionHash, 0f);
		base.transform.localEulerAngles = new Vector3(0f, base.transform.localEulerAngles.y, 0f);
		this.FlyAway();
	}

	private void FlyAway()
	{
		base.StopCoroutine("FlyToTarget");
		this.anim.SetBool(this.landingBoolHash, false);
		this.controller.SendMessage("BirdFindTarget", base.gameObject);
		this.FeatherDice = UnityEngine.Random.Range(0, 10);
		if (this.FeatherDice == 1)
		{
			this.SpawnAFeather();
		}
	}

	private void SpawnAFeather()
	{
		UnityEngine.Object.Instantiate<GameObject>(this.MyFeather, base.transform.position, base.transform.rotation);
	}

	private void Hit(int damage)
	{
		this.health -= damage;
		if (this.health < 1)
		{
			this.die();
		}
	}

	private void die()
	{
		base.StopAllCoroutines();
		this.ragDoll.metgoragdoll(default(Vector3));
		this.controller.Unspawn(base.gameObject);
	}

	private void DieFire()
	{
		base.StopAllCoroutines();
		this.ragDoll.burning = true;
		this.ragDoll.metgoragdoll(default(Vector3));
		this.controller.Unspawn(base.gameObject);
	}

	private void Flee()
	{
		base.StopCoroutine("FlyToTarget");
		base.GetComponent<AudioSource>().Stop();
		Vector3 vector = base.transform.position;
		vector += new Vector3((float)UnityEngine.Random.Range(-50, 50), 30f, (float)UnityEngine.Random.Range(-50, 50));
		base.StartCoroutine("FlyToTarget", vector);
	}

	private void SetController(lb_BirdController cont)
	{
		this.controller = cont;
	}

	private void ResetHopInt()
	{
		this.anim.SetInteger(this.hopIntHash, 0);
	}

	private void ResetFlyingLandingVariables()
	{
		if (this.flying || this.landing)
		{
			this.flying = false;
			this.landing = false;
		}
	}

	private void PlaySong()
	{
		if (UnityEngine.Random.value < 0.005f)
		{
			base.GetComponent<AudioSource>().PlayOneShot(this.song1[UnityEngine.Random.Range(0, this.song1.Length)], 0.5f);
			this.songBreak = true;
			base.Invoke("resetSongBreak", 12f);
		}
	}

	private void resetSongBreak()
	{
		this.songBreak = false;
	}

	private void Update()
	{
		if (this.onGround && !this.paused)
		{
			this.OnGroundBehaviors();
		}
	}

	public AudioClip[] song1;

	public AudioClip flyAway1;

	public AudioClip flyAway2;

	public bool typeSparrow;

	public bool typeSeagull;

	public GameObject MyFeather;

	public int health;

	private bool songBreak;

	private Animator anim;

	private lb_BirdController controller;

	private clsragdollify ragDoll;

	private bool paused;

	private bool idle = true;

	private bool flying;

	private bool landing;

	private bool perched;

	private bool onGround = true;

	private float distanceToTarget;

	private float agitationLevel = 0.5f;

	private float originalAnimSpeed = 1f;

	private Quaternion lastRotation;

	private Quaternion desiredRotation;

	private Vector3 originalVelocity = Vector3.zero;

	private int idleAnimationHash;

	private int singAnimationHash;

	private int ruffleAnimationHash;

	private int preenAnimationHash;

	private int peckAnimationHash;

	private int hopForwardAnimationHash;

	private int hopBackwardAnimationHash;

	private int hopLeftAnimationHash;

	private int hopRightAnimationHash;

	private int worriedAnimationHash;

	private int landingAnimationHash;

	private int flyTagHash;

	private int hopIntHash;

	private int flyingBoolHash;

	private int peckBoolHash;

	private int ruffleBoolHash;

	private int preenBoolHash;

	private int landingBoolHash;

	private int singTriggerHash;

	private int flyingDirectionHash;

	private int FeatherDice;

	private enum birdBehaviors
	{
		sing,
		preen,
		ruffle,
		peck,
		hopForward,
		hopBackward,
		hopLeft,
		hopRight
	}
}
