using System;
using System.Collections.Generic;

namespace Ceto.Common.Containers.Queues
{
	public class DictionaryQueue<KEY, VALUE>
	{
		public DictionaryQueue()
		{
			this.m_dictionary = new Dictionary<KEY, LinkedListNode<KeyValuePair<KEY, VALUE>>>();
			this.m_list = new LinkedList<KeyValuePair<KEY, VALUE>>();
		}

		public DictionaryQueue(IEqualityComparer<KEY> comparer)
		{
			this.m_dictionary = new Dictionary<KEY, LinkedListNode<KeyValuePair<KEY, VALUE>>>(comparer);
			this.m_list = new LinkedList<KeyValuePair<KEY, VALUE>>();
		}

		public VALUE this[KEY key]
		{
			get
			{
				return this.m_dictionary[key].Value.Value;
			}
			set
			{
				this.Replace(key, value);
			}
		}

		public IEnumerator<VALUE> GetEnumerator()
		{
			foreach (KeyValuePair<KEY, VALUE> keyValuePair in this.m_list)
			{
				yield return keyValuePair.Value;
			}
			yield break;
		}

		public bool ContainsKey(KEY key)
		{
			return this.m_dictionary.ContainsKey(key);
		}

		public void Replace(KEY key, VALUE val)
		{
			LinkedListNode<KeyValuePair<KEY, VALUE>> linkedListNode = this.m_dictionary[key];
			linkedListNode.Value = new KeyValuePair<KEY, VALUE>(key, val);
		}

		public void AddFirst(KEY key, VALUE val)
		{
			this.m_dictionary.Add(key, this.m_list.AddFirst(new KeyValuePair<KEY, VALUE>(key, val)));
		}

		public void AddLast(KEY key, VALUE val)
		{
			this.m_dictionary.Add(key, this.m_list.AddLast(new KeyValuePair<KEY, VALUE>(key, val)));
		}

		public int Count
		{
			get
			{
				return this.m_dictionary.Count;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return this.m_dictionary.Count == 0;
			}
		}

		public KeyValuePair<KEY, VALUE> First()
		{
			return this.m_list.First.Value;
		}

		public KeyValuePair<KEY, VALUE> Last()
		{
			return this.m_list.Last.Value;
		}

		public VALUE RemoveFirst()
		{
			LinkedListNode<KeyValuePair<KEY, VALUE>> first = this.m_list.First;
			this.m_list.RemoveFirst();
			this.m_dictionary.Remove(first.Value.Key);
			return first.Value.Value;
		}

		public VALUE RemoveLast()
		{
			LinkedListNode<KeyValuePair<KEY, VALUE>> last = this.m_list.Last;
			this.m_list.RemoveLast();
			this.m_dictionary.Remove(last.Value.Key);
			return last.Value.Value;
		}

		public VALUE Remove(KEY key)
		{
			LinkedListNode<KeyValuePair<KEY, VALUE>> linkedListNode = this.m_dictionary[key];
			this.m_dictionary.Remove(key);
			this.m_list.Remove(linkedListNode);
			return linkedListNode.Value.Value;
		}

		public void Clear()
		{
			this.m_dictionary.Clear();
			this.m_list.Clear();
		}

		private Dictionary<KEY, LinkedListNode<KeyValuePair<KEY, VALUE>>> m_dictionary;

		private LinkedList<KeyValuePair<KEY, VALUE>> m_list;
	}
}
