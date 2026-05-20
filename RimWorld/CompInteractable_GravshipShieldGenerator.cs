using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompInteractable_GravshipShieldGenerator : CompInteractable
{
	private const int MinHitPointsToActivate = 100;

	private CompProjectileInterceptor shield;

	private CompGravshipFacility facility;

	private CompProjectileInterceptor Shield => shield ?? parent.TryGetComp<CompProjectileInterceptor>();

	private CompGravshipFacility Facility => facility ?? parent.TryGetComp<CompGravshipFacility>();

	protected override string ActivateOptionLabel => Shield.Active ? "Deactivate".Translate() : "Activate".Translate();

	public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		if (Facility.LinkedBuildings.NullOrEmpty())
		{
			return "ShieldNotConnectedToGravship".Translate();
		}
		if (Shield.Charging)
		{
			return "ShieldOnCooldown".Translate();
		}
		if (!Shield.Active && Shield.currentHitPoints < 100)
		{
			return "ShieldBelowMinHitpoints".Translate();
		}
		return base.CanInteract(activateBy, checkOptionalItems);
	}

	protected override void OnInteracted(Pawn caster)
	{
		if (!Shield.Active)
		{
			Shield.Activate();
		}
		else
		{
			Shield.Deactivate();
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (parent.SpawnedOrAnyParentSpawned)
		{
			string defaultLabel;
			string defaultDesc;
			if (!Shield.Active)
			{
				defaultLabel = "GravshipShieldOrderActivation".Translate() + "...";
				defaultDesc = "GravshipShieldOrderActivationDesc".Translate(parent.Named("THING"));
			}
			else
			{
				defaultLabel = "GravshipShieldOrderDeactivation".Translate() + "...";
				defaultDesc = "GravshipShieldOrderDeactivationDesc".Translate(parent.Named("THING"));
			}
			Command_Action command_Action = new Command_Action
			{
				defaultLabel = defaultLabel,
				defaultDesc = defaultDesc,
				icon = base.UIIcon,
				groupable = false,
				action = delegate
				{
					SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
					Find.Targeter.BeginTargeting(this);
				}
			};
			AcceptanceReport acceptanceReport = CanInteract();
			if (!acceptanceReport.Accepted)
			{
				command_Action.Disable(acceptanceReport.Reason.CapitalizeFirst());
			}
			yield return command_Action;
		}
	}
}
