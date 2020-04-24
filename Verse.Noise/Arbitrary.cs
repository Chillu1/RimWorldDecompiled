using System;

namespace Verse.Noise
{
	public class Arbitrary : ModuleBase
	{
		private Func<double, double> processor;

		public Arbitrary()
			: base(1)
		{
		}

		public Arbitrary(ModuleBase source, Func<double, double> processor)
			: base(1)
		{
			modules[0] = source;
			this.processor = processor;
		}

		public override double GetValue(double x, double y, double z)
		{
			return processor(modules[0].GetValue(x, y, z));
		}
	}
}
