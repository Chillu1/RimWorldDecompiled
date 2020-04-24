namespace Verse
{
	public class HediffComp_RecoveryThought : HediffComp
	{
		public HediffCompProperties_RecoveryThought Props => (HediffCompProperties_RecoveryThought)props;

		public override void CompPostPostRemoved()
		{
			base.CompPostPostRemoved();
			if (!base.Pawn.Dead && base.Pawn.needs.mood != null)
			{
				base.Pawn.needs.mood.thoughts.memories.TryGainMemory(Props.thought);
			}
		}
	}
}
