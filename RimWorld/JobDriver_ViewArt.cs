using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_ViewArt : JobDriver_VisitJoyThing
{
	private Thing ArtThing => job.GetTarget(TargetIndex.A).Thing;

	protected override void WaitTickAction(int delta)
	{
		float num = ArtThing.GetStatValue(StatDefOf.Beauty) / ArtThing.def.GetStatValueAbstract(StatDefOf.Beauty);
		float extraJoyGainFactor = ((num > 0f) ? num : 0f);
		pawn.GainComfortFromCellIfPossible(delta);
		JoyUtility.JoyTickCheckEnd(pawn, delta, JoyTickFullJoyAction.EndJob, extraJoyGainFactor, (Building)ArtThing);
	}
}
