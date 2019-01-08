using System;
using UnityEngine;

[RequireComponent(typeof(OVRRaycaster))]
public class OVRMousePointer : MonoBehaviour
{
	public GameObject pointerObject
	{
		get
		{
			return base.GetComponent<OVRRaycaster>().pointer;
		}
	}

	private void Awake()
	{
		this.raycaster = base.GetComponent<OVRRaycaster>();
	}

	private void Update()
	{
		if (this.mouseShowPolicy == OVRMousePointer.MouseShowPolicy.withActivity)
		{
			this.pointerObject.SetActive(this.HasMovedRecently() && this.raycaster.IsFocussed());
		}
		else if (this.mouseShowPolicy == OVRMousePointer.MouseShowPolicy.withGaze)
		{
			this.pointerObject.SetActive(this.raycaster.IsFocussed());
		}
		if (this.hideGazePointerWhenActive && this.HasMovedRecently() && this.raycaster.IsFocussed())
		{
			OVRGazePointer.instance.RequestHide();
		}
		if (this.defaultMouseMovement && this.raycaster.IsFocussed())
		{
			Vector2 vector = this.pointerObject.GetComponent<RectTransform>().localPosition;
			vector += new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * this.mouseMoveSpeed;
			float width = base.GetComponent<RectTransform>().rect.width;
			float height = base.GetComponent<RectTransform>().rect.height;
			vector.x = Mathf.Clamp(vector.x, -width / 2f, width / 2f);
			vector.y = Mathf.Clamp(vector.y, -height / 2f, height / 2f);
			this.SetLocalPosition(vector);
		}
	}

	public bool HasMovedRecently()
	{
		return this.lastMouseActivityTime + this.inactivityTimeout > Time.time;
	}

	public void SetLocalPosition(Vector3 pos)
	{
		if ((this.pointerObject.GetComponent<RectTransform>().localPosition - pos).magnitude > 0.001f)
		{
			this.lastMouseActivityTime = Time.time;
		}
		this.pointerObject.GetComponent<RectTransform>().localPosition = pos;
	}

	[Tooltip("Period of inactivity before mouse disappears")]
	public float inactivityTimeout = 1f;

	[Tooltip("Policy regarding when mouse pointer should be shown")]
	public OVRMousePointer.MouseShowPolicy mouseShowPolicy;

	[Tooltip("Should the mouse pointer being active cause the gaze pointer to fade")]
	public bool hideGazePointerWhenActive;

	[Tooltip("Move the pointer in response to mouse movement")]
	public bool defaultMouseMovement = true;

	public float mouseMoveSpeed = 5f;

	private float lastMouseActivityTime;

	private OVRRaycaster raycaster;

	public enum MouseShowPolicy
	{
		always,
		withGaze,
		withActivity
	}
}
