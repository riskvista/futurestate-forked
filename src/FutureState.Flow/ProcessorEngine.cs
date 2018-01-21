﻿using System;
using System.Collections.Generic;
using System.Linq;
using FutureState.Flow.Core;
using NLog;

namespace FutureState.Flow
{
    /// <summary>
    ///     Extracts entities from a given data sources in a managed way to use in
    /// </summary>
    /// <typeparam name="TEntityDto">The type of entity to process.</typeparam>
    public class ProcessorEngine<TEntityDto> : IProcessorEngine
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IProcessResultRepository<ProcessResult> _repository;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public ProcessorEngine(
            string processorName,
            IProcessResultRepository<ProcessResult> repository = null)
        {
            Warnings = new List<string>();

            ProcessName = processorName ?? GetType().Name;

            _repository = repository ?? new ProcessResultRepository<ProcessResult>(Environment.CurrentDirectory);
        }


        /// <summary>
        ///     Gets/set the processor name.
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        ///     Initializes a new procesor.
        /// </summary>
        public Action Initialize { get; set; }

        /// <summary>
        ///     Gets the action to read results from.
        /// </summary>
        public IEnumerable<TEntityDto> EntitiesReader { get; set; }

        /// <summary>
        ///     Gets the action to process one item.
        /// </summary>
        public Func<TEntityDto, IEnumerable<ErrorEvent>> ProcessItem { get; set; }

        /// <summary>
        ///     Gets the error handler.
        /// </summary>
        public Action<TEntityDto, Exception> OnError { get; set; }

        /// <summary>
        ///     Gets the list of warnings accumulated.
        /// </summary>
        public List<string> Warnings { get; }

        /// <summary>
        ///     Action to execute when finished processing.
        /// </summary>
        public Action Commit { get; set; }

        /// <summary>
        ///     Gets the current entity or row count being procesed.
        /// </summary>
        public int Current { get; private set; }

        /// <summary>
        ///     Gets the date the process started.
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        ///     Processes all  data from the incoming source and records. Will record to file and memory the entities that were and
        ///     were not processed and returns
        ///     a summary of the processes' execution status.
        /// </summary>
        /// <returns></returns>
        public ProcessResult Process(BatchProcess process, ProcessResult<TEntityDto> result = null)
        {
            Guard.ArgumentNotNull(process, nameof(process));

            StartTime = DateTime.UtcNow;

            Current = 0;

            if (EntitiesReader == null)
                throw new InvalidOperationException("EntitiesReader has not been assigned.");

            if (result == null)
                result = new ProcessResult<TEntityDto>();

            result.BatchProcess = process;

            Initialize?.Invoke();

            var onError = OnError ?? ((_, ___) => { });

            var processed = new List<TEntityDto>();
            var errors = new List<ProcessError<TEntityDto>>();
            var exceptions = new List<Exception>();

            foreach (var dto in EntitiesReader)
            {
                Current++;

                try
                {
                    var errorsEvents = ProcessItem(dto);

                    var errorEvents = errorsEvents as ErrorEvent[] ?? errorsEvents.ToArray();
                    if (!errorEvents.Any())
                        processed.Add(dto);
                    else
                        foreach (var error in errorEvents)
                            errors.Add(new ProcessError<TEntityDto> {Error = error, Item = dto});
                }
                catch (ApplicationException apex)
                {
                    if (Logger.IsErrorEnabled)
                        Logger.Error(apex, $"Can't process row {Current} due to error: {apex.Message}");

                    onError(dto, apex);

                    exceptions.Add(apex);

                    errors.Add(new ProcessError<TEntityDto>
                    {
                        Error = new ErrorEvent {Type = "Exception", Message = apex.Message},
                        Item = dto
                    });
                }
                catch (Exception ex)
                {
                    if (Logger.IsErrorEnabled)
                        Logger.Error(ex, $"Can't process row {Current} due to an unexpected error.");

                    onError(dto, ex);

                    exceptions.Add(ex);

                    errors.Add(new ProcessError<TEntityDto>
                    {
                        Error = new ErrorEvent {Type = "Exception", Message = ex.Message},
                        Item = dto
                    });
                }
            }

            // update target
            try
            {
                Commit();
            }
            catch (Exception ex)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error(ex, "An unexpected error occurred commiting the processed result.");

                // roll back items into an error state
                foreach (var entityDto in processed)
                    errors.Add(new ProcessError<TEntityDto>
                    {
                        Error =
                            new ErrorEvent {Type = "Exception", Message = $"Failed to commit changes: {ex.Message}"},
                        Item = entityDto
                    });

                //reset items
                processed.Clear();
            }

            // added/updated
            if (Logger.IsInfoEnabled)
                Logger.Info($"Finised processing.");

            result.ProcessName = ProcessName;
            result.ProcessedCount = processed.Count;
            result.Errors = errors;
            result.Exceptions = exceptions;
            result.Warnings = Warnings;
            result.Input = processed;
            result.ProcessTime = DateTime.UtcNow - StartTime;

            // save flow
            _repository.Save(result);

            return result;
        }
    }
}