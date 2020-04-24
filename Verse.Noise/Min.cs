using System;

namespace Verse.Noise
{
	public class Min : ModuleBase
	{
		public Min()
			: base(2)
		{
		}

		public Min(ModuleBase lhs, ModuleBase rhs)
			: base(2)
		{
			modules[0] = lhs;
			modules[1] = rhs;
		}

		public override double GetValue(double x, double y, double z)
		{
			double value = modules[0].GetValue(x, y, z);
			double value2 = modules[1].GetValue(x, y, z);
			return Math.Min(value, value2);
		}
	}
}
