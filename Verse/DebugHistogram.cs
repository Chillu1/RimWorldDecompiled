using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	public class DebugHistogram
	{
		private float[] buckets;

		private int[] counts;

		public DebugHistogram(float[] buckets)
		{
			this.buckets = buckets.Concat(float.PositiveInfinity).ToArray();
			counts = new int[this.buckets.Length];
		}

		public void Add(float val)
		{
			for (int i = 0; i < buckets.Length; i++)
			{
				if (buckets[i] > val)
				{
					counts[i]++;
					break;
				}
			}
		}

		public void Display()
		{
			StringBuilder stringBuilder = new StringBuilder();
			Display(stringBuilder);
			Log.Message(stringBuilder.ToString());
		}

		public void Display(StringBuilder sb)
		{
			int num = Mathf.Max(counts.Max(), 1);
			int num2 = counts.Aggregate((int a, int b) => a + b);
			for (int i = 0; i < buckets.Length; i++)
			{
				sb.AppendLine($"{new string('#', counts[i] * 40 / num)}    {buckets[i]}: {counts[i]} ({(double)counts[i] * 100.0 / (double)num2:F2}%)");
			}
		}
	}
}
