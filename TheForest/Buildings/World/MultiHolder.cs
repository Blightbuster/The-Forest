using System;
using System.Collections;
using Bolt;
using TheForest.Audio;
using TheForest.Items;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	[DoNotSerializePublic]
	public class MultiHolder : EntityBehaviour<IMultiHolderState>
	{
		
		private void Awake()
		{
			this._originalMass = base.transform.parent.GetComponent<Rigidbody>().mass;
			this._originalDrag = base.transform.parent.GetComponent<Rigidbody>().drag;
			this.SetEnabled(false);
		}

		
		private void Update()
		{
			if ((BoltNetwork.isRunning && this.MP_CanInterract()) || (!BoltNetwork.isRunning && !LocalPlayer.FpCharacter.PushingSled))
			{
				MultiHolder.ContentTypes contentTypes = MultiHolder.ContentTypes.None;
				if (this._contentTypeActual != MultiHolder.ContentTypes.None)
				{
					this._addingContentType = this._contentTypeActual;
				}
				if (this._addingContentType == MultiHolder.ContentTypes.Log)
				{
					this.LogContentUpdate(ref contentTypes);
				}
				else if (this._addingContentType == MultiHolder.ContentTypes.Body)
				{
					if (!BoltNetwork.isRunning)
					{
						this.BodyContentUpdate(ref contentTypes);
					}
				}
				else if (this._addingContentType == MultiHolder.ContentTypes.Rock)
				{
					this.RockContentUpdate(ref contentTypes);
				}
				else if (this._addingContentType == MultiHolder.ContentTypes.Stick)
				{
					this.StickContentUpdate(ref contentTypes);
				}
				if (this._contentTypeActual == MultiHolder.ContentTypes.None)
				{
					bool flag = this.CanToggleNextAddItem();
					if (flag && TheForest.Utils.Input.GetButtonDown("Rotate"))
					{
						LocalPlayer.Sfx.PlayWhoosh();
						this.ToggleNextAddContent();
					}
					switch (this._addingContentType)
					{
					case MultiHolder.ContentTypes.None:
						Scene.HudGui.MultiSledAddWidget.Shutdown();
						break;
					case MultiHolder.ContentTypes.Log:
						Scene.HudGui.MultiSledAddWidget.ShowList(this.LogItemId, this.TakeIcon.transform, SideIcons.Craft);
						break;
					case MultiHolder.ContentTypes.Body:
						Scene.HudGui.MultiSledAddWidget.ShowList(-1, this.TakeIcon.transform, SideIcons.Craft);
						break;
					case MultiHolder.ContentTypes.Rock:
						Scene.HudGui.MultiSledAddWidget.ShowList(this.RockItemId, this.TakeIcon.transform, SideIcons.Craft);
						break;
					case MultiHolder.ContentTypes.Stick:
						Scene.HudGui.MultiSledAddWidget.ShowList(this.StickItemId, this.TakeIcon.transform, SideIcons.Craft);
						break;
					}
				}
				else
				{
					Scene.HudGui.MultiSledAddWidget.Shutdown();
				}
			}
			else
			{
				Scene.HudGui.HolderWidgets[0].ShutDown();
				Scene.HudGui.HolderWidgets[1].ShutDown();
				Scene.HudGui.HolderWidgets[2].ShutDown();
				Scene.HudGui.HolderWidgets[6].ShutDown();
				Scene.HudGui.MultiSledAddWidget.Shutdown();
			}
		}

		
		private void OnDeserialized()
		{
			if (this._contentTypeActual == MultiHolder.ContentTypes.Body)
			{
				if (this._bodies == null)
				{
					this._bodies = new GameObject[3];
				}
				if (this._bodyTypes == null)
				{
					this._bodyTypes = new EnemyType[3];
				}
				for (int i = 0; i < this._contentActual; i++)
				{
					this.SpawnBody(i);
				}
			}
			else if (this._contentTypeActual == MultiHolder.ContentTypes.Rock)
			{
				if (!BoltNetwork.isRunning)
				{
					for (int j = this._contentActual - 1; j >= 0; j--)
					{
						this.RockRender[j].SetActive(true);
					}
				}
			}
			else if (this._contentTypeActual == MultiHolder.ContentTypes.Stick && !BoltNetwork.isRunning)
			{
				for (int k = this._contentActual - 1; k >= 0; k--)
				{
					this.StickRender[k].SetActive(true);
				}
			}
		}

		
		private void SetEnabled(bool value)
		{
			base.enabled = value;
		}

		
		private void GrabEnter()
		{
			if (!LocalPlayer.AnimControl.doSledPushMode)
			{
				LocalPlayer.Inventory.DontShowDrop = true;
				this.SetEnabled(true);
				if (LocalPlayer.AnimControl.carry && !BoltNetwork.isRunning)
				{
					this._addingContentType = MultiHolder.ContentTypes.Body;
				}
				else if (LocalPlayer.Inventory.Logs.HasLogs)
				{
					this._addingContentType = MultiHolder.ContentTypes.Log;
				}
				else if (!LocalPlayer.Inventory.IsRightHandEmpty())
				{
					if (LocalPlayer.Inventory.RightHandOrNext.ItemCache._id == this.RockItemId)
					{
						this._addingContentType = MultiHolder.ContentTypes.Rock;
					}
					else if (LocalPlayer.Inventory.RightHandOrNext.ItemCache._id == this.StickItemId)
					{
						this._addingContentType = MultiHolder.ContentTypes.Stick;
					}
					else
					{
						this._addingContentType--;
						this.ToggleNextAddContent();
					}
				}
				else
				{
					this._addingContentType--;
					this.ToggleNextAddContent();
				}
			}
		}

		
		public void GrabExit()
		{
			Scene.HudGui.HolderWidgets[0].ShutDown();
			Scene.HudGui.HolderWidgets[1].ShutDown();
			Scene.HudGui.HolderWidgets[2].ShutDown();
			Scene.HudGui.HolderWidgets[6].ShutDown();
			Scene.HudGui.MultiSledAddWidget.Shutdown();
			LocalPlayer.Inventory.DontShowDrop = false;
			this.SetEnabled(false);
		}

		
		private void ToggleNextAddContent()
		{
			this._addingContentType = this.GetNextItemIndex();
		}

		
		public bool CanToggleNextAddItem()
		{
			return this.GetNextItemIndex() != this._addingContentType;
		}

		
		private MultiHolder.ContentTypes GetNextItemIndex()
		{
			if (this._addingContentType < MultiHolder.ContentTypes.None)
			{
				this._addingContentType = MultiHolder.ContentTypes.None;
			}
			for (int i = 1; i < 5; i++)
			{
				int result = (int)((this._addingContentType + i) % (MultiHolder.ContentTypes)5);
				switch (result)
				{
				case 1:
					if (LocalPlayer.Inventory.Owns(this.LogItemId, false))
					{
						return (MultiHolder.ContentTypes)result;
					}
					break;
				case 2:
					if (LocalPlayer.AnimControl.carry && !BoltNetwork.isRunning)
					{
						return (MultiHolder.ContentTypes)result;
					}
					break;
				case 3:
					if (LocalPlayer.Inventory.Owns(this.RockItemId, false))
					{
						return (MultiHolder.ContentTypes)result;
					}
					break;
				case 4:
					if (LocalPlayer.Inventory.Owns(this.StickItemId, false))
					{
						return (MultiHolder.ContentTypes)result;
					}
					break;
				}
			}
			return this._addingContentType;
		}

		
		private void RefreshMassAndDrag()
		{
			if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner && !base.state.IsReal)
			{
				return;
			}
			if (this.Pushable)
			{
				base.transform.parent.GetComponent<Rigidbody>().mass = this._originalMass + (float)(((this._contentTypeActual != MultiHolder.ContentTypes.Log) ? 20 : 10) * this._contentActual);
				base.transform.parent.GetComponent<Rigidbody>().drag = this._originalDrag + 0.5f * (float)this._contentActual;
			}
		}

		
		private void RockContentUpdate(ref MultiHolder.ContentTypes showAddIcon)
		{
			bool flag = true;
			bool takeIcon = false;
			if ((!BoltNetwork.isRunning && this._contentActual > 0) || (BoltNetwork.isRunning && this._contentActual > 0 && flag))
			{
				takeIcon = true;
				if (TheForest.Utils.Input.GetButtonDown("Take"))
				{
					if (BoltNetwork.isRunning)
					{
						ItemHolderTakeItem itemHolderTakeItem = ItemHolderTakeItem.Create(GlobalTargets.OnlyServer);
						itemHolderTakeItem.ContentType = (int)this._contentTypeActual;
						itemHolderTakeItem.Target = base.entity;
						itemHolderTakeItem.Player = LocalPlayer.Entity;
						itemHolderTakeItem.Send();
					}
					else if (LocalPlayer.Inventory.AddItem(this.RockItemId, 1, false, false, null) || LocalPlayer.Inventory.FakeDrop(this.RockItemId, null))
					{
						this.RockRender[this._contentActual - 1].SetActive(false);
						this._contentActual--;
						if (this._contentActual == 0)
						{
							takeIcon = false;
							this._contentTypeActual = MultiHolder.ContentTypes.None;
						}
						this.RefreshMassAndDrag();
					}
				}
			}
			if (this._contentActual < this.RockRender.Length && LocalPlayer.Inventory.Owns(this.RockItemId, true) && flag && (this._content == MultiHolder.ContentTypes.Rock || this._addingContentType == MultiHolder.ContentTypes.Rock))
			{
				showAddIcon = MultiHolder.ContentTypes.Rock;
				if (TheForest.Utils.Input.GetButtonDown("Craft"))
				{
					LocalPlayer.Inventory.RemoveItem(this.RockItemId, 1, false, true);
					Sfx.Play(SfxInfo.SfxTypes.AddLog, this.RockRender[this._contentActual].transform, true);
					if (BoltNetwork.isRunning)
					{
						ItemHolderAddItem itemHolderAddItem = ItemHolderAddItem.Create(GlobalTargets.OnlyServer);
						itemHolderAddItem.ContentType = 3;
						itemHolderAddItem.Target = base.entity;
						itemHolderAddItem.Send();
					}
					else
					{
						this._contentTypeActual = MultiHolder.ContentTypes.Rock;
						this._contentActual++;
						this.RockRender[this._contentActual - 1].SetActive(true);
						this.RefreshMassAndDrag();
					}
				}
			}
			Scene.HudGui.HolderWidgets[1].Show(takeIcon, this._contentTypeActual == MultiHolder.ContentTypes.Rock && showAddIcon == MultiHolder.ContentTypes.Rock, this.TakeIcon.transform);
		}

		
		private void StickContentUpdate(ref MultiHolder.ContentTypes showAddIcon)
		{
			bool flag = true;
			bool takeIcon = false;
			if ((!BoltNetwork.isRunning && this._contentActual > 0) || (BoltNetwork.isRunning && this._contentActual > 0 && flag))
			{
				takeIcon = true;
				if (TheForest.Utils.Input.GetButtonDown("Take"))
				{
					if (BoltNetwork.isRunning)
					{
						ItemHolderTakeItem itemHolderTakeItem = ItemHolderTakeItem.Create(GlobalTargets.OnlyServer);
						itemHolderTakeItem.ContentType = (int)this._contentTypeActual;
						itemHolderTakeItem.Target = base.entity;
						itemHolderTakeItem.Player = LocalPlayer.Entity;
						itemHolderTakeItem.Send();
					}
					else if (LocalPlayer.Inventory.AddItem(this.StickItemId, 1, false, false, null) || LocalPlayer.Inventory.FakeDrop(this.StickItemId, null))
					{
						this.StickRender[this._contentActual - 1].SetActive(false);
						this._contentActual--;
						if (this._contentActual == 0)
						{
							takeIcon = false;
							this._contentTypeActual = MultiHolder.ContentTypes.None;
						}
						this.RefreshMassAndDrag();
					}
				}
			}
			if (this._contentActual < this.StickRender.Length && LocalPlayer.Inventory.Owns(this.StickItemId, true) && flag && (this._content == MultiHolder.ContentTypes.Stick || this._addingContentType == MultiHolder.ContentTypes.Stick))
			{
				showAddIcon = MultiHolder.ContentTypes.Stick;
				if (TheForest.Utils.Input.GetButtonDown("Craft"))
				{
					LocalPlayer.Inventory.RemoveItem(this.StickItemId, 1, false, true);
					Sfx.Play(SfxInfo.SfxTypes.AddLog, this.StickRender[this._contentActual].transform, true);
					if (BoltNetwork.isRunning)
					{
						ItemHolderAddItem itemHolderAddItem = ItemHolderAddItem.Create(GlobalTargets.OnlyServer);
						itemHolderAddItem.ContentType = 4;
						itemHolderAddItem.Target = base.entity;
						itemHolderAddItem.Send();
					}
					else
					{
						this._contentTypeActual = MultiHolder.ContentTypes.Stick;
						this._contentActual++;
						this.StickRender[this._contentActual - 1].SetActive(true);
						this.RefreshMassAndDrag();
					}
				}
			}
			Scene.HudGui.HolderWidgets[0].Show(takeIcon, this._contentTypeActual == MultiHolder.ContentTypes.Stick && showAddIcon == MultiHolder.ContentTypes.Stick, this.TakeIcon.transform);
		}

		
		private void LogContentUpdate(ref MultiHolder.ContentTypes showAddIcon)
		{
			bool flag = true;
			bool takeIcon = false;
			if ((!BoltNetwork.isRunning && this._contentActual > 0) || (BoltNetwork.isRunning && this._contentActual > 0 && flag))
			{
				takeIcon = true;
				if (TheForest.Utils.Input.GetButtonDown("Take"))
				{
					if (BoltNetwork.isRunning)
					{
						ItemHolderTakeItem itemHolderTakeItem = ItemHolderTakeItem.Create(GlobalTargets.OnlyServer);
						itemHolderTakeItem.ContentType = (int)this._contentTypeActual;
						itemHolderTakeItem.Target = base.entity;
						itemHolderTakeItem.Player = LocalPlayer.Entity;
						itemHolderTakeItem.Send();
					}
					else if (LocalPlayer.Inventory.Logs.Lift() || LocalPlayer.Inventory.FakeDrop(this.LogItemId, null))
					{
						this.LogRender[this._contentActual - 1].SetActive(false);
						this._contentActual--;
						if (this._contentActual == 0)
						{
							this._contentTypeActual = MultiHolder.ContentTypes.None;
						}
						this.RefreshMassAndDrag();
					}
				}
			}
			if (this._contentActual < this.LogRender.Length && LocalPlayer.Inventory.Logs.Amount > 0 && flag && this._addingContentType == MultiHolder.ContentTypes.Log)
			{
				showAddIcon = MultiHolder.ContentTypes.Log;
				if (TheForest.Utils.Input.GetButtonDown("Craft"))
				{
					this._contentTypeActual = MultiHolder.ContentTypes.Log;
					Sfx.Play(SfxInfo.SfxTypes.AddLog, this.LogRender[this._contentActual].transform, true);
					LocalPlayer.Inventory.Logs.PutDown(false, false, true, null);
					if (BoltNetwork.isRunning)
					{
						ItemHolderAddItem itemHolderAddItem = ItemHolderAddItem.Create(GlobalTargets.OnlyServer);
						itemHolderAddItem.ContentType = 1;
						itemHolderAddItem.Target = base.entity;
						itemHolderAddItem.Send();
					}
					else
					{
						this._contentActual++;
						this.LogRender[this._contentActual - 1].SetActive(true);
						this.RefreshMassAndDrag();
					}
				}
			}
			Scene.HudGui.HolderWidgets[2].Show(takeIcon, this._contentTypeActual == MultiHolder.ContentTypes.Log && showAddIcon == MultiHolder.ContentTypes.Log, this.TakeIcon.transform);
		}

		
		private void BodyContentUpdate_MP(ref MultiHolder.ContentTypes showAddIcon)
		{
			bool takeIcon = false;
			if ((base.state.Body0 || base.state.Body1 || base.state.Body2) && !LocalPlayer.Inventory.Logs.HasLogs)
			{
				takeIcon = true;
				if (TheForest.Utils.Input.GetButtonDown("Take"))
				{
					TakeBody takeBody = TakeBody.Create(GlobalTargets.OnlyServer);
					takeBody.Sled = base.entity;
					if (base.state.Body2)
					{
						takeBody.Body = base.state.Body2;
					}
					else if (base.state.Body1)
					{
						takeBody.Body = base.state.Body1;
					}
					else if (base.state.Body0)
					{
						takeBody.Body = base.state.Body0;
					}
					takeBody.Send();
					Debug.Log("TakeBody:Send");
				}
			}
			if (LocalPlayer.AnimControl.placedBodyGo && LocalPlayer.AnimControl.placedBodyGo.GetComponentInChildren<BoltEntity>() && (!base.state.Body0 || !base.state.Body1 || !base.state.Body2))
			{
				showAddIcon = MultiHolder.ContentTypes.Body;
				if (TheForest.Utils.Input.GetButtonDown("Craft"))
				{
					GameObject placedBodyGo = LocalPlayer.AnimControl.placedBodyGo;
					AddBody addBody = AddBody.Create(GlobalTargets.OnlyServer);
					addBody.Body = placedBodyGo.GetComponentInChildren<BoltEntity>();
					addBody.Sled = base.entity;
					addBody.Send();
					LocalPlayer.AnimControl.heldBodyGo.SetActive(false);
					LocalPlayer.Animator.SetBoolReflected("bodyHeld", false);
					LocalPlayer.AnimControl.carry = false;
					LocalPlayer.AnimControl.placedBodyGo = null;
					LocalPlayer.Inventory.ShowAllEquiped(true);
					Scene.HudGui.DropButton.SetActive(false);
					placedBodyGo.SetActive(true);
					placedBodyGo.SendMessage("dropFromCarry", false, SendMessageOptions.DontRequireReceiver);
				}
			}
			Scene.HudGui.HolderWidgets[6].Show(takeIcon, this._contentTypeActual == MultiHolder.ContentTypes.Body && showAddIcon == MultiHolder.ContentTypes.Body, this.TakeIcon.transform);
		}

		
		private void BodyContentUpdate(ref MultiHolder.ContentTypes showAddIcon)
		{
			bool takeIcon = false;
			if (BoltNetwork.isRunning)
			{
				this.BodyContentUpdate_MP(ref showAddIcon);
				return;
			}
			if (this._contentActual > 0 && !LocalPlayer.AnimControl.heldBodyGo.activeSelf && !LocalPlayer.Inventory.Logs.HasLogs)
			{
				takeIcon = true;
				if (TheForest.Utils.Input.GetButtonDown("Take"))
				{
					LocalPlayer.AnimControl.setMutantPickUp(this.PickUpBody());
					this.RefreshMassAndDrag();
				}
			}
			if (this._contentActual < 3 && LocalPlayer.AnimControl.carry && this._addingContentType == MultiHolder.ContentTypes.Body)
			{
				showAddIcon = MultiHolder.ContentTypes.Body;
				if (TheForest.Utils.Input.GetButtonDown("Craft"))
				{
					if (this._bodies == null)
					{
						this._bodies = new GameObject[3];
					}
					if (this._bodyTypes == null || this._bodyTypes.Length < 3)
					{
						this._bodyTypes = new EnemyType[3];
					}
					this._contentTypeActual = MultiHolder.ContentTypes.Body;
					LocalPlayer.AnimControl.heldBodyGo.SetActive(false);
					GameObject placedBodyGo = LocalPlayer.AnimControl.placedBodyGo;
					dummyTypeSetup dummyTypeSetup = placedBodyGo.GetComponent<dummyTypeSetup>() ?? placedBodyGo.GetComponentInChildren<dummyTypeSetup>();
					placedBodyGo.SetActive(true);
					this.DisableBodyCollisions(placedBodyGo);
					MultiHolder.GetTriggerChild(placedBodyGo.transform).gameObject.SetActive(false);
					dummyAnimatorControl dummyAnimatorControl = placedBodyGo.GetComponent<dummyAnimatorControl>() ?? placedBodyGo.GetComponentInChildren<dummyAnimatorControl>();
					dummyAnimatorControl.BodyCollider.enabled = false;
					dummyAnimatorControl.burnTrigger.gameObject.SetActive(false);
					placedBodyGo.transform.position = this.MutantBodySlots[this._contentActual].transform.position;
					placedBodyGo.transform.rotation = this.MutantBodySlots[this._contentActual].transform.rotation;
					placedBodyGo.transform.parent = base.transform.root;
					dummyAnimatorControl.Invoke("disableControl", 2.5f);
					dummyAnimatorControl.bodyOnSled = true;
					placedBodyGo.SendMessage("dropFromCarry", false, SendMessageOptions.DontRequireReceiver);
					this._bodyTypes[this._contentActual] = dummyTypeSetup._type;
					this._bodies[this._contentActual] = placedBodyGo;
					this._contentActual++;
					Sfx.Play(SfxInfo.SfxTypes.AddLog, placedBodyGo.transform, true);
					this.RefreshMassAndDrag();
					Scene.HudGui.DropButton.SetActive(false);
					LocalPlayer.Animator.SetBoolReflected("bodyHeld", false);
					LocalPlayer.AnimControl.carry = false;
					LocalPlayer.AnimControl.placedBodyGo = null;
					LocalPlayer.Inventory.ShowAllEquiped(true);
				}
			}
			Scene.HudGui.HolderWidgets[6].Show(takeIcon, this._contentTypeActual == MultiHolder.ContentTypes.Body && showAddIcon == MultiHolder.ContentTypes.Body, this.TakeIcon.transform);
		}

		
		public void TakeBodyMP(BoltEntity body, BoltConnection from)
		{
			if (!body)
			{
				return;
			}
			dummyTypeSetup componentInChildren = body.gameObject.GetComponentInChildren<dummyTypeSetup>();
			MultiHolder multiHolder;
			if (base.state.IsReal)
			{
				IMultiHolderState state = base.state;
				multiHolder = this;
			}
			else
			{
				if (!base.state.Replaces)
				{
					return;
				}
				IMultiHolderState state = base.state.Replaces.GetState<IMultiHolderState>();
				multiHolder = base.state.Replaces.GetComponentsInChildren<MultiHolder>(true)[0];
			}
			bool flag = false;
			TakeBodyApprove takeBodyApprove = (from != null) ? TakeBodyApprove.Create(from) : TakeBodyApprove.Create(GlobalTargets.OnlySelf);
			if (base.state.Body0 == body)
			{
				Debug.Log("TakeBody:Body0:" + base.state.Body0);
				flag = true;
				takeBodyApprove.Body = body;
				takeBodyApprove.Send();
				base.state.Body0 = null;
				multiHolder._contentActual = 0;
				multiHolder._contentTypeActual = MultiHolder.ContentTypes.None;
				multiHolder._bodyTypes[0] = EnemyType.regularMale;
			}
			else if (base.state.Body1 == body)
			{
				Debug.Log("TakeBody:Body1:" + base.state.Body1);
				flag = true;
				takeBodyApprove.Body = body;
				takeBodyApprove.Send();
				base.state.Body1 = null;
				multiHolder._contentActual = 1;
				multiHolder._bodyTypes[1] = EnemyType.regularMale;
			}
			else if (base.state.Body2 == body)
			{
				Debug.Log("TakeBody:Body2:" + base.state.Body2);
				flag = true;
				takeBodyApprove.Body = body;
				takeBodyApprove.Send();
				base.state.Body2 = null;
				multiHolder._contentActual = 2;
				multiHolder._bodyTypes[2] = EnemyType.regularMale;
			}
			if (flag)
			{
				body.GetState<IMutantState>().Transform.SetTransforms(body.transform);
			}
		}

		
		public void AddBodyMP(BoltEntity body)
		{
			Debug.Log("addbodymp1");
			if (!body)
			{
				return;
			}
			this.DisableBodyCollisions(body.gameObject);
			Debug.Log("addbodymp2");
			dummyTypeSetup componentInChildren = body.gameObject.GetComponentInChildren<dummyTypeSetup>();
			IMultiHolderState state;
			MultiHolder multiHolder;
			if (base.state.IsReal)
			{
				state = base.state;
				multiHolder = this;
				Debug.Log("addbodymp3-1");
			}
			else
			{
				state = base.state.Replaces.GetState<IMultiHolderState>();
				multiHolder = base.state.Replaces.GetComponentsInChildren<MultiHolder>(true)[0];
				Debug.Log("addbodymp3-2");
			}
			if (multiHolder._bodyTypes == null || multiHolder._bodyTypes.Length == 0)
			{
				Debug.Log("addbodymp4");
				multiHolder._bodyTypes = new EnemyType[3];
			}
			if (state.Body0 == null)
			{
				Debug.Log("addbodymp5");
				state.Body0 = body;
				state.Body0.GetState<IMutantState>().Transform.SetTransforms(null, null);
				multiHolder._bodyTypes[0] = componentInChildren._type;
				multiHolder._contentActual = 1;
				multiHolder._contentTypeActual = MultiHolder.ContentTypes.Body;
				MultiHolder.GetTriggerChild(body.transform).gameObject.SetActive(false);
			}
			else if (state.Body1 == null)
			{
				Debug.Log("addbodymp6");
				state.Body1 = body;
				state.Body1.GetState<IMutantState>().Transform.SetTransforms(null, null);
				multiHolder._bodyTypes[1] = componentInChildren._type;
				multiHolder._contentActual = 2;
				multiHolder._contentTypeActual = MultiHolder.ContentTypes.Body;
				MultiHolder.GetTriggerChild(body.transform).gameObject.SetActive(false);
			}
			else if (state.Body2 == null)
			{
				Debug.Log("addbodymp7");
				state.Body2 = body;
				state.Body2.GetState<IMutantState>().Transform.SetTransforms(null, null);
				multiHolder._bodyTypes[2] = componentInChildren._type;
				multiHolder._contentActual = 3;
				multiHolder._contentTypeActual = MultiHolder.ContentTypes.Body;
				MultiHolder.GetTriggerChild(body.transform).gameObject.SetActive(false);
			}
		}

		
		public static Transform GetTriggerChild(Transform t)
		{
			Transform transform = t.transform.Find("Trigger");
			if (!transform)
			{
				int num = LayerMask.NameToLayer("PickUp");
				IEnumerator enumerator = t.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform transform2 = (Transform)obj;
						IEnumerator enumerator2 = transform2.GetEnumerator();
						try
						{
							while (enumerator2.MoveNext())
							{
								object obj2 = enumerator2.Current;
								Transform transform3 = (Transform)obj2;
								if (transform3.gameObject.layer == num)
								{
									transform = transform3;
									break;
								}
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
						if (transform)
						{
							break;
						}
					}
				}
				finally
				{
					IDisposable disposable2;
					if ((disposable2 = (enumerator as IDisposable)) != null)
					{
						disposable2.Dispose();
					}
				}
			}
			return transform;
		}

		
		private void SpawnBody(int bodyId)
		{
			Debug.Log("SPAWN BODY:" + bodyId);
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Prefabs.Instance._deadMutantBodies[(int)this._bodyTypes[bodyId]]);
			gameObject.SetActive(true);
			this.DisableBodyCollisions(gameObject);
			gameObject.transform.Find("Trigger").gameObject.SetActive(false);
			gameObject.transform.position = this.MutantBodySlots[bodyId].transform.position;
			gameObject.transform.rotation = this.MutantBodySlots[bodyId].transform.rotation;
			gameObject.transform.parent = base.transform.root;
			dummyAnimatorControl component = gameObject.GetComponent<dummyAnimatorControl>();
			component.bodyOnSled = true;
			gameObject.SendMessage("dropFromCarry", false, SendMessageOptions.DontRequireReceiver);
			component.Invoke("disableControl", 2.5f);
			this._bodies[bodyId] = gameObject;
		}

		
		public GameObject PickUpBody()
		{
			this._contentActual--;
			if (this._contentActual == 0)
			{
				this._contentTypeActual = MultiHolder.ContentTypes.None;
			}
			GameObject gameObject = this._bodies[this._contentActual];
			this._bodies[this._contentActual] = null;
			gameObject.transform.parent = null;
			MultiHolder.GetTriggerChild(gameObject.transform).gameObject.SetActive(true);
			dummyAnimatorControl dummyAnimatorControl = gameObject.GetComponent<dummyAnimatorControl>() ?? gameObject.GetComponentInChildren<dummyAnimatorControl>();
			dummyAnimatorControl.enabled = true;
			return gameObject;
		}

		
		private void DisableBodyCollisions(GameObject b)
		{
			Collider[] componentsInChildren = b.transform.GetComponentsInChildren<Collider>();
			foreach (Collider collider in componentsInChildren)
			{
				if (collider.gameObject.activeSelf && !collider.isTrigger && collider.enabled)
				{
					Physics.IgnoreCollision(this.DynamicCollision, collider, true);
				}
			}
		}

		
		public override void Attached()
		{
			if (!BoltNetwork.isServer || base.entity.isOwner)
			{
			}
			base.state.AddCallback("LogCount", new PropertyCallbackSimple(this.ItemCountChangedMP));
			if (BoltNetwork.isServer && base.entity.isOwner && this._content == MultiHolder.ContentTypes.Body)
			{
				int contentAmount = this._contentAmount;
				for (int i = 0; i < contentAmount; i++)
				{
					base.state.IsReal = true;
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Prefabs.Instance._deadMutantBodies[(int)this._bodyTypes[i]]);
					gameObject.SetActive(true);
					BoltNetwork.Attach(gameObject);
					gameObject.SendMessage("dropFromCarry", false, SendMessageOptions.DontRequireReceiver);
					gameObject.GetComponent<dummyAnimatorControl>().enabled = false;
					this.AddBodyMP(gameObject.GetComponent<BoltEntity>());
				}
			}
		}

		
		private bool MP_CanInterract()
		{
			if (!base.entity || !base.entity.isAttached || !Grabber.FocusedItem || !Grabber.FocusedItem.gameObject.Equals(base.gameObject))
			{
				return false;
			}
			if (base.state.IsReal)
			{
				return !base.state.GrabbedBy;
			}
			return !base.entity.isOwner;
		}

		
		public void TakeItemMP(BoltEntity targetPlayer, MultiHolder.ContentTypes type)
		{
			if (type != this._contentTypeActual)
			{
				return;
			}
			if (this._contentActual > 0)
			{
				if (BoltNetwork.isServer)
				{
					PlayerAddItem playerAddItem;
					if (targetPlayer.isOwner)
					{
						playerAddItem = PlayerAddItem.Create(GlobalTargets.OnlySelf);
					}
					else
					{
						playerAddItem = PlayerAddItem.Create(targetPlayer.source);
					}
					if (this._contentTypeActual == MultiHolder.ContentTypes.Rock)
					{
						playerAddItem.ItemId = this.RockItemId;
					}
					else if (this._contentTypeActual == MultiHolder.ContentTypes.Stick)
					{
						playerAddItem.ItemId = this.StickItemId;
					}
					else
					{
						playerAddItem.ItemId = this.LogItemId;
					}
					playerAddItem.Send();
				}
				if (base.entity.isOwner)
				{
					this._contentActual = Mathf.Max(this._contentActual - 1, 0);
					if (this._contentActual == 0)
					{
						this._contentTypeActual = MultiHolder.ContentTypes.None;
					}
					this.RefreshMassAndDrag();
				}
				else
				{
					ItemHolderTakeItem itemHolderTakeItem = ItemHolderTakeItem.Create(base.entity.source);
					itemHolderTakeItem.Target = base.entity;
					itemHolderTakeItem.ContentType = (int)this._contentTypeActual;
					itemHolderTakeItem.Send();
				}
				return;
			}
		}

		
		public void AddItemMP(MultiHolder.ContentTypes type, BoltConnection source)
		{
			if ((this._contentTypeActual == MultiHolder.ContentTypes.Log || this._contentTypeActual == MultiHolder.ContentTypes.None) && type == MultiHolder.ContentTypes.Log)
			{
				if (type == MultiHolder.ContentTypes.None)
				{
					this._contentActual = 0;
				}
				if (base.entity.isOwner)
				{
					this._contentTypeActual = MultiHolder.ContentTypes.Log;
					if (this._contentActual < this.LogRender.Length)
					{
						this._contentActual = Mathf.Min(this._contentActual + 1, this.LogRender.Length);
					}
					else
					{
						PlayerAddItem playerAddItem = PlayerAddItem.Create(source);
						playerAddItem.ItemId = this.LogItemId;
						playerAddItem.Send();
					}
					this.RefreshMassAndDrag();
				}
				else
				{
					ItemHolderAddItem itemHolderAddItem = ItemHolderAddItem.Create(base.entity.source);
					itemHolderAddItem.Target = base.entity;
					itemHolderAddItem.ContentType = 1;
					itemHolderAddItem.Send();
				}
			}
			if ((this._contentTypeActual == MultiHolder.ContentTypes.Rock || this._contentTypeActual == MultiHolder.ContentTypes.None) && type == MultiHolder.ContentTypes.Rock)
			{
				if (type == MultiHolder.ContentTypes.None)
				{
					this._contentActual = 0;
				}
				if (base.entity.isOwner)
				{
					this._contentTypeActual = MultiHolder.ContentTypes.Rock;
					if (this._contentActual < this.RockRender.Length)
					{
						this._contentActual = Mathf.Min(this._contentActual + 1, this.RockRender.Length);
					}
					else
					{
						PlayerAddItem playerAddItem2 = PlayerAddItem.Create(source);
						playerAddItem2.ItemId = this.RockItemId;
						playerAddItem2.Send();
					}
					this.RefreshMassAndDrag();
				}
				else
				{
					ItemHolderAddItem itemHolderAddItem2 = ItemHolderAddItem.Create(base.entity.source);
					itemHolderAddItem2.ContentType = 3;
					itemHolderAddItem2.Target = base.entity;
					itemHolderAddItem2.Send();
				}
			}
			if ((this._contentTypeActual == MultiHolder.ContentTypes.Stick || this._contentTypeActual == MultiHolder.ContentTypes.None) && type == MultiHolder.ContentTypes.Stick)
			{
				if (type == MultiHolder.ContentTypes.None)
				{
					this._contentActual = 0;
				}
				if (base.entity.isOwner)
				{
					this._contentTypeActual = MultiHolder.ContentTypes.Stick;
					if (this._contentActual < this.StickRender.Length)
					{
						this._contentActual = Mathf.Min(this._contentActual + 1, this.StickRender.Length);
					}
					else
					{
						PlayerAddItem playerAddItem3 = PlayerAddItem.Create(source);
						playerAddItem3.ItemId = this.StickItemId;
						playerAddItem3.Send();
					}
					this.RefreshMassAndDrag();
				}
				else
				{
					ItemHolderAddItem itemHolderAddItem3 = ItemHolderAddItem.Create(base.entity.source);
					itemHolderAddItem3.ContentType = 4;
					itemHolderAddItem3.Target = base.entity;
					itemHolderAddItem3.Send();
				}
			}
		}

		
		public void ItemCountChangedMP()
		{
			this._contentAmount = base.state.LogCount;
			if (base.state.GrabbedBy)
			{
				return;
			}
			for (int i = 0; i < this.RockRender.Length; i++)
			{
				this.RockRender[i].SetActive(false);
			}
			for (int j = 0; j < this.StickRender.Length; j++)
			{
				this.StickRender[j].SetActive(false);
			}
			for (int k = 0; k < this.LogRender.Length; k++)
			{
				this.LogRender[k].SetActive(false);
			}
			if (this._contentTypeActual == MultiHolder.ContentTypes.Log)
			{
				for (int l = 0; l < this._contentActual; l++)
				{
					this.LogRender[l].SetActive(true);
				}
			}
			else if (this._contentTypeActual == MultiHolder.ContentTypes.Rock)
			{
				for (int m = 0; m < this._contentActual; m++)
				{
					this.RockRender[m].SetActive(true);
				}
			}
			else if (this._contentTypeActual == MultiHolder.ContentTypes.Stick)
			{
				for (int n = 0; n < this._contentActual; n++)
				{
					this.StickRender[n].SetActive(true);
				}
			}
		}

		
		
		
		public int _contentActual
		{
			get
			{
				if (BoltNetwork.isRunning)
				{
					return base.state.LogCount;
				}
				return this._contentAmount;
			}
			set
			{
				if (BoltNetwork.isRunning)
				{
					base.state.LogCount = value;
				}
				this._contentAmount = value;
			}
		}

		
		
		
		public MultiHolder.ContentTypes _contentTypeActual
		{
			get
			{
				return this._content;
			}
			set
			{
				if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner)
				{
					base.state.ContentType = (int)value;
				}
				this._content = value;
			}
		}

		
		public GameObject[] LogRender;

		
		public GameObject[] RockRender;

		
		public GameObject[] StickRender;

		
		public GameObject[] MutantBodySlots;

		
		public GameObject TakeIcon;

		
		public GameObject AddIcon;

		
		public GameObject TakeRockIcon;

		
		public GameObject AddRockIcon;

		
		public GameObject TakeStickIcon;

		
		public GameObject AddStickIcon;

		
		public Collider DynamicCollision;

		
		public bool Pushable;

		
		[ItemIdPicker]
		public int LogItemId;

		
		[ItemIdPicker]
		public int RockItemId;

		
		[ItemIdPicker]
		public int StickItemId;

		
		[SerializeThis]
		public MultiHolder.ContentTypes _content;

		
		[SerializeThis]
		public int _contentAmount;

		
		[SerializeThis]
		public EnemyType[] _bodyTypes;

		
		private MultiHolder.ContentTypes _addingContentType;

		
		private GameObject[] _bodies;

		
		private float _originalMass;

		
		private float _originalDrag;

		
		[SerializeField]
		private SphereCollider _sc;

		
		public enum ContentTypes
		{
			
			None,
			
			Log,
			
			Body,
			
			Rock,
			
			Stick
		}
	}
}
