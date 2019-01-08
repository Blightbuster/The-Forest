using System;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.World
{
	[DoNotSerializePublic]
	public class RepairTool : MonoBehaviour
	{
		private void Awake()
		{
			this._triggerLayers = LayerMask.NameToLayer("PickUp");
			this._colliderLayers = 36700161;
			this._iconPos = this._icon.transform.localPosition;
			this._icon.gameObject.SetActive(false);
			this._iconLogPos = this._iconLog.transform.localPosition;
			this._iconLog.gameObject.SetActive(false);
			this._pickupIcon.SetActive(false);
		}

		private void OnEnable()
		{
			if (LocalPlayer.Grabber && LocalPlayer.Grabber.OnExit != null)
			{
				Scene.HudGui.RepairMissingToolIcon.SetActive(false);
				LocalPlayer.Grabber.OnExit();
				LocalPlayer.Grabber.Clear();
				LocalPlayer.Grabber.ValidateCollider = new Func<Collider, bool>(this.ValidateCollider);
				LocalPlayer.Grabber.OnEnter = new Action(this.OnGrabberEnter);
				LocalPlayer.Grabber.OnExit = new Action(this.OnGrabberExit);
				LocalPlayer.Grabber.RefreshCollider();
				this.IsEquipped = true;
			}
		}

		private void OnDisable()
		{
			this.IsEquipped = false;
			this.GrabExit();
			if (LocalPlayer.Grabber.ValidateCollider == new Func<Collider, bool>(this.ValidateCollider))
			{
				LocalPlayer.Grabber.ResetDefaultMessages();
				LocalPlayer.Grabber.RefreshCollider();
			}
		}

		private void Update()
		{
			if (this._buildingTarget != null || this._foundationTarget)
			{
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				if (this._buildingTarget != null)
				{
					flag = (this._buildingTarget.CalcMissingRepairLogs() > 0);
					flag2 = (flag && LocalPlayer.Inventory.Owns(this._logItemId, true));
					flag3 = (this._buildingTarget.CalcMissingRepairMaterial() > 0);
				}
				if (this._foundationTarget != null)
				{
					if (!flag)
					{
						flag = (this._foundationTarget.CalcMissingRepairLogs() > 0);
						flag2 = (flag && LocalPlayer.Inventory.Logs.HasLogs);
					}
					flag3 = (flag3 || this._foundationTarget.CalcMissingRepairMaterial() > 0);
				}
				bool flag4 = flag3;
				RaycastHit raycastHit;
				if (!flag && Physics.SphereCast(LocalPlayer.MainCamTr.position, 0.1f, LocalPlayer.MainCamTr.forward, out raycastHit, 8f, this._colliderLayers.value) && !LocalPlayer.FpCharacter.Sitting)
				{
					LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("atRepairTrigger").Value = true;
					this.FocusedRepairCollider = raycastHit.collider;
				}
				else
				{
					this.FocusedRepairCollider = null;
				}
				this.UpdateLogDisplay(flag2);
				this.UpdateSapDisplay(!flag && !LocalPlayer.Inventory.Logs.HasLogs, !flag2);
				if (flag2)
				{
					if (!Scene.HudGui.AddIcon.activeSelf)
					{
						Scene.HudGui.AddIcon.SetActive(true);
					}
					if (Scene.HudGui.DropButton.activeSelf)
					{
						Scene.HudGui.DropButton.SetActive(false);
					}
					if (TheForest.Utils.Input.GetButtonDown("Take"))
					{
						if (!flag2)
						{
							Debug.LogError(string.Concat(new object[]
							{
								"Repair system error, please report (canAddLog=",
								flag2,
								", canAddRepairMat=",
								flag4,
								")"
							}));
							return;
						}
						LocalPlayer.Inventory.RemoveItem(this._logItemId, 1, false, true);
						bool isLog = true;
						LocalPlayer.Sfx.PlayWhoosh();
						if (this._foundationTarget && this._foundationTarget.CalcMissingRepairLogs() > 0)
						{
							this._foundationTarget.AddRepairMaterial(isLog);
							this.CheckFinalizeRepair();
						}
						else if (this._buildingTarget && this._buildingTarget.CalcMissingRepairLogs() > 0)
						{
							this._buildingTarget.AddRepairMaterial(isLog);
							this.CheckFinalizeRepair();
						}
					}
				}
				else
				{
					if (Scene.HudGui.AddIcon.activeSelf)
					{
						Scene.HudGui.AddIcon.SetActive(false);
					}
					if (Scene.HudGui.DropButton.activeSelf != LocalPlayer.Inventory.Logs.HasLogs)
					{
						Scene.HudGui.DropButton.SetActive(LocalPlayer.Inventory.Logs.HasLogs);
					}
				}
				this._usedFrame = flag;
			}
		}

		private bool ValidateCollider(Collider other)
		{
			if (Grabber.FocusedItem != other)
			{
				PrefabIdentifier componentInParent = other.GetComponentInParent<PrefabIdentifier>();
				if (componentInParent)
				{
					BuildingHealth component = componentInParent.GetComponent<BuildingHealth>();
					if (component && component.CanBeRepaired && (component.CalcMissingRepairMaterial() > 0 || component.CalcMissingRepairLogs() > 0) && LocalPlayer.Inventory.Logs.HasLogs == component.CalcMissingRepairLogs() > 0)
					{
						return !other.isTrigger;
					}
					FoundationHealth component2 = componentInParent.GetComponent<FoundationHealth>();
					if (component2 && component2.CanBeRepaired)
					{
						bool flag = component2.CalcMissingRepairMaterial() > 0;
						bool flag2 = component2.CalcMissingRepairLogs() > 0;
						if (!LocalPlayer.Inventory.Logs.HasLogs == flag || (LocalPlayer.Inventory.Logs.HasLogs && flag2))
						{
							return !other.isTrigger;
						}
					}
				}
			}
			return LocalPlayer.Grabber.IsValid(other);
		}

		private void OnGrabberEnter()
		{
			PrefabIdentifier componentInParent = Grabber.FocusedItemGO.GetComponentInParent<PrefabIdentifier>();
			if (componentInParent)
			{
				BuildingHealth component = componentInParent.GetComponent<BuildingHealth>();
				FoundationHealth component2 = componentInParent.GetComponent<FoundationHealth>();
				if ((component && component.CanBeRepaired && (component.CalcMissingRepairMaterial() > 0 || component.CalcMissingRepairLogs() > 0) && !LocalPlayer.Inventory.Logs.HasLogs) || (component2 && component2.CanBeRepaired && (!LocalPlayer.Inventory.Logs.HasLogs == component2.CalcMissingRepairMaterial() > 0 || (LocalPlayer.Inventory.Logs.HasLogs && component2.CalcMissingRepairLogs() > 0))))
				{
					this._buildingTarget = component;
					this._foundationTarget = component2;
					return;
				}
			}
			this.GrabExit();
			this._buildingTarget = null;
			this._foundationTarget = null;
			LocalPlayer.Grabber.EnterMessage();
		}

		private void OnGrabberExit()
		{
			if (Scene.HudGui.AddIcon.activeSelf)
			{
				Scene.HudGui.AddIcon.SetActive(false);
			}
			if (Scene.HudGui.DropButton.activeSelf != LocalPlayer.Inventory.Logs.HasLogs)
			{
				Scene.HudGui.DropButton.SetActive(LocalPlayer.Inventory.Logs.HasLogs);
			}
			if (this._buildingTarget || this._foundationTarget)
			{
				this.CheckFinalizeRepair();
				this._buildingTarget = null;
				this._foundationTarget = null;
			}
			else
			{
				LocalPlayer.Grabber.ExitMessage();
			}
			this.GrabExit();
		}

		private void OnRepairStructure(GameObject go)
		{
			if (!this._usedFrame)
			{
				if (this._buildingTarget || this._foundationTarget)
				{
					PrefabIdentifier componentInParent = Grabber.FocusedItemGO.GetComponentInParent<PrefabIdentifier>();
					if (componentInParent)
					{
						if (this._foundationTarget && this._foundationTarget.CalcMissingRepairMaterial() > 0)
						{
							if (this._foundationTarget.gameObject == componentInParent.gameObject)
							{
								this._usedFrame = true;
								this._foundationTarget.AddRepairMaterial(false);
								this.CheckFinalizeRepair();
							}
						}
						else if (this._buildingTarget && this._buildingTarget.gameObject == componentInParent.gameObject)
						{
							this._usedFrame = true;
							this._buildingTarget.AddRepairMaterial(false);
							this.CheckFinalizeRepair();
						}
						if (!this._buildingTarget && !this._foundationTarget)
						{
							this.GrabExit();
						}
					}
				}
			}
			else
			{
				this.FocusedRepairCollider = null;
			}
		}

		private void CheckFinalizeRepair()
		{
			if ((!this._buildingTarget || (this._buildingTarget.CalcMissingRepairMaterial() == 0 && this._buildingTarget.CalcMissingRepairLogs() == 0)) && (!this._foundationTarget || (this._foundationTarget.CalcMissingRepairMaterial() == 0 && this._foundationTarget.CalcMissingRepairLogs() == 0)))
			{
				if (this._foundationTarget)
				{
					this._foundationTarget.RepairMaterial = 0;
					this._foundationTarget.ResetHP();
					this._foundationTarget = null;
				}
				this._buildingTarget = null;
			}
		}

		private void GrabExit()
		{
			if (Scene.HudGui)
			{
				if (Scene.HudGui.AddIcon.activeSelf)
				{
					Scene.HudGui.AddIcon.SetActive(false);
				}
				if (LocalPlayer.Inventory && LocalPlayer.Inventory.Logs && Scene.HudGui.DropButton.activeSelf != LocalPlayer.Inventory.Logs.HasLogs)
				{
					Scene.HudGui.DropButton.SetActive(LocalPlayer.Inventory.Logs.HasLogs);
				}
				Scene.HudGui.RepairIcon.Hide();
				Scene.HudGui.RepairLogIcon.Hide();
			}
			if (!this._iconLog.transform.parent)
			{
				this._iconLog.transform.parent = base.transform;
			}
			this._iconLog.gameObject.SetActive(false);
			if (LocalPlayer.ScriptSetup && LocalPlayer.ScriptSetup.pmControl && LocalPlayer.ScriptSetup.pmControl.FsmVariables != null)
			{
				LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("atRepairTrigger").Value = false;
			}
		}

		public void UpdateLogDisplay(bool canAdd)
		{
			int num = 0;
			int num2 = 0;
			if (this._buildingTarget)
			{
				num += this._buildingTarget.RepairLogs;
				num2 += this._buildingTarget.CollapsedLogs;
			}
			if (this._foundationTarget)
			{
				num = Mathf.Max(num - this._foundationTarget.CollapsedLogs, 0);
				num += this._foundationTarget.RepairLogs;
				num2 += this._foundationTarget.CollapsedLogs;
			}
			if (num < num2)
			{
				Scene.HudGui.RepairLogIcon.Show(num, num2, canAdd);
			}
			else
			{
				Scene.HudGui.RepairLogIcon.Hide();
			}
		}

		public void UpdateSapDisplay(bool canEnable, bool canAdd)
		{
			int num = 0;
			int num2 = 0;
			if (this._buildingTarget)
			{
				num += this._buildingTarget.RepairMaterial;
				num2 += this._buildingTarget.CalcTotalRepairMaterial();
			}
			if (this._foundationTarget)
			{
				num += this._foundationTarget.RepairMaterial;
				num2 += this._foundationTarget.CalcTotalRepairMaterial();
			}
			if (canEnable && num < num2)
			{
				Scene.HudGui.RepairIcon.Show(num, num2, canAdd);
			}
			else
			{
				Scene.HudGui.RepairIcon.Hide();
			}
		}

		public bool IsEquipped { get; private set; }

		public bool IsRepairFocused
		{
			get
			{
				return this._buildingTarget || this._foundationTarget;
			}
		}

		public Collider FocusedRepairCollider { get; private set; }

		public BuildingHealth _buildingTarget;

		public FoundationHealth _foundationTarget;

		public GameObject _pickupIcon;

		public GUITexture _icon;

		public GUIText _text;

		public GUITexture _iconLog;

		public GUIText _textLog;

		public Color _white;

		public Color _red;

		[ItemIdPicker]
		public int _itemId;

		[ItemIdPicker]
		public int _logItemId;

		private int _triggerLayers = -1;

		private LayerMask _colliderLayers = -1;

		private Vector3 _iconPos;

		private Vector3 _iconLogPos;

		private int _displayedLogCount;

		private int _displayedTotalLogCount;

		private int _displayedSapCount;

		private int _displayedTotalSapCount;

		private bool _usedFrame;
	}
}
