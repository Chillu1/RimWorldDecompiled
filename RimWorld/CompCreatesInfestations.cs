using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompCreatesInfestations : ThingComp
	{
		private int lastCreatedInfestationTick = -999999;

		private const float MinRefireDays = 7f;

		private const float PreventInfestationsDist = 10f;

		public bool CanCreateInfestationNow
		{
			get
			{
				CompDeepDrill comp = parent.GetComp<CompDeepDrill>();
				if (comp != null && !comp.UsedLastTick())
				{
					return false;
				}
				if (CantFireBecauseCreatedInfestationRecently)
				{
					return false;
				}
				if (CantFireBecauseSomethingElseCreatedInfestationRecently)
				{
					return false;
				}
				return true;
			}
		}

		public bool CantFireBecauseCreatedInfestationRecently => Find.TickManager.TicksGame <= lastCreatedInfestationTick + 420000;

		public bool CantFireBecauseSomethingElseCreatedInfestationRecently
		{
			get
			{
				if (!parent.Spawned)
				{
					return false;
				}
				List<Thing> list = parent.Map.listerThings.ThingsInGroup(ThingRequestGroup.CreatesInfestations);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] != parent && list[i].Position.InHorDistOf(parent.Position, 10f) && list[i].TryGetComp<CompCreatesInfestations>().CantFireBecauseCreatedInfestationRecently)
					{
						return true;
					}
				}
				return false;
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref lastCreatedInfestationTick, "lastCreatedInfestationTick", -999999);
		}

		public void Notify_CreatedInfestation()
		{
			lastCreatedInfestationTick = Find.TickManager.TicksGame;
		}
	}
}
