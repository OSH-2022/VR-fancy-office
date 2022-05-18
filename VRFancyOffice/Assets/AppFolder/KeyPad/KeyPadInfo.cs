using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPadInfo : MonoBehaviour
{
    //keycode
    const int KeyPad_UPDOT=0; //`
    const int KeyPad_1=1;
    const int KeyPad_2=2;
    const int KeyPad_3=3;
    const int KeyPad_4=4;
    const int KeyPad_5=5;
    const int KeyPad_6=6;
    const int KeyPad_7=7;
    const int KeyPad_8=8;
    const int KeyPad_9=9;
    const int KeyPad_0=10;
    const int KeyPad_MINUS=11; //-
    const int KeyPad_EQUAL=12; //=
    const int KeyPad_BACKSPACE=13; //Backspace
    const int KeyPad_TAB=14; //Tab
    const int KeyPad_Q=15;
    const int KeyPad_W=16;
    const int KeyPad_E=17;
    const int KeyPad_R=18;
    const int KeyPad_T=19;
    const int KeyPad_Y=20;
    const int KeyPad_U=21;
    const int KeyPad_I=22;
    const int KeyPad_O=23;
    const int KeyPad_P=24;
    const int KeyPad_LBRACKET=25; //[
    const int KeyPad_RBRACKET=26; //]
    const int KeyPad_INVDASH=27; //\ inversed dash
    const int KeyPad_CAPSLOCK=28;
    const int KeyPad_A=29;
    const int KeyPad_S=30;
    const int KeyPad_D=31;
    const int KeyPad_F=32;
    const int KeyPad_G=33;
    const int KeyPad_H=34;
    const int KeyPad_J=35;
    const int KeyPad_K=36;
    const int KeyPad_L=37;
    const int KeyPad_SEMICOLON=38; //;
    const int KeyPad_QUOTE=39; //'
    const int KeyPad_ENTER=40;
    const int KeyPad_LSHIFT=41;
    const int KeyPad_Z=42;
    const int KeyPad_X=43;
    const int KeyPad_C=44;
    const int KeyPad_V=45;
    const int KeyPad_B=46;
    const int KeyPad_N=47;
    const int KeyPad_M=48;
    const int KeyPad_COMMA=49; //,
    const int KeyPad_DOT=50; //.
    const int KeyPad_DASH=51; ///
    const int KeyPad_RSHIFT=52;
    const int KeyPad_LCTRL=53; //Left Ctrl
    const int KeyPad_LALT=54; //Left Alt
    const int KeyPad_SPACE=55;
    const int KeyPad_RALT=56;
    const int KeyPad_RCTRL=57;
    const int KeyPad_UP=58;
    const int KeyPad_DOWN=59;
    const int KeyPad_LEFT=60;
    const int KeyPad_RIGHT=61;
    public List<bool> KeyState;
    private int NumKeys;
    // Start is called before the first frame update
    void Start()
    {
        KeyState=GameObject.Find("GlobalScripts").GetComponent<GlobalVar>().KeyState;
        NumKeys = KeyState.Count;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void SetKeyState(int KeyCode, bool state)
    {
        if(KeyCode>=0&&KeyCode<NumKeys)
        {
            KeyState[KeyCode]=state;
        }
    }
    public void PressKey(int KeyCode)
    {
        SetKeyState(KeyCode, true);
    }
    public void ReleaseKey(int KeyCode)
    {
        SetKeyState(KeyCode, false);
    }
}
