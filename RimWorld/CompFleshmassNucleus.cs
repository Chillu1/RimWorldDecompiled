using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompFleshmassNucleus : ThingComp, IActivity, IRoofCollapseAlert
{
	private int minNextFireTick;

	private CompActivity activityComp;

	private int debugNextMeatTick = -1;

	private int debugActiveTick = -1;

	private static readonly IntRange InitialMeatDelayRange = new IntRange(5000, 12500);

	private static readonly IntRange MeatClumpRange = new IntRange(3, 6);

	private const int MinRefireTicks = 15000;

	private const int MeatDaysMTB = 1;

	private const int MinMeatPerStack = 20;

	private const float HeartPointsFactor = 0.5f;

	public CompProperties_FleshmassNucleus Props => (CompProperties_FleshmassNucleus)props;

	public CompActivity Activity => activityComp ?? (activityComp = Pawn.TryGetComp<CompActivity>());

	public Pawn Pawn => (Pawn)parent;

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref minNextFireTick, "minNextFireTick", 0);
	}

	public override void PostPostMake()
	{
		minNextFireTick = GenTicks.TicksGame + InitialMeatDelayRange.RandomInRange;
	}

	public override void CompTick()
	{
		if (debugNextMeatTick > 0 && GenTicks.TicksGame == debugNextMeatTick)
		{
			CreateMeat();
		}
		if (debugActiveTick > 0 && GenTicks.TicksGame == debugActiveTick)
		{
			Activity.EnterActiveState();
		}
		if (GenTicks.TicksGame >= minNextFireTick && Rand.MTBEventOccurs(1f, 60000f, 1f))
		{
			CreateMeat();
		}
	}

	public override void Notify_Downed()
	{
		Pawn.Kill(null, null);
	}

	private void CreateMeat()
	{
		minNextFireTick = GenTicks.TicksGame + 15000;
		int num = Mathf.CeilToInt(Props.activityMeatPerDayCurve.Evaluate(Activity.ActivityLevel));
		int randomInRange = MeatClumpRange.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			if (CellFinder.TryRandomClosewalkCellNear(Pawn.PositionHeld, Pawn.MapHeld, 3, out var result))
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Meat_Twisted);
				thing.stackCount = Rand.RangeInclusive(20, num - 20 * (randomInRange - i - 1));
				GenDrop.TryDropSpawn(thing, result, Pawn.MapHeld, ThingPlaceMode.Near, out var _);
			}
		}
		EffecterDefOf.MeatExplosion.Spawn(Pawn.PositionHeld, Pawn.MapHeld).Cleanup();
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		yield return new Command_Action
		{
			defaultLabel = "DEV: Create meat",
			action = delegate
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				for (int num = 10; num >= 0; num--)
				{
					int ticks = num * 60;
					list.Add(new FloatMenuOption(num + "s", delegate
					{
						debugNextMeatTick = GenTicks.TicksGame + ticks;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		};
		yield return new Command_Action
		{
			defaultLabel = "DEV: Go active in...",
			action = delegate
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				for (int i = 0; i <= 5; i++)
				{
					int ticks = i * 60;
					list.Add(new FloatMenuOption(i + "s", delegate
					{
						debugActiveTick = GenTicks.TicksGame + ticks;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		};
	}

	public void OnActivityActivated()
	{
		IntVec3 positionHeld = Pawn.PositionHeld;
		Map mapHeld = Pawn.MapHeld;
		FleshbeastUtility.MeatSplatter(10, positionHeld, mapHeld, FleshbeastUtility.MeatExplosionSize.Large);
		Pawn.Destroy();
		float threatPoints = StorytellerUtility.DefaultThreatPointsNow(mapHeld) * 0.5f;
		Building_FleshmassHeart obj = (Building_FleshmassHeart)ThingMaker.MakeThing(ThingDefOf.FleshmassHeart);
		obj.Comp.threatPoints = threatPoints;
		obj.SetFaction(Faction.OfEntities);
		GenSpawn.Spawn(obj, positionHeld, mapHeld);
	}

	public void OnPassive()
	{
	}

	public bool ShouldGoPassive()
	{
		return false;
	}

	public bool CanBeSuppressed()
	{
		return true;
	}

	public bool CanActivate()
	{
		foreach (IntVec3 item in GenAdj.CellsOccupiedBy(Pawn.PositionHeld, Rot4.South, new IntVec2(2, 2)))
		{
			if (!item.SupportsStructureType(Pawn.MapHeld, ThingDefOf.FleshmassHeart.terrainAffordanceNeeded))
			{
				return false;
			}
		}
		return true;
	}

	public string ActivityTooltipExtra()
	{
		return null;
	}

	public RoofCollapseResponse Notify_OnBeforeRoofCollapse()
	{
		if (RCellFinder.TryFindRandomCellNearWith(Pawn.Position, (IntVec3 c) => IsValidCell(c, Pawn.MapHeld), Pawn.MapHeld, out var result, 10))
		{
			SkipUtility.SkipTo(Pawn, result, Pawn.MapHeld);
			Activity.AdjustActivity(Props.activityOnRoofCollapsed);
		}
		return RoofCollapseResponse.RemoveThing;
	}

	private static bool IsValidCell(IntVec3 cell, Map map)
	{
		if (cell.InBounds(map))
		{
			return cell.Walkable(map);
		}
		return false;
	}
}
