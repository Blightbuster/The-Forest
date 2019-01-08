using System;
using System.Collections;
using System.Collections.Generic;

namespace UniLinq
{
	public static class Enumerable
	{
		public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
		{
			Check.SourceAndFunc(source, func);
			TSource result;
			using (IEnumerator<TSource> enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					throw Enumerable.EmptySequence();
				}
				TSource tsource = enumerator.Current;
				while (enumerator.MoveNext())
				{
					TSource arg = enumerator.Current;
					tsource = func(tsource, arg);
				}
				result = tsource;
			}
			return result;
		}

		public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
		{
			Check.SourceAndFunc(source, func);
			TAccumulate taccumulate = seed;
			foreach (TSource arg in source)
			{
				taccumulate = func(taccumulate, arg);
			}
			return taccumulate;
		}

		public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
		{
			Check.SourceAndFunc(source, func);
			if (resultSelector == null)
			{
				throw new ArgumentNullException("resultSelector");
			}
			TAccumulate arg = seed;
			foreach (TSource arg2 in source)
			{
				arg = func(arg, arg2);
			}
			return resultSelector(arg);
		}

		public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate(source, predicate);
			foreach (TSource arg in source)
			{
				if (!predicate(arg))
				{
					return false;
				}
			}
			return true;
		}

		public static bool Any<TSource>(this IEnumerable<TSource> source)
		{
			Check.Source(source);
			ICollection<TSource> collection = source as ICollection<TSource>;
			if (collection != null)
			{
				return collection.Count > 0;
			}
			bool result;
			using (IEnumerator<TSource> enumerator = source.GetEnumerator())
			{
				result = enumerator.MoveNext();
			}
			return result;
		}

		public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate(source, predicate);
			foreach (TSource arg in source)
			{
				if (predicate(arg))
				{
					return true;
				}
			}
			return false;
		}

		public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source)
		{
			return source;
		}

		public static double Average(this IEnumerable<int> source)
		{
			Check.Source(source);
			long num = 0L;
			int num2 = 0;
			foreach (int num3 in source)
			{
				checked
				{
					num += unchecked((long)num3);
				}
				num2++;
			}
			if (num2 == 0)
			{
				throw Enumerable.EmptySequence();
			}
			return (double)num / (double)num2;
		}

		public static double Average(this IEnumerable<long> source)
		{
			Check.Source(source);
			long num = 0L;
			long num2 = 0L;
			foreach (long num3 in source)
			{
				num += num3;
				num2 += 1L;
			}
			if (num2 == 0L)
			{
				throw Enumerable.EmptySequence();
			}
			return (double)num / (double)num2;
		}

		public static double Average(this IEnumerable<double> source)
		{
			Check.Source(source);
			double num = 0.0;
			long num2 = 0L;
			foreach (double num3 in source)
			{
				num += num3;
				num2 += 1L;
			}
			if (num2 == 0L)
			{
				throw Enumerable.EmptySequence();
			}
			return num / (double)num2;
		}

		public static float Average(this IEnumerable<float> source)
		{
			Check.Source(source);
			float num = 0f;
			long num2 = 0L;
			foreach (float num3 in source)
			{
				num += num3;
				num2 += 1L;
			}
			if (num2 == 0L)
			{
				throw Enumerable.EmptySequence();
			}
			return num / (float)num2;
		}

		public static decimal Average(this IEnumerable<decimal> source)
		{
			Check.Source(source);
			decimal d = 0m;
			long num = 0L;
			foreach (decimal d2 in source)
			{
				d += d2;
				num += 1L;
			}
			if (num == 0L)
			{
				throw Enumerable.EmptySequence();
			}
			return d / num;
		}

		private static TResult? AverageNullable<TElement, TAggregate, TResult>(this IEnumerable<TElement?> source, Func<TAggregate, TElement, TAggregate> func, Func<TAggregate, long, TResult> result) where TElement : struct where TAggregate : struct where TResult : struct
		{
			Check.Source(source);
			TAggregate arg = default(TAggregate);
			long num = 0L;
			foreach (TElement? telement in source)
			{
				if (telement != null)
				{
					arg = func(arg, telement.Value);
					num += 1L;
				}
			}
			if (num == 0L)
			{
				return null;
			}
			return new TResult?(result(arg, num));
		}

		public static double? Average(this IEnumerable<int?> source)
		{
			Check.Source(source);
			long num = 0L;
			long num2 = 0L;
			foreach (int? num3 in source)
			{
				if (num3 != null)
				{
					num += (long)num3.Value;
					num2 += 1L;
				}
			}
			if (num2 == 0L)
			{
				return null;
			}
			return new double?((double)num / (double)num2);
		}

		public static double? Average(this IEnumerable<long?> source)
		{
			Check.Source(source);
			long num = 0L;
			long num2 = 0L;
			foreach (long? num3 in source)
			{
				if (num3 != null)
				{
					checked
					{
						num += num3.Value;
					}
					num2 += 1L;
				}
			}
			if (num2 == 0L)
			{
				return null;
			}
			return new double?((double)num / (double)num2);
		}

		public static double? Average(this IEnumerable<double?> source)
		{
			Check.Source(source);
			double num = 0.0;
			long num2 = 0L;
			foreach (double? num3 in source)
			{
				if (num3 != null)
				{
					num += num3.Value;
					num2 += 1L;
				}
			}
			if (num2 == 0L)
			{
				return null;
			}
			return new double?(num / (double)num2);
		}

		public static decimal? Average(this IEnumerable<decimal?> source)
		{
			Check.Source(source);
			decimal d = 0m;
			long num = 0L;
			foreach (decimal? num2 in source)
			{
				if (num2 != null)
				{
					d += num2.Value;
					num += 1L;
				}
			}
			if (num == 0L)
			{
				return null;
			}
			return new decimal?(d / num);
		}

		public static float? Average(this IEnumerable<float?> source)
		{
			Check.Source(source);
			float num = 0f;
			long num2 = 0L;
			foreach (float? num3 in source)
			{
				if (num3 != null)
				{
					num += num3.Value;
					num2 += 1L;
				}
			}
			if (num2 == 0L)
			{
				return null;
			}
			return new float?(num / (float)num2);
		}

		public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
		{
			Check.SourceAndSelector(source, selector);
			long num = 0L;
			long num2 = 0L;
			foreach (TSource arg in source)
			{
				num += (long)selector(arg);
				num2 += 1L;
			}
			if (num2 == 0L)
			{
				throw Enumerable.EmptySequence();
			}
			return (double)num / (double)num2;
		}

		public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
		{
			Check.SourceAndSelector(source, selector);
			long num = 0L;
			long num2 = 0L;
			foreach (TSource arg in source)
			{
				int? num3 = selector(arg);
				if (num3 != null)
				{
					num += (long)num3.Value;
					num2 += 1L;
				}
			}
			if (num2 == 0L)
			{
				return null;
			}
			return new double?((double)num / (double)num2);
		}

		public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
		{
			Check.SourceAndSelector(source, selector);
			long num = 0L;
			long num2 = 0L;
			foreach (TSource arg in source)
			{
				checked
				{
					num += selector(arg);
				}
				num2 += 1L;
			}
			if (num2 == 0L)
			{
				throw Enumerable.EmptySequence();
			}
			return (double)num / (double)num2;
		}

		public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
		{
			Check.SourceAndSelector(source, selector);
			long num = 0L;
			long num2 = 0L;
			foreach (TSource arg in source)
			{
				long? num3 = selector(arg);
				if (num3 != null)
				{
					checked
					{
						num += num3.Value;
					}
					num2 += 1L;
				}
			}
			if (num2 == 0L)
			{
				return null;
			}
			return new double?((double)num / (double)num2);
		}

		public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
		{
			Check.SourceAndSelector(source, selector);
			double num = 0.0;
			long num2 = 0L;
			foreach (TSource arg in source)
			{
				num += selector(arg);
				num2 += 1L;
			}
			if (num2 == 0L)
			{
				throw Enumerable.EmptySequence();
			}
			return num / (double)num2;
		}

		public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
		{
			Check.SourceAndSelector(source, selector);
			double num = 0.0;
			long num2 = 0L;
			foreach (TSource arg in source)
			{
				double? num3 = selector(arg);
				if (num3 != null)
				{
					num += num3.Value;
					num2 += 1L;
				}
			}
			if (num2 == 0L)
			{
				return null;
			}
			return new double?(num / (double)num2);
		}

		public static float Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
		{
			Check.SourceAndSelector(source, selector);
			float num = 0f;
			long num2 = 0L;
			foreach (TSource arg in source)
			{
				num += selector(arg);
				num2 += 1L;
			}
			if (num2 == 0L)
			{
				throw Enumerable.EmptySequence();
			}
			return num / (float)num2;
		}

		public static float? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
		{
			Check.SourceAndSelector(source, selector);
			float num = 0f;
			long num2 = 0L;
			foreach (TSource arg in source)
			{
				float? num3 = selector(arg);
				if (num3 != null)
				{
					num += num3.Value;
					num2 += 1L;
				}
			}
			if (num2 == 0L)
			{
				return null;
			}
			return new float?(num / (float)num2);
		}

		public static decimal Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
		{
			Check.SourceAndSelector(source, selector);
			decimal d = 0m;
			long num = 0L;
			foreach (TSource arg in source)
			{
				d += selector(arg);
				num += 1L;
			}
			if (num == 0L)
			{
				throw Enumerable.EmptySequence();
			}
			return d / num;
		}

		public static decimal? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
		{
			Check.SourceAndSelector(source, selector);
			decimal d = 0m;
			long num = 0L;
			foreach (TSource arg in source)
			{
				decimal? num2 = selector(arg);
				if (num2 != null)
				{
					d += num2.Value;
					num += 1L;
				}
			}
			if (num == 0L)
			{
				return null;
			}
			return new decimal?(d / num);
		}

		public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source)
		{
			Check.Source(source);
			IEnumerable<TResult> enumerable = source as IEnumerable<TResult>;
			if (enumerable != null)
			{
				return enumerable;
			}
			return Enumerable.CreateCastIterator<TResult>(source);
		}

		private static IEnumerable<TResult> CreateCastIterator<TResult>(IEnumerable source)
		{
			IEnumerator enumerator = source.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					TResult element = (TResult)((object)obj);
					yield return element;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			yield break;
		}

		public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			Check.FirstAndSecond(first, second);
			return Enumerable.CreateConcatIterator<TSource>(first, second);
		}

		private static IEnumerable<TSource> CreateConcatIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			foreach (TSource element in first)
			{
				yield return element;
			}
			foreach (TSource element2 in second)
			{
				yield return element2;
			}
			yield break;
		}

		public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value)
		{
			ICollection<TSource> collection = source as ICollection<TSource>;
			if (collection != null)
			{
				return collection.Contains(value);
			}
			return source.Contains(value, null);
		}

		public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
		{
			Check.Source(source);
			if (comparer == null)
			{
				comparer = EqualityComparer<TSource>.Default;
			}
			foreach (TSource x in source)
			{
				if (comparer.Equals(x, value))
				{
					return true;
				}
			}
			return false;
		}

		public static int Count<TSource>(this IEnumerable<TSource> source)
		{
			Check.Source(source);
			ICollection<TSource> collection = source as ICollection<TSource>;
			if (collection != null)
			{
				return collection.Count;
			}
			int num = 0;
			checked
			{
				using (IEnumerator<TSource> enumerator = source.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						num++;
					}
				}
				return num;
			}
		}

		public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndSelector(source, predicate);
			int num = 0;
			checked
			{
				foreach (TSource arg in source)
				{
					if (predicate(arg))
					{
						num++;
					}
				}
				return num;
			}
		}

		public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source)
		{
			return source.DefaultIfEmpty(default(TSource));
		}

		public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source, TSource defaultValue)
		{
			Check.Source(source);
			return Enumerable.CreateDefaultIfEmptyIterator<TSource>(source, defaultValue);
		}

		private static IEnumerable<TSource> CreateDefaultIfEmptyIterator<TSource>(IEnumerable<TSource> source, TSource defaultValue)
		{
			bool empty = true;
			foreach (TSource item in source)
			{
				empty = false;
				yield return item;
			}
			if (empty)
			{
				yield return defaultValue;
			}
			yield break;
		}

		public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source)
		{
			return source.Distinct(null);
		}

		public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
		{
			Check.Source(source);
			if (comparer == null)
			{
				comparer = EqualityComparer<TSource>.Default;
			}
			return Enumerable.CreateDistinctIterator<TSource>(source, comparer);
		}

		private static IEnumerable<TSource> CreateDistinctIterator<TSource>(IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
		{
			HashSet<TSource> items = new HashSet<TSource>(comparer);
			foreach (TSource element in source)
			{
				if (!items.Contains(element))
				{
					items.Add(element);
					yield return element;
				}
			}
			yield break;
		}

		private static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index, Enumerable.Fallback fallback)
		{
			long num = 0L;
			foreach (TSource result in source)
			{
				long num2 = (long)index;
				long num3 = num;
				num = num3 + 1L;
				if (num2 == num3)
				{
					return result;
				}
			}
			if (fallback == Enumerable.Fallback.Throw)
			{
				throw new ArgumentOutOfRangeException();
			}
			return default(TSource);
		}

		public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
		{
			Check.Source(source);
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			IList<TSource> list = source as IList<TSource>;
			if (list != null)
			{
				return list[index];
			}
			return source.ElementAt(index, Enumerable.Fallback.Throw);
		}

		public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index)
		{
			Check.Source(source);
			if (index < 0)
			{
				return default(TSource);
			}
			IList<TSource> list = source as IList<TSource>;
			if (list != null)
			{
				return (index >= list.Count) ? default(TSource) : list[index];
			}
			return source.ElementAt(index, Enumerable.Fallback.Default);
		}

		public static IEnumerable<TResult> Empty<TResult>()
		{
			return new TResult[0];
		}

		public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			return first.Except(second, null);
		}

		public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			Check.FirstAndSecond(first, second);
			if (comparer == null)
			{
				comparer = EqualityComparer<TSource>.Default;
			}
			return Enumerable.CreateExceptIterator<TSource>(first, second, comparer);
		}

		private static IEnumerable<TSource> CreateExceptIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			HashSet<TSource> items = new HashSet<TSource>(second, comparer);
			foreach (TSource element in first)
			{
				if (items.Add(element))
				{
					yield return element;
				}
			}
			yield break;
		}

		private static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Enumerable.Fallback fallback)
		{
			foreach (TSource tsource in source)
			{
				if (predicate(tsource))
				{
					return tsource;
				}
			}
			if (fallback == Enumerable.Fallback.Throw)
			{
				throw Enumerable.NoMatchingElement();
			}
			return default(TSource);
		}

		public static TSource First<TSource>(this IEnumerable<TSource> source)
		{
			Check.Source(source);
			IList<TSource> list = source as IList<TSource>;
			if (list != null)
			{
				if (list.Count != 0)
				{
					return list[0];
				}
			}
			else
			{
				using (IEnumerator<TSource> enumerator = source.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						return enumerator.Current;
					}
				}
			}
			throw Enumerable.EmptySequence();
		}

		public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate(source, predicate);
			return source.First(predicate, Enumerable.Fallback.Throw);
		}

		public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
		{
			Check.Source(source);
			using (IEnumerator<TSource> enumerator = source.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					return enumerator.Current;
				}
			}
			return default(TSource);
		}

		public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate(source, predicate);
			return source.First(predicate, Enumerable.Fallback.Default);
		}

		public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			return source.GroupBy(keySelector, null);
		}

		public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
		{
			Check.SourceAndKeySelector(source, keySelector);
			return source.CreateGroupByIterator(keySelector, comparer);
		}

		private static IEnumerable<IGrouping<TKey, TSource>> CreateGroupByIterator<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
		{
			Dictionary<TKey, List<TSource>> groups = new Dictionary<TKey, List<TSource>>(comparer);
			List<TSource> nullList = new List<TSource>();
			int counter = 0;
			int nullCounter = -1;
			foreach (TSource tsource in source)
			{
				TKey tkey = keySelector(tsource);
				if (tkey == null)
				{
					nullList.Add(tsource);
					if (nullCounter == -1)
					{
						nullCounter = counter;
						counter++;
					}
				}
				else
				{
					List<TSource> list;
					if (!groups.TryGetValue(tkey, out list))
					{
						list = new List<TSource>();
						groups.Add(tkey, list);
						counter++;
					}
					list.Add(tsource);
				}
			}
			counter = 0;
			foreach (KeyValuePair<TKey, List<TSource>> group in groups)
			{
				if (counter == nullCounter)
				{
					yield return new Grouping<TKey, TSource>(default(TKey), nullList);
					counter++;
				}
				yield return new Grouping<TKey, TSource>(group.Key, group.Value);
				counter++;
			}
			if (counter == nullCounter)
			{
				yield return new Grouping<TKey, TSource>(default(TKey), nullList);
				counter++;
			}
			yield break;
		}

		public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
		{
			return source.GroupBy(keySelector, elementSelector, null);
		}

		public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
		{
			Check.SourceAndKeyElementSelectors(source, keySelector, elementSelector);
			return source.CreateGroupByIterator(keySelector, elementSelector, comparer);
		}

		private static IEnumerable<IGrouping<TKey, TElement>> CreateGroupByIterator<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
		{
			Dictionary<TKey, List<TElement>> groups = new Dictionary<TKey, List<TElement>>(comparer);
			List<TElement> nullList = new List<TElement>();
			int counter = 0;
			int nullCounter = -1;
			foreach (TSource arg in source)
			{
				TKey tkey = keySelector(arg);
				TElement item = elementSelector(arg);
				if (tkey == null)
				{
					nullList.Add(item);
					if (nullCounter == -1)
					{
						nullCounter = counter;
						counter++;
					}
				}
				else
				{
					List<TElement> list;
					if (!groups.TryGetValue(tkey, out list))
					{
						list = new List<TElement>();
						groups.Add(tkey, list);
						counter++;
					}
					list.Add(item);
				}
			}
			counter = 0;
			foreach (KeyValuePair<TKey, List<TElement>> group in groups)
			{
				if (counter == nullCounter)
				{
					yield return new Grouping<TKey, TElement>(default(TKey), nullList);
					counter++;
				}
				yield return new Grouping<TKey, TElement>(group.Key, group.Value);
				counter++;
			}
			if (counter == nullCounter)
			{
				yield return new Grouping<TKey, TElement>(default(TKey), nullList);
				counter++;
			}
			yield break;
		}

		public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
		{
			return source.GroupBy(keySelector, elementSelector, resultSelector, null);
		}

		public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
		{
			Check.GroupBySelectors(source, keySelector, elementSelector, resultSelector);
			return source.CreateGroupByIterator(keySelector, elementSelector, resultSelector, comparer);
		}

		private static IEnumerable<TResult> CreateGroupByIterator<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
		{
			IEnumerable<IGrouping<TKey, TElement>> groups = source.GroupBy(keySelector, elementSelector, comparer);
			foreach (IGrouping<TKey, TElement> group in groups)
			{
				yield return resultSelector(group.Key, group);
			}
			yield break;
		}

		public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
		{
			return source.GroupBy(keySelector, resultSelector, null);
		}

		public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
		{
			Check.SourceAndKeyResultSelectors(source, keySelector, resultSelector);
			return source.CreateGroupByIterator(keySelector, resultSelector, comparer);
		}

		private static IEnumerable<TResult> CreateGroupByIterator<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
		{
			IEnumerable<IGrouping<TKey, TSource>> groups = source.GroupBy(keySelector, comparer);
			foreach (IGrouping<TKey, TSource> group in groups)
			{
				yield return resultSelector(group.Key, group);
			}
			yield break;
		}

		public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
		{
			return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector, null);
		}

		public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
		{
			Check.JoinSelectors(outer, inner, outerKeySelector, innerKeySelector, resultSelector);
			if (comparer == null)
			{
				comparer = EqualityComparer<TKey>.Default;
			}
			return outer.CreateGroupJoinIterator(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
		}

		private static IEnumerable<TResult> CreateGroupJoinIterator<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
		{
			ILookup<TKey, TInner> innerKeys = inner.ToLookup(innerKeySelector, comparer);
			foreach (TOuter element in outer)
			{
				TKey outerKey = outerKeySelector(element);
				if (outerKey != null && innerKeys.Contains(outerKey))
				{
					yield return resultSelector(element, innerKeys[outerKey]);
				}
				else
				{
					yield return resultSelector(element, Enumerable.Empty<TInner>());
				}
			}
			yield break;
		}

		public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			return first.Intersect(second, null);
		}

		public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			Check.FirstAndSecond(first, second);
			if (comparer == null)
			{
				comparer = EqualityComparer<TSource>.Default;
			}
			return Enumerable.CreateIntersectIterator<TSource>(first, second, comparer);
		}

		private static IEnumerable<TSource> CreateIntersectIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			HashSet<TSource> items = new HashSet<TSource>(second, comparer);
			foreach (TSource element in first)
			{
				if (items.Remove(element))
				{
					yield return element;
				}
			}
			yield break;
		}

		public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
		{
			Check.JoinSelectors(outer, inner, outerKeySelector, innerKeySelector, resultSelector);
			if (comparer == null)
			{
				comparer = EqualityComparer<TKey>.Default;
			}
			return outer.CreateJoinIterator(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
		}

		private static IEnumerable<TResult> CreateJoinIterator<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
		{
			ILookup<TKey, TInner> innerKeys = inner.ToLookup(innerKeySelector, comparer);
			foreach (TOuter element in outer)
			{
				TKey outerKey = outerKeySelector(element);
				if (outerKey != null && innerKeys.Contains(outerKey))
				{
					foreach (TInner innerElement in innerKeys[outerKey])
					{
						yield return resultSelector(element, innerElement);
					}
				}
			}
			yield break;
		}

		public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
		{
			return outer.Join(inner, outerKeySelector, innerKeySelector, resultSelector, null);
		}

		private static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Enumerable.Fallback fallback)
		{
			bool flag = true;
			TSource result = default(TSource);
			foreach (TSource tsource in source)
			{
				if (predicate(tsource))
				{
					result = tsource;
					flag = false;
				}
			}
			if (!flag)
			{
				return result;
			}
			if (fallback == Enumerable.Fallback.Throw)
			{
				throw Enumerable.NoMatchingElement();
			}
			return result;
		}

		public static TSource Last<TSource>(this IEnumerable<TSource> source)
		{
			Check.Source(source);
			ICollection<TSource> collection = source as ICollection<TSource>;
			if (collection != null && collection.Count == 0)
			{
				throw Enumerable.EmptySequence();
			}
			IList<TSource> list = source as IList<TSource>;
			if (list != null)
			{
				return list[list.Count - 1];
			}
			bool flag = true;
			TSource result = default(TSource);
			foreach (TSource tsource in source)
			{
				result = tsource;
				flag = false;
			}
			if (!flag)
			{
				return result;
			}
			throw Enumerable.EmptySequence();
		}

		public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate(source, predicate);
			return source.Last(predicate, Enumerable.Fallback.Throw);
		}

		public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source)
		{
			Check.Source(source);
			IList<TSource> list = source as IList<TSource>;
			if (list != null)
			{
				return (list.Count <= 0) ? default(TSource) : list[list.Count - 1];
			}
			bool flag = true;
			TSource result = default(TSource);
			foreach (TSource tsource in source)
			{
				result = tsource;
				flag = false;
			}
			if (!flag)
			{
				return result;
			}
			return result;
		}

		public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate(source, predicate);
			return source.Last(predicate, Enumerable.Fallback.Default);
		}

		public static long LongCount<TSource>(this IEnumerable<TSource> source)
		{
			Check.Source(source);
			TSource[] array = source as TSource[];
			if (array != null)
			{
				return array.LongLength;
			}
			long num = 0L;
			using (IEnumerator<TSource> enumerator = source.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					num += 1L;
				}
			}
			return num;
		}

		public static long LongCount<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndSelector(source, predicate);
			long num = 0L;
			foreach (TSource arg in source)
			{
				if (predicate(arg))
				{
					num += 1L;
				}
			}
			return num;
		}

		public static int Max(this IEnumerable<int> source)
		{
			Check.Source(source);
			bool flag = true;
			int num = int.MinValue;
			foreach (int val in source)
			{
				num = Math.Max(val, num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.EmptySequence();
			}
			return num;
		}

		public static long Max(this IEnumerable<long> source)
		{
			Check.Source(source);
			bool flag = true;
			long num = long.MinValue;
			foreach (long val in source)
			{
				num = Math.Max(val, num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.EmptySequence();
			}
			return num;
		}

		public static double Max(this IEnumerable<double> source)
		{
			Check.Source(source);
			bool flag = true;
			double num = double.MinValue;
			foreach (double val in source)
			{
				num = Math.Max(val, num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.EmptySequence();
			}
			return num;
		}

		public static float Max(this IEnumerable<float> source)
		{
			Check.Source(source);
			bool flag = true;
			float num = float.MinValue;
			foreach (float val in source)
			{
				num = Math.Max(val, num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.EmptySequence();
			}
			return num;
		}

		public static decimal Max(this IEnumerable<decimal> source)
		{
			Check.Source(source);
			bool flag = true;
			decimal num = decimal.MinValue;
			foreach (decimal val in source)
			{
				num = Math.Max(val, num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.EmptySequence();
			}
			return num;
		}

		public static int? Max(this IEnumerable<int?> source)
		{
			Check.Source(source);
			bool flag = true;
			int num = int.MinValue;
			foreach (int? num2 in source)
			{
				if (num2 != null)
				{
					num = Math.Max(num2.Value, num);
					flag = false;
				}
			}
			if (flag)
			{
				return null;
			}
			return new int?(num);
		}

		public static long? Max(this IEnumerable<long?> source)
		{
			Check.Source(source);
			bool flag = true;
			long num = long.MinValue;
			foreach (long? num2 in source)
			{
				if (num2 != null)
				{
					num = Math.Max(num2.Value, num);
					flag = false;
				}
			}
			if (flag)
			{
				return null;
			}
			return new long?(num);
		}

		public static double? Max(this IEnumerable<double?> source)
		{
			Check.Source(source);
			bool flag = true;
			double num = double.MinValue;
			foreach (double? num2 in source)
			{
				if (num2 != null)
				{
					num = Math.Max(num2.Value, num);
					flag = false;
				}
			}
			if (flag)
			{
				return null;
			}
			return new double?(num);
		}

		public static float? Max(this IEnumerable<float?> source)
		{
			Check.Source(source);
			bool flag = true;
			float num = float.MinValue;
			foreach (float? num2 in source)
			{
				if (num2 != null)
				{
					num = Math.Max(num2.Value, num);
					flag = false;
				}
			}
			if (flag)
			{
				return null;
			}
			return new float?(num);
		}

		public static decimal? Max(this IEnumerable<decimal?> source)
		{
			Check.Source(source);
			bool flag = true;
			decimal num = decimal.MinValue;
			foreach (decimal? num2 in source)
			{
				if (num2 != null)
				{
					num = Math.Max(num2.Value, num);
					flag = false;
				}
			}
			if (flag)
			{
				return null;
			}
			return new decimal?(num);
		}

		public static TSource Max<TSource>(this IEnumerable<TSource> source)
		{
			Check.Source(source);
			Comparer<TSource> @default = Comparer<TSource>.Default;
			TSource tsource = default(TSource);
			if (default(TSource) == null)
			{
				foreach (TSource tsource2 in source)
				{
					if (tsource2 != null)
					{
						if (tsource == null || @default.Compare(tsource2, tsource) > 0)
						{
							tsource = tsource2;
						}
					}
				}
			}
			else
			{
				bool flag = true;
				foreach (TSource tsource3 in source)
				{
					if (flag)
					{
						tsource = tsource3;
						flag = false;
					}
					else if (@default.Compare(tsource3, tsource) > 0)
					{
						tsource = tsource3;
					}
				}
				if (flag)
				{
					throw Enumerable.EmptySequence();
				}
			}
			return tsource;
		}

		public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			int num = int.MinValue;
			foreach (TSource arg in source)
			{
				num = Math.Max(selector(arg), num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.NoMatchingElement();
			}
			return num;
		}

		public static long Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			long num = long.MinValue;
			foreach (TSource arg in source)
			{
				num = Math.Max(selector(arg), num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.NoMatchingElement();
			}
			return num;
		}

		public static double Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			double num = double.MinValue;
			foreach (TSource arg in source)
			{
				num = Math.Max(selector(arg), num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.NoMatchingElement();
			}
			return num;
		}

		public static float Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			float num = float.MinValue;
			foreach (TSource arg in source)
			{
				num = Math.Max(selector(arg), num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.NoMatchingElement();
			}
			return num;
		}

		public static decimal Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			decimal num = decimal.MinValue;
			foreach (TSource arg in source)
			{
				num = Math.Max(selector(arg), num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.NoMatchingElement();
			}
			return num;
		}

		private static U Iterate<T, U>(IEnumerable<T> source, U initValue, Func<T, U, U> selector)
		{
			bool flag = true;
			foreach (T arg in source)
			{
				initValue = selector(arg, initValue);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.NoMatchingElement();
			}
			return initValue;
		}

		public static int? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			int? num = null;
			foreach (TSource arg in source)
			{
				int? num2 = selector(arg);
				if (num == null)
				{
					num = num2;
				}
				else if (num2 > num)
				{
					num = num2;
				}
				flag = false;
			}
			if (flag)
			{
				return null;
			}
			return num;
		}

		public static long? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			long? num = null;
			foreach (TSource arg in source)
			{
				long? num2 = selector(arg);
				if (num == null)
				{
					num = num2;
				}
				else if (num2 > num)
				{
					num = num2;
				}
				flag = false;
			}
			if (flag)
			{
				return null;
			}
			return num;
		}

		public static double? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			double? num = null;
			foreach (TSource arg in source)
			{
				double? num2 = selector(arg);
				if (num == null)
				{
					num = num2;
				}
				else if (num2 > num)
				{
					num = num2;
				}
				flag = false;
			}
			if (flag)
			{
				return null;
			}
			return num;
		}

		public static float? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			float? num = null;
			foreach (TSource arg in source)
			{
				float? num2 = selector(arg);
				if (num == null)
				{
					num = num2;
				}
				else if (num2 > num)
				{
					num = num2;
				}
				flag = false;
			}
			if (flag)
			{
				return null;
			}
			return num;
		}

		public static decimal? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			decimal? num = null;
			foreach (TSource arg in source)
			{
				decimal? num2 = selector(arg);
				if (num == null)
				{
					num = num2;
				}
				else if (num2 > num)
				{
					num = num2;
				}
				flag = false;
			}
			if (flag)
			{
				return null;
			}
			return num;
		}

		public static TResult Max<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			Check.SourceAndSelector(source, selector);
			return source.Select(selector).Max<TResult>();
		}

		public static int Min(this IEnumerable<int> source)
		{
			Check.Source(source);
			bool flag = true;
			int num = int.MaxValue;
			foreach (int val in source)
			{
				num = Math.Min(val, num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.EmptySequence();
			}
			return num;
		}

		public static long Min(this IEnumerable<long> source)
		{
			Check.Source(source);
			bool flag = true;
			long num = long.MaxValue;
			foreach (long val in source)
			{
				num = Math.Min(val, num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.EmptySequence();
			}
			return num;
		}

		public static double Min(this IEnumerable<double> source)
		{
			Check.Source(source);
			bool flag = true;
			double num = double.MaxValue;
			foreach (double val in source)
			{
				num = Math.Min(val, num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.EmptySequence();
			}
			return num;
		}

		public static float Min(this IEnumerable<float> source)
		{
			Check.Source(source);
			bool flag = true;
			float num = float.MaxValue;
			foreach (float val in source)
			{
				num = Math.Min(val, num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.EmptySequence();
			}
			return num;
		}

		public static decimal Min(this IEnumerable<decimal> source)
		{
			Check.Source(source);
			bool flag = true;
			decimal num = decimal.MaxValue;
			foreach (decimal val in source)
			{
				num = Math.Min(val, num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.EmptySequence();
			}
			return num;
		}

		public static int? Min(this IEnumerable<int?> source)
		{
			Check.Source(source);
			bool flag = true;
			int num = int.MaxValue;
			foreach (int? num2 in source)
			{
				if (num2 != null)
				{
					num = Math.Min(num2.Value, num);
					flag = false;
				}
			}
			if (flag)
			{
				return null;
			}
			return new int?(num);
		}

		public static long? Min(this IEnumerable<long?> source)
		{
			Check.Source(source);
			bool flag = true;
			long num = long.MaxValue;
			foreach (long? num2 in source)
			{
				if (num2 != null)
				{
					num = Math.Min(num2.Value, num);
					flag = false;
				}
			}
			if (flag)
			{
				return null;
			}
			return new long?(num);
		}

		public static double? Min(this IEnumerable<double?> source)
		{
			Check.Source(source);
			bool flag = true;
			double num = double.MaxValue;
			foreach (double? num2 in source)
			{
				if (num2 != null)
				{
					num = Math.Min(num2.Value, num);
					flag = false;
				}
			}
			if (flag)
			{
				return null;
			}
			return new double?(num);
		}

		public static float? Min(this IEnumerable<float?> source)
		{
			Check.Source(source);
			bool flag = true;
			float num = float.MaxValue;
			foreach (float? num2 in source)
			{
				if (num2 != null)
				{
					num = Math.Min(num2.Value, num);
					flag = false;
				}
			}
			if (flag)
			{
				return null;
			}
			return new float?(num);
		}

		public static decimal? Min(this IEnumerable<decimal?> source)
		{
			Check.Source(source);
			bool flag = true;
			decimal num = decimal.MaxValue;
			foreach (decimal? num2 in source)
			{
				if (num2 != null)
				{
					num = Math.Min(num2.Value, num);
					flag = false;
				}
			}
			if (flag)
			{
				return null;
			}
			return new decimal?(num);
		}

		public static TSource Min<TSource>(this IEnumerable<TSource> source)
		{
			Check.Source(source);
			Comparer<TSource> @default = Comparer<TSource>.Default;
			TSource tsource = default(TSource);
			if (default(TSource) == null)
			{
				foreach (TSource tsource2 in source)
				{
					if (tsource2 != null)
					{
						if (tsource == null || @default.Compare(tsource2, tsource) < 0)
						{
							tsource = tsource2;
						}
					}
				}
			}
			else
			{
				bool flag = true;
				foreach (TSource tsource3 in source)
				{
					if (flag)
					{
						tsource = tsource3;
						flag = false;
					}
					else if (@default.Compare(tsource3, tsource) < 0)
					{
						tsource = tsource3;
					}
				}
				if (flag)
				{
					throw Enumerable.EmptySequence();
				}
			}
			return tsource;
		}

		public static int Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			int num = int.MaxValue;
			foreach (TSource arg in source)
			{
				num = Math.Min(selector(arg), num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.NoMatchingElement();
			}
			return num;
		}

		public static long Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			long num = long.MaxValue;
			foreach (TSource arg in source)
			{
				num = Math.Min(selector(arg), num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.NoMatchingElement();
			}
			return num;
		}

		public static double Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			double num = double.MaxValue;
			foreach (TSource arg in source)
			{
				num = Math.Min(selector(arg), num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.NoMatchingElement();
			}
			return num;
		}

		public static float Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			float num = float.MaxValue;
			foreach (TSource arg in source)
			{
				num = Math.Min(selector(arg), num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.NoMatchingElement();
			}
			return num;
		}

		public static decimal Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			decimal num = decimal.MaxValue;
			foreach (TSource arg in source)
			{
				num = Math.Min(selector(arg), num);
				flag = false;
			}
			if (flag)
			{
				throw Enumerable.NoMatchingElement();
			}
			return num;
		}

		public static int? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			int? num = null;
			foreach (TSource arg in source)
			{
				int? num2 = selector(arg);
				if (num == null)
				{
					num = num2;
				}
				else if (num2 < num)
				{
					num = num2;
				}
				flag = false;
			}
			if (flag)
			{
				return null;
			}
			return num;
		}

		public static long? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			long? num = null;
			foreach (TSource arg in source)
			{
				long? num2 = selector(arg);
				if (num == null)
				{
					num = num2;
				}
				else if (num2 < num)
				{
					num = num2;
				}
				flag = false;
			}
			if (flag)
			{
				return null;
			}
			return num;
		}

		public static float? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			float? num = null;
			foreach (TSource arg in source)
			{
				float? num2 = selector(arg);
				if (num == null)
				{
					num = num2;
				}
				else if (num2 < num)
				{
					num = num2;
				}
				flag = false;
			}
			if (flag)
			{
				return null;
			}
			return num;
		}

		public static double? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			double? num = null;
			foreach (TSource arg in source)
			{
				double? num2 = selector(arg);
				if (num == null)
				{
					num = num2;
				}
				else if (num2 < num)
				{
					num = num2;
				}
				flag = false;
			}
			if (flag)
			{
				return null;
			}
			return num;
		}

		public static decimal? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
		{
			Check.SourceAndSelector(source, selector);
			bool flag = true;
			decimal? num = null;
			foreach (TSource arg in source)
			{
				decimal? num2 = selector(arg);
				if (num == null)
				{
					num = num2;
				}
				else if (num2 < num)
				{
					num = num2;
				}
				flag = false;
			}
			if (flag)
			{
				return null;
			}
			return num;
		}

		public static TResult Min<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			Check.SourceAndSelector(source, selector);
			return source.Select(selector).Min<TResult>();
		}

		public static IEnumerable<TResult> OfType<TResult>(this IEnumerable source)
		{
			Check.Source(source);
			return Enumerable.CreateOfTypeIterator<TResult>(source);
		}

		private static IEnumerable<TResult> CreateOfTypeIterator<TResult>(IEnumerable source)
		{
			IEnumerator enumerator = source.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object element = enumerator.Current;
					if (element is TResult)
					{
						yield return (TResult)((object)element);
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			yield break;
		}

		public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			return source.OrderBy(keySelector, null);
		}

		public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
		{
			Check.SourceAndKeySelector(source, keySelector);
			return new OrderedSequence<TSource, TKey>(source, keySelector, comparer, SortDirection.Ascending);
		}

		public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			return source.OrderByDescending(keySelector, null);
		}

		public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
		{
			Check.SourceAndKeySelector(source, keySelector);
			return new OrderedSequence<TSource, TKey>(source, keySelector, comparer, SortDirection.Descending);
		}

		public static IEnumerable<int> Range(int start, int count)
		{
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			if ((long)start + (long)count - 1L > 2147483647L)
			{
				throw new ArgumentOutOfRangeException();
			}
			return Enumerable.CreateRangeIterator(start, count);
		}

		private static IEnumerable<int> CreateRangeIterator(int start, int count)
		{
			for (int i = 0; i < count; i++)
			{
				yield return start + i;
			}
			yield break;
		}

		public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count)
		{
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			return Enumerable.CreateRepeatIterator<TResult>(element, count);
		}

		private static IEnumerable<TResult> CreateRepeatIterator<TResult>(TResult element, int count)
		{
			for (int i = 0; i < count; i++)
			{
				yield return element;
			}
			yield break;
		}

		public static IEnumerable<TSource> Reverse<TSource>(this IEnumerable<TSource> source)
		{
			Check.Source(source);
			return Enumerable.CreateReverseIterator<TSource>(source);
		}

		private static IEnumerable<TSource> CreateReverseIterator<TSource>(IEnumerable<TSource> source)
		{
			TSource[] array = source.ToArray<TSource>();
			for (int i = array.Length - 1; i >= 0; i--)
			{
				yield return array[i];
			}
			yield break;
		}

		public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			Check.SourceAndSelector(source, selector);
			return Enumerable.CreateSelectIterator<TSource, TResult>(source, selector);
		}

		private static IEnumerable<TResult> CreateSelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			foreach (TSource element in source)
			{
				yield return selector(element);
			}
			yield break;
		}

		public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
		{
			Check.SourceAndSelector(source, selector);
			return Enumerable.CreateSelectIterator<TSource, TResult>(source, selector);
		}

		private static IEnumerable<TResult> CreateSelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
		{
			int counter = 0;
			foreach (TSource element in source)
			{
				yield return selector(element, counter);
				counter++;
			}
			yield break;
		}

		public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
		{
			Check.SourceAndSelector(source, selector);
			return Enumerable.CreateSelectManyIterator<TSource, TResult>(source, selector);
		}

		private static IEnumerable<TResult> CreateSelectManyIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
		{
			foreach (TSource element in source)
			{
				foreach (TResult item in selector(element))
				{
					yield return item;
				}
			}
			yield break;
		}

		public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
		{
			Check.SourceAndSelector(source, selector);
			return Enumerable.CreateSelectManyIterator<TSource, TResult>(source, selector);
		}

		private static IEnumerable<TResult> CreateSelectManyIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
		{
			int counter = 0;
			foreach (TSource element in source)
			{
				foreach (TResult item in selector(element, counter))
				{
					yield return item;
				}
				counter++;
			}
			yield break;
		}

		public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
		{
			Check.SourceAndCollectionSelectors(source, collectionSelector, resultSelector);
			return Enumerable.CreateSelectManyIterator<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
		}

		private static IEnumerable<TResult> CreateSelectManyIterator<TSource, TCollection, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> selector)
		{
			foreach (TSource element in source)
			{
				foreach (TCollection collection in collectionSelector(element))
				{
					yield return selector(element, collection);
				}
			}
			yield break;
		}

		public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
		{
			Check.SourceAndCollectionSelectors(source, collectionSelector, resultSelector);
			return Enumerable.CreateSelectManyIterator<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
		}

		private static IEnumerable<TResult> CreateSelectManyIterator<TSource, TCollection, TResult>(IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> selector)
		{
			int counter = 0;
			foreach (TSource element in source)
			{
				TSource arg = element;
				int arg2;
				counter = (arg2 = counter) + 1;
				foreach (TCollection collection in collectionSelector(arg, arg2))
				{
					yield return selector(element, collection);
				}
			}
			yield break;
		}

		private static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Enumerable.Fallback fallback)
		{
			bool flag = false;
			TSource result = default(TSource);
			foreach (TSource tsource in source)
			{
				if (predicate(tsource))
				{
					if (flag)
					{
						throw Enumerable.MoreThanOneMatchingElement();
					}
					flag = true;
					result = tsource;
				}
			}
			if (!flag && fallback == Enumerable.Fallback.Throw)
			{
				throw Enumerable.NoMatchingElement();
			}
			return result;
		}

		public static TSource Single<TSource>(this IEnumerable<TSource> source)
		{
			Check.Source(source);
			bool flag = false;
			TSource result = default(TSource);
			foreach (TSource tsource in source)
			{
				if (flag)
				{
					throw Enumerable.MoreThanOneElement();
				}
				flag = true;
				result = tsource;
			}
			if (!flag)
			{
				throw Enumerable.NoMatchingElement();
			}
			return result;
		}

		public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate(source, predicate);
			return source.Single(predicate, Enumerable.Fallback.Throw);
		}

		public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source)
		{
			Check.Source(source);
			bool flag = false;
			TSource result = default(TSource);
			foreach (TSource tsource in source)
			{
				if (flag)
				{
					throw Enumerable.MoreThanOneMatchingElement();
				}
				flag = true;
				result = tsource;
			}
			return result;
		}

		public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate(source, predicate);
			return source.Single(predicate, Enumerable.Fallback.Default);
		}

		public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
		{
			Check.Source(source);
			return Enumerable.CreateSkipIterator<TSource>(source, count);
		}

		private static IEnumerable<TSource> CreateSkipIterator<TSource>(IEnumerable<TSource> source, int count)
		{
			IEnumerator<TSource> enumerator = source.GetEnumerator();
			try
			{
				do
				{
					int num;
					count = (num = count) - 1;
					if (num <= 0)
					{
						goto Block_4;
					}
				}
				while (enumerator.MoveNext());
				yield break;
				Block_4:
				while (enumerator.MoveNext())
				{
					TSource tsource = enumerator.Current;
					yield return tsource;
				}
			}
			finally
			{
				enumerator.Dispose();
			}
			yield break;
		}

		public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate(source, predicate);
			return Enumerable.CreateSkipWhileIterator<TSource>(source, predicate);
		}

		private static IEnumerable<TSource> CreateSkipWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			bool yield = false;
			foreach (TSource element in source)
			{
				if (yield)
				{
					yield return element;
				}
				else if (!predicate(element))
				{
					yield return element;
					yield = true;
				}
			}
			yield break;
		}

		public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
			Check.SourceAndPredicate(source, predicate);
			return Enumerable.CreateSkipWhileIterator<TSource>(source, predicate);
		}

		private static IEnumerable<TSource> CreateSkipWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
			int counter = 0;
			bool yield = false;
			foreach (TSource element in source)
			{
				if (yield)
				{
					yield return element;
				}
				else if (!predicate(element, counter))
				{
					yield return element;
					yield = true;
				}
				counter++;
			}
			yield break;
		}

		public static int Sum(this IEnumerable<int> source)
		{
			Check.Source(source);
			int num = 0;
			checked
			{
				foreach (int num2 in source)
				{
					num += num2;
				}
				return num;
			}
		}

		public static int? Sum(this IEnumerable<int?> source)
		{
			Check.Source(source);
			int num = 0;
			checked
			{
				foreach (int? num2 in source)
				{
					if (num2 != null)
					{
						num += num2.Value;
					}
				}
				return new int?(num);
			}
		}

		public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
		{
			Check.SourceAndSelector(source, selector);
			int num = 0;
			checked
			{
				foreach (TSource arg in source)
				{
					num += selector(arg);
				}
				return num;
			}
		}

		public static int? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
		{
			Check.SourceAndSelector(source, selector);
			int num = 0;
			checked
			{
				foreach (TSource arg in source)
				{
					int? num2 = selector(arg);
					if (num2 != null)
					{
						num += num2.Value;
					}
				}
				return new int?(num);
			}
		}

		public static long Sum(this IEnumerable<long> source)
		{
			Check.Source(source);
			long num = 0L;
			checked
			{
				foreach (long num2 in source)
				{
					num += num2;
				}
				return num;
			}
		}

		public static long? Sum(this IEnumerable<long?> source)
		{
			Check.Source(source);
			long num = 0L;
			checked
			{
				foreach (long? num2 in source)
				{
					if (num2 != null)
					{
						num += num2.Value;
					}
				}
				return new long?(num);
			}
		}

		public static long Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
		{
			Check.SourceAndSelector(source, selector);
			long num = 0L;
			checked
			{
				foreach (TSource arg in source)
				{
					num += selector(arg);
				}
				return num;
			}
		}

		public static long? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
		{
			Check.SourceAndSelector(source, selector);
			long num = 0L;
			checked
			{
				foreach (TSource arg in source)
				{
					long? num2 = selector(arg);
					if (num2 != null)
					{
						num += num2.Value;
					}
				}
				return new long?(num);
			}
		}

		public static double Sum(this IEnumerable<double> source)
		{
			Check.Source(source);
			double num = 0.0;
			foreach (double num2 in source)
			{
				num += num2;
			}
			return num;
		}

		public static double? Sum(this IEnumerable<double?> source)
		{
			Check.Source(source);
			double num = 0.0;
			foreach (double? num2 in source)
			{
				if (num2 != null)
				{
					num += num2.Value;
				}
			}
			return new double?(num);
		}

		public static double Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
		{
			Check.SourceAndSelector(source, selector);
			double num = 0.0;
			foreach (TSource arg in source)
			{
				num += selector(arg);
			}
			return num;
		}

		public static double? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
		{
			Check.SourceAndSelector(source, selector);
			double num = 0.0;
			foreach (TSource arg in source)
			{
				double? num2 = selector(arg);
				if (num2 != null)
				{
					num += num2.Value;
				}
			}
			return new double?(num);
		}

		public static float Sum(this IEnumerable<float> source)
		{
			Check.Source(source);
			float num = 0f;
			foreach (float num2 in source)
			{
				num += num2;
			}
			return num;
		}

		public static float? Sum(this IEnumerable<float?> source)
		{
			Check.Source(source);
			float num = 0f;
			foreach (float? num2 in source)
			{
				if (num2 != null)
				{
					num += num2.Value;
				}
			}
			return new float?(num);
		}

		public static float Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
		{
			Check.SourceAndSelector(source, selector);
			float num = 0f;
			foreach (TSource arg in source)
			{
				num += selector(arg);
			}
			return num;
		}

		public static float? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
		{
			Check.SourceAndSelector(source, selector);
			float num = 0f;
			foreach (TSource arg in source)
			{
				float? num2 = selector(arg);
				if (num2 != null)
				{
					num += num2.Value;
				}
			}
			return new float?(num);
		}

		public static decimal Sum(this IEnumerable<decimal> source)
		{
			Check.Source(source);
			decimal num = 0m;
			foreach (decimal d in source)
			{
				num += d;
			}
			return num;
		}

		public static decimal? Sum(this IEnumerable<decimal?> source)
		{
			Check.Source(source);
			decimal num = 0m;
			foreach (decimal? num2 in source)
			{
				if (num2 != null)
				{
					num += num2.Value;
				}
			}
			return new decimal?(num);
		}

		public static decimal Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
		{
			Check.SourceAndSelector(source, selector);
			decimal num = 0m;
			foreach (TSource arg in source)
			{
				num += selector(arg);
			}
			return num;
		}

		public static decimal? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
		{
			Check.SourceAndSelector(source, selector);
			decimal num = 0m;
			foreach (TSource arg in source)
			{
				decimal? num2 = selector(arg);
				if (num2 != null)
				{
					num += num2.Value;
				}
			}
			return new decimal?(num);
		}

		public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
		{
			Check.Source(source);
			return Enumerable.CreateTakeIterator<TSource>(source, count);
		}

		private static IEnumerable<TSource> CreateTakeIterator<TSource>(IEnumerable<TSource> source, int count)
		{
			if (count <= 0)
			{
				yield break;
			}
			int counter = 0;
			foreach (TSource element in source)
			{
				yield return element;
				if (++counter == count)
				{
					yield break;
				}
			}
			yield break;
		}

		public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate(source, predicate);
			return Enumerable.CreateTakeWhileIterator<TSource>(source, predicate);
		}

		private static IEnumerable<TSource> CreateTakeWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			foreach (TSource element in source)
			{
				if (!predicate(element))
				{
					yield break;
				}
				yield return element;
			}
			yield break;
		}

		public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
			Check.SourceAndPredicate(source, predicate);
			return Enumerable.CreateTakeWhileIterator<TSource>(source, predicate);
		}

		private static IEnumerable<TSource> CreateTakeWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
			int counter = 0;
			foreach (TSource element in source)
			{
				if (!predicate(element, counter))
				{
					yield break;
				}
				yield return element;
				counter++;
			}
			yield break;
		}

		public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			return source.ThenBy(keySelector, null);
		}

		public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
		{
			Check.SourceAndKeySelector(source, keySelector);
			OrderedEnumerable<TSource> orderedEnumerable = source as OrderedEnumerable<TSource>;
			return orderedEnumerable.CreateOrderedEnumerable<TKey>(keySelector, comparer, false);
		}

		public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			return source.ThenByDescending(keySelector, null);
		}

		public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
		{
			Check.SourceAndKeySelector(source, keySelector);
			OrderedEnumerable<TSource> orderedEnumerable = source as OrderedEnumerable<TSource>;
			return orderedEnumerable.CreateOrderedEnumerable<TKey>(keySelector, comparer, true);
		}

		public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
		{
			Check.Source(source);
			ICollection<TSource> collection = source as ICollection<TSource>;
			TSource[] array;
			if (collection == null)
			{
				int num = 0;
				array = new TSource[0];
				foreach (TSource tsource in source)
				{
					if (num == array.Length)
					{
						if (num == 0)
						{
							array = new TSource[4];
						}
						else
						{
							Array.Resize<TSource>(ref array, num * 2);
						}
					}
					array[num++] = tsource;
				}
				if (num != array.Length)
				{
					Array.Resize<TSource>(ref array, num);
				}
				return array;
			}
			if (collection.Count == 0)
			{
				return new TSource[0];
			}
			array = new TSource[collection.Count];
			collection.CopyTo(array, 0);
			return array;
		}

		public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
		{
			return source.ToDictionary(keySelector, elementSelector, null);
		}

		public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
		{
			Check.SourceAndKeyElementSelectors(source, keySelector, elementSelector);
			if (comparer == null)
			{
				comparer = EqualityComparer<TKey>.Default;
			}
			Dictionary<TKey, TElement> dictionary = new Dictionary<TKey, TElement>(comparer);
			foreach (TSource arg in source)
			{
				dictionary.Add(keySelector(arg), elementSelector(arg));
			}
			return dictionary;
		}

		public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			return source.ToDictionary(keySelector, null);
		}

		public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
		{
			Check.SourceAndKeySelector(source, keySelector);
			if (comparer == null)
			{
				comparer = EqualityComparer<TKey>.Default;
			}
			Dictionary<TKey, TSource> dictionary = new Dictionary<TKey, TSource>(comparer);
			foreach (TSource tsource in source)
			{
				dictionary.Add(keySelector(tsource), tsource);
			}
			return dictionary;
		}

		public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
		{
			Check.Source(source);
			return new List<TSource>(source);
		}

		public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			return source.ToLookup(keySelector, null);
		}

		public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
		{
			Check.SourceAndKeySelector(source, keySelector);
			List<TSource> list = null;
			Dictionary<TKey, List<TSource>> dictionary = new Dictionary<TKey, List<TSource>>(comparer ?? EqualityComparer<TKey>.Default);
			foreach (TSource tsource in source)
			{
				TKey tkey = keySelector(tsource);
				List<TSource> list2;
				if (tkey == null)
				{
					if (list == null)
					{
						list = new List<TSource>();
					}
					list2 = list;
				}
				else if (!dictionary.TryGetValue(tkey, out list2))
				{
					list2 = new List<TSource>();
					dictionary.Add(tkey, list2);
				}
				list2.Add(tsource);
			}
			return new Lookup<TKey, TSource>(dictionary, list);
		}

		public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
		{
			return source.ToLookup(keySelector, elementSelector, null);
		}

		public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
		{
			Check.SourceAndKeyElementSelectors(source, keySelector, elementSelector);
			List<TElement> list = null;
			Dictionary<TKey, List<TElement>> dictionary = new Dictionary<TKey, List<TElement>>(comparer ?? EqualityComparer<TKey>.Default);
			foreach (TSource arg in source)
			{
				TKey tkey = keySelector(arg);
				List<TElement> list2;
				if (tkey == null)
				{
					if (list == null)
					{
						list = new List<TElement>();
					}
					list2 = list;
				}
				else if (!dictionary.TryGetValue(tkey, out list2))
				{
					list2 = new List<TElement>();
					dictionary.Add(tkey, list2);
				}
				list2.Add(elementSelector(arg));
			}
			return new Lookup<TKey, TElement>(dictionary, list);
		}

		public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			return first.SequenceEqual(second, null);
		}

		public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			Check.FirstAndSecond(first, second);
			if (comparer == null)
			{
				comparer = EqualityComparer<TSource>.Default;
			}
			bool result;
			using (IEnumerator<TSource> enumerator = first.GetEnumerator())
			{
				using (IEnumerator<TSource> enumerator2 = second.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (!enumerator2.MoveNext())
						{
							return false;
						}
						if (!comparer.Equals(enumerator.Current, enumerator2.Current))
						{
							return false;
						}
					}
					result = !enumerator2.MoveNext();
				}
			}
			return result;
		}

		public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			Check.FirstAndSecond(first, second);
			return first.Union(second, null);
		}

		public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			Check.FirstAndSecond(first, second);
			if (comparer == null)
			{
				comparer = EqualityComparer<TSource>.Default;
			}
			return Enumerable.CreateUnionIterator<TSource>(first, second, comparer);
		}

		private static IEnumerable<TSource> CreateUnionIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			HashSet<TSource> items = new HashSet<TSource>(comparer);
			foreach (TSource element in first)
			{
				if (!items.Contains(element))
				{
					items.Add(element);
					yield return element;
				}
			}
			foreach (TSource element2 in second)
			{
				if (!items.Contains(element2))
				{
					items.Add(element2);
					yield return element2;
				}
			}
			yield break;
		}

		public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate(source, predicate);
			TSource[] array = source as TSource[];
			if (array != null)
			{
				return Enumerable.CreateWhereIterator<TSource>(array, predicate);
			}
			return Enumerable.CreateWhereIterator<TSource>(source, predicate);
		}

		private static IEnumerable<TSource> CreateWhereIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			foreach (TSource element in source)
			{
				if (predicate(element))
				{
					yield return element;
				}
			}
			yield break;
		}

		private static IEnumerable<TSource> CreateWhereIterator<TSource>(TSource[] source, Func<TSource, bool> predicate)
		{
			foreach (TSource element in source)
			{
				if (predicate(element))
				{
					yield return element;
				}
			}
			yield break;
		}

		public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
			Check.SourceAndPredicate(source, predicate);
			TSource[] array = source as TSource[];
			if (array != null)
			{
				return Enumerable.CreateWhereIterator<TSource>(array, predicate);
			}
			return Enumerable.CreateWhereIterator<TSource>(source, predicate);
		}

		private static IEnumerable<TSource> CreateWhereIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
			int counter = 0;
			foreach (TSource element in source)
			{
				if (predicate(element, counter))
				{
					yield return element;
				}
				counter++;
			}
			yield break;
		}

		private static IEnumerable<TSource> CreateWhereIterator<TSource>(TSource[] source, Func<TSource, int, bool> predicate)
		{
			for (int i = 0; i < source.Length; i++)
			{
				TSource element = source[i];
				if (predicate(element, i))
				{
					yield return element;
				}
			}
			yield break;
		}

		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (T obj in source)
			{
				action(obj);
			}
		}

		public static IEnumerable<TSource> Do<TSource>(this IEnumerable<TSource> source, Action<TSource> predicate)
		{
			Check.SourceAndPredicate(source, predicate);
			return Enumerable.DoIterator<TSource>(source, predicate);
		}

		private static IEnumerable<TSource> DoIterator<TSource>(IEnumerable<TSource> source, Action<TSource> predicate)
		{
			foreach (TSource element in source)
			{
				predicate(element);
				yield return element;
			}
			yield break;
		}

		private static Exception EmptySequence()
		{
			return new InvalidOperationException("Sequence contains no elements");
		}

		private static Exception NoMatchingElement()
		{
			return new InvalidOperationException("Sequence contains no matching element");
		}

		private static Exception MoreThanOneElement()
		{
			return new InvalidOperationException("Sequence contains more than one element");
		}

		private static Exception MoreThanOneMatchingElement()
		{
			return new InvalidOperationException("Sequence contains more than one matching element");
		}

		private enum Fallback
		{
			Default,
			Throw
		}
	}
}
