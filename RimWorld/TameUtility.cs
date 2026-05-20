using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public static class TameUtility
{
	public const int MinTameInterval = 30000;

	public static void ShowDesignationWarnings(Pawn pawn, bool showManhunterOnTameFailWarning = true)
	{
		if (showManhunterOnTameFailWarning)
		{
			float manhunterOnTameFailChance = PawnUtility.GetManhunterOnTameFailChance(pawn);
			if (manhunterOnTameFailChance >= 0.015f)
			{
				Messages.Message("MessageAnimalManhuntsOnTameFailed".Translate(pawn.kindDef.GetLabelPlural().CapitalizeFirst(), manhunterOnTameFailChance.ToStringPercent(), pawn.Named("ANIMAL")), pawn, MessageTypeDefOf.CautionInput, historical: false);
			}
		}
		if (pawn.RaceProps.Insect && pawn.HostileTo(Faction.OfPlayer))
		{
			Messages.Message("MessageInsectsHostileOnTaming".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.CautionInput, historical: false);
		}
		IEnumerable<Pawn> source = pawn.Map.mapPawns.FreeColonistsSpawned.Where((Pawn c) => c.workSettings.WorkIsActive(WorkTypeDefOf.Handling));
		if (!source.Any())
		{
			source = pawn.Map.mapPawns.FreeColonistsSpawned;
		}
		if (!source.Any())
		{
			return;
		}
		IEnumerable<Pawn> source2 = source.Where((Pawn p) => p.health.capacities.CapableOf(PawnCapacityDefOf.Talking) && p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
		if (source2.Any())
		{
			Pawn pawn2 = source2.MaxBy((Pawn p) => p.skills.GetSkill(SkillDefOf.Animals).Level);
			int level = pawn2.skills.GetSkill(SkillDefOf.Animals).Level;
			int num = TrainableUtility.MinimumHandlingSkill(pawn);
			if (num > level)
			{
				Messages.Message("MessageNoHandlerSkilledEnough".Translate(pawn.kindDef.label, num.ToStringCached(), SkillDefOf.Animals.LabelCap, pawn2.LabelShort, level, pawn.Named("ANIMAL"), pawn2.Named("HANDLER")), pawn, MessageTypeDefOf.CautionInput, historical: false);
			}
		}
	}

	public static bool CanTame(Pawn pawn)
	{
		if (pawn.AnimalOrWildMan() && (pawn.Faction == null || !pawn.Faction.def.humanlikeFaction) && pawn.GetStatValue(StatDefOf.Wildness) < 1f && pawn.RaceProps.animalType != AnimalType.Dryad)
		{
			return !pawn.health.hediffSet.HasHediff(HediffDefOf.Scaria);
		}
		return false;
	}

	public static bool TriedToTameTooRecently(Pawn animal)
	{
		return Find.TickManager.TicksGame < animal.mindState.lastAssignedInteractTime + 30000;
	}
}
