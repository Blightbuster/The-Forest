using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	public class CeilingSnap : MonoBehaviour
	{
		
		private IEnumerator Start()
		{
			if (base.transform.parent != null && LocalPlayer.Create && LocalPlayer.Create.BuildingPlacer && base.transform.parent == LocalPlayer.Create.BuildingPlacer.transform)
			{
				this._offsetWithPlacer = base.transform.localPosition;
				yield return null;
				LocalPlayer.Create.Grabber.ClosePlace();
				Scene.HudGui.RotateIcon.SetActive(true);
			}
			else
			{
				base.enabled = false;
			}
			yield break;
		}

		
		private void Update()
		{
			RaycastHit value;
			if (Physics.Raycast(LocalPlayer.Create.BuildingPlacer.transform.position + Vector3.down, Vector3.up, out value, this._raycastDistance, this._wallLayers.value))
			{
				bool flag = value.normal.y < -0.4f;
				base.transform.parent = null;
				base.transform.position = value.point;
				if (flag)
				{
					base.transform.rotation = LocalPlayer.Create.BuildingPlacer.transform.rotation;
					LocalPlayer.Create.BuildingPlacer.LastHit = new RaycastHit?(value);
					LocalPlayer.Create.BuildingPlacer.SetClear();
					Scene.HudGui.PlaceIcon.SetActive(true);
				}
				else
				{
					LocalPlayer.Create.BuildingPlacer.SetNotclear();
					Scene.HudGui.PlaceIcon.SetActive(false);
				}
			}
			else if (base.transform.parent == null)
			{
				base.transform.parent = LocalPlayer.Create.BuildingPlacer.transform;
				base.transform.localPosition = this._offsetWithPlacer;
				LocalPlayer.Create.BuildingPlacer.SetNotclear();
				Scene.HudGui.PlaceIcon.SetActive(false);
			}
			else
			{
				LocalPlayer.Create.BuildingPlacer.SetNotclear();
				Scene.HudGui.PlaceIcon.SetActive(false);
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

		
		public LayerMask _wallLayers;

		
		public float _raycastDistance = 2f;

		
		private Vector3 _offsetWithPlacer;
	}
}
