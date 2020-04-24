namespace Verse
{
	public class WorldGenStepDef : Def
	{
		public float order;

		public WorldGenStep worldGenStep;

		public override void PostLoad()
		{
			base.PostLoad();
			worldGenStep.def = this;
		}
	}
}
