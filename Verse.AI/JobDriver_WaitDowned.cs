namespace Verse.AI
{
	public class JobDriver_WaitDowned : JobDriver_Wait
	{
		public override void DecorateWaitToil(Toil wait)
		{
			base.DecorateWaitToil(wait);
			wait.AddFailCondition(() => !pawn.Downed);
		}
	}
}
