using System;
using Serialization;
using UnityEngine;


[AttributeListProvider(typeof(Camera))]
public class ProvideCameraAttributes : ProvideAttributes
{
	
	public ProvideCameraAttributes() : base(new string[0])
	{
	}
}
