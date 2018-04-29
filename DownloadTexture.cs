using System;
using System.Collections;
using UnityEngine;


[RequireComponent(typeof(UITexture))]
public class DownloadTexture : MonoBehaviour
{
	
	private IEnumerator Start()
	{
		WWW www = new WWW(this.url);
		yield return www;
		this.mTex = www.texture;
		if (this.mTex != null)
		{
			UITexture ut = base.GetComponent<UITexture>();
			ut.mainTexture = this.mTex;
			if (this.pixelPerfect)
			{
				ut.MakePixelPerfect();
			}
		}
		www.Dispose();
		yield break;
	}

	
	private void OnDestroy()
	{
		if (this.mTex != null)
		{
			UnityEngine.Object.Destroy(this.mTex);
		}
	}

	
	public string url = "http:

	
	public bool pixelPerfect = true;

	
	private Texture2D mTex;
}
