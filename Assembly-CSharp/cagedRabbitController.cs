using System;
using UnityEngine;

public class cagedRabbitController : MonoBehaviour
{
	private void OnEnable()
	{
		if (!this.animator)
		{
			this.animator = base.GetComponent<Animator>();
		}
		this.animator.SetInteger("idleType", UnityEngine.Random.Range(0, 3));
		this.changeIdle = Time.time + UnityEngine.Random.Range(5f, 10f);
		this.animator.speed = UnityEngine.Random.Range(0.7f, 1.4f);
	}

	private void Update()
	{
		if (Time.time > this.changeIdle)
		{
			this.animator.SetInteger("idleType", UnityEngine.Random.Range(0, 3));
			this.changeIdle = Time.time + UnityEngine.Random.Range(5f, 10f);
		}
	}

	private Animator animator;

	private float changeIdle;
}
