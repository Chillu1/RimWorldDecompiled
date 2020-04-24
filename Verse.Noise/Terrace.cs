using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.Noise
{
	public class Terrace : ModuleBase
	{
		private List<double> m_data = new List<double>();

		private bool m_inverted;

		public int ControlPointCount => m_data.Count;

		public List<double> ControlPoints => m_data;

		public bool IsInverted
		{
			get
			{
				return m_inverted;
			}
			set
			{
				m_inverted = value;
			}
		}

		public Terrace()
			: base(1)
		{
		}

		public Terrace(ModuleBase input)
			: base(1)
		{
			modules[0] = input;
		}

		public Terrace(bool inverted, ModuleBase input)
			: base(1)
		{
			modules[0] = input;
			IsInverted = inverted;
		}

		public void Add(double input)
		{
			if (!m_data.Contains(input))
			{
				m_data.Add(input);
			}
			m_data.Sort((double lhs, double rhs) => lhs.CompareTo(rhs));
		}

		public void Clear()
		{
			m_data.Clear();
		}

		public void Generate(int steps)
		{
			if (steps < 2)
			{
				throw new ArgumentException("Need at least two steps");
			}
			Clear();
			double num = 2.0 / ((double)steps - 1.0);
			double num2 = -1.0;
			for (int i = 0; i < steps; i++)
			{
				Add(num2);
				num2 += num;
			}
		}

		public override double GetValue(double x, double y, double z)
		{
			double value = modules[0].GetValue(x, y, z);
			int i;
			for (i = 0; i < m_data.Count && !(value < m_data[i]); i++)
			{
			}
			int num = Mathf.Clamp(i - 1, 0, m_data.Count - 1);
			int num2 = Mathf.Clamp(i, 0, m_data.Count - 1);
			if (num == num2)
			{
				return m_data[num2];
			}
			double num3 = m_data[num];
			double num4 = m_data[num2];
			double num5 = (value - num3) / (num4 - num3);
			if (m_inverted)
			{
				num5 = 1.0 - num5;
				double num6 = num3;
				num3 = num4;
				num4 = num6;
			}
			num5 *= num5;
			return Utils.InterpolateLinear(num3, num4, num5);
		}
	}
}
