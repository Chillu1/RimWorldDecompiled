using Verse;

namespace RimWorld
{
	public class ThoughtWorker_IsOutdoorsForUndergrounder : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return p.Awake() && (p.Position.UsesOutdoorTemperature(p.Map) || !p.Position.Roofed(p.Map));
		}
	}
}
