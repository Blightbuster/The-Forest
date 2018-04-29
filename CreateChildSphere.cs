using System;
using UnityEngine;


public class CreateChildSphere : MonoBehaviour
{
	
	static CreateChildSphere()
	{
		DelegateSupport.RegisterFunctionType<CreateChildSphere, string>();
		DelegateSupport.RegisterFunctionType<CreateChildSphere, bool>();
		DelegateSupport.RegisterFunctionType<CreateChildSphere, Transform>();
	}

	
	private void Start()
	{
		if (!LevelSerializer.IsDeserializing && (double)UnityEngine.Random.value < 0.4)
		{
			Transform transform = UnityEngine.Object.Instantiate<Transform>(this.prefab, base.transform.position + UnityEngine.Random.onUnitSphere * 3f, Quaternion.identity);
			transform.parent = base.transform;
		}
	}

	
	public Transform prefab;
}
