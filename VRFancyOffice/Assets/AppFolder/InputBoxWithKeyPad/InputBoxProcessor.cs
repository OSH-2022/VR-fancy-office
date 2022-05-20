using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class InputBoxProcessor : MonoBehaviour
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
    private List<bool> KeyState;
    public string buffer=string.Empty;
    public TMP_Text display;
    public GameObject DestroyAfterCancelled;
    public UnityEvent AfterSubmit;
    private bool[] KeyProcessed=new bool[62];
    private int NumKeys=62;
    private bool CapsLk=false;
    // Start is called before the first frame update
    void Start()
    {
        KeyState=GameObject.Find("GlobalScripts").GetComponent<GlobalVar>().KeyState;
        for(int i=0;i<NumKeys;i++) KeyProcessed[i]=false;
    }

    // Update is called once per frame
    void Update()
    {
        display.SetText(buffer);
        if(!CapsLk&&!KeyState[KeyPad_LSHIFT]&&!KeyState[KeyPad_RSHIFT])
        {
            CheckKey(KeyPad_UPDOT, "`");
            CheckKey(KeyPad_0, "0");
            CheckKey(KeyPad_1, "1");
            CheckKey(KeyPad_2, "2");
            CheckKey(KeyPad_3, "3");
            CheckKey(KeyPad_4, "4");
            CheckKey(KeyPad_5, "5");
            CheckKey(KeyPad_6, "6");
            CheckKey(KeyPad_7, "7");
            CheckKey(KeyPad_8, "8");
            CheckKey(KeyPad_9, "9");
            CheckKey(KeyPad_TAB, "\t");
            CheckKey(KeyPad_Q, "q");
            CheckKey(KeyPad_W, "w");
            CheckKey(KeyPad_E, "e");
            CheckKey(KeyPad_R, "r");
            CheckKey(KeyPad_T, "t");
            CheckKey(KeyPad_Y, "y");
            CheckKey(KeyPad_U, "u");
            CheckKey(KeyPad_I, "i");
            CheckKey(KeyPad_O, "o");
            CheckKey(KeyPad_P, "p");
            CheckKey(KeyPad_LBRACKET, "[");
            CheckKey(KeyPad_RBRACKET, "]");
            CheckKey(KeyPad_INVDASH, "\\");
            CheckKey(KeyPad_CAPSLOCK, "CapsLock");
            CheckKey(KeyPad_A , "a");
            CheckKey(KeyPad_S , "s");
            CheckKey(KeyPad_D , "d");
            CheckKey(KeyPad_F , "f");
            CheckKey(KeyPad_G , "g");
            CheckKey(KeyPad_H , "h");
            CheckKey(KeyPad_J , "j");
            CheckKey(KeyPad_K , "k");
            CheckKey(KeyPad_L , "l");
            CheckKey(KeyPad_SEMICOLON , ";");
            CheckKey(KeyPad_QUOTE , "'");
            CheckKey(KeyPad_ENTER , "\n");
            CheckKey(KeyPad_Z , "z");
            CheckKey(KeyPad_X , "x");
            CheckKey(KeyPad_C , "c");
            CheckKey(KeyPad_V , "v");
            CheckKey(KeyPad_B , "b");
            CheckKey(KeyPad_N , "n");
            CheckKey(KeyPad_M , "m");
            CheckKey(KeyPad_SPACE , " ");
            CheckKey(KeyPad_MINUS , "-");
            CheckKey(KeyPad_EQUAL , "=");
            CheckKey(KeyPad_BACKSPACE , "Backspace");
        }
        else if(CapsLk&&(KeyState[KeyPad_LSHIFT]||KeyState[KeyPad_RSHIFT]))
        {
            CheckKey(KeyPad_UPDOT, "~");
            CheckKey(KeyPad_0, ")");
            CheckKey(KeyPad_1, "!");
            CheckKey(KeyPad_2, "@");
            CheckKey(KeyPad_3, "#");
            CheckKey(KeyPad_4, "$");
            CheckKey(KeyPad_5, "%");
            CheckKey(KeyPad_6, "^");
            CheckKey(KeyPad_7, "&");
            CheckKey(KeyPad_8, "*");
            CheckKey(KeyPad_9, "(");
            CheckKey(KeyPad_TAB, "\t");
            CheckKey(KeyPad_Q, "q");
            CheckKey(KeyPad_W, "w");
            CheckKey(KeyPad_E, "e");
            CheckKey(KeyPad_R, "r");
            CheckKey(KeyPad_T, "t");
            CheckKey(KeyPad_Y, "y");
            CheckKey(KeyPad_U, "u");
            CheckKey(KeyPad_I, "i");
            CheckKey(KeyPad_O, "o");
            CheckKey(KeyPad_P, "p");
            CheckKey(KeyPad_LBRACKET, "{");
            CheckKey(KeyPad_RBRACKET, "}");
            CheckKey(KeyPad_INVDASH, "|");
            CheckKey(KeyPad_CAPSLOCK, "CapsLock");
            CheckKey(KeyPad_A , "a");
            CheckKey(KeyPad_S , "s");
            CheckKey(KeyPad_D , "d");
            CheckKey(KeyPad_F , "f");
            CheckKey(KeyPad_G , "g");
            CheckKey(KeyPad_H , "h");
            CheckKey(KeyPad_J , "j");
            CheckKey(KeyPad_K , "k");
            CheckKey(KeyPad_L , "l");
            CheckKey(KeyPad_SEMICOLON , ":");
            CheckKey(KeyPad_QUOTE , "\"");
            CheckKey(KeyPad_ENTER , "\n");
            CheckKey(KeyPad_Z , "z");
            CheckKey(KeyPad_X , "x");
            CheckKey(KeyPad_C , "c");
            CheckKey(KeyPad_V , "v");
            CheckKey(KeyPad_B , "b");
            CheckKey(KeyPad_N , "n");
            CheckKey(KeyPad_M , "m");
            CheckKey(KeyPad_SPACE , " ");
            CheckKey(KeyPad_MINUS , "_");
            CheckKey(KeyPad_EQUAL , "+");
            CheckKey(KeyPad_BACKSPACE , "Backspace");
        }
        else if(!CapsLk&&(KeyState[KeyPad_LSHIFT]||KeyState[KeyPad_RSHIFT]))
        {
            CheckKey(KeyPad_UPDOT, "~");
            CheckKey(KeyPad_0, ")");
            CheckKey(KeyPad_1, "!");
            CheckKey(KeyPad_2, "@");
            CheckKey(KeyPad_3, "#");
            CheckKey(KeyPad_4, "$");
            CheckKey(KeyPad_5, "%");
            CheckKey(KeyPad_6, "^");
            CheckKey(KeyPad_7, "&");
            CheckKey(KeyPad_8, "*");
            CheckKey(KeyPad_9, "(");
            CheckKey(KeyPad_TAB, "\t");
            CheckKey(KeyPad_Q, "Q");
            CheckKey(KeyPad_W, "W");
            CheckKey(KeyPad_E, "E");
            CheckKey(KeyPad_R, "R");
            CheckKey(KeyPad_T, "T");
            CheckKey(KeyPad_Y, "Y");
            CheckKey(KeyPad_U, "U");
            CheckKey(KeyPad_I, "I");
            CheckKey(KeyPad_O, "O");
            CheckKey(KeyPad_P, "P");
            CheckKey(KeyPad_LBRACKET, "{");
            CheckKey(KeyPad_RBRACKET, "}");
            CheckKey(KeyPad_INVDASH, "|");
            CheckKey(KeyPad_CAPSLOCK, "CapsLock");
            CheckKey(KeyPad_A , "A");
            CheckKey(KeyPad_S , "S");
            CheckKey(KeyPad_D , "D");
            CheckKey(KeyPad_F , "F");
            CheckKey(KeyPad_G , "G");
            CheckKey(KeyPad_H , "H");
            CheckKey(KeyPad_J , "J");
            CheckKey(KeyPad_K , "K");
            CheckKey(KeyPad_L , "L");
            CheckKey(KeyPad_SEMICOLON , ":");
            CheckKey(KeyPad_QUOTE , "\"");
            CheckKey(KeyPad_ENTER , "\n");
            CheckKey(KeyPad_Z , "Z");
            CheckKey(KeyPad_X , "X");
            CheckKey(KeyPad_C , "C");
            CheckKey(KeyPad_V , "V");
            CheckKey(KeyPad_B , "B");
            CheckKey(KeyPad_N , "N");
            CheckKey(KeyPad_M , "M");
            CheckKey(KeyPad_SPACE , " ");
            CheckKey(KeyPad_MINUS , "_");
            CheckKey(KeyPad_EQUAL , "+");
            CheckKey(KeyPad_BACKSPACE , "Backspace");
        }
        else if(CapsLk&&(!KeyState[KeyPad_LSHIFT]&&!KeyState[KeyPad_RSHIFT]))
        {
            CheckKey(KeyPad_UPDOT, "`");
            CheckKey(KeyPad_0, "0");
            CheckKey(KeyPad_1, "1");
            CheckKey(KeyPad_2, "2");
            CheckKey(KeyPad_3, "3");
            CheckKey(KeyPad_4, "4");
            CheckKey(KeyPad_5, "5");
            CheckKey(KeyPad_6, "6");
            CheckKey(KeyPad_7, "7");
            CheckKey(KeyPad_8, "8");
            CheckKey(KeyPad_9, "9");
            CheckKey(KeyPad_TAB, "\t");
            CheckKey(KeyPad_Q, "Q");
            CheckKey(KeyPad_W, "W");
            CheckKey(KeyPad_E, "E");
            CheckKey(KeyPad_R, "R");
            CheckKey(KeyPad_T, "T");
            CheckKey(KeyPad_Y, "Y");
            CheckKey(KeyPad_U, "U");
            CheckKey(KeyPad_I, "I");
            CheckKey(KeyPad_O, "O");
            CheckKey(KeyPad_P, "P");
            CheckKey(KeyPad_LBRACKET, "[");
            CheckKey(KeyPad_RBRACKET, "]");
            CheckKey(KeyPad_INVDASH, "\\");
            CheckKey(KeyPad_CAPSLOCK, "CapsLock");
            CheckKey(KeyPad_A , "A");
            CheckKey(KeyPad_S , "S");
            CheckKey(KeyPad_D , "D");
            CheckKey(KeyPad_F , "F");
            CheckKey(KeyPad_G , "G");
            CheckKey(KeyPad_H , "H");
            CheckKey(KeyPad_J , "J");
            CheckKey(KeyPad_K , "K");
            CheckKey(KeyPad_L , "L");
            CheckKey(KeyPad_SEMICOLON , ";");
            CheckKey(KeyPad_QUOTE , "'");
            CheckKey(KeyPad_ENTER , "\n");
            CheckKey(KeyPad_Z , "Z");
            CheckKey(KeyPad_X , "X");
            CheckKey(KeyPad_C , "C");
            CheckKey(KeyPad_V , "V");
            CheckKey(KeyPad_B , "B");
            CheckKey(KeyPad_N , "N");
            CheckKey(KeyPad_M , "M");
            CheckKey(KeyPad_SPACE , " ");
            CheckKey(KeyPad_MINUS , "-");
            CheckKey(KeyPad_EQUAL , "=");
            CheckKey(KeyPad_BACKSPACE , "Backspace");
        }
    }
    void CheckKey(int KeyCode, string character)
    {
        if(KeyState[KeyCode]&&!KeyProcessed[KeyCode])
        {
            if(character=="Backspace") buffer=buffer.Substring(0,buffer.Length-1);
            else if(character=="CapsLock") CapsLk=!CapsLk;
            else buffer+=character;
            KeyProcessed[KeyCode]=true;
        }
        if(KeyState[KeyCode]==false) KeyProcessed[KeyCode]=false;
    }
    public void Submit()
    {
        AfterSubmit.Invoke();
    }
    public void Cancel()
    {
        Destroy(DestroyAfterCancelled);
    }
}
