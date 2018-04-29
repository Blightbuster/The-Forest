using System;
using System.Collections;
using Bolt;
using UnityEngine;


public class CoopWorldCrates : EntityBehaviour<IWorldCrates>
{
	
	private void Awake()
	{
		CoopWorldCrates.Instance = this;
	}

	
	public override void Attached()
	{
		base.StartCoroutine(this.UpdateRoutine());
		base.state.AddCallback("Broken[]", new PropertyCallback(this.OnBroken));
	}

	
	private void OnBroken(IState _, string propertyPath, ArrayIndices arrayIndices)
	{
		for (int i = 0; i < this.Crates.Length; i++)
		{
			if (this.Crates[i] && base.state.Broken[i] == 1)
			{
				this.Crates[i].gameObject.SendMessage("ExplosionReal");
			}
		}
	}

	
	private IEnumerator UpdateRoutine()
	{
		for (;;)
		{
			if (this.entity.IsAttached())
			{
				for (int i = 0; i < this.Crates.Length; i++)
				{
					if (!this.Crates[i])
					{
						if (base.state.Broken[i] == 0)
						{
							BreakCrateEvent ev = BreakCrateEvent.Create(GlobalTargets.OnlyServer);
							ev.Index = i;
							ev.Send();
							this._lastSend = Time.time + 1f;
						}
					}
				}
			}
			yield return new WaitForSeconds(1f);
		}
		yield break;
	}

	
	[Header("World Creates (MAX 64 CURRENT)")]
	public BreakCrate[] Crates;

	
	public static CoopWorldCrates Instance;

	
	private float _lastSend;
}
