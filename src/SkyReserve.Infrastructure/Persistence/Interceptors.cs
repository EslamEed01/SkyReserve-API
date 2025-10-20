using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyReserve.Infrastructure.Persistence
{
    public class QueryTimingInterceptor : DbCommandInterceptor
    {
        private readonly ILogger<QueryTimingInterceptor>? _logger;

        public QueryTimingInterceptor()
        {
        }

        public QueryTimingInterceptor(ILogger<QueryTimingInterceptor> logger)
        {
            _logger = logger;
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            command.Connection!.StateChange += (sender, args) => { };

            var stopwatch = Stopwatch.StartNew();
            eventData.Context?.ChangeTracker.Entries().ToList();

            var message = $" Executing Query: {command.CommandText.Substring(0, Math.Min(100, command.CommandText.Length))}...";

            if (_logger != null)
                _logger.LogInformation(message);
            else
                Console.WriteLine(message);

            if (command.Parameters.Count == 0 || !command.Parameters.Contains("__Stopwatch__"))
            {
                var param = command.CreateParameter();
                param.ParameterName = "__Stopwatch__";
                param.Value = stopwatch;
                command.Parameters.Add(param);
            }

            return base.ReaderExecuting(command, eventData, result);
        }

        public override DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
        {
            LogExecutionTime(command, eventData);
            return base.ReaderExecuted(command, eventData, result);
        }

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            LogExecutionTime(command, eventData);
            return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        private void LogExecutionTime(DbCommand command, CommandExecutedEventData eventData)
        {
            var stopwatchParam = command.Parameters.Cast<DbParameter>()
                .FirstOrDefault(p => p.ParameterName == "__Stopwatch__");

            var executionTime = eventData.Duration.TotalMilliseconds;

            var message = $"Query completed in {executionTime:F2} ms";

            var performanceLevel = executionTime switch
            {
                < 10 => " FAST",
                < 50 => "MODERATE",
                < 200 => " SLOW",
                _ => " VERY SLOW"
            };

            message += $" [{performanceLevel}]";

            if (executionTime > 100)
            {
                message += $"\nFull Query: {command.CommandText}";
            }

            if (_logger != null)
            {
                if (executionTime > 200)
                    _logger.LogWarning(message);
                else if (executionTime > 50)
                    _logger.LogInformation(message);
                else
                    _logger.LogDebug(message);
            }
            else
            {
                Console.WriteLine(message);
            }

            if (stopwatchParam != null)
            {
                command.Parameters.Remove(stopwatchParam);
            }
        }
    }

    public class QueryInterceptor : DbCommandInterceptor
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            _stopwatch.Restart();
            Console.WriteLine($"Executing SQL: {command.CommandText}");
            return base.ReaderExecuting(command, eventData, result);
        }

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            _stopwatch.Stop();
            Console.WriteLine($"SQL executed in {_stopwatch.Elapsed.TotalMilliseconds} ms");
            return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }
    }

}
