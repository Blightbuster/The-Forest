using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem
{
	[RequireComponent(typeof(Interactable))]
	public class UIElement : MonoBehaviour
	{
		private void Awake()
		{
			Button component = base.GetComponent<Button>();
			if (component)
			{
				component.onClick.AddListener(new UnityAction(this.OnButtonClick));
			}
		}

		private void OnHandHoverBegin(Hand hand)
		{
			this.currentHand = hand;
			InputModule.instance.HoverBegin(base.gameObject);
			ControllerButtonHints.ShowButtonHint(hand, new EVRButtonId[]
			{
				EVRButtonId.k_EButton_Axis1
			});
		}

		private void OnHandHoverEnd(Hand hand)
		{
			InputModule.instance.HoverEnd(base.gameObject);
			ControllerButtonHints.HideButtonHint(hand, new EVRButtonId[]
			{
				EVRButtonId.k_EButton_Axis1
			});
			this.currentHand = null;
		}

		private void HandHoverUpdate(Hand hand)
		{
			if (hand.GetStandardInteractionButtonDown())
			{
				InputModule.instance.Submit(base.gameObject);
				ControllerButtonHints.HideButtonHint(hand, new EVRButtonId[]
				{
					EVRButtonId.k_EButton_Axis1
				});
			}
		}

		private void OnButtonClick()
		{
			this.onHandClick.Invoke(this.currentHand);
		}

		public CustomEvents.UnityEventHand onHandClick;

		private Hand currentHand;
	}
}
