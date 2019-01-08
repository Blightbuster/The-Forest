using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace TheForest.Items.World
{
	public class QuickSelectViewsPool : MonoBehaviour
	{
		private static QuickSelectViewsPool Instance
		{
			get
			{
				if (!QuickSelectViewsPool.instance)
				{
					QuickSelectViewsPool.instance = new GameObject("QuickSelectViewPool").AddComponent<QuickSelectViewsPool>();
					QuickSelectViewsPool.instance.gameObject.SetActive(false);
				}
				return QuickSelectViewsPool.instance;
			}
		}

		private void OnDestroy()
		{
			if (QuickSelectViewsPool.instance == this)
			{
				QuickSelectViewsPool.instance = null;
			}
			this._pooledViews.Clear();
			this._pooledShadowViews.Clear();
			this._pooledViewsUsageOrder.Clear();
			this._pooledShadowViewsUsageOrder.Clear();
		}

		public static bool Spawn(int itemId, Transform parent, bool shadowsOnly)
		{
			Queue<Transform> queue;
			if (((!shadowsOnly) ? QuickSelectViewsPool.Instance._pooledViews.TryGetValue(itemId, out queue) : QuickSelectViewsPool.Instance._pooledShadowViews.TryGetValue(itemId, out queue)) && queue.Count > 0)
			{
				Transform transform = queue.Dequeue();
				Vector3 localPosition = transform.localPosition;
				Quaternion localRotation = transform.localRotation;
				transform.parent = parent;
				transform.localPosition = localPosition;
				transform.localRotation = localRotation;
				if (shadowsOnly)
				{
					QuickSelectViewsPool.Instance._pooledShadowViewsUsageOrder.Remove(itemId);
					QuickSelectViewsPool.Instance._pooledShadowViewsUsageOrder.Insert(0, itemId);
				}
				else
				{
					QuickSelectViewsPool.Instance._pooledViewsUsageOrder.Remove(itemId);
					QuickSelectViewsPool.Instance._pooledViewsUsageOrder.Insert(0, itemId);
				}
				return true;
			}
			return false;
		}

		public static void Despawn(int itemId, Transform view, bool shadowsOnly)
		{
			Queue<Transform> queue;
			if ((!shadowsOnly) ? (!QuickSelectViewsPool.Instance._pooledViews.TryGetValue(itemId, out queue)) : (!QuickSelectViewsPool.Instance._pooledShadowViews.TryGetValue(itemId, out queue)))
			{
				QuickSelectViewsPool.Instance.CheckTrim(itemId, shadowsOnly);
				queue = new Queue<Transform>();
				if (shadowsOnly)
				{
					QuickSelectViewsPool.Instance._pooledShadowViews[itemId] = queue;
					QuickSelectViewsPool.Instance._pooledShadowViewsUsageOrder.Insert(0, itemId);
				}
				else
				{
					QuickSelectViewsPool.Instance._pooledViews[itemId] = queue;
					QuickSelectViewsPool.Instance._pooledViewsUsageOrder.Insert(0, itemId);
				}
			}
			else if (shadowsOnly)
			{
				QuickSelectViewsPool.Instance._pooledShadowViewsUsageOrder.Remove(itemId);
				QuickSelectViewsPool.Instance._pooledShadowViewsUsageOrder.Insert(0, itemId);
			}
			else
			{
				QuickSelectViewsPool.Instance._pooledViewsUsageOrder.Remove(itemId);
				QuickSelectViewsPool.Instance._pooledViewsUsageOrder.Insert(0, itemId);
			}
			Vector3 localPosition = view.localPosition;
			Quaternion localRotation = view.localRotation;
			view.parent = QuickSelectViewsPool.Instance.transform;
			view.localPosition = localPosition;
			view.localRotation = localRotation;
			queue.Enqueue(view);
		}

		private void CheckTrim(int itemId, bool shadowsOnly)
		{
			int num = (!BoltNetwork.isRunning) ? 1 : (BoltNetwork.clients.Count<BoltConnection>() + 1);
			if (shadowsOnly)
			{
				int num2 = 4;
				while (QuickSelectViewsPool.Instance._pooledShadowViewsUsageOrder.Count > num2)
				{
					int count = QuickSelectViewsPool.Instance._pooledShadowViewsUsageOrder.Count;
					int key = QuickSelectViewsPool.Instance._pooledShadowViewsUsageOrder[count - 1];
					foreach (Transform transform in QuickSelectViewsPool.Instance._pooledShadowViews[key])
					{
						UnityEngine.Object.Destroy(transform.gameObject);
					}
					QuickSelectViewsPool.Instance._pooledShadowViews[key].Clear();
					QuickSelectViewsPool.Instance._pooledShadowViews.Remove(count - 1);
					QuickSelectViewsPool.Instance._pooledShadowViewsUsageOrder.RemoveAt(count - 1);
				}
			}
			else
			{
				int num2 = 4 * num;
				while (QuickSelectViewsPool.Instance._pooledViewsUsageOrder.Count > num2)
				{
					int count2 = QuickSelectViewsPool.Instance._pooledViewsUsageOrder.Count;
					int key2 = QuickSelectViewsPool.Instance._pooledViewsUsageOrder[count2 - 1];
					foreach (Transform transform2 in QuickSelectViewsPool.Instance._pooledViews[key2])
					{
						UnityEngine.Object.Destroy(transform2.gameObject);
					}
					QuickSelectViewsPool.Instance._pooledViews[key2].Clear();
					QuickSelectViewsPool.Instance._pooledViews.Remove(count2 - 1);
					QuickSelectViewsPool.Instance._pooledViewsUsageOrder.RemoveAt(count2 - 1);
				}
			}
		}

		private Dictionary<int, Queue<Transform>> _pooledViews = new Dictionary<int, Queue<Transform>>();

		private Dictionary<int, Queue<Transform>> _pooledShadowViews = new Dictionary<int, Queue<Transform>>();

		private List<int> _pooledViewsUsageOrder = new List<int>();

		private List<int> _pooledShadowViewsUsageOrder = new List<int>();

		private static QuickSelectViewsPool instance;
	}
}
