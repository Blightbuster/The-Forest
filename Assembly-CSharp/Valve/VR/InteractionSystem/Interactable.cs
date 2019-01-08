using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class Interactable : MonoBehaviour
	{
		[HideInInspector]
		public event Interactable.OnAttachedToHandDelegate onAttachedToHand;

		[HideInInspector]
		public event Interactable.OnDetachedFromHandDelegate onDetachedFromHand;

		private void OnAttachedToHand(Hand hand)
		{
			if (this.onAttachedToHand != null)
			{
				this.onAttachedToHand(hand);
			}
		}

		private void OnDetachedFromHand(Hand hand)
		{
			if (this.onDetachedFromHand != null)
			{
				this.onDetachedFromHand(hand);
			}
		}

		public delegate void OnAttachedToHandDelegate(Hand hand);

		public delegate void OnDetachedFromHandDelegate(Hand hand);
	}
}
