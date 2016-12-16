namespace VspWS.Plugins
{
    public static class Constants
    {
        public static readonly string LedgerKey = "ledger";
        public static readonly string JtlFileNameFormat = "{0}-VSPT.jtl";
        public static readonly int ConcurrencyLevel = 2000;
        public static readonly string AverageLabelSuffix = "-Average";
        public static readonly string AlSysConnectionStringName = "AlSys";
        public static readonly string FalconConnectionStringName = "Falcon";

        public static class Messages
        {
            public static readonly string ExceededMaximumAverageDuration = "Average duration exceeded threshold.";
            public static readonly string IndeterminateProcessingDuration = "Processing duration could not be determined.";
            public static string RequestDurationTooLong = "Request duration exceeded threshold.";
        }
    }
}
