using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	public class GamepadInputToSelectedPopup : MonoBehaviour
	{
		private void Awake()
		{
			this._selectedControl = base.GetComponent<GamepadSelectedControl>();
		}

		private void Update()
		{
			if (TheForest.Utils.Input.IsGamePad && this.CurrentTarget)
			{
				float axisDown = TheForest.Utils.Input.GetAxisDown("SetOption");
				if (Mathf.Abs(axisDown) > this._deadZone && Mathf.Abs(axisDown) > Mathf.Abs(TheForest.Utils.Input.GetAxisDown("Vertical")))
				{
					if (this.ForwardButton && axisDown > 0f)
					{
						this.ForwardButton.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
					}
					else if (this.BackwardButton && axisDown < 0f)
					{
						this.BackwardButton.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}

		public GameObject CurrentTarget { get; set; }

		public GameObject ForwardButton { get; set; }

		public GameObject BackwardButton { get; set; }

		public float _deadZone = 0.075f;

		private GamepadSelectedControl _selectedControl;
	}
}
