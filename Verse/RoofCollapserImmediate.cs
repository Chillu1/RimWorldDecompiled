using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse.Sound;

namespace Verse;

public static class RoofCollapserImmediate
{
	private static readonly IntRange ThinRoofCrushDamageRange = new IntRange(15, 30);

	public static void DropRoofInCells(IntVec3 c, Map map, List<Thing> outCrushedThings = null)
	{
		if (c.Roofed(map))
		{
			DropRoofInCellPhaseOne(c, map, outCrushedThings);
			DropRoofInCellPhaseTwo(c, map);
			SoundDefOf.Roof_Collapse.PlayOneShot(new TargetInfo(c, map));
		}
	}

	public static void DropRoofInCells(IEnumerable<IntVec3> cells, Map map, List<Thing> outCrushedThings = null)
	{
		IntVec3 cell = IntVec3.Invalid;
		foreach (IntVec3 cell2 in cells)
		{
			if (cell2.Roofed(map))
			{
				DropRoofInCellPhaseOne(cell2, map, outCrushedThings);
			}
		}
		foreach (IntVec3 cell3 in cells)
		{
			if (cell3.Roofed(map))
			{
				DropRoofInCellPhaseTwo(cell3, map);
				cell = cell3;
			}
		}
		if (cell.IsValid)
		{
			SoundDefOf.Roof_Collapse.PlayOneShot(new TargetInfo(cell, map));
		}
	}

	public static void DropRoofInCells(List<IntVec3> cells, Map map, List<Thing> outCrushedThings = null)
	{
		if (cells.NullOrEmpty())
		{
			return;
		}
		IntVec3 cell = IntVec3.Invalid;
		for (int i = 0; i < cells.Count; i++)
		{
			if (cells[i].InBounds(map) && cells[i].Roofed(map))
			{
				DropRoofInCellPhaseOne(cells[i], map, outCrushedThings);
			}
		}
		for (int j = 0; j < cells.Count; j++)
		{
			if (cells[j].InBounds(map) && cells[j].Roofed(map))
			{
				DropRoofInCellPhaseTwo(cells[j], map);
				cell = cells[j];
			}
		}
		if (cell.IsValid)
		{
			SoundDefOf.Roof_Collapse.PlayOneShot(new TargetInfo(cell, map));
		}
	}

	private static void DropRoofInCellPhaseOne(IntVec3 c, Map map, List<Thing> outCrushedThings)
	{
		RoofDef roofDef = map.roofGrid.RoofAt(c);
		if (roofDef == null)
		{
			return;
		}
		if (roofDef.collapseLeavingThingDef != null && roofDef.collapseLeavingThingDef.passability == Traversability.Impassable)
		{
			for (int i = 0; i < 2; i++)
			{
				List<Thing> thingList = c.GetThingList(map);
				for (int num = thingList.Count - 1; num >= 0; num--)
				{
					Thing thing = thingList[num];
					if (thing is ThingWithComps thingWithComps)
					{
						bool flag = thingWithComps is IRoofCollapseAlert roofCollapseAlert && roofCollapseAlert.Notify_OnBeforeRoofCollapse() == RoofCollapseResponse.RemoveThing;
						foreach (IRoofCollapseAlert comp in thingWithComps.GetComps<IRoofCollapseAlert>())
						{
							flag = flag || comp.Notify_OnBeforeRoofCollapse() == RoofCollapseResponse.RemoveThing;
						}
						if (flag)
						{
							return;
						}
					}
					TryAddToCrushedThingsList(thing, outCrushedThings);
					Pawn pawn = thing as Pawn;
					DamageInfo dinfo;
					if (pawn != null)
					{
						dinfo = new DamageInfo(DamageDefOf.Crush, 99999f, 999f, -1f, null, pawn.health.hediffSet.GetBrain(), null, DamageInfo.SourceCategory.Collapse);
					}
					else
					{
						dinfo = new DamageInfo(DamageDefOf.Crush, 99999f, 999f, -1f, null, null, null, DamageInfo.SourceCategory.Collapse);
						dinfo.SetBodyRegion(BodyPartHeight.Top, BodyPartDepth.Outside);
					}
					BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = null;
					if (i == 0 && pawn != null)
					{
						battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_Ceiling);
						Find.BattleLog.Add(battleLogEntry_DamageTaken);
					}
					thing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_DamageTaken);
					if (!thing.Destroyed && thing.def.destroyable)
					{
						thing.Kill(new DamageInfo(DamageDefOf.Crush, 99999f, 999f, -1f, null, null, null, DamageInfo.SourceCategory.Collapse));
					}
				}
			}
		}
		else
		{
			List<Thing> thingList2 = c.GetThingList(map);
			for (int num2 = thingList2.Count - 1; num2 >= 0; num2--)
			{
				Thing thing2 = thingList2[num2];
				if (thing2.def.category == ThingCategory.Item || thing2.def.category == ThingCategory.Plant || thing2.def.category == ThingCategory.Building || thing2.def.category == ThingCategory.Pawn)
				{
					TryAddToCrushedThingsList(thing2, outCrushedThings);
					float num3 = ThinRoofCrushDamageRange.RandomInRange;
					if (thing2.def.building != null)
					{
						num3 *= thing2.def.building.roofCollapseDamageMultiplier;
					}
					BattleLogEntry_DamageTaken battleLogEntry_DamageTaken2 = null;
					if (thing2 is Pawn)
					{
						battleLogEntry_DamageTaken2 = new BattleLogEntry_DamageTaken((Pawn)thing2, RulePackDefOf.DamageEvent_Ceiling);
						Find.BattleLog.Add(battleLogEntry_DamageTaken2);
					}
					DamageInfo dinfo2 = new DamageInfo(DamageDefOf.Crush, GenMath.RoundRandom(num3), 0f, -1f, null, null, null, DamageInfo.SourceCategory.Collapse);
					dinfo2.SetBodyRegion(BodyPartHeight.Top, BodyPartDepth.Outside);
					thing2.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_DamageTaken2);
				}
			}
		}
		if (roofDef.collapseLeavingThingDef != null)
		{
			Thing thing3 = GenSpawn.Spawn(roofDef.collapseLeavingThingDef, c, map);
			if (thing3.def.rotatable)
			{
				thing3.Rotation = Rot4.Random;
			}
		}
		for (int j = 0; j < 1; j++)
		{
			FleckMaker.ThrowDustPuff(c.ToVector3Shifted() + Gen.RandomHorizontalVector(0.6f), map, 2f);
		}
	}

	private static void DropRoofInCellPhaseTwo(IntVec3 c, Map map)
	{
		RoofDef roofDef = map.roofGrid.RoofAt(c);
		if (roofDef == null)
		{
			return;
		}
		if (roofDef.filthLeaving != null)
		{
			FilthMaker.TryMakeFilth(c, map, roofDef.filthLeaving);
		}
		if (roofDef.VanishOnCollapse)
		{
			map.roofGrid.SetRoof(c, null);
		}
		CellRect bound = CellRect.CenteredOn(c, 2);
		foreach (Pawn item in map.mapPawns.AllPawnsSpawned.Where((Pawn pawn) => bound.Contains(pawn.Position)))
		{
			TaleRecorder.RecordTale(TaleDefOf.CollapseDodged, item);
		}
	}

	private static void TryAddToCrushedThingsList(Thing t, List<Thing> outCrushedThings)
	{
		if (outCrushedThings != null && !outCrushedThings.Contains(t) && WorthMentioningInCrushLetter(t))
		{
			outCrushedThings.Add(t);
		}
	}

	private static bool WorthMentioningInCrushLetter(Thing t)
	{
		if (!t.def.destroyable)
		{
			return false;
		}
		return t.def.category switch
		{
			ThingCategory.Building => true, 
			ThingCategory.Pawn => true, 
			ThingCategory.Item => t.MarketValue > 0.01f, 
			_ => false, 
		};
	}
}
