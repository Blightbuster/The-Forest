using System;
using UnityEngine;


public class SteamVR_Teleporter : MonoBehaviour
{
	
	
	private Transform reference
	{
		get
		{
			SteamVR_Camera steamVR_Camera = SteamVR_Render.Top();
			return (!(steamVR_Camera != null)) ? null : steamVR_Camera.origin;
		}
	}

	
	private void Start()
	{
		SteamVR_TrackedController steamVR_TrackedController = base.GetComponent<SteamVR_TrackedController>();
		if (steamVR_TrackedController == null)
		{
			steamVR_TrackedController = base.gameObject.AddComponent<SteamVR_TrackedController>();
		}
		steamVR_TrackedController.TriggerClicked += this.DoClick;
		if (this.teleportType == SteamVR_Teleporter.TeleportType.TeleportTypeUseTerrain)
		{
			Transform reference = this.reference;
			if (reference != null)
			{
				reference.position = new Vector3(reference.position.x, Terrain.activeTerrain.SampleHeight(reference.position), reference.position.z);
			}
		}
	}

	
	private void DoClick(object sender, ClickedEventArgs e)
	{
		if (this.teleportOnClick)
		{
			Transform reference = this.reference;
			if (reference == null)
			{
				return;
			}
			float y = reference.position.y;
			Plane plane = new Plane(Vector3.up, -y);
			Ray ray = new Ray(base.transform.position, base.transform.forward);
			float d = 0f;
			bool flag;
			if (this.teleportType == SteamVR_Teleporter.TeleportType.TeleportTypeUseTerrain)
			{
				TerrainCollider component = Terrain.activeTerrain.GetComponent<TerrainCollider>();
				RaycastHit raycastHit;
				flag = component.Raycast(ray, out raycastHit, 1000f);
				d = raycastHit.distance;
			}
			else if (this.teleportType == SteamVR_Teleporter.TeleportType.TeleportTypeUseCollider)
			{
				RaycastHit raycastHit2;
				flag = Physics.Raycast(ray, out raycastHit2);
				d = raycastHit2.distance;
			}
			else
			{
				flag = plane.Raycast(ray, out d);
			}
			if (flag)
			{
				Vector3 b = new Vector3(SteamVR_Render.Top().head.position.x, y, SteamVR_Render.Top().head.position.z);
				reference.position = reference.position + (ray.origin + ray.direction * d) - b;
			}
		}
	}

	
	public bool teleportOnClick;

	
	public SteamVR_Teleporter.TeleportType teleportType = SteamVR_Teleporter.TeleportType.TeleportTypeUseZeroY;

	
	public enum TeleportType
	{
		
		TeleportTypeUseTerrain,
		
		TeleportTypeUseCollider,
		
		TeleportTypeUseZeroY
	}
}
