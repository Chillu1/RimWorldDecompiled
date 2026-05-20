using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class ComplexThreatWorker_SecurityCrate : ComplexThreatWorker
{
	public IEnumerable<ComplexThreatDef> SubThreats
	{
		get
		{
			yield return ComplexThreatDefOf.SleepingInsects;
			yield return ComplexThreatDefOf.SleepingMechanoids;
			yield return ComplexThreatDefOf.Infestation;
			yield return ComplexThreatDefOf.CryptosleepPods;
		}
	}

	protected override bool CanResolveInt(ComplexResolveParams parms)
	{
		if (base.CanResolveInt(parms) && ComplexUtility.TryFindRandomSpawnCell(ThingDefOf.AncientSecurityCrate, parms.room, parms.map, out var _))
		{
			return SubThreats.Any((ComplexThreatDef st) => st.Worker.CanResolve(parms));
		}
		return false;
	}

	protected override void ResolveInt(ComplexResolveParams parms, ref float threatPointsUsed, List<Thing> outSpawnedThings)
	{
		ComplexUtility.TryFindRandomSpawnCell(ThingDefOf.AncientSecurityCrate, parms.room, parms.map, out var spawnPosition);
		Building_Crate building_Crate = (Building_Crate)GenSpawn.Spawn(ThingDefOf.AncientSecurityCrate, spawnPosition, parms.map);
		List<Thing> list = ThingSetMakerDefOf.MapGen_AncientComplex_SecurityCrate.root.Generate(default(ThingSetMakerParams));
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (!building_Crate.TryAcceptThing(list[num], allowSpecialEffects: false))
			{
				list[num].Destroy();
			}
		}
		outSpawnedThings.Add(building_Crate);
		parms.spawnedThings.Add(building_Crate);
		if (building_Crate.openedSignal.NullOrEmpty())
		{
			building_Crate.openedSignal = "CrateOpened" + Find.UniqueIDsManager.GetNextSignalTagID();
		}
		parms.triggerSignal = building_Crate.openedSignal;
		if (SubThreats.Where((ComplexThreatDef st) => st.Worker.CanResolve(parms)).TryRandomElement(out var result))
		{
			float threatPointsUsed2 = 0f;
			result.Worker.Resolve(parms, ref threatPointsUsed2, outSpawnedThings);
			threatPointsUsed += threatPointsUsed2;
		}
		else
		{
			Log.Warning("Failed to find a viable subthreat when placing a security crate threat.");
		}
	}
}
