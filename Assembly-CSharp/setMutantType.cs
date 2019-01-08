using System;
using UnityEngine;

public class setMutantType : MonoBehaviour
{
	private void OnEnable()
	{
		if (this.Type == setMutantType.MutantType.MaleSkinnyLeader)
		{
			base.SendMessage("setSkinnyLeader");
			base.SendMessage("setMaleSkinny");
		}
		else
		{
			base.SendMessage("set" + this.Type.ToString());
		}
	}

	public setMutantType.MutantType Type;

	public enum MutantType
	{
		FemaleSkinny,
		MaleSkinny,
		Leader,
		MaleSkinnyLeader,
		Fireman,
		Pale,
		PaleLeader
	}
}
