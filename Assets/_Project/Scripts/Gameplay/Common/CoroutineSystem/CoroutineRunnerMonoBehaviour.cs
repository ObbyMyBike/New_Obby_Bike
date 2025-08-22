using System.Collections;
using UnityEngine;

public class CoroutineRunnerMonoBehaviour : MonoBehaviour, ICoroutineRunner
{
    public new Coroutine StartCoroutine(IEnumerator routine) => base.StartCoroutine(routine);
}