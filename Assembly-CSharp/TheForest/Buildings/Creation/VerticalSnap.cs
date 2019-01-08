using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	public class VerticalSnap : MonoBehaviour
	{
		private IEnumerator Start()
		{
			if (base.transform.parent != null && LocalPlayer.Create && LocalPlayer.Create.BuildingPlacer && base.transform.parent == LocalPlayer.Create.BuildingPlacer.transform)
			{
				this._offsetWithPlacer = base.transform.localPosition;
				yield return null;
				LocalPlayer.Create.Grabber.ClosePlace();
			}
			else
			{
				base.enabled = false;
			}
			yield break;
		}

		private void Update()
		{
			if (this._allowInTree)
			{
				LocalPlayer.Create.TargetTree = null;
			}
			RaycastHit value;
			if (Physics.Raycast((this._origin != VerticalSnap.RayCastOrigins.Player) ? base.transform.position : LocalPlayer.MainCamTr.position, LocalPlayer.MainCamTr.forward, out value, this._raycastDistance, this._wallLayers.value))
			{
				DynamicBuilding componentInParent = value.collider.GetComponentInParent<DynamicBuilding>();
				bool flag = (double)Mathf.Abs(value.normal.y) < 0.5;
				bool flag2 = !componentInParent || componentInParent._allowParenting;
				if (this._origin == VerticalSnap.RayCastOrigins.Player)
				{
					base.transform.parent = null;
					base.transform.position = value.point;
				}
				if (!this._verticalOnly)
				{
					if (this._origin == VerticalSnap.RayCastOrigins.Player)
					{
						base.transform.rotation = Quaternion.FromToRotation(Vector3.up, value.normal) * LocalPlayer.Create.BuildingPlacer.transform.rotation;
						Scene.HudGui.RotateIcon.SetActive(true);
					}
				}
				else if (flag)
				{
					Vector3 normal = value.normal;
					normal.y = 0f;
					base.transform.rotation = Quaternion.Euler(0f, LocalPlayer.Create.BuildingPlacer.transform.rotation.y, 0f) * Quaternion.LookRotation(normal);
				}
				if (flag2 && this._allowInTree)
				{
					if (value.collider.CompareTag("conTree"))
					{
						LocalPlayer.Create.TargetTree = value.collider.transform;
					}
					else if (value.collider.CompareTag("Tree"))
					{
						LocalPlayer.Create.TargetTree = value.collider.transform;
					}
				}
				if (flag2 && (!this._verticalOnly || flag))
				{
					LocalPlayer.Create.BuildingPlacer.LastHit = new RaycastHit?(value);
					LocalPlayer.Create.BuildingPlacer.SetClear();
					Scene.HudGui.PlaceIcon.SetActive(true);
					Scene.HudGui.RotateIcon.SetActive(!this._verticalOnly);
				}
				else
				{
					LocalPlayer.Create.BuildingPlacer.SetNotclear();
					Scene.HudGui.PlaceIcon.SetActive(false);
					Scene.HudGui.RotateIcon.SetActive(true);
				}
			}
			else if (base.transform.parent == null)
			{
				base.transform.parent = LocalPlayer.Create.BuildingPlacer.transform;
				base.transform.localPosition = this._offsetWithPlacer;
				if (this._verticalOnly)
				{
					LocalPlayer.Create.BuildingPlacer.SetNotclear();
					Scene.HudGui.PlaceIcon.SetActive(false);
					Scene.HudGui.RotateIcon.SetActive(true);
				}
			}
			else
			{
				LocalPlayer.Create.BuildingPlacer.SetNotclear();
				Scene.HudGui.PlaceIcon.SetActive(false);
				Scene.HudGui.RotateIcon.SetActive(true);
			}
		}

		private void OnDestroy()
		{
			if (base.enabled)
			{
				if (this._allowInTree)
				{
					LocalPlayer.Create.TargetTree = null;
				}
				Scene.HudGui.PlaceIcon.SetActive(false);
				Scene.HudGui.RotateIcon.SetActive(false);
			}
		}

		private void OnPlacingRemotely()
		{
			this.OnPlaced();
		}

		private void OnPlaced()
		{
			base.enabled = false;
			Scene.HudGui.PlaceIcon.SetActive(false);
			Scene.HudGui.RotateIcon.SetActive(false);
		}

		private void OnDeserialized()
		{
			base.enabled = false;
		}

		public bool _verticalOnly;

		public bool _allowInTree;

		public LayerMask _wallLayers;

		public VerticalSnap.RayCastOrigins _origin;

		public float _raycastDistance = 5f;

		private Vector3 _offsetWithPlacer;

		public enum RayCastOrigins
		{
			Player,
			Building
		}
	}
}
