using System;
using System.Collections.Generic;
using UnityEngine;

public class CoopMecanimReplicatorTransitionData : ScriptableObject
{
	public void Init()
	{
		this.Lookup = new Dictionary<int, Dictionary<bool, float>>();
		foreach (CoopMecanimReplicatorTransitionData.TransitionData transitionData in this.Data)
		{
			if (!this.Lookup.ContainsKey(transitionData.HashFrom - transitionData.HashTo))
			{
				Dictionary<bool, float> dictionary;
				this.Lookup.Add(transitionData.HashFrom - transitionData.HashTo, dictionary = new Dictionary<bool, float>());
				dictionary.Add(transitionData.fixedDuration, transitionData.Duration);
			}
		}
	}

	public Dictionary<int, Dictionary<bool, float>> Lookup;

	[SerializeField]
	public CoopMecanimReplicatorTransitionData.TransitionData[] Data;

	[Serializable]
	public class TransitionData
	{
		public int HashFrom;

		public int HashTo;

		public float Duration;

		public bool fixedDuration;
	}
}
