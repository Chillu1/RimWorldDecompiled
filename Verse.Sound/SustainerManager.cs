using System;
using System.Collections.Generic;

namespace Verse.Sound;

public class SustainerManager
{
	private List<Sustainer> allSustainers = new List<Sustainer>();

	private static Dictionary<SoundDef, List<Sustainer>> playingPerDef = new Dictionary<SoundDef, List<Sustainer>>();

	private static readonly Comparison<Sustainer> SortSustainersByCameraDistanceCached = (Sustainer a, Sustainer b) => a.CameraDistanceSquared.CompareTo(b.CameraDistanceSquared);

	public List<Sustainer> AllSustainers => allSustainers;

	public void RegisterSustainer(Sustainer newSustainer)
	{
		allSustainers.Add(newSustainer);
	}

	public void DeregisterSustainer(Sustainer oldSustainer)
	{
		allSustainers.Remove(oldSustainer);
	}

	public bool SustainerExists(SoundDef def)
	{
		for (int i = 0; i < allSustainers.Count; i++)
		{
			if (allSustainers[i].def == def)
			{
				return true;
			}
		}
		return false;
	}

	public void SustainerManagerUpdate()
	{
		for (int num = allSustainers.Count - 1; num >= 0; num--)
		{
			allSustainers[num].SustainerUpdate();
		}
		UpdateAllSustainerScopes();
	}

	public void UpdateAllSustainerScopes()
	{
		playingPerDef.Clear();
		for (int i = 0; i < allSustainers.Count; i++)
		{
			Sustainer sustainer = allSustainers[i];
			if (!playingPerDef.ContainsKey(sustainer.def))
			{
				List<Sustainer> list = SimplePool<List<Sustainer>>.Get();
				list.Add(sustainer);
				playingPerDef.Add(sustainer.def, list);
			}
			else
			{
				playingPerDef[sustainer.def].Add(sustainer);
			}
		}
		foreach (KeyValuePair<SoundDef, List<Sustainer>> item in playingPerDef)
		{
			SoundDef key = item.Key;
			List<Sustainer> value = item.Value;
			if (value.Count - key.maxVoices < 0)
			{
				for (int j = 0; j < value.Count; j++)
				{
					value[j].scopeFader.inScope = true;
				}
				continue;
			}
			for (int k = 0; k < value.Count; k++)
			{
				value[k].scopeFader.inScope = false;
			}
			value.Sort(SortSustainersByCameraDistanceCached);
			int num = 0;
			for (int l = 0; l < value.Count; l++)
			{
				value[l].scopeFader.inScope = true;
				num++;
				if (num >= key.maxVoices)
				{
					break;
				}
			}
			for (int m = 0; m < value.Count; m++)
			{
				if (!value[m].scopeFader.inScope)
				{
					value[m].scopeFader.inScopePercent = 0f;
				}
			}
		}
		foreach (KeyValuePair<SoundDef, List<Sustainer>> item2 in playingPerDef)
		{
			item2.Value.Clear();
			SimplePool<List<Sustainer>>.Return(item2.Value);
		}
		playingPerDef.Clear();
	}

	public void EndAllInMap(Map map)
	{
		for (int num = allSustainers.Count - 1; num >= 0; num--)
		{
			if (allSustainers[num].info.Maker.Map == map)
			{
				allSustainers[num].End();
			}
		}
	}
}
