using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class RitualBehaviorWorker_DancePartyTech : RitualBehaviorWorker
	{
		private Sustainer soundPlaying;

		public override Sustainer SoundPlaying => soundPlaying;

		public RitualBehaviorWorker_DancePartyTech()
		{
		}

		public RitualBehaviorWorker_DancePartyTech(RitualBehaviorDef def)
			: base(def)
		{
		}

		public override void Tick(LordJob_Ritual ritual)
		{
			base.Tick(ritual);
			Thing thing = ritual.selectedTarget.Thing;
			if (Find.TickManager.TicksGame % 20 == 0)
			{
				SoundDef soundDef = null;
				foreach (Pawn ownedPawn in ritual.lord.ownedPawns)
				{
					if (GatheringsUtility.InGatheringArea(ownedPawn.Position, ritual.selectedTarget.Cell, ritual.Map))
					{
						soundDef = SoundDefOf.DanceParty_NoMusic;
						break;
					}
				}
				if (thing != null)
				{
					CompLightball compLightball = thing.TryGetComp<CompLightball>();
					if (compLightball != null)
					{
						soundDef = (compLightball.Playing ? compLightball.SoundToPlay : null);
					}
				}
				if (soundDef != null && (soundPlaying == null || soundPlaying.def != soundDef))
				{
					soundPlaying = soundDef.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(ritual.selectedTarget.Cell, ritual.selectedTarget.Map), MaintenanceType.PerTick));
				}
			}
			if (soundPlaying != null && !soundPlaying.Ended)
			{
				soundPlaying.Maintain();
			}
		}

		public override void Cleanup(LordJob_Ritual ritual)
		{
			base.Cleanup(ritual);
			soundPlaying = null;
		}
	}
}
