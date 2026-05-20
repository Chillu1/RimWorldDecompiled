using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompSpawnEffectersInRoom : ThingComp
{
	private readonly Dictionary<IntVec3, Effecter> effecters = new Dictionary<IntVec3, Effecter>();

	private CompProperties_SpawnEffectersInRoom Props => (CompProperties_SpawnEffectersInRoom)props;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		effecters.Clear();
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		base.PostDeSpawn(map, mode);
		foreach (Effecter value in effecters.Values)
		{
			value.Cleanup();
		}
	}

	public override void CompTick()
	{
		if (!parent.Spawned)
		{
			return;
		}
		Room room = parent.GetRoom();
		if (room == null || room.TouchesMapEdge || !parent.IsRitualTarget())
		{
			return;
		}
		foreach (IntVec3 cell in room.Cells)
		{
			if (cell.InHorDistOf(parent.Position, Props.radius))
			{
				CheckEffecter(cell);
			}
		}
	}

	private void CheckEffecter(IntVec3 cell)
	{
		if (effecters.ContainsKey(cell))
		{
			if (effecters[cell] == null)
			{
				effecters[cell] = Props.effecter.Spawn();
				effecters[cell].Trigger(new TargetInfo(cell, parent.Map), TargetInfo.Invalid);
			}
		}
		else
		{
			effecters[cell] = Props.effecter.Spawn();
			effecters[cell].Trigger(new TargetInfo(cell, parent.Map), TargetInfo.Invalid);
		}
		effecters[cell].EffectTick(new TargetInfo(cell, parent.Map), TargetInfo.Invalid);
	}
}
