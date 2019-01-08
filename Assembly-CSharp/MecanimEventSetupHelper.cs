using System;
using UnityEngine;

public class MecanimEventSetupHelper : MonoBehaviour
{
	private void Awake()
	{
		if (this.dataSource == null && (this.dataSources == null || this.dataSources.Length == 0))
		{
			Debug.Log("Please setup data source of event system.");
			return;
		}
		if (this.dataSource != null)
		{
			MecanimEventManager.SetEventDataSource(this.dataSource);
		}
		else
		{
			MecanimEventManager.SetEventDataSource(this.dataSources);
		}
	}

	private void refreshDataSources()
	{
		if (this.dataSource != null)
		{
			MecanimEventManager.SetEventDataSource(this.dataSource);
		}
		else
		{
			MecanimEventManager.SetEventDataSource(this.dataSources);
		}
	}

	public MecanimEventData dataSource;

	public MecanimEventData[] dataSources;
}
