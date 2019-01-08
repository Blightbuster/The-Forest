using System;
using UnityEngine;

public class GunAim : MonoBehaviour
{
	private void Start()
	{
		this.parentCamera = base.GetComponentInParent<Camera>();
	}

	private void Update()
	{
		float x = Input.mousePosition.x;
		float y = Input.mousePosition.y;
		if (x <= (float)this.borderLeft || x >= (float)(Screen.width - this.borderRight) || y <= (float)this.borderBottom || y >= (float)(Screen.height - this.borderTop))
		{
			this.isOutOfBounds = true;
		}
		else
		{
			this.isOutOfBounds = false;
		}
		if (!this.isOutOfBounds)
		{
			base.transform.LookAt(this.parentCamera.ScreenToWorldPoint(new Vector3(x, y, 5f)));
		}
	}

	public bool GetIsOutOfBounds()
	{
		return this.isOutOfBounds;
	}

	public int borderLeft;

	public int borderRight;

	public int borderTop;

	public int borderBottom;

	private Camera parentCamera;

	private bool isOutOfBounds;
}
