using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompPolluteOverTime : ThingComp
{
	private CompProperties_PolluteOverTime Props => (CompProperties_PolluteOverTime)props;

	private int TicksToPolluteCell => 60000 / Props.cellsToPollutePerDay;

	public override void CompTick()
	{
		if (parent.Spawned && parent.IsHashIntervalTick(TicksToPolluteCell))
		{
			Pollute();
		}
	}

	private void Pollute()
	{
		if (!ModsConfig.BiotechActive)
		{
			return;
		}
		int num = GenRadial.NumCellsInRadius(GenRadial.MaxRadialPatternRadius - 1f);
		for (int i = 0; i < num; i++)
		{
			IntVec3 intVec = parent.Position + GenRadial.RadialPattern[i];
			if (!intVec.IsPolluted(parent.Map) && intVec.CanPollute(parent.Map))
			{
				intVec.Pollute(parent.Map);
				parent.Map.effecterMaintainer.AddEffecterToMaintain(EffecterDefOf.CellPollution.Spawn(intVec, parent.Map, Vector3.zero), intVec, 45);
				break;
			}
		}
	}

	public override string CompInspectStringExtra()
	{
		return "TilePollution".Translate() + ": " + "CellsPerDay".Translate(Props.cellsToPollutePerDay);
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (DebugSettings.ShowDevGizmos)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Pollute",
				action = Pollute
			};
		}
	}
}
