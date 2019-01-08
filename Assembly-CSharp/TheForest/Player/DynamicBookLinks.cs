using System;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player
{
	public class DynamicBookLinks : MonoBehaviour
	{
		private void OnEnable()
		{
			foreach (DynamicBookLinks.State state in this._states)
			{
				if (state.CheckCondition())
				{
					if (state._condition != this._lastCondition)
					{
						this._lastCondition = state._condition;
						state.ApplyTo(this._pages);
					}
					break;
				}
			}
		}

		public DynamicBookLinks.Page[] _pages;

		public DynamicBookLinks.State[] _states;

		private DynamicBookLinks.State.Condition _lastCondition = (DynamicBookLinks.State.Condition)(-1);

		[Serializable]
		public class State
		{
			public bool CheckCondition()
			{
				switch (this._condition)
				{
				case DynamicBookLinks.State.Condition.FreshGame:
					return !LocalPlayer.SavedData.ReachedLowSanityThreshold && !LocalPlayer.SavedData.ExitedEndgame;
				case DynamicBookLinks.State.Condition.GoneInsane:
					return LocalPlayer.SavedData.ReachedLowSanityThreshold && !LocalPlayer.SavedData.ExitedEndgame;
				case DynamicBookLinks.State.Condition.ExitedEndgameSane:
					return !LocalPlayer.SavedData.ReachedLowSanityThreshold && LocalPlayer.SavedData.ExitedEndgame;
				case DynamicBookLinks.State.Condition.ExitedEndgameInsane:
					return LocalPlayer.SavedData.ReachedLowSanityThreshold && LocalPlayer.SavedData.ExitedEndgame;
				default:
					return false;
				}
			}

			public void ApplyTo(DynamicBookLinks.Page[] pages)
			{
				for (int i = 0; i < pages.Length; i++)
				{
					if (this._pagesState[i]._state)
					{
						if (pages[i]._prevButton)
						{
							pages[i]._prevButton.MyPageNew = this._pagesState[i]._prevButtonTarget;
						}
						if (pages[i]._nextButton)
						{
							pages[i]._nextButton.MyPageNew = this._pagesState[i]._nextButtonTarget;
						}
						if (pages[i]._prevButtonText)
						{
							pages[i]._prevButtonText._key = this._pagesState[i]._prevButtonKey;
							pages[i]._prevButtonText.SetText(UiTranslationDatabase.TranslateKey(this._pagesState[i]._prevButtonKey, this._pagesState[i]._prevButtonKey, true));
						}
						if (pages[i]._nextButtonText)
						{
							pages[i]._nextButtonText._key = this._pagesState[i]._nextButtonKey;
							pages[i]._nextButtonText.SetText(UiTranslationDatabase.TranslateKey(this._pagesState[i]._nextButtonKey, this._pagesState[i]._nextButtonKey, true));
						}
					}
					if (!pages[i]._prevButton && pages[i]._nextButton && !pages[i]._nextButtonText && pages[i]._go.activeSelf != this._pagesState[i]._state)
					{
						pages[i]._go.SetActive(this._pagesState[i]._state);
					}
				}
			}

			public DynamicBookLinks.State.Condition _condition;

			public DynamicBookLinks.State.PageState[] _pagesState;

			[Serializable]
			public class PageState
			{
				public GameObject _prevButtonTarget;

				public GameObject _nextButtonTarget;

				public string _prevButtonKey;

				public string _nextButtonKey;

				public bool _state;
			}

			public enum Condition
			{
				FreshGame,
				GoneInsane,
				ExitedEndgameSane,
				ExitedEndgameInsane
			}
		}

		[Serializable]
		public class Page
		{
			public GameObject _go;

			public SelectPageNumber _prevButton;

			public SelectPageNumber _nextButton;

			public UiTranslationTextMesh _prevButtonText;

			public UiTranslationTextMesh _nextButtonText;
		}
	}
}
