namespace Verse
{
	public class HediffComp_DisappearsOnDeath : HediffComp
	{
		public override void Notify_PawnDied()
		{
			base.Notify_PawnDied();
			base.Pawn.health.RemoveHediff(parent);
		}
	}
}
