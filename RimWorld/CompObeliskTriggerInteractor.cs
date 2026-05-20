using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompObeliskTriggerInteractor : CompInteractable
{
	private CompObelisk obeliskComp;

	private const int StudyNeeded = 2;

	private CompObelisk ObeliskComp => obeliskComp ?? (obeliskComp = parent.GetComp<CompObelisk>());

	public override string ExposeKey => "Interactor";

	public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
	{
		if (ObeliskComp.StudyLevel < 2 || ObeliskComp.ActivityComp.Deactivated || ObeliskComp.Activated)
		{
			return false;
		}
		return base.CanInteract(activateBy, checkOptionalItems);
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (2 > ObeliskComp.StudyLevel || ObeliskComp.ActivityComp.Deactivated || ObeliskComp.Activated)
		{
			yield break;
		}
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
	}

	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		if (2 > ObeliskComp.StudyLevel || ObeliskComp.ActivityComp.Deactivated || ObeliskComp.Activated)
		{
			yield break;
		}
		foreach (FloatMenuOption item in base.CompFloatMenuOptions(selPawn))
		{
			yield return item;
		}
	}

	protected override void OnInteracted(Pawn caster)
	{
		if (!obeliskComp.Activated)
		{
			ObeliskComp.TriggerInteractionEffect(caster, triggeredByPlayer: true);
		}
	}
}
