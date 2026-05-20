using Verse;

namespace RimWorld;

public class CompGravshipShieldGenerator : CompProjectileInterceptor
{
	private const int HitPointsPerIntervalPowered = 2;

	private const int HitPointsPerIntervalUnpowered = -1;

	private CompPowerTrader power;

	private CompGravshipFacility facility;

	private CompPowerTrader Power => power ?? (power = parent.TryGetComp<CompPowerTrader>());

	private CompGravshipFacility Facility => facility ?? parent.TryGetComp<CompGravshipFacility>();

	protected override int NumInactiveDots => 0;

	private bool ShouldCharge
	{
		get
		{
			if (Power.PowerOn)
			{
				return Facility.LinkedBuildings.Count > 0;
			}
			return false;
		}
	}

	protected override int HitPointsPerInterval
	{
		get
		{
			if (base.Active)
			{
				return 0;
			}
			if (ShouldCharge)
			{
				return 2;
			}
			return -1;
		}
	}
}
