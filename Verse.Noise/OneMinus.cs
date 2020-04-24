namespace Verse.Noise
{
	public class OneMinus : ModuleBase
	{
		public OneMinus()
			: base(1)
		{
		}

		public OneMinus(ModuleBase module)
			: base(1)
		{
			modules[0] = module;
		}

		public override double GetValue(double x, double y, double z)
		{
			return 1.0 - modules[0].GetValue(x, y, z);
		}
	}
}
