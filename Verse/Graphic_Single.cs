using UnityEngine;

namespace Verse
{
	public class Graphic_Single : Graphic
	{
		protected Material mat;

		public static readonly string MaskSuffix = "_m";

		public override Material MatSingle => mat;

		public override Material MatWest => mat;

		public override Material MatSouth => mat;

		public override Material MatEast => mat;

		public override Material MatNorth => mat;

		public override bool ShouldDrawRotated
		{
			get
			{
				if (data != null && !data.drawRotated)
				{
					return false;
				}
				return true;
			}
		}

		public override void Init(GraphicRequest req)
		{
			data = req.graphicData;
			path = req.path;
			color = req.color;
			colorTwo = req.colorTwo;
			drawSize = req.drawSize;
			MaterialRequest req2 = default(MaterialRequest);
			req2.mainTex = ContentFinder<Texture2D>.Get(req.path);
			req2.shader = req.shader;
			req2.color = color;
			req2.colorTwo = colorTwo;
			req2.renderQueue = req.renderQueue;
			req2.shaderParameters = req.shaderParameters;
			if (req.shader.SupportsMaskTex())
			{
				req2.maskTex = ContentFinder<Texture2D>.Get(req.path + MaskSuffix, reportFailure: false);
			}
			mat = MaterialPool.MatFrom(req2);
		}

		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			return GraphicDatabase.Get<Graphic_Single>(path, newShader, drawSize, newColor, newColorTwo, data);
		}

		public override Material MatAt(Rot4 rot, Thing thing = null)
		{
			return mat;
		}

		public override string ToString()
		{
			return "Single(path=" + path + ", color=" + color + ", colorTwo=" + colorTwo + ")";
		}
	}
}
