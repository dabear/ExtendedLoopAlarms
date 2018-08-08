using System;
namespace ExtendedLoopAlarms.Utils
{
    public class ConfigProduction : IExtendedLoopAlarmsConfig
    {
        public string NsHost => (Environment.GetEnvironmentVariable("NS_Host") ?? "").TrimEnd(new[] { '/' });
    }
}
