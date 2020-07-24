using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public struct MaterialRequest : IEquatable<MaterialRequest>
	{
		public Shader shader;

		public Texture2D mainTex;

		public Color color;

		public Color colorTwo;

		public Texture2D maskTex;

		public int renderQueue;

		public List<ShaderParameter> shaderParameters;

		public string BaseTexPath
		{
			set
			{
				mainTex = ContentFinder<Texture2D>.Get(value);
			}
		}

		public MaterialRequest(Texture2D tex)
		{
			shader = ShaderDatabase.Cutout;
			mainTex = tex;
			color = Color.white;
			colorTwo = Color.white;
			maskTex = null;
			renderQueue = 0;
			shaderParameters = null;
		}

		public MaterialRequest(Texture2D tex, Shader shader)
		{
			this.shader = shader;
			mainTex = tex;
			color = Color.white;
			colorTwo = Color.white;
			maskTex = null;
			renderQueue = 0;
			shaderParameters = null;
		}

		public MaterialRequest(Texture2D tex, Shader shader, Color color)
		{
			this.shader = shader;
			mainTex = tex;
			this.color = color;
			colorTwo = Color.white;
			maskTex = null;
			renderQueue = 0;
			shaderParameters = null;
		}

		public override int GetHashCode()
		{
			return Gen.HashCombine(Gen.HashCombineInt(Gen.HashCombine(Gen.HashCombine(Gen.HashCombineStruct(Gen.HashCombineStruct(Gen.HashCombine(0, shader), color), colorTwo), mainTex), maskTex), renderQueue), shaderParameters);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is MaterialRequest))
			{
				return false;
			}
			return Equals((MaterialRequest)obj);
		}

		public bool Equals(MaterialRequest other)
		{
			if (other.shader == shader && other.mainTex == mainTex && other.color == color && other.colorTwo == colorTwo && other.maskTex == maskTex && other.renderQueue == renderQueue)
			{
				return other.shaderParameters == shaderParameters;
			}
			return false;
		}

		public static bool operator ==(MaterialRequest lhs, MaterialRequest rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(MaterialRequest lhs, MaterialRequest rhs)
		{
			return !(lhs == rhs);
		}

		public override string ToString()
		{
			return "MaterialRequest(" + shader.name + ", " + mainTex.name + ", " + color.ToString() + ", " + colorTwo.ToString() + ", " + maskTex.ToString() + ", " + renderQueue + ")";
		}
	}
}
