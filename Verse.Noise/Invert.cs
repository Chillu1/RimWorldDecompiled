namespace Verse.Noise
{
	public class Invert : ModuleBase
	{
		public Invert()
			: base(1)
		{
		}

		public Invert(ModuleBase input)
			: base(1)
		{
			modules[0] = input;
		}

		public override double GetValue(double x, double y, double z)
		{
			return 0.0 - modules[0].GetValue(x, y, z);
		}
	}
}
