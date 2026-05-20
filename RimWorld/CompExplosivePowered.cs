using Verse;

namespace RimWorld;

public class CompExplosivePowered : CompExplosive
{
	protected override bool CanEverExplodeFromDamage
	{
		get
		{
			CompMechPowerCell compMechPowerCell = parent.TryGetComp<CompMechPowerCell>();
			if (compMechPowerCell != null && compMechPowerCell.depleted)
			{
				return false;
			}
			return base.CanEverExplodeFromDamage;
		}
	}
}
