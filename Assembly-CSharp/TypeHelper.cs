using System;
using System.Reflection;
using UniLinq;

public static class TypeHelper
{
	public static T Attribute<T>(this Type tp) where T : Attribute
	{
		return System.Attribute.GetCustomAttribute(tp, typeof(T)) as T;
	}

	public static T Attribute<T>(this object o) where T : Attribute, new()
	{
		if (o is MemberInfo)
		{
			T result;
			if ((result = (T)((object)(o as MemberInfo).GetCustomAttributes(typeof(T), false).FirstOrDefault<object>())) == null)
			{
				result = Activator.CreateInstance<T>();
			}
			return result;
		}
		if (o is ParameterInfo)
		{
			T result2;
			if ((result2 = (T)((object)(o as ParameterInfo).GetCustomAttributes(typeof(T), false).FirstOrDefault<object>())) == null)
			{
				result2 = Activator.CreateInstance<T>();
			}
			return result2;
		}
		T result3;
		if ((result3 = o.GetType().Attribute<T>()) == null)
		{
			result3 = Activator.CreateInstance<T>();
		}
		return result3;
	}
}
