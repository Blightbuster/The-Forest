using System;
using System.Collections.Generic;
using TheForest.Commons.Delegates;
using UnityEngine;

public class LOD_V2 : MonoBehaviour
{
	public bool HasHigh
	{
		get
		{
			return this.High != null && this.High.Length > 0;
		}
	}

	public bool HasMid
	{
		get
		{
			return this.Mid != null && this.Mid.Length > 0;
		}
	}

	public bool HasLow
	{
		get
		{
			return this.Low != null && this.Low.Length > 0;
		}
	}

	public int MaxLOD
	{
		get
		{
			if (this.HasLow)
			{
				return 3;
			}
			if (this.HasMid)
			{
				return 2;
			}
			return 1;
		}
	}

	public float GetRange(int index)
	{
		if (index == 0)
		{
			return this.HighRange;
		}
		if (index != 1)
		{
			return this.LowRange;
		}
		return this.MidRange;
	}

	private void OnEnable()
	{
		if (this.billboardId == -1 && this.billboard != null)
		{
			this.billboardId = this.billboard.Register(base.transform.position, base.transform.rotation.y);
		}
		this.wsToken = WorkScheduler.Register(new WsTask(this.RefreshLODs), base.transform.position, false);
		this.RefreshLODs();
	}

	private void OnDisable()
	{
		WorkScheduler.Unregister(new WsTask(this.RefreshLODs), this.wsToken);
	}

	private void OnDestroy()
	{
		if (this.billboardId >= 0 && this.billboard != null)
		{
			this.billboard.SetAlive(this.billboardId, false);
		}
	}

	private void ToggleComponents(Component[] components, bool enabled)
	{
		if (components == null || components.Length <= 0)
		{
			return;
		}
		foreach (Component component in components)
		{
			Transform transform = component as Transform;
			if (transform != null)
			{
				if (transform.gameObject.activeSelf != enabled)
				{
					transform.gameObject.SetActive(enabled);
				}
			}
			else
			{
				Renderer renderer = component as Renderer;
				if (renderer != null)
				{
					renderer.enabled = enabled;
				}
				else
				{
					Collider collider = component as Collider;
					if (collider != null)
					{
						collider.enabled = enabled;
					}
					else
					{
						Behaviour behaviour = component as Behaviour;
						if (behaviour != null)
						{
							behaviour.enabled = enabled;
						}
					}
				}
			}
		}
	}

	public void SetLOD(int lod)
	{
		this.ToggleComponents(this.High, false);
		this.ToggleComponents(this.Mid, false);
		this.ToggleComponents(this.Low, false);
		if (lod != 0)
		{
			if (lod != 1)
			{
				if (lod == 2)
				{
					this.ToggleComponents(this.Low, true);
				}
			}
			else
			{
				this.ToggleComponents(this.Mid, true);
			}
		}
		else
		{
			this.ToggleComponents(this.High, true);
		}
		this.currentLOD = lod;
	}

	private void RefreshLODs()
	{
		int lod = this.GetLOD();
		if (lod != this.currentLOD)
		{
			this.SetLOD(lod);
		}
	}

	public int GetLOD()
	{
		Vector3 position = base.transform.position;
		if (this.Use2dDistance)
		{
			position.y = PlayerCamLocation.PlayerLoc.y;
		}
		float magnitude = (position - PlayerCamLocation.PlayerLoc).magnitude;
		int maxLOD = this.MaxLOD;
		float num = this.LodPadding / 2f;
		int i = 0;
		while (i <= maxLOD)
		{
			float num2 = (i != 0) ? this.GetRange(i - 1) : (-num);
			float num3 = (i != maxLOD) ? this.GetRange(i) : float.MaxValue;
			float num4 = magnitude - num2;
			float num5 = num3 - magnitude;
			if (num4 >= 0f && num5 >= 0f)
			{
				if (Mathf.Abs(i - this.currentLOD) != 1)
				{
					return i;
				}
				if (num4 >= num && num5 >= num)
				{
					return i;
				}
				return this.currentLOD;
			}
			else
			{
				i++;
			}
		}
		return maxLOD;
	}

	private List<Material> FindMaterials()
	{
		this._foundMaterials.Clear();
		foreach (Renderer renderer in base.GetComponentsInChildren<Renderer>())
		{
			foreach (Material item in renderer.sharedMaterials)
			{
				this._foundMaterials.Add(item);
			}
		}
		return this._foundMaterials;
	}

	public bool FindStippleRange(out float range)
	{
		if (this.billboard != null)
		{
			range = this.billboard.FadeFarDistance - this.billboard.FadeNearDistance;
			return true;
		}
		foreach (Material material in this.FindMaterials())
		{
			if (material.HasProperty("_StippleFar"))
			{
				range = material.GetFloat("_StippleFar") - material.GetFloat("_StippleNear");
				return true;
			}
		}
		range = 0f;
		return false;
	}

	public void SetStippleRange(float stippleRange)
	{
		float range = this.GetRange(this.MaxLOD - 1);
		float num = range - stippleRange;
		float num2 = range;
		if (this.billboard != null)
		{
			this.billboard.FadeNearDistance = num;
			this.billboard.FadeFarDistance = num2;
		}
		foreach (Material material in this.FindMaterials())
		{
			if (material && material.HasProperty("_StippleFar"))
			{
				material.SetFloat("_StippleNear", num);
				material.SetFloat("_StippleFar", num2);
			}
		}
	}

	public Component[] High;

	public Component[] Mid;

	public Component[] Low;

	[Range(10f, 200f)]
	public float HighRange = 20f;

	[Range(10f, 200f)]
	public float MidRange = 40f;

	[Range(10f, 200f)]
	public float LowRange = 75f;

	public CustomBillboard billboard;

	public bool Use2dDistance = true;

	[Range(0f, 5f)]
	public float LodPadding = 1f;

	private int currentLOD = -1;

	private int billboardId = -1;

	private int wsToken;

	private List<Material> _foundMaterials = new List<Material>();
}
