using System;

namespace Verse.Noise
{
	public class Max : ModuleBase
	{
		public Max()
			: base(2)
		{
		}

		public Max(ModuleBase lhs, ModuleBase rhs)
			: base(2)
		{
			modules[0] = lhs;
			modules[1] = rhs;
		}

		public override double GetValue(double x, double y, double z)
		{
			double value = modules[0].GetValue(x, y, z);
			double value2 = modules[1].GetValue(x, y, z);
			return Math.Max(value, value2);
		}
	}
}
