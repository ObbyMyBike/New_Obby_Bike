using System;
using System.Collections.Generic;

public class NameGenerator
{
    private const int COUNT_NAMES = 100;
    private const int SEED_NAMES = 1337;
    
    private readonly Queue<string> _pool;

    public NameGenerator(int count = COUNT_NAMES, int seed = SEED_NAMES)
    {
        string[] baseNames = new[]
        {
            "Alex","Max","Leo","Mia","Zoe","Kai","Nova","Ivy","Niko","Luna",
            "Ari","Jax","Rex","Zara","Theo","Elle","Skye","Lia","Nina","Ira",
            "Vera","Owen","Noa","Ray","Ash","Finn","Liam","Eli","Ava","Bea",
            "Cole","Drew","Gus","Jude","Kira","Maks","Maya","Noel","Pax","Quin",
            "Rey","Sage","Tess","Una","Vale","Wes","Yara","Ziv","Ren","Ana",
            "Mika","Tara","Nora","Rina","Sora","Tori","Rina","Mori","Sena","Aiko",
            "Hana","Kato","Riko","Sumi","Yuki","Akira","Kenji","Hiro","Emi","Sora2",
            "Rio","Asha","Mila","Dina","Zina","Tala","Nika","Zoya","Aria","Lada",
            "Vlad","Olya","Sefa","Rafa","Ilya","Dara","Alya","Glen","Kane","Rei",
            "Echo","Nash","Nova2","Lark","Lux","Nyx","Rook","Wren","Zed","Kato2"
        };

        Random random = new Random(seed);
        List<string> listNames = new List<string>(baseNames);
        
        for (int i = listNames.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            
            (listNames[i], listNames[j]) = (listNames[j], listNames[i]);
        }
        
        if (listNames.Count > count)
            listNames.RemoveRange(count, listNames.Count - count);

        _pool = new Queue<string>(listNames);
    }

    public string GetNext()
    {
        if (_pool.Count == 0)
            return "Guest";
        
        return _pool.Dequeue();
    }
}