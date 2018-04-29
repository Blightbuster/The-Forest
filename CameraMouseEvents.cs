using System;
using TheForest.Utils;
using UnityEngine;


public class CameraMouseEvents : MonoBehaviour
{
	
	private void Awake()
	{
		this.hitInfo = default(RaycastHit);
		this.cam = base.GetComponent<Camera>();
	}

	
	private void Update()
	{
		Ray ray;
		if (this.RayOverride)
		{
			ray = new Ray(this.RayOverride.position, this.RayOverride.forward);
		}
		else
		{
			ray = ((!this.cam) ? LocalPlayer.MainCam.ScreenPointToRay(TheForest.Utils.Input.mousePosition) : this.cam.ScreenPointToRay(TheForest.Utils.Input.mousePosition));
		}
		this.SpawnDebugRay(ray.origin, ray.direction, this.RayColor, this.RaySize, 100f);
		GameObject gameObject = (!Physics.Raycast(ray, out this.hitInfo, 100f, this.layerMask) && (!this.useSpherecast || !TheForest.Utils.Input.IsGamePad || !Physics.SphereCast(ray, this.radius, out this.hitInfo, 100f, this.layerMask))) ? null : this.hitInfo.collider.gameObject;
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

	
	private void SpawnDebugRay(Vector3 outterPos, Vector3 dir, Color col, float size, float length)
	{
		if (!this.ShowRay)
		{
			if (this.mouseEventDebugSphere)
			{
				UnityEngine.Object.Destroy(this.mouseEventDebugSphere.gameObject);
				this.mouseEventDebugSphere = null;
			}
			if (this.mouseEventDebugCylinder)
			{
				UnityEngine.Object.Destroy(this.mouseEventDebugCylinder.gameObject);
				this.mouseEventDebugCylinder = null;
			}
			return;
		}
		if (!this.mouseEventDebugSphere)
		{
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			gameObject.transform.localScale *= size * 2f;
			gameObject.name = "DebugRay - Origin";
			gameObject.GetComponent<Renderer>().material.color = col;
			gameObject.layer = 23;
			UnityEngine.Object.Destroy(gameObject.GetComponent<Collider>());
			this.mouseEventDebugSphere = gameObject.transform;
		}
		if (!this.mouseEventDebugCylinder)
		{
			GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			gameObject2.name = "DebugRay";
			gameObject2.GetComponent<Renderer>().material.color = col;
			gameObject2.layer = 23;
			UnityEngine.Object.Destroy(gameObject2.GetComponent<Collider>());
			this.mouseEventDebugCylinder = gameObject2.transform;
		}
		this.mouseEventDebugSphere.position = outterPos;
		this.mouseEventDebugCylinder.localScale = new Vector3(size, length / 2f, size);
		this.mouseEventDebugCylinder.position = outterPos + dir.normalized * (length / 4f);
		this.mouseEventDebugCylinder.LookAt(outterPos + dir);
		this.mouseEventDebugCylinder.LookAt(this.mouseEventDebugCylinder.position + this.mouseEventDebugCylinder.up);
	}

	
	public LayerMask layerMask = -1;

	
	public bool useSpherecast = true;

	
	public float radius = 0.25f;

	
	public Transform RayOverride;

	
	public bool ShowRay;

	
	public Color RayColor = Color.red;

	
	public float RaySize = 0.0125f;

	
	private GameObject selectedObject;

	
	private bool isMouseDown;

	
	private Camera cam;

	
	private RaycastHit hitInfo;

	
	private Transform mouseEventDebugSphere;

	
	private Transform mouseEventDebugCylinder;
}
