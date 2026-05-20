using Verse;

namespace RimWorld
{
	public class RitualStage_WaitingForParticipants : RitualStage
	{
		public override float ProgressPerTick(LordJob_Ritual ritual)
		{
			foreach (Pawn item in ritual.assignments.SpectatorsForReading)
			{
				if (!ritual.IsParticipating(item))
				{
					return 0f;
				}
			}
			return base.ProgressPerTick(ritual);
		}
	}
}
