using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem
{
	
	public class ControllerButtonHints : MonoBehaviour
	{
		
		
		
		public bool initialized { get; private set; }

		
		private void Awake()
		{
			this.renderModelLoadedAction = SteamVR_Events.RenderModelLoadedAction(new UnityAction<SteamVR_RenderModel, bool>(this.OnRenderModelLoaded));
			this.colorID = Shader.PropertyToID("_Color");
		}

		
		private void Start()
		{
			this.player = Player.instance;
		}

		
		private void HintDebugLog(string msg)
		{
			if (this.debugHints)
			{
				Debug.Log("Hints: " + msg);
			}
		}

		
		private void OnEnable()
		{
			this.renderModelLoadedAction.enabled = true;
		}

		
		private void OnDisable()
		{
			this.renderModelLoadedAction.enabled = false;
			this.Clear();
		}

		
		private void OnParentHandInputFocusLost()
		{
			this.HideAllButtonHints();
			this.HideAllText();
		}

		
		private void OnHandInitialized(int deviceIndex)
		{
			this.renderModel = new GameObject("SteamVR_RenderModel").AddComponent<SteamVR_RenderModel>();
			this.renderModel.transform.parent = base.transform;
			this.renderModel.transform.localPosition = Vector3.zero;
			this.renderModel.transform.localRotation = Quaternion.identity;
			this.renderModel.transform.localScale = Vector3.one;
			this.renderModel.SetDeviceIndex(deviceIndex);
			if (!this.initialized)
			{
				this.renderModel.gameObject.SetActive(true);
			}
		}

		
		private void OnRenderModelLoaded(SteamVR_RenderModel renderModel, bool succeess)
		{
			if (renderModel == this.renderModel)
			{
				this.textHintParent = new GameObject("Text Hints").transform;
				this.textHintParent.SetParent(base.transform);
				this.textHintParent.localPosition = Vector3.zero;
				this.textHintParent.localRotation = Quaternion.identity;
				this.textHintParent.localScale = Vector3.one;
				using (SteamVR_RenderModel.RenderModelInterfaceHolder renderModelInterfaceHolder = new SteamVR_RenderModel.RenderModelInterfaceHolder())
				{
					CVRRenderModels instance = renderModelInterfaceHolder.instance;
					if (instance != null)
					{
						string text = "Components for render model " + renderModel.index;
						IEnumerator enumerator = renderModel.transform.GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								object obj = enumerator.Current;
								Transform transform = (Transform)obj;
								ulong componentButtonMask = instance.GetComponentButtonMask(renderModel.renderModelName, transform.name);
								this.componentButtonMasks.Add(new KeyValuePair<string, ulong>(transform.name, componentButtonMask));
								string text2 = text;
								text = string.Concat(new object[]
								{
									text2,
									"\n\t",
									transform.name,
									": ",
									componentButtonMask
								});
							}
						}
						finally
						{
							IDisposable disposable;
							if ((disposable = (enumerator as IDisposable)) != null)
							{
								disposable.Dispose();
							}
						}
						this.HintDebugLog(text);
					}
				}
				this.buttonHintInfos = new Dictionary<EVRButtonId, ControllerButtonHints.ButtonHintInfo>();
				this.CreateAndAddButtonInfo(EVRButtonId.k_EButton_Axis1);
				this.CreateAndAddButtonInfo(EVRButtonId.k_EButton_ApplicationMenu);
				this.CreateAndAddButtonInfo(EVRButtonId.k_EButton_System);
				this.CreateAndAddButtonInfo(EVRButtonId.k_EButton_Grip);
				this.CreateAndAddButtonInfo(EVRButtonId.k_EButton_Axis0);
				this.CreateAndAddButtonInfo(EVRButtonId.k_EButton_A);
				this.ComputeTextEndTransforms();
				this.initialized = true;
				renderModel.gameObject.SetActive(false);
			}
		}

		
		private void CreateAndAddButtonInfo(EVRButtonId buttonID)
		{
			Transform transform = null;
			List<MeshRenderer> list = new List<MeshRenderer>();
			string text = "Looking for button: " + buttonID;
			EVRButtonId evrbuttonId = buttonID;
			if (buttonID == EVRButtonId.k_EButton_Grip && SteamVR.instance.hmd_TrackingSystemName.ToLowerInvariant().Contains("oculus"))
			{
				evrbuttonId = EVRButtonId.k_EButton_Axis2;
			}
			ulong num = 1UL << (int)evrbuttonId;
			string text2;
			foreach (KeyValuePair<string, ulong> keyValuePair in this.componentButtonMasks)
			{
				if ((keyValuePair.Value & num) == num)
				{
					text2 = text;
					text = string.Concat(new object[]
					{
						text2,
						"\nFound component: ",
						keyValuePair.Key,
						" ",
						keyValuePair.Value
					});
					Transform transform2 = this.renderModel.FindComponent(keyValuePair.Key);
					transform = transform2;
					text2 = text;
					text = string.Concat(new object[]
					{
						text2,
						"\nFound componentTransform: ",
						transform2,
						" buttonTransform: ",
						transform
					});
					list.AddRange(transform2.GetComponentsInChildren<MeshRenderer>());
				}
			}
			text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"\nFound ",
				list.Count,
				" renderers for ",
				buttonID
			});
			foreach (MeshRenderer meshRenderer in list)
			{
				text = text + "\n\t" + meshRenderer.name;
			}
			this.HintDebugLog(text);
			if (transform == null)
			{
				this.HintDebugLog("Couldn't find buttonTransform for " + buttonID);
				return;
			}
			ControllerButtonHints.ButtonHintInfo buttonHintInfo = new ControllerButtonHints.ButtonHintInfo();
			this.buttonHintInfos.Add(buttonID, buttonHintInfo);
			buttonHintInfo.componentName = transform.name;
			buttonHintInfo.renderers = list;
			buttonHintInfo.localTransform = transform.Find("attach");
			ControllerButtonHints.OffsetType offsetType = ControllerButtonHints.OffsetType.Right;
			switch (buttonID)
			{
			case EVRButtonId.k_EButton_System:
				offsetType = ControllerButtonHints.OffsetType.Right;
				break;
			case EVRButtonId.k_EButton_ApplicationMenu:
				offsetType = ControllerButtonHints.OffsetType.Right;
				break;
			case EVRButtonId.k_EButton_Grip:
				offsetType = ControllerButtonHints.OffsetType.Forward;
				break;
			default:
				if (buttonID != EVRButtonId.k_EButton_Axis0)
				{
					if (buttonID == EVRButtonId.k_EButton_Axis1)
					{
						offsetType = ControllerButtonHints.OffsetType.Right;
					}
				}
				else
				{
					offsetType = ControllerButtonHints.OffsetType.Up;
				}
				break;
			}
			switch (offsetType)
			{
			case ControllerButtonHints.OffsetType.Up:
				buttonHintInfo.textEndOffsetDir = buttonHintInfo.localTransform.up;
				break;
			case ControllerButtonHints.OffsetType.Right:
				buttonHintInfo.textEndOffsetDir = buttonHintInfo.localTransform.right;
				break;
			case ControllerButtonHints.OffsetType.Forward:
				buttonHintInfo.textEndOffsetDir = buttonHintInfo.localTransform.forward;
				break;
			case ControllerButtonHints.OffsetType.Back:
				buttonHintInfo.textEndOffsetDir = -buttonHintInfo.localTransform.forward;
				break;
			}
			Vector3 position = buttonHintInfo.localTransform.position + buttonHintInfo.localTransform.forward * 0.01f;
			buttonHintInfo.textHintObject = UnityEngine.Object.Instantiate<GameObject>(this.textHintPrefab, position, Quaternion.identity);
			buttonHintInfo.textHintObject.name = "Hint_" + buttonHintInfo.componentName + "_Start";
			buttonHintInfo.textHintObject.transform.SetParent(this.textHintParent);
			buttonHintInfo.textHintObject.layer = base.gameObject.layer;
			buttonHintInfo.textHintObject.tag = base.gameObject.tag;
			buttonHintInfo.textStartAnchor = buttonHintInfo.textHintObject.transform.Find("Start");
			buttonHintInfo.textEndAnchor = buttonHintInfo.textHintObject.transform.Find("End");
			buttonHintInfo.canvasOffset = buttonHintInfo.textHintObject.transform.Find("CanvasOffset");
			buttonHintInfo.line = buttonHintInfo.textHintObject.transform.Find("Line").GetComponent<LineRenderer>();
			buttonHintInfo.textCanvas = buttonHintInfo.textHintObject.GetComponentInChildren<Canvas>();
			buttonHintInfo.text = buttonHintInfo.textCanvas.GetComponentInChildren<Text>();
			buttonHintInfo.textMesh = buttonHintInfo.textCanvas.GetComponentInChildren<TextMesh>();
			buttonHintInfo.textHintObject.SetActive(false);
			buttonHintInfo.textStartAnchor.position = position;
			if (buttonHintInfo.text != null)
			{
				buttonHintInfo.text.text = buttonHintInfo.componentName;
			}
			if (buttonHintInfo.textMesh != null)
			{
				buttonHintInfo.textMesh.text = buttonHintInfo.componentName;
			}
			this.centerPosition += buttonHintInfo.textStartAnchor.position;
			buttonHintInfo.textCanvas.transform.localScale = Vector3.Scale(buttonHintInfo.textCanvas.transform.localScale, this.player.transform.localScale);
			buttonHintInfo.textStartAnchor.transform.localScale = Vector3.Scale(buttonHintInfo.textStartAnchor.transform.localScale, this.player.transform.localScale);
			buttonHintInfo.textEndAnchor.transform.localScale = Vector3.Scale(buttonHintInfo.textEndAnchor.transform.localScale, this.player.transform.localScale);
			buttonHintInfo.line.transform.localScale = Vector3.Scale(buttonHintInfo.line.transform.localScale, this.player.transform.localScale);
		}

		
		private void ComputeTextEndTransforms()
		{
			this.centerPosition /= (float)this.buttonHintInfos.Count;
			float num = 0f;
			foreach (KeyValuePair<EVRButtonId, ControllerButtonHints.ButtonHintInfo> keyValuePair in this.buttonHintInfos)
			{
				keyValuePair.Value.distanceFromCenter = Vector3.Distance(keyValuePair.Value.textStartAnchor.position, this.centerPosition);
				if (keyValuePair.Value.distanceFromCenter > num)
				{
					num = keyValuePair.Value.distanceFromCenter;
				}
			}
			foreach (KeyValuePair<EVRButtonId, ControllerButtonHints.ButtonHintInfo> keyValuePair2 in this.buttonHintInfos)
			{
				Vector3 vector = keyValuePair2.Value.textStartAnchor.position - this.centerPosition;
				vector.Normalize();
				vector = Vector3.Project(vector, this.renderModel.transform.forward);
				float num2 = keyValuePair2.Value.distanceFromCenter / num;
				float d = keyValuePair2.Value.distanceFromCenter * Mathf.Pow(2f, 10f * (num2 - 1f)) * 20f;
				float d2 = 0.1f;
				Vector3 position = keyValuePair2.Value.textStartAnchor.position + keyValuePair2.Value.textEndOffsetDir * d2 + vector * d * 0.1f;
				keyValuePair2.Value.textEndAnchor.position = position;
				keyValuePair2.Value.canvasOffset.position = position;
				keyValuePair2.Value.canvasOffset.localRotation = Quaternion.identity;
			}
		}

		
		private void ShowButtonHint(params EVRButtonId[] buttons)
		{
			this.renderModel.gameObject.SetActive(true);
			this.renderModel.GetComponentsInChildren<MeshRenderer>(this.renderers);
			for (int i = 0; i < this.renderers.Count; i++)
			{
				Texture mainTexture = this.renderers[i].material.mainTexture;
				this.renderers[i].sharedMaterial = this.controllerMaterial;
				this.renderers[i].material.mainTexture = mainTexture;
				this.renderers[i].material.renderQueue = this.controllerMaterial.shader.renderQueue;
			}
			for (int j = 0; j < buttons.Length; j++)
			{
				if (this.buttonHintInfos.ContainsKey(buttons[j]))
				{
					ControllerButtonHints.ButtonHintInfo buttonHintInfo = this.buttonHintInfos[buttons[j]];
					foreach (MeshRenderer item in buttonHintInfo.renderers)
					{
						if (!this.flashingRenderers.Contains(item))
						{
							this.flashingRenderers.Add(item);
						}
					}
				}
			}
			this.startTime = Time.realtimeSinceStartup;
			this.tickCount = 0f;
		}

		
		private void HideAllButtonHints()
		{
			this.Clear();
			this.renderModel.gameObject.SetActive(false);
		}

		
		private void HideButtonHint(params EVRButtonId[] buttons)
		{
			Color color = this.controllerMaterial.GetColor(this.colorID);
			for (int i = 0; i < buttons.Length; i++)
			{
				if (this.buttonHintInfos.ContainsKey(buttons[i]))
				{
					ControllerButtonHints.ButtonHintInfo buttonHintInfo = this.buttonHintInfos[buttons[i]];
					foreach (MeshRenderer meshRenderer in buttonHintInfo.renderers)
					{
						meshRenderer.material.color = color;
						this.flashingRenderers.Remove(meshRenderer);
					}
				}
			}
			if (this.flashingRenderers.Count == 0)
			{
				this.renderModel.gameObject.SetActive(false);
			}
		}

		
		private bool IsButtonHintActive(EVRButtonId button)
		{
			if (this.buttonHintInfos.ContainsKey(button))
			{
				ControllerButtonHints.ButtonHintInfo buttonHintInfo = this.buttonHintInfos[button];
				foreach (MeshRenderer item in buttonHintInfo.renderers)
				{
					if (this.flashingRenderers.Contains(item))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		
		private IEnumerator TestButtonHints()
		{
			for (;;)
			{
				this.ShowButtonHint(new EVRButtonId[]
				{
					EVRButtonId.k_EButton_Axis1
				});
				yield return new WaitForSeconds(1f);
				this.ShowButtonHint(new EVRButtonId[]
				{
					EVRButtonId.k_EButton_ApplicationMenu
				});
				yield return new WaitForSeconds(1f);
				this.ShowButtonHint(new EVRButtonId[1]);
				yield return new WaitForSeconds(1f);
				this.ShowButtonHint(new EVRButtonId[]
				{
					EVRButtonId.k_EButton_Grip
				});
				yield return new WaitForSeconds(1f);
				this.ShowButtonHint(new EVRButtonId[]
				{
					EVRButtonId.k_EButton_Axis0
				});
				yield return new WaitForSeconds(1f);
			}
			yield break;
		}

		
		private IEnumerator TestTextHints()
		{
			for (;;)
			{
				this.ShowText(EVRButtonId.k_EButton_Axis1, "Trigger", true);
				yield return new WaitForSeconds(3f);
				this.ShowText(EVRButtonId.k_EButton_ApplicationMenu, "Application", true);
				yield return new WaitForSeconds(3f);
				this.ShowText(EVRButtonId.k_EButton_System, "System", true);
				yield return new WaitForSeconds(3f);
				this.ShowText(EVRButtonId.k_EButton_Grip, "Grip", true);
				yield return new WaitForSeconds(3f);
				this.ShowText(EVRButtonId.k_EButton_Axis0, "Touchpad", true);
				yield return new WaitForSeconds(3f);
				this.HideAllText();
				yield return new WaitForSeconds(3f);
			}
			yield break;
		}

		
		private void Update()
		{
			if (this.renderModel != null && this.renderModel.gameObject.activeInHierarchy && this.flashingRenderers.Count > 0)
			{
				Color color = this.controllerMaterial.GetColor(this.colorID);
				float num = (Time.realtimeSinceStartup - this.startTime) * 3.14159274f * 2f;
				num = Mathf.Cos(num);
				num = Util.RemapNumberClamped(num, -1f, 1f, 0f, 1f);
				float num2 = Time.realtimeSinceStartup - this.startTime;
				if (num2 - this.tickCount > 1f)
				{
					this.tickCount += 1f;
					SteamVR_Controller.Device device = SteamVR_Controller.Input((int)this.renderModel.index);
					if (device != null)
					{
						device.TriggerHapticPulse(500, EVRButtonId.k_EButton_Axis0);
					}
				}
				for (int i = 0; i < this.flashingRenderers.Count; i++)
				{
					Renderer renderer = this.flashingRenderers[i];
					renderer.material.SetColor(this.colorID, Color.Lerp(color, this.flashColor, num));
				}
				if (this.initialized)
				{
					foreach (KeyValuePair<EVRButtonId, ControllerButtonHints.ButtonHintInfo> keyValuePair in this.buttonHintInfos)
					{
						if (keyValuePair.Value.textHintActive)
						{
							this.UpdateTextHint(keyValuePair.Value);
						}
					}
				}
			}
		}

		
		private void UpdateTextHint(ControllerButtonHints.ButtonHintInfo hintInfo)
		{
			Transform hmdTransform = this.player.hmdTransform;
			Vector3 forward = hmdTransform.position - hintInfo.canvasOffset.position;
			Quaternion a = Quaternion.LookRotation(forward, Vector3.up);
			Quaternion b = Quaternion.LookRotation(forward, hmdTransform.up);
			float t;
			if (hmdTransform.forward.y > 0f)
			{
				t = Util.RemapNumberClamped(hmdTransform.forward.y, 0.6f, 0.4f, 1f, 0f);
			}
			else
			{
				t = Util.RemapNumberClamped(hmdTransform.forward.y, -0.8f, -0.6f, 1f, 0f);
			}
			hintInfo.canvasOffset.rotation = Quaternion.Slerp(a, b, t);
			Transform transform = hintInfo.line.transform;
			hintInfo.line.useWorldSpace = false;
			hintInfo.line.SetPosition(0, transform.InverseTransformPoint(hintInfo.textStartAnchor.position));
			hintInfo.line.SetPosition(1, transform.InverseTransformPoint(hintInfo.textEndAnchor.position));
		}

		
		private void Clear()
		{
			this.renderers.Clear();
			this.flashingRenderers.Clear();
		}

		
		private void ShowText(EVRButtonId button, string text, bool highlightButton = true)
		{
			if (this.buttonHintInfos.ContainsKey(button))
			{
				ControllerButtonHints.ButtonHintInfo buttonHintInfo = this.buttonHintInfos[button];
				buttonHintInfo.textHintObject.SetActive(true);
				buttonHintInfo.textHintActive = true;
				if (buttonHintInfo.text != null)
				{
					buttonHintInfo.text.text = text;
				}
				if (buttonHintInfo.textMesh != null)
				{
					buttonHintInfo.textMesh.text = text;
				}
				this.UpdateTextHint(buttonHintInfo);
				if (highlightButton)
				{
					this.ShowButtonHint(new EVRButtonId[]
					{
						button
					});
				}
				this.renderModel.gameObject.SetActive(true);
			}
		}

		
		private void HideText(EVRButtonId button)
		{
			if (this.buttonHintInfos.ContainsKey(button))
			{
				ControllerButtonHints.ButtonHintInfo buttonHintInfo = this.buttonHintInfos[button];
				buttonHintInfo.textHintObject.SetActive(false);
				buttonHintInfo.textHintActive = false;
				this.HideButtonHint(new EVRButtonId[]
				{
					button
				});
			}
		}

		
		private void HideAllText()
		{
			foreach (KeyValuePair<EVRButtonId, ControllerButtonHints.ButtonHintInfo> keyValuePair in this.buttonHintInfos)
			{
				keyValuePair.Value.textHintObject.SetActive(false);
				keyValuePair.Value.textHintActive = false;
			}
			this.HideAllButtonHints();
		}

		
		private string GetActiveHintText(EVRButtonId button)
		{
			if (this.buttonHintInfos.ContainsKey(button))
			{
				ControllerButtonHints.ButtonHintInfo buttonHintInfo = this.buttonHintInfos[button];
				if (buttonHintInfo.textHintActive)
				{
					return buttonHintInfo.text.text;
				}
			}
			return string.Empty;
		}

		
		private static ControllerButtonHints GetControllerButtonHints(Hand hand)
		{
			if (hand != null)
			{
				ControllerButtonHints componentInChildren = hand.GetComponentInChildren<ControllerButtonHints>();
				if (componentInChildren != null && componentInChildren.initialized)
				{
					return componentInChildren;
				}
			}
			return null;
		}

		
		public static void ShowButtonHint(Hand hand, params EVRButtonId[] buttons)
		{
			ControllerButtonHints controllerButtonHints = ControllerButtonHints.GetControllerButtonHints(hand);
			if (controllerButtonHints != null)
			{
				controllerButtonHints.ShowButtonHint(buttons);
			}
		}

		
		public static void HideButtonHint(Hand hand, params EVRButtonId[] buttons)
		{
			ControllerButtonHints controllerButtonHints = ControllerButtonHints.GetControllerButtonHints(hand);
			if (controllerButtonHints != null)
			{
				controllerButtonHints.HideButtonHint(buttons);
			}
		}

		
		public static void HideAllButtonHints(Hand hand)
		{
			ControllerButtonHints controllerButtonHints = ControllerButtonHints.GetControllerButtonHints(hand);
			if (controllerButtonHints != null)
			{
				controllerButtonHints.HideAllButtonHints();
			}
		}

		
		public static bool IsButtonHintActive(Hand hand, EVRButtonId button)
		{
			ControllerButtonHints controllerButtonHints = ControllerButtonHints.GetControllerButtonHints(hand);
			return controllerButtonHints != null && controllerButtonHints.IsButtonHintActive(button);
		}

		
		public static void ShowTextHint(Hand hand, EVRButtonId button, string text, bool highlightButton = true)
		{
			ControllerButtonHints controllerButtonHints = ControllerButtonHints.GetControllerButtonHints(hand);
			if (controllerButtonHints != null)
			{
				controllerButtonHints.ShowText(button, text, highlightButton);
			}
		}

		
		public static void HideTextHint(Hand hand, EVRButtonId button)
		{
			ControllerButtonHints controllerButtonHints = ControllerButtonHints.GetControllerButtonHints(hand);
			if (controllerButtonHints != null)
			{
				controllerButtonHints.HideText(button);
			}
		}

		
		public static void HideAllTextHints(Hand hand)
		{
			ControllerButtonHints controllerButtonHints = ControllerButtonHints.GetControllerButtonHints(hand);
			if (controllerButtonHints != null)
			{
				controllerButtonHints.HideAllText();
			}
		}

		
		public static string GetActiveHintText(Hand hand, EVRButtonId button)
		{
			ControllerButtonHints controllerButtonHints = ControllerButtonHints.GetControllerButtonHints(hand);
			if (controllerButtonHints != null)
			{
				return controllerButtonHints.GetActiveHintText(button);
			}
			return string.Empty;
		}

		
		public Material controllerMaterial;

		
		public Color flashColor = new Color(1f, 0.557f, 0f);

		
		public GameObject textHintPrefab;

		
		[Header("Debug")]
		public bool debugHints;

		
		private SteamVR_RenderModel renderModel;

		
		private Player player;

		
		private List<MeshRenderer> renderers = new List<MeshRenderer>();

		
		private List<MeshRenderer> flashingRenderers = new List<MeshRenderer>();

		
		private float startTime;

		
		private float tickCount;

		
		private Dictionary<EVRButtonId, ControllerButtonHints.ButtonHintInfo> buttonHintInfos;

		
		private Transform textHintParent;

		
		private List<KeyValuePair<string, ulong>> componentButtonMasks = new List<KeyValuePair<string, ulong>>();

		
		private int colorID;

		
		private Vector3 centerPosition = Vector3.zero;

		
		private SteamVR_Events.Action renderModelLoadedAction;

		
		private enum OffsetType
		{
			
			Up,
			
			Right,
			
			Forward,
			
			Back
		}

		
		private class ButtonHintInfo
		{
			
			public string componentName;

			
			public List<MeshRenderer> renderers;

			
			public Transform localTransform;

			
			public GameObject textHintObject;

			
			public Transform textStartAnchor;

			
			public Transform textEndAnchor;

			
			public Vector3 textEndOffsetDir;

			
			public Transform canvasOffset;

			
			public Text text;

			
			public TextMesh textMesh;

			
			public Canvas textCanvas;

			
			public LineRenderer line;

			
			public float distanceFromCenter;

			
			public bool textHintActive;
		}
	}
}
