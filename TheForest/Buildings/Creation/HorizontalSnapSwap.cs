using System;
using System.Collections;
using Bolt;
using TheForest.Buildings.Interfaces;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	public class HorizontalSnapSwap : MonoBehaviour
	{
		
		private IEnumerator Start()
		{
			base.enabled = false;
			this._offsetWithPlacer = base.transform.localPosition;
			yield return null;
			GameObject trigger = this._freeGo.transform.GetChild(0).Find("Trigger").gameObject;
			GameObject trigger2 = this._snappedGo.transform.GetChild(0).Find("Trigger").gameObject;
			trigger.SetActive(false);
			trigger2.SetActive(false);
			Craft_Structure tmpCraftStructure = trigger.GetComponent<Craft_Structure>();
			Craft_Structure tmpCraftStructure2 = trigger2.GetComponent<Craft_Structure>();
			LocalPlayer.Create.CraftStructures.Add(tmpCraftStructure);
			LocalPlayer.Create.CraftStructures.Add(tmpCraftStructure2);
			if (this._snappedIsMultipointStructure)
			{
				this._snappedGoISS = this._snappedGo.transform.GetChild(0).GetComponent<IStructureSupport>();
			}
			if (this._freeIsMultipointStructure)
			{
				this._freeGoISS = this._freeGo.transform.GetChild(0).GetComponent<IStructureSupport>();
			}
			yield return null;
			this._snappedGo.SetActive(false);
			this._freeGo.SetActive(false);
			this._snappedRenderer = this._snappedGo.transform.GetChild(0).GetComponent<Renderer>();
			this._freeRenderer = this._freeGo.transform.GetChild(0).GetComponent<Renderer>();
			base.enabled = true;
			yield break;
		}

		
		private void Update()
		{
			float num = (!this._snappedGo.activeSelf) ? this._raycastDistance : (this._raycastDistance * 2f);
			Vector3 vector = (this._origin != HorizontalSnapSwap.RayCastOrigins.Player) ? LocalPlayer.Create.BuildingPlacer.transform.position : LocalPlayer.MainCamTr.position;
			if (!LocalPlayer.IsInCaves)
			{
				float num2 = Terrain.activeTerrain.SampleHeight(vector);
				if (num2 > vector.y)
				{
					vector.y = num2;
				}
			}
			bool flag = this._freeIsMultipointStructure && this._freeGoISS.GetMultiPointsPositions(true).Count > 0;
			bool flag2 = this._snappedIsMultipointStructure && this._snappedGoISS.GetMultiPointsPositions(true).Count > 0;
			RaycastHit raycastHit;
			if (!flag && Physics.SphereCast(vector + Vector3.up * (num / 2f), 1f, Vector3.down, out raycastHit, num, this._layers.value))
			{
				bool flag3 = (double)Mathf.Abs(raycastHit.normal.y) > 0.5;
				if (flag3 || flag2)
				{
					if (this._origin == HorizontalSnapSwap.RayCastOrigins.Player)
					{
						base.transform.parent = null;
						base.transform.position = raycastHit.point;
					}
					if (flag3)
					{
						raycastHit.normal.y = 0f;
						base.transform.localRotation = Quaternion.identity;
					}
					this._snappedGo.SetActive(true);
					this._freeGo.SetActive(false);
					if (this._snappedHasCustomPlace)
					{
						LocalPlayer.Create.Grabber.ClosePlace();
					}
					else
					{
						LocalPlayer.Create.Grabber.ShowPlace();
					}
					LocalPlayer.Create.BuildingPlacer.SetClear();
					if (this._setSnappedRenderer)
					{
						LocalPlayer.Create.BuildingPlacer.SetRenderer(this._snappedRenderer);
					}
					LocalPlayer.Create.BuildingPlacer.ApplyLeaningPosRot(base.transform, this._snappedIsAirborne, false);
					Scene.HudGui.RoofConstructionIcons.Shutdown();
				}
				else if (this._snappedGo.activeSelf)
				{
					this._snappedGo.SetActive(false);
					this._freeGo.SetActive(true);
					if (this._freeHasCustomPlace)
					{
						LocalPlayer.Create.Grabber.ClosePlace();
					}
					else
					{
						LocalPlayer.Create.Grabber.ShowPlace();
					}
					LocalPlayer.Create.BuildingPlacer.Airborne = this._freeIsAirborne;
					if (this._setFreeRenderer)
					{
						LocalPlayer.Create.BuildingPlacer.SetRenderer(this._freeRenderer);
					}
					base.transform.parent = LocalPlayer.Create.BuildingPlacer.transform;
					base.transform.localPosition = this._offsetWithPlacer;
					base.transform.localRotation = Quaternion.identity;
					Scene.HudGui.RoofConstructionIcons.Shutdown();
				}
			}
			else if (!this._freeGo.activeSelf)
			{
				this._snappedGo.SetActive(false);
				this._freeGo.SetActive(true);
				if (this._freeHasCustomPlace)
				{
					LocalPlayer.Create.Grabber.ClosePlace();
				}
				else
				{
					LocalPlayer.Create.Grabber.ShowPlace();
				}
				LocalPlayer.Create.BuildingPlacer.Airborne = this._freeIsAirborne;
				if (this._setFreeRenderer)
				{
					LocalPlayer.Create.BuildingPlacer.SetRenderer(this._freeRenderer);
				}
				base.transform.parent = LocalPlayer.Create.BuildingPlacer.transform;
				base.transform.localPosition = this._offsetWithPlacer;
				base.transform.localRotation = Quaternion.identity;
				Scene.HudGui.RoofConstructionIcons.Shutdown();
			}
		}

		
		private void OnPlaced()
		{
			base.enabled = false;
			Transform child;
			if (this._snappedGo.activeSelf)
			{
				child = this._snappedGo.transform.GetChild(0);
			}
			else
			{
				child = this._freeGo.transform.GetChild(0);
			}
			if (!BoltNetwork.isRunning)
			{
				if (LocalPlayer.Create.BuildingPlacer.LastHit != null && LocalPlayer.Create.BuildingPlacer.LastHit.Value.transform.GetComponentInParent<BoltEntity>())
				{
					child.parent = LocalPlayer.Create.BuildingPlacer.LastHit.Value.transform.GetComponentInParent<BoltEntity>().transform;
				}
				else if (LocalPlayer.Create.ParentEntity)
				{
					DynamicBuilding component = LocalPlayer.Create.ParentEntity.GetComponent<DynamicBuilding>();
					child.transform.parent = ((!component || !component._parentOverride) ? LocalPlayer.Create.ParentEntity.transform : component._parentOverride);
				}
				else
				{
					child.parent = null;
				}
				child.SendMessage("OnPlaced", SendMessageOptions.DontRequireReceiver);
				GameObject gameObject = child.Find("Trigger").gameObject;
				gameObject.SetActive(true);
				if ((this._snappedGo.activeSelf && this._initializeSnapped) || (this._freeGo.activeSelf && this._initializeAirborne))
				{
					gameObject.GetComponent<Craft_Structure>().Initialize();
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				CoopConstructionEx component2 = child.GetComponent<CoopConstructionEx>();
				if (component2)
				{
					BoltEntity component3 = child.GetComponent<BoltEntity>();
					BoltEntity boltEntity = LocalPlayer.Create.GetParentEntity(child.gameObject) ?? LocalPlayer.Create.ParentEntity;
					component2.SendMessage("OnSerializing");
					CoopConstructionExToken coopConstructionExToken = LocalPlayer.Create.GetCoopConstructionExToken(component2, boltEntity);
					PlaceFoundationEx placeFoundationEx = PlaceFoundationEx.Create(GlobalTargets.OnlyServer);
					placeFoundationEx.Parent = boltEntity;
					placeFoundationEx.Position = child.transform.position;
					placeFoundationEx.Prefab = component3.prefabId;
					placeFoundationEx.Token = coopConstructionExToken;
					placeFoundationEx.Send();
				}
				else
				{
					PlaceConstruction placeConstruction = PlaceConstruction.Create(GlobalTargets.OnlyServer);
					if (LocalPlayer.Create.BuildingPlacer.LastHit != null)
					{
						placeConstruction.Parent = LocalPlayer.Create.BuildingPlacer.LastHit.Value.transform.GetComponentInParent<BoltEntity>();
					}
					placeConstruction.PrefabId = child.GetComponent<BoltEntity>().prefabId;
					placeConstruction.Position = child.position;
					placeConstruction.Rotation = child.rotation;
					FoundationArchitect component4 = child.GetComponent<FoundationArchitect>();
					if (component4)
					{
						placeConstruction.AboveGround = component4._aboveGround;
					}
					placeConstruction.Send();
				}
				UnityEngine.Object.Destroy(base.gameObject, 0.05f);
			}
		}

		
		public LayerMask _layers;

		
		public HorizontalSnapSwap.RayCastOrigins _origin;

		
		public float _raycastDistance = 5f;

		
		public GameObject _snappedGo;

		
		public GameObject _freeGo;

		
		public bool _snappedIsAirborne;

		
		public bool _freeIsAirborne;

		
		public bool _snappedHasCustomPlace;

		
		public bool _freeHasCustomPlace;

		
		public bool _initializeSnapped;

		
		public bool _initializeAirborne;

		
		public bool _setSnappedRenderer = true;

		
		public bool _setFreeRenderer = true;

		
		public bool _snappedIsMultipointStructure = true;

		
		public bool _freeIsMultipointStructure = true;

		
		private Vector3 _offsetWithPlacer;

		
		private Renderer _snappedRenderer;

		
		private Renderer _freeRenderer;

		
		private IStructureSupport _snappedGoISS;

		
		private IStructureSupport _freeGoISS;

		
		public enum RayCastOrigins
		{
			
			Player,
			
			Placer
		}
	}
}
