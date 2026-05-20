using System;

namespace Verse.Noise;

public class DistFromPoint : ModuleBase
{
	public float span = 1f;

	public DistFromPoint()
		: base(0)
	{
	}

	public DistFromPoint(float span)
		: base(0)
	{
		this.span = span;
	}

	public override double GetValue(double x, double y, double z)
	{
		if (span <= 0f)
		{
			Log.ErrorOnce("Span must be greater than zero", 8745456);
			return 0.0;
		}
		return Math.Sqrt(x * x + z * z) / (double)span;
	}
}
