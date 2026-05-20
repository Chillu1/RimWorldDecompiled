using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class PlayLog : IExposable
{
	private List<LogEntry> entries = new List<LogEntry>();

	private const int Capacity = 150;

	public List<LogEntry> AllEntries => entries;

	public int LastTick
	{
		get
		{
			if (entries.Count == 0)
			{
				return 0;
			}
			return entries[0].Tick;
		}
	}

	public void Add(LogEntry entry)
	{
		entries.Insert(0, entry);
		ReduceToCapacity();
	}

	private void ReduceToCapacity()
	{
		while (entries.Count > 150)
		{
			RemoveEntry(entries[entries.Count - 1]);
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref entries, "entries", LookMode.Deep);
	}

	public void Notify_PawnDiscarded(Pawn p, bool silentlyRemoveReferences)
	{
		for (int num = entries.Count - 1; num >= 0; num--)
		{
			if (entries[num].Concerns(p))
			{
				if (!silentlyRemoveReferences)
				{
					Log.Warning("Discarding pawn " + p?.ToString() + ", but he is referenced by a play log entry " + entries[num]?.ToString() + ".");
				}
				RemoveEntry(entries[num]);
			}
		}
	}

	public void Notify_FactionRemoved(Faction faction)
	{
		for (int i = 0; i < entries.Count; i++)
		{
			entries[i].Notify_FactionRemoved(faction);
		}
	}

	public void Notify_IdeoRemoved(Ideo ideo)
	{
		for (int i = 0; i < entries.Count; i++)
		{
			entries[i].Notify_IdeoRemoved(ideo);
		}
	}

	private void RemoveEntry(LogEntry entry)
	{
		entries.Remove(entry);
	}

	public bool AnyEntryConcerns(Pawn p)
	{
		for (int i = 0; i < entries.Count; i++)
		{
			if (entries[i].Concerns(p))
			{
				return true;
			}
		}
		return false;
	}
}
