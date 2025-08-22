using System.Collections;
using UnityEngine;

public class CheckPointEffect
{
    private readonly ParticleSystem _particleSystem;

    public CheckPointEffect(ParticleSystem particleSystem)
    {
        _particleSystem = particleSystem;
    }

    public IEnumerator Play()
    {
        _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _particleSystem.Play();

        yield return new WaitWhile(() => _particleSystem.IsAlive(true));
    }
}