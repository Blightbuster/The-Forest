using System;
using Bolt;

public class CoopForceDetachNestedEntities : EntityBehaviour
{
	public override void Detached()
	{
		foreach (BoltEntity boltEntity in base.GetComponentsInChildren<BoltEntity>())
		{
			boltEntity.transform.parent = null;
		}
	}
}
