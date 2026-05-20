using System.Xml;
using UnityEngine;

namespace Verse;

public class ShaderParameter
{
	private enum Type
	{
		Float,
		Vector,
		Matrix,
		Texture
	}

	[NoTranslate]
	private string name;

	private Vector4 value;

	private Texture2D valueTex;

	private Type type;

	public void Apply(Material mat)
	{
		switch (type)
		{
		case Type.Float:
			mat.SetFloat(name, value.x);
			break;
		case Type.Vector:
			mat.SetVector(name, value);
			break;
		case Type.Texture:
			if (valueTex == null)
			{
				Log.ErrorOnce("Texture for " + name + " is not yet loaded; file may be invalid, or main thread may not have loaded it yet", 27929440);
			}
			mat.SetTexture(name, valueTex);
			break;
		case Type.Matrix:
			break;
		}
	}

	public void Apply(MaterialPropertyBlock block)
	{
		switch (type)
		{
		case Type.Float:
			block.SetFloat(name, value.x);
			break;
		case Type.Vector:
			block.SetVector(name, value);
			break;
		case Type.Texture:
			if (valueTex == null)
			{
				Log.ErrorOnce("Texture for " + name + " is not yet loaded; file may be invalid, or main thread may not have loaded it yet", 27929440);
			}
			block.SetTexture(name, valueTex);
			break;
		case Type.Matrix:
			break;
		}
	}

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		if (xmlRoot.ChildNodes.Count != 1)
		{
			Log.Error("Misconfigured ShaderParameter: " + xmlRoot.OuterXml);
			return;
		}
		name = xmlRoot.Name;
		string valstr = xmlRoot.FirstChild.Value;
		if (!valstr.NullOrEmpty() && valstr[0] == '(')
		{
			value = ParseHelper.FromStringVector4Adaptive(valstr);
			type = Type.Vector;
		}
		else if (!valstr.NullOrEmpty() && valstr[0] == '/')
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				valueTex = ContentFinder<Texture2D>.Get(valstr.TrimStart('/'));
			});
			type = Type.Texture;
		}
		else
		{
			value = Vector4.one * ParseHelper.FromString<float>(valstr);
			type = Type.Float;
		}
	}
}
