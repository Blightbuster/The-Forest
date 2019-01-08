using System;
using System.Text;
using UnityEngine;

public class IntervalTextureLogger : MonoBehaviour
{
	public static void Start(float interval)
	{
		if (IntervalTextureLogger._instance == null)
		{
			GameObject gameObject = new GameObject("_textureLogger_");
			IntervalTextureLogger._instance = gameObject.AddComponent<IntervalTextureLogger>();
		}
		IntervalTextureLogger._instance.Interval = interval;
	}

	public static void Stop()
	{
		if (IntervalTextureLogger._instance == null)
		{
			return;
		}
		UnityEngine.Object.Destroy(IntervalTextureLogger._instance.gameObject);
	}

	private int GetBitsPerPixel(TextureFormat format)
	{
		switch (format)
		{
		case TextureFormat.Alpha8:
			return 8;
		case TextureFormat.ARGB4444:
			return 16;
		case TextureFormat.RGB24:
			return 24;
		case TextureFormat.RGBA32:
			return 32;
		case TextureFormat.ARGB32:
			return 32;
		default:
			switch (format)
			{
			case TextureFormat.PVRTC_RGB2:
				return 2;
			case TextureFormat.PVRTC_RGBA2:
				return 2;
			case TextureFormat.PVRTC_RGB4:
				return 4;
			case TextureFormat.PVRTC_RGBA4:
				return 4;
			case TextureFormat.ETC_RGB4:
				return 4;
			default:
				return 0;
			}
			break;
		case TextureFormat.RGB565:
			return 16;
		case TextureFormat.DXT1:
			return 4;
		case TextureFormat.DXT5:
			return 8;
		case TextureFormat.RGBA4444:
			return 16;
		case TextureFormat.BGRA32:
			return 32;
		}
	}

	private int CalculateTextureSizeBytes(Texture tTexture)
	{
		int num = tTexture.width;
		int num2 = tTexture.height;
		if (tTexture is Texture2D)
		{
			Texture2D texture2D = tTexture as Texture2D;
			int bitsPerPixel = this.GetBitsPerPixel(texture2D.format);
			int mipmapCount = texture2D.mipmapCount;
			int i = 1;
			int num3 = 0;
			while (i <= mipmapCount)
			{
				num3 += num * num2 * bitsPerPixel / 8;
				num /= 2;
				num2 /= 2;
				i++;
			}
			return num3;
		}
		if (tTexture is Texture2DArray)
		{
			Texture2DArray texture2DArray = tTexture as Texture2DArray;
			int bitsPerPixel2 = this.GetBitsPerPixel(texture2DArray.format);
			int num4 = 10;
			int j = 1;
			int num5 = 0;
			while (j <= num4)
			{
				num5 += num * num2 * bitsPerPixel2 / 8;
				num /= 2;
				num2 /= 2;
				j++;
			}
			return num5 * texture2DArray.depth;
		}
		if (tTexture is Cubemap)
		{
			Cubemap cubemap = tTexture as Cubemap;
			int bitsPerPixel3 = this.GetBitsPerPixel(cubemap.format);
			return num * num2 * 6 * bitsPerPixel3 / 8;
		}
		return 0;
	}

	private void LateUpdate()
	{
		if (Time.realtimeSinceStartup - this._lastRun < this.Interval)
		{
			return;
		}
		this._lastRun = Time.realtimeSinceStartup;
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		Texture2D[] array = Resources.FindObjectsOfTypeAll<Texture2D>();
		int num2 = 0;
		foreach (Texture2D texture2D in array)
		{
			if (!(texture2D == null))
			{
				num++;
				num2 += this.CalculateTextureSizeBytes(texture2D);
				if (texture2D.width == 128 && texture2D.height == 2)
				{
					stringBuilder.AppendLine(string.Format("\"{0}\" {1}x{2} {3}", new object[]
					{
						texture2D.name,
						texture2D.width,
						texture2D.height,
						texture2D.format
					}));
				}
			}
		}
		stringBuilder.Insert(0, string.Format("Count: {0} / TotalSize: {1}\n", num, IntervalTextureLogger.ToReadable(num2)));
		Debug.Log(stringBuilder);
	}

	private static string ToReadable(int size)
	{
		string[] array = new string[]
		{
			"B",
			"KB",
			"MB",
			"GB",
			"TB"
		};
		int num = 0;
		while (size >= 1024 && num < array.Length - 1)
		{
			num++;
			size /= 1024;
		}
		return string.Format("{0:0.####} {1}", size, array[num]);
	}

	public float Interval = 2f;

	private float _lastRun;

	private static IntervalTextureLogger _instance;
}
