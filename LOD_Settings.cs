using System;
using TheForest.Utils;
using UnityEngine;


[Serializable]
public class LOD_Settings
{
	
	public LOD_Settings(float[] ranges)
	{
	}

	
	public void Update(float quality)
	{
		if (Scene.Atmosphere && this.CaveMode != LOD_Settings.CaveModes.Always && (this.CaveMode == LOD_Settings.CaveModes.CaveOnly == LocalPlayer.IsInOutsideWorld || this.CaveMode == LOD_Settings.CaveModes.OutsideOnly == LocalPlayer.IsInClosedArea))
		{
			for (int i = 0; i < this.BaseRanges.Length; i++)
			{
				this.currentRanges[i] = 0f;
			}
			return;
		}
		float num = quality * (1f + (LOD_Manager.TreeOcclusionBonusRatio - 1f) * this.TreeOcclusionBonusMultiplier);
		bool flag = this.BaseRanges.Length > 2;
		for (int j = 0; j < this.BaseRanges.Length; j++)
		{
			if (j == 0)
			{
				this.currentQuality[0] = 1f;
			}
			else
			{
				this.currentQuality[j] = num;
			}
			this.currentRanges[j] = this.GetRange(j);
		}
		float num2 = this.currentRanges[0];
		float num3 = this.currentRanges[1];
		float num4 = this.currentRanges[2];
		float num5;
		float num6;
		if (flag)
		{
			num5 = Mathf.Min(Mathf.Max(num4 - num3, 0f) * 0.75f, 35f);
			num6 = num4 - num5 * 0.3f;
		}
		else
		{
			num5 = Mathf.Min(Mathf.Max(num3 - num2, 0f) * 0.75f, 35f);
			num6 = num3 - num5 * 0.2f;
		}
		float num7 = num6 - num5;
		float num8 = num7 + num5 * 0.5f;
		float num9 = num8 - num5;
		if (this.DrawDebug && LocalPlayer.Transform)
		{
			Debug.DrawLine(LocalPlayer.Transform.position + LocalPlayer.Transform.forward * num9 - Vector3.up, LocalPlayer.Transform.position + LocalPlayer.Transform.forward * num8 + Vector3.up, Color.blue);
			Debug.DrawLine(LocalPlayer.Transform.position + LocalPlayer.Transform.forward * num6 - Vector3.up, LocalPlayer.Transform.position + LocalPlayer.Transform.forward * num7 + Vector3.up, Color.red);
			Debug.DrawLine(LocalPlayer.Transform.position + LocalPlayer.Transform.forward * this.currentRanges[1], LocalPlayer.Transform.position + LocalPlayer.Transform.forward * this.currentRanges[2], Color.white);
			Debug.DrawLine(LocalPlayer.Transform.position + LocalPlayer.Transform.forward * this.currentRanges[0], LocalPlayer.Transform.position + LocalPlayer.Transform.forward * this.currentRanges[1], Color.cyan);
		}
		Vector2 lhs = new Vector2(num3 - num5 + num9 + num4, num3 + num8);
		bool flag2 = lhs != this.lastStippleRange || (this.Billboards != null && this.lastBillboardCount != this.Billboards.Length) || (this.StippledMaterials != null && this.lastStippledMaterialCount != this.StippledMaterials.Length);
		if (flag2)
		{
			if (this.Billboards != null)
			{
				foreach (CustomBillboard customBillboard in this.Billboards)
				{
					customBillboard.FadeNearDistance = num9;
					customBillboard.FadeFarDistance = num8;
				}
			}
			if (this.StippledMaterials != null)
			{
				foreach (Material material in this.StippledMaterials)
				{
					material.SetFloat("_StippleNear", num7);
					material.SetFloat("_StippleFar", num6);
				}
			}
			this.lastStippleRange = lhs;
			this.lastBillboardCount = ((this.Billboards == null) ? 0 : this.Billboards.Length);
			this.lastStippledMaterialCount = ((this.StippledMaterials == null) ? 0 : this.StippledMaterials.Length);
		}
	}

	
	public float GetRange(int index)
	{
		float num;
		if (index >= 0 && index < this.BaseRanges.Length)
		{
			num = this.BaseRanges[index];
			num *= this.currentQuality[index];
		}
		else
		{
			num = -1f;
		}
		return Mathf.Min(num, 400f);
	}

	
	public int GetLOD(Vector3 position, int currentLOD)
	{
		if (this.Use2dDistance)
		{
			position.y = PlayerCamLocation.PlayerLoc.y;
		}
		float num = Vector3.Distance(position, PlayerCamLocation.PlayerLoc);
		int num2 = this.BaseRanges.Length;
		int result = num2;
		float num3 = LOD_Manager.Instance.Padding / 2f;
		int i = 0;
		while (i <= num2)
		{
			float num4 = (i != 0) ? this.currentRanges[i - 1] : (-num3);
			float num5 = (i != num2) ? this.currentRanges[i] : float.MaxValue;
			float num6 = num - num4;
			float num7 = num5 - num;
			if (num6 >= 0f && num7 >= 0f)
			{
				if (Mathf.Abs(i - currentLOD) != 1)
				{
					result = i;
					break;
				}
				if (num6 >= num3 && num7 >= num3)
				{
					result = i;
					break;
				}
				result = currentLOD;
				break;
			}
			else
			{
				i++;
			}
		}
		return result;
	}

	
	
	public TheForestQualitySettings.DrawDistances GetNewObjectMaxDrawDistance
	{
		get
		{
			if (++this.newObjectMaxDrawDistance > this.newObjectDrawDistanceTo)
			{
				this.newObjectMaxDrawDistance = this.newObjectDrawDistanceFrom;
			}
			return this.newObjectMaxDrawDistance;
		}
	}

	
	[Range(0f, 300f)]
	public float[] BaseRanges = new float[0];

	
	public float TreeOcclusionBonusMultiplier;

	
	public bool Use2dDistance = true;

	
	public CustomBillboard[] Billboards;

	
	public Material[] StippledMaterials;

	
	public LOD_Settings.CaveModes CaveMode = LOD_Settings.CaveModes.OutsideOnly;

	
	public TheForestQualitySettings.DrawDistances newObjectDrawDistanceFrom = TheForestQualitySettings.DrawDistances.UltraLow;

	
	public TheForestQualitySettings.DrawDistances newObjectDrawDistanceTo = TheForestQualitySettings.DrawDistances.UltraLow;

	
	public bool DrawDebug;

	
	private Vector2 lastStippleRange = Vector2.zero;

	
	private int lastBillboardCount;

	
	private int lastStippledMaterialCount;

	
	private float[] currentQuality = new float[3];

	
	private float[] currentRanges = new float[3];

	
	private int unConstrainedMidLodCount;

	
	private TheForestQualitySettings.DrawDistances newObjectMaxDrawDistance = TheForestQualitySettings.DrawDistances.UltraLow;

	
	public enum LowLodModes
	{
		
		Always,
		
		HighDrawDistance
	}

	
	public enum BillboardLowLodModes
	{
		
		MatchLowMode,
		
		RangeOnly
	}

	
	public enum CaveModes
	{
		
		Always,
		
		CaveOnly,
		
		OutsideOnly
	}

	
	public enum ScaleModes
	{
		
		ScaledQuality = 1,
		
		Quality,
		
		One
	}

	
	public enum LODs
	{
		
		LOD0_high,
		
		LOD1_mid,
		
		LOD2_low
	}
}
