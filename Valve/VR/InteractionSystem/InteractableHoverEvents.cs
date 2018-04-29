using System;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
	
	[RequireComponent(typeof(Interactable))]
	public class InteractableHoverEvents : MonoBehaviour
	{
		
		private void OnHandHoverBegin()
		{
			this.onHandHoverBegin.Invoke();
		}

		
		private void OnHandHoverEnd()
		{
			this.onHandHoverEnd.Invoke();
		}

		
		private void OnAttachedToHand(Hand hand)
		{
			this.onAttachedToHand.Invoke();
		}

		
		private void OnDetachedFromHand(Hand hand)
		{
			this.onDetachedFromHand.Invoke();
		}

		
		public UnityEvent onHandHoverBegin;

		
		public UnityEvent onHandHoverEnd;

		
		public UnityEvent onAttachedToHand;

		
		public UnityEvent onDetachedFromHand;
	}
}
