using System;


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ComponentSerializerFor : Attribute
{
	
	public ComponentSerializerFor(Type serializesType)
	{
		this.SerializesType = serializesType;
	}

	
	public Type SerializesType;
}
