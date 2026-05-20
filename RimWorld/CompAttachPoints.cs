using Verse;

namespace RimWorld;

public class CompAttachPoints : ThingComp
{
	public AttachPointTracker points;

	public CompProperties_AttachPoints Props => (CompProperties_AttachPoints)props;

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		points = new AttachPointTracker(Props.points, parent);
	}
}
