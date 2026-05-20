using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class IncidentWorker_RevenantEmergence : IncidentWorker
{
	private static readonly IntRange EmergenceDelay = new IntRange(7500, 12500);

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (parms.target is Caravan caravan)
		{
			Thing thing;
			Pawn owner;
			return CaravanInventoryUtility.TryGetThingOfDef(caravan, ThingDefOf.RevenantSpine, out thing, out owner);
		}
		if (parms.target is Map map)
		{
			return map.listerThings.ThingsOfDef(ThingDefOf.RevenantSpine).Count > 0;
		}
		return false;
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		if (parms.target is Caravan caravan)
		{
			CaravanInventoryUtility.TryGetThingOfDef(caravan, ThingDefOf.RevenantSpine, out var thing, out var _);
			if (thing == null)
			{
				return false;
			}
			CompSpawnsRevenant compSpawnsRevenant = thing.TryGetComp<CompSpawnsRevenant>();
			if (compSpawnsRevenant.spawnTick > 0)
			{
				return false;
			}
			compSpawnsRevenant.spawnTick = Find.TickManager.TicksGame + EmergenceDelay.RandomInRange;
			IncidentWorker.SendIncidentLetter(def.letterLabel, def.letterText, LetterDefOf.NegativeEvent, parms, caravan, def);
			return true;
		}
		if (parms.target is Map map)
		{
			(from s in map.listerThings.ThingsOfDef(ThingDefOf.RevenantSpine)
				where s.TryGetComp<CompSpawnsRevenant>().spawnTick < 0
				select s).TryRandomElement(out var result);
			if (result == null)
			{
				return false;
			}
			result.TryGetComp<CompSpawnsRevenant>().spawnTick = Find.TickManager.TicksGame + EmergenceDelay.RandomInRange;
			IncidentWorker.SendIncidentLetter(def.letterLabel, def.letterText, LetterDefOf.NegativeEvent, parms, result, def);
			return true;
		}
		return false;
	}
}
