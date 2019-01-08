using System;

namespace TheForest.UI
{
	public class UiTranslationTextMeshDyn : UiTranslationTextMesh
	{
		private void Awake()
		{
			this._node.RegisterDynamicLabel(this);
		}

		private void OnDestroy()
		{
			this._node.UnregisterDynamicLabel(this);
		}

		public UiTranslationTextMeshNode _node;
	}
}
