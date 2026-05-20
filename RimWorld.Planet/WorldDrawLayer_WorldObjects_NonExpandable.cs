namespace RimWorld.Planet;

public class WorldDrawLayer_WorldObjects_NonExpandable : WorldDrawLayer_WorldObjects
{
	protected override bool ShouldSkip(WorldObject worldObject)
	{
		return worldObject.def.expandingIcon;
	}
}
