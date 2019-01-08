using System;
using System.Collections;
using Bolt;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	public class InOutWaterSwap : MonoBehaviour
	{
		private IEnumerator Start()
		{
			base.enabled = false;
			yield return null;
			GameObject trigger = this._outWaterGo.transform.GetChild(0).Find("Trigger").gameObject;
			GameObject trigger2 = this._inWaterGo.transform.GetChild(0).Find("Trigger").gameObject;
			trigger.SetActive(false);
			trigger2.SetActive(false);
			Craft_Structure tmpCraftStructure = trigger.GetComponent<Craft_Structure>();
			Craft_Structure tmpCraftStructure2 = trigger2.GetComponent<Craft_Structure>();
			LocalPlayer.Create.CraftStructures.Add(tmpCraftStructure);
			LocalPlayer.Create.CraftStructures.Add(tmpCraftStructure2);
			yield return null;
			this._outWaterGo.SetActive(false);
			this._inWaterGo.SetActive(false);
			this._inWaterRenderer = this._outWaterGo.transform.GetChild(0).GetComponent<Renderer>();
			this._outWaterRenderer = this._inWaterGo.transform.GetChild(0).GetComponent<Renderer>();
			base.enabled = true;
			yield break;
		}

		private void Update()
		{
			bool flag = LocalPlayer.FpCharacter.Diving || LocalPlayer.FpCharacter.swimming;
			RaycastHit raycastHit = default(RaycastHit);
			if (flag || Physics.Raycast(((this._origin != InOutWaterSwap.RayCastOrigins.Player) ? LocalPlayer.Create.BuildingPlacer.transform.position : LocalPlayer.MainCamTr.position) + new Vector3(0f, this._raycastDistance / 2f), Vector3.down, out raycastHit, (!this._inWaterGo.activeSelf) ? this._raycastDistance : (this._raycastDistance * 2f), this._groundLayers | this._waterLayers))
			{
				bool flag2 = flag || (1 << raycastHit.collider.gameObject.layer & this._waterLayers) != 0;
				if (flag2)
				{
					this._inWaterGo.SetActive(true);
					this._outWaterGo.SetActive(false);
					if (this._outWaterHasCustomPlace)
					{
						LocalPlayer.Create.Grabber.ClosePlace();
					}
					else
					{
						LocalPlayer.Create.Grabber.ShowPlace();
					}
					LocalPlayer.Create.BuildingPlacer.SetClear();
					LocalPlayer.Create.BuildingPlacer.SetRenderer(this._outWaterRenderer);
				}
				else if (this._inWaterGo.activeSelf || !this._outWaterGo.activeSelf)
				{
					this._inWaterGo.SetActive(false);
					this._outWaterGo.SetActive(true);
					if (this._inWaterHasCustomPlace)
					{
						LocalPlayer.Create.Grabber.ClosePlace();
					}
					else
					{
						LocalPlayer.Create.Grabber.ShowPlace();
					}
					LocalPlayer.Create.BuildingPlacer.SetRenderer(this._inWaterRenderer);
				}
			}
			else if (!this._outWaterGo.activeSelf)
			{
				this._inWaterGo.SetActive(false);
				this._outWaterGo.SetActive(true);
				if (this._inWaterHasCustomPlace)
				{
					LocalPlayer.Create.Grabber.ClosePlace();
				}
				else
				{
					LocalPlayer.Create.Grabber.ShowPlace();
				}
				LocalPlayer.Create.BuildingPlacer.SetRenderer(this._inWaterRenderer);
			}
		}

		private void OnPlaced()
		{
			base.enabled = false;
			Transform child;
			if (this._inWaterGo.activeSelf)
			{
				child = this._inWaterGo.transform.GetChild(0);
			}
			else
			{
				child = this._outWaterGo.transform.GetChild(0);
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
				if ((this._inWaterGo.activeSelf && this._initializeOutWater) || (this._outWaterGo.activeSelf && this._initializeInWater))
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

		public LayerMask _groundLayers;

		public LayerMask _waterLayers;

		public InOutWaterSwap.RayCastOrigins _origin;

		public float _raycastDistance = 3f;

		public GameObject _outWaterGo;

		public GameObject _inWaterGo;

		public bool _outWaterHasCustomPlace;

		public bool _inWaterHasCustomPlace;

		public bool _initializeOutWater;

		public bool _initializeInWater;

		private Renderer _outWaterRenderer;

		private Renderer _inWaterRenderer;

		public enum RayCastOrigins
		{
			Player,
			Placer
		}
	}
}
