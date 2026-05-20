using Verse;

namespace RimWorld;

public class OrbitalTransition : MusicTransition
{
	public override bool IsTransitionSatisfied()
	{
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		if (!base.IsTransitionSatisfied())
		{
			return false;
		}
		foreach (Map map in Find.Maps)
		{
			if (map.Tile.LayerDef.isSpace)
			{
				return true;
			}
		}
		return false;
	}
}
