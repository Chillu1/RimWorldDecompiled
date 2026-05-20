namespace Verse.Noise;

public class DistFromAxis_Directional : ModuleBase
{
	public float span;

	public DistFromAxis_Directional()
		: base(0)
	{
	}

	public DistFromAxis_Directional(float span)
		: base(0)
	{
		this.span = span;
	}

	public override double GetValue(double x, double y, double z)
	{
		return x / (double)span;
	}
}
