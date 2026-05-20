using Verse;

namespace RimWorld;

public class CompObelisk_Labyrinth : CompInteractable
{
	public new CompProperties_ObeliskLabyrinth Props => (CompProperties_ObeliskLabyrinth)props;

	protected override void OnInteracted(Pawn caster)
	{
		Messages.Message(Props.messageActivating, parent, MessageTypeDefOf.NeutralEvent, historical: false);
		parent.Map.GetComponent<LabyrinthMapComponent>().StartClosing();
	}
}
