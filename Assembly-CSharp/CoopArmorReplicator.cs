using System;
using Bolt;
using UnityEngine;

public class CoopArmorReplicator : EntityBehaviour<IPlayerState>
{
	public override void Attached()
	{
		if (!base.entity.isOwner)
		{
			base.state.AddCallback("Armors[]", new PropertyCallbackSimple(this.ArmorsChanged));
			base.state.AddCallback("Leaves[]", delegate
			{
				this.AnyArmorChanged(this.Leaves, base.state.Leaves);
			});
			base.state.AddCallback("Bones[]", delegate
			{
				this.AnyArmorChanged(this.Bones, base.state.Bones);
			});
			base.state.AddCallback("Creepy[]", delegate
			{
				this.AnyArmorChanged(this.Creepy, base.state.Creepy);
			});
			base.state.AddCallback("Rebreater", new PropertyCallbackSimple(this.ReBreatherChanged));
			base.state.AddCallback("Warmsuit", new PropertyCallbackSimple(this.WarmsuitChanged));
		}
	}

	private void ReBreatherChanged()
	{
		this.ReBreather.SetActive(base.state.Rebreater);
	}

	private void WarmsuitChanged()
	{
		this.Warmsuit.SetActive(base.state.Warmsuit);
	}

	private void AnyArmorChanged(GameObject[] armorType, NetworkArray_Integer stateArray)
	{
		for (int i = 0; i < Mathf.Min(stateArray.Length, armorType.Length); i++)
		{
			if (armorType[i])
			{
				armorType[i].SetActive(stateArray[i] == 1);
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
		if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner)
		{
			if (base.state.Rebreater != this.ReBreather.activeInHierarchy)
			{
				base.state.Rebreater = this.ReBreather.activeInHierarchy;
			}
			if (base.state.Warmsuit != this.Warmsuit.activeInHierarchy)
			{
				base.state.Warmsuit = this.Warmsuit.activeInHierarchy;
			}
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
			for (int k = 0; k < Mathf.Min(base.state.Creepy.Length, this.Creepy.Length); k++)
			{
				if (this.Creepy[k])
				{
					int num3 = (!this.Creepy[k].activeInHierarchy) ? 0 : 1;
					if (num3 != base.state.Creepy[k])
					{
						base.state.Creepy[k] = num3;
					}
				}
			}
			for (int l = 0; l < Mathf.Min(base.state.Armors.Length, this.Armors.Length); l++)
			{
				if (this.Armors[l])
				{
					int num4 = 0;
					if (this.Armors[l].activeInHierarchy)
					{
						for (int m = 0; m < this.ArmorMaterials.Length; m++)
						{
							if (object.ReferenceEquals(this.ArmorMaterials[m], this.Armors[l].GetComponent<Renderer>().sharedMaterial))
							{
								num4 = m + 1;
								break;
							}
						}
					}
					if (num4 != base.state.Armors[l])
					{
						base.state.Armors[l] = num4;
					}
				}
			}
		}
	}

	public Material[] ArmorMaterials;

	public GameObject[] Armors;

	public GameObject[] Leaves;

	public GameObject[] Bones;

	public GameObject[] Creepy;

	public GameObject ReBreather;

	public GameObject Warmsuit;
}
