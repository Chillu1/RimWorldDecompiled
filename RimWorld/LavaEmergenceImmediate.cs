using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class LavaEmergenceImmediate : LavaEmergence
{
	protected override IntRange PoolSizeRange => new IntRange(100, 150);

	protected override bool FireLetter => false;

	protected override void Tick()
	{
		while (lavaCells.Count < poolSize && openCells.Count > 0)
		{
			SpreadLava();
		}
		BeginCooling();
	}

	protected override IEnumerable<IntVec3> GetInitialCells(Map map)
	{
		yield return base.Position;
	}
}
