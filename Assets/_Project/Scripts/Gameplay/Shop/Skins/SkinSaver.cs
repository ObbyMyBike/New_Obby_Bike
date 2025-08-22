using System;
using System.Collections.Generic;
using UnityEngine;

public class SkinSaver
{
    private const string PurchasedKey = "PurchasedSkins";
    private const string SelectedKey = "SelectedSkin";

    private readonly HashSet<string> _purchased = new HashSet<string>();

    public SkinSaver()
    {
        string data = PlayerPrefs.GetString(PurchasedKey, "");
        
        if (!string.IsNullOrEmpty(data))
            foreach (var id in data.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                _purchased.Add(id);
    }

    public bool IsPurchased(string skinId) => _purchased.Contains(skinId);

    public string GetSelected()
    {
        string value = PlayerPrefs.GetString(SelectedKey, "");
        
        return value;
    }
    
    public void ClearAll()
    {
        _purchased.Clear();
        
        PlayerPrefs.DeleteKey(PurchasedKey);
        PlayerPrefs.DeleteKey(SelectedKey);
        PlayerPrefs.Save();
    }
    
    public void AddPurchased(string skinId)
    {
        if (_purchased.Add(skinId))
            SavePurchased();
    }
    
    public void SetSelected(string skinId)
    {
        PlayerPrefs.SetString(SelectedKey, skinId);
        PlayerPrefs.Save();
    }
    
    private void SavePurchased()
    {
        PlayerPrefs.SetString(PurchasedKey, string.Join(",", _purchased));
        PlayerPrefs.Save();
    }
}