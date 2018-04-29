using System;
using UnityEngine;


[RequireComponent(typeof(PlayMakerFSM))]
[RequireComponent(typeof(Animator))]
public class PlayMakerAnimatorMoveProxy : MonoBehaviour
{
	
	
	
	public event Action OnAnimatorMoveEvent;

	
	private void Start()
	{
	}

	
	private void Update()
	{
	}

	
	private void OnAnimatorMove()
	{
		if (this.OnAnimatorMoveEvent != null)
		{
			this.OnAnimatorMoveEvent();
		}
	}
}
