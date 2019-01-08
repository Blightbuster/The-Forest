using System;
using UnityEngine;

namespace AdvancedTerrainGrass
{
	public class GrassCutter : MonoBehaviour
	{
		private void Start()
		{
			this.trans = base.transform;
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				this.Grassmanager.RemoveGrass(this.trans.position, this.trans.lossyScale.x * 0.5f);
				Vector3 position = this.trans.position;
				position.z += 15f;
				this.trans.position = position;
			}
		}

		public GrassManager Grassmanager;

		private Transform trans;
	}
}
