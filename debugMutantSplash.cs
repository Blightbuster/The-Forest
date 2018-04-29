using System;
using PathologicalGames;
using UnityEngine;


public class debugMutantSplash : MonoBehaviour
{
	
	private void Start()
	{
		this.animator = base.transform.GetComponent<Animator>();
	}

	
	private void Update()
	{
		this.animator.SetFloat("Speed", this.speed);
	}

	
	private void tempLeftWalkSplash()
	{
		Vector3 position = this.leftFoot.position;
		position.y += 0.5f;
		PoolManager.Pools["Particles"].Spawn(this.waterSplashWalk.transform, position, Quaternion.identity);
	}

	
	private void tempRightWalkSplash()
	{
		Vector3 position = this.rightFoot.position;
		position.y += 0.5f;
		PoolManager.Pools["Particles"].Spawn(this.waterSplashWalk.transform, position, Quaternion.identity);
	}

	
	private void tempLeftRunSplash()
	{
		Vector3 position = this.leftFoot.position;
		position.y += 0.5f;
		PoolManager.Pools["Particles"].Spawn(this.waterSplashRun.transform, position, Quaternion.identity);
	}

	
	private void tempRightRunSplash()
	{
		Vector3 position = this.rightFoot.position;
		position.y += 0.5f;
		PoolManager.Pools["Particles"].Spawn(this.waterSplashRun.transform, position, Quaternion.identity);
	}

	
	public Transform leftFoot;

	
	public Transform rightFoot;

	
	public GameObject waterSplashWalk;

	
	public GameObject waterSplashRun;

	
	public float speed;

	
	private Animator animator;
}
