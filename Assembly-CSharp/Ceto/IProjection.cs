using System;
using UnityEngine;

namespace Ceto
{
	public interface IProjection
	{
		void UpdateProjection(Camera cam, CameraData data, bool projectSceneView);

		bool IsDouble { get; }

		bool IsFlipped { get; }
	}
}
