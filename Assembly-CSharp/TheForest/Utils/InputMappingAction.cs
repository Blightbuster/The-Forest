using System;
using UnityEngine;

namespace TheForest.Utils
{
	[Obsolete("Use InputMappingButton / InputMappingButtonManager")]
	public class InputMappingAction : MonoBehaviour
	{
		public InputMapping _mappingManager;

		public UILabel _inputActionCategoryPrefab;

		public InputActionRow _inputActionRowPrefab;

		public InputActionButton _inputActionButtonPrefab;

		public InputActionButton _inputAxisActionButtonPrefab;

		public UITable _table;

		public UIScrollView _scrollView;

		public UIScrollBar _scrollbar;

		public UILabel _selectionScreenTimer;

		public UILabel _mappingConflictResolutionKeyLabel;

		public UILabel _mappingConflictResolutionActionLabel;

		public UILabel _mappingSystemConflictUI;

		public GameObject _cancelButton;

		public float _inputSelectionDuration = 5f;

		public float _interChangeDelay = 0.5f;

		public ActionTriggerEvent[] _actionTriggerEventToLock;
	}
}
