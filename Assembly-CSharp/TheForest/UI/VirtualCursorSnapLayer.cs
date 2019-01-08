using System;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	public class VirtualCursorSnapLayer : MonoBehaviour
	{
		private void LateUpdate()
		{
			if (TheForest.Utils.Input.IsGamePad && !ForestVR.Enabled && (PlayerPreferences.UseGamepadCursorSnapping || this._ignoreSnapOption))
			{
				if (this._delayCurrentNodeRefresh)
				{
					this._delayCurrentNodeRefresh = false;
				}
				else
				{
					this.CheckCurrentNode();
					this.Navigate();
					this.ToggleComponents(false);
				}
				this._hoveredNode = null;
			}
			else
			{
				VirtualCursor.Instance.OverridingPosition = null;
				if (this._currentNode)
				{
					this.SendExitMessage(this._currentNode);
					this._currentNode = null;
				}
				this.ToggleComponents(true);
			}
		}

		private void OnEnable()
		{
			if (ForestVR.Prototype)
			{
				base.enabled = false;
				return;
			}
			if (ForestVR.Enabled)
			{
				this._currentNode = null;
			}
			if (TheForest.Utils.Input.IsGamePad && this._currentNode != null)
			{
				this.SetCurrentNode(this._currentNode);
				VirtualCursor.Instance.OverridingPosition = ((!this._currentNode) ? null : base.gameObject);
			}
		}

		private void OnDisable()
		{
			if (VirtualCursor.Instance && VirtualCursor.Instance.OverridingPosition == base.gameObject)
			{
				VirtualCursor.Instance.OverridingPosition = null;
			}
			this.ToggleComponents(true);
		}

		public virtual void RegisterNode(VirtualCursorSnapNode node)
		{
			if (node.GroupTester != null)
			{
				for (int i = 0; i < this._nodes.Count; i++)
				{
					if (this._nodes[i].GroupTester != null && this._nodes[i] != node && node.GroupTester.BelongWith(this._nodes[i].GroupTester))
					{
						this._nodes[i].GroupedNodes.Add(node);
						node.MasterGroupNode = this._nodes[i];
						return;
					}
				}
			}
			this._nodes.Add(node);
			this._delayCurrentNodeRefresh = true;
			if (node == this._currentNode)
			{
				this.SetCurrentNode(this._currentNode);
				VirtualCursor.Instance.OverridingPosition = ((!this._currentNode) ? null : base.gameObject);
			}
		}

		public virtual void UnregisterNode(VirtualCursorSnapNode node)
		{
			if (node == this._currentNode)
			{
			}
			if (node.MasterGroupNode)
			{
				node.MasterGroupNode.GroupedNodes.Remove(node);
				node.MasterGroupNode = null;
				return;
			}
			if (node.GroupedNodes != null && node.GroupedNodes.Count > 0)
			{
				VirtualCursorSnapNode virtualCursorSnapNode = node.GroupedNodes[0];
				virtualCursorSnapNode.MasterGroupNode = null;
				node.GroupedNodes.RemoveAt(0);
				for (int i = 0; i < node.GroupedNodes.Count; i++)
				{
					virtualCursorSnapNode.GroupedNodes.Add(node);
					node.MasterGroupNode = virtualCursorSnapNode;
				}
				node.GroupedNodes.Clear();
				this._nodes.Add(virtualCursorSnapNode);
			}
			this._nodes.Remove(node);
			this._delayCurrentNodeRefresh = true;
		}

		protected void CheckCurrentNode()
		{
			if (this._currentNode == null)
			{
				this.SetCurrentNode(this.GetClosestNode(base.transform.position));
			}
			else if (!this._currentNode.CanBeSnapedTo)
			{
				if (this._currentNode.MasterGroupNode && this._currentNode.MasterGroupNode.GroupedNodes.Count > 0)
				{
					this.SetCurrentNode(this._currentNode.MasterGroupNode.GroupedNodes.FindLast((VirtualCursorSnapNode n) => n != this._currentNode));
				}
				else if (this._currentNode.MasterGroupNode)
				{
					this.SetCurrentNode(this._currentNode.MasterGroupNode);
				}
				else
				{
					this.SetCurrentNode(this.GetClosestNode(base.transform.position));
				}
			}
			else
			{
				VirtualCursor.Instance.SetTargetWorldPosition(this._currentNode.Center);
			}
			VirtualCursor.Instance.OverridingPosition = ((!this._currentNode) ? null : base.gameObject);
		}

		public void SetCurrentNode(VirtualCursorSnapNode node)
		{
			if (node != null)
			{
				this.SendExitMessage(this._currentNode);
				this._currentNode = node;
				this.SendEnterMessage(this._currentNode);
				VirtualCursor.Instance.SetTargetWorldPosition(node.Center);
				if (this._actionIcon && TheForest.Utils.Input.IsGamePad && !ForestVR.Enabled)
				{
					this._actionIcon.transform.position = node.Center;
					if (!this._actionIcon.gameObject.activeSelf)
					{
						this._actionIcon.gameObject.SetActive(true);
					}
				}
			}
		}

		public void NodeHovered(VirtualCursorSnapNode node, bool on)
		{
			if (this._actionIcon && !TheForest.Utils.Input.IsGamePad)
			{
				if (on)
				{
					if (this._hoveredNode != node)
					{
						this._hoveredNode = node;
						this._actionIcon.transform.position = node.Center;
						if (!this._actionIcon.gameObject.activeSelf)
						{
							this._actionIcon.gameObject.SetActive(true);
						}
					}
				}
				else if (this._hoveredNode == node)
				{
					this._hoveredNode = null;
					if (this._actionIcon.gameObject.activeSelf)
					{
						this._actionIcon.gameObject.SetActive(false);
					}
				}
			}
		}

		private void SendEnterMessage(VirtualCursorSnapNode node)
		{
			if (node != null && !string.IsNullOrEmpty(this._onTargetEnterMessage))
			{
				node.SendMessage(this._onTargetEnterMessage, SendMessageOptions.DontRequireReceiver);
			}
		}

		private void SendExitMessage(VirtualCursorSnapNode node)
		{
			if (node != null && !string.IsNullOrEmpty(this._onTargetExitMessage))
			{
				node.SendMessage(this._onTargetExitMessage, SendMessageOptions.DontRequireReceiver);
			}
		}

		public virtual void Navigate()
		{
			if (this._nextSnap < Time.realtimeSinceStartup)
			{
				float num = (!TheForest.Utils.Input.IsGamePad) ? (-TheForest.Utils.Input.GetAxis("Mouse X")) : (-TheForest.Utils.Input.GetAxis("Horizontal"));
				float num2 = (!TheForest.Utils.Input.IsGamePad) ? TheForest.Utils.Input.GetAxis("Mouse Y") : TheForest.Utils.Input.GetAxis("Vertical");
				if (Mathf.Abs(num) < 0.1f)
				{
					num = 0f;
				}
				if (Mathf.Abs(num2) < 0.1f)
				{
					num2 = 0f;
				}
				if (!Mathf.Approximately(num, 0f) || !Mathf.Approximately(num2, 0f))
				{
					VirtualCursorSnapNode nodeAtDir = this.GetNodeAtDir(new Vector3(0f, num2, num));
					this.SetCurrentNode(nodeAtDir);
					if (nodeAtDir)
					{
						if (this._speedUp)
						{
							this._nextSnap = Time.realtimeSinceStartup + this._snapDelay / 2f;
						}
						else
						{
							this._nextSnap = Time.realtimeSinceStartup + this._snapDelay;
							if (this._canSpeedUp)
							{
								this._speedUp = true;
							}
							else
							{
								this._canSpeedUp = true;
							}
						}
					}
				}
				else
				{
					this._canSpeedUp = false;
					this._speedUp = false;
				}
			}
		}

		protected Vector3 FlattenPosition(Vector3 worldPos)
		{
			if (this._flattenPositions)
			{
				Vector3 vector = base.transform.InverseTransformPoint(worldPos);
				vector = Vector3.Scale(vector, new Vector3(0f, 1f, 1f));
				return base.transform.TransformPoint(vector);
			}
			return worldPos;
		}

		public virtual VirtualCursorSnapNode GetNodeAtDir(Vector3 myDir)
		{
			Transform transform = base.transform;
			myDir.Normalize();
			bool flag = Mathf.Abs(Mathf.Abs(myDir.y) - Mathf.Abs(myDir.z)) < 0.25f;
			bool flag2 = Mathf.Abs(myDir.y) < Mathf.Abs(myDir.z);
			float num = (!flag && !flag2) ? 2f : 1f;
			float num2 = (!flag && flag2) ? 2f : 1f;
			myDir = transform.TransformDirection(myDir);
			Vector3 b = (!(this._currentNode != null)) ? base.transform.position : this.FlattenPosition(this._currentNode.Center);
			float num3 = float.MaxValue;
			VirtualCursorSnapNode result = null;
			for (int i = 0; i < this._nodes.Count; i++)
			{
				VirtualCursorSnapNode virtualCursorSnapNode;
				if (this._nodes[i].GroupedNodes != null && this._nodes[i].GroupedNodes.Count > 0)
				{
					virtualCursorSnapNode = this._nodes[i].GroupedNodes[this._nodes[i].GroupedNodes.Count - 1];
				}
				else
				{
					virtualCursorSnapNode = this._nodes[i];
				}
				if (virtualCursorSnapNode.CanBeSnapedTo)
				{
					Vector3 direction = this.FlattenPosition(virtualCursorSnapNode.Center) - b;
					float num4 = Vector3.Dot(myDir, direction.normalized);
					if (num4 >= 0.5f)
					{
						direction = transform.InverseTransformDirection(direction);
						direction.x *= num + (1f - num4) * 2.5f;
						direction.y *= num2 + (1f - num4) * 2.5f;
						float sqrMagnitude = direction.sqrMagnitude;
						if (sqrMagnitude <= num3)
						{
							result = virtualCursorSnapNode;
							num3 = sqrMagnitude;
						}
					}
				}
			}
			return result;
		}

		public VirtualCursorSnapNode GetClosestNode(Vector3 pos)
		{
			Transform transform = base.transform;
			pos = transform.TransformDirection(pos);
			Vector3 b = (!(this._currentNode != null)) ? base.transform.position : this._currentNode.Center;
			float num = float.MaxValue;
			VirtualCursorSnapNode result = null;
			for (int i = 0; i < this._nodes.Count; i++)
			{
				VirtualCursorSnapNode virtualCursorSnapNode = this._nodes[i];
				if (virtualCursorSnapNode.CanBeSnapedTo)
				{
					float num2 = Vector3.Distance(virtualCursorSnapNode.Center, b);
					if (num2 <= num)
					{
						result = virtualCursorSnapNode;
						num = num2;
					}
				}
			}
			return result;
		}

		private void ToggleComponents(bool enable)
		{
			foreach (Behaviour behaviour in this._disableWhenActive)
			{
				if (behaviour && behaviour.enabled != enable)
				{
					behaviour.enabled = enable;
				}
			}
		}

		public bool _ignoreSnapOption;

		public bool _flattenPositions;

		public List<VirtualCursorSnapNode> _nodes;

		public float _snapDelay = 0.3f;

		public string _onTargetEnterMessage;

		public string _onTargetExitMessage;

		public Behaviour[] _disableWhenActive;

		public ActionIconWorld _actionIcon;

		protected VirtualCursorSnapNode _currentNode;

		protected VirtualCursorSnapNode _hoveredNode;

		protected float _nextSnap;

		protected bool _canSpeedUp;

		protected bool _speedUp;

		protected bool _delayCurrentNodeRefresh;
	}
}
