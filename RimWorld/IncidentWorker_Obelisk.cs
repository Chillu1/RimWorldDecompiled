using Verse;

namespace RimWorld;

public abstract class IncidentWorker_Obelisk : IncidentWorker
{
	public abstract ThingDef ObeliskDef { get; }

	public override float ChanceFactorNow(IIncidentTarget target)
	{
		if (!(target is Map map))
		{
			return base.ChanceFactorNow(target);
		}
		int num = map.listerBuildings.allBuildingsNonColonist.Count((Building b) => b.def.GetCompProperties<CompProperties_Obelisk>() != null);
		return ((num > 0) ? ((float)num * 0.7f) : 1f) * base.ChanceFactorNow(target);
	}

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		IntVec3 cell;
		return TryFindCell(out cell, map, ObeliskDef);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		Skyfaller skyfaller = SpawnObeliskIncoming(map);
		if (skyfaller == null)
		{
			return false;
		}
		skyfaller.impactLetter = LetterMaker.MakeLetter(def.letterLabel, def.letterText, def.letterDef ?? LetterDefOf.NeutralEvent, new TargetInfo(skyfaller.Position, map));
		return true;
	}

	private Skyfaller SpawnObeliskIncoming(Map map)
	{
		if (!TryFindCell(out var cell, map, ObeliskDef))
		{
			return null;
		}
		return SkyfallerMaker.SpawnSkyfaller(ThingDefOf.WarpedObeliskIncoming, ThingMaker.MakeThing(ObeliskDef), cell, map);
	}

	private bool TryFindCell(out IntVec3 cell, Map map, ThingDef obeliskDef)
	{
		return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.WarpedObeliskIncoming, map, ObeliskDef.terrainAffordanceNeeded, out cell, 10, default(IntVec3), -1, allowRoofedCells: true, allowCellsWithItems: false, allowCellsWithBuildings: false, colonyReachable: false, avoidColonistsIfExplosive: true, alwaysAvoidColonists: true, delegate(IntVec3 x)
		{
			if ((float)x.DistanceToEdge(map) < 20f + (float)map.Size.x * 0.1f)
			{
				return false;
			}
			foreach (IntVec3 item in CellRect.CenteredOn(x, obeliskDef.Size.x, obeliskDef.Size.z))
			{
				if (!item.InBounds(map) || !item.Standable(map) || !item.GetAffordances(map).Contains(obeliskDef.terrainAffordanceNeeded))
				{
					return false;
				}
			}
			return true;
		});
	}
}
