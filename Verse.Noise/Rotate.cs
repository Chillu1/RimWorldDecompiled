using System;

namespace Verse.Noise
{
	public class Rotate : ModuleBase
	{
		private double m_x;

		private double m_x1Matrix;

		private double m_x2Matrix;

		private double m_x3Matrix;

		private double m_y;

		private double m_y1Matrix;

		private double m_y2Matrix;

		private double m_y3Matrix;

		private double m_z;

		private double m_z1Matrix;

		private double m_z2Matrix;

		private double m_z3Matrix;

		public double X
		{
			get
			{
				return m_x;
			}
			set
			{
				SetAngles(value, m_y, m_z);
			}
		}

		public double Y
		{
			get
			{
				return m_y;
			}
			set
			{
				SetAngles(m_x, value, m_z);
			}
		}

		public double Z
		{
			get
			{
				return m_x;
			}
			set
			{
				SetAngles(m_x, m_y, value);
			}
		}

		public Rotate()
			: base(1)
		{
			SetAngles(0.0, 0.0, 0.0);
		}

		public Rotate(ModuleBase input)
			: base(1)
		{
			modules[0] = input;
		}

		public Rotate(double x, double y, double z, ModuleBase input)
			: base(1)
		{
			modules[0] = input;
			SetAngles(x, y, z);
		}

		private void SetAngles(double x, double y, double z)
		{
			double num = Math.Cos(x * (Math.PI / 180.0));
			double num2 = Math.Cos(y * (Math.PI / 180.0));
			double num3 = Math.Cos(z * (Math.PI / 180.0));
			double num4 = Math.Sin(x * (Math.PI / 180.0));
			double num5 = Math.Sin(y * (Math.PI / 180.0));
			double num6 = Math.Sin(z * (Math.PI / 180.0));
			m_x1Matrix = num5 * num4 * num6 + num2 * num3;
			m_y1Matrix = num * num6;
			m_z1Matrix = num5 * num3 - num2 * num4 * num6;
			m_x2Matrix = num5 * num4 * num3 - num2 * num6;
			m_y2Matrix = num * num3;
			m_z2Matrix = (0.0 - num2) * num4 * num3 - num5 * num6;
			m_x3Matrix = (0.0 - num5) * num;
			m_y3Matrix = num4;
			m_z3Matrix = num2 * num;
			m_x = x;
			m_y = y;
			m_z = z;
		}

		public override double GetValue(double x, double y, double z)
		{
			double x2 = m_x1Matrix * x + m_y1Matrix * y + m_z1Matrix * z;
			double y2 = m_x2Matrix * x + m_y2Matrix * y + m_z2Matrix * z;
			double z2 = m_x3Matrix * x + m_y3Matrix * y + m_z3Matrix * z;
			return modules[0].GetValue(x2, y2, z2);
		}
	}
}
