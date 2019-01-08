using System;
using System.Collections.Generic;

[Serializable]
public class ObservedList<T> : List<T>
{
	public event Action<int> Changed = delegate
	{
	};

	public new T this[int index]
	{
		get
		{
			return base[index];
		}
		set
		{
			base[index] = value;
			this.Changed(index);
		}
	}
}
