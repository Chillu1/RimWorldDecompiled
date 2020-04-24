using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class GraphicDatabaseHeadRecords
	{
		private class HeadGraphicRecord
		{
			public Gender gender;

			public CrownType crownType;

			public string graphicPath;

			private List<KeyValuePair<Color, Graphic_Multi>> graphics = new List<KeyValuePair<Color, Graphic_Multi>>();

			public HeadGraphicRecord(string graphicPath)
			{
				this.graphicPath = graphicPath;
				string[] array = Path.GetFileNameWithoutExtension(graphicPath).Split('_');
				try
				{
					crownType = ParseHelper.FromString<CrownType>(array[array.Length - 2]);
					gender = ParseHelper.FromString<Gender>(array[array.Length - 3]);
				}
				catch (Exception ex)
				{
					Log.Error("Parse error with head graphic at " + graphicPath + ": " + ex.Message);
					crownType = CrownType.Undefined;
					gender = Gender.None;
				}
			}

			public Graphic_Multi GetGraphic(Color color, bool dessicated = false)
			{
				Shader shader = (!dessicated) ? ShaderDatabase.CutoutSkin : ShaderDatabase.Cutout;
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

		private static List<HeadGraphicRecord> heads = new List<HeadGraphicRecord>();

		private static HeadGraphicRecord skull;

		private static HeadGraphicRecord stump;

		private static readonly string[] HeadsFolderPaths = new string[2]
		{
			"Things/Pawn/Humanlike/Heads/Male",
			"Things/Pawn/Humanlike/Heads/Female"
		};

		private static readonly string SkullPath = "Things/Pawn/Humanlike/Heads/None_Average_Skull";

		private static readonly string StumpPath = "Things/Pawn/Humanlike/Heads/None_Average_Stump";

		public static void Reset()
		{
			heads.Clear();
			skull = null;
			stump = null;
		}

		private static void BuildDatabaseIfNecessary()
		{
			if (heads.Count <= 0 || skull == null || stump == null)
			{
				heads.Clear();
				string[] headsFolderPaths = HeadsFolderPaths;
				foreach (string text in headsFolderPaths)
				{
					foreach (string item in GraphicDatabaseUtility.GraphicNamesInFolder(text))
					{
						heads.Add(new HeadGraphicRecord(text + "/" + item));
					}
				}
				skull = new HeadGraphicRecord(SkullPath);
				stump = new HeadGraphicRecord(StumpPath);
			}
		}

		public static Graphic_Multi GetHeadNamed(string graphicPath, Color skinColor)
		{
			BuildDatabaseIfNecessary();
			for (int i = 0; i < heads.Count; i++)
			{
				HeadGraphicRecord headGraphicRecord = heads[i];
				if (headGraphicRecord.graphicPath == graphicPath)
				{
					return headGraphicRecord.GetGraphic(skinColor);
				}
			}
			Log.Message("Tried to get pawn head at path " + graphicPath + " that was not found. Defaulting...");
			return heads.First().GetGraphic(skinColor);
		}

		public static Graphic_Multi GetSkull()
		{
			BuildDatabaseIfNecessary();
			return skull.GetGraphic(Color.white, dessicated: true);
		}

		public static Graphic_Multi GetStump(Color skinColor)
		{
			BuildDatabaseIfNecessary();
			return stump.GetGraphic(skinColor);
		}

		public static Graphic_Multi GetHeadRandom(Gender gender, Color skinColor, CrownType crownType)
		{
			BuildDatabaseIfNecessary();
			Predicate<HeadGraphicRecord> predicate = delegate(HeadGraphicRecord head)
			{
				if (head.crownType != crownType)
				{
					return false;
				}
				return (head.gender == gender) ? true : false;
			};
			int num = 0;
			do
			{
				HeadGraphicRecord headGraphicRecord = heads.RandomElement();
				if (predicate(headGraphicRecord))
				{
					return headGraphicRecord.GetGraphic(skinColor);
				}
				num++;
			}
			while (num <= 40);
			foreach (HeadGraphicRecord item in heads.InRandomOrder())
			{
				if (predicate(item))
				{
					return item.GetGraphic(skinColor);
				}
			}
			Log.Error("Failed to find head for gender=" + gender + ". Defaulting...");
			return heads.First().GetGraphic(skinColor);
		}
	}
}
