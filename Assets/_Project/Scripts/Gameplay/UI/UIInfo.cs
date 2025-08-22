using UnityEngine;

public class UIInfo : MonoBehaviour
{
    public bool IsDown { get; private set; }

    public void Down()
    {
        IsDown = true;   
    }
    public void Up()
    {
        IsDown = false;
    }
}