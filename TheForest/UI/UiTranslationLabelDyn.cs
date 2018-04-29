using System;

namespace TheForest.UI
{
	
	public class UiTranslationLabelDyn : UiTranslationLabel
	{
		
		private void Awake()
		{
			this._node.RegisterDynamicLabel(this);
		}

		
		private void OnDestroy()
		{
			this._node.UnregisterDynamicLabel(this);
		}

		
		public UiTranslationNode _node;
	}
}
