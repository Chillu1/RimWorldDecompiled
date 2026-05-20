using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class CompGrowsFleshmassLump : ThingComp
{
	private int nextSpawnTick;

	private List<IntVec3> lumpCells;

	private int lumpCellIndex;

	private CompProperties_GrowsFleshmassLump Props => (CompProperties_GrowsFleshmassLump)props;

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref lumpCellIndex, "lumpCellIndex", 0);
		Scribe_Values.Look(ref nextSpawnTick, "nextSpawnTick", 0);
		Scribe_Collections.Look(ref lumpCells, "lumpCells", LookMode.Value);
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!respawningAfterLoad)
		{
			nextSpawnTick = Find.TickManager.TicksGame + Props.ticksBetweenFleshmassRange.RandomInRange;
			lumpCells = GridShapeMaker.IrregularLump(parent.Position, parent.Map, Props.maxFleshmassRange.RandomInRange).ToList();
		}
	}

	public override void CompTick()
	{
		if (lumpCellIndex < lumpCells.Count - 1 && Find.TickManager.TicksGame >= nextSpawnTick)
		{
			IntVec3 intVec = lumpCells[lumpCellIndex];
			if (intVec.Standable(parent.Map))
			{
				GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Fleshmass_Active), intVec, parent.Map, WipeMode.FullRefund).SetFaction(parent.Faction);
				EffecterDefOf.MeatExplosion.Spawn(intVec, parent.Map).Cleanup();
			}
			nextSpawnTick = Find.TickManager.TicksGame + Props.ticksBetweenFleshmassRange.RandomInRange;
			lumpCellIndex++;
		}
	}
}
