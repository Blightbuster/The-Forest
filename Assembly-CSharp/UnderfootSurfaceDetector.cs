using System;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;

public class UnderfootSurfaceDetector : MonoBehaviour
{
	public UnderfootSurfaceDetector.SurfaceType Surface { get; private set; }

	public bool IsOnGore
	{
		get
		{
			return this.goreColliders.Count > 0;
		}
	}

	private void Start()
	{
		this.collider = base.GetComponent<Collider>();
		this.colliders = new List<Collider>();
		this.goreColliders = new List<Collider>();
		this.Surface = UnderfootSurfaceDetector.SurfaceType.None;
		base.InvokeRepeating("ValidateColliders", 1f, 1f);
	}

	public static UnderfootSurfaceDetector.SurfaceType GetSurfaceType(Collider collider)
	{
		if (LocalPlayer.IsInCaves && collider.CompareTag("TerrainMain"))
		{
			return UnderfootSurfaceDetector.SurfaceType.Rock;
		}
		UnderfootSurfaceDetector.SurfaceType result;
		if (UnderfootSurfaceDetector.SurfaceTags.TryGetValue(collider.tag, out result))
		{
			return result;
		}
		if (UnderfootSurfaceDetector.CheckComponentTags.Contains(collider.tag))
		{
			UnderfootSurface component = collider.GetComponent<UnderfootSurface>();
			if (component != null)
			{
				return component.surfaceType;
			}
		}
		return UnderfootSurfaceDetector.SurfaceType.None;
	}

	private void UpdateSurface()
	{
		if (this.colliders.Count > 0)
		{
			this.Surface = UnderfootSurfaceDetector.GetSurfaceType(this.colliders[this.colliders.Count - 1]);
		}
		else
		{
			this.Surface = UnderfootSurfaceDetector.SurfaceType.None;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		UnderfootSurfaceDetector.SurfaceType surfaceType = UnderfootSurfaceDetector.GetSurfaceType(other);
		if (surfaceType != UnderfootSurfaceDetector.SurfaceType.None)
		{
			int i = 0;
			while (i < this.colliders.Count)
			{
				if (this.colliders[i] == null || this.colliders[i] == other)
				{
					this.colliders.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
			this.colliders.Add(other);
			this.Surface = surfaceType;
		}
		if (other.tag == UnderfootSurfaceDetector.GoreTag)
		{
			this.goreColliders.Add(other);
		}
	}

	private static bool RemoveIfPresent(List<Collider> list, Collider entry)
	{
		bool result = false;
		int i = 0;
		while (i < list.Count)
		{
			if (list[i] == null || list[i] == entry)
			{
				list.RemoveAt(i);
				result = true;
			}
			else
			{
				i++;
			}
		}
		return result;
	}

	private void OnTriggerExit(Collider other)
	{
		UnderfootSurfaceDetector.RemoveIfPresent(this.goreColliders, other);
		if (UnderfootSurfaceDetector.RemoveIfPresent(this.colliders, other))
		{
			this.UpdateSurface();
		}
	}

	private static bool RemoveNonIntersecting(List<Collider> list, Collider collider)
	{
		bool result = false;
		int i = 0;
		while (i < list.Count)
		{
			if (list[i] == null || !list[i].bounds.Intersects(collider.bounds))
			{
				list.RemoveAt(i);
				result = true;
			}
			else
			{
				i++;
			}
		}
		return result;
	}

	private void ValidateColliders()
	{
		UnderfootSurfaceDetector.RemoveNonIntersecting(this.goreColliders, this.collider);
		if (UnderfootSurfaceDetector.RemoveNonIntersecting(this.colliders, this.collider))
		{
			this.UpdateSurface();
		}
	}

	private Collider collider;

	private List<Collider> colliders;

	private List<Collider> goreColliders;

	public const string TAG_WOOD = "UnderfootWood";

	private static Dictionary<string, UnderfootSurfaceDetector.SurfaceType> SurfaceTags = new Dictionary<string, UnderfootSurfaceDetector.SurfaceType>
	{
		{
			"UnderfootWood",
			UnderfootSurfaceDetector.SurfaceType.Wood
		},
		{
			"BreakableWood",
			UnderfootSurfaceDetector.SurfaceType.Wood
		},
		{
			"structure",
			UnderfootSurfaceDetector.SurfaceType.Wood
		},
		{
			"SLTier1",
			UnderfootSurfaceDetector.SurfaceType.Wood
		},
		{
			"SLTier2",
			UnderfootSurfaceDetector.SurfaceType.Wood
		},
		{
			"SLTier3",
			UnderfootSurfaceDetector.SurfaceType.Wood
		},
		{
			"DeadTree",
			UnderfootSurfaceDetector.SurfaceType.Wood
		},
		{
			"Multisled",
			UnderfootSurfaceDetector.SurfaceType.Wood
		},
		{
			"Target",
			UnderfootSurfaceDetector.SurfaceType.Wood
		},
		{
			"Fire",
			UnderfootSurfaceDetector.SurfaceType.Wood
		},
		{
			"UnderfootRock",
			UnderfootSurfaceDetector.SurfaceType.Rock
		},
		{
			"BreakableRock",
			UnderfootSurfaceDetector.SurfaceType.Rock
		},
		{
			"Block",
			UnderfootSurfaceDetector.SurfaceType.Rock
		},
		{
			"UnderfootMetal",
			UnderfootSurfaceDetector.SurfaceType.Metal
		},
		{
			"UnderfootCarpet",
			UnderfootSurfaceDetector.SurfaceType.Carpet
		},
		{
			"UnderfootDirt",
			UnderfootSurfaceDetector.SurfaceType.Dirt
		},
		{
			"UnderfootPlastic",
			UnderfootSurfaceDetector.SurfaceType.Plastic
		},
		{
			"UnderfootMetalGrate",
			UnderfootSurfaceDetector.SurfaceType.MetalGrate
		},
		{
			"UnderfootBrokenGlass",
			UnderfootSurfaceDetector.SurfaceType.BrokenGlass
		}
	};

	private static HashSet<string> CheckComponentTags = new HashSet<string>
	{
		"jumpObject"
	};

	private static string GoreTag = "UnderfootGore";

	public enum SurfaceType
	{
		None,
		Wood,
		Rock,
		Carpet,
		Dirt,
		Metal,
		Plastic,
		MetalGrate,
		BrokenGlass
	}
}
