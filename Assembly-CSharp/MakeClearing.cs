using System;
using System.Collections.Generic;
using UnityEngine;

public class MakeClearing : MonoBehaviour
{
	private int GetStepCount()
	{
		return (int)(this.Length / this.Radius) + 1;
	}

	private Vector3 GetPosition(float progress)
	{
		progress = Mathf.Clamp01(progress);
		Vector3 vector = base.transform.position - base.transform.forward * (progress * this.Length);
		Terrain activeTerrain = Terrain.activeTerrain;
		if (activeTerrain)
		{
			vector.y = activeTerrain.transform.position.y + activeTerrain.SampleHeight(vector);
		}
		return vector;
	}

	private void Start()
	{
		if (this.ShouldCutGrass)
		{
			this.startGrassClearing();
		}
	}

	private void startGrassClearing()
	{
		int stepCount = this.GetStepCount();
		for (int i = 0; i < stepCount; i++)
		{
			float progress = (float)i / (float)stepCount;
			Vector3 position = this.GetPosition(progress);
			if (this.ShouldCutGrass)
			{
				NeoGrassCutter.Cut(position, this.Radius, false);
			}
		}
	}

	public void startClearing()
	{
		int stepCount = this.GetStepCount();
		LOD_Base[] array = UnityEngine.Object.FindObjectsOfType<LOD_Base>();
		List<LOD_Base> list = new List<LOD_Base>();
		Vector3 position = base.transform.position;
		float num = this.Radius + this.Length;
		num *= num;
		float num2 = this.Radius * this.Radius;
		foreach (LOD_Base lod_Base in array)
		{
			if (!(lod_Base is LOD_Rocks) && (lod_Base.transform.position - position).sqrMagnitude < num)
			{
				list.Add(lod_Base);
			}
		}
		for (int j = 0; j < stepCount; j++)
		{
			float progress = (float)j / (float)stepCount;
			Vector3 position2 = this.GetPosition(progress);
			foreach (LOD_Base lod_Base2 in list)
			{
				if ((lod_Base2.transform.position - position2).sqrMagnitude < num2)
				{
					CoopTreeId coopTreeId = null;
					if (BoltNetwork.isServer)
					{
						coopTreeId = lod_Base2.GetComponent<CoopTreeId>();
					}
					Debug.Log(lod_Base2 + " CLEARED");
					UnityEngine.Object.DestroyImmediate(lod_Base2);
					if (coopTreeId)
					{
						coopTreeId.Goto_Removed();
					}
				}
			}
			if (this.ShouldCutGrass)
			{
				NeoGrassCutter.Cut(position2, this.Radius, false);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		int stepCount = this.GetStepCount();
		Color color = Gizmos.color;
		Gizmos.color = Color.red;
		for (int i = 0; i < stepCount; i++)
		{
			float progress = (float)i / (float)stepCount;
			Vector3 position = this.GetPosition(progress);
			Gizmos.DrawWireSphere(position, this.Radius);
		}
		Gizmos.color = color;
	}

	public bool ShouldCutGrass;

	[Range(0.5f, 40f)]
	public float Radius = 5f;

	[Range(1f, 200f)]
	public float Length = 1f;
}
