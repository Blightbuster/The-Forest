using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UniLinq;
using UnityEngine;

internal class BoltDebugStartSettings
{
	public static bool startServer
	{
		get
		{
			return Environment.GetCommandLineArgs().Contains("--bolt-debugstart-server");
		}
	}

	public static bool startClient
	{
		get
		{
			return Environment.GetCommandLineArgs().Contains("--bolt-debugstart-client");
		}
	}

	public static int windowIndex
	{
		get
		{
			foreach (string text in Environment.GetCommandLineArgs())
			{
				if (text.StartsWith("--bolt-window-index-"))
				{
					return int.Parse(text.Replace("--bolt-window-index-", string.Empty));
				}
			}
			return 0;
		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool EnumWindows(BoltDebugStartSettings.EnumWindowsProc callback, IntPtr extraData);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetSystemMetrics(int index);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

	private static bool Window(IntPtr hWnd, IntPtr lParam)
	{
		int num = -1;
		int id = Process.GetCurrentProcess().Id;
		BoltDebugStartSettings.GetWindowThreadProcessId(new HandleRef(BoltDebugStartSettings.handle, hWnd), out num);
		if (num == id)
		{
			BoltDebugStartSettings.unityHandle = new HandleRef(BoltDebugStartSettings.handle, hWnd);
			return false;
		}
		return true;
	}

	public static void PositionWindow()
	{
		if (BoltDebugStartSettings.startClient || BoltDebugStartSettings.startServer)
		{
			if (BoltDebugStartSettings.<>f__mg$cache0 == null)
			{
				BoltDebugStartSettings.<>f__mg$cache0 = new BoltDebugStartSettings.EnumWindowsProc(BoltDebugStartSettings.Window);
			}
			BoltDebugStartSettings.EnumWindows(BoltDebugStartSettings.<>f__mg$cache0, IntPtr.Zero);
			if (BoltDebugStartSettings.unityHandle.Wrapper != null)
			{
				int width = Screen.width;
				int height = Screen.height;
				int x = 0;
				int y = 0;
				int systemMetrics = BoltDebugStartSettings.GetSystemMetrics(0);
				int systemMetrics2 = BoltDebugStartSettings.GetSystemMetrics(1);
				if (BoltDebugStartSettings.startServer)
				{
					x = systemMetrics / 2 - width / 2;
					y = systemMetrics2 / 2 - height / 2;
				}
				else
				{
					int num = BoltDebugStartSettings.windowIndex % 4;
					if (num != 1)
					{
						if (num != 2)
						{
							if (num == 3)
							{
								x = systemMetrics - width;
								y = systemMetrics2 - height;
							}
						}
						else
						{
							y = systemMetrics2 - height;
						}
					}
					else
					{
						x = systemMetrics - width;
					}
				}
				BoltDebugStartSettings.SetWindowPos(BoltDebugStartSettings.unityHandle.Handle, BoltDebugStartSettings.HWND.Top, x, y, width, height, BoltDebugStartSettings.SWP.NOSIZE);
			}
		}
	}

	private static readonly object handle = new object();

	private static HandleRef unityHandle = default(HandleRef);

	[CompilerGenerated]
	private static BoltDebugStartSettings.EnumWindowsProc <>f__mg$cache0;

	private static class HWND
	{
		public static IntPtr NoTopMost = new IntPtr(-2);

		public static IntPtr TopMost = new IntPtr(-1);

		public static IntPtr Top = new IntPtr(0);

		public static IntPtr Bottom = new IntPtr(1);
	}

	private static class SWP
	{
		public static readonly int NOSIZE = 1;

		public static readonly int NOMOVE = 2;

		public static readonly int NOZORDER = 4;

		public static readonly int NOREDRAW = 8;

		public static readonly int NOACTIVATE = 16;

		public static readonly int DRAWFRAME = 32;

		public static readonly int FRAMECHANGED = 32;

		public static readonly int SHOWWINDOW = 64;

		public static readonly int HIDEWINDOW = 128;

		public static readonly int NOCOPYBITS = 256;

		public static readonly int NOOWNERZORDER = 512;

		public static readonly int NOREPOSITION = 512;

		public static readonly int NOSENDCHANGING = 1024;

		public static readonly int DEFERERASE = 8192;

		public static readonly int ASYNCWINDOWPOS = 16384;
	}

	private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
}
