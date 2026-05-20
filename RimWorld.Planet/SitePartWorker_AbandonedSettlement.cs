using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class SitePartWorker_AbandonedSettlement : SitePartWorker
{
	private const float CorpsePointFactor = 0.2f;

	public static readonly IntRange GravlitePanelsCountRange = new IntRange(150, 200);

	public override void Init(Site site, SitePart sitePart)
	{
		base.Init(site, sitePart);
		sitePart.things = new ThingOwner<Thing>(sitePart);
		sitePart.things.TryAdd(ThingMaker.MakeThing(ThingDefOf.Gravcore));
		Thing thing = ThingMaker.MakeThing(ThingDefOf.GravlitePanel);
		int num = GravlitePanelsCountRange.RandomInRange;
		while (num > 0)
		{
			int num2 = Mathf.Min(num, thing.def.stackLimit);
			num -= num2;
			Thing thing2 = ThingMaker.MakeThing(ThingDefOf.GravlitePanel);
			thing2.stackCount = num2;
			sitePart.things.TryAdd(thing2);
		}
	}

	public override void PostMapGenerate(Map map)
	{
		float points = SymbolResolver_Settlement.DefaultPawnsPoints.RandomInRange * 0.2f;
		Find.FactionManager.GetFactions().TryRandomElement(out var result);
		MapGenUtility.ScatterCorpses(map, result, points, new FloatRange(20f, 60f));
		MapGenUtility.DestroyTurrets(map);
		MapGenUtility.ForbidAllItems(map);
	}
}
