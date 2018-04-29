using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;


public class OVRDebugInfo : MonoBehaviour
{
	
	private void Awake()
	{
		this.debugUIManager = new GameObject();
		this.debugUIManager.name = "DebugUIManager";
		this.debugUIManager.transform.parent = GameObject.Find("LeftEyeAnchor").transform;
		RectTransform rectTransform = this.debugUIManager.AddComponent<RectTransform>();
		rectTransform.sizeDelta = new Vector2(100f, 100f);
		rectTransform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
		rectTransform.localPosition = new Vector3(0.01f, 0.17f, 0.53f);
		rectTransform.localEulerAngles = Vector3.zero;
		Canvas canvas = this.debugUIManager.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.WorldSpace;
		canvas.pixelPerfect = false;
	}

	
	private void Update()
	{
		if (this.initUIComponent && !this.isInited)
		{
			this.InitUIComponents();
		}
		if (Input.GetKeyDown(KeyCode.Space) && this.riftPresentTimeout < 0f)
		{
			this.initUIComponent = true;
			this.showVRVars ^= true;
		}
		this.UpdateDeviceDetection();
		if (this.showVRVars)
		{
			this.debugUIManager.SetActive(true);
			this.UpdateVariable();
			this.UpdateStrings();
		}
		else
		{
			this.debugUIManager.SetActive(false);
		}
	}

	
	private void OnDestroy()
	{
		this.isInited = false;
	}

	
	private void InitUIComponents()
	{
		float num = 0f;
		int fontSize = 20;
		this.debugUIObject = new GameObject();
		this.debugUIObject.name = "DebugInfo";
		this.debugUIObject.transform.parent = GameObject.Find("DebugUIManager").transform;
		this.debugUIObject.transform.localPosition = new Vector3(0f, 100f, 0f);
		this.debugUIObject.transform.localEulerAngles = Vector3.zero;
		this.debugUIObject.transform.localScale = new Vector3(1f, 1f, 1f);
		if (!string.IsNullOrEmpty(this.strFPS))
		{
			this.fps = this.VariableObjectManager(this.fps, "FPS", num -= this.offsetY, this.strFPS, fontSize);
		}
		if (!string.IsNullOrEmpty(this.strIPD))
		{
			this.ipd = this.VariableObjectManager(this.ipd, "IPD", num -= this.offsetY, this.strIPD, fontSize);
		}
		if (!string.IsNullOrEmpty(this.strFOV))
		{
			this.fov = this.VariableObjectManager(this.fov, "FOV", num -= this.offsetY, this.strFOV, fontSize);
		}
		if (!string.IsNullOrEmpty(this.strHeight))
		{
			this.height = this.VariableObjectManager(this.height, "Height", num -= this.offsetY, this.strHeight, fontSize);
		}
		if (!string.IsNullOrEmpty(this.strDepth))
		{
			this.depth = this.VariableObjectManager(this.depth, "Depth", num -= this.offsetY, this.strDepth, fontSize);
		}
		if (!string.IsNullOrEmpty(this.strResolutionEyeTexture))
		{
			this.resolutionEyeTexture = this.VariableObjectManager(this.resolutionEyeTexture, "Resolution", num -= this.offsetY, this.strResolutionEyeTexture, fontSize);
		}
		if (!string.IsNullOrEmpty(this.strLatencies))
		{
			this.latencies = this.VariableObjectManager(this.latencies, "Latency", num - this.offsetY, this.strLatencies, 17);
		}
		this.initUIComponent = false;
		this.isInited = true;
	}

	
	private void UpdateVariable()
	{
		this.UpdateIPD();
		this.UpdateEyeHeightOffset();
		this.UpdateEyeDepthOffset();
		this.UpdateFOV();
		this.UpdateResolutionEyeTexture();
		this.UpdateLatencyValues();
		this.UpdateFPS();
	}

	
	private void UpdateStrings()
	{
		if (this.debugUIObject == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(this.strFPS))
		{
			this.fps.GetComponentInChildren<Text>().text = this.strFPS;
		}
		if (!string.IsNullOrEmpty(this.strIPD))
		{
			this.ipd.GetComponentInChildren<Text>().text = this.strIPD;
		}
		if (!string.IsNullOrEmpty(this.strFOV))
		{
			this.fov.GetComponentInChildren<Text>().text = this.strFOV;
		}
		if (!string.IsNullOrEmpty(this.strResolutionEyeTexture))
		{
			this.resolutionEyeTexture.GetComponentInChildren<Text>().text = this.strResolutionEyeTexture;
		}
		if (!string.IsNullOrEmpty(this.strLatencies))
		{
			this.latencies.GetComponentInChildren<Text>().text = this.strLatencies;
			this.latencies.GetComponentInChildren<Text>().fontSize = 14;
		}
		if (!string.IsNullOrEmpty(this.strHeight))
		{
			this.height.GetComponentInChildren<Text>().text = this.strHeight;
		}
		if (!string.IsNullOrEmpty(this.strDepth))
		{
			this.depth.GetComponentInChildren<Text>().text = this.strDepth;
		}
	}

	
	private void RiftPresentGUI(GameObject guiMainOBj)
	{
		this.riftPresent = this.ComponentComposition(this.riftPresent);
		this.riftPresent.transform.SetParent(guiMainOBj.transform);
		this.riftPresent.name = "RiftPresent";
		RectTransform component = this.riftPresent.GetComponent<RectTransform>();
		component.localPosition = new Vector3(0f, 0f, 0f);
		component.localScale = new Vector3(1f, 1f, 1f);
		component.localEulerAngles = Vector3.zero;
		Text componentInChildren = this.riftPresent.GetComponentInChildren<Text>();
		componentInChildren.text = this.strRiftPresent;
		componentInChildren.fontSize = 20;
	}

	
	private void UpdateDeviceDetection()
	{
		if (this.riftPresentTimeout >= 0f)
		{
			this.riftPresentTimeout -= Time.deltaTime;
		}
	}

	
	private GameObject VariableObjectManager(GameObject gameObject, string name, float posY, string str, int fontSize)
	{
		gameObject = this.ComponentComposition(gameObject);
		gameObject.name = name;
		gameObject.transform.SetParent(this.debugUIObject.transform);
		RectTransform component = gameObject.GetComponent<RectTransform>();
		component.localPosition = new Vector3(0f, posY -= this.offsetY, 0f);
		Text componentInChildren = gameObject.GetComponentInChildren<Text>();
		componentInChildren.text = str;
		componentInChildren.fontSize = fontSize;
		gameObject.transform.localEulerAngles = Vector3.zero;
		component.localScale = new Vector3(1f, 1f, 1f);
		return gameObject;
	}

	
	private GameObject ComponentComposition(GameObject GO)
	{
		GO = new GameObject();
		GO.AddComponent<RectTransform>();
		GO.AddComponent<CanvasRenderer>();
		GO.AddComponent<Image>();
		GO.GetComponent<RectTransform>().sizeDelta = new Vector2(350f, 50f);
		GO.GetComponent<Image>().color = new Color(0.02745098f, 0.1764706f, 0.2784314f, 0.784313738f);
		this.texts = new GameObject();
		this.texts.AddComponent<RectTransform>();
		this.texts.AddComponent<CanvasRenderer>();
		this.texts.AddComponent<Text>();
		this.texts.GetComponent<RectTransform>().sizeDelta = new Vector2(350f, 50f);
		this.texts.GetComponent<Text>().font = (Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font);
		this.texts.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
		this.texts.transform.SetParent(GO.transform);
		this.texts.name = "TextBox";
		return GO;
	}

	
	private void UpdateIPD()
	{
		this.strIPD = string.Format("IPD (mm): {0:F4}", OVRManager.profile.ipd * 1000f);
	}

	
	private void UpdateEyeHeightOffset()
	{
		float eyeHeight = OVRManager.profile.eyeHeight;
		this.strHeight = string.Format("Eye Height (m): {0:F3}", eyeHeight);
	}

	
	private void UpdateEyeDepthOffset()
	{
		float eyeDepth = OVRManager.profile.eyeDepth;
		this.strDepth = string.Format("Eye Depth (m): {0:F3}", eyeDepth);
	}

	
	private void UpdateFOV()
	{
		this.strFOV = string.Format("FOV (deg): {0:F3}", OVRManager.display.GetEyeRenderDesc(VRNode.LeftEye).fov.y);
	}

	
	private void UpdateResolutionEyeTexture()
	{
		OVRDisplay.EyeRenderDesc eyeRenderDesc = OVRManager.display.GetEyeRenderDesc(VRNode.LeftEye);
		OVRDisplay.EyeRenderDesc eyeRenderDesc2 = OVRManager.display.GetEyeRenderDesc(VRNode.RightEye);
		float renderViewportScale = VRSettings.renderViewportScale;
		float num = (float)((int)(renderViewportScale * (eyeRenderDesc.resolution.x + eyeRenderDesc2.resolution.x)));
		float num2 = (float)((int)(renderViewportScale * Mathf.Max(eyeRenderDesc.resolution.y, eyeRenderDesc2.resolution.y)));
		this.strResolutionEyeTexture = string.Format("Resolution : {0} x {1}", num, num2);
	}

	
	private void UpdateLatencyValues()
	{
		OVRDisplay.LatencyData latency = OVRManager.display.latency;
		if (latency.render < 1E-06f && latency.timeWarp < 1E-06f && latency.postPresent < 1E-06f)
		{
			this.strLatencies = string.Format("Latency values are not available.", new object[0]);
		}
		else
		{
			this.strLatencies = string.Format("Render: {0:F3} TimeWarp: {1:F3} Post-Present: {2:F3}\nRender Error: {3:F3} TimeWarp Error: {4:F3}", new object[]
			{
				latency.render,
				latency.timeWarp,
				latency.postPresent,
				latency.renderError,
				latency.timeWarpError
			});
		}
	}

	
	private void UpdateFPS()
	{
		this.timeLeft -= Time.unscaledDeltaTime;
		this.accum += Time.unscaledDeltaTime;
		this.frames++;
		if ((double)this.timeLeft <= 0.0)
		{
			float num = (float)this.frames / this.accum;
			this.strFPS = string.Format("FPS: {0:F2}", num);
			this.timeLeft += this.updateInterval;
			this.accum = 0f;
			this.frames = 0;
		}
	}

	
	private GameObject debugUIManager;

	
	private GameObject debugUIObject;

	
	private GameObject riftPresent;

	
	private GameObject fps;

	
	private GameObject ipd;

	
	private GameObject fov;

	
	private GameObject height;

	
	private GameObject depth;

	
	private GameObject resolutionEyeTexture;

	
	private GameObject latencies;

	
	private GameObject texts;

	
	private string strRiftPresent;

	
	private string strFPS;

	
	private string strIPD;

	
	private string strFOV;

	
	private string strHeight;

	
	private string strDepth;

	
	private string strResolutionEyeTexture;

	
	private string strLatencies;

	
	private float updateInterval = 0.5f;

	
	private float accum;

	
	private int frames;

	
	private float timeLeft;

	
	private bool initUIComponent;

	
	private bool isInited;

	
	private float offsetY = 55f;

	
	private float riftPresentTimeout;

	
	private bool showVRVars;
}
