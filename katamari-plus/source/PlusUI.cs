using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MyGame;
using MyGame.InputStatus;
using DefineEnum;

public sealed partial class KatamariQoL
{
    static QualityManager plusQuality;
    static UguiUtility qualityUtility;
    static bool plusMode, plusFromPause, plusPending;
    static PauseMenu pauseOwner, guideOwner;
    static Text pauseEntry, pauseTip, plusTip, pauseContinue, pauseHome;
    static KeyImageCheck pauseIcon;
    static GameObject[] actionButtons=new GameObject[2];
    static UguiFocus originalAction6,originalAction7;
    static Text videoEntry, plusHeading;
    static float acceptInputAfter;
    static float pauseAcceptAfter;
    static Dictionary<Text,string> savedTexts=new Dictionary<Text,string>();
    static List<Behaviour> suspendedLocalizers=new List<Behaviour>();
    struct Layout { public Vector2 Position,Size; public bool Active; }
    static Dictionary<RectTransform,Layout> savedLayout=new Dictionary<RectTransform,Layout>();
    static Dictionary<UguiFocus,UguiFocus[]> savedNavigation=new Dictionary<UguiFocus,UguiFocus[]>();
    // Textos por idioma: ver PlusText.cs.

    static Text FindText(Component parent,string name)
    {
        foreach(Text t in parent.GetComponentsInChildren<Text>(true))if(t.name==name)return t;
        return null;
    }

    static Text NewText(Text style,Transform parent,string name)
    {
        GameObject go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));
        go.layer=style.gameObject.layer;
        go.transform.SetParent(parent,false);
        Text t=go.GetComponent<Text>();
        t.font=style.font;t.fontStyle=style.fontStyle;t.fontSize=style.fontSize;
        t.color=style.color;t.material=style.material;t.lineSpacing=style.lineSpacing;
        t.alignment=TextAnchor.MiddleLeft;t.raycastTarget=false;
        t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;
        foreach(Shadow effect in style.GetComponents<Shadow>())
        {
            Shadow copy=(Shadow)go.AddComponent(effect.GetType());
            copy.effectColor=effect.effectColor;copy.effectDistance=effect.effectDistance;copy.useGraphicAlpha=effect.useGraphicAlpha;
        }
        return t;
    }

    static void AtBottom(Text text,Transform canvas,float height,float y)
    {
        RectTransform rt=text.rectTransform;
        rt.anchorMin=rt.anchorMax=new Vector2(.5f,0f);rt.pivot=new Vector2(.5f,0f);
        rt.anchoredPosition=new Vector2(0f,y);rt.sizeDelta=new Vector2(1600f,height);
        text.alignment=TextAnchor.MiddleCenter;
    }

    static void EnsurePauseGuide(PauseMenu pause,GlobalWork w)
    {
        if(!Playable(w) || w.u8Pause==0)
        {if(guideOwner==pause && pauseTip!=null)pauseTip.gameObject.SetActive(false);return;}
        GameObject guide=Get<GameObject>(pause,"objGuidePause");
        if(guideOwner!=pause || pauseEntry==null)
        {
            guideOwner=pause;
            Text back=FindText(guide.transform,"Text_Back");
            Text home=FindText(guide.transform,"Text_PrinceStar");
            if(back==null || home==null)return;
            pauseContinue=back;pauseHome=home;
            // Extend the game's own bottom command strip. The guide root is only 100x100;
            // anchor the explanatory tooltip to its full-size Canvas, not to that root.
            Canvas nativeCanvas=guide.GetComponentInParent<Canvas>();
            if(nativeCanvas==null)return;
            Transform canvas=nativeCanvas.transform;
            RectTransform strip=guide.GetComponent<RectTransform>();
            strip.anchorMin=Vector2.zero;strip.anchorMax=Vector2.one;strip.offsetMin=strip.offsetMax=Vector2.zero;
            foreach(string name in new string[]{"_go_windowNormal","_go_windowConstellation","_go_windowKumaUsi"})
            {GameObject panelObject=Get<GameObject>(pause,name);if(panelObject!=null){RectTransform panel=panelObject.GetComponent<RectTransform>();if(panel!=null)panel.anchoredPosition+=new Vector2(0,100f);}}
            foreach(Text item in new Text[]{back,home})
            {item.rectTransform.anchorMin=item.rectTransform.anchorMax=new Vector2(.5f,0f);item.rectTransform.pivot=new Vector2(.5f,0f);item.rectTransform.sizeDelta=new Vector2(300f,56f);}
            back.rectTransform.anchoredPosition=new Vector2(-550f,55f);
            home.rectTransform.anchoredPosition=new Vector2(0f,55f);
            pauseEntry=NewText(back,guide.transform,"KatamariPlusEntry");
            RectTransform rt=pauseEntry.rectTransform;
            rt.anchorMin=back.rectTransform.anchorMin;rt.anchorMax=back.rectTransform.anchorMax;rt.pivot=back.rectTransform.pivot;
            rt.anchoredPosition=new Vector2(550f,55f);rt.sizeDelta=new Vector2(300f,56f);
            pauseEntry.text="Katamari Plus";pauseEntry.fontSize=32;pauseEntry.raycastTarget=true;
            Button button=pauseEntry.gameObject.AddComponent<Button>();
            button.transition=Selectable.Transition.None;button.onClick.AddListener(delegate{OpenPlusFromPause(pause);});
            KeyImageCheck[] nativeIcons=back.GetComponentsInChildren<KeyImageCheck>(true);
            if(nativeIcons.Length>0)
            {
                GameObject icon=UnityEngine.Object.Instantiate(nativeIcons[0].gameObject,pauseEntry.transform,false);
                icon.name="PlusViewIcon";icon.SetActive(true);
                pauseIcon=icon.GetComponent<KeyImageCheck>();pauseIcon.KeyMap=KeyMap.Back;
                Set(pauseIcon,"visibleType",KeyImageCheck.eVisibleType.All);Set(pauseIcon,"changeIconType",true);
                pauseIcon.GetComponent<Image>().enabled=true;
                // Keep the native sprite rectangle, scale and aspect ratio.
            }
            pauseTip=NewText(back,canvas,"PlusPauseTooltip");
            pauseTip.fontSize=22;AtBottom(pauseTip,canvas,42f,5f);
            pauseTip.resizeTextForBestFit=true;pauseTip.resizeTextMinSize=14;pauseTip.resizeTextMaxSize=22;
            Log("Pause native command strip extended");
        }
        bool visible=w.u8Pause!=0 && w.u8State==GAMEINFO_STATE.GAMEINFO_STATE_PLAY && !pause.isActiveDialogReturn && !plusPending && !plusMode;
        pauseEntry.gameObject.SetActive(visible);pauseTip.gameObject.SetActive(visible);
        if(visible)
        {
            pauseTip.text=T.PauseTip;
            // Nos demais idiomas, a faixa da pausa mantém os textos nativos do jogo.
            if(IsPortuguese){pauseContinue.text=T.Continue;pauseHome.text=T.Home;}
        }

    }

    static void OpenPlusFromPause(PauseMenu pause)
    {
        if(plusPending || plusMode || pause==null || work.u8Pause==0 || pause.isActiveDialogReturn || Time.unscaledTime<pauseAcceptAfter)return;
        pauseOwner=pause;plusFromPause=true;plusPending=true;
        Active(pause,"objUIPause",false);Active(pause,"objGuidePause",false);
        if(pauseTip!=null)pauseTip.gameObject.SetActive(false);
        instance.StartCoroutine(LoadPlus());
    }

    static IEnumerator LoadPlus()
    {
        Log("Opening native Quality scene for Plus from pause");
        yield return SceneManager.LoadSceneAsync("Quality",LoadSceneMode.Additive);
        yield return null;
        if(plusQuality==null)
        {
            plusPending=false;if(plusFromOptions)RestoreOptions();else RestorePause();Log("Plus menu did not initialize");
        }
    }

    public static void QualityStart(QualityManager manager)
    {
        try
        {
            plusQuality=manager;
            qualityUtility=Get<UguiUtility>(manager,"mainUguiUtility");
            Log("Native Quality ready");
            if(plusPending && (plusFromPause||plusFromOptions))
            {
                // Both native settings canvases are screen-space overlays. Its generic
                // scene camera is unnecessary here and renders the level from a wrong view.
                foreach(GameObject rootObject in manager.gameObject.scene.GetRootGameObjects())
                    foreach(Camera camera in rootObject.GetComponentsInChildren<Camera>(true))camera.enabled=false;
                if(plusFromPause)
                    foreach(RawImage image in qualityUtility.GetComponentsInChildren<RawImage>(true))
                        if(image.name=="RawImage")image.enabled=false;
                BeginPlus(manager);return;
            }
        }
        catch(Exception ex){plusPending=false;Log("Quality setup: "+ex);}
    }

    static void AddVideoEntry()
    {
        UguiFocus last=null,first=null;
        foreach(UguiFocus focus in qualityUtility.GetComponentsInChildren<UguiFocus>(true))
        {
            if(focus.FocusID==7)last=focus;
            if(focus.FocusID==0)first=focus;
        }
        if(last==null || first==null)throw new InvalidOperationException("Native quality rows missing");
        Text style=last.GetComponent<Text>();
        videoEntry=NewText(style,last.transform.parent,"KatamariPlusVideoEntry");
        RectTransform rt=videoEntry.rectTransform,src=style.rectTransform;
        rt.anchorMin=src.anchorMin;rt.anchorMax=src.anchorMax;rt.pivot=src.pivot;
        rt.sizeDelta=new Vector2(900f,src.sizeDelta.y);rt.anchoredPosition=src.anchoredPosition+new Vector2(0,-60f);
        videoEntry.text="Katamari Plus";videoEntry.raycastTarget=true;
        Button button=videoEntry.gameObject.AddComponent<Button>();button.transition=Selectable.Transition.None;
        button.onClick.AddListener(delegate{BeginPlus(plusQuality);});
        UguiFocusButton next=videoEntry.gameObject.AddComponent<UguiFocusButton>();
        Field(typeof(UguiFocus),"focusID").SetValue(next,8);
        next.CursorOffset=last.CursorOffset;next.UpKeyMove=last;next.DownKeyMove=first;
        last.DownKeyMove=next;first.UpKeyMove=next;
        Get<Dictionary<int,UguiFocus>>(qualityUtility,"listUiFocus").Add(next.gameObject.GetHashCode(),next);
        Log("Plus entry added to native video configuration navigation");
    }

    static void BeginPlus(QualityManager manager)
    {
        if(plusMode || manager==null)return;
        plusQuality=manager;plusMode=true;plusPending=false;
        savedTexts.Clear();suspendedLocalizers.Clear();savedLayout.Clear();savedNavigation.Clear();
        foreach(RectTransform rt in qualityUtility.GetComponentsInChildren<RectTransform>(true))
            savedLayout[rt]=new Layout{Position=rt.anchoredPosition,Size=rt.sizeDelta,Active=rt.gameObject.activeSelf};
        UguiFocus[] rows=new UguiFocus[8];
        foreach(UguiFocus focus in qualityUtility.GetComponentsInChildren<UguiFocus>(true))
        {
            savedNavigation[focus]=new UguiFocus[]{focus.UpKeyMove,focus.DownKeyMove};
            if(focus.FocusID>=0&&focus.FocusID<8)rows[focus.FocusID]=focus;
        }
        for(int i=0;i<8;i++)
        {
            if(rows[i]!=null){rows[i].UpKeyMove=rows[(i+7)%8];rows[i].DownKeyMove=rows[(i+1)%8];}
            foreach(RectTransform rt in qualityUtility.GetComponentsInChildren<RectTransform>(true))
                if(rt.name=="TextItem"+i || rt.name=="TextItemExp"+i || rt.name=="Button"+i+"L" || rt.name=="Button"+i+"R")
                {
                    rt.gameObject.SetActive(true);
                    rt.anchoredPosition=new Vector2(rt.anchoredPosition.x,323f-i*80f);
                    Text value=rt.GetComponent<Text>();
                    if(value!=null && rt.name=="TextItem"+i)rt.sizeDelta=new Vector2(500f,60f);
                }
        }
        foreach(Text t in qualityUtility.GetComponentsInChildren<Text>(true))savedTexts[t]=t.text;
        foreach(Behaviour behavior in qualityUtility.GetComponentsInChildren<Behaviour>(true))
            if(behavior.enabled && behavior.GetType().Name.ToLowerInvariant().Contains("localiz"))
            {suspendedLocalizers.Add(behavior);behavior.enabled=false;}
        foreach(Text t in savedTexts.Keys)
        {
            string value=t.text.ToLowerInvariant();
            if(!t.name.StartsWith("TextItem") && IsQualityHeading(value))t.text="Katamari Plus";
        }
        if(videoEntry!=null)videoEntry.gameObject.SetActive(false);
        Text style=FindText(qualityUtility,"TextItem0");
        Canvas canvas=qualityUtility.GetComponentInParent<Canvas>();
        if(canvas==null)canvas=qualityUtility.GetComponentInChildren<Canvas>(true);
        if(canvas==null)throw new InvalidOperationException("Native quality Canvas missing");
        if(plusHeading==null)
        {
            plusHeading=NewText(style,canvas.transform,"PlusHeading");
            RectTransform hr=plusHeading.rectTransform;hr.anchorMin=hr.anchorMax=new Vector2(.5f,1f);hr.pivot=new Vector2(.5f,1f);
            hr.anchoredPosition=new Vector2(0f,-45f);hr.sizeDelta=new Vector2(900f,65f);
            plusHeading.fontSize=42;plusHeading.alignment=TextAnchor.MiddleCenter;plusHeading.text="Katamari Plus";
        }
        plusHeading.gameObject.SetActive(true);
        if(plusTip==null)plusTip=NewText(style,canvas.transform,"PlusFeatureTooltip");
        plusTip.fontSize=24;plusTip.color=Color.white;AtBottom(plusTip,canvas.transform,95f,65f);plusTip.gameObject.SetActive(true);
        plusTip.resizeTextForBestFit=true;plusTip.resizeTextMinSize=16;plusTip.resizeTextMaxSize=24;
        SetupActionButtons();
        qualityUtility.SetFocus(0);Set(manager,"cursorLine",0);
        acceptInputAfter=Time.unscaledTime+.2f;
        RefreshPlus();
        Log("Plus native settings menu opened; fromPause="+plusFromPause);
    }

    static void RefreshPlus()
    {
        if(!plusMode || plusQuality==null)return;
        PlusLang t=T;
        string[] values={settings.InfiniteTime?t.On:t.Off,settings.Music<0?t.Original:MusicName(settings.Music),
            SlotLabel(settings.AutoSaveSlot),SlotLabel(settings.AutoLoadSlot),settings.SkipOpening?t.On:t.Off,string.Format(t.Seconds,settings.AutoSaveSeconds),"",""};
        for(int i=0;i<6;i++)
        {
            Text label=FindText(qualityUtility,"TextItem"+i),value=FindText(qualityUtility,"TextItemExp"+i);
            if(!label.resizeTextForBestFit){label.resizeTextMaxSize=label.fontSize;label.resizeTextMinSize=Mathf.Min(18,label.fontSize);label.resizeTextForBestFit=true;}
            label.text=t.Labels[i];value.text=values[i];value.resizeTextForBestFit=true;value.resizeTextMinSize=20;value.resizeTextMaxSize=36;
        }
        int row=Mathf.Clamp(Get<int>(plusQuality,"cursorLine"),0,7);
        plusTip.text=row==6&&!plusFromPause?t.Tips[7]:t.Tips[row];
    }

    static string SlotLabel(int slot){return slot<0?T.Off:string.Format(T.Slot,slot+1);}

    // Título nativo da tela de vídeo/gráficos em qualquer idioma do jogo (ou da tradução).
    static bool IsQualityHeading(string lower)
    {
        foreach(string key in new string[]{"graphi","grafi","gráfi","vídeo","vidéo","video","グラフィック"})
            if(lower.Contains(key))return true;
        return false;
    }

    static void SetupActionButtons()
    {
        Dictionary<int,UguiFocus> focusList=Get<Dictionary<int,UguiFocus>>(qualityUtility,"listUiFocus");
        UguiFocus[] rows=new UguiFocus[8];
        foreach(UguiFocus focus in focusList.Values)if(focus.FocusID>=0&&focus.FocusID<8)rows[focus.FocusID]=focus;
        originalAction6=rows[6];originalAction7=rows[7];
        for(int i=6;i<8;i++)
        {
            GameObject source=qualityUtility.UIObjectGet("Button"+i+"R");
            GameObject buttonObject=UnityEngine.Object.Instantiate(source,source.transform.parent,false);
            buttonObject.name="PlusAction"+i;
            RectTransform rt=buttonObject.GetComponent<RectTransform>();
            rt.anchoredPosition=new Vector2(323f,323f-i*80f);rt.sizeDelta=new Vector2(620f,65f);
            foreach(Text text in buttonObject.GetComponentsInChildren<Text>(true))text.gameObject.SetActive(false);
            Text style=FindText(qualityUtility,"TextItemExp"+i);
            Text label=NewText(style,buttonObject.transform,"PlusActionLabel");
            label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=Vector2.one;
            label.rectTransform.offsetMin=label.rectTransform.offsetMax=Vector2.zero;
            label.alignment=TextAnchor.MiddleCenter;label.text=i==6&&plusFromPause?T.Labels[6]:T.Labels[7];
            Button button=buttonObject.GetComponent<Button>();button.onClick=new Button.ButtonClickedEvent();
            bool finish=i==6&&plusFromPause;button.onClick.AddListener(delegate{ClosePlus(finish);});
            UguiFocusButton focus=buttonObject.AddComponent<UguiFocusButton>();Field(typeof(UguiFocus),"focusID").SetValue(focus,i);
            focus.CursorOffset=new Vector3(-895f,0f,0f);
            focusList.Remove(rows[i].gameObject.GetHashCode());focusList.Add(buttonObject.GetHashCode(),focus);rows[i]=focus;
            actionButtons[i-6]=buttonObject;
            foreach(string name in new string[]{"TextItem"+i,"TextItemExp"+i,"Button"+i+"L","Button"+i+"R"})qualityUtility.UIObjectGet(name).SetActive(false);
        }
        int count=plusFromPause?8:7;
        actionButtons[1].SetActive(plusFromPause);
        for(int i=0;i<count;i++){rows[i].UpKeyMove=rows[(i+count-1)%count];rows[i].DownKeyMove=rows[(i+1)%count];}
    }

    static void RestoreActionButtons()
    {
        if(qualityUtility==null)return;
        Dictionary<int,UguiFocus> list=Get<Dictionary<int,UguiFocus>>(qualityUtility,"listUiFocus");
        foreach(GameObject button in actionButtons)if(button!=null){list.Remove(button.GetHashCode());button.SetActive(false);UnityEngine.Object.Destroy(button);}
        if(originalAction6!=null)list[originalAction6.gameObject.GetHashCode()]=originalAction6;
        if(originalAction7!=null)list[originalAction7.gameObject.GetHashCode()]=originalAction7;
    }

    public static bool QualityUpdate(QualityManager manager)
    {
        if(manager!=plusQuality)return true;
        InputController input=SingletonMonoBehaviour<InputController>.Instance;
        if(!plusMode)
        {
            if(Get<int>(manager,"cursorLine")!=8)return true;
            if(input.IsChancelDown())return true;
            if(input.IsSelectDown()||input.Pad().IsDown(KeyMap.Right)||input.Pad().IsDown(KeyMap.Left))BeginPlus(manager);
            return false;
        }
        RefreshPlus();
        if(Time.unscaledTime<acceptInputAfter)return false;
        if(input.IsChancelDown()||input.Pad().IsDown(KeyMap.Back)) {ClosePlus(false);return false;}
        if(input.Pad().IsDown(KeyMap.Left))ChangePlus(-1);
        else if(input.Pad().IsDown(KeyMap.Right)||input.IsSelectDown())ChangePlus(1);
        return false;
    }

    public static bool QualityClick(QualityManager manager,GameObject obj)
    {
        if(!plusMode || manager!=plusQuality)return true;
        if(obj.name=="Image_icon"){ClosePlus(false);return false;}
        for(int i=0;i<8;i++)
            if(obj.name.StartsWith("Button"+i+"L")||obj.name.StartsWith("Button"+i+"R"))
            {qualityUtility.SetFocus(i);Set(manager,"cursorLine",i);ChangePlus(obj.name.StartsWith("Button"+i+"L")?-1:1);break;}
        return false;
    }

    static int Cycle(int current,int add,int count){return (current+add+count)%count;}
    static void ChangePlus(int direction)
    {
        int row=Mathf.Clamp(Get<int>(plusQuality,"cursorLine"),0,7);
        switch(row)
        {
            case 0:settings.InfiniteTime=!settings.InfiniteTime;break;
            case 1:settings.Music=Cycle(settings.Music+1,direction,musicNames.Length+1)-1;PreviewMusic();break;
            case 2:settings.AutoSaveSlot=Cycle(settings.AutoSaveSlot+1,direction,4)-1;nextSave=Time.realtimeSinceStartup+settings.AutoSaveSeconds;break;
            case 3:settings.AutoLoadSlot=Cycle(settings.AutoLoadSlot+1,direction,4)-1;break;
            case 4:settings.SkipOpening=!settings.SkipOpening;fastIntro=settings.SkipOpening;break;
            case 5:settings.AutoSaveSeconds=saveIntervals[Cycle(Array.IndexOf(saveIntervals,settings.AutoSaveSeconds),direction,saveIntervals.Length)];nextSave=Time.realtimeSinceStartup+settings.AutoSaveSeconds;break;
            case 6:ClosePlus(plusFromPause);return;
            case 7:ClosePlus(false);return;
        }
        StoreSettings();RefreshPlus();
        SingletonMonoBehaviour<SoundController>.Instance.Play(SoundController.eChannel.SE,SoundData.eClipGroup.CommonBank,1,false,1f,0f,1f,0UL);
        Log("Plus setting changed: "+logLabels[row]);
    }

    static void ClosePlus(bool finish)
    {
        if(!plusMode)return;
        StoreSettings();plusMode=false;
        if(plusTip!=null)plusTip.gameObject.SetActive(false);
        if(plusHeading!=null)plusHeading.gameObject.SetActive(false);
        RestoreActionButtons();
        if(plusFromPause||plusFromOptions)
        {
            qualityUtility.BlocksRaycast=false;
            plusPending=true;
            instance.StartCoroutine(UnloadPlus(finish));
        }
        else
        {
            foreach(KeyValuePair<Text,string> pair in savedTexts)if(pair.Key!=null)pair.Key.text=pair.Value;
            foreach(KeyValuePair<RectTransform,Layout> pair in savedLayout)if(pair.Key!=null)
            {pair.Key.anchoredPosition=pair.Value.Position;pair.Key.sizeDelta=pair.Value.Size;pair.Key.gameObject.SetActive(pair.Value.Active);}
            foreach(KeyValuePair<UguiFocus,UguiFocus[]> pair in savedNavigation)if(pair.Key!=null)
            {pair.Key.UpKeyMove=pair.Value[0];pair.Key.DownKeyMove=pair.Value[1];}
            foreach(Behaviour behavior in suspendedLocalizers)if(behavior!=null)behavior.enabled=true;
            if(videoEntry!=null)videoEntry.gameObject.SetActive(true);
            qualityUtility.SetFocus(8);Set(plusQuality,"cursorLine",8);
        }
        Log("Plus menu closed; finish="+finish);
    }

    static IEnumerator UnloadPlus(bool finish)
    {
        yield return SceneManager.UnloadSceneAsync("Quality");
        plusQuality=null;qualityUtility=null;
        bool paused=plusFromPause, fromOptions=plusFromOptions;
        plusFromPause=false;plusFromOptions=false;plusPending=false;
        if(paused)RestorePause();
        if(fromOptions)RestoreOptions();
        if(finish && pauseOwner!=null)
        {
            originalQuestion=Get<Text>(pauseOwner,"_text_question2").text;
            pauseOwner.isDialogSelectYes=false;pauseOwner.ShowDialog(false);
            Get<Text>(pauseOwner,"_text_question2").text=T.Confirm;
            customDialog=pauseOwner;Log("Native finish confirmation opened from Plus menu");
        }
    }

    static void RestorePause()
    {
        if(pauseOwner==null)return;
        pauseAcceptAfter=Time.unscaledTime+.3f;
        Time.timeScale=0f;pauseOwner.canvas.gameObject.SetActive(true);
        Active(pauseOwner,"objUIPause",true);Active(pauseOwner,"objUIMask",true);Active(pauseOwner,"objGuidePause",true);
        SingletonMonoBehaviour<SoundController>.Instance.Pause(SoundController.eChannel.BGM);
    }

    static FieldInfo Field(Type type,string name)
    {
        for(Type t=type;t!=null;t=t.BaseType){FieldInfo f=t.GetField(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly);if(f!=null)return f;}
        throw new MissingFieldException(type.Name,name);
    }

    static void UpdatePlus()
    {
        if(work.u8Pause!=0 || !Playable(work))CancelSimpleCharge();
        if(!saveReady && work.saveSlotIndex>=0 && work.siGame!=null && work.siGame.canLoadSaveData!=0
            && SceneManager.GetActiveScene().name=="UI_MainMenu")saveReady=true;
        if(plusMode && plusQuality==null){plusMode=false;plusPending=false;plusFromPause=false;}
    }
}
