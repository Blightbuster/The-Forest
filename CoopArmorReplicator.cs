using System;
using Bolt;
using UnityEngine;


public class CoopArmorReplicator : EntityBehaviour<IPlayerState>
{
	
	public override void Attached()
	{
		if (!this.entity.isOwner)
		{
			base.state.AddCallback("Armors[]", new PropertyCallbackSimple(this.ArmorsChanged));
			base.state.AddCallback("Leaves[]", new PropertyCallbackSimple(this.LeavesChanged));
			base.state.AddCallback("Bones[]", new PropertyCallbackSimple(this.BonesChanged));
			base.state.AddCallback("Rebreater", new PropertyCallbackSimple(this.ReBreatherChanged));
		}
	}

	
	private void ReBreatherChanged()
	{
		this.ReBreather.SetActive(base.state.Rebreater);
	}

	
	private void LeavesChanged()
	{
		for (int i = 0; i < Mathf.Min(base.state.Leaves.Length, this.Leaves.Length); i++)
		{
			if (this.Leaves[i])
			{
				this.Leaves[i].SetActive(base.state.Leaves[i] == 1);
			}
		}
	}

	
	private void BonesChanged()
	{
		for (int i = 0; i < Mathf.Min(base.state.Bones.Length, this.Bones.Length); i++)
		{
			if (this.Bones[i])
			{
				this.Bones[i].SetActive(base.state.Bones[i] == 1);
			}
		}
	}

	
	private void ArmorsChanged()
	{
		for (int i = 0; i < Mathf.Min(base.state.Armors.Length, this.Armors.Length); i++)
		{
			if (this.Armors[i])
			{
				if (base.state.Armors[i] > 0)
				{
					this.Armors[i].SetActive(true);
					this.Armors[i].GetComponent<Renderer>().sharedMaterial = this.ArmorMaterials[base.state.Armors[i] - 1];
				}
				else
				{
					this.Armors[i].SetActive(false);
				}
			}
		}
	}

	
	private void Update()
	{
		if (BoltNetwork.isRunning && this.entity && this.entity.isAttached && this.entity.isOwner)
		{
			base.state.Rebreater = this.ReBreather.activeInHierarchy;
			for (int i = 0; i < Mathf.Min(base.state.Leaves.Length, this.Leaves.Length); i++)
			{
				if (this.Leaves[i])
				{
					int num = (!this.Leaves[i].activeInHierarchy) ? 0 : 1;
					if (num != base.state.Leaves[i])
					{
						base.state.Leaves[i] = num;
					}
				}
			}
			for (int j = 0; j < Mathf.Min(base.state.Bones.Length, this.Bones.Length); j++)
			{
				if (this.Bones[j])
				{
					int num2 = (!this.Bones[j].activeInHierarchy) ? 0 : 1;
					if (num2 != base.state.Bones[j])
					{
						base.state.Bones[j] = num2;
					}
				}
			}
			for (int k = 0; k < Mathf.Min(base.state.Armors.Length, this.Armors.Length); k++)
			{
				if (this.Armors[k])
				{
					int num3 = 0;
					if (this.Armors[k].activeInHierarchy)
					{
						for (int l = 0; l < this.ArmorMaterials.Length; l++)
						{
							if (object.ReferenceEquals(this.ArmorMaterials[l], this.Armors[k].GetComponent<Renderer>().sharedMaterial))
							{
								num3 = l + 1;
								break;
							}
						}
					}
					if (num3 != base.state.Armors[k])
					{
						base.state.Armors[k] = num3;
					}
				}
			}
		}
	}

	
	public Material[] ArmorMaterials;

	
	public GameObject[] Armors;

	
	public GameObject[] Leaves;

	
	public GameObject[] Bones;

	
	public GameObject ReBreather;
}
