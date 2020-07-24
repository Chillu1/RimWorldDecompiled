using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_Waterskip : CompAbilityEffect
	{
		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			Map map = parent.pawn.Map;
			foreach (IntVec3 item in AffectedCells(target, map))
			{
				List<Thing> thingList = item.GetThingList(map);
				for (int num = thingList.Count - 1; num >= 0; num--)
				{
					if (thingList[num] is Fire)
					{
						thingList[num].Destroy();
					}
				}
				if (!item.Filled(map))
				{
					FilthMaker.TryMakeFilth(item, map, ThingDefOf.Filth_Water);
				}
				Mote mote = MoteMaker.MakeStaticMote(item.ToVector3Shifted(), map, ThingDefOf.Mote_WaterskipSplashParticles);
				mote.rotationRate = Rand.Range(-30f, 30f);
				mote.exactRotation = 90 * Rand.RangeInclusive(0, 3);
				if (item != target.Cell)
				{
					MoteMaker.MakeStaticMote(item, parent.pawn.Map, ThingDefOf.Mote_PsycastSkipEffect);
				}
			}
		}

		private IEnumerable<IntVec3> AffectedCells(LocalTargetInfo target, Map map)
		{
			if (target.Cell.Filled(parent.pawn.Map))
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
}
