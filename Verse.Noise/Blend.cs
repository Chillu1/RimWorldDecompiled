namespace Verse.Noise
{
	public class Blend : ModuleBase
	{
		public ModuleBase Controller
		{
			get
			{
				return modules[2];
			}
			set
			{
				modules[2] = value;
			}
		}

		public Blend()
			: base(3)
		{
		}

		public Blend(ModuleBase lhs, ModuleBase rhs, ModuleBase controller)
			: base(3)
		{
			modules[0] = lhs;
			modules[1] = rhs;
			modules[2] = controller;
		}

		public override double GetValue(double x, double y, double z)
		{
			double value = modules[0].GetValue(x, y, z);
			double value2 = modules[1].GetValue(x, y, z);
			double position = (modules[2].GetValue(x, y, z) + 1.0) / 2.0;
			return Utils.InterpolateLinear(value, value2, position);
		}
	}
}
