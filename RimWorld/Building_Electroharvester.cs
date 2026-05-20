using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Building_Electroharvester : Building
{
	private CompPowerPlantElectroharvester cachedElectroharvester;

	private CompPowerTrader compPowerTrader;

	private bool initalized;

	private CompCableConnection cableConnection;

	public CompPowerPlantElectroharvester Electroharvester => cachedElectroharvester ?? (cachedElectroharvester = GetComp<CompPowerPlantElectroharvester>());

	public CompPowerTrader PowerTrader => compPowerTrader ?? (compPowerTrader = GetComp<CompPowerTrader>());

	public CompCableConnection CableConnection => cableConnection ?? (cableConnection = GetComp<CompCableConnection>());

	public bool PowerOn => PowerTrader.PowerOn;

	public override bool IsWorking()
	{
		return PowerOn;
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (!respawningAfterLoad)
		{
			Initialize();
		}
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		base.DeSpawn(mode);
		Electroharvester.FacilityComp.OnLinkAdded -= OnLinkAdded;
		Electroharvester.FacilityComp.OnLinkRemoved -= OnLinkRemoved;
	}

	private void Initialize()
	{
		if (initalized)
		{
			RebuildCables();
			return;
		}
		initalized = true;
		Electroharvester.FacilityComp.OnLinkAdded += OnLinkAdded;
		Electroharvester.FacilityComp.OnLinkRemoved += OnLinkRemoved;
		foreach (Thing platform in Electroharvester.Platforms)
		{
			if (platform is Building_HoldingPlatform building_HoldingPlatform)
			{
				building_HoldingPlatform.innerContainer.OnContentsChanged += RebuildCables;
			}
		}
		RebuildCables();
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		base.DrawAt(drawLoc, flip);
		if (!initalized)
		{
			Initialize();
		}
	}

	private void OnLinkRemoved(CompFacility facility, Thing thing)
	{
		if (thing is Building_HoldingPlatform building_HoldingPlatform)
		{
			building_HoldingPlatform.innerContainer.OnContentsChanged -= RebuildCables;
			RebuildCables();
		}
	}

	public override void Notify_DefsHotReloaded()
	{
		base.Notify_DefsHotReloaded();
		RebuildCables();
	}

	private void OnLinkAdded(CompFacility facility, Thing thing)
	{
		if (thing is Building_HoldingPlatform building_HoldingPlatform)
		{
			building_HoldingPlatform.innerContainer.OnContentsChanged += RebuildCables;
			RebuildCables();
		}
	}

	private void RebuildCables()
	{
		CableConnection.RebuildCables(Electroharvester.Platforms, (Thing thing) => thing is Building_HoldingPlatform building_HoldingPlatform && building_HoldingPlatform.Occupied);
	}
}
