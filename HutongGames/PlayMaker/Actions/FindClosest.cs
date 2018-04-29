using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.GameObject)]
	[Tooltip("Finds the closest object to the specified Game Object.\nOptionally filter by Tag and Visibility.")]
	public class FindClosest : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.withTag = "Untagged";
			this.ignoreOwner = true;
			this.mustBeVisible = false;
			this.storeObject = null;
			this.storeDistance = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoFindClosest();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoFindClosest();
		}

		
		private void DoFindClosest()
		{
			GameObject gameObject = (this.gameObject.OwnerOption != OwnerDefaultOption.UseOwner) ? this.gameObject.GameObject.Value : base.Owner;
			GameObject[] array;
			if (string.IsNullOrEmpty(this.withTag.Value) || this.withTag.Value == "Untagged")
			{
				array = (GameObject[])UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
			}
			else
			{
				array = GameObject.FindGameObjectsWithTag(this.withTag.Value);
			}
			GameObject value = null;
			float num = float.PositiveInfinity;
			foreach (GameObject gameObject2 in array)
			{
				if (!this.ignoreOwner.Value || !(gameObject2 == base.Owner))
				{
					if (!this.mustBeVisible.Value || ActionHelpers.IsVisible(gameObject2))
					{
						float sqrMagnitude = (gameObject.transform.position - gameObject2.transform.position).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							num = sqrMagnitude;
							value = gameObject2;
						}
					}
				}
			}
			this.storeObject.Value = value;
			if (!this.storeDistance.IsNone)
			{
				this.storeDistance.Value = Mathf.Sqrt(num);
			}
		}

		
		[Tooltip("The GameObject to measure from.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[UIHint(UIHint.Tag)]
		[Tooltip("Only consider objects with this Tag. NOTE: It's generally a lot quicker to find objects with a Tag!")]
		[RequiredField]
		public FsmString withTag;

		
		[Tooltip("If checked, ignores the object that owns this FSM.")]
		public FsmBool ignoreOwner;

		
		[Tooltip("Only consider objects visible to the camera.")]
		public FsmBool mustBeVisible;

		
		[Tooltip("Store the closest object.")]
		[UIHint(UIHint.Variable)]
		public FsmGameObject storeObject;

		
		[Tooltip("Store the distance to the closest object.")]
		[UIHint(UIHint.Variable)]
		public FsmFloat storeDistance;

		
		[Tooltip("Repeat every frame")]
		public bool everyFrame;
	}
}
