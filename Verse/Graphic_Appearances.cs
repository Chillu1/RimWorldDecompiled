using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class Graphic_Appearances : Graphic
	{
		protected Graphic[] subGraphics;

		public override Material MatSingle => subGraphics[StuffAppearanceDefOf.Smooth.index].MatSingle;

		public override Material MatAt(Rot4 rot, Thing thing = null)
		{
			return SubGraphicFor(thing).MatAt(rot, thing);
		}

		public override void Init(GraphicRequest req)
		{
			data = req.graphicData;
			path = req.path;
			color = req.color;
			drawSize = req.drawSize;
			List<StuffAppearanceDef> allDefsListForReading = DefDatabase<StuffAppearanceDef>.AllDefsListForReading;
			subGraphics = new Graphic[allDefsListForReading.Count];
			for (int i = 0; i < subGraphics.Length; i++)
			{
				StuffAppearanceDef stuffAppearance = allDefsListForReading[i];
				string text = req.path;
				if (!stuffAppearance.pathPrefix.NullOrEmpty())
				{
					text = stuffAppearance.pathPrefix + "/" + text.Split('/').Last();
				}
				Texture2D texture2D = (from x in ContentFinder<Texture2D>.GetAllInFolder(text)
					where x.name.EndsWith(stuffAppearance.defName)
					select x).FirstOrDefault();
				if (texture2D != null)
				{
					subGraphics[i] = GraphicDatabase.Get<Graphic_Single>(text + "/" + texture2D.name, req.shader, drawSize, color);
				}
			}
			for (int j = 0; j < subGraphics.Length; j++)
			{
				if (subGraphics[j] == null)
				{
					subGraphics[j] = subGraphics[StuffAppearanceDefOf.Smooth.index];
				}
			}
		}

		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			if (newColorTwo != Color.white)
			{
				Log.ErrorOnce("Cannot use Graphic_Appearances.GetColoredVersion with a non-white colorTwo.", 9910251);
			}
			return GraphicDatabase.Get<Graphic_Appearances>(path, newShader, drawSize, newColor, Color.white, data);
		}

		public override Material MatSingleFor(Thing thing)
		{
			return SubGraphicFor(thing).MatSingleFor(thing);
		}

		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
		{
			SubGraphicFor(thing).DrawWorker(loc, rot, thingDef, thing, extraRotation);
		}

		public Graphic SubGraphicFor(Thing thing)
		{
			StuffAppearanceDef smooth = StuffAppearanceDefOf.Smooth;
			if (thing != null)
			{
				return SubGraphicFor(thing.Stuff);
			}
			return subGraphics[smooth.index];
		}

		public Graphic SubGraphicFor(ThingDef stuff)
		{
			StuffAppearanceDef stuffAppearanceDef = StuffAppearanceDefOf.Smooth;
			if (stuff != null && stuff.stuffProps.appearance != null)
			{
				stuffAppearanceDef = stuff.stuffProps.appearance;
			}
			return subGraphics[stuffAppearanceDef.index];
		}

		public override string ToString()
		{
			return string.Concat("Appearance(path=", path, ", color=", color, ", colorTwo=unsupported)");
		}
	}
}
