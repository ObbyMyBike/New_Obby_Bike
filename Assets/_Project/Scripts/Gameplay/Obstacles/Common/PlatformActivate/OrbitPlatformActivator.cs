public class OrbitPlatformActivator
{
    private readonly OrbitActivationHub orbitHub;
    private readonly MovingPlatformTriggered platform;

    public OrbitPlatformActivator(OrbitActivationHub orbitHub, MovingPlatformTriggered platform)
    {
        this.orbitHub = orbitHub;
        this.platform = platform;
    }

    public void ActivateFirstUser()
    {
        orbitHub?.AddUser();
        platform?.SetPlayerInside(true);
    }

    public void DeactivateLastUser()
    {
        orbitHub?.RemoveUser();
        platform?.SetPlayerInside(false);
    }
}