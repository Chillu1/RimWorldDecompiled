using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Command_Psycast : Command_Ability
	{
		public Command_Psycast(Psycast ability)
			: base(ability)
		{
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
		{
			AbilityDef def = ability.def;
			Pawn pawn = ability.pawn;
			disabled = false;
			if (def.EntropyGain > float.Epsilon)
			{
				Hediff hediff = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff h) => h.def == HediffDefOf.PsychicAmplifier);
				if (hediff == null || hediff.Severity < (float)def.level)
				{
					DisableWithReason("CommandPsycastHigherLevelAmplifierRequired".Translate(def.level));
				}
				else if (pawn.psychicEntropy.WouldOverflowEntropy(def.EntropyGain + PsycastUtility.TotalEntropyFromQueuedPsycasts(pawn)))
				{
					DisableWithReason("CommandPsycastWouldExceedEntropy".Translate(def.label));
				}
			}
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth);
			if (def.EntropyGain > float.Epsilon)
			{
				Text.Font = GameFont.Tiny;
				string text = def.EntropyGain.ToString();
				float x = Text.CalcSize(text).x;
				Widgets.Label(new Rect(topLeft.x + GetWidth(maxWidth) - x - 5f, topLeft.y + 5f, x, 18f), text);
			}
			return result;
		}
	}
}
