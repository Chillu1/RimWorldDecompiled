using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class CompGravshipFacility : CompFacility
{
	[Unsaved(false)]
	public Building_GravEngine engine;

	private List<Building_GravEngine> tmpEngines = new List<Building_GravEngine>();

	private int lastGotEnginesTick = -1;

	public new CompProperties_GravshipFacility Props => (CompProperties_GravshipFacility)props;

	protected override string MaxConnectedString => "GravFacilityMaxSimultaneousConnections".Translate();

	public override bool CanBeActive
	{
		get
		{
			if (!ModsConfig.OdysseyActive)
			{
				return false;
			}
			if (!base.CanBeActive)
			{
				return false;
			}
			if (Find.TickManager.TicksGame != lastGotEnginesTick)
			{
				tmpEngines.Clear();
				tmpEngines.AddRange(parent.Map.listerBuildings.AllBuildingsColonistOfClass<Building_GravEngine>());
				lastGotEnginesTick = Find.TickManager.TicksGame;
			}
			foreach (Building_GravEngine tmpEngine in tmpEngines)
			{
				if (!parent.Position.InHorDistOf(tmpEngine.Position, Props.maxDistance))
				{
					return false;
				}
				if (Props.onlyRequiresLooseConnection && tmpEngine.LooselyConnectedToGravEngine(parent))
				{
					return true;
				}
				if (tmpEngine.OnValidSubstructure(parent))
				{
					return true;
				}
			}
			return false;
		}
	}

	public override void PostSpawnSetup(bool respawningAfterReload)
	{
		base.OnLinkAdded += Notify_LinkAdded;
		base.OnLinkRemoved += Notify_LinkRemoved;
		base.PostSpawnSetup(respawningAfterReload);
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		base.PostDeSpawn(map, mode);
		base.OnLinkAdded -= Notify_LinkAdded;
		base.OnLinkRemoved -= Notify_LinkRemoved;
	}

	protected virtual void Notify_LinkAdded(CompFacility facility, Thing thing)
	{
		if (ModsConfig.OdysseyActive && thing is Building_GravEngine building_GravEngine && engine == null)
		{
			engine = building_GravEngine;
			engine.AddComponent(this);
		}
	}

	protected virtual void Notify_LinkRemoved(CompFacility facility, Thing thing)
	{
		if (ModsConfig.OdysseyActive && thing is Building_GravEngine building_GravEngine && building_GravEngine == engine)
		{
			engine.RemoveComponent(this);
			engine = null;
		}
	}

	public override string CompInspectStringExtra()
	{
		StringBuilder stringBuilder = new StringBuilder(base.CompInspectStringExtra());
		if (!parent.Spawned)
		{
			return stringBuilder.ToString();
		}
		if (parent.Spawned && (base.LinkedBuildings.NullOrEmpty() || !CanBeActive))
		{
			stringBuilder.AppendLineIfNotEmpty();
			stringBuilder.Append("NotConnectedToGravEngine".Translate().Colorize(ColorLibrary.RedReadable));
		}
		bool flag = false;
		foreach (Building_GravEngine item in parent.Map.listerBuildings.AllBuildingsColonistOfClass<Building_GravEngine>())
		{
			if (parent.Position.InHorDistOf(item.Position, Props.maxDistance))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			stringBuilder.AppendLineIfNotEmpty().Append("MessageMustBePlacedInRangeOfGravEngine".Translate().Colorize(ColorLibrary.RedReadable));
		}
		return stringBuilder.ToString();
	}
}
