using System;
using System.Collections.Generic;
using PathologicalGames;
using TheForest.Buildings.Interfaces;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	public class MultipointShapeGizmo : MonoBehaviour
	{
		
		private void Awake()
		{
			this._gizmos = new List<Transform>();
			base.enabled = false;
		}

		
		private void Update()
		{
			if (this.CheckStructure())
			{
				this.DespawnGizmos();
				this.SpawnGizmos();
			}
			else
			{
				this.Shutdown();
			}
		}

		
		public void Show(IStructureSupport structure, Vector3 offset = default(Vector3))
		{
			this._offset = offset;
			this._structure = structure;
			base.enabled = true;
		}

		
		public void Shutdown()
		{
			this.DespawnGizmos();
			this._structure = null;
			base.enabled = false;
		}

		
		private bool CheckStructure()
		{
			return this._structure != null && this._structure as MonoBehaviour;
		}

		
		private void DespawnGizmos()
		{
			for (int i = 0; i < this._gizmos.Count; i++)
			{
				PoolManager.Pools["misc"].Despawn(this._gizmos[i]);
			}
			this._gizmos.Clear();
		}

		
		private void SpawnGizmos()
		{
			List<Vector3> multiPointsPositions = this._structure.GetMultiPointsPositions(true);
			if (multiPointsPositions.Count > 0)
			{
				Transform item = PoolManager.Pools["misc"].Spawn(this._pointGizmo, multiPointsPositions[0] + this._offset, Quaternion.identity);
				this._gizmos.Add(item);
				Transform transform;
				for (int i = 1; i < multiPointsPositions.Count; i++)
				{
					item = PoolManager.Pools["misc"].Spawn(this._pointGizmo, multiPointsPositions[i] + this._offset, Quaternion.identity);
					transform = PoolManager.Pools["misc"].Spawn(this._lineGizmo, multiPointsPositions[i] + this._offset, Quaternion.LookRotation(multiPointsPositions[i - 1] - multiPointsPositions[i]));
					transform.localScale = new Vector3(1f, 1f, Vector3.Distance(multiPointsPositions[i], multiPointsPositions[i - 1]));
					this._gizmos.Add(item);
					this._gizmos.Add(transform);
				}
				Vector3 vector = multiPointsPositions[multiPointsPositions.Count - 1];
				Vector3 vector2;
				if (this._structure is FloorArchitect)
				{
					vector2 = (this._structure as FloorArchitect).GetCurrentEdgePoint();
				}
				else if (this._structure is FoundationArchitect)
				{
					vector2 = (this._structure as FoundationArchitect).GetCurrentEdgePoint();
				}
				else if (this._structure is RoofArchitect)
				{
					RoofArchitect roofArchitect = this._structure as RoofArchitect;
					if (roofArchitect.LockMode == RoofArchitect.LockModes.Height)
					{
						return;
					}
					vector2 = roofArchitect.GetCurrentEdgePoint();
				}
				else
				{
					vector2 = (this._structure as MonoBehaviour).transform.position;
					vector2.y = vector.y;
				}
				item = PoolManager.Pools["misc"].Spawn(this._pointGizmo, vector2 + this._offset, Quaternion.identity);
				transform = PoolManager.Pools["misc"].Spawn(this._lineGizmo, vector2 + this._offset, Quaternion.LookRotation(vector - vector2));
				transform.localScale = new Vector3(1f, 1f, Vector3.Distance(vector2, vector));
				this._gizmos.Add(item);
				this._gizmos.Add(transform);
			}
			else
			{
				Vector3 vector2;
				if (this._structure is FloorArchitect)
				{
					vector2 = (this._structure as FloorArchitect).GetCurrentEdgePoint();
				}
				else if (this._structure is RoofArchitect)
				{
					vector2 = (this._structure as RoofArchitect).GetCurrentEdgePoint();
				}
				else
				{
					vector2 = (this._structure as MonoBehaviour).transform.position;
					vector2.y = this._structure.GetLevel();
				}
				Transform item = PoolManager.Pools["misc"].Spawn(this._pointGizmo, vector2 + this._offset, Quaternion.identity);
				this._gizmos.Add(item);
			}
		}

		
		public Transform _pointGizmo;

		
		public Transform _lineGizmo;

		
		private IStructureSupport _structure;

		
		private Vector3 _offset;

		
		private List<Transform> _gizmos;
	}
}
