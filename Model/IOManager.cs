using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Windows;

namespace ImageCompare.Model
{
    public class IOManager
    {
        public static void AddOrUpdateSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }

                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (Exception e)
            {
                MessageBox.Show("Something went wrong while saving the settings.\n" + e.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static string ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                var result = appSettings[key] ?? "";
                return result;
            }
            catch (Exception e)
            {
                MessageBox.Show("Something went wrong while loading the settings.\n" + e.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return "";
        }

        public static void Save(string path, string item1, string item2, string delimiter)
        {
            try
            {
                File.AppendAllLines(path, new List<string> { $"{item1}{delimiter}{item2}" });
            }
            catch (Exception e)
            {
                MessageBox.Show("Something went wrong while saving the string.\n" + e.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static List<Tuple<string, string>>? Load(string path, string delimiter)
        {
            try
            {
                if (!File.Exists(path)) return null;

                var lines = File.ReadAllLines(path);
                var result = new List<Tuple<string, string>>();
                foreach (var line in lines)
                {
                    var tuple = line.Split(delimiter);
                    result.Add(Tuple.Create(tuple[0], tuple[1]));
                }
                return result;
            }
            catch (Exception e)
            {
                MessageBox.Show("Something went wrong while loading the skipped Images.\n" + e.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}