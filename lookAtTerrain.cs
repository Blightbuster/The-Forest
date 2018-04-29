using System;
using TheForest.Utils;
using UnityEngine;


public class lookAtTerrain : MonoBehaviour
{
	
	private void Start()
	{
		this.tr = base.transform;
		this.layerMask = 102768640;
	}

	
	private void OnEnable()
	{
	}

	
	private void OnDisable()
	{
	}

	
	public void setSledCollider(BoxCollider col)
	{
		this.sledCollider = col;
	}

	
	public void resetSledCollider()
	{
		this.sledCollider = null;
	}

	
	private void FixedUpdate()
	{
		this.samplePos = this.endTr.position;
		this.samplePos.y = this.samplePos.y + 6f;
		if (Physics.SphereCast(this.samplePos, 0.5f, Vector3.down, out this.hit, 10f, this.layerMask))
		{
			this.terrainPos = this.hit.point.y + 0.2f;
			this.lookAtPos = new Vector3(this.endTr.position.x, this.terrainPos, this.endTr.position.z);
			Quaternion to = Quaternion.LookRotation(this.lookAtPos - this.tr.position);
			this.tr.rotation = Quaternion.Slerp(this.tr.rotation, to, Time.deltaTime * this.damping);
		}
		if (LocalPlayer.AnimControl.swimming || !LocalPlayer.Inventory.IsRightHandEmpty())
		{
			if (Grabber.Filter)
			{
				activateSledPush component = Grabber.Filter.GetComponent<activateSledPush>();
				if (component)
				{
					component.StartCoroutine(component.disableSled(false));
				}
			}
			LocalPlayer.SpecialActions.SendMessage("exitPushSled");
		}
	}

	
	public Transform endTr;

	
	private Transform tr;

	
	private float terrainPos;

	
	private Vector3 lookAtPos;

	
	private Vector3 samplePos;

	
	public float damping;

	
	public BoxCollider sledCollider;

	
	private int layerMask;

	
	private RaycastHit hit;
}
