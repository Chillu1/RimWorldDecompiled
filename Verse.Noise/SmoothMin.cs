namespace Verse.Noise;

public class SmoothMin : ModuleBase
{
	private double m_smoothness = 1.0;

	public SmoothMin()
		: base(2)
	{
	}

	public SmoothMin(ModuleBase lhs, ModuleBase rhs, double smoothness)
		: base(2)
	{
		modules[0] = lhs;
		modules[1] = rhs;
		m_smoothness = smoothness;
	}

	public override double GetValue(double x, double y, double z)
	{
		double value = modules[0].GetValue(x, y, z);
		double value2 = modules[1].GetValue(x, y, z);
		return GenMath.SmoothMin(value, value2, m_smoothness);
	}
}
