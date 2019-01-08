using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Valve.VR.InteractionSystem
{
	public class InputModule : BaseInputModule
	{
		public static InputModule instance
		{
			get
			{
				if (InputModule._instance == null)
				{
					InputModule._instance = UnityEngine.Object.FindObjectOfType<InputModule>();
				}
				return InputModule._instance;
			}
		}

		public override bool ShouldActivateModule()
		{
			return base.ShouldActivateModule() && this.submitObject != null;
		}

		public void HoverBegin(GameObject gameObject)
		{
			PointerEventData eventData = new PointerEventData(base.eventSystem);
			ExecuteEvents.Execute<IPointerEnterHandler>(gameObject, eventData, ExecuteEvents.pointerEnterHandler);
		}

		public void HoverEnd(GameObject gameObject)
		{
			ExecuteEvents.Execute<IPointerExitHandler>(gameObject, new PointerEventData(base.eventSystem)
			{
				selectedObject = null
			}, ExecuteEvents.pointerExitHandler);
		}

		public void Submit(GameObject gameObject)
		{
			this.submitObject = gameObject;
		}

		public override void Process()
		{
			if (this.submitObject)
			{
				BaseEventData baseEventData = this.GetBaseEventData();
				baseEventData.selectedObject = this.submitObject;
				ExecuteEvents.Execute<ISubmitHandler>(this.submitObject, baseEventData, ExecuteEvents.submitHandler);
				this.submitObject = null;
			}
		}

		private GameObject submitObject;

		private static InputModule _instance;
	}
}
