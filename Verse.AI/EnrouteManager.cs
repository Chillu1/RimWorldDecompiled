using System.Collections.Generic;
using System.Text;
using RimWorld;

namespace Verse.AI;

public class EnrouteManager : IExposable
{
	private List<ThingCountTracker> enroute = new List<ThingCountTracker>();

	private Map map;

	private readonly Dictionary<IHaulEnroute, ThingCountTracker> lookup = new Dictionary<IHaulEnroute, ThingCountTracker>();

	public EnrouteManager(Map map)
	{
		this.map = map;
	}

	public void AddEnroute(IHaulEnroute container, Pawn pawn, ThingDef stuff, int count)
	{
		GetOrAddTracker(container).Add(pawn, stuff, count);
		pawn.MapHeld.events.Notify_HaulEnrouteAdded((Thing)container, pawn, stuff, count);
	}

	public int GetEnroute(IHaulEnroute thing, ThingDef stuff, Pawn excludeEnrouteFor = null)
	{
		if (lookup.TryGetValue(thing, out var value))
		{
			return value.Get(stuff, excludeEnrouteFor);
		}
		return 0;
	}

	public bool AnyEnrouteTo(Thing thing)
	{
		if (!(thing is IHaulEnroute key))
		{
			return false;
		}
		return lookup.ContainsKey(key);
	}

	public void Notify_ContainerDespawned(IHaulEnroute thing)
	{
		if (lookup.TryGetValue(thing, out var value))
		{
			enroute.Remove(value);
			lookup.Remove(thing);
		}
	}

	private ThingCountTracker GetOrAddTracker(IHaulEnroute container)
	{
		if (lookup.TryGetValue(container, out var value))
		{
			return value;
		}
		value = new ThingCountTracker(container);
		enroute.Add(value);
		lookup[container] = value;
		return value;
	}

	public void SendReservations(IHaulEnroute from, IHaulEnroute to)
	{
		if (lookup.TryGetValue(from, out var value))
		{
			GetOrAddTracker(to).CopyReservations(value);
		}
	}

	public void ReleaseFor(IHaulEnroute container, Pawn pawn)
	{
		if (lookup.TryGetValue(container, out var value))
		{
			value.ReleaseFor(pawn);
			if (value.CanCleanup())
			{
				lookup.Remove(container);
				enroute.Remove(value);
			}
			map.events.Notify_HaulEnrouteReleased((Thing)container, pawn);
		}
	}

	public void ReleaseAllClaimedBy(Pawn pawn)
	{
		for (int num = enroute.Count - 1; num >= 0; num--)
		{
			ThingCountTracker thingCountTracker = enroute[num];
			thingCountTracker.ReleaseFor(pawn);
			if (thingCountTracker.CanCleanup())
			{
				lookup.Remove(thingCountTracker.parent);
				enroute.RemoveAt(num);
			}
			map.events.Notify_HaulEnrouteReleased(thingCountTracker.ParentThing, pawn);
		}
	}

	public void InterruptEnroutePawns(IHaulEnroute container, Pawn exclude)
	{
		if (lookup.TryGetValue(container, out var value))
		{
			value.InterruptEnroutePawns(exclude);
			if (value.CanCleanup())
			{
				lookup.Remove(value.parent);
				enroute.Remove(value);
			}
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref enroute, "enroute", LookMode.Deep);
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		for (int num = enroute.Count - 1; num >= 0; num--)
		{
			if (enroute[num].parent == null)
			{
				enroute.RemoveAt(num);
			}
		}
		foreach (ThingCountTracker item in enroute)
		{
			lookup.Add(item.parent, item);
		}
	}

	public void LogEnroute()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("======= Enroute =======");
		stringBuilder.AppendLine("Count: " + enroute.Count);
		for (int i = 0; i < enroute.Count; i++)
		{
			ThingCountTracker thingCountTracker = enroute[i];
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(string.Format("[{0}]: {1}", i, (thingCountTracker.parent is Thing thing) ? thing.LabelShort : "NON-THING"));
			if (thingCountTracker.ReadOnlyPairs.Count == 0)
			{
				stringBuilder.AppendLine("   NO ELEMENTS - SHOULD HAVE BEEN RELEASED");
				continue;
			}
			foreach (var (thingDef2, list2) in thingCountTracker.ReadOnlyPairs)
			{
				stringBuilder.AppendLine($"   {thingDef2.label}, {thingCountTracker.Get(thingDef2)} enroute:");
				for (int j = 0; j < list2.Count; j++)
				{
					ThingCountTracker.PawnCount pawnCount = list2[j];
					stringBuilder.AppendLine($"      [{j}]: {pawnCount.pawn.LabelShort,-12} x {pawnCount.count}");
				}
			}
		}
		Log.Message(stringBuilder.ToString());
	}
}
