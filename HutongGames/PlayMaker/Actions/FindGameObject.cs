using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.GameObject)]
	[Tooltip("Finds a Game Object by Name and/or Tag.")]
	public class FindGameObject : FsmStateAction
	{
		
		public override void Reset()
		{
			this.objectName = string.Empty;
			this.withTag = "Untagged";
			this.store = null;
		}

		
		public override void OnEnter()
		{
			base.Finish();
			if (!(this.withTag.Value != "Untagged"))
			{
				this.store.Value = GameObject.Find(this.objectName.Value);
				return;
			}
			if (!string.IsNullOrEmpty(this.objectName.Value))
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag(this.withTag.Value);
				foreach (GameObject gameObject in array)
				{
					if (gameObject.name == this.objectName.Value)
					{
						this.store.Value = gameObject;
						return;
					}
				}
				this.store.Value = null;
				return;
			}
			this.store.Value = GameObject.FindGameObjectWithTag(this.withTag.Value);
		}

		
		public override string ErrorCheck()
		{
			if (string.IsNullOrEmpty(this.objectName.Value) && string.IsNullOrEmpty(this.withTag.Value))
			{
				return "Specify Name, Tag, or both.";
			}
			return null;
		}

		
		[Tooltip("The name of the GameObject to find. You can leave this empty if you specify a Tag.")]
		public FsmString objectName;

		
		[Tooltip("Find a GameObject with this tag. If Object Name is specified then both name and Tag must match.")]
		[UIHint(UIHint.Tag)]
		public FsmString withTag;

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result in a GameObject variable.")]
		public FsmGameObject store;
	}
}
