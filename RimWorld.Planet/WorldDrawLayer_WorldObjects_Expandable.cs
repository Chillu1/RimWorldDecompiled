using Verse;

namespace RimWorld.Planet;

public class WorldDrawLayer_WorldObjects_Expandable : WorldDrawLayer_WorldObjects
{
	protected override float Alpha
	{
		get
		{
			if (!Find.PlaySettings.showBasesExpandingIcons || !Find.PlaySettings.showImportantExpandingIcons)
			{
				return 1f;
			}
			return 1f - ExpandableWorldObjectsUtility.RawTransitionPct;
		}
	}

	protected override bool ShouldSkip(WorldObject worldObject)
	{
		return !worldObject.def.expandingIcon;
	}
}
