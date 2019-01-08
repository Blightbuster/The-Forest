using System;
using UnityEngine;

namespace Rewired.UI.ControlMapper
{
	[AddComponentMenu("")]
	public class ThemedElement : MonoBehaviour
	{
		private void Start()
		{
			ControlMapper.ApplyTheme(this._elements);
		}

		[SerializeField]
		private ThemedElement.ElementInfo[] _elements;

		[Serializable]
		public class ElementInfo
		{
			public string themeClass
			{
				get
				{
					return this._themeClass;
				}
			}

			public Component component
			{
				get
				{
					return this._component;
				}
			}

			[SerializeField]
			private string _themeClass;

			[SerializeField]
			private Component _component;
		}
	}
}
