namespace RimWorld
{
	public class ThoughtWorker_MyParentsHappy : ThoughtWorker_MyParentHappy
	{
		protected override int RequiredParentCount => 2;
	}
}
