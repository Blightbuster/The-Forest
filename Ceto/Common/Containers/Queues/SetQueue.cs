using System;
using System.Collections.Generic;

namespace Ceto.Common.Containers.Queues
{
	
	public class SetQueue<VALUE>
	{
		
		public SetQueue()
		{
			this.m_dictionary = new Dictionary<VALUE, LinkedListNode<VALUE>>();
			this.m_list = new LinkedList<VALUE>();
		}

		
		public SetQueue(IEqualityComparer<VALUE> comparer)
		{
			this.m_dictionary = new Dictionary<VALUE, LinkedListNode<VALUE>>(comparer);
			this.m_list = new LinkedList<VALUE>();
		}

		
		public VALUE this[VALUE key]
		{
			get
			{
				return this.m_dictionary[key].Value;
			}
			set
			{
				this.Replace(key, value);
			}
		}

		
		public IEnumerator<VALUE> GetEnumerator()
		{
			foreach (VALUE value in this.m_list)
			{
				yield return value;
			}
			yield break;
		}

		
		public bool Contains(VALUE val)
		{
			return this.m_dictionary.ContainsKey(val);
		}

		
		public void Replace(VALUE key, VALUE val)
		{
			LinkedListNode<VALUE> linkedListNode = this.m_dictionary[key];
			linkedListNode.Value = val;
			this.m_dictionary.Remove(key);
			this.m_dictionary.Add(val, linkedListNode);
		}

		
		public void AddFirst(VALUE val)
		{
			this.m_dictionary.Add(val, this.m_list.AddFirst(val));
		}

		
		public void AddLast(VALUE val)
		{
			this.m_dictionary.Add(val, this.m_list.AddLast(val));
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

		
		public VALUE First()
		{
			return this.m_list.First.Value;
		}

		
		public VALUE Last()
		{
			return this.m_list.Last.Value;
		}

		
		public VALUE RemoveFirst()
		{
			LinkedListNode<VALUE> first = this.m_list.First;
			this.m_list.RemoveFirst();
			this.m_dictionary.Remove(first.Value);
			return first.Value;
		}

		
		public VALUE RemoveLast()
		{
			LinkedListNode<VALUE> last = this.m_list.Last;
			this.m_list.RemoveLast();
			this.m_dictionary.Remove(last.Value);
			return last.Value;
		}

		
		public void Remove(VALUE val)
		{
			LinkedListNode<VALUE> node = this.m_dictionary[val];
			this.m_dictionary.Remove(val);
			this.m_list.Remove(node);
		}

		
		public void Clear()
		{
			this.m_dictionary.Clear();
			this.m_list.Clear();
		}

		
		private Dictionary<VALUE, LinkedListNode<VALUE>> m_dictionary;

		
		private LinkedList<VALUE> m_list;
	}
}
