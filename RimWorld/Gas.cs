using Verse;

namespace RimWorld
{
	public class Gas : Thing
	{
		public int destroyTick;

		public float graphicRotation;

		public float graphicRotationSpeed;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			while (true)
			{
				Thing gas = base.Position.GetGas(map);
				if (gas == null)
				{
					break;
				}
				gas.Destroy();
			}
			base.SpawnSetup(map, respawningAfterLoad);
			destroyTick = Find.TickManager.TicksGame + def.gas.expireSeconds.RandomInRange.SecondsToTicks();
			graphicRotationSpeed = Rand.Range(0f - def.gas.rotationSpeed, def.gas.rotationSpeed) / 60f;
		}

		public override void Tick()
		{
			if (destroyTick <= Find.TickManager.TicksGame)
			{
				Destroy();
			}
			graphicRotation += graphicRotationSpeed;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref destroyTick, "destroyTick", 0);
		}
	}
}
