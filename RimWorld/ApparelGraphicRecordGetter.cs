using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ApparelGraphicRecordGetter
	{
		public static bool TryGetGraphicApparel(Apparel apparel, BodyTypeDef bodyType, out ApparelGraphicRecord rec)
		{
			if (bodyType == null)
			{
				Log.Error("Getting apparel graphic with undefined body type.");
				bodyType = BodyTypeDefOf.Male;
			}
			if (apparel.def.apparel.wornGraphicPath.NullOrEmpty())
			{
				rec = new ApparelGraphicRecord(null, null);
				return false;
			}
			string path = ((apparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead && !PawnRenderer.RenderAsPack(apparel) && !(apparel.def.apparel.wornGraphicPath == BaseContent.PlaceholderImagePath)) ? (apparel.def.apparel.wornGraphicPath + "_" + bodyType.defName) : apparel.def.apparel.wornGraphicPath);
			Shader shader = ShaderDatabase.Cutout;
			if (apparel.def.apparel.useWornGraphicMask)
			{
				shader = ShaderDatabase.CutoutComplex;
			}
			Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
			rec = new ApparelGraphicRecord(graphic, apparel);
			return true;
		}
	}
}
