using UnityEngine;

namespace Verse
{
	public class Verb_ArcSpray : Verb_Spray
	{
		protected override void PreparePath()
		{
			path.Clear();
			Vector3 normalized = (currentTarget.CenterVector3 - caster.Position.ToVector3Shifted()).Yto0().normalized;
			Vector3 tan = normalized.RotatedBy(90f);
			for (int i = 0; i < verbProps.sprayNumExtraCells; i++)
			{
				for (int j = 0; j < 15; j++)
				{
					float value = Rand.Value;
					float num = Rand.Value - 0.5f;
					float num2 = value * verbProps.sprayWidth * 2f - verbProps.sprayWidth;
					float num3 = num * (float)verbProps.sprayThicknessCells + num * 2f * verbProps.sprayArching;
					IntVec3 item = (currentTarget.CenterVector3 + num2 * tan - num3 * normalized).ToIntVec3();
					if (!path.Contains(item) || Rand.Value < 0.25f)
					{
						path.Add(item);
						break;
					}
				}
			}
			path.Add(currentTarget.Cell);
			path.SortBy((IntVec3 c) => (c.ToVector3Shifted() - caster.DrawPos).Yto0().normalized.AngleToFlat(tan));
		}
	}
}
