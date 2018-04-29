using System;
using ModAPI;
using TheForest.Buildings.Creation;
using TheForest.Buildings.World;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UltimateCheatmenu;
using UnityEngine;


public class KeepAboveTerrain : MonoBehaviour
{
	
	
	
	public bool Clear
	{
		get
		{
			return this._clearInternal && this._clearDynamicBuilding && this._clearSmallStructures && this.ClearOfCollision;
		}
		set
		{
			this._clearInternal = value;
		}
	}

	
	
	
	public bool ClearOfCollision { get; private set; }

	
	
	public bool OnDynamic
	{
		get
		{
			return this.shouldDoDynamicBuildingCheck && this._clearDynamicBuilding;
		}
	}

	
	
	
	public bool IsInSinkHole { get; private set; }

	
	
	
	public bool IgnoreLookAtCollision { get; set; }

	
	
	
	public float AirBorneHeight { get; set; }

	
	
	
	public float RegularHeight { get; set; }

	
	
	
	public float MinHeight { get; set; }

	
	
	public float YRotation
	{
		get
		{
			return this.curDir;
		}
	}

	
	
	
	public Quaternion CurrentRotation { get; set; }

	
	
	
	public RaycastHit? LastHit { get; set; }

	
	
	
	public GameObject ForcedParent { get; set; }

	
	
	
	public LayerMask FloorLayersFinal { get; set; }

	
	
	public Vector3 LookingAtPointCam
	{
		get
		{
			return this.lookingAtPointCam;
		}
	}

	
	
	
	public Func<Transform, bool> ValidateAnchor { get; set; }

	
	private void __Awake__Original()
	{
		this.startLocalPosition = base.transform.localPosition;
		this.activeTerrain = Terrain.activeTerrain;
		this.boxCollider = base.gameObject.GetComponent<BoxCollider>();
	}

	
	private void Start()
	{
		this.sinkHolePos = Scene.SinkHoleCenter.position;
	}

	
	private void OnEnable()
	{
		this.turn = 0f;
		this.Clear = true;
		this.Waterborne = false;
		this.WaterborneExclusive = false;
		this.Hydrophobic = false;
		this.IgnoreLookAtCollision = false;
		if (LocalPlayer.Create.CurrentBlueprint != null)
		{
			this.FloorLayersFinal = ((!LocalPlayer.Create.CurrentBlueprint._allowParentingToDynamic) ? this.FloorLayers : this.DynFloorLayers);
		}
	}

	
	private void __Update__Original()
	{
		if (!TheForest.Utils.Input.IsGamePad)
		{
			if (TheForest.Utils.Input.GetButtonUp("Rotate"))
			{
				this.turn = 0f;
			}
			else if (TheForest.Utils.Input.GetButton("Rotate"))
			{
				this.turn = 60f * Time.deltaTime * (float)((!TheForest.Utils.Input.player.GetButtonTimedPress("Rotate", 0.5f)) ? 1 : 3);
			}
		}
		else
		{
			float num = Mathf.Clamp(TheForest.Utils.Input.GetAxis("Rotate"), 0f, 1f);
			float axisTimeActive = TheForest.Utils.Input.player.GetAxisTimeActive("Rotate");
			if (!Mathf.Approximately(num, 0f))
			{
				this.turn = num * 60f * Time.deltaTime * (float)((axisTimeActive <= 0.5f) ? 1 : 3);
			}
			else
			{
				this.turn = 0f;
			}
		}
		this.curDir = (this.curDir + this.turn + 360f) % 360f;
		this.CurrentRotation = Quaternion.Euler(0f, this.curDir, 0f);
		if (!this.Locked && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World)
		{
			Vector3 vector = base.transform.parent.TransformPoint(this.startLocalPosition);
			this.shouldDoSmallStructureCheck = false;
			this.shouldDoDynamicBuildingCheck = false;
			this._lockDynamicBuilding = false;
			Vector3 to = this.boxCollider.center - this.boxCollider.size / 2f;
			Vector3 vector2 = this.boxCollider.center + this.boxCollider.size / 2f;
			float cornersRaycastDistanceAlpha = LocalPlayer.Create.CurrentBlueprint._cornersRaycastDistanceAlpha;
			Vector3 vector3 = base.transform.TransformPoint(Vector3.Lerp(this.boxCollider.center, to, cornersRaycastDistanceAlpha));
			Vector3 vector4 = base.transform.TransformPoint(Vector3.Lerp(this.boxCollider.center, new Vector3(to.x, to.y, vector2.z), cornersRaycastDistanceAlpha));
			Vector3 vector5 = base.transform.TransformPoint(Vector3.Lerp(this.boxCollider.center, new Vector3(vector2.x, to.y, to.z), cornersRaycastDistanceAlpha));
			Vector3 vector6 = base.transform.TransformPoint(Vector3.Lerp(this.boxCollider.center, new Vector3(vector2.x, to.y, vector2.z), cornersRaycastDistanceAlpha));
			this.sinkHolePos.y = base.transform.position.y;
			this.IsInSinkHole = (Vector3.Distance(this.sinkHolePos, vector) < 190f);
			float maxDistance = (!this.IsInSinkHole) ? this.maxBuildingHeight : (this.maxBuildingHeight + 300f);
			Vector3 position = LocalPlayer.MainCamTr.position;
			bool flag = false;
			Vector3 point;
			if (MathEx.ClosestPointsOnTwoLines(out this.lookingAtPointCam, out point, position, LocalPlayer.MainCamTr.forward, vector, Vector3.up))
			{
				float num2 = Vector3.Distance(position, this.lookingAtPointCam) * 0.96f;
				if (!LocalPlayer.IsInCaves && !this.IsInSinkHole)
				{
					point.y = Mathf.Max(point.y, Terrain.activeTerrain.SampleHeight(vector));
				}
				Vector3 a;
				if (this.IgnoreLookAtCollision || !Physics.Raycast(position, LocalPlayer.MainCamTr.forward, out this.hit, num2, this.GetValidLayers(this.FloorLayersFinal.value)))
				{
					a = LocalPlayer.MainCamTr.position + LocalPlayer.MainCamTr.forward * num2;
					this.LastHit = null;
					if (this.Airborne)
					{
						vector3.y = point.y;
						vector4.y = point.y;
						vector5.y = point.y;
						vector6.y = point.y;
					}
					else
					{
						vector3.y = point.y + 2f;
						vector4.y = point.y + 2f;
						vector5.y = point.y + 2f;
						vector6.y = point.y + 2f;
					}
				}
				else
				{
					a = this.hit.point;
					float y;
					if (!LocalPlayer.IsInCaves)
					{
						y = Mathf.Max(LocalPlayer.Transform.position.y + 2f, Terrain.activeTerrain.SampleHeight(vector) + 2f);
					}
					else
					{
						y = LocalPlayer.Transform.position.y + 2f;
					}
					this.LastHit = new RaycastHit?(this.hit);
					point = this.hit.point;
					if (this.hit.normal.y > 0f)
					{
						flag = true;
						point.y = y;
						vector3.y = y;
						vector4.y = y;
						vector5.y = y;
						vector6.y = y;
					}
					else if (!this.Airborne)
					{
						point.y += this.maxBuildingHeight / 2f;
						vector3.y = this.hit.point.y + this.maxBuildingHeight / 2f;
						vector4.y = this.hit.point.y + this.maxBuildingHeight / 2f;
						vector5.y = this.hit.point.y + this.maxBuildingHeight / 2f;
						vector6.y = this.hit.point.y + this.maxBuildingHeight / 2f;
					}
					this.ParentingRulesLookup();
				}
				Vector3 b = new Vector3(1f, 0f, 1f);
				float num3 = Vector3.Distance(Vector3.Scale(a, b), Vector3.Scale(LocalPlayer.Transform.position, b));
				float num4 = Vector3.Distance(Vector3.Scale(base.transform.position, b), Vector3.Scale(LocalPlayer.Transform.position, b));
				if (!Mathf.Approximately(num3, num4))
				{
					Vector3 b2 = LocalPlayer.Transform.forward * (num3 - num4);
					base.transform.position += b2;
					vector3 += b2;
					vector4 += b2;
					vector5 += b2;
					vector6 += b2;
				}
			}
			RaycastHit? raycastHit = null;
			RaycastHit? raycastHit2 = null;
			RaycastHit? raycastHit3 = null;
			RaycastHit? raycastHit4 = null;
			int num5 = 0;
			float radius = 0.2f;
			if (Physics.SphereCast(vector3, radius, Vector3.down, out this.hit, maxDistance, this.GetValidLayers(this.FloorLayersFinal.value)))
			{
				if (!this.MatchingExclusionGroup(this.hit.collider, true))
				{
					raycastHit = new RaycastHit?(this.hit);
				}
				else
				{
					this.SetColliding();
				}
				vector3.y = this.hit.point.y;
				if ((1 << this.hit.collider.gameObject.layer & this.WaterLayers) != 0)
				{
					num5++;
				}
				this.ParentingRulesLookup();
			}
			else
			{
				vector3.y = this.activeTerrain.SampleHeight(vector3) + this.activeTerrain.transform.position.y;
			}
			if (Physics.SphereCast(vector4, radius, Vector3.down, out this.hit, maxDistance, this.GetValidLayers(this.FloorLayersFinal.value)))
			{
				if (!this.MatchingExclusionGroup(this.hit.collider, true))
				{
					raycastHit2 = new RaycastHit?(this.hit);
				}
				else
				{
					this.SetColliding();
				}
				vector4.y = this.hit.point.y;
				if ((1 << this.hit.collider.gameObject.layer & this.WaterLayers) != 0)
				{
					num5++;
				}
				this.ParentingRulesLookup();
			}
			else
			{
				vector4.y = this.activeTerrain.SampleHeight(vector4) + this.activeTerrain.transform.position.y;
			}
			if (Physics.SphereCast(vector5, radius, Vector3.down, out this.hit, maxDistance, this.GetValidLayers(this.FloorLayersFinal.value)))
			{
				if (!this.MatchingExclusionGroup(this.hit.collider, true))
				{
					raycastHit4 = new RaycastHit?(this.hit);
				}
				else
				{
					this.SetColliding();
				}
				vector5.y = this.hit.point.y;
				if ((1 << this.hit.collider.gameObject.layer & this.WaterLayers) != 0)
				{
					num5++;
				}
				this.ParentingRulesLookup();
			}
			else
			{
				vector5.y = this.activeTerrain.SampleHeight(vector5) + this.activeTerrain.transform.position.y;
			}
			if (Physics.SphereCast(vector6, radius, Vector3.down, out this.hit, maxDistance, this.GetValidLayers(this.FloorLayersFinal.value)))
			{
				if (!this.MatchingExclusionGroup(this.hit.collider, true))
				{
					raycastHit3 = new RaycastHit?(this.hit);
				}
				else
				{
					this.SetColliding();
				}
				vector6.y = this.hit.point.y;
				if ((1 << this.hit.collider.gameObject.layer & this.WaterLayers) != 0)
				{
					num5++;
				}
				this.ParentingRulesLookup();
			}
			else
			{
				vector6.y = this.activeTerrain.SampleHeight(vector6) + this.activeTerrain.transform.position.y;
			}
			bool flag2 = false;
			Vector3 position2 = base.transform.position;
			position2.y = point.y + 1f;
			float num6;
			if (Physics.SphereCast(position2, radius, Vector3.down, out this.hit, maxDistance, this.GetValidLayers(this.FloorLayersFinal.value)))
			{
				num6 = this.hit.point.y;
				if (!this.MatchingExclusionGroup(this.hit.collider, true))
				{
					if (this.LastHit == null)
					{
						this.LastHit = new RaycastHit?(this.hit);
					}
				}
				else
				{
					this.SetColliding();
				}
				if (this.hit.transform.name.Equals("MainTerrain") || this.hit.transform.gameObject.layer == 17)
				{
					flag2 = true;
				}
				if ((1 << this.hit.collider.gameObject.layer & this.WaterLayers) != 0)
				{
					num5++;
				}
				this.ParentingRulesLookup();
			}
			else
			{
				num6 = this.activeTerrain.SampleHeight(vector) + this.activeTerrain.transform.position.y;
			}
			if (this.CheckForHole(vector3, vector4, vector5, vector6))
			{
				if (num6 < vector3.y)
				{
					if (raycastHit != null)
					{
						this.LastHit = new RaycastHit?(this.hit);
					}
					num6 = vector3.y;
				}
			}
			else if (this.CheckForHole(vector4, vector3, vector6, vector5) && num6 < vector4.y)
			{
				if (raycastHit2 != null)
				{
					this.LastHit = new RaycastHit?(this.hit);
				}
				num6 = vector4.y;
			}
			if (this.CheckForHole(vector6, vector4, vector5, vector3) && num6 < vector6.y)
			{
				if (raycastHit3 != null)
				{
					this.LastHit = new RaycastHit?(this.hit);
				}
				num6 = vector6.y;
			}
			if (this.CheckForHole(vector5, vector6, vector3, vector4) && num6 < vector5.y)
			{
				if (raycastHit4 != null)
				{
					this.LastHit = new RaycastHit?(this.hit);
				}
				num6 = vector5.y;
			}
			if (flag)
			{
				point.y = num6;
			}
			float num7 = (vector3.y + vector4.y + vector5.y + vector6.y + num6) / 5f;
			if (!flag2)
			{
				float num8 = 1f;
				if (Mathf.Abs((vector4.y + vector5.y + vector6.y + num6) / 4f - vector3.y) > num8)
				{
					num7 = (vector4.y + vector5.y + vector6.y + num6) / 4f;
					vector3.y = num7;
				}
				if (Mathf.Abs((vector3.y + vector5.y + vector6.y + num6) / 4f - vector4.y) > num8)
				{
					num7 = (vector3.y + vector5.y + vector6.y + num6) / 4f;
					vector4.y = num7;
				}
				if (Mathf.Abs((vector3.y + vector4.y + vector6.y + num6) / 4f - vector5.y) > num8)
				{
					num7 = (vector3.y + vector4.y + vector6.y + num6) / 4f;
					vector5.y = num7;
				}
				if (Mathf.Abs((vector3.y + vector4.y + vector5.y + num6) / 4f - vector6.y) > num8)
				{
					num7 = (vector3.y + vector4.y + vector5.y + num6) / 4f;
					vector6.y = num7;
				}
			}
			if (this.Airborne)
			{
				if (this.IsInSinkHole)
				{
					float num9 = LocalPlayer.Transform.position.y + this.maxAirBorneHeight;
					if (point.y > num9)
					{
						point.y = num9;
					}
				}
				else if (point.y - num7 > this.maxAirBorneHeight)
				{
					point.y = num7 + this.maxAirBorneHeight;
				}
			}
			if (this.WaterborneExclusive)
			{
				if (num5 == 5)
				{
					this.SetClear();
				}
				else
				{
					this.SetNotclear();
				}
			}
			else if (this.Hydrophobic && num5 > 0)
			{
				this.SetColliding();
				this.SetNotclear();
			}
			Vector3 a2 = Vector3.Cross(vector4 - vector3, vector5 - vector3);
			Vector3 b3 = Vector3.Cross(vector5 - vector6, vector4 - vector6);
			this.curNormal = Vector3.Normalize((a2 + b3) / 2f);
			this.MinHeight = Mathf.Min(new float[]
			{
				vector3.y,
				vector4.y,
				vector5.y,
				vector6.y,
				num6
			});
			if (!this.AllowFoundation || Vector3.Angle(Vector3.up, this.curNormal) < this.FoundationMinSlope)
			{
				this.AirBorneHeight = Mathf.Max(point.y, Mathf.Min(num6, num7));
				this.RegularHeight = Mathf.Min(num6, num7);
				if (this.curNormal.y < 0.5f)
				{
					this.curNormal = Vector3.up;
					if (this.Airborne)
					{
						this.SetClear();
					}
				}
				this.curGroundTilt = Quaternion.FromToRotation(Vector3.up, this.curNormal);
				this.ApplyLeaningPosRot(base.transform, this.Airborne, this.TreeStructure);
			}
			else
			{
				this.AirBorneHeight = Mathf.Max(new float[]
				{
					vector3.y,
					vector4.y,
					vector5.y,
					vector6.y,
					num6,
					point.y
				});
				this.RegularHeight = Mathf.Max(new float[]
				{
					vector3.y,
					vector4.y,
					vector5.y,
					vector6.y,
					num6
				});
				base.transform.position = new Vector3(base.transform.position.x, (!this.Airborne) ? this.RegularHeight : this.AirBorneHeight, base.transform.position.z);
				base.transform.rotation = this.CurrentRotation;
			}
			this.ParentingRulesCheck();
		}
		if (this.TreeStructure)
		{
			LocalPlayer.Create.CurrentGhost.transform.rotation = this.CurrentRotation;
			if (this.Locked != this.Clear)
			{
				if (this.Locked && this.ClearOfCollision)
				{
					if (this.MyRender)
					{
						this.MyRender.material = ((!LocalPlayer.Create.CurrentBlueprint._useFlatMeshMaterial) ? this.ClearMat : this.ClearMatFlatMesh);
					}
					this.Clear = true;
				}
				else
				{
					if (this.MyRender)
					{
						this.MyRender.material = this.RedMat;
					}
					this.Clear = false;
				}
			}
		}
	}

	
	private void FixedUpdate()
	{
		this.ClearOfCollision = true;
	}

	
	private void OnTriggerStay(Collider other)
	{
		if (!this.IgnoreLookAtCollision && ((other.gameObject.CompareTag("Tree") && !this.TreeStructure) || (other.gameObject.CompareTag("Block") && !this.IgnoreBlock) || (other.gameObject.CompareTag("jumpObject") && !this.TreeStructure) || this.MatchingExclusionGroup(other, true)))
		{
			this.SetColliding();
			this.SetNotclear();
		}
	}

	
	private void OnTriggerExit(Collider other)
	{
		if (!this.IgnoreLookAtCollision && ((other.gameObject.CompareTag("Tree") && !this.TreeStructure) || (other.gameObject.CompareTag("Block") && !this.IgnoreBlock) || (other.gameObject.CompareTag("jumpObject") && !this.TreeStructure) || this.MatchingExclusionGroup(other, false)))
		{
			this.SetClear();
		}
	}

	
	private bool MatchingExclusionGroup(Collider other, bool enter)
	{
		if (LocalPlayer.Create.CurrentBlueprint._exclusionGroup != ExclusionGroups.None)
		{
			PrefabIdentifier componentInParent = other.GetComponentInParent<PrefabIdentifier>();
			if (componentInParent)
			{
				BuildingHealth component = componentInParent.GetComponent<BuildingHealth>();
				if (component && component._type != BuildingTypes.None && Prefabs.Instance.Constructions.GetBlueprintExclusionGroup(component._type) == LocalPlayer.Create.CurrentBlueprint._exclusionGroup)
				{
					if (enter)
					{
						this._inExclusionGroup = true;
					}
					else
					{
						this._inExclusionGroup = false;
					}
					return true;
				}
			}
		}
		return false;
	}

	
	private bool CheckForHole(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 h1)
	{
		if (Mathf.Abs(p1.y - p2.y) < 0.1f && Mathf.Abs(p1.y - p3.y) < 0.1f && h1.y < p1.y)
		{
			h1.y = p1.y;
			return true;
		}
		return false;
	}

	
	private void ParentingRulesLookup()
	{
		if (!LocalPlayer.Create.CurrentBlueprint._skipParentingToDynamicChecks)
		{
			if (this.hit.collider.GetComponentInParent<DynamicBuilding>())
			{
				this.shouldDoDynamicBuildingCheck = true;
			}
			else
			{
				Rigidbody componentInParent = this.hit.collider.GetComponentInParent<Rigidbody>();
				if ((componentInParent && !componentInParent.isKinematic) || this.hit.collider.CompareTag("NoConstruction") || (this.hit.collider.transform.parent && this.hit.collider.transform.parent.CompareTag("NoConstruction")))
				{
					this.shouldDoDynamicBuildingCheck = true;
				}
				this._lockDynamicBuilding = true;
			}
		}
		if (!LocalPlayer.Create.CurrentBlueprint._isSmall && this.hit.collider.GetComponentInParent<SmallStructureSupport>())
		{
			this.shouldDoSmallStructureCheck = true;
		}
	}

	
	private void ParentingRulesCheck()
	{
		bool clear = this.Clear;
		if (this.shouldDoDynamicBuildingCheck)
		{
			this._clearDynamicBuilding = (LocalPlayer.Create.CurrentBlueprint._allowParentingToDynamic && !this._lockDynamicBuilding);
		}
		else
		{
			this._clearDynamicBuilding = true;
		}
		if (this.shouldDoSmallStructureCheck)
		{
			this._clearSmallStructures = LocalPlayer.Create.CurrentBlueprint._isSmall;
		}
		else
		{
			this._clearSmallStructures = true;
		}
		if (clear != this.Clear)
		{
			this.UpdateRendererColor();
		}
	}

	
	public void ApplyLeaningPosRot(Transform t, bool airborne, bool treeStructure)
	{
		t.position = new Vector3(t.position.x, (!airborne) ? this.RegularHeight : this.AirBorneHeight, t.position.z);
		t.rotation = ((!airborne && !treeStructure) ? (this.curGroundTilt * this.CurrentRotation) : this.CurrentRotation);
	}

	
	public void SetWaterborne(bool exclusive)
	{
		this.Waterborne = true;
		this.WaterborneExclusive = exclusive;
		this.FloorLayersFinal |= this.WaterLayers;
	}

	
	public void SetHydrophobic()
	{
		this.Hydrophobic = true;
		this.FloorLayersFinal |= this.WaterLayers;
	}

	
	public void __SetColliding__Original()
	{
		this.ClearOfCollision = false;
	}

	
	public void SetClear()
	{
		if (this._inExclusionGroup)
		{
			return;
		}
		this.Clear = true;
		this.UpdateRendererColor();
	}

	
	public void SetNotclear()
	{
		this.Clear = false;
		this.UpdateRendererColor();
	}

	
	public void SetRenderer(Renderer r)
	{
		this.MyRender = r;
		if (this.MyRender != null)
		{
			this.MyRender.sharedMaterial = ((!LocalPlayer.Create.CurrentBlueprint._useFlatMeshMaterial) ? this.ClearMat : this.ClearMatFlatMesh);
		}
	}

	
	private void UpdateRendererColor()
	{
		if (this.MyRender)
		{
			if (this.Clear)
			{
				this.MyRender.sharedMaterial = ((!LocalPlayer.Create.CurrentBlueprint._useFlatMeshMaterial) ? this.ClearMat : this.ClearMatFlatMesh);
			}
			else
			{
				this.MyRender.sharedMaterial = this.RedMat;
			}
		}
	}

	
	public int GetValidLayers(int layers)
	{
		return (!this.IsInSinkHole) ? layers : (layers ^ 67108864);
	}

	
	private void Awake()
	{
		try
		{
			this.maxBuildingHeight = 5000f;
			this.maxAirBorneHeight = 5000f;
			this.__Awake__Original();
		}
		catch (Exception ex)
		{
			Log.Write("Exception thrown: " + ex.ToString(), "UltimateCheatmenu");
			this.__Awake__Original();
		}
	}

	
	public void SetColliding()
	{
		try
		{
			if (UCheatmenu.BuildingCollision)
			{
				this.__SetColliding__Original();
				return;
			}
			this.ClearOfCollision = true;
		}
		catch (Exception ex)
		{
			Log.Write("Exception thrown: " + ex.ToString(), "UltimateCheatmenu");
			this.__SetColliding__Original();
		}
	}

	
	private void Update()
	{
		try
		{
			this.__Update__Original();
			if (!UCheatmenu.BuildingCollision)
			{
				this._clearInternal = true;
				this._clearDynamicBuilding = true;
				this._clearSmallStructures = true;
				this.ClearOfCollision = true;
			}
		}
		catch (Exception ex)
		{
			Log.Write("Exception thrown: " + ex.ToString(), "UltimateCheatmenu");
			this.__Update__Original();
		}
	}

	
	public GameObject ShowAnchorArea;

	
	public GameObject ShowSupportAnchorArea;

	
	public LayerMask FloorLayers;

	
	public LayerMask DynFloorLayers;

	
	public LayerMask WaterLayers;

	
	public Material RedMat;

	
	public Material ClearMat;

	
	public Material ClearMatFlatMesh;

	
	public Renderer MyRender;

	
	public bool Locked;

	
	public bool TreeStructure;

	
	public bool IgnoreBlock;

	
	public bool AllowFoundation;

	
	public bool Airborne;

	
	public bool Waterborne;

	
	public bool WaterborneExclusive;

	
	public bool Hydrophobic;

	
	public float FoundationMinSlope = 0.02f;

	
	public float maxBuildingHeight = 100f;

	
	public float maxAirBorneHeight = 25f;

	
	private bool _clearInternal = true;

	
	private bool _clearDynamicBuilding = true;

	
	private bool _lockDynamicBuilding = true;

	
	private bool _clearSmallStructures = true;

	
	private bool shouldDoDynamicBuildingCheck;

	
	private bool shouldDoSmallStructureCheck;

	
	private bool _inExclusionGroup;

	
	private RaycastHit hit;

	
	private Terrain activeTerrain;

	
	private BoxCollider boxCollider;

	
	private float turn;

	
	private float curDir;

	
	private Vector3 curNormal = Vector3.up;

	
	private Quaternion curGroundTilt = Quaternion.identity;

	
	private Vector3 sinkHolePos;

	
	private Vector3 lookingAtPointCam;

	
	private Vector3 startLocalPosition;
}
