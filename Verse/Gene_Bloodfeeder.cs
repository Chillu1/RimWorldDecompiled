using RimWorld;

namespace Verse;

public class Gene_Bloodfeeder : Gene
{
	public override void PostAdd()
	{
		base.PostAdd();
		if (pawn.IsPrisonerOfColony && pawn.guest != null && pawn.guest.HasInteractionWith((PrisonerInteractionModeDef interaction) => interaction.hideIfNoBloodfeeders))
		{
			pawn.guest.SetNoInteraction();
		}
	}
}
