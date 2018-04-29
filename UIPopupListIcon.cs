using System;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Popup List")]
public class UIPopupListIcon : UIPopupList
{
	
	
	
	public new string value
	{
		get
		{
			return this.mSelectedItem;
		}
		set
		{
			this.mSelectedItem = value;
			if (this.mSelectedItem == null)
			{
				return;
			}
			if (this.mSelectedItem != null)
			{
				base.TriggerCallbacks();
			}
		}
	}

	
	
	private bool isValid
	{
		get
		{
			return this.bitmapFont != null || this.trueTypeFont != null;
		}
	}

	
	
	private int activeFontSize
	{
		get
		{
			return (!(this.trueTypeFont != null) && !(this.bitmapFont == null)) ? this.bitmapFont.defaultSize : this.fontSize;
		}
	}

	
	
	private float activeFontScale
	{
		get
		{
			return (!(this.trueTypeFont != null) && !(this.bitmapFont == null)) ? ((float)this.fontSize / (float)this.bitmapFont.defaultSize) : 1f;
		}
	}

	
	public new void Clear()
	{
		this.items.Clear();
		this.itemsIcon.Clear();
		this.itemData.Clear();
	}

	
	public void AddItem(string text, Texture icon, object data)
	{
		this.items.Add(text);
		this.itemsIcon.Add(icon);
		this.itemData.Add(data);
	}

	
	public new void RemoveItem(string text)
	{
		int num = this.items.IndexOf(text);
		if (num != -1)
		{
			this.items.RemoveAt(num);
			this.itemsIcon.RemoveAt(num);
			this.itemData.RemoveAt(num);
		}
	}

	
	public void SetSelection()
	{
		int num = this.items.IndexOf(this.mSelectedItem);
		if (num != -1)
		{
			if (this.itemsIcon[num])
			{
				this.icon.mainTexture = this.itemsIcon[num];
				this.icon.enabled = true;
				base.GetComponentInChildren<UILabel>().enabled = false;
			}
			else
			{
				this.icon.enabled = false;
				base.GetComponentInChildren<UILabel>().enabled = true;
			}
		}
	}

	
	public override void Show()
	{
		if (base.enabled && NGUITools.GetActive(base.gameObject) && UIPopupList.mChild == null && this.atlas != null && this.isValid && this.items.Count > 0)
		{
			this.mLabelList.Clear();
			base.StopCoroutine("CloseIfUnselected");
			UICamera.selectedObject = (UICamera.hoveredObject ?? base.gameObject);
			this.mSelection = UICamera.selectedObject;
			this.source = UICamera.selectedObject;
			if (this.source == null)
			{
				Debug.LogError("Popup list needs a source object...");
				return;
			}
			this.mOpenFrame = Time.frameCount;
			if (this.mPanel == null)
			{
				this.mPanel = UIPanel.Find(base.transform);
				if (this.mPanel == null)
				{
					return;
				}
			}
			UIPopupList.mChild = new GameObject("Drop-down List");
			UIPopupList.mChild.layer = base.gameObject.layer;
			UIPopupList.current = this;
			Transform transform = UIPopupList.mChild.transform;
			transform.parent = this.mPanel.cachedTransform;
			Vector3 vector;
			Vector3 vector2;
			Vector3 vector3;
			if (this.openOn == UIPopupList.OpenOn.Manual && this.mSelection != base.gameObject)
			{
				vector = UICamera.lastEventPosition;
				vector2 = this.mPanel.cachedTransform.InverseTransformPoint(this.mPanel.anchorCamera.ScreenToWorldPoint(vector));
				vector3 = vector2;
				transform.localPosition = vector2;
				vector = transform.position;
			}
			else
			{
				Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(this.mPanel.cachedTransform, base.transform, false, false);
				vector2 = bounds.min;
				vector3 = bounds.max;
				transform.localPosition = vector2;
				vector = transform.position;
			}
			base.StartCoroutine("CloseIfUnselected");
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			this.mBackground = NGUITools.AddSprite(UIPopupList.mChild, this.atlas, this.backgroundSprite);
			this.mBackground.pivot = UIWidget.Pivot.TopLeft;
			this.mBackground.depth = NGUITools.CalculateNextDepth(this.mPanel.gameObject);
			this.mBackground.color = this.backgroundColor;
			Vector4 border = this.mBackground.border;
			this.mBgBorder = border.y;
			this.mBackground.cachedTransform.localPosition = new Vector3(0f, border.y, 0f);
			this.mHighlight = NGUITools.AddSprite(UIPopupList.mChild, this.atlas, this.highlightSprite);
			this.mHighlight.pivot = UIWidget.Pivot.TopLeft;
			this.mHighlight.color = this.highlightColor;
			UISpriteData atlasSprite = this.mHighlight.GetAtlasSprite();
			if (atlasSprite == null)
			{
				return;
			}
			float num = (float)atlasSprite.borderTop;
			float num2 = (float)this.activeFontSize;
			float activeFontScale = this.activeFontScale;
			float num3 = num2 * activeFontScale;
			float num4 = 0f;
			float num5 = -this.padding.y;
			List<UILabel> list = new List<UILabel>();
			List<UITexture> list2 = new List<UITexture>();
			if (!this.items.Contains(this.mSelectedItem))
			{
				this.mSelectedItem = null;
			}
			int i = 0;
			int count = this.items.Count;
			while (i < count)
			{
				string text = this.items[i];
				Texture texture = this.itemsIcon[i];
				UILabel uilabel = NGUITools.AddWidget<UILabel>(UIPopupList.mChild);
				uilabel.name = i.ToString();
				uilabel.pivot = UIWidget.Pivot.TopLeft;
				uilabel.bitmapFont = this.bitmapFont;
				uilabel.trueTypeFont = this.trueTypeFont;
				uilabel.fontSize = this.fontSize;
				uilabel.fontStyle = this.fontStyle;
				string text2 = (!this.isLocalized) ? text : Localization.Get(text);
				if (this.toUpper)
				{
					text2 = text2.ToUpper();
				}
				uilabel.text = text2;
				uilabel.color = this.textColor;
				uilabel.cachedTransform.localPosition = new Vector3(border.x + this.padding.x - uilabel.pivotOffset.x + this.iconWidth, num5, -1f);
				uilabel.overflowMethod = UILabel.Overflow.ResizeFreely;
				uilabel.alignment = this.alignment;
				list.Add(uilabel);
				if (texture)
				{
					UITexture uitexture = NGUITools.AddWidget<UITexture>(uilabel.gameObject);
					uitexture.name = i.ToString();
					uitexture.pivot = UIWidget.Pivot.TopLeft;
					uitexture.width = 28;
					uitexture.height = 18;
					uitexture.mainTexture = texture;
					uitexture.cachedTransform.localPosition = new Vector3(-this.iconWidth, 0f, -1f);
					list2.Add(uitexture);
				}
				else
				{
					list2.Add(null);
				}
				num5 -= num3;
				num5 -= this.padding.y;
				num4 = Mathf.Max(num4, uilabel.printedSize.x);
				UIEventListener uieventListener = UIEventListener.Get(uilabel.gameObject);
				uieventListener.onHover = new UIEventListener.BoolDelegate(base.OnItemHover);
				uieventListener.onPress = new UIEventListener.BoolDelegate(base.OnItemPress);
				uieventListener.parameter = text;
				if (this.mSelectedItem == text || (i == 0 && string.IsNullOrEmpty(this.mSelectedItem)))
				{
					base.Highlight(uilabel, true);
				}
				this.mLabelList.Add(uilabel);
				i++;
			}
			num4 = Mathf.Max(num4 + this.iconWidth, vector3.x - vector2.x - (border.x + this.padding.x) * 2f);
			float num6 = num4;
			Vector3 vector4 = new Vector3(num6 * 0.5f, -num3 * 0.5f, 0f);
			Vector3 vector5 = new Vector3(num6, num3 + this.padding.y, 1f);
			int j = 0;
			int count2 = list.Count;
			while (j < count2)
			{
				UILabel uilabel2 = list[j];
				NGUITools.AddWidgetCollider(uilabel2.gameObject);
				uilabel2.autoResizeBoxCollider = false;
				BoxCollider component = uilabel2.GetComponent<BoxCollider>();
				if (component != null)
				{
					vector4.z = component.center.z;
					component.center = vector4;
					component.size = vector5;
				}
				else
				{
					BoxCollider2D component2 = uilabel2.GetComponent<BoxCollider2D>();
					component2.offset = vector4;
					component2.size = vector5;
				}
				j++;
			}
			int width = Mathf.RoundToInt(num4);
			num4 += (border.x + this.padding.x) * 2f;
			num5 -= border.y;
			this.mBackground.width = Mathf.RoundToInt(num4);
			this.mBackground.height = Mathf.RoundToInt(-num5 + border.y);
			int k = 0;
			int count3 = list.Count;
			while (k < count3)
			{
				UILabel uilabel3 = list[k];
				uilabel3.overflowMethod = UILabel.Overflow.ShrinkContent;
				uilabel3.width = width;
				k++;
			}
			float num7 = 2f * this.atlas.pixelSize;
			float f = num4 - (border.x + this.padding.x) * 2f + (float)atlasSprite.borderLeft * num7;
			float f2 = num3 + num * num7;
			this.mHighlight.width = Mathf.RoundToInt(f);
			this.mHighlight.height = Mathf.RoundToInt(f2);
			bool flag = this.position == UIPopupList.Position.Above;
			if (this.position == UIPopupList.Position.Auto)
			{
				UICamera uicamera = UICamera.FindCameraForLayer(this.mSelection.layer);
				if (uicamera != null)
				{
					flag = (uicamera.cachedCamera.WorldToViewportPoint(vector).y < 0.5f);
				}
			}
			if (this.isAnimated)
			{
				base.AnimateColor(this.mBackground);
				if (Time.timeScale == 0f || Time.timeScale >= 0.1f)
				{
					float bottom = num5 + num3;
					base.Animate(this.mHighlight, flag, bottom);
					int l = 0;
					int count4 = list.Count;
					while (l < count4)
					{
						base.Animate(list[l], flag, bottom);
						l++;
					}
					base.AnimateScale(this.mBackground, flag, bottom);
				}
			}
			if (flag)
			{
				vector2.y = vector3.y - border.y;
				vector3.y = vector2.y + (float)this.mBackground.height;
				vector3.x = vector2.x + (float)this.mBackground.width;
				transform.localPosition = new Vector3(vector2.x, vector3.y - border.y, vector2.z);
			}
			else
			{
				vector3.y = vector2.y + border.y;
				vector2.y = vector3.y - (float)this.mBackground.height;
				vector3.x = vector2.x + (float)this.mBackground.width;
			}
			Transform parent = this.mPanel.cachedTransform.parent;
			if (parent != null)
			{
				vector2 = this.mPanel.cachedTransform.TransformPoint(vector2);
				vector3 = this.mPanel.cachedTransform.TransformPoint(vector3);
				vector2 = parent.InverseTransformPoint(vector2);
				vector3 = parent.InverseTransformPoint(vector3);
			}
			Vector3 b = (!this.mPanel.hasClipping) ? this.mPanel.CalculateConstrainOffset(vector2, vector3) : Vector3.zero;
			vector = transform.localPosition + b;
			vector.x = Mathf.Round(vector.x);
			vector.y = Mathf.Round(vector.y);
			transform.localPosition = vector;
		}
		else
		{
			base.OnSelect(false);
		}
	}

	
	protected override Vector3 GetHighlightPosition()
	{
		if (this.mHighlightedLabel == null || this.mHighlight == null)
		{
			return Vector3.zero;
		}
		UISpriteData atlasSprite = this.mHighlight.GetAtlasSprite();
		if (atlasSprite == null)
		{
			return Vector3.zero;
		}
		float pixelSize = this.atlas.pixelSize;
		float num = (float)atlasSprite.borderLeft * pixelSize + this.iconWidth;
		float y = (float)atlasSprite.borderTop * pixelSize;
		return this.mHighlightedLabel.cachedTransform.localPosition + new Vector3(-num, y, 1f);
	}

	
	public UITexture icon;

	
	public List<Texture> itemsIcon = new List<Texture>();

	
	public float iconWidth = 30f;
}
