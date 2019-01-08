using System;
using System.Collections;
using System.IO;
using TheForest.Commons.Enums;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	public class LoadSaveSlotThumbnail : MonoBehaviour
	{
		private void OnEnable()
		{
			this._slotNum = (int)this._slot;
			base.StartCoroutine(this.LoadImageRoutine());
		}

		private IEnumerator LoadImageRoutine()
		{
			string path = SaveSlotUtils.GetLocalSlotPath(this._slot);
			string thumbnailPath = path + "thumb.png";
			if (File.Exists(thumbnailPath))
			{
				string url = "file:///" + thumbnailPath;
				WWW www = new WWW(url);
				yield return www;
				this._texture.mainTexture = www.texture;
				this._texture.mainTexture.mipMapBias = -0.5f;
				this._texture.enabled = true;
			}
			else if (!File.Exists(path + "__RESUME__"))
			{
				this._texture.enabled = false;
			}
			yield break;
		}

		public UITexture _texture;

		public Slots _slot;

		[HideInInspector]
		public int _slotNum;
	}
}
