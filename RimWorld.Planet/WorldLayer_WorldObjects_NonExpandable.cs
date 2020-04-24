namespace RimWorld.Planet
{
	public class WorldLayer_WorldObjects_NonExpandable : WorldLayer_WorldObjects
	{
		protected override bool ShouldSkip(WorldObject worldObject)
		{
			return worldObject.def.expandingIcon;
		}
	}
}
