using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LabUtilities
{
    public class Logging
    {
        public static Logging Instance { get; } = new Logging();

        private ILoggerFactory _factory = new NullLoggerFactory();

        public ILoggerFactory Factory
        {
            get => _factory;
            set => _factory = Args.NotNull(value, "Factory");
        }

        private Logging()
        {
        }

        public ILogger GetLogger(string name)
        {
            return Factory.CreateLogger(name);
        }

        public ILogger GetLogger(Type type)
        {
            return Factory.CreateLogger(type);
        }

        public ILogger<T> GetLogger<T>()
        {
            return Factory.CreateLogger<T>();
        }

        public static ILogger<T> Logger<T>()
        {
            return Instance.GetLogger<T>();
        }

        public static ILogger Logger(Type type)
        {
            return Instance.GetLogger(type);
        }

        public static ILogger Logger(string name)
        {
            return Instance.GetLogger(name);
        }
    }
}
