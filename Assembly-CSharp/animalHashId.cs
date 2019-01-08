using System;
using UnityEngine;

public class animalHashId : MonoBehaviour
{
	private void Start()
	{
		PlayMakerFSM[] components = base.gameObject.GetComponents<PlayMakerFSM>();
		foreach (PlayMakerFSM playMakerFSM in components)
		{
			if (playMakerFSM.FsmName == "aiBaseFSM")
			{
				this.pm = playMakerFSM;
			}
		}
		this.pm.FsmVariables.GetFsmInt("HashOnTree").Value = Animator.StringToHash("onTree");
	}

	private PlayMakerFSM pm;

	public int onTree;
}
