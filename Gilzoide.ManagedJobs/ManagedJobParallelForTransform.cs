using System;
using System.Runtime.InteropServices;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace Gilzoide.ManagedJobs;

public struct ManagedJobParallelForTransform : IJobParallelForTransform, IDisposable
{
	private GCHandle _managedJobGcHandle;

	public IJobParallelForTransform Job
	{
		get
		{
			if (!_managedJobGcHandle.IsAllocated)
			{
				return null;
			}
			return (IJobParallelForTransform)_managedJobGcHandle.Target;
		}
	}

	public bool HasJob => Job != null;

	public ManagedJobParallelForTransform(IJobParallelForTransform managedJob)
	{
		_managedJobGcHandle = ((managedJob != null) ? GCHandle.Alloc(managedJob) : default(GCHandle));
	}

	public void Execute(int index, TransformAccess transform)
	{
		Job?.Execute(index, transform);
	}

	public void Dispose()
	{
		if (_managedJobGcHandle.IsAllocated)
		{
			_managedJobGcHandle.Free();
		}
	}

	public JobHandle Schedule(TransformAccessArray transforms, JobHandle dependsOn = default(JobHandle))
	{
		JobHandle jobHandle = IJobParallelForTransformExtensions.Schedule(this, transforms, dependsOn);
		new DisposeJob<ManagedJobParallelForTransform>(this).Schedule(jobHandle);
		return jobHandle;
	}
}
