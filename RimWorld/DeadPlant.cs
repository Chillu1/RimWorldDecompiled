namespace RimWorld
{
	public class DeadPlant : Plant
	{
		protected override bool Resting => false;

		public override float GrowthRate => 0f;

		public override float CurrentDyingDamagePerTick => 0f;

		public override string LabelMouseover => LabelCap;

		public override PlantLifeStage LifeStage => PlantLifeStage.Mature;

		public override string GetInspectStringLowPriority()
		{
			return null;
		}

		public override string GetInspectString()
		{
			return "";
		}

		public override void CropBlighted()
		{
		}
	}
}
