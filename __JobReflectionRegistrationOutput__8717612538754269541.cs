using System;
using Gilzoide.ManagedJobs;
using RimWorld.Planet;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Verse;
using Verse.Glow;

[Unity.Jobs.DOTSCompilerGenerated]
internal class __JobReflectionRegistrationOutput__8717612538754269541
{
	public static void CreateJobReflectionData()
	{
		try
		{
			IJobExtensions.EarlyJobInit<ManagedJob>();
			IJobForExtensions.EarlyJobInit<ManagedJobFor>();
			IJobParallelForExtensions.EarlyJobInit<ManagedJobParallelFor>();
			IJobParallelForTransformExtensions.EarlyJobInit<ManagedJobParallelForTransform>();
			IJobParallelForExtensions.EarlyJobInit<DynamicDrawManager.CullJob>();
			IJobParallelForExtensions.EarlyJobInit<DynamicDrawManager.ComputeSilhouetteMatricesJob>();
			IJobParallelForExtensions.EarlyJobInit<GlowGrid.CombineColorsJob>();
			IJobExtensions.EarlyJobInit<PathFinderJob>();
			IJobParallelForExtensions.EarlyJobInit<PathGridJob>();
			IJobParallelForExtensions.EarlyJobInit<ComputeGlowGridsJob>();
			IJobParallelForExtensions.EarlyJobInit<FastTileFinder.ComputeQueryJob>();
			IJobExtensions.EarlyJobInit<DisposeJob<ManagedJob>>();
			IJobExtensions.EarlyJobInit<DisposeJob<ManagedJobFor>>();
			IJobExtensions.EarlyJobInit<DisposeJob<ManagedJobParallelFor>>();
			IJobExtensions.EarlyJobInit<DisposeJob<ManagedJobParallelForTransform>>();
		}
		catch (Exception ex)
		{
			EarlyInitHelpers.JobReflectionDataCreationFailed(ex);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	public static void EarlyInit()
	{
		CreateJobReflectionData();
	}
}
