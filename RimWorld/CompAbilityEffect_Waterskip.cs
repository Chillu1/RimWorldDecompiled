using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_Waterskip : CompAbilityEffect
{
	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Map map = parent.pawn.Map;
		foreach (IntVec3 item in AffectedCells(target, map))
		{
			if (!item.InBounds(map))
			{
				continue;
			}
			List<Thing> thingList = item.GetThingList(map);
			for (int num = thingList.Count - 1; num >= 0; num--)
			{
				if (thingList[num] is Fire)
				{
					thingList[num].Destroy();
				}
				else if (thingList[num] is Pawn pawn)
				{
					pawn.GetInvisibilityComp()?.DisruptInvisibility();
				}
			}
			if (!item.Filled(map))
			{
				FilthMaker.TryMakeFilth(item, map, ThingDefOf.Filth_Water);
			}
			FleckCreationData dataStatic = FleckMaker.GetDataStatic(item.ToVector3Shifted(), map, FleckDefOf.WaterskipSplashParticles);
			dataStatic.rotationRate = Rand.Range(-30, 30);
			dataStatic.rotation = 90 * Rand.RangeInclusive(0, 3);
			map.flecks.CreateFleck(dataStatic);
			CompAbilityEffect_Teleport.SendSkipUsedSignal(item, parent.pawn);
		}
	}

	private IEnumerable<IntVec3> AffectedCells(LocalTargetInfo target, Map map)
	{
		if (!target.Cell.InBounds(map) || target.Cell.Filled(parent.pawn.Map))
		{
			yield break;
		}
		foreach (IntVec3 item in GenRadial.RadialCellsAround(target.Cell, parent.def.EffectRadius, useCenter: true))
		{
			if (item.InBounds(map) && GenSight.LineOfSightToEdges(target.Cell, item, map, skipFirstCell: true))
			{
				yield return item;
			}
		}
	}

	public override void DrawEffectPreview(LocalTargetInfo target)
	{
		GenDraw.DrawFieldEdges(AffectedCells(target, parent.pawn.Map).ToList(), Valid(target) ? Color.white : Color.red);
	}

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		if (target.Cell.Filled(parent.pawn.Map))
		{
			if (throwMessages)
			{
				Messages.Message("AbilityOccupiedCells".Translate(parent.def.LabelCap), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		return true;
	}
}
