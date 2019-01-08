using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniLinq;
using UnityEngine;

[AddComponentMenu("Storage/Advanced/Only In Range Manager")]
public class OnlyInRangeManager : MonoBehaviour
{
	public void AddRangedItem(GameObject go)
	{
		PrefabIdentifier ui = go.GetComponent<PrefabIdentifier>();
		if (ui == null)
		{
			return;
		}
		if (!this.rangedItems.Any((OnlyInRangeManager.InRange i) => i.id == ui.Id))
		{
			this.rangedItems.Add(new OnlyInRangeManager.InRange
			{
				id = ui.Id,
				transform = go.transform
			});
		}
	}

	public void DestroyRangedItem(GameObject go)
	{
		PrefabIdentifier ui = go.GetComponent<PrefabIdentifier>();
		if (ui == null)
		{
			return;
		}
		OnlyInRangeManager.InRange inRange = this.rangedItems.FirstOrDefault((OnlyInRangeManager.InRange i) => i.id == ui.Id);
		if (inRange == null || inRange.inprogress)
		{
			return;
		}
		if (File.Exists(Application.persistentDataPath + "/" + inRange.id + ".dat"))
		{
			File.Delete(Application.persistentDataPath + "/" + inRange.id + ".dat");
		}
		this.rangedItems.Remove(inRange);
	}

	private void Awake()
	{
		OnlyInRangeManager.Instance = this;
	}

	private void LateUpdate()
	{
		if (LevelSerializer.IsDeserializing)
		{
			return;
		}
		float sqrRange = this.range * this.range;
		Vector3 position = base.transform.position;
		foreach (OnlyInRangeManager.InRange inRange in this.rangedItems)
		{
			inRange.Test(this, position, sqrRange);
		}
		if (this.hideList.Count > 0 && (Time.frameCount & 1) != 0)
		{
			OnlyInRangeManager.InRange inRange2 = this.hideList.First<OnlyInRangeManager.InRange>();
			this.hideList.Remove(inRange2);
			inRange2.inprogress = true;
			base.StartCoroutine(this.HideItem(inRange2));
		}
		if (this.viewList.Count > 0 && (Time.frameCount & 1) == 0)
		{
			OnlyInRangeManager.InRange inRange3 = this.viewList.First<OnlyInRangeManager.InRange>();
			this.viewList.Remove(inRange3);
			inRange3.inprogress = true;
			base.StartCoroutine(this.ViewItem(inRange3));
		}
	}

	private IEnumerator HideItem(OnlyInRangeManager.InRange item)
	{
		LevelSerializer.DontCollect();
		byte[] data = LevelSerializer.SerializeLevel(false, item.transform.GetComponent<UniqueIdentifier>().Id);
		yield return new WaitForEndOfFrame();
		LevelSerializer.Collect();
		FileStream f = File.Create(Application.persistentDataPath + "/" + item.id + ".dat");
		f.Write(data, 0, data.Length);
		yield return null;
		f.Close();
		yield return new WaitForEndOfFrame();
		item.lastPosition = item.transform.position;
		UnityEngine.Object.Destroy(item.transform.gameObject);
		yield return new WaitForEndOfFrame();
		item.transform = null;
		item.inprogress = false;
		yield break;
	}

	private IEnumerator ViewItem(OnlyInRangeManager.InRange item)
	{
		if (!File.Exists(Application.persistentDataPath + "/" + item.id + ".dat"))
		{
			yield break;
		}
		yield return new WaitForEndOfFrame();
		FileStream f = File.Open(Application.persistentDataPath + "/" + item.id + ".dat", FileMode.Open);
		byte[] d = new byte[f.Length];
		f.Read(d, 0, (int)f.Length);
		f.Close();
		yield return new WaitForEndOfFrame();
		bool complete = false;
		LevelLoader loader = null;
		LevelSerializer.DontCollect();
		LevelSerializer.LoadNow(d, true, false, delegate(LevelLoader usedLevelLoader)
		{
			complete = true;
			loader = usedLevelLoader;
			LevelSerializer.Collect();
		});
		while (!complete)
		{
			yield return null;
		}
		item.transform = loader.Last.transform;
		item.inprogress = false;
		yield break;
	}

	public static OnlyInRangeManager Instance;

	[SerializeThis]
	private HashSet<OnlyInRangeManager.InRange> rangedItems = new HashSet<OnlyInRangeManager.InRange>();

	[SerializeThis]
	private HashSet<OnlyInRangeManager.InRange> hideList = new HashSet<OnlyInRangeManager.InRange>();

	[SerializeThis]
	private HashSet<OnlyInRangeManager.InRange> viewList = new HashSet<OnlyInRangeManager.InRange>();

	public float range = 5f;

	public class InRange
	{
		public bool inprogress
		{
			get
			{
				return this._inprogress;
			}
			set
			{
				this._inprogress = value;
				this.count = 0;
			}
		}

		public void Test(OnlyInRangeManager manager, Vector3 position, float sqrRange)
		{
			if (this.inprogress)
			{
				return;
			}
			if (this.transform != null)
			{
				if ((this.transform.position - position).sqrMagnitude < sqrRange)
				{
					this.count++;
					if (this.count > 3)
					{
						manager.hideList.Remove(this);
					}
				}
				else
				{
					this.count = 0;
					manager.hideList.Add(this);
				}
			}
			else if ((this.lastPosition - position).sqrMagnitude < sqrRange)
			{
				this.count++;
				if (this.count > 3)
				{
					manager.viewList.Add(this);
				}
			}
			else
			{
				this.count = 0;
				manager.viewList.Remove(this);
			}
		}

		public Transform transform;

		public Vector3 lastPosition;

		private bool _inprogress;

		public string id;

		public int count;
	}
}
