using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse;

public class Gene_PsychicBonding : Gene
{
	private Pawn bondedPawn;

	public bool CanBondToNewPawn
	{
		get
		{
			if (bondedPawn != null)
			{
				return false;
			}
			if (pawn.health.hediffSet.HasHediff(HediffDefOf.PsychicBondTorn))
			{
				return false;
			}
			if (pawn.needs?.mood?.thoughts?.memories?.GetFirstMemoryOfDef(ThoughtDefOf.PsychicBondTorn) != null)
			{
				return false;
			}
			return true;
		}
	}

	public override void PostAdd()
	{
		base.PostAdd();
		if (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond) is Hediff_PsychicBond hediff_PsychicBond)
		{
			bondedPawn = (Pawn)hediff_PsychicBond.target;
		}
	}

	public override void PostRemove()
	{
		base.PostRemove();
		Notify_MyOrPartnersGeneRemoved();
	}

	public void BondTo(Pawn newBond)
	{
		if (!ModLister.CheckBiotech("Psychic bonding") || newBond == null || bondedPawn == newBond)
		{
			return;
		}
		if (bondedPawn != null)
		{
			Log.Error("Tried to bond to more than one pawn.");
			return;
		}
		bondedPawn = newBond;
		pawn.needs?.mood?.thoughts?.memories?.RemoveMemoriesOfDefIf(ThoughtDefOf.PsychicBondTorn, (Thought_Memory m) => m.otherPawn == bondedPawn);
		bondedPawn.needs?.mood?.thoughts?.memories?.RemoveMemoriesOfDefIf(ThoughtDefOf.PsychicBondTorn, (Thought_Memory m) => m.otherPawn == pawn);
		Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBondTorn);
		if (firstHediffOfDef != null)
		{
			pawn.health.RemoveHediff(firstHediffOfDef);
		}
		Hediff firstHediffOfDef2 = bondedPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBondTorn);
		if (firstHediffOfDef2 != null)
		{
			bondedPawn.health.RemoveHediff(firstHediffOfDef2);
		}
		Hediff_PsychicBond hediff_PsychicBond = (Hediff_PsychicBond)HediffMaker.MakeHediff(HediffDefOf.PsychicBond, pawn);
		hediff_PsychicBond.target = bondedPawn;
		pawn.health.AddHediff(hediff_PsychicBond);
		Gene_PsychicBonding gene_PsychicBonding = bondedPawn.genes?.GetFirstGeneOfType<Gene_PsychicBonding>();
		if (gene_PsychicBonding != null)
		{
			gene_PsychicBonding.BondTo(pawn);
			return;
		}
		Hediff_PsychicBond hediff_PsychicBond2 = (Hediff_PsychicBond)HediffMaker.MakeHediff(HediffDefOf.PsychicBond, bondedPawn);
		hediff_PsychicBond2.target = pawn;
		bondedPawn.health.AddHediff(hediff_PsychicBond2);
	}

	public void RemoveBond()
	{
		if (bondedPawn == null)
		{
			return;
		}
		base.pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.PsychicBondTorn, bondedPawn);
		bondedPawn.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.PsychicBondTorn, base.pawn);
		Pawn pawn = bondedPawn;
		bondedPawn = null;
		if (base.pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond) is Hediff_PsychicBond hediff_PsychicBond && hediff_PsychicBond.target == pawn)
		{
			base.pawn.health.RemoveHediff(hediff_PsychicBond);
		}
		if (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond) is Hediff_PsychicBond hediff)
		{
			pawn.health.RemoveHediff(hediff);
		}
		pawn.genes?.GetFirstGeneOfType<Gene_PsychicBonding>()?.RemoveBond();
		if (!base.pawn.Dead)
		{
			if (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBondTorn) == null)
			{
				Hediff_PsychicBondTorn hediff_PsychicBondTorn = (Hediff_PsychicBondTorn)HediffMaker.MakeHediff(HediffDefOf.PsychicBondTorn, pawn);
				hediff_PsychicBondTorn.target = base.pawn;
				pawn.health.AddHediff(hediff_PsychicBondTorn);
			}
			if (DefDatabase<MentalBreakDef>.AllDefsListForReading.Where((MentalBreakDef d) => d.intensity == MentalBreakIntensity.Extreme && d.Worker.BreakCanOccur(base.pawn)).TryRandomElementByWeight((MentalBreakDef d) => d.Worker.CommonalityFor(base.pawn, moodCaused: true), out var result))
			{
				result.Worker.TryStart(base.pawn, "MentalStateReason_BondedHumanDeath".Translate(pawn), causedByMood: false);
			}
		}
	}

	public void Notify_MyOrPartnersGeneRemoved()
	{
		if (bondedPawn != null && bondedPawn.genes?.GetFirstGeneOfType<Gene_PsychicBonding>() == null)
		{
			Pawn pawn = bondedPawn;
			bondedPawn = null;
			if (base.pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond) is Hediff_PsychicBond hediff_PsychicBond && hediff_PsychicBond.target == pawn)
			{
				base.pawn.health.RemoveHediff(hediff_PsychicBond);
			}
			if (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond) is Hediff_PsychicBond hediff)
			{
				pawn.health.RemoveHediff(hediff);
			}
			pawn.genes?.GetFirstGeneOfType<Gene_PsychicBonding>()?.Notify_MyOrPartnersGeneRemoved();
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		if (!DebugSettings.ShowDevGizmos || !CanBondToNewPawn || !pawn.Spawned)
		{
			yield break;
		}
		yield return new Command_Action
		{
			defaultLabel = "DEV: Bond to random pawn",
			action = delegate
			{
				if ((from x in pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction)
					where x.RaceProps.Humanlike && x != pawn
					select x).TryRandomElement(out var result))
				{
					BondTo(result);
				}
			}
		};
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref bondedPawn, "bondedPawn");
	}
}
