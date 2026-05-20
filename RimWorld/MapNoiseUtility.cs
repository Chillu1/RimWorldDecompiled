using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public static class MapNoiseUtility
{
	public static ModuleBase FalloffAtEdge(float width, Rot4 side, Map map)
	{
		ModuleBase moduleBase = new DistFromAxis(width);
		if (side == Rot4.North)
		{
			moduleBase = new Rotate(0.0, 90.0, 0.0, moduleBase);
			moduleBase = new Translate(0.0, 0.0, -map.Size.z, moduleBase);
		}
		if (side == Rot4.East)
		{
			moduleBase = new Translate(-map.Size.x, 0.0, 0.0, moduleBase);
		}
		if (side == Rot4.South)
		{
			moduleBase = new Rotate(0.0, 90.0, 0.0, moduleBase);
		}
		return moduleBase;
	}

	public static ModuleBase FalloffAtAngle(float angle, float offsetPct, Map map)
	{
		ModuleBase input = new DistFromAxis_Directional((float)map.Size.x / 2f);
		input = new ScaleBias(0.5, 0.5, input);
		input = new Translate((float)map.Size.x * (0.5f - offsetPct), 0.0, 0.0, input);
		input = new Rotate(0.0, angle, 0.0, input);
		return new Translate((float)(-map.Size.x) / 2f, 0.0, (float)(-map.Size.z) / 2f, input);
	}

	public static ModuleBase CreateFalloffRadius(float radius, Vector2 offset, float exponent = 1f, bool invert = true)
	{
		ModuleBase input = new DistFromPoint(radius);
		if (invert)
		{
			input = new ScaleBias(-1.0, 1.0, input);
		}
		input = new Clamp(0.0, 1.0, input);
		input = new Power(input, new Const(exponent));
		return new Translate(0f - offset.x, 0.0, 0f - offset.y, input);
	}

	public static ModuleBase AddDisplacementNoise(ModuleBase baseShape, float frequency, float strength, int octaves = 4, int seed = -1)
	{
		if (seed < 0)
		{
			seed = Rand.Int;
		}
		ModuleBase lhs = new Perlin(frequency, 2.0, 0.5, octaves, seed, QualityMode.Medium);
		ModuleBase lhs2 = new Perlin(frequency, 2.0, 0.5, octaves, seed + 1, QualityMode.Medium);
		lhs = new Multiply(lhs, new Const(strength));
		lhs2 = new Multiply(lhs2, new Const(strength));
		return new Displace(baseShape, lhs, new Const(0.0), lhs2);
	}
}
