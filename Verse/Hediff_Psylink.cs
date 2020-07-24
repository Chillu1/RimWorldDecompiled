using RimWorld;
using System;
using System.Linq;

namespace Verse
{
	public class Hediff_Psylink : Hediff_ImplantWithLevel
	{
		public override void PostAdd(DamageInfo? dinfo)
		{
			base.PostAdd(dinfo);
			TryGiveAbilityOfLevel(level);
			pawn.psychicEntropy?.Notify_GainedPsylink();
		}

		public override void ChangeLevel(int levelOffset)
		{
			if (levelOffset > 0)
			{
				float num = Math.Min(levelOffset, def.maxSeverity - (float)level);
				for (int i = 0; (float)i < num; i++)
				{
					int abilityLevel = level + 1 + i;
					TryGiveAbilityOfLevel(abilityLevel);
					pawn.psychicEntropy?.Notify_GainedPsylink();
				}
			}
			base.ChangeLevel(levelOffset);
		}

		public void TryGiveAbilityOfLevel(int abilityLevel)
		{
			string str = "LetterLabelPsylinkLevelGained".Translate() + ": " + pawn.LabelShortCap;
			string text = ((abilityLevel == 1) ? "LetterPsylinkLevelGained_First" : "LetterPsylinkLevelGained_NotFirst").Translate(pawn.Named("USER"));
			if (!pawn.abilities.abilities.Any((Ability a) => a.def.level == abilityLevel))
			{
				AbilityDef abilityDef = DefDatabase<AbilityDef>.AllDefs.Where((AbilityDef a) => a.level == abilityLevel).RandomElement();
				pawn.abilities.GainAbility(abilityDef);
				text += "\n\n" + "LetterPsylinkLevelGained_PsycastLearned".Translate(pawn.Named("USER"), abilityLevel, abilityDef.LabelCap);
			}
			if (PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				Find.LetterStack.ReceiveLetter(str, text, LetterDefOf.PositiveEvent, pawn);
			}
		}

		public override void PostRemoved()
		{
			base.PostRemoved();
			pawn.needs?.AddOrRemoveNeedsAsAppropriate();
		}
	}
}
