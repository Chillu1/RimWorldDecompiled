using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class EscapeShip : MapParent
{
	private Material cachedPostGenerateMat;

	public override Texture2D ExpandingIcon
	{
		get
		{
			if (!base.HasMap || base.Faction == null)
			{
				return base.ExpandingIcon;
			}
			return base.Faction.def.FactionIcon;
		}
	}

	public override Color ExpandingIconColor
	{
		get
		{
			if (!base.HasMap || base.Faction == null)
			{
				return base.ExpandingIconColor;
			}
			return base.Faction.Color;
		}
	}

	public override Material Material
	{
		get
		{
			if (!base.HasMap || base.Faction == null)
			{
				return base.Material;
			}
			if (cachedPostGenerateMat == null)
			{
				cachedPostGenerateMat = MaterialPool.MatFrom(base.Faction.def.settlementTexturePath, ShaderDatabase.WorldOverlayTransparentLit, base.Faction.Color, 3550);
			}
			return cachedPostGenerateMat;
		}
	}

	public override void PostMapGenerate()
	{
		base.PostMapGenerate();
		Find.World.renderer.SetDirty<WorldDrawLayer_WorldObjects>(base.Tile.Layer);
	}
}
