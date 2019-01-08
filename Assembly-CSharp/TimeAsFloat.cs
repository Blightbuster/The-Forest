using System;
using Serialization;
using UnityEngine;

[SpecialistProvider]
public class TimeAsFloat : ISpecialist
{
	public object Serialize(object value)
	{
		return (float)value - Time.time;
	}

	public object Deserialize(object value)
	{
		return Time.time + (float)value;
	}
}
