using System;
using UnityEngine;
using MyGame;
using UnityEngine.SceneManagement;

public sealed partial class KatamariQoL
{
    static bool musicInternal;
    static int originalMusic=3;
    static SoundData.eClipGroup originalGroup=SoundData.eClipGroup.BGM;
    static bool originalLoop=true;
    static bool StageMusicActive()
    {
        return Playable(work) && work.manGame!=null && SceneManager.GetSceneByName("GameMain").isLoaded
            && (work.u8State==DefineEnum.GAMEINFO_STATE.GAMEINFO_STATE_PLAY || work.u8State==DefineEnum.GAMEINFO_STATE.GAMEINFO_STATE_DEMO);
    }
    static readonly string[] musicNames = {
        "Katamari Nah-Nah", "Fugue #7777", "Katamari Nah-Nah (Sepia)",
        "Skipping Beneath the Stars", "Lovely Angel", "Stardust Fanfare",
        "Katamari Stars", "Katamari of Love", "Game Over", "Overture",
        "Katamari on the Rocks", "The Moon & The Prince", "Gin & Tonic & Red Red Roses",
        "Lonely Rolling Star", "You Are Smart", "Katamari Mambo", "Cherry Tree Times",
        "Angel Gifts", "Roll Me In", "Que Sera Sera", "Tutorial 2", "Tutorial 3", "Música da demonstração"
    };
    public static int MusicIndex(SoundController.eChannel channel, SoundData.eClipGroup group, int index, bool loop)
    {
        if(channel!=SoundController.eChannel.BGM || musicInternal) return index;
        if(StageMusicActive() && group==SoundData.eClipGroup.BGM && index>=10 && index<=19)
        {
            originalGroup=group;originalMusic=index;originalLoop=loop;
            if(settings.Music>=0)return settings.Music;
        }
        return index;
    }

    static void PreviewMusic()
    {
        if(!StageMusicActive())return;
        SoundController sound=SingletonMonoBehaviour<SoundController>.Instance;
        if(sound==null)return;
        try
        {
            musicInternal=true;
            sound.Play(SoundController.eChannel.BGM,settings.Music<0?originalGroup:SoundData.eClipGroup.BGM,
                settings.Music<0?originalMusic:settings.Music,settings.Music<0?originalLoop:true,1f,0f,1f,0UL);
            Log("Jukebox: "+(settings.Music<0?"original":musicNames[settings.Music]));
        }
        finally {musicInternal=false;}
    }
}
