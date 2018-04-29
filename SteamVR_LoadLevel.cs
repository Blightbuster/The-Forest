using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;


public class SteamVR_LoadLevel : MonoBehaviour
{
	
	
	public static bool loading
	{
		get
		{
			return SteamVR_LoadLevel._active != null;
		}
	}

	
	
	public static float progress
	{
		get
		{
			return (!(SteamVR_LoadLevel._active != null) || SteamVR_LoadLevel._active.async == null) ? 0f : SteamVR_LoadLevel._active.async.progress;
		}
	}

	
	
	public static Texture progressTexture
	{
		get
		{
			return (!(SteamVR_LoadLevel._active != null)) ? null : SteamVR_LoadLevel._active.renderTexture;
		}
	}

	
	private void OnEnable()
	{
		if (this.autoTriggerOnEnable)
		{
			this.Trigger();
		}
	}

	
	public void Trigger()
	{
		if (!SteamVR_LoadLevel.loading && !string.IsNullOrEmpty(this.levelName))
		{
			base.StartCoroutine(this.LoadLevel());
		}
	}

	
	public static void Begin(string levelName, bool showGrid = false, float fadeOutTime = 0.5f, float r = 0f, float g = 0f, float b = 0f, float a = 1f)
	{
		SteamVR_LoadLevel steamVR_LoadLevel = new GameObject("loader").AddComponent<SteamVR_LoadLevel>();
		steamVR_LoadLevel.levelName = levelName;
		steamVR_LoadLevel.showGrid = showGrid;
		steamVR_LoadLevel.fadeOutTime = fadeOutTime;
		steamVR_LoadLevel.backgroundColor = new Color(r, g, b, a);
		steamVR_LoadLevel.Trigger();
	}

	
	private void OnGUI()
	{
		if (SteamVR_LoadLevel._active != this)
		{
			return;
		}
		if (this.progressBarEmpty != null && this.progressBarFull != null)
		{
			if (this.progressBarOverlayHandle == 0UL)
			{
				this.progressBarOverlayHandle = this.GetOverlayHandle("progressBar", (!(this.progressBarTransform != null)) ? base.transform : this.progressBarTransform, this.progressBarWidthInMeters);
			}
			if (this.progressBarOverlayHandle != 0UL)
			{
				float num = (this.async == null) ? 0f : this.async.progress;
				int width = this.progressBarFull.width;
				int height = this.progressBarFull.height;
				if (this.renderTexture == null)
				{
					this.renderTexture = new RenderTexture(width, height, 0);
					this.renderTexture.Create();
				}
				RenderTexture active = RenderTexture.active;
				RenderTexture.active = this.renderTexture;
				if (Event.current.type == EventType.Repaint)
				{
					GL.Clear(false, true, Color.clear);
				}
				GUILayout.BeginArea(new Rect(0f, 0f, (float)width, (float)height));
				GUI.DrawTexture(new Rect(0f, 0f, (float)width, (float)height), this.progressBarEmpty);
				GUI.DrawTextureWithTexCoords(new Rect(0f, 0f, num * (float)width, (float)height), this.progressBarFull, new Rect(0f, 0f, num, 1f));
				GUILayout.EndArea();
				RenderTexture.active = active;
				CVROverlay overlay = OpenVR.Overlay;
				if (overlay != null)
				{
					Texture_t texture_t = default(Texture_t);
					texture_t.handle = this.renderTexture.GetNativeTexturePtr();
					texture_t.eType = SteamVR.instance.textureType;
					texture_t.eColorSpace = EColorSpace.Auto;
					overlay.SetOverlayTexture(this.progressBarOverlayHandle, ref texture_t);
				}
			}
		}
	}

	
	private void Update()
	{
		if (SteamVR_LoadLevel._active != this)
		{
			return;
		}
		this.alpha = Mathf.Clamp01(this.alpha + this.fadeRate * Time.deltaTime);
		CVROverlay overlay = OpenVR.Overlay;
		if (overlay != null)
		{
			if (this.loadingScreenOverlayHandle != 0UL)
			{
				overlay.SetOverlayAlpha(this.loadingScreenOverlayHandle, this.alpha);
			}
			if (this.progressBarOverlayHandle != 0UL)
			{
				overlay.SetOverlayAlpha(this.progressBarOverlayHandle, this.alpha);
			}
		}
	}

	
	private IEnumerator LoadLevel()
	{
		if (this.loadingScreen != null && this.loadingScreenDistance > 0f)
		{
			SteamVR_Controller.Device hmd = SteamVR_Controller.Input(0);
			while (!hmd.hasTracking)
			{
				yield return null;
			}
			SteamVR_Utils.RigidTransform tloading = hmd.transform;
			tloading.rot = Quaternion.Euler(0f, tloading.rot.eulerAngles.y, 0f);
			tloading.pos += tloading.rot * new Vector3(0f, 0f, this.loadingScreenDistance);
			Transform t = (!(this.loadingScreenTransform != null)) ? base.transform : this.loadingScreenTransform;
			t.position = tloading.pos;
			t.rotation = tloading.rot;
		}
		SteamVR_LoadLevel._active = this;
		SteamVR_Events.Loading.Send(true);
		if (this.loadingScreenFadeInTime > 0f)
		{
			this.fadeRate = 1f / this.loadingScreenFadeInTime;
		}
		else
		{
			this.alpha = 1f;
		}
		CVROverlay overlay = OpenVR.Overlay;
		if (this.loadingScreen != null && overlay != null)
		{
			this.loadingScreenOverlayHandle = this.GetOverlayHandle("loadingScreen", (!(this.loadingScreenTransform != null)) ? base.transform : this.loadingScreenTransform, this.loadingScreenWidthInMeters);
			if (this.loadingScreenOverlayHandle != 0UL)
			{
				Texture_t texture_t = default(Texture_t);
				texture_t.handle = this.loadingScreen.GetNativeTexturePtr();
				texture_t.eType = SteamVR.instance.textureType;
				texture_t.eColorSpace = EColorSpace.Auto;
				overlay.SetOverlayTexture(this.loadingScreenOverlayHandle, ref texture_t);
			}
		}
		bool fadedForeground = false;
		SteamVR_Events.LoadingFadeOut.Send(this.fadeOutTime);
		CVRCompositor compositor = OpenVR.Compositor;
		if (compositor != null)
		{
			if (this.front != null)
			{
				SteamVR_Skybox.SetOverride(this.front, this.back, this.left, this.right, this.top, this.bottom);
				compositor.FadeGrid(this.fadeOutTime, true);
				yield return new WaitForSeconds(this.fadeOutTime);
			}
			else if (this.backgroundColor != Color.clear)
			{
				if (this.showGrid)
				{
					compositor.FadeToColor(0f, this.backgroundColor.r, this.backgroundColor.g, this.backgroundColor.b, this.backgroundColor.a, true);
					compositor.FadeGrid(this.fadeOutTime, true);
					yield return new WaitForSeconds(this.fadeOutTime);
				}
				else
				{
					compositor.FadeToColor(this.fadeOutTime, this.backgroundColor.r, this.backgroundColor.g, this.backgroundColor.b, this.backgroundColor.a, false);
					yield return new WaitForSeconds(this.fadeOutTime + 0.1f);
					compositor.FadeGrid(0f, true);
					fadedForeground = true;
				}
			}
		}
		SteamVR_Render.pauseRendering = true;
		while (this.alpha < 1f)
		{
			yield return null;
		}
		base.transform.parent = null;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (!string.IsNullOrEmpty(this.internalProcessPath))
		{
			UnityEngine.Debug.Log("Launching external application...");
			CVRApplications applications = OpenVR.Applications;
			if (applications == null)
			{
				UnityEngine.Debug.Log("Failed to get OpenVR.Applications interface!");
			}
			else
			{
				string currentDirectory = Directory.GetCurrentDirectory();
				string text = Path.Combine(currentDirectory, this.internalProcessPath);
				UnityEngine.Debug.Log("LaunchingInternalProcess");
				UnityEngine.Debug.Log("ExternalAppPath = " + this.internalProcessPath);
				UnityEngine.Debug.Log("FullPath = " + text);
				UnityEngine.Debug.Log("ExternalAppArgs = " + this.internalProcessArgs);
				UnityEngine.Debug.Log("WorkingDirectory = " + currentDirectory);
				EVRApplicationError evrapplicationError = applications.LaunchInternalProcess(text, this.internalProcessArgs, currentDirectory);
				UnityEngine.Debug.Log("LaunchInternalProcessError: " + evrapplicationError);
				Process.GetCurrentProcess().Kill();
			}
		}
		else
		{
			LoadSceneMode mode = (!this.loadAdditive) ? LoadSceneMode.Single : LoadSceneMode.Additive;
			if (this.loadAsync)
			{
				Application.backgroundLoadingPriority = ThreadPriority.Low;
				this.async = SceneManager.LoadSceneAsync(this.levelName, mode);
				while (!this.async.isDone)
				{
					yield return null;
				}
			}
			else
			{
				SceneManager.LoadScene(this.levelName, mode);
			}
		}
		yield return null;
		GC.Collect();
		yield return null;
		Shader.WarmupAllShaders();
		yield return new WaitForSeconds(this.postLoadSettleTime);
		SteamVR_Render.pauseRendering = false;
		if (this.loadingScreenFadeOutTime > 0f)
		{
			this.fadeRate = -1f / this.loadingScreenFadeOutTime;
		}
		else
		{
			this.alpha = 0f;
		}
		SteamVR_Events.LoadingFadeIn.Send(this.fadeInTime);
		compositor = OpenVR.Compositor;
		if (compositor != null)
		{
			if (fadedForeground)
			{
				compositor.FadeGrid(0f, false);
				compositor.FadeToColor(this.fadeInTime, 0f, 0f, 0f, 0f, false);
				yield return new WaitForSeconds(this.fadeInTime);
			}
			else
			{
				compositor.FadeGrid(this.fadeInTime, false);
				yield return new WaitForSeconds(this.fadeInTime);
				if (this.front != null)
				{
					SteamVR_Skybox.ClearOverride();
				}
			}
		}
		while (this.alpha > 0f)
		{
			yield return null;
		}
		if (overlay != null)
		{
			if (this.progressBarOverlayHandle != 0UL)
			{
				overlay.HideOverlay(this.progressBarOverlayHandle);
			}
			if (this.loadingScreenOverlayHandle != 0UL)
			{
				overlay.HideOverlay(this.loadingScreenOverlayHandle);
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
		SteamVR_LoadLevel._active = null;
		SteamVR_Events.Loading.Send(false);
		yield break;
	}

	
	private ulong GetOverlayHandle(string overlayName, Transform transform, float widthInMeters = 1f)
	{
		ulong num = 0UL;
		CVROverlay overlay = OpenVR.Overlay;
		if (overlay == null)
		{
			return num;
		}
		string pchOverlayKey = SteamVR_Overlay.key + "." + overlayName;
		EVROverlayError evroverlayError = overlay.FindOverlay(pchOverlayKey, ref num);
		if (evroverlayError != EVROverlayError.None)
		{
			evroverlayError = overlay.CreateOverlay(pchOverlayKey, overlayName, ref num);
		}
		if (evroverlayError == EVROverlayError.None)
		{
			overlay.ShowOverlay(num);
			overlay.SetOverlayAlpha(num, this.alpha);
			overlay.SetOverlayWidthInMeters(num, widthInMeters);
			if (SteamVR.instance.textureType == ETextureType.DirectX)
			{
				VRTextureBounds_t vrtextureBounds_t = default(VRTextureBounds_t);
				vrtextureBounds_t.uMin = 0f;
				vrtextureBounds_t.vMin = 1f;
				vrtextureBounds_t.uMax = 1f;
				vrtextureBounds_t.vMax = 0f;
				overlay.SetOverlayTextureBounds(num, ref vrtextureBounds_t);
			}
			SteamVR_Camera steamVR_Camera = (this.loadingScreenDistance != 0f) ? null : SteamVR_Render.Top();
			if (steamVR_Camera != null && steamVR_Camera.origin != null)
			{
				SteamVR_Utils.RigidTransform rigidTransform = new SteamVR_Utils.RigidTransform(steamVR_Camera.origin, transform);
				rigidTransform.pos.x = rigidTransform.pos.x / steamVR_Camera.origin.localScale.x;
				rigidTransform.pos.y = rigidTransform.pos.y / steamVR_Camera.origin.localScale.y;
				rigidTransform.pos.z = rigidTransform.pos.z / steamVR_Camera.origin.localScale.z;
				HmdMatrix34_t hmdMatrix34_t = rigidTransform.ToHmdMatrix34();
				overlay.SetOverlayTransformAbsolute(num, SteamVR_Render.instance.trackingSpace, ref hmdMatrix34_t);
			}
			else
			{
				SteamVR_Utils.RigidTransform rigidTransform2 = new SteamVR_Utils.RigidTransform(transform);
				HmdMatrix34_t hmdMatrix34_t2 = rigidTransform2.ToHmdMatrix34();
				overlay.SetOverlayTransformAbsolute(num, SteamVR_Render.instance.trackingSpace, ref hmdMatrix34_t2);
			}
		}
		return num;
	}

	
	private static SteamVR_LoadLevel _active;

	
	public string levelName;

	
	public string internalProcessPath;

	
	public string internalProcessArgs;

	
	public bool loadAdditive;

	
	public bool loadAsync = true;

	
	public Texture loadingScreen;

	
	public Texture progressBarEmpty;

	
	public Texture progressBarFull;

	
	public float loadingScreenWidthInMeters = 6f;

	
	public float progressBarWidthInMeters = 3f;

	
	public float loadingScreenDistance;

	
	public Transform loadingScreenTransform;

	
	public Transform progressBarTransform;

	
	public Texture front;

	
	public Texture back;

	
	public Texture left;

	
	public Texture right;

	
	public Texture top;

	
	public Texture bottom;

	
	public Color backgroundColor = Color.black;

	
	public bool showGrid;

	
	public float fadeOutTime = 0.5f;

	
	public float fadeInTime = 0.5f;

	
	public float postLoadSettleTime;

	
	public float loadingScreenFadeInTime = 1f;

	
	public float loadingScreenFadeOutTime = 0.25f;

	
	private float fadeRate = 1f;

	
	private float alpha;

	
	private AsyncOperation async;

	
	private RenderTexture renderTexture;

	
	private ulong loadingScreenOverlayHandle;

	
	private ulong progressBarOverlayHandle;

	
	public bool autoTriggerOnEnable;
}
