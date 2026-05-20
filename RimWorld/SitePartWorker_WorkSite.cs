using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public abstract class SitePartWorker_WorkSite : SitePartWorker
{
	public struct CampLootThingStruct
	{
		public ThingDef thing;

		public ThingDef thing2;

		public float weight;
	}

	public static readonly SimpleCurve PointsMarketValue = new SimpleCurve
	{
		new CurvePoint(100f, 200f),
		new CurvePoint(250f, 450f),
		new CurvePoint(800f, 2000f),
		new CurvePoint(10000f, 5000f)
	};

	public abstract IEnumerable<PreceptDef> DisallowedPrecepts { get; }

	public abstract PawnGroupKindDef WorkerGroupKind { get; }

	public virtual bool CanSpawnOn(PlanetTile tile)
	{
		return LootThings(tile).Any();
	}

	public override void Init(Site site, SitePart sitePart)
	{
		base.Init(site, sitePart);
		CampLootThingStruct loot = LootThings(site.Tile).RandomElementByWeight((CampLootThingStruct t) => t.weight);
		OnLootChosen(site, sitePart, loot);
		float x = PointsMarketValue.Evaluate(sitePart.parms.threatPoints);
		List<ThingDefCount> list = new List<ThingDefCount>();
		sitePart.things = new ThingOwner<Thing>(sitePart);
		sitePart.things.dontTickContents = true;
		List<ThingDefCount> list2 = new List<ThingDefCount>();
		float num = PointsMarketValue.Evaluate(x);
		if (loot.thing2 == null)
		{
			list2.Add(new ThingDefCount(loot.thing, Mathf.CeilToInt(num / loot.thing.BaseMarketValue)));
		}
		else
		{
			list2.Add(new ThingDefCount(loot.thing, Mathf.CeilToInt(num / 2f / loot.thing.BaseMarketValue)));
			list2.Add(new ThingDefCount(loot.thing2, Mathf.CeilToInt(num / 2f / loot.thing2.BaseMarketValue)));
		}
		foreach (ThingDefCount item in list2)
		{
			int num2 = item.Count;
			ThingDef thingDef = item.ThingDef;
			while (num2 > 0)
			{
				Thing thing = ThingMaker.MakeThing(thingDef);
				thing.stackCount = Mathf.Min(num2, thing.def.stackLimit);
				list.Add(new ThingDefCount(thingDef, thing.stackCount));
				num2 -= thing.stackCount;
				sitePart.things.TryAdd(thing);
			}
		}
		sitePart.lootThings = list;
		sitePart.expectedEnemyCount = GetEnemiesCount(site, sitePart.parms);
	}

	protected virtual void OnLootChosen(Site site, SitePart sitePart, CampLootThingStruct loot)
	{
	}

	public virtual IEnumerable<CampLootThingStruct> LootThings(PlanetTile tile)
	{
		foreach (SitePartDef.WorkSiteLootThing item in def.lootTable)
		{
			yield return new CampLootThingStruct
			{
				thing = item.thing,
				weight = item.weight
			};
		}
	}

	public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
	{
		string arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
		lookTargets = new LookTargets(map.Parent);
		return arrivedLetterPart;
	}

	public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
	{
		base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
		int enemiesCount = GetEnemiesCount(part.site, part.parms);
		outExtraDescriptionRules.Add(new Rule_String("enemiesCount", enemiesCount.ToString()));
		outExtraDescriptionRules.Add(new Rule_String("enemiesLabel", GetEnemiesLabel(part.site, enemiesCount)));
	}

	public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
	{
		return GenText.TrimEndNewlines(site.Label + ": " + "KnownSiteThreatEnemyCountAppend".Translate(GetEnemiesCount(site, sitePart.parms), "People".Translate())) + ("\n" + "Contains".Translate() + ": " + string.Join(", ", sitePart.lootThings.Select((ThingDefCount t) => t.ThingDef).Distinct().Select(delegate(ThingDef t)
		{
			int num = 0;
			foreach (ThingDefCount lootThing in sitePart.lootThings)
			{
				if (lootThing.ThingDef == t)
				{
					num += lootThing.Count;
				}
			}
			return string.Concat(t.LabelCap + " x", num.ToString());
		})));
	}

	public override SitePartParams GenerateDefaultParams(float myThreatPoints, PlanetTile tile, Faction faction)
	{
		SitePartParams sitePartParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
		sitePartParams.threatPoints = Mathf.Max(sitePartParams.threatPoints, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Settlement));
		return sitePartParams;
	}

	public override bool FactionCanOwn(Faction faction)
	{
		if (faction.Hidden || faction.temporary || !faction.def.pawnGroupMakers.Any((PawnGroupMaker x) => x.kindDef == WorkerGroupKind))
		{
			return false;
		}
		if (faction.ideos == null || faction.ideos.PrimaryIdeo == null)
		{
			return true;
		}
		Ideo ideology = faction.ideos.PrimaryIdeo;
		return !DisallowedPrecepts.Any((PreceptDef pDef) => ideology.PreceptsListForReading.Any((Precept p) => p.def == pDef));
	}

	protected int GetEnemiesCount(Site site, SitePartParams parms)
	{
		return GenStep_WorkSitePawns.GetEnemiesCount(site, parms, WorkerGroupKind);
	}

	protected string GetEnemiesLabel(Site site, int enemiesCount)
	{
		if (site.Faction == null)
		{
			return (enemiesCount == 1) ? "Enemy".Translate() : "Enemies".Translate();
		}
		if (enemiesCount != 1)
		{
			return site.Faction.def.pawnsPlural;
		}
		return site.Faction.def.pawnSingular;
	}
}
