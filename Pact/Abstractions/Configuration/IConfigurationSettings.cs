﻿using System.Windows;

namespace Pact
{
    public interface IConfigurationSettings
    {
        int CardTextOffset { get; }

        int FontSize { get; }

        bool HasLoaded { get; }

        string PowerLogFilePath { get; }

        Point? TrackerWindowLocation { get; }

        Size? TrackerWindowSize { get; }
    }
}