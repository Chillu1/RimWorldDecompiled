using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompDestroyAfterDelay : ThingComp
	{
		public int spawnTick;

		public CompProperties_DestroyAfterDelay Props => (CompProperties_DestroyAfterDelay)props;

		public override void CompTick()
		{
			base.CompTick();
			if (Find.TickManager.TicksGame > spawnTick + Props.delayTicks && !parent.Destroyed)
			{
				parent.Destroy(Props.destroyMode);
			}
		}

		public override string CompInspectStringExtra()
		{
			if (Props.countdownLabel.NullOrEmpty())
			{
				return "";
			}
			int numTicks = Mathf.Max(0, spawnTick + Props.delayTicks - Find.TickManager.TicksGame);
			return Props.countdownLabel + ": " + numTicks.ToStringSecondsFromTicks();
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				spawnTick = Find.TickManager.TicksGame;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref spawnTick, "spawnTick", 0);
		}
	}
}
