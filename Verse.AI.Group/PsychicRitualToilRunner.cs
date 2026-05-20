using System;

namespace Verse.AI.Group;

public static class PsychicRitualToilRunner
{
	public static void Start(PsychicRitualToil toil, PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		try
		{
			toil?.Start(psychicRitual, parent);
		}
		catch (Exception arg)
		{
			Log.Error($"PsychicRitualToil {toil.ToStringSafe()} threw an exception during Start(): {arg}");
		}
	}

	public static bool Tick(PsychicRitualToil toil, PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		try
		{
			return toil?.Tick(psychicRitual, parent) ?? true;
		}
		catch (Exception arg)
		{
			Log.Error($"PsychicRitualToil {toil.ToStringSafe()} threw an exception during Tick(): {arg}");
		}
		return true;
	}

	public static void UpdateAllDuties(PsychicRitualToil toil, PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		try
		{
			toil?.UpdateAllDuties(psychicRitual, parent);
		}
		catch (Exception arg)
		{
			Log.Error($"PsychicRitualToil {toil.ToStringSafe()} threw an exception during UpdateAllDuties(): {arg}");
		}
	}

	public static void End(PsychicRitualToil toil, PsychicRitual psychicRitual, PsychicRitualGraph parent, bool success)
	{
		try
		{
			toil?.End(psychicRitual, parent, success);
		}
		catch (Exception arg)
		{
			Log.Error($"PsychicRitualToil {toil.ToStringSafe()} threw an exception during End(): {arg}");
		}
	}
}
