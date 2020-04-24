using System;

namespace Verse.Noise
{
	public class PowerKeepSign : ModuleBase
	{
		public PowerKeepSign()
			: base(2)
		{
		}

		public PowerKeepSign(ModuleBase lhs, ModuleBase rhs)
			: base(2)
		{
			modules[0] = lhs;
			modules[1] = rhs;
		}

		public override double GetValue(double x, double y, double z)
		{
			double value = modules[0].GetValue(x, y, z);
			return (double)Math.Sign(value) * Math.Pow(Math.Abs(value), modules[1].GetValue(x, y, z));
		}
	}
}
