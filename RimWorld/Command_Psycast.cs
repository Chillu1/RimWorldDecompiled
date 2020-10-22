using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Command_Psycast : Command_Ability
	{
		public override string Label
		{
			get
			{
				if (ability.pawn.IsCaravanMember())
				{
					Pawn pawn = ability.pawn;
					Pawn_PsychicEntropyTracker psychicEntropy = pawn.psychicEntropy;
					StringBuilder stringBuilder = new StringBuilder(base.Label + " (" + pawn.LabelShort);
					if (ability.def.PsyfocusCost > float.Epsilon)
					{
						stringBuilder.Append(", " + "PsyfocusLetter".Translate() + ":" + psychicEntropy.CurrentPsyfocus.ToStringPercent("0"));
					}
					if (ability.def.EntropyGain > float.Epsilon)
					{
						if (ability.def.PsyfocusCost > float.Epsilon)
						{
							stringBuilder.Append(",");
						}
						stringBuilder.Append((string)(" " + "NeuralHeatLetter".Translate() + ":") + Mathf.Round(psychicEntropy.EntropyValue));
					}
					stringBuilder.Append(")");
					return stringBuilder.ToString();
				}
				return base.Label;
			}
		}

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

		public Command_Psycast(Psycast ability)
			: base(ability)
		{
			shrinkable = true;
		}

		protected override void DisabledCheck()
		{
			AbilityDef def = ability.def;
			Pawn pawn = ability.pawn;
			disabled = false;
			if (def.EntropyGain > float.Epsilon)
			{
				Hediff hediff = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff h) => h.def == HediffDefOf.PsychicAmplifier);
				if (hediff == null || hediff.Severity < (float)def.level)
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
}
