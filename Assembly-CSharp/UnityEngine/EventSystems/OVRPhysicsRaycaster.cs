using System;
using System.Collections.Generic;

namespace UnityEngine.EventSystems
{
	[RequireComponent(typeof(OVRCameraRig))]
	public class OVRPhysicsRaycaster : BaseRaycaster
	{
		protected OVRPhysicsRaycaster()
		{
		}

		public override Camera eventCamera
		{
			get
			{
				return base.GetComponent<OVRCameraRig>().leftEyeCamera;
			}
		}

		public virtual int depth
		{
			get
			{
				return (!(this.eventCamera != null)) ? 16777215 : ((int)this.eventCamera.depth);
			}
		}

		public override int sortOrderPriority
		{
			get
			{
				return this.sortOrder;
			}
		}

		public int finalEventMask
		{
			get
			{
				return (!(this.eventCamera != null)) ? -1 : (this.eventCamera.cullingMask & this.m_EventMask);
			}
		}

		public LayerMask eventMask
		{
			get
			{
				return this.m_EventMask;
			}
			set
			{
				this.m_EventMask = value;
			}
		}

		public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
		{
			if (this.eventCamera == null)
			{
				return;
			}
			if (!eventData.IsVRPointer())
			{
				return;
			}
			Ray ray = eventData.GetRay();
			float maxDistance = this.eventCamera.farClipPlane - this.eventCamera.nearClipPlane;
			RaycastHit[] array = Physics.RaycastAll(ray, maxDistance, this.finalEventMask);
			if (array.Length > 1)
			{
				Array.Sort<RaycastHit>(array, (RaycastHit r1, RaycastHit r2) => r1.distance.CompareTo(r2.distance));
			}
			if (array.Length != 0)
			{
				int i = 0;
				int num = array.Length;
				while (i < num)
				{
					RaycastResult item = new RaycastResult
					{
						gameObject = array[i].collider.gameObject,
						module = this,
						distance = array[i].distance,
						index = (float)resultAppendList.Count,
						worldPosition = array[0].point,
						worldNormal = array[0].normal
					};
					resultAppendList.Add(item);
					i++;
				}
			}
		}

		public void Spherecast(PointerEventData eventData, List<RaycastResult> resultAppendList, float radius)
		{
			if (this.eventCamera == null)
			{
				return;
			}
			if (!eventData.IsVRPointer())
			{
				return;
			}
			Ray ray = eventData.GetRay();
			float maxDistance = this.eventCamera.farClipPlane - this.eventCamera.nearClipPlane;
			RaycastHit[] array = Physics.SphereCastAll(ray, radius, maxDistance, this.finalEventMask);
			if (array.Length > 1)
			{
				Array.Sort<RaycastHit>(array, (RaycastHit r1, RaycastHit r2) => r1.distance.CompareTo(r2.distance));
			}
			if (array.Length != 0)
			{
				int i = 0;
				int num = array.Length;
				while (i < num)
				{
					RaycastResult item = new RaycastResult
					{
						gameObject = array[i].collider.gameObject,
						module = this,
						distance = array[i].distance,
						index = (float)resultAppendList.Count,
						worldPosition = array[0].point,
						worldNormal = array[0].normal
					};
					resultAppendList.Add(item);
					i++;
				}
			}
		}

		public Vector2 GetScreenPos(Vector3 worldPosition)
		{
			return this.eventCamera.WorldToScreenPoint(worldPosition);
		}

		protected const int kNoEventMaskSet = -1;

		[SerializeField]
		protected LayerMask m_EventMask = -1;

		public int sortOrder = 20;
	}
}
