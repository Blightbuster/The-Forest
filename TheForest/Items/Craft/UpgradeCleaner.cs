using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.Craft
{
	
	public class UpgradeCleaner : MonoBehaviour
	{
		
		private void Update()
		{
			if (!LocalPlayer.Inventory._craftingCog._upgradeCog.enabled && LocalPlayer.Inventory._craftingCog.Ingredients.Count == 1)
			{
				foreach (UpgradeViewReceiver upgradeViewReceiver in this._receivers)
				{
					if (upgradeViewReceiver.Count > 0)
					{
						this.CheckDisassemble();
						return;
					}
				}
			}
			Scene.HudGui.DisassembleInfo.SetActive(false);
		}

		
		private void OnDisable()
		{
			if (Scene.HudGui)
			{
				Scene.HudGui.DisassembleInfo.SetActive(false);
				this.ResetTimeScale();
			}
		}

		
		private void CheckDisassemble()
		{
			Scene.HudGui.DisassembleInfo.SetActive(true);
			if (TheForest.Utils.Input.GetButtonDown("Craft"))
			{
				int layer = LayerMask.NameToLayer("UI");
				int itemId = base.GetComponent<InventoryItemView>()._itemId;
				if (LocalPlayer.Inventory.InventoryItemViewsCache[itemId][0]._heldWeaponInfo)
				{
					LocalPlayer.Inventory.InventoryItemViewsCache[itemId][0]._heldWeaponInfo.ResetStats();
					foreach (KeyValuePair<int, int> keyValuePair in LocalPlayer.Inventory.ItemsUpgradeCounters[itemId])
					{
						if (keyValuePair.Value > 0)
						{
							LocalPlayer.Inventory.AddItem(keyValuePair.Key, keyValuePair.Value * 40 / 100, false, false, null);
						}
					}
					LocalPlayer.Inventory.ItemsUpgradeCounters[itemId].Clear();
					foreach (UpgradeViewReceiver upgradeViewReceiver in this._receivers)
					{
						upgradeViewReceiver.CurrentUpgrades.Clear();
						upgradeViewReceiver.Count = 0;
						while (upgradeViewReceiver.transform.childCount > 0)
						{
							Transform child = upgradeViewReceiver.transform.GetChild(0);
							GameObject gameObject = new GameObject("UpgradeViewShell");
							gameObject.transform.position = child.transform.position;
							gameObject.transform.rotation = child.transform.rotation;
							gameObject.layer = layer;
							gameObject.gameObject.AddComponent<destroyAfter>().destroyTime = 1.9f;
							Rigidbody rigidbody = gameObject.gameObject.AddComponent<Rigidbody>();
							rigidbody.AddExplosionForce(70f, gameObject.transform.position - gameObject.transform.forward, 3f);
							BoxCollider boxCollider = gameObject.gameObject.AddComponent<BoxCollider>();
							boxCollider.size = Vector3.one * 0.03f;
							boxCollider.center = Vector3.forward * 0.02f;
							child.parent = gameObject.transform;
						}
						IEnumerator enumerator2 = upgradeViewReceiver._mirrorHeld.GetEnumerator();
						try
						{
							while (enumerator2.MoveNext())
							{
								object obj = enumerator2.Current;
								Transform transform = (Transform)obj;
								UnityEngine.Object.Destroy(transform.gameObject);
							}
						}
						finally
						{
							IDisposable disposable;
							if ((disposable = (enumerator2 as IDisposable)) != null)
							{
								disposable.Dispose();
							}
						}
						IEnumerator enumerator3 = upgradeViewReceiver._mirrorInventory.GetEnumerator();
						try
						{
							while (enumerator3.MoveNext())
							{
								object obj2 = enumerator3.Current;
								Transform transform2 = (Transform)obj2;
								UnityEngine.Object.Destroy(transform2.gameObject);
							}
						}
						finally
						{
							IDisposable disposable2;
							if ((disposable2 = (enumerator3 as IDisposable)) != null)
							{
								disposable2.Dispose();
							}
						}
						upgradeViewReceiver.Version = 0;
					}
					Time.timeScale = 1f;
					base.Invoke("ResetTimeScale", 2f);
				}
				Scene.HudGui.DisassembleInfo.SetActive(false);
			}
		}

		
		private void ResetTimeScale()
		{
			if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Inventory && !BoltNetwork.isRunning)
			{
				Time.timeScale = 0f;
			}
		}

		
		public UpgradeViewReceiver[] _receivers;
	}
}
