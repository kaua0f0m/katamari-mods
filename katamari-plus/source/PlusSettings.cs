using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class KatamariQoL
{
    sealed class Settings
    {
        public bool InfiniteTime = true;
        public bool SkipOpening = true;
        public int AutoSaveSlot = 0;
        public int AutoLoadSlot = 0;
        public int AutoSaveSeconds = 60;
        public int Music = -1;
    }
    static Settings settings = new Settings();
    static readonly int[] saveIntervals = {30,60,120,300};
    static string SettingsPath { get { return Path.Combine(root,"plus-settings.ini"); } }

    static void LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsPath)) { StoreSettings(); return; }
            foreach (string raw in File.ReadAllLines(SettingsPath))
            {
                string[] pair = raw.Split(new char[]{'='},2);
                if (pair.Length != 2) continue;
                int n;
                if (!int.TryParse(pair[1].Trim(),out n)) continue;
                switch(pair[0].Trim())
                {
                    case "InfiniteTime": settings.InfiniteTime=n!=0;break;
                    case "SkipOpening": settings.SkipOpening=n!=0;break;
                    case "AutoSaveSlot": settings.AutoSaveSlot=Mathf.Clamp(n,-1,2);break;
                    case "AutoLoadSlot": settings.AutoLoadSlot=Mathf.Clamp(n,-1,2);break;
                    case "AutoSaveSeconds": if(Array.IndexOf(saveIntervals,n)>=0) settings.AutoSaveSeconds=n;break;
                    case "Music": settings.Music=Mathf.Clamp(n,-1,22);break;
                }
            }
        }
        catch(Exception ex) { Log("Settings load: "+ex.Message); }
    }

    static void StoreSettings()
    {
        try
        {
            string value = "# Katamari Plus 0.2\r\n# Slots: -1=desligado, 0=save 1, 1=save 2, 2=save 3\r\n"
                +"InfiniteTime="+(settings.InfiniteTime?1:0)+"\r\n"
                +"\r\nSkipOpening="+(settings.SkipOpening?1:0)+"\r\nAutoSaveSlot="+settings.AutoSaveSlot
                +"\r\nAutoLoadSlot="+settings.AutoLoadSlot+"\r\nAutoSaveSeconds="+settings.AutoSaveSeconds
                +"\r\nMusic="+settings.Music+"\r\n";
            string temp=SettingsPath+".tmp";
            File.WriteAllText(temp,value);
            if(File.Exists(SettingsPath)) File.Replace(temp,SettingsPath,SettingsPath+".previous");
            else File.Move(temp,SettingsPath);
        }
        catch(Exception ex) { Log("Settings save: "+ex.Message); }
    }
}
