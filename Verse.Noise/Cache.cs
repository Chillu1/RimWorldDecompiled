namespace Verse.Noise
{
	public class Cache : ModuleBase
	{
		private double m_value;

		private bool m_cached;

		private double m_x;

		private double m_y;

		private double m_z;

		public override ModuleBase this[int index]
		{
			get
			{
				return base[index];
			}
			set
			{
				base[index] = value;
				m_cached = false;
			}
		}

		public Cache()
			: base(1)
		{
		}

		public Cache(ModuleBase input)
			: base(1)
		{
			modules[0] = input;
		}

		public override double GetValue(double x, double y, double z)
		{
			if (!m_cached || m_x != x || m_y != y || m_z != z)
			{
				m_value = modules[0].GetValue(x, y, z);
				m_x = x;
				m_y = y;
				m_z = z;
			}
			m_cached = true;
			return m_value;
		}
	}
}
