using System;
using System.Runtime.InteropServices;
using Unity.Jobs;

namespace Gilzoide.ManagedJobs;

public struct ManagedJobParallelFor : IJobParallelFor, IDisposable
{
	private GCHandle _managedJobGcHandle;

	public IJobParallelFor Job
	{
		get
		{
			if (!_managedJobGcHandle.IsAllocated)
			{
				return null;
			}
			return (IJobParallelFor)_managedJobGcHandle.Target;
		}
	}

	public bool HasJob => Job != null;

	public ManagedJobParallelFor(IJobParallelFor managedJob)
	{
		_managedJobGcHandle = ((managedJob != null) ? GCHandle.Alloc(managedJob) : default(GCHandle));
	}

	public void Execute(int index)
	{
		Job?.Execute(index);
	}

	public void Dispose()
	{
		if (_managedJobGcHandle.IsAllocated)
		{
			_managedJobGcHandle.Free();
		}
	}

	public JobHandle Schedule(int arrayLength, int innerloopBatchCount, JobHandle dependsOn = default(JobHandle))
	{
		JobHandle jobHandle = IJobParallelForExtensions.Schedule(this, arrayLength, innerloopBatchCount, dependsOn);
		new DisposeJob<ManagedJobParallelFor>(this).Schedule(jobHandle);
		return jobHandle;
	}

	public void Run(int arrayLength)
	{
		try
		{
			IJobParallelForExtensions.Run(this, arrayLength);
		}
		finally
		{
			Dispose();
		}
	}
}
