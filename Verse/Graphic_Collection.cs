using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public abstract class Graphic_Collection : Graphic
	{
		protected Graphic[] subGraphics;

		public override void Init(GraphicRequest req)
		{
			data = req.graphicData;
			if (req.path.NullOrEmpty())
			{
				throw new ArgumentNullException("folderPath");
			}
			if (req.shader == null)
			{
				throw new ArgumentNullException("shader");
			}
			base.path = req.path;
			color = req.color;
			colorTwo = req.colorTwo;
			drawSize = req.drawSize;
			List<Texture2D> list = (from x in ContentFinder<Texture2D>.GetAllInFolder(req.path)
				where !x.name.EndsWith(Graphic_Single.MaskSuffix)
				orderby x.name
				select x).ToList();
			if (list.NullOrEmpty())
			{
				Log.Error("Collection cannot init: No textures found at path " + req.path);
				subGraphics = new Graphic[1]
				{
					BaseContent.BadGraphic
				};
				return;
			}
			subGraphics = new Graphic[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				string path = req.path + "/" + list[i].name;
				subGraphics[i] = GraphicDatabase.Get(typeof(Graphic_Single), path, req.shader, drawSize, color, colorTwo, null, req.shaderParameters);
			}
		}
	}
}
