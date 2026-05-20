using Verse;

namespace RimWorld;

public class PlaceWorker_ShowSpeakerConnections : PlaceWorker
{
	public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
	{
		DrawConnections((ThingDef)checkingDef, loc, map);
		return true;
	}

	public static void DrawConnections(ThingDef checker, IntVec3 loc, Map map)
	{
		ThingDef thingDef = ((checker == ThingDefOf.LightBall) ? ThingDefOf.Loudspeaker : ThingDefOf.LightBall);
		int num = GenRadial.NumCellsInRadius(12f);
		for (int i = 0; i < num; i++)
		{
			IntVec3 c = loc + GenRadial.RadialPattern[i];
			Thing firstThing = c.GetFirstThing(map, thingDef);
			if (firstThing != null && firstThing.def == thingDef)
			{
				GenDraw.DrawLineBetween(loc.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays.AltitudeFor()), c.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays.AltitudeFor()), SimpleColor.Green);
			}
		}
	}
}
