using System;
using Bolt;
using UnityEngine;


public class CoopSpearedFish : EntityBehaviour<IPlayerState>
{
	
	private void Awake()
	{
		if (!BoltNetwork.isRunning)
		{
			base.enabled = false;
		}
	}

	
	private void Update()
	{
		if (base.entity && base.entity.isAttached)
		{
			if (base.entity.isOwner)
			{
				if (this._weapon.MyFish && this._weapon.MyFish.gameObject.activeSelf)
				{
					base.state.SpearedFishType = this._weapon.MyFish.fishPrefabTypeInt * this._fishDefinitions.Length + this._weapon.MyFish.fishTypeInt;
				}
				else
				{
					base.state.SpearedFishType = -1;
				}
			}
			else
			{
				bool flag = base.state.SpearedFishType > -1;
				if (flag)
				{
					int num = base.state.SpearedFishType / this._fishDefinitions.Length;
					int num2 = base.state.SpearedFishType % this._fishDefinitions.Length;
					for (int i = 0; i < this._fishDefinitions.Length; i++)
					{
						bool flag2 = num == i;
						CoopSpearedFish.FishDefinition fishDefinition = this._fishDefinitions[i];
						if (fishDefinition._go.activeSelf != flag2)
						{
							fishDefinition._go.SetActive(flag2);
							if (flag2)
							{
								if (fishDefinition._fishSource.fishTypeMat.Length > 0)
								{
									fishDefinition._renderer.sharedMaterial = fishDefinition._fishSource.fishTypeMat[num2];
								}
								if (fishDefinition._fishSource.fishTypeMesh.Length > 0)
								{
									fishDefinition._renderer.sharedMesh = fishDefinition._fishSource.fishTypeMesh[num2];
								}
								if (fishDefinition._anim)
								{
									fishDefinition._anim.SetBoolReflected("Dead", true);
								}
							}
						}
					}
				}
				else
				{
					for (int j = 0; j < this._fishDefinitions.Length; j++)
					{
						CoopSpearedFish.FishDefinition fishDefinition2 = this._fishDefinitions[j];
						if (fishDefinition2._go.activeSelf)
						{
							fishDefinition2._go.SetActive(false);
						}
					}
				}
			}
		}
	}

	
	public override void Attached()
	{
		if (base.entity.isOwner)
		{
			base.state.SpearedFishType = -1;
		}
		base.enabled = true;
	}

	
	public weaponInfo _weapon;

	
	public CoopSpearedFish.FishDefinition[] _fishDefinitions;

	
	[Serializable]
	public class FishDefinition
	{
		
		public GameObject _go;

		
		public SkinnedMeshRenderer _renderer;

		
		public Animator _anim;

		
		public Fish _fishSource;
	}
}
