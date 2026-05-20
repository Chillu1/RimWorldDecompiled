using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompGrayStatueGas : CompGrayStatue
{
	private const float RoomCellGasFactor = 0.15f;

	private const int MaxGas = 12750;

	private CompProperties_GrayStatueGas Props => (CompProperties_GrayStatueGas)props;

	private int GasAmount => Mathf.Min(Mathf.CeilToInt((float)parent.GetRoom().CellCount * 0.15f * 255f), 12750);

	protected override void Trigger(Pawn target)
	{
		if (Props.gas == GasType.DeadlifeDust)
		{
			GasUtility.AddDeadifeGas(parent.Position, parent.Map, Faction.OfEntities, GasAmount);
		}
		else
		{
			GasUtility.AddGas(parent.Position, parent.Map, Props.gas, GasAmount);
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		yield return new Command_Action
		{
			defaultLabel = "DEV: Test gas spread",
			action = delegate
			{
				parent.Map.gasGrid.EstimateGasDiffusion(parent.Position, GasType.DeadlifeDust, GasAmount, delegate(IntVec3 c)
				{
					GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.PowerConduit), c, parent.Map);
				});
			}
		};
	}
}
