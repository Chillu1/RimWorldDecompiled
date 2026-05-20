using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Alert_NeedWarmClothes : Alert
{
	private PriorityQueue<Thing, float> jackets = new PriorityQueue<Thing, float>();

	private PriorityQueue<Thing, float> shirts = new PriorityQueue<Thing, float>();

	private PriorityQueue<Thing, float> pants = new PriorityQueue<Thing, float>();

	private List<Pawn> colonistsWithoutWarmClothes = new List<Pawn>();

	private int needWarmClothesCount;

	private int colonistsWithWarmClothesCount;

	private int missingWarmClothesCount;

	private float lowestTemperatureComing;

	private List<Thing> tmpApparelList = new List<Thing>();

	private const int CheckNextTwelfthsCount = 3;

	private const float CanShowAlertOnlyIfTempBelow = 5f;

	public Alert_NeedWarmClothes()
	{
		defaultLabel = "NeedWarmClothes".Translate();
		defaultPriority = AlertPriority.High;
	}

	private bool AnyColonistsNeedWarmClothes(Map map)
	{
		colonistsWithoutWarmClothes.Clear();
		needWarmClothesCount = 0;
		colonistsWithWarmClothesCount = 0;
		missingWarmClothesCount = 0;
		if (lowestTemperatureComing < ThingDefOf.Human.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin))
		{
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				if (!item.DevelopmentalStage.Baby() && !item.IsGhoul && !item.IsShambler && !item.apparel.AnyApparelLocked && (item.story?.traits == null || !item.story.traits.HasTrait(TraitDefOf.Nudist)) && (item.Ideo == null || !item.Ideo.IdeoPrefersNudity()))
				{
					needWarmClothesCount++;
					if (item.GetStatValue(StatDefOf.ComfyTemperatureMin) <= lowestTemperatureComing)
					{
						colonistsWithWarmClothesCount++;
					}
					else
					{
						colonistsWithoutWarmClothes.Add(item);
					}
				}
			}
			missingWarmClothesCount = Mathf.Max(needWarmClothesCount - colonistsWithWarmClothesCount - FreeWarmClothesSetsCount(map), 0);
			return missingWarmClothesCount > 0;
		}
		return false;
	}

	private int FreeWarmClothesSetsCount(Map map)
	{
		jackets.Clear();
		shirts.Clear();
		pants.Clear();
		tmpApparelList.Clear();
		tmpApparelList.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.Apparel));
		foreach (IHaulSource item2 in map.haulDestinationManager.AllHaulSourcesListForReading)
		{
			foreach (Thing item3 in (IEnumerable<Thing>)item2.GetDirectlyHeldThings())
			{
				if (item3 is Apparel item)
				{
					tmpApparelList.Add(item);
				}
			}
		}
		for (int i = 0; i < tmpApparelList.Count; i++)
		{
			if (!tmpApparelList[i].IsInAnyStorage())
			{
				continue;
			}
			float statValue = tmpApparelList[i].GetStatValue(StatDefOf.Insulation_Cold, applyPostProcess: true, 120);
			if (statValue <= 0f)
			{
				continue;
			}
			if (tmpApparelList[i].def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
			{
				if (tmpApparelList[i].def.apparel.layers.Contains(ApparelLayerDefOf.OnSkin))
				{
					shirts.Enqueue(tmpApparelList[i], 0f - statValue);
				}
				else
				{
					jackets.Enqueue(tmpApparelList[i], 0f - statValue);
				}
			}
			if (tmpApparelList[i].def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs))
			{
				pants.Enqueue(tmpApparelList[i], 0f - statValue);
			}
		}
		tmpApparelList.Clear();
		float num = ThingDefOf.Human.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin) - lowestTemperatureComing;
		if (num <= 0f)
		{
			return GenMath.Max(jackets.Count, shirts.Count, pants.Count);
		}
		int num2 = 0;
		while (jackets.Count > 0 || shirts.Count > 0 || pants.Count > 0)
		{
			float num3 = 0f;
			if (jackets.TryDequeue(out var element, out var priority))
			{
				num3 += priority;
			}
			if (shirts.Count > 0 && num3 < num && shirts.TryDequeue(out element, out var priority2))
			{
				num3 += priority2;
			}
			if (pants.Count > 0 && num3 < num && pants.TryDequeue(out element, out var priority3))
			{
				num3 += priority3;
			}
			if (!(num3 >= num))
			{
				break;
			}
			num2++;
		}
		jackets.Clear();
		shirts.Clear();
		pants.Clear();
		return num2;
	}

	public override TaggedString GetExplanation()
	{
		if (missingWarmClothesCount == needWarmClothesCount)
		{
			return "NeedWarmClothesDesc1All".Translate() + "\n\n" + "NeedWarmClothesDesc2".Translate(lowestTemperatureComing.ToStringTemperature("F0"));
		}
		return "NeedWarmClothesDesc1".Translate(missingWarmClothesCount) + "\n\n" + "NeedWarmClothesDesc2".Translate(lowestTemperatureComing.ToStringTemperature("F0"));
	}

	public override AlertReport GetReport()
	{
		if (MapWithMissingWarmClothes() == null)
		{
			return false;
		}
		return AlertReport.CulpritsAre(colonistsWithoutWarmClothes);
	}

	private Map MapWithMissingWarmClothes()
	{
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			Map map = maps[i];
			if (map.IsPlayerHome && !map.Biome.inVacuum)
			{
				lowestTemperatureComing = LowestTemperatureComing(map);
				if (!(lowestTemperatureComing >= 5f) && AnyColonistsNeedWarmClothes(map))
				{
					return map;
				}
			}
		}
		return null;
	}

	private float LowestTemperatureComing(Map map)
	{
		Twelfth twelfth = GenLocalDate.Twelfth(map);
		float a = GetTemperature(twelfth, map);
		for (int i = 0; i < 3; i++)
		{
			twelfth = twelfth.NextTwelfth();
			a = Mathf.Min(a, GetTemperature(twelfth, map));
		}
		return Mathf.Min(a, map.mapTemperature.OutdoorTemp);
	}

	private float GetTemperature(Twelfth twelfth, Map map)
	{
		return GenTemperature.AverageTemperatureAtTileForTwelfth(map.Tile, twelfth);
	}
}
