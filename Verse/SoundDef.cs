using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public class SoundDef : Def
{
	[Description("If checked, this sound is a sustainer.\n\nSustainers are used for sounds with a defined beginning and end (as opposed to OneShots, which just fire at a given instant).\n\nThis value must match what the game expects from the SubSoundDef with this name.")]
	[DefaultValue(false)]
	public bool sustain;

	[Description("When the sound is allowed to play: only when the map view is active, only when the world view is active, or always (map + world + main menu).")]
	[DefaultValue(SoundContext.Any)]
	public SoundContext context;

	[Description("Event names for this sound. \n\nThe code will look up sounds to play them according to their name. If the code finds the event name it wants in this list, it will trigger this sound.\n\nThe Def name is also used as an event name. Obsolete")]
	public List<string> eventNames = new List<string>();

	[Description("For one-shots, this is the number of individual sounds from this Def than can be playing at a time.\n\n For sustainers, this is the number of sustainers that can be running with this sound (each of which can have sub-sounds). Sustainers can fade in and out as you move the camera or objects move, to keep the nearest ones audible.\n\nThis setting may not work for on-camera sounds.")]
	[DefaultValue(4)]
	public int maxVoices = 4;

	[Description("The number of instances of this sound that can play at almost exactly the same moment. Handles cases like six gunners all firing their identical guns at the same time because a target came into view of all of them at the same time. Ordinarily this would make a painfully loud sound, but you can reduce it with this.")]
	[DefaultValue(3)]
	public int maxSimultaneous = 3;

	[Description("If the system has to not play some instances of this sound because of maxVoices, this determines which ones are ignored.\n\nYou should use PrioritizeNewest for things like gunshots, so older still-playing samples are overridden by newer, more important ones.\n\nSustained sounds should usually prioritize nearest, so if a new fire starts burning nearby it can override a more distant one.")]
	[DefaultValue(VoicePriorityMode.PrioritizeNewest)]
	public VoicePriorityMode priorityMode;

	[Description("The special sound slot this sound takes. If a sound with this slot is playing, new sounds in this slot will not play.\n\nOnly works for on-camera sounds.")]
	[DefaultValue("")]
	public string slot = "";

	[LoadAlias("sustainerStartSound")]
	[Description("The name of the SoundDef that will be played when this sustainer starts.")]
	[DefaultValue("")]
	public SoundDef sustainStartSound;

	[LoadAlias("sustainerStopSound")]
	[Description("The name of the SoundDef that will be played when this sustainer ends.")]
	[DefaultValue("")]
	public SoundDef sustainStopSound;

	[Description("After a sustainer is ended, the sound will fade out over this many real-time seconds.")]
	[DefaultValue(0f)]
	public float sustainFadeoutTime;

	[LoadAlias("sustainerFadeoutStartSound")]
	[Description("The name of the SoundDef that will be played when this sustainer starts to fade out.")]
	[DefaultValue("")]
	public SoundDef sustainFadeoutStartSound;

	[Description("All the sounds that will play when this set is triggered.")]
	public List<SubSoundDef> subSounds = new List<SubSoundDef>();

	[Unsaved(false)]
	public bool isUndefined;

	[Unsaved(false)]
	public Sustainer testSustainer;

	private static Dictionary<string, SoundDef> undefinedSoundDefs = new Dictionary<string, SoundDef>();

	private static object undefinedSoundDefsLock = new object();

	private bool HasSubSoundsOnCamera
	{
		get
		{
			for (int i = 0; i < subSounds.Count; i++)
			{
				if (subSounds[i].onCamera)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool HasSubSoundsInWorld
	{
		get
		{
			for (int i = 0; i < subSounds.Count; i++)
			{
				if (!subSounds[i].onCamera)
				{
					return true;
				}
			}
			return false;
		}
	}

	public int MaxSimultaneousSamples => maxSimultaneous * subSounds.Count;

	public FloatRange Duration
	{
		get
		{
			float num = float.PositiveInfinity;
			float num2 = float.NegativeInfinity;
			foreach (SubSoundDef subSound in subSounds)
			{
				num = Mathf.Min(num, subSound.Duration.min);
				num2 = Mathf.Max(num2, subSound.Duration.max);
			}
			return new FloatRange((num == float.PositiveInfinity) ? 0f : num, (num2 == float.NegativeInfinity) ? 0f : num2);
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		for (int i = 0; i < subSounds.Count; i++)
		{
			subSounds[i].parentDef = this;
			if (subSounds[i].name == "UnnamedSubSoundDef")
			{
				subSounds[i].name = defName + "_" + i;
			}
			subSounds[i].ResolveReferences();
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		if (slot != "" && !HasSubSoundsOnCamera)
		{
			yield return "Sound slots only work for on-camera sounds.";
		}
		if (HasSubSoundsInWorld && context != SoundContext.MapOnly)
		{
			yield return "Sounds with non-on-camera subsounds should use MapOnly context.";
		}
		if (priorityMode == VoicePriorityMode.PrioritizeNewest && sustain)
		{
			yield return "PrioritizeNewest is not supported with sustainers.";
		}
		if (maxVoices < 1)
		{
			yield return "Max voices is less than 1.";
		}
		if (!sustain && (sustainStartSound != null || sustainStopSound != null))
		{
			yield return "Sustainer start and end sounds only work with sounds defined as sustainers.";
		}
		if (sustainFadeoutStartSound != null && sustainFadeoutTime <= 0f)
		{
			yield return "Sustainer fadeout sound is set, but fadeout time is not set.";
		}
		if (!sustain)
		{
			for (int i = 0; i < subSounds.Count; i++)
			{
				if (subSounds[i].startDelayRange.TrueMax > 0.001f)
				{
					yield return "startDelayRange is only supported on sustainers.";
				}
			}
		}
		if (!subSounds.NullOrEmpty())
		{
			for (int i = 0; i < subSounds.Count; i++)
			{
				SubSoundDef s = subSounds[i];
				foreach (string item in s.ConfigErrors())
				{
					yield return $"SubSound[{i}] ({s}): {item}";
				}
			}
		}
		List<SoundDef> defs = DefDatabase<SoundDef>.AllDefsListForReading;
		for (int i = 0; i < defs.Count; i++)
		{
			if (defs[i].eventNames.NullOrEmpty())
			{
				continue;
			}
			for (int j = 0; j < defs[i].eventNames.Count; j++)
			{
				if (defs[i].eventNames[j] == defName)
				{
					yield return defName + " is also defined in the eventNames list for " + defs[i];
				}
			}
		}
	}

	public void DoEditWidgets(WidgetRow widgetRow)
	{
		if (testSustainer == null)
		{
			if (!widgetRow.ButtonIcon(TexButton.Play))
			{
				return;
			}
			ResolveReferences();
			SoundInfo info;
			if (HasSubSoundsInWorld)
			{
				IntVec3 mapPosition = Find.CameraDriver.MapPosition;
				info = SoundInfo.InMap(new TargetInfo(mapPosition, Find.CurrentMap), MaintenanceType.PerFrame);
				for (int i = 0; i < 5; i++)
				{
					FleckMaker.ThrowDustPuff(mapPosition, Find.CurrentMap, 1.5f);
				}
			}
			else
			{
				info = SoundInfo.OnCamera(MaintenanceType.PerFrame);
			}
			info.testPlay = true;
			if (sustain)
			{
				testSustainer = this.TrySpawnSustainer(info);
			}
			else
			{
				this.PlayOneShot(info);
			}
		}
		else
		{
			testSustainer.Maintain();
			if (widgetRow.ButtonIcon(TexButton.Stop))
			{
				testSustainer.End();
				testSustainer = null;
			}
		}
	}

	public static SoundDef Named(string defName)
	{
		SoundDef namedSilentFail = DefDatabase<SoundDef>.GetNamedSilentFail(defName);
		if (namedSilentFail != null)
		{
			return namedSilentFail;
		}
		if (!Prefs.DevMode)
		{
			lock (undefinedSoundDefsLock)
			{
				if (undefinedSoundDefs.ContainsKey(defName))
				{
					return UndefinedDefNamed(defName);
				}
			}
		}
		List<SoundDef> allDefsListForReading = DefDatabase<SoundDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			if (allDefsListForReading[i].eventNames.Count <= 0)
			{
				continue;
			}
			for (int j = 0; j < allDefsListForReading[i].eventNames.Count; j++)
			{
				if (allDefsListForReading[i].eventNames[j] == defName)
				{
					return allDefsListForReading[i];
				}
			}
		}
		if (DefDatabase<SoundDef>.DefCount == 0)
		{
			Log.Warning("Tried to get SoundDef named " + defName + ", but sound defs aren't loaded yet (is it a static variable initialized before play data?).");
		}
		return UndefinedDefNamed(defName);
	}

	private static SoundDef UndefinedDefNamed(string defName)
	{
		SoundDef value;
		lock (undefinedSoundDefsLock)
		{
			if (!undefinedSoundDefs.TryGetValue(defName, out value))
			{
				value = new SoundDef();
				value.isUndefined = true;
				value.defName = defName;
				value.ResolveDefNameHash();
				undefinedSoundDefs.Add(defName, value);
			}
		}
		return value;
	}
}
