using System;
using System.Collections;
using System.Collections.Generic;

namespace UniLinq
{
	
	public class Lookup<TKey, TElement> : IEnumerable, IEnumerable<IGrouping<TKey, TElement>>, ILookup<TKey, TElement>
	{
		
		internal Lookup(Dictionary<TKey, List<TElement>> lookup, IEnumerable<TElement> nullKeyElements)
		{
			this.groups = new Dictionary<TKey, IGrouping<TKey, TElement>>(lookup.Comparer);
			foreach (KeyValuePair<TKey, List<TElement>> keyValuePair in lookup)
			{
				this.groups.Add(keyValuePair.Key, new Grouping<TKey, TElement>(keyValuePair.Key, keyValuePair.Value));
			}
			if (nullKeyElements != null)
			{
				this.nullGrouping = new Grouping<TKey, TElement>(default(TKey), nullKeyElements);
			}
		}

		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		
		
		public int Count
		{
			get
			{
				return (this.nullGrouping != null) ? (this.groups.Count + 1) : this.groups.Count;
			}
		}

		
		public IEnumerable<TElement> this[TKey key]
		{
			get
			{
				if (key == null && this.nullGrouping != null)
				{
					return this.nullGrouping;
				}
				IGrouping<TKey, TElement> result;
				if (key != null && this.groups.TryGetValue(key, out result))
				{
					return result;
				}
				return new TElement[0];
			}
		}

		
		public IEnumerable<TResult> ApplyResultSelector<TResult>(Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
		{
			if (this.nullGrouping != null)
			{
				yield return resultSelector(this.nullGrouping.Key, this.nullGrouping);
			}
			foreach (IGrouping<TKey, TElement> group in this.groups.Values)
			{
				yield return resultSelector(group.Key, group);
			}
			yield break;
		}

		
		public bool Contains(TKey key)
		{
			return (key == null) ? (this.nullGrouping != null) : this.groups.ContainsKey(key);
		}

		
		public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
		{
			if (this.nullGrouping != null)
			{
				yield return this.nullGrouping;
			}
			foreach (IGrouping<TKey, TElement> g in this.groups.Values)
			{
				yield return g;
			}
			yield break;
		}

		
		private IGrouping<TKey, TElement> nullGrouping;

		
		private Dictionary<TKey, IGrouping<TKey, TElement>> groups;
	}
}
