﻿using System;
using System.IO;
using Newtonsoft.Json;

namespace FutureState.Flow.Core
{
    public class ProcessResultRepository<T> : IProcessResultRepository<T> where T : ProcessResult
    {
        /// <summary>
        ///     Create a new results repository.
        /// </summary>
        public ProcessResultRepository(string workingFolder)
        {
            WorkingFolder = workingFolder;
        }

        /// <summary>
        ///     Gets or sets the working folder to persist temporary files to.
        /// </summary>
        public string WorkingFolder { get; }

        // keep a log of the entities which errored out or were processed
        public void Save(T data)
        {
            if (!Directory.Exists(WorkingFolder))
                try
                {
                    Directory.CreateDirectory(WorkingFolder);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Can't create working folder {WorkingFolder}.", ex);
                }

            var i = 1;
            var fileName =
                $@"{WorkingFolder}\{data.ProcessName}-{data.BatchProcess.ProcessId}-{data.BatchProcess.BatchId}.json";
            while (File.Exists(fileName))
                fileName =
                    $@"{WorkingFolder}\{data.ProcessName}-{data.BatchProcess.ProcessId}-{
                            data.BatchProcess.BatchId
                        }-{i++}.json";

            var body = JsonConvert.SerializeObject(data, new JsonSerializerSettings());

            File.WriteAllText(fileName, body);
        }

        public T Get(string processName, Guid correlationId, long batchId)
        {
            var fileName = $@"{WorkingFolder}\{processName}-{correlationId}-{batchId}.json";

            if (File.Exists(fileName))
            {
                var body = File.ReadAllText(fileName);

                return JsonConvert.DeserializeObject<T>(body);
            }

            return default(T);
        }
    }
}