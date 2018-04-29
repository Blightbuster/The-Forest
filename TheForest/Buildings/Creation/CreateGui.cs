using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	[AddComponentMenu("Buildings/Creation/Create GUI")]
	public class CreateGui : MonoBehaviour
	{
		
		private void Awake()
		{
			if (BoltNetwork.isRunning && this._blockInMP && this._blockedMaterial)
			{
				base.gameObject.GetComponent<Renderer>().sharedMaterial = this._blockedMaterial;
			}
			this.NotActive();
		}

		
		private void OnDisable()
		{
			this._wasDisabled = true;
		}

		
		private void OnDrawGizmosSelected()
		{
			if (this.rename)
			{
				this.rename = false;
				base.name = "GuiBuild" + this._buildingType;
			}
		}

		
		private void Update()
		{
			if (this._wasDisabled)
			{
				this._wasDisabled = false;
			}
			else if (!this._blockInMP || !BoltNetwork.isRunning)
			{
				if (this.ScreenRect().Contains(TheForest.Utils.Input.mousePosition) && !LocalPlayer.IsInOverlookArea)
				{
					this.Active();
					if (TheForest.Utils.Input.GetButtonDown("Fire1") || (TheForest.Utils.Input.IsGamePad && TheForest.Utils.Input.GetButtonDown("Take")))
					{
						LocalPlayer.Create.CreateBuilding((!LocalPlayer.IsInCaves || this._buildingTypeUnderground == BuildingTypes.None) ? this._buildingType : this._buildingTypeUnderground);
						LocalPlayer.Create.CloseTheBook(false);
					}
				}
				else
				{
					this.NotActive();
				}
			}
			else
			{
				this.Active();
			}
		}

		
		private void Active()
		{
			base.SendMessage("SetHovered", true, SendMessageOptions.DontRequireReceiver);
			this.SwitchMatColor(this._hoveredColor);
		}

		
		private void NotActive()
		{
			base.SendMessage("SetHovered", false, SendMessageOptions.DontRequireReceiver);
			base.GetComponent<Renderer>().enabled = false;
		}

		
		public Rect ScreenRect()
		{
			BoxCollider component = base.GetComponent<BoxCollider>();
			Vector3 a = base.transform.TransformVector(component.size);
			Vector3 vector = LocalPlayer.MainCam.WorldToScreenPoint(base.transform.position - a / 2f);
			Vector3 vector2 = LocalPlayer.MainCam.WorldToScreenPoint(base.transform.position + a / 2f);
			return new Rect(vector2.x, vector2.y, Mathf.Abs(vector2.x - vector.x), Mathf.Abs(vector2.y - vector.y));
		}

		
		private void SwitchMatColor(Color color)
		{
			if (this._myMatPropertyBlock == null)
			{
				this._myMatPropertyBlock = new MaterialPropertyBlock();
			}
			Renderer component = base.GetComponent<Renderer>();
			component.GetPropertyBlock(this._myMatPropertyBlock);
			this._myMatPropertyBlock.SetColor("_Color", color);
			this._myMatPropertyBlock.SetColor("_OutlineColor", color);
			component.SetPropertyBlock(this._myMatPropertyBlock);
			component.enabled = true;
		}

		
		public BuildingTypes _buildingType;

		
		public BuildingTypes _buildingTypeUnderground;

		
		public bool _blockInMP;

		
		public Material _blockedMaterial;

		
		public Color _hoveredColor;

		
		private Color _defaultColor;

		
		private bool _wasDisabled;

		
		private MaterialPropertyBlock _myMatPropertyBlock;

		
		public bool rename;
	}
}
