namespace Verse.Noise
{
	public class InverseLerp : ModuleBase
	{
		private float from;

		private float to;

		public InverseLerp()
			: base(1)
		{
		}

		public InverseLerp(ModuleBase module, float from, float to)
			: base(1)
		{
			modules[0] = module;
			this.from = from;
			this.to = to;
		}

		public override double GetValue(double x, double y, double z)
		{
			double num = (modules[0].GetValue(x, y, z) - (double)from) / (double)(to - from);
			if (num < 0.0)
			{
				return 0.0;
			}
			if (num > 1.0)
			{
				return 1.0;
			}
			return num;
		}
	}
}
