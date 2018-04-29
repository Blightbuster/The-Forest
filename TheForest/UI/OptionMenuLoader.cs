using System;
using UnityEngine;

namespace TheForest.UI
{
	
	public class OptionMenuLoader : MonoBehaviour
	{
		
		private void Awake()
		{
			Transform transform = UnityEngine.Object.Instantiate<Transform>(this._optionMenuPrefab);
			transform.parent = base.transform;
			transform.localPosition = Vector3.zero;
			transform.localScale = Vector3.one;
			OptionMenuTweens component = transform.GetComponent<OptionMenuTweens>();
			for (int i = 0; i < component._backwardTweener.Length; i++)
			{
				if (component._backwardTweener[i] != null)
				{
					component._backwardTweener[i].tweenTarget = base.gameObject;
				}
			}
			for (int j = 0; j < component._forwardTweener.Length; j++)
			{
				if (component._forwardTweener[j] != null)
				{
					component._forwardTweener[j].tweenTarget = this._mainMenuGO;
				}
			}
			if (component._centerOnScreen)
			{
				UIRoot componentInParent = base.GetComponentInParent<UIRoot>();
				component.transform.position = componentInParent.transform.position;
				TweenTransform component2 = base.GetComponent<TweenTransform>();
				if (component2)
				{
					component.transform.position -= component2.to.position - component2.from.position;
				}
			}
		}

		
		private void OnEnable()
		{
			this._screenResolutionHash = this.GetScreenResolutionHash();
		}

		
		private void Update()
		{
			if (this._screenResolutionHash != this.GetScreenResolutionHash())
			{
				this._screenResolutionHash = this.GetScreenResolutionHash();
				base.SendMessage("PlayForward");
			}
		}

		
		private int GetScreenResolutionHash()
		{
			return Screen.width * 100000 + Screen.height;
		}

		
		public Transform _optionMenuPrefab;

		
		public Transform _optionMenuPrefabPS4;

		
		public GameObject _mainMenuGO;

		
		public GameObject _controlSettingsGO;

		
		public GameObject _normalCursor;

		
		private int _screenResolutionHash;
	}
}
