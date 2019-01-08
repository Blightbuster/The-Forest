using System;
using TheForest.Utils;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class RadialMenu : MonoBehaviour
{
	private void Awake()
	{
		foreach (RadialButton radialButton in this.OutsideButtons)
		{
			radialButton.Awake();
		}
	}

	private void Update()
	{
		if (this.SourceHand == null)
		{
			this.SourceHand = base.GetComponentInParent<Hand>();
		}
		if (this.SourceHand == null || this.SourceHand.controller == null)
		{
			return;
		}
		if (TheForest.Utils.Input.GetButtonDown("Radial"))
		{
			this.SetVisible(!this.IsVisible);
		}
		if (!this.IsVisible)
		{
			return;
		}
		Vector2 axis = this.SourceHand.controller.GetAxis(EVRButtonId.k_EButton_Axis0);
		if (!this.Manual)
		{
			this.SourceValues = axis;
		}
		this.UpdateSelectedIndex();
		this.UpdateCursor();
		bool flag = this.SelectedIndex == -2;
		if (flag)
		{
			this.CenterButton.SetSelected(true, this.SpeedMultiplier, this.ExtraPositionOffset);
		}
		else
		{
			this.CenterButton.SetSelected(false, this.SpeedMultiplier * 1.5f, this.ExtraPositionOffset);
		}
		for (int i = 0; i < this.OutsideButtons.Length; i++)
		{
			if (i == this.SelectedIndex)
			{
				this.OutsideButtons[i].SetSelected(true, this.SpeedMultiplier, this.ExtraPositionOffset);
			}
			else
			{
				this.OutsideButtons[i].SetSelected(false, this.SpeedMultiplier * 1.5f, this.ExtraPositionOffset);
			}
		}
	}

	private void SetVisible(bool newVisible)
	{
		this.IsVisible = newVisible;
		for (int i = 0; i < this.OutsideButtons.Length; i++)
		{
			this.OutsideButtons[i].Parent.gameObject.SetActive(newVisible);
			this.OutsideButtons[i].Parent.localPosition = Vector3.zero;
			this.OutsideButtons[i].Parent.localScale = Vector3.zero;
		}
		this.CenterButton.Parent.gameObject.SetActive(newVisible);
		this.CenterButton.Parent.localPosition = Vector3.zero;
		this.CenterButton.Parent.localScale = Vector3.zero;
		this.Cursor.gameObject.SetActive(newVisible);
		this.Cursor.localPosition = Vector3.zero;
	}

	private void UpdateCursor()
	{
		this.Cursor.gameObject.SetActive(this.SelectedIndex != -1);
		this.Cursor.transform.localPosition = new Vector3(this.SourceValues.x * this.CursorScale.x, 0f, this.SourceValues.y * this.CursorScale.y);
	}

	private void UpdateSelectedIndex()
	{
		if (!this.SourceHand.controller.GetTouch(EVRButtonId.k_EButton_Axis0))
		{
			this.SelectedIndex = -1;
			return;
		}
		if (this.SourceValues.sqrMagnitude <= this.CenterThreshold)
		{
			this.SelectedIndex = -2;
			return;
		}
		float y = this.SourceValues.y;
		float x = this.SourceValues.x;
		float num = Mathf.Atan2(x, y);
		this.RadialPosition = num * 57.29578f / 360f;
		if (this.RadialPosition < 0f)
		{
			this.RadialPosition = 1f + this.RadialPosition;
		}
		float num2 = Mathf.Clamp01(this.RadialPosition);
		this.SelectedIndex = Mathf.RoundToInt(num2 * (float)this.OutsideButtons.Length) % this.OutsideButtons.Length;
	}

	public int SelectedIndex = -1;

	public Vector3 ExtraPositionOffset;

	public RadialButton[] OutsideButtons;

	public RadialButton CenterButton;

	public Hand SourceHand;

	public float RadialPosition;

	public Vector2 SourceValues;

	public Transform Cursor;

	public Vector2 CursorScale = Vector2.one;

	public bool Manual;

	public float SpeedMultiplier = 10f;

	public float CenterThreshold = 0.1f;

	public float IgnoreThreshold = 1E-06f;

	public bool IsVisible;
}
