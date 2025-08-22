public class RidingPlayer : IUpdatable
{
    private readonly BikeMovement bike;
    private readonly IInput input;

    public RidingPlayer(BikeMovement bike, IInput input)
    {
        this.bike = bike;
        this.input = input;
    }

    void IUpdatable.Tick()
    {
        bike.UpdateInput(input);
    }
}