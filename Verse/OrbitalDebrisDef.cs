using System.Collections.Generic;

namespace Verse;

public class OrbitalDebrisDef : Def
{
	public class DebrisGraphic
	{
		public GraphicData graphicData;

		public float weighting = 1f;

		public int order;

		public float parallaxPer10Cells = 1f;
	}

	public List<DebrisGraphic> graphics = new List<DebrisGraphic>();
}
