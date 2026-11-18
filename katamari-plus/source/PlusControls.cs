using UnityEngine;
using MyGame;
using MyGame.InputStatus;
using DefineEnum;

public sealed partial class KatamariQoL
{
    static bool dashCharging;
    static float dashStarted;
    static bool IsSimple(Player player)
    {
        return player.playerNo==0 && Playable(work) && work.u8State==GAMEINFO_STATE.GAMEINFO_STATE_PLAY
            && Get<GlobalWork.eMoveType>(player,"inputMoveType")==GlobalWork.eMoveType.Easy
            && work.u8Pause==0 && work.u8SwFreeze==0 && work.oujiViewMode[0]==0;
    }
    static bool DashGesture(Player player)
    {
        if(!IsSimple(player)){CancelSimpleCharge();return false;}
        Vector2 right=SingletonMonoBehaviour<InputController>.Instance.Pad(player.controllerNo).GetAxis(StickMap.StickRight);
        bool up=right.y < -0.7f;
        if(up && !dashCharging){dashCharging=true;dashStarted=Time.time;Log("Simple native dash gesture started");}
        if(!up && dashCharging){dashCharging=false;Log("Simple native dash gesture released");}
        return up;
    }
    static bool DashPulse(){return ((int)((Time.time-dashStarted)*12f)&1)==0;}
    public static Vector2 RemapSimpleLeft(Vector2 original,Player player)
    {
        // Holding right-up repeats the native easy-mode charging gesture. This only
        // takes over the hands during the dash; ordinary left-stick input stays intact.
        if(DashGesture(player))
        {
            bool pulse=DashPulse();
            if(!pulse)Set(player,"StickDashSpace",false);
            return new Vector2(pulse?1f:0f,0f);
        }
        return original;
    }
    public static Vector2 RemapSimpleRight(Vector2 original,Player player)
    {
        if(!IsSimple(player))return original;
        if(DashGesture(player))return new Vector2(DashPulse()?-1f:0f,0f);
        return new Vector2(0f,Mathf.Abs(original.x)<0.2f?0f:original.x);
    }
    static void CancelSimpleCharge(){dashCharging=false;}
}
