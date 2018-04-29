using System;
using UnityEngine;


public class CoopTreeFall : CoopBase<ITreeFallState>
{
	
	public override void Attached()
	{
		if (!base.entity.isOwner)
		{
			foreach (Rigidbody rigidbody in base.GetComponentsInChildren<Rigidbody>())
			{
				rigidbody.useGravity = false;
				rigidbody.isKinematic = true;
			}
			foreach (Collider collider in base.GetComponentsInChildren<Collider>())
			{
				collider.enabled = false;
			}
		}
		base.state.Transform.SetTransforms(base.transform);
		base.state.AddCallback("CutTree", delegate
		{
			if (base.state.CutTree)
			{
				Transform transform = base.state.CutTree.transform;
				Transform transform2 = null;
				for (int k = 0; k < transform.childCount; k++)
				{
					Transform child = transform.GetChild(k);
					if (child.name == "Lower")
					{
						transform2 = child;
					}
					else
					{
						child.gameObject.SetActive(false);
					}
				}
				transform2.parent = null;
			}
		});
	}
}
