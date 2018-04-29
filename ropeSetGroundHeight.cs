using System;
using System.Collections;
using UnityEngine;


public class ropeSetGroundHeight : MonoBehaviour
{
	
	private IEnumerator Start()
	{
		if (!this.disableGroundHeightCheck)
		{
			yield return YieldPresets.WaitOneSecond;
			this.setGroundTriggerHeight();
		}
		yield break;
	}

	
	private void setGroundTriggerHeight()
	{
		if (this.floorLayers == 0)
		{
			Vector3 position = this.triggerBottom.transform.position;
			position.y = Terrain.activeTerrain.SampleHeight(this.triggerBottom.transform.position) + Terrain.activeTerrain.transform.position.y + 3.5f;
			this.triggerBottom.transform.position = position;
		}
		else
		{
			Vector3 position2 = this.triggerBottom.transform.position;
			position2.y = base.transform.position.y;
			RaycastHit raycastHit;
			if (Physics.Raycast(position2, Vector3.down, out raycastHit, 16.5f, this.floorLayers))
			{
				position2.y = raycastHit.point.y + 3.5f;
				this.triggerBottom.transform.position = position2;
			}
			else
			{
				float num = Terrain.activeTerrain.SampleHeight(base.transform.position);
				if (position2.y < num && base.transform.position.y > num)
				{
					position2.y = num + 3.5f;
					this.triggerBottom.transform.position = position2;
				}
			}
		}
	}

	
	public LayerMask floorLayers;

	
	public GameObject triggerBottom;

	
	public bool disableGroundHeightCheck;
}
