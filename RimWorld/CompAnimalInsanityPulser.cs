using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompAnimalInsanityPulser : ThingComp
	{
		private int ticksToInsanityPulse;

		public CompProperties_AnimalInsanityPulser Props => (CompProperties_AnimalInsanityPulser)props;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				ticksToInsanityPulse = Props.pulseInterval.RandomInRange;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref ticksToInsanityPulse, "ticksToInsanityPulse", 0);
		}

		public override void CompTick()
		{
			if (parent.Spawned)
			{
				ticksToInsanityPulse--;
				if (ticksToInsanityPulse <= 0)
				{
					DoAnimalInsanityPulse();
					ticksToInsanityPulse = Props.pulseInterval.RandomInRange;
				}
			}
		}

		private void DoAnimalInsanityPulse()
		{
			IEnumerable<Pawn> enumerable = parent.Map.mapPawns.AllPawnsSpawned.Where((Pawn p) => p.RaceProps.Animal && p.Position.InHorDistOf(parent.Position, Props.radius));
			bool flag = false;
			foreach (Pawn item in enumerable)
			{
				if (item.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter))
				{
					flag = true;
				}
			}
			if (flag)
			{
				Messages.Message("MessageAnimalInsanityPulse".Translate(parent.Named("SOURCE")), parent, MessageTypeDefOf.ThreatSmall);
				SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(parent.Map);
				if (parent.Map == Find.CurrentMap)
				{
					Find.CameraDriver.shaker.DoShake(4f);
				}
			}
		}
	}
}
