using System;

public class CurrencyService
{
    public event Action<int> GoldChanged;

    public CurrencyService(int startingGold)
    {
        CurrentGold = startingGold;
        
        GoldChanged?.Invoke(CurrentGold);
    }
    
    public int CurrentGold { get; private set; }
    
    public bool TrySpend(int amount)
    {
        if (CurrentGold < amount)
            return false;
        
        CurrentGold -= amount;
        
        GoldChanged?.Invoke(CurrentGold);
        
        return true;
    }

    public void AddGold(int amount)
    {
        CurrentGold += amount;
        
        GoldChanged?.Invoke(CurrentGold);
    }
}