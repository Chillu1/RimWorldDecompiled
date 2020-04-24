using Verse;

namespace RimWorld
{
	public class LiquidFuel : Filth
	{
		private int spawnTick;

		private const int DryOutTime = 1500;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref spawnTick, "spawnTick", 0);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			spawnTick = Find.TickManager.TicksGame;
		}

		public void Refill()
		{
			spawnTick = Find.TickManager.TicksGame;
		}

		public override void Tick()
		{
			if (spawnTick + 1500 < Find.TickManager.TicksGame)
			{
				Destroy();
			}
		}

		public override void ThickenFilth()
		{
			base.ThickenFilth();
			Refill();
		}
	}
}
