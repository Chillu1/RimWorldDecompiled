using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class RitualBehaviorWorker_AnimaLinking : RitualBehaviorWorker
	{
		public RitualBehaviorWorker_AnimaLinking()
		{
		}

		public RitualBehaviorWorker_AnimaLinking(RitualBehaviorDef def)
			: base(def)
		{
		}

		public override string GetExplanation(Precept_Ritual ritual, RitualRoleAssignments assignments, float quality)
		{
			int count = assignments.SpectatorsForReading.Count;
			float num = RitualOutcomeEffectWorker_AnimaTreeLinking.RestoredGrassFromQuality.Evaluate(quality);
			TaggedString taggedString = "AnimaLinkingExplanationBase".Translate(count, num);
			if (assignments.ExtraRequiredPawnsForReading.Any())
			{
				TaggedString psylinkAffectedByTraitsNegativelyWarning = RoyalTitleUtility.GetPsylinkAffectedByTraitsNegativelyWarning(assignments.ExtraRequiredPawnsForReading.FirstOrDefault());
				if (psylinkAffectedByTraitsNegativelyWarning.RawText != null)
				{
					taggedString += "\n\n" + psylinkAffectedByTraitsNegativelyWarning.Resolve();
				}
			}
			return taggedString;
		}

		public override string ExpectedDuration(Precept_Ritual ritual, RitualRoleAssignments assignments, float quality)
		{
			int count = assignments.SpectatorsForReading.Count;
			return Mathf.RoundToInt((float)ritual.behavior.def.durationTicks.max / RitualStage_AnimaTreeLinking.ProgressPerParticipantCurve.Evaluate(count + 1)).ToStringTicksToPeriod(allowSeconds: false);
		}
	}
}
