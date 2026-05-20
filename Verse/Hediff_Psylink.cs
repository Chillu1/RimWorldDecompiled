using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse;

public class Hediff_Psylink : Hediff_Level
{
	public bool suppressPostAddLetter;

	public override void PostAdd(DamageInfo? dinfo)
	{
		base.PostAdd(dinfo);
		TryGiveAbilityOfLevel(level, !suppressPostAddLetter);
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
				TryGiveAbilityOfLevel(abilityLevel, sendLetter);
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

	public void TryGiveAbilityOfLevel(int abilityLevel, bool sendLetter = true)
	{
		string text = "LetterLabelPsylinkLevelGained".Translate() + ": " + pawn.LabelShortCap;
		string text2 = null;
		if (!pawn.abilities.abilities.Any((Ability a) => a.def.IsPsycast && a.def.level == abilityLevel))
		{
			AbilityDef val = DefDatabase<AbilityDef>.AllDefs.Where((AbilityDef a) => a.IsPsycast && a.level == abilityLevel).RandomElement();
			pawn.abilities.GainAbility(val);
			text2 = MakeLetterTextNewPsylinkLevel(pawn, abilityLevel, Gen.YieldSingle(val));
		}
		else
		{
			text2 = MakeLetterTextNewPsylinkLevel(pawn, abilityLevel);
		}
		if (sendLetter && PawnUtility.ShouldSendNotificationAbout(pawn) && !pawn.IsDuplicate)
		{
			Find.LetterStack.ReceiveLetter(text, text2, LetterDefOf.PositiveEvent, pawn);
		}
	}

	public override void CopyFrom(Hediff other)
	{
		base.CopyFrom(other);
		if (other is Hediff_Psylink hediff_Psylink)
		{
			level = hediff_Psylink.level;
			suppressPostAddLetter = true;
		}
	}

	public override void PostRemoved()
	{
		base.PostRemoved();
		pawn.needs?.AddOrRemoveNeedsAsAppropriate();
	}
}
