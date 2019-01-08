using System;
using UnityEngine;

[Serializable]
public class RadialButton
{
	public void SetSelected(bool selectedValue, float speedMultiplier, Vector3 extraPosOffset)
	{
		float t = Time.deltaTime * speedMultiplier;
		if (selectedValue)
		{
			this.Parent.localPosition = Vector3.Slerp(this.Parent.localPosition, this._basePos * this.Offset + extraPosOffset, t);
			this.Parent.localScale = Vector3.Slerp(this.Parent.localScale, Vector3.one * this.SelectedScale, t);
		}
		else
		{
			this.Parent.localPosition = Vector3.Slerp(this.Parent.localPosition, this._basePos, t);
			this.Parent.localScale = Vector3.Slerp(this.Parent.localScale, Vector3.one, t);
		}
	}

	public void Awake()
	{
		this._basePos = this.Parent.localPosition;
	}

	public Transform Parent;

	public GameObject Icon;

	public GameObject Border;

	public float SelectedScale = 1.2f;

	public float Offset = 1f;

	private Vector3 _basePos = Vector3.zero;
}
