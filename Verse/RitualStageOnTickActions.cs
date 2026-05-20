using System.Collections.Generic;
using RimWorld;

namespace Verse
{
	public class RitualStageOnTickActions : IExposable
	{
		public List<ActionOnTick> actions = new List<ActionOnTick>();

		public void ExposeData()
		{
			Scribe_Collections.Look(ref actions, "actions", LookMode.Deep);
		}
	}
}
