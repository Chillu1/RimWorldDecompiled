using Verse;

namespace RimWorld
{
	public class Building_TempControl : Building
	{
		public CompTempControl compTempControl;

		public CompPowerTrader compPowerTrader;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			compTempControl = GetComp<CompTempControl>();
			compPowerTrader = GetComp<CompPowerTrader>();
		}
	}
}
