﻿using System.Collections;
using Code.Runtime.Configs;
using Code.Runtime.Infrastructure;
using Code.Runtime.Infrastructure.ObjectPool;
using UnityEngine;

namespace Code.Runtime.Logic.Factories
{
    public class ParticlesFactory : IParticlesFactory
    {
        private ICoroutineRunner _coroutineRunner;

        private IGameObjectPool<ParticleSystem> _tapVfxPool;
        private IGameObjectPool<ParticleSystem> _deathVfxPool;

        public ParticlesFactory(ParticleSystem tapVfx, ParticleSystem deathVfx,
            ICoroutineRunner coroutineRunner, IGameObjectsPoolContainer gameObjectsPoolContainer)
        {
            _coroutineRunner = coroutineRunner;

            _tapVfxPool = new ComponentPool<ParticleSystem>(tapVfx,
                ObjectPoolStaticData.PreloadParticlesCount, gameObjectsPoolContainer);
            _deathVfxPool = new ComponentPool<ParticleSystem>(deathVfx,
                ObjectPoolStaticData.PreloadParticlesCount, gameObjectsPoolContainer);

            _tapVfxPool.Initialize();
            _deathVfxPool.Initialize();
        }

        public ParticleSystem CreateTapVfx(Vector3 at)
        {
            ParticleSystem tapVfx = _tapVfxPool.Get(at);
            tapVfx.Play();

            _coroutineRunner.StartCoroutine(RemoveParticle(tapVfx, _tapVfxPool));
            return tapVfx;
        }

        public ParticleSystem CreateDeathVfx(Vector3 at)
        {
            ParticleSystem deathVfx = _deathVfxPool.Get(at);
            deathVfx.Play();

            _coroutineRunner.StartCoroutine(RemoveParticle(deathVfx, _deathVfxPool));
            return deathVfx;
        }

        private IEnumerator RemoveParticle(ParticleSystem ps, IGameObjectPool<ParticleSystem> pool)
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f);

                if (ps == null) break;

                if (!ps.IsAlive(true))
                {
                    pool.Return(ps);
                    break;
                }
            }
        }
    }
}