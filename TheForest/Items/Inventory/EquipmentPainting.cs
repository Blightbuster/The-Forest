using System;
using UnityEngine;

namespace TheForest.Items.Inventory
{
	
	[DoNotSerializePublic]
	[AddComponentMenu("Items/World/Equipement Painting (held)")]
	public class EquipmentPainting : MonoBehaviour
	{
		
		public void ApplyColor()
		{
			EquipmentPainting.Colors color = this._color;
			if (color != EquipmentPainting.Colors.Green)
			{
				if (color == EquipmentPainting.Colors.Orange)
				{
					this.PaintInOrange();
				}
			}
			else
			{
				this.PaintInGreen();
			}
		}

		
		public void PaintInGreen()
		{
			this._color = EquipmentPainting.Colors.Green;
			foreach (Renderer renderer in this._renderers)
			{
				renderer.sharedMaterial = this._greenMat;
			}
			Bloodify component = base.GetComponent<Bloodify>();
			if (component)
			{
				UnityEngine.Object.Destroy(component);
			}
		}

		
		public void PaintInOrange()
		{
			this._color = EquipmentPainting.Colors.Orange;
			foreach (Renderer renderer in this._renderers)
			{
				renderer.sharedMaterial = this._orangeMat;
			}
			Bloodify component = base.GetComponent<Bloodify>();
			if (component)
			{
				UnityEngine.Object.Destroy(component);
			}
		}

		
		
		
		public EquipmentPainting.Colors Color
		{
			get
			{
				return this._color;
			}
			set
			{
				this._color = value;
				this.ApplyColor();
			}
		}

		
		public Material _greenMat;

		
		public Material _orangeMat;

		
		public Renderer[] _renderers;

		
		[SerializeThis]
		private EquipmentPainting.Colors _color;

		
		public enum Colors
		{
			
			Default,
			
			Green,
			
			Orange
		}
	}
}
