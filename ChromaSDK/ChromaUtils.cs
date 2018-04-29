using System;
using System.Collections.Generic;
using System.Threading;
using ChromaSDK.Api;
using ChromaSDK.ChromaPackage.Model;
using UnityEngine;

namespace ChromaSDK
{
	
	public static class ChromaUtils
	{
		
		public static int GetMaxColumn(ChromaDevice2DEnum device)
		{
			switch (device)
			{
			case ChromaDevice2DEnum.Keyboard:
				return 22;
			case ChromaDevice2DEnum.Keypad:
				return 5;
			case ChromaDevice2DEnum.Mouse:
				return 7;
			default:
				return 0;
			}
		}

		
		public static int GetMaxRow(ChromaDevice2DEnum device)
		{
			switch (device)
			{
			case ChromaDevice2DEnum.Keyboard:
				return 6;
			case ChromaDevice2DEnum.Keypad:
				return 4;
			case ChromaDevice2DEnum.Mouse:
				return 9;
			default:
				return 0;
			}
		}

		
		public static int GetMaxLeds(ChromaDevice1DEnum device)
		{
			switch (device)
			{
			case ChromaDevice1DEnum.ChromaLink:
				return 5;
			case ChromaDevice1DEnum.Headset:
				return 5;
			case ChromaDevice1DEnum.Mousepad:
				return 15;
			default:
				return 0;
			}
		}

		
		public static int GetLowByte(int mask)
		{
			return mask & 255;
		}

		
		public static int GetHighByte(int mask)
		{
			return (mask & 65280) >> 8 & 255;
		}

		
		public static EffectArray1dInput CreateColors1D(ChromaDevice1DEnum device)
		{
			int maxLeds = ChromaUtils.GetMaxLeds(device);
			EffectArray1dInput effectArray1dInput = new EffectArray1dInput();
			for (int i = 0; i < maxLeds; i++)
			{
				effectArray1dInput.Add(new int?(0));
			}
			return effectArray1dInput;
		}

		
		public static EffectArray2dInput CreateColors2D(ChromaDevice2DEnum device)
		{
			int maxRow = ChromaUtils.GetMaxRow(device);
			int maxColumn = ChromaUtils.GetMaxColumn(device);
			EffectArray2dInput effectArray2dInput = new EffectArray2dInput();
			for (int i = 0; i < maxRow; i++)
			{
				List<int> list = new List<int>();
				for (int j = 0; j < maxColumn; j++)
				{
					list.Add(0);
				}
				effectArray2dInput.Add(list);
			}
			return effectArray2dInput;
		}

		
		public static EffectArray1dInput CreateRandomColors1D(ChromaDevice1DEnum device)
		{
			int maxLeds = ChromaUtils.GetMaxLeds(device);
			EffectArray1dInput effectArray1dInput = new EffectArray1dInput();
			for (int i = 0; i < maxLeds; i++)
			{
				effectArray1dInput.Add(new int?(ChromaUtils._sRandom.Next(16777215)));
			}
			return effectArray1dInput;
		}

		
		public static EffectArray2dInput CreateRandomColors2D(ChromaDevice2DEnum device)
		{
			int maxRow = ChromaUtils.GetMaxRow(device);
			int maxColumn = ChromaUtils.GetMaxColumn(device);
			EffectArray2dInput effectArray2dInput = new EffectArray2dInput();
			for (int i = 0; i < maxRow; i++)
			{
				List<int> list = new List<int>();
				for (int j = 0; j < maxColumn; j++)
				{
					list.Add(ChromaUtils._sRandom.Next(16777215));
				}
				effectArray2dInput.Add(list);
			}
			return effectArray2dInput;
		}

		
		public static EffectResponseId CreateEffectCustom1D(ChromaDevice1DEnum device, EffectArray1dInput input)
		{
			if (ChromaConnectionManager.Instance.Connected)
			{
				ChromaApi apiChromaInstance = ChromaConnectionManager.Instance.ApiChromaInstance;
				return ChromaUtils.CreateEffectCustom1D(apiChromaInstance, device, input);
			}
			return null;
		}

		
		private static EffectResponseId CreateEffectCustom1D(ChromaApi api, ChromaDevice1DEnum device, EffectArray1dInput input)
		{
			if (api == null)
			{
				Debug.LogError("CreateEffectCustom1D: Parameter api is null!");
				return null;
			}
			if (input == null)
			{
				Debug.LogError("CreateEffectCustom1D: Parameter input is null!");
				return null;
			}
			int maxLeds = ChromaUtils.GetMaxLeds(device);
			if (maxLeds != input.Count)
			{
				Debug.LogError(string.Format("CreateEffectCustom1D Array size mismatch element: %d==%d!", maxLeds, input.Count));
			}
			try
			{
				switch (device)
				{
				case ChromaDevice1DEnum.ChromaLink:
					return api.PostChromaLinkCustom(input);
				case ChromaDevice1DEnum.Headset:
					return api.PostHeadsetCustom(input);
				case ChromaDevice1DEnum.Mousepad:
					return api.PostMousepadCustom(input);
				}
			}
			catch (Exception)
			{
			}
			return null;
		}

		
		public static EffectResponseId CreateEffectCustom2D(ChromaDevice2DEnum device, EffectArray2dInput input)
		{
			if (ChromaConnectionManager.Instance.Connected)
			{
				ChromaApi apiChromaInstance = ChromaConnectionManager.Instance.ApiChromaInstance;
				return ChromaUtils.CreateEffectCustom2D(apiChromaInstance, device, input);
			}
			return null;
		}

		
		private static EffectResponseId CreateEffectCustom2D(ChromaApi api, ChromaDevice2DEnum device, EffectArray2dInput input)
		{
			if (api == null)
			{
				Debug.LogError("CreateEffectCustom2D: Parameter api is null!");
				return null;
			}
			if (input == null)
			{
				Debug.LogError("CreateEffectCustom2D: Parameter input is null!");
				return null;
			}
			int maxRow = ChromaUtils.GetMaxRow(device);
			int maxColumn = ChromaUtils.GetMaxColumn(device);
			if (maxRow != input.Count || (input.Count > 0 && maxColumn != input[0].Count))
			{
				Debug.LogError(string.Format("CreateEffectCustom2D Array size mismatch row: %d==%d column: %d==%d!", new object[]
				{
					maxRow,
					input.Count,
					maxColumn,
					(input.Count <= 0) ? 0 : input[0].Count
				}));
			}
			try
			{
				switch (device)
				{
				case ChromaDevice2DEnum.Keyboard:
					return api.PostKeyboardCustom(input);
				case ChromaDevice2DEnum.Keypad:
					return api.PostKeypadCustom(input);
				case ChromaDevice2DEnum.Mouse:
					return api.PostMouseCustom(input);
				}
			}
			catch (Exception)
			{
			}
			return null;
		}

		
		public static EffectIdentifierResponse SetEffect(string effectId)
		{
			if (ChromaConnectionManager.Instance.Connected)
			{
				ChromaApi apiChromaInstance = ChromaConnectionManager.Instance.ApiChromaInstance;
				return ChromaUtils.SetEffect(apiChromaInstance, effectId);
			}
			return null;
		}

		
		private static EffectIdentifierResponse SetEffect(ChromaApi api, string effectId)
		{
			if (api == null)
			{
				Debug.LogError("SetEffect: Parameter api is null!");
				return null;
			}
			if (string.IsNullOrEmpty(effectId))
			{
				Debug.LogError("SetEffect: Parameter effectId cannot be null or empty!");
				return null;
			}
			EffectIdentifierInput data = new EffectIdentifierInput(effectId, null);
			try
			{
				return api.PutEffect(data);
			}
			catch (Exception)
			{
			}
			return null;
		}

		
		public static EffectIdentifierResponse RemoveEffect(string effectId)
		{
			if (ChromaConnectionManager.Instance.Connected)
			{
				ChromaApi apiChromaInstance = ChromaConnectionManager.Instance.ApiChromaInstance;
				return ChromaUtils.RemoveEffect(apiChromaInstance, effectId);
			}
			return null;
		}

		
		private static EffectIdentifierResponse RemoveEffect(ChromaApi api, string effectId)
		{
			if (api == null)
			{
				Debug.LogError("RemoveEffect: Parameter api is null!");
				return null;
			}
			if (string.IsNullOrEmpty(effectId))
			{
				Debug.LogError("RemoveEffect: Parameter effectId cannot be null or empty!");
				return null;
			}
			EffectIdentifierInput data = new EffectIdentifierInput(effectId, null);
			try
			{
				return api.RemoveEffect(data);
			}
			catch (Exception)
			{
			}
			return null;
		}

		
		public static int ToBGR(Color color)
		{
			int num = (int)(Mathf.Clamp01(color.r) * 255f);
			int num2 = (int)(Mathf.Clamp01(color.g) * 255f) << 8;
			int num3 = (int)(Mathf.Clamp01(color.b) * 255f) << 16;
			return num3 | num2 | num;
		}

		
		public static Color ToRGB(int color)
		{
			int num = color & 255;
			int num2 = (color & 65280) >> 8;
			int num3 = (color & 16711680) >> 16;
			return new Color((float)num * 0.003921569f, (float)num2 * 0.003921569f, (float)num3 * 0.003921569f);
		}

		
		public static void RunOnThread(Action action)
		{
			Thread thread = new Thread(delegate
			{
				try
				{
					action();
				}
				catch (Exception)
				{
				}
			});
			thread.Start();
		}

		
		private static System.Random _sRandom = new System.Random(123);
	}
}
