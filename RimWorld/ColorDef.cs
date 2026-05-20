using UnityEngine;
using Verse;

namespace RimWorld;

public class ColorDef : Def
{
	public Color color;

	public ColorType colorType;

	public int displayOrder = int.MaxValue;

	public bool displayInStylingStationUI = true;

	public bool randomlyPickable = true;
}
