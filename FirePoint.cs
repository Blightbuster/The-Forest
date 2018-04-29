using System;
using System.Collections;
using PathologicalGames;
using UnityEngine;


public class FirePoint : MonoBehaviour
{
	
	private void Start()
	{
		this.fuel = this.startingFuel;
	}

	
	private void OnDestroy()
	{
		if (this.fireStarted)
		{
			this.endFire();
		}
	}

	
	private void Burn()
	{
		this.startFire();
	}

	
	private void Update()
	{
		if (this.fireStarted)
		{
			this.fuel -= this.fuelConsumption * Time.deltaTime;
			if (Time.time - this.fTime >= this.spreadTime && !this.fSpread)
			{
				this.spreadFire();
				if (UnityEngine.Random.value > 0.95f && base.transform.root && base.transform.root.GetComponent<Renderer>() && !this.bColor)
				{
					this.bColor = true;
				}
			}
			if (!this.fellOut)
			{
				if (UnityEngine.Random.value > 1f - this.fallOutC)
				{
					this.fallOut();
				}
				this.fellOut = true;
			}
		}
		if (this.fuel <= 0f && this.fireStarted)
		{
			this.endFire();
		}
		if (this.bColor)
		{
			Color color = base.transform.root.GetComponent<Renderer>().material.color;
			color.r -= 0.01f * Time.deltaTime;
			if (color.r < 0.1f)
			{
				color.r = 0.1f;
				this.bColor = false;
			}
			color.g = color.r;
			color.b = color.r;
			base.transform.root.GetComponent<Renderer>().material.color = color;
		}
	}

	
	public void startFire()
	{
		if (!this.fireStarted)
		{
			this.fuel = 50f;
			this.fireStarted = true;
			foreach (Transform transform in this.firePoints)
			{
				Transform transform2 = PoolManager.Pools["Particles"].Spawn(this.fireParticle.GetComponent<ParticleSystem>(), transform.position, Quaternion.identity).transform;
				this.fireC[this.count] = transform2;
				this.count++;
			}
			this.fTime = Time.time;
		}
	}

	
	public void endFire()
	{
		foreach (Transform transform in this.fireC)
		{
			if (transform.GetComponent<ParticleSystem>())
			{
				transform.GetComponent<ParticleSystem>().enableEmission = false;
			}
			IEnumerator enumerator = transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform2 = (Transform)obj;
					if (transform2.GetComponent<ParticleSystem>())
					{
						transform2.GetComponent<ParticleSystem>().enableEmission = false;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}
		this.fireStarted = false;
		foreach (Transform transform3 in this.firePoints)
		{
			UnityEngine.Object.Destroy(transform3.gameObject, 5f);
		}
		UnityEngine.Object.Destroy(this);
	}

	
	public void Awake()
	{
		this.fuel = 50f;
		this.fireC = new Transform[this.firePoints.Length];
		this.setWind();
	}

	
	public IEnumerator spreadFire()
	{
		this.fSpread = true;
		Collider[] InRange = Physics.OverlapSphere(this.spPos, this.fireSpread);
		foreach (Collider all in InRange)
		{
			yield return new WaitForSeconds(UnityEngine.Random.value * this.randomRange);
			if (all.CompareTag("fire"))
			{
				all.SendMessage("startFire");
			}
		}
		yield break;
	}

	
	public void fallOut()
	{
	}

	
	public IEnumerator setWind()
	{
		yield return YieldPresets.WaitPointOneSeconds;
		switch (WindControl.windV)
		{
		case 0:
			this.spPos = base.transform.position;
			break;
		case 1:
			this.spPos = base.transform.position + new Vector3(0f, 0f, this.fireSpread - this.fireSpread / (1f + 2f * (WindControl.windS + 0.001f)));
			break;
		case 2:
			this.spPos = base.transform.position - new Vector3(this.fireSpread - this.fireSpread / (1f + 2f * (WindControl.windS + 0.001f)), 0f, 0f);
			break;
		case 3:
			this.spPos = base.transform.position + new Vector3(this.fireSpread - this.fireSpread / (1f + 2f * (WindControl.windS + 0.001f)), 0f, 0f);
			break;
		case 4:
			this.spPos = base.transform.position - new Vector3(0f, 0f, this.fireSpread - this.fireSpread / (1f + 2f * (WindControl.windS + 0.001f)));
			break;
		}
		yield break;
	}

	
	public Transform fireParticle;

	
	public Transform[] firePoints;

	
	public float startingFuel = 100f;

	
	public float fuel;

	
	public float fuelConsumption = 30f;

	
	public float fireSpread = 2f;

	
	public float spreadTime = 3f;

	
	public float randomRange = 1.2f;

	
	public float fallOutC = 0.05f;

	
	private bool fireStarted;

	
	private Transform[] fireC;

	
	private int count;

	
	private float fTime;

	
	private bool fSpread;

	
	private bool fellOut;

	
	private bool bColor;

	
	private Vector3 spPos = Vector3.zero;
}
