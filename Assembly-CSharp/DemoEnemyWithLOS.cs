using System;
using System.Collections;
using PathologicalGames;
using UnityEngine;

public class DemoEnemyWithLOS : MonoBehaviour
{
	private void Awake()
	{
		this.startingColor = base.GetComponent<Renderer>().material.color;
		Targetable component = base.GetComponent<Targetable>();
		component.AddOnDetectedDelegate(new Targetable.OnDetectedDelegate(this.OnDetected));
		component.AddOnNotDetectedDelegate(new Targetable.OnNotDetectedDelegate(this.OnNotDetected));
		component.AddOnHitColliderDelegate(new Targetable.OnHitColliderDelegate(this.OnHit));
	}

	private void OnHit(HitEffectList effects, Target target, Collider other)
	{
		if (this.isDead)
		{
			return;
		}
		if (other != null)
		{
			Debug.Log(base.name + " was hit by collider on " + other.name);
		}
		foreach (HitEffect hitEffect in effects)
		{
			if (hitEffect.name == "Damage")
			{
				this.life -= (int)hitEffect.value;
			}
		}
		if (this.life <= 0)
		{
			this.isDead = true;
			UnityEngine.Object.Instantiate<GameObject>(this.explosion.gameObject, base.transform.position, base.transform.rotation);
			base.gameObject.SetActive(false);
		}
	}

	private void OnDetected(TargetTracker source)
	{
		base.StartCoroutine(this.UpdateStartWhileDetected(source));
	}

	private void OnNotDetected(TargetTracker source)
	{
		base.StopAllCoroutines();
		this.ResetStates();
	}

	private void ResetStates()
	{
		base.transform.localScale = Vector3.one;
		base.GetComponent<Renderer>().material.color = this.startingColor;
	}

	private IEnumerator UpdateStartWhileDetected(TargetTracker source)
	{
		for (;;)
		{
			if (this.isDead)
			{
				yield return null;
			}
			if (source.targets.Contains(new Target(base.transform, source)))
			{
				base.transform.localScale = new Vector3(2f, 2f, 2f);
				base.GetComponent<Renderer>().material.color = Color.green;
			}
			else
			{
				this.ResetStates();
			}
			yield return null;
		}
		yield break;
	}

	public int life = 100;

	public ParticleSystem explosion;

	private Color startingColor;

	private bool isDead;
}
