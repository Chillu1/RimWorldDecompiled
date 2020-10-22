using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse
{
	public class Hediff_Psylink : Hediff_ImplantWithLevel
	{
		public bool suppressPostAddLetter;

		public override void PostAdd(DamageInfo? dinfo)
		{
			base.PostAdd(dinfo);
			TryGiveAbilityOfLevel_NewTemp(level, !suppressPostAddLetter);
			pawn.psychicEntropy?.Notify_GainedPsylink();
		}

		public void ChangeLevel(int levelOffset, bool sendLetter)
		{
			if (levelOffset > 0)
			{
				float num = Math.Min(levelOffset, def.maxSeverity - (float)level);
				for (int i = 0; (float)i < num; i++)
				{
					int abilityLevel = level + 1 + i;
					TryGiveAbilityOfLevel_NewTemp(abilityLevel, sendLetter);
					pawn.psychicEntropy?.Notify_GainedPsylink();
				}
			}
			base.ChangeLevel(levelOffset);
		}

		public override void ChangeLevel(int levelOffset)
		{
			ChangeLevel(levelOffset, sendLetter: true);
		}

		public static string MakeLetterTextNewPsylinkLevel(Pawn pawn, int abilityLevel, IEnumerable<AbilityDef> newAbilities = null)
		{
			string text = ((abilityLevel == 1) ? "LetterPsylinkLevelGained_First" : "LetterPsylinkLevelGained_NotFirst").Translate(pawn.Named("USER"));
			if (!newAbilities.EnumerableNullOrEmpty())
			{
				text += "\n\n" + "LetterPsylinkLevelGained_PsycastLearned".Translate(pawn.Named("USER"), abilityLevel, newAbilities.Select((AbilityDef a) => a.LabelCap.Resolve()).ToLineList());
			}
			return text;
		}

		public void TryGiveAbilityOfLevel_NewTemp(int abilityLevel, bool sendLetter = true)
		{
			string str = "LetterLabelPsylinkLevelGained".Translate() + ": " + pawn.LabelShortCap;
			string text = null;
			if (!pawn.abilities.abilities.Any((Ability a) => a.def.level == abilityLevel))
			{
				AbilityDef abilityDef = DefDatabase<AbilityDef>.AllDefs.Where((AbilityDef a) => a.level == abilityLevel).RandomElement();
				pawn.abilities.GainAbility(abilityDef);
				text = MakeLetterTextNewPsylinkLevel(pawn, abilityLevel, Gen.YieldSingle(abilityDef));
			}
			else
			{
				text = MakeLetterTextNewPsylinkLevel(pawn, abilityLevel);
			}
			if (sendLetter && PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				Find.LetterStack.ReceiveLetter(str, text, LetterDefOf.PositiveEvent, pawn);
			}
		}

		[Obsolete("Will be removed in a future version")]
		public void TryGiveAbilityOfLevel(int abilityLevel)
		{
			TryGiveAbilityOfLevel_NewTemp(abilityLevel);
		}

		public override void PostRemoved()
		{
			base.PostRemoved();
			pawn.needs?.AddOrRemoveNeedsAsAppropriate();
		}
	}
}
