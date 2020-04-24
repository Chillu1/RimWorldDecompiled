namespace Verse.Noise
{
	public class Filter : ModuleBase
	{
		private float from;

		private float to;

		public Filter()
			: base(1)
		{
		}

		public Filter(ModuleBase module, float from, float to)
			: base(1)
		{
			modules[0] = module;
			this.from = from;
			this.to = to;
		}

		public override double GetValue(double x, double y, double z)
		{
			double value = modules[0].GetValue(x, y, z);
			if (value >= (double)from && value <= (double)to)
			{
				return 1.0;
			}
			return 0.0;
		}
	}
}
