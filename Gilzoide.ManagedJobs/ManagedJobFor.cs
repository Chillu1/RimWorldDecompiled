using System;
using System.Runtime.InteropServices;
using Unity.Jobs;

namespace Gilzoide.ManagedJobs;

public struct ManagedJobFor : IJobFor, IDisposable
{
	private GCHandle _managedJobGcHandle;

	public IJobFor Job
	{
		get
		{
			if (!_managedJobGcHandle.IsAllocated)
			{
				return null;
			}
			return (IJobFor)_managedJobGcHandle.Target;
		}
	}

	public bool HasJob => Job != null;

	public ManagedJobFor(IJobFor managedJob)
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

	public JobHandle Schedule(int arrayLength, JobHandle dependsOn = default(JobHandle))
	{
		JobHandle jobHandle = IJobForExtensions.Schedule(this, arrayLength, dependsOn);
		new DisposeJob<ManagedJobFor>(this).Schedule(jobHandle);
		return jobHandle;
	}

	public JobHandle ScheduleParallel(int arrayLength, int innerloopBatchCount, JobHandle dependsOn = default(JobHandle))
	{
		JobHandle jobHandle = IJobForExtensions.ScheduleParallel(this, arrayLength, innerloopBatchCount, dependsOn);
		new DisposeJob<ManagedJobFor>(this).Schedule(jobHandle);
		return jobHandle;
	}

	public void Run(int arrayLength)
	{
		try
		{
			IJobForExtensions.Run(this, arrayLength);
		}
		finally
		{
			Dispose();
		}
	}
}
