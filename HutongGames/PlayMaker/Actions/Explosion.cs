using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Applies an explosion Force to all Game Objects with a Rigid Body inside a Radius.")]
	[ActionCategory(ActionCategory.Physics)]
	public class Explosion : FsmStateAction
	{
		
		public override void Reset()
		{
			this.center = null;
			this.upwardsModifier = 0f;
			this.forceMode = ForceMode.Force;
			this.everyFrame = false;
		}

		
		public override void Awake()
		{
			base.Fsm.HandleFixedUpdate = true;
		}

		
		public override void OnEnter()
		{
			this.DoExplosion();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnFixedUpdate()
		{
			this.DoExplosion();
		}

		
		private void DoExplosion()
		{
			Collider[] array = Physics.OverlapSphere(this.center.Value, this.radius.Value);
			foreach (Collider collider in array)
			{
				Rigidbody component = collider.gameObject.GetComponent<Rigidbody>();
				if (component != null && this.ShouldApplyForce(collider.gameObject))
				{
					component.AddExplosionForce(this.force.Value, this.center.Value, this.radius.Value, this.upwardsModifier.Value, this.forceMode);
				}
			}
		}

		
		private bool ShouldApplyForce(GameObject go)
		{
			int num = ActionHelpers.LayerArrayToLayerMask(this.layerMask, this.invertMask.Value);
			return (1 << go.layer & num) > 0;
		}

		
		[Tooltip("The world position of the center of the explosion.")]
		[RequiredField]
		public FsmVector3 center;

		
		[RequiredField]
		[Tooltip("The strength of the explosion.")]
		public FsmFloat force;

		
		[RequiredField]
		[Tooltip("The radius of the explosion. Force falls of linearly with distance.")]
		public FsmFloat radius;

		
		[Tooltip("Applies the force as if it was applied from beneath the object. This is useful since explosions that throw things up instead of pushing things to the side look cooler. A value of 2 will apply a force as if it is applied from 2 meters below while not changing the actual explosion position.")]
		public FsmFloat upwardsModifier;

		
		[Tooltip("The type of force to apply.")]
		public ForceMode forceMode;

		
		[UIHint(UIHint.Layer)]
		public FsmInt layer;

		
		[Tooltip("Layers to effect.")]
		[UIHint(UIHint.Layer)]
		public FsmInt[] layerMask;

		
		[Tooltip("Invert the mask, so you effect all layers except those defined above.")]
		public FsmBool invertMask;

		
		[Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;
	}
}
