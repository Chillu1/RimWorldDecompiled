using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompGiveHediffSeverity : ThingComp
{
	private const int UpdateInterval = 87;

	private const float ChemicalSatisfactionPerSecond = 0.1f;

	private CompProperties_GiveHediffSeverity Props => (CompProperties_GiveHediffSeverity)props;

	private bool AppliesTo(Pawn pawn)
	{
		if (pawn.GetRoom() != parent.GetRoom())
		{
			return false;
		}
		if (!Props.allowMechs && !pawn.RaceProps.IsFlesh)
		{
			return false;
		}
		return true;
	}

	public override void CompTick()
	{
		if (!parent.Spawned || !parent.IsHashIntervalTick(87))
		{
			return;
		}
		CompRefuelable compRefuelable = parent.TryGetComp<CompRefuelable>();
		CompPowerTrader compPowerTrader = parent.TryGetComp<CompPowerTrader>();
		if ((compRefuelable != null && !compRefuelable.HasFuel) || (compPowerTrader != null && !compPowerTrader.PowerOn))
		{
			return;
		}
		int num = GenRadial.NumCellsInRadius(Props.range);
		for (int i = 0; i < num; i++)
		{
			List<Thing> thingList = (parent.Position + GenRadial.RadialPattern[i]).GetThingList(parent.Map);
			for (int j = 0; j < thingList.Count; j++)
			{
				if (!(thingList[j] is Pawn pawn) || !AppliesTo(pawn))
				{
					continue;
				}
				Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
				float num2 = Props.severityPerSecond * 1.45f;
				if (firstHediffOfDef != null)
				{
					firstHediffOfDef.Severity += num2;
				}
				else
				{
					pawn.health.AddHediff(Props.hediff).Severity = num2;
				}
				if (!Props.drugExposure)
				{
					continue;
				}
				pawn.mindState.lastTakeRecreationalDrugTick = Find.TickManager.TicksGame;
				if (pawn.needs?.drugsDesire != null)
				{
					pawn.needs.drugsDesire.CurLevel += 0.14500001f;
				}
				if (Props.chemical?.addictionHediff != null && pawn.needs != null)
				{
					HediffDef addictionHediffDef = Props.chemical.addictionHediff;
					Need need = pawn.needs.AllNeeds.Find((Need x) => x.def == addictionHediffDef.chemicalNeed);
					if (need != null)
					{
						need.CurLevel += 0.14500001f;
					}
				}
			}
		}
	}
}
