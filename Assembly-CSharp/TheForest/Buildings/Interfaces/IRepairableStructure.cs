using System;

namespace TheForest.Buildings.Interfaces
{
	public interface IRepairableStructure
	{
		int CalcMissingRepairLogs();

		int CalcMissingRepairMaterial();

		int CalcTotalRepairMaterial();

		void AddRepairMaterial(bool isLog);

		int RepairLogs { get; }

		int CollapsedLogs { get; }

		int RepairMaterial { get; }

		bool CanBeRepaired { get; }
	}
}
