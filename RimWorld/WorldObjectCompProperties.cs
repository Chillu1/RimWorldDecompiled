using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class WorldObjectCompProperties
{
	[TranslationHandle]
	public Type compClass = typeof(WorldObjectComp);

	public virtual IEnumerable<string> ConfigErrors(WorldObjectDef parentDef)
	{
		if (compClass == null)
		{
			yield return parentDef.defName + " has WorldObjectCompProperties with null compClass.";
		}
	}

	public virtual void ResolveReferences(WorldObjectDef parentDef)
	{
	}
}
