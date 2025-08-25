using System.Collections;
using UnityEngine;
using Zenject;

public class DeathEffect
{
    private readonly ObjectPool<ParticleSystem> _pool;
    private readonly ICoroutineRunner _runner;

    [Inject]
    public DeathEffect(ParticleSystem dieEffectPrefab, int initialPoolSize, Transform parent, ICoroutineRunner runner)
    {
        _runner = runner;
        _pool = new ObjectPool<ParticleSystem>(dieEffectPrefab, initialPoolSize, parent);
    }

    public void PlayDieEffect(Vector3 position)
    {
        ParticleSystem effect = _pool.Get();
        
        var main = effect.main;
        main.useUnscaledTime = true;
        main.stopAction = ParticleSystemStopAction.None;
        
        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        effect.transform.SetPositionAndRotation(position, Quaternion.identity);
        effect.Play();

        float lifetime = main.duration + main.startLifetime.constantMax;
        
        _runner.StartCoroutine(ReleaseAfter(effect, lifetime));
    }

    private IEnumerator ReleaseAfter(ParticleSystem effect, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        
        if (effect == null)
            yield break;
        
        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _pool.Release(effect);
    }
}