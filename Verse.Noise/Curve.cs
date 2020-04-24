using System.Collections.Generic;
using UnityEngine;

namespace Verse.Noise
{
	public class Curve : ModuleBase
	{
		private List<KeyValuePair<double, double>> m_data = new List<KeyValuePair<double, double>>();

		public int ControlPointCount => m_data.Count;

		public List<KeyValuePair<double, double>> ControlPoints => m_data;

		public Curve()
			: base(1)
		{
		}

		public Curve(ModuleBase input)
			: base(1)
		{
			modules[0] = input;
		}

		public void Add(double input, double output)
		{
			KeyValuePair<double, double> item = new KeyValuePair<double, double>(input, output);
			if (!m_data.Contains(item))
			{
				m_data.Add(item);
			}
			m_data.Sort((KeyValuePair<double, double> lhs, KeyValuePair<double, double> rhs) => lhs.Key.CompareTo(rhs.Key));
		}

		public void Clear()
		{
			m_data.Clear();
		}

		public override double GetValue(double x, double y, double z)
		{
			double value = modules[0].GetValue(x, y, z);
			int i;
			for (i = 0; i < m_data.Count && !(value < m_data[i].Key); i++)
			{
			}
			int index = Mathf.Clamp(i - 2, 0, m_data.Count - 1);
			int num = Mathf.Clamp(i - 1, 0, m_data.Count - 1);
			int num2 = Mathf.Clamp(i, 0, m_data.Count - 1);
			int index2 = Mathf.Clamp(i + 1, 0, m_data.Count - 1);
			if (num == num2)
			{
				return m_data[num].Value;
			}
			double key = m_data[num].Key;
			double key2 = m_data[num2].Key;
			double position = (value - key) / (key2 - key);
			return Utils.InterpolateCubic(m_data[index].Value, m_data[num].Value, m_data[num2].Value, m_data[index2].Value, position);
		}
	}
}
