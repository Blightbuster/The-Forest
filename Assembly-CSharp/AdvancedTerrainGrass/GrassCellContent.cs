using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace AdvancedTerrainGrass
{
	[Serializable]
	public class GrassCellContent
	{
		public void RelaseBuffers()
		{
		}

		public void ReleaseCellContent()
		{
			this.state = 0;
			this.PoolItemIndex = -1;
		}

		public void UpdateCellContent_Delegated()
		{
			this.args[1] = (uint)this.Instances;
			this.RelatedGrassManager.argsBuffers[this.PoolItemIndex].SetData(this.args);
			this.state = 3;
		}

		public void InitCellContent_Delegated()
		{
			uint num = (!(this.v_mesh != null)) ? 0u : this.v_mesh.GetIndexCount(0);
			this.args[0] = num;
			this.args[1] = (uint)this.Instances;
			this.RelatedGrassManager.argsBuffers[this.PoolItemIndex].SetData(this.args);
			this.bounds.center = this.Center;
			float num2 = (this.Center.x - this.Pivot.x) * 2f;
			this.bounds.extents = new Vector3(num2, num2, num2);
			this.state = 3;
		}

		public void DrawCellContent_Delegated(Camera CameraInWichGrassWillBeDrawn, int CameraLayer)
		{
			Graphics.DrawMeshInstancedIndirect(this.v_mesh, 0, this.v_mat, this.bounds, this.RelatedGrassManager.argsBuffers[this.PoolItemIndex], 0, this.RelatedGrassManager.materialPropertyBlocks[this.PoolItemIndex], this.ShadowCastingMode, true, CameraLayer, CameraInWichGrassWillBeDrawn);
		}

		public GrassManager RelatedGrassManager;

		public int PoolItemIndex = -1;

		public int index;

		public int Layer;

		public int[] SoftlyMergedLayers;

		public int state;

		public Mesh v_mesh;

		public Material v_mat;

		public int GrassMatrixBufferPID;

		public ShadowCastingMode ShadowCastingMode;

		public int Instances;

		public Vector3 Center;

		public Vector3 Pivot;

		public int PatchOffsetX;

		public int PatchOffsetZ;

		private uint[] args = new uint[5];

		private Bounds bounds = default(Bounds);
	}
}
