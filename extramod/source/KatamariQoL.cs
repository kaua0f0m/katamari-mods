using System;
using System.IO;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MyGame;
using MyGame.InputStatus;
using Steamworks;
using DefineEnum;

// Compiled against the game's Mono 2.0 libraries. No loader or external UI framework.
public sealed class KatamariQoL : MonoBehaviour
{
    static KatamariQoL instance;
    static bool fastIntro;
    static bool saveReady;
    static GlobalWork work;
    static string root;
    static string saveFolder;
    static PauseMenu customDialog;
    static string originalQuestion;
    static Text hint;
    static PauseMenu hintOwner;
    static float nextSave;
    static string previousScene;
    static bool loggedTimer;
    const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    public static void Install()
    {
        if (instance != null) return;
        root = Path.Combine(Path.GetDirectoryName(Application.dataPath), "KatamariQoL");
        Directory.CreateDirectory(root);
        work = SingletonMonoBehaviour<GlobalWork>.Instance;
        GameObject go = new GameObject("KatamariQoL");
        UnityEngine.Object.DontDestroyOnLoad(go);
        instance = go.AddComponent<KatamariQoL>();
        nextSave = Time.realtimeSinceStartup + 60f;
        Log("Mod 0.1 initialized");
    }

    static void Log(string message)
    {
        try { File.AppendAllText(Path.Combine(root, "qol.log"), DateTime.Now.ToString("s") + " " + message + Environment.NewLine); }
        catch { }
    }

    static T Get<T>(object obj, string name) { return (T)obj.GetType().GetField(name, Fields).GetValue(obj); }
    static void Set(object obj, string name, object value) { obj.GetType().GetField(name, Fields).SetValue(obj, value); }
    static void Active(PauseMenu p, string field, bool value)
    {
        GameObject go = Get<GameObject>(p, field);
        if (go != null) go.SetActive(value);
    }

    static bool ValidSlotOne()
    {
        SystemSaveData sys = new SystemSaveData();
        byte[] bytes;
        uint crc;
        return SaveManager2.ExistFile(SaveManager2.Slot.ONE)
            && SaveManager2.FileSizeCheck(SaveManager2.Slot.ONE)
            && SaveManager2.SystemLoad(ref sys)
            && uint.TryParse(sys.GetData("crcSlot1"), out crc)
            && SaveManager2.LoadBytes(out bytes, SaveManager2.Slot.ONE)
            && MyUtility.Crc32(bytes, 0, bytes.Length) == crc;
    }

    public static IEnumerator TitleStart(Title2Manager title, IEnumerator original)
    {
        while (original.MoveNext()) yield return original.Current;
        bool loaded = false;
        try
        {
            if (ValidSlotOne())
            {
                SaveManager2.SaveData data = new SaveManager2.SaveData();
                data.Init();
                if (SaveManager2.Load(ref data, SaveManager2.Slot.ONE) && data.siGame.canLoadSaveData != 0)
                {
                    work = SingletonMonoBehaviour<GlobalWork>.Instance;
                    SaveManager2.SetSaveData(ref data, work);
                    work.saveSlotIndex = 0;
                    work.InitObjMessage();
                    saveReady = true;
                    fastIntro = true;
                    loaded = true;
                    Log("Slot 1 loaded; opening movie skipped");
                }
            }
        }
        catch (Exception ex) { Log("Autoload failed; native title retained: " + ex.Message); }
        if (!loaded) yield break;
        title.StopAllCoroutines();
        SingletonMonoBehaviour<SoundController>.Instance.SetVolume(SoundController.eAudioMixerGroup.VOICE, 1f);
        SingletonMonoBehaviour<SoundController>.Instance.Stop(SoundController.eChannel.BGM);
        SceneManager.LoadScene("Title3");
    }

    public static bool FastIntro() { return fastIntro; }

    public static bool SkipMovie(UIMoviePlayer player, string filename)
    {
        if (!fastIntro || !filename.StartsWith("op_movie_katamari") || SceneManager.GetActiveScene().name != "Title3") return false;
        Set(player, "_playing", false);
        Set(player, "_playStartWait", false);
        Set(player, "_isEnd", true);
        Set(player, "_skiped", true);
        if (player.objSubtitle != null) player.objSubtitle.SetActive(false);
        return true;
    }

    static bool Playable(GlobalWork w)
    {
        return w != null && w.u8GameInfoMode == GAMEINFO_MODE.GAMEINFO_MODE_1P
            && w.u8GameMode == GI_GMODE.GI_GMODE_NORMAL && w.u8SwPlayDemo == 0
            && w.u8GameType != GI_GAMETYPE.GI_GAMETYPE_S && w.u8GameType != GI_GAMETYPE.GI_GAMETYPE_F;
    }

    public static bool TickTimer(GameManager manager)
    {
        GlobalWork w = manager.gWork;
        if (w.u8SwFreeze != 0) return false;
        w.u32GameTime++;
        bool decrease = true;
        switch (w.u8GameType)
        {
            case GI_GAMETYPE.GI_GAMETYPE_N: decrease = w.u8GameMode != GI_GMODE.GI_GMODE_TUTORIAL_B; break;
            case GI_GAMETYPE.GI_GAMETYPE_C:
            case GI_GAMETYPE.GI_GAMETYPE_E: decrease = false; break;
            case GI_GAMETYPE.GI_GAMETYPE_S:
            case GI_GAMETYPE.GI_GAMETYPE_X: return false;
        }
        if (w.u8SwPlayDemo != 0) return false;
        if (Playable(w))
        {
            if (!loggedTimer) { Log("Infinite timer active: " + w.playMission + "; remaining=" + w.s32RemainTime); loggedTimer = true; }
            decrease = false;
        }
        if (decrease && w.s32RemainTime != 0)
        {
            w.s32RemainTime--;
            if (w.s32RemainTime == 0)
                SingletonMonoBehaviour<SoundController>.Instance.Play(SoundController.eChannel.SE, SoundData.eClipGroup.GameBank, 46, false, 1f, 0f, 1f, 0UL);
        }
        return true;
    }

    public static void BeforePause(PauseMenu pause)
    {
        try
        {
            GlobalWork w = Get<GlobalWork>(pause, "gWork");
            if (!Playable(w) || w.u8State != GAMEINFO_STATE.GAMEINFO_STATE_PLAY) return;
            if (customDialog == pause)
            {
                Get<Text>(pause, "_text_question2").text = "Encerrar a fase agora?";
                return;
            }
            if (w.u8Pause == 0 || pause.isActiveDialogReturn) return;
            InputController input = Get<InputController>(pause, "input");
            if (input.Pad().IsDown(KeyMap.Back))
            {
                originalQuestion = Get<Text>(pause, "_text_question2").text;
                pause.isDialogSelectYes = false;
                pause.ShowDialog(false);
                Get<Text>(pause, "_text_question2").text = "Encerrar a fase agora?";
                customDialog = pause;
                Log("Native finish-stage confirmation opened");
            }
        }
        catch (Exception ex) { Log("Pause input: " + ex.Message); }
    }

    public static void AfterPause(PauseMenu pause)
    {
        try
        {
            GlobalWork w = Get<GlobalWork>(pause, "gWork");
            if (customDialog == pause && !pause.isActiveDialogReturn)
            {
                customDialog = null;
                Get<Text>(pause, "_text_question2").text = originalQuestion;
                if (pause.isDialogSelectYes)
                {
                    pause.Pause();
                    typeof(PauseMenu).GetMethod("sContinue", Fields).Invoke(pause, null);
                    w.manGame.sVolumeDiff_End();
                    Active(pause, "objGuidePause", false);
                    if (w.u8GameType == GI_GAMETYPE.GI_GAMETYPE_C || w.u8GameType == GI_GAMETYPE.GI_GAMETYPE_X)
                        w.u8SwGameCancel = 1;
                    else if (w.u8GameType == GI_GAMETYPE.GI_GAMETYPE_E)
                        w.su32TypeE_ClearType = GAMEINFO_CLEAR.GAMEINFO_CLEAR_SUCCESS;
                    else
                    {
                        w.s32RemainTime = 0;
                        w.u8SwReduceTime = 1;
                    }
                    Log("Finish stage confirmed: " + w.playMission + "; diameter=" + w.katamariDiameterInt[0]);
                }
                else
                {
                    Time.timeScale = 0f;
                    Active(pause, "objUIPause", true);
                    Active(pause, "objUIMask", true);
                    Active(pause, "objGuidePause", true);
                    Log("Finish stage canceled; game remains paused");
                }
            }
            bool showHint = Playable(w) && w.u8State == GAMEINFO_STATE.GAMEINFO_STATE_PLAY
                && w.u8Pause != 0 && !pause.isActiveDialogReturn;
            if (showHint && (hint == null || hintOwner != pause))
            {
                Text style = Get<Text>(pause, "_text_dialogNo");
                GameObject go = new GameObject("QOLFinishGuide", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                go.transform.SetParent(Get<GameObject>(pause, "objGuidePause").transform, false);
                hint = go.GetComponent<Text>();
                hintOwner = pause;
                hint.font = style.font;
                hint.fontStyle = style.fontStyle;
                hint.color = style.color;
                hint.alignment = TextAnchor.MiddleCenter;
                hint.fontSize = 26;
                hint.resizeTextForBestFit = true;
                hint.resizeTextMinSize = 18;
                hint.resizeTextMaxSize = 26;
                hint.raycastTarget = false;
                hint.text = "Tempo infinito   |   SELECT/View: encerrar fase";
                RectTransform rt = hint.rectTransform;
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0f, 75f);
                rt.sizeDelta = new Vector2(900f, 45f);
            }
            if (hint != null && hintOwner == pause) hint.gameObject.SetActive(showHint);
        }
        catch (Exception ex) { Log("Pause UI: " + ex.Message); }
    }

    void Update()
    {
        if (work == null) return;
        string scene = SceneManager.GetActiveScene().name;
        if (scene != previousScene)
        {
            Log("Scene " + scene);
            previousScene = scene;
            loggedTimer = false;
            if (saveReady && (scene == "UI_MainMenu" || scene == "UI_OujiStar")) nextSave = Time.realtimeSinceStartup + 5f;
        }
        if (!saveReady && Playable(work) && work.manGame != null && work.saveSlotIndex >= 0) saveReady = true;
        if (saveReady && Time.realtimeSinceStartup >= nextSave)
        {
            nextSave = Time.realtimeSinceStartup + 60f;
            SaveNow("periodic");
        }
    }

    static void SaveNow(string reason)
    {
        if (!saveReady || work == null || work.siGame == null || work.siMission == null || work.saveSlotIndex < 0) return;
        if (previousScene == "Title2" || previousScene == "Title1" || previousScene == "TutorialOpening") return;
        string backup = null;
        try
        {
            if (saveFolder == null)
                saveFolder = Application.persistentDataPath.Replace("/LocalLow/", "/Local/")
                    + "/Saved/SaveGames/" + SteamUser.GetSteamID().GetAccountID().m_AccountID + "/Save";
            string slot = Path.Combine(saveFolder, "saveDataONE");
            string system = Path.Combine(saveFolder, "systemData");
            if (File.Exists(slot) && File.Exists(system))
            {
                // Preserve the previous pair before the native writer changes CRC metadata.
                if (!ValidSlotOne()) { Log("Save skipped: existing slot 1 failed CRC validation"); return; }
                backup = Path.Combine(root, "previous-save");
                Directory.CreateDirectory(backup);
                File.Copy(slot, Path.Combine(backup, "saveDataONE"), true);
                File.Copy(system, Path.Combine(backup, "systemData"), true);
            }
            SaveManager2.Save(work, SaveManager2.Slot.ONE, false);
            if (!ValidSlotOne()) throw new IOException("Native save CRC validation failed");
            Log("Slot 1 saved and CRC verified (" + reason + ")");
        }
        catch (Exception ex)
        {
            Log("Save failed: " + ex.Message);
            if (backup != null)
            {
                try
                {
                    File.Copy(Path.Combine(backup, "saveDataONE"), Path.Combine(saveFolder, "saveDataONE"), true);
                    File.Copy(Path.Combine(backup, "systemData"), Path.Combine(saveFolder, "systemData"), true);
                    Log("Previous save pair restored");
                }
                catch (Exception recovery) { Log("Save rollback failed: " + recovery.Message); }
            }
        }
    }

    void OnApplicationQuit() { SaveNow("application quit"); }
}
