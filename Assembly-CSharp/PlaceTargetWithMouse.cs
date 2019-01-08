using System;
using UnityEngine;

public class PlaceTargetWithMouse : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit raycastHit;
			if (Physics.Raycast(ray, out raycastHit))
			{
				base.transform.position = raycastHit.point + raycastHit.normal * this.surfaceOffset;
				if (this.setTargetOn != null)
				{
					this.setTargetOn.SendMessage("SetTarget", base.transform);
				}
			}
		}
	}

	public float surfaceOffset = 1.5f;

	public GameObject setTargetOn;
}
