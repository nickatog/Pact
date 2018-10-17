using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

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
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                File.WriteAllText(filePath, $"[{SECTIONNAME_POWER}]\n{SETTINGNAME_FILEPRINTING}=True");

                return;
            }

            var sectionHeaderRegex = new Regex(@"^\s*\[(?<Name>.*)\]\s*$");
            var filePrintingSettingRegex =
                new Regex(
                    $@"^\s*{SETTINGNAME_FILEPRINTING}=(?<Value>\S*)\s*$",
                    RegexOptions.IgnoreCase);
            
            int? powerSectionPosition = null;
            bool inPowerSection = false;
            bool settingFound = false;

            var lines = new List<string>(File.ReadAllLines(filePath));
            for (int linePosition = 0; linePosition < lines.Count; linePosition++)
            {
                Match match = sectionHeaderRegex.Match(lines[linePosition]);
                if (match.Success)
                {
                    inPowerSection = string.Equals(match.Groups["Name"].Value, SECTIONNAME_POWER, StringComparison.OrdinalIgnoreCase);
                    if (inPowerSection)
                        powerSectionPosition = linePosition;
                }
                else if (inPowerSection)
                {
                    match = filePrintingSettingRegex.Match(lines[linePosition]);
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
