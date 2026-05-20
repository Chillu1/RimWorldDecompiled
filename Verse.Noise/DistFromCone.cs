using UnityEngine;

namespace Verse.Noise;

public class DistFromCone : ModuleBase
{
	private ModuleBase shape;

	private float span;

	public DistFromCone()
		: base(0)
	{
	}

	public DistFromCone(float gradient, float span)
		: base(0)
	{
		ModuleBase input = new AxisAsValueX();
		input = new Rotate(0.0, Mathf.Atan2(1f, gradient) * 57.29578f, 0.0, input);
		ModuleBase input2 = new AxisAsValueX();
		input2 = new Rotate(0.0, Mathf.Atan2(1f, 0f - gradient) * 57.29578f, 0.0, input2);
		shape = new Min(input, input2);
		shape = new ScaleBias(-1.0, 1.0, shape);
		this.span = span;
	}

	public override double GetValue(double x, double y, double z)
	{
		return shape.GetValue(x / (double)span, y / (double)span, z / (double)span);
	}
}
