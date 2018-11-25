using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using Pact.Extensions.String;

namespace Pact
{
    public sealed class HearthstoneConfiguration
        : IHearthstoneConfiguration
    {
        void IHearthstoneConfiguration.EnableLogging()
        {
            const string SECTIONNAME_POWER = "Power";
            const string SETTINGNAME_FILEPRINTING = "FilePrinting";

            string filePath =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Blizzard\Hearthstone\log.config");

            if (!File.Exists(filePath))
            {
                var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(filePath));
                if (!directoryInfo.Exists)
                    directoryInfo.Create();

                File.WriteAllText(filePath, $"[{SECTIONNAME_POWER}]\n{SETTINGNAME_FILEPRINTING}=True");

                return;
            }

            var sectionHeaderPattern = new Regex(@"^\s*\[(?<Name>.*)\]\s*$");
            var filePrintingSettingPattern =
                new Regex(
                    $@"^\s*{SETTINGNAME_FILEPRINTING}=(?<Value>\S*)\s*$",
                    RegexOptions.IgnoreCase);
            
            int? powerSectionPosition = null;
            bool inPowerSection = false;
            bool settingFound = false;

            var lines = new List<string>(File.ReadAllLines(filePath));
            for (int linePosition = 0; linePosition < lines.Count; linePosition++)
            {
                Match match = sectionHeaderPattern.Match(lines[linePosition]);
                if (match.Success)
                {
                    inPowerSection = match.Groups["Name"].Value.Eq(SECTIONNAME_POWER);
                    if (inPowerSection)
                        powerSectionPosition = linePosition;
                }
                else if (inPowerSection)
                {
                    match = filePrintingSettingPattern.Match(lines[linePosition]);
                    if (match.Success)
                    {
                        bool.TryParse(match.Groups["Value"].Value, out bool filePrintingSettingValue);
                        if (filePrintingSettingValue)
                            return;

                        lines[linePosition] = $"{SETTINGNAME_FILEPRINTING}=True";

                        settingFound = true;

                        break;
                    }
                }
            }

            if (!settingFound)
            {
                int settingPosition;

                if (powerSectionPosition.HasValue)
                {
                    settingPosition = powerSectionPosition.Value + 1;
                }
                else
                {
                    lines.Add($"[{SECTIONNAME_POWER}]");

                    settingPosition = lines.Count;
                }

                lines.Insert(settingPosition, $"{SETTINGNAME_FILEPRINTING}=True");
            }

            File.WriteAllLines(filePath, lines);
        }
    }
}
