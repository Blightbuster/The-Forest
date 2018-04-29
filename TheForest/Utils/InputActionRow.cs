using System;
using Rewired;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class InputActionRow : MonoBehaviour
	{
		
		public UILabel _label;

		
		[NameFromEnumIndex(typeof(ControllerType))]
		public UIButton[] _addButtons;

		
		public InputAction _action;
	}
}
