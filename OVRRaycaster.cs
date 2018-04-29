using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(Canvas))]
public class OVRRaycaster : GraphicRaycaster, IPointerEnterHandler, IEventSystemHandler
{
	
	protected OVRRaycaster()
	{
	}

	
	
	private Canvas canvas
	{
		get
		{
			if (this.m_Canvas != null)
			{
				return this.m_Canvas;
			}
			this.m_Canvas = base.GetComponent<Canvas>();
			return this.m_Canvas;
		}
	}

	
	
	public override Camera eventCamera
	{
		get
		{
			return this.canvas.worldCamera;
		}
	}

	
	
	public override int sortOrderPriority
	{
		get
		{
			return this.sortOrder;
		}
	}

	
	private void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList, Ray ray, bool checkForBlocking)
	{
		if (this.canvas == null)
		{
			return;
		}
		float num = float.MaxValue;
		if (checkForBlocking && base.blockingObjects != GraphicRaycaster.BlockingObjects.None)
		{
			float farClipPlane = this.eventCamera.farClipPlane;
			if (base.blockingObjects == GraphicRaycaster.BlockingObjects.ThreeD || base.blockingObjects == GraphicRaycaster.BlockingObjects.All)
			{
				UnityEngine.RaycastHit[] array = Physics.RaycastAll(ray, farClipPlane, this.m_BlockingMask);
				if (array.Length > 0 && array[0].distance < num)
				{
					num = array[0].distance;
				}
			}
			if (base.blockingObjects == GraphicRaycaster.BlockingObjects.TwoD || base.blockingObjects == GraphicRaycaster.BlockingObjects.All)
			{
				RaycastHit2D[] rayIntersectionAll = Physics2D.GetRayIntersectionAll(ray, farClipPlane, this.m_BlockingMask);
				if (rayIntersectionAll.Length > 0 && rayIntersectionAll[0].fraction * farClipPlane < num)
				{
					num = rayIntersectionAll[0].fraction * farClipPlane;
				}
			}
		}
		this.m_RaycastResults.Clear();
		this.GraphicRaycast(this.canvas, ray, this.m_RaycastResults);
		for (int i = 0; i < this.m_RaycastResults.Count; i++)
		{
			GameObject gameObject = this.m_RaycastResults[i].graphic.gameObject;
			bool flag = true;
			if (base.ignoreReversedGraphics)
			{
				Vector3 direction = ray.direction;
				Vector3 rhs = gameObject.transform.rotation * Vector3.forward;
				flag = (Vector3.Dot(direction, rhs) > 0f);
			}
			if (this.eventCamera.transform.InverseTransformPoint(this.m_RaycastResults[i].worldPos).z <= 0f)
			{
				flag = false;
			}
			if (flag)
			{
				float num2 = Vector3.Distance(ray.origin, this.m_RaycastResults[i].worldPos);
				if (num2 < num)
				{
					RaycastResult item = new RaycastResult
					{
						gameObject = gameObject,
						module = this,
						distance = num2,
						index = (float)resultAppendList.Count,
						depth = this.m_RaycastResults[i].graphic.depth,
						worldPosition = this.m_RaycastResults[i].worldPos
					};
					resultAppendList.Add(item);
				}
			}
		}
	}

	
	public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
	{
		if (eventData.IsVRPointer())
		{
			this.Raycast(eventData, resultAppendList, eventData.GetRay(), true);
		}
	}

	
	public void RaycastPointer(PointerEventData eventData, List<RaycastResult> resultAppendList)
	{
		if (this.pointer != null && this.pointer.activeInHierarchy)
		{
			this.Raycast(eventData, resultAppendList, new Ray(this.eventCamera.transform.position, (this.pointer.transform.position - this.eventCamera.transform.position).normalized), false);
		}
	}

	
	private void GraphicRaycast(Canvas canvas, Ray ray, List<OVRRaycaster.RaycastHit> results)
	{
		IList<Graphic> graphicsForCanvas = GraphicRegistry.GetGraphicsForCanvas(canvas);
		OVRRaycaster.s_SortedGraphics.Clear();
		for (int i = 0; i < graphicsForCanvas.Count; i++)
		{
			Graphic graphic = graphicsForCanvas[i];
			if (graphic.depth != -1 && !(this.pointer == graphic.gameObject))
			{
				Vector3 vector;
				if (OVRRaycaster.RayIntersectsRectTransform(graphic.rectTransform, ray, out vector))
				{
					Vector2 sp = this.eventCamera.WorldToScreenPoint(vector);
					if (graphic.Raycast(sp, this.eventCamera))
					{
						OVRRaycaster.RaycastHit item;
						item.graphic = graphic;
						item.worldPos = vector;
						item.fromMouse = false;
						OVRRaycaster.s_SortedGraphics.Add(item);
					}
				}
			}
		}
		OVRRaycaster.s_SortedGraphics.Sort((OVRRaycaster.RaycastHit g1, OVRRaycaster.RaycastHit g2) => g2.graphic.depth.CompareTo(g1.graphic.depth));
		for (int j = 0; j < OVRRaycaster.s_SortedGraphics.Count; j++)
		{
			results.Add(OVRRaycaster.s_SortedGraphics[j]);
		}
	}

	
	public Vector2 GetScreenPosition(RaycastResult raycastResult)
	{
		return this.eventCamera.WorldToScreenPoint(raycastResult.worldPosition);
	}

	
	private static bool RayIntersectsRectTransform(RectTransform rectTransform, Ray ray, out Vector3 worldPos)
	{
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Plane plane = new Plane(array[0], array[1], array[2]);
		float distance;
		if (!plane.Raycast(ray, out distance))
		{
			worldPos = Vector3.zero;
			return false;
		}
		Vector3 point = ray.GetPoint(distance);
		Vector3 vector = array[3] - array[0];
		Vector3 vector2 = array[1] - array[0];
		float num = Vector3.Dot(point - array[0], vector);
		float num2 = Vector3.Dot(point - array[0], vector2);
		if (num < vector.sqrMagnitude && num2 < vector2.sqrMagnitude && num >= 0f && num2 >= 0f)
		{
			worldPos = array[0] + num2 * vector2 / vector2.sqrMagnitude + num * vector / vector.sqrMagnitude;
			return true;
		}
		worldPos = Vector3.zero;
		return false;
	}

	
	public bool IsFocussed()
	{
		OVRInputModule ovrinputModule = EventSystem.current.currentInputModule as OVRInputModule;
		return ovrinputModule && ovrinputModule.activeGraphicRaycaster == this;
	}

	
	public void OnPointerEnter(PointerEventData e)
	{
		if (e.IsVRPointer())
		{
			OVRInputModule ovrinputModule = EventSystem.current.currentInputModule as OVRInputModule;
			ovrinputModule.activeGraphicRaycaster = this;
		}
	}

	
	[Tooltip("A world space pointer for this canvas")]
	public GameObject pointer;

	
	public int sortOrder;

	
	[NonSerialized]
	private Canvas m_Canvas;

	
	[NonSerialized]
	private List<OVRRaycaster.RaycastHit> m_RaycastResults = new List<OVRRaycaster.RaycastHit>();

	
	[NonSerialized]
	private static readonly List<OVRRaycaster.RaycastHit> s_SortedGraphics = new List<OVRRaycaster.RaycastHit>();

	
	private struct RaycastHit
	{
		
		public Graphic graphic;

		
		public Vector3 worldPos;

		
		public bool fromMouse;
	}
}
