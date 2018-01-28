﻿using System;
using System.IO;
using Newtonsoft.Json;
using NLog;

namespace FutureState.Flow.Data
{
    public class ProcessResultRepository<T> : IProcessResultRepository<T> where T : ProcessResult
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private string _workingFolder;

        /// <summary>
        ///     Create a new results repository.
        /// </summary>
        public ProcessResultRepository(string workingFolder = null)
        {
            WorkingFolder = workingFolder ?? Environment.CurrentDirectory;
        }

        /// <summary>
        ///     Gets or sets the working folder to persist temporary files to.
        /// </summary>
        public string WorkingFolder
        {
            get => _workingFolder;
            set
            {
                Guard.ArgumentNotNullOrEmptyOrWhiteSpace(value, nameof(WorkingFolder));

                _workingFolder = value;
            }
        }

        // keep a log of the entities which errored out or were processed
        public void Save(T data)
        {
            CreateDirIfNotExists();


            var i = 1;
            var fileName =
                $@"{WorkingFolder}\{data.ProcessName}-{data.BatchProcess.FlowId}-{data.BatchProcess.BatchId}.json";
            while (File.Exists(fileName))
                fileName =
                    $@"{WorkingFolder}\{data.ProcessName}-{data.BatchProcess.FlowId}-{
                            data.BatchProcess.BatchId
                        }-{i++}.json";

            if (_logger.IsInfoEnabled)
                _logger.Info(($"Saving process output to {fileName}."));

            var body = JsonConvert.SerializeObject(data, new JsonSerializerSettings());

            File.WriteAllText(fileName, body);

            if(_logger.IsInfoEnabled)
                _logger.Info(($"Saved process output to {fileName}."));
        }

        public T Get(string processName, Guid correlationId, long batchId)
        {
            var fileName = $@"{WorkingFolder}\{processName}-{correlationId}-{batchId}.json";

            if (!File.Exists(fileName))
                return default(T);

            if (_logger.IsInfoEnabled)
                _logger.Info(($"Reading process output from {fileName}."));

            var body = File.ReadAllText(fileName);

            var result =  JsonConvert.DeserializeObject<T>(body);

            if (_logger.IsInfoEnabled)
                _logger.Info(($"Read process output from {fileName}."));

            return result;
        }

        public T Get(string dataSource)
        {
            if (!File.Exists(dataSource))
                return default(T);

            var body = File.ReadAllText(dataSource);

            return JsonConvert.DeserializeObject<T>(body);
        }

        private void CreateDirIfNotExists()
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
        }
    }
}