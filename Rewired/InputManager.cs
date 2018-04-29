﻿using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Rewired.Platforms;
using Rewired.Utils;
using Rewired.Utils.Interfaces;
using UnityEngine;

namespace Rewired
{
	
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class InputManager : InputManager_Base
	{
		
		protected override void DetectPlatform()
		{
			this.editorPlatform = EditorPlatform.None;
			this.platform = Platform.Unknown;
			this.webplayerPlatform = WebplayerPlatform.None;
			this.isEditor = false;
			string text = SystemInfo.deviceName ?? string.Empty;
			string text2 = SystemInfo.deviceModel ?? string.Empty;
			this.platform = Platform.Windows;
		}

		
		protected override void CheckRecompile()
		{
		}

		
		protected override string GetFocusedEditorWindowTitle()
		{
			return string.Empty;
		}

		
		protected override IExternalTools GetExternalTools()
		{
			return new ExternalTools();
		}

		
		private bool CheckDeviceName(string searchPattern, string deviceName, string deviceModel)
		{
			return Regex.IsMatch(deviceName, searchPattern, RegexOptions.IgnoreCase) || Regex.IsMatch(deviceModel, searchPattern, RegexOptions.IgnoreCase);
		}
	}
}
