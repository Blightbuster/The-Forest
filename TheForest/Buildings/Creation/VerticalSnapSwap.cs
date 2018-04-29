using System;
using System.Collections;
using Bolt;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	public class VerticalSnapSwap : MonoBehaviour
	{
		
		private IEnumerator Start()
		{
			base.enabled = false;
			this._offsetWithPlacer = base.transform.localPosition;
			yield return YieldPresets.WaitForEndOfFrame;
			GameObject trigger = this._freeGo.transform.GetChild(0).Find("Trigger").gameObject;
			trigger.SetActive(false);
			GameObject trigger2 = this._snappedGo.transform.GetChild(0).Find("Trigger").gameObject;
			trigger2.SetActive(false);
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
			RaycastHit raycastHit;
			if (Physics.Raycast((this._origin != VerticalSnapSwap.RayCastOrigins.Player) ? base.transform.position : LocalPlayer.MainCamTr.position, LocalPlayer.MainCamTr.forward, out raycastHit, this._raycastDistance, this._layers.value))
			{
				bool flag = (double)Mathf.Abs(raycastHit.normal.y) < 0.5;
				if (flag)
				{
					if (this._origin == VerticalSnapSwap.RayCastOrigins.Player)
					{
						base.transform.parent = null;
						base.transform.position = raycastHit.point;
					}
					if (flag)
					{
						Vector3 normal = raycastHit.normal;
						normal.y = 0f;
						base.transform.rotation = Quaternion.Euler(0f, LocalPlayer.Create.BuildingPlacer.transform.rotation.y, 0f) * Quaternion.LookRotation(normal);
					}
					this._snappedGo.SetActive(true);
					this._freeGo.SetActive(false);
					LocalPlayer.Create.BuildingPlacer.SetRenderer(this._snappedRenderer);
				}
				else if (this._snappedGo.activeSelf)
				{
					this._snappedGo.SetActive(false);
					this._freeGo.SetActive(true);
					LocalPlayer.Create.BuildingPlacer.SetRenderer(this._freeRenderer);
					base.transform.parent = LocalPlayer.Create.BuildingPlacer.transform;
					base.transform.localPosition = this._offsetWithPlacer;
				}
			}
			else if (!this._freeGo.activeSelf)
			{
				this._snappedGo.SetActive(false);
				this._freeGo.SetActive(true);
				LocalPlayer.Create.BuildingPlacer.SetRenderer(this._freeRenderer);
				base.transform.parent = LocalPlayer.Create.BuildingPlacer.transform;
				base.transform.localPosition = this._offsetWithPlacer;
			}
		}

		
		private IEnumerator OnPlaced()
		{
			base.enabled = false;
			yield return null;
			Transform ghost;
			if (this._snappedGo.activeSelf)
			{
				ghost = this._snappedGo.transform.GetChild(0);
			}
			else
			{
				ghost = this._freeGo.transform.GetChild(0);
			}
			if (!BoltNetwork.isRunning)
			{
				if (LocalPlayer.Create.BuildingPlacer.LastHit != null && LocalPlayer.Create.BuildingPlacer.LastHit.Value.transform.GetComponentInParent<BoltEntity>())
				{
					ghost.parent = LocalPlayer.Create.BuildingPlacer.LastHit.Value.transform.GetComponentInParent<BoltEntity>().transform;
				}
				else
				{
					ghost.parent = null;
				}
				ghost.SendMessage("OnPlaced", SendMessageOptions.DontRequireReceiver);
				GameObject trigger = ghost.Find("Trigger").gameObject;
				trigger.SetActive(true);
				if ((this._snappedGo.activeSelf && this._initializeSnapped) || (this._freeGo.activeSelf && this._initializeAirborne))
				{
					trigger.GetComponent<Craft_Structure>().Initialize();
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				PlaceConstruction ev = PlaceConstruction.Create(GlobalTargets.OnlyServer);
				if (LocalPlayer.Create.BuildingPlacer.LastHit != null)
				{
					ev.Parent = LocalPlayer.Create.BuildingPlacer.LastHit.Value.transform.GetComponentInParent<BoltEntity>();
				}
				ev.PrefabId = ghost.GetComponent<BoltEntity>().prefabId;
				ev.Position = ghost.position;
				ev.Rotation = ghost.rotation;
				FoundationArchitect fa = ghost.GetComponent<FoundationArchitect>();
				if (fa)
				{
					ev.AboveGround = fa._aboveGround;
				}
				ev.Send();
				UnityEngine.Object.Destroy(base.gameObject, 0.05f);
			}
			yield break;
		}

		
		public LayerMask _layers;

		
		public VerticalSnapSwap.RayCastOrigins _origin;

		
		public float _raycastDistance = 5f;

		
		public GameObject _snappedGo;

		
		public GameObject _freeGo;

		
		public bool _initializeSnapped;

		
		public bool _initializeAirborne;

		
		private Vector3 _offsetWithPlacer;

		
		private Renderer _snappedRenderer;

		
		private Renderer _freeRenderer;

		
		public enum RayCastOrigins
		{
			
			Player,
			
			Building
		}
	}
}
