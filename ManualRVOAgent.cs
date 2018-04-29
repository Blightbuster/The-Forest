using System;
using Pathfinding.RVO;
using UnityEngine;


[RequireComponent(typeof(RVOController))]
public class ManualRVOAgent : MonoBehaviour
{
	
	private void Awake()
	{
		this.rvo = base.GetComponent<RVOController>();
	}

	
	private void Update()
	{
		float axis = Input.GetAxis("Horizontal");
		float axis2 = Input.GetAxis("Vertical");
		Vector3 vector = new Vector3(axis, 0f, axis2) * this.speed;
		this.rvo.ForceSetVelocity(vector);
		base.transform.position += vector * Time.deltaTime;
	}

	
	private RVOController rvo;

	
	public float speed = 1f;
}
