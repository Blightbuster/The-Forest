using System;
using TheForest.Buildings.Interfaces;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	public class BuildingRepair : MonoBehaviour
	{
		
		private void Awake()
		{
			this._icon.gameObject.SetActive(false);
			base.enabled = false;
		}

		
		private void Update()
		{
			if (!LocalPlayer.RepairTool.IsEquipped && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World && !Scene.HudGui.AddIcon.activeSelf)
			{
				int collapsedLogs = this._target.CollapsedLogs;
				if (!LocalPlayer.Inventory.Logs.HasLogs || collapsedLogs == 0)
				{
					this.ShowIcon();
				}
			}
			else
			{
				this.HideIcon();
			}
			if (!LocalPlayer.Grabber.GetComponent<Collider>().bounds.Intersects(base.GetComponent<Collider>().bounds))
			{
				base.enabled = false;
				this.HideIcon();
			}
		}

		
		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Grabber"))
			{
				base.enabled = true;
				if (!LocalPlayer.RepairTool.IsEquipped && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World)
				{
					int collapsedLogs = this._target.CollapsedLogs;
					if (!LocalPlayer.Inventory.Logs.HasLogs || collapsedLogs > 0)
					{
						this.ShowIcon();
					}
				}
				else
				{
					this.HideIcon();
				}
			}
		}

		
		private void OnTriggerExit(Collider other)
		{
			if (other.CompareTag("Grabber"))
			{
				base.enabled = false;
				this.HideIcon();
			}
		}

		
		private void OnDisable()
		{
			this.HideIcon();
		}

		
		private void OnDestroy()
		{
			this.HideIcon();
		}

		
		private void ShowIcon()
		{
			Scene.HudGui.RepairMissingToolIcon.SetActive(true);
		}

		
		private void HideIcon()
		{
			if (Scene.HudGui)
			{
				Scene.HudGui.RepairMissingToolIcon.SetActive(false);
			}
		}

		
		public IRepairableStructure _target;

		
		public GUITexture _icon;

		
		private Vector3 _iconPos;
	}
}
