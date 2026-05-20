using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class Building_CommsConsole : Building
{
	private CompPowerTrader powerComp;

	public bool CanUseCommsNow
	{
		get
		{
			if (base.Spawned && base.Map.gameConditionManager.ElectricityDisabled(base.Map))
			{
				return false;
			}
			if (powerComp != null)
			{
				return powerComp.PowerOn;
			}
			return true;
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		powerComp = GetComp<CompPowerTrader>();
		LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.GoodToKnow);
		LessonAutoActivator.TeachOpportunity(ConceptDefOf.OpeningComms, OpportunityType.GoodToKnow);
		if (CanUseCommsNow)
		{
			LongEventHandler.ExecuteWhenFinished(AnnounceTradeShips);
		}
	}

	private void UseAct(Pawn myPawn, ICommunicable commTarget)
	{
		Job job = JobMaker.MakeJob(JobDefOf.UseCommsConsole, this);
		job.commTarget = commTarget;
		myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
	}

	private FloatMenuOption GetFailureReason(Pawn myPawn)
	{
		if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some))
		{
			return new FloatMenuOption("CannotUseNoPath".Translate(), null);
		}
		if (base.Spawned && base.Map.gameConditionManager.ElectricityDisabled(base.Map))
		{
			return new FloatMenuOption("CannotUseSolarFlare".Translate(), null);
		}
		if (powerComp != null && !powerComp.PowerOn)
		{
			return new FloatMenuOption("CannotUseNoPower".Translate(), null);
		}
		if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
		{
			return new FloatMenuOption("CannotUseReason".Translate("IncapableOfCapacity".Translate(PawnCapacityDefOf.Talking.label, myPawn.Named("PAWN"))), null);
		}
		if (!GetCommTargets(myPawn).Any())
		{
			return new FloatMenuOption("CannotUseReason".Translate("NoCommsTarget".Translate()), null);
		}
		if (!CanUseCommsNow)
		{
			Log.Error(myPawn?.ToString() + " could not use comm console for unknown reason.");
			return new FloatMenuOption("Cannot use now", null);
		}
		return null;
	}

	public IEnumerable<ICommunicable> GetCommTargets(Pawn myPawn)
	{
		return myPawn.Map.passingShipManager.passingShips.Cast<ICommunicable>().Concat(Find.FactionManager.AllFactionsVisibleInViewOrder.Where((Faction f) => !f.temporary && !f.IsPlayer).Cast<ICommunicable>());
	}

	public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
	{
		FloatMenuOption failureReason = GetFailureReason(myPawn);
		if (failureReason != null)
		{
			yield return failureReason;
			yield break;
		}
		foreach (ICommunicable commTarget in GetCommTargets(myPawn))
		{
			FloatMenuOption floatMenuOption = commTarget.CommFloatMenuOption(this, myPawn);
			if (floatMenuOption != null)
			{
				yield return floatMenuOption;
			}
		}
		foreach (FloatMenuOption floatMenuOption2 in base.GetFloatMenuOptions(myPawn))
		{
			yield return floatMenuOption2;
		}
	}

	public void GiveUseCommsJob(Pawn negotiator, ICommunicable target)
	{
		Job job = JobMaker.MakeJob(JobDefOf.UseCommsConsole, this);
		job.commTarget = target;
		negotiator.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
	}

	private void AnnounceTradeShips()
	{
		foreach (TradeShip item in from s in base.Map.passingShipManager.passingShips.OfType<TradeShip>()
			where !s.WasAnnounced
			select s)
		{
			TaggedString baseLetterText = "TraderArrival".Translate(item.name, item.def.label, (item.Faction == null) ? "TraderArrivalNoFaction".Translate() : "TraderArrivalFromFaction".Translate(item.Faction.Named("FACTION")));
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = base.Map;
			incidentParms.traderKind = item.TraderKind;
			IncidentWorker.SendIncidentLetter(item.def.LabelCap, baseLetterText, LetterDefOf.PositiveEvent, incidentParms, LookTargets.Invalid, null);
			item.WasAnnounced = true;
		}
	}
}
