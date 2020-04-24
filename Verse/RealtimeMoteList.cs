using System.Collections.Generic;

namespace Verse
{
	public class RealtimeMoteList
	{
		public List<Mote> allMotes = new List<Mote>();

		public void Clear()
		{
			allMotes.Clear();
		}

		public void MoteSpawned(Mote newMote)
		{
			allMotes.Add(newMote);
		}

		public void MoteDespawned(Mote oldMote)
		{
			allMotes.Remove(oldMote);
		}

		public void MoteListUpdate()
		{
			for (int num = allMotes.Count - 1; num >= 0; num--)
			{
				allMotes[num].RealtimeUpdate();
			}
		}
	}
}
