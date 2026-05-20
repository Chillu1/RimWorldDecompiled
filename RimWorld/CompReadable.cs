using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompReadable : ThingComp
{
	protected List<ReadingOutcomeDoer> doers;

	public IEnumerable<ReadingOutcomeDoer> Doers => doers;

	public CompProperties_Readable Props => (CompProperties_Readable)props;

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		InitializeDoers();
	}

	public override void PostPostMake()
	{
		base.PostPostMake();
		InitializeDoers();
		if (doers != null)
		{
			for (int i = 0; i < doers.Count; i++)
			{
				doers[i].PostMake();
			}
		}
	}

	public bool TryGetDoer<T>(out T doer) where T : ReadingOutcomeDoer
	{
		if (doers == null)
		{
			doer = null;
			return false;
		}
		for (int i = 0; i < doers.Count; i++)
		{
			if (doers[i] is T val)
			{
				doer = val;
				return true;
			}
		}
		doer = null;
		return false;
	}

	public T GetDoer<T>() where T : ReadingOutcomeDoer
	{
		if (doers == null)
		{
			return null;
		}
		for (int i = 0; i < doers.Count; i++)
		{
			if (doers[i] is T result)
			{
				return result;
			}
		}
		return null;
	}

	public IEnumerable<T> GetDoers<T>() where T : ReadingOutcomeDoer
	{
		if (doers == null)
		{
			yield break;
		}
		for (int i = 0; i < doers.Count; i++)
		{
			if (doers[i] is T val)
			{
				yield return val;
			}
		}
	}

	private void InitializeDoers()
	{
		if (Props.doers.Empty())
		{
			return;
		}
		doers = new List<ReadingOutcomeDoer>();
		for (int i = 0; i < Props.doers.Count; i++)
		{
			ReadingOutcomeDoer readingOutcomeDoer = null;
			ReadingOutcomeProperties readingOutcomeProperties = Props.doers[i];
			try
			{
				readingOutcomeDoer = (ReadingOutcomeDoer)Activator.CreateInstance(readingOutcomeProperties.DoerClass);
				doers.Add(readingOutcomeDoer);
				readingOutcomeDoer.Initialize(parent, readingOutcomeProperties);
			}
			catch (Exception arg)
			{
				Log.Error($"Could not instantiate or initialize reading outcome doer ({readingOutcomeProperties.DoerClass}): {arg}");
				doers.Remove(readingOutcomeDoer);
			}
		}
	}

	public override void PostExposeData()
	{
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			InitializeDoers();
		}
		if (doers != null)
		{
			for (int i = 0; i < doers.Count; i++)
			{
				doers[i].PostExposeData();
			}
		}
	}
}
