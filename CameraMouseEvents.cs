using System;
using TheForest.Utils;
using UnityEngine;


public class CameraMouseEvents : MonoBehaviour
{
	
	private void Awake()
	{
		this.cam = base.GetComponent<Camera>();
	}

	
	private void Update()
	{
		RaycastHit raycastHit = default(RaycastHit);
		Ray ray = (!this.cam) ? LocalPlayer.MainCam.ScreenPointToRay(TheForest.Utils.Input.mousePosition) : this.cam.ScreenPointToRay(TheForest.Utils.Input.mousePosition);
		GameObject gameObject = (!Physics.Raycast(ray, out raycastHit, 100f, this.layerMask) && (!this.useSpherecast || !TheForest.Utils.Input.IsGamePad || !Physics.SphereCast(ray, this.radius, out raycastHit, 100f, this.layerMask))) ? null : raycastHit.collider.gameObject;
		if (gameObject != this.selectedObject)
		{
			if (this.selectedObject)
			{
				if (this.isMouseDown)
				{
					this.selectedObject.SendMessage("OnMouseUpCollider", SendMessageOptions.DontRequireReceiver);
					this.isMouseDown = false;
				}
				this.selectedObject.SendMessage("OnMouseExitCollider", SendMessageOptions.DontRequireReceiver);
			}
			if (gameObject)
			{
				gameObject.SendMessage("OnMouseEnterCollider", SendMessageOptions.DontRequireReceiver);
			}
			this.selectedObject = gameObject;
		}
		if (this.selectedObject)
		{
			this.selectedObject.SendMessage("OnMouseOverCollider", SendMessageOptions.DontRequireReceiver);
			if (TheForest.Utils.Input.GetButtonDown("Fire1"))
			{
				this.selectedObject.SendMessage("OnMouseDownCollider", SendMessageOptions.DontRequireReceiver);
				this.isMouseDown = true;
			}
			if (TheForest.Utils.Input.GetButtonUp("Fire1"))
			{
				this.selectedObject.SendMessage("OnMouseUpCollider", SendMessageOptions.DontRequireReceiver);
				this.isMouseDown = false;
			}
		}
	}

	
	private void OnDisable()
	{
		if (this.selectedObject)
		{
			if (this.isMouseDown)
			{
				this.selectedObject.SendMessage("OnMouseUpCollider", SendMessageOptions.DontRequireReceiver);
				this.isMouseDown = false;
			}
			this.selectedObject.SendMessage("OnMouseExitCollider", SendMessageOptions.DontRequireReceiver);
		}
		this.selectedObject = null;
	}

	
	public LayerMask layerMask = -1;

	
	public bool useSpherecast = true;

	
	public float radius = 0.25f;

	
	private GameObject selectedObject;

	
	private bool isMouseDown;

	
	private Camera cam;
}
