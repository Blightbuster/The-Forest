﻿using System;
using System.Reflection;

namespace Serialization
{
	public abstract class GetSet
	{
		public MemberInfo MemberInfo
		{
			get
			{
				return (this.Info == null) ? this.FieldInfo : this.Info;
			}
		}

		public int Priority = 100;

		public PropertyInfo Info;

		public string Name;

		public FieldInfo FieldInfo;

		public object Vanilla;

		public bool CollectionType;

		public Func<object, object> Get;

		public Action<object, object> Set;

		public bool IsStatic;
	}
}
