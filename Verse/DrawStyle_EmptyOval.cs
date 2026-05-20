namespace Verse;

public class DrawStyle_EmptyOval : DrawStyle_FilledOval
{
	protected override bool Filled(float x, float z, float radius, float ratio)
	{
		if (Inside(x, z, radius, ratio))
		{
			if (Inside(x + 1f, z, radius, ratio) && Inside(x - 1f, z, radius, ratio) && Inside(x, z + 1f, radius, ratio) && Inside(x, z - 1f, radius, ratio) && Inside(x + 1f, z + 1f, radius, ratio) && Inside(x + 1f, z - 1f, radius, ratio) && Inside(x - 1f, z - 1f, radius, ratio))
			{
				return !Inside(x - 1f, z + 1f, radius, ratio);
			}
			return true;
		}
		return false;
	}
}
