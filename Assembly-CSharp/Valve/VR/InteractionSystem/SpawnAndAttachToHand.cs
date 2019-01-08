using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class SpawnAndAttachToHand : MonoBehaviour
	{
		public void SpawnAndAttach(Hand passedInhand)
		{
			Hand hand = passedInhand;
			if (passedInhand == null)
			{
				hand = this.hand;
			}
			if (hand == null)
			{
				return;
			}
			GameObject objectToAttach = UnityEngine.Object.Instantiate<GameObject>(this.prefab);
			hand.AttachObject(objectToAttach, Hand.AttachmentFlags.SnapOnAttach | Hand.AttachmentFlags.DetachOthers | Hand.AttachmentFlags.DetachFromOtherHand | Hand.AttachmentFlags.ParentToHand, string.Empty);
		}

		public Hand hand;

		public GameObject prefab;
	}
}
