using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Set the Tag on all children of a GameObject. Optionally filter by component.")]
	[ActionCategory(ActionCategory.GameObject)]
	public class SetTagsOnChildren : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.tag = null;
			this.filterByComponent = null;
		}

		
		public override void OnEnter()
		{
			this.SetTag(base.Fsm.GetOwnerDefaultTarget(this.gameObject));
			base.Finish();
		}

		
		private void SetTag(GameObject parent)
		{
			if (parent == null)
			{
				return;
			}
			if (string.IsNullOrEmpty(this.filterByComponent.Value))
			{
				foreach (object obj in parent.transform)
				{
					Transform transform = (Transform)obj;
					transform.gameObject.tag = this.tag.Value;
				}
			}
			else
			{
				this.UpdateComponentFilter();
				if (this.componentFilter != null)
				{
					Component[] componentsInChildren = parent.GetComponentsInChildren(this.componentFilter);
					foreach (Component component in componentsInChildren)
					{
						component.gameObject.tag = this.tag.Value;
					}
				}
			}
			base.Finish();
		}

		
		private void UpdateComponentFilter()
		{
			this.componentFilter = ReflectionUtils.GetGlobalType(this.filterByComponent.Value);
			if (this.componentFilter == null)
			{
				this.componentFilter = ReflectionUtils.GetGlobalType("UnityEngine." + this.filterByComponent.Value);
			}
			if (this.componentFilter == null)
			{
				Debug.LogWarning("Couldn't get type: " + this.filterByComponent.Value);
			}
		}

		
		[RequiredField]
		[Tooltip("GameObject Parent")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Set Tag To...")]
		[RequiredField]
		[UIHint(UIHint.Tag)]
		public FsmString tag;

		
		[UIHint(UIHint.ScriptComponent)]
		[Tooltip("Only set the Tag on children with this component.")]
		public FsmString filterByComponent;

		
		private Type componentFilter;
	}
}
