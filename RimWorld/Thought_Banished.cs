namespace RimWorld
{
	public class Thought_Banished : Thought_Memory
	{
		public override bool ShouldDiscard
		{
			get
			{
				if (!base.ShouldDiscard)
				{
					return otherPawn.Faction == pawn.Faction;
				}
				return true;
			}
		}
	}
}
