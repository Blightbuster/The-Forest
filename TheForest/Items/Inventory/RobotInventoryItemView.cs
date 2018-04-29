using System;
using System.Collections.Generic;
using TheForest.Tools;
using UnityEngine;

namespace TheForest.Items.Inventory
{
	
	[AddComponentMenu("Items/Inventory/Robot Item Inventory View")]
	[DoNotSerializePublic]
	public class RobotInventoryItemView : InventoryItemView, IItemPartInventoryView
	{
		
		private void Awake()
		{
			if (!LevelSerializer.IsDeserializing)
			{
				this.Init();
			}
		}

		
		public override void OnDeserialized()
		{
			this.Init();
			base.OnDeserialized();
		}

		
		public override void Init()
		{
			if (!this._initDone)
			{
				base.Init();
				this._initDone = true;
				if (!this._isCraft)
				{
					this._startHeldPosition = this._held.transform.localPosition;
				}
				this._revealedPieces.RemoveRange(6, this._revealedPieces.Count - 6);
				for (int i = 0; i < 6; i++)
				{
					if (this._revealedPieces[i])
					{
						if (!this._isCraft)
						{
							((IItemPartInventoryView)this._inventory._craftingCog.ItemViewsCache[this._itemId]).AddPiece(i, false);
						}
						else
						{
							this._revealedPieces[i] = true;
							this._piecesRenders[i].SetActive(true);
						}
					}
				}
			}
		}

		
		public void AddPiece(int pieceNum, bool fromCraft)
		{
			if (fromCraft && (pieceNum == 0 || pieceNum == 2))
			{
				if (!this._revealedPieces[pieceNum] && !this._revealedPieces[pieceNum + 1])
				{
					if (pieceNum == 0 && this._leftArmView.gameObject.activeSelf)
					{
						pieceNum++;
					}
					if (pieceNum == 2 && this._leftLegView.gameObject.activeSelf)
					{
						pieceNum++;
					}
				}
				else if (this._revealedPieces[pieceNum])
				{
					pieceNum++;
				}
			}
			this._revealedPieces[pieceNum] = true;
			this._piecesRenders[pieceNum].SetActive(true);
			if (!this._isCraft)
			{
				this.Init();
				this._piecesRendersHeld[pieceNum].SetActive(true);
				int num = 0;
				for (int i = 0; i < 6; i++)
				{
					if (this._revealedPieces[i])
					{
						num++;
					}
				}
				EventRegistry.Achievements.Publish(TfEvent.Achievements.RobotPieces, num);
			}
			else
			{
				((IItemPartInventoryView)this._inventory.InventoryItemViewsCache[this._itemId][0]).AddPiece(pieceNum, fromCraft);
			}
		}

		
		[SerializeThis]
		public List<bool> _revealedPieces = new List<bool>(6);

		
		public GameObject[] _piecesIIV = new GameObject[6];

		
		public GameObject[] _piecesRenders = new GameObject[6];

		
		public GameObject[] _piecesRendersHeld = new GameObject[6];

		
		public GameObject _leftLegView;

		
		public GameObject _leftArmView;

		
		private bool _initDone;

		
		private Vector3 _startHeldPosition;
	}
}
