using System;
using System.Collections.Generic;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	public class UiTranslationOverlaySystem : MonoBehaviour
	{
		private void Awake()
		{
			UiTranslationOverlaySystem.Instance = this;
		}

		private void Update()
		{
			if (LocalPlayer.Inventory)
			{
				bool flag = LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World || (LocalPlayer.Inventory.CurrentView >= PlayerInventory.PlayerViews.Sleep && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.PlayerList);
				if (flag != this._iconHolderTr.gameObject.activeSelf)
				{
					this._iconHolderTr.gameObject.SetActive(flag);
				}
				if (!flag)
				{
					bool flag2 = LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Book;
					if (flag2 != this._iconHolderBookTr.gameObject.activeSelf)
					{
						this._iconHolderBookTr.gameObject.SetActive(flag2);
					}
				}
			}
		}

		private void OnDestroy()
		{
			if (UiTranslationOverlaySystem.Instance == this)
			{
				UiTranslationOverlaySystem.Instance = null;
			}
		}

		public static void RegisterLabel(Transform target, string translationKey, Color textColor, Color backgroundColor, NGUIText.Alignment alignment, UiTranslationOverlaySystem.CurrentViewOptions currentViewOption = UiTranslationOverlaySystem.CurrentViewOptions.AllowInWorld)
		{
			if (UiTranslationOverlaySystem.Instance && !UiTranslationOverlaySystem.Instance._activeLabels.ContainsKey(target) && UiTranslationDatabase.HasKey(translationKey))
			{
				string text = UiTranslationDatabase.TranslateKey(translationKey, string.Empty, false);
				if (!string.IsNullOrEmpty(text))
				{
					UiTranslationOverlayLabel uiTranslationOverlayLabel;
					if (UiTranslationOverlaySystem.Instance._labelPool.Count > 0)
					{
						uiTranslationOverlayLabel = UiTranslationOverlaySystem.Instance._labelPool.Dequeue();
						uiTranslationOverlayLabel.gameObject.SetActive(true);
						UiTranslationOverlaySystem.Instance.SetIconHolderTr(uiTranslationOverlayLabel.transform, currentViewOption);
					}
					else
					{
						uiTranslationOverlayLabel = UnityEngine.Object.Instantiate<UiTranslationOverlayLabel>(UiTranslationOverlaySystem.Instance._overlayLabelPrefab);
						UiTranslationOverlaySystem.Instance.SetIconHolderTr(uiTranslationOverlayLabel.transform, currentViewOption);
						uiTranslationOverlayLabel.transform.localScale = UiTranslationOverlaySystem.Instance._overlayLabelPrefab.transform.localScale;
					}
					uiTranslationOverlayLabel._label.text = text;
					bool flag = alignment != NGUIText.Alignment.Left;
					if (flag)
					{
						if (uiTranslationOverlayLabel._label.pivot != UIWidget.Pivot.Center)
						{
							uiTranslationOverlayLabel._label.pivot = UIWidget.Pivot.Center;
							uiTranslationOverlayLabel._label.transform.localPosition = Vector3.zero;
							uiTranslationOverlayLabel._background.pivot = UIWidget.Pivot.Center;
							uiTranslationOverlayLabel._background.transform.localPosition = new Vector3(0f, uiTranslationOverlayLabel._background.transform.localPosition.y, 0f);
						}
					}
					else if (uiTranslationOverlayLabel._label.pivot != UIWidget.Pivot.Left)
					{
						uiTranslationOverlayLabel._label.pivot = UIWidget.Pivot.Left;
						uiTranslationOverlayLabel._label.transform.localPosition = Vector3.zero;
						uiTranslationOverlayLabel._background.pivot = UIWidget.Pivot.Left;
						uiTranslationOverlayLabel._background.transform.localPosition = new Vector3(-10f, uiTranslationOverlayLabel._background.transform.localPosition.y, 0f);
					}
					uiTranslationOverlayLabel._label.color = textColor;
					uiTranslationOverlayLabel._background.color = backgroundColor;
					uiTranslationOverlayLabel._follow._target = target;
					uiTranslationOverlayLabel._follow._inBook = (currentViewOption == UiTranslationOverlaySystem.CurrentViewOptions.AllowInBook);
					uiTranslationOverlayLabel._follow._inInventory = (currentViewOption == UiTranslationOverlaySystem.CurrentViewOptions.AllowInInventory);
					UiTranslationOverlaySystem.Instance._activeLabels.Add(target, uiTranslationOverlayLabel);
				}
			}
		}

		public static void UnregisterIcon(Transform target)
		{
			UiTranslationOverlayLabel uiTranslationOverlayLabel;
			if (UiTranslationOverlaySystem.Instance && UiTranslationOverlaySystem.Instance._activeLabels.TryGetValue(target, out uiTranslationOverlayLabel))
			{
				uiTranslationOverlayLabel._follow._target2 = null;
				UiTranslationOverlaySystem.Instance._activeLabels.Remove(target);
				UiTranslationOverlaySystem.Instance.DisableOverlayLabel(uiTranslationOverlayLabel);
			}
		}

		private void DisableOverlayLabel(UiTranslationOverlayLabel overlayLabel)
		{
			UiTranslationOverlaySystem.Instance._labelPool.Enqueue(overlayLabel);
			overlayLabel.gameObject.SetActive(false);
		}

		public static UiTranslationOverlayLabel GetOverlayLabel(Transform target)
		{
			UiTranslationOverlayLabel result;
			if (UiTranslationOverlaySystem.Instance && UiTranslationOverlaySystem.Instance._activeLabels.TryGetValue(target, out result))
			{
				return result;
			}
			return null;
		}

		private void SetIconHolderTr(Transform t, UiTranslationOverlaySystem.CurrentViewOptions currentViewOption)
		{
			switch (currentViewOption)
			{
			case UiTranslationOverlaySystem.CurrentViewOptions.AllowInWorld:
				t.parent = this._iconHolderTr;
				break;
			case UiTranslationOverlaySystem.CurrentViewOptions.AllowInBook:
				t.parent = this._iconHolderBookTr;
				break;
			case UiTranslationOverlaySystem.CurrentViewOptions.AllowInPlane:
				t.parent = this._iconHolderPlaneTr;
				break;
			case UiTranslationOverlaySystem.CurrentViewOptions.AllowInInventory:
				t.parent = this._iconHolderInventoryTr;
				break;
			}
		}

		public UiTranslationOverlayLabel _overlayLabelPrefab;

		public Transform _iconHolderTr;

		public Transform _iconHolderBookTr;

		public Transform _iconHolderPlaneTr;

		public Transform _iconHolderInventoryTr;

		private static UiTranslationOverlaySystem Instance;

		private Queue<UiTranslationOverlayLabel> _labelPool = new Queue<UiTranslationOverlayLabel>();

		private Dictionary<Transform, UiTranslationOverlayLabel> _activeLabels = new Dictionary<Transform, UiTranslationOverlayLabel>();

		public enum CurrentViewOptions
		{
			AllowInWorld,
			AllowInBook,
			AllowInPlane,
			AllowInInventory
		}
	}
}
