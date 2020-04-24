using UnityEngine;

namespace Verse
{
	public class Graphic_Multi : Graphic
	{
		private Material[] mats = new Material[4];

		private bool westFlipped;

		private bool eastFlipped;

		private float drawRotatedExtraAngleOffset;

		public string GraphicPath => path;

		public override Material MatSingle => MatSouth;

		public override Material MatWest => mats[3];

		public override Material MatSouth => mats[2];

		public override Material MatEast => mats[1];

		public override Material MatNorth => mats[0];

		public override bool WestFlipped => westFlipped;

		public override bool EastFlipped => eastFlipped;

		public override bool ShouldDrawRotated
		{
			get
			{
				if (data != null && !data.drawRotated)
				{
					return false;
				}
				if (!(MatEast == MatNorth))
				{
					return MatWest == MatNorth;
				}
				return true;
			}
		}

		public override float DrawRotatedExtraAngleOffset => drawRotatedExtraAngleOffset;

		public override void Init(GraphicRequest req)
		{
			data = req.graphicData;
			path = req.path;
			color = req.color;
			colorTwo = req.colorTwo;
			drawSize = req.drawSize;
			Texture2D[] array = new Texture2D[mats.Length];
			array[0] = ContentFinder<Texture2D>.Get(req.path + "_north", reportFailure: false);
			array[1] = ContentFinder<Texture2D>.Get(req.path + "_east", reportFailure: false);
			array[2] = ContentFinder<Texture2D>.Get(req.path + "_south", reportFailure: false);
			array[3] = ContentFinder<Texture2D>.Get(req.path + "_west", reportFailure: false);
			if (array[0] == null)
			{
				if (array[2] != null)
				{
					array[0] = array[2];
					drawRotatedExtraAngleOffset = 180f;
				}
				else if (array[1] != null)
				{
					array[0] = array[1];
					drawRotatedExtraAngleOffset = -90f;
				}
				else if (array[3] != null)
				{
					array[0] = array[3];
					drawRotatedExtraAngleOffset = 90f;
				}
				else
				{
					array[0] = ContentFinder<Texture2D>.Get(req.path, reportFailure: false);
				}
			}
			if (array[0] == null)
			{
				Log.Error("Failed to find any textures at " + req.path + " while constructing " + this.ToStringSafe());
				return;
			}
			if (array[2] == null)
			{
				array[2] = array[0];
			}
			if (array[1] == null)
			{
				if (array[3] != null)
				{
					array[1] = array[3];
					eastFlipped = base.DataAllowsFlip;
				}
				else
				{
					array[1] = array[0];
				}
			}
			if (array[3] == null)
			{
				if (array[1] != null)
				{
					array[3] = array[1];
					westFlipped = base.DataAllowsFlip;
				}
				else
				{
					array[3] = array[0];
				}
			}
			Texture2D[] array2 = new Texture2D[mats.Length];
			if (req.shader.SupportsMaskTex())
			{
				array2[0] = ContentFinder<Texture2D>.Get(req.path + "_northm", reportFailure: false);
				array2[1] = ContentFinder<Texture2D>.Get(req.path + "_eastm", reportFailure: false);
				array2[2] = ContentFinder<Texture2D>.Get(req.path + "_southm", reportFailure: false);
				array2[3] = ContentFinder<Texture2D>.Get(req.path + "_westm", reportFailure: false);
				if (array2[0] == null)
				{
					if (array2[2] != null)
					{
						array2[0] = array2[2];
					}
					else if (array2[1] != null)
					{
						array2[0] = array2[1];
					}
					else if (array2[3] != null)
					{
						array2[0] = array2[3];
					}
				}
				if (array2[2] == null)
				{
					array2[2] = array2[0];
				}
				if (array2[1] == null)
				{
					if (array2[3] != null)
					{
						array2[1] = array2[3];
					}
					else
					{
						array2[1] = array2[0];
					}
				}
				if (array2[3] == null)
				{
					if (array2[1] != null)
					{
						array2[3] = array2[1];
					}
					else
					{
						array2[3] = array2[0];
					}
				}
			}
			for (int i = 0; i < mats.Length; i++)
			{
				MaterialRequest req2 = default(MaterialRequest);
				req2.mainTex = array[i];
				req2.shader = req.shader;
				req2.color = color;
				req2.colorTwo = colorTwo;
				req2.maskTex = array2[i];
				req2.shaderParameters = req.shaderParameters;
				mats[i] = MaterialPool.MatFrom(req2);
			}
		}

		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			return GraphicDatabase.Get<Graphic_Multi>(path, newShader, drawSize, newColor, newColorTwo, data);
		}

		public override string ToString()
		{
			return "Multi(initPath=" + path + ", color=" + color + ", colorTwo=" + colorTwo + ")";
		}

		public override int GetHashCode()
		{
			return Gen.HashCombineStruct(Gen.HashCombineStruct(Gen.HashCombine(0, path), color), colorTwo);
		}
	}
}
