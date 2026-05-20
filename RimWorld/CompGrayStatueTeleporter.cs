using Verse;

namespace RimWorld;

public class CompGrayStatueTeleporter : CompGrayStatue
{
	private CompProperties_GrayStatueTeleporter Props => (CompProperties_GrayStatueTeleporter)props;

	protected override void Trigger(Pawn target)
	{
		parent.MapHeld.GetComponent<LabyrinthMapComponent>().TeleportToLabyrinth(target);
	}
}
