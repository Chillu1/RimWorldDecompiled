using Verse;

namespace RimWorld;

public class CompInteractableFoamTurret : CompInteractable
{
	private Building_TurretGun ParentGun => (Building_TurretGun)parent;

	public override bool CanCooldown
	{
		get
		{
			if (power != null)
			{
				return power.PowerOn;
			}
			return true;
		}
	}

	public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
	{
		AcceptanceReport result = base.CanInteract(activateBy, checkOptionalItems);
		if (!result.Accepted)
		{
			return result;
		}
		if (!ParentGun.TryFindNewTarget().IsValid)
		{
			return "NoNearbyFire".Translate();
		}
		return true;
	}

	protected override void SendDeactivateMessage()
	{
		Messages.Message("MessageActivationCanceled".Translate(parent) + ": " + "NoNearbyFireNoFuelUsed".Translate(), parent, MessageTypeDefOf.NeutralEvent);
	}

	protected override bool ShouldDeactivate()
	{
		return !CanInteract();
	}

	protected override bool TryInteractTick()
	{
		ParentGun.TryActivateBurst();
		if (ParentGun.CurrentTarget.IsValid)
		{
			ParentGun.Top.ForceFaceTarget(ParentGun.CurrentTarget);
			return true;
		}
		return false;
	}
}
