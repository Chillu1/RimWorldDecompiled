using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class RitualRoleBehavior : IExposable
	{
		[NoTranslate]
		public string roleId;

		public DutyDef dutyDef;

		[MustTranslate]
		public string jobReportOverride;

		protected List<RitualPosition> customPositions;

		public InteractionDef speakerInteraction;

		public List<RitualPosition> CustomPositionsForReading => customPositions;

		public RitualPosition GetPosition(IntVec3 spot, Pawn p, LordJob_Ritual ritual)
		{
			if (!customPositions.NullOrEmpty())
			{
				foreach (RitualPosition customPosition in customPositions)
				{
					if (customPosition.CanUse(spot, p, ritual))
					{
						return customPosition;
					}
				}
			}
			return null;
		}

		public virtual string GetJobReportOverride(Pawn pawn)
		{
			TaggedString? taggedString = jobReportOverride?.Formatted(pawn.Named("PAWN")).CapitalizeFirst();
			if (!taggedString.HasValue)
			{
				return null;
			}
			return taggedString.GetValueOrDefault();
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref roleId, "roleId");
			Scribe_Defs.Look(ref dutyDef, "dutyDef");
			Scribe_Collections.Look(ref customPositions, "customPositions", LookMode.Deep);
			Scribe_Defs.Look(ref speakerInteraction, "speakerInteraction");
		}
	}
}
