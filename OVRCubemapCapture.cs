using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;


public class OVRCubemapCapture : MonoBehaviour
{
	
	private void Update()
	{
		if (this.autoTriggerAfterLaunch)
		{
			this.autoTriggerElapse += Time.deltaTime;
			if (this.autoTriggerElapse >= this.autoTriggerDelay)
			{
				this.autoTriggerAfterLaunch = false;
				OVRCubemapCapture.TriggerCubemapCapture(base.transform.position, this.cubemapSize, this.pathName);
			}
		}
		if (Input.GetKeyDown(this.triggeredByKey))
		{
			OVRCubemapCapture.TriggerCubemapCapture(base.transform.position, this.cubemapSize, this.pathName);
		}
	}

	
	public static void TriggerCubemapCapture(Vector3 capturePos, int cubemapSize = 2048, string pathName = null)
	{
		GameObject gameObject = new GameObject("CubemapCamera", new Type[]
		{
			typeof(Camera)
		});
		gameObject.hideFlags = HideFlags.HideAndDontSave;
		gameObject.transform.position = capturePos;
		gameObject.transform.rotation = Quaternion.identity;
		Camera component = gameObject.GetComponent<Camera>();
		component.farClipPlane = 10000f;
		component.enabled = false;
		Cubemap cubemap = new Cubemap(cubemapSize, TextureFormat.RGB24, false);
		OVRCubemapCapture.RenderIntoCubemap(component, cubemap);
		OVRCubemapCapture.SaveCubemapCapture(cubemap, pathName);
		UnityEngine.Object.DestroyImmediate(cubemap);
		UnityEngine.Object.DestroyImmediate(gameObject);
	}

	
	public static void RenderIntoCubemap(Camera ownerCamera, Cubemap outCubemap)
	{
		int width = outCubemap.width;
		int height = outCubemap.height;
		CubemapFace[] array = new CubemapFace[6];
		RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.$field-DB17E883A647963A26D973378923EF4649801319).FieldHandle);
		CubemapFace[] array2 = array;
		Vector3[] array3 = new Vector3[]
		{
			new Vector3(0f, 90f, 0f),
			new Vector3(0f, -90f, 0f),
			new Vector3(-90f, 0f, 0f),
			new Vector3(90f, 0f, 0f),
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, 180f, 0f)
		};
		RenderTexture active = RenderTexture.active;
		float fieldOfView = ownerCamera.fieldOfView;
		float aspect = ownerCamera.aspect;
		Quaternion rotation = ownerCamera.transform.rotation;
		RenderTexture renderTexture = new RenderTexture(width, height, 24);
		renderTexture.antiAliasing = 8;
		renderTexture.dimension = TextureDimension.Tex2D;
		renderTexture.hideFlags = HideFlags.HideAndDontSave;
		Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);
		texture2D.hideFlags = HideFlags.HideAndDontSave;
		ownerCamera.targetTexture = renderTexture;
		ownerCamera.fieldOfView = 90f;
		ownerCamera.aspect = 1f;
		Color[] array4 = new Color[texture2D.height * texture2D.width];
		for (int i = 0; i < array2.Length; i++)
		{
			ownerCamera.transform.eulerAngles = array3[i];
			ownerCamera.Render();
			RenderTexture.active = renderTexture;
			texture2D.ReadPixels(new Rect(0f, 0f, (float)width, (float)height), 0, 0);
			Color[] pixels = texture2D.GetPixels();
			for (int j = 0; j < height; j++)
			{
				for (int k = 0; k < width; k++)
				{
					array4[j * width + k] = pixels[(height - 1 - j) * width + k];
				}
			}
			outCubemap.SetPixels(array4, array2[i]);
		}
		outCubemap.SmoothEdges();
		RenderTexture.active = active;
		ownerCamera.fieldOfView = fieldOfView;
		ownerCamera.aspect = aspect;
		ownerCamera.transform.rotation = rotation;
		ownerCamera.targetTexture = active;
		UnityEngine.Object.DestroyImmediate(texture2D);
		UnityEngine.Object.DestroyImmediate(renderTexture);
	}

	
	public static bool SaveCubemapCapture(Cubemap cubemap, string pathName = null)
	{
		int width = cubemap.width;
		int height = cubemap.height;
		int num = 0;
		int y = 0;
		bool flag = true;
		string text;
		string text2;
		if (string.IsNullOrEmpty(pathName))
		{
			text = Application.persistentDataPath + "/OVR_ScreenShot360/";
			text2 = null;
		}
		else
		{
			text = Path.GetDirectoryName(pathName);
			text2 = Path.GetFileName(pathName);
			if (text[text.Length - 1] != '/' || text[text.Length - 1] != '\\')
			{
				text += "/";
			}
		}
		if (string.IsNullOrEmpty(text2))
		{
			text2 = "OVR_" + DateTime.Now.ToString("hh_mm_ss") + ".png";
		}
		string extension = Path.GetExtension(text2);
		if (extension == ".png")
		{
			flag = true;
		}
		else
		{
			if (!(extension == ".jpg"))
			{
				Debug.LogError("Unsupported file format" + extension);
				return false;
			}
			flag = false;
		}
		try
		{
			Directory.CreateDirectory(text);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to create path " + text + " since " + ex.ToString());
			return false;
		}
		Texture2D texture2D = new Texture2D(width * 6, height, TextureFormat.RGB24, false);
		if (texture2D == null)
		{
			Debug.LogError("[OVRScreenshotWizard] Failed creating the texture!");
			return false;
		}
		CubemapFace[] array = new CubemapFace[6];
		RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.$field-DB17E883A647963A26D973378923EF4649801319).FieldHandle);
		CubemapFace[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Color[] pixels = cubemap.GetPixels(array2[i]);
			Color[] array3 = new Color[pixels.Length];
			for (int j = 0; j < height; j++)
			{
				for (int k = 0; k < width; k++)
				{
					array3[j * width + k] = pixels[(height - 1 - j) * width + k];
				}
			}
			texture2D.SetPixels(num, y, width, height, array3);
			num += width;
		}
		try
		{
			byte[] bytes = (!flag) ? texture2D.EncodeToJPG() : texture2D.EncodeToPNG();
			File.WriteAllBytes(text + text2, bytes);
			Debug.Log("Cubemap file created " + text + text2);
		}
		catch (Exception ex2)
		{
			Debug.LogError("Failed to save cubemap file since " + ex2.ToString());
			return false;
		}
		UnityEngine.Object.DestroyImmediate(texture2D);
		return true;
	}

	
	public bool autoTriggerAfterLaunch = true;

	
	public float autoTriggerDelay = 1f;

	
	private float autoTriggerElapse;

	
	public KeyCode triggeredByKey = KeyCode.F8;

	
	public string pathName;

	
	public int cubemapSize = 2048;
}
