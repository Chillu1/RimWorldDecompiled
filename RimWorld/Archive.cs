using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Archive : IExposable
	{
		private List<IArchivable> archivables = new List<IArchivable>();

		private HashSet<IArchivable> pinnedArchivables = new HashSet<IArchivable>();

		public const int MaxNonPinnedArchivables = 200;

		public List<IArchivable> ArchivablesListForReading => archivables;

		public void ExposeData()
		{
			Scribe_Collections.Look(ref archivables, "archivables", LookMode.Deep);
			Scribe_Collections.Look(ref pinnedArchivables, "pinnedArchivables", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				archivables.RemoveAll((IArchivable x) => x == null);
				pinnedArchivables.RemoveWhere((IArchivable x) => x == null);
			}
		}

		public bool Add(IArchivable archivable)
		{
			if (archivable == null)
			{
				Log.Error("Tried to add null archivable.");
				return false;
			}
			if (Contains(archivable))
			{
				return false;
			}
			archivables.Add(archivable);
			archivables.SortBy((IArchivable x) => x.CreatedTicksGame);
			CheckCullArchivables();
			return true;
		}

		public bool Remove(IArchivable archivable)
		{
			if (!Contains(archivable))
			{
				return false;
			}
			archivables.Remove(archivable);
			pinnedArchivables.Remove(archivable);
			return true;
		}

		public bool Contains(IArchivable archivable)
		{
			return archivables.Contains(archivable);
		}

		public void Pin(IArchivable archivable)
		{
			if (Contains(archivable) && !IsPinned(archivable))
			{
				pinnedArchivables.Add(archivable);
			}
		}

		public void Unpin(IArchivable archivable)
		{
			if (Contains(archivable) && IsPinned(archivable))
			{
				pinnedArchivables.Remove(archivable);
			}
		}

		public bool IsPinned(IArchivable archivable)
		{
			return pinnedArchivables.Contains(archivable);
		}

		private void CheckCullArchivables()
		{
			int num = 0;
			for (int i = 0; i < archivables.Count; i++)
			{
				if (!IsPinned(archivables[i]) && archivables[i].CanCullArchivedNow)
				{
					num++;
				}
			}
			int num2 = num - 200;
			for (int j = 0; j < archivables.Count; j++)
			{
				if (num2 <= 0)
				{
					break;
				}
				if (!IsPinned(archivables[j]) && archivables[j].CanCullArchivedNow && Remove(archivables[j]))
				{
					num2--;
					j--;
				}
			}
		}

		public void Notify_MapRemoved(Map map)
		{
			for (int i = 0; i < archivables.Count; i++)
			{
				archivables[i].LookTargets?.Notify_MapRemoved(map);
			}
		}
	}
}
