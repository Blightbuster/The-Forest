using System;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

public class LaserPointer : MonoBehaviour
{
	private void Update()
	{
		this.Length = this.MaxCastDistance;
		this._showEnd = false;
		PlayerInventory.PlayerViews currentView = LocalPlayer.CurrentView;
		switch (currentView)
		{
		case PlayerInventory.PlayerViews.Loading:
		case PlayerInventory.PlayerViews.Inventory:
			break;
		default:
			if (currentView != PlayerInventory.PlayerViews.Pause)
			{
				if (LocalPlayer.Create)
				{
					this.DoCreateUpdate();
				}
				goto IL_60;
			}
			break;
		}
		this.DoInventoryAndMenusUpdate();
		IL_60:
		this.UpdateLaser();
		this.UpdateEnd();
		this.UpdateLayer();
	}

	private void DoCreateUpdate()
	{
		if (this.Laser.IsNull() || LocalPlayer.Create.IsNull() || LocalPlayer.Create.BuildingPlacer.IsNull())
		{
			return;
		}
		this.Length = Vector3.Distance(this.Laser.position, LocalPlayer.Create.BuildingPlacer.transform.position) / base.transform.lossyScale.z;
		this._showEnd = true;
	}

	private void DoInventoryAndMenusUpdate()
	{
		if (this.Laser.IsNull())
		{
			return;
		}
		RaycastHit raycastHit;
		if (!Physics.SphereCast(this.Laser.position, 0.05f, this.Laser.forward, out raycastHit, this.MaxCastDistance, this.InventoryCastLayers, QueryTriggerInteraction.Ignore))
		{
			return;
		}
		if (!LocalPlayer.IsInInventory)
		{
			Vector3 lastRenderTextureHitViewportPos = raycastHit.collider.transform.InverseTransformPoint(raycastHit.point);
			lastRenderTextureHitViewportPos.x = (lastRenderTextureHitViewportPos.x * raycastHit.collider.transform.lossyScale.x * -1f + 5f) / 10f;
			lastRenderTextureHitViewportPos.y = (lastRenderTextureHitViewportPos.z * raycastHit.collider.transform.lossyScale.z * -1f + 5f) / 10f;
			lastRenderTextureHitViewportPos.z = 1000f;
			LaserPointer.LastRenderTextureHitViewportPos = lastRenderTextureHitViewportPos;
		}
		this.Length = raycastHit.distance / base.transform.lossyScale.z;
		this._showEnd = true;
	}

	private void UpdateLaser()
	{
		if (this.Laser != null)
		{
			this.Laser.localScale = new Vector3(this.Laser.localScale.x, this.Laser.localScale.y, this.Length);
		}
	}

	private void UpdateEnd()
	{
		if (this.End == null)
		{
			return;
		}
		this.End.localPosition = this.Reference.localPosition + Vector3.forward * this.Length;
		if (this.End.gameObject.activeSelf == this._showEnd)
		{
			return;
		}
		this.End.gameObject.SetActive(this._showEnd);
	}

	private void UpdateLayer()
	{
		this.Laser.gameObject.layer = ((!LocalPlayer.IsInPauseMenu) ? LayerMask.NameToLayer("Player") : LayerMask.NameToLayer("HUD"));
		this.End.gameObject.layer = ((!LocalPlayer.IsInPauseMenu) ? LayerMask.NameToLayer("Player") : LayerMask.NameToLayer("HUD"));
	}

	public float Length = 1f;

	public Transform Reference;

	public Transform Laser;

	public Transform End;

	public float MaxCastDistance = 18.8f;

	public LayerMask InventoryCastLayers;

	public LayerMask PlaceGhostCastLayers;

	private bool _showEnd;

	public static Vector3 LastRenderTextureHitViewportPos;
}
