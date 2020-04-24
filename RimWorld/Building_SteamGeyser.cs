using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Building_SteamGeyser : Building
	{
		private IntermittentSteamSprayer steamSprayer;

		public Building harvester;

		private Sustainer spraySustainer;

		private int spraySustainerStartTick = -999;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			steamSprayer = new IntermittentSteamSprayer(this);
			steamSprayer.startSprayCallback = StartSpray;
			steamSprayer.endSprayCallback = EndSpray;
		}

		private void StartSpray()
		{
			SnowUtility.AddSnowRadial(this.OccupiedRect().RandomCell, base.Map, 4f, -0.06f);
			spraySustainer = SoundDefOf.GeyserSpray.TrySpawnSustainer(new TargetInfo(base.Position, base.Map));
			spraySustainerStartTick = Find.TickManager.TicksGame;
		}

		private void EndSpray()
		{
			if (spraySustainer != null)
			{
				spraySustainer.End();
				spraySustainer = null;
			}
		}

		public override void Tick()
		{
			if (harvester == null)
			{
				steamSprayer.SteamSprayerTick();
			}
			if (spraySustainer != null && Find.TickManager.TicksGame > spraySustainerStartTick + 1000)
			{
				Log.Message("Geyser spray sustainer still playing after 1000 ticks. Force-ending.");
				spraySustainer.End();
				spraySustainer = null;
			}
		}
	}
}
