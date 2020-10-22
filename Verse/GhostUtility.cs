using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public static class GhostUtility
	{
		private static Dictionary<int, Graphic> ghostGraphics = new Dictionary<int, Graphic>();

		public static Graphic GhostGraphicFor(Graphic baseGraphic, ThingDef thingDef, Color ghostCol)
		{
			if (thingDef.useSameGraphicForGhost)
			{
				return baseGraphic;
			}
			int seed = 0;
			seed = Gen.HashCombine(seed, baseGraphic);
			seed = Gen.HashCombine(seed, thingDef);
			seed = Gen.HashCombineStruct(seed, ghostCol);
			if (!ghostGraphics.TryGetValue(seed, out var value))
			{
				if (thingDef.graphicData.Linked || thingDef.IsDoor)
				{
					value = GraphicDatabase.Get<Graphic_Single>(thingDef.uiIconPath, ShaderTypeDefOf.EdgeDetect.Shader, thingDef.graphicData.drawSize, ghostCol);
				}
				else
				{
					if (baseGraphic == null)
					{
						baseGraphic = thingDef.graphic;
					}
					GraphicData graphicData = null;
					if (baseGraphic.data != null)
					{
						graphicData = new GraphicData();
						graphicData.CopyFrom(baseGraphic.data);
						graphicData.shadowData = null;
					}
					value = GraphicDatabase.Get(baseGraphic.GetType(), baseGraphic.path, ShaderTypeDefOf.EdgeDetect.Shader, baseGraphic.drawSize, ghostCol, Color.white, graphicData, null);
				}
				ghostGraphics.Add(seed, value);
			}
			return value;
		}
	}
}
