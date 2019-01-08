using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

public class weaponFireParticleController : MonoBehaviour
{
	private void Start()
	{
		if (BoltNetwork.isRunning)
		{
			BoltEntity componentInParent = base.transform.GetComponentInParent<BoltEntity>();
			if (componentInParent != null && componentInParent.isAttached && !componentInParent.isOwner)
			{
				this.netFire = true;
			}
		}
		if (base.transform.parent)
		{
			CoopWeaponFire component = base.transform.parent.GetComponent<CoopWeaponFire>();
			if (component)
			{
				component.spawnedFireTransform = base.transform;
			}
		}
		if (this.weaponFireGo && this.weaponFireGo.transform.parent)
		{
			alignWeaponFire componentInChildren = this.weaponFireGo.transform.parent.GetComponentInChildren<alignWeaponFire>(true);
			if (componentInChildren)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		this.weaponFollowTr = base.transform.parent.parent;
		if (!this.netFire)
		{
			base.transform.parent = null;
		}
		this.fp = base.transform.GetComponent<FireParticle>();
		if (this.fp)
		{
			this.fp.MyFuel = 1000f;
		}
		this.p = base.transform.GetComponent<ParticleSystem>();
		this.lastPos = base.transform.position;
		this.particles = new ParticleSystem.Particle[this.p.particleCount];
		if (!this.netFire)
		{
			base.transform.localScale = new Vector3(1f, 1f, 1f);
		}
		this.smoothPos = base.transform.position;
	}

	private void LateUpdate()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			return;
		}
		if (this.weaponFireGo == null && !this.doTimeout)
		{
			base.StartCoroutine(this.removeParticlerRoutine());
			this.doTimeout = true;
		}
		if (this.weaponFollowTr != null)
		{
			base.transform.position = this.weaponFollowTr.position;
			base.transform.rotation = this.weaponFollowTr.rotation;
		}
		if (this.netFire)
		{
			if (this.weaponFollowTr != null && !this.weaponFollowTr.gameObject.activeInHierarchy)
			{
				base.StopAllCoroutines();
				UnityEngine.Object.Destroy(base.gameObject);
			}
			return;
		}
		if (LocalPlayer.AnimControl.enteringACave)
		{
			if (this.fp.enabled)
			{
				this.fp.enabled = false;
				ParticleSystem component = base.transform.GetComponent<ParticleSystem>();
				if (component)
				{
					component.Stop();
				}
				if (this.weaponFollowTr)
				{
					base.transform.position = Vector3.zero;
				}
			}
		}
		else if (!this.fp.enabled)
		{
			this.fp.enabled = true;
			ParticleSystem component2 = base.transform.GetComponent<ParticleSystem>();
			if (component2)
			{
				component2.Play();
			}
		}
		if (this.weaponFollowTr != null && !this.weaponFollowTr.gameObject.activeInHierarchy)
		{
			base.StopAllCoroutines();
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator removeParticlerRoutine()
	{
		ParticleSystem ps = base.transform.GetComponent<ParticleSystem>();
		if (ps)
		{
			if (ps.main.duration < 1f)
			{
				yield break;
			}
			ps.Stop();
			ps.main.duration = 0.1f;
			ps.Play();
		}
		yield return YieldPresets.WaitFourSeconds;
		if (base.gameObject != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		yield return null;
		yield break;
	}

	public GameObject weaponFireGo;

	public Transform weaponFollowTr;

	private FireParticle fp;

	public float driftAmount = 1f;

	public float localMagMult = 4f;

	public bool player;

	private bool doTimeout;

	private Vector3 smoothPos;

	private Vector3 lastPos;

	private ParticleSystem p;

	private ParticleSystem.Particle[] particles;

	public bool netFire;
}
