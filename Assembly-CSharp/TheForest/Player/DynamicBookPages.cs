using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Items;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player
{
	[DoNotSerializePublic]
	public class DynamicBookPages : MonoBehaviour
	{
		private void OnValidate()
		{
			foreach (DynamicBookPages.ItemLinkedPage itemLinkedPage in this.Pages)
			{
				if (itemLinkedPage != null)
				{
					if (itemLinkedPage.PrevLabel != null)
					{
						itemLinkedPage.PrevLabelTranslation = itemLinkedPage.PrevLabel.gameObject.GetComponent<UiTranslationTextMesh>();
					}
					if (itemLinkedPage.NextLabel != null)
					{
						itemLinkedPage.NextLabelTranslation = itemLinkedPage.NextLabel.gameObject.GetComponent<UiTranslationTextMesh>();
					}
				}
			}
		}

		private void Awake()
		{
			foreach (DynamicBookPages.ItemLinkedPage itemLinkedPage in this.Pages)
			{
				if (itemLinkedPage != null)
				{
					itemLinkedPage.CollectDefaults();
				}
			}
		}

		private void OnEnable()
		{
			if (LocalPlayer.Inventory == null)
			{
				return;
			}
			base.StartCoroutine(this.UpdateLinksWorker());
		}

		private IEnumerator UpdateLinksWorker()
		{
			for (int i = 0; i < this.Tabs.Count; i++)
			{
				this.Tabs[i].gameObject.SetActive(false);
			}
			yield return null;
			yield return null;
			int activeCount = 0;
			foreach (DynamicBookPages.ItemLinkedPage itemLinkedPage in this.Pages)
			{
				if (itemLinkedPage != null && !itemLinkedPage.Anchor)
				{
					DynamicBookPages.UpdatePage(itemLinkedPage, this.Pages);
					if (itemLinkedPage.Activated)
					{
						this.Tabs[activeCount].MyPageNew = itemLinkedPage.Page;
						this.Tabs[activeCount].gameObject.SetActive(true);
						activeCount++;
					}
				}
			}
			yield break;
		}

		private void OnDisable()
		{
			foreach (DynamicBookPages.ItemLinkedPage itemLinkedPage in this.Pages)
			{
				if (itemLinkedPage != null)
				{
					DynamicBookPages.RestoreDefaults(itemLinkedPage);
				}
			}
		}

		private static void RestoreDefaults(DynamicBookPages.ItemLinkedPage linkedPage)
		{
			if (linkedPage == null)
			{
				return;
			}
			linkedPage.RestoreDefaults();
		}

		private static void UpdatePage(DynamicBookPages.ItemLinkedPage linkedPage, List<DynamicBookPages.ItemLinkedPage> allPages)
		{
			if (LocalPlayer.Inventory == null || linkedPage.RequiredItemId == 0)
			{
				return;
			}
			bool flag = LocalPlayer.Inventory.Owns(linkedPage.RequiredItemId, true);
			if (flag == linkedPage.Activated)
			{
				return;
			}
			linkedPage.Activated = flag;
			if (!linkedPage.Activated)
			{
				DynamicBookPages.Deactivate(linkedPage, allPages);
			}
		}

		private static void Deactivate(DynamicBookPages.ItemLinkedPage linkedPage, List<DynamicBookPages.ItemLinkedPage> allPages)
		{
			linkedPage.Activated = false;
			GameObject prevTarget = linkedPage.GetPrevTarget();
			string prevTargetLabelKey = linkedPage.GetPrevTargetLabelKey();
			string prevTargetLabelText = linkedPage.GetPrevTargetLabelText();
			GameObject nextTarget = linkedPage.GetNextTarget();
			string nextTargetLabelKey = linkedPage.GetNextTargetLabelKey();
			string nextTargetLabelText = linkedPage.GetNextTargetLabelText();
			foreach (DynamicBookPages.ItemLinkedPage itemLinkedPage in allPages)
			{
				if (itemLinkedPage != null)
				{
					if (itemLinkedPage.NextButton != null && itemLinkedPage.NextButton.MyPageNew == linkedPage.Page)
					{
						itemLinkedPage.NextButton.MyPageNew = nextTarget;
						if (itemLinkedPage.NextLabelTranslation != null)
						{
							itemLinkedPage.NextLabelTranslation._key = nextTargetLabelKey;
							DynamicBookPages.RefreshLabelTranslation(itemLinkedPage.NextLabelTranslation, nextTargetLabelText);
						}
					}
					if (itemLinkedPage.PrevButton != null && itemLinkedPage.PrevButton.MyPageNew == linkedPage.Page)
					{
						itemLinkedPage.PrevButton.MyPageNew = prevTarget;
						if (itemLinkedPage.PrevLabelTranslation != null)
						{
							itemLinkedPage.PrevLabelTranslation._key = prevTargetLabelKey;
							DynamicBookPages.RefreshLabelTranslation(itemLinkedPage.PrevLabelTranslation, prevTargetLabelText);
						}
					}
				}
			}
		}

		private static void RefreshLabelTranslation(UiTranslationTextMesh label, string defaultText)
		{
			label.SetText(UiTranslationDatabase.TranslateKey(label._key, defaultText, true));
		}

		private DynamicBookPages.ItemLinkedPage GetLink(GameObject prevTarget)
		{
			if (prevTarget == null)
			{
				return null;
			}
			foreach (DynamicBookPages.ItemLinkedPage itemLinkedPage in this.Pages)
			{
				if (itemLinkedPage.Page == prevTarget)
				{
					return itemLinkedPage;
				}
			}
			return null;
		}

		public List<DynamicBookPages.ItemLinkedPage> Pages;

		public List<SelectPageNumber> Tabs;

		[Serializable]
		public class ItemLinkedPage
		{
			internal GameObject GetPrevTarget()
			{
				if (this.PrevButton == null)
				{
					return null;
				}
				return this.PrevButton.MyPageNew;
			}

			internal GameObject GetNextTarget()
			{
				if (this.NextButton == null)
				{
					return null;
				}
				return this.NextButton.MyPageNew;
			}

			public void CollectDefaults()
			{
				if (this.PrevButton != null)
				{
					this._defaultPrev = this.PrevButton.MyPageNew;
				}
				if (this.NextButton != null)
				{
					this._defaultNext = this.NextButton.MyPageNew;
				}
				if (this.PrevLabel != null)
				{
					this._defaultPrevLabelText = this.PrevLabel.text;
				}
				if (this.NextLabel != null)
				{
					this._defaultNextLabelText = this.NextLabel.text;
				}
				if (this.PrevLabelTranslation != null)
				{
					this._defaultPrevLabelKey = this.PrevLabelTranslation._key;
				}
				if (this.NextLabelTranslation != null)
				{
					this._defaultNextLabelKey = this.NextLabelTranslation._key;
				}
				this._defaultActivated = this.Activated;
			}

			public void RestoreDefaults()
			{
				this.Activated = this._defaultActivated;
				if (this.NextButton != null)
				{
					this.NextButton.MyPageNew = this._defaultNext;
				}
				if (this.PrevButton != null)
				{
					this.PrevButton.MyPageNew = this._defaultPrev;
				}
				if (this.PrevLabel != null)
				{
					this.PrevLabel.text = this._defaultPrevLabelText;
				}
				if (this.NextLabel != null)
				{
					this.NextLabel.text = this._defaultNextLabelText;
				}
				if (this.PrevLabelTranslation != null)
				{
					this.PrevLabelTranslation._key = this._defaultPrevLabelKey;
				}
				if (this.NextLabelTranslation != null)
				{
					this.NextLabelTranslation._key = this._defaultNextLabelKey;
				}
			}

			public string GetPrevTargetLabelText()
			{
				return (!(this.PrevLabel != null)) ? null : this.PrevLabel.text;
			}

			public string GetNextTargetLabelText()
			{
				return (!(this.NextLabel != null)) ? null : this.NextLabel.text;
			}

			public string GetPrevTargetLabelKey()
			{
				return (!(this.PrevLabelTranslation != null)) ? null : this.PrevLabelTranslation._key;
			}

			public string GetNextTargetLabelKey()
			{
				return (!(this.NextLabelTranslation != null)) ? null : this.NextLabelTranslation._key;
			}

			public bool Anchor;

			public bool Activated;

			public GameObject Page;

			[ItemIdPicker]
			public int RequiredItemId;

			public SelectPageNumber PrevButton;

			public TextMesh PrevLabel;

			public UiTranslationTextMesh PrevLabelTranslation;

			public SelectPageNumber NextButton;

			public TextMesh NextLabel;

			public UiTranslationTextMesh NextLabelTranslation;

			private GameObject _defaultPrev;

			private GameObject _defaultNext;

			private bool _defaultActivated;

			private string _defaultPrevLabelKey;

			private string _defaultNextLabelKey;

			private string _defaultPrevLabelText;

			private string _defaultNextLabelText;
		}
	}
}
