using System;
using UnityEngine;


public class GunShoot : MonoBehaviour
{
	
	private void Start()
	{
		this.anim = base.GetComponent<Animator>();
		this.gunAim = base.GetComponentInParent<GunAim>();
	}

	
	private void Update()
	{
		if (Input.GetButtonDown("Fire1") && Time.time > this.nextFire && !this.gunAim.GetIsOutOfBounds())
		{
			this.nextFire = Time.time + this.fireRate;
			this.muzzleFlash.Play();
			this.cartridgeEjection.Play();
			this.anim.SetTrigger("Fire");
			Vector3 position = this.gunEnd.position;
			RaycastHit hit;
			if (Physics.Raycast(position, this.gunEnd.forward, out hit, this.weaponRange))
			{
				this.HandleHit(hit);
			}
		}
	}

	
	private void HandleHit(RaycastHit hit)
	{
		if (hit.collider.sharedMaterial != null)
		{
			string name = hit.collider.sharedMaterial.name;
			switch (name)
			{
			case "Metal":
				this.SpawnDecal(hit, this.metalHitEffect);
				break;
			case "Sand":
				this.SpawnDecal(hit, this.sandHitEffect);
				break;
			case "Stone":
				this.SpawnDecal(hit, this.stoneHitEffect);
				break;
			case "WaterFilled":
				this.SpawnDecal(hit, this.waterLeakEffect);
				this.SpawnDecal(hit, this.metalHitEffect);
				break;
			case "Wood":
				this.SpawnDecal(hit, this.woodHitEffect);
				break;
			case "Meat":
				this.SpawnDecal(hit, this.fleshHitEffects[UnityEngine.Random.Range(0, this.fleshHitEffects.Length)]);
				break;
			case "Character":
				this.SpawnDecal(hit, this.fleshHitEffects[UnityEngine.Random.Range(0, this.fleshHitEffects.Length)]);
				break;
			}
		}
	}

	
	private void SpawnDecal(RaycastHit hit, GameObject prefab)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, hit.point, Quaternion.LookRotation(hit.normal));
		gameObject.transform.SetParent(hit.collider.transform);
	}

	
	public float fireRate = 0.25f;

	
	public float weaponRange = 20f;

	
	public Transform gunEnd;

	
	public ParticleSystem muzzleFlash;

	
	public ParticleSystem cartridgeEjection;

	
	public GameObject metalHitEffect;

	
	public GameObject sandHitEffect;

	
	public GameObject stoneHitEffect;

	
	public GameObject waterLeakEffect;

	
	public GameObject[] fleshHitEffects;

	
	public GameObject woodHitEffect;

	
	private float nextFire;

	
	private Animator anim;

	
	private GunAim gunAim;
}
