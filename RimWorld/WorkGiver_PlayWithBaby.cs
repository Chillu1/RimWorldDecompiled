using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_PlayWithBaby : WorkGiver_Scanner
{
	private const float BabyPlayNeedFullThreshold = 0.95f;

	public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.mapPawns.SpawnedBabiesInFaction(pawn.Faction);
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (pawn.IsBurning())
		{
			return true;
		}
		return false;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Pawn pawn2))
		{
			return false;
		}
		if (!pawn2.DevelopmentalStage.Baby())
		{
			return false;
		}
		if (pawn2.needs.play == null)
		{
			return false;
		}
		if (forced)
		{
			if (pawn2.needs.play.CurLevelPercentage >= 0.95f)
			{
				JobFailReason.Is("CannotInteractBabyPlayFull".Translate());
				return false;
			}
		}
		else
		{
			if (!pawn2.Awake())
			{
				return false;
			}
			if (!pawn2.needs.play.IsLow)
			{
				return false;
			}
		}
		foreach (BabyPlayDef item in DefDatabase<BabyPlayDef>.AllDefs.InRandomOrder())
		{
			if (item.Worker.CanDo(pawn, pawn2))
			{
				return true;
			}
		}
		return false;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Pawn baby = (Pawn)t;
		foreach (BabyPlayDef item in DefDatabase<BabyPlayDef>.AllDefs.InRandomOrder())
		{
			if (item.Worker.CanDo(pawn, baby))
			{
				return item.Worker.TryGiveJob(pawn, baby);
			}
		}
		return null;
	}
}
