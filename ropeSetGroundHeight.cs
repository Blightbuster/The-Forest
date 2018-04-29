using System;
using System.Collections;
using UnityEngine;


public class ropeSetGroundHeight : MonoBehaviour
{
	
	private IEnumerator Start()
	{
		if (!this.disableGroundHeightCheck)
		{
			yield return YieldPresets.WaitPointFiveSeconds;
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
		activateClimbTop componentInChildren = base.transform.GetComponentInChildren<activateClimbTop>();
		if (componentInChildren)
		{
			float num2 = Vector3.Distance(componentInChildren.transform.position, this.triggerBottom.transform.position);
			if (num2 < 3f)
			{
				Transform transform = null;
				MeshFilter[] componentsInChildren = base.transform.GetComponentsInChildren<MeshFilter>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (componentsInChildren[i].name.Equals("rope"))
					{
						transform = componentsInChildren[i].transform;
					}
				}
				if (transform)
				{
					if (transform.parent.GetComponent<LODGroup>())
					{
						transform.localPosition = new Vector3(transform.localPosition.x, -0.38f, transform.localPosition.z);
						transform.localScale = new Vector3(1f, 0.0806f, 1f);
					}
					else
					{
						transform.localPosition = new Vector3(transform.localPosition.x, 15.95f, transform.localPosition.z);
						transform.localScale = new Vector3(1f, 0.0819f, 1f);
					}
				}
				componentInChildren.gameObject.SetActive(false);
				this.triggerBottom.SetActive(false);
			}
		}
	}

	
	public LayerMask floorLayers;

	
	public GameObject triggerBottom;

	
	public bool disableGroundHeightCheck;
}
