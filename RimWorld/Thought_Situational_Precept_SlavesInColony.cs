namespace RimWorld
{
	public class Thought_Situational_Precept_SlavesInColony : Thought_Situational
	{
		public override float MoodOffset()
		{
			return BaseMoodOffset * (float)FactionUtility.GetSlavesInFactionCount(pawn.Faction);
		}
	}
}
