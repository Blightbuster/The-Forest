using System;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;

public class VRPlayerBlockedEffect : MonoBehaviour
{
	private void OnEnable()
	{
		if (!ForestVR.Enabled)
		{
			base.enabled = false;
		}
	}

	public void UpdateGoalPosition(Vector3 goalPosition)
	{
		this._currentGoalPosition = new Vector2(goalPosition.x, goalPosition.z);
	}

	private void Update()
	{
		if (!ForestVR.Enabled)
		{
			return;
		}
		if (this._activatedTime < 0f)
		{
			return;
		}
		if (Time.time - this._activatedTime > this.HideDelay)
		{
			this.DoHide();
		}
		else
		{
			this.DoShow();
		}
	}

	private void DoShow()
	{
		this.EffectObject.transform.localScale = Vector3.Lerp(this.EffectObject.transform.localScale, this.EffectScale, Time.smoothDeltaTime * this.ShowSpeed);
	}

	private void DoHide()
	{
		this.EffectObject.transform.localScale = Vector3.Lerp(this.EffectObject.transform.localScale, Vector3.zero, Time.smoothDeltaTime * this.HideSpeed);
		if (this.EffectObject.transform.localScale.sqrMagnitude < 0.01f)
		{
			this.EffectObject.transform.localScale = Vector3.zero;
			this._activatedTime = -1f;
		}
	}

	private void FixedUpdate()
	{
		if (!ForestVR.Enabled)
		{
			return;
		}
		if (LocalPlayer.Rigidbody == null)
		{
			return;
		}
		Vector2 vector = Vector2.zero;
		if (this.HeadMoved)
		{
			Vector3 position = LocalPlayer.Rigidbody.position;
			Vector2 b = new Vector2(position.x, position.z);
			vector = this._currentGoalPosition - b;
		}
		if (this.Offsets.Count != this.Max)
		{
			this.Offsets.Clear();
			for (int i = 0; i < this.Max; i++)
			{
				this.Offsets.Add(Vector2.zero);
			}
		}
		this.Offsets.Add(vector);
		if (this.Offsets.Count > this.Max)
		{
			Vector2 a = this.Offsets[0];
			this.Offsets.RemoveAt(0);
			this.CalculatedOffset -= a / (float)this.Max;
		}
		this.CalculatedOffset += vector / (float)this.Max;
		this.CalculatedOffsetMag = this.CalculatedOffset.magnitude;
		this.TrackedMax = Mathf.Max(this.TrackedMax, this.CalculatedOffsetMag);
		this.CalculatedEffectActivator = Mathf.Max(0f, (this.CalculatedOffsetMag - this.MinThreshold) / (this.MaxThreshold - this.MinThreshold));
		if (this.CalculatedEffectActivator >= 1f)
		{
			this._activatedTime = Time.time;
		}
	}

	public void ClearOffsets()
	{
		for (int i = 0; i < this.Offsets.Count; i++)
		{
			this.Offsets[i] = Vector2.zero;
		}
		this.CalculatedOffset = Vector2.zero;
		this.CalculatedOffsetMag = 0f;
	}

	public GameObject EffectObject;

	public Vector3 EffectScale = Vector3.one;

	public float ShowSpeed = 1f;

	public float HideSpeed = 1f;

	public float HideDelay = 0.5f;

	public float MinThreshold = 0.01f;

	public float MaxThreshold = 0.5f;

	public Vector2 CalculatedOffset;

	public float CalculatedOffsetMag;

	public float TrackedMax;

	public float CalculatedEffectActivator;

	private Vector2 _currentGoalPosition;

	public int Max = 20;

	public List<Vector2> Offsets = new List<Vector2>();

	public bool HeadMoved;

	private float _activatedTime = -1f;
}
