using Verse;

namespace RimWorld
{
	public class GameCondition_TemperatureOffset : GameCondition
	{
		public float tempOffset;

		public override void Init()
		{
			base.Init();
			tempOffset = def.temperatureOffset;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref tempOffset, "tempOffset", 0f);
		}

		public override float TemperatureOffset()
		{
			return tempOffset;
		}
	}
}
