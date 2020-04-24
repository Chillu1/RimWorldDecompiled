using RimWorld;

namespace Verse.AI.Group
{
	public class TriggerFilter_NoSapperSapping : TriggerFilter
	{
		public override bool AllowActivation(Lord lord, TriggerSignal signal)
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if ((pawn.mindState.duty != null && pawn.mindState.duty.def == DutyDefOf.Sapper && pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Mine && pawn.CurJob.targetA.Cell.InHorDistOf(pawn.Position, 5f)) || (pawn.CurJob.def == JobDefOf.UseVerbOnThing && pawn.CurJob.targetA.Cell.InHorDistOf(pawn.Position, 20f)))
				{
					return false;
				}
			}
			return true;
		}
	}
}
