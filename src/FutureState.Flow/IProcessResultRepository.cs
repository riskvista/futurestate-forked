﻿using System;

namespace FutureState.Flow
{
    public interface IProcessResultRepository<T> where T : ProcessResult
    {
        T Get(string processName, Guid correlationId, long batchId);
        void Save(T data);
    }
}