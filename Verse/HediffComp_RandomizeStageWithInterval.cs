using RimWorld;

namespace Verse
{
	public class HediffComp_RandomizeStageWithInterval : HediffComp_Randomizer
	{
		public HediffCompProperties_RandomizeStageWithInterval Props => (HediffCompProperties_RandomizeStageWithInterval)props;

		public override void Randomize()
		{
			int curStageIndex = parent.CurStageIndex;
			parent.Severity = parent.def.stages.RandomElement().minSeverity;
			int curStageIndex2 = parent.CurStageIndex;
			if (curStageIndex != curStageIndex2 && !Props.notifyMessage.NullOrEmpty() && PawnUtility.ShouldSendNotificationAbout(parent.pawn))
			{
				Messages.Message(Props.notifyMessage.Formatted(parent.pawn.Named("PAWN"), parent.def.stages[curStageIndex].label, parent.def.stages[curStageIndex2].label), parent.pawn, MessageTypeDefOf.NeutralEvent);
			}
		}
	}
}
