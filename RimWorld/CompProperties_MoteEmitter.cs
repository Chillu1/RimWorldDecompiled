using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompProperties_MoteEmitter : CompProperties
{
	public ThingDef mote;

	public List<ThingDef> perRotationMotes;

	public Vector3 offset;

	public Vector3 offsetMin = Vector3.zero;

	public Vector3 offsetMax = Vector3.zero;

	public Vector3 offsetNorth = Vector3.zero;

	public Vector3 offsetSouth = Vector3.zero;

	public Vector3 offsetEast = Vector3.zero;

	public Vector3 offsetWest = Vector3.zero;

	public bool useParentRotation;

	public SoundDef soundOnEmission;

	public int emissionInterval = -1;

	public int ticksSinceLastEmittedMaxOffset;

	public bool maintain;

	[NoTranslate]
	public string saveKeysPrefix;

	public Vector3 EmissionOffset => new Vector3(Rand.Range(offsetMin.x, offsetMax.x), Rand.Range(offsetMin.y, offsetMax.y), Rand.Range(offsetMin.z, offsetMax.z));

	public CompProperties_MoteEmitter()
	{
		compClass = typeof(CompMoteEmitter);
	}

	public Vector3 RotationOffset(Rot4 rot)
	{
		return rot.AsInt switch
		{
			0 => offsetNorth, 
			1 => offsetEast, 
			2 => offsetSouth, 
			3 => offsetWest, 
			_ => Vector3.zero, 
		};
	}

	public ThingDef RotationMote(Rot4 rot)
	{
		return perRotationMotes?[rot.AsInt];
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		if (mote == null && perRotationMotes.NullOrEmpty())
		{
			yield return "CompMoteEmitter must have a mote assigned.";
		}
		if (!perRotationMotes.NullOrEmpty() && perRotationMotes.Count != 4)
		{
			yield return "perRotationMotes must contain 4 elements for North, East, South, West";
		}
	}
}
