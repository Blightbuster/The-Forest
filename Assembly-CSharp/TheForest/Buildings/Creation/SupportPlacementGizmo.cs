using System;
using TheForest.Buildings.Interfaces;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	public class SupportPlacementGizmo : MonoBehaviour
	{
		public void Show(IStructureSupport ghost, IStructureSupport support, Collider supportCol, float offsetWithSupport)
		{
			Transform transform = (ghost as MonoBehaviour).transform;
			Transform transform2 = (support as MonoBehaviour).transform;
			if (support is WallChunkArchitect)
			{
				if (this._showHorizontalGrid)
				{
					WallChunkArchitect wallChunkArchitect = (WallChunkArchitect)support;
					Vector3 position = transform2.position;
					position.y = support.GetLevel() + offsetWithSupport;
					this._horizontal.position = position;
					this._horizontal.rotation = Quaternion.LookRotation(transform2.forward);
					this._horizontal.localScale = new Vector3(1f, 1f, Vector3.Distance(wallChunkArchitect.P1, wallChunkArchitect.P2));
					this._horizontalMat.mainTextureScale = new Vector2(6.25f, 5f * this._horizontal.localScale.z);
					this._horizontal.gameObject.SetActive(true);
				}
				if (this._showVerticalGrid)
				{
					Vector3 position2 = transform.position;
					position2.y = support.GetLevel();
					this._edge1.position = position2;
					int count = ghost.GetMultiPointsPositions(true).Count;
					if (count > 0)
					{
						Vector3 forward = ghost.GetMultiPointsPositions(true).Last<Vector3>() - position2;
						forward.y = 0f;
						this._edge1.rotation = Quaternion.LookRotation(forward);
					}
					else
					{
						this._edge1.rotation = this._horizontal.rotation;
					}
					this._edge1.gameObject.SetActive(true);
					if (count > 1)
					{
						this._edge2.position = position2;
						Vector3 forward2 = ghost.GetMultiPointsPositions(true).First<Vector3>() - position2;
						forward2.y = 0f;
						this._edge2.rotation = Quaternion.LookRotation(forward2);
						this._edge2.gameObject.SetActive(true);
					}
					else
					{
						this._edge2.gameObject.SetActive(false);
					}
				}
			}
		}

		public void Hide()
		{
			this._horizontal.gameObject.SetActive(false);
			this._edge1.gameObject.SetActive(false);
			this._edge2.gameObject.SetActive(false);
		}

		public Transform _horizontal;

		public Transform _edge1;

		public Transform _edge2;

		public Material _horizontalMat;

		public Material _edgeMat;

		public bool _showHorizontalGrid;

		public bool _showVerticalGrid;
	}
}
