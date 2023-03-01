using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace CodeNifty.DraftYourViewers2
{
    [JsonObject(MemberSerialization.OptOut)]
    public struct DraftConfig
    {
        public string streamerAccessToken;
        public List<string> blockedUsernames;
        public bool includeDraftedInDrawings;
        public bool includeDrawnInDrawings;
        public bool addKerman;

        public DraftConfig(DraftConfig config)
        {
            streamerAccessToken = config.streamerAccessToken;
            blockedUsernames = config.blockedUsernames;
            includeDraftedInDrawings = config.includeDraftedInDrawings;
            includeDrawnInDrawings = config.includeDrawnInDrawings;
            addKerman = config.addKerman;
        }
    }

    [JsonObject(MemberSerialization.OptOut)]
    public struct DraftSave
    {
        public Dictionary<string, string> draftedUserIds;
        public List<string> drawnUserIds;
    }

    // File read/write system for DYV 2. Handles creation, saving, and loading of global config and per-campaign save data.
    class FileManager
    {
        private static char slash = Path.DirectorySeparatorChar;

        public static DraftConfig NewDraftConfig(bool save = false)
        {
            DraftConfig config = new DraftConfig()
            {
                streamerAccessToken = "",
                blockedUsernames = new List<string>(),
                addKerman = true
            };

            if (save)
            {
                SaveDraftConfig(config);
            }

            return config;
        }

        public static DraftConfig LoadDraftConfig()
        {
            string configPath = $"{Application.persistentDataPath}{slash}ModData{slash}Draft Your Viewers 2{slash}config.json";
            try
            {
                string configJson = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<DraftConfig>(configJson);
            }
            catch (DirectoryNotFoundException)
            {
                Logger.LogInfo("Config not found for this save. Starting in a blank state.");
                return NewDraftConfig(true);
            }
            catch (FileNotFoundException)
            {
                Logger.LogInfo("Config not found for this save. Starting in a blank state.");
                return NewDraftConfig(true);
            }
            catch (JsonException e)
            {
                Logger.LogWarn($"Error parsing config from JSON. Starting in a blank state.\n{e.Message}\n{e.StackTrace}");
                return NewDraftConfig();
            }
            catch (Exception e)
            {
                Logger.LogError($"Unkown error reading or parsing config. Starting in a blank state.\n{e.Message}\n{e.StackTrace}");
                return NewDraftConfig();
            }
        }

        public static void SaveDraftConfig(DraftConfig config)
        {
            string configPath = $"{Application.persistentDataPath}{slash}ModData{slash}Draft Your Viewers 2{slash}config.json";
            FileInfo file = new FileInfo(configPath);
            file.Directory.Create();
            try
            {
                string configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(configPath, configJson);
            }
            catch (JsonException e)
            {
                Logger.LogWarn($"Error stringifying config to JSON. Keeping current config for now.\n{e.Message}\n{e.StackTrace}");
            }
            catch (Exception e)
            {
                Logger.LogError($"Unkown error stringifying or writing config. Keeping current config for now.\n{e.Message}\n{e.StackTrace}");
            }
        }

        public static DraftSave NewDraftSave(string campaignPath, bool save = false)
        {
            DraftSave config = new DraftSave()
            {
                draftedUserIds = new Dictionary<string, string>(),
                drawnUserIds = new List<string>()
            };

            if (save)
            {
                SaveDraftSave(campaignPath, config);
            }

            return config;
        }

        public static DraftSave LoadDraftSave(string campaignPath)
        {
            string configPath = $"{campaignPath}{slash}ModData{slash}Draft Your Viewers 2{slash}save.json";
            try
            {
                string configJson = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<DraftSave>(configJson);
            }
            catch (DirectoryNotFoundException)
            {
                Logger.LogInfo("Config not found for this save. Starting in a blank state.");
                return NewDraftSave(campaignPath, true);
            }
            catch (FileNotFoundException)
            {
                Logger.LogInfo("Config not found for this save. Starting in a blank state.");
                return NewDraftSave(campaignPath, true);
            }
            catch (JsonException e)
            {
                Logger.LogWarn($"Error parsing config from JSON. Starting in a blank state.\n{e.Message}\n{e.StackTrace}");
                return NewDraftSave("");
            }
            catch (Exception e)
            {
                Logger.LogError($"Unkown error reading or parsing config. Starting in a blank state.\n{e.Message}\n{e.StackTrace}");
                return NewDraftSave("");
            }
        }

        public static void SaveDraftSave(string campaignPath, DraftSave config)
        {
            string configPath = $"{campaignPath}{slash}ModData{slash}Draft Your Viewers 2{slash}save.json";
            FileInfo file = new FileInfo(configPath);
            file.Directory.Create();
            try
            {
                string configJson = JsonConvert.SerializeObject(config);
                File.WriteAllText(configPath, configJson);
            }
            catch (JsonException e)
            {
                Logger.LogWarn($"Error stringifying config to JSON. Keeping current config for now.\n{e.Message}\n{e.StackTrace}");
            }
            catch (Exception e)
            {
                Logger.LogError($"Unkown error stringifying or writing config. Keeping current config for now.\n{e.Message}\n{e.StackTrace}");
            }
        }
    }
}
