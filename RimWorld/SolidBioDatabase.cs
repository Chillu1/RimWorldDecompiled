using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using Verse;

namespace RimWorld;

public class SolidBioDatabase
{
	public static List<PawnBio> allBios = new List<PawnBio>();

	private static readonly XmlReaderSettings Settings = new XmlReaderSettings
	{
		IgnoreComments = true,
		IgnoreWhitespace = true,
		CheckCharacters = false,
		Async = false
	};

	public static void Clear()
	{
		allBios.Clear();
		PawnNameDatabaseSolid.Clear();
	}

	public static void LoadAllBios()
	{
		TextAsset[] array = Resources.LoadAll<TextAsset>("Backstories/Solid");
		List<PawnBio> list = new List<PawnBio>();
		TextAsset[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			ParseTextAsset(array2[i], list);
		}
		foreach (PawnBio item in list)
		{
			item.name.ResolveMissingPieces();
			if (item.childhood == null || item.adulthood == null)
			{
				PawnNameDatabaseSolid.AddPlayerContentName(item.name, item.gender);
				continue;
			}
			item.ResolveReferences();
			allBios.Add(item);
		}
	}

	private static void ParseTextAsset(TextAsset textObject, List<PawnBio> outputBios)
	{
		StringReader stringReader = new StringReader(textObject.text);
		XmlReader xmlReader = XmlReader.Create(stringReader, Settings);
		Dictionary<string, PawnBio> dictionary = new Dictionary<string, PawnBio>();
		PawnBio pawnBio = null;
		bool flag = false;
		string text = null;
		string text2 = null;
		bool flag2 = false;
		string text3 = null;
		string text4 = null;
		string text5 = null;
		while (xmlReader.Read())
		{
			switch (xmlReader.NodeType)
			{
			case XmlNodeType.Element:
				if (xmlReader.Name == "PawnBio")
				{
					flag = xmlReader.GetAttribute("Abstract") == "True";
					text2 = xmlReader.GetAttribute("ParentName");
					text = xmlReader.GetAttribute("Name");
					text3 = null;
					text4 = null;
					text5 = null;
					pawnBio = ((text2 == null || !dictionary.TryGetValue(text2, out var value)) ? new PawnBio() : new PawnBio
					{
						gender = value.gender,
						childhood = value.childhood,
						adulthood = value.adulthood,
						pirateKing = value.pirateKing,
						rare = value.rare
					});
				}
				else if (strcmp(xmlReader.Name, "Name"))
				{
					flag2 = true;
				}
				else if (strcmp(xmlReader.Name, "First") || (strcmp(xmlReader.Name, "firstInt") && flag2))
				{
					text3 = xmlReader.ReadString();
				}
				else if (strcmp(xmlReader.Name, "Last") || (strcmp(xmlReader.Name, "lastInt") && flag2))
				{
					text4 = xmlReader.ReadString();
				}
				else if (strcmp(xmlReader.Name, "Nick") || (strcmp(xmlReader.Name, "nickInt") && flag2))
				{
					text5 = xmlReader.ReadString();
				}
				else if (strcmp(xmlReader.Name, "Childhood"))
				{
					string defName = xmlReader.ReadString();
					pawnBio.childhood = DefDatabase<BackstoryDef>.GetNamed(defName, errorOnFail: false);
				}
				else if (strcmp(xmlReader.Name, "Adulthood"))
				{
					string defName2 = xmlReader.ReadString();
					pawnBio.adulthood = DefDatabase<BackstoryDef>.GetNamed(defName2, errorOnFail: false);
				}
				else if (strcmp(xmlReader.Name, "Gender"))
				{
					if (Enum.TryParse<GenderPossibility>(xmlReader.ReadString(), out var result))
					{
						pawnBio.gender = result;
					}
				}
				else if (strcmp(xmlReader.Name, "PirateKing"))
				{
					pawnBio.pirateKing = bool.Parse(xmlReader.ReadString());
				}
				else if (strcmp(xmlReader.Name, "Rare"))
				{
					pawnBio.rare = bool.Parse(xmlReader.ReadString());
				}
				break;
			case XmlNodeType.EndElement:
				if (xmlReader.Name == "PawnBio")
				{
					if (text3 != null || text4 != null || text5 != null)
					{
						pawnBio.name = new NameTriple(text3, text5, text4);
					}
					if (flag && text != null)
					{
						dictionary[text] = pawnBio;
					}
					else
					{
						outputBios.Add(pawnBio);
					}
					pawnBio = null;
					flag = false;
					text = null;
					text2 = null;
				}
				else if (xmlReader.Name == "Name")
				{
					flag2 = false;
				}
				break;
			}
		}
		xmlReader.Close();
		stringReader.Close();
		static bool strcmp(string a, string b)
		{
			return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}
	}
}
