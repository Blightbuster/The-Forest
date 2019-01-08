using System;

public class Index<TK, TR> : Lookup<TK, TR>, IChanged where TR : class, new()
{
	public event Action<TK, TR, TR> Setting;

	public event Action<TK, TR> Getting = delegate
	{
	};

	public void Changed(object index)
	{
		if (this.Setting != null)
		{
			TR tr = (TR)((object)null);
			if (base.ContainsKey((TK)((object)index)))
			{
				tr = base[(TK)((object)index)];
			}
			this.Setting((TK)((object)index), tr, tr);
		}
	}

	public override TR this[TK index]
	{
		get
		{
			if (base.ContainsKey(index))
			{
				return base[index];
			}
			TR tr = Activator.CreateInstance<TR>();
			if (tr is INeedParent)
			{
				(tr as INeedParent).SetParent(this, index);
			}
			base[index] = tr;
			this.Getting(index, tr);
			return tr;
		}
		set
		{
			if (this.Setting != null)
			{
				TR arg = (TR)((object)null);
				if (base.ContainsKey(index))
				{
					arg = base[index];
				}
				this.Setting(index, arg, value);
			}
			base[index] = value;
		}
	}
}
