using Verse;

namespace RimWorld;

public class CompLaunchable_TransportPod : CompLaunchable
{
	private new CompProperties_Launchable_TransportPod Props => (CompProperties_Launchable_TransportPod)props;

	public CompRefuelable FuelingPortSource => FuelingPortUtility.FuelingPortGiverAtFuelingPortCell(parent.Position, parent.Map).TryGetComp<CompRefuelable>();

	public bool ConnectedToFuelingPort => FuelingPortSource != null;

	public override CompRefuelable Refuelable => FuelingPortSource;

	public override float FuelLevel => FuelingPortSourceFuel;

	public override bool RequiresFuelingPort => Props.requiresFuelingPort;

	public override float MaxFuelLevel
	{
		get
		{
			if (!Props.requiresFuelingPort)
			{
				return float.PositiveInfinity;
			}
			return FuelingPortSource.Props.fuelCapacity;
		}
	}

	private float FuelingPortSourceFuel
	{
		get
		{
			if (!RequiresFuelingPort)
			{
				return float.PositiveInfinity;
			}
			if (FuelingPortSource == null)
			{
				return 0f;
			}
			return FuelingPortSource.Fuel;
		}
	}

	private bool AllInGroupConnectedToFuelingPort
	{
		get
		{
			foreach (CompTransporter item in base.TransportersInGroup)
			{
				CompLaunchable_TransportPod obj = item.Launchable as CompLaunchable_TransportPod;
				if (obj != null && !obj.ConnectedToFuelingPort)
				{
					return false;
				}
			}
			return true;
		}
	}

	public override AcceptanceReport CanLaunch(float? overrideFuelLevel)
	{
		if (base.Transporter.LoadingInProgressOrReadyToLaunch && Props.requiresFuelingPort && !AllInGroupConnectedToFuelingPort)
		{
			return "CommandLaunchGroupFailNotConnectedToFuelingPort".Translate();
		}
		return base.CanLaunch(overrideFuelLevel);
	}

	public override string CompInspectStringExtra()
	{
		if (base.Transporter.LoadingInProgressOrReadyToLaunch && Props.requiresFuelingPort && !AllInGroupConnectedToFuelingPort)
		{
			return "NotReadyForLaunch".Translate() + ": " + "NotAllInGroupConnectedToFuelingPort".Translate().CapitalizeFirst() + ".";
		}
		return base.CompInspectStringExtra();
	}
}
