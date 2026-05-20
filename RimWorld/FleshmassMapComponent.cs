using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class FleshmassMapComponent : MapComponent
{
	private Queue<Thing> fleshmassToDestroy = new Queue<Thing>();

	private Queue<int> fleshmassDestroyTicks = new Queue<int>();

	private bool destroyInChunks;

	public FleshmassMapComponent(Map map)
		: base(map)
	{
	}

	public override void MapComponentTick()
	{
		if (fleshmassDestroyTicks == null || fleshmassDestroyTicks.Count == 0)
		{
			return;
		}
		int num = fleshmassDestroyTicks.Peek();
		while (Find.TickManager.TicksGame >= num && fleshmassToDestroy.Count > 0)
		{
			Thing thing = fleshmassToDestroy.Dequeue();
			fleshmassDestroyTicks.Dequeue();
			if (thing != null && !thing.Destroyed)
			{
				if (destroyInChunks)
				{
					thing.Kill();
				}
				else
				{
					thing.Destroy(DestroyMode.KillFinalize);
				}
			}
			if (fleshmassDestroyTicks.Count > 0)
			{
				num = fleshmassDestroyTicks.Peek();
			}
		}
	}

	public void DestroyFleshmass(int destructionPeriodTicks, float rateExponent = 1f, bool destroyInChunks = false, Thing onlyFromSource = null)
	{
		PriorityQueue<Thing, int> priorityQueue = new PriorityQueue<Thing, int>();
		List<Thing> list = fleshmassToDestroy.ToList();
		List<int> list2 = fleshmassDestroyTicks.ToList();
		for (int i = 0; i < fleshmassToDestroy.Count; i++)
		{
			priorityQueue.Enqueue(list[i], list2[i]);
		}
		fleshmassToDestroy.Clear();
		fleshmassDestroyTicks.Clear();
		this.destroyInChunks = destroyInChunks;
		foreach (Thing item in map.listerThings.ThingsOfDef(ThingDefOf.Fleshmass_Active))
		{
			if (onlyFromSource == null || item.TryGetComp<CompFleshmass>()?.source == onlyFromSource)
			{
				float num = 1f - Mathf.Pow(Rand.Value, rateExponent);
				priorityQueue.Enqueue(item, Find.TickManager.TicksGame + Mathf.RoundToInt((float)destructionPeriodTicks * num));
			}
		}
		if (onlyFromSource == null)
		{
			foreach (Thing item2 in map.listerThings.ThingsOfDef(ThingDefOf.Fleshmass))
			{
				float num2 = 1f - Mathf.Pow(Rand.Value, rateExponent);
				priorityQueue.Enqueue(item2, Find.TickManager.TicksGame + Mathf.RoundToInt((float)destructionPeriodTicks * num2));
			}
		}
		while (priorityQueue.Count > 0)
		{
			priorityQueue.TryPeek(out var element, out var priority);
			priorityQueue.Dequeue();
			fleshmassToDestroy.Enqueue(element);
			fleshmassDestroyTicks.Enqueue(priority);
		}
	}

	public override void ExposeData()
	{
		Scribe_Collections.Look(ref fleshmassToDestroy, "fleshmassToDestroy", LookMode.Reference);
		Scribe_Collections.Look(ref fleshmassDestroyTicks, "fleshmassDestroyTicks", LookMode.Value);
		Scribe_Values.Look(ref destroyInChunks, "destroyInChunks", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (fleshmassToDestroy == null)
			{
				fleshmassToDestroy = new Queue<Thing>();
			}
			if (fleshmassDestroyTicks == null)
			{
				fleshmassDestroyTicks = new Queue<int>();
			}
		}
	}
}
