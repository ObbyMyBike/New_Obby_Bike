using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class GameplayInstaller : MonoInstaller
{
    private const int COUNT_NAMES = 100;
    private const int SEED_NAMES = 1337;
    
    [SerializeField] private Player _playerPrefab;
    [SerializeField] private PlayerConfig _playerConfig; 
    [SerializeField] private MobileInput _mobileInput;
    
    [FormerlySerializedAs("_progressBar")]
    [Header("UI Elements")]
    [SerializeField] private ProgressBarView _progressBarView;
    [SerializeField] private BoostTimerUI _boostTimerUI;

    [Header("Internet Checker")]
    [SerializeField] private float _internetCheckInterval = 60f;

    [Header("Effects Settings")]
    [SerializeField] private ParticleSystem _dieEffectPrefab;
    [SerializeField] private Transform _effectsParent;
    [SerializeField] private int _dieEffectPoolSize = 10;
    
    [Header("Message and name bots settings")]
    [SerializeField] private NameplateView _nameplatePrefab;
    [SerializeField] private PushPopupView _pushPopupPrefab;
    [SerializeField] private Vector3 _nameplateOffset = new Vector3(0, 2.2f, 0);
    [SerializeField] private Vector3 _pushPopupOffset = new Vector3(0f, 2.8f, 0f);

    public override void InstallBindings()
    {
        BindInstance(CreateRunner());
        BindInstance(_playerConfig);
        BindInstance(new CurrencyService(_playerConfig.StartCountGold));
        BindSingleton<SkinSaver>();
        BindInstance(new NameGenerator(COUNT_NAMES, SEED_NAMES));
        BindConstruct<NameplateService>(_nameplatePrefab, _nameplateOffset);
        BindSingleton<NameAssigner>();
        BindConstruct<PushPopup>(_pushPopupPrefab, _pushPopupOffset);
        BindSingleton<PushFeedbackUI>();
        BindSelfFromHierarchy<UIInfo>();
        BindSelfFromHierarchy<CameraControl>();
        BindSelfFromHierarchy<Camera>();
        BindInterfacesFromHierarchy<GameManager>();
        BindInterfacesFromHierarchy<UIController>();
        BindSelfFromHierarchy<Restart>();
        BindSelfFromHierarchy<CheckPoints>();
        BindPrefab(_playerPrefab, bindInterfaces: true);
        BindSelfFromHierarchy<PlayerSkin>();
        
        if (Application.isMobilePlatform)
            BindMobileInput();
        else
            BindSingleton<DesktopInput>();
        
        BindInstance(_progressBarView);
        BindInstance(_boostTimerUI);
        BindDeathEffect(_dieEffectPrefab, _effectsParent, _dieEffectPoolSize);
        BindInternetChecker(_internetCheckInterval);
    }

    private void BindConstruct<T>(params object[] args) where T : class
    {
        Container.BindInterfacesAndSelfTo<T>().AsSingle().WithArguments(args).NonLazy();
    }
    
    private T BindInstance<T>(T instance) where T : class
    {
        Container.BindInterfacesAndSelfTo<T>().FromInstance(instance).AsSingle();
        return instance;
    }

    private void BindSelfFromHierarchy<T>() where T : Component
    {
        Container.Bind<T>().FromComponentInHierarchy().AsSingle().NonLazy();
    }

    private void BindInterfacesFromHierarchy<T>() where T : Component
    {
        Container.BindInterfacesAndSelfTo<T>().FromComponentInHierarchy().AsSingle().NonLazy();
    }

    private void BindPrefab<T>(T prefab, bool bindInterfaces = false) where T : Component
    {
        if (bindInterfaces)
            Container.BindInterfacesAndSelfTo<T>().FromComponentInNewPrefab(prefab).AsSingle().NonLazy();
        else
            Container.Bind<T>().FromComponentInNewPrefab(prefab).AsSingle().NonLazy();
    }

    private void BindSingleton<T>() where T : class
    {
        Container.BindInterfacesAndSelfTo<T>().AsSingle().NonLazy();
    }

    private void BindMobileInput()
    {
        Container.BindInterfacesAndSelfTo<MobileInput>().FromComponentInHierarchy().AsSingle().OnInstantiated<MobileInput>((_, mi) => mi.Activate()).NonLazy();
    }

    private void BindDeathEffect(ParticleSystem fxPrefab, Transform parent, int poolSize)
    {
        Container.Bind<ParticleSystem>().FromInstance(fxPrefab).AsSingle().WhenInjectedInto<DeathEffect>();
        Container.Bind<Transform>().FromInstance(parent).AsSingle().WhenInjectedInto<DeathEffect>();
        Container.Bind<int>().FromInstance(poolSize).AsSingle().WhenInjectedInto<DeathEffect>();
        Container.Bind<DeathEffect>().AsSingle().NonLazy();
    }

    private void BindInternetChecker(float interval)
    {
        Container.Bind<float>().FromInstance(interval).AsSingle().WhenInjectedInto<InternetConnectionChecker>();
        Container.BindInterfacesAndSelfTo<InternetConnectionChecker>().AsSingle().NonLazy();
    }
    
    private ICoroutineRunner CreateRunner()
    {
        GameObject coroutineObject = new GameObject("CoroutineRunner");
        
        DontDestroyOnLoad(coroutineObject);
        
        return coroutineObject.AddComponent<CoroutineRunnerMonoBehaviour>();
    }
}