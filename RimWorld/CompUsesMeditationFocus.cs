using Verse;

namespace RimWorld;

public class CompUsesMeditationFocus : ThingComp
{
	public override void PostDrawExtraSelectionOverlays()
	{
		MeditationUtility.DrawMeditationSpotOverlay(parent.Position, parent.Map);
	}
}
