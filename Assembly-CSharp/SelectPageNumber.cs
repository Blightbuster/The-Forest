using System;
using TheForest.Utils;
using UnityEngine;

public class SelectPageNumber : MonoBehaviour
{
	public int BranchOvered { get; set; }

	public bool SelfOvered { get; set; }

	public bool IsSelected
	{
		get
		{
			return this.SelfOvered || this.HasMouseOver || (this.Tab && this.MyPageNew != null && this.MyPageNew.activeSelf);
		}
	}

	public bool IsSelectedOrHighlighted
	{
		get
		{
			return this.IsSelected || this.Highlighted;
		}
	}

	public bool IsOvered
	{
		get
		{
			return this.BranchOvered > 0 || this.IsSelected;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (this.updateRenderer)
		{
			this.updateRenderer = false;
			this.TargetRenderer = base.GetComponentInChildren<Renderer>();
		}
	}

	private bool IsOverCollider()
	{
		return this.HasMouseOver;
	}

	private void Awake()
	{
		if (!this.TargetRenderer)
		{
			this.TargetRenderer = base.GetComponent<Renderer>();
			if (!this.SelfHighlighting)
			{
				this.TargetRenderer.enabled = false;
			}
		}
		if (!this.MyMat)
		{
			this.MyMat = this.TargetRenderer.sharedMaterial;
		}
		this.SwitchColor = this.MyMat.color;
		this.SwitchColor.a = 0f;
		this.RefreshVisuals();
	}

	private void SwitchMatColor(Color color)
	{
		if (this.MyMatPropertyBlock == null)
		{
			this.MyMatPropertyBlock = new MaterialPropertyBlock();
		}
		Renderer targetRenderer = this.TargetRenderer;
		targetRenderer.GetPropertyBlock(this.MyMatPropertyBlock);
		this.MyMatPropertyBlock.SetColor("_Color", color);
		if (color.a > 0f)
		{
			color.a = 0.35f;
		}
		this.MyMatPropertyBlock.SetColor("_OutlineColor", color);
		targetRenderer.SetPropertyBlock(this.MyMatPropertyBlock);
	}

	private void OnMouseOverCollider()
	{
		this.HasMouseOver = true;
	}

	private void OnMouseExitCollider()
	{
		this.HasMouseOver = false;
	}

	private void Update()
	{
		this.SelfOvered = this.IsOverCollider();
		this.PageIsActive = (this.MyPageNew && this.MyPageNew.activeSelf);
		if (TheForest.Utils.Input.GetButtonDown("Select") && this.SelfOvered)
		{
			this.SelfOvered = false;
			this.PageIsActive = false;
			this.OnClick();
		}
		if (this.Highlighted && ((this.HighlightedPage && this.HighlightedPage.activeSelf) || (!this.HighlightedPage && this.PageIsActive)))
		{
			this.Unhighlight();
		}
		this.RefreshVisuals();
	}

	private void RefreshVisuals()
	{
		if (this.IsSelected != this.ShouldHighlightPrev)
		{
			this.ShouldHighlightPrev = this.IsSelected;
			if (!this.Tab)
			{
				this.SwitchColor.a = ((!this.PageIsActive) ? ((!this.HasMouseOver) ? 0f : 0.2f) : 0.6f);
				this.SwitchMatColor(this.SwitchColor);
				this.TargetRenderer.enabled = this.HasMouseOver;
				base.SendMessage("SetHovered", this.HasMouseOver, SendMessageOptions.DontRequireReceiver);
			}
			else if (this.HighlightMaterial)
			{
				this.TargetRenderer.sharedMaterial = ((!this.IsSelected) ? this.MyMat : this.HighlightMaterial);
			}
		}
	}

	private void OnDisable()
	{
		this.BranchOvered = 0;
		this.OnMouseExitCollider();
	}

	public void OnClick()
	{
		LocalPlayer.Sfx.PlayTurnPage();
		if (!this.Index && !this.Tab)
		{
			base.SendMessage("SetHovered", false, SendMessageOptions.DontRequireReceiver);
			((!this.ThisPageOverride) ? base.transform.parent : this.ThisPageOverride).gameObject.SetActive(false);
			this.MyPageNew.SetActive(true);
			LocalPlayer.AnimatedBook.sharedMaterial = this.MyPageNew.GetComponent<Renderer>().sharedMaterial;
		}
		else if (this.Index)
		{
			base.SendMessage("SetHovered", false, SendMessageOptions.DontRequireReceiver);
			this.TurnOffAllPages();
			this.MyPageNew.SetActive(true);
			LocalPlayer.AnimatedBook.sharedMaterial = this.MyPageNew.GetComponent<Renderer>().sharedMaterial;
		}
		else if (this.Tab)
		{
			this.TurnOffAllPages();
			this.IndexPage.SetActive(false);
			if (this.Highlighted && this.HighlightedPage)
			{
				this.HighlightedPage.SetActive(true);
			}
			else
			{
				this.MyPageNew.SetActive(true);
			}
			LocalPlayer.AnimatedBook.sharedMaterial = this.MyPageNew.GetComponent<Renderer>().sharedMaterial;
		}
		this.Unhighlight();
	}

	public bool Highlight(GameObject highlightedPage = null)
	{
		if (!this.Highlighted)
		{
			this.Highlighted = true;
			if (this.HighlightMaterial)
			{
				if (!this.MyMat)
				{
					this.MyMat = this.TargetRenderer.sharedMaterial;
				}
				this.SwitchMatColor(this.SwitchColor);
			}
			base.transform.localPosition += this.HighlightOffset;
			this.HighlightedPage = highlightedPage;
			base.GetComponent<Renderer>().enabled = true;
		}
		return this.HighlightedPage == highlightedPage;
	}

	private void Unhighlight()
	{
		if (this.Highlighted)
		{
			this.Highlighted = false;
			if (!this.SelfHighlighting)
			{
				base.GetComponent<Renderer>().enabled = false;
			}
			base.transform.localPosition -= this.HighlightOffset;
			this.SwitchColor.a = 1f;
			this.SwitchMatColor(this.SwitchColor);
		}
	}

	public void TurnOffAllPages()
	{
		for (int i = this.Pages.transform.childCount - 1; i >= 0; i--)
		{
			this.Pages.transform.GetChild(i).gameObject.SetActive(false);
		}
	}

	public Transform ThisPageOverride;

	public Renderer TargetRenderer;

	public GameObject MyPageNew;

	public bool Index;

	public bool Tab;

	public bool SelfHighlighting;

	public GameObject Pages;

	public GameObject IndexPage;

	public Vector3 HighlightOffset;

	public Material HighlightMaterial;

	private GameObject HighlightedPage;

	private bool Highlighted;

	private bool PageIsActive;

	private bool HasMouseOver;

	private bool ShouldHighlightPrev;

	private Color SwitchColor;

	private Material MyMat;

	private MaterialPropertyBlock MyMatPropertyBlock;

	public bool updateRenderer;
}
