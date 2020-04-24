using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class TameUtility
	{
		public const int MinTameInterval = 30000;

		public static void ShowDesignationWarnings(Pawn pawn, bool showManhunterOnTameFailWarning = true)
		{
			if (showManhunterOnTameFailWarning)
			{
				float manhunterOnTameFailChance = pawn.RaceProps.manhunterOnTameFailChance;
				if (manhunterOnTameFailChance >= 0.015f)
				{
					Messages.Message("MessageAnimalManhuntsOnTameFailed".Translate(pawn.kindDef.GetLabelPlural().CapitalizeFirst(), manhunterOnTameFailChance.ToStringPercent(), pawn.Named("ANIMAL")), pawn, MessageTypeDefOf.CautionInput, historical: false);
				}
			}
			IEnumerable<Pawn> source = pawn.Map.mapPawns.FreeColonistsSpawned.Where((Pawn c) => c.workSettings.WorkIsActive(WorkTypeDefOf.Handling));
			if (!source.Any())
			{
				source = pawn.Map.mapPawns.FreeColonistsSpawned;
			}
			if (source.Any())
			{
				Pawn pawn2 = source.MaxBy((Pawn c) => c.skills.GetSkill(SkillDefOf.Animals).Level);
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
			if (pawn.AnimalOrWildMan() && (pawn.Faction == null || !pawn.Faction.def.humanlikeFaction))
			{
				return pawn.RaceProps.wildness < 1f;
			}
			return false;
		}

		public static bool TriedToTameTooRecently(Pawn animal)
		{
			return Find.TickManager.TicksGame < animal.mindState.lastAssignedInteractTime + 30000;
		}
	}
}
