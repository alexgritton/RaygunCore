using System;
using Microsoft.Extensions.Logging;

namespace RaygunCore.Services
{
	/// <summary>
	/// Preforms logging to Raygun via <see cref="IRaygunClient"/>.
	/// Works only with log level warning and higher.
	/// </summary>
	public class RaygunLogger : ILogger
	{
		readonly IRaygunClient _client;

		public RaygunLogger(IRaygunClient client)
		{
			_client = client ?? throw new ArgumentNullException(nameof(client));
		}

		/// <summary>
		/// Not implemeted.
		/// </summary>
		public IDisposable BeginScope<TState>(TState state) => null;

		/// <inheritdoc/>
		public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

		/// <inheritdoc/>
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (formatter == null)
				throw new ArgumentNullException(nameof(formatter));
			if (!IsEnabled(logLevel))
				return;

			var message = formatter(state, exception);
			if (!string.IsNullOrEmpty(message) || exception != null)
				_client.SendAsync(message, exception, GetSeverity(logLevel));
		}

		RaygunSeverity GetSeverity(LogLevel logLevel)
		{
			switch (logLevel)
			{
				case LogLevel.Warning:
					return RaygunSeverity.Warning;
				case LogLevel.Error:
					return RaygunSeverity.Error;
				case LogLevel.Critical:
					return RaygunSeverity.Critical;
				default:
					throw new NotSupportedException($"LogLevel {logLevel} is not supported");
			}
		}
	}
}