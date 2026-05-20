using RimWorld;

namespace Verse;

public class HediffComp_AttachPoints : HediffComp
{
	private AttachPointTracker points;

	public HediffCompProperties_AttachPoints Props => (HediffCompProperties_AttachPoints)props;

	public AttachPointTracker Points
	{
		get
		{
			if (parent.pawn.story == null)
			{
				return null;
			}
			if (points == null)
			{
				if (parent.pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated && parent.pawn.story?.bodyType?.attachPointsDessicated == null)
				{
					Log.Warning($"Pawn {parent.pawn} is dessicated but their {parent.pawn.story?.bodyType} bodyType def doesn't declare attachPointsDessicated, falling back to non-dessicated attachpoints instead");
				}
				if (parent.pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated && parent.pawn.story?.bodyType?.attachPointsDessicated != null)
				{
					points = new AttachPointTracker(parent.pawn.story.bodyType.attachPointsDessicated, parent.pawn);
				}
				else if (parent.pawn.story?.bodyType?.attachPoints != null)
				{
					points = new AttachPointTracker(parent.pawn.story.bodyType.attachPoints, parent.pawn);
				}
			}
			return points;
		}
	}
}
