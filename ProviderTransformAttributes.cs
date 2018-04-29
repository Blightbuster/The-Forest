﻿using System;
using Serialization;
using UnityEngine;


[AttributeListProvider(typeof(Transform))]
public class ProviderTransformAttributes : ProvideAttributes
{
	
	public ProviderTransformAttributes() : base(new string[]
	{
		"localPosition",
		"localRotation",
		"localScale"
	}, false)
	{
	}
}
