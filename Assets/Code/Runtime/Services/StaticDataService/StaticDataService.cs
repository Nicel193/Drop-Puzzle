﻿using Code.Runtime.Configs;
using Code.Runtime.Services.LogService;
using Plugin.DocuFlow.Documentation;
using UnityEngine;

namespace Code.Runtime.Services.StaticDataService
{
    [Doc("The StaticDataService class is responsible for loading and providing static data configurations used throughout the game. It loads various configurations from resource files and exposes them through properties for easy access.")]
    public class StaticDataService : IStaticDataService
    {
        private const string CoreConfigPath = "Configs/CoreConfig";

        public ShapeConfig ShapeConfig { get; private set; }
        public ShapeScoreConfig ShapeScoreConfig { get; private set; }
        public PurchasedBackgroundsConfig PurchasedBackgroundsConfig { get; private set; }
        public AudioConfig AudioConfig { get; private set; }
        public WindowAssetsConfig WindowAssetsConfig { get; private set; }
        public GameplayConfig GameplayConfig { get; private set; }
        public AdConfig AdConfig { get; private set; }
        public AnimationConfig AnimationConfig { get; private set; }

        private readonly ILogService log;

        public StaticDataService(ILogService log)
        {
            this.log = log;
        }

        public void Initialize()
        {
            CoreConfig coreConfig = LoadResource<CoreConfig>(CoreConfigPath);

            ShapeConfig = coreConfig.shapeConfig;
            ShapeScoreConfig = coreConfig.ShapeScoreConfig;
            PurchasedBackgroundsConfig = coreConfig.PurchasedBackgroundsConfig;
            AudioConfig = coreConfig.AudioConfig;
            WindowAssetsConfig = coreConfig.WindowAssetsConfig;
            GameplayConfig = coreConfig.GameplayConfig;
            AdConfig = coreConfig.AdConfig;
            AnimationConfig = coreConfig.AnimationConfig;

            log.Log("Static data loaded");
        }

        private T LoadResource<T>(string path) where T : Object
        {
            T loadResource = Resources.Load<T>(path);

            if (loadResource == null)
            {
                log.LogError($"Failed to load with path: {path}");
            }

            return loadResource;
        }
    }
}