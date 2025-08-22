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
        effect.transform.SetPositionAndRotation(position, Quaternion.identity);

        var main = effect.main;
        main.useUnscaledTime = true;
        
        effect.Play();

        float lifetime = main.duration + main.startLifetime.constantMax;
        
        _runner.StartCoroutine(ReleaseAfter(effect, lifetime));
    }

    private IEnumerator ReleaseAfter(ParticleSystem effect, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        
        _pool.Release(effect);
    }
}