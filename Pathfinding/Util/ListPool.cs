using System;
using System.Collections.Generic;

namespace Pathfinding.Util
{
	
	public static class ListPool<T>
	{
		
		public static List<T> Claim()
		{
			List<List<T>> obj = ListPool<T>.pool;
			List<T> result;
			lock (obj)
			{
				if (ListPool<T>.pool.Count > 0)
				{
					List<T> list = ListPool<T>.pool[ListPool<T>.pool.Count - 1];
					ListPool<T>.pool.RemoveAt(ListPool<T>.pool.Count - 1);
					ListPool<T>.inPool.Remove(list);
					result = list;
				}
				else
				{
					result = new List<T>();
				}
			}
			return result;
		}

		
		public static List<T> Claim(int capacity)
		{
			List<List<T>> obj = ListPool<T>.pool;
			List<T> result;
			lock (obj)
			{
				List<T> list = null;
				int index = -1;
				int num = 0;
				while (num < ListPool<T>.pool.Count && num < 8)
				{
					List<T> list2 = ListPool<T>.pool[ListPool<T>.pool.Count - 1 - num];
					if (list2.Capacity >= capacity)
					{
						ListPool<T>.pool.RemoveAt(ListPool<T>.pool.Count - 1 - num);
						ListPool<T>.inPool.Remove(list2);
						return list2;
					}
					if (list == null || list2.Capacity > list.Capacity)
					{
						list = list2;
						index = ListPool<T>.pool.Count - 1 - num;
					}
					num++;
				}
				if (list == null)
				{
					list = new List<T>(capacity);
				}
				else
				{
					list.Capacity = capacity;
					ListPool<T>.pool[index] = ListPool<T>.pool[ListPool<T>.pool.Count - 1];
					ListPool<T>.pool.RemoveAt(ListPool<T>.pool.Count - 1);
					ListPool<T>.inPool.Remove(list);
				}
				result = list;
			}
			return result;
		}

		
		public static void Warmup(int count, int size)
		{
			List<List<T>> obj = ListPool<T>.pool;
			lock (obj)
			{
				List<T>[] array = new List<T>[count];
				for (int i = 0; i < count; i++)
				{
					array[i] = ListPool<T>.Claim(size);
				}
				for (int j = 0; j < count; j++)
				{
					ListPool<T>.Release(array[j]);
				}
			}
		}

		
		public static void Release(List<T> list)
		{
			list.Clear();
			List<List<T>> obj = ListPool<T>.pool;
			lock (obj)
			{
				ListPool<T>.pool.Add(list);
			}
		}

		
		public static void Clear()
		{
			List<List<T>> obj = ListPool<T>.pool;
			lock (obj)
			{
				ListPool<T>.pool.Clear();
			}
		}

		
		public static int GetSize()
		{
			return ListPool<T>.pool.Count;
		}

		
		private const int MaxCapacitySearchLength = 8;

		
		private static readonly List<List<T>> pool = new List<List<T>>();

		
		private static readonly HashSet<List<T>> inPool = new HashSet<List<T>>();
	}
}
