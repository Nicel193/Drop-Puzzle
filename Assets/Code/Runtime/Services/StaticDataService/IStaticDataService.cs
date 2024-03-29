﻿using Code.Runtime.Configs;

namespace Code.Runtime.Services.StaticDataService
{
    public interface IStaticDataService
    {
        void Initialize();
        ShapeConfig ShapeConfig { get; }
        ShapeScoreConfig ShapeScoreConfig { get; }
        PurchasedBackgroundsConfig PurchasedBackgroundsConfig { get; }
        AudioConfig AudioConfig { get; }
        WindowAssetsConfig WindowAssetsConfig { get; }
        GameplayConfig GameplayConfig { get; }
        AdConfig AdConfig { get; }
        AnimationConfig AnimationConfig { get; }
    }
}