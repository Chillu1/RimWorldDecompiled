using Verse;

namespace RimWorld;

public class CompDestroyHeart : CompInteractable
{
	public Building_FleshmassHeart Heart => parent as Building_FleshmassHeart;

	public override bool HideInteraction => !CanInteract();

	public override NamedArgument? ExtraNamedArg => Heart.BiosignatureName.Named("BIOSIGNATURE");

	protected override void OnInteracted(Pawn caster)
	{
		Heart?.StartTachycardiacOverload();
	}

	public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
	{
		Find.AnalysisManager.TryGetAnalysisProgress(Heart.Biosignature, out var details);
		if (details == null || !details.Satisfied)
		{
			return string.Format("{0}: {1}/{2}", "DestroyHeartDisabled".Translate(), details?.timesDone ?? 0, details?.required ?? 0);
		}
		return base.CanInteract(activateBy, checkOptionalItems);
	}
}
