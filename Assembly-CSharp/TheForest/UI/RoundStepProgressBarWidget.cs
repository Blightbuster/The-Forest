using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.UI
{
	public class RoundStepProgressBarWidget : MonoBehaviour
	{
		public void Show(int currentStep, int maxSteps, bool canAdd)
		{
			bool flag = false;
			currentStep = Mathf.Clamp(currentStep, 0, maxSteps);
			while (this._shownStepSprites.Count > maxSteps)
			{
				flag = true;
				this.ReturnStepSprite(this._shownStepSprites.Pop());
			}
			while (this._shownStepSprites.Count < maxSteps)
			{
				flag = true;
				this._shownStepSprites.Push(this.GetStepSprite());
			}
			if (flag || currentStep != this._lastCurrentStepDisplayed)
			{
				int num = 0;
				float num2 = -360f / (float)maxSteps;
				float fillAmount = 1f / (float)maxSteps - Mathf.Min(1f / (float)maxSteps * 0.5f, 0.025f);
				foreach (UISprite uisprite in this._shownStepSprites)
				{
					uisprite.transform.localEulerAngles = new Vector3(0f, 0f, num2 * (float)num);
					uisprite.fillAmount = fillAmount;
					uisprite.color = ((num >= currentStep) ? this._clearColor : this._filledColor);
					num++;
				}
				this._lastCurrentStepDisplayed = currentStep;
			}
			this.UpdateIconColor(canAdd);
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
			}
		}

		public void Hide()
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(false);
			}
		}

		private UISprite GetStepSprite()
		{
			UISprite uisprite;
			if (this._pooledStepSprites.Count > 0)
			{
				uisprite = this._pooledStepSprites.Dequeue();
			}
			else
			{
				uisprite = UnityEngine.Object.Instantiate<UISprite>(this._stepSprite);
				uisprite.transform.parent = base.transform;
				uisprite.transform.localPosition = Vector3.zero;
				uisprite.transform.localScale = Vector3.one;
			}
			uisprite.gameObject.SetActive(true);
			return uisprite;
		}

		private void ReturnStepSprite(UISprite stepSprite)
		{
			stepSprite.gameObject.SetActive(false);
			this._pooledStepSprites.Enqueue(stepSprite);
		}

		private void UpdateIconColor(bool canAdd)
		{
			if (this._icon && this._lastIconCanAddState != canAdd)
			{
				this._lastIconCanAddState = canAdd;
				this._icon.color = ((!canAdd) ? this._cannotAddColor : this._canAddColor);
			}
		}

		[Header("Icon")]
		public UISprite _icon;

		public Color _canAddColor;

		public Color _cannotAddColor;

		[Header("Round steps")]
		public UISprite _stepSprite;

		public Color _clearColor;

		public Color _filledColor;

		private bool _lastIconCanAddState = true;

		private int _lastCurrentStepDisplayed = -1;

		private Queue<UISprite> _pooledStepSprites = new Queue<UISprite>();

		private Stack<UISprite> _shownStepSprites = new Stack<UISprite>();
	}
}
