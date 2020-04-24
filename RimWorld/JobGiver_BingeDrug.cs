using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_BingeDrug : JobGiver_Binge
	{
		private const int BaseIngestInterval = 600;

		private const float OverdoseSeverityToAvoid = 0.786f;

		private static readonly SimpleCurve IngestIntervalFactorCurve_Drunkness = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(1f, 4f)
		};

		private static readonly SimpleCurve IngestIntervalFactorCurve_DrugOverdose = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(1f, 5f)
		};

		protected override int IngestInterval(Pawn pawn)
		{
			ChemicalDef chemical = GetChemical(pawn);
			int num = 600;
			if (chemical == ChemicalDefOf.Alcohol)
			{
				Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.AlcoholHigh);
				if (firstHediffOfDef != null)
				{
					num = (int)((float)num * IngestIntervalFactorCurve_Drunkness.Evaluate(firstHediffOfDef.Severity));
				}
			}
			else
			{
				Hediff firstHediffOfDef2 = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.DrugOverdose);
				if (firstHediffOfDef2 != null)
				{
					num = (int)((float)num * IngestIntervalFactorCurve_DrugOverdose.Evaluate(firstHediffOfDef2.Severity));
				}
			}
			return num;
		}

		protected override Thing BestIngestTarget(Pawn pawn)
		{
			ChemicalDef chemical = GetChemical(pawn);
			DrugCategory drugCategory = GetDrugCategory(pawn);
			if (chemical == null)
			{
				Log.ErrorOnce("Tried to binge on null chemical.", 1393746152);
				return null;
			}
			Hediff overdose = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.DrugOverdose);
			Predicate<Thing> validator = delegate(Thing t)
			{
				if (!IgnoreForbid(pawn) && t.IsForbidden(pawn))
				{
					return false;
				}
				if (!pawn.CanReserve(t))
				{
					return false;
				}
				CompDrug compDrug = t.TryGetComp<CompDrug>();
				if (compDrug.Props.chemical != chemical)
				{
					return false;
				}
				if (overdose != null && compDrug.Props.CanCauseOverdose && overdose.Severity + compDrug.Props.overdoseSeverityOffset.max >= 0.786f)
				{
					return false;
				}
				if (!pawn.Position.InHorDistOf(t.Position, 60f) && !t.Position.Roofed(t.Map) && !pawn.Map.areaManager.Home[t.Position] && t.GetSlotGroup() == null)
				{
					return false;
				}
				return t.def.ingestible.drugCategory.IncludedIn(drugCategory) ? true : false;
			};
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Drug), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, validator);
		}

		private ChemicalDef GetChemical(Pawn pawn)
		{
			return ((MentalState_BingingDrug)pawn.MentalState).chemical;
		}

		private DrugCategory GetDrugCategory(Pawn pawn)
		{
			return ((MentalState_BingingDrug)pawn.MentalState).drugCategory;
		}
	}
}
