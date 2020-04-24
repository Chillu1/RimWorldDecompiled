using System;

namespace Verse.Noise
{
	public class Power : ModuleBase
	{
		public Power()
			: base(2)
		{
		}

		public Power(ModuleBase lhs, ModuleBase rhs)
			: base(2)
		{
			modules[0] = lhs;
			modules[1] = rhs;
		}

		public override double GetValue(double x, double y, double z)
		{
			return Math.Pow(modules[0].GetValue(x, y, z), modules[1].GetValue(x, y, z));
		}
	}
}
