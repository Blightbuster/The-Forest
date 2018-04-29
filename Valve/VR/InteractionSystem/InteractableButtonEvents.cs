using System;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
	
	[RequireComponent(typeof(Interactable))]
	public class InteractableButtonEvents : MonoBehaviour
	{
		
		private void Update()
		{
			for (int i = 0; i < Player.instance.handCount; i++)
			{
				Hand hand = Player.instance.GetHand(i);
				if (hand.controller != null)
				{
					if (hand.controller.GetPressDown(EVRButtonId.k_EButton_Axis1))
					{
						this.onTriggerDown.Invoke();
					}
					if (hand.controller.GetPressUp(EVRButtonId.k_EButton_Axis1))
					{
						this.onTriggerUp.Invoke();
					}
					if (hand.controller.GetPressDown(EVRButtonId.k_EButton_Grip))
					{
						this.onGripDown.Invoke();
					}
					if (hand.controller.GetPressUp(EVRButtonId.k_EButton_Grip))
					{
						this.onGripUp.Invoke();
					}
					if (hand.controller.GetPressDown(EVRButtonId.k_EButton_Axis0))
					{
						this.onTouchpadDown.Invoke();
					}
					if (hand.controller.GetPressUp(EVRButtonId.k_EButton_Axis0))
					{
						this.onTouchpadUp.Invoke();
					}
					if (hand.controller.GetTouchDown(EVRButtonId.k_EButton_Axis0))
					{
						this.onTouchpadTouch.Invoke();
					}
					if (hand.controller.GetTouchUp(EVRButtonId.k_EButton_Axis0))
					{
						this.onTouchpadRelease.Invoke();
					}
				}
			}
		}

		
		public UnityEvent onTriggerDown;

		
		public UnityEvent onTriggerUp;

		
		public UnityEvent onGripDown;

		
		public UnityEvent onGripUp;

		
		public UnityEvent onTouchpadDown;

		
		public UnityEvent onTouchpadUp;

		
		public UnityEvent onTouchpadTouch;

		
		public UnityEvent onTouchpadRelease;
	}
}
