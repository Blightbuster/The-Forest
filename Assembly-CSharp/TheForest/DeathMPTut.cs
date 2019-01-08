using System;
using System.Collections;
using TheForest.Buildings.World;
using TheForest.Utils;
using UnityEngine;

namespace TheForest
{
	public class DeathMPTut : MonoBehaviour
	{
		private IEnumerator Start()
		{
			LocalPlayer.Tuts.ShowDeathMP();
			GameObject lbtGO = new GameObject("LastBuiltLocation");
			lbtGO.transform.localScale = new Vector3(0f, 0f, 1f);
			lbtGO.transform.parent = base.transform;
			lbtGO.transform.localPosition = Vector3.one;
			GUITexture gt = lbtGO.AddComponent<GUITexture>();
			gt.texture = Resources.Load<Texture>("PlayerIcon");
			gt.pixelInset = new Rect(-64f, -29f, 58f, 58f);
			Color c = gt.color;
			c.a = 0.2f;
			gt.color = c;
			LastBuiltLocation lbc = lbtGO.AddComponent<LastBuiltLocation>();
			lbc.target = base.transform;
			yield return null;
			lbc.IsOverLayIcon = false;
			yield return null;
			lbc.IsOverLayIcon = false;
			yield break;
		}

		private void OnDestroy()
		{
			if (Scene.HudGui)
			{
				LocalPlayer.Tuts.HideDeathMP();
			}
		}

		private void OnLevelWasLoaded(int level)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
