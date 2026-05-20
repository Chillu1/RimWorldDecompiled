using Verse;

namespace RimWorld;

public class Command_Psycast : Command_Ability
{
	public override string TopRightLabel
	{
		get
		{
			AbilityDef def = ability.def;
			string text = "";
			if (def.EntropyGain > float.Epsilon)
			{
				text += "NeuralHeatLetter".Translate() + ": " + def.EntropyGain.ToString() + "\n";
			}
			if (def.PsyfocusCost > float.Epsilon)
			{
				string text2 = "";
				text2 = ((!def.AnyCompOverridesPsyfocusCost) ? def.PsyfocusCostPercent : ((!(def.PsyfocusCostRange.Span > float.Epsilon)) ? def.PsyfocusCostPercentMax : (def.PsyfocusCostRange.min * 100f + "-" + def.PsyfocusCostPercentMax)));
				text += "PsyfocusLetter".Translate() + ": " + text2;
			}
			return text.TrimEndNewlines();
		}
	}

	public Command_Psycast(Psycast ability, Pawn pawn)
		: base(ability, pawn)
	{
	}

	protected override void DisabledCheck()
	{
		AbilityDef def = ability.def;
		Pawn pawn = ability.pawn;
		disabled = false;
		if (def.EntropyGain > float.Epsilon)
		{
			if (pawn.GetPsylinkLevel() < def.level)
			{
				DisableWithReason("CommandPsycastHigherLevelPsylinkRequired".Translate(def.level));
			}
			else if (pawn.psychicEntropy.WouldOverflowEntropy(def.EntropyGain + PsycastUtility.TotalEntropyFromQueuedPsycasts(pawn)))
			{
				DisableWithReason("CommandPsycastWouldExceedEntropy".Translate(def.label));
			}
		}
		base.DisabledCheck();
	}
}
