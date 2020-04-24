namespace Verse.Noise
{
	public class Displace : ModuleBase
	{
		public ModuleBase X
		{
			get
			{
				return modules[1];
			}
			set
			{
				modules[1] = value;
			}
		}

		public ModuleBase Y
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

		public ModuleBase Z
		{
			get
			{
				return modules[3];
			}
			set
			{
				modules[3] = value;
			}
		}

		public Displace()
			: base(4)
		{
		}

		public Displace(ModuleBase input, ModuleBase x, ModuleBase y, ModuleBase z)
			: base(4)
		{
			modules[0] = input;
			modules[1] = x;
			modules[2] = y;
			modules[3] = z;
		}

		public override double GetValue(double x, double y, double z)
		{
			double x2 = x + modules[1].GetValue(x, y, z);
			double y2 = y + modules[2].GetValue(x, y, z);
			double z2 = z + modules[3].GetValue(x, y, z);
			return modules[0].GetValue(x2, y2, z2);
		}
	}
}
