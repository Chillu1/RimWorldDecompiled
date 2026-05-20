using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class BodyDef : Def
{
	public BodyPartRecord corePart;

	[Unsaved(false)]
	private List<BodyPartRecord> cachedAllParts = new List<BodyPartRecord>();

	[Unsaved(false)]
	private List<BodyPartRecord> cachedPartsVulnerableToFrostbite;

	[Unsaved(false)]
	public Dictionary<BodyPartTagDef, List<BodyPartRecord>> cachedPartsByTag = new Dictionary<BodyPartTagDef, List<BodyPartRecord>>();

	[Unsaved(false)]
	public Dictionary<BodyPartDef, List<BodyPartRecord>> cachedPartsByDef = new Dictionary<BodyPartDef, List<BodyPartRecord>>();

	public List<BodyPartRecord> AllParts => cachedAllParts;

	public List<BodyPartRecord> AllPartsVulnerableToFrostbite => cachedPartsVulnerableToFrostbite;

	public List<BodyPartRecord> GetPartsWithTag(BodyPartTagDef tag)
	{
		if (!cachedPartsByTag.ContainsKey(tag))
		{
			cachedPartsByTag[tag] = new List<BodyPartRecord>();
			for (int i = 0; i < AllParts.Count; i++)
			{
				BodyPartRecord bodyPartRecord = AllParts[i];
				if (bodyPartRecord.def.tags.Contains(tag))
				{
					cachedPartsByTag[tag].Add(bodyPartRecord);
				}
			}
		}
		return cachedPartsByTag[tag];
	}

	public List<BodyPartRecord> GetPartsWithDef(BodyPartDef def)
	{
		if (!cachedPartsByDef.ContainsKey(def))
		{
			cachedPartsByDef[def] = new List<BodyPartRecord>();
			for (int i = 0; i < AllParts.Count; i++)
			{
				BodyPartRecord bodyPartRecord = AllParts[i];
				if (bodyPartRecord.def == def)
				{
					cachedPartsByDef[def].Add(bodyPartRecord);
				}
			}
		}
		return cachedPartsByDef[def];
	}

	public bool HasPartWithTag(BodyPartTagDef tag)
	{
		for (int i = 0; i < AllParts.Count; i++)
		{
			if (AllParts[i].def.tags.Contains(tag))
			{
				return true;
			}
		}
		return false;
	}

	public BodyPartRecord GetPartAtIndex(int index)
	{
		if (index < 0 || index >= cachedAllParts.Count)
		{
			return null;
		}
		return cachedAllParts[index];
	}

	public int GetIndexOfPart(BodyPartRecord rec)
	{
		for (int i = 0; i < cachedAllParts.Count; i++)
		{
			if (cachedAllParts[i] == rec)
			{
				return i;
			}
		}
		return -1;
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (cachedPartsVulnerableToFrostbite.NullOrEmpty())
		{
			yield return "no parts vulnerable to frostbite";
		}
		foreach (BodyPartRecord allPart in AllParts)
		{
			if (allPart.def.conceptual && allPart.coverageAbs != 0f)
			{
				yield return $"part {allPart} is tagged conceptual, but has nonzero coverage";
			}
			else
			{
				if (!Prefs.DevMode || allPart.def.conceptual)
				{
					continue;
				}
				float num = 0f;
				foreach (BodyPartRecord part in allPart.parts)
				{
					num += part.coverage;
				}
				if (num >= 1f)
				{
					Log.Warning("BodyDef " + defName + " has BodyPartRecord of " + allPart.def.defName + " whose children have more or equal coverage than 100% (" + (num * 100f).ToString("0.00") + "%)");
				}
			}
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		if (corePart != null)
		{
			CacheDataRecursive(corePart);
		}
		cachedPartsVulnerableToFrostbite = new List<BodyPartRecord>();
		List<BodyPartRecord> allParts = AllParts;
		for (int i = 0; i < allParts.Count; i++)
		{
			if (allParts[i].def.frostbiteVulnerability > 0f)
			{
				cachedPartsVulnerableToFrostbite.Add(allParts[i]);
			}
		}
	}

	private void CacheDataRecursive(BodyPartRecord node)
	{
		if (node.def == null)
		{
			Log.Error("BodyPartRecord with null def. body=" + this);
			return;
		}
		node.body = this;
		for (int i = 0; i < node.parts.Count; i++)
		{
			node.parts[i].parent = node;
		}
		if (node.parent != null)
		{
			node.coverageAbsWithChildren = node.parent.coverageAbsWithChildren * node.coverage;
		}
		else
		{
			node.coverageAbsWithChildren = 1f;
		}
		float num = 1f;
		for (int j = 0; j < node.parts.Count; j++)
		{
			num -= node.parts[j].coverage;
		}
		if (Mathf.Abs(num) < 1E-05f)
		{
			num = 0f;
		}
		if (num <= 0f)
		{
			num = 0f;
		}
		node.coverageAbs = node.coverageAbsWithChildren * num;
		if (node.height == BodyPartHeight.Undefined)
		{
			node.height = BodyPartHeight.Middle;
		}
		if (node.depth == BodyPartDepth.Undefined)
		{
			node.depth = BodyPartDepth.Outside;
		}
		for (int k = 0; k < node.parts.Count; k++)
		{
			if (node.parts[k].height == BodyPartHeight.Undefined)
			{
				node.parts[k].height = node.height;
			}
			if (node.parts[k].depth == BodyPartDepth.Undefined)
			{
				node.parts[k].depth = node.depth;
			}
		}
		cachedAllParts.Add(node);
		for (int l = 0; l < node.parts.Count; l++)
		{
			CacheDataRecursive(node.parts[l]);
		}
	}

	public override void ClearCachedData()
	{
		base.ClearCachedData();
		cachedPartsByTag.Clear();
		cachedPartsByDef.Clear();
	}
}
