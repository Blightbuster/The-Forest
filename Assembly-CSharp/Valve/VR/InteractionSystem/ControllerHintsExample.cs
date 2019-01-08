using System;
using System.Collections;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class ControllerHintsExample : MonoBehaviour
	{
		public void ShowButtonHints(Hand hand)
		{
			if (this.buttonHintCoroutine != null)
			{
				base.StopCoroutine(this.buttonHintCoroutine);
			}
			this.buttonHintCoroutine = base.StartCoroutine(this.TestButtonHints(hand));
		}

		public void ShowTextHints(Hand hand)
		{
			if (this.textHintCoroutine != null)
			{
				base.StopCoroutine(this.textHintCoroutine);
			}
			this.textHintCoroutine = base.StartCoroutine(this.TestTextHints(hand));
		}

		public void DisableHints()
		{
			if (this.buttonHintCoroutine != null)
			{
				base.StopCoroutine(this.buttonHintCoroutine);
				this.buttonHintCoroutine = null;
			}
			if (this.textHintCoroutine != null)
			{
				base.StopCoroutine(this.textHintCoroutine);
				this.textHintCoroutine = null;
			}
			foreach (Hand hand in Player.instance.hands)
			{
				ControllerButtonHints.HideAllButtonHints(hand);
				ControllerButtonHints.HideAllTextHints(hand);
			}
		}

		private IEnumerator TestButtonHints(Hand hand)
		{
			ControllerButtonHints.HideAllButtonHints(hand);
			for (;;)
			{
				ControllerButtonHints.ShowButtonHint(hand, new EVRButtonId[]
				{
					EVRButtonId.k_EButton_ApplicationMenu
				});
				yield return new WaitForSeconds(1f);
				ControllerButtonHints.ShowButtonHint(hand, new EVRButtonId[1]);
				yield return new WaitForSeconds(1f);
				ControllerButtonHints.ShowButtonHint(hand, new EVRButtonId[]
				{
					EVRButtonId.k_EButton_Grip
				});
				yield return new WaitForSeconds(1f);
				ControllerButtonHints.ShowButtonHint(hand, new EVRButtonId[]
				{
					EVRButtonId.k_EButton_Axis1
				});
				yield return new WaitForSeconds(1f);
				ControllerButtonHints.ShowButtonHint(hand, new EVRButtonId[]
				{
					EVRButtonId.k_EButton_Axis0
				});
				yield return new WaitForSeconds(1f);
				ControllerButtonHints.HideAllButtonHints(hand);
				yield return new WaitForSeconds(1f);
			}
			yield break;
		}

		private IEnumerator TestTextHints(Hand hand)
		{
			ControllerButtonHints.HideAllTextHints(hand);
			for (;;)
			{
				ControllerButtonHints.ShowTextHint(hand, EVRButtonId.k_EButton_ApplicationMenu, "Application", true);
				yield return new WaitForSeconds(3f);
				ControllerButtonHints.ShowTextHint(hand, EVRButtonId.k_EButton_System, "System", true);
				yield return new WaitForSeconds(3f);
				ControllerButtonHints.ShowTextHint(hand, EVRButtonId.k_EButton_Grip, "Grip", true);
				yield return new WaitForSeconds(3f);
				ControllerButtonHints.ShowTextHint(hand, EVRButtonId.k_EButton_Axis1, "Trigger", true);
				yield return new WaitForSeconds(3f);
				ControllerButtonHints.ShowTextHint(hand, EVRButtonId.k_EButton_Axis0, "Touchpad", true);
				yield return new WaitForSeconds(3f);
				ControllerButtonHints.HideAllTextHints(hand);
				yield return new WaitForSeconds(3f);
			}
			yield break;
		}

		private Coroutine buttonHintCoroutine;

		private Coroutine textHintCoroutine;
	}
}
