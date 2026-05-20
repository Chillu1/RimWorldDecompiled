using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public abstract class ThingSetMaker
{
	public ThingSetMakerParams fixedParams;

	public static List<List<Thing>> thingsBeingGeneratedNow;

	static ThingSetMaker()
	{
		thingsBeingGeneratedNow = new List<List<Thing>>();
	}

	public List<Thing> Generate()
	{
		return Generate(default(ThingSetMakerParams));
	}

	public List<Thing> Generate(ThingSetMakerParams parms)
	{
		List<Thing> list = new List<Thing>();
		thingsBeingGeneratedNow.Add(list);
		try
		{
			ThingSetMakerParams parms2 = ApplyFixedParams(parms);
			Generate(parms2, list);
			PostProcess(list);
		}
		catch (Exception ex)
		{
			Log.Error("Exception while generating thing set: " + ex);
			for (int num = list.Count - 1; num >= 0; num--)
			{
				list[num].Destroy();
				list.RemoveAt(num);
			}
		}
		finally
		{
			thingsBeingGeneratedNow.Remove(list);
		}
		return list;
	}

	public bool CanGenerate(ThingSetMakerParams parms)
	{
		ThingSetMakerParams parms2 = ApplyFixedParams(parms);
		return CanGenerateSub(parms2);
	}

	protected virtual bool CanGenerateSub(ThingSetMakerParams parms)
	{
		return true;
	}

	protected abstract void Generate(ThingSetMakerParams parms, List<Thing> outThings);

	public IEnumerable<ThingDef> AllGeneratableThingsDebug()
	{
		return AllGeneratableThingsDebug(default(ThingSetMakerParams));
	}

	public IEnumerable<ThingDef> AllGeneratableThingsDebug(ThingSetMakerParams parms)
	{
		if (!CanGenerate(parms))
		{
			yield break;
		}
		ThingSetMakerParams parms2 = ApplyFixedParams(parms);
		foreach (ThingDef item in AllGeneratableThingsDebugSub(parms2).Distinct())
		{
			yield return item;
		}
	}

	public virtual float ExtraSelectionWeightFactor(ThingSetMakerParams parms)
	{
		return 1f;
	}

	protected abstract IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms);

	private void PostProcess(List<Thing> things)
	{
		if (things.RemoveAll((Thing x) => x == null) != 0)
		{
			Log.Error(GetType()?.ToString() + " generated null things.");
		}
		ChangeDeadPawnsToTheirCorpses(things);
		for (int num = things.Count - 1; num >= 0; num--)
		{
			if (things[num].Destroyed)
			{
				Log.Error(GetType()?.ToString() + " generated destroyed thing " + things[num].ToStringSafe());
				things.RemoveAt(num);
			}
			else if (things[num].stackCount <= 0)
			{
				Log.Error(GetType()?.ToString() + " generated " + things[num].ToStringSafe() + " with stackCount=" + things[num].stackCount);
				things.RemoveAt(num);
			}
		}
		Minify(things);
	}

	private void Minify(List<Thing> things)
	{
		for (int i = 0; i < things.Count; i++)
		{
			if (things[i].def.Minifiable)
			{
				int stackCount = things[i].stackCount;
				things[i].stackCount = 1;
				MinifiedThing minifiedThing = things[i].MakeMinified();
				minifiedThing.stackCount = stackCount;
				things[i] = minifiedThing;
			}
		}
	}

	private void ChangeDeadPawnsToTheirCorpses(List<Thing> things)
	{
		for (int i = 0; i < things.Count; i++)
		{
			if (things[i].ParentHolder is Corpse)
			{
				things[i] = (Corpse)things[i].ParentHolder;
			}
		}
	}

	private ThingSetMakerParams ApplyFixedParams(ThingSetMakerParams parms)
	{
		ThingSetMakerParams replaceIn = fixedParams;
		Gen.ReplaceNullFields(ref replaceIn, parms);
		return replaceIn;
	}

	public virtual void ResolveReferences()
	{
		if (fixedParams.filter != null)
		{
			fixedParams.filter.ResolveReferences();
		}
	}

	public virtual IEnumerable<string> ConfigErrors()
	{
		yield break;
	}
}
