using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualStageAction_Multi : RitualStageAction
	{
		public List<RitualStageAction> subActions;

		public override void Apply(LordJob_Ritual ritual)
		{
			if (subActions == null)
			{
				Log.Error("RitualStageAction_Multi without any sub actions on ritual " + ritual);
				return;
			}
			foreach (RitualStageAction subAction in subActions)
			{
				subAction.Apply(ritual);
			}
		}

		public override void ApplyToPawn(LordJob_Ritual ritual, Pawn pawn)
		{
			if (subActions == null)
			{
				Log.Error("RitualStageAction_Multi without any sub actions on ritual " + ritual);
				return;
			}
			foreach (RitualStageAction subAction in subActions)
			{
				subAction.ApplyToPawn(ritual, pawn);
			}
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref subActions, "subActions", LookMode.Deep);
		}
	}
}
