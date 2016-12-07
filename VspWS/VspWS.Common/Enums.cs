﻿namespace VspWS.Common
{
    public enum MessageType
    {
        primarySlow,
        secondaryNormal,
        tertiaryFast
    }

    public enum MessageSource
    {
        unknown,
        receiver,
        processor
    }

    public enum MeasurementType
    {
        TotalDuration,
        RequestDuration,
        ProcessingDuration
    }
}
