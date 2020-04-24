using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class WorldComponentUtility
	{
		public static void WorldComponentUpdate(World world)
		{
			List<WorldComponent> components = world.components;
			for (int i = 0; i < components.Count; i++)
			{
				try
				{
					components[i].WorldComponentUpdate();
				}
				catch (Exception ex)
				{
					Log.Error(ex.ToString());
				}
			}
		}

		public static void WorldComponentTick(World world)
		{
			List<WorldComponent> components = world.components;
			for (int i = 0; i < components.Count; i++)
			{
				try
				{
					components[i].WorldComponentTick();
				}
				catch (Exception ex)
				{
					Log.Error(ex.ToString());
				}
			}
		}

		public static void FinalizeInit(World world)
		{
			List<WorldComponent> components = world.components;
			for (int i = 0; i < components.Count; i++)
			{
				try
				{
					components[i].FinalizeInit();
				}
				catch (Exception ex)
				{
					Log.Error(ex.ToString());
				}
			}
		}
	}
}
