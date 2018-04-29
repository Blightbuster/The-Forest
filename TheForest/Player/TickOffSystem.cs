using System;
using System.Collections;
using TheForest.Items;
using TheForest.Tools;
using TheForest.UI;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Player
{
	
	[DoNotSerializePublic]
	public class TickOffSystem : MonoBehaviour
	{
		
		private void Awake()
		{
			if (!LevelSerializer.IsDeserializing)
			{
				base.StartCoroutine(this.DelayedAwake());
			}
		}

		
		private void OnDestroy()
		{
			EventRegistry.Player.Unsubscribe(TfEvent.TickedOffEntry, new EventRegistry.SubscriberCallback(this.DoneMessage));
			foreach (TickOffSystem.Entry entry in this._entries)
			{
				entry.Clear();
			}
		}

		
		private IEnumerator DelayedAwake()
		{
			if (!this._initialized)
			{
				this._initialized = true;
				yield return null;
				EventRegistry.Player.Subscribe(TfEvent.TickedOffEntry, new EventRegistry.SubscriberCallback(this.DoneMessage));
				foreach (TickOffSystem.Entry entry in this._entries)
				{
					if (this._tickedEntries == null || !this._tickedEntries.Contains(entry._id))
					{
						entry.Init();
					}
					else
					{
						entry._ticked = true;
						entry._tickGo.SetActive(true);
					}
				}
			}
			yield break;
		}

		
		private IEnumerator OnDeserialized()
		{
			yield return null;
			this.Awake();
			yield break;
		}

		
		private void OnSerializing()
		{
			this._tickedEntries = (from e in this._entries
			where e._ticked
			select e._id).ToArray<int>();
		}

		
		public void LogMessage(string message)
		{
			Scene.HudGui.ShowTodoListMessage(message);
		}

		
		public void DoneMessage(object o)
		{
			TickOffSystem.EntryType type = (o as TickOffSystem.Entry)._type;
			if (type != TickOffSystem.EntryType.CollectItem)
			{
				if (type != TickOffSystem.EntryType.InspectAnimal)
				{
					if (type == TickOffSystem.EntryType.InspectPlant)
					{
						this.LogMessage(UiTranslationDatabase.TranslateKey(this._inspectedPlantMessageKey, "NEW PLANT DISCOVERED", true));
					}
				}
				else
				{
					this.LogMessage(UiTranslationDatabase.TranslateKey(this._inspectedAnimalMessageKey, "NEW ANIMAL DISCOVERED", true));
				}
			}
			else
			{
				this.LogMessage(UiTranslationDatabase.TranslateKey(this._collectedItemMessageKey, "NEW PLANT DISCOVERED", true));
			}
			if (this._tickOffTab)
			{
				this._tickOffTab.Highlight(null);
			}
			LocalPlayer.Sfx.PlayTaskCompleted();
		}

		
		public SelectPageNumber _tickOffTab;

		
		public string _collectedItemMessageKey = "COLLECTED_ITEM_MESSAGE";

		
		public string _inspectedAnimalMessageKey = "INSPECTED_ANIMAL_MESSAGE";

		
		public string _inspectedPlantMessageKey = "INSPECTED_PLANT_MESSAGE";

		
		public TickOffSystem.Entry[] _entries;

		
		[SerializeThis]
		private int[] _tickedEntries;

		
		private bool _initialized;

		
		[Serializable]
		public class Entry
		{
			
			public void Init()
			{
				TickOffSystem.EntryType type = this._type;
				if (type != TickOffSystem.EntryType.CollectItem)
				{
					if (type != TickOffSystem.EntryType.InspectAnimal)
					{
						if (type == TickOffSystem.EntryType.InspectPlant)
						{
							EventRegistry.Player.Subscribe(TfEvent.InspectedPlant, new EventRegistry.SubscriberCallback(this.OnInspectedPlant));
						}
					}
					else
					{
						EventRegistry.Player.Subscribe(TfEvent.InspectedAnimal, new EventRegistry.SubscriberCallback(this.OnInspectedAnimal));
					}
				}
				else
				{
					EventRegistry.Player.Subscribe(TfEvent.AddedItem, new EventRegistry.SubscriberCallback(this.OnCollectedItem));
					EventRegistry.Player.Subscribe(TfEvent.UsedItem, new EventRegistry.SubscriberCallback(this.OnCollectedItem));
				}
			}

			
			public void Clear()
			{
				TickOffSystem.EntryType type = this._type;
				if (type != TickOffSystem.EntryType.CollectItem)
				{
					if (type != TickOffSystem.EntryType.InspectAnimal)
					{
						if (type == TickOffSystem.EntryType.InspectPlant)
						{
							EventRegistry.Player.Unsubscribe(TfEvent.InspectedPlant, new EventRegistry.SubscriberCallback(this.OnInspectedPlant));
						}
					}
					else
					{
						EventRegistry.Player.Unsubscribe(TfEvent.InspectedAnimal, new EventRegistry.SubscriberCallback(this.OnInspectedAnimal));
					}
				}
				else
				{
					EventRegistry.Player.Unsubscribe(TfEvent.AddedItem, new EventRegistry.SubscriberCallback(this.OnCollectedItem));
					EventRegistry.Player.Unsubscribe(TfEvent.UsedItem, new EventRegistry.SubscriberCallback(this.OnCollectedItem));
				}
				this._tickGo = null;
			}

			
			private void OnCollectedItem(object o)
			{
				int num = (int)o;
				if (num == this._itemId)
				{
					this._ticked = true;
					if (this._tickGo)
					{
						this._tickGo.SetActive(true);
					}
					else
					{
						Debug.LogError(string.Concat(new object[]
						{
							"Missing tick go for TickOff entry: ",
							this._id,
							" (",
							this._type,
							")"
						}));
					}
					this.Clear();
					EventRegistry.Player.Publish(TfEvent.TickedOffEntry, this);
				}
			}

			
			private void OnInspectedAnimal(object o)
			{
				AnimalType animalType = (AnimalType)o;
				if (animalType == this._animalType)
				{
					this._ticked = true;
					this._tickGo.SetActive(true);
					this.Clear();
					EventRegistry.Player.Publish(TfEvent.TickedOffEntry, this);
				}
			}

			
			private void OnInspectedPlant(object o)
			{
				AnimalType animalType = (AnimalType)o;
				if (animalType == this._animalType)
				{
					this._ticked = true;
					this._tickGo.SetActive(true);
					this.Clear();
					EventRegistry.Player.Publish(TfEvent.TickedOffEntry, this);
				}
			}

			
			public TickOffSystem.EntryType _type;

			
			public int _id;

			
			[ItemIdPicker]
			public int _itemId;

			
			public AnimalType _animalType;

			
			public GameObject _tickGo;

			
			public bool _ticked;
		}

		
		public enum EntryType
		{
			
			CollectItem,
			
			InspectAnimal,
			
			InspectPlant
		}
	}
}
