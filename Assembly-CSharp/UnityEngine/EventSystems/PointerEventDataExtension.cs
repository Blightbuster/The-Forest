using System;

namespace UnityEngine.EventSystems
{
	public static class PointerEventDataExtension
	{
		public static bool IsVRPointer(this PointerEventData pointerEventData)
		{
			return pointerEventData is OVRPointerEventData;
		}

		public static Ray GetRay(this PointerEventData pointerEventData)
		{
			OVRPointerEventData ovrpointerEventData = pointerEventData as OVRPointerEventData;
			return ovrpointerEventData.worldSpaceRay;
		}

		public static Vector2 GetSwipeStart(this PointerEventData pointerEventData)
		{
			OVRPointerEventData ovrpointerEventData = pointerEventData as OVRPointerEventData;
			return ovrpointerEventData.swipeStart;
		}

		public static void SetSwipeStart(this PointerEventData pointerEventData, Vector2 start)
		{
			OVRPointerEventData ovrpointerEventData = pointerEventData as OVRPointerEventData;
			ovrpointerEventData.swipeStart = start;
		}
	}
}
