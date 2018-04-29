using System;
using UnityEngine;

namespace TheForest.Modding.UI
{
	
	public class ModList : MonoBehaviour
	{
		
		private void OnEnable()
		{
			this.Refresh();
		}

		
		public void Refresh()
		{
		}

		
		private void ClearUI()
		{
			if (this._grid)
			{
				int i = this._grid.transform.childCount;
				while (i > 0)
				{
					UnityEngine.Object.Destroy(this._grid.transform.GetChild(--i).gameObject);
				}
			}
		}

		
		public string _dirPath = "./Mods/";

		
		public ModRow _rowPrefab;

		
		public UIGrid _grid;
	}
}
