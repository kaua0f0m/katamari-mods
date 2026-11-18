using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyGame;
using MyGame.InputStatus;

public sealed partial class KatamariQoL
{
    static KatamariPauseController options;
    static UguiUtility optionUtility;
    static UguiFocus optionEntryFocus;
    static GameObject optionEntry;
    static Text optionControlTip;
    static bool plusFromOptions;
    static float optionsAcceptAfter;

    public static void OptionsStart(KatamariPauseController manager)
    {
        try
        {
            options=manager;optionUtility=Get<UguiUtility>(manager,"mainUguiUtility");
            GameObject source=optionUtility.UIObjectGet("Button4L");
            optionEntry=UnityEngine.Object.Instantiate(source,source.transform.parent,false);
            optionEntry.name="KatamariPlusOptionsButton";
            RectTransform rt=optionEntry.GetComponent<RectTransform>();rt.anchoredPosition+=new Vector2(0f,-85f);
            foreach(Behaviour b in optionEntry.GetComponentsInChildren<Behaviour>(true))
                if(b.GetType().Name.Contains("Localiz") || b is UguiFocus)b.enabled=false;
            foreach(Text t in optionEntry.GetComponentsInChildren<Text>(true))t.gameObject.SetActive(false);
            Text style=FindText(optionUtility,"TextItemExp4");
            Text label=NewText(style,optionEntry.transform,"PlusOptionsLabel");
            label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=Vector2.one;
            label.rectTransform.offsetMin=label.rectTransform.offsetMax=Vector2.zero;
            label.alignment=TextAnchor.MiddleCenter;label.text="Katamari Plus";
            Button button=optionEntry.GetComponent<Button>();button.onClick=new Button.ButtonClickedEvent();
            button.onClick.AddListener(OpenPlusFromOptions);
            UguiFocusButton focus=optionEntry.AddComponent<UguiFocusButton>();
            Field(typeof(UguiFocus),"focusID").SetValue(focus,5);
            optionEntryFocus=focus;
            Get<Dictionary<int,UguiFocus>>(optionUtility,"listUiFocus").Add(optionEntry.GetHashCode(),focus);
            Canvas canvas=Get<Canvas>(manager,"mainCanvas");
            optionControlTip=NewText(style,canvas.transform,"SimpleControlTooltip");
            optionControlTip.fontSize=24;optionControlTip.color=Color.white;AtBottom(optionControlTip,canvas.transform,70f,85f);
            optionControlTip.text=T.ControlTip;
            optionControlTip.resizeTextForBestFit=true;optionControlTip.resizeTextMinSize=16;optionControlTip.resizeTextMaxSize=24;
            optionControlTip.gameObject.SetActive(false);
            OptionsAfter(manager);
            Log("Plus button added below keyboard configuration; simple right-stick remap enabled");
        }
        catch(Exception ex){Log("Options setup: "+ex);}
    }

    static void OpenPlusFromOptions()
    {
        if(options==null || plusMode || plusPending || Time.unscaledTime<optionsAcceptAfter)return;
        plusFromOptions=true;plusFromPause=false;plusPending=true;
        Set(options,"isSetting",true);optionUtility.BlocksRaycast=false;
        Get<Canvas>(options,"mainCanvas").enabled=false;
        instance.StartCoroutine(LoadPlus());
    }

    public static bool OptionsUpdate(KatamariPauseController manager)
    {
        if(manager!=options)return true;
        if(plusFromOptions && (plusMode||plusPending))return false;
        if(Time.unscaledTime<optionsAcceptAfter)return false;
        if(Get<bool>(manager,"isSetting")||Get<bool>(manager,"isDescription")||Get<bool>(manager,"isKeyChange"))return true;
        int row=Get<int>(manager,"cursorLine");
        InputController input=SingletonMonoBehaviour<InputController>.Instance;
        if(row==5 && input.IsSelectDown()){OpenPlusFromOptions();return false;}
        return true;
    }

    public static void OptionsAfter(KatamariPauseController manager)
    {
        if(manager!=options || optionUtility==null || optionEntryFocus==null)return;
        int player=Get<int>(manager,"playerIndex");
        if(!Get<bool>(manager,"isSetting"))optionUtility.BlocksRaycast=Time.unscaledTime>=optionsAcceptAfter;
        UguiFocus first=null,last=null;
        foreach(UguiFocus focus in optionUtility.GetComponentsInChildren<UguiFocus>(true))
        {if(focus.FocusID==0)first=focus;if(focus.FocusID==(player==0?4:3))last=focus;}
        if(first!=null&&last!=null)
        {last.DownKeyMove=optionEntryFocus;first.UpKeyMove=optionEntryFocus;optionEntryFocus.UpKeyMove=last;optionEntryFocus.DownKeyMove=first;optionEntryFocus.CursorOffset=last.CursorOffset;}
        if(optionControlTip!=null)optionControlTip.gameObject.SetActive(work.moveType[0]==1 && player==0 && Get<int>(manager,"cursorLine")==1
            && !Get<bool>(manager,"isSetting") && !Get<bool>(manager,"isDescription") && !Get<bool>(manager,"isKeyChange"));
    }

    static void RestoreOptions()
    {
        if(options==null)return;
        Get<Canvas>(options,"mainCanvas").enabled=true;
        optionsAcceptAfter=Time.unscaledTime+.3f;
        Set(options,"isSetting",false);optionUtility.BlocksRaycast=false;
        optionUtility.SetFocus(5);Set(options,"cursorLine",5);OptionsAfter(options);
    }
}
