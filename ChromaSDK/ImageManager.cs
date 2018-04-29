using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace ChromaSDK
{
	
	public class ImageManager
	{
		
		static ImageManager()
		{
			ImageManager.SetupLogMechanism();
		}

		
		[DllImport("UnityImageNativePlugin")]
		private static extern void PluginLoadImage(IntPtr path);

		
		[DllImport("UnityImageNativePlugin")]
		public static extern int PluginGetFrameCount();

		
		[DllImport("UnityImageNativePlugin")]
		public static extern int PluginGetHeight();

		
		[DllImport("UnityImageNativePlugin")]
		public static extern int PluginGetWidth();

		
		[DllImport("UnityImageNativePlugin")]
		public static extern int PluginGetPixel(int frame, int x, int y);

		
		[DllImport("UnityImageNativePlugin")]
		private static extern void PluginSetLogDelegate(IntPtr logDelegate);

		
		private static void LogCallBack(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			Debug.Log(string.Format(":C++: {0}", text));
		}

		
		private static void SetupLogMechanism()
		{
			ImageManager.DebugLogDelegate d = new ImageManager.DebugLogDelegate(ImageManager.LogCallBack);
			ImageManager._sLogDelegate = Marshal.GetFunctionPointerForDelegate(d);
			ImageManager.PluginSetLogDelegate(ImageManager._sLogDelegate);
		}

		
		public static void LoadImage(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return;
			}
			FileInfo fileInfo = new FileInfo(path);
			if (fileInfo.Exists)
			{
				byte[] bytes = Encoding.ASCII.GetBytes(fileInfo.FullName);
				IntPtr intPtr = Marshal.AllocHGlobal(bytes.Length);
				Marshal.Copy(bytes, 0, intPtr, bytes.Length);
				ImageManager.PluginLoadImage(intPtr);
				Marshal.FreeHGlobal(intPtr);
			}
		}

		
		private const string DLL_NAME = "UnityImageNativePlugin";

		
		private static IntPtr _sLogDelegate = IntPtr.Zero;

		
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void DebugLogDelegate(string text);
	}
}
