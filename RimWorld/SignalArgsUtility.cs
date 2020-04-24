using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public static class SignalArgsUtility
	{
		public static bool TryGetLookTargets(SignalArgs args, string name, out LookTargets lookTargets)
		{
			if (args.TryGetArg(name, out lookTargets))
			{
				return true;
			}
			if (args.TryGetArg(name, out Thing arg))
			{
				lookTargets = arg;
				return true;
			}
			if (args.TryGetArg(name, out WorldObject arg2))
			{
				lookTargets = arg2;
				return true;
			}
			if (args.TryGetArg(name, out GlobalTargetInfo arg3))
			{
				lookTargets = arg3;
				return true;
			}
			if (args.TryGetArg(name, out TargetInfo arg4))
			{
				lookTargets = arg4;
				return true;
			}
			lookTargets = LookTargets.Invalid;
			return false;
		}
	}
}
