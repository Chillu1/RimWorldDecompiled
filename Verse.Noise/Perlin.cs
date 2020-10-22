using UnityEngine;

namespace Verse.Noise
{
	public class Perlin : ModuleBase
	{
		private double frequency = 1.0;

		private double lacunarity = 2.0;

		private QualityMode quality = QualityMode.Medium;

		private int octaveCount = 6;

		private double persistence = 0.5;

		private int seed;

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

		public override double GetValue(double x, double y, double z)
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
			return num;
		}
	}
}
