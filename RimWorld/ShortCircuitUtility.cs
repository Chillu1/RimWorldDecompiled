using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class ShortCircuitUtility
{
	private static Dictionary<PowerNet, bool> tmpPowerNetHasActivePowerSource = new Dictionary<PowerNet, bool>();

	private static List<IntVec3> tmpCells = new List<IntVec3>();

	public static IEnumerable<Building> GetShortCircuitablePowerConduits(Map map)
	{
		tmpPowerNetHasActivePowerSource.Clear();
		try
		{
			List<Thing> conduits = map.listerThings.ThingsOfDef(ThingDefOf.PowerConduit);
			for (int i = 0; i < conduits.Count; i++)
			{
				Building building = (Building)conduits[i];
				CompPower powerComp = building.PowerComp;
				if (powerComp != null)
				{
					if (!tmpPowerNetHasActivePowerSource.TryGetValue(powerComp.PowerNet, out var value))
					{
						value = powerComp.PowerNet.HasActivePowerSource;
						tmpPowerNetHasActivePowerSource.Add(powerComp.PowerNet, value);
					}
					if (value)
					{
						yield return building;
					}
				}
			}
		}
		finally
		{
			tmpPowerNetHasActivePowerSource.Clear();
		}
	}

	public static void DoShortCircuit(Building culprit)
	{
		PowerNet powerNet = culprit.PowerComp.PowerNet;
		Map map = culprit.Map;
		float totalEnergy = 0f;
		float explosionRadius = 0f;
		bool flag = false;
		if (powerNet.batteryComps.Any((CompPowerBattery x) => x.StoredEnergy > 20f))
		{
			DrainBatteriesAndCauseExplosion(powerNet, culprit, out totalEnergy, out explosionRadius);
		}
		else
		{
			flag = TryStartFireNear(culprit);
		}
		string text = ((culprit.def != ThingDefOf.PowerConduit) ? Find.ActiveLanguageWorker.WithIndefiniteArticlePostProcessed(culprit.Label) : ((string)"AnElectricalConduit".Translate()));
		StringBuilder stringBuilder = new StringBuilder();
		if (flag)
		{
			stringBuilder.Append("ShortCircuitStartedFire".Translate(text));
		}
		else
		{
			stringBuilder.Append("ShortCircuit".Translate(text));
		}
		if (totalEnergy > 0f)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.Append("ShortCircuitDischargedEnergy".Translate(totalEnergy.ToString("F0")));
		}
		if (explosionRadius > 5f)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.Append("ShortCircuitWasLarge".Translate());
		}
		if (explosionRadius > 8f)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.Append("ShortCircuitWasHuge".Translate());
		}
		Find.LetterStack.ReceiveLetter("LetterLabelShortCircuit".Translate(), stringBuilder.ToString(), LetterDefOf.NegativeEvent, new TargetInfo(culprit.Position, map));
	}

	public static bool TryShortCircuitInRain(Thing thing)
	{
		CompPowerTrader compPowerTrader = thing.TryGetComp<CompPowerTrader>();
		if ((compPowerTrader != null && compPowerTrader.PowerOn && compPowerTrader.Props.shortCircuitInRain) || (thing.TryGetComp<CompPowerBattery>() != null && thing.TryGetComp<CompPowerBattery>().StoredEnergy > 100f))
		{
			TaggedString taggedString = "ShortCircuitRain".Translate(thing.Label, thing);
			TargetInfo targetInfo = new TargetInfo(thing.Position, thing.Map);
			if (thing.Faction == Faction.OfPlayer)
			{
				Find.LetterStack.ReceiveLetter("LetterLabelShortCircuit".Translate(), taggedString, LetterDefOf.NegativeEvent, targetInfo);
			}
			else
			{
				Messages.Message(taggedString, targetInfo, MessageTypeDefOf.NeutralEvent);
			}
			GenExplosion.DoExplosion(thing.OccupiedRect().RandomCell, thing.Map, 1.9f, DamageDefOf.Flame, null);
			return true;
		}
		return false;
	}

	private static void DrainBatteriesAndCauseExplosion(PowerNet net, Building culprit, out float totalEnergy, out float explosionRadius)
	{
		totalEnergy = 0f;
		for (int i = 0; i < net.batteryComps.Count; i++)
		{
			CompPowerBattery compPowerBattery = net.batteryComps[i];
			totalEnergy += compPowerBattery.StoredEnergy;
			compPowerBattery.DrawPower(compPowerBattery.StoredEnergy);
		}
		explosionRadius = Mathf.Sqrt(totalEnergy) * 0.05f;
		explosionRadius = Mathf.Clamp(explosionRadius, 1.5f, 14.9f);
		GenExplosion.DoExplosion(culprit.Position, net.Map, explosionRadius, DamageDefOf.Flame, null);
		if (explosionRadius > 3.5f)
		{
			GenExplosion.DoExplosion(culprit.Position, net.Map, explosionRadius * 0.3f, DamageDefOf.Bomb, null);
		}
	}

	private static bool TryStartFireNear(Building b)
	{
		tmpCells.Clear();
		int num = GenRadial.NumCellsInRadius(3f);
		CellRect startRect = b.OccupiedRect();
		for (int i = 0; i < num; i++)
		{
			IntVec3 intVec = b.Position + GenRadial.RadialPattern[i];
			if (GenSight.LineOfSight(b.Position, intVec, b.Map, startRect, CellRect.SingleCell(intVec)) && FireUtility.ChanceToStartFireIn(intVec, b.Map) > 0f)
			{
				tmpCells.Add(intVec);
			}
		}
		if (tmpCells.Any())
		{
			return FireUtility.TryStartFireIn(tmpCells.RandomElement(), b.Map, Rand.Range(0.1f, 1.75f), null);
		}
		return false;
	}
}
