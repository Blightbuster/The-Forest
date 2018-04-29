using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Creates a Game Object on all clients in a network game.")]
	public class NetworkInstantiate : FsmStateAction
	{
		
		public override void Reset()
		{
			this.prefab = null;
			this.spawnPoint = null;
			this.position = new FsmVector3
			{
				UseVariable = true
			};
			this.rotation = new FsmVector3
			{
				UseVariable = true
			};
			this.storeObject = null;
			this.networkGroup = 0;
		}

		
		public override void OnEnter()
		{
			GameObject value = this.prefab.Value;
			if (value != null)
			{
				Vector3 a = Vector3.zero;
				Vector3 euler = Vector3.up;
				if (this.spawnPoint.Value != null)
				{
					a = this.spawnPoint.Value.transform.position;
					if (!this.position.IsNone)
					{
						a += this.position.Value;
					}
					euler = (this.rotation.IsNone ? this.spawnPoint.Value.transform.eulerAngles : this.rotation.Value);
				}
				else
				{
					if (!this.position.IsNone)
					{
						a = this.position.Value;
					}
					if (!this.rotation.IsNone)
					{
						euler = this.rotation.Value;
					}
				}
				GameObject value2 = (GameObject)Network.Instantiate(value, a, Quaternion.Euler(euler), this.networkGroup.Value);
				this.storeObject.Value = value2;
			}
			base.Finish();
		}

		
		[RequiredField]
		[Tooltip("The prefab will be instanted on all clients in the game.")]
		public FsmGameObject prefab;

		
		[Tooltip("Optional Spawn Point.")]
		public FsmGameObject spawnPoint;

		
		[Tooltip("Spawn Position. If a Spawn Point is defined, this is used as a local offset from the Spawn Point position.")]
		public FsmVector3 position;

		
		[Tooltip("Spawn Rotation. NOTE: Overrides the rotation of the Spawn Point.")]
		public FsmVector3 rotation;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Optionally store the created object.")]
		public FsmGameObject storeObject;

		
		[Tooltip("Usually 0. The group number allows you to group together network messages which allows you to filter them if so desired.")]
		public FsmInt networkGroup;
	}
}
