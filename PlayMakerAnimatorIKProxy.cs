using System;
using UnityEngine;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayMakerFSM))]
public class PlayMakerAnimatorIKProxy : MonoBehaviour
{
	
	
	
	public event Action<int> OnAnimatorIKEvent;

	
	private void OnAnimatorIK(int layerIndex)
	{
		if (this.OnAnimatorIKEvent != null)
		{
			this.OnAnimatorIKEvent(layerIndex);
		}
	}

	
	private Animator _animator;
}
