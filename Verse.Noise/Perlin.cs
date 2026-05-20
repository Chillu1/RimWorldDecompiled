using UnityEngine;

namespace Verse.Noise;

public class Perlin : ModuleBase
{
	private double frequency = 1.0;

	private double lacunarity = 2.0;

	private QualityMode quality = QualityMode.Medium;

	private int octaveCount = 6;

	private double persistence = 0.5;

	private int seed;

	private bool normalized;

	private bool invert;

	public double Frequency
	{
		get
		{
			return frequency;
		}
		set
		{
			frequency = value;
		}
	}

	public double Lacunarity
	{
		get
		{
			return lacunarity;
		}
		set
		{
			lacunarity = value;
		}
	}

	public QualityMode Quality
	{
		get
		{
			return quality;
		}
		set
		{
			quality = value;
		}
	}

	public int OctaveCount
	{
		get
		{
			return octaveCount;
		}
		set
		{
			octaveCount = Mathf.Clamp(value, 1, 30);
		}
	}

	public double Persistence
	{
		get
		{
			return persistence;
		}
		set
		{
			persistence = value;
		}
	}

	public int Seed
	{
		get
		{
			return seed;
		}
		set
		{
			seed = value;
		}
	}

	public bool Invert
	{
		get
		{
			return invert;
		}
		set
		{
			invert = value;
		}
	}

	public bool Normalized
	{
		get
		{
			return normalized;
		}
		set
		{
			normalized = value;
		}
	}

	public Perlin()
		: base(0)
	{
	}

	public Perlin(double frequency, double lacunarity, double persistence, int octaves, int seed, QualityMode quality)
		: base(0)
	{
		Frequency = frequency;
		Lacunarity = lacunarity;
		OctaveCount = octaves;
		Persistence = persistence;
		Seed = seed;
		Quality = quality;
	}

	public Perlin(double frequency, double lacunarity, double persistence, int octaves, bool normalized, bool invert, int seed, QualityMode quality)
		: base(0)
	{
		Frequency = frequency;
		Lacunarity = lacunarity;
		OctaveCount = octaves;
		Persistence = persistence;
		Seed = seed;
		Normalized = normalized;
		Invert = invert;
		Quality = quality;
	}

	public static double GetValue(double x, double y, double z, double frequency, int seed, double lacunarity = 2.0, double persistence = 0.5, int octaveCount = 6, bool normalized = false, bool invert = false, QualityMode quality = QualityMode.Medium)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 1.0;
		x *= frequency;
		y *= frequency;
		z *= frequency;
		for (int i = 0; i < octaveCount; i++)
		{
			double x2 = Utils.MakeInt32Range(x);
			double y2 = Utils.MakeInt32Range(y);
			double z2 = Utils.MakeInt32Range(z);
			long num4 = (seed + i) & 0xFFFFFFFFu;
			num2 = Utils.GradientCoherentNoise3D(x2, y2, z2, num4, quality);
			num += num2 * num3;
			x *= lacunarity;
			y *= lacunarity;
			z *= lacunarity;
			num3 *= persistence;
		}
		if (normalized)
		{
			num = num * 0.5 + 0.5;
			if (num < 0.0)
			{
				num = 0.0;
			}
			if (num > 1.0)
			{
				num = 1.0;
			}
		}
		if (invert)
		{
			num = 1.0 - num;
		}
		return num;
	}

	public override double GetValue(double x, double y, double z)
	{
		return GetValue(x, y, z, frequency, seed, lacunarity, persistence, octaveCount, normalized, invert, quality);
	}
}
