using Verse;

namespace RimWorld;

public class Verb_ShootOneUse : Verb_Shoot
{
	protected override bool TryCastShot()
	{
		if (base.TryCastShot())
		{
			if (burstShotsLeft <= 1)
			{
				SelfConsume();
			}
			return true;
		}
		if (burstShotsLeft < base.BurstShotCount)
		{
			SelfConsume();
		}
		return false;
	}

	public override void Notify_EquipmentLost()
	{
		base.Notify_EquipmentLost();
		if (state == VerbState.Bursting && burstShotsLeft < base.BurstShotCount)
		{
			SelfConsume();
		}
	}

	private void SelfConsume()
	{
		if (base.EquipmentSource != null && !base.EquipmentSource.Destroyed)
		{
			base.EquipmentSource.Destroy();
		}
	}
}
