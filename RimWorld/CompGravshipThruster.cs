using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class CompGravshipThruster : CompGravshipFacility
{
	[Unsaved(false)]
	private Thing blockedBy;

	[Unsaved(false)]
	private bool blockedBySubstructure;

	[Unsaved(false)]
	private bool? outdoors;

	[Unsaved(false)]
	private int lastCheckedBlockedTick = -99999;

	[Unsaved(false)]
	private CompBreakdownable breakdownableComp;

	private static List<IntVec3> tmpExclusionCells = new List<IntVec3>();

	public CompBreakdownable Breakdownable => breakdownableComp ?? (breakdownableComp = parent.GetComp<CompBreakdownable>());

	public new CompProperties_GravshipThruster Props => (CompProperties_GravshipThruster)props;

	public bool Blocked
	{
		get
		{
			if (blockedBy == null && !blockedBySubstructure)
			{
				return outdoors == false;
			}
			return true;
		}
	}

	public override bool CanBeActive
	{
		get
		{
			if (!base.CanBeActive)
			{
				return false;
			}
			if (Blocked)
			{
				return false;
			}
			if (Breakdownable.BrokenDown)
			{
				return false;
			}
			if (!outdoors.HasValue)
			{
				RecalculateOutdoors();
			}
			if (base.LinkedBuildings.NullOrEmpty())
			{
				return false;
			}
			return outdoors == true;
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		RecalculateBlocked();
		RecalculateOutdoors();
	}

	public override void PostMapInit()
	{
		RecalculateBlocked();
		RecalculateOutdoors();
		base.PostMapInit();
	}

	public override void CompTickRare()
	{
		base.CompTickRare();
		RecalculateBlocked();
		RecalculateOutdoors();
	}

	public override bool CanLink()
	{
		if (Blocked)
		{
			return false;
		}
		if (!outdoors.HasValue)
		{
			RecalculateOutdoors();
		}
		return outdoors == true;
	}

	private void RecalculateBlocked()
	{
		if (parent.Spawned)
		{
			lastCheckedBlockedTick = Find.TickManager.TicksGame;
			bool num = blockedBy != null || blockedBySubstructure;
			IsBlocked(parent.def, parent.Map, parent.Position, parent.Rotation, out blockedBy, out blockedBySubstructure);
			if (num != (blockedBy != null || blockedBySubstructure))
			{
				Notify_ThingChanged();
			}
		}
	}

	private void RecalculateOutdoors()
	{
		if (parent.Spawned)
		{
			bool valueOrDefault = outdoors == true;
			outdoors = IsOutdoors(parent.def, parent.Map, parent.Position, parent.Rotation);
			if (outdoors != valueOrDefault)
			{
				Notify_ThingChanged();
			}
		}
	}

	public static bool IsBlocked(ThingDef thrusterDef, Map map, IntVec3 position, Rot4 rotation, out Thing blockedBy, out bool blockedBySubstructure)
	{
		blockedBy = null;
		blockedBySubstructure = false;
		thrusterDef.GetCompProperties<CompProperties_GravshipThruster>().GetExclusionZone(position, rotation, ref tmpExclusionCells);
		for (int i = 0; i < tmpExclusionCells.Count; i++)
		{
			IntVec3 c = tmpExclusionCells[i];
			if (!c.InBounds(map))
			{
				continue;
			}
			TerrainDef terrainDef = map.terrainGrid.FoundationAt(c);
			if (terrainDef != null && terrainDef.IsSubstructure)
			{
				blockedBySubstructure = true;
			}
			foreach (Thing item in map.thingGrid.ThingsListAt(c))
			{
				if (item.def.blockWind && !(item is Plant))
				{
					blockedBy = item;
					return true;
				}
			}
		}
		return blockedBySubstructure;
	}

	public static bool IsOutdoors(ThingDef thrusterDef, Map map, IntVec3 position, Rot4 rotation)
	{
		if (!map.regionAndRoomUpdater.Enabled)
		{
			return false;
		}
		foreach (IntVec3 item in GenAdj.CellsAdjacentAlongEdge(position, rotation, thrusterDef.size, rotation.Opposite.AsInt))
		{
			if (item.InBounds(map))
			{
				Room room = item.GetRoom(map);
				if (room != null && !room.UsesOutdoorTemperature)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override string CompInspectStringExtra()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Props.showMaxSimultaneous)
		{
			stringBuilder.Append(MaxConnectedString);
			stringBuilder.Append(": " + Props.maxSimultaneous);
		}
		string text = "";
		if (parent.Spawned && Find.TickManager.TicksGame != lastCheckedBlockedTick)
		{
			RecalculateBlocked();
			RecalculateOutdoors();
		}
		if (Breakdownable.BrokenDown)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += "BrokenDown".Translate();
		}
		if (blockedBy != null)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += "ThrusterBlockedBy".Translate(blockedBy);
		}
		else if (blockedBySubstructure)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += "BlockedBySubstructure".Translate();
		}
		if (outdoors == false)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += "MustBeOutside".Translate();
		}
		if (text.NullOrEmpty() && parent.Spawned && (base.LinkedBuildings.NullOrEmpty() || !CanBeActive))
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += "ThrusterNotConnected".Translate();
		}
		if (!text.NullOrEmpty())
		{
			stringBuilder.AppendLineIfNotEmpty().Append(("ThrusterNotFunctional".Translate() + ": " + text.CapitalizeFirst()).Colorize(ColorLibrary.RedReadable));
		}
		return stringBuilder.ToString();
	}
}
