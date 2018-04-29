using System;
using UnityEngine;

namespace TheForest.Graphics
{
	
	[ExecuteInEditMode]
	[AddComponentMenu("The Forest/Graphics/RenderProxy")]
	public class RenderProxy : MonoBehaviour
	{
		
		private void OnWillRenderObject()
		{
			this.frame++;
			if (this.frame == RenderProxy.globalFrame)
			{
				return;
			}
			RenderProxy.globalFrame = this.frame;
			if (WaterEngine.RenderCameras != null)
			{
				WaterEngine.RenderCameras();
			}
		}

		
		private int frame;

		
		private static int globalFrame;
	}
}
