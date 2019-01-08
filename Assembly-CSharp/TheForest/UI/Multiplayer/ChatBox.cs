using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI.Multiplayer
{
	public class ChatBox : MonoBehaviour
	{
		private void Awake()
		{
			if (BoltNetwork.isRunning)
			{
				this.CheckInit();
				this._input.value = null;
				this._input.gameObject.SetActive(false);
				this._messageRowPrefab.gameObject.SetActive(false);
				this._eventHandler.enabled = false;
			}
			else
			{
				UnityEngine.Object.Destroy(this._eventHandler);
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		private IEnumerator Start()
		{
			UIWidget widget = base.GetComponent<UIWidget>();
			widget.enabled = false;
			yield return null;
			widget.enabled = true;
			yield break;
		}

		private void SendLine(string line)
		{
			if (BoltNetwork.isRunning)
			{
				if (line[0] == '/')
				{
					line = line.Trim(new char[]
					{
						' ',
						'/'
					});
					int num = line.IndexOf(' ');
					if (num == -1)
					{
						CoopAdminCommand.Send(line, string.Empty);
					}
					else
					{
						CoopAdminCommand.Send(line.Substring(0, num), line.Substring(num + 1, line.Length - (num + 1)));
					}
				}
				else
				{
					ChatEvent chatEvent = ChatEvent.Create(GlobalTargets.OnlyServer);
					chatEvent.Message = line;
					chatEvent.Sender = LocalPlayer.Entity.networkId;
					chatEvent.Send();
				}
			}
		}

		private void Update()
		{
			if (LocalPlayer.Inventory == null)
			{
				return;
			}
			if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Book)
			{
				this._justCloseTheBook = true;
				return;
			}
			if (this._justCloseTheBook)
			{
				this._justCloseTheBook = false;
				this._mustOpen = false;
				this._grid.gameObject.SetActive(false);
			}
			if (this._lastInteractionTime + this._visibleDuration < Time.time && !this._input.gameObject.activeSelf)
			{
				this._grid.gameObject.SetActive(false);
			}
			if (!this._input.gameObject.activeSelf && !this._mustClose && TheForest.Utils.Input.GetButtonDown("OpenChat"))
			{
				if (this._skipNextOpen)
				{
					this._skipNextOpen = false;
				}
				else
				{
					this._eventHandler.enabled = true;
					this._mustOpen = true;
				}
			}
			else if (TheForest.Utils.Input.GetButtonDown("CloseChat"))
			{
				this._mustClose = true;
			}
			else if (this._mustOpen)
			{
				ChatBox.IsChatOpen = true;
				TheForest.Utils.Input.SetState(InputState.Chat, true);
				this._mustOpen = false;
				this._grid.gameObject.SetActive(true);
				this._input.gameObject.SetActive(true);
				this._input.isSelected = true;
				LocalPlayer.Inventory.enabled = false;
				if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World && !LocalPlayer.AnimControl.holdingGlider)
				{
					LocalPlayer.FpCharacter.MovementLocked = true;
				}
			}
			else if (this._mustClose || (this._input.gameObject.activeSelf && !this._input.isSelected))
			{
				this._mustClose = false;
				base.StartCoroutine(this.Close());
				if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World)
				{
					LocalPlayer.FpCharacter.MovementLocked = false;
				}
			}
		}

		private void OnApplicationPause(bool pause)
		{
			if (this._input.gameObject.activeSelf)
			{
				this._input.isSelected = true;
			}
		}

		private void OnDestroy()
		{
			if (this._players != null)
			{
				this._players.Clear();
			}
			if (this._messageRows != null)
			{
				this._messageRows.Clear();
			}
		}

		public void RegisterPlayer(string name, NetworkId id)
		{
			Color color = new Color(UnityEngine.Random.Range(0.25f, 1f), UnityEngine.Random.Range(0.25f, 1f), UnityEngine.Random.Range(0.25f, 1f), 1f);
			color.r += 0.1f;
			color.g += 0.1f;
			color.b += 0.1f;
			this.CheckInit();
			this._players[id] = new ChatBox.Player
			{
				_name = name,
				_color = color
			};
		}

		public void UnregisterPlayer(NetworkId id)
		{
			this.CheckInit();
			this._players.Remove(id);
		}

		public void AddLine(NetworkId? playerId, string message, bool system)
		{
			this.CheckInit();
			if ((playerId != null && this._players.ContainsKey(playerId.Value)) || system)
			{
				ChatMessageRow chatMessageRow = UnityEngine.Object.Instantiate<ChatMessageRow>(this._messageRowPrefab);
				if (system)
				{
					chatMessageRow._name.text = message;
					chatMessageRow._name.color = Color.white;
				}
				else
				{
					chatMessageRow._name.text = this._players[playerId.Value]._name + " : ";
					chatMessageRow._name.color = this._players[playerId.Value]._color;
				}
				chatMessageRow._message.text = NGUIText.StripSymbols(message);
				chatMessageRow.name = this._lastMessageId++ + chatMessageRow.name;
				chatMessageRow.gameObject.SetActive(true);
				chatMessageRow.transform.parent = this._grid.transform;
				chatMessageRow.transform.localPosition = Vector3.zero;
				chatMessageRow.transform.localScale = Vector3.one;
				this._messageRows.Enqueue(chatMessageRow);
				if (this._messageRows.Count > this._historySize)
				{
					UnityEngine.Object.Destroy(this._messageRows.Dequeue().gameObject);
				}
				this._grid.repositionNow = true;
				this._grid.gameObject.SetActive(true);
				this._lastInteractionTime = Time.time;
			}
		}

		public void OnSubmit()
		{
			if (!string.IsNullOrEmpty(this._input.value))
			{
				this.SendLine(this._input.value);
				this._input.value = null;
			}
			this._mustClose = true;
			this._lastInteractionTime = Time.time;
		}

		public IEnumerator Close()
		{
			ChatBox.IsChatOpen = false;
			this._input.value = null;
			this._input.gameObject.SetActive(false);
			this._eventHandler.enabled = false;
			LocalPlayer.Inventory.enabled = true;
			yield return YieldPresets.WaitPointOneSeconds;
			this._mustClose = false;
			TheForest.Utils.Input.SetState(InputState.Chat, false);
			yield break;
		}

		public void ForceRefreshInput()
		{
			TheForest.Utils.Input.SetState(InputState.Chat, ChatBox.IsChatOpen);
		}

		private void CheckInit()
		{
			if (this._players == null)
			{
				this._players = new Dictionary<NetworkId, ChatBox.Player>(NetworkId.EqualityComparer.Instance);
			}
			if (this._messageRows == null)
			{
				this._messageRows = new Queue<ChatMessageRow>();
			}
		}

		public static bool IsChatOpen { get; private set; }

		public UICamera _eventHandler;

		public UITable _grid;

		public UIInput _input;

		public ChatMessageRow _messageRowPrefab;

		public float _visibleDuration = 10f;

		public int _historySize = 10;

		private Dictionary<NetworkId, ChatBox.Player> _players;

		private Queue<ChatMessageRow> _messageRows;

		private float _lastInteractionTime;

		private int _lastMessageId;

		private bool _mustOpen;

		private bool _mustClose;

		private bool _skipNextOpen;

		private NetworkId _localPlayerId;

		private bool _justCloseTheBook;

		public class Player
		{
			public string _name;

			public Color _color;
		}
	}
}
