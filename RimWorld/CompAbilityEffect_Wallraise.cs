using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_Wallraise : CompAbilityEffect
{
	public static Color DustColor = new Color(0.55f, 0.55f, 0.55f, 4f);

	public new CompProperties_AbilityWallraise Props => (CompProperties_AbilityWallraise)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Map map = parent.pawn.Map;
		List<Thing> list = new List<Thing>();
		list.AddRange(AffectedCells(target, map).SelectMany((IntVec3 c) => from t in c.GetThingList(map)
			where t.def.category == ThingCategory.Item
			select t));
		foreach (Thing item in list)
		{
			item.DeSpawn();
		}
		foreach (IntVec3 item2 in AffectedCells(target, map))
		{
			GenSpawn.Spawn(ThingDefOf.RaisedRocks, item2, map);
			FleckMaker.ThrowDustPuffThick(item2.ToVector3Shifted(), map, Rand.Range(1.5f, 3f), DustColor);
			CompAbilityEffect_Teleport.SendSkipUsedSignal(item2, parent.pawn);
		}
		foreach (Thing item3 in list)
		{
			IntVec3 intVec = IntVec3.Invalid;
			for (int num = 0; num < 9; num++)
			{
				IntVec3 intVec2 = item3.Position + GenRadial.RadialPattern[num];
				if (intVec2.InBounds(map) && intVec2.Walkable(map) && map.thingGrid.ThingsListAtFast(intVec2).Count <= 0)
				{
					intVec = intVec2;
					break;
				}
			}
			if (intVec != IntVec3.Invalid)
			{
				GenSpawn.Spawn(item3, intVec, map);
			}
			else
			{
				GenPlace.TryPlaceThing(item3, item3.Position, map, ThingPlaceMode.Near);
			}
		}
	}

	public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
	{
		return Valid(target, throwMessages: true);
	}

	public override void DrawEffectPreview(LocalTargetInfo target)
	{
		GenDraw.DrawFieldEdges(AffectedCells(target, parent.pawn.Map).ToList(), Valid(target) ? Color.white : Color.red);
	}

	private IEnumerable<IntVec3> AffectedCells(LocalTargetInfo target, Map map)
	{
		foreach (IntVec2 item in Props.pattern)
		{
			IntVec3 intVec = target.Cell + new IntVec3(item.x, 0, item.z);
			if (intVec.InBounds(map))
			{
				yield return intVec;
			}
		}
	}

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		if (AffectedCells(target, parent.pawn.Map).Any((IntVec3 c) => c.Filled(parent.pawn.Map)))
		{
			if (throwMessages)
			{
				Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityOccupiedCells".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		if (AffectedCells(target, parent.pawn.Map).Any((IntVec3 c) => !c.Standable(parent.pawn.Map)))
		{
			if (throwMessages)
			{
				Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityUnwalkable".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		if (AffectedCells(target, parent.pawn.Map).Any((IntVec3 c) => c.GetThingList(parent.pawn.Map).Any((Thing t) => !t.def.destroyable)))
		{
			if (throwMessages)
			{
				Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityNotEnoughFreeSpace".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		return true;
	}
}
