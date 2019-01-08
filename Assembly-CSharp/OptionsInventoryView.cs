using System;
using TheForest.Utils;
using UnityEngine;

public class OptionsInventoryView : MonoBehaviour
{
	public void OnMouseEnterCollider()
	{
		base.enabled = true;
		this.hovered = true;
		this._text.color = this._selectedColor;
	}

	public void OnMouseExitCollider()
	{
		this.hovered = false;
		this._text.color = this._baseColor;
	}

	private void OnEnable()
	{
		this._text.color = this._baseColor;
	}

	private void OnDisable()
	{
		this.hovered = false;
	}

	private void Update()
	{
		if (this.hovered && TheForest.Utils.Input.GetButtonUp("Combine"))
		{
			LocalPlayer.Inventory.ToggleInventory();
			LocalPlayer.Inventory.TogglePauseMenu();
			base.enabled = false;
			this.hovered = false;
			this._text.color = this._baseColor;
		}
	}

	private bool hovered;

	public TextMesh _text;

	public Color _baseColor;

	public Color _selectedColor;
}
