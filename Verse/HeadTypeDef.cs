using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class HeadTypeDef : Def
{
	public string graphicPath;

	public Gender gender;

	public bool narrow;

	public Vector2 hairMeshSize = new Vector2(1.5f, 1.5f);

	public Vector2 beardMeshSize = new Vector2(1.5f, 1.5f);

	public Vector3 beardOffset;

	public Vector3? eyeOffsetEastWest;

	public float beardOffsetXEast;

	public float selectionWeight = 1f;

	public bool randomChosen = true;

	public List<GeneDef> requiredGenes;

	[Unsaved(false)]
	private List<KeyValuePair<Color, Graphic_Multi>> graphics = new List<KeyValuePair<Color, Graphic_Multi>>();

	public virtual Texture2D Icon => ContentFinder<Texture2D>.Get(graphicPath + "_south");

	public Graphic_Multi GetGraphic(Pawn pawn, Color color)
	{
		Shader shader = (pawn.Drawer.renderer.StatueColor.HasValue ? ShaderDatabase.Cutout : ShaderUtility.GetSkinShader(pawn));
		for (int i = 0; i < graphics.Count; i++)
		{
			if (color.IndistinguishableFrom(graphics[i].Key) && graphics[i].Value.Shader == shader)
			{
				return graphics[i].Value;
			}
		}
		Graphic_Multi graphic_Multi = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(graphicPath, shader, Vector2.one, color);
		graphics.Add(new KeyValuePair<Color, Graphic_Multi>(color, graphic_Multi));
		return graphic_Multi;
	}
}
