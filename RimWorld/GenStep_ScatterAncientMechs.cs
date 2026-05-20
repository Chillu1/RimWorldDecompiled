using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GenStep_ScatterAncientMechs : GenStep_Scatterer
{
	private const float MechClawChance = 0.75f;

	private const float MechLegChance = 0.75f;

	private const float OilSmearChance = 0.2f;

	private ThingDef thingToPlace;

	private Rot4 rotation;

	private static IEnumerable<ThingDef> MechThingDefs
	{
		get
		{
			yield return ThingDefOf.AncientWarwalkerTorso;
			yield return ThingDefOf.AncientMiniWarwalkerRemains;
			yield return ThingDefOf.AncientWarspiderRemains;
			yield return ThingDefOf.AncientWarwalkerFoot;
		}
	}

	public override int SeedPart => 1034745625;

	protected override float GetPlacementFactor(Map map)
	{
		float num = 1f;
		foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
		{
			num *= mutator.junkDensityFactor;
		}
		return num;
	}

	public override void Generate(Map map, GenStepParams parms)
	{
		if (ModLister.CheckIdeology("Scatter ancient outdoor building"))
		{
			count = 1;
			allowInWaterBiome = false;
			thingToPlace = MechThingDefs.RandomElement();
			base.Generate(map, parms);
		}
	}

	protected override bool CanScatterAt(IntVec3 c, Map map)
	{
		if (!base.CanScatterAt(c, map))
		{
			return false;
		}
		if (thingToPlace.rotatable)
		{
			int num = Rand.RangeInclusive(1, 4);
			for (int i = 0; i < 4; i++)
			{
				rotation = new Rot4((i + num) % 4);
				if (CanPlaceThingAt(c, rotation, map, thingToPlace))
				{
					return true;
				}
			}
		}
		rotation = thingToPlace.defaultPlacingRot;
		return CanPlaceThingAt(c, rotation, map, thingToPlace);
	}

	private bool CanPlaceThingAt(IntVec3 c, Rot4 rot, Map map, ThingDef thingDef)
	{
		return ScatterDebrisUtility.CanPlaceThingAt(c, rot, map, thingDef);
	}

	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
	{
		Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(thingToPlace), loc, map, rotation);
		ScatterDebrisUtility.ScatterFilthAroundThing(thing, map, ThingDefOf.Filth_MachineBits);
		ScatterDebrisUtility.ScatterFilthAroundThing(thing, map, ThingDefOf.Filth_OilSmear, 0.2f, 0);
		if (thing.def != ThingDefOf.AncientWarwalkerTorso)
		{
			return;
		}
		if (Rand.Chance(0.75f))
		{
			int num = ThingDefOf.AncientWarwalkerTorso.size.z / 2 + ThingDefOf.AncientWarwalkerLeg.size.z / 2 + Rand.Range(1, 3);
			IntVec3 intVec = thing.Position + new IntVec3(Rand.Range(-3, 3), 0, -num).RotatedBy(rotation);
			if (CanPlaceThingAt(intVec, rotation, map, ThingDefOf.AncientWarwalkerLeg))
			{
				ScatterDebrisUtility.ScatterFilthAroundThing(GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.AncientWarwalkerLeg), intVec, map, rotation), map, ThingDefOf.Filth_MachineBits);
			}
		}
		if (Rand.Chance(0.75f))
		{
			int newX = (ThingDefOf.AncientWarwalkerTorso.size.x / 2 + ThingDefOf.AncientWarwalkerClaw.size.x / 2 + Rand.Range(3, 5)) * ((!Rand.Bool) ? 1 : (-1));
			IntVec3 intVec2 = thing.Position + new IntVec3(newX, 0, Rand.Range(-1, 1)).RotatedBy(rotation);
			if (CanPlaceThingAt(intVec2, rotation, map, ThingDefOf.AncientWarwalkerClaw))
			{
				Thing thing2 = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.AncientWarwalkerClaw), intVec2, map, rotation);
				ScatterDebrisUtility.ScatterFilthAroundThing(thing2, map, ThingDefOf.Filth_MachineBits);
				ScatterDebrisUtility.ScatterFilthAroundThing(thing2, map, ThingDefOf.Filth_OilSmear, 0.2f, 0);
			}
		}
	}
}
