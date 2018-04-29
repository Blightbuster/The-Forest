using System;
using ScionEngine;
using UnityEngine;


[ExecuteInEditMode]
public class SceneCameraMovement : MonoBehaviour
{
	
	private void Start()
	{
		this.origMoveSpeed = this.moveSpeed;
		this.mPitch = base.transform.localEulerAngles.x;
		this.mHdg = base.transform.localEulerAngles.y;
	}

	
	private void Update()
	{
		if (Input.GetKeyDown("c"))
		{
			if (this.postProcessComponent.enabled)
			{
				this.postProcessComponent.enabled = false;
			}
			else
			{
				this.postProcessComponent.enabled = true;
			}
		}
		if (this.limitMovement)
		{
			Vector3 vector = base.transform.position;
			vector = Vector3.Min(vector, new Vector3(40f, 40f, 40f));
			vector = Vector3.Max(vector, new Vector3(-30f, 0f, -30f));
			base.transform.position = vector;
		}
		if (Application.isPlaying)
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				this.moveSpeed = 5f * this.origMoveSpeed;
			}
			else
			{
				this.moveSpeed = this.origMoveSpeed;
			}
			if (Input.GetMouseButton(1))
			{
				float aVal = Input.GetAxisRaw("Mouse X") * this.sensitivityX;
				float num = Input.GetAxisRaw("Mouse Y") * this.sensitivityY;
				this.ChangeHeading(aVal);
				this.ChangePitch(-num);
			}
			if (Input.GetKey(KeyCode.D))
			{
				this.Strafe(this.moveSpeed * Time.smoothDeltaTime);
			}
			if (Input.GetKey(KeyCode.A))
			{
				this.Strafe(-this.moveSpeed * Time.smoothDeltaTime);
			}
			if (Input.GetKey(KeyCode.W))
			{
				this.MoveForwards(this.moveSpeed * Time.smoothDeltaTime);
			}
			if (Input.GetKey(KeyCode.S))
			{
				this.MoveForwards(-this.moveSpeed * Time.smoothDeltaTime);
			}
		}
	}

	
	private void MoveForwards(float aVal)
	{
		base.transform.position += aVal * base.transform.forward;
	}

	
	private void Strafe(float aVal)
	{
		base.transform.position += aVal * base.transform.right;
	}

	
	private void ChangeHeight(float aVal)
	{
		base.transform.position += aVal * Vector3.up;
	}

	
	private void ChangeHeading(float aVal)
	{
		this.mHdg += aVal;
		SceneCameraMovement.WrapAngle(ref this.mHdg);
		base.transform.localEulerAngles = new Vector3(this.mPitch, this.mHdg, 0f);
	}

	
	private void ChangePitch(float aVal)
	{
		this.mPitch += aVal;
		SceneCameraMovement.WrapAngle(ref this.mPitch);
		base.transform.localEulerAngles = new Vector3(this.mPitch, this.mHdg, 0f);
	}

	
	public static void WrapAngle(ref float angle)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
	}

	
	public float sensitivityX = 4f;

	
	public float sensitivityY = 4f;

	
	private float origMoveSpeed;

	
	public float moveSpeed = 1f;

	
	private float mHdg;

	
	private float mPitch;

	
	public ScionPostProcess postProcessComponent;

	
	private bool limitMovement;
}
