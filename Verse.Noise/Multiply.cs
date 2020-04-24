namespace Verse.Noise
{
	public class Multiply : ModuleBase
	{
		public Multiply()
			: base(2)
		{
		}

		public Multiply(ModuleBase lhs, ModuleBase rhs)
			: base(2)
		{
			modules[0] = lhs;
			modules[1] = rhs;
		}

		public override double GetValue(double x, double y, double z)
		{
			return modules[0].GetValue(x, y, z) * modules[1].GetValue(x, y, z);
		}
	}
}
