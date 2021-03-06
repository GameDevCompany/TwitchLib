using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace TwitchLib.Logging
{
	/// <inheritdoc />
	/// <summary>
	///   The TraceLogger sends all logging to the System.Diagnostics.TraceSource
	///   built into the .net framework.
	/// </summary>
	/// <remarks>
	///   Logging can be configured in the system.diagnostics configuration 
	///   section. 
	///   If logger doesn't find a source name with a full match it will
	///   use source names which match the namespace partially. 
	///   If no portion of the namespace matches the source named "Default" will
	///   be used.
	/// </remarks>
	public class TraceLogger : LevelFilteredLogger
	{
		private static readonly Dictionary<string, TraceSource> Cache = new Dictionary<string, TraceSource>();

		private TraceSource _traceSource;

		/// <summary>
		/// Build a new trace logger based on the named TraceSource
		/// </summary>
		/// <param name="name">The name used to locate the best TraceSource. In most cases comes from the using type's fullname.</param>
		public TraceLogger(string name)
			: base(name)
		{
			Initialize();
			Level = MapLoggerLevel(_traceSource.Switch.Level);
		}

		/// <summary>
		/// Build a new trace logger based on the named TraceSource
		/// </summary>
		/// <param name="name">The name used to locate the best TraceSource. In most cases comes from the using type's fullname.</param>
		/// <param name="level">The default logging level at which this source should write messages. In almost all cases this
		/// default value will be overridden in the config file. </param>
		public TraceLogger(string name, LoggerLevel level)
			: base(name, level)
		{
			Initialize();
			Level = MapLoggerLevel(_traceSource.Switch.Level);
		}

		/// <summary>
		/// Create a new child logger.
		/// The name of the child logger is [current-loggers-name].[passed-in-name]
		/// </summary>
		/// <param name="loggerName">The Subname of this logger.</param>
		/// <returns>The New ILogger instance.</returns> 
		public override ILogger CreateChildLogger(string loggerName)
		{
			return InternalCreateChildLogger(loggerName);
		}

		private ILogger InternalCreateChildLogger(string loggerName)
		{
			return new TraceLogger($"{Name}.{loggerName}", Level);
		}

		protected override void Log(LoggerLevel loggerLevel, string loggerName, string message, Exception exception)
		{
			if (exception == null)
			{
				_traceSource.TraceEvent(MapTraceEventType(loggerLevel), 0, message);
			}
			else
			{
				_traceSource.TraceData(MapTraceEventType(loggerLevel), 0, message, exception);
			}
		}

		private void Initialize()
		{
			lock (Cache)
			{
				// because TraceSource is meant to be used as a static member, and because
				// building up the configuraion inheritance is non-trivial, the instances
				// themselves are cached for so multiple TraceLogger instances will reuse
				// the named TraceSources which have been created

				if (Cache.TryGetValue(Name, out _traceSource))
				{
					return;
				}

				var defaultLevel = MapSourceLevels(Level);
				_traceSource = new TraceSource(Name, defaultLevel);

				// no further action necessary when the named source is configured
				if (IsSourceConfigured(_traceSource))
				{
					Cache.Add(Name, _traceSource);
					return;
				}

				// otherwise hunt for a shorter source that been configured            
				var foundSource = new TraceSource("Default", defaultLevel);

				var searchName = ShortenName(Name);
				while (!string.IsNullOrEmpty(searchName))
				{
					var searchSource = new TraceSource(searchName, defaultLevel);
					if (IsSourceConfigured(searchSource))
					{
						foundSource = searchSource;
						break;
					}

					searchName = ShortenName(searchName);
				}

				// reconfigure the created source to act like the found source
				_traceSource.Switch = foundSource.Switch;
				_traceSource.Listeners.Clear();
				foreach (TraceListener listener in foundSource.Listeners)
				{
					_traceSource.Listeners.Add(listener);
				}

				Cache.Add(Name, _traceSource);
			}
		}

		private static string ShortenName(string name)
		{
			var lastDot = name.LastIndexOf('.');
			return lastDot != -1 ? name.Substring(0, lastDot) : null;
		}

		private static bool IsSourceConfigured(TraceSource source)
		{
		    return source.Listeners.Count != 1 || !(source.Listeners[0] is DefaultTraceListener) || source.Listeners[0].Name != "Default";
		}

		private static LoggerLevel MapLoggerLevel(SourceLevels level)
		{
			switch (level)
			{
				case SourceLevels.All:
					return LoggerLevel.Debug;
				case SourceLevels.Verbose:
					return LoggerLevel.Debug;
				case SourceLevels.Information:
					return LoggerLevel.Info;
				case SourceLevels.Warning:
					return LoggerLevel.Warn;
				case SourceLevels.Error:
					return LoggerLevel.Error;
				case SourceLevels.Critical:
					return LoggerLevel.Fatal;
			}
			return LoggerLevel.Off;
		}

		private static SourceLevels MapSourceLevels(LoggerLevel level)
		{
			switch (level)
			{
				case LoggerLevel.Debug:
					return SourceLevels.Verbose;
				case LoggerLevel.Info:
					return SourceLevels.Information;
				case LoggerLevel.Warn:
					return SourceLevels.Warning;
				case LoggerLevel.Error:
					return SourceLevels.Error;
				case LoggerLevel.Fatal:
					return SourceLevels.Critical;
			}
			return SourceLevels.Off;
		}

		private static TraceEventType MapTraceEventType(LoggerLevel level)
		{
			switch (level)
			{
				case LoggerLevel.Debug:
					return TraceEventType.Verbose;
				case LoggerLevel.Info:
					return TraceEventType.Information;
				case LoggerLevel.Warn:
					return TraceEventType.Warning;
				case LoggerLevel.Error:
					return TraceEventType.Error;
				case LoggerLevel.Fatal:
					return TraceEventType.Critical;
			}
			return TraceEventType.Verbose;
		}
	}
}