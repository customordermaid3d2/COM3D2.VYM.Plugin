using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using UnityEngine;
using UnityInjector.Attributes;
using PluginExt;
using UnityInjector;
using System.Xml;
//using COM3D2.GripMovePlugin.Plugin;
//using Valve.VR;


//@API実装・・・このコメントのある箇所にAPI実装関係の変更・追加をさせていただきました(17.3.23~4.26 nn@)

// コンパイル用コマンド  （COM3D2 64bit用）
// "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc" /t:library /lib:..\COM3D2x64_Data（環境に合わせて変更） /r:UnityEngine.dll /r:UnityInjector.dll /r:Assembly-CSharp.dll /r:Assembly-CSharp-firstpass.dll /r:ExIni.dll /r:PluginExt.dll COM3D2.VYM.Plugin.cs

namespace COM3D2.VibeYourMaid.Plugin
{

    [PluginFilter("COM3D2x64"), PluginFilter("COM3D2x86"), PluginFilter("COM3D2VRx64"),
    PluginFilter("COM3D2OHx64"), PluginFilter("COM3D2OHx86"), PluginFilter("COM3D2OHVRx64"),
    PluginName("VibeYourMaid"), PluginVersion("1.0.3.0")] //@API実装


    //public class VibeYourMaid : UnityInjector.PluginBase 
    public class VibeYourMaid : ExPluginBase
    //public class VibeYourMaid : ExPluginBase, DirectTouchTool.IDirectTouchEventHandlerProvider
    {
        //　設定クラス（Iniファイルで読み書きしたい変数はここに格納する）
        public class VibeYourMaidConfig
        { //@API実装//→API用にpublicに変更


            // 一般設定
            public KeyCode keyPluginToggleV0 = KeyCode.I;        //　本プラグインの有効無効の切替キー（Ｉキー）
            public KeyCode keyPluginToggleV1 = KeyCode.O;        //　GUI表示切り替えキー（Ｏキー）

            public KeyCode keyPluginToggleV2 = KeyCode.J;        //　バイブ停止キー（Ｊキー）
            public KeyCode keyPluginToggleV3 = KeyCode.K;        //　バイブ弱キー（Ｋキー）
            public KeyCode keyPluginToggleV4 = KeyCode.L;        //　バイブ強キー（Ｌキー）

            public KeyCode keyPluginToggleV5 = KeyCode.P;        //　メイド切替（Ｐキー）
            public KeyCode keyPluginToggleV6 = KeyCode.N;        //　男表示切替（Ｎキー）

            public KeyCode keyPluginToggleV7 = KeyCode.Keypad1;        //　一人称視点切替（テンキー１）
            public KeyCode keyPluginToggleV8 = KeyCode.Keypad2;        //　快感値ロック（テンキー２）
            public KeyCode keyPluginToggleV9 = KeyCode.Keypad3;        //　絶頂値ロック（テンキー３）

            public KeyCode keyPluginToggleV10 = KeyCode.Keypad4;        //　オートモード切り替え


            public bool bPluginEnabledV = true;                 //　本プラグインの有効状態（下記キーでON/OFFトグル）
            public int GuiFlag = 1;                            //　GUIの表示フラグ（0：非表示、1：表示、2：最小化）
            public bool GuiFlag2 = false;                      //　設定画面の表示フラグ
            public bool GuiFlag3 = false;                      //　命令画面の表示フラグ
            public bool bVoiceOverrideEnabledV = true;          //　キス時の音声オーバライド（上書き）機能を使う
            public int iYodareAppearLevelV = 3;                 //　所定の興奮レベル以上でよだれをつける（１～４のどれかを入れる、０で無効） 
            public int vExciteLevelThresholdV1 = 100;           //　興奮レベル１→２閾値
            public int vExciteLevelThresholdV2 = 180;           //　興奮レベル２→３閾値
            public int vExciteLevelThresholdV3 = 250;           //　興奮レベル３→４閾値


            //改変　表情管理（バイブ）
            public int vStateAltTime1VBase = 120;                 //　フェイスアニメの変化時間１（秒）（20→21の遷移、40→41の遷移）
            public int vStateAltTime2VBase = 180;                 //　フェイスアニメの変化時間２（秒）（30におけるランダム再遷移）
            public int vStateAltTime1VRandomExtend = 120;         //　変化時間１へのランダム加算（秒）
            public int vStateAltTime2VRandomExtend = 180;         //　変化時間２へのランダム加算（秒）
            public float fAnimeFadeTimeV = 1.0f;                //　バイブモードのフェイスアニメ等のフェード時間（秒）



            //　表情テーブル　（バイブ）
            public string[][] sFaceAnime20Vibe = new string[][] {
            new string[] { "困った" , "ダンス困り顔" , "恥ずかしい" , "苦笑い" , "引きつり笑顔" , "まぶたギュ" },
            new string[] { "困った" , "ダンス困り顔" , "恥ずかしい" , "苦笑い" , "引きつり笑顔" , "まぶたギュ" },
            new string[] { "怒り" , "興奮射精後１" , "発情" , "エロ痛み２" , "引きつり笑顔" , "エロ我慢３" },
            new string[] { "怒り" , "興奮射精後１" , "発情" , "エロ痛み２" , "引きつり笑顔" , "エロ我慢３" }
            };
            public string[][] sFaceAnime30Vibe = new string[][] {
            new string[] { "エロ痛み１" , "エロ痛み２" , "エロ我慢１" , "エロ我慢２" , "泣き" , "怒り" },
            new string[] { "エロ痛み１" , "エロ痛み２" , "エロ我慢１" , "エロ我慢２" , "泣き" , "怒り" },
            new string[] { "エロ痛み我慢" , "エロ痛み我慢２" , "エロ痛み我慢３" , "エロメソ泣き" , "エロ羞恥３" , "エロ我慢３" },
            new string[] { "エロ痛み我慢" , "エロ痛み我慢２" , "エロ痛み我慢３" , "エロメソ泣き" , "エロ羞恥３" , "エロ我慢３" }
            };
            public string[] sFaceAnime40Vibe = new string[] { "少し怒り", "思案伏せ目", "まぶたギュ", "エロメソ泣き" };

            public string[] sFaceAnimeStun = new string[] { "絶頂射精後１", "興奮射精後１", "エロメソ泣き", "エロ痛み２", "エロ我慢３", "引きつり笑顔", "エロ通常３", "泣き" };



            //　性格別声テーブル　弱バイブ版---------------------------------------------------------------
            //通常
            public string[][] sLoopVoice20PrideVibe = new string[][] {
            new string[] { "s0_01236.ogg" , "s0_01237.ogg" , "s0_01238.ogg" , "s0_01239.ogg" },
            new string[] { "s0_01236.ogg" , "s0_01237.ogg" , "s0_01238.ogg" , "s0_01239.ogg" },
            new string[] { "s0_01236.ogg" , "s0_01237.ogg" , "s0_01238.ogg" , "s0_01239.ogg" },
            new string[] { "s0_01236.ogg" , "s0_01237.ogg" , "s0_01238.ogg" , "s0_01239.ogg" },
            new string[] { "s0_01236.ogg" , "s0_01237.ogg" , "s0_01238.ogg" , "s0_01239.ogg" }
            };
            public string[][] sLoopVoice20CoolVibe = new string[][] {
            new string[] { "s1_02396.ogg" , "s1_02390.ogg" , "s1_02391.ogg" , "s1_02392.ogg" },
            new string[] { "s1_02396.ogg" , "s1_02390.ogg" , "s1_02391.ogg" , "s1_02392.ogg" },
            new string[] { "s1_02396.ogg" , "s1_02390.ogg" , "s1_02391.ogg" , "s1_02392.ogg" },
            new string[] { "s1_02396.ogg" , "s1_02390.ogg" , "s1_02391.ogg" , "s1_02392.ogg" },
            new string[] { "s1_02396.ogg" , "s1_02390.ogg" , "s1_02391.ogg" , "s1_02392.ogg" }
            };
            public string[][] sLoopVoice20PureVibe = new string[][] {
            new string[] { "s2_01235.ogg" , "s2_01236.ogg" , "s2_01237.ogg" , "s2_01238.ogg" },
            new string[] { "s2_01235.ogg" , "s2_01236.ogg" , "s2_01237.ogg" , "s2_01238.ogg" },
            new string[] { "s2_01235.ogg" , "s2_01236.ogg" , "s2_01237.ogg" , "s2_01238.ogg" },
            new string[] { "s2_01235.ogg" , "s2_01236.ogg" , "s2_01237.ogg" , "s2_01238.ogg" },
            new string[] { "s2_01235.ogg" , "s2_01236.ogg" , "s2_01237.ogg" , "s2_01238.ogg" }
            };
            public string[][] sLoopVoice20MukuVibe = new string[][] {
            new string[] { "H0_00053.ogg" , "H0_00054.ogg" , "H0_00055.ogg" , "H0_00056.ogg" },
            new string[] { "H0_09210.ogg" , "H0_09211.ogg" , "H0_09212.ogg" , "H0_09213.ogg" },
            new string[] { "H0_00053.ogg" , "H0_00054.ogg" , "H0_00055.ogg" , "H0_00056.ogg" },
            new string[] { "H0_09210.ogg" , "H0_09211.ogg" , "H0_09212.ogg" , "H0_09213.ogg" },
            new string[] { "H0_00053.ogg" , "H0_00054.ogg" , "H0_00055.ogg" , "H0_00056.ogg" }
            };
            public string[][] sLoopVoice20MajimeVibe = new string[][] {
            new string[] { "H1_00329_vd.ogg" , "H1_00330_vd.ogg" , "H1_00331_vd.ogg" , "H1_00332_vd.ogg" },
            new string[] { "H1_00417_vd.ogg" , "H1_00418_vd.ogg" , "H1_00419_vd.ogg" , "H1_00420_vd.ogg" },
            new string[] { "H1_00329_vd.ogg" , "H1_00330_vd.ogg" , "H1_00331_vd.ogg" , "H1_00332_vd.ogg" },
            new string[] { "H1_00417_vd.ogg" , "H1_00418_vd.ogg" , "H1_00419_vd.ogg" , "H1_00420_vd.ogg" },
            new string[] { "H1_00417_vd.ogg" , "H1_00418_vd.ogg" , "H1_00419_vd.ogg" , "H1_00420_vd.ogg" }
            };
            public string[][] sLoopVoice20RindereVibe = new string[][] {
            new string[] { "H2_00035.ogg" , "H2_00036.ogg" , "H2_00037.ogg" , "H2_00038.ogg" },
            new string[] { "H2_00035.ogg" , "H2_00036.ogg" , "H2_00037.ogg" , "H2_00038.ogg" },
            new string[] { "H2_00035.ogg" , "H2_00036.ogg" , "H2_00037.ogg" , "H2_00038.ogg" },
            new string[] { "H2_00035.ogg" , "H2_00036.ogg" , "H2_00037.ogg" , "H2_00038.ogg" },
            new string[] { "H2_00035.ogg" , "H2_00036.ogg" , "H2_00037.ogg" , "H2_00038.ogg" }
            };
            public string[][] sLoopVoice20SilentVibe = new string[][] {
            new string[] { "H3_00519.ogg" , "H3_00520.ogg" , "H3_00521.ogg" , "H3_00522.ogg" },
            new string[] { "H3_00527.ogg" , "H3_00528.ogg" , "H3_00529.ogg" , "H3_00530.ogg" },
            new string[] { "H3_00519.ogg" , "H3_00520.ogg" , "H3_00521.ogg" , "H3_00522.ogg" },
            new string[] { "H3_00527.ogg" , "H3_00528.ogg" , "H3_00529.ogg" , "H3_00530.ogg" },
            new string[] { "H3_00519.ogg" , "H3_00520.ogg" , "H3_00521.ogg" , "H3_00522.ogg" }
            };
            public string[][] sLoopVoice20DevilishVibe = new string[][] {
            new string[] { "H4_00853.ogg" , "H4_00854.ogg" , "H4_00855.ogg" , "H4_00856.ogg" },
            new string[] { "H4_01069.ogg" , "H4_01070.ogg" , "H4_01071.ogg" , "H4_01072.ogg" },
            new string[] { "H4_00853.ogg" , "H4_00854.ogg" , "H4_00855.ogg" , "H4_00856.ogg" },
            new string[] { "H4_01069.ogg" , "H4_01070.ogg" , "H4_01071.ogg" , "H4_01072.ogg" },
            new string[] { "H4_00853.ogg" , "H4_00854.ogg" , "H4_00855.ogg" , "H4_00856.ogg" }
            };
            public string[][] sLoopVoice20LadylikeVibe = new string[][] {
            new string[] { "H5_00919.ogg" , "H5_00600.ogg" , "H5_00921.ogg" , "H5_00928.ogg" },
            new string[] { "H5_00929.ogg" , "H5_00930.ogg" , "H5_00602.ogg" , "H5_00603.ogg" },
            new string[] { "H5_00919.ogg" , "H5_00600.ogg" , "H5_00921.ogg" , "H5_00928.ogg" },
            new string[] { "H5_00929.ogg" , "H5_00930.ogg" , "H5_00602.ogg" , "H5_00603.ogg" },
            new string[] { "H5_00919.ogg" , "H5_00600.ogg" , "H5_00921.ogg" , "H5_00928.ogg" }
            };
            public string[][] sLoopVoice20SecretaryVibe = new string[][] {
            new string[] { "H6_00358.ogg" , "H6_00359.ogg" , "H6_00360.ogg" , "H6_00361.ogg" },
            new string[] { "H6_00222.ogg" , "H6_00223.ogg" , "H6_00224.ogg" , "H6_00225.ogg" },
            new string[] { "H6_00358.ogg" , "H6_00359.ogg" , "H6_00360.ogg" , "H6_00361.ogg" },
            new string[] { "H6_00222.ogg" , "H6_00223.ogg" , "H6_00224.ogg" , "H6_00225.ogg" },
            new string[] { "H6_00358.ogg" , "H6_00359.ogg" , "H6_00360.ogg" , "H6_00361.ogg" }
            };
            public string[][] sLoopVoice20SisterVibe = new string[][] {
            new string[] { "H7_03089.ogg" , "H7_03090.ogg" , "H7_03091.ogg" , "H7_03098.ogg" },
            new string[] { "H7_02770.ogg" , "H7_02771.ogg" , "H7_02772.ogg" , "H7_02773.ogg" },
            new string[] { "H7_03089.ogg" , "H7_03090.ogg" , "H7_03091.ogg" , "H7_03098.ogg" },
            new string[] { "H7_02770.ogg" , "H7_02771.ogg" , "H7_02772.ogg" , "H7_02773.ogg" },
            new string[] { "H7_03089.ogg" , "H7_03090.ogg" , "H7_03091.ogg" , "H7_03098.ogg" }
            };
            
            #region 動作未確認ボイス
            /*
            *DLC未購入 or 未実装な性格のため、ファイルの存在確認できず
            */
            public string[][] sLoopVoice20YandereVibe = new string[][] {
            new string[] { "s3_02767.ogg" , "s3_02768.ogg" , "s3_02769.ogg" , "s3_02770.ogg" },
            new string[] { "s3_02767.ogg" , "s3_02768.ogg" , "s3_02769.ogg" , "s3_02770.ogg" },
            new string[] { "s3_02767.ogg" , "s3_02768.ogg" , "s3_02769.ogg" , "s3_02770.ogg" },
            new string[] { "s3_02767.ogg" , "s3_02768.ogg" , "s3_02769.ogg" , "s3_02770.ogg" },
            new string[] { "s3_02767.ogg" , "s3_02768.ogg" , "s3_02769.ogg" , "s3_02770.ogg" }
            };
            public string[][] sLoopVoice20AnesanVibe = new string[][] {
            new string[] { "s4_08211.ogg" , "s4_08212.ogg" , "s4_08213.ogg" , "s4_08214.ogg" },
            new string[] { "s4_08211.ogg" , "s4_08212.ogg" , "s4_08213.ogg" , "s4_08214.ogg" },
            new string[] { "s4_08211.ogg" , "s4_08212.ogg" , "s4_08213.ogg" , "s4_08214.ogg" },
            new string[] { "s4_08211.ogg" , "s4_08212.ogg" , "s4_08213.ogg" , "s4_08214.ogg" },
            new string[] { "s4_08211.ogg" , "s4_08212.ogg" , "s4_08213.ogg" , "s4_08214.ogg" }
            };
            public string[][] sLoopVoice20GenkiVibe = new string[][] {
            new string[] { "s5_04127.ogg" , "s5_04129.ogg" , "s5_04130.ogg" , "s5_04131.ogg" },
            new string[] { "s5_04127.ogg" , "s5_04048.ogg" , "s5_04130.ogg" , "s5_04048.ogg" },
            new string[] { "s5_04133.ogg" , "s5_04134.ogg" , "s5_04047.ogg" , "s5_04048.ogg" },
            new string[] { "s5_04133.ogg" , "s5_04134.ogg" , "s5_04047.ogg" , "s5_04131.ogg" },
            new string[] { "s5_04133.ogg" , "s5_04134.ogg" , "s5_04047.ogg" , "s5_04131.ogg" }
            };
            public string[][] sLoopVoice20SadistVibe = new string[][] {
            new string[] { "S6_02244.ogg" , "S6_02180.ogg" , "S6_02181.ogg" , "S6_02245.ogg" },
            new string[] { "S6_02179.ogg" , "S6_02243.ogg" , "S6_02246.ogg" , "S6_02182.ogg" },
            new string[] { "S6_02179.ogg" , "S6_02183.ogg" , "S6_02246.ogg" , "S6_02247.ogg" },
            new string[] { "S6_02183.ogg" , "S6_02184.ogg" , "S6_02246.ogg" , "S6_02247.ogg" },
            new string[] { "S6_02179.ogg" , "S6_02180.ogg" , "S6_02181.ogg" , "S6_02182.ogg" }
            };
            #endregion
            //フェラ
            public string[][] sLoopVoice20PrideFera = new string[][] {
            new string[] { "S0_01383.ogg" , "S0_01367.ogg" , "S0_01384.ogg" , "S0_01369.ogg" },
            new string[] { "S0_01383.ogg" , "S0_01367.ogg" , "S0_01384.ogg" , "S0_01369.ogg" },
            new string[] { "S0_01383.ogg" , "S0_01367.ogg" , "S0_01384.ogg" , "S0_01369.ogg" },
            new string[] { "S0_01383.ogg" , "S0_01367.ogg" , "S0_01384.ogg" , "S0_01369.ogg" },
            new string[] { "S0_01383.ogg" , "S0_01367.ogg" , "S0_01384.ogg" , "S0_01369.ogg" }
            };
            public string[][] sLoopVoice20CoolFera = new string[][] {
            new string[] { "S1_02455.ogg" , "S1_02440.ogg" , "S1_02457.ogg" , "S1_02442.ogg" },
            new string[] { "S1_02455.ogg" , "S1_02440.ogg" , "S1_02457.ogg" , "S1_02442.ogg" },
            new string[] { "S1_02455.ogg" , "S1_02440.ogg" , "S1_02457.ogg" , "S1_02442.ogg" },
            new string[] { "S1_02455.ogg" , "S1_02440.ogg" , "S1_02457.ogg" , "S1_02442.ogg" },
            new string[] { "S1_02455.ogg" , "S1_02440.ogg" , "S1_02457.ogg" , "S1_02442.ogg" }
            };
            public string[][] sLoopVoice20PureFera = new string[][] {
            new string[] { "S2_01296.ogg" , "S2_01281.ogg" , "S2_01298.ogg" , "S2_01282.ogg" },
            new string[] { "S2_01296.ogg" , "S2_01281.ogg" , "S2_01298.ogg" , "S2_01282.ogg" },
            new string[] { "S2_01296.ogg" , "S2_01281.ogg" , "S2_01298.ogg" , "S2_01282.ogg" },
            new string[] { "S2_01296.ogg" , "S2_01281.ogg" , "S2_01298.ogg" , "S2_01282.ogg" },
            new string[] { "S2_01296.ogg" , "S2_01281.ogg" , "S2_01298.ogg" , "S2_01282.ogg" }
            };

            public string[][] sLoopVoice20MukuFera = new string[][] {
            new string[] { "H0_00093.ogg" , "H0_00094.ogg" , "H0_00095.ogg" , "H0_00096.ogg" },
            new string[] { "H0_00101.ogg" , "H0_00102.ogg" , "H0_00103.ogg" , "H0_00104.ogg" },
            new string[] { "H0_00093.ogg" , "H0_00094.ogg" , "H0_00095.ogg" , "H0_00096.ogg" },
            new string[] { "H0_00101.ogg" , "H0_00102.ogg" , "H0_00103.ogg" , "H0_00104.ogg" },
            new string[] { "H0_00101.ogg" , "H0_00102.ogg" , "H0_00103.ogg" , "H0_00104.ogg" }
            };
            public string[][] sLoopVoice20MajimeFera = new string[][] {
            new string[] { "H1_00268.ogg" , "H1_00269.ogg" , "H1_00270.ogg" , "H1_00271.ogg" },
            new string[] { "H1_00273.ogg" , "H1_00274.ogg" , "H1_00275.ogg" , "H1_00276.ogg" },
            new string[] { "H1_00268.ogg" , "H1_00269.ogg" , "H1_00270.ogg" , "H1_00271.ogg" },
            new string[] { "H1_00273.ogg" , "H1_00274.ogg" , "H1_00275.ogg" , "H1_00276.ogg" },
            new string[] { "H1_00273.ogg" , "H1_00274.ogg" , "H1_00275.ogg" , "H1_00276.ogg" }
            };
            public string[][] sLoopVoice20RindereFera = new string[][] {
            new string[] { "H2_00067.ogg" , "H2_00068.ogg" , "H2_00069.ogg" , "H2_00070.ogg" },
            new string[] { "H2_00075.ogg" , "H2_00076.ogg" , "H2_00077.ogg" , "H2_00078.ogg" },
            new string[] { "H2_00067.ogg" , "H2_00068.ogg" , "H2_00069.ogg" , "H2_00070.ogg" },
            new string[] { "H2_00075.ogg" , "H2_00076.ogg" , "H2_00077.ogg" , "H2_00078.ogg" },
            new string[] { "H2_00075.ogg" , "H2_00076.ogg" , "H2_00077.ogg" , "H2_00078.ogg" }
            };
            public string[][] sLoopVoice20SilentFera = new string[][] {
            new string[] { "H3_00566.ogg" , "H3_00567.ogg" , "H3_00568.ogg" , "H3_00569.ogg" },
            new string[] { "H3_00574.ogg" , "H3_00575.ogg" , "H3_00576.ogg" , "H3_00577.ogg" },
            new string[] { "H3_00566.ogg" , "H3_00567.ogg" , "H3_00568.ogg" , "H3_00569.ogg" },
            new string[] { "H3_00574.ogg" , "H3_00575.ogg" , "H3_00576.ogg" , "H3_00577.ogg" },
            new string[] { "H3_00574.ogg" , "H3_00575.ogg" , "H3_00576.ogg" , "H3_00577.ogg" }
            };
            public string[][] sLoopVoice20DevilishFera = new string[][] {
            new string[] { "H4_00901.ogg" , "H4_00902.ogg" , "H4_00903.ogg" , "H4_00904.ogg" },
            new string[] { "H4_00909.ogg" , "H4_00910.ogg" , "H4_00911.ogg" , "H4_00912.ogg" },
            new string[] { "H4_00901.ogg" , "H4_00902.ogg" , "H4_00903.ogg" , "H4_00904.ogg" },
            new string[] { "H4_00909.ogg" , "H4_00910.ogg" , "H4_00911.ogg" , "H4_00912.ogg" },
            new string[] { "H4_00901.ogg" , "H4_00902.ogg" , "H4_00903.ogg" , "H4_00904.ogg" }
            };
            public string[][] sLoopVoice20LadylikeFera = new string[][] {
            new string[] { "H5_00753.ogg" , "H5_00754.ogg" , "H5_00755.ogg" , "H5_00757.ogg" },
            new string[] { "H5_00756.ogg" , "H5_00758.ogg" , "H5_00759.ogg" , "H5_00769.ogg" },
            new string[] { "H5_00753.ogg" , "H5_00754.ogg" , "H5_00755.ogg" , "H5_00757.ogg" },
            new string[] { "H5_00756.ogg" , "H5_00758.ogg" , "H5_00759.ogg" , "H5_00769.ogg" },
            new string[] { "H5_00753.ogg" , "H5_00754.ogg" , "H5_00755.ogg" , "H5_00757.ogg" }
            };
            public string[][] sLoopVoice20SecretaryFera = new string[][] {
            new string[] { "H6_00214.ogg" , "H6_00215.ogg" , "H6_00216.ogg" , "H6_00217.ogg" },
            new string[] { "H6_00218.ogg" , "H6_00219.ogg" , "H6_00318.ogg" , "H6_00319.ogg" },
            new string[] { "H6_00214.ogg" , "H6_00215.ogg" , "H6_00216.ogg" , "H6_00217.ogg" },
            new string[] { "H6_00218.ogg" , "H6_00219.ogg" , "H6_00318.ogg" , "H6_00319.ogg" },
            new string[] { "H6_00214.ogg" , "H6_00215.ogg" , "H6_00216.ogg" , "H6_00217.ogg" }
            };
            public string[][] sLoopVoice20SisterFera = new string[][] {
            new string[] { "H7_02922.ogg" , "H7_02923.ogg" , "H7_02924.ogg" , "H7_02925.ogg" },
            new string[] { "H7_02926.ogg" , "H7_02927.ogg" , "H7_02928.ogg" , "H7_02929.ogg" },
            new string[] { "H7_02922.ogg" , "H7_02923.ogg" , "H7_02924.ogg" , "H7_02925.ogg" },
            new string[] { "H7_02926.ogg" , "H7_02927.ogg" , "H7_02928.ogg" , "H7_02929.ogg" },
            new string[] { "H7_02922.ogg" , "H7_02923.ogg" , "H7_02924.ogg" , "H7_02925.ogg" }
            };

            #region 動作未確認ボイス
            public string[][] sLoopVoice20YandereFera = new string[][] {
            new string[] { "S3_02833.ogg" , "S3_02818.ogg" , "S3_02835.ogg" , "S3_02820.ogg" },
            new string[] { "S3_02833.ogg" , "S3_02818.ogg" , "S3_02835.ogg" , "S3_02820.ogg" },
            new string[] { "S3_02833.ogg" , "S3_02818.ogg" , "S3_02835.ogg" , "S3_02820.ogg" },
            new string[] { "S3_02833.ogg" , "S3_02818.ogg" , "S3_02835.ogg" , "S3_02820.ogg" },
            new string[] { "S3_02833.ogg" , "S3_02818.ogg" , "S3_02835.ogg" , "S3_02820.ogg" }
            };
            public string[][] sLoopVoice20AnesanFera = new string[][] {
            new string[] { "S4_08241.ogg" , "S4_08258.ogg" , "S4_08243.ogg" , "S4_08259.ogg" },
            new string[] { "S4_08241.ogg" , "S4_08258.ogg" , "S4_08243.ogg" , "S4_08259.ogg" },
            new string[] { "S4_08241.ogg" , "S4_08258.ogg" , "S4_08243.ogg" , "S4_08259.ogg" },
            new string[] { "S4_08241.ogg" , "S4_08258.ogg" , "S4_08243.ogg" , "S4_08259.ogg" },
            new string[] { "S4_08241.ogg" , "S4_08258.ogg" , "S4_08243.ogg" , "S4_08259.ogg" }
            };
            public string[][] sLoopVoice20GenkiFera = new string[][] {
            new string[] { "S5_04163.ogg" , "S5_04162.ogg" , "S5_04179.ogg" , "S5_04181.ogg" },
            new string[] { "S5_04163.ogg" , "S5_04162.ogg" , "S5_04179.ogg" , "S5_04181.ogg" },
            new string[] { "S5_04163.ogg" , "S5_04162.ogg" , "S5_04179.ogg" , "s5_04174.ogg" },
            new string[] { "S5_04163.ogg" , "S5_04162.ogg" , "S5_04179.ogg" , "s5_04174.ogg" },
            new string[] { "S5_04163.ogg" , "S5_04162.ogg" , "S5_04179.ogg" , "s5_04174.ogg" }
            };
            public string[][] sLoopVoice20SadistFera = new string[][] {
            new string[] { "S6_02219.ogg" , "S6_02220.ogg" , "S6_02221.ogg" , "S6_02222.ogg" },
            new string[] { "S6_02219.ogg" , "S6_02220.ogg" , "S6_02221.ogg" , "S6_02222.ogg" },
            new string[] { "S6_02219.ogg" , "S6_02220.ogg" , "S6_02221.ogg" , "S6_02222.ogg" },
            new string[] { "S6_02219.ogg" , "S6_02220.ogg" , "S6_02221.ogg" , "S6_02222.ogg" },
            new string[] { "S6_02219.ogg" , "S6_02220.ogg" , "S6_02221.ogg" , "S6_02222.ogg" }
            };
            #endregion

            //カスタムボイス１
            #region 動作未確認ボイス
            public string[][] sLoopVoice20Custom1 = new string[][] {
            new string[] { "N0_00435.ogg" , "N0_00449.ogg" },
            new string[] { "N0_00435.ogg" , "N0_00449.ogg" },
            new string[] { "N0_00435.ogg" , "N0_00449.ogg" },
            new string[] { "N0_00435.ogg" , "N0_00449.ogg" },
            new string[] { "N0_00435.ogg" , "N0_00449.ogg" }
            };
            //カスタムボイス２
            public string[][] sLoopVoice20Custom2 = new string[][] {
            new string[] { "N7_00262.ogg" , "N7_00267.ogg" , "N7_00269.ogg" , "N7_00272.ogg" },
            new string[] { "N7_00262.ogg" , "N7_00267.ogg" , "N7_00269.ogg" , "N7_00272.ogg" },
            new string[] { "N7_00262.ogg" , "N7_00267.ogg" , "N7_00269.ogg" , "N7_00272.ogg" },
            new string[] { "N7_00262.ogg" , "N7_00267.ogg" , "N7_00269.ogg" , "N7_00272.ogg" },
            new string[] { "N7_00262.ogg" , "N7_00267.ogg" , "N7_00269.ogg" , "N7_00272.ogg" }
            };
            //カスタムボイス３
            public string[][] sLoopVoice20Custom3 = new string[][] {
            new string[] { "N1_00170.ogg" , "N1_00191.ogg" , "N1_00192.ogg" , "N1_00194.ogg" },
            new string[] { "N1_00170.ogg" , "N1_00191.ogg" , "N1_00192.ogg" , "N1_00194.ogg" },
            new string[] { "N1_00170.ogg" , "N1_00191.ogg" , "N1_00192.ogg" , "N1_00194.ogg" },
            new string[] { "N1_00170.ogg" , "N1_00191.ogg" , "N1_00192.ogg" , "N1_00194.ogg" },
            new string[] { "N1_00170.ogg" , "N1_00191.ogg" , "N1_00192.ogg" , "N1_00194.ogg" }
            };
            //カスタムボイス４
            public string[][] sLoopVoice20Custom4 = new string[][] {
            new string[] { "N3_00157.ogg" , "N3_00370.ogg" },
            new string[] { "N3_00157.ogg" , "N3_00370.ogg" },
            new string[] { "N3_00157.ogg" , "N3_00370.ogg" },
            new string[] { "N3_00157.ogg" , "N3_00370.ogg" },
            new string[] { "N3_00157.ogg" , "N3_00370.ogg" }
            };
            #endregion



            //　性格別声テーブル　強バイブ版---------------------------------------------------------------
            //通常
            public string[][] sLoopVoice30PrideVibe = new string[][] {
            new string[] { "s0_01326.ogg" , "s0_01327.ogg" , "s0_01330.ogg" , "s0_01331.ogg" },
            new string[] { "s0_01326.ogg" , "s0_01327.ogg" , "s0_01330.ogg" , "s0_01331.ogg" },
            new string[] { "s0_01326.ogg" , "s0_01327.ogg" , "s0_01330.ogg" , "s0_01331.ogg" },
            new string[] { "s0_01326.ogg" , "s0_01327.ogg" , "s0_01330.ogg" , "s0_01331.ogg" },
            new string[] { "s0_01236.ogg" , "s0_01237.ogg" , "s0_01238.ogg" , "s0_01239.ogg" }
            };
            public string[][] sLoopVoice30CoolVibe = new string[][] {
            new string[] { "s1_02401.ogg" , "s1_02400.ogg" , "s1_02402.ogg" , "s1_02404.ogg" },
            new string[] { "s1_02401.ogg" , "s1_02400.ogg" , "s1_02402.ogg" , "s1_02404.ogg" },
            new string[] { "s1_02401.ogg" , "s1_02400.ogg" , "s1_02402.ogg" , "s1_02404.ogg" },
            new string[] { "s1_02401.ogg" , "s1_02400.ogg" , "s1_02402.ogg" , "s1_02404.ogg" },
            new string[] { "s1_02396.ogg" , "s1_02390.ogg" , "s1_02391.ogg" , "s1_02392.ogg" }
            };
            public string[][] sLoopVoice30PureVibe = new string[][] {
            new string[] { "s2_01185.ogg" , "s2_01186.ogg" , "s2_01187.ogg" , "s2_01188.ogg" },
            new string[] { "s2_01185.ogg" , "s2_01186.ogg" , "s2_01187.ogg" , "s2_01188.ogg" },
            new string[] { "s2_01185.ogg" , "s2_01186.ogg" , "s2_01187.ogg" , "s2_01188.ogg" },
            new string[] { "s2_01185.ogg" , "s2_01186.ogg" , "s2_01187.ogg" , "s2_01188.ogg" },
            new string[] { "s2_01235.ogg" , "s2_01236.ogg" , "s2_01237.ogg" , "s2_01238.ogg" }
            };

            public string[][] sLoopVoice30MukuVibe = new string[][] {
            new string[] { "H0_00057.ogg" , "H0_00058.ogg" , "H0_00059.ogg" , "H0_00060.ogg" },
            new string[] { "H0_09214.ogg" , "H0_09215.ogg" , "H0_09216.ogg" , "H0_09217.ogg" },
            new string[] { "H0_00129.ogg" , "H0_00130.ogg" , "H0_00131.ogg" , "H0_00132.ogg" },
            new string[] { "H0_09230.ogg" , "H0_09231.ogg" , "H0_09232.ogg" , "H0_09233.ogg" },
            new string[] { "H0_00233.ogg" , "H0_00234.ogg" , "H0_00235.ogg" , "H0_00236.ogg" }
            };
            public string[][] sLoopVoice30MajimeVibe = new string[][] {
            new string[] { "H1_00229.ogg" , "H1_00230.ogg" , "H1_00231.ogg" , "H1_00232.ogg" },
            new string[] { "H1_08956.ogg" , "H1_08957.ogg" , "H1_08958.ogg" , "H1_08959.ogg" },
            new string[] { "H1_00405.ogg" , "H1_00406.ogg" , "H1_00407.ogg" , "H1_00408.ogg" },
            new string[] { "H1_00245.ogg" , "H1_00246.ogg" , "H1_00247.ogg" , "H1_00248.ogg" },
            new string[] { "H1_00239.ogg" , "H1_00240.ogg" , "H1_00255.ogg" , "H1_00256.ogg" }
            };
            public string[][] sLoopVoice30RindereVibe = new string[][] {
            new string[] { "H2_00031.ogg" , "H2_00032.ogg" , "H2_00033.ogg" , "H2_00034.ogg" },
            new string[] { "H2_09831.ogg" , "H2_09832.ogg" , "H2_09833.ogg" , "H2_09834.ogg" },
            new string[] { "H2_00041.ogg" , "H2_00042.ogg" , "H2_09865.ogg" , "H2_09866.ogg" },
            new string[] { "H2_00047.ogg" , "H2_00048.ogg" , "H2_00049.ogg" , "H2_00050.ogg" },
            new string[] { "H2_00087.ogg" , "H2_00088.ogg" , "H2_00089.ogg" , "H2_00090.ogg" }
            };
            public string[][] sLoopVoice30SilentVibe = new string[][] {
            new string[] { "H3_00523.ogg" , "H3_00524.ogg" , "H3_00525.ogg" , "H3_00532.ogg" },
            new string[] { "H3_00739.ogg" , "H3_00740.ogg" , "H3_00741.ogg" , "H3_00533.ogg" },
            new string[] { "H3_00538.ogg" , "H3_00539.ogg" , "H3_00540.ogg" , "H3_00541.ogg" },
            new string[] { "H3_00556.ogg" , "H3_00557.ogg" , "H3_00562.ogg" , "H3_00563.ogg" },
            new string[] { "H3_00586.ogg" , "H3_00587.ogg" , "H3_00588.ogg" , "H3_00589.ogg" }
            };
            public string[][] sLoopVoice30DevilishVibe = new string[][] {
            new string[] { "H4_00857.ogg" , "H4_00858.ogg" , "H4_00859.ogg" , "H4_00860.ogg" },
            new string[] { "H4_01073.ogg" , "H4_01074.ogg" , "H4_01075.ogg" , "H4_01076.ogg" },
            new string[] { "H4_00867.ogg" , "H4_00868.ogg" , "H4_00897.ogg" , "H4_00898.ogg" },
            new string[] { "H4_00873.ogg" , "H4_00874.ogg" , "H4_00875.ogg" , "H4_00876.ogg" },
            new string[] { "H4_00921.ogg" , "H4_00922.ogg" , "H4_00923.ogg" , "H4_00924.ogg" }
            };
            public string[][] sLoopVoice30LadylikeVibe = new string[][] {
            new string[] { "H5_00660.ogg" , "H5_00659.ogg" , "H5_00662.ogg" , "H5_00663.ogg" },
            new string[] { "H5_00661.ogg" , "H5_00667.ogg" , "H5_00668.ogg" , "H5_00799.ogg" },
            new string[] { "H5_00660.ogg" , "H5_00659.ogg" , "H5_00662.ogg" , "H5_00663.ogg" },
            new string[] { "H5_00661.ogg" , "H5_00667.ogg" , "H5_00668.ogg" , "H5_00799.ogg" },
            new string[] { "H5_00661.ogg" , "H5_00667.ogg" , "H5_00668.ogg" , "H5_00799.ogg" }
            };
            public string[][] sLoopVoice30SecretaryVibe = new string[][] {
            new string[] { "H6_00362.ogg" , "H6_00363.ogg" , "H6_00364.ogg" , "H6_00365.ogg" },
            new string[] { "H6_00361.ogg" , "H6_00251.ogg" , "H6_00252.ogg" , "H6_00253.ogg" },
            new string[] { "H6_00362.ogg" , "H6_00363.ogg" , "H6_00364.ogg" , "H6_00365.ogg" },
            new string[] { "H6_00361.ogg" , "H6_00251.ogg" , "H6_00252.ogg" , "H6_00253.ogg" },
            new string[] { "H6_00362.ogg" , "H6_00363.ogg" , "H6_00364.ogg" , "H6_00365.ogg" }
            };
            public string[][] sLoopVoice30SisterVibe = new string[][] {
            new string[] { "H7_02774.ogg" , "H7_02775.ogg" , "H7_02776.ogg" , "H7_02777.ogg" },
            new string[] { "H7_02798.ogg" , "H7_02799.ogg" , "H7_02800.ogg" , "H7_02801.ogg" },
            new string[] { "H7_02774.ogg" , "H7_02775.ogg" , "H7_02776.ogg" , "H7_02777.ogg" },
            new string[] { "H7_02798.ogg" , "H7_02799.ogg" , "H7_02800.ogg" , "H7_02801.ogg" },
            new string[] { "H7_02774.ogg" , "H7_02775.ogg" , "H7_02776.ogg" , "H7_02777.ogg" }
            };

            #region 動作未確認ボイス
            public string[][] sLoopVoice30YandereVibe = new string[][] {
            new string[] { "s3_02797.ogg" , "s3_02798.ogg" , "s3_02691.ogg" , "s3_02796.ogg" },
            new string[] { "s3_02797.ogg" , "s3_02798.ogg" , "s3_02691.ogg" , "s3_02796.ogg" },
            new string[] { "s3_02797.ogg" , "s3_02798.ogg" , "s3_02691.ogg" , "s3_02796.ogg" },
            new string[] { "s3_02797.ogg" , "s3_02798.ogg" , "s3_02691.ogg" , "s3_02796.ogg" },
            new string[] { "s3_02767.ogg" , "s3_02768.ogg" , "s3_02769.ogg" , "s3_02770.ogg" }
            };
            public string[][] sLoopVoice30AnesanVibe = new string[][] {
            new string[] { "s4_08140.ogg" , "s4_08141.ogg" , "s4_08142.ogg" , "s4_08145.ogg" },
            new string[] { "s4_08140.ogg" , "s4_08141.ogg" , "s4_08142.ogg" , "s4_08145.ogg" },
            new string[] { "s4_08140.ogg" , "s4_08141.ogg" , "s4_08149.ogg" , "s4_08150.ogg" },
            new string[] { "s4_08140.ogg" , "s4_08134.ogg" , "s4_08149.ogg" , "s4_08150.ogg" },
            new string[] { "s4_08211.ogg" , "s4_08212.ogg" , "s4_08213.ogg" , "s4_08214.ogg" }
            };
            public string[][] sLoopVoice30GenkiVibe = new string[][] {
            new string[] { "s5_04133.ogg" , "s5_04058.ogg" , "s5_04055.ogg" , "s5_04050.ogg" },
            new string[] { "s5_04133.ogg" , "s5_04058.ogg" , "s5_04055.ogg" , "s5_04050.ogg" },
            new string[] { "s5_04051.ogg" , "s5_04055.ogg" , "s5_04054.ogg" , "s5_04052.ogg" },
            new string[] { "s5_04055.ogg" , "s5_04061.ogg" , "s5_04054.ogg" , "s5_04052.ogg" },
            new string[] { "s5_04133.ogg" , "s5_04134.ogg" , "s5_04047.ogg" , "s5_04131.ogg" }
            };
            public string[][] sLoopVoice30SadistVibe = new string[][] {
            new string[] { "S6_02183.ogg" , "S6_02184.ogg" , "S6_02246.ogg" , "S6_02247.ogg" },
            new string[] { "S6_02183.ogg" , "S6_02184.ogg" , "S6_02246.ogg" , "S6_02247.ogg" },
            new string[] { "S6_02248.ogg" , "S6_02184.ogg" , "S6_02185.ogg" , "S6_02249.ogg" },
            new string[] { "S6_02249.ogg" , "S6_02250.ogg" , "S6_02185.ogg" , "S6_02186.ogg" },
            new string[] { "S6_02243.ogg" , "S6_02244.ogg" , "S6_02245.ogg" , "S6_02246.ogg" }
            };
            #endregion

            //フェラ
            public string[][] sLoopVoice30PrideFera = new string[][] {
            new string[] { "S0_01385.ogg" , "S0_01371.ogg" , "S0_01386.ogg" , "S0_01387.ogg" },
            new string[] { "S0_01385.ogg" , "S0_01371.ogg" , "S0_01386.ogg" , "S0_01387.ogg" },
            new string[] { "S0_01385.ogg" , "S0_01371.ogg" , "S0_01386.ogg" , "S0_01387.ogg" },
            new string[] { "S0_01385.ogg" , "S0_01371.ogg" , "S0_01386.ogg" , "S0_01387.ogg" },
            new string[] { "S0_01383.ogg" , "S0_01367.ogg" , "S0_01384.ogg" , "S0_01369.ogg" }
            };
            public string[][] sLoopVoice30CoolFera = new string[][] {
            new string[] { "S1_02458.ogg" , "S1_02459.ogg" , "S1_02444.ogg" , "S1_02460.ogg" },
            new string[] { "S1_02458.ogg" , "S1_02459.ogg" , "S1_02444.ogg" , "S1_02460.ogg" },
            new string[] { "S1_02458.ogg" , "S1_02459.ogg" , "S1_02444.ogg" , "S1_02460.ogg" },
            new string[] { "S1_02458.ogg" , "S1_02459.ogg" , "S1_02444.ogg" , "S1_02460.ogg" },
            new string[] { "S1_02455.ogg" , "S1_02440.ogg" , "S1_02457.ogg" , "S1_02442.ogg" }
            };
            public string[][] sLoopVoice30PureFera = new string[][] {
            new string[] { "S2_01299.ogg" , "S2_01300.ogg" , "S2_01285.ogg" , "S2_01301.ogg" },
            new string[] { "S2_01299.ogg" , "S2_01300.ogg" , "S2_01285.ogg" , "S2_01301.ogg" },
            new string[] { "S2_01299.ogg" , "S2_01300.ogg" , "S2_01285.ogg" , "S2_01301.ogg" },
            new string[] { "S2_01299.ogg" , "S2_01300.ogg" , "S2_01285.ogg" , "S2_01301.ogg" },
            new string[] { "S2_01296.ogg" , "S2_01281.ogg" , "S2_01298.ogg" , "S2_01282.ogg" }
            };
            #region 動作未確認ボイス
            public string[][] sLoopVoice30YandereFera = new string[][] {
            new string[] { "S3_02836.ogg" , "S3_02837.ogg" , "S3_02822.ogg" , "S3_02838.ogg" },
            new string[] { "S3_02836.ogg" , "S3_02837.ogg" , "S3_02822.ogg" , "S3_02838.ogg" },
            new string[] { "S3_02836.ogg" , "S3_02837.ogg" , "S3_02822.ogg" , "S3_02838.ogg" },
            new string[] { "S3_02836.ogg" , "S3_02837.ogg" , "S3_02822.ogg" , "S3_02838.ogg" },
            new string[] { "S3_02833.ogg" , "S3_02818.ogg" , "S3_02835.ogg" , "S3_02820.ogg" }
            };
            public string[][] sLoopVoice30AnesanFera = new string[][] {
            new string[] { "S4_08244.ogg" , "S4_08245.ogg" , "S4_08262.ogg" , "S4_08246.ogg" },
            new string[] { "S4_08244.ogg" , "S4_08245.ogg" , "S4_08262.ogg" , "S4_08246.ogg" },
            new string[] { "S4_08244.ogg" , "S4_08245.ogg" , "S4_08262.ogg" , "S4_08246.ogg" },
            new string[] { "S4_08244.ogg" , "S4_08245.ogg" , "S4_08262.ogg" , "S4_08246.ogg" },
            new string[] { "S4_08241.ogg" , "S4_08258.ogg" , "S4_08243.ogg" , "S4_08259.ogg" }
            };
            public string[][] sLoopVoice30GenkiFera = new string[][] {
            new string[] { "S5_04093.ogg" , "S5_04094.ogg" , "S5_04102.ogg" , "S5_04100.ogg" },
            new string[] { "S5_04093.ogg" , "S5_04094.ogg" , "S5_04102.ogg" , "S5_04100.ogg" },
            new string[] { "S5_04093.ogg" , "S5_04094.ogg" , "S5_04102.ogg" , "S5_04100.ogg" },
            new string[] { "S5_04093.ogg" , "S5_04094.ogg" , "S5_04102.ogg" , "S5_04100.ogg" },
            new string[] { "S5_04163.ogg" , "S5_04162.ogg" , "S5_04179.ogg" , "s5_04174.ogg" }
            };
            public string[][] sLoopVoice30SadistFera = new string[][] {
            new string[] { "S6_02223.ogg" , "S6_02224.ogg" , "S6_02225.ogg" , "S6_02226.ogg" },
            new string[] { "S6_02223.ogg" , "S6_02224.ogg" , "S6_02225.ogg" , "S6_02226.ogg" },
            new string[] { "S6_02223.ogg" , "S6_02224.ogg" , "S6_02225.ogg" , "S6_02226.ogg" },
            new string[] { "S6_02223.ogg" , "S6_02224.ogg" , "S6_02225.ogg" , "S6_02226.ogg" },
            new string[] { "S6_02219.ogg" , "S6_02220.ogg" , "S6_02221.ogg" , "S6_02222.ogg" }
            };
            #endregion

            public string[][] sLoopVoice30RindereFera = new string[][] {
            new string[] { "H2_00143_vd.ogg" , "H2_00144_vd.ogg" , "H2_00145_vd.ogg" , "H2_00146_vd.ogg" },
            new string[] { "H2_00231_vd.ogg" , "H2_00232_vd.ogg" , "H2_00233_vd.ogg" , "H2_00234_vd.ogg" },
            new string[] { "H2_00071.ogg" , "H2_00072.ogg" , "H2_00073.ogg" , "H2_00074.ogg" },
            new string[] { "H2_00143.ogg" , "H2_00144.ogg" , "H2_00145.ogg" , "H2_00146.ogg" },
            new string[] { "H2_00231.ogg" , "H2_00232.ogg" , "H2_00233.ogg" , "H2_00234.ogg" }
            };
            public string[][] sLoopVoice30MajimeFera = new string[][] {
            new string[] { "H1_00341_vd.ogg" , "H1_00342_vd.ogg" , "H1_00343_vd.ogg" , "H1_00344_vd.ogg" },
            new string[] { "H1_00429_vd.ogg" , "H1_00430_vd.ogg" , "H1_00431_vd.ogg" , "H1_00432_vd.ogg" },
            new string[] { "H1_08990.ogg" , "H1_08991.ogg" , "H1_00383.ogg" , "H1_00384.ogg" },
            new string[] { "H1_00269.ogg" , "H1_00270.ogg" , "H1_00271.ogg" , "H1_00272.ogg" },
            new string[] { "H1_00341.ogg" , "H1_00342.ogg" , "H1_00343.ogg" , "H1_00344.ogg" }
            };
            public string[][] sLoopVoice30MukuFera = new string[][] {
            new string[] { "H0_00097.ogg" , "H0_00098.ogg" , "H0_00099.ogg" , "H0_00100.ogg" },
            new string[] { "H0_00107.ogg" , "H0_00108.ogg" , "H0_00203.ogg" , "H0_00204.ogg" },
            new string[] { "H0_00169.ogg" , "H0_00170.ogg" , "H0_00171.ogg" , "H0_00172.ogg" },
            new string[] { "H0_00257.ogg" , "H0_00258.ogg" , "H0_00259.ogg" , "H0_00260.ogg" },
            new string[] { "H0_00177.ogg" , "H0_00178.ogg" , "H0_00179.ogg" , "H0_00180.ogg" }
            };
            public string[][] sLoopVoice30SilentFera = new string[][] {
            new string[] { "H3_00570.ogg" , "H3_00571.ogg" , "H3_00572.ogg" , "H3_00573.ogg" },
            new string[] { "H3_00580.ogg" , "H3_00581.ogg" , "H3_00700.ogg" , "H3_00701.ogg" },
            new string[] { "H3_00674.ogg" , "H3_00675.ogg" , "H3_00676.ogg" , "H3_00677.ogg" },
            new string[] { "H3_00762.ogg" , "H3_00763.ogg" , "H3_00764.ogg" , "H3_00765.ogg" },
            new string[] { "H3_00682.ogg" , "H3_00683.ogg" , "H3_00684.ogg" , "H3_00685.ogg" }
            };
            public string[][] sLoopVoice30DevilishFera = new string[][] {
            new string[] { "H4_00905.ogg" , "H4_00906.ogg" , "H4_00907.ogg" , "H4_00908.ogg" },
            new string[] { "H4_00915.ogg" , "H4_00916.ogg" , "H4_01035.ogg" , "H4_01036.ogg" },
            new string[] { "H4_01009.ogg" , "H4_01010.ogg" , "H4_01011.ogg" , "H4_01012.ogg" },
            new string[] { "H4_01097.ogg" , "H4_01098.ogg" , "H4_01099.ogg" , "H4_01100.ogg" },
            new string[] { "H4_01017.ogg" , "H4_01018.ogg" , "H4_01019.ogg" , "H4_01020.ogg" }
            };
            public string[][] sLoopVoice30LadylikeFera = new string[][] {
            new string[] { "H5_00785.ogg" , "H5_00787.ogg" , "H5_00790.ogg" , "H5_00791.ogg" },
            new string[] { "H5_00800.ogg" , "H5_00801.ogg" , "H5_00802.ogg" , "H5_00804.ogg" },
            new string[] { "H5_00785.ogg" , "H5_00787.ogg" , "H5_00790.ogg" , "H5_00791.ogg" },
            new string[] { "H5_00800.ogg" , "H5_00801.ogg" , "H5_00802.ogg" , "H5_00804.ogg" },
            new string[] { "H5_00785.ogg" , "H5_00787.ogg" , "H5_00790.ogg" , "H5_00791.ogg" }
            };
            public string[][] sLoopVoice30SecretaryFera = new string[][] {
            new string[] { "H6_00366.ogg" , "H6_00367.ogg" , "H6_00368.ogg" , "H6_00369.ogg" },
            new string[] { "H6_00370.ogg" , "H6_00371.ogg" , "H6_00372.ogg" , "H6_00373.ogg" },
            new string[] { "H6_00366.ogg" , "H6_00367.ogg" , "H6_00368.ogg" , "H6_00369.ogg" },
            new string[] { "H6_00370.ogg" , "H6_00371.ogg" , "H6_00372.ogg" , "H6_00373.ogg" },
            new string[] { "H6_00366.ogg" , "H6_00367.ogg" , "H6_00368.ogg" , "H6_00369.ogg" }
            };
            public string[][] sLoopVoice30SisterFera = new string[][] {
            new string[] { "H7_02970.ogg" , "H7_02971.ogg" , "H7_02972.ogg" , "H7_02973.ogg" },
            new string[] { "H7_02974.ogg" , "H7_02975.ogg" , "H7_02976.ogg" , "H7_02977.ogg" },
            new string[] { "H7_02970.ogg" , "H7_02971.ogg" , "H7_02972.ogg" , "H7_02973.ogg" },
            new string[] { "H7_02974.ogg" , "H7_02975.ogg" , "H7_02976.ogg" , "H7_02977.ogg" },
            new string[] { "H7_02970.ogg" , "H7_02971.ogg" , "H7_02972.ogg" , "H7_02973.ogg" }
            };

            //カスタムボイス
            #region 動作未確認ボイス
            public string[][] sLoopVoice30Custom1 = new string[][] {
            new string[] { "N0_00421.ogg" , "N0_00422.ogg" , "N0_00423.ogg" },
            new string[] { "N0_00421.ogg" , "N0_00422.ogg" , "N0_00423.ogg" },
            new string[] { "N0_00421.ogg" , "N0_00422.ogg" , "N0_00423.ogg" },
            new string[] { "N0_00421.ogg" , "N0_00422.ogg" , "N0_00423.ogg" },
            new string[] { "N0_00435.ogg" , "N0_00449.ogg" }
            };
            public string[][] sLoopVoice30Custom2 = new string[][] {
            new string[] { "N7_00252.ogg" , "N7_00255.ogg" , "N7_00267.ogg" , "N7_00261.ogg" },
            new string[] { "N7_00252.ogg" , "N7_00255.ogg" , "N7_00267.ogg" , "N7_00261.ogg" },
            new string[] { "N7_00252.ogg" , "N7_00255.ogg" , "N7_00267.ogg" , "N7_00261.ogg" },
            new string[] { "N7_00252.ogg" , "N7_00255.ogg" , "N7_00267.ogg" , "N7_00261.ogg" },
            new string[] { "N7_00262.ogg" , "N7_00267.ogg" , "N7_00269.ogg" , "N7_00272.ogg" }
            };
            public string[][] sLoopVoice30Custom3 = new string[][] {
            new string[] { "N1_00183.ogg" , "N1_00195.ogg" , "N1_00323.ogg" , "N1_00330.ogg" },
            new string[] { "N1_00183.ogg" , "N1_00195.ogg" , "N1_00323.ogg" , "N1_00330.ogg" },
            new string[] { "N1_00183.ogg" , "N1_00195.ogg" , "N1_00323.ogg" , "N1_00330.ogg" },
            new string[] { "N1_00183.ogg" , "N1_00195.ogg" , "N1_00323.ogg" , "N1_00330.ogg" },
            new string[] { "N1_00170.ogg" , "N1_00191.ogg" , "N1_00192.ogg" , "N1_00194.ogg" }
            };
            public string[][] sLoopVoice30Custom4 = new string[][] {
            new string[] { "N3_00310.ogg" , "N3_00318.ogg" , "N3_00377.ogg" },
            new string[] { "N3_00310.ogg" , "N3_00318.ogg" , "N3_00377.ogg" },
            new string[] { "N3_00310.ogg" , "N3_00318.ogg" , "N3_00377.ogg" },
            new string[] { "N3_00310.ogg" , "N3_00318.ogg" , "N3_00377.ogg" },
            new string[] { "N3_00157.ogg" , "N3_00370.ogg" }
            };
            #endregion


            //　性格別声テーブル　絶頂時---------------------------------------------------------------
            //通常
            public string[][] sOrgasmVoice30PrideVibe = new string[][] {
              new string[] { "s0_01898.ogg" , "s0_01899.ogg" , "s0_01902.ogg" , "s0_01900.ogg" },
              new string[] { "s0_01913.ogg" , "s0_01918.ogg" , "s0_01919.ogg" , "s0_01917.ogg" },
              new string[] { "s0_09072.ogg" , "s0_09070.ogg" , "s0_09099.ogg" , "s0_09059.ogg" },
              new string[] { "s0_09067.ogg" , "s0_09068.ogg" , "s0_09069.ogg" , "s0_09071.ogg" , "s0_09085.ogg" , "s0_09086.ogg" , "s0_09087.ogg" , "s0_09091.ogg" },
              new string[] { "s0_01898.ogg" , "s0_01899.ogg" , "s0_01902.ogg" , "s0_01900.ogg" }
              };
            public string[][] sOrgasmVoice30CoolVibe = new string[][] {
              new string[] { "s1_03223.ogg" , "s1_03246.ogg" , "s1_03247.ogg" , "s1_03210.ogg" },
              new string[] { "s1_03214.ogg" , "s1_03215.ogg" , "s1_03216.ogg" , "s1_03209.ogg" },
              new string[] { "s1_03207.ogg" , "s1_03205.ogg" , "s1_08993.ogg" , "s1_08971.ogg" },
              new string[] { "s1_09344.ogg" , "s1_09370.ogg" , "s1_09371.ogg" , "s1_09372.ogg" , "s1_09374.ogg" , "s1_09398.ogg" , "s1_09392.ogg" , "s1_09365.ogg" },
              new string[] { "s1_03223.ogg" , "s1_03246.ogg" , "s1_03247.ogg" , "s1_03210.ogg" }
              };
            public string[][] sOrgasmVoice30PureVibe = new string[][] {
              new string[] { "s2_01478.ogg" , "s2_01477.ogg" , "s2_01476.ogg" , "s2_01475.ogg" },
              new string[] { "s2_01432.ogg" , "s2_01433.ogg" , "s2_01434.ogg" , "s2_01436.ogg" },
              new string[] { "s2_09039.ogg" , "s2_09067.ogg" , "s2_09052.ogg" , "s2_08502.ogg" },
              new string[] { "s2_09047.ogg" , "s2_09048.ogg" , "s2_09049.ogg" , "s2_09050.ogg" , "s2_09051.ogg" , "s2_09066.ogg" , "s2_09069.ogg" , "s2_09073.ogg" },
              new string[] { "s2_01478.ogg" , "s2_01477.ogg" , "s2_01476.ogg" , "s2_01475.ogg" }
              };
            public string[][] sOrgasmVoice30YandereVibe = new string[][] {
              new string[] { "s3_02908.ogg" , "s3_02950.ogg" , "s3_02923.ogg" , "s3_02932.ogg" },
              new string[] { "s3_02909.ogg" , "s3_02910.ogg" , "s3_02915.ogg" , "s3_02914.ogg" },
              new string[] { "s3_02905.ogg" , "s3_02906.ogg" , "s3_02907.ogg" , "s3_05540.ogg" },
              new string[] { "s3_05657.ogg" , "s3_05658.ogg" , "s3_05659.ogg" , "s3_05660.ogg" , "s3_05661.ogg" , "s3_05678.ogg" , "s3_05651.ogg" , "s3_05656.ogg" },
              new string[] { "s3_02908.ogg" , "s3_02950.ogg" , "s3_02923.ogg" , "s3_02932.ogg" }
              };
            public string[][] sOrgasmVoice30AnesanVibe = new string[][] {
              new string[] { "s4_08348.ogg" , "s4_08354.ogg" , "s4_08365.ogg" , "s4_08374.ogg" },
              new string[] { "s4_08345.ogg" , "s4_08346.ogg" , "s4_08349.ogg" , "s4_08350.ogg" },
              new string[] { "s4_08347.ogg" , "s4_08355.ogg" , "s4_08356.ogg" , "s4_11658.ogg" },
              new string[] { "s4_11684.ogg" , "s4_11677.ogg" , "s4_11680.ogg" , "s4_11683.ogg" , "s4_11661.ogg" , "s4_11659.ogg" , "s4_11654.ogg" , "s4_11660.ogg" },
              new string[] { "s4_08348.ogg" , "s4_08354.ogg" , "s4_08365.ogg" , "s4_08374.ogg" }
              };
            public string[][] sOrgasmVoice30GenkiVibe = new string[][] {
              new string[] { "s5_04264.ogg" , "s5_04258.ogg" , "s5_04256.ogg" , "s5_04255.ogg" },
              new string[] { "s5_04265.ogg" , "s5_04270.ogg" , "s5_04267.ogg" , "s5_04268.ogg" },
              new string[] { "s5_04266.ogg" , "s5_18375.ogg" , "s5_18380.ogg" , "s5_18393.ogg" },
              new string[] { "s5_18379.ogg" , "s5_18380.ogg" , "s5_18382.ogg" , "s5_18384.ogg" , "s5_18385.ogg" , "s5_18400.ogg" , "s5_18402.ogg" , "s5_18119.ogg" },
              new string[] { "s5_04264.ogg" , "s5_04258.ogg" , "s5_04256.ogg" , "s5_04255.ogg" }
              };
            public string[][] sOrgasmVoice30SadistVibe = new string[][] {
              new string[] { "s6_01744.ogg" , "s6_02700.ogg" , "s6_02450.ogg" , "s6_02357.ogg" },
              new string[] { "S6_28847.ogg" , "S6_28853.ogg" , "S6_28814.ogg" , "S6_02397.ogg" },
              new string[] { "S6_28817.ogg" , "S6_02398.ogg" , "S6_02399.ogg" , "s6_02402.ogg" },
              new string[] { "S6_09048.ogg" , "S6_01984.ogg" , "S6_01988.ogg" , "S6_01991.ogg" , "S6_02000.ogg" , "S6_01996.ogg" , "S6_01997.ogg" , "S6_01998.ogg" , "S6_01999.ogg" , "S6_02001.ogg" , "s6_05796.ogg" , "s6_05797.ogg" , "s6_05798.ogg" , "s6_05799.ogg" , "s6_05800.ogg" , "s6_05801.ogg" },
              new string[] { "s6_01744.ogg" , "s6_02700.ogg" , "s6_02450.ogg" , "s6_02357.ogg" }
              };

            public string[][] sOrgasmVoice30RindereVibe = new string[][] {
              new string[] { "H2_13465.ogg" , "H2_13458.ogg" , "H2_13459.ogg" , "H2_13464.ogg" },
              new string[] { "H2_13525.ogg" , "H2_13528.ogg" , "H2_08326.ogg" , "H2_13592.ogg" },
              new string[] { "H2_13591.ogg" , "H2_13592.ogg" , "H2_13593.ogg" , "H2_13612.ogg" },
              new string[] { "H2_13663.ogg" , "H2_13678.ogg" , "H2_13692.ogg" , "H2_08099.ogg" , "H2_13464.ogg" , "H2_08003.ogg" , "H2_08339.ogg" , "H2_08338.ogg" },
              new string[] { "H2_11229.ogg" , "H2_11230.ogg" , "H2_10443.ogg" , "H2_10284.ogg" }
              };
            public string[][] sOrgasmVoice30MajimeVibe = new string[][] {
              new string[] { "H1_M3_14661.ogg" , "H1_05H_15497.ogg" , "H1_04_14942.ogg" , "H1_04_14939.ogg" },
              new string[] { "H1_11742.ogg" , "H1_11745.ogg" , "H1_11750.ogg" , "H1_11841.ogg" },
              new string[] { "H1_10835.ogg" , "H1_09998.ogg" , "H1_09999.ogg" , "H1_09996.ogg" },
              new string[] { "H1_10118.ogg" , "H1_10119.ogg" , "H1_14162.ogg" , "H1_14163.ogg" , "H1_14165.ogg" , "H1_14166.ogg" , "H1_11324.ogg" , "H1_11513.ogg" },
              new string[] { "H1_11742.ogg" , "H1_11745.ogg" , "H1_11750.ogg" , "H1_11841.ogg" }
              };
            public string[][] sOrgasmVoice30MukuVibe = new string[][] {
              new string[] { "H0_M3_14206.ogg" , "H0_M3_14234.ogg" , "H0_M3_14247.ogg" , "H0_05H_14924.ogg" },
              new string[] { "H0_05H_14925.ogg" , "H0_05H_14953.ogg" , "H0_05H_14954.ogg" , "H0_05H_14968.ogg" },
              new string[] { "H0_05H_15058.ogg" , "H0_05H_15093.ogg" , "H0_05H_15094.ogg" , "H0_05H_15095.ogg" },
              new string[] { "H0_08_16703.ogg" , "H0_08_16704.ogg" , "H0_ANIM_15167.ogg" , "H0_09812.ogg" , "H0_09805.ogg" , "H0_09883.ogg" , "H0_10870.ogg" , "s2_09073.ogg" },
              new string[] { "H0_05H_15058.ogg" , "H0_05H_15093.ogg" , "H0_05H_15094.ogg" , "H0_05H_15095.ogg" }
              };
            public string[][] sOrgasmVoice30SilentVibe = new string[][] {
              new string[] { "H3_08926.ogg" , "H3_08947.ogg" , "H3_08949.ogg" , "H3_08950.ogg" },
              new string[] { "H3_00781.ogg" , "H3_00785.ogg" , "H3_04792.ogg" , "H3_04793.ogg" },
              new string[] { "H3_08926.ogg" , "H3_08947.ogg" , "H3_08949.ogg" , "H3_08950.ogg" },
              new string[] { "H3_08599.ogg" , "H3_08697.ogg" , "H3_07553.ogg" , "H3_07571.ogg" , "H3_08926.ogg" , "H3_08949.ogg" , "H3_08950.ogg" , "H3_08844" },
              new string[] { "H3_00781.ogg" , "H3_00785.ogg" , "H3_04792.ogg" , "H3_04793.ogg" }
              };
            public string[][] sOrgasmVoice30DevilishVibe = new string[][] {
              new string[] { "H4_07896.ogg" , "H4_08495.ogg" , "H4_08496.ogg" , "H4_08510.ogg" },
              new string[] { "H4_08508.ogg" , "H4_08509.ogg" , "H4_08510.ogg" , "H4_08536.ogg" },
              new string[] { "H4_08533.ogg" , "H4_08534.ogg" , "H4_08535.ogg" , "H4_08536.ogg" },
              new string[] { "H4_08572.ogg" , "H4_08573.ogg" , "H4_08574.ogg" , "H4_08579.ogg" , "H4_08582.ogg" , "H4_08612.ogg" , "H4_08535.ogg" , "H4_08536.ogg" },
              new string[] { "H4_08533.ogg" , "H4_08534.ogg" , "H4_08535.ogg" , "H4_08536.ogg" }
              };
            public string[][] sOrgasmVoice30LadylikeVibe = new string[][] {
              new string[] { "H5_08468.ogg" , "H5_08486.ogg" , "H5_08487.ogg" , "H5_08667.ogg" },
              new string[] { "H5_02481.ogg" , "H5_06606.ogg" , "H5_05051.ogg" , "H5_05053.ogg" },
              new string[] { "H5_08468.ogg" , "H5_08486.ogg" , "H5_08487.ogg" , "H5_08667.ogg" },
              new string[] { "H5_00399.ogg" , "H5_05048.ogg" , "H5_01509.ogg" , "H5_05038.ogg" , "H5_05031.ogg" , "H5_00250.ogg" , "H5_08759.ogg" , "H5_08720.ogg" },
              new string[] { "H5_02481.ogg" , "H5_06606.ogg" , "H5_05051.ogg" , "H5_05053.ogg" }
              };
            public string[][] sOrgasmVoice30SecretaryVibe = new string[][] {
              new string[] { "H6_07912.ogg" , "H6_08721.ogg" , "H6_08912.ogg" , "H6_08956.ogg" },
              new string[] { "H6_09008.ogg" , "H6_09023.ogg" , "H6_09045.ogg" , "H6_08568.ogg" },
              new string[] { "H6_07912.ogg" , "H6_08721.ogg" , "H6_08912.ogg" , "H6_08956.ogg" },
              new string[] { "H6_04233.ogg" , "H6_09023.ogg" , "H6_04233.ogg" , "H6_08956.ogg" , "H6_09028.ogg" , "H6_08869.ogg" , "H6_08868.ogg" , "H6_08860.ogg" },
              new string[] { "H6_09008.ogg" , "H6_09023.ogg" , "H6_09045.ogg" , "H6_08568.ogg" }
              };
            public string[][] sOrgasmVoice30SisterVibe = new string[][] {
              new string[] { "H7_08974.ogg" , "H7_08797.ogg" , "H7_08798.ogg" , "H7_08794.ogg" },
              new string[] { "H7_04076.ogg" , "H7_01667.ogg" , "H7_02031.ogg" , "H7_02347.ogg" },
              new string[] { "H7_08974.ogg" , "H7_08797.ogg" , "H7_08798.ogg" , "H7_08794.ogg" },
              new string[] { "H7_02309.ogg" , "H7_08717.ogg" , "H7_02020.ogg" , "H7_08794.ogg" , "H7_00233.ogg" , "H7_01677.ogg" , "H7_06091.ogg" , "H7_03017.ogg" },
              new string[] { "H7_04076.ogg" , "H7_01667.ogg" , "H7_02031.ogg" , "H7_02347.ogg" }
              };

            //フェラ
            public string[][] sOrgasmVoice30PrideFera = new string[][] {
              new string[] { "S0_01922.ogg" , "S0_01920.ogg" , "S0_01921.ogg" },
              new string[] { "S0_01922.ogg" , "S0_01920.ogg" , "S0_01921.ogg" },
              new string[] { "S0_01922.ogg" , "S0_01920.ogg" , "S0_01921.ogg" },
              new string[] { "S0_11361.ogg" , "S0_01931.ogg" , "S0_11350.ogg" , "S0_11349.ogg" },
              new string[] { "S0_01922.ogg" , "S0_01920.ogg" , "S0_01921.ogg" }
              };
            public string[][] sOrgasmVoice30CoolFera = new string[][] {
              new string[] { "S1_03219.ogg" , "S1_03218.ogg" , "S1_03228.ogg" },
              new string[] { "S1_03219.ogg" , "S1_03218.ogg" , "S1_03228.ogg" },
              new string[] { "S1_03219.ogg" , "S1_03218.ogg" , "S1_03228.ogg" },
              new string[] { "S1_11440.ogg" , "S1_11429.ogg" , "S1_11952.ogg" , "S1_19221.ogg" },
              new string[] { "S1_03219.ogg" , "S1_03218.ogg" , "S1_03228.ogg" }
              };
            public string[][] sOrgasmVoice30PureFera = new string[][] {
              new string[] { "S2_01446.ogg" , "S2_01445.ogg" , "S2_01495.ogg" },
              new string[] { "S2_01446.ogg" , "S2_01445.ogg" , "S2_01495.ogg" },
              new string[] { "S2_01446.ogg" , "S2_01445.ogg" , "S2_01495.ogg" },
              new string[] { "S2_11371.ogg" , "S2_11370.ogg" , "S2_11358.ogg" , "S2_11347.ogg" },
              new string[] { "S2_01446.ogg" , "S2_01445.ogg" , "S2_01495.ogg" }
              };
            public string[][] sOrgasmVoice30YandereFera = new string[][] {
              new string[] { "S3_02919.ogg" , "S3_02918.ogg" , "S3_02928.ogg" },
              new string[] { "S3_02919.ogg" , "S3_02918.ogg" , "S3_02928.ogg" },
              new string[] { "S3_02919.ogg" , "S3_02918.ogg" , "S3_02928.ogg" },
              new string[] { "S3_03084.ogg" , "S3_03184.ogg" , "S3_03162.ogg" , "S3_18748.ogg" },
              new string[] { "S3_02919.ogg" , "S3_02918.ogg" , "S3_02928.ogg" }
              };
            public string[][] sOrgasmVoice30AnesanFera = new string[][] {
              new string[] { "S4_08359.ogg" , "S4_08358.ogg" , "S4_08368.ogg" },
              new string[] { "S4_08359.ogg" , "S4_08358.ogg" , "S4_08368.ogg" },
              new string[] { "S4_08359.ogg" , "S4_08358.ogg" , "S4_08368.ogg" },
              new string[] { "S4_05728.ogg" , "S4_05726.ogg" , "S4_05680.ogg" , "S4_05668.ogg" },
              new string[] { "S4_08359.ogg" , "S4_08358.ogg" , "S4_08368.ogg" }
              };
            public string[][] sOrgasmVoice30GenkiFera = new string[][] {
              new string[] { "s5_04271.ogg" , "s5_04272.ogg" , "s5_04273.ogg" },
              new string[] { "s5_04271.ogg" , "s5_04272.ogg" , "s5_04273.ogg" },
              new string[] { "s5_04271.ogg" , "s5_04272.ogg" , "s5_04273.ogg" },
              new string[] { "S5_07752.ogg" , "S5_07753.ogg" , "s5_04273.ogg" , "s5_04271.ogg" },
              new string[] { "s5_04271.ogg" , "s5_04272.ogg" , "s5_04273.ogg" }
              };
            public string[][] sOrgasmVoice30SadistFera = new string[][] {
              new string[] { "S6_28832.ogg" , "s6_02403.ogg" , "S6_28835.ogg" },
              new string[] { "S6_28835.ogg" , "s6_02403.ogg" , "s6_02404.ogg" },
              new string[] { "S6_28838.ogg" , "s6_02404.ogg" , "s6_02405.ogg" },
              new string[] { "S6_02420.ogg" , "S6_08109.ogg" , "S6_08112.ogg" , "S6_08114.ogg" , "s6_02404.ogg" , "s6_02405.ogg"  },
              new string[] { "S6_28832.ogg" , "s6_02403.ogg" , "S6_28835.ogg" }
              };

            public string[][] sOrgasmVoice30RindereFera = new string[][] {
              new string[] { "H2_05H_14450.ogg" , "H2_05H_14366.ogg" , "H2_05H_14370.ogg" },
              new string[] { "H2_05H_14450.ogg" , "H2_05H_14366.ogg" , "H2_05H_14370.ogg" },
              new string[] { "H2_05H_14450.ogg" , "H2_05H_14366.ogg" , "H2_05H_14370.ogg" },
              new string[] { "H2_13670.ogg" , "H2_05H_14450.ogg" , "H2_08822.ogg" , "H2_08824.ogg" },
              new string[] { "H2_05H_14450.ogg" , "H2_05H_14366.ogg" , "H2_05H_14370.ogg" }
              };
            public string[][] sOrgasmVoice30MajimeFera = new string[][] {
              new string[] { "H1_09204.ogg" , "H1_09205.ogg" , "H1_00399.ogg" },
              new string[] { "H1_09204.ogg" , "H1_09205.ogg" , "H1_00399.ogg" },
              new string[] { "H1_09204.ogg" , "H1_09205.ogg" , "H1_00399.ogg" },
              new string[] { "H1_09204.ogg" , "H1_04634.ogg" , "H1_08832.ogg" , "H1_03647.ogg" },
              new string[] { "H1_09204.ogg" , "H1_09205.ogg" , "H1_00399.ogg" }
              };
            public string[][] sOrgasmVoice30MukuFera = new string[][] {
              new string[] { "H0_01829.ogg" , "H0_01830.ogg" , "H0_08714.ogg" },
              new string[] { "H0_01829.ogg" , "H0_01830.ogg" , "H0_08714.ogg" },
              new string[] { "H0_01829.ogg" , "H0_01830.ogg" , "H0_08714.ogg" },
              new string[] { "H0_08739.ogg" , "H0_08741.ogg" , "H0_12666.ogg" , "H0_08714.ogg" },
              new string[] { "H0_01829.ogg" , "H0_01830.ogg" , "H0_08714.ogg" }
              };
            public string[][] sOrgasmVoice30SilentFera = new string[][] {
              new string[] { "H3_04950.ogg" , "H3_04953.ogg" , "H3_04954.ogg" },
              new string[] { "H3_04950.ogg" , "H3_04953.ogg" , "H3_04954.ogg" },
              new string[] { "H3_04950.ogg" , "H3_04953.ogg" , "H3_04954.ogg" },
              new string[] { "H3_08196.ogg" , "H3_08198.ogg" , "H3_08202.ogg" , "H3_08126.ogg" },
              new string[] { "H3_04950.ogg" , "H3_04953.ogg" , "H3_04954.ogg" }
              };
            public string[][] sOrgasmVoice30DevilishFera = new string[][] {
              new string[] { "H4_01044.ogg" , "H4_01051.ogg" , "H4_01052.ogg" },
              new string[] { "H4_01044.ogg" , "H4_01051.ogg" , "H4_01052.ogg" },
              new string[] { "H4_01044.ogg" , "H4_01051.ogg" , "H4_01052.ogg" },
              new string[] { "H4_01065.ogg" , "H4_01066.ogg" , "H4_01067.ogg" , "H4_01068.ogg" },
              new string[] { "H4_01044.ogg" , "H4_01051.ogg" , "H4_01052.ogg" }
              };
            public string[][] sOrgasmVoice30LadylikeFera = new string[][] {
              new string[] { "H5_00791.ogg" , "H5_00790.ogg" , "H5_00800.ogg" },
              new string[] { "H5_00801.ogg" , "H5_00802.ogg" , "H5_00803.ogg" },
              new string[] { "H5_00791.ogg" , "H5_00790.ogg" , "H5_00800.ogg" },
              new string[] { "H5_00804.ogg" , "H5_00805.ogg" , "H5_00806.ogg" , "H5_00807.ogg" },
              new string[] { "H5_00801.ogg" , "H5_00802.ogg" , "H5_00803.ogg" }
              };
            public string[][] sOrgasmVoice30SecretaryFera = new string[][] {
              new string[] { "H6_00345.ogg" , "H6_00346.ogg" , "H6_00347.ogg" },
              new string[] { "H6_00350.ogg" , "H6_00351.ogg" , "H6_00352.ogg" },
              new string[] { "H6_00356.ogg" , "H6_00357.ogg" , "H6_00366.ogg" },
              new string[] { "H6_00367.ogg" , "H6_00368.ogg" , "H6_00369.ogg" , "H6_00370.ogg" },
              new string[] { "H6_00371.ogg" , "H6_00372.ogg" , "H6_00373.ogg" }
              };
            public string[][] sOrgasmVoice30SisterFera = new string[][] {
              new string[] { "H7_02944.ogg" , "H7_02961.ogg" , "H7_02975.ogg" },
              new string[] { "H6_00350.ogg" , "H6_00351.ogg" , "H6_00352.ogg" },
              new string[] { "H7_02944.ogg" , "H7_02961.ogg" , "H7_02975.ogg" },
              new string[] { "H7_02944.ogg" , "H7_02961.ogg" , "H7_02975.ogg" , "H7_02977.ogg" },
              new string[] { "H6_00350.ogg" , "H6_00351.ogg" , "H6_00352.ogg" }
              };

            //カスタムボイス
            public string[][] sOrgasmVoice30Custom1 = new string[][] {
              new string[] { "N0_00424.ogg" , "N0_00459.ogg" , "N0_00503.ogg" , "N0_00508.ogg" , "N0_00534.ogg" },
              new string[] { "N0_00424.ogg" , "N0_00459.ogg" , "N0_00503.ogg" , "N0_00508.ogg" , "N0_00534.ogg" },
              new string[] { "N0_00424.ogg" , "N0_00457.ogg" , "N0_00503.ogg" , "N0_00508.ogg" , "N0_00534.ogg" },
              new string[] { "N0_00456.ogg" , "N0_00457.ogg" , "N0_00458.ogg" , "N0_00534.ogg" , "N0_00288.ogg" , "N0_00292.ogg" , "N0_00293.ogg" },
              new string[] { "N0_00424.ogg" , "N0_00459.ogg" , "N0_00503.ogg" , "N0_00508.ogg" , "N0_00534.ogg" }
              };
            public string[][] sOrgasmVoice30Custom2 = new string[][] {
              new string[] { "N7_00251.ogg" , "N7_00267.ogg" , "N7_00275.ogg" , "N7_00276.ogg" , "N7_00280.ogg" },
              new string[] { "N7_00251.ogg" , "N7_00267.ogg" , "N7_00275.ogg" , "N7_00276.ogg" , "N7_00280.ogg" },
              new string[] { "N7_00251.ogg" , "N7_00267.ogg" , "N7_00275.ogg" , "N7_00276.ogg" , "N7_00280.ogg" },
              new string[] { "N7_00284.ogg" , "N7_00291.ogg" , "N7_00293.ogg" , "N7_00294.ogg" , "N7_00295.ogg" , "N7_00275.ogg" , "n7_00295.ogg" },
              new string[] { "N7_00251.ogg" , "N7_00267.ogg" , "N7_00275.ogg" , "N7_00276.ogg" , "N7_00280.ogg" }
              };
            public string[][] sOrgasmVoice30Custom3 = new string[][] {
              new string[] { "N1_00179.ogg" , "N1_00180.ogg" , "N1_00200.ogg" , "N1_00204.ogg" , "N1_00209.ogg" },
              new string[] { "N1_00179.ogg" , "N1_00180.ogg" , "N1_00200.ogg" , "N1_00204.ogg" , "N1_00209.ogg" },
              new string[] { "N1_00179.ogg" , "N1_00180.ogg" , "N1_00200.ogg" , "N1_00204.ogg" , "N1_00209.ogg" },
              new string[] { "N1_00179.ogg" , "N1_00180.ogg" , "N1_00198.ogg" , "N1_00199.ogg" , "N1_00205.ogg" , "N1_00217.ogg" , "N1_00333.ogg" },
              new string[] { "N1_00179.ogg" , "N1_00180.ogg" , "N1_00200.ogg" , "N1_00204.ogg" , "N1_00209.ogg" }
              };
            public string[][] sOrgasmVoice30Custom4 = new string[][] {
              new string[] { "N3_00193.ogg" , "N3_00194.ogg" , "N3_00195.ogg" , "N3_00330.ogg" , "N3_00378.ogg" },
              new string[] { "N3_00193.ogg" , "N3_00194.ogg" , "N3_00195.ogg" , "N3_00330.ogg" , "N3_00378.ogg" },
              new string[] { "N3_00193.ogg" , "N3_00194.ogg" , "N3_00195.ogg" , "N3_00330.ogg" , "N3_00378.ogg" },
              new string[] { "N3_00376.ogg" , "N3_00194.ogg" , "N3_00195.ogg" , "N3_00197.ogg" , "N3_00203.ogg" , "N3_00328.ogg" , "N3_00330.ogg" , "N3_00379.ogg" },
              new string[] { "N3_00193.ogg" , "N3_00194.ogg" , "N3_00195.ogg" , "N3_00330.ogg" , "N3_00378.ogg" }
              };


            //　性格別声テーブル　停止時
            public string[] sLoopVoice40PrideVibe = new string[] { "S0_01967.ogg", "S0_01967.ogg", "S0_01968.ogg", "S0_01969.ogg", "S0_01969.ogg" };
            public string[] sLoopVoice40CoolVibe = new string[] { "S1_03264.ogg", "S1_03264.ogg", "S1_03265.ogg", "S1_03266.ogg", "S1_03266.ogg" };
            public string[] sLoopVoice40PureVibe = new string[] { "s2_01491.ogg", "s2_01491.ogg", "s2_01492.ogg", "s2_01493.ogg", "s2_01493.ogg" };
            public string[] sLoopVoice40YandereVibe = new string[] { "S3_02964.ogg", "S3_02964.ogg", "S3_02965.ogg", "S3_02966.ogg", "S3_02966.ogg" };
            public string[] sLoopVoice40AnesanVibe = new string[] { "s4_08424.ogg", "s4_08426.ogg", "s4_08427.ogg", "s4_08428.ogg", "s4_08428.ogg" };
            public string[] sLoopVoice40GenkiVibe = new string[] { "s5_04127.ogg", "s5_04129.ogg", "s5_04131.ogg", "s5_04134.ogg", "s5_04134.ogg" };
            public string[] sLoopVoice40SadistVibe = new string[] { "s6_02477.ogg", "s6_02478.ogg", "s6_02479.ogg", "s6_02481.ogg", "s6_02480.ogg" };
            public string[] sLoopVoice40RindereVibe = new string[] { "H2_00119.ogg", "H2_09861.ogg", "H2_09866.ogg", "H2_00217.ogg", "H2_00122.ogg" };
            public string[] sLoopVoice40MajimeVibe = new string[] { "H1_00509.ogg", "H1_00515.ogg", "H1_00516.ogg", "H1_00511.ogg", "H1_00517.ogg" };
            public string[] sLoopVoice40MukuVibe = new string[] { "H0_00339_vd.ogg", "H0_00352_vd.ogg", "H0_00353_vd.ogg", "H0_00354_vd.ogg", "H0_00342_vd.ogg" };
            public string[] sLoopVoice40SilentVibe = new string[] { "H3_00844.ogg", "H3_00857.ogg", "H3_00858.ogg", "H3_00859.ogg", "H3_00847.ogg" };
            public string[] sLoopVoice40DevilishVibe = new string[] { "H4_01179.ogg", "H4_01194.ogg", "H4_01182.ogg", "H4_01191.ogg", "H4_00858.ogg" };
            public string[] sLoopVoice40LadylikeVibe = new string[] { "H5_00918.ogg", "H5_00924.ogg", "H5_00931.ogg", "H5_00932.ogg", "H5_00933.ogg" };
            public string[] sLoopVoice40SecretaryVibe = new string[] { "H6_00483.ogg", "H6_00489.ogg", "H6_00484.ogg", "H6_00490.ogg", "H6_00492.ogg" };
            public string[] sLoopVoice40SisterVibe = new string[] { "H7_03089.ogg", "H7_03090.ogg", "H7_03091.ogg", "H7_03098.ogg", "H7_03099.ogg" };

            public string[] sLoopVoice40Custom1 = new string[] { "N0_00460.ogg", "N0_00460.ogg", "N0_00460.ogg", "N0_00460.ogg", "N0_00460.ogg" };
            public string[] sLoopVoice40Custom2 = new string[] { "N7_00277.ogg", "N7_00277.ogg", "N7_00277.ogg", "N7_00277.ogg", "N7_00277.ogg" };
            public string[] sLoopVoice40Custom3 = new string[] { "N1_00382.ogg", "N1_00382.ogg", "N1_00382.ogg", "N1_00382.ogg", "N1_00382.ogg" };
            public string[] sLoopVoice40Custom4 = new string[] { "N3_00205.ogg", "N3_00205.ogg", "N3_00205.ogg", "N3_00205.ogg", "N3_00205.ogg" };


            //　性格別声テーブル　こっち来て
            public string[] sCallVoice = new string[] { "S0_13972.ogg", "S1_03893.ogg", "S2_08163.ogg", "S3_11386.ogg", "S4_15255.ogg", "S5_16924.ogg", "S6_18089.ogg", "H2_06796.ogg", "H2_06796.ogg", "H0_08819.ogg", "H2_06796.ogg", "H2_06796.ogg", "H2_06796.ogg", "H6_04771.ogg", "H7_08624.ogg", "" };



            //　シェイプキーアニメリスト
            //　모양 키 애니메이션 목록
            public string[] ShapeListR = new string[] { "randamX", "randamY" };
            public string[] ShapeListW = new string[] { "X1", "Y1", "waveX", "waveY" };
            public string[] ShapeListW2 = new string[] { "逆反復" };
            public string[] ShapeListI = new string[] { "シェイプキーを記述" };

            //　バイブ弱時のアニメーション設定
            public float RandamMin1 = 0f;
            public float RandamMax1 = 30f;

            public float WaveMin1 = 0f;
            public float WaveMax1 = 100f;
            public float WaveSpead1 = 12f;

            public float IncreaseMax1 = 100f;
            public float IncreaseSpead1 = 5f;

            //　バイブ強時のアニメーション設定
            public float RandamMin2 = 0f;
            public float RandamMax2 = 60f;

            public float WaveMin2 = 0f;
            public float WaveMax2 = 100f;
            public float WaveSpead2 = 20f;

            public float IncreaseMax2 = 100f;
            public float IncreaseSpead2 = 10f;


            //　痙攣幅の設定
            public float orgasmValue1 = 15f;
            public float orgasmValue2 = 30f;
            public float orgasmValue3 = 40f;


            //　クリトリス勃起上限
            public int clitorisMax = 150;

            //　ちんぽ勃起設定
            public float ChinpoMax = 100f;
            public float ChinpoMin = 50f;
            public float SoriMax = 100f;
            public float SoriMin = 50f;
            public float TamaValue = 0f;

            //有効シーン設定
            public int[] SceneList = new int[] { 20, 22, 5, 4, 15, 26, 24, 28, 27, 30, 31, 32, 34, 35, 36, 37, 43 };



            //改変終了---------------------------------------

        }

        //GUIで設定保存したい変数はここ
        public class VibeYourMaidCfgWriting
        {  //@API実装//→API用にpublicに変更

            //演出有効フラグ
            public bool NamidaEnabled = true;
            public bool HohoEnabled = true;
            public bool YodareEnabled = true;
            public bool CliAnimeEnabled = false;
            public bool ChinpoAnimeEnabled = false;
            public bool OrgsmAnimeEnabled = true;
            public bool SioEnabled = true;
            public bool NyoEnabled = true;
            public bool AheEnabled = false;
            public bool aseAnimeEnabled = false;
            public bool MotionChangeEnabled = true;
            public bool ZeccyouAnimeEnabled = true;
            public bool ZeccyouManAnimeEnabled = true;
            public bool zViceWaitEnabled = false;

            public bool MouthNomalEnabled = false;
            public bool MouthKissEnabled = false;
            public bool MouthFeraEnabled = false;
            public bool MouthZeccyouEnabled = false;

            public bool BreastMilkEnabled = true;
            public bool ChinpoMilkEnabled = true;
            public bool EnemaMilkEnabled = true;

            public bool hibuAnime1Enabled = false;
            public float hibuSlider1Value = 0f;
            public float analSlider1Value = 0f;
            public float kupaWave = 5f;

            public bool hibuAnime2Enabled = false;
            public float hibuSlider2Value = 0f;
            public float analSlider2Value = 0f;

            public bool ClearEnabled = false;
            public bool TaikiEnabled = true;
            public bool CamChangeEnabled = false;

            //public bool MaidLinkEnabled = false;
            public bool MaidLinkMotionEnabled = false;
            public bool MaidLinkVoiceEnabled = false;
            public bool MaidLinkFaceEnabled = false;
            public bool MaidLinkShapeEnabled = false;

            public bool camCheckEnabled = true;
            public float camCheckRange = 0.5f;

            public bool autoManEnabled = false;

            public bool[] andKeyEnabled = new bool[] { false, false, false };

            public int SelectSE = 0;

        }

        private bool StartFlag = false;                    //　シーンがかわってから操作されたかどうか

        //メイド情報
        private CameraMain mainCamera;
        private Maid maid = null;
        private Transform[] maidHead = new Transform[20];
        private Transform maidMune;


        //複数メイド取得用
        private Maid[] SubMaids = new Maid[20];
        private CharacterMgr cm;
        private int iCurrentMaid = 0;
        private bool bReGetMaid = false;
        private List<int> maidDataList = new List<int>();

        //男モデル
        private Maid man;
        private Maid[] SubMans = new Maid[4];
        private int[] MansTg = new int[4];
        private string[] SubMansName = new string[] { "ご主人様", "モブ男Ａ", "モブ男Ｂ", "モブ男Ｃ" };
        private bool RankoEnabled = false;
        private int MansFGet = 0;



        //IK操作用 位置調整が面倒で使いものにならないのでボツ
        /*
        private int iSousa = 0;
        private int iTarget = 0;
        private int iHand = 0;
        public string[][] HandSelect = new string[][] {
          new string[] { "左手" , "左手" },
          new string[] { "右手" , "右手" },
          //new string[] { "_IK_vagina" , "あそこ" },
          //new string[] { "_IK_footL" , "左足" },
          //new string[] { "_IK_footR" , "右足" }
        };

        private string[][] BoneList = new string[][] {
          //new string[] { "_IK_hohoL" , "左頬" },
          //new string[] { "_IK_hohoR" , "右頬" },
          new string[] { "_IK_muneL" , "左胸" },
          new string[] { "_IK_muneR" , "右胸" },
          //new string[] { "_IK_handL" , "左手" },
          //new string[] { "_IK_handR" , "右手" },
          //new string[] { "_IK_hara" , "おなか" },
          new string[] { "_IK_vagina" , "あそこ" },
          new string[] { "_IK_hipL" , "お尻左" },
          new string[] { "_IK_hipR" , "お尻右" },
          new string[] { "_IK_anal" , "アナル" },
          //new string[] { "_IK_footL" , "左足" },
          //new string[] { "_IK_footR" , "右足" }
        };
        
        private float OffsetXValue = 0f;
        private float OffsetYValue = 0f;
        private float OffsetZValue = 0f;
        */



        //　状態管理
        private int vState = 0;                             //　ステート番号
        private int vStateMajor = 10;                       //　強弱によるステート
        private int vStateMajorOld = 10;                    //　強弱によるステート（前回値）
        private int vStateMinor = 0;                        //　時間経過によるステート

        //　10 …　停止
        //　20 …　弱
        //　30 …　強

        public int VLevel = 0;                              //　改変　バイブ状態

        //　音声・表情のモード切り替え用
        public bool AutoModeEnabled = true;
        public int ModeSelect = 0;
        public string[] ModeSelectList = new string[] { "通常固定", "フェラ固定", "カスタム１", "カスタム２", "カスタム３", "カスタム４" };


        //SE切替関連
        private string[][] SeFileList = new string[][] {
          new string[] { "バイブ音" , "抽挿音" , "オート" },
          new string[] { "se020.ogg" , "se028.ogg" },
          new string[] { "se019.ogg" , "se029.ogg" }
        };


        //オートモード関連
        public int autoSelect = 0;
        public string[] autoSelectList = new string[] { "オート無効", "じっくり", "激しく", "ほどほど", "セミオート" };

        public string[][] autoMotionList;


        //　メイドの絶頂回数・感度のセーブ（一晩でリセット）
        private bool SaveFlag = false;
        private List<double> vBoostBaseSave = new List<double>();
        private List<int> vOrgasmCountSave = new List<int>();
        private List<string> MaidNameSave = new List<string>();

        private List<int> VLevelSave = new List<int>();
        private List<string> FaceBackupSave = new List<string>();
        private List<string> MotionBackupSave = new List<string>();

        private int MaidNum;


        //　表情管理
        private float vStateHoldTime;
        private int vStateAltTime1;
        private int vStateAltTime2;
        private string sFaceAnimeBackupV = "";
        private string sFaceBlendBackupV = "";

        private float SekimenValue = 0f;


        //　声管理
        private bool bIsVoiceOverridingV = false;            //　音声オーバライド（上書き）を適用中
        private string sLoopVoiceOverridingV = "";           //　音声オーバライド（上書き）を適用している音声ファイル名
        private bool bOverrideInterruptedV = false;          //　音声オーバライド（上書き）を適用したが、スキル変更などにより割りこまれた
        private string sLoopVoiceBackupV = "";               //　音声オーバライド（上書き）を終了した時に、復元再生する音声ファイル名
        private int[] iRandomVoiceBackup = new int[] { -1, -1, -1, -1, -1 };
        //private int iRandomVoiceBackup2 = -1;

        //　興奮度管理
        private int vExciteLevel = 1;                       //　０～３００の興奮度を、１～４の興奮レベルに変換した値
        private double iCurrentExcite = 0;                     //　現在興奮値

        private double vResistDef = 10;                     //　抵抗値の初期値
        private double vResistBase = 0;                    //　抵抗値のベース値

        private double vResistBonus = 0;                    //　抵抗の特別加算値
        private double vResistGet = 0;                      //　現在抵抗値

        private double vBoostDef = 0.5;                     //　感度の初期値
        private double vBoostBase = 0;                      //　感度のベース値
        private double vBoostBonus = 0;                    //　感度の特別加算値
        private double vBoostGet = 0;                       //　現在感度

        private double vMaidStamina = 3000;                 //　スタミナ
        private bool vMaidStun = false;

        private double vJirashi = 0;                    //　焦らし度

        private double clitorisValue1 = 0;              //　クリ勃起値
        private float ChinpoValue1 = 0f;              //　ちんぽ勃起値
        private float SoriValue1 = 0f;                //　ちんぽ反り値

        private double AheValue = 0;                    //　アヘ値
        private double AheValue2 = 0;
        private float fEyePosToSliderMul = 5000f;
        private float fAheDefEye = 0f;
        private bool AheResetFlag = false;
        private bool AheResetFlag2 = false;

        private bool ExciteLock = false;
        private bool OrgasmLock = false;


        private float OlgTime = 0;                          //痙攣時間判定
        private bool OlgFlag = true;                        //痙攣動作フラグ
        private bool OlgFlag2 = false;

        private float ShapeKeyRandomInterval = 0.01f;       // 動作間隔(秒)
        private float ShapeKeyRandomDelta = 0f;             // 前回動作からの経過時間

        private float ShapeKeyWaveValue = 0f;
        private bool ShapeKeyWaveFlag = true;

        private float ShapeKeyIncreaseValue = 0f;
        private float ShapeKeySpeedRate = 60f;              // 60fps を基本倍率とする

        private double vOrgasmValue = 0;                     //　現在絶頂値　100になると絶頂
        private int vOrgasmCount = 0;                       //　絶頂回数
        private int vOrgasmCmb = 0;                         //　連続絶頂回数
        private float vOrgasmHoldTime = 0;                  //　絶頂後のボーナスタイム
        private int OrgasmVoice = 0;                        //　絶頂時音声フラグ
        private float vStateHoldTime2;                      //　バイブ責めの継続時間


        //噴乳・射精処理用変数
        private float BreastWaitTime = 0;  //絶頂から噴乳（胸）までのタイムラグ
        private float BreastTime = 0;      //噴乳（胸）してる時間
        private float BreastStopTime = 0;      //噴乳（胸）が途切れる時間
        private bool BreastFlag = false;       //噴乳（胸）開始フラグ
        private float ShapeKeyBreastValue = 0f;

        private float EnemaWaitTime = 0;  //絶頂から噴乳（尻）までのタイムラグ
        private float EnemaTime = 0;      //噴乳（尻）してる時間
        private float EnemaStopTime = 0;      //噴乳（尻）が途切れる時間
        private bool EnemaFlag = false;       //噴乳（尻）開始フラグ
        private float ShapeKeyEnemaValue = 0f;

        private float ChinpoWaitTime = 0;  //射精のタイムラグ
        private int ChinpoTimes = 0;      //射精の回数
        private bool ChinpoFlag = false;       //射精開始フラグ
        private float MaxChinpoValue = 0f;
        private float ChinpoValue = 0f;
        private float ShapeKeyChinpoValue = 0f;


        private float WaitTime = 0;                         //　シーン15開始時の待機時間用

        //シェイプキーチェック用
        //private bool ClitorisKeyCheck = false ;
        //private bool OrgasmKeyCheck = false ;


        //　Chu-B Lip / VR
        private bool bChuBLip;
        private bool bOculusVR;
        private bool installed = false;

        //　フェラ状態チェック
        private int[] bIsBlowjobing = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private string sLastAnimeFileName = "";
        private string sLastAnimeFileNameOld = "";
        private string[] ZAnimeFileName = new string[20];

        //
        private int vSceneLevel = 0;
        private bool SceneLevelEnable = false;
        private bool bIsYotogiScene;
        private bool maidActive = false;
        //private int vFrameCount = 0;
        //private int vFpsMax = 60;
        //private int vFpsMaxVR = 75;

        //視点変更関連
        private GameObject manHead;
        private Renderer manHeadRen;
        private Transform manBipHead;
        private bool fpsModeEnabled = false; //一人称視点フラグ
        private bool bfpsMode = false;
        private float fieldOfViewBack = 35.0f;
        private bool vacationEnabled = false;



        //移植  モーション変更関連----------------------------------------
        private float vStateHoldTimeM;
        private int vStateAltTimeM;
        private int mcFlag = -1;

        private string MaidMotionBack = "";

        private Regex regZeccyou = new Regex(@"^([jtk]_)?(.*)_[23].*");  // モーション名から基本となる部分を取り出す（不安）
        private Regex regZeccyouBackup = new Regex(@"^(.*\.anm).*");  // たまにモーション名の後についてる「 - Queほにゃらら」を除く（適当）

        private List<string> allFiles = new List<string>();

        private List<List<string>> YotogiList = new List<List<string>>();
        private List<List<string>> YotogiListName = new List<List<string>>();
        private List<string> YotogiListBase = new List<string>();
        private List<List<string>> YotogiListSabun = new List<List<string>>();
        private List<string> YotogiGroup = new List<string>();
        private int YotogiMenu = 0;

        private string[] sZeccyouMaidMotion = new string[] { "_zeccyou_f_once_", "_seikantaizeccyou_f_once_", "_shasei_naka_f_once_" };
        private string[] sZeccyouManMotion = new string[] { "_zeccyou_m_once_", "_seikantaizeccyou_m_once_", "_shasei_naka_m_once_" };

        private string sZeccyouMaidMotion2 = "_zeccyou_f2_once_";
        private string sZeccyouMaidMotion3 = "_zeccyou_f3_once_";
        private string[] sHighExciteMaidMotion = new string[] { "_3_f", "_3a01_f", "_3b01_f", "_3b02_f" };
        //private string sHighExciteManMotion = "_3_m";

        //　バイブ弱時のモーションリスト
        private string[][] MotionList20 = new string[][] {
          new string[] { "rosyutu_omocya_1_f.anm" , "rosyutu_tati_vibe_onani_1_f.anm" }, //立ち
          new string[] { "settai_vibe_in_kaiwa_jaku_a01_f.anm" , "settai_vibe_in_kaiwa_jaku_f.anm" }, //椅子座り
          new string[] { "soji_zoukin_vibe.anm" }, //雑巾
          new string[] { "work_kyuuzi_vibe_b01.anm" , "work_kyuuzi_vibe_b02.anm" }, //給仕
          new string[] { "fukisouji1_vibe.anm" }, //拭き掃除
          new string[] { "soji_mop_vibe.anm" }, //モップ
          new string[] { "tati_kiss_loop_f.anm" }, //立ちキス
          new string[] { "k_aibu_kiss_2_f.anm" } //椅子座りキス
        };

        //　バイブ強時のモーションリスト
        private string[][] MotionList30 = new string[][] {
          new string[] { "rosyutu_omocya_2_f.anm" , "rosyutu_omocya_3_f.anm" , "rosyutu_tati_vibe_onani_2_f.anm" }, //立ち
          new string[] { "settai_vibe_in_kaiwa_kyou_a01_f.anm" , "settai_vibe_in_kaiwa_kyou_f.anm" }, //椅子座り
          new string[] { "soji_zoukin_vibe_a01.anm" }, //雑巾
          new string[] { "work_kyuuzi_vibe_a01.anm" }, //給仕
          new string[] { "fukisouji1_vibe_a01.anm" }, //拭き掃除
          new string[] { "soji_mop_vibe_a01.anm" }, //モップ
          new string[] { "tati_kiss_loop_f.anm" }, //立ちキス
          new string[] { "k_aibu_kiss_3_f.anm" } //椅子座りキス
        };

        //　バイブ停止時のモーションリスト
        private string[][] MotionList40 = new string[][] {
          new string[] { "rosyutu_omocya_taiki_f.anm" }, //立ち
          new string[] { "settai_vibe_in_taiki_f.anm" },  //椅子座り
          new string[] { "maid_orz.anm" }, //雑巾
          new string[] { "work_kyuuzi_vibe.anm" }, //給仕
          new string[] { "fukisouji1_vibe.anm" }, //拭き掃除
          new string[] { "soji_mop_vibe.anm" }, //モップ
          new string[] { "tati_kiss_taiki_f.anm" }, //立ちキス
          new string[] { "k_aibu_kiss_1_f.anm" } //椅子座りキス
        };

        //チェック用モーションリスト
        private List<string> MotionList_tati = new List<string>();
        private List<string> MotionList_suwari = new List<string>();
        private List<string> MotionList_zoukin = new List<string>();
        private List<string> MotionList_kyuuzi = new List<string>();
        private List<string> MotionList_fukisouji = new List<string>();
        private List<string> MotionList_mop = new List<string>();

        private List<string> MotionList_vibe = new List<string>();


        //移植終了  モーション変更関連----------------------------------------






        //ステータス表示用
        private string[] SucoreText1 = new string[] { "☆ ☆ ☆", "★ ☆ ☆", "★ ★ ☆", "★ ★ ★" };
        private string[] SucoreText2 = new string[] { "☆ ☆ ☆", "★ ☆ ☆", "★ ★ ☆", "★ ★ ★" };
        private string[] SucoreText3 = new string[] { "☆ ☆ ☆", "★ ☆ ☆", "★ ★ ☆", "★ ★ ★" };


        //夜伽EDIT関連
        private int[] Edit_MaidsNum = new int[] { -1, -1, -1, -1 };
        private int Edit_MaidSet = 0;
        private int Edit_SubMenu = 0;
        private List<string> xmlFilesY = new List<string>();
        private List<string> xmlFilesV = new List<string>();

        private string Edit_YotogiNameT = "";
        private string[] Edit_MaidMotionT = new string[] { "", "", "", "" };

        private bool Edit_Overwrite = false;

        public class YotogiEdit_Xml
        {

            public string Edit_YotogiName = "";
            public bool Edit_RankoEnabled = false;

            public bool[] Edit_MaidEnabled = new bool[] { false, false, false, false };
            public string[] Edit_MaidMotion = new string[] { "", "", "", "" };
            public string[] Edit_ManMotion = new string[] { "", "", "", "" };

            public bool[] Edit_ManEnabled = new bool[] { false, false, false, false };
            public int[] Edit_MansTg = new int[] { -1, -1, -1, -1 };

            public string[] Edit_VoiceSet = new string[] { "", "", "", "" };
            public float[] Edit_VsInterval = new float[] { 500f, 500f, 500f, 500f };

            public float[][] Edit_MaidPos = new float[][] {
            new float[] { 0 , 0 , 0 },
            new float[] { 0 , 0 , 0 },
            new float[] { 0 , 0 , 0 },
            new float[] { 0 , 0 , 0 }
          };
            public float[][] Edit_MaidEul = new float[][] {
            new float[] { 0 , 0 , 0 },
            new float[] { 0 , 0 , 0 },
            new float[] { 0 , 0 , 0 },
            new float[] { 0 , 0 , 0 }
          };
            public float[][] Edit_ManPos = new float[][] {
            new float[] { 0 , 0 , 0 },
            new float[] { 0 , 0 , 0 },
            new float[] { 0 , 0 , 0 },
            new float[] { 0 , 0 , 0 }
          };
            public float[][] Edit_ManEul = new float[][] {
            new float[] { 0 , 0 , 0 },
            new float[] { 0 , 0 , 0 },
            new float[] { 0 , 0 , 0 },
            new float[] { 0 , 0 , 0 }
          };

        }



        //ボイスセット関連
        private bool vs_Overwrite = false;
        private int vsErrer = 0;
        private string[] vsErrerText = new string[] { "", "ボイスセット名が空白のため保存できません", "上書きする場合は『上書／ｸﾘｱ』にチェックを入れて下さい", "クリアする場合は『上書／ｸﾘｱ』にチェックを入れて下さい" };
        private string[][] personalList = new string[][] {
        new string[]{ "純真" , "ツンデレ" , "クーデレ" , "ヤンデレ" , "姉ちゃん" , "僕っ娘" , "ドＳ" , "凜デレ" , "真面目" , "無垢" , "文学少女" , "小悪魔" , "おしとやか" , "メイド秘書" , "ふわふわ妹" , "指定無" },
        new string[]{ "Pure" , "Pride" , "Cool" , "Yandere" , "Anesan" , "Genki" , "Sadist" , "Rindere" , "Majime" , "Muku" , "Silent" , "Devilish" , "Ladylike" , "Secretary" , "Sister" , ""}
        };
        private List<int> iPersonal = new List<int>();
        private string[] vsState = new string[] { "弱", "強", "指定無" };
        private string[] vsCondition = new string[] { "通常", "キス", "フェラ", "絶頂後", "放心", "指定無" };
        private string[] vsLevel = new string[] { "0", "1", "2", "3", "－" };

        private string editVoiceSetName = "";
        private List<string[]> editVoiceSet = new List<string[]>{
          new string[] { "" , "0" , "0" , "3" , "0" , "3" , "0" , "0" } //ファイル名、性格、興奮低、興奮高、絶頂低、絶頂高、強度、メイド状態
        };

        private int[] fVoiceSet = new int[] { 14, 4, 4, 2, 5 };			//配列の終端値 personalList[0]、vsLevel、vsLevel、vsState、vsCondition

        private float[] vsInterval = new float[] { 500f, 500f, 500f, 500f };

        private string[] madiVoiceSetName = new string[] { "", "", "", "" };
        private List<string[]> madiVoiceSet0 = new List<string[]>{
          new string[] { "" , "0" , "0" , "3" , "0" , "3" , "0" , "0"  }
        };
        private List<string[]> madiVoiceSet1 = new List<string[]>{
          new string[] { "" , "0" , "0" , "3" , "0" , "3" , "0" , "0"  }
        };
        private List<string[]> madiVoiceSet2 = new List<string[]>{
          new string[] { "" , "0" , "0" , "3" , "0" , "3" , "0" , "0"  }
        };
        private List<string[]> madiVoiceSet3 = new List<string[]>{
          new string[] { "" , "0" , "0" , "3" , "0" , "3" , "0" , "0"  }
        };

        public class VoiceSet_Xml
        {
            public string saveVoiceSetName = "";
            public List<string[]> saveVoiceSet = new List<string[]>();

        }

        private bool scKeyOff = false;
        private bool vsSettei = false;

        private List<List<string>> unzipVoiceSet = new List<List<string>>{
          new List<string>(),
          new List<string>()
        };





        public void Awake()
        {
            GameObject.DontDestroyOnLoad(this);

            string path = UnityEngine.Application.dataPath;

            // Iniファイル読み出し
            cfg = ReadConfig<VibeYourMaidConfig>("Config");
            cfgw = ReadConfig<VibeYourMaidCfgWriting>("CfgWriting");

            // Iniファイル書き出し
            SaveConfig(cfg, "Config");
            SaveConfig(cfgw, "CfgWriting");

            //UNZIPボイスセットCSVの読み込み 未完成
            //CsvReadUvs();

            // ChuBLip判別
            bChuBLip = path.Contains("COM3D2OHx64") || path.Contains("COM3D2OHx86") || path.Contains("COM3D2OHVRx64");

            // VR判別
            bOculusVR = path.Contains("COM3D2OHVRx64") || path.Contains("COM3D2VRx64") || Environment.CommandLine.ToLower().Contains("/vr");

            #region 

            // COM3D2のモーションファイル全列挙
            Console.WriteLine("モーションファイル読み込み開始");

            string[] Files = GameUty.FileSystem.GetList("", AFileSystemBase.ListType.AllFile);

            foreach (string file in Files)
            {
                if (Path.GetExtension(file) == ".anm")
                {
                    allFiles.Add(Path.GetFileNameWithoutExtension(file));


                }
            }
            //ファイル名でソート
            allFiles.Sort();

            Console.WriteLine("モーションファイル読み込み終了");

          

            // 読み込んだモーションファイルの中からモーション変更可能なものを抽出
            Console.WriteLine("夜伽リスト抽出開始");
            string m2 = "";
            string m3 = "";

            List<string> YotogiList0 = new List<string>();
            List<string> YotogiList1 = new List<string>();
            List<string> YotogiList2 = new List<string>();
            List<string> YotogiList3 = new List<string>();
            List<string> YotogiList4 = new List<string>();
            List<string> YotogiList5 = new List<string>();
            List<string> YotogiList6 = new List<string>();
            List<string> YotogiList7 = new List<string>();
            List<string> YotogiList8 = new List<string>();

            List<string> YotogiListName0 = new List<string>();
            List<string> YotogiListName1 = new List<string>();
            List<string> YotogiListName2 = new List<string>();
            List<string> YotogiListName3 = new List<string>();
            List<string> YotogiListName4 = new List<string>();
            List<string> YotogiListName5 = new List<string>();
            List<string> YotogiListName6 = new List<string>();
            List<string> YotogiListName7 = new List<string>();
            List<string> YotogiListName8 = new List<string>();

            List<string> _YotogiListSabun = new List<string>();


            foreach (string file in allFiles)
            {

                if (Regex.IsMatch(file, "_[123]") && ((Regex.IsMatch(file, @"^[a-zA-Z]_") && !Regex.IsMatch(file, "m_")) || Regex.IsMatch(file, @"[a-zA-Z][0-9][0-9]")) && (file.EndsWith("_f") || file.EndsWith("_f2") || file.EndsWith("_f3")))
                {

                    string basefile = file + ".anm";
                    if (!Regex.IsMatch(basefile, "m_")) basefile = Regex.Replace(basefile, @"^[a-zA-Z]_", "");
                    basefile = Regex.Replace(basefile, @"[a-zA-Z][0-9][0-9]", "");

                    int i = YotogiListBase.IndexOf(basefile);
                    if (i < 0)
                    {
                        YotogiListBase.Add(basefile);
                        _YotogiListSabun.Add(basefile);
                        _YotogiListSabun.Add(file + ".anm");
                    }
                    else
                    {
                        _YotogiListSabun.Add(file + ".anm");
                    }

                }

                if (file.Contains("_1") && (!Regex.IsMatch(file, @"^[a-zA-Z]_") || Regex.IsMatch(file, "x_manguri") || Regex.IsMatch(file, "m_")) && !Regex.IsMatch(file, @"[a-zA-Z][0-9][0-9]") && (file.EndsWith("_f") || file.EndsWith("_f2") || file.EndsWith("_f3")))
                {
                    m2 = file.Replace("_1", "_2");
                    m3 = file.Replace("_1", "_3");

                    if (allFiles.Contains(m2) && allFiles.Contains(m3))
                    {

                        string name = file.Replace("seijyoui", "正常位").Replace("sexsofa", "ソファ").Replace("aibu", "愛撫").Replace("kyousitu", "教室").Replace("poseizi", "ポーズ維持").Replace("vibe", "バイブ").Replace("yorisoi", "寄添い")
                        .Replace("settai", "接待").Replace("turusi", "吊し").Replace("hasamikomi", "挟み込み").Replace("kaiwa", "会話").Replace("ryoutenaburi", "両手")
                        .Replace("manguri", "まんぐり").Replace("ritui", "立位").Replace("taimen", "対面").Replace("kijyoui", "騎乗位").Replace("haimen", "背面").Replace("tikan", "痴漢").Replace("ekiben", "駅弁")
                        .Replace("kouhaii", "後背位").Replace("kubisime", "首絞め").Replace("sokui", "側位").Replace("sukebeisu", "スケベ椅子").Replace("udemoti", "腕持ち").Replace("utubuse", "俯せ").Replace("onani", "オナニー")
                        .Replace("tekoki", "手コキ").Replace("toilet", "トイレ").Replace("zikkyou", "実況").Replace("asiname", "足舐め").Replace("fera", "フェラ").Replace("arai", "洗い").Replace("paizuri", "パイズリ")
                        .Replace("siriname", "尻舐め").Replace("tinguri", "チングリ").Replace("siriyubi", "尻指").Replace("umanori", "馬乗り").Replace("harituke", "磔").Replace("kousokudai", "拘束台").Replace("nonosiru", "罵り")
                        .Replace("maeyubi", "前指").Replace("kousoku", "拘束").Replace("mokuba", "木馬").Replace("harem", "ハーレム").Replace("housi", "奉仕").Replace("naburu", "嬲る").Replace("ran3p", "乱交3P")
                        .Replace("2ana", "二穴").Replace("onedari", "おねだり").Replace("ran4p", "乱交4P").Replace("ganmen", "顔面").Replace("omocya", "玩具").Replace("sixnine", "シックスナイン").Replace("asikoki", "足コキ")
                        .Replace("tekoona", "手コキオナニー").Replace("rosyutu", "露出").Replace("yuri", "百合").Replace("kiss", "キス").Replace("kaiawase", "貝合せ").Replace("kunni", "クンニ").Replace("momi", "揉み")
                        .Replace("soutou", "双頭").Replace("cli", "クリ").Replace("hibu", "秘部").Replace("kuti", "くち").Replace("sex", "SEX").Replace("muri_3p", "無理矢理3P").Replace("muri_6p", "無理矢理6P").Replace("daki", "抱き")
                        .Replace("tikubi", "乳首").Replace("itya", "ｲﾁｬｲﾁｬ").Replace("name", "舐め").Replace("daijyou", "台上").Replace("kakae", "抱え").Replace("sumata", "素股").Replace("osae", "押え")
                        .Replace("hold", "ホールド").Replace("fusagu", "塞ぎ").Replace("zai", "座位").Replace("siriage", "尻上げ").Replace("siri", "尻").Replace("tati", "立ち").Replace("hiraki", "開き")
                        .Replace("ubi", "指").Replace("mune", "胸").Replace("isu", "椅子").Replace("ude", "腕").Replace("inu", "犬").Replace("iziri", "弄り").Replace("ir", "ｲﾗﾏﾁｵ").Replace("uma", "馬乗").Replace("gr", "ｸﾞﾗｲﾝﾄﾞ")
                        .Replace("pai", "ﾊﾟｲｽﾞﾘ").Replace("hekimen", "壁面").Replace("dildo", "ディルド").Replace("mzi", "M字").Replace("kyou", "強").Replace("peace", "ピース").Replace("self", "セルフ").Replace("oku", "最奥")
                        .Replace("mp", "MP").Replace("le", "左").Replace("ri", "右").Replace("_1_f", "").Replace("_f", "").Replace("x_", "").Replace("m_", "M豚").Replace("matuba", "松葉崩し");


                        if (file.Contains("ganmenkijyoui"))
                        {//８：その他
                            YotogiList8.Add(file);
                            YotogiListName8.Add(name);
                            Console.WriteLine(file);

                        }
                        else if (file.Contains("yuri"))
                        {//６：百合
                            YotogiList6.Add(file);
                            YotogiListName6.Add(name);
                            Console.WriteLine(file);

                        }
                        else if (file.Contains("ran") || file.Contains("harem") || file.Contains("3p_") || file.Contains("6p_"))
                        {//７：複数プレイ
                            YotogiList7.Add(file);
                            YotogiListName7.Add(name);
                            Console.WriteLine(file);

                        }
                        else if (file.Contains("kousoku") || file.Contains("mokuba") || file.Contains("harituke"))
                        {//５：ＳＭ
                            YotogiList5.Add(file);
                            YotogiListName5.Add(name);
                            Console.WriteLine(file);

                        }
                        else if (file.Contains("onani"))
                        {//３：オナニー
                            YotogiList3.Add(file);
                            YotogiListName3.Add(name);
                            Console.WriteLine(file);

                        }
                        else if (file.Contains("aibu") || file.Contains("poseizi") || file.Contains("vibe") || file.Contains("kunni"))
                        {//０：愛撫
                            YotogiList0.Add(file);
                            YotogiListName0.Add(name);
                            Console.WriteLine(file);

                        }
                        else if (file.Contains("fera") || file.Contains("paizuri") || file.Contains("tekoki") || file.Contains("arai") || file.Contains("asiname"))
                        {//４：奉仕
                            YotogiList4.Add(file);
                            YotogiListName4.Add(name);
                            Console.WriteLine(file);

                        }
                        else if (file.Contains("haimen") || file.Contains("kouhaii") || file.Contains("sokui") || file.Contains("tikan_sex") || file.Contains("sukebeisu_sex") || file.Contains("kakaemzi"))
                        {//２：挿入 後
                            YotogiList2.Add(file);
                            YotogiListName2.Add(name);
                            Console.WriteLine(file);

                        }
                        else if (file.Contains("sex") || file.Contains("manguri") || file.Contains("ritui") || file.Contains("seijyoui") || file.Contains("kijyoui") || file.Contains("taimenzai") || file.Contains("ekiben"))
                        {//１：挿入 前
                            YotogiList1.Add(file);
                            YotogiListName1.Add(name);
                            Console.WriteLine(file);

                        }
                        else
                        {//８：その他
                            YotogiList8.Add(file);
                            YotogiListName8.Add(name);
                            Console.WriteLine(file);

                        }

                        //if(unzipVoiceSet[0].IndexOf(name) == -1){
                        //  unzipVoiceSet[0].Add(name);
                        //  unzipVoiceSet[1].Add("");
                        //}

                    }
                }

            }

            //CsvSaveUvs();

            YotogiList.Add(YotogiList0);
            YotogiList.Add(YotogiList1);
            YotogiList.Add(YotogiList2);
            YotogiList.Add(YotogiList3);
            YotogiList.Add(YotogiList4);
            YotogiList.Add(YotogiList5);
            YotogiList.Add(YotogiList6);
            YotogiList.Add(YotogiList7);
            YotogiList.Add(YotogiList8);

            YotogiListName.Add(YotogiListName0);
            YotogiListName.Add(YotogiListName1);
            YotogiListName.Add(YotogiListName2);
            YotogiListName.Add(YotogiListName3);
            YotogiListName.Add(YotogiListName4);
            YotogiListName.Add(YotogiListName5);
            YotogiListName.Add(YotogiListName6);
            YotogiListName.Add(YotogiListName7);
            YotogiListName.Add(YotogiListName8);

            YotogiGroup.Add("愛撫");
            YotogiGroup.Add("挿入 前");
            YotogiGroup.Add("挿入 後");
            YotogiGroup.Add("オナニー");
            YotogiGroup.Add("奉仕");
            YotogiGroup.Add("ＳＭ");
            YotogiGroup.Add("百合");
            YotogiGroup.Add("複数");
            YotogiGroup.Add("その他");
            YotogiGroup.Add("ランダム");


            //差分ファイル振り分け
            foreach (string file in YotogiListBase)
            {
                List<string> list = new List<string>();

                foreach (string sabun in _YotogiListSabun)
                {

                    string basefile = sabun;
                    if (!Regex.IsMatch(basefile, "m_")) basefile = Regex.Replace(basefile, @"^[a-zA-Z]_", "");
                    basefile = Regex.Replace(basefile, @"[a-zA-Z][0-9][0-9]", "");

                    if (file == basefile)
                    {
                        list.Add(sabun);
                        //Console.WriteLine(sabun + "　→　" + file);
                    }
                }

                YotogiListSabun.Add(list);
            }



            Console.WriteLine("夜伽リスト抽出終了");

#endregion

            // チェック用モーションリストの読み込み
            //立ちモーション
            MotionList_tati = ReadTextFaile(@"Sybaris\UnityInjector\Config\VibeYourMaid_MList.txt", "tati_list");

            //座りモーション
            MotionList_suwari = ReadTextFaile(@"Sybaris\UnityInjector\Config\VibeYourMaid_MList.txt", "suwari_list");

            //雑巾がけモーション
            MotionList_zoukin = ReadTextFaile(@"Sybaris\UnityInjector\Config\VibeYourMaid_MList.txt", "zoukin_list");

            //給仕モーション
            MotionList_kyuuzi = ReadTextFaile(@"Sybaris\UnityInjector\Config\VibeYourMaid_MList.txt", "kyuuzi_list");

            //拭き掃除モーション
            MotionList_fukisouji = ReadTextFaile(@"Sybaris\UnityInjector\Config\VibeYourMaid_MList.txt", "fukisouji_list");

            //モップ掛けモーション
            MotionList_mop = ReadTextFaile(@"Sybaris\UnityInjector\Config\VibeYourMaid_MList.txt", "mop_list");



            //　チェック様にバイブモーションをひとまとめにする

            Console.WriteLine("バイブモーションリスト結合開始");
            foreach (string[] x in MotionList20)
            {
                foreach (string xx in x)
                {
                    MotionList_vibe.Add(xx);
                }
            }
            foreach (string[] x in MotionList30)
            {
                foreach (string xx in x)
                {
                    MotionList_vibe.Add(xx);
                }
            }
            foreach (string[] x in MotionList40)
            {
                foreach (string xx in x)
                {
                    MotionList_vibe.Add(xx);
                }
            }
            //foreach (string x in MotionList_vibe) {
            //  Console.Write(x + "　");
            //}
            Console.WriteLine("バイブモーションリスト結合終了");


            //XML・CSV保存フォルダの情報をチェックする
            XmlFilesCheck();
            CsvFilesCheck();

            //ランダム用モーションリストに取り敢えずセット
            if (csvFilesR.Count > 0) CsvRead(csvFilesR[0]);


        }
        VibeYourMaidConfig cfg;
        VibeYourMaidCfgWriting cfgw;

        //XMLファイル用のインスタンス作成
        YotogiEdit_Xml YEX = new YotogiEdit_Xml();
        VoiceSet_Xml VSX = new VoiceSet_Xml();




        public void Start()
        {

            cm = GameMain.Instance.CharacterMgr;

        }

        public void OnDestroy()
        {

        }



        void OnLevelWasLoaded(int level)
        {

            //　レベルの取得
            vSceneLevel = level;

            SceneLevelEnable = false;

            // 夜伽シーンに有るかチェック
            checkYotogiScene();

            //有効シーンにある場合プラグインを有効化
            if (0 <= Array.IndexOf(cfg.SceneList, vSceneLevel)) SceneLevelEnable = true;
            if (bIsYotogiScene) SceneLevelEnable = true;

            //バケーションモードの切替チェック
            if (vSceneLevel == 43 && !vacationEnabled) vacationEnabled = true;
            if (vSceneLevel == 3 && vacationEnabled) vacationEnabled = false;


            if (vStateMajor != 10)
            {
                GameMain.Instance.SoundMgr.StopSe();
            }

            //各変数の初期化
            maid = null;
            SubMaids = new Maid[20];
            maidActive = false;
            MaidDataClear();

            MansTg = new int[] { 0, 0, 0, 0 };

            Edit_MaidsNum = new int[] { -1, -1, -1, -1 };
            Edit_YotogiNameT = "";
            Edit_MaidMotionT = new string[] { "", "", "", "" };
            YEX.Edit_YotogiName = "";
            YEX.Edit_MaidMotion = new string[] { "", "", "", "" };
            YEX.Edit_ManMotion = new string[] { "", "", "", "" };
            YEX.Edit_MansTg = new int[] { -1, -1, -1, -1 };
            YEX.Edit_VoiceSet = new string[] { "", "", "", "" };
            YEX.Edit_VsInterval = new float[] { 500f, 500f, 500f, 500f };

            StartFlag = false;

            VLevel = 0;
            vStateMajor = 10;
            vStateMajorOld = 10;
            iCurrentExcite = 0;
            vResistBase = vResistDef;
            vResistGet = 0;
            vOrgasmValue = 0;
            vOrgasmCmb = 0;
            OrgasmVoice = 0;
            clitorisValue1 = 0;
            AheValue = 0;
            AheValue2 = 0;
            EnemaFlag = false;
            AheResetFlag = false;
            AheResetFlag2 = false;

            vMaidStamina = 3000;
            vMaidStun = false;

            SioFlag = false;
            SioFlag2 = false;
            SioTime = 0;
            SioTime2 = 0;

            WaitTime = 0;
            ShapeKeyWaveValue = 0f;
            ShapeKeyWaveFlag = true;
            ShapeKeyIncreaseValue = 0f;
            ShapeKeyRandomDelta = 0f;

            autoSelect = 0;
            autoTime1 = 0;
            autoTime2 = 0;
            autoTime3 = 0;
            autoTime4 = 0;
            amlBack = new string[] { "0", "0", "0" };

            SaveFlag = false;

            cameraCheck = new bool[] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };

            if (vSceneLevel == 16)
            {
                vBoostBaseSave.Clear();
                vOrgasmCountSave.Clear();
                VLevelSave.Clear();
                FaceBackupSave.Clear();
                MotionBackupSave.Clear();
                MaidNameSave.Clear();

            }


            //　メインカメラの取得
            mainCamera = GameMain.Instance.MainCamera;

            //　メイドさんの取得
            maid = GameMain.Instance.CharacterMgr.GetMaid(0);

            // ご主人様の取得
            man = GameMain.Instance.CharacterMgr.GetMan(0);

            //乱交用の男モデル取得
            for (int i = 0; i < 4; i++)
            {
                SubMans[i] = GameMain.Instance.CharacterMgr.GetMan(i);
            }
            //男Body取得のため、一度呼び出しておく
            if (vSceneLevel == 15 && MansFGet < 2)
            {
                ++MansFGet;
                for (int i = 0; i < 4; i++)
                {
                    SubMans[i].Visible = true;
                    SubMans[i].transform.position = new Vector3(0f, -10f, 0f);
                }
            }


            if (maid) { maidActive = maid.gameObject.activeInHierarchy; }

            if (maidActive)
            {

                //シェイプキーチェック
                //ClitorisKeyCheck = maid.body0.goSlot[0].morph.hash.ContainsKey("clitoris");
                //OrgasmKeyCheck = maid.body0.goSlot[0].morph.hash.ContainsKey("orgasm");

                updateShapeKeyOrgasmValue(0f);
                updateShapeKeyEnemaValue(0f, 0f);
                updateShapeKeyBreastValue(0f, 0f);
                updateShapeKeyChinpoValue(0f, 0f);

                //Transform[] objList = maid.transform.GetComponentsInChildren<Transform>();
                //if(objList.Count() == 0) {
                //
                //} else {
                //  foreach(var gameobject in objList) {
                //    if(gameobject.name == "Bone_Face") {
                //      maidHead = gameobject;
                //    }
                //  }
                //}

                //　メイドの状態セーブ処理
                //string MaidName = maid.Param.status.last_name + " " + maid.Param.status.first_name;
                string MaidName = maid.status.lastName + " " + maid.status.firstName;
                Console.WriteLine("A:" + MaidName);

                MaidNum = MaidNameSave.IndexOf(MaidName);
                Console.WriteLine("B:" + MaidNum);

                if (MaidNum == -1)
                {
                    MaidNameSave.Add(MaidName);
                    vBoostBaseSave.Add(vBoostDef);
                    vOrgasmCountSave.Add(0);
                    VLevelSave.Add(0);
                    FaceBackupSave.Add("");
                    MotionBackupSave.Add("");

                    MaidNum = MaidNameSave.IndexOf(MaidName);

                    //絶頂回数・感度を初期化
                    vBoostBase = vBoostDef;
                    vOrgasmCount = 0;

                    Console.WriteLine("C:" + MaidNum);
                    Console.WriteLine("D:" + MaidNameSave[MaidNum]);

                }
                else
                {
                    vBoostBase = vBoostBaseSave[MaidNum];
                    vOrgasmCount = vOrgasmCountSave[MaidNum];

                    Console.WriteLine("感度 / 絶頂数：" + vBoostBase + " / " + vOrgasmCount);
                }
                SaveFlag = true;

            }

            //　設定ファイルの読込・初期化
            //  loadConfigIni();

            //　各バックアップ値の初期化
            sFaceAnimeBackupV = "";
            sFaceBlendBackupV = "";
            sLoopVoiceBackupV = "";


            /*if (!installed){
              // GripMovePlugin の DirectTouchTool フレームワークへの自身の登録
              DirectTouchTool.Instance.RegisterEventHandlerProvider(this);
              installed = true;
            }*/


        }

        Rect node = new Rect(UnityEngine.Screen.width - 250, UnityEngine.Screen.height - 370, 220, 220);
        Rect node2 = new Rect(UnityEngine.Screen.width - 650, UnityEngine.Screen.height - 685, 620, 315);
        Rect node3 = new Rect(UnityEngine.Screen.width - 650, UnityEngine.Screen.height - 370, 400, 220);
        Rect node4 = new Rect(UnityEngine.Screen.width - 1050, UnityEngine.Screen.height - 685, 400, 535);

        private int vsGui = 0;
        private int vsTg;

        void OnGUI()
        {

            if (maid) { maidActive = maid.gameObject.activeInHierarchy; }

            if (cfg.bPluginEnabledV && maid && maidActive && cfg.GuiFlag > 0)
            {


                if (SceneLevelEnable)
                {

                    if (vSceneLevel == 15 && WaitTime < 120)
                    {

                        WaitTime += Time.deltaTime * 60;

                    }
                    else
                    {

                        if (cfg.GuiFlag == 1)
                        {
                            node.height = 220;
                        }
                        else if (cfg.GuiFlag == 2)
                        {
                            node.height = 20;
                        }

                        GUIStyle gsWin = new GUIStyle("box");
                        gsWin.fontSize = 11;
                        gsWin.alignment = TextAnchor.UpperLeft;

                        GUIStyle gsWin2 = new GUIStyle("box");
                        gsWin2.fontSize = 12;
                        gsWin2.alignment = TextAnchor.UpperLeft;

                        node = GUI.Window(1, node, WindowCallback, "リモコンスイッチ+API 1.0.3.0-2", gsWin);    //@API実装

                        if (cfg.GuiFlag2)
                        {
                            node2 = GUI.Window(2, node2, WindowCallback2, "VibeYourMaid 設定画面", gsWin2);
                        }

                        if (cfg.GuiFlag3)
                        {
                            node3 = GUI.Window(3, node3, WindowCallback3, "ムラムラしたのでメイドさんを押し倒す", gsWin2);
                        }

                        if (vsGui > 0)
                        {
                            if (vsTg == 0)
                            {
                                node4 = GUI.Window(4, node4, WindowCallback4, "ボイスセット編集画面【メインメイド】", gsWin2);
                            }
                            else
                            {
                                node4 = GUI.Window(4, node4, WindowCallback4, "ボイスセット編集画面【サブメイド" + vsTg + "】", gsWin2);
                            }
                        }


                    }

                }
            }


#if DEBUGjkk
                //GUIへのデバッグ表示
                if (maid){
                    GUI.Label(new Rect(0, 0, 900, 30),
                                  string.Format("Level={0}, Yotogi={1}, vState={2}, fDistanceToMaidFace={3}, sLastAnimeFileName={4} ",
                                                 vSceneLevel, bIsYotogiScene, vState, fDistanceToMaidFace, sLastAnimeFileName));

                    if (maid.AudioMan != null && maid.AudioMan.FileName != null) {
                        GUI.Label(new Rect(0, 30, 900, 60),
                                      string.Format("AudioClip={0}, IsLoop={1}, AudioTime={2} of {3}, ActiveFace={4}, ExciteLevel={5}",
                                                     maid.AudioMan.FileName, maid.AudioMan.audiosource.loop, maid.AudioMan.audiosource.time, maid.AudioMan.audiosource.clip.length, maid.ActiveFace, vExciteLevel));

                        GUI.Label(new Rect(0, 60, 900, 90),
                                      string.Format("bIsVoiceOverridingV={0}, sLoopVoiceBackupV={1}, vStateAltTime1={2}, vStateAltTime2={3}",
                                                    bIsVoiceOverridingV, sLoopVoiceBackupV, vStateAltTime1, vStateAltTime2));
                    }
                }
#endif
        }

        void WindowCallback(int id)
        {

            GUIStyle gsLabel = new GUIStyle("label");
            gsLabel.fontSize = 12;
            gsLabel.alignment = TextAnchor.MiddleLeft;

            GUIStyle gsButton = new GUIStyle("button");
            gsButton.fontSize = 12;
            gsButton.alignment = TextAnchor.MiddleCenter;

            GUIStyle gsButton2 = new GUIStyle("button");
            gsButton2.fontSize = 10;
            gsButton2.alignment = TextAnchor.MiddleCenter;



            //現在ステータス表示
            int SucoreLevel2 = 0;
            if (Math.Floor(vOrgasmValue) < 30)
            {
                SucoreLevel2 = 0;
            }
            else if (Math.Floor(vOrgasmValue) < 50)
            {
                SucoreLevel2 = 1;
            }
            else if (Math.Floor(vOrgasmValue) < 80)
            {
                SucoreLevel2 = 2;
            }
            else
            {
                SucoreLevel2 = 3;
            }

            int SucoreLevel3 = 0;
            if (Math.Floor(vResistGet) < 6)
            {
                SucoreLevel3 = 0;
            }
            else if (Math.Floor(vResistGet) < 13)
            {
                SucoreLevel3 = 1;
            }
            else if (Math.Floor(vResistGet) < 20)
            {
                SucoreLevel3 = 2;
            }
            else
            {
                SucoreLevel3 = 3;
            }

            if (GUI.Button(new Rect(200, 0, 20, 20), "x", gsButton))
            {
                cfg.GuiFlag = 0;
                Console.WriteLine("GUI非表示");
            }

            if (cfg.GuiFlag == 1)
            {
                if (GUI.Button(new Rect(180, 0, 20, 20), "－", gsButton))
                {
                    cfg.GuiFlag = 2;
                    Console.WriteLine("GUI最小化");
                }


                int bMaid;
                if (GUI.Button(new Rect(5, 25, 30, 20), "<", gsButton))
                {
                    bMaid = iCurrentMaid;
                    GetMaidCount();

                    if (maidDataList.Count > 1)
                    {
                        VibeDataClear(1);

                        --iCurrentMaid;
                        if (iCurrentMaid < 0) { iCurrentMaid = maidDataList.Count - 1; }
                        bReGetMaid = true;

                    }
                    if (bMaid != iCurrentMaid && cfgw.CamChangeEnabled) CameraChange(SubMaids[maidDataList[iCurrentMaid]], SubMaids[maidDataList[bMaid]]);

                }

                if (GUI.Button(new Rect(40, 25, 30, 20), ">", gsButton))
                {
                    bMaid = iCurrentMaid;
                    GetMaidCount();

                    if (maidDataList.Count > 1)
                    {
                        VibeDataClear(1);

                        ++iCurrentMaid;
                        if (iCurrentMaid > maidDataList.Count - 1) { iCurrentMaid = 0; }
                        bReGetMaid = true;
                    }
                    if (bMaid != iCurrentMaid && cfgw.CamChangeEnabled) CameraChange(SubMaids[maidDataList[iCurrentMaid]], SubMaids[maidDataList[bMaid]]);
                }

                string MaidName = maid.status.lastName + " " + maid.status.firstName;
                GUI.Label(new Rect(75, 25, 140, 20), MaidName, gsLabel);



                if (GUI.Button(new Rect(150, 50, 60, 20), "設定", gsButton))
                {
                    cfg.GuiFlag2 = !cfg.GuiFlag2;
                }


                if (GUI.Button(new Rect(150, 75, 60, 20), "UNZIP!", gsButton))
                {
                    cfg.GuiFlag3 = !cfg.GuiFlag3;
                }


                if (!ExciteLock)
                {
                    GUI.Label(new Rect(5, 50, 190, 20), "【 快 感 】 " + SucoreText1[vExciteLevel - 1], gsLabel);
                }
                else
                {
                    GUI.Label(new Rect(5, 50, 190, 20), "【 Lock 】 " + SucoreText1[vExciteLevel - 1], gsLabel);
                }

                if (!OrgasmLock)
                {
                    GUI.Label(new Rect(5, 70, 190, 20), "【 絶 頂 】 " + SucoreText2[SucoreLevel2], gsLabel);
                }
                else
                {
                    GUI.Label(new Rect(5, 70, 190, 20), "【 Lock 】 " + SucoreText2[SucoreLevel2], gsLabel);
                }

                GUI.Label(new Rect(5, 90, 190, 20), "【 抵 抗 】 " + SucoreText3[SucoreLevel3], gsLabel);

                if (GUI.Button(new Rect(112, 52, 18, 16), "L", gsButton2))
                {
                    ExciteLock = !ExciteLock;
                }
                if (GUI.Button(new Rect(112, 72, 18, 16), "L", gsButton2))
                {
                    OrgasmLock = !OrgasmLock;
                }

                if (GUI.Button(new Rect(10, 115, 95, 20), "弱　[ " + cfg.keyPluginToggleV3 + " ]", gsButton))
                {
                    VLevel = 1;
                    Console.WriteLine("バイブ弱");
                }
                if (GUI.Button(new Rect(115, 115, 95, 20), "強　[ " + cfg.keyPluginToggleV4 + " ]", gsButton))
                {
                    VLevel = 2;
                    Console.WriteLine("バイブ強");
                }

                if (GUI.Button(new Rect(10, 140, 200, 20), "停　止　[ " + cfg.keyPluginToggleV2 + " ]", gsButton))
                {
                    VLevel = 0;
                    Console.WriteLine("バイブ停止");
                }

                if (vMaidStun)
                {
                    if (GUI.Button(new Rect(130, 165, 80, 20), "叩き起す", gsButton))
                    {
                        vMaidStun = false;
                        vMaidStamina = 3000;
                        GameMain.Instance.SoundMgr.PlaySe("se013.ogg", false);
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(130, 165, 80, 20), "こっち来て", gsButton))
                    {
                        ComeMaid(0);
                    }
                }

                //オートモード切り替えボタン
                if (GUI.Button(new Rect(130, 190, 80, 20), autoSelectList[autoSelect], gsButton))
                {

                    if (autoSelect == 0)
                    {
                        autoTime1 = 0;
                        autoTime2 = 0;
                        autoTime3 = 0;
                        autoTime4 = 0;
                    }

                    ++autoSelect;
                    if (autoSelect > 4) autoSelect = 0;

                    if (autoSelect == 0) VLevel = 0;
                }

                GUI.Label(new Rect(5, 165, 210, 20), " ﾌﾟﾗｸﾞｲﾝ無効： [ " + cfg.keyPluginToggleV0 + " ]", gsLabel);
                GUI.Label(new Rect(5, 185, 210, 20), " GUI表示切替： [ " + cfg.keyPluginToggleV1 + " ]", gsLabel);



            }
            else if (cfg.GuiFlag == 2)
            {
                if (GUI.Button(new Rect(180, 0, 20, 20), "+", gsButton))
                {
                    cfg.GuiFlag = 1;
                    Console.WriteLine("GUI表示");
                }
            }


            GUI.DragWindow();
        }


        private int ConfigFlag = 0;
        public Vector2 EditScroll = Vector2.zero;
        void WindowCallback2(int id)
        {

            GUIStyle gsLabel = new GUIStyle("label");
            gsLabel.fontSize = 12;
            gsLabel.alignment = TextAnchor.MiddleLeft;

            GUIStyle gsButton = new GUIStyle("button");
            gsButton.fontSize = 12;
            gsButton.alignment = TextAnchor.MiddleCenter;

            GUIStyle gsToggle = new GUIStyle("toggle");
            gsToggle.fontSize = 12;
            gsToggle.alignment = TextAnchor.MiddleLeft;

            Vector3 vm;
            Vector3 em;
            float emValue;


            if (GUI.Button(new Rect(600, 0, 20, 20), "x", gsButton))
            {
                cfg.GuiFlag2 = false;
            }

            if (ConfigFlag != 2)
            {

                if (GUI.Button(new Rect(440, 10, 60, 20), "再読込", gsButton))
                {
                    cfg = ReadConfig<VibeYourMaidConfig>("Config");
                    cfgw = ReadConfig<VibeYourMaidCfgWriting>("CfgWriting");
                }

                if (GUI.Button(new Rect(505, 10, 80, 20), "設定保存", gsButton))
                {
                    SaveConfig(cfgw, "CfgWriting");
                }

            }


            if (GUI.Button(new Rect(180, 10, 60, 20), "設定１", gsButton))
            {
                ConfigFlag = 0;
            }
            if (GUI.Button(new Rect(245, 10, 60, 20), "設定２", gsButton))
            {
                ConfigFlag = 1;
            }
            if (GUI.Button(new Rect(310, 10, 80, 20), "夜伽EDIT", gsButton))
            {
                ConfigFlag = 2;
            }


            if (ConfigFlag == 0)
            {

                //一列目
                GUI.Label(new Rect(5, 35, 190, 20), "【特殊操作】", gsLabel);

                if (GUI.Button(new Rect(10, 60, 85, 20), "状態リセット", gsButton))
                {

                    VibeDataClear(0);

                    //フェイスブレンドリセット
                    maid.FaceBlend("頬０涙０");

                    //感度・絶頂回数リセット
                    vBoostBase = vBoostDef;
                    vBoostBaseSave[MaidNum] = vBoostDef;
                    vOrgasmCount = 0;
                    vOrgasmCountSave[MaidNum] = 0;
                }

                if (GUI.Button(new Rect(105, 60, 85, 20), "MCopy", gsButton))
                {
                    string t = maid.body0.LastAnimeFN;

                    //t = maid.AudioMan.FileName;
                    Console.WriteLine("クリップボード出力：" + t);
                    Clipboard.SetDataObject(t, true);

                }

                if (GUI.Button(new Rect(10, 85, 85, 20), "SE切替", gsButton))
                {
                    cfgw.SelectSE += 1;

                    if (cfgw.SelectSE >= SeFileList[0].Length)
                    {
                        cfgw.SelectSE = 0;
                    }
                }
                GUI.Label(new Rect(105, 85, 85, 20), SeFileList[0][cfgw.SelectSE], gsLabel);


                //音声モード切替
                GUI.Label(new Rect(5, 115, 190, 20), "【音声モード切替】", gsLabel);

                if (GUI.Button(new Rect(10, 140, 85, 20), "オート", gsButton))
                {
                    AutoModeEnabled = true;
                }
                if (AutoModeEnabled)
                {
                    GUI.Label(new Rect(105, 140, 85, 20), "オートモード", gsLabel);
                }
                else
                {
                    GUI.Label(new Rect(105, 140, 85, 20), ModeSelectList[ModeSelect], gsLabel);
                }

                if (GUI.Button(new Rect(10, 165, 85, 20), "通常固定", gsButton))
                {
                    ModeSelect = 0;
                    AutoModeEnabled = false;
                }
                if (GUI.Button(new Rect(105, 165, 85, 20), "フェラ固定", gsButton))
                {
                    ModeSelect = 1;
                    AutoModeEnabled = false;
                }

                if (GUI.Button(new Rect(10, 190, 85, 20), "カスタム１", gsButton))
                {
                    ModeSelect = 2;
                    AutoModeEnabled = false;
                }
                if (GUI.Button(new Rect(105, 190, 85, 20), "カスタム２", gsButton))
                {
                    ModeSelect = 3;
                    AutoModeEnabled = false;
                }
                if (GUI.Button(new Rect(10, 215, 85, 20), "カスタム３", gsButton))
                {
                    ModeSelect = 4;
                    AutoModeEnabled = false;
                }
                if (GUI.Button(new Rect(105, 215, 85, 20), "カスタム４", gsButton))
                {
                    ModeSelect = 5;
                    AutoModeEnabled = false;
                }


                GUI.Label(new Rect(5, 245, 100, 20), "【メイド操作】", gsLabel);

                vm = maid.transform.position;
                em = maid.transform.eulerAngles;
                if (GUI.Button(new Rect(105, 260, 25, 20), "X↑", gsButton))
                {
                    maid.transform.position = new Vector3(vm.x + 0.01f, vm.y, vm.z);
                }
                if (GUI.Button(new Rect(135, 260, 25, 20), "Y↑", gsButton))
                {
                    maid.transform.position = new Vector3(vm.x, vm.y + 0.01f, vm.z);
                }
                if (GUI.Button(new Rect(165, 260, 25, 20), "Z↑", gsButton))
                {
                    maid.transform.position = new Vector3(vm.x, vm.y, vm.z + 0.01f);
                }

                if (GUI.Button(new Rect(105, 285, 25, 20), "X↓", gsButton))
                {
                    maid.transform.position = new Vector3(vm.x - 0.01f, vm.y, vm.z);
                }
                if (GUI.Button(new Rect(135, 285, 25, 20), "Y↓", gsButton))
                {
                    maid.transform.position = new Vector3(vm.x, vm.y - 0.01f, vm.z);
                }
                if (GUI.Button(new Rect(165, 285, 25, 20), "Z↓", gsButton))
                {
                    maid.transform.position = new Vector3(vm.x, vm.y, vm.z - 0.01f);
                }

                if (GUI.Button(new Rect(200, 260, 45, 20), "左回", gsButton))
                {
                    emValue = em.y + 10f;
                    if (emValue >= 360f) emValue -= 360f;
                    maid.transform.eulerAngles = new Vector3(em.x, emValue, em.z);
                }
                if (GUI.Button(new Rect(250, 260, 45, 20), "右回", gsButton))
                {
                    emValue = em.y - 10f;
                    if (emValue < 0f) emValue += 360f;
                    maid.transform.eulerAngles = new Vector3(em.x, emValue, em.z);
                }
                if (GUI.Button(new Rect(300, 260, 45, 20), "反転", gsButton))
                {
                    emValue = em.y + 180f;
                    if (emValue >= 360f) emValue -= 360f;
                    maid.transform.eulerAngles = new Vector3(em.x, emValue, em.z);
                }

                if (GUI.Button(new Rect(200, 285, 45, 20), "上回", gsButton))
                {
                    emValue = em.x - 10f;
                    if (emValue < 0f) emValue += 360f;
                    if (emValue > 90f && emValue < 270f) emValue = 270f;
                    maid.transform.eulerAngles = new Vector3(emValue, em.y, em.z);
                }
                if (GUI.Button(new Rect(250, 285, 45, 20), "下回", gsButton))
                {
                    emValue = em.x + 10f;
                    if (emValue >= 360f) emValue -= 360f;
                    if (emValue > 90f && emValue < 270f) emValue = 90f;
                    maid.transform.eulerAngles = new Vector3(emValue, em.y, em.z);
                }
                if (GUI.Button(new Rect(300, 285, 45, 20), "ﾘｾｯﾄ", gsButton))
                {
                    maid.transform.eulerAngles = new Vector3(0f, em.y, em.z);
                }

                maid.body0.boEyeToCam = GUI.Toggle(new Rect(10, 265, 90, 20), maid.body0.boEyeToCam, "目を向ける", gsToggle);
                maid.body0.boHeadToCam = GUI.Toggle(new Rect(10, 285, 90, 20), maid.body0.boHeadToCam, "顔を向ける", gsToggle);


                GUI.Label(new Rect(405, 245, 190, 20), "【視点変更】", gsLabel);
                if (man.Visible) fpsModeEnabled = GUI.Toggle(new Rect(405, 265, 190, 20), fpsModeEnabled, "一人称視点にする", gsToggle);
                if (!man.Visible) GUI.Label(new Rect(405, 265, 190, 20), "×一人称視点にする", gsLabel);
                GUI.Label(new Rect(405, 285, 90, 20), "視野角：" + Math.Floor(Camera.main.fieldOfView), gsLabel);
                if (!bOculusVR) Camera.main.fieldOfView = GUI.HorizontalSlider(new Rect(495, 290, 100, 20), Camera.main.fieldOfView, 35.0F, 90.0F);
                if (bOculusVR) Camera.main.fieldOfView = GUI.HorizontalSlider(new Rect(495, 290, 100, 20), Camera.main.fieldOfView, 90.0F, 130.0F);

                /*
                GUI.Label (new Rect (405, 180, 90, 20), "ｶﾒﾗX：" + Math.Floor(mainCamera.transform.eulerAngles.x) , gsLabel);
                GUI.Label (new Rect (405, 200, 90, 20), "ｶﾒﾗY：" + Math.Floor(mainCamera.transform.eulerAngles.y) , gsLabel);
                GUI.Label (new Rect (405, 220, 90, 20), "ｶﾒﾗZ：" + Math.Floor(mainCamera.transform.eulerAngles.z) , gsLabel);

                if(man.Visible && manHead){
                  GUI.Label (new Rect (505, 180, 90, 20), "頭X：" + Math.Floor(manHead.transform.eulerAngles.x) , gsLabel);
                  GUI.Label (new Rect (505, 200, 90, 20), "頭Y：" + Math.Floor(manHead.transform.eulerAngles.y) , gsLabel);
                  GUI.Label (new Rect (505, 220, 90, 20), "頭Z：" + Math.Floor(manHead.transform.eulerAngles.z) , gsLabel);
                }
                */

                //二列目
                GUI.Label(new Rect(205, 35, 190, 20), "【男モデルの表示】", gsLabel);

                if (GUI.Button(new Rect(320, 35, 60, 20), "切替", gsButton))
                {
                    RankoEnabled = !RankoEnabled;

                    if (!RankoEnabled)
                    {  //通常モード時
                        for (int i = 1; i < SubMans.Length; i++)
                        {
                            SubMans[i].Visible = false;
                        }
                        man.transform.position = maid.transform.position;
                        man.transform.eulerAngles = maid.transform.eulerAngles;
                        MansTg[0] = maidDataList[iCurrentMaid];
                        MotionChange(true);

                    }/* else {  //乱交モード時
                      for (int i = 0; i < SubMans.Length; i++){
                        SubMans[i].Visible = true;
                      }
                    }*/

                }

                if (!RankoEnabled)
                {
                    GUI.Label(new Rect(390, 35, 190, 20), "通常モード", gsLabel);
                }
                else
                {
                    GUI.Label(new Rect(390, 35, 190, 20), "乱交モード", gsLabel);
                }



                if (!RankoEnabled)
                {

                    if (GUI.Button(new Rect(210, 60, 85, 20), "表示切替", gsButton))
                    {
                        if (!man) { man = GameMain.Instance.CharacterMgr.GetMan(0); }
                        if (man)
                        {
                            man.Visible = !man.Visible;
                            if (man.Visible)
                            {
                                man.transform.position = maid.transform.position;
                                man.transform.eulerAngles = maid.transform.eulerAngles;
                            }

                            //表示の有無を判断してメイドの顔の向きを変更
                            if (man.Visible)
                            {
                                maid.EyeToCamera((Maid.EyeMoveType)0, 0.8f);
                            }
                            else
                            {
                                maid.EyeToCamera((Maid.EyeMoveType)5, 0.8f);
                            }

                        }
                    }

                    vm = man.transform.position;
                    if (GUI.Button(new Rect(305, 60, 25, 20), "X↑", gsButton))
                    {
                        man.transform.position = new Vector3(vm.x + 0.01f, vm.y, vm.z);
                    }
                    if (GUI.Button(new Rect(335, 60, 25, 20), "Y↑", gsButton))
                    {
                        man.transform.position = new Vector3(vm.x, vm.y + 0.01f, vm.z);
                    }
                    if (GUI.Button(new Rect(365, 60, 25, 20), "Z↑", gsButton))
                    {
                        man.transform.position = new Vector3(vm.x, vm.y, vm.z + 0.01f);
                    }

                    if (GUI.Button(new Rect(210, 85, 85, 20), "位置合せ", gsButton))
                    {
                        man.transform.position = maid.transform.position;
                        man.transform.eulerAngles = maid.transform.eulerAngles;
                    }
                    if (GUI.Button(new Rect(305, 85, 25, 20), "X↓", gsButton))
                    {
                        man.transform.position = new Vector3(vm.x - 0.01f, vm.y, vm.z);
                    }
                    if (GUI.Button(new Rect(335, 85, 25, 20), "Y↓", gsButton))
                    {
                        man.transform.position = new Vector3(vm.x, vm.y - 0.01f, vm.z);
                    }
                    if (GUI.Button(new Rect(365, 85, 25, 20), "Z↓", gsButton))
                    {
                        man.transform.position = new Vector3(vm.x, vm.y, vm.z - 0.01f);
                    }

                    cfgw.autoManEnabled = GUI.Toggle(new Rect(210, 110, 180, 20), cfgw.autoManEnabled, "UNZIP時に男を自動表示", gsToggle);

                }
                else
                {
                    //乱交モード時の表示

                    int x = 0;
                    int y = 0;
                    for (int i = 0; i < SubMans.Length; i++)
                    {

                        if (i % 2 == 0)
                        {
                            if (i != 0) { y += 80; }
                            x = 0;
                        }
                        else
                        {
                            x = 200;
                        }

                        GUI.Label(new Rect(210 + x, 60 + y, 85, 20), SubMansName[i], gsLabel);

                        vm = SubMans[i].transform.position;

                        if (GUI.Button(new Rect(210 + x, 85 + y, 85, 20), "表示切替", gsButton))
                        {
                            if (!SubMans[i]) { SubMans[i] = GameMain.Instance.CharacterMgr.GetMan(i + 1); }
                            if (SubMans[i])
                            {
                                SubMans[i].Visible = !SubMans[i].Visible;
                                if (SubMans[i].Visible)
                                {
                                    SubMans[i].transform.position = SubMaids[MansTg[i]].transform.position;
                                    SubMans[i].transform.eulerAngles = SubMaids[MansTg[i]].transform.eulerAngles;

                                }

                                if (i == 0)
                                {
                                    //ご主人様の場合のみ表示の有無を判断してメイドの顔の向きを変更
                                    if (man.Visible)
                                    {
                                        maid.EyeToCamera((Maid.EyeMoveType)0, 0.8f);
                                    }
                                    else
                                    {
                                        maid.EyeToCamera((Maid.EyeMoveType)5, 0.8f);
                                    }
                                }
                            }
                        }

                        if (SubMans[i].Visible)
                        {
                            if (GUI.Button(new Rect(305 + x, 60 + y, 25, 20), "X↑", gsButton))
                            {
                                SubMans[i].transform.position = new Vector3(vm.x + 0.01f, vm.y, vm.z);
                            }
                            if (GUI.Button(new Rect(335 + x, 60 + y, 25, 20), "Y↑", gsButton))
                            {
                                SubMans[i].transform.position = new Vector3(vm.x, vm.y + 0.01f, vm.z);
                            }
                            if (GUI.Button(new Rect(365 + x, 60 + y, 25, 20), "Z↑", gsButton))
                            {
                                SubMans[i].transform.position = new Vector3(vm.x, vm.y, vm.z + 0.01f);
                            }

                            if (GUI.Button(new Rect(305 + x, 85 + y, 25, 20), "X↓", gsButton))
                            {
                                SubMans[i].transform.position = new Vector3(vm.x - 0.01f, vm.y, vm.z);
                            }
                            if (GUI.Button(new Rect(335 + x, 85 + y, 25, 20), "Y↓", gsButton))
                            {
                                SubMans[i].transform.position = new Vector3(vm.x, vm.y - 0.01f, vm.z);
                            }
                            if (GUI.Button(new Rect(365 + x, 85 + y, 25, 20), "Z↓", gsButton))
                            {
                                SubMans[i].transform.position = new Vector3(vm.x, vm.y, vm.z - 0.01f);
                            }


                            if (GUI.Button(new Rect(210 + x, 110 + y, 60, 20), "対象", gsButton))
                            {
                                GetMaidCount();
                                if (maidDataList.Count > 0)
                                {

                                    int next = maidDataList.IndexOf(MansTg[i]) + 1;
                                    if (next > maidDataList.Count - 1)
                                    {
                                        MansTg[i] = maidDataList[0];
                                    }
                                    else
                                    {
                                        MansTg[i] = maidDataList[next];
                                    }

                                    if (SubMans[i].Visible)
                                    {
                                        SubMans[i].transform.position = SubMaids[MansTg[i]].transform.position;
                                        SubMans[i].transform.eulerAngles = SubMaids[MansTg[i]].transform.eulerAngles;
                                        if (MansTg[i] == maidDataList[iCurrentMaid])
                                        {
                                            MotionChange(true);
                                        }
                                        else
                                        {
                                            MotionChangeSub(MansTg[i], true);
                                        }
                                    }

                                }
                            }

                            //表示されているメイドが減った場合に選択されないようにする
                            if (!maidDataList.Contains(MansTg[i])) { MansTg[i] = maidDataList[0]; }
                            string TargetName = "";
                            TargetName = SubMaids[MansTg[i]].status.lastName + " " + SubMaids[MansTg[i]].status.firstName;
                            GUI.Label(new Rect(280 + x, 110 + y, 120, 20), TargetName, gsLabel);
                        }

                    }
                }

            }


            if (ConfigFlag == 1)
            {

                //一列目
                GUI.Label(new Rect(5, 35, 190, 20), "【各演出の有無】", gsLabel);

                cfgw.HohoEnabled = GUI.Toggle(new Rect(5, 55, 55, 20), cfgw.HohoEnabled, "頬染め", gsToggle);
                cfgw.NamidaEnabled = GUI.Toggle(new Rect(80, 55, 40, 20), cfgw.NamidaEnabled, "涙", gsToggle);

                cfgw.YodareEnabled = GUI.Toggle(new Rect(5, 75, 55, 20), cfgw.YodareEnabled, "よだれ", gsToggle);
                cfgw.aseAnimeEnabled = GUI.Toggle(new Rect(80, 75, 55, 20), cfgw.aseAnimeEnabled, "汗", gsToggle);

                cfgw.SioEnabled = GUI.Toggle(new Rect(5, 95, 55, 20), cfgw.SioEnabled, "潮吹き", gsToggle);
                cfgw.NyoEnabled = GUI.Toggle(new Rect(80, 95, 70, 20), cfgw.NyoEnabled, "お漏らし", gsToggle);

                cfgw.OrgsmAnimeEnabled = GUI.Toggle(new Rect(5, 115, 75, 20), cfgw.OrgsmAnimeEnabled, "痙攣動作", gsToggle);
                cfgw.AheEnabled = GUI.Toggle(new Rect(80, 115, 115, 20), cfgw.AheEnabled, "瞳上昇（アヘ）", gsToggle);

                cfgw.CliAnimeEnabled = GUI.Toggle(new Rect(5, 135, 75, 20), cfgw.CliAnimeEnabled, "クリ勃起", gsToggle);
                cfgw.ChinpoAnimeEnabled = GUI.Toggle(new Rect(80, 135, 115, 20), cfgw.ChinpoAnimeEnabled, "ち○ぽ勃起", gsToggle);

                cfgw.BreastMilkEnabled = GUI.Toggle(new Rect(5, 155, 190, 20), cfgw.BreastMilkEnabled, "ミルク（おっぱい）", gsToggle);
                cfgw.ChinpoMilkEnabled = GUI.Toggle(new Rect(5, 175, 190, 20), cfgw.ChinpoMilkEnabled, "ミルク（おにんにん）", gsToggle);
                cfgw.EnemaMilkEnabled = GUI.Toggle(new Rect(5, 195, 190, 20), cfgw.EnemaMilkEnabled, "ミルク（お尻）", gsToggle);

                cfgw.MotionChangeEnabled = GUI.Toggle(new Rect(5, 215, 190, 20), cfgw.MotionChangeEnabled, "通常時モーション変更", gsToggle);
                cfgw.ZeccyouAnimeEnabled = GUI.Toggle(new Rect(5, 235, 190, 20), cfgw.ZeccyouAnimeEnabled, "絶頂時モーション変更", gsToggle);
                cfgw.ZeccyouManAnimeEnabled = GUI.Toggle(new Rect(20, 255, 175, 20), cfgw.ZeccyouManAnimeEnabled, "男も同期させる", gsToggle);

                cfgw.zViceWaitEnabled = GUI.Toggle(new Rect(5, 275, 190, 20), cfgw.zViceWaitEnabled, "絶頂時に音声終了を待つ", gsToggle);




                //二列目
                GUI.Label(new Rect(205, 35, 190, 20), "【秘部のシェイプアニメを連動】", gsLabel);
                cfgw.hibuAnime1Enabled = GUI.Toggle(new Rect(205, 55, 190, 20), cfgw.hibuAnime1Enabled, "動作時", gsToggle);
                GUI.Label(new Rect(205, 75, 90, 20), "kupa開度：" + Math.Floor(cfgw.hibuSlider1Value), gsLabel);
                cfgw.hibuSlider1Value = GUI.HorizontalSlider(new Rect(295, 80, 100, 20), cfgw.hibuSlider1Value, 0.0F, 100.0F);
                GUI.Label(new Rect(205, 95, 90, 20), "anal開度：" + Math.Floor(cfgw.analSlider1Value), gsLabel);
                cfgw.analSlider1Value = GUI.HorizontalSlider(new Rect(295, 100, 100, 20), cfgw.analSlider1Value, 0.0F, 100.0F);

                cfgw.hibuAnime2Enabled = GUI.Toggle(new Rect(205, 120, 190, 20), cfgw.hibuAnime2Enabled, "停止時", gsToggle);
                GUI.Label(new Rect(205, 140, 90, 20), "kupa開度：" + Math.Floor(cfgw.hibuSlider2Value), gsLabel);
                cfgw.hibuSlider2Value = GUI.HorizontalSlider(new Rect(295, 145, 100, 20), cfgw.hibuSlider2Value, 0.0F, 100.0F);
                GUI.Label(new Rect(205, 160, 90, 20), "anal開度：" + Math.Floor(cfgw.analSlider2Value), gsLabel);
                cfgw.analSlider2Value = GUI.HorizontalSlider(new Rect(295, 165, 100, 20), cfgw.analSlider2Value, 0.0F, 100.0F);

                GUI.Label(new Rect(205, 185, 190, 20), "【カメラの距離判定機能】", gsLabel);
                cfgw.camCheckEnabled = GUI.Toggle(new Rect(205, 205, 190, 20), cfgw.camCheckEnabled, "自動でキスに変更する", gsToggle);
                GUI.Label(new Rect(205, 225, 90, 20), "判定範囲：" + Math.Floor(cfgw.camCheckRange * 100), gsLabel);
                cfgw.camCheckRange = GUI.HorizontalSlider(new Rect(295, 230, 100, 20), cfgw.camCheckRange, 0.0F, 1.0F);

                GUI.Label(new Rect(205, 250, 190, 20), "【ショートカットの同時押し】", gsLabel);
                cfgw.andKeyEnabled[0] = GUI.Toggle(new Rect(205, 270, 55, 20), cfgw.andKeyEnabled[0], " Ctrl", gsToggle);
                if (cfgw.andKeyEnabled[0]) cfgw.andKeyEnabled = new bool[] { true, false, false };
                cfgw.andKeyEnabled[1] = GUI.Toggle(new Rect(265, 270, 55, 20), cfgw.andKeyEnabled[1], " Alt", gsToggle);
                if (cfgw.andKeyEnabled[1]) cfgw.andKeyEnabled = new bool[] { false, true, false };
                cfgw.andKeyEnabled[2] = GUI.Toggle(new Rect(325, 270, 70, 20), cfgw.andKeyEnabled[2], " Shift", gsToggle);
                if (cfgw.andKeyEnabled[2]) cfgw.andKeyEnabled = new bool[] { false, false, true };


                //三列目
                GUI.Label(new Rect(405, 35, 190, 20), "【複数メイドの同期設定】", gsLabel);
                //cfgw.MaidLinkEnabled = GUI.Toggle(new Rect(405, 195, 190, 20), cfgw.MaidLinkEnabled, "全メイドを同期させる", gsToggle);
                cfgw.MaidLinkMotionEnabled = GUI.Toggle(new Rect(405, 55, 190, 20), cfgw.MaidLinkMotionEnabled, "モーション変更を同期", gsToggle);
                cfgw.MaidLinkVoiceEnabled = GUI.Toggle(new Rect(405, 75, 190, 20), cfgw.MaidLinkVoiceEnabled, "音声変更を同期", gsToggle);
                cfgw.MaidLinkFaceEnabled = GUI.Toggle(new Rect(405, 95, 190, 20), cfgw.MaidLinkFaceEnabled, "表情変更を同期", gsToggle);
                cfgw.MaidLinkShapeEnabled = GUI.Toggle(new Rect(405, 115, 190, 20), cfgw.MaidLinkShapeEnabled, "シェイプ操作を同期", gsToggle);

                GUI.Label(new Rect(405, 140, 190, 20), "【メイド切替時の設定】", gsLabel);
                if (cfgw.TaikiEnabled) cfgw.ClearEnabled = false;
                if (cfgw.MaidLinkMotionEnabled || cfgw.MaidLinkVoiceEnabled || cfgw.MaidLinkFaceEnabled)
                {
                    cfgw.ClearEnabled = false;
                    GUI.Label(new Rect(415, 160, 180, 20), "×モーションや音声をクリアする", gsLabel);
                }
                else
                {
                    cfgw.ClearEnabled = GUI.Toggle(new Rect(405, 160, 190, 20), cfgw.ClearEnabled, "モーションや音声をクリアする", gsToggle);
                }

                if (cfgw.ClearEnabled) cfgw.TaikiEnabled = false;
                if (cfgw.MaidLinkMotionEnabled || cfgw.MaidLinkVoiceEnabled || cfgw.MaidLinkFaceEnabled)
                {
                    cfgw.TaikiEnabled = false;
                    GUI.Label(new Rect(415, 180, 180, 20), "×余韻状態にする", gsLabel);
                }
                else
                {
                    cfgw.TaikiEnabled = GUI.Toggle(new Rect(405, 180, 190, 20), cfgw.TaikiEnabled, "余韻状態にする", gsToggle);
                }

                cfgw.CamChangeEnabled = GUI.Toggle(new Rect(405, 200, 190, 20), cfgw.CamChangeEnabled, "カメラを追従する", gsToggle);



                GUI.Label(new Rect(405, 225, 190, 20), "【口元の変更】", gsLabel);
                cfgw.MouthNomalEnabled = GUI.Toggle(new Rect(405, 245, 90, 20), cfgw.MouthNomalEnabled, "通常時", gsToggle);
                cfgw.MouthKissEnabled = GUI.Toggle(new Rect(495, 245, 90, 20), cfgw.MouthKissEnabled, "キス", gsToggle);
                cfgw.MouthZeccyouEnabled = GUI.Toggle(new Rect(405, 265, 90, 20), cfgw.MouthZeccyouEnabled, "連続絶頂", gsToggle);
                cfgw.MouthFeraEnabled = GUI.Toggle(new Rect(495, 265, 90, 20), cfgw.MouthFeraEnabled, "フェラ", gsToggle);

            }



            //夜伽EDIT画面
            if (ConfigFlag == 2)
            {

                int x = 0;
                int y = 0;
                int h = 0;

                Edit_MaidsNum[0] = maidDataList[iCurrentMaid];

                scKeyOff = GUI.Toggle(new Rect(450, 5, 150, 20), scKeyOff, "ショートカット無効", gsToggle);

                GUI.Label(new Rect(410, 35, 80, 20), "夜伽名", gsLabel);
                Edit_Overwrite = GUI.Toggle(new Rect(510, 35, 90, 20), Edit_Overwrite, "上書き保存", gsToggle);

                Edit_YotogiNameT = GUI.TextField(new Rect(410, 55, 180, 20), Edit_YotogiNameT);


                //XMLファイルに保存する
                if (GUI.Button(new Rect(410, 80, 180, 20), "夜伽EDITの保存", gsButton))
                {

                    // フォルダ確認
                    if (!System.IO.Directory.Exists(@"Sybaris\UnityInjector\Config\VibeYourMaid\"))
                    {
                        //ない場合はフォルダ作成
                        System.IO.DirectoryInfo di = System.IO.Directory.CreateDirectory(@"Sybaris\UnityInjector\Config\VibeYourMaid");
                    }

                    //現在状態の読み込み
                    EditSave();

                    if (YEX.Edit_YotogiName == "")
                    {  //夜伽名が空白の場合保存しない
                        Edit_SubMenu = 4;

                    }
                    else
                    {
                        //保存先のファイル名
                        string fileName = @"Sybaris\UnityInjector\Config\VibeYourMaid\Y_" + YEX.Edit_YotogiName + @".xml";

                        if (System.IO.File.Exists(fileName) && !Edit_Overwrite)
                        {  //上書きのチェック
                            Edit_SubMenu = 5;

                        }
                        else
                        {

                            //XmlSerializerオブジェクトを作成
                            //オブジェクトの型を指定する
                            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(YotogiEdit_Xml));

                            //書き込むファイルを開く（UTF-8 BOM無し）
                            System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName, false, new System.Text.UTF8Encoding(false));

                            //シリアル化し、XMLファイルに保存する
                            serializer.Serialize(sw, YEX);
                            //ファイルを閉じる
                            sw.Close();

                            Edit_Overwrite = false;
                            Edit_SubMenu = 6;
                        }

                    }

                }



                //現在の状態を読み込む
                if (GUI.Button(new Rect(410, 115, 180, 20), "現在モーション取得", gsButton))
                {

                    //メイド状態読み込み
                    for (int i = 0; i < Edit_MaidsNum.Length; i++)
                    {

                        if (Edit_MaidsNum[i] >= 0)
                        {
                            Edit_MaidMotionT[i] = SubMaids[Edit_MaidsNum[i]].body0.LastAnimeFN;  //各メイドの現在モーション読み込み

                        }
                        else
                        {
                            Edit_MaidMotionT[i] = ""; //モーション名を初期化
                        }
                    }

                    //男状態読み込み
                    for (int i = 0; i < 4; i++)
                    {

                        if (SubMans[i].Visible)
                        {
                            YEX.Edit_ManMotion[i] = SubMans[i].body0.LastAnimeFN;  //各男の現在モーション読み込み

                        }
                        else
                        {
                            YEX.Edit_ManMotion[i] = "";  //モーション名を初期化
                        }
                    }

                }


                if (GUI.Button(new Rect(410, 140, 85, 20), "メイド選択", gsButton))
                {
                    GetMaidCount();
                    Edit_MaidSet = 1;
                    Edit_SubMenu = 1;
                    Edit_MaidsNum[1] = -1;
                    Edit_MaidsNum[2] = -1;
                    Edit_MaidsNum[3] = -1;
                }

                if (GUI.Button(new Rect(505, 140, 85, 20), "夜伽読込", gsButton))
                {
                    Edit_SubMenu = 2;
                    XmlFilesCheck();
                }


                //メイド選択メニューの表示
                if (Edit_SubMenu == 1)
                {

                    if (maidDataList.Count > Edit_MaidSet)
                    {
                        GUI.Label(new Rect(410, 170, 140, 20), "サブメイド" + Edit_MaidSet + "を選択", gsLabel);
                    }
                    else
                    {
                        GUI.Label(new Rect(410, 170, 140, 20), "選択可能なメイド無し", gsLabel);
                    }

                    if (GUI.Button(new Rect(550, 170, 40, 20), "閉", gsButton))
                    {
                        Edit_SubMenu = 0;
                    }


                    h = 22 * (maidDataList.Count - 1);
                    if (h < 100) { h = 100; }
                    EditScroll = GUI.BeginScrollView(new Rect(410, 195, 200, 100), EditScroll, new Rect(0, 0, 180, h));

                    y = 0;
                    foreach (int n in maidDataList)
                    {

                        if (!Edit_MaidsNum.Contains(n))
                        {
                            string MaidName = SubMaids[n].status.lastName + " " + SubMaids[n].status.firstName;
                            if (GUI.Button(new Rect(0, 0 + y, 180, 20), MaidName, gsButton))
                            {
                                Edit_MaidsNum[Edit_MaidSet] = n;
                                ++Edit_MaidSet;
                                if (Edit_MaidSet > 3) { Edit_MaidSet = 0; }
                            }
                            y += 22;
                        }

                    }

                    GUI.EndScrollView();

                }

                //オリジナル夜伽メニューの表示
                if (Edit_SubMenu == 2)
                {

                    if (xmlFilesY.Count > 0)
                    {
                        GUI.Label(new Rect(410, 170, 140, 20), "読み込む夜伽を選択", gsLabel);
                    }
                    else
                    {
                        GUI.Label(new Rect(410, 170, 140, 20), "夜伽ファイル無し", gsLabel);
                    }

                    if (GUI.Button(new Rect(550, 170, 40, 20), "閉", gsButton))
                    {
                        Edit_SubMenu = 0;
                    }

                    h = 22 * (xmlFilesY.Count);
                    if (h < 100) { h = 100; }
                    EditScroll = GUI.BeginScrollView(new Rect(410, 195, 200, 100), EditScroll, new Rect(0, 0, 180, h));

                    y = 0;
                    foreach (string f in xmlFilesY)
                    {
                        string FileName = f.Replace("y_", "").Replace(".xml", "");

                        if (GUI.Button(new Rect(0, 0 + y, 180, 20), FileName, gsButton))
                        {
                            EditRead(f);
                            if (autoSelect != 0) autoSelect = 0;

                        }
                        y += 22;
                    }

                    GUI.EndScrollView();

                }


                if (Edit_SubMenu == 3)
                {
                    GUI.Label(new Rect(410, 170, 180, 20), "メイドの人数が足りません", gsLabel);
                }
                if (Edit_SubMenu == 4)
                {
                    GUI.Label(new Rect(410, 170, 180, 40), "夜伽名が空白のため保存できません", gsLabel);
                }
                if (Edit_SubMenu == 5)
                {
                    GUI.Label(new Rect(410, 170, 180, 60), "同名の夜伽ファイルが既に存在します。保存する場合は「上書き保存」にチェックを入れて下さい", gsLabel);
                }
                if (Edit_SubMenu == 6)
                {
                    GUI.Label(new Rect(410, 170, 180, 40), "夜伽ファイルを保存しました", gsLabel);
                }


                y = 0;
                for (int i = 0; i < Edit_MaidsNum.Length; i++)
                {

                    if (i % 2 == 0)
                    {
                        if (i != 0) { y += 105; }
                        x = 0;
                    }
                    else
                    {
                        x = 200;
                    }

                    string MaidName = "";

                    if (i == 0)
                    {
                        MaidName = maid.status.lastName + " " + maid.status.firstName;
                        GUI.Label(new Rect(5 + x, 35 + y, 190, 20), "【メインメイド】 " + MaidName, gsLabel);

                    }
                    else
                    {
                        if (!maidDataList.Contains(Edit_MaidsNum[i]) || Edit_MaidsNum[i] == Edit_MaidsNum[0])
                        {
                            Edit_MaidsNum[i] = -1;
                        }
                        if (Edit_MaidsNum[i] >= 0)
                        {

                            MaidName = SubMaids[Edit_MaidsNum[i]].status.lastName + " " + SubMaids[Edit_MaidsNum[i]].status.firstName;
                        }
                        else
                        {
                            MaidName = "未選択";
                        }
                        GUI.Label(new Rect(5 + x, 35 + y, 190, 20), "【サブメイド" + i + "】 " + MaidName, gsLabel);
                    }

                    if (Edit_MaidsNum[i] >= 0)
                    {
                        if (GUI.Button(new Rect(5 + x, 60 + y, 55, 20), "ﾓｰｼｮﾝ", gsButton))
                        {
                            SubMaids[Edit_MaidsNum[i]].CrossFadeAbsolute(Edit_MaidMotionT[i], false, true, false, 0.7f, 1f);
                        }

                        Edit_MaidMotionT[i] = GUI.TextField(new Rect(65 + x, 60 + y, 130, 20), Edit_MaidMotionT[i]);


                        if (GUI.Button(new Rect(5 + x, 85 + y, 55, 20), "ﾎﾞｲｽｾｯﾄ", gsButton))
                        {
                            XmlFilesCheck();
                            vsGui = 1;
                            vsTg = i;

                            editVoiceSetName = madiVoiceSetName[i];
                            if (i == 0) editVoiceSet = madiVoiceSet0;
                            if (i == 1) editVoiceSet = madiVoiceSet1;
                            if (i == 2) editVoiceSet = madiVoiceSet2;
                            if (i == 3) editVoiceSet = madiVoiceSet3;

                        }

                        GUI.Label(new Rect(65 + x, 85 + y, 95, 20), madiVoiceSetName[i], gsLabel);

                        if (GUI.Button(new Rect(165 + x, 85 + y, 30, 20), "ｸﾘｱ", gsButton))
                        {

                            madiVoiceSetName[i] = "";

                            if (i == 0) madiVoiceSet0 = new List<string[]>{
                          new string[] { "" , "0" , "0" , "3" , "0" , "3" , "0" , "0" }
                        };
                            if (i == 1) madiVoiceSet1 = new List<string[]>{
                          new string[] { "" , "0" , "0" , "3" , "0" , "3" , "0" , "0" }
                        };
                            if (i == 2) madiVoiceSet2 = new List<string[]>{
                          new string[] { "" , "0" , "0" , "3" , "0" , "3" , "0" , "0" }
                        };
                            if (i == 3) madiVoiceSet3 = new List<string[]>{
                          new string[] { "" , "0" , "0" , "3" , "0" , "3" , "0" , "0" }
                        };
                        }

                        GUI.Label(new Rect(5 + x, 110 + y, 90, 20), "再生間隔：" + Math.Floor(vsInterval[i]), gsLabel);
                        vsInterval[i] = GUI.HorizontalSlider(new Rect(100 + x, 115 + y, 95, 20), vsInterval[i], 100.0F, 1500.0F);

                    }



                    //GUI.Label (new Rect (5 + x, 75 + y, 190, 20), "位置｜ X：" + Edit_MaidPos[i][0] + "　Y：" + Edit_MaidPos[i][1] + "　Z：" + Edit_MaidPos[i][2] , gsLabel);
                    //GUI.Label (new Rect (5 + x, 95 + y, 190, 20), "向き｜ X：" + Edit_MaidEul[i][0] + "　Y：" + Edit_MaidEul[i][1] + "　Z：" + Edit_MaidEul[i][2] , gsLabel);

                }


                for (int i = 0; i < 4; i++)
                {

                    if (i % 4 == 0)
                    {
                        if (i == 0)
                        {
                            y += 110;
                        }
                        else
                        {
                            y += 45;
                        }
                        x = 0;
                    }
                    else
                    {
                        x += 100;
                    }

                    GUI.Label(new Rect(5 + x, 35 + y, 90, 20), "【" + SubMansName[i] + "】", gsLabel);

                    if (!maidDataList.Contains(MansTg[i])) { MansTg[i] = maidDataList[0]; }
                    if (SubMans[i].Visible)
                    {
                        string MaidName = SubMaids[MansTg[i]].status.lastName + " " + SubMaids[MansTg[i]].status.firstName;
                        GUI.Label(new Rect(5 + x, 55 + y, 90, 40), MaidName, gsLabel);
                    }
                    else
                    {
                        GUI.Label(new Rect(5 + x, 55 + y, 90, 20), " 非表示", gsLabel);
                    }

                    //GUI.Label (new Rect (5 + x, 75 + y, 190, 20), "位置｜ X：" + Edit_ManPos[i][0] + "　Y：" + Edit_ManPos[i][1] + "　Z：" + Edit_ManPos[i][2] , gsLabel);
                    //GUI.Label (new Rect (5 + x, 95 + y, 190, 20), "向き｜ X：" + Edit_ManEul[i][0] + "　Y：" + Edit_ManEul[i][1] + "　Z：" + Edit_ManEul[i][2] , gsLabel);

                }







                /*
                                GUI.Label(new Rect(5, 35, 190, 20), "【メイド位置】", gsLabel);
                                GUI.Label(new Rect(5, 55, 190, 20), "X：" + maid.transform.position.x, gsLabel);
                                GUI.Label(new Rect(5, 75, 190, 20), "Y：" + maid.transform.position.y, gsLabel);
                                GUI.Label(new Rect(5, 95, 190, 20), "Z：" + maid.transform.position.z, gsLabel);

                                GUI.Label(new Rect(5, 120, 190, 20), "【メイド向き】", gsLabel);
                                GUI.Label(new Rect(5, 140, 190, 20), "X：" + maid.transform.eulerAngles.x, gsLabel);
                                GUI.Label(new Rect(5, 160, 190, 20), "Y：" + maid.transform.eulerAngles.y, gsLabel);
                                GUI.Label(new Rect(5, 180, 190, 20), "Z：" + maid.transform.eulerAngles.z, gsLabel);


                                GUI.Label(new Rect(205, 35, 190, 20), "【カメラ位置】", gsLabel);
                                GUI.Label(new Rect(205, 55, 190, 20), "X：" + mainCamera.transform.position.x, gsLabel);
                                GUI.Label(new Rect(205, 75, 190, 20), "Y：" + mainCamera.transform.position.y, gsLabel);
                                GUI.Label(new Rect(205, 95, 190, 20), "Z：" + mainCamera.transform.position.z, gsLabel);

                                GUI.Label(new Rect(205, 120, 190, 20), "【カメラ向き】", gsLabel);
                                GUI.Label(new Rect(205, 140, 190, 20), "X：" + mainCamera.transform.eulerAngles.x, gsLabel);
                                GUI.Label(new Rect(205, 160, 190, 20), "Y：" + mainCamera.transform.eulerAngles.y, gsLabel);
                                GUI.Label(new Rect(205, 180, 190, 20), "Z：" + mainCamera.transform.eulerAngles.z, gsLabel);


                                      if(GUI.Button(new Rect (5, 210, 85, 20), "こっち来い", gsButton)) {

                                        float MX = mainCamera.transform.position.x + (((mainCamera.transform.eulerAngles.y - 180) / -90) * ((mainCamera.transform.eulerAngles.x - 270) / 90));
                                        float MY = mainCamera.transform.position.z + (mainCamera.transform.eulerAngles.x - 180 / -90);
                                        float MZ = mainCamera.transform.position.z + (((mainCamera.transform.eulerAngles.y - 270) / -90) * ((mainCamera.transform.eulerAngles.x - 270) / 90));
                                        maid.transform.position = new Vector3(MX, MY, MZ);

                                      }

                                //X = 
                                //

                                //90 = 0
                                //180 = 1
                                //270 = 0
                                //360 = -1

                */
            }



            GUI.DragWindow();
        }

        Vector2 YotogiScrollPos = Vector2.zero;
        void WindowCallback3(int id)
        {

            GUIStyle gsLabel = new GUIStyle("label");
            gsLabel.fontSize = 12;
            gsLabel.alignment = TextAnchor.MiddleLeft;

            GUIStyle gsButton = new GUIStyle("button");
            gsButton.fontSize = 12;
            gsButton.alignment = TextAnchor.MiddleCenter;

            GUIStyle gsToggle = new GUIStyle("toggle");
            gsToggle.fontSize = 12;
            gsToggle.alignment = TextAnchor.MiddleLeft;


            if (GUI.Button(new Rect(380, 0, 20, 20), "x", gsButton))
            {
                cfg.GuiFlag3 = false;
            }

            //vsSettei = GUI.Toggle(new Rect(240, 5, 140, 20), vsSettei, "VS設定", gsToggle); 未完成

            int y = 0;
            int x = 0;
            int i = 0;
            foreach (string t in YotogiGroup)
            {
                if (i % 5 == 0)
                {
                    y += 1;
                }
                x = 5 + (i % 5) * 75;

                if (GUI.Button(new Rect(x, 25 * y, 70, 20), t, gsButton))
                {
                    YotogiMenu = i;
                    if (t == "ランダム") CsvFilesCheck();
                }
                i += 1;

            }


            int h = 185 - y * 25;
            if (YotogiGroup.Count - 1 > YotogiMenu)
            {
                h = YotogiList[YotogiMenu].Count * 11 + 11;
                if (h < (185 - y * 25))
                {
                    h = 185 - y * 25;
                }
            }
            else
            {
                h = csvFilesR.Count * 11 + 11;
                if (h < (185 - y * 25))
                {
                    h = 185 - y * 25;
                }
            }

            Rect scrlRect = new Rect(10, 25 + y * 25, 389, 185 - y * 25);
            Rect contentRect = new Rect(0, 0, 370, h);
            YotogiScrollPos = GUI.BeginScrollView(scrlRect, YotogiScrollPos, contentRect, false, true);

            //夜伽モーションのリスト画面
            if (YotogiGroup.Count - 1 > YotogiMenu)
            {
                y = 0;
                x = 0;
                i = 0;
                foreach (string t in YotogiList[YotogiMenu])
                {
                    if (i % 2 == 0)
                    {
                        if (i != 0) { y += 1; }
                        x = 0;
                    }
                    else
                    {
                        x = 182;
                    }

                    string name = YotogiListName[YotogiMenu][i];

                    i += 1;

                    if (GUI.Button(new Rect(x, 22 * y, 180, 20), name, gsButton))
                    {

                        //メイドのモーション変更
                        maid.CrossFadeAbsolute(t + ".anm", false, true, false, 0.7f, 1f);
                        //-----------------------------------------------------------------------------------------------------------------
                        // 2018/10/07 COM3D2 Ver1.20.1 暫定対応
                        //-----------------------------------------------------------------------------------------------------------------
                        //maid.IKTargetToBone("左手", null, "無し", Vector3.zero, IKCtrlData.IKAttachType.Point, false, 0f, false, false);
                        //maid.IKTargetToBone("右手", null, "無し", Vector3.zero, IKCtrlData.IKAttachType.Point, false, 0f, false, false);
                        //-----------------------------------------------------------------------------------------------------------------
                        maid.IKTargetToBone("左手", null, "無し", Vector3.zero, IKCtrlData.IKAttachType.Point, false, false, false);
                        maid.IKTargetToBone("右手", null, "無し", Vector3.zero, IKCtrlData.IKAttachType.Point, false, false, false);
                        //-----------------------------------------------------------------------------------------------------------------

                        //男の自動表示
                        if (cfgw.autoManEnabled && !RankoEnabled && !man.Visible)
                        {
                            if (!man) { man = GameMain.Instance.CharacterMgr.GetMan(0); }
                            if (man)
                            {
                                man.Visible = true;
                                man.transform.position = maid.transform.position;
                                man.transform.eulerAngles = maid.transform.eulerAngles;
                                maid.EyeToCamera((Maid.EyeMoveType)0, 0.8f);
                            }
                        }


                        //男のモーション変更
                        MotionChangeMan(maidDataList[iCurrentMaid]);
                        /*string m = "";
                        m = Regex.Replace(t, "_f", "_m");
                        m = Regex.Replace(m, "[a-zA-Z][0-9][0-9]", "");

                        if (man.Visible && allFiles.Contains(m)) {
                          man.CrossFadeAbsolute( m + ".anm", false, true, false, 0.7f, 1f );
                        }*/

                        vStateHoldTimeM = 0; //モーションタイマーリセット
                        checkBlowjobing(maid, maidDataList[iCurrentMaid]); //フェラORキスのチェック

                        if (autoSelect != 0 || amlBack[0] != "0" || amlBack[1] != "0" || amlBack[2] != "0")
                        {
                            autoSelect = 0;
                            autoModeReset();
                        }

                        //ボイスセット読み込み　未完成
                        //int iv = unzipVoiceSet[0].IndexOf(name);
                        //if(iv != -1){
                        //  string vs = "v_" + unzipVoiceSet[1][iv] + ".xml";
                        //  voiceSetLoad(vs, 0);
                        //}

                    }
                }
            }
            else if (csvFilesR.Count > 0)
            {
                //ランダムモーションセット
                y = 0;
                x = 0;
                i = 0;
                foreach (string t in csvFilesR)
                {
                    if (i % 2 == 0)
                    {
                        if (i != 0) { y += 1; }
                        x = 0;
                    }
                    else
                    {
                        x = 182;
                    }

                    string name = t.Replace("r_", "").Replace(".csv", "");

                    i += 1;

                    if (GUI.Button(new Rect(x, 22 * y, 180, 20), name, gsButton))
                    {

                        //男の自動表示
                        if (cfgw.autoManEnabled && !RankoEnabled && !man.Visible)
                        {
                            if (!man) { man = GameMain.Instance.CharacterMgr.GetMan(0); }
                            if (man)
                            {
                                man.Visible = true;
                                man.transform.position = maid.transform.position;
                                man.transform.eulerAngles = maid.transform.eulerAngles;
                                maid.EyeToCamera((Maid.EyeMoveType)0, 0.8f);
                            }
                        }

                        //CSVからランダムモーションリストを読み込み
                        CsvRead(t);

                        if (autoSelect == 0) autoSelect = 1;
                        autoTime1 = 0;
                        autoTime2 = 0;
                        autoTime3 = 0;
                        autoTime4 = 0;

                    }
                }

            }

            GUI.EndScrollView();


            GUI.DragWindow();
        }


        Vector2 vsScrollPos1 = Vector2.zero;
        Vector2 vsScrollPos2 = Vector2.zero;
        void WindowCallback4(int id)
        {

            GUIStyle gsLabel = new GUIStyle("label");
            gsLabel.fontSize = 12;
            gsLabel.alignment = TextAnchor.MiddleLeft;

            GUIStyle gsLabel2 = new GUIStyle("label");
            gsLabel2.fontSize = 12;
            gsLabel2.alignment = TextAnchor.MiddleCenter;

            GUIStyle gsButton = new GUIStyle("button");
            gsButton.fontSize = 12;
            gsButton.alignment = TextAnchor.MiddleCenter;

            GUIStyle gsButton2 = new GUIStyle("button");
            gsButton2.fontSize = 10;
            gsButton2.alignment = TextAnchor.MiddleCenter;

            GUIStyle gsToggle = new GUIStyle("toggle");
            gsToggle.fontSize = 12;
            gsToggle.alignment = TextAnchor.MiddleLeft;


            if (GUI.Button(new Rect(380, 0, 20, 20), "x", gsButton))
            {
                vsGui = 0;
            }

            scKeyOff = GUI.Toggle(new Rect(240, 5, 140, 20), scKeyOff, "ショートカット無効", gsToggle);

            //ボイスセット一覧
            int x = 0;
            int y = 0;
            int c = 0;
            int h1 = 11 * xmlFilesV.Count + 11;
            if (h1 < 70) h1 = 70;

            Rect scrlRect1 = new Rect(0, 30, 400, 80);
            Rect contentRect1 = new Rect(-10, 0, 370, h1);
            vsScrollPos1 = GUI.BeginScrollView(scrlRect1, vsScrollPos1, contentRect1, false, true);

            foreach (string f in xmlFilesV)
            {
                if (c % 2 == 0)
                {
                    if (c != 0) { y += 22; }
                    x = 0;
                }
                else
                {
                    x = 185;
                }

                string FileName = f.Replace("v_", "").Replace(".xml", "");

                if (GUI.Button(new Rect(0 + x, 0 + y, 180, 20), FileName, gsButton))
                {
                    voiceSetLoad(f, vsTg);

                }

                ++c;
            }

            GUI.EndScrollView();

            GUI.Label(new Rect(10, 110, 380, 20), "―――――――――――――――――――――――――――――", gsLabel2);


            //ボイスセット編集画面
            GUI.Label(new Rect(5, 125, 90, 20), "ボイスセット名", gsLabel);
            vs_Overwrite = GUI.Toggle(new Rect(105, 125, 70, 20), vs_Overwrite, "上書／ｸﾘｱ", gsToggle);
            editVoiceSetName = GUI.TextField(new Rect(5, 145, 170, 20), editVoiceSetName);

            if (GUI.Button(new Rect(55, 170, 55, 20), "クリア", gsButton))
            {
                if (vs_Overwrite)
                {
                    editVoiceSet = new List<string[]>{
                      new string[] { "" , "0" , "0" , "3" , "0" , "3" , "0" , "0" }
                    };
                    editVoiceSetName = "";
                    vs_Overwrite = false;
                }
                else
                {
                    vsErrer = 3;
                }
            }

            //XMLファイルに保存する
            if (GUI.Button(new Rect(120, 170, 55, 20), "保存", gsButton))
            {
                voiceSetSave(vsTg);
                XmlFilesCheck();
            }

            if (vsErrer != 0)
            {
                if (GUI.Button(new Rect(5, 190, 20, 20), "x", gsButton))
                {
                    vsErrer = 0;
                }
                GUI.Label(new Rect(30, 190, 390, 20), vsErrerText[vsErrer], gsLabel);
            }


            //フィルタ機能
            GUI.Label(new Rect(200, 125, 145, 20), "フィルタリング", gsLabel);

            if (GUI.Button(new Rect(200, 145, 70, 20), personalList[0][fVoiceSet[0]], gsButton))
            {
                ++fVoiceSet[0];
                if (fVoiceSet[0] >= personalList[0].Length) fVoiceSet[0] = 0;
            }

            if (GUI.Button(new Rect(275, 145, 55, 20), vsState[fVoiceSet[3]], gsButton))
            {
                ++fVoiceSet[3];
                if (fVoiceSet[3] >= vsState.Length) fVoiceSet[3] = 0;
            }

            if (GUI.Button(new Rect(335, 145, 55, 20), vsCondition[fVoiceSet[4]], gsButton))
            {
                ++fVoiceSet[4];
                if (fVoiceSet[4] >= vsCondition.Length) fVoiceSet[4] = 0;
            }


            GUI.Label(new Rect(200, 170, 30, 20), "興奮", gsLabel);
            if (GUI.Button(new Rect(230, 170, 20, 20), vsLevel[fVoiceSet[1]], gsButton))
            {
                ++fVoiceSet[1];
                if (fVoiceSet[1] > 4) fVoiceSet[1] = 0;
            }

            GUI.Label(new Rect(260, 170, 30, 20), "絶頂", gsLabel);
            if (GUI.Button(new Rect(290, 170, 20, 20), vsLevel[fVoiceSet[2]], gsButton))
            {
                ++fVoiceSet[2];
                if (fVoiceSet[2] > 4) fVoiceSet[2] = 0;
            }

            if (GUI.Button(new Rect(320, 170, 70, 20), "リセット", gsButton))
            {
                //fVoiceSet = new int[] { 7, 4, 4, 2, 5 };
                fVoiceSet = new int[] {13, 4, 4, 2, 5 };
            }

            int h2 = editVoiceSet.Count * 65 + 30;
            if (h2 < 310) h2 = 310;
            y = 0;

            Rect scrlRect2 = new Rect(0, 215, 400, 310);
            Rect contentRect2 = new Rect(-5, 0, 375, h2);
            vsScrollPos2 = GUI.BeginScrollView(scrlRect2, vsScrollPos2, contentRect2, false, true);

            int iv;
            for (int i = 0; i < editVoiceSet.Count; i++)
            {

                if ((fVoiceSet[0] == intCnv(editVoiceSet[i][1]) || fVoiceSet[0] == personalList[0].Length - 1 || intCnv(editVoiceSet[i][1]) == personalList[0].Length - 1)
                   && (intCnv(editVoiceSet[i][2]) <= fVoiceSet[1] && fVoiceSet[1] <= intCnv(editVoiceSet[i][3]) || fVoiceSet[1] == 4)
                   && (intCnv(editVoiceSet[i][4]) <= fVoiceSet[2] && fVoiceSet[2] <= intCnv(editVoiceSet[i][5]) || fVoiceSet[2] == 4)
                   && (fVoiceSet[3] == intCnv(editVoiceSet[i][6]) || fVoiceSet[3] == 2 || intCnv(editVoiceSet[i][6]) == 2)
                   && (fVoiceSet[4] == intCnv(editVoiceSet[i][7]) || fVoiceSet[4] == 5 || intCnv(editVoiceSet[i][7]) == 5))
                {

                    GUI.Label(new Rect(0, 0 + y, 30, 20), "音声", gsLabel);
                    editVoiceSet[i][0] = GUI.TextField(new Rect(30, 0 + y, 110, 20), editVoiceSet[i][0]);


                    //性格選択
                    iv = intCnv(editVoiceSet[i][1]);
                    if (GUI.Button(new Rect(150, 0 + y, 70, 20), personalList[0][iv], gsButton))
                    {
                        ++iv;
                        if (iv >= personalList[0].Length) iv = 0;
                        editVoiceSet[i][1] = iv.ToString();
                    }

                    //強度選択
                    iv = intCnv(editVoiceSet[i][6]);
                    if (GUI.Button(new Rect(225, 0 + y, 70, 20), vsState[iv], gsButton))
                    {
                        ++iv;
                        if (iv >= vsState.Length) iv = 0;
                        editVoiceSet[i][6] = iv.ToString();
                    }

                    //状態選択
                    iv = intCnv(editVoiceSet[i][7]);
                    if (GUI.Button(new Rect(300, 0 + y, 70, 20), vsCondition[iv], gsButton))
                    {
                        ++iv;
                        if (iv >= vsCondition.Length) iv = 0;
                        editVoiceSet[i][7] = iv.ToString();
                    }

                    //興奮度指定
                    GUI.Label(new Rect(0, 25 + y, 30, 20), "興奮", gsLabel);
                    iv = intCnv(editVoiceSet[i][2]);
                    if (GUI.Button(new Rect(30, 25 + y, 20, 20), editVoiceSet[i][2], gsButton))
                    {
                        ++iv;
                        if (iv >= 4) iv = 0;
                        editVoiceSet[i][2] = iv.ToString();
                    }

                    GUI.Label(new Rect(55, 25 + y, 15, 20), "～", gsLabel);

                    iv = intCnv(editVoiceSet[i][3]);
                    if (GUI.Button(new Rect(70, 25 + y, 20, 20), editVoiceSet[i][3], gsButton))
                    {
                        ++iv;
                        if (iv >= 4) iv = 0;
                        editVoiceSet[i][3] = iv.ToString();
                    }

                    if (intCnv(editVoiceSet[i][2]) > intCnv(editVoiceSet[i][3])) editVoiceSet[i][3] = editVoiceSet[i][2];

                    //絶頂度指定
                    GUI.Label(new Rect(100, 25 + y, 30, 20), "絶頂", gsLabel);
                    iv = intCnv(editVoiceSet[i][4]);
                    if (GUI.Button(new Rect(130, 25 + y, 20, 20), editVoiceSet[i][4], gsButton))
                    {
                        ++iv;
                        if (iv >= 4) iv = 0;
                        editVoiceSet[i][4] = iv.ToString();
                    }

                    GUI.Label(new Rect(155, 25 + y, 15, 20), "～", gsLabel);

                    iv = intCnv(editVoiceSet[i][5]);
                    if (GUI.Button(new Rect(170, 25 + y, 20, 20), editVoiceSet[i][5], gsButton))
                    {
                        ++iv;
                        if (iv >= 4) iv = 0;
                        editVoiceSet[i][5] = iv.ToString();
                    }

                    if (intCnv(editVoiceSet[i][4]) > intCnv(editVoiceSet[i][5])) editVoiceSet[i][5] = editVoiceSet[i][4];


                    if (GUI.Button(new Rect(220, 25 + y, 80, 20), "テスト再生", gsButton))
                    {
                        if (!Regex.IsMatch(editVoiceSet[i][0], @"\.[a-zA-Z][a-zA-Z]")) editVoiceSet[i][0] = editVoiceSet[i][0] + ".ogg";
                        SubMaids[Edit_MaidsNum[vsTg]].AudioMan.LoadPlay(editVoiceSet[i][0], 0f, false, false);
                    }

                    if (GUI.Button(new Rect(310, 25 + y, 60, 20), "削除", gsButton))
                    {
                        editVoiceSet.RemoveAt(i);
                    }

                    GUI.Label(new Rect(5, 45 + y, 370, 20), "―――――――――――――――――――――――――――――", gsLabel2);

                    y += 65;
                }

            }

            if (GUI.Button(new Rect(315, 0 + y, 60, 20), "追加", gsButton))
            {
                string[] set = new string[] { "", fVoiceSet[0].ToString(), "0", "3", "0", "3", fVoiceSet[3].ToString(), fVoiceSet[4].ToString() };
                editVoiceSet.Add(set);
            }

            GUI.EndScrollView();

            GUI.DragWindow();
        }





        void Update()
        {


            //全体処理　開始---------------------------

            if (cfg.bPluginEnabledV)
            {

                if (bReGetMaid)
                {
                    maid = GameMain.Instance.CharacterMgr.GetMaid(maidDataList[iCurrentMaid]);
                    SaveFlag = false;
                    bReGetMaid = false;

                    maid.AudioMan.Stop();

                    if (man.Visible && !RankoEnabled)
                    {
                        man.transform.position = maid.transform.position;
                        man.transform.eulerAngles = maid.transform.eulerAngles;
                        MotionChange(true);
                    }
                }

                if (SceneLevelEnable)
                {
                    //maid = GameMain.Instance.CharacterMgr.GetMaid(0);
                    //SaveFlag = false;

                    GetMaidCount();
                    if (maidDataList.Count > 0 && !maidActive)
                    {
                        iCurrentMaid = 0;
                        maid = GameMain.Instance.CharacterMgr.GetMaid(maidDataList[iCurrentMaid]);
                        VibeDataClear(0);
                        SaveFlag = false;
                    }
                    else if (maidDataList.Count == 0)
                    {
                        maid = null;
                        maidActive = false;
                        if (vStateMajor != 10)
                        {
                            VibeDataClear(0);
                            SaveFlag = false;
                        }
                    }

                }


                if (SceneLevelEnable && maid)
                {

                    //メイドさんがシーンに存在するかどうか判定
                    maidActive = maid.gameObject.activeInHierarchy;

                    if (!maidActive)
                    {
                        GetMaidCount();
                        if (maidDataList.Count > 0)
                        {
                            iCurrentMaid = 0;
                            maid = GameMain.Instance.CharacterMgr.GetMaid(maidDataList[iCurrentMaid]);
                        }
                        else
                        {
                            maid = null;
                        }

                        VibeDataClear(0);
                        SaveFlag = false;
                    }

                    if (maidActive && !SaveFlag)
                    {

                        //　メイドの状態セーブ処理
                        string MaidName = maid.status.lastName + " " + maid.status.firstName;
                        Console.WriteLine("A:" + MaidName);

                        MaidNum = MaidNameSave.IndexOf(MaidName);
                        Console.WriteLine("B:" + MaidNum);

                        if (MaidNum == -1)
                        {
                            MaidNameSave.Add(MaidName);
                            vBoostBaseSave.Add(vBoostDef);
                            vOrgasmCountSave.Add(0);
                            VLevelSave.Add(0);
                            FaceBackupSave.Add("");
                            MotionBackupSave.Add("");

                            MaidNum = MaidNameSave.IndexOf(MaidName);

                            //絶頂回数・感度を初期化
                            vBoostBase = vBoostDef;
                            vOrgasmCount = 0;

                            Console.WriteLine("C:" + MaidNum);
                            Console.WriteLine("D:" + MaidNameSave[MaidNum]);
                        }
                        else
                        {
                            vBoostBase = vBoostBaseSave[MaidNum];
                            vOrgasmCount = vOrgasmCountSave[MaidNum];
                            VLevel = VLevelSave[MaidNum];
                            sFaceAnimeBackupV = FaceBackupSave[MaidNum];
                            MaidMotionBack = MotionBackupSave[MaidNum];

                            Console.WriteLine("感度 / 絶頂数：" + vBoostBase + " / " + vOrgasmCount);
                            Console.WriteLine("VLevel：" + VLevel);
                        }
                        SaveFlag = true;

                    }



                    if (vSceneLevel == 15 && WaitTime < 120)
                    {

                        WaitTime += Time.deltaTime * 60;

                    }
                    else if (maidActive)
                    {

                        if (!StartFlag && VLevel != 0) { StartFlag = true; }


                        //夜伽EDITのメイド状態チェック
                        for (int i = 0; i < Edit_MaidsNum.Length; i++)
                        {
                            if (i == 0)
                            {
                                Edit_MaidsNum[0] = maidDataList[iCurrentMaid];
                            }
                            else
                            {
                                if (!maidDataList.Contains(Edit_MaidsNum[i]) || Edit_MaidsNum[i] == Edit_MaidsNum[0])
                                {
                                    Edit_MaidsNum[i] = -1;
                                }
                            }
                        }


                        //ダブルクリック判定
                        DClicCheck();

                        if (StartFlag)
                        {
                            //表情等変更処理を実施する
                            checkFaceDistance2();

                            //ボイスセット再生処理
                            VoiceSetPlay();
                        }

                        //オートモード処理
                        if (autoSelect != 0) autoMode();


                        //フェードアウトを判定して状態クリア（バカンスモード用）
                        if (FadeMgr.GetFadeIn() && vacationEnabled)
                        {
                            StatusClear();
                        }





                        if (mainCamera && Camera.main)
                        { //@API実装ついで//Nullエラー修正(複数メイドプラグインでメイドを増やした時の暗転時)
                          //カメラと顔の位置チェック
                            CameraPosCheck();
                        }

                        //ちんぽ勃起処理
                        updateShapeKeyChinpoValue();

                        //潮吹き
                        StartSio();

                        //乱交モードでない時にご主人様のターゲットがメインメイドでない場合、メインメイドに戻す
                        if (!RankoEnabled && MansTg[0] != maidDataList[iCurrentMaid])
                        {
                            MansTg[0] = maidDataList[iCurrentMaid];
                        }


                        //一人称視点処理
                        if (fpsModeEnabled && man.Visible)
                        {
                            if (!bfpsMode)
                            {
                                bfpsMode = true;
                                if (!bOculusVR)
                                {
                                    fieldOfViewBack = Camera.main.fieldOfView;
                                    Camera.main.fieldOfView = 60.0f;
                                }
                                frameCount = 0;
                            }

                            FpsModeChange();


                            if (manHeadRen.enabled) manHeadRen.enabled = false;
                        }
                        if ((!fpsModeEnabled || !man.Visible) && bfpsMode)
                        {
                            bfpsMode = false;
                            if (!bOculusVR) Camera.main.fieldOfView = fieldOfViewBack;
                            manHeadRen.enabled = true;
                        }






                        //―――――――――――――――――――――――
                        //ショートカットキー
                        //―――――――――――――――――――――――
                        if ((ConfigFlag != 2 || !cfg.GuiFlag2) && vsGui == 0 && scKeyOff)
                        {
                            scKeyOff = false;
                        }

                        bool andKey = false;
                        int index1 = Array.IndexOf(cfgw.andKeyEnabled, true);
                        if (index1 == -1)
                        {
                            if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt) && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) andKey = true;
                        }
                        else if (index1 == 0)
                        {
                            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) andKey = true;
                        }
                        else if (index1 == 1)
                        {
                            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) andKey = true;
                        }
                        else if (index1 == 2)
                        {
                            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) andKey = true;
                        }


                        if (!scKeyOff && andKey)
                        {
                            //　バイブの切替
                            if (Input.GetKeyDown(cfg.keyPluginToggleV4))
                            {
                                VLevel = 2;
                                Console.WriteLine("バイブ強");

                            }
                            else if (Input.GetKeyDown(cfg.keyPluginToggleV3))
                            {
                                VLevel = 1;
                                Console.WriteLine("バイブ弱");

                            }
                            else if (Input.GetKeyDown(cfg.keyPluginToggleV2))
                            {
                                VLevel = 0;
                                Console.WriteLine("バイブ停止");

                            }

                            //　メイド切替
                            if (Input.GetKeyDown(cfg.keyPluginToggleV5))
                            {
                                int bMaid = iCurrentMaid;
                                GetMaidCount();
                                if (maidDataList.Count > 1)
                                {
                                    VibeDataClear(1);

                                    ++iCurrentMaid;
                                    if (iCurrentMaid > maidDataList.Count - 1) { iCurrentMaid = 0; }
                                    bReGetMaid = true;
                                }

                                if (bMaid != iCurrentMaid && cfgw.CamChangeEnabled) CameraChange(SubMaids[maidDataList[iCurrentMaid]], SubMaids[maidDataList[bMaid]]);
                            }

                            //　男表示切替
                            if (Input.GetKeyDown(cfg.keyPluginToggleV6))
                            {
                                if (!man) { man = GameMain.Instance.CharacterMgr.GetMan(0); }
                                if (man)
                                {
                                    man.Visible = !man.Visible;
                                    if (man.Visible)
                                    {
                                        man.transform.position = maid.transform.position;
                                        man.transform.eulerAngles = maid.transform.eulerAngles;
                                        MotionChange(true);
                                    }
                                }
                            }


                            //　感度全開（デバッグ用）
                            /*if(Input.GetKeyDown(KeyCode.Z)) {
                              vOrgasmCount = 50;
                              vBoostBase = 50;
                            }*/


                            //　GUI表示の切り替え
                            if (Input.GetKeyDown(cfg.keyPluginToggleV1))
                            {
                                cfg.GuiFlag = cfg.GuiFlag + 1;
                                if (cfg.GuiFlag > 2) cfg.GuiFlag = 0;
                            }

                            //　一人称視点切り替え
                            if (Input.GetKeyDown(cfg.keyPluginToggleV7))
                            {
                                fpsModeEnabled = !fpsModeEnabled;

                                if (fpsModeEnabled && !man.Visible)
                                {
                                    if (!man) { man = GameMain.Instance.CharacterMgr.GetMan(0); }
                                    if (man)
                                    {
                                        man.Visible = !man.Visible;
                                        if (man.Visible)
                                        {
                                            man.transform.position = maid.transform.position;
                                            man.transform.eulerAngles = maid.transform.eulerAngles;
                                            MotionChange(true);
                                        }
                                    }
                                }
                            }

                            //　快感値ロック
                            if (Input.GetKeyDown(cfg.keyPluginToggleV8)) ExciteLock = !ExciteLock;

                            //　絶頂値ロック
                            if (Input.GetKeyDown(cfg.keyPluginToggleV9)) OrgasmLock = !OrgasmLock;

                            //　オートモード切り替え
                            if (Input.GetKeyDown(cfg.keyPluginToggleV10))
                            {

                                if (autoSelect == 0)
                                {
                                    autoTime1 = 0;
                                    autoTime2 = 0;
                                    autoTime3 = 0;
                                    autoTime4 = 0;
                                }
                                ++autoSelect;
                                if (autoSelect > 4) autoSelect = 0;

                                if (autoSelect == 0) VLevel = 0;

                            }

                        }


                        //―――――――――――――――――――――――
                        //Oculus Touchでの操作
                        //―――――――――――――――――――――――                  
                        /*if (bOculusVR){
                          foreach (DirectTouchController c in DirectTouchTool.Instance.controllers){

                            DeviceWrapper vrdeviceWrapper = c.vrdeviceWrapper;

                            bool trigger = vrdeviceWrapper.GetPress(EVRButtonId.k_EButton_SteamVR_Trigger); //トリガーを押している
                            bool triggerD = vrdeviceWrapper.GetPressDown(EVRButtonId.k_EButton_SteamVR_Trigger); //トリガーを押した
                            bool grip = vrdeviceWrapper.GetPress(EVRButtonId.k_EButton_Grip); //グリップを押している
                            bool gripD = vrdeviceWrapper.GetPressDown(EVRButtonId.k_EButton_Grip); //グリップを押した
                            bool touchpad = vrdeviceWrapper.GetPress(EVRButtonId.k_EButton_SteamVR_Touchpad); //タッチパッド（X/Aボタン）を押している
                            bool touchpadD = vrdeviceWrapper.GetPressDown(EVRButtonId.k_EButton_SteamVR_Touchpad); //タッチパッド（X/Aボタン）を押した
                            //bool menu = vrdeviceWrapper.GetPress(SteamVR_Controller.ButtonMask.ApplicationMenu);
                            //bool menuD = vrdeviceWrapper.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu);
                            Vector2 touch_pos = vrdeviceWrapper.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad);


                            if (grip && !trigger) {
                              if(touch_pos.y > 0.5) VLevel = 2;
                              if(touch_pos.y < -0.5) VLevel = 1;

                              Console.WriteLine("グリップオン　" + touch_pos.y);
                            }



                          }
                        }*/


                    }


                }
                else if (VLevel != 0)
                {

                    //　バイブ音停止
                    GameMain.Instance.SoundMgr.StopSe();

                    //　ステートをリセット
                    vStateMajor = 10;
                    vStateMinor = 0;
                    vStateMajorOld = 10;

                    //　興奮値をリセット
                    iCurrentExcite = 0;

                    //　オーバライド状態を解除
                    bIsVoiceOverridingV = false;
                    bOverrideInterruptedV = false;

                    //バイブ停止
                    VLevel = 0;

                }

            }

            //全体処理　終了---------------------------


            //　本プラグインの有効無効の切替
            if (Input.GetKeyDown(cfg.keyPluginToggleV0) && !scKeyOff)
            {
                cfg.bPluginEnabledV = !cfg.bPluginEnabledV;

                if (cfg.bPluginEnabledV)
                {
                    Console.WriteLine("VibeYourMaid Plugin Enabled.");
                }
                else
                {
                    Console.WriteLine("VibeYourMaid Plugin Disabled.");
                }


                // プラグイン無効化時の処理
                if (!cfg.bPluginEnabledV && sFaceAnimeBackupV != "")
                {

                    //　表情の復元
                    restoreFace();

                    //　ステートをリセット
                    vStateMajor = 10;
                    vStateMinor = 0;
                    vStateMajorOld = 10;

                    //　興奮値をリセット
                    iCurrentExcite = 0;

                    //　噴乳中なら停止
                    if (EnemaFlag)
                    {
                        updateShapeKeyEnemaValue(0f, 0f);
                        EnemaFlag = false;
                    }

                    //バイブ音停止
                    GameMain.Instance.SoundMgr.StopSe();

                    //　音声の復元もしくは停止
                    if (bIsVoiceOverridingV)
                    {

                        //　オーバライド状態を解除
                        bIsVoiceOverridingV = false;
                        bOverrideInterruptedV = false;

                        //　復元もしくは停止
                        if (sLoopVoiceBackupV != "")
                        {
                            maid.AudioMan.LoadPlay(sLoopVoiceBackupV, 0f, false, true);
                            debugPrintConsole("voice restore done. " + sLoopVoiceBackupV);

                        }
                        else
                        {
                            maid.AudioMan.Stop();
                            debugPrintConsole("voice stop done. " + sLoopVoiceBackupV);
                        }

                        sLoopVoiceBackupV = "";

                    }
                }
            }
        }


        void LateUpdate()
        {

            //赤面処理
            if (cfgw.HohoEnabled)
            {
                if (vOrgasmCmb > 3 || vMaidStun)
                {
                    SekimenValue = 1.0f;
                }
                else if (SekimenValue > 0)
                {
                    SekimenValue -= Time.deltaTime * 0.02f;
                }

                if (SekimenValue > 0 && maidActive)
                {
                    if (!cfgw.MaidLinkFaceEnabled)
                    {
                        //maid.body0.Face.morph.BlendValues[(int) maid.body0.Face.morph.hash[(object) "hoho2"]] = SekimenValue;
                        //maid.body0.Face.morph.FixBlendValues_Face();
                        try { VertexMorph_FromProcItem(this.maid.body0, "hoho2", SekimenValue); } catch { /*LogError(ex);*/ }
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            //M.body0.Face.morph.BlendValues[(int) M.body0.Face.morph.hash[(object) "hoho2"]] = SekimenValue;
                            //M.body0.Face.morph.FixBlendValues_Face();
                            try { VertexMorph_FromProcItem(M.body0, "hoho2", SekimenValue); } catch { /*LogError(ex);*/ }
                        }
                    }
                }
            }


            //口元の変更処理
            if (cfgw.MouthNomalEnabled || cfgw.MouthKissEnabled || cfgw.MouthFeraEnabled || cfgw.MouthZeccyouEnabled)
            {
                foreach (int i in maidDataList)
                {
                    if (i == maidDataList[iCurrentMaid] || (cfgw.MaidLinkFaceEnabled && i < 20 && SubMaids[i].Visible))
                    {

                        if (vStateMajor == 20 || vStateMajor == 30)
                        {
                            if (!cfgw.MouthKissEnabled && MouthMode[i] == 1) MouthMode[i] = 0;
                            if (!cfgw.MouthFeraEnabled && MouthMode[i] == 2) MouthMode[i] = 0;
                            if (!cfgw.MouthZeccyouEnabled && MouthMode[i] >= 3) MouthMode[i] = 0;

                            if (MouthMode[i] > 0)
                            {
                                MouthChange(i, MouthMode[i]);
                            }
                            else if (cfgw.MouthNomalEnabled)
                            {
                                MouthChange(i, MouthMode[i]);
                            }

                        }
                        else if (vStateMajor == 40 && cfgw.MouthNomalEnabled)
                        {
                            if (vMaidStun) MouthMode[i] = 3;
                            if (!vMaidStun) MouthMode[i] = 5;
                            MouthChange(i, MouthMode[i]);
                        }
                    }
                }
            }

            //シェイプキー処理の適用
            //if(maid != null)VertexMorph_FromProcItem_Fix(this.maid.body0);
            VertexMorph_FixBlendValues();


        }


        //　開始　---------------------------------------------------
        //　メイドさんの状態を変更する（フェイスアニメ、フェイスブレンド、モーション等）
        private void checkFaceDistance2()
        {

            // タイマー・数値増減の基本倍率(FPS変動に対応)
            float timerRate = Time.deltaTime * 60;

            //　バイブステートの変更
            if (VLevel == 2)
            { //　「バイブ強」
                if (vStateMajor != 30)
                {
                    ChangeSE(true);
                    vStateMajor = 30;
                }

            }
            else if (VLevel == 1)
            { //　「バイブ弱」
                if (vStateMajor != 20)
                {
                    ChangeSE(true);
                    vStateMajor = 20;
                }

            }
            else if (VLevel == 0)
            { //　「バイブ停止」        
                if (vStateMajor != 10 && vStateMajor != 40)
                {
                    GameMain.Instance.SoundMgr.StopSe();
                    vStateMajor = 40;

                    //感度や連続絶頂回数によって余韻時間増加
                    vStateAltTime2 = cfg.vStateAltTime2VBase + UnityEngine.Random.Range(0, cfg.vStateAltTime2VRandomExtend + 1) + Mathf.CeilToInt((float)vBoostGet) * 20 + Mathf.CeilToInt(Mathf.Sqrt((float)vOrgasmCmb)) * 60 + Mathf.CeilToInt((float)vOrgasmValue) + Mathf.CeilToInt((float)iCurrentExcite / 120);
                    if (vStateAltTime2 > 3600) { vStateAltTime2 = 3600; }
                    //Console.WriteLine("余韻時間：" + vStateAltTime2 / 60);
                }

            }

            //　「バイブ停止」から遷移してくる時には、その時点での表情などをバックアップしておく
            if (vStateMajor != 10 && vStateMajorOld == 10) backupFace();

            //　バイブステートが変わったら、時間カウンタをリセットする
            if (vStateMajor != vStateMajorOld)
            {
                //　時間カウンタのリセット
                vStateHoldTime = 0;
                vStateHoldTime2 = 0;
                vStateHoldTimeM = 0;
                //　表情変化時間のランダマイズ
                vStateAltTime1 = cfg.vStateAltTime1VBase + UnityEngine.Random.Range(0, cfg.vStateAltTime1VRandomExtend + 1);
                //Console.WriteLine("vStateAltTime1：" + vStateAltTime1);
            }


            //　時間ステートの変更
            if (vStateMajor == 10) vStateMinor = 0;

            //if(vStateMajor == 20 || vStateMajor == 40) {
            //　時間経過により、一方通行で表情変化      
            //vStateMinor = 0;
            //if(vStateAltTime1 <= vStateHoldTime) vStateMinor = 1;
            //}

            if (vStateMajor == 30 || vStateMajor == 20 || vStateMajor == 40)
            {
                vStateMinor = 0;
                if (vStateAltTime2 <= vStateHoldTime)
                {//表情・音声変更用時間カウンタ
                 //　時間カウンタのリセット
                    vStateHoldTime = 0;
                    //　表情変化時間のランダマイズ
                    vStateAltTime2 = cfg.vStateAltTime2VBase + UnityEngine.Random.Range(0, cfg.vStateAltTime2VRandomExtend + 1);
                    //Console.WriteLine("vStateAltTime2：" + vStateAltTime2);

                    //停止余韻状態の場合は完全な停止に（放心中は停止しない）
                    if (vStateMajor == 40 && !vMaidStun)
                    {
                        vStateMajor = 10;
                        vStateHoldTimeM = 0; //モーションタイマーも同時にリセット
                    }
                }

                if (vStateAltTimeM <= vStateHoldTimeM)
                {//モーション変更用時間カウンタ
                 //　時間カウンタのリセット
                    vStateHoldTimeM = 0;
                    //　モーション変化時間のランダマイズ
                    vStateAltTimeM = UnityEngine.Random.Range(200, 600);

                }

            }

            vState = vStateMajor + vStateMinor;


            //モーション変更処理
            if (vStateHoldTimeM <= 0)
            {
                MotionChange(false);

                if (cfgw.MaidLinkMotionEnabled)
                {
                    foreach (int i in maidDataList)
                    {
                        if (i < 20 && i != maidDataList[iCurrentMaid])
                        {
                            MotionChangeSub(i, false);
                        }
                    }
                }
            }


            //　興奮度の判定
            if (iCurrentExcite < cfg.vExciteLevelThresholdV1 * 60)
            {
                vExciteLevel = 1;
            }
            else if (cfg.vExciteLevelThresholdV1 * 60 <= iCurrentExcite && iCurrentExcite < cfg.vExciteLevelThresholdV2 * 60)
            {
                vExciteLevel = 2;
            }
            else if (cfg.vExciteLevelThresholdV2 * 60 <= iCurrentExcite && iCurrentExcite < cfg.vExciteLevelThresholdV3 * 60)
            {
                vExciteLevel = 3;
            }
            else if (cfg.vExciteLevelThresholdV3 * 60 <= iCurrentExcite)
            {
                vExciteLevel = 4;
            }



            double vExcitePlusBase = 0;     //興奮のベース加算値
            if (vStateMajor == 20)
            { //　バイブ弱時のベース加算値
                vExcitePlusBase = 13;
            }
            else if (vStateMajor == 30)
            { //　バイブ強時のベース加算値
                vExcitePlusBase = 20;
            }


            //抵抗値変動処理（同じバイブの強度を続けると抵抗値が上がる）
            if (vStateMajor != vStateMajorOld)
            {
                vResistBonus = 0; //強度が変わった時はリセット

            }
            else if (vStateMajor != 10)
            { //強度が同じ時は、経過時間により加算
                if (vStateHoldTime2 < 120)
                {  //開始2秒は減少
                    vResistBonus -= 0.02 * timerRate;
                }
                else if (vStateHoldTime2 < 2000)
                {
                    if (vStateMajor == 20)
                    {
                        vResistBonus += 0.01 * vExciteLevel * timerRate;
                    }
                    else if (vStateMajor == 30)
                    {
                        vResistBonus += 0.03 * vExciteLevel * timerRate;
                    }
                }
            }




            vBoostGet = vBoostBase + vBoostBonus;                                          //現在感度を計算
            vResistGet = vResistBase + vResistBonus + vExciteLevel * vExciteLevel - vBoostGet; //現在抵抗値を計算

            double vExcitePlus = (vExcitePlusBase - vResistGet) * Mathf.Sqrt((float)vBoostGet);    //興奮加算値を計算
            if (vExcitePlus < -1)
            {
                vExcitePlus = -1;
            }
            else if (vExcitePlus > 10 && vOrgasmCmb == 0)
            {
                vExcitePlus = 10;
            }
            else if (vExcitePlus > 20 && vOrgasmCmb >= 1)
            {
                vExcitePlus = 20;
            }
            else if (vExcitePlus > 30 && vOrgasmCmb >= 5)
            {
                vExcitePlus = 30;
            }


            //　興奮値、勃起値、変動処理---------------------------------------------
            int RandamValue;

            if (vStateMajor == 10 || vStateMajor == 40)
            { //　バイブ停止時　現在抵抗値に従って減少
              //興奮値を減算
                if (iCurrentExcite > 0 && !ExciteLock)
                {
                    iCurrentExcite -= 10 * vExciteLevel * timerRate;
                }

                if (iCurrentExcite < 0) { iCurrentExcite = 0; }

                //絶頂値を減算
                if (vOrgasmValue > 0 && !OrgasmLock)
                {
                    if (vMaidStun) vOrgasmValue -= 0.04 * timerRate;
                    if (!vMaidStun) vOrgasmValue -= 0.1 * timerRate;
                }

                //勃起値減算
                clitorisValue1 -= 0.05 * timerRate;

                //スタミナ回復
                if (vMaidStun) vMaidStamina += 25 * Time.deltaTime;
                if (!vMaidStun) vMaidStamina += 15 * Time.deltaTime;
                if (vMaidStamina > 3000) vMaidStamina = 3000;

            }
            else if (vStateMajor == 20)
            { //　バイブ弱時
              //興奮値を加算
                if (!ExciteLock) { iCurrentExcite += vExcitePlus * timerRate; }
                if (iCurrentExcite > 18000) { iCurrentExcite = 18000; }
                if (iCurrentExcite < 0) { iCurrentExcite = 0; }

                //感度加算判定　300分の1の確率で加算（現在興奮度により上昇値変動）
                if (!ExciteLock && !OrgasmLock)
                {
                    RandamValue = UnityEngine.Random.Range(0, (int)(300 / timerRate));
                    if (RandamValue < 1 && iCurrentExcite > 0)
                    {
                        vBoostBase = vBoostBase + 0.2 * vExciteLevel;
                    }
                }

                //絶頂値加算処理
                if (vExciteLevel > 1 && vOrgasmValue > 30 && !OrgasmLock)
                {
                    if (vMaidStun) vOrgasmValue -= 0.02 * timerRate;
                    if (!vMaidStun) vOrgasmValue -= 0.05 * timerRate;
                    vJirashi += Mathf.Sqrt((float)vBoostGet) * vOrgasmValue * 0.001 * timerRate;
                }
                //勃起値加算
                clitorisValue1 += 0.05 * timerRate;

                //スタミナ回復
                if (vMaidStun) vMaidStamina += 10 * Time.deltaTime;
                if (!vMaidStun) vMaidStamina += 6 * Time.deltaTime;
                if (vMaidStamina > 3000) vMaidStamina = 3000;

                //シェイプ
                ShapeKeyRandam(cfg.ShapeListR, cfg.RandamMin1, cfg.RandamMax1);
                ShapeKeyWave(cfg.ShapeListW, cfg.ShapeListW2, cfg.WaveMin1, cfg.WaveMax1, cfg.WaveSpead1);
                ShapeKeyIncrease(cfg.ShapeListI, cfg.IncreaseMax1, cfg.IncreaseSpead1);

            }
            else if (vStateMajor == 30)
            { //　バイブ強時
              //興奮値を加算
                if (!ExciteLock) { iCurrentExcite += vExcitePlus * timerRate; }
                if (iCurrentExcite > 18000) { iCurrentExcite = 18000; }
                if (iCurrentExcite < 0) { iCurrentExcite = 0; }


                //感度加算判定　300分の1の確率で0.1加算
                if (!ExciteLock && !OrgasmLock)
                {
                    RandamValue = UnityEngine.Random.Range(0, (int)(300 / timerRate));
                    if (RandamValue < 1 && iCurrentExcite > 0)
                    {
                        vBoostBase = vBoostBase + 0.1;
                    }
                }

                //絶頂値加算処理
                //vOrgasmValue += Mathf.Sqrt((float)vBoostGet * (vExciteLevel - 1)) * 0.02 * timerRate;
                if (vExciteLevel > 1 && !OrgasmLock)
                {
                    if (vMaidStun) vOrgasmValue += Mathf.Sqrt((float)vBoostGet) * 0.012 * timerRate;
                    if (!vMaidStun) vOrgasmValue += Mathf.Sqrt((float)vBoostGet) * 0.03 * timerRate;
                }
                //勃起値加算
                clitorisValue1 += 0.1 * timerRate;

                //スタミナ減少
                vMaidStamina -= (vExcitePlus + 1) * 0.33 * Time.deltaTime;
                if (vMaidStamina > 3000) vMaidStamina = 3000;
                if (vMaidStamina < 0) vMaidStamina = 0;

                //シェイプ
                ShapeKeyRandam(cfg.ShapeListR, cfg.RandamMin2, cfg.RandamMax2);
                ShapeKeyWave(cfg.ShapeListW, cfg.ShapeListW2, cfg.WaveMin2, cfg.WaveMax2, cfg.WaveSpead2);
                ShapeKeyIncrease(cfg.ShapeListI, cfg.IncreaseMax2, cfg.IncreaseSpead2);

            }
            //---------------------------------------------




            //　絶頂処理---------------------------------------------
            //絶頂に達した場合の処理
            if (vOrgasmValue >= 100 && vStateMajor == 30 && ((OrgasmVoice != 2 && vsFlag[0] != 2) || !cfgw.zViceWaitEnabled))
            {

                vOrgasmCount += 1;  //絶頂カウント加算
                vOrgasmCmb += 1;      //連続絶頂数加算
                vResistBonus = 0;
                if (!ExciteLock && !OrgasmLock)
                {
                    vBoostBase += 1;  //感度加算
                }

                //スタミナ減少
                vMaidStamina -= vBoostGet;
                if (vMaidStamina < 0) vMaidStamina = 0;

                vOrgasmHoldTime = 600;   //　絶頂後のボーナスタイム設定

                //連続絶頂の場合マウスモードをランダム変更
                if (vOrgasmCmb > 3)
                {
                    foreach (int i in maidDataList)
                    {
                        if (i < 20 && SubMaids[i].Visible)
                        {
                            MouthMode[i] = UnityEngine.Random.Range(2, 5);
                            if (MouthMode[i] < 3) MouthMode[i] = 0;


                        }
                    }
                }

                //興奮値を削減
                if (!ExciteLock)
                {
                    if (vOrgasmCmb > 3)
                    {
                        iCurrentExcite = iCurrentExcite * 0.8;
                    }
                    else
                    {
                        iCurrentExcite = iCurrentExcite * 0.5;
                    }
                }


                //噴乳開始設定
                //噴乳（胸）
                if (cfgw.BreastMilkEnabled && isExistVertexMorph(maid.body0, "breast_milk"))
                {
                    BreastWaitTime = UnityEngine.Random.Range(60, 90);  //噴乳までのタイムラグ設定
                    BreastTime = UnityEngine.Random.Range(180, 300);  //噴乳時間を設定
                    BreastFlag = true;  //噴乳処理ON
                }

                //噴乳（尻）
                if (cfgw.EnemaMilkEnabled && isExistVertexMorph(maid.body0, "enema_milk"))
                {
                    EnemaWaitTime = 120;  //噴乳までのタイムラグ設定
                    EnemaTime = UnityEngine.Random.Range(180, 300);  //噴乳時間を設定
                    EnemaFlag = true;  //噴乳処理ON
                }

                //射精
                if (cfgw.ChinpoMilkEnabled && isExistVertexMorph(maid.body0, "chinpo_milk"))
                {
                    if (vOrgasmCmb > 3 && MaxChinpoValue == 0)
                    {
                        ChinpoWaitTime = 0;  //射精のタイムラグ初期化
                        ChinpoTimes = 0;  //射精回数を初期化
                        MaxChinpoValue = UnityEngine.Random.Range(50, 80);
                        ChinpoFlag = true;  //噴乳処理ON

                    }
                    else if (vOrgasmCmb <= 3)
                    {
                        ChinpoWaitTime = UnityEngine.Random.Range(60, 90);  //射精のタイムラグ初期化
                        ChinpoTimes = 0;  //射精回数を初期化
                        MaxChinpoValue = 100;
                        ChinpoFlag = true;  //噴乳処理ON

                    }
                }


                //絶頂値リセット
                vOrgasmValue = 0;
                /*if (vExciteLevel == 2){
                  vOrgasmValue = 30;
                }else {
                  vOrgasmValue = 0;
                }*/


                SioFlag = true;
                if (vOrgasmCmb % 4 == 0 || vMaidStun) { StartNyo(); }

                //絶頂時の音声処理
                maid.AudioMan.Stop();     //現在の音声停止
                OrgasmVoice = 1;          //絶頂時音声フラグON
                vStateHoldTime = 0;       //音声をすぐ再生するため、タイマーリセット
                vStateHoldTimeM = 0;       //モーション用タイマーリセット

                //アヘ値の変更
                AheValue2 = vOrgasmCmb * 10;
                if (AheValue2 > 60) { AheValue2 = UnityEngine.Random.Range(0, 60); }

                //絶頂時のモーションに変更
                ZeccyouAnim();

                if (cfgw.MaidLinkMotionEnabled)
                {
                    foreach (int i in maidDataList)
                    {
                        if (i < 20 && i != maidDataList[iCurrentMaid])
                        {
                            ZeccyouAnimSub(i);
                        }
                    }
                }

                //オートモード時、絶頂後すぐにモーションが変わらないように時間追加
                if (autoSelect != 0)
                {
                    if (autoTime1 < 100) autoTime1 += 100;
                    if (autoTime2 < 100) autoTime2 += 100;
                    if (autoTime3 < 100) autoTime3 += 100;
                }

            }

            //絶頂ボーナスタイムの処理
            if (vOrgasmHoldTime > 0)
            {
                vBoostBonus = vJirashi / 20 + 3 * vOrgasmCmb;  //感度ボーナス設定

                if (vOrgasmValue < 100) vOrgasmHoldTime -= timerRate;

                if (vOrgasmHoldTime <= 0)
                { //ボーナスタイム終了時の処理
                    vJirashi = 0;
                    vBoostBonus = 0;
                    vOrgasmCmb = 0;
                    vOrgasmValue = vOrgasmValue / 3;
                    updateShapeKeyOrgasmValue(0f);

                    //噴乳処理OFF
                    if (BreastFlag)
                    {
                        updateShapeKeyBreastValue(0f, 0f);
                        BreastFlag = false;
                    }
                    if (EnemaFlag)
                    {
                        updateShapeKeyEnemaValue(0f, 0f);
                        EnemaFlag = false;
                    }
                    if (ChinpoFlag)
                    {
                        updateShapeKeyChinpoValue(0f, 0f);
                        ChinpoFlag = false;
                    }

                }
            }

            //絶頂音声が終わった時の処理
            if (OrgasmVoice == 2 && !maid.AudioMan.audiosource.isPlaying)
            {
                OrgasmVoice = 0;          //絶頂時音声フラグOFF
                vStateHoldTime = 0;       //音声をすぐ再生するため、タイマーリセット
            }


            //---------------------------------------------

            //感度の上限設定
            if (vOrgasmCount < 15)
            {
                if (vBoostBase > 15) { vBoostBase = 15; }
            }
            else
            {
                if (vBoostBase > 50) { vBoostBase = 50; }
            }

            if (vBoostBonus > 200) { vBoostBonus = 200; }


            //メイド状態のセーブ処理
            if (vBoostBaseSave[MaidNum] < vBoostBase / 2) { vBoostBaseSave[MaidNum] = vBoostBase / 2; }
            if (vOrgasmCountSave[MaidNum] < vOrgasmCount) { vOrgasmCountSave[MaidNum] = vOrgasmCount; }


            //瞳のアヘ処理
            double AheValue3 = AheValue2;

            if (vOrgasmCmb > 0)
            {
                if (vBoostBase - 15 > 0 && vExciteLevel >= 2)
                {
                    AheValue3 = AheValue2 + vBoostBase / 3;
                    if (vMaidStun) AheValue3 += 15;
                }
                if (AheValue3 > 60) { AheValue3 = 60; }

                if (AheValue < AheValue3)
                {
                    AheValue += 0.1 * timerRate;
                }
                else if (AheValue > AheValue3)
                {
                    AheValue -= 0.1 * timerRate;
                }

            }
            else if ((vBoostBase - 15 > 0 && vExciteLevel >= 2) || vMaidStun)
            {
                AheValue3 = vBoostBase / 2;
                if (vMaidStun) AheValue3 += 15;

                if (AheValue < AheValue3)
                {
                    AheValue += 0.1 * timerRate;
                }
                else if (AheValue > AheValue3)
                {
                    AheValue -= 0.1 * timerRate;
                }

            }
            else if (AheValue > 0)
            {
                AheValue -= 0.05 * timerRate;
            }

            updateMaidEyePosY((float)AheValue);




            //秘部アニメーション処理
            updateHibuAnime();


            //痙攣処理
            float OlgValue = 0f;

            if (vOrgasmValue > 90)
            { //絶頂前痙攣

                OlgValue = UnityEngine.Random.Range(-cfg.orgasmValue1, cfg.orgasmValue1);
                updateShapeKeyOrgasmValue(OlgValue);
                if (!OlgFlag2) { OlgFlag2 = !OlgFlag2; }

            }
            else if (OlgFlag2)
            {
                updateShapeKeyOrgasmValue(0f);
                OlgFlag2 = !OlgFlag2;
            }

            if (vOrgasmCmb > 3)
            { //絶頂痙攣 強

                if (OlgTime <= 0)
                {

                    OlgFlag = !OlgFlag;
                    if (OlgFlag)
                    {
                        OlgTime = UnityEngine.Random.Range(0, 120);
                    }
                    else
                    {
                        OlgTime = UnityEngine.Random.Range(0, 30);
                    }

                }
                else
                {

                    if (OlgFlag)
                    {
                        OlgValue = UnityEngine.Random.Range(-cfg.orgasmValue3, cfg.orgasmValue3);
                        updateShapeKeyOrgasmValue(OlgValue);
                    }
                    OlgTime -= timerRate;

                }


            }
            else if (vOrgasmCmb > 0)
            { //絶頂痙攣 弱

                if (OlgTime <= 0)
                {

                    OlgFlag = !OlgFlag;
                    if (OlgFlag)
                    {
                        OlgTime = UnityEngine.Random.Range(0, 120);
                    }
                    else
                    {
                        OlgTime = UnityEngine.Random.Range(0, 30);
                    }

                }
                else
                {

                    if (OlgFlag)
                    {
                        OlgValue = UnityEngine.Random.Range(-cfg.orgasmValue2, cfg.orgasmValue2);
                        updateShapeKeyOrgasmValue(OlgValue);
                    }
                    OlgTime -= timerRate;

                }

            }
            else if (vMaidStun)
            { //痙攣 放心中

                if (OlgTime <= 0)
                {

                    OlgFlag = !OlgFlag;
                    if (OlgFlag)
                    {
                        OlgTime = UnityEngine.Random.Range(0, 40);
                    }
                    else
                    {
                        OlgTime = UnityEngine.Random.Range(0, 300);
                    }

                }
                else
                {

                    if (OlgFlag)
                    {
                        OlgValue = UnityEngine.Random.Range(-cfg.orgasmValue1, cfg.orgasmValue1);
                        updateShapeKeyOrgasmValue(OlgValue);
                    }
                    OlgTime -= timerRate;

                }

            }



            //クリトリス勃起処理
            updateShapeKeyCliValue(OlgValue);

            //汗かき処理
            updateShapeKeyAseValue();


            //噴乳処理　移植---------------------------------------

            //噴乳（胸）
            float BreastValue;
            float MaxBreastValue;
            if (BreastFlag)
            {
                if (BreastWaitTime > 0)
                {
                    //噴乳までのタイムラグ
                    if (BreastWaitTime < 30)
                    {
                        //出始め
                        MaxBreastValue = 100f - BreastWaitTime * 3;
                        BreastValue = UnityEngine.Random.Range(MaxBreastValue - 30, MaxBreastValue);
                        updateShapeKeyBreastValue(BreastValue, 6f);
                    }
                    else
                    {
                        updateShapeKeyBreastValue(0f, 0f);
                    }
                    BreastWaitTime -= timerRate;
                }
                else
                {
                    //噴乳中
                    if (BreastTime > 100)
                    {
                        MaxBreastValue = 100f;
                    }
                    else
                    {
                        MaxBreastValue = BreastTime;
                    }
                    BreastValue = UnityEngine.Random.Range(MaxBreastValue - 50, MaxBreastValue);
                    updateShapeKeyBreastValue(BreastValue, 9f);

                    BreastTime -= timerRate;
                    if (BreastTime <= 0)
                    {
                        BreastTime = UnityEngine.Random.Range(20, 60);
                    }
                }
            }

            //噴乳（尻）
            float EnemaValue;
            float MaxEnemaValue;
            if (EnemaFlag)
            {
                if (EnemaWaitTime > 0)
                {
                    //噴乳までのタイムラグ
                    if (EnemaWaitTime < 30)
                    {
                        //出始め
                        MaxEnemaValue = 100f - EnemaWaitTime * 3;
                        EnemaValue = UnityEngine.Random.Range(MaxEnemaValue - 30, MaxEnemaValue);
                        updateShapeKeyEnemaValue(EnemaValue, 6f);
                    }
                    else
                    {
                        updateShapeKeyEnemaValue(0f, 0f);
                    }
                    EnemaWaitTime -= timerRate;
                }
                else
                {
                    //噴乳中
                    if (EnemaTime > 100)
                    {
                        MaxEnemaValue = 100f;
                    }
                    else
                    {
                        MaxEnemaValue = EnemaTime;
                    }
                    EnemaValue = UnityEngine.Random.Range(MaxEnemaValue - 50, MaxEnemaValue);
                    updateShapeKeyEnemaValue(EnemaValue, 12f);

                    EnemaTime -= timerRate;
                    if (EnemaTime <= 0)
                    {
                        //ランダムで噴乳繰り返し
                        if (UnityEngine.Random.Range(1, 20) <= 1)
                        {
                            EnemaTime = UnityEngine.Random.Range(20, 60);
                        }
                        else
                        {
                            updateShapeKeyEnemaValue(0f, 0f);
                        }
                    }
                }
            }

            //射精
            if (ChinpoFlag)
            {
                if (ChinpoWaitTime > 0)
                {
                    updateShapeKeyChinpoValue(0f, 0f);
                    ChinpoWaitTime -= timerRate;

                }
                else
                {
                    if (ChinpoTimes <= 0)
                    {
                        ChinpoTimes = UnityEngine.Random.Range(1, 4);  //射精回数を設定
                        ChinpoValue = MaxChinpoValue + UnityEngine.Random.Range(10, 20);
                        if (ChinpoValue > 100) { ChinpoValue = 100; }
                        MaxChinpoValue = MaxChinpoValue - UnityEngine.Random.Range(7, 13) * ChinpoTimes;
                        if (MaxChinpoValue < 0) { MaxChinpoValue = 0; }
                    }
                    else
                    {

                        //射精中
                        if (ChinpoValue > 80)
                        {
                            updateShapeKeyChinpoValue(ChinpoValue, 10f);
                        }
                        else if (ChinpoValue > 30)
                        {
                            updateShapeKeyChinpoValue(ChinpoValue, 6f);
                        }
                        else
                        {
                            updateShapeKeyChinpoValue(ChinpoValue, 1.5f);
                        }

                        if (ChinpoTimes <= 0)
                        {
                            ChinpoWaitTime = UnityEngine.Random.Range(30, 60);
                        }

                    }
                }
            }
            //噴乳処理　移植終了---------------------------------------


            //メイドの放心判定
            if (!vMaidStun && vMaidStamina < 500)
            {
                vMaidStun = true;
                vOrgasmValue = 0;  //絶頂値リセット
                if (!cfgw.MaidLinkFaceEnabled)
                {
                    maid.EyeToCamera((Maid.EyeMoveType)0, 0.8f);
                }
                else
                {
                    foreach (Maid M in SubMaids)
                    {
                        M.EyeToCamera((Maid.EyeMoveType)0, 0.8f);
                    }
                }

                //ハイライト消去
                /*int nFileNameRID = strMenuFileName.ToLower().GetHashCode();
                maid.SetProp("eye_hi", "_I_SkinHi002.menu", nFileNameRID, true);
                maid.AllProcPropSeqStart();//変更適用 (menuによる他のPartの処理など)*/

            }
            if (vMaidStun && vMaidStamina > 1500)
            {
                vMaidStun = false;
                vStateHoldTime = 0;  //時間カウンタのリセット

                //ハイライトを戻す
                /*maid.ResetProp("eye_hi");
                maid.AllProcPropSeqStart();//変更適用 (menuによる他のPartの処理など)*/
            }


            //　ループ音声の適用

            //　ループ音声が流れているときはバックアップする（オーバライドしたものは除く）
            //　夜伽前の会話シーンなど、ループ音声が拾えない時もあるので注意すること
            if (cfg.bVoiceOverrideEnabledV)
            {

                //　ループ音声のバックアップ（通常版）
                if (!bChuBLip && !bIsVoiceOverridingV && maid.AudioMan.audiosource.loop && maid.AudioMan.audiosource.isPlaying)
                {
                    bOverrideInterruptedV = false;
                    sLoopVoiceBackupV = maid.AudioMan.FileName;
                    debugPrintConsole("voice backup done. " + sLoopVoiceBackupV);
                }

                //　一回再生音声のバックアップ（ChuBLip版）
                if (bChuBLip && !bIsVoiceOverridingV)
                {
                    bOverrideInterruptedV = false;
                    sLoopVoiceBackupV = maid.AudioMan.FileName;
                    debugPrintConsole("voice backup done. " + sLoopVoiceBackupV);
                }




                //　音声オーバライド判定開始ここから               
                bool bAllowVoiceOverrideV = false;

                if (vStateMajor == 30 || vStateMajor == 20 || vStateMajor == 40)
                {
                    // 割り込みされた後、バックアップが拾えてない状況ではない
                    //if(!bOverrideInterruptedV) {
                    //　音声オーバライド未だ or 時間カウントリセット時
                    if (!bIsVoiceOverridingV || vStateHoldTime <= 0)
                    {
                        //　ループ音声を再生中、もしくは一回再生音声が再生済みなら介入してよい
                        if (maid.AudioMan.audiosource.loop || (!maid.AudioMan.audiosource.loop && !maid.AudioMan.audiosource.isPlaying))
                        {
                            bAllowVoiceOverrideV = true;
                        }

                        //　ChuBLip版では、ループ音声が夜伽で使われないので、上の条件によらず許可する
                        if (bChuBLip)
                        {
                            bAllowVoiceOverrideV = true;
                        }

                    }

                    //}
                }



                //　上記でオーバーライドが許可されたら実際に再生する
                if (bAllowVoiceOverrideV)
                {
                    bIsVoiceOverridingV = true;

                    //メインメイドの音声再生
                    MaidVoicePlay(maid, maidDataList[iCurrentMaid]);

                    //サブメイドの音声再生
                    if (cfgw.MaidLinkVoiceEnabled)
                    {
                        foreach (int i in maidDataList)
                        {
                            if (i < 20 && i != maidDataList[iCurrentMaid])
                            {
                                MaidVoicePlay(SubMaids[i], i);
                            }
                        }
                    }


                    //　再生を始めたファイル名を記憶
                    sLoopVoiceOverridingV = maid.AudioMan.FileName;

                }



                //　音声の割り込み判定
                //　音声オーバライド状態において、一回再生音声に割り込まれたら、
                //　オーバライド状態を一度解除し、バックアップ音声を拾い直す（キスしながらイッてぐったりしたメイドさんに、イク前の発情ボイスを復元してしまうといった事故を防ぐ）
                if (bIsVoiceOverridingV)
                {
                    //　音を切り替えるタイミングではない
                    if ((vStateMajor == 30 && vStateHoldTime > 0)
                        || (vStateMajor == 20 && vStateHoldTime >= vStateAltTime1)
                        || (vStateMajor == 40 && vStateHoldTime >= vStateAltTime1))
                    {
                        //　再生中の音声が、オーバライドした音声と一致しない
                        if (maid.AudioMan.FileName != sLoopVoiceOverridingV)
                        {
                            bOverrideInterruptedV = true;
                            bIsVoiceOverridingV = false;
                            sLoopVoiceBackupV = "";
                            debugPrintConsole("override interrupted." + sLoopVoiceBackupV);
                        }
                    }
                }


                //　音声オーバライドの停止と復元
                if (vStateMajor == 10 && bIsVoiceOverridingV)
                {

                    //　オーバライド状態を解除
                    bIsVoiceOverridingV = false;
                    bOverrideInterruptedV = false;

                    //　バイブ音停止
                    GameMain.Instance.SoundMgr.StopSe();

                    if (!bChuBLip)
                    {
                        //　復元もしくは停止
                        if (sLoopVoiceBackupV != "")
                        {
                            maid.AudioMan.LoadPlay(sLoopVoiceBackupV, 0f, false, true);
                            debugPrintConsole("voice restore done. " + sLoopVoiceBackupV);
                        }
                        else
                        {
                            maid.AudioMan.Stop();
                            debugPrintConsole("voice stop done. " + sLoopVoiceBackupV);
                        }
                    }
                    else
                    {
                        //　Chu-B Lip版は復元をしない（現状、音声セリフ付きの音を復元してしまい不自然になることがある）

                    }
                    sLoopVoiceBackupV = "";
                }


            }








            //　バイブフェイスアニメの適用
            bool bAllowChangeFaceAnime = false;

            //　遷移直後かカウンタリセット時のタイミングで適用
            if ((vStateHoldTime <= 0)
                //|| (vStateMajor == 20 && vStateHoldTime >= vStateAltTime1)
                //|| (vStateMajor == 40 && vStateHoldTime >= vStateAltTime1)
                )
            {
                bAllowChangeFaceAnime = true;
            }

            int iRandomFace = 0;
            if (bAllowChangeFaceAnime)
            {
                string sFaceAnimeName = "";

                if (vMaidStun)
                {
                    iRandomFace = UnityEngine.Random.Range(0, cfg.sFaceAnimeStun.Length);
                    sFaceAnimeName = cfg.sFaceAnimeStun[iRandomFace];

                }
                else if (vState == 20)
                {
                    iRandomFace = UnityEngine.Random.Range(0, cfg.sFaceAnime20Vibe[vExciteLevel - 1].Length);
                    sFaceAnimeName = cfg.sFaceAnime20Vibe[vExciteLevel - 1][iRandomFace];

                }
                else if (vState == 40)
                {
                    if (vOrgasmCmb > 0)
                    {
                        sFaceAnimeName = cfg.sFaceAnime40Vibe[3];
                    }
                    else
                    {
                        sFaceAnimeName = cfg.sFaceAnime40Vibe[vExciteLevel - 1];
                    }

                }
                else if (vState == 30)
                {
                    iRandomFace = UnityEngine.Random.Range(0, cfg.sFaceAnime30Vibe[vExciteLevel - 1].Length);
                    sFaceAnimeName = cfg.sFaceAnime30Vibe[vExciteLevel - 1][iRandomFace];
                }

                //　""か"変更しない"でなければ、フェイスアニメを適用する
                if (sFaceAnimeName != "" && sFaceAnimeName != "変更しない")
                {
                    maid.FaceAnime(sFaceAnimeName, cfg.fAnimeFadeTimeV, 0);
                }


                //サブメイドにフェイスアニメ適用
                if (cfgw.MaidLinkFaceEnabled)
                {
                    foreach (int i in maidDataList)
                    {
                        if (i < 20 && i != maidDataList[iCurrentMaid])
                        {

                            if (vState == 20)
                            {
                                iRandomFace = UnityEngine.Random.Range(0, cfg.sFaceAnime20Vibe[vExciteLevel - 1].Length);
                                sFaceAnimeName = cfg.sFaceAnime20Vibe[vExciteLevel - 1][iRandomFace];

                            }
                            else if (vState == 40)
                            {
                                if (vOrgasmCmb > 0)
                                {
                                    sFaceAnimeName = cfg.sFaceAnime40Vibe[3];
                                }
                                else
                                {
                                    sFaceAnimeName = cfg.sFaceAnime40Vibe[vExciteLevel - 1];
                                }

                            }
                            else if (vState == 30)
                            {
                                iRandomFace = UnityEngine.Random.Range(0, cfg.sFaceAnime30Vibe[vExciteLevel - 1].Length);
                                sFaceAnimeName = cfg.sFaceAnime30Vibe[vExciteLevel - 1][iRandomFace];
                            }

                            if (sFaceAnimeName != "" && sFaceAnimeName != "変更しない")
                            {
                                SubMaids[i].FaceAnime(sFaceAnimeName, cfg.fAnimeFadeTimeV, 0);
                            }

                        }
                    }
                }

                //Console.WriteLine("フェイスアニメ：" + sFaceAnimeName);

            }



            //　フェイスブレンドの適用
            //　ステートに応じたフェイスブレンドに上書きする。
            //　ただし、より強いものが適用されるなら、そちらを尊重して上書きしない

            string sFaceBlendCurrent = maid.FaceName3;
            sFaceBlendCurrent = sFaceBlendCurrent.Replace("オリジナル", ""); //取得したフェイスブレンド情報から「オリジナル」の記述を削除


            if (sFaceBlendCurrent == "") sFaceBlendCurrent = "頬０涙０";  // 背景選択時、スキル選択時は、"" が返ってきてエラーが出るため

            string sCurrentCheek = "";
            string sCurrentTears = "";
            int iCurrentCheek = 0;
            int iCurrentTears = 0;
            bool bCurrentYodare = false;

            string sChangeCheek = "";
            string sChangeTears = "";
            int iChangeCheek = 0;
            int iChangeTears = 0;
            string sChangeYodare = "";
            string sChangeBlend = "";

            int iOverrideCheek = 0;
            int iOverrideTears = 0;
            bool bOverrideYodare = false;


            //　興奮度によってフェイスブレンドを変更する
            if (vExciteLevel == 1)
            {
                iOverrideCheek = 1;     //"頬１"
                iOverrideTears = 1;     //"涙１"

            }
            else if (vExciteLevel == 2)
            {
                iOverrideCheek = 2;     //"頬２"
                iOverrideTears = 1;     //"涙１"

            }
            else if (vExciteLevel == 3)
            {
                iOverrideCheek = 3;     //"頬３"
                iOverrideTears = 2;     //"涙２"

            }
            else if (vExciteLevel == 4)
            {
                iOverrideCheek = 3;     //"頬３"
                iOverrideTears = 3;     //"涙３"

            }


            //　よだれ（興奮レベルが一定以上の時にだけよだれをつける）
            if (cfg.iYodareAppearLevelV != 0 && vExciteLevel >= cfg.iYodareAppearLevelV)
            {
                bOverrideYodare = true;
            }
            else if (vOrgasmCmb > 0 || vMaidStun)
            {
                bOverrideYodare = true;
            }
            else
            {
                bOverrideYodare = false;
            }

            //　元々のフェイスブレンドと比較する
            sCurrentCheek = sFaceBlendCurrent.Substring(0, 2);
            if (sCurrentCheek == "頬０") iCurrentCheek = 0;
            if (sCurrentCheek == "頬１") iCurrentCheek = 1;
            if (sCurrentCheek == "頬２") iCurrentCheek = 2;
            if (sCurrentCheek == "頬３") iCurrentCheek = 3;
            iChangeCheek = iCurrentCheek;
            if (iOverrideCheek > iChangeCheek) iChangeCheek = iOverrideCheek;
            if (iChangeCheek == 0) sChangeCheek = "頬０";
            if (iChangeCheek == 1) sChangeCheek = "頬１";
            if (iChangeCheek == 2) sChangeCheek = "頬２";
            if (iChangeCheek == 3) sChangeCheek = "頬３";

            sCurrentTears = sFaceBlendCurrent.Substring(2, 2);
            if (sCurrentTears == "涙０") iCurrentTears = 0;
            if (sCurrentTears == "涙１") iCurrentTears = 1;
            if (sCurrentTears == "涙２") iCurrentTears = 2;
            if (sCurrentTears == "涙３") iCurrentTears = 3;
            iChangeTears = iCurrentTears;
            if (iOverrideTears > iChangeTears) iChangeTears = iOverrideTears;
            if (iChangeTears == 0) sChangeTears = "涙０";
            if (iChangeTears == 1) sChangeTears = "涙１";
            if (iChangeTears == 2) sChangeTears = "涙２";
            if (iChangeTears == 3) sChangeTears = "涙３";

            if (sFaceBlendCurrent.Length == 7) bCurrentYodare = true;
            if (bCurrentYodare || bOverrideYodare) sChangeYodare = "よだれ";

            //設定により各ブレンドを除外
            if (!cfgw.HohoEnabled) sChangeCheek = sCurrentCheek;
            if (!cfgw.NamidaEnabled) sChangeTears = sCurrentTears;
            if (!cfgw.YodareEnabled)
            {
                if (bCurrentYodare) sChangeYodare = "よだれ";
                if (!bCurrentYodare) sChangeYodare = "";

            }
            sChangeBlend = sChangeCheek + sChangeTears + sChangeYodare;

            //メインメイドにフェイスブレンド適用
            maid.FaceBlend(sChangeBlend);

            //サブメイドにフェイスブレンド適用
            if (cfgw.MaidLinkFaceEnabled)
            {
                foreach (int i in maidDataList)
                {
                    if (i < 20 && i != maidDataList[iCurrentMaid])
                    {
                        SubMaids[i].FaceBlend(sChangeBlend);
                    }
                }
            }


            //debugPrintConsole("FaceBlendLevel=" + iChangeCheek + " , " + iChangeTears + " , " + sChangeYodare);


            //　停止時にフェイスアニメ・フェイスブレンドを復元し、スタートフラグを初期化する
            if (vStateMajor == 10 && vStateMajorOld != 10)
            {
                restoreFace();
                StartFlag = false;
            }

            //　バイブステートの過去値を更新
            vStateMajorOld = vStateMajor;

            //　バイブステート保持時間を加算する（18000でカンスト）
            vStateHoldTime += timerRate;
            if (vStateHoldTime > 3600) vStateHoldTime = 3600;
            vStateHoldTime2 += timerRate;
            if (vStateHoldTime2 > 6000) vStateHoldTime2 = 6000;
            vStateHoldTimeM += timerRate;
            if (vStateHoldTimeM > 6000) vStateHoldTimeM = 6000;



//            debugPrintConsole("Level=" + vSceneLevel + " bIsYotogi=" + bIsYotogiScene + " vState=" + vState);
        }
        //メイドさんの状態変更　終了　---------------------------------------------------





        //　フェイスアニメ・フェイスブレンドのバックアップ
        private void backupFace()
        {
            sFaceAnimeBackupV = maid.ActiveFace;
            sFaceBlendBackupV = maid.FaceName3;

            //Console.WriteLine(sFaceBlendBackupV);

            debugPrintConsole("face backup done. " + sFaceAnimeBackupV + " , " + sFaceBlendBackupV);
        }



        //　フェイスアニメ・フェイスブレンドの復元
        private void restoreFace()
        {
            if (sFaceAnimeBackupV != "")
            {
                if (maid) //@API実装時//複数メイドプラグインでメイドを増やした時にNULLエラーがでることがあるのでチェック追加
                    maid.FaceAnime(sFaceAnimeBackupV, cfg.fAnimeFadeTimeV * 2, 0);

            }

            maid.FaceBlend(sFaceBlendBackupV);

            sFaceAnimeBackupV = "";
            sFaceBlendBackupV = "";

            debugPrintConsole("face restore done. " + sFaceAnimeBackupV + " , " + sFaceBlendBackupV);
            //if (bDebug) Console.WriteLine("face restore done. {0}, {1}", sFaceAnimeBackupV, sFaceBlendBackupV);
        }



        //　夜伽シーンにいるかをチェック
        private void checkYotogiScene()
        {
            // OH版は夜伽シーンでもSceneが14にならない(10のまま)ので、YotogiManagerの有無で判別する
            int iYotogiManagerCount = FindObjectsOfType<YotogiManager>().Length;
            bIsYotogiScene = false;
            if (iYotogiManagerCount > 0) bIsYotogiScene = true;
        }



        //フェラしてるかチェック
        private void checkBlowjobing(Maid m, int Num)
        {

            if (maid)
            {
                //メイドさんのモーションファイル名に含まれる文字列で判別させる
                if (OrgasmVoice == 0)
                {
                    sLastAnimeFileName = m.body0.LastAnimeFN;
                }
                else if (Num == maidDataList[iCurrentMaid] && cfgw.ZeccyouAnimeEnabled)
                {
                    sLastAnimeFileName = ZAnimeFileName[Num];
                }
                else if (Num != maidDataList[iCurrentMaid] && cfgw.ZeccyouAnimeEnabled && cfgw.MaidLinkMotionEnabled)
                {
                    sLastAnimeFileName = ZAnimeFileName[Num];
                }


                if (sLastAnimeFileName != null)
                {

                    bIsBlowjobing[Num] = 0;

                    if (sLastAnimeFileName.Contains("fera")) bIsBlowjobing[Num] = 2; //フェラ
                    if (sLastAnimeFileName.Contains("sixnine")) bIsBlowjobing[Num] = 2;　//シックスナイン
                    if (sLastAnimeFileName.Contains("_ir_")) bIsBlowjobing[Num] = 2;　//イラマ
                    if (sLastAnimeFileName.Contains("_kuti")) bIsBlowjobing[Num] = 2;　//乱交３Ｐ
                    if (sLastAnimeFileName.Contains("housi")) bIsBlowjobing[Num] = 2;　//乱交奉仕
                    if (sLastAnimeFileName.Contains("kiss")) bIsBlowjobing[Num] = 1;　//キス
                    if (sLastAnimeFileName.Contains("ran4p")) bIsBlowjobing[Num] = 2; //乱交４Ｐ

                    if (sLastAnimeFileName.Contains("taiki")) bIsBlowjobing[Num] = 0;　//待機中は含めない
                    if (sLastAnimeFileName.Contains("shaseigo")) bIsBlowjobing[Num] = 0;　//射精後は含めない
                    if (sLastAnimeFileName.Contains("surituke")) bIsBlowjobing[Num] = 1;　//乱交３Ｐ擦り付け時は咥えないのでは含めない
                    if (sLastAnimeFileName.Contains("siriname")) bIsBlowjobing[Num] = 1;　//尻舐めはキス扱い
                    if (sLastAnimeFileName.Contains("asiname")) bIsBlowjobing[Num] = 1;　//足舐めはキス扱い
                    if (sLastAnimeFileName.Contains("tikubiname")) bIsBlowjobing[Num] = 1;　//乳首舐めはキス扱い

                    if (sLastAnimeFileName.Contains("ir_in_taiki")) bIsBlowjobing[Num] = 2;　//咥え始めはフェラに含める
                    if (sLastAnimeFileName.Contains("dt_in_taiki")) bIsBlowjobing[Num] = 2;　//咥え始めはフェラに含める
                    if (sLastAnimeFileName.Contains("kuti_in_taiki")) bIsBlowjobing[Num] = 2;　//咥え始めはフェラに含める
                    if (sLastAnimeFileName.Contains("kutia_in_taiki")) bIsBlowjobing[Num] = 2;　//咥え始めはフェラに含める

                    sLastAnimeFileNameOld = sLastAnimeFileName;

                    //メインメイドの場合はマウスモードを切り替える
                    //if(Num == maidDataList[iCurrentMaid]){
                    if (bIsBlowjobing[Num] == 0 && vOrgasmCmb <= 3)
                    {  //0の時は連続絶頂中じゃなければ切り替える
                        if (vBoostBase > 40)
                        {  //感度が40以上の時はランダムで歯を食いしばる
                            MouthMode[Num] = UnityEngine.Random.Range(2, 5);
                            if (MouthMode[Num] < 4) MouthMode[Num] = 0;
                        }
                        else
                        {
                            MouthMode[Num] = bIsBlowjobing[Num];
                        }
                    }

                    if (bIsBlowjobing[Num] == 1 && cfgw.MouthKissEnabled) MouthMode[Num] = bIsBlowjobing[Num]; //1の時はキスが有効なら切り替える
                    if (bIsBlowjobing[Num] == 2 && cfgw.MouthFeraEnabled) MouthMode[Num] = bIsBlowjobing[Num]; //2の時はフェラが有効なら切り替える

                    if (vMaidStun) MouthMode[Num] = 3;  //放心中は無条件でアヘらせる
                                                        //}

                    //カメラが顔に近づいている場合、キスに変更
                    if (cameraCheck[Num] && bIsBlowjobing[Num] == 0)
                    {
                        bIsBlowjobing[Num] = 1;
                        if (autoSelect != 0)
                        {

                            //キスモーションへの変更（試作のため無効化）
                            /*kissChange = -1;
                            int i = 0;
                            foreach (string str in autoMotionList[ami1]){
                              if(str.IndexOf("kiss") != -1){
                                kissChange = i;
                                Console.WriteLine("キス判定:" + kissChange);
                                break;
                              }
                              ++i;
                            }*/

                        }
                        //if(cfgw.MouthKissEnabled && Num == maidDataList[iCurrentMaid] && !vMaidStun){
                        if (cfgw.MouthKissEnabled && !vMaidStun)
                        {
                            MouthMode[Num] = 1;
                        }
                    }


                    //フェラの時は顔をカメラに向けないようにする
                    if (bIsBlowjobing[Num] == 2)
                    {
                        m.EyeToCamera((Maid.EyeMoveType)0, 0.8f);
                    }

                }
            }
        }



        private void StartNyo()
        {
            if (cfgw.NyoEnabled)
            {
                foreach (int i in maidDataList)
                {
                    SubMaids[i].AddPrefab("Particle/pNyou_cm3D2", "pNyou_cm3D2", "_IK_vagina", new Vector3(0f, -0.047f, 0.011f), new Vector3(20.0f, -180.0f, 180.0f));
                    GameMain.Instance.SoundMgr.PlaySe("SE011.ogg", false);
                }
            }
        }


        private bool SioFlag = false;
        private bool SioFlag2 = false;
        private float SioTime = 0;
        private float SioTime2 = 0;
        private void StartSio()
        {
            float timerRate = Time.deltaTime * 60;

            if (cfgw.SioEnabled)
            {

                if (SioFlag)
                {
                    if (!SioFlag2)
                    {
                        SioTime = 300f;
                        SioFlag2 = true;
                    }
                    if (SioTime2 <= 0)
                    {
                        //GameMain.Instance.SoundMgr.PlaySe("SE018.ogg", false);
                        foreach (int i in maidDataList)
                        {
                            SubMaids[i].AddPrefab("Particle/pSio2_cm3D2", "pSio2_cm3D2", "_IK_vagina", new Vector3(0f, 0f, -0.01f), new Vector3(0f, 180.0f, 0f));
                        }
                        SioTime2 = UnityEngine.Random.Range(10f, 90f);
                    }
                    SioTime -= timerRate;
                    SioTime2 -= timerRate;
                    if (SioTime <= 0)
                    {
                        SioFlag = false;
                        SioFlag2 = false;
                        SioTime2 = 0;
                    }
                }
            }
        }


        //瞳操作
        private void updateMaidEyePosY(float value)
        {
            if (cfgw.AheEnabled)
            {
                if (value < 0f) value = 0f;

                foreach (int i in maidDataList)
                {
                    if (i == maidDataList[iCurrentMaid] || cfgw.MaidLinkFaceEnabled)
                    {
                        Vector3 vl = SubMaids[i].body0.trsEyeL.localPosition;
                        Vector3 vr = SubMaids[i].body0.trsEyeR.localPosition;
                        SubMaids[i].body0.trsEyeL.localPosition = new Vector3(vl.x, Math.Max((fAheDefEye + value) / fEyePosToSliderMul, 0f), vl.z);
                        SubMaids[i].body0.trsEyeR.localPosition = new Vector3(vr.x, Math.Min((fAheDefEye - value) / fEyePosToSliderMul, 0f), vr.z);
                    }
                    else if (AheResetFlag2)
                    {
                        Vector3 vl = SubMaids[i].body0.trsEyeL.localPosition;
                        Vector3 vr = SubMaids[i].body0.trsEyeR.localPosition;
                        SubMaids[i].body0.trsEyeL.localPosition = new Vector3(vl.x, Math.Max((fAheDefEye + 0f) / fEyePosToSliderMul, 0f), vl.z);
                        SubMaids[i].body0.trsEyeR.localPosition = new Vector3(vr.x, Math.Min((fAheDefEye - 0f) / fEyePosToSliderMul, 0f), vr.z);
                    }
                }

                if (!AheResetFlag) AheResetFlag = true;
                if (!AheResetFlag2 && cfgw.MaidLinkFaceEnabled) AheResetFlag2 = true;
                if (AheResetFlag2 && !cfgw.MaidLinkFaceEnabled) AheResetFlag2 = false;

            }
            else if (AheResetFlag)
            {
                foreach (int i in maidDataList)
                {
                    if (i == maidDataList[iCurrentMaid] || cfgw.MaidLinkFaceEnabled)
                    {
                        Vector3 vl = SubMaids[i].body0.trsEyeL.localPosition;
                        Vector3 vr = SubMaids[i].body0.trsEyeR.localPosition;
                        SubMaids[i].body0.trsEyeL.localPosition = new Vector3(vl.x, Math.Max((fAheDefEye + 0f) / fEyePosToSliderMul, 0f), vl.z);
                        SubMaids[i].body0.trsEyeR.localPosition = new Vector3(vr.x, Math.Min((fAheDefEye - 0f) / fEyePosToSliderMul, 0f), vr.z);
                    }
                }

                AheResetFlag = false;
                //Console.WriteLine("瞳位置リセット");
            }
        }



        //痙攣操作
        private bool OrgasmResetFlag = false;
        private void updateShapeKeyOrgasmValue(float value)
        {
            if (cfgw.OrgsmAnimeEnabled)
            {
                try
                {
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, "orgasm", value / 100f);
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, "orgasm", value / 100f);
                        }
                    }
                }
                catch { /*LogError(ex);*/ }

                if (!OrgasmResetFlag) { OrgasmResetFlag = true; }

            }
            else if (OrgasmResetFlag)
            {

                try
                {
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, "orgasm", 0f);
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, "orgasm", 0f);
                        }
                    }
                }
                catch { /*LogError(ex);*/ }

                OrgasmResetFlag = false;
                //Console.WriteLine("痙攣リセット");
            }
        }



        //噴乳(胸)操作
        private void updateShapeKeyBreastValue(float max, float sp)
        {
            float timerRate = Time.deltaTime * 60;

            if (cfgw.BreastMilkEnabled)
            {
                sp *= Time.deltaTime * ShapeKeySpeedRate;

                ShapeKeyBreastValue = ShapeKeyBreastValue + sp;

                if (ShapeKeyBreastValue > max)
                {
                    ShapeKeyBreastValue = 0f;
                }
                else if (ShapeKeyBreastValue < 0f)
                {
                    ShapeKeyBreastValue = max;
                }

                if (BreastStopTime > 0)
                {
                    ShapeKeyBreastValue = 0f;
                    BreastStopTime -= timerRate;
                }
                else if (UnityEngine.Random.Range(1, 50) <= 1)
                {
                    BreastStopTime = UnityEngine.Random.Range(20, 40);
                }

                if ((ShapeKeyBreastValue >= 16 && ShapeKeyBreastValue <= 22)
                   || (ShapeKeyBreastValue >= 48 && ShapeKeyBreastValue <= 54))
                {
                    GameMain.Instance.SoundMgr.PlaySe("se028.ogg", false);
                }

                try
                {
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, "breast_milk", ShapeKeyBreastValue / 100f);
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, "breast_milk", ShapeKeyBreastValue / 100f);
                        }
                    }
                }
                catch { /*LogError(ex);*/ }

            }
            else if (BreastFlag)
            {
                BreastFlag = false;

                try
                {
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, "breast_milk", 0f);
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, "breast_milk", 0f);
                        }
                    }
                }
                catch { /*LogError(ex);*/ }

            }
        }



        //噴乳(尻)操作
        private void updateShapeKeyEnemaValue(float max, float sp)
        {
            float timerRate = Time.deltaTime * 60;

            if (cfgw.EnemaMilkEnabled)
            {
                sp *= Time.deltaTime * ShapeKeySpeedRate;

                ShapeKeyEnemaValue = ShapeKeyEnemaValue + sp;

                if (ShapeKeyEnemaValue > max)
                {
                    ShapeKeyEnemaValue = 0f;
                }
                else if (ShapeKeyEnemaValue < 0f)
                {
                    ShapeKeyEnemaValue = max;
                }

                if (EnemaStopTime > 0)
                {
                    ShapeKeyEnemaValue = 0f;
                    EnemaStopTime -= timerRate;
                }
                else if (UnityEngine.Random.Range(1, 50) <= 1)
                {
                    EnemaStopTime = UnityEngine.Random.Range(7, 12);
                }

                if ((ShapeKeyEnemaValue >= 20 && ShapeKeyEnemaValue <= 28)
                   || (ShapeKeyEnemaValue >= 44 && ShapeKeyEnemaValue <= 52)
                   || (ShapeKeyEnemaValue >= 70 && ShapeKeyEnemaValue <= 78))
                {
                    GameMain.Instance.SoundMgr.PlaySe("se016.ogg", false);
                }

                try
                {
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, "enema_milk", ShapeKeyEnemaValue / 100f);
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, "enema_milk", ShapeKeyEnemaValue / 100f);
                        }
                    }
                }
                catch { /*LogError(ex);*/ }

            }
            else if (EnemaFlag)
            {
                EnemaFlag = false;

                try
                {
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, "enema_milk", 0f);
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, "enema_milk", 0f);
                        }
                    }
                }
                catch { /*LogError(ex);*/ }

            }
        }


        //射精操作
        private void updateShapeKeyChinpoValue(float max, float sp)
        {
            float timerRate = Time.deltaTime * 60;

            if (cfgw.ChinpoMilkEnabled)
            {

                if (ShapeKeyChinpoValue == 0 && ChinpoTimes > 0 && max > 20)
                {
                    GameMain.Instance.SoundMgr.PlaySe("se016.ogg", false);
                }

                sp *= Time.deltaTime * ShapeKeySpeedRate;
                ShapeKeyChinpoValue = ShapeKeyChinpoValue + sp;

                if (ShapeKeyChinpoValue > max)
                {
                    ShapeKeyChinpoValue = 0f;
                    ChinpoTimes -= 1;
                }

                try
                {
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, "chinpo_milk", ShapeKeyChinpoValue / 100f);
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, "chinpo_milk", ShapeKeyChinpoValue / 100f);
                        }
                    }
                }
                catch { /*LogError(ex);*/ }

            }
            else if (ChinpoFlag)
            {
                ChinpoFlag = false;

                try
                {
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, "chinpo_milk", 0f);
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, "chinpo_milk", 0f);
                        }
                    }
                }
                catch { /*LogError(ex);*/ }

            }
        }


        //汗かき処理
        private bool aseResetFlag = false;
        private float aseTime = 0f;
        private void updateShapeKeyAseValue()
        {

            float timerRate = Time.deltaTime * 60;

            if (cfgw.aseAnimeEnabled)
            {
                double aseValue1;
                double aseValue2;
                double aseValue3;

                if (aseTime <= 0)
                {

                    aseValue1 = Math.Floor(110 - ((vBoostBase * 2) + (iCurrentExcite / 360)));
                    if (aseValue1 < 0) aseValue1 = 0;
                    if (aseValue1 > 100) aseValue1 = 100;

                    aseValue2 = Math.Floor((3000 - vMaidStamina) / 100);

                    aseValue3 = Math.Floor(vOrgasmValue / 3);

                    try
                    {
                        if (!cfgw.MaidLinkShapeEnabled)
                        {
                            VertexMorph_FromProcItem(this.maid.body0, "dry", (float)aseValue1 / 100f);
                            VertexMorph_FromProcItem(this.maid.body0, "swet", (float)aseValue2 / 100f);
                            VertexMorph_FromProcItem(this.maid.body0, "swet_tare", (float)aseValue3 / 100f);
                        }
                        else
                        {
                            foreach (Maid M in SubMaids)
                            {
                                VertexMorph_FromProcItem(M.body0, "dry", (float)aseValue1 / 100f);
                                VertexMorph_FromProcItem(M.body0, "swet", (float)aseValue2 / 100f);
                                VertexMorph_FromProcItem(M.body0, "swet_tare", (float)aseValue3 / 100f);
                            }
                        }
                    }
                    catch { /*LogError(ex);*/ }

                    if (!aseResetFlag) aseResetFlag = true;

                    aseTime = 60;

                }

                aseTime -= timerRate;

            }
            else if (aseResetFlag)
            {
                try
                {
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, "dry", 0f);
                        VertexMorph_FromProcItem(this.maid.body0, "swet", 0f);
                        VertexMorph_FromProcItem(this.maid.body0, "swet_tare", 0f);
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, "dry", 0f);
                            VertexMorph_FromProcItem(M.body0, "swet", 0f);
                            VertexMorph_FromProcItem(M.body0, "swet_tare", 0f);
                        }
                    }
                }
                catch { /*LogError(ex);*/ }

                aseResetFlag = false;

            }
        }


        //クリ勃起操作
        private bool CliResetFlag = false;
        private void updateShapeKeyCliValue(float value)
        {
            if (cfgw.CliAnimeEnabled)
            {
                double clitorisValue2;
                clitorisValue2 = 30 + vBoostBase * 3 + vOrgasmCount * 3;
                if (clitorisValue2 > cfg.clitorisMax) { clitorisValue2 = cfg.clitorisMax; }
                if (clitorisValue1 > clitorisValue2) { clitorisValue1 = clitorisValue2; }
                if (clitorisValue1 < 0) { clitorisValue1 = 0; }

                try
                {
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, "clitoris", (float)clitorisValue1 / 100f + value / 400f);
                        //VertexMorph_FromProcItem(this.maid.body0, "chikubi_bokki", (float)clitorisValue1/500f + value/2000f);  //自分用
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, "clitoris", (float)clitorisValue1 / 100f + value / 400f);
                            //VertexMorph_FromProcItem(M.body0, "chikubi_bokki", (float)clitorisValue1/500f + value/2000f);  //自分用
                        }
                    }
                }
                catch { /*LogError(ex);*/ }

                if (!CliResetFlag) { CliResetFlag = true; }

            }
            else if (CliResetFlag)
            {

                try
                {
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, "clitoris", 0f);
                        //VertexMorph_FromProcItem(this.maid.body0, "chikubi_bokki", 0f);  //自分用
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, "clitoris", 0f);
                            //VertexMorph_FromProcItem(M.body0, "chikubi_bokki", 0f);  //自分用
                        }
                    }
                }
                catch { /*LogError(ex);*/ }

                CliResetFlag = false;
                //Console.WriteLine("クリトリスリセット");

            }
        }

        //ちんぽ勃起操作
        private bool ChinpoResetFlag = false;
        private void updateShapeKeyChinpoValue()
        {

            float timerRate = Time.deltaTime * 60;

            if (cfgw.ChinpoAnimeEnabled)
            {
                float ChinpoValue2;
                //float SoriValue2;
                //float MilkSoriValue2;

                ChinpoValue2 = (float)iCurrentExcite / (60f * 300f / (cfg.ChinpoMax - cfg.ChinpoMin));
                if (ChinpoValue2 > ChinpoValue1) { ChinpoValue1 += 0.05f * timerRate; }
                if (ChinpoValue2 < ChinpoValue1) { ChinpoValue1 -= 0.05f * timerRate; }

                SoriValue1 = ChinpoValue1 / ((cfg.ChinpoMax - cfg.ChinpoMin) / (cfg.SoriMax - cfg.SoriMin)) + cfg.SoriMin;

                try
                {
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, "FUTA", (100f - ChinpoValue1 - cfg.ChinpoMin) / 100f);
                        VertexMorph_FromProcItem(this.maid.body0, "futa_sori", (100f - SoriValue1) / 100f);
                        VertexMorph_FromProcItem(this.maid.body0, "tamatama", (100f - cfg.TamaValue) / 100f);
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, "FUTA", (100f - ChinpoValue1 - cfg.ChinpoMin) / 100f);
                            VertexMorph_FromProcItem(M.body0, "futa_sori", (100f - SoriValue1) / 100f);
                            VertexMorph_FromProcItem(M.body0, "tamatama", (100f - cfg.TamaValue) / 100f);
                        }
                    }
                }
                catch { /*LogError(ex);*/ }

                if (!ChinpoResetFlag) { ChinpoResetFlag = true; }

            }
            else if (ChinpoResetFlag)
            {

                try
                {
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, "FUTA", (100f - cfg.ChinpoMin) / 100f);
                        VertexMorph_FromProcItem(this.maid.body0, "futa_sori", (100f - cfg.SoriMin) / 100f);
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, "FUTA", (100f - cfg.ChinpoMin) / 100f);
                            VertexMorph_FromProcItem(M.body0, "futa_sori", (100f - cfg.SoriMin) / 100f);
                        }
                    }
                }
                catch { /*LogError(ex);*/ }

                ChinpoResetFlag = false;

            }
        }

        //秘部アニメーション処理
        private bool PikuFlag = false;
        private float PikuTime = 0;
        private float PikuTime2 = 0;
        private float labiaValue = 0;
        private void updateHibuAnime()
        {

            float timerRate = Time.deltaTime * 60;

            //バイブ動作時
            if (cfgw.hibuAnime1Enabled)
            {
                if (vStateMajor == 20 || vStateMajor == 30)
                {

                    labiaValue = cfgw.hibuSlider1Value;
                    if (labiaValue > 25) labiaValue = 25;
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        try
                        {
                            VertexMorph_FromProcItem(this.maid.body0, "kupa", cfgw.hibuSlider1Value / 100f + ShapeKeyWaveValue / 10000f * cfgw.kupaWave);
                            VertexMorph_FromProcItem(this.maid.body0, "analkupa", cfgw.analSlider1Value / 100f + ShapeKeyWaveValue / 10000f * cfgw.kupaWave);
                            VertexMorph_FromProcItem(this.maid.body0, "labiakupa", labiaValue / 100f);
                        }
                        catch { /*LogError(ex);*/ }
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            try
                            {
                                VertexMorph_FromProcItem(M.body0, "kupa", cfgw.hibuSlider1Value / 100f + ShapeKeyWaveValue / 10000f * cfgw.kupaWave);
                                VertexMorph_FromProcItem(M.body0, "analkupa", cfgw.analSlider1Value / 100f + ShapeKeyWaveValue / 10000f * cfgw.kupaWave);
                                VertexMorph_FromProcItem(M.body0, "labiakupa", labiaValue / 100f);
                            }
                            catch { /*LogError(ex);*/ }
                        }
                    }

                    if (PikuTime <= 0)
                    {

                        PikuFlag = !PikuFlag;
                        if (PikuFlag)
                        {
                            PikuTime = UnityEngine.Random.Range(0, 90);
                            PikuTime2 = PikuTime;
                        }
                        else
                        {
                            PikuTime = UnityEngine.Random.Range(30, 210);
                        }

                    }
                    else
                    {
                        if (PikuFlag)
                        {
                            if (PikuTime2 - PikuTime > 1)
                            {

                                chituWave(true);
                                float analtwitchValue = UnityEngine.Random.Range(0f, 30f);

                                try
                                {
                                    if (!cfgw.MaidLinkShapeEnabled)
                                    {
                                        VertexMorph_FromProcItem(this.maid.body0, "analtwitch3", analtwitchValue / 100f);
                                    }
                                    else
                                    {
                                        foreach (Maid M in SubMaids)
                                        {
                                            VertexMorph_FromProcItem(M.body0, "analtwitch3", analtwitchValue / 100f);
                                        }
                                    }
                                }
                                catch { /*LogError(ex);*/ }

                                PikuTime2 = PikuTime;
                            }
                        }
                        else
                        {
                            chituWave(false);
                            try
                            {
                                if (!cfgw.MaidLinkShapeEnabled)
                                {
                                    VertexMorph_FromProcItem(this.maid.body0, "analtwitch3", 0f);
                                }
                                else
                                {
                                    foreach (Maid M in SubMaids)
                                    {
                                        VertexMorph_FromProcItem(M.body0, "analtwitch3", 0f);
                                    }
                                }
                            }
                            catch { /*LogError(ex);*/ }
                        }
                        PikuTime -= timerRate;
                    }

                    //自分用
                    /*if (!cfgw.MaidLinkShapeEnabled) {
                      try {
                        VertexMorph_FromProcItem(this.maid.body0, "kupu", 0.8f);
                        VertexMorph_FromProcItem(this.maid.body0, "analkupu", 0.8f);
                      } catch {  }
                    }else{
                      foreach (Maid M in SubMaids){
                        try {
                          VertexMorph_FromProcItem(M.body0, "kupu", 0.8f);
                          VertexMorph_FromProcItem(M.body0, "analkupu", 0.8f);
                        } catch {  }
                      }
                    }*/
                }
            }

            //バイブ停止時
            if (cfgw.hibuAnime2Enabled && vStateMajor == 40)
            {

                if (PikuTime <= 0)
                {

                    PikuFlag = !PikuFlag;
                    if (PikuFlag)
                    {
                        PikuTime = UnityEngine.Random.Range(0, 60);
                        PikuTime2 = PikuTime;
                    }
                    else
                    {
                        PikuTime = UnityEngine.Random.Range(30, 90);
                    }

                }
                else
                {

                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, "orgasm_vagina", 0f);
                        //VertexMorph_FromProcItem(this.maid.body0, "kupu", 0f);        //自分用
                        //VertexMorph_FromProcItem(this.maid.body0, "analkupu", 0f);    //自分用
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            try
                            {
                                VertexMorph_FromProcItem(M.body0, "orgasm_vagina", 0f);
                                //VertexMorph_FromProcItem(M.body0, "kupu", 0f);         //自分用
                                //VertexMorph_FromProcItem(M.body0, "analkupu", 0f);     //自分用
                            }
                            catch { }
                        }
                    }

                    if (PikuFlag)
                    {

                        if (PikuTime2 - PikuTime > 2)
                        {
                            float PikuValue = UnityEngine.Random.Range(0, 5);
                            try
                            {
                                if (!cfgw.MaidLinkShapeEnabled)
                                {
                                    VertexMorph_FromProcItem(this.maid.body0, "kupa", (cfgw.hibuSlider2Value + PikuValue) / 100f);
                                    VertexMorph_FromProcItem(this.maid.body0, "analkupa", (cfgw.analSlider2Value + PikuValue) / 100f);
                                }
                                else
                                {
                                    foreach (Maid M in SubMaids)
                                    {
                                        VertexMorph_FromProcItem(M.body0, "kupa", (cfgw.hibuSlider2Value + PikuValue) / 100f);
                                        VertexMorph_FromProcItem(M.body0, "analkupa", (cfgw.analSlider2Value + PikuValue) / 100f);
                                    }
                                }
                            }
                            catch { /*LogError(ex);*/ }

                            PikuTime2 = PikuTime;
                        }

                    }
                    else
                    {
                        try
                        {
                            if (!cfgw.MaidLinkShapeEnabled)
                            {
                                VertexMorph_FromProcItem(this.maid.body0, "kupa", cfgw.hibuSlider2Value / 100f);
                                VertexMorph_FromProcItem(this.maid.body0, "analkupa", cfgw.analSlider2Value / 100f);
                            }
                            else
                            {
                                foreach (Maid M in SubMaids)
                                {
                                    VertexMorph_FromProcItem(M.body0, "kupa", cfgw.hibuSlider2Value / 100f);
                                    VertexMorph_FromProcItem(M.body0, "analkupa", cfgw.analSlider2Value / 100f);
                                }
                            }
                        }
                        catch { /*LogError(ex);*/ }
                    }
                    PikuTime -= timerRate;
                }

            }

            if (labiaValue > 0)
            {
                labiaValue -= 0.2f;
                if (labiaValue < 0) labiaValue = 0;
                try
                {
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, "labiakupa", labiaValue / 100f);
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, "labiakupa", labiaValue / 100f);
                        }
                    }

                }
                catch { /*LogError(ex);*/ }
            }

        }


        //膣内アニメ処理
        private bool chituWaveFlag = true;
        private float chituWaveValue = 0f;
        private void chituWave(bool piku)
        {
            float sp = Time.deltaTime * ShapeKeySpeedRate;
            float min = 0f;
            float max = 100f;

            if (vStateMajor == 20) sp *= 2.5f;
            if (vStateMajor == 30) sp *= 3f;

            if (chituWaveFlag)
            {
                chituWaveValue = chituWaveValue + sp;

                if (chituWaveValue > max)
                {
                    chituWaveValue = max;
                    chituWaveFlag = !chituWaveFlag;
                }
            }
            else
            {
                chituWaveValue = chituWaveValue - sp;

                if (chituWaveValue < min)
                {
                    chituWaveValue = min;
                    chituWaveFlag = !chituWaveFlag;
                }
            }

            float chituValue = chituWaveValue * 1.5f;
            if (piku) chituValue = UnityEngine.Random.Range(chituValue + 0f, chituValue + 40f);

            if (!cfgw.MaidLinkShapeEnabled)
            {
                try
                {
                    VertexMorph_FromProcItem(this.maid.body0, "orgasm_vagina", chituValue / 100f);
                }
                catch { /*LogError(ex);*/ }
            }
            else
            {
                foreach (Maid M in SubMaids)
                {
                    try
                    {
                        VertexMorph_FromProcItem(M.body0, "orgasm_vagina", chituValue / 100f);
                    }
                    catch { }
                }
            }

        }


        //シェイプランダム
        private void ShapeKeyRandam(string[] name, float min, float max)
        {
            ShapeKeyRandomDelta += Time.deltaTime;
            if (ShapeKeyRandomDelta < ShapeKeyRandomInterval)
            {
                return;
            }
            ShapeKeyRandomDelta -= ShapeKeyRandomInterval;

            for (int i = 0; i < name.Length; i++)
            {
                float value;
                value = UnityEngine.Random.Range(min, max);

                try
                {
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, name[i], value / 100f);
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, name[i], value / 100f);
                        }
                    }
                }
                catch { /*LogError(ex);*/ }
            }
        }


        //シェイプウェイブ
        private void ShapeKeyWave(string[] name, string[] name2, float min, float max, float sp)
        {
            sp *= Time.deltaTime * ShapeKeySpeedRate;

            if (ShapeKeyWaveFlag)
            {
                ShapeKeyWaveValue = ShapeKeyWaveValue + sp;

                if (ShapeKeyWaveValue > max)
                {
                    ShapeKeyWaveValue = max;
                    ShapeKeyWaveFlag = !ShapeKeyWaveFlag;
                }
            }
            else
            {
                ShapeKeyWaveValue = ShapeKeyWaveValue - sp;

                if (ShapeKeyWaveValue < min)
                {
                    ShapeKeyWaveValue = min;
                    ShapeKeyWaveFlag = !ShapeKeyWaveFlag;
                }
            }


            for (int i = 0; i < name.Length; i++)
            {
                try
                {
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, name[i], ShapeKeyWaveValue / 100f);
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, name[i], ShapeKeyWaveValue / 100f);
                        }
                    }
                }
                catch { /*LogError(ex);*/ }
            }

            float rValue = max - ShapeKeyWaveValue;
            for (int i = 0; i < name2.Length; i++)
            {
                try
                {
                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, name2[i], rValue / 100f);
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, name2[i], rValue / 100f);
                        }
                    }


                }
                catch { /*LogError(ex);*/ }
            }
        }


        //シェイプ増加
        private void ShapeKeyIncrease(string[] name, float max, float sp)
        {
            sp *= Time.deltaTime * ShapeKeySpeedRate;

            ShapeKeyIncreaseValue = ShapeKeyIncreaseValue + sp;

            if (ShapeKeyIncreaseValue > max)
            {

                ShapeKeyIncreaseValue = 0f;

            }
            else if (ShapeKeyIncreaseValue < 0f)
            {

                ShapeKeyIncreaseValue = max;

            }

            for (int i = 0; i < name.Length; i++)
            {
                try
                {

                    if (!cfgw.MaidLinkShapeEnabled)
                    {
                        VertexMorph_FromProcItem(this.maid.body0, name[i], ShapeKeyIncreaseValue / 100f);
                    }
                    else
                    {
                        foreach (Maid M in SubMaids)
                        {
                            VertexMorph_FromProcItem(M.body0, name[i], ShapeKeyIncreaseValue / 100f);
                        }
                    }
                }
                catch { /*LogError(ex);*/ }
            }

        }



        //メイドの口元変更
        private float[] MouthHoldTime = new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
        //private int MouthMode = 0;
        private int[] MouthMode = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private int[] OldMode = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private float[] MaValue = new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
        private float[] MiValue = new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
        //private float McValue;
        private float[] MdwValue = new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
        private float[] TupValue = new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
        private float[] ToutValue = new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
        private float[] TopenValue = new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
        private float[] TupValue2 = new float[] { 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f };
        private float[] ToutValue2 = new float[] { 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f };
        private float[] TopenValue2 = new float[] { 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f };

        private float[] maVBack = new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
        private float[] mdwVBack = new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };

        private void MouthChange(int Num, int mode)
        {

            float timerRate = Time.deltaTime * 60;
            Maid M = SubMaids[Num];

            //          float maV = M.body0.Face.morph.BlendValues[(int) M.body0.Face.morph.hash[(object) "moutha"]]; //口あ
            float maV = M.body0.Face.morph.GetBlendValues((int)M.body0.Face.morph.hash[(object)"moutha"]); //口あ
            float miV; //口い
            float mcV; //口う
            float msV; //笑顔
            float mdwV = M.body0.Face.morph.GetBlendValues((int)M.body0.Face.morph.hash[(object)"mouthdw"]); //口角上げ
            float mupV; //口角下げ

            if (mode != OldMode[Num])
            {
                MouthHoldTime[Num] = 0;
                OldMode[Num] = mode;
            }

            if (MouthHoldTime[Num] <= 0)
            {
                MouthHoldTime[Num] = UnityEngine.Random.Range(180f, 360f);

                if (mode == 0)
                {  //通常時
                    MaValue[Num] = UnityEngine.Random.Range(0f, 30f) / 100f;
                    MdwValue[Num] = UnityEngine.Random.Range(0f, 30f) / 100f;
                }
                if (mode == 1)
                {  //キス時
                    MaValue[Num] = UnityEngine.Random.Range(20f, 60f) / 100f;
                    MdwValue[Num] = UnityEngine.Random.Range(0f, 50f) / 100f;
                }
                if (mode == 2)
                {  //フェラ時
                    MaValue[Num] = UnityEngine.Random.Range(80f, 100f) / 100f;
                }
                if (mode == 3)
                {  //連続絶頂時１
                    MaValue[Num] = UnityEngine.Random.Range(70f, 90f) / 100f;
                    MdwValue[Num] = UnityEngine.Random.Range(30f, 90f) / 100f;
                }
                if (mode == 4)
                {  //連続絶頂時２
                    MiValue[Num] = UnityEngine.Random.Range(30f, 50f) / 100f;
                    MdwValue[Num] = UnityEngine.Random.Range(20f, 40f) / 100f;
                }
                if (mode == 5)
                {  //余韻時
                    MaValue[Num] = UnityEngine.Random.Range(10f, 40f) / 100f;
                    MdwValue[Num] = UnityEngine.Random.Range(0f, 30f) / 100f;
                }

            }

            MouthHoldTime[Num] -= timerRate;

            if (maVBack[Num] > maV)
            {
                maV += MaValue[Num];
                maVBack[Num] = maV;
            }
            miV = M.body0.Face.morph.GetBlendValues((int)M.body0.Face.morph.hash[(object)"mouthi"]) + MiValue[Num];
            mcV = M.body0.Face.morph.GetBlendValues((int)M.body0.Face.morph.hash[(object)"mouthc"]);
            msV = M.body0.Face.morph.GetBlendValues((int)M.body0.Face.morph.hash[(object)"mouths"]);
            if (mdwVBack[Num] > mdwV)
            {
                mdwV += MdwValue[Num];
                mdwVBack[Num] = mdwV;
            }
            mupV = M.body0.Face.morph.GetBlendValues((int)M.body0.Face.morph.hash[(object)"mouthup"]);


            //舌の動き処理
            //キス時とフェラ時
            if (mode == 1 || mode == 2)
            {
                if (TupValue[Num] < TupValue2[Num])
                {
                    TupValue[Num] += Time.deltaTime * 0.5f;
                    if (TupValue[Num] >= TupValue2[Num]) { TupValue2[Num] = UnityEngine.Random.Range(0f, 60f) / 100f; }
                }
                else
                {
                    TupValue[Num] -= Time.deltaTime * 0.5f;
                    if (TupValue[Num] <= TupValue2[Num]) { TupValue2[Num] = UnityEngine.Random.Range(0f, 60f) / 100f; }
                }

                if (ToutValue[Num] < ToutValue2[Num])
                {
                    ToutValue[Num] += Time.deltaTime * 0.8f;
                    if (ToutValue[Num] >= ToutValue2[Num])
                    {
                        if (mode == 1) ToutValue2[Num] = UnityEngine.Random.Range(0f, 60f) / 100f;
                        if (mode == 2) ToutValue2[Num] = UnityEngine.Random.Range(-20f, 80f) / 100f;
                    }
                }
                else
                {
                    ToutValue[Num] -= Time.deltaTime * 0.8f;
                    if (ToutValue[Num] <= ToutValue2[Num])
                    {
                        if (mode == 1) ToutValue2[Num] = UnityEngine.Random.Range(0f, 60f) / 100f;
                        if (mode == 2) ToutValue2[Num] = UnityEngine.Random.Range(-20f, 80f) / 100f;
                    }
                }

                if (TopenValue[Num] < TopenValue2[Num])
                {
                    TopenValue[Num] += Time.deltaTime * 0.5f;
                    if (TopenValue[Num] >= TopenValue2[Num]) { TopenValue2[Num] = UnityEngine.Random.Range(0f, 40f) / 100f; }
                }
                else
                {
                    TopenValue[Num] -= Time.deltaTime * 0.5f;
                    if (TopenValue[Num] <= TopenValue2[Num]) { TopenValue2[Num] = UnityEngine.Random.Range(0f, 40f) / 100f; }
                }
            }
            //連続絶頂時
            if (mode == 3)
            {
                if (TupValue[Num] < TupValue2[Num])
                {
                    TupValue[Num] += Time.deltaTime * 0.5f;
                    if (TupValue[Num] >= TupValue2[Num]) { TupValue2[Num] = UnityEngine.Random.Range(0f, 40f) / 100f; }
                }
                else
                {
                    TupValue[Num] -= Time.deltaTime * 0.5f;
                    if (TupValue[Num] <= TupValue2[Num]) { TupValue2[Num] = UnityEngine.Random.Range(0f, 40f) / 100f; }
                }

                if (ToutValue[Num] < ToutValue2[Num])
                {
                    ToutValue[Num] += Time.deltaTime * 0.5f;
                    if (ToutValue[Num] >= ToutValue2[Num]) { ToutValue2[Num] = UnityEngine.Random.Range(60f, 100f) / 100f; }
                }
                else
                {
                    ToutValue[Num] -= Time.deltaTime * 0.5f;
                    if (ToutValue[Num] <= ToutValue2[Num]) { ToutValue2[Num] = UnityEngine.Random.Range(60f, 100f) / 100f; }
                }

                if (TopenValue[Num] < TopenValue2[Num])
                {
                    TopenValue[Num] += Time.deltaTime * 0.5f;
                    if (TopenValue[Num] >= TopenValue2[Num]) { TopenValue2[Num] = UnityEngine.Random.Range(0f, 60f) / 100f; }
                }
                else
                {
                    TopenValue[Num] -= Time.deltaTime * 0.5f;
                    if (TopenValue[Num] <= TopenValue2[Num]) { TopenValue2[Num] = UnityEngine.Random.Range(0f, 60f) / 100f; }
                }
            }


            //口元破綻の抑制とシェイプキー操作
            if (mode == 0)
            {  //通常時
                try { VertexMorph_FromProcItem(M.body0, "moutha", maV); } catch { }
                try { VertexMorph_FromProcItem(M.body0, "mouthdw", mdwV); } catch { }

            }
            if (mode == 1)
            {  //キス時
                if (miV > 0.1f) try { VertexMorph_FromProcItem(M.body0, "mouthi", 0.1f); } catch { }
                if (maV > 0.6f) maV = 0.6f;
                try { VertexMorph_FromProcItem(M.body0, "moutha", maV); } catch { }
                try { VertexMorph_FromProcItem(M.body0, "mouthdw", mdwV); } catch { }
                try { VertexMorph_FromProcItem(M.body0, "tangup", TupValue[Num]); } catch { }
                try { VertexMorph_FromProcItem(M.body0, "tangout", ToutValue[Num]); } catch { }
                try { VertexMorph_FromProcItem(M.body0, "tangopen", TopenValue[Num]); } catch { }

            }
            if (mode == 2)
            {  //フェラ時
                if (miV > 0.1f) try { VertexMorph_FromProcItem(M.body0, "mouthi", 0.1f); } catch { }
                if (mcV > 0.2f) try { VertexMorph_FromProcItem(M.body0, "mouthc", 0.2f); } catch { }
                if (msV > 0.1f) try { VertexMorph_FromProcItem(M.body0, "mouths", 0.1f); } catch { }
                if (mupV > 0.1f) try { VertexMorph_FromProcItem(M.body0, "mouthup", 0.1f); } catch { }
                if (maV > 1.0f) maV = 1.0f;
                try { VertexMorph_FromProcItem(M.body0, "moutha", maV); } catch { }
                try { VertexMorph_FromProcItem(M.body0, "mouthdw", mdwV); } catch { }
                try { VertexMorph_FromProcItem(M.body0, "tangup", TupValue[Num]); } catch { }
                try { VertexMorph_FromProcItem(M.body0, "tangout", ToutValue[Num]); } catch { }
                try { VertexMorph_FromProcItem(M.body0, "tangopen", TopenValue[Num]); } catch { }

                if (vStateMajor == 20) HyottokoFera("fera1", 0f, 80f, 4f);
                if (vStateMajor == 30) HyottokoFera("fera1", 0f, 80f, 8f);

            }
            else
            {
                if (hWaveValue != 0f)
                {
                    hWaveValue = 0f;
                    try { VertexMorph_FromProcItem(M.body0, "fera1", hWaveValue); } catch { }
                }
            }
            if (mode == 3)
            {  //連続絶頂時１
                if (miV > 0.1f) try { VertexMorph_FromProcItem(M.body0, "mouthi", 0.1f); } catch { }
                if (mcV > 0.2f) try { VertexMorph_FromProcItem(M.body0, "mouthc", 0.2f); } catch { }
                if (msV > 0.1f) try { VertexMorph_FromProcItem(M.body0, "mouths", 0.1f); } catch { }
                if (mupV > 0.1f) try { VertexMorph_FromProcItem(M.body0, "mouthup", 0.1f); } catch { }
                if (maV > 1.0f) maV = 1.0f;
                try { VertexMorph_FromProcItem(M.body0, "moutha", maV); } catch { }
                try { VertexMorph_FromProcItem(M.body0, "mouthdw", mdwV); } catch { }
                try { VertexMorph_FromProcItem(M.body0, "tangup", TupValue[Num]); } catch { }
                try { VertexMorph_FromProcItem(M.body0, "tangout", ToutValue[Num]); } catch { }
                try { VertexMorph_FromProcItem(M.body0, "tangopen", TopenValue[Num]); } catch { }

            }
            if (mode == 4)
            {  //連続絶頂時２
                if (mupV > 0f) try { VertexMorph_FromProcItem(M.body0, "mouthup", 0f); } catch { }
                if (msV > 0.1f) try { VertexMorph_FromProcItem(M.body0, "mouths", 0.1f); } catch { }
                try { VertexMorph_FromProcItem(M.body0, "mouthi", miV); } catch { }
                try { VertexMorph_FromProcItem(M.body0, "mouthdw", mdwV); } catch { }
                try { VertexMorph_FromProcItem(M.body0, "toothoff", 0f); } catch { }

            }
            if (mode == 5)
            {  //余韻時
                try { VertexMorph_FromProcItem(M.body0, "moutha", maV); } catch { }
                try { VertexMorph_FromProcItem(M.body0, "mouthdw", mdwV); } catch { }

            }


        }



        //ひょっとこフェラ
        private bool hWaveFlag = true;
        private float hWaveValue = 0f;
        private void HyottokoFera(string name, float min, float max, float sp)
        {
            sp *= Time.deltaTime * ShapeKeySpeedRate;

            if (hWaveFlag)
            {
                hWaveValue = hWaveValue + sp;

                if (hWaveValue > max)
                {
                    hWaveValue = max;
                    hWaveFlag = !hWaveFlag;
                }
            }
            else
            {
                hWaveValue = hWaveValue - sp;

                if (hWaveValue < min)
                {
                    hWaveValue = min;
                    hWaveFlag = !hWaveFlag;
                }
            }
            try
            {
                VertexMorph_FromProcItem(this.maid.body0, name, hWaveValue / 100f);
            }
            catch { /*LogError(ex);*/ }

        }




        // 通常時のモーション変更
        private void MotionChange(bool mode1)
        {
            if (cfgw.MotionChangeEnabled)
            {

                //現在のモーションを調べる
                string t = maid.body0.LastAnimeFN;
                string tb = t;

                float ls = 1f;
                float cs = 0.5f;

                t = regZeccyouBackup.Match(t).Groups[1].Value;  // 「 - Que…」を除く

                if (MotionList_tati.Contains(t))
                { //立ちモーションの場合
                    mcFlag = 0;
                    if (cameraCheck[maidDataList[iCurrentMaid]]) mcFlag = 6;

                }
                else if (MotionList_suwari.Contains(t))
                { //座りモーションの場合
                    mcFlag = 1;
                    if (cameraCheck[maidDataList[iCurrentMaid]]) mcFlag = 7;

                }
                else if (MotionList_zoukin.Contains(t))
                { //雑巾がけモーションの場合
                    mcFlag = 2;

                }
                else if (MotionList_kyuuzi.Contains(t))
                { //給仕モーションの場合
                    mcFlag = 3;

                }
                else if (MotionList_fukisouji.Contains(t))
                { //拭き掃除モーションの場合
                    mcFlag = 4;

                }
                else if (MotionList_mop.Contains(t))
                { //モップ掛けモーションの場合
                    mcFlag = 5;

                }
                else if (Regex.IsMatch(t, "_[123]"))
                { //変更可能な夜伽モーションの場合
                    mcFlag = 10;

                }
                else
                {
                    mcFlag = -1;

                }


                if (mcFlag == 10)
                { //夜伽モーションをバイブ強度に応じて変更

                    //一段階目のモーションをバックアップとして取る
                    MaidMotionBack = Regex.Replace(t, "_[123]", "_1");
                    if (!Regex.IsMatch(t, "m_")) MaidMotionBack = Regex.Replace(MaidMotionBack, @"^[a-zA-Z]_", "");
                    MaidMotionBack = Regex.Replace(MaidMotionBack, @"[a-zA-Z][0-9][0-9]", "");

                    //モーション変更処理を実行
                    if (vStateMajor == 20)
                    {
                        //モーション名を変換
                        t = Regex.Replace(t, "_[13](?!ana)(?!p_)", "_2");
                        if (!Regex.IsMatch(t, "m_")) t = Regex.Replace(t, @"^[a-zA-Z]_", "");
                        t = Regex.Replace(t, @"[a-zA-Z][0-9][0-9]", "");

                    }

                    if (vStateMajor == 30)
                    {
                        //モーション名を変換
                        t = Regex.Replace(t, "_[12](?!ana)(?!p_)", "_3");
                        if (!Regex.IsMatch(t, "m_")) t = Regex.Replace(t, @"^[a-zA-Z]_", "");
                        t = Regex.Replace(t, @"[a-zA-Z][0-9][0-9]", "");

                    }

                    if (vStateMajor == 40 || vStateMajor == 10)
                    {
                        //モーション名を変換
                        t = Regex.Replace(t, "_[23](?!ana)(?!p_)", "_1");
                        if (!Regex.IsMatch(t, "m_")) t = Regex.Replace(t, @"^[a-zA-Z]_", "");
                        t = Regex.Replace(t, @"[a-zA-Z][0-9][0-9]", "");

                    }

                    //差分モーションが有るかどうかチェック
                    int i = YotogiListBase.IndexOf(t);
                    if (i >= 0)
                    {
                        int r = UnityEngine.Random.Range(0, YotogiListSabun[i].Count);
                        t = YotogiListSabun[i][r];
                    }

                    //まんぐりバイブだけはファイルがおかしいため変更
                    if (t == "manguri_vibe_1_f.anm") t = "x_manguri_vibe_1_f.anm";
                    if (t == "manguri_vibe_2_f.anm") t = "x_manguri_vibe_2_f.anm";
                    if (t == "manguri_vibe_3_f.anm") t = "x_manguri_vibe_3_f.anm";
                    if (t == "manguri_vibe_oku_1_f.anm") t = "x_manguri_vibe_oku_1_f.anm";
                    if (t == "manguri_vibe_oku_2_f.anm") t = "x_manguri_vibe_oku_2_f.anm";
                    if (t == "manguri_vibe_oku_3_f.anm") t = "x_manguri_vibe_oku_3_f.anm";

                    if (t != tb || mode1)
                    {
                        //メイドのモーション変更
                        if (allFiles.Contains(t.Replace(".anm", "")))
                        {
                            maid.CrossFadeAbsolute(t, false, true, false, cs, ls);
                            Console.WriteLine("モーション変更：" + t);

                            //IKのアタッチ解除
                            //-----------------------------------------------------------------------------------------------------------------
                            // 2018/10/07 COM3D2 Ver1.20.1 暫定対応
                            //-----------------------------------------------------------------------------------------------------------------
                            //maid.IKTargetToBone("左手", null, "無し", Vector3.zero, IKCtrlData.IKAttachType.Point, false, 0f, false, false);
                            //maid.IKTargetToBone("右手", null, "無し", Vector3.zero, IKCtrlData.IKAttachType.Point, false, 0f, false, false);
                            //-----------------------------------------------------------------------------------------------------------------
                            maid.IKTargetToBone("左手", null, "無し", Vector3.zero, IKCtrlData.IKAttachType.Point, false, false, false);
                            maid.IKTargetToBone("右手", null, "無し", Vector3.zero, IKCtrlData.IKAttachType.Point, false, false, false);
                            //-----------------------------------------------------------------------------------------------------------------

                        }
                        else
                        {
                            Console.WriteLine("対応モーション無し：" + t);
                        }


                        //男のモーション変更
                        MotionChangeMan(maidDataList[iCurrentMaid]);


                    }

                }
                else if (mcFlag >= 0)
                {

                    int i20 = UnityEngine.Random.Range(0, MotionList20[mcFlag].Length);
                    int i30 = UnityEngine.Random.Range(0, MotionList30[mcFlag].Length);
                    int i40 = UnityEngine.Random.Range(0, MotionList40[mcFlag].Length);

                    //通常モーションから遷移する場合にバックアップを取る
                    if (!MotionList_vibe.Contains(t) && (0 > t.IndexOf(sZeccyouMaidMotion[0]) || 0 > t.IndexOf(sZeccyouMaidMotion[1]) || 0 > t.IndexOf(sZeccyouMaidMotion[2])))
                    {
                        MaidMotionBack = t;
                    }

                    //モーション変更処理を実行
                    if (vStateMajor == 20 && t != MotionList20[mcFlag][i20])
                    {
                        maid.CrossFadeAbsolute(MotionList20[mcFlag][i20], false, true, false, cs, ls);
                        //Console.WriteLine(MotionList20[mcFlag][i20]);
                    }
                    if (vStateMajor == 30 && t != MotionList30[mcFlag][i30])
                    {
                        maid.CrossFadeAbsolute(MotionList30[mcFlag][i30], false, true, false, cs, ls);
                        //Console.WriteLine(MotionList30[mcFlag][i30]);
                    }
                    if (vStateMajor == 40 && vStateMajorOld != 40)
                    {
                        //if (mcFlag == 4 || mcFlag == 5){ ls = 0.3f; }
                        maid.CrossFadeAbsolute(MotionList40[mcFlag][i40], false, true, false, cs, ls);
                        //Console.WriteLine(MotionList40[mcFlag][i40]);
                    }
                    if (vStateMajor == 10 && vStateMajorOld == 40)
                    {
                        if (allFiles.Contains(MaidMotionBack.Replace(".anm", "")))
                        {
                            maid.CrossFadeAbsolute(MaidMotionBack, false, true, false, 1f, 1f);
                            //Console.WriteLine(MaidMotionBack);
                        }
                    }

                    //悪戯用ボイスセット読み込み
                    //voiceSetLoad("v_悪戯.xml", 0);

                }

            }

            ChangeSE(false);

        }



        // サブメイド用　通常時のモーション変更
        private void MotionChangeSub(int Nam, bool mode1)
        {

            Maid cm = SubMaids[Nam];

            if (cfgw.MotionChangeEnabled)
            {
                //メイド名とセーブ番号取得
                string MaidName = cm.status.lastName + " " + cm.status.firstName;
                int mn = MaidNameSave.IndexOf(MaidName);
                VLevelSave[mn] = VLevel;

                //現在のモーションを調べる
                string t = cm.body0.LastAnimeFN;

                float ls = 1f;
                float cs = 0.5f;

                t = regZeccyouBackup.Match(t).Groups[1].Value;  // 「 - Que…」を除く

                if (MotionList_tati.Contains(t))
                { //立ちモーションの場合
                    mcFlag = 0;
                    if (cameraCheck[Nam]) mcFlag = 6;

                }
                else if (MotionList_suwari.Contains(t))
                { //座りモーションの場合
                    mcFlag = 1;
                    if (cameraCheck[Nam]) mcFlag = 7;

                }
                else if (MotionList_zoukin.Contains(t))
                { //雑巾がけモーションの場合
                    mcFlag = 2;

                }
                else if (MotionList_kyuuzi.Contains(t))
                { //給仕モーションの場合
                    mcFlag = 3;

                }
                else if (MotionList_fukisouji.Contains(t))
                { //拭き掃除モーションの場合
                    mcFlag = 4;

                }
                else if (MotionList_mop.Contains(t))
                { //モップ掛けモーションの場合
                    mcFlag = 5;

                }
                else if (Regex.IsMatch(t, "_[123]"))
                { //変更可能な夜伽モーションの場合
                    mcFlag = 10;

                }
                else
                {
                    mcFlag = -1;

                }


                if (mcFlag == 10)
                { //夜伽モーションをバイブ強度に応じて変更

                    //一段階目のモーションをバックアップとして取る
                    MotionBackupSave[mn] = Regex.Replace(t, "_[123]", "_1");
                    if (!Regex.IsMatch(t, "m_")) MotionBackupSave[mn] = Regex.Replace(MotionBackupSave[mn], @"^[a-zA-Z]_", "");
                    MotionBackupSave[mn] = Regex.Replace(MotionBackupSave[mn], @"[a-zA-Z][0-9][0-9]", "");

                    bool cf = false;

                    //モーション変更処理を実行
                    if (vStateMajor == 20 && Regex.IsMatch(t, "_[13]"))
                    {
                        //モーション名を変換
                        t = Regex.Replace(t, "_[13](?!ana)(?!p_)", "_2");
                        if (!Regex.IsMatch(t, "m_")) t = Regex.Replace(t, @"^[a-zA-Z]_", "");
                        t = Regex.Replace(t, @"[a-zA-Z][0-9][0-9]", "");

                        cf = true;

                    }

                    if (vStateMajor == 30 && Regex.IsMatch(t, "_[12]"))
                    {
                        //モーション名を変換
                        t = Regex.Replace(t, "_[12](?!ana)(?!p_)", "_3");
                        if (!Regex.IsMatch(t, "m_")) t = Regex.Replace(t, @"^[a-zA-Z]_", "");
                        t = Regex.Replace(t, @"[a-zA-Z][0-9][0-9]", "");

                        cf = true;

                    }

                    if (vStateMajor == 40 && Regex.IsMatch(t, "_[23]"))
                    {
                        //モーション名を変換
                        t = Regex.Replace(t, "_[23](?!ana)(?!p_)", "_1");
                        if (!Regex.IsMatch(t, "m_")) t = Regex.Replace(t, @"^[a-zA-Z]_", "");
                        t = Regex.Replace(t, @"[a-zA-Z][0-9][0-9]", "");

                        cf = true;

                    }

                    if (cf || mode1)
                    {
                        //差分モーションが有るかどうかチェック
                        int i = YotogiListBase.IndexOf(t);
                        if (i >= 0)
                        {
                            int r = UnityEngine.Random.Range(0, YotogiListSabun[i].Count);
                            t = YotogiListSabun[i][r];
                        }

                        //まんぐりバイブだけはファイルがおかしいため変更
                        if (t == "manguri_vibe_1_f.anm") t = "x_manguri_vibe_1_f.anm";
                        if (t == "manguri_vibe_2_f.anm") t = "x_manguri_vibe_2_f.anm";
                        if (t == "manguri_vibe_3_f.anm") t = "x_manguri_vibe_3_f.anm";
                        if (t == "manguri_vibe_oku_1_f.anm") t = "x_manguri_vibe_oku_1_f.anm";
                        if (t == "manguri_vibe_oku_2_f.anm") t = "x_manguri_vibe_oku_2_f.anm";
                        if (t == "manguri_vibe_oku_3_f.anm") t = "x_manguri_vibe_oku_3_f.anm";

                        //メイドのモーション変更
                        if (allFiles.Contains(t.Replace(".anm", "")))
                        {
                            cm.CrossFadeAbsolute(t, false, true, false, cs, ls);
                            Console.WriteLine("モーション変更：" + t);

                            //IKのアタッチ解除
                            //-----------------------------------------------------------------------------------------------------------------
                            // 2018/10/07 COM3D2 Ver1.20.1 暫定対応
                            //-----------------------------------------------------------------------------------------------------------------
                            //cm.IKTargetToBone("左手", null, "無し", Vector3.zero, IKCtrlData.IKAttachType.Point, false, 0f, false, false);
                            //cm.IKTargetToBone("右手", null, "無し", Vector3.zero, IKCtrlData.IKAttachType.Point, false, 0f, false, false);
                            //-----------------------------------------------------------------------------------------------------------------
                            cm.IKTargetToBone("左手", null, "無し", Vector3.zero, IKCtrlData.IKAttachType.Point, false, false, false);
                            cm.IKTargetToBone("右手", null, "無し", Vector3.zero, IKCtrlData.IKAttachType.Point, false, false, false);
                            //-----------------------------------------------------------------------------------------------------------------
                        }
                        else
                        {
                            Console.WriteLine("対応モーション無し：" + t);
                        }


                        //男のモーション変更
                        MotionChangeMan(Nam);


                    }


                }
                else if (mcFlag >= 0)
                {

                    int i20 = UnityEngine.Random.Range(0, MotionList20[mcFlag].Length);
                    int i30 = UnityEngine.Random.Range(0, MotionList30[mcFlag].Length);
                    int i40 = UnityEngine.Random.Range(0, MotionList40[mcFlag].Length);

                    //通常モーションから遷移する場合にバックアップを取る
                    if (!MotionList_vibe.Contains(t) && (0 > t.IndexOf(sZeccyouMaidMotion[0]) || 0 > t.IndexOf(sZeccyouMaidMotion[1]) || 0 > t.IndexOf(sZeccyouMaidMotion[2])))
                    {
                        MotionBackupSave[mn] = t;
                    }

                    //モーション変更処理を実行
                    if (vStateMajor == 20 && t != MotionList20[mcFlag][i20])
                    {
                        cm.CrossFadeAbsolute(MotionList20[mcFlag][i20], false, true, false, cs, ls);
                        //Console.WriteLine(MotionList20[mcFlag][i20]);
                    }
                    if (vStateMajor == 30 && t != MotionList30[mcFlag][i30])
                    {
                        cm.CrossFadeAbsolute(MotionList30[mcFlag][i30], false, true, false, cs, ls);
                        //Console.WriteLine(MotionList30[mcFlag][i30]);
                    }
                    if (vStateMajor == 40 && vStateMajorOld != 40)
                    {
                        //if (mcFlag == 4 || mcFlag == 5){ ls = 0.3f; }
                        cm.CrossFadeAbsolute(MotionList40[mcFlag][i40], false, true, false, cs, ls);
                        //Console.WriteLine(MotionList40[mcFlag][i40]);
                    }
                    if (vStateMajor == 10 && vStateMajorOld == 40)
                    {
                        if (allFiles.Contains(MotionBackupSave[mn].Replace(".anm", "")))
                        {
                            cm.CrossFadeAbsolute(MotionBackupSave[mn], false, true, false, 1f, 1f);
                            //Console.WriteLine(MaidMotionBack);
                        }
                    }


                }

            }
        }



        // 絶頂モーションに変更
        private void ZeccyouAnim()
        {
            if (cfgw.ZeccyouAnimeEnabled && !vMaidStun)
            {
                string anim = maid.body0.LastAnimeFN;

                Match match = regZeccyou.Match(anim);
                string sHighExciteMotionBackup = regZeccyouBackup.Match(anim).Groups[1].Value;  // 「 - Que…」を除く

                if (match.Success || sHighExciteMotionBackup == "settai_vibe_in_kaiwa_kyou_a01_f.anm" || sHighExciteMotionBackup == "settai_vibe_in_kaiwa_kyou_f.anm")
                {

                    //現在モーションファイル名の先頭部分取得
                    string sZeccyouMotion = match.Groups[2].Value;

                    //ポーズ維持、接待バイブのモーション名の場合は変換
                    if (sZeccyouMotion == "poseizi_hibu")
                    {
                        sZeccyouMotion = "poseizi";
                    }
                    else if (sZeccyouMotion == "poseizi2_hibu")
                    {
                        sZeccyouMotion = "poseizi2";
                    }
                    else if (sHighExciteMotionBackup == "settai_vibe_in_kaiwa_kyou_a01_f.anm" || sHighExciteMotionBackup == "settai_vibe_in_kaiwa_kyou_f.anm")
                    {
                        sZeccyouMotion = "settai_vibe_in";
                    }


                    //絶頂モーションのファイル名が有るかどうかチェック
                    bool zf = false;
                    string sZeccyouMotionMaid = "";
                    string sZeccyouMotionMan = "";

                    foreach (string z in sZeccyouMaidMotion)
                    {
                        sZeccyouMotionMaid = sZeccyouMotion + z;
                        sZeccyouMotionMan = sZeccyouMotion + z.Replace("_f_", "_m_");

                        if (sZeccyouMotion == "harem_sex" || sZeccyouMotion == "yuri_aibu" || sZeccyouMotion == "yuri_kaiawase" || sZeccyouMotion == "yuri_kunni" || sZeccyouMotion == "yuri_soutouvibe")
                        {
                            if (0 <= sHighExciteMotionBackup.IndexOf("_f."))
                            {
                                sZeccyouMotionMaid = sZeccyouMotion + z;

                            }
                            else if (0 <= sHighExciteMotionBackup.IndexOf("_f2."))
                            {
                                sZeccyouMotionMaid = sZeccyouMotion + sZeccyouMaidMotion2;

                            }
                            else if (0 <= sHighExciteMotionBackup.IndexOf("_f3."))
                            {
                                sZeccyouMotionMaid = sZeccyouMotion + sZeccyouMaidMotion3;

                            }
                        }

                        if (allFiles.Contains(sZeccyouMotionMaid))
                        {
                            zf = true;
                            break;
                        }
                    }

                    //上記チェックで引っかからなかった場合、「cli」や「kiss」などの文字列を除去してもう一度チェック
                    if (!zf)
                    {
                        sZeccyouMotion = sZeccyouMotion.Replace("_hibu", "").Replace("_kiss", "").Replace("_cli", "").Replace("_momi", "");

                        foreach (string z in sZeccyouMaidMotion)
                        {
                            sZeccyouMotionMaid = sZeccyouMotion + z;
                            sZeccyouMotionMan = sZeccyouMotion + z.Replace("_f_", "_m_");

                            if (sZeccyouMotion == "harem_sex" || sZeccyouMotion == "yuri_aibu" || sZeccyouMotion == "yuri_kaiawase" || sZeccyouMotion == "yuri_kunni" || sZeccyouMotion == "yuri_soutouvibe")
                            {
                                if (0 <= sHighExciteMotionBackup.IndexOf("_f."))
                                {
                                    sZeccyouMotionMaid = sZeccyouMotion + z;

                                }
                                else if (0 <= sHighExciteMotionBackup.IndexOf("_f2."))
                                {
                                    sZeccyouMotionMaid = sZeccyouMotion + sZeccyouMaidMotion2;

                                }
                                else if (0 <= sHighExciteMotionBackup.IndexOf("_f3."))
                                {
                                    sZeccyouMotionMaid = sZeccyouMotion + sZeccyouMaidMotion3;

                                }
                            }

                            if (allFiles.Contains(sZeccyouMotionMaid))
                            {
                                zf = true;
                                break;
                            }
                        }

                    }



                    //絶頂モーションに変更
                    if (zf)
                    {

                        // 強制的に再生
                        //メイドモーション変更
                        maid.CrossFadeAbsolute(sZeccyouMotionMaid + ".anm", false, false, false, 0.5f, 1f);
                        ZAnimeFileName[maidDataList[iCurrentMaid]] = sZeccyouMotionMaid + ".anm";

                        //男のモーション変更
                        if (!RankoEnabled && man.Visible && cfgw.ZeccyouManAnimeEnabled)
                        {  //通常モードで男が表示されている場合
                            if (allFiles.Contains(sZeccyouMotionMan))
                            {
                                man.CrossFadeAbsolute(sZeccyouMotionMan + ".anm", false, false, false, 0.5f, 1f);
                            }

                        }
                        else if (RankoEnabled && MansTg.Contains(maidDataList[iCurrentMaid]) && cfgw.ZeccyouManAnimeEnabled)
                        {  //乱交モードかつ対象の男がいる場合

                            int im2 = 2;
                            string t = sZeccyouMotionMan + ".anm";
                            for (int im = 0; im < SubMans.Length; im++)
                            {

                                if (MansTg[im] == maidDataList[iCurrentMaid] && SubMans[im].Visible)
                                {

                                    if (allFiles.Contains(sZeccyouMotionMan))
                                    {
                                        SubMans[im].CrossFadeAbsolute(t, false, false, false, 0.5f, 1f);
                                    }
                                    else
                                    {
                                        break;
                                    }

                                    t = Regex.Replace(t, "_m(|[0-9])_", "_m" + im2 + "_");
                                    ++im2;

                                }
                            }

                        }

                        // 終わったら再生する
                        //メイドモーション変更
                        maid.CrossFade(sHighExciteMotionBackup, false, true, true, 0.5f, 1f);

                        //男のモーション変更
                        string t2 = sHighExciteMotionBackup;
                        t2 = Regex.Replace(t2, "_f", "_m");
                        t2 = Regex.Replace(t2, "[a-zA-Z][0-9][0-9]", "");

                        if (!RankoEnabled && man.Visible && cfgw.ZeccyouManAnimeEnabled)
                        {  //通常モードで男が表示されている場合
                            if (allFiles.Contains(t2.Replace(".anm", "")))
                            {
                                man.CrossFade(t2, false, true, true, 0.5f, 1f);
                            }
                        }
                        else if (RankoEnabled && MansTg.Contains(maidDataList[iCurrentMaid]) && cfgw.ZeccyouManAnimeEnabled)
                        {  //乱交モードかつ対象の男がいる場合

                            int im2 = 2;
                            for (int im = 0; im < SubMans.Length; im++)
                            {

                                if (MansTg[im] == maidDataList[iCurrentMaid] && SubMans[im].Visible)
                                {

                                    if (allFiles.Contains(t2.Replace(".anm", "")))
                                    {
                                        SubMans[im].CrossFade(t2, false, true, true, 0.5f, 1f);
                                    }
                                    else
                                    {
                                        break;
                                    }

                                    t2 = Regex.Replace(t2, @"_m(|[0-9])\.", "_m" + im2 + ".");
                                    ++im2;

                                }
                            }

                        }

                    }
                    else
                    {
                        //絶頂モーションがない場合は、現在モーションをフェラ判別用に挿入する
                        ZAnimeFileName[maidDataList[iCurrentMaid]] = maid.body0.LastAnimeFN;
                    }
                }
            }
        }



        // サブメイド用絶頂モーション変更
        private void ZeccyouAnimSub(int Nam)
        {
            Maid zm = SubMaids[Nam];
            if (cfgw.ZeccyouAnimeEnabled)
            {
                string anim = zm.body0.LastAnimeFN;

                Match match = regZeccyou.Match(anim);
                string sHighExciteMotionBackup = regZeccyouBackup.Match(anim).Groups[1].Value;  // 「 - Que…」を除く

                if (match.Success || sHighExciteMotionBackup == "settai_vibe_in_kaiwa_kyou_a01_f.anm" || sHighExciteMotionBackup == "settai_vibe_in_kaiwa_kyou_f.anm")
                {

                    //現在モーションファイル名の先頭部分取得
                    string sZeccyouMotion = match.Groups[2].Value;

                    //ポーズ維持、接待バイブのモーション名の場合は変換
                    if (sZeccyouMotion == "poseizi_hibu")
                    {
                        sZeccyouMotion = "poseizi";
                    }
                    else if (sZeccyouMotion == "poseizi2_hibu")
                    {
                        sZeccyouMotion = "poseizi2";
                    }
                    else if (sHighExciteMotionBackup == "settai_vibe_in_kaiwa_kyou_a01_f.anm" || sHighExciteMotionBackup == "settai_vibe_in_kaiwa_kyou_f.anm")
                    {
                        sZeccyouMotion = "settai_vibe_in";
                    }


                    //絶頂モーションのファイル名が有るかどうかチェック
                    bool zf = false;
                    string sZeccyouMotionMaid = "";
                    string sZeccyouMotionMan = "";

                    foreach (string z in sZeccyouMaidMotion)
                    {
                        sZeccyouMotionMaid = sZeccyouMotion + z;
                        sZeccyouMotionMan = sZeccyouMotion + z.Replace("_f_", "_m_");

                        if (sZeccyouMotion == "harem_sex" || sZeccyouMotion == "yuri_aibu" || sZeccyouMotion == "yuri_kaiawase" || sZeccyouMotion == "yuri_kunni" || sZeccyouMotion == "yuri_soutouvibe")
                        {
                            if (0 <= sHighExciteMotionBackup.IndexOf("_f."))
                            {
                                sZeccyouMotionMaid = sZeccyouMotion + z;

                            }
                            else if (0 <= sHighExciteMotionBackup.IndexOf("_f2."))
                            {
                                sZeccyouMotionMaid = sZeccyouMotion + sZeccyouMaidMotion2;

                            }
                            else if (0 <= sHighExciteMotionBackup.IndexOf("_f3."))
                            {
                                sZeccyouMotionMaid = sZeccyouMotion + sZeccyouMaidMotion3;

                            }
                        }

                        if (allFiles.Contains(sZeccyouMotionMaid))
                        {
                            zf = true;
                            break;
                        }
                    }

                    //上記チェックで引っかからなかった場合、「cli」や「kiss」などの文字列を除去してもう一度チェック
                    if (!zf)
                    {
                        sZeccyouMotion = sZeccyouMotion.Replace("_hibu", "").Replace("_kiss", "").Replace("_cli", "").Replace("_momi", "");

                        foreach (string z in sZeccyouMaidMotion)
                        {
                            sZeccyouMotionMaid = sZeccyouMotion + z;
                            sZeccyouMotionMan = sZeccyouMotion + z.Replace("_f_", "_m_");

                            if (sZeccyouMotion == "harem_sex" || sZeccyouMotion == "yuri_aibu" || sZeccyouMotion == "yuri_kaiawase" || sZeccyouMotion == "yuri_kunni" || sZeccyouMotion == "yuri_soutouvibe")
                            {
                                if (0 <= sHighExciteMotionBackup.IndexOf("_f."))
                                {
                                    sZeccyouMotionMaid = sZeccyouMotion + z;

                                }
                                else if (0 <= sHighExciteMotionBackup.IndexOf("_f2."))
                                {
                                    sZeccyouMotionMaid = sZeccyouMotion + sZeccyouMaidMotion2;

                                }
                                else if (0 <= sHighExciteMotionBackup.IndexOf("_f3."))
                                {
                                    sZeccyouMotionMaid = sZeccyouMotion + sZeccyouMaidMotion3;

                                }
                            }

                            if (allFiles.Contains(sZeccyouMotionMaid))
                            {
                                zf = true;
                                break;
                            }
                        }

                    }



                    //絶頂モーションに変更
                    if (zf)
                    {

                        // 強制的に再生
                        //メイドモーション変更
                        zm.CrossFadeAbsolute(sZeccyouMotionMaid + ".anm", false, false, false, 0.5f, 1f);
                        ZAnimeFileName[Nam] = sZeccyouMotionMaid + ".anm";

                        //男のモーション変更
                        if (RankoEnabled && MansTg.Contains(Nam) && cfgw.ZeccyouManAnimeEnabled)
                        {  //乱交モードかつ対象の男がいる場合

                            int im2 = 2;
                            string t = sZeccyouMotionMan + ".anm";
                            for (int im = 0; im < SubMans.Length; im++)
                            {

                                if (MansTg[im] == Nam && SubMans[im].Visible)
                                {

                                    if (allFiles.Contains(sZeccyouMotionMan))
                                    {
                                        SubMans[im].CrossFadeAbsolute(t, false, false, false, 0.5f, 1f);
                                    }
                                    else
                                    {
                                        break;
                                    }

                                    t = Regex.Replace(t, "_m(|[0-9])_", "_m" + im2 + "_");
                                    ++im2;

                                }
                            }

                        }

                        // 終わったら再生する
                        //メイドモーション変更
                        zm.CrossFade(sHighExciteMotionBackup, false, true, true, 0.5f, 1f);

                        //男のモーション変更
                        string t2 = sHighExciteMotionBackup;
                        t2 = Regex.Replace(t2, "_f", "_m");
                        t2 = Regex.Replace(t2, "[a-zA-Z][0-9][0-9]", "");

                        if (RankoEnabled && MansTg.Contains(Nam) && cfgw.ZeccyouManAnimeEnabled)
                        {  //乱交モードかつ対象の男がいる場合

                            int im2 = 2;
                            for (int im = 0; im < SubMans.Length; im++)
                            {

                                if (MansTg[im] == Nam && SubMans[im].Visible)
                                {

                                    if (allFiles.Contains(t2.Replace(".anm", "")))
                                    {
                                        SubMans[im].CrossFade(t2, false, true, true, 0.5f, 1f);
                                    }
                                    else
                                    {
                                        break;
                                    }

                                    t2 = Regex.Replace(t2, @"_m(|[0-9])\.", "_m" + im2 + ".");
                                    ++im2;

                                }
                            }

                        }



                    }
                    else
                    {
                        //絶頂モーションがない場合は、現在モーションをフェラ判別用に挿入する
                        ZAnimeFileName[Nam] = zm.body0.LastAnimeFN;
                    }
                }
            }
        }




        //男通常モーション変更処理
        private void MotionChangeMan(int Nam)
        {

            string t = SubMaids[Nam].body0.LastAnimeFN;
            t = Regex.Replace(t, "_f", "_m");

            float ls = 1f;
            float cs = 0.5f;

            if (Nam == maidDataList[iCurrentMaid] && !RankoEnabled && man.Visible)
            {  //通常モードで男が表示されている場合

                if (allFiles.Contains(t.Replace(".anm", "")))
                {
                    man.CrossFadeAbsolute(t, false, true, false, cs, ls);
                }
                else
                {
                    t = Regex.Replace(t, "[a-zA-Z][0-9][0-9]", "");
                    if (allFiles.Contains(t.Replace(".anm", "")))
                    {
                        man.CrossFadeAbsolute(t, false, true, false, cs, ls);
                    }
                }

            }
            else if (RankoEnabled && MansTg.Contains(Nam))
            {  //乱交モードかつ対象の男がいる場合

                int im2 = 2;
                for (int im = 0; im < SubMans.Length; im++)
                {

                    if (MansTg[im] == Nam && SubMans[im].Visible)
                    {

                        if (allFiles.Contains(t.Replace(".anm", "")))
                        {
                            SubMans[im].CrossFadeAbsolute(t, false, true, false, cs, ls);
                        }
                        else
                        {
                            t = Regex.Replace(t, "[a-zA-Z][0-9][0-9]", "");
                            if (allFiles.Contains(t.Replace(".anm", "")))
                            {
                                SubMans[im].CrossFadeAbsolute(t, false, true, false, cs, ls);
                            }
                            else
                            {
                                break;
                            }
                        }

                        t = Regex.Replace(t, @"_m(|[0-9])\.", "_m" + im2 + ".");
                        ++im2;

                    }
                }
            }

        }

        //変更速度個別設定用
        private void MotionChangeMan(int Nam, float cs)
        {

            string t = SubMaids[Nam].body0.LastAnimeFN;
            t = Regex.Replace(t, "_f", "_m");

            float ls = 1f;

            if (Nam == maidDataList[iCurrentMaid] && !RankoEnabled && man.Visible)
            {  //通常モードで男が表示されている場合

                if (allFiles.Contains(t.Replace(".anm", "")))
                {
                    man.CrossFadeAbsolute(t, false, true, false, cs, ls);
                }
                else
                {
                    t = Regex.Replace(t, "[a-zA-Z][0-9][0-9]", "");
                    if (allFiles.Contains(t.Replace(".anm", "")))
                    {
                        man.CrossFadeAbsolute(t, false, true, false, cs, ls);
                    }
                }

            }
            else if (RankoEnabled && MansTg.Contains(Nam))
            {  //乱交モードかつ対象の男がいる場合

                int im2 = 2;
                for (int im = 0; im < SubMans.Length; im++)
                {

                    if (MansTg[im] == Nam && SubMans[im].Visible)
                    {

                        if (allFiles.Contains(t.Replace(".anm", "")))
                        {
                            SubMans[im].CrossFadeAbsolute(t, false, true, false, cs, ls);
                        }
                        else
                        {
                            t = Regex.Replace(t, "[a-zA-Z][0-9][0-9]", "");
                            if (allFiles.Contains(t.Replace(".anm", "")))
                            {
                                SubMans[im].CrossFadeAbsolute(t, false, true, false, cs, ls);
                            }
                            else
                            {
                                break;
                            }
                        }

                        t = Regex.Replace(t, @"_m(|[0-9])\.", "_m" + im2 + ".");
                        ++im2;

                    }
                }
            }

        }





        //メイドの音声再生処理
        private void MaidVoicePlay(Maid vm, int Num)
        {

            //フェラしているかのチェック
            checkBlowjobing(vm, Num);

            if (AutoModeEnabled)
            {
                if (bIsBlowjobing[Num] > 0)
                {
                    ModeSelect = 1;
                }
                else
                {
                    ModeSelect = 0;
                }
            }

            //            string sPersonal = vm.Param.status.personal.ToString();
            // string sPersonal = vm.status.personal.ToString();
            string sPersonal = vm.status.personal.uniqueName;

            string[] VoiceList = new string[1];
            int vi = 0;

            //Console.WriteLine(sPersonal);

            //バイブ弱の音声
            if (vStateMajor == 20)
            {
				Console.WriteLine("[DEBUG]sPersonal:"+ sPersonal);
                if (vMaidStun)
                {
                    vi = 4;
                }
                else
                {
                    vi = vExciteLevel - 1;
                }

                if (ModeSelect == 0)
                { //通常音声
                    if (sPersonal == "Pure")
                    {
                        VoiceList = cfg.sLoopVoice20PureVibe[vi];
                    }
                    else if (sPersonal == "Cool")
                    {
                        VoiceList = cfg.sLoopVoice20CoolVibe[vi];
                    }
                    else if (sPersonal == "Pride")
                    {
                        VoiceList = cfg.sLoopVoice20PrideVibe[vi];
                    }
                    else if (sPersonal == "Yandere")
                    {
                        VoiceList = cfg.sLoopVoice20YandereVibe[vi];
                    }
                    else if (sPersonal == "Anesan")
                    {
                        VoiceList = cfg.sLoopVoice20AnesanVibe[vi];
                    }
                    else if (sPersonal == "Genki")
                    {
                        VoiceList = cfg.sLoopVoice20GenkiVibe[vi];
                    }
                    else if (sPersonal == "Sadist")
                    {
                        VoiceList = cfg.sLoopVoice20SadistVibe[vi];
                    }
                    else if (sPersonal == "Muku")
                    {
                        VoiceList = cfg.sLoopVoice20MukuVibe[vi];
                    }
                    else if (sPersonal == "Majime")
                    {
                        VoiceList = cfg.sLoopVoice20MajimeVibe[vi];
                    }
                    else if (sPersonal == "Rindere")
                    {
                        VoiceList = cfg.sLoopVoice20RindereVibe[vi];
                        Console.WriteLine("[DEBUG]VoiceList:" + VoiceList[0]);
                        Console.WriteLine("[DEBUG]VoiceList:" + VoiceList[3]);

                    }
                    else if (sPersonal == "Silent")
                    {
                        VoiceList = cfg.sLoopVoice20SilentVibe[vi];
                        Console.WriteLine("[DEBUG]VoiceList:" + VoiceList[0]);
                        Console.WriteLine("[DEBUG]VoiceList:" + VoiceList[3]);

                    }
                    else if (sPersonal == "Devilish")
                    {
                        VoiceList = cfg.sLoopVoice20DevilishVibe[vi];
                        Console.WriteLine("[DEBUG]VoiceList:" + VoiceList[0]);
                        Console.WriteLine("[DEBUG]VoiceList:" + VoiceList[3]);

                    }
                    else if (sPersonal == "Ladylike")
                    {
                        VoiceList = cfg.sLoopVoice20LadylikeVibe[vi];
                        Console.WriteLine("[DEBUG]VoiceList:" + VoiceList[0]);
                        Console.WriteLine("[DEBUG]VoiceList:" + VoiceList[3]);
                    }
                    else if (sPersonal == "Secretary")
                    {
                        VoiceList = cfg.sLoopVoice20SecretaryVibe[vi];
                        Console.WriteLine("[DEBUG]VoiceList:" + VoiceList[0]);
                        Console.WriteLine("[DEBUG]VoiceList:" + VoiceList[3]);
                    }
                    else if (sPersonal == "Sister")
                    {
                        VoiceList = cfg.sLoopVoice20SisterVibe[vi];
                        Console.WriteLine("[DEBUG]VoiceList:" + VoiceList[0]);
                        Console.WriteLine("[DEBUG]VoiceList:" + VoiceList[3]);
                    }
                    
                    Console.WriteLine("[DEBUG]VoiceList:" + VoiceList[1]);

                }
                else if (ModeSelect == 1)
                { //フェラ音声
                    if (sPersonal == "Pure")
                    {
                        VoiceList = cfg.sLoopVoice20PureFera[vi];
                    }
                    else if (sPersonal == "Cool")
                    {
                        VoiceList = cfg.sLoopVoice20CoolFera[vi];
                    }
                    else if (sPersonal == "Pride")
                    {
                        VoiceList = cfg.sLoopVoice20PrideFera[vi];
                    }
                    else if (sPersonal == "Yandere")
                    {
                        VoiceList = cfg.sLoopVoice20YandereFera[vi];
                    }
                    else if (sPersonal == "Anesan")
                    {
                        VoiceList = cfg.sLoopVoice20AnesanFera[vi];
                    }
                    else if (sPersonal == "Genki")
                    {
                        VoiceList = cfg.sLoopVoice20GenkiFera[vi];
                    }
                    else if (sPersonal == "Sadist")
                    {
                        VoiceList = cfg.sLoopVoice20SadistFera[vi];
                    }

                    if (sPersonal == "Muku")
                    {
                        VoiceList = cfg.sLoopVoice20MukuFera[vi];
                    }
                    else if (sPersonal == "Majime")
                    {
                        VoiceList = cfg.sLoopVoice20MajimeFera[vi];
                    }
                    else if (sPersonal == "Rindere")
                    {
                        VoiceList = cfg.sLoopVoice20RindereFera[vi];
                    }
                    else if (sPersonal == "Silent")
                    {
                        VoiceList = cfg.sLoopVoice20SilentFera[vi];
                    }
                    else if (sPersonal == "Devilish")
                    {
                        VoiceList = cfg.sLoopVoice20DevilishFera[vi];
                    }
                    else if (sPersonal == "Ladylike")
                    {
                        VoiceList = cfg.sLoopVoice20LadylikeFera[vi];
                    }
                    else if (sPersonal == "Secretary")
                    {
                        VoiceList = cfg.sLoopVoice20SecretaryFera[vi];
                    }
                    else if (sPersonal == "Sister")
                    {
                        VoiceList = cfg.sLoopVoice20SisterFera[vi];
                    }
                }
                else if (ModeSelect == 2)
                { //カスタム音声１
                    VoiceList = cfg.sLoopVoice20Custom1[vi];
                }
                else if (ModeSelect == 3)
                { //カスタム音声２
                    VoiceList = cfg.sLoopVoice20Custom2[vi];
                }
                else if (ModeSelect == 4)
                { //カスタム音声３
                    VoiceList = cfg.sLoopVoice20Custom3[vi];
                }
                else if (ModeSelect == 5)
                { //カスタム音声４
                    VoiceList = cfg.sLoopVoice20Custom4[vi];
                }

            }

            //バイブ強の音声
            if (vStateMajor == 30)
            {
                if (OrgasmVoice == 0)
                {

                    if (vMaidStun)
                    {
                        vi = 4;
                    }
                    else
                    {
                        vi = vExciteLevel - 1;
                    }

                    if (ModeSelect == 0)
                    { //通常音声
                        if (sPersonal == "Pure")
                        {
                            VoiceList = cfg.sLoopVoice30PureVibe[vi];
                        }
                        else if (sPersonal == "Cool")
                        {
                            VoiceList = cfg.sLoopVoice30CoolVibe[vi];
                        }
                        else if (sPersonal == "Pride")
                        {
                            VoiceList = cfg.sLoopVoice30PrideVibe[vi];
                        }
                        else if (sPersonal == "Yandere")
                        {
                            VoiceList = cfg.sLoopVoice30YandereVibe[vi];
                        }
                        else if (sPersonal == "Anesan")
                        {
                            VoiceList = cfg.sLoopVoice30AnesanVibe[vi];
                        }
                        else if (sPersonal == "Genki")
                        {
                            VoiceList = cfg.sLoopVoice30GenkiVibe[vi];
                        }
                        else if (sPersonal == "Sadist")
                        {
                            VoiceList = cfg.sLoopVoice30SadistVibe[vi];
                        }

                        if (sPersonal == "Muku")
                        {
                            VoiceList = cfg.sLoopVoice30MukuVibe[vi];
                        }
                        else if (sPersonal == "Majime")
                        {
                            VoiceList = cfg.sLoopVoice30MajimeVibe[vi];
                        }
                        else if (sPersonal == "Rindere")
                        {
                            VoiceList = cfg.sLoopVoice30RindereVibe[vi];
                        }
                        else if (sPersonal == "Silent")
                        {
                            VoiceList = cfg.sLoopVoice30SilentVibe[vi];
                        }
                        else if (sPersonal == "Devilish")
                        {
                            VoiceList = cfg.sLoopVoice30DevilishVibe[vi];
                        }
                        else if (sPersonal == "Ladylike")
                        {
                            VoiceList = cfg.sLoopVoice30LadylikeVibe[vi];
                        }
                        else if (sPersonal == "Secretary")
                        {
                            VoiceList = cfg.sLoopVoice30SecretaryVibe[vi];
                        }
                        else if (sPersonal == "Sister")
                        {
                            VoiceList = cfg.sLoopVoice30SisterVibe[vi];
                        }

                    }
                    else if (ModeSelect == 1)
                    { //フェラ音声
                        if (sPersonal == "Pure")
                        {
                            VoiceList = cfg.sLoopVoice30PureFera[vi];
                        }
                        else if (sPersonal == "Cool")
                        {
                            VoiceList = cfg.sLoopVoice30CoolFera[vi];
                        }
                        else if (sPersonal == "Pride")
                        {
                            VoiceList = cfg.sLoopVoice30PrideFera[vi];
                        }
                        else if (sPersonal == "Yandere")
                        {
                            VoiceList = cfg.sLoopVoice30YandereFera[vi];
                        }
                        else if (sPersonal == "Anesan")
                        {
                            VoiceList = cfg.sLoopVoice30AnesanFera[vi];
                        }
                        else if (sPersonal == "Genki")
                        {
                            VoiceList = cfg.sLoopVoice30GenkiFera[vi];
                        }
                        else if (sPersonal == "Sadist")
                        {
                            VoiceList = cfg.sLoopVoice30SadistFera[vi];
                        }

                        if (sPersonal == "Muku")
                        {
                            VoiceList = cfg.sLoopVoice30MukuFera[vi];
                        }
                        else if (sPersonal == "Majime")
                        {
                            VoiceList = cfg.sLoopVoice30MajimeFera[vi];
                        }
                        else if (sPersonal == "Rindere")
                        {
                            VoiceList = cfg.sLoopVoice30RindereFera[vi];
                        }
                        else if (sPersonal == "Silent")
                        {
                            VoiceList = cfg.sLoopVoice30SilentFera[vi];
                        }
                        else if (sPersonal == "Devilish")
                        {
                            VoiceList = cfg.sLoopVoice30DevilishFera[vi];
                        }
                        else if (sPersonal == "Ladylike")
                        {
                            VoiceList = cfg.sLoopVoice30LadylikeFera[vi];
                        }
                        else if (sPersonal == "Secretary")
                        {
                            VoiceList = cfg.sLoopVoice30SecretaryFera[vi];
                        }
                        else if (sPersonal == "Sister")
                        {
                            VoiceList = cfg.sLoopVoice30SisterFera[vi];
                        }
                    }
                    else if (ModeSelect == 2)
                    { //カスタム音声１
                        VoiceList = cfg.sLoopVoice30Custom1[vi];
                    }
                    else if (ModeSelect == 3)
                    { //カスタム音声２
                        VoiceList = cfg.sLoopVoice30Custom2[vi];
                    }
                    else if (ModeSelect == 4)
                    { //カスタム音声３
                        VoiceList = cfg.sLoopVoice30Custom3[vi];
                    }
                    else if (ModeSelect == 5)
                    { //カスタム音声４
                        VoiceList = cfg.sLoopVoice30Custom4[vi];
                    }

                }
                else
                {  //放心中の絶頂時音声

                    if (vMaidStun)
                    {
                        vi = 4;
                    }
                    else if (vOrgasmCmb < 4)
                    {
                        vi = vExciteLevel - 2;
                    }
                    else
                    {
                        vi = 3;
                    }

                    if (ModeSelect == 0)
                    { //通常音声
                        if (sPersonal == "Pure")
                        {
                            VoiceList = cfg.sOrgasmVoice30PureVibe[vi];
                        }
                        else if (sPersonal == "Cool")
                        {
                            VoiceList = cfg.sOrgasmVoice30CoolVibe[vi];
                        }
                        else if (sPersonal == "Pride")
                        {
                            VoiceList = cfg.sOrgasmVoice30PrideVibe[vi];
                        }
                        else if (sPersonal == "Yandere")
                        {
                            VoiceList = cfg.sOrgasmVoice30YandereVibe[vi];
                        }
                        else if (sPersonal == "Anesan")
                        {
                            VoiceList = cfg.sOrgasmVoice30AnesanVibe[vi];
                        }
                        else if (sPersonal == "Genki")
                        {
                            VoiceList = cfg.sOrgasmVoice30GenkiVibe[vi];
                        }
                        else if (sPersonal == "Sadist")
                        {
                            VoiceList = cfg.sOrgasmVoice30SadistVibe[vi];
                        }
                        if (sPersonal == "Muku")
                        {
                            VoiceList = cfg.sOrgasmVoice30MukuVibe[vi];
                        }
                        else if (sPersonal == "Majime")
                        {
                            VoiceList = cfg.sOrgasmVoice30MajimeVibe[vi];
                        }
                        else if (sPersonal == "Rindere")
                        {
                            VoiceList = cfg.sOrgasmVoice30RindereVibe[vi];
                        }
                        else if (sPersonal == "Silent")
                        {
                            VoiceList = cfg.sOrgasmVoice30SilentVibe[vi];
                        }
                        else if (sPersonal == "Devilish")
                        {
                            VoiceList = cfg.sOrgasmVoice30DevilishVibe[vi];
                        }
                        else if (sPersonal == "Ladylike")
                        {
                            VoiceList = cfg.sOrgasmVoice30LadylikeVibe[vi];
                        }
                        else if (sPersonal == "Secretary")
                        {
                            VoiceList = cfg.sOrgasmVoice30SecretaryVibe[vi];
                        }
                        else if (sPersonal == "Sister")
                        {
                            VoiceList = cfg.sOrgasmVoice30SisterVibe[vi];
                        }
                    }
                    else if (ModeSelect == 1)
                    { //フェラ音声
                        if (sPersonal == "Pure")
                        {
                            VoiceList = cfg.sOrgasmVoice30PureFera[vi];
                        }
                        else if (sPersonal == "Cool")
                        {
                            VoiceList = cfg.sOrgasmVoice30CoolFera[vi];
                        }
                        else if (sPersonal == "Pride")
                        {
                            VoiceList = cfg.sOrgasmVoice30PrideFera[vi];
                        }
                        else if (sPersonal == "Yandere")
                        {
                            VoiceList = cfg.sOrgasmVoice30YandereFera[vi];
                        }
                        else if (sPersonal == "Anesan")
                        {
                            VoiceList = cfg.sOrgasmVoice30AnesanFera[vi];
                        }
                        else if (sPersonal == "Genki")
                        {
                            VoiceList = cfg.sOrgasmVoice30GenkiFera[vi];
                        }
                        else if (sPersonal == "Sadist")
                        {
                            VoiceList = cfg.sOrgasmVoice30SadistFera[vi];
                        }

                        if (sPersonal == "Muku")
                        {
                            VoiceList = cfg.sOrgasmVoice30MukuFera[vi];
                        }
                        else if (sPersonal == "Majime")
                        {
                            VoiceList = cfg.sOrgasmVoice30MajimeFera[vi];
                        }
                        else if (sPersonal == "Rindere")
                        {
                            VoiceList = cfg.sOrgasmVoice30RindereFera[vi];
                        }
                        else if (sPersonal == "Silent")
                        {
                            VoiceList = cfg.sOrgasmVoice30SilentFera[vi];
                        }
                        else if (sPersonal == "Devilish")
                        {
                            VoiceList = cfg.sOrgasmVoice30DevilishFera[vi];
                        }
                        else if (sPersonal == "Ladylike")
                        {
                            VoiceList = cfg.sOrgasmVoice30LadylikeFera[vi];
                        }
                        else if (sPersonal == "Secretary")
                        {
                            VoiceList = cfg.sOrgasmVoice30SecretaryFera[vi];
                        }
                        else if (sPersonal == "Sister")
                        {
                            VoiceList = cfg.sOrgasmVoice30SisterFera[vi];
                        }

                    }
                    else if (ModeSelect == 2)
                    { //カスタム音声１
                        VoiceList = cfg.sOrgasmVoice30Custom1[vi];
                    }
                    else if (ModeSelect == 3)
                    { //カスタム音声２
                        VoiceList = cfg.sOrgasmVoice30Custom2[vi];
                    }
                    else if (ModeSelect == 4)
                    { //カスタム音声３
                        VoiceList = cfg.sOrgasmVoice30Custom3[vi];
                    }
                    else if (ModeSelect == 5)
                    { //カスタム音声４
                        VoiceList = cfg.sOrgasmVoice30Custom4[vi];
                    }

                }

            }


            int iRandomVoice = UnityEngine.Random.Range(0, VoiceList.Length);
            if (OrgasmVoice != 0)
            {
                do
                {
                    while (iRandomVoice == iRandomVoiceBackup[vi] && VoiceList.Length > 1)
                    {
                        iRandomVoice = UnityEngine.Random.Range(0, VoiceList.Length);
                    }
                } while (VoiceList[iRandomVoice] == API.mMaidLastZcVoiceFN); //@API連動で追加//夜伽中の音声と重複を避けるため
                iRandomVoiceBackup[vi] = iRandomVoice;
            }

            debugPrintConsole("メイド性格 : " + sPersonal);
            // debugPrintConsole(string.Join(", ", cfg.sLoopVoice20PureVibe[0]));
            debugPrintConsole("list : " + string.Join(", ",VoiceList)+", index :  " + iRandomVoice + ", \r\n target : " + VoiceList[iRandomVoice]);

            Console.WriteLine("[DEBUG]vStateMajor:" + vStateMajor);

            //バイブ動作時の音声を実際に再生する
            if (vStateMajor == 20)
            {
                int index1 = Array.IndexOf(Edit_MaidsNum, Num);
                Console.WriteLine("[DEBUG]index1:" + index1);
                Console.WriteLine("[DEBUG]vsFlag[index1]:" + vsFlag[index1]);
                if (index1 == -1)
                {
                    vm.AudioMan.LoadPlay(VoiceList[iRandomVoice], 0f, false, true);
                }
                else if (vsFlag[index1] == 0)
                {
                    Console.WriteLine("[DEBUG]VoiceList:" + VoiceList[iRandomVoice]);
                    vm.AudioMan.LoadPlay(VoiceList[iRandomVoice], 0f, false, true);
                }
            }
            if (vStateMajor == 30)
            {
                if (OrgasmVoice == 0)
                {
                    int index1 = Array.IndexOf(Edit_MaidsNum, Num);
                    if (index1 == -1)
                    {
                        vm.AudioMan.LoadPlay(VoiceList[iRandomVoice], 0f, false, true);
                    }
                    else if (vsFlag[index1] == 0)
                    {
                        vm.AudioMan.LoadPlay(VoiceList[iRandomVoice], 0f, false, true);
                    }

                }
                else
                {
                    vm.AudioMan.LoadPlay(VoiceList[iRandomVoice], 0f, false, false);
                    OrgasmVoice = 2;   //絶頂音声再生中のフラグ

                    int index1 = Array.IndexOf(Edit_MaidsNum, Num);
                    if (index1 != -1) vsFlag[index1] = 0;

                }
            }



            //バイブ停止時の音声
            if (vStateMajor == 40)
            {
                int VoiceValue;

                if (vMaidStun)
                {
                    vi = 1;
                }
                else
                {
                    vi = 0;
                }

                if (vOrgasmCmb > 0)
                {
                    VoiceValue = 3 + vi;
                }
                else
                {
                    VoiceValue = vExciteLevel - 1 + vi;
                }

                if (ModeSelect == 2)
                {
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40Custom1[VoiceValue], 0f, false, true);

                }
                else if (ModeSelect == 3)
                {
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40Custom2[VoiceValue], 0f, false, true);

                }
                else if (ModeSelect == 4)
                {
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40Custom3[VoiceValue], 0f, false, true);

                }
                else if (ModeSelect == 5)
                {
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40Custom4[VoiceValue], 0f, false, true);

                }
                else if (sPersonal == "Pure")
                {
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40PureVibe[VoiceValue], 0f, false, true);

                }
                else if (sPersonal == "Cool")
                {
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40CoolVibe[VoiceValue], 0f, false, true);

                }
                else if (sPersonal == "Pride")
                {
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40PrideVibe[VoiceValue], 0f, false, true);

                }
                else if (sPersonal == "Yandere")
                {
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40YandereVibe[VoiceValue], 0f, false, true);

                }
                else if (sPersonal == "Anesan")
                {
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40AnesanVibe[VoiceValue], 0f, false, true);
                }
                else if (sPersonal == "Genki")
                {
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40GenkiVibe[VoiceValue], 0f, false, true);
                }
                else if (sPersonal == "Sadist")
                {
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40SadistVibe[VoiceValue], 0f, false, true);
                }

                else if (sPersonal == "Rindere")
                {
					Console.WriteLine("[DEBUG]sLoopVoice40RindereVibe:");
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40RindereVibe[VoiceValue], 0f, false, true);
                }
                
                else if (sPersonal == "Majime")
                {
					Console.WriteLine("[DEBUG]sLoopVoice40MajimeVibe:");
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40MajimeVibe[VoiceValue], 0f, false, true);
                }
                
                else if (sPersonal == "Muku")
                {
					Console.WriteLine("[DEBUG]sLoopVoice40MukuVibe:");
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40MukuVibe[VoiceValue], 0f, false, true);
                }
                
                else if (sPersonal == "Silent")
                {
					Console.WriteLine("[DEBUG]sLoopVoice40SilentVibe:");
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40SilentVibe[VoiceValue], 0f, false, true);
                }
                
                else if (sPersonal == "Devilish")
                {
					Console.WriteLine("[DEBUG]sLoopVoice40DevilishVibe:");
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40DevilishVibe[VoiceValue], 0f, false, true);
                }
                else if (sPersonal == "Ladylike")
                {
					Console.WriteLine("[DEBUG]sLoopVoice40LadylikeVibe:");
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40LadylikeVibe[VoiceValue], 0f, false, true);
                }
                else if (sPersonal == "Secretary")
                {
					Console.WriteLine("[DEBUG]sLoopVoice40SecretaryVibe:");
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40SecretaryVibe[VoiceValue], 0f, false, true);
                }
                else if (sPersonal == "Sister")
                {
					Console.WriteLine("[DEBUG]sLoopVoice40SisterVibe:");
                    vm.AudioMan.LoadPlay(cfg.sLoopVoice40SisterVibe[VoiceValue], 0f, false, true);
                }

                int index1 = Array.IndexOf(Edit_MaidsNum, Num);
                if (index1 != -1) vsFlag[index1] = 0;
            }

        }


        //SE変更処理
        private string seFileBack = "";
        private void ChangeSE(bool mode)
        {

            if (VLevel != 0)
            {

                int iSE = VLevel;
                string lm = maid.body0.LastAnimeFN;
                string seFile = "";

                if (cfgw.SelectSE != 2)
                {
                    seFile = SeFileList[iSE][cfgw.SelectSE];
                }
                else if (Regex.IsMatch(lm, "vibe") || Regex.IsMatch(lm, "omocya"))
                {
                    seFile = SeFileList[iSE][0];
                }
                else
                {
                    seFile = SeFileList[iSE][1];
                }

                if (seFile != seFileBack || mode)
                {
                    GameMain.Instance.SoundMgr.StopSe();
                    GameMain.Instance.SoundMgr.PlaySe(seFile, true);
                    seFileBack = seFile;
                }

            }
        }


        //メイドさんこっち来て処理
        private void ComeMaid(int i)
        {
            debugPrintConsole("ComeMaid call i:" + i);
            //カメラ位置取得
            Transform cameraPos;
            cameraPos = mainCamera.transform;
            if (bOculusVR)
            {
                try
                {
                    Transform vrCameraPos = Util.GetObject.ByString("GameMain.m_objInstance.m_OvrMgr.m_trEyeAnchor") as Transform;
                    cameraPos = vrCameraPos;
                }
                catch (Exception ex)
                {
                    debugPrintConsole(ex.ToString());
                }
            }

            float cpy = cameraPos.position.y;
            float cey = mainCamera.transform.eulerAngles.y;
            if (i == 0)
            {

                cpy -= 1.5f;
                if (cpy < 0f) cpy = 0f;

                cey += 180f;
                if (cey > 360f) cey -= 360f;

                maid.transform.position = new Vector3(cameraPos.position.x, cpy, cameraPos.position.z);
                maid.transform.eulerAngles = new Vector3(0f, cey, 0f);

                maid.CrossFadeAbsolute("kottikoi.anm", false, false, false, 0f, 1f);

            }
            else if (i == 1)
            {

                cpy -= 0.7f;
                if (cpy < 0f) cpy = 0f;

                maid.transform.position = new Vector3(cameraPos.position.x, cpy, cameraPos.position.z);
                maid.transform.eulerAngles = new Vector3(0f, cey, 0f);

                maid.CrossFadeAbsolute("kottikoi2.anm", false, false, false, 0f, 1f);
            }

            maid.EyeToCamera((Maid.EyeMoveType)5, 0.8f);

            string sPersonal = maid.status.personal.ToString();
            debugPrintConsole("sPersonal:" + sPersonal);
            if (sPersonal == "Pride") maid.AudioMan.LoadPlay(cfg.sCallVoice[0], 0f, false, false);
            if (sPersonal == "Cool") maid.AudioMan.LoadPlay(cfg.sCallVoice[1], 0f, false, false);
            if (sPersonal == "Pure") maid.AudioMan.LoadPlay(cfg.sCallVoice[2], 0f, false, false);
            if (sPersonal == "Yandere") maid.AudioMan.LoadPlay(cfg.sCallVoice[3], 0f, false, false);
            if (sPersonal == "Anesan") maid.AudioMan.LoadPlay(cfg.sCallVoice[4], 0f, false, false);
            if (sPersonal == "Genki") maid.AudioMan.LoadPlay(cfg.sCallVoice[5], 0f, false, false);
            if (sPersonal == "Sadist") maid.AudioMan.LoadPlay(cfg.sCallVoice[6], 0f, false, false);
            if (sPersonal == "Rindere") maid.AudioMan.LoadPlay(cfg.sCallVoice[7], 0f, false, false);
            if (sPersonal == "Majime") maid.AudioMan.LoadPlay(cfg.sCallVoice[8], 0f, false, false);
            if (sPersonal == "Muku") maid.AudioMan.LoadPlay(cfg.sCallVoice[9], 0f, false, false);
            if (sPersonal == "Silent") maid.AudioMan.LoadPlay(cfg.sCallVoice[10], 0f, false, false);
            if (sPersonal == "Devilish") maid.AudioMan.LoadPlay(cfg.sCallVoice[11], 0f, false, false);
            if (sPersonal == "Ladylike") maid.AudioMan.LoadPlay(cfg.sCallVoice[12], 0f, false, false);
            if (sPersonal == "Secretary") maid.AudioMan.LoadPlay(cfg.sCallVoice[13], 0f, false, false);
            if (sPersonal == "Sister") maid.AudioMan.LoadPlay(cfg.sCallVoice[14], 0f, false, false);

        }



        //CSVファイルのリストアップ処理
        private List<string> csvFilesR = new List<string>();
        private void CsvFilesCheck()
        {

            List<string> _fileR = new List<string>();
            string fileName = "";

            //CSV保存フォルダ確認
            if (!System.IO.Directory.Exists(@"Sybaris\UnityInjector\Config\VibeYourMaid\"))
            {
                //ない場合はフォルダ作成
                System.IO.DirectoryInfo di = System.IO.Directory.CreateDirectory(@"Sybaris\UnityInjector\Config\VibeYourMaid");
            }

            string[] files = Directory.GetFiles(@"Sybaris\UnityInjector\Config\VibeYourMaid\", "*.csv");

            foreach (string file in files)
            {
                fileName = Path.GetFileName(file);
                if (Regex.IsMatch(fileName, "^r_"))
                {
                    _fileR.Add(fileName);
                }
            }

            csvFilesR = _fileR;

        }


        //CSVファイルを読み込む
        private void CsvRead(string csv)
        {

            //保存先のファイル名
            string fileName = @"Sybaris\UnityInjector\Config\VibeYourMaid\" + csv;
            Console.WriteLine(fileName);

            autoMotionList = ReadCsvFaile(fileName);

        }


        private void CsvReadUvs()
        {

            //保存先のファイル名
            string fileName = @"Sybaris\UnityInjector\Config\VibeYourMaid\unzipvoiceset.csv";

            //CSV保存フォルダ確認
            if (!System.IO.Directory.Exists(@"Sybaris\UnityInjector\Config\VibeYourMaid\"))
            {
                //ない場合はフォルダ作成
                System.IO.DirectoryInfo di = System.IO.Directory.CreateDirectory(@"Sybaris\UnityInjector\Config\VibeYourMaid");
            }
            //CSVファイル確認
            if (!System.IO.File.Exists(fileName))
            {
                //ない場合はファイル作成
                CsvSaveUvs();
            }


            System.IO.StreamReader sr = new System.IO.StreamReader(fileName, System.Text.Encoding.GetEncoding("utf-8"));
            List<List<string>> cavData = new List<List<string>>{
              new List<string>(),
              new List<string>()
            };

            while (sr.Peek() > -1)
            {
                List<string> lineData = new List<string>();
                string m = sr.ReadLine();
                int i = 0;

                // 読み込んだ一行をカンマ毎に分けて配列に格納する
                string[] values = m.Split(',');
                // 出力する
                foreach (string value in values)
                {
                    string f = value.Replace(" ", "");
                    if (value != "")
                    {
                        cavData[i].Add(f);
                    }
                    ++i;
                }

            }

            sr.Close();

            unzipVoiceSet = cavData;

        }


        private void CsvSaveUvs()
        {

            using (var sw = new System.IO.StreamWriter(@"Sybaris\UnityInjector\Config\VibeYourMaid\unzipvoiceset.csv", false, System.Text.Encoding.GetEncoding("utf-8")))
            {
                for (int i = 0; i < unzipVoiceSet[0].Count; i++)
                {
                    sw.WriteLine("{0},{1}", unzipVoiceSet[0][i], unzipVoiceSet[1][i]);
                }
            }

        }


        private string[][] ReadCsvFaile(string file)
        {

            System.IO.StreamReader sr = new System.IO.StreamReader(file, System.Text.Encoding.GetEncoding("utf-8"));
            List<string[]> cavData = new List<string[]>();

            while (sr.Peek() > -1)
            {
                List<string> lineData = new List<string>();
                string m = sr.ReadLine();
                int i = 0;

                // 読み込んだ一行をカンマ毎に分けて配列に格納する
                string[] values = m.Split(',');
                // 出力する
                foreach (string value in values)
                {
                    if (value != "")
                    {
                        lineData.Add(value);
                    }
                    else if (i <= 3 && value == "")
                    {
                        lineData.Add("0");
                    }
                    ++i;
                }

                cavData.Add(lineData.ToArray());

            }

            sr.Close();
            return cavData.ToArray();
        }


        //オートモード処理
        private float autoTime1 = 0;
        private float autoTime2 = 0;
        private float autoTime3 = 0;
        private float autoTime4 = 0;
        private int ami1 = 0;
        private int ami2 = 0;
        private int ami1b = 0;
        private int ami2b = 0;
        private int iCsv = 0;
        private int kissChange = -1;
        private string[] amlBack = new string[] { "0", "0", "0" };
        private void autoMode()
        {

            float timerRate = Time.deltaTime * 60;
            Vector3 vm = maid.transform.position;
            Vector3 vm2 = man.transform.position;
            Vector3 em = maid.transform.eulerAngles;
            Vector3 em2 = man.transform.eulerAngles;

            //左ダブルクリックで大カテゴリチェンジ
            if (dClickL) autoTime2 = 0;

            //右ダブルクリックでランダムセット変更
            if (dClickR)
            {
                ++iCsv;
                if (iCsv >= csvFilesR.Count) iCsv = 0;
                CsvRead(csvFilesR[iCsv]);
                autoTime1 = 0;
                autoTime2 = 0;
                autoTime3 = 0;
                autoTime4 = 0;
            }

            //激しさチェンジ
            if (autoTime1 < 0 && autoSelect != 4)
            {
                if (VLevel == 0 || VLevel == 2)
                {
                    VLevel = 1;

                    if (autoSelect == 1)
                    {//じっくりモード
                        autoTime1 = UnityEngine.Random.Range(300, 900);
                    }
                    if (autoSelect == 2)
                    {//激しくモード
                        autoTime1 = UnityEngine.Random.Range(100, 200);
                    }
                    if (autoSelect == 3)
                    {//ほどほどモード
                        autoTime1 = UnityEngine.Random.Range(250, 600);
                    }

                }
                else if (VLevel == 1)
                {
                    VLevel = 2;

                    if (autoSelect == 1)
                    {//じっくりモード
                        if (UnityEngine.Random.Range(0, 100) < 15)
                        {
                            VLevel = 0;
                            autoTime1 = UnityEngine.Random.Range(250, 600);
                        }
                        else
                        {
                            autoTime1 = UnityEngine.Random.Range(180, 480);
                        }
                    }
                    if (autoSelect == 2)
                    {//激しくモード
                        autoTime1 = UnityEngine.Random.Range(300, 900);
                    }
                    if (autoSelect == 3)
                    {//ほどほどりモード
                        if (UnityEngine.Random.Range(0, 100) < 15)
                        {
                            VLevel = 0;
                        }
                        autoTime1 = UnityEngine.Random.Range(250, 600);
                    }

                }
            }

            //モーションチェンジ
            if (autoTime2 < 0)
            {  //大カテゴリ変更
                ami1 = UnityEngine.Random.Range(0, autoMotionList.GetLength(0));

                //向き変更
                if (autoMotionList[ami1][0] == amlBack[0])
                {
                    maid.transform.eulerAngles = new Vector3(float.Parse(autoMotionList[ami1][1]), em.y, em.z);
                    if (man.Visible) man.transform.eulerAngles = new Vector3(float.Parse(autoMotionList[ami1][1]), em2.y, em2.z);
                }
                else
                {
                    float ey = em.y + 180f;
                    if (ey >= 360f) ey -= 360f;
                    float ey2 = em2.y + 180f;
                    if (ey2 >= 360f) ey2 -= 360f;
                    maid.transform.eulerAngles = new Vector3(float.Parse(autoMotionList[ami1][1]), ey, em.z);
                    if (man.Visible) man.transform.eulerAngles = new Vector3(float.Parse(autoMotionList[ami1][1]), ey2, em2.z);
                }

                //高さ変更
                float py = (float.Parse(autoMotionList[ami1][2]) - float.Parse(amlBack[2])) / 100f;
                maid.transform.position = new Vector3(vm.x, vm.y + py, vm.z);
                if (man.Visible) man.transform.position = new Vector3(vm2.x, vm2.y + py, vm2.z);

                //ボイスセット読み込み
                string vs = "v_" + autoMotionList[ami1][3] + ".xml";
                voiceSetLoad(vs, 0);

                autoTime2 = UnityEngine.Random.Range(3000, 5000) + ((autoMotionList[ami1].Length - 5) * 300);
                autoTime3 = -100;

                amlBack = autoMotionList[ami1];
                Console.WriteLine("大カテゴリ変更：" + ami1);

            }

            if (autoTime3 < 0 || kissChange >= 0)
            {  //小カテゴリ変更

                ami2 = UnityEngine.Random.Range(4, autoMotionList[ami1].Length);
                if (kissChange >= 0)
                {
                    ami2 = kissChange;
                    kissChange = -2;
                    Console.WriteLine("キスモーションセット");
                }

                if (autoTime3 < -90) ami2 = 4;

                if (ami1 != ami1b || ami2 != ami2b)
                {
                    //メイドと男のモーション変更
                    if (kissChange == -2)
                    {
                        maid.CrossFadeAbsolute(autoMotionList[ami1][ami2], false, true, false, 1f, 1f);
                        MotionChangeMan(maidDataList[iCurrentMaid], 2f);
                        kissChange = -1;
                        Console.WriteLine("キス開始");
                    }
                    else
                    {
                        maid.CrossFadeAbsolute(autoMotionList[ami1][ami2], false, true, false, 1f, 1f);
                        MotionChangeMan(maidDataList[iCurrentMaid], 1f);
                    }

                    vStateHoldTimeM = 0; //モーションタイマーリセット
                    checkBlowjobing(maid, maidDataList[iCurrentMaid]); //フェラORキスのチェック

                    ami1b = ami1;
                    ami2b = ami2;
                }

                autoTime3 = UnityEngine.Random.Range(1000, 1500);

                Console.WriteLine("小カテゴリ変更：" + ami2);

            }


            //顔と目線のチェンジ
            if (autoTime4 < 0)
            {
                if (UnityEngine.Random.Range(0, 100) < 50) maid.body0.boEyeToCam = !maid.body0.boEyeToCam;
                if (maid.body0.boHeadToCam)
                {
                    if (UnityEngine.Random.Range(0, 100) < 70) maid.body0.boHeadToCam = !maid.body0.boHeadToCam;
                }
                else
                {
                    if (UnityEngine.Random.Range(0, 100) < 30) maid.body0.boHeadToCam = !maid.body0.boHeadToCam;
                }

                if (vMaidStun) maid.body0.boEyeToCam = false;
                if (vMaidStun || bIsBlowjobing[maidDataList[iCurrentMaid]] == 2 || (bIsBlowjobing[maidDataList[iCurrentMaid]] == 1 && fpsModeEnabled)) maid.body0.boHeadToCam = false;

                autoTime4 = UnityEngine.Random.Range(400, 600);
            }


            if (vOrgasmCmb == 0 || autoSelect == 1) autoTime1 -= timerRate;
            autoTime2 -= timerRate;
            autoTime3 -= timerRate;
            autoTime4 -= timerRate;

        }


        //オートモード終了時に向きと高さを元に戻す
        private void autoModeReset()
        {

            Vector3 vm = maid.transform.position;
            Vector3 vm2 = man.transform.position;
            Vector3 em = maid.transform.eulerAngles;
            Vector3 em2 = man.transform.eulerAngles;

            //向き変更
            if ("0" == amlBack[0])
            {
                maid.transform.eulerAngles = new Vector3(0f, em.y, em.z);
                if (man.Visible) man.transform.eulerAngles = new Vector3(0f, em2.y, em2.z);
            }
            else
            {
                float ey = em.y + 180f;
                if (ey >= 360f) ey -= 360f;
                float ey2 = em2.y + 180f;
                if (ey2 >= 360f) ey2 -= 360f;
                maid.transform.eulerAngles = new Vector3(0f, ey, em.z);
                if (man.Visible) man.transform.eulerAngles = new Vector3(0f, ey2, em2.z);
            }

            //高さ変更
            float py = (0f - float.Parse(amlBack[2])) / 100f;
            maid.transform.position = new Vector3(vm.x, vm.y + py, vm.z);
            if (man.Visible) man.transform.position = new Vector3(vm2.x, vm2.y + py, vm2.z);

            amlBack = new string[] { "0", "0", "0" };

        }



        //ダブルクリック判定処理
        private bool dClickL = false;
        private bool dClickR = false;
        private float delayTime1 = 0f;
        private float delayTime2 = 0f;
        private void DClicCheck()
        {
            if (delayTime1 > 0 && Input.GetMouseButtonDown(0)) dClickL = true;
            if (delayTime2 > 0 && Input.GetMouseButtonDown(1)) dClickR = true;

            if (delayTime1 <= 0 && Input.GetMouseButtonDown(0)) delayTime1 = 0.2f;
            if (delayTime2 <= 0 && Input.GetMouseButtonDown(1)) delayTime2 = 0.2f;

            if (!Input.GetMouseButtonDown(0) && dClickL) dClickL = false;
            if (!Input.GetMouseButtonDown(1) && dClickR) dClickR = false;
            if (delayTime1 > 0) delayTime1 -= Time.deltaTime;
            if (delayTime2 > 0) delayTime2 -= Time.deltaTime;
        }



        //フェードアウトチェック
        private class FadeMgr
        {
            public static bool isFade { get; private set; }
            private static bool isFadeMySelf { get; set; }

            public static void FadeOut()
            {
                isFadeMySelf = true;
                GameMain.Instance.LoadIcon.force_draw_new = true;
                GameMain.Instance.MainCamera.FadeOut(0f, false, null, true, default(Color));
            }

            public static void FadeIn()
            {
                if (GameMain.Instance.MainCamera.IsFadeProc() && isFadeMySelf)
                    return;

                isFadeMySelf = false;
                GameMain.Instance.LoadIcon.force_draw_new = false;
                GameMain.Instance.MainCamera.FadeIn(0f, false, null, true, true, default(Color));
            }

            public static bool GetFadeIn()
            {
                if (GameMain.Instance.MainCamera.IsFadeProc())
                    isFade = true;
                else if (isFade && !GameMain.Instance.MainCamera.IsFadeProc())
                {
                    isFade = false;
                    return true;
                }
                return false;
            }
        }



        //メイド切替時にカメラ追従
        private void CameraChange(Maid vm1, Maid vm2)
        {
            if (!fpsModeEnabled)
            {
                Vector3 vNewPosition = Vector3.zero;


                //　メイドさんの胸を取得する
                Transform[] objList = vm1.transform.GetComponentsInChildren<Transform>();
                if (objList.Count() != 0)
                {
                    maidMune = null;

                    foreach (var gameobject in objList)
                    {
                        if (gameobject.name == "Bip01 Spine1" && maidMune == null)
                        {
                            maidMune = gameobject;
                        }
                    }
                }

                //カメラの移動先決定
                //if(!bOculusVR){
                vNewPosition = maidMune.transform.position;
                //}else{
                //  Vector3 dPos = new Vector3( vm2.transform.position.x - vm1.transform.position.x , vm2.transform.position.y - vm1.transform.position.y , vm2.transform.position.z - vm1.transform.position.z );
                //  vNewPosition = new Vector3( mainCamera.transform.position.x + dPos.x , mainCamera.transform.position.y + dPos.y , mainCamera.transform.position.z + dPos.z );
                //}

                //カメラ移動
                //if(!bOculusVR){
                mainCamera.SetPos(vNewPosition);
                mainCamera.SetTargetPos(vNewPosition, true);
                mainCamera.SetDistance(1.4f, true);
                //}else{
                //  mainCamera.SetPos(vNewPosition);
                //}
            }
        }



        //一人称視点の処理
        private int frameCount;
        private float scrollValue = 0f;
        private Vector3 bManHeadPos = Vector3.zero;
        private void FpsModeChange()
        {

            Vector3 vNewPosition = Vector3.zero;

            if (frameCount <= 0)
            {

                //　顔目追従を無効
                //maid.EyeToCamera(Maid.EyeMoveType.無し, 0f);

                //　ご主人様の頭を取得する
                ManHeadGet();

                //　カメラの移動チェック
                float fDistanceToMandHead = Vector3.Distance(bManHeadPos, mainCamera.transform.position);

                //大きく移動していれば向きを調整
                if (fDistanceToMandHead > 0.3f)
                {
                    if (100 < manHead.transform.eulerAngles.z && manHead.transform.eulerAngles.z < 260)
                    {
                        float cy = manHead.transform.eulerAngles.y + 180f;
                        if (cy >= 360) cy -= 360;
                        mainCamera.transform.eulerAngles = new Vector3(manHead.transform.eulerAngles.x, cy, 0.0f);
                    }
                    else
                    {
                        float cx = manHead.transform.eulerAngles.x + 90f;
                        if (cx >= 360) cx -= 360;
                        mainCamera.transform.eulerAngles = new Vector3(cx, manHead.transform.eulerAngles.y, 0.0f);
                    }
                    Console.WriteLine("カメラ向き変更");

                    scrollValue = 0f;  //ホイール値リセット
                }

                bManHeadPos = mainCamera.transform.position;

                frameCount = 30;
            }
            else
            {
                --frameCount;
            }


            //頭の位置を挿入
            vNewPosition = manHead.transform.position;

            //マウスホイールでカメラ位置の前後調整
            if (Input.GetMouseButton(0)) scrollValue += Input.GetAxis("Mouse ScrollWheel") / 10;
            if (scrollValue > 0.2f) scrollValue = 0.2f;
            if (scrollValue < -0.2f) scrollValue = -0.2f;
            if (Input.GetMouseButtonDown(2)) scrollValue = 0f;
            if (scrollValue != 0)
            {
                Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 1, 1)).normalized;
                Vector3 moveForward = cameraForward * scrollValue;
                vNewPosition += moveForward;
            }

            //カメラ位置の移動
            if (!bOculusVR)
            {
                mainCamera.SetPos(vNewPosition);
                if (!vacationEnabled)
                {
                    mainCamera.SetTargetPos(vNewPosition, true);
                    mainCamera.SetDistance(0f, true);
                }
            }
            else
            {
                mainCamera.SetPos(vNewPosition);
            }


        }


        //ご主人様の頭取得
        private void ManHeadGet()
        {
            if (!manHead)
            {

                Transform[] objList = man.transform.GetComponentsInChildren<Transform>();
                if (objList.Count() != 0)
                {
                    manHead = null;

                    foreach (var gameobject in objList)
                    {
                        if (gameobject.name == "ManBip Head" && manHead == null)
                        {
                            manBipHead = gameobject;

                            foreach (Transform mh in manBipHead)
                            {
                                if (mh.name.IndexOf("_SM_mhead") > -1)
                                {
                                    GameObject smManHead = mh.gameObject;
                                    foreach (Transform smmh in smManHead.transform)
                                    {
                                        if (smmh.name.IndexOf("ManHead") > -1)
                                        {
                                            manHead = smmh.gameObject;
                                            manHeadRen = manHead.GetComponent<Renderer>();

                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }




        //　カメラの向き変更（顔の前を向かせる）
        /*private void setCameraDirection(){
            if (!bOculusVR){
              mainCamera.transform.eulerAngles = new Vector3(mainCamera.transform.eulerAngles.x, manHead.transform.eulerAngles.y, mainCamera.transform.eulerAngles.y);
              //Console.WriteLine("X:" +  manHead.transform.eulerAngles.x + "  Y:" +  manHead.transform.eulerAngles.y + "  Z:" +  manHead.transform.eulerAngles.z );

            }else{
              mainCamera.transform.eulerAngles = manHead.transform.eulerAngles;
              mainCamera.transform.Rotate(Vector3.up, -90.0f);
              mainCamera.transform.Rotate(Vector3.right, mainCamera.transform.eulerAngles.x * -1);//ｘ軸回転（pitch）の打ち消し
              mainCamera.transform.Rotate(Vector3.forward, mainCamera.transform.eulerAngles.z * -1); //ｚ軸回転（roll）の打ち消し
            }
        }*/




        //カメラとメイドさんの距離判定
        private float fDistanceToTarget;
        private float cameraCheckTime = 0f;
        private bool[] EyeToCamOld = new bool[20];
        private bool[] HeadToCamOld = new bool[20];
        private bool[] cameraCheck = new bool[20];

        private void CameraPosCheck()
        {

            if (cfgw.camCheckEnabled)
            {
                cameraCheckTime -= Time.deltaTime * 60;

                if (cameraCheckTime <= 0)
                {

                    cameraCheckTime = 60;

                    foreach (int i in maidDataList)
                    {

                        if (bIsBlowjobing[i] == 2) continue;

                        //　メイドさんの顔を取得する
                        Transform[] objList = SubMaids[i].transform.GetComponentsInChildren<Transform>();
                        if (objList.Count() == 0)
                        {

                        }
                        else
                        {
                            maidHead[i] = null;

                            foreach (var gameobject in objList)
                            {
                                if (gameobject.name == "Bone_Face" && maidHead[i] == null)
                                {
                                    maidHead[i] = gameobject;
                                }
                            }
                        }
                        //@API実装ついで->//NULL例外エラー修正 '17.04.30
                        if (!maidHead[i])
                        { //不特定のタイミングで取れないことがある様子
                            debugPrintConsole("CameraPosCheck maid" + i + " maidHead=Null -> Skip");
                            continue;
                        }
                        //@API実装ついで<-//ここまで//


                        //　主観視点のカメラ位置を取得
                        Transform cameraPos;
                        cameraPos = mainCamera.transform;
                        if (bOculusVR)
                        {
                            try
                            {
                                Transform vrCameraPos = Util.GetObject.ByString("GameMain.m_objInstance.m_OvrMgr.m_trEyeAnchor") as Transform;
                                cameraPos = vrCameraPos;
                            }
                            catch (Exception ex)
                            {
                                debugPrintConsole(ex.ToString());
                            }
                        }

                        //　主観視点と近接判定対象（ターゲット）の距離取得
                        float fDistanceToMaidHead = Vector3.Distance(maidHead[i].transform.position, cameraPos.position);
                        fDistanceToTarget = fDistanceToMaidHead;

                        float camCR = 0f;
                        if (!bOculusVR) camCR = cfgw.camCheckRange * (35.0f / Camera.main.fieldOfView);
                        if (bOculusVR) camCR = cfgw.camCheckRange * (60.0f / Camera.main.fieldOfView);

                        //if(bIsBlowjobing[i] != 2){
                        if (fDistanceToTarget < camCR && !cameraCheck[i])
                        {
                            EyeToCamOld[i] = SubMaids[i].body0.boEyeToCam;
                            HeadToCamOld[i] = SubMaids[i].body0.boHeadToCam;
                            cameraCheck[i] = true;

                            if (!fpsModeEnabled && man.Visible) SubMaids[i].EyeToCamera((Maid.EyeMoveType)5, 0.8f); //　一人称視点でない場合のみ、顔と目の追従を自動で有効にする
                            if (vMaidStun && i == maidDataList[iCurrentMaid]) SubMaids[i].body0.boEyeToCam = false;

                            if (bIsBlowjobing[i] != 1)
                            {
                                maid.AudioMan.Stop();     //現在の音声停止
                                vStateHoldTime = 0;     //音声タイマーリセット
                            }

                        }
                        else if (fDistanceToTarget >= camCR && cameraCheck[i])
                        {
                            SubMaids[i].body0.boEyeToCam = EyeToCamOld[i];
                            SubMaids[i].body0.boHeadToCam = HeadToCamOld[i];
                            cameraCheck[i] = false;
                            vStateHoldTime = 0;

                        }
                        //}
                    }
                }
            }
            else
            {
                foreach (int i in maidDataList)
                {
                    if (cameraCheck[i])
                    {
                        SubMaids[i].body0.boEyeToCam = EyeToCamOld[i];
                        SubMaids[i].body0.boHeadToCam = HeadToCamOld[i];
                        vStateHoldTime = 0;
                    }
                }
                cameraCheck = new bool[] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };

            }

        }


        //XMLファイルのリストアップ処理
        private void XmlFilesCheck()
        {

            List<string> _fileY = new List<string>();
            List<string> _fileV = new List<string>();
            string fileName = "";

            //XML保存フォルフォルダ確認
            if (!System.IO.Directory.Exists(@"Sybaris\UnityInjector\Config\VibeYourMaid\"))
            {
                //ない場合はフォルダ作成
                System.IO.DirectoryInfo di = System.IO.Directory.CreateDirectory(@"Sybaris\UnityInjector\Config\VibeYourMaid");
            }

            string[] files = Directory.GetFiles(@"Sybaris\UnityInjector\Config\VibeYourMaid\", "*.xml");

            foreach (string file in files)
            {
                fileName = Path.GetFileName(file);
                if (Regex.IsMatch(fileName, "^y_"))
                {
                    _fileY.Add(fileName);
                }
                else if (Regex.IsMatch(fileName, "^v_"))
                {
                    _fileV.Add(fileName);
                }
            }

            xmlFilesY = _fileY;
            xmlFilesV = _fileV;

        }



        //夜伽エディットにXMLファイルを読み込む
        private void EditRead(string xml)
        {

            //保存先のファイル名
            string fileName = @"Sybaris\UnityInjector\Config\VibeYourMaid\" + xml;
            Console.WriteLine(fileName);

            //XmlSerializerオブジェクトを作成
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(YotogiEdit_Xml));
            //読み込むファイルを開く
            System.IO.StreamReader sr = new System.IO.StreamReader(fileName, new System.Text.UTF8Encoding(false));

            //XMLファイルから読み込み、逆シリアル化する
            YEX = (YotogiEdit_Xml)serializer.Deserialize(sr);

            //ファイルを閉じる
            sr.Close();
            Console.WriteLine("読み込み完了");


            //メイドの人数チェック
            int Count = 0;
            foreach (bool b in YEX.Edit_MaidEnabled)
            {
                if (b) { ++Count; }
            }
            Console.WriteLine(Count);
            if (maidDataList.Count >= Count)
            {
                Console.WriteLine("適用開始");
                EditLoad();
            }
            else
            {
                Edit_SubMenu = 3;
            }

        }



        //読み込んだXMLファイルを夜伽EDITに適用する
        private void EditLoad()
        {

            GetMaidCount();
            Vector3 mp = maid.transform.position;

            //夜伽名適用
            Edit_YotogiNameT = YEX.Edit_YotogiName;

            RankoEnabled = YEX.Edit_RankoEnabled; //乱交の有無

            //メイドに適用
            for (int i = 0; i < Edit_MaidsNum.Length; i++)
            {

                if (YEX.Edit_MaidEnabled[i] && Edit_MaidsNum[i] < 0)
                {
                    foreach (int m in maidDataList)
                    {
                        if (!Edit_MaidsNum.Contains(m))
                        {
                            Edit_MaidsNum[i] = m;
                            break;
                        }
                    }
                }


                if (Edit_MaidsNum[i] >= 0 && YEX.Edit_MaidEnabled[i])
                {
                    Maid Emaid = SubMaids[Edit_MaidsNum[i]];

                    //モーション適用
                    Emaid.CrossFadeAbsolute(YEX.Edit_MaidMotion[i], false, true, false, 0.7f, 1f);
                    Edit_MaidMotionT[i] = YEX.Edit_MaidMotion[i];

                    //位置情報と向き情報を適用
                    if (i != 0)
                    {
                        Emaid.transform.position = new Vector3(mp.x + YEX.Edit_MaidPos[i][0], mp.y + YEX.Edit_MaidPos[i][1], mp.z + YEX.Edit_MaidPos[i][2]);
                    }
                    Emaid.transform.eulerAngles = new Vector3(YEX.Edit_MaidEul[i][0], YEX.Edit_MaidEul[i][1], YEX.Edit_MaidEul[i][2]);

                    //ボイスセット関連適用
                    madiVoiceSetName[i] = YEX.Edit_VoiceSet[i];
                    vsInterval[i] = YEX.Edit_VsInterval[i];
                    string xml = "v_" + madiVoiceSetName[i] + ".xml";
                    voiceSetLoad(xml, i);

                }
                else
                {
                    Edit_MaidMotionT[i] = ""; //モーション名を初期化
                    YEX.Edit_MaidPos[i] = new float[] { 0, 0, 0 };  //位置初期化
                    YEX.Edit_MaidEul[i] = new float[] { 0, 0, 0 };  //向き初期化
                }
            }


            //男に適用
            for (int i = 0; i < 4; i++)
            {
                Maid Eman = SubMans[i];

                Eman.Visible = YEX.Edit_ManEnabled[i];  //男の表示状態

                //ターゲットメイドの適用
                if (Edit_MaidsNum[YEX.Edit_MansTg[i]] >= 0)
                {
                    MansTg[i] = Edit_MaidsNum[YEX.Edit_MansTg[i]];
                }
                else
                {
                    Eman.Visible = false;
                }

                if (Eman.Visible && YEX.Edit_MansTg[i] >= 0)
                {

                    //位置情報と向き情報を適用
                    Eman.transform.position = new Vector3(mp.x + YEX.Edit_ManPos[i][0], mp.y + YEX.Edit_ManPos[i][1], mp.z + YEX.Edit_ManPos[i][2]);
                    Eman.transform.eulerAngles = new Vector3(YEX.Edit_ManEul[i][0], YEX.Edit_ManEul[i][1], YEX.Edit_ManEul[i][2]);

                    //モーション適用
                    Eman.CrossFadeAbsolute(YEX.Edit_ManMotion[i], false, true, false, 0.7f, 1f);

                }
                else
                {
                    YEX.Edit_ManEnabled[i] = false;
                    YEX.Edit_ManMotion[i] = "";  //モーション名を初期化
                    YEX.Edit_ManPos[i] = new float[] { 0, 0, 0 };  //位置初期化
                    YEX.Edit_ManEul[i] = new float[] { 0, 0, 0 };  //向き初期化

                }
            }

        }


        //夜伽エディットにて、XMLファイルに保存するために現在の状態を取得する
        private void EditSave()
        {

            //夜伽名読み込み
            YEX.Edit_YotogiName = Edit_YotogiNameT;

            //乱交モードチェック
            YEX.Edit_RankoEnabled = RankoEnabled;

            //メイド状態読み込み
            for (int i = 0; i < Edit_MaidsNum.Length; i++)
            {

                if (Edit_MaidsNum[i] >= 0)
                {
                    Maid Emaid = SubMaids[Edit_MaidsNum[i]];

                    YEX.Edit_MaidEnabled[i] = true;

                    //各メイドのモーション読み込み
                    if (Edit_MaidMotionT[i] == "")
                    {
                        YEX.Edit_MaidMotion[i] = Emaid.body0.LastAnimeFN;
                    }
                    else
                    {
                        YEX.Edit_MaidMotion[i] = Edit_MaidMotionT[i];
                    }

                    if (i == 0)
                    {
                        YEX.Edit_MaidPos[i] = new float[] {  //各メイドの現在位置読み込み
                  Emaid.transform.position.x , Emaid.transform.position.y , Emaid.transform.position.z
                };

                        YEX.Edit_MaidEul[i] = new float[] {  //各メイドの現在向き読み込み
                  Emaid.transform.eulerAngles.x , Emaid.transform.eulerAngles.y , Emaid.transform.eulerAngles.z
                };

                    }
                    else
                    {
                        YEX.Edit_MaidPos[i] = new float[] {  //各メイドの現在位置読み込み
                  Emaid.transform.position.x - YEX.Edit_MaidPos[0][0] , Emaid.transform.position.y - YEX.Edit_MaidPos[0][1] , Emaid.transform.position.z - YEX.Edit_MaidPos[0][2]
                };

                        YEX.Edit_MaidEul[i] = new float[] {  //各メイドの現在向き読み込み
                  Emaid.transform.eulerAngles.x , Emaid.transform.eulerAngles.y , Emaid.transform.eulerAngles.z
                };

                    }

                    //ボイスセット関連
                    YEX.Edit_VoiceSet[i] = madiVoiceSetName[i];
                    YEX.Edit_VsInterval[i] = vsInterval[i];

                }
                else
                {
                    YEX.Edit_MaidEnabled[i] = false;
                    YEX.Edit_MaidMotion[i] = ""; //モーション名を初期化
                    YEX.Edit_MaidPos[i] = new float[] { 0, 0, 0 };  //位置初期化
                    YEX.Edit_MaidEul[i] = new float[] { 0, 0, 0 };  //向き初期化
                }
            }

            //男状態読み込み
            for (int i = 0; i < 4; i++)
            {
                Maid Eman = SubMans[i];

                YEX.Edit_ManEnabled[i] = Eman.Visible;  //男の表示状態

                YEX.Edit_MansTg[i] = Array.IndexOf(Edit_MaidsNum, MansTg[i]);

                if (Eman.Visible && YEX.Edit_MansTg[i] >= 0)
                {

                    YEX.Edit_ManMotion[i] = Eman.body0.LastAnimeFN;  //各男の現在モーション読み込み

                    YEX.Edit_ManPos[i] = new float[] {  //各男の現在位置読み込み
                Eman.transform.position.x - YEX.Edit_MaidPos[0][0] , Eman.transform.position.y - YEX.Edit_MaidPos[0][1] , Eman.transform.position.z - YEX.Edit_MaidPos[0][2]
              };

                    YEX.Edit_ManEul[i] = new float[] {  //各男の現在向き読み込み
                Eman.transform.eulerAngles.x , Eman.transform.eulerAngles.y , Eman.transform.eulerAngles.z
              };

                }
                else
                {
                    YEX.Edit_ManEnabled[i] = false;
                    YEX.Edit_ManMotion[i] = "";  //モーション名を初期化
                    YEX.Edit_ManPos[i] = new float[] { 0, 0, 0 };  //位置初期化
                    YEX.Edit_ManEul[i] = new float[] { 0, 0, 0 };  //向き初期化

                }
            }

        }



        //ボイスセットにXMLファイルを読み込む
        private void voiceSetLoad(string xml, int num)
        {

            //保存先のファイル名
            string fileName = @"Sybaris\UnityInjector\Config\VibeYourMaid\" + xml;
            Console.WriteLine(fileName);

            if (System.IO.File.Exists(fileName))
            {
                //XmlSerializerオブジェクトを作成
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(VoiceSet_Xml));
                //読み込むファイルを開く
                System.IO.StreamReader sr = new System.IO.StreamReader(fileName, new System.Text.UTF8Encoding(false));

                //XMLファイルから読み込み、逆シリアル化する
                VSX = (VoiceSet_Xml)serializer.Deserialize(sr);

                //ファイルを閉じる
                sr.Close();
                Console.WriteLine("読み込み完了");

                //読み込んだ情報を挿入
                editVoiceSetName = VSX.saveVoiceSetName;
                editVoiceSet = VSX.saveVoiceSet;

                if (0 <= num && num <= 3) madiVoiceSetName[num] = VSX.saveVoiceSetName;
                if (num == 0) madiVoiceSet0 = VSX.saveVoiceSet;
                if (num == 1) madiVoiceSet1 = VSX.saveVoiceSet;
                if (num == 2) madiVoiceSet2 = VSX.saveVoiceSet;
                if (num == 3) madiVoiceSet3 = VSX.saveVoiceSet;

            }
            else
            {
                madiVoiceSetName[num] = "";

                if (num == 0) madiVoiceSet0 = new List<string[]>{
               new string[] { "" , "0" , "0" , "3" , "0" , "3" , "0" , "0" }
             };
                if (num == 1) madiVoiceSet1 = new List<string[]>{
               new string[] { "" , "0" , "0" , "3" , "0" , "3" , "0" , "0" }
             };
                if (num == 2) madiVoiceSet2 = new List<string[]>{
               new string[] { "" , "0" , "0" , "3" , "0" , "3" , "0" , "0" }
             };
                if (num == 3) madiVoiceSet3 = new List<string[]>{
               new string[] { "" , "0" , "0" , "3" , "0" , "3" , "0" , "0" }
             };

            }

        }


        //ボイスセットをXMLファイルに保存する
        private void voiceSetSave(int num)
        {

            // フォルダ確認
            if (!System.IO.Directory.Exists(@"Sybaris\UnityInjector\Config\VibeYourMaid\"))
            {
                //ない場合はフォルダ作成
                System.IO.DirectoryInfo di = System.IO.Directory.CreateDirectory(@"Sybaris\UnityInjector\Config\VibeYourMaid");
            }

            //現在状態の読み込み
            VSX.saveVoiceSetName = editVoiceSetName;
            VSX.saveVoiceSet = editVoiceSet;

            if (VSX.saveVoiceSetName == "")
            {  //夜伽名が空白の場合保存しない
                vsErrer = 1;

            }
            else
            {
                //保存先のファイル名
                string fileName = @"Sybaris\UnityInjector\Config\VibeYourMaid\V_" + VSX.saveVoiceSetName + @".xml";

                if (System.IO.File.Exists(fileName) && !vs_Overwrite)
                {  //上書きのチェック
                    vsErrer = 2;

                }
                else
                {

                    //XmlSerializerオブジェクトを作成
                    //オブジェクトの型を指定する
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(VoiceSet_Xml));

                    //書き込むファイルを開く（UTF-8 BOM無し）
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName, false, new System.Text.UTF8Encoding(false));

                    //シリアル化し、XMLファイルに保存する
                    serializer.Serialize(sw, VSX);
                    //ファイルを閉じる
                    sw.Close();

                    vs_Overwrite = false;
                    vsErrer = 0;

                    if (0 <= num && num <= 3) madiVoiceSetName[num] = VSX.saveVoiceSetName;
                    if (num == 0) madiVoiceSet0 = VSX.saveVoiceSet;
                    if (num == 1) madiVoiceSet1 = VSX.saveVoiceSet;
                    if (num == 2) madiVoiceSet2 = VSX.saveVoiceSet;
                    if (num == 3) madiVoiceSet3 = VSX.saveVoiceSet;

                }
            }

        }


        //ボイスセット再生処理
        private float[] vsTime = new float[] { 0, 0, 0, 0 };
        private int[] vsFlag = new int[] { 0, 0, 0, 0 };

        private void VoiceSetPlay()
        {
            string[] vsFile;

            for (int i = 0; i < 4; i++)
            {
                if (Edit_MaidsNum[i] != -1)
                {
                    Maid vm = SubMaids[Edit_MaidsNum[i]];

                    if (vStateMajor == 20 || vStateMajor == 30)
                    {
                        if (vsFlag[i] == 0)
                        {
                            vsTime[i] -= Time.deltaTime * 60; //再生していないときだけタイマーを動かす
                                                              //Console.WriteLine(vsTime[i]);
                        }

                        if (vsTime[i] < 0 && OrgasmVoice == 0)
                        {
                            vsFile = VoiceSetCheck(i, vm); //該当する音声ファイルをリストアップ
                            debugPrintConsole("VoiceSetPlay i:" + i + " vm:" + vm + " vsFile:" + vsFile + " vsFlag[i]" + vsFlag[i]);
                            //音声再生
                            if (vsFlag[i] == 1)
                            {
                                int iRandomVoice = UnityEngine.Random.Range(0, vsFile.Length);
                                debugPrintConsole("VoiceSetPlay vsFile[iRandomVoice]:" + vsFile[iRandomVoice]);
                                vm.AudioMan.LoadPlay(vsFile[iRandomVoice], 0f, false, false);

                                if (vm.AudioMan.FileName == vsFile[iRandomVoice])
                                {
                                    vsFlag[i] = 2; //再生中フラグON
                                }
                                else
                                {
                                    vsFlag[i] = 0; //再生フラグOFF
                                }
                            }

                            vsTime[i] = UnityEngine.Random.Range(vsInterval[i] - 200f, vsInterval[i] + 200f);

                        }
                    }

                    //音声が終わった時の処理
                    if (vsFlag[i] == 2 && !vm.AudioMan.audiosource.isPlaying)
                    {
                        vsFlag[i] = 0;        //再生中フラグOFF

                        if (i == 0)
                        {
                            vStateHoldTime = 0;       //音声をすぐ再生するため、タイマーリセット
                        }
                        else
                        {
                            MaidVoicePlay(vm, Edit_MaidsNum[i]);  //サブメイドの場合は直接再生
                        }
                    }

                }
            }

        }

        //ボイスセットの該当チェック
        private string[] VoiceSetCheck(int num, Maid vm)
        {
            debugPrintConsole("VoiceSetCheck call num:" + num + " vm:" + vm);
            debugPrintConsole("vm.status.personal.ToString" + vm.status.personal.ToString());
            int iPersonal = Array.IndexOf(personalList[1], vm.status.personal.uniqueName);

            int eLevel = vExciteLevel - 1;

            int oLevel;
            if (Math.Floor(vOrgasmValue) < 30)
            {
                oLevel = 0;
            }
            else if (Math.Floor(vOrgasmValue) < 50)
            {
                oLevel = 1;
            }
            else if (Math.Floor(vOrgasmValue) < 80)
            {
                oLevel = 2;
            }
            else
            {
                oLevel = 3;
            }


            int iState = -1;
            if (num == 0)
            {
                if (vStateMajor == 20) iState = 0;
                if (vStateMajor == 30) iState = 1;

            }
            else
            {
                //メイド名とセーブ番号取得
                string MaidName = vm.status.lastName + " " + vm.status.firstName;
                int mn = MaidNameSave.IndexOf(MaidName);
                if (VLevelSave[mn] == 1) iState = 0;
                if (VLevelSave[mn] == 2) iState = 1;

            }

            checkBlowjobing(vm, num);
            int iCondition = bIsBlowjobing[num];
            if (vOrgasmHoldTime > 0) iCondition = 3;
            if (vMaidStun) iCondition = 4;


            List<string> _vsFile = new List<string>();
            List<string[]> vsList = new List<string[]>();
            if (num == 0) vsList = madiVoiceSet0;
            if (num == 1) vsList = madiVoiceSet1;
            if (num == 2) vsList = madiVoiceSet2;
            if (num == 3) vsList = madiVoiceSet3;

            foreach (string[] vs in vsList)
            {
                if (Regex.IsMatch(vs[2], "[^0-3]")) vs[2] = "0";
                if (Regex.IsMatch(vs[3], "[^0-3]")) vs[3] = "3";
                if (Regex.IsMatch(vs[4], "[^0-3]")) vs[4] = "0";
                if (Regex.IsMatch(vs[5], "[^0-3]")) vs[5] = "3";

debugPrintConsole("VoiceSetCheck checking intCnv(vs[1])" + intCnv(vs[1]));
debugPrintConsole("VoiceSetCheck checking intCnv(vs[2])" + intCnv(vs[2]));
debugPrintConsole("VoiceSetCheck checking intCnv(vs[3])" + intCnv(vs[3]));
debugPrintConsole("VoiceSetCheck checking intCnv(vs[4])" + intCnv(vs[4]));
debugPrintConsole("VoiceSetCheck checking intCnv(vs[5])" + intCnv(vs[5]));
debugPrintConsole("VoiceSetCheck checking intCnv(vs[6])" + intCnv(vs[6]));
debugPrintConsole("VoiceSetCheck checking intCnv(vs[7])" + intCnv(vs[7]));
debugPrintConsole("VoiceSetCheck checking eLevel" + eLevel);
debugPrintConsole("VoiceSetCheck checking oLevel" + oLevel);
debugPrintConsole("VoiceSetCheck checking iState" + iState);
debugPrintConsole("VoiceSetCheck checking iCondition" + iCondition);

                if (vs[0] != "" && (intCnv(vs[1]) == iPersonal || intCnv(vs[1]) == personalList[0].Length - 1)
                   && (intCnv(vs[2]) <= eLevel && eLevel <= intCnv(vs[3]))
                   && (intCnv(vs[4]) <= oLevel && oLevel <= intCnv(vs[5]))
                   && (intCnv(vs[6]) == iState || intCnv(vs[6]) == 2)
                   && (intCnv(vs[7]) == iCondition || intCnv(vs[7]) == 5))
                {
                    debugPrintConsole("VoiceSetCheck checking iPersonal:" + iPersonal + "intCnv(vs[1])" + intCnv(vs[1]));
                    _vsFile.Add(vs[0]);
                    if (vsFlag[num] == 0) vsFlag[num] = 1;
                }

            }
            return _vsFile.ToArray();
        }


        private void VibeDataClear(int mode)
        {

            //余韻変更が有効時に余韻状態に変更する
            if (cfgw.TaikiEnabled && vState != 10 && mode == 1)
            {
                vStateMajor = 40;

                //モーション変更
                MotionChange(false);

                //表情変更
                string sFaceAnimeName = "";
                if (vOrgasmCmb > 0)
                {
                    sFaceAnimeName = cfg.sFaceAnime40Vibe[3];
                }
                else
                {
                    sFaceAnimeName = cfg.sFaceAnime40Vibe[vExciteLevel - 1];
                }
                //　""か"変更しない"でなければ、フェイスアニメを適用する
                if (sFaceAnimeName != "" && sFaceAnimeName != "変更しない")
                {
                    maid.FaceAnime(sFaceAnimeName, cfg.fAnimeFadeTimeV, 0);
                }

                //音声変更
                string sPersonal = maid.status.personal.ToString();
                int VoiceValue;
                int vi = 0;
                if (vMaidStun)
                {
                    vi = 1;
                }
                else
                {
                    vi = 0;
                }
                if (vOrgasmCmb > 0)
                {
                    VoiceValue = 3 + vi;
                }
                else
                {
                    VoiceValue = vExciteLevel - 1 + vi;
                }

                if (ModeSelect == 2)
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40Custom1[VoiceValue], 0f, false, false);
                }
                else if (ModeSelect == 3)
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40Custom2[VoiceValue], 0f, false, false);
                }
                else if (ModeSelect == 4)
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40Custom3[VoiceValue], 0f, false, false);
                }
                else if (ModeSelect == 5)
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40Custom4[VoiceValue], 0f, false, false);
                }
                else if (sPersonal == "Pure")
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40PureVibe[VoiceValue], 0f, false, false);
                }
                else if (sPersonal == "Cool")
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40CoolVibe[VoiceValue], 0f, false, false);
                }
                else if (sPersonal == "Pride")
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40PrideVibe[VoiceValue], 0f, false, false);
                }
                else if (sPersonal == "Yandere")
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40YandereVibe[VoiceValue], 0f, false, false);
                }
                else if (sPersonal == "Anesan")
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40AnesanVibe[VoiceValue], 0f, false, false);
                }
                else if (sPersonal == "Genki")
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40GenkiVibe[VoiceValue], 0f, false, false);
                }
                else if (sPersonal == "Sadist")
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40SadistVibe[VoiceValue], 0f, false, false);
                }
                else if (sPersonal == "Rindere")
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40RindereVibe[VoiceValue], 0f, false, false);
                }
                else if (sPersonal == "Majime")
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40MajimeVibe[VoiceValue], 0f, false, false);
                }
                else if (sPersonal == "Muku")
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40MukuVibe[VoiceValue], 0f, false, false);
                }
                else if (sPersonal == "Silent")
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40SilentVibe[VoiceValue], 0f, false, false);
                }
                else if (sPersonal == "Devilish")
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40DevilishVibe[VoiceValue], 0f, false, false);
                }
                else if (sPersonal == "Ladylike")
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40LadylikeVibe[VoiceValue], 0f, false, false);
                }
                else if (sPersonal == "Secretary")
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40SecretaryVibe[VoiceValue], 0f, false, false);
                }
                else if (sPersonal == "Sister")
                {
                    maid.AudioMan.LoadPlay(cfg.sLoopVoice40SisterVibe[VoiceValue], 0f, false, false);
                }

            }


            //　表情の復元
            if (mode == 0 || cfgw.ClearEnabled)
            {
                restoreFace();
            }
            else
            {

                FaceBackupSave[MaidNum] = sFaceAnimeBackupV;

                sFaceAnimeBackupV = "";
                sFaceBlendBackupV = "";
            }

            //　バイブ音停止
            GameMain.Instance.SoundMgr.StopSe();

            //　ステートをリセット
            vStateMajor = 10;
            vStateMinor = 0;
            vStateMajorOld = 10;

            //　興奮値をリセット
            iCurrentExcite = 0;

            //　音声の復元もしくは停止
            if (bIsVoiceOverridingV && (mode == 0 || cfgw.ClearEnabled))
            {

                //　オーバライド状態を解除
                bIsVoiceOverridingV = false;
                bOverrideInterruptedV = false;

                //　復元もしくは停止
                if (sLoopVoiceBackupV != "")
                {
                    maid.AudioMan.LoadPlay(sLoopVoiceBackupV, 0f, false, true);
                    debugPrintConsole("voice restore done. " + sLoopVoiceBackupV);

                }
                else
                {
                    maid.AudioMan.Stop();
                    debugPrintConsole("voice stop done. " + sLoopVoiceBackupV);
                }

            }

            sLoopVoiceBackupV = "";

            //モーションの復元もしくは停止
            if (mcFlag >= 0 && (mode == 0 || cfgw.ClearEnabled))
            {
                maid.CrossFadeAbsolute(MaidMotionBack, false, true, false, 1f, 1f);
                mcFlag = -1;
            }
            else
            {
                MotionBackupSave[MaidNum] = MaidMotionBack;
            }
            MaidMotionBack = "";

            //メイド変更時かつ、クリア設定が無効のときはメイドのバイブ状態を保存
            if (mode == 1 && !cfgw.ClearEnabled)
            {
                VLevelSave[MaidNum] = VLevel;
                Console.WriteLine("VLevelSave:" + VLevel);
            }
            else
            {
                //それ以外の時は、バイブ状態の保存をクリアし、バイブ停止
                VLevelSave[MaidNum] = 0;
                VLevel = 0;
            }


            StatusClear();


        }

        private void StatusClear()
        {

            //各ステータスリセット
            vOrgasmValue = 0;
            vOrgasmCmb = 0;
            OrgasmVoice = 0;
            clitorisValue1 = 0;
            AheValue = 0;
            AheValue2 = 0;
            vJirashi = 0;
            vBoostBonus = 0;
            updateShapeKeyOrgasmValue(0f);
            updateShapeKeyEnemaValue(0f, 0f);
            updateShapeKeyBreastValue(0f, 0f);
            updateShapeKeyChinpoValue(0f, 0f);
            EnemaFlag = false;
            vMaidStamina = 3000;
            vMaidStun = false;
            SioFlag = false;
            SioFlag2 = false;
            SioTime = 0;
            SioTime2 = 0;


            //時間リセット
            vStateHoldTimeM = 0;
            vStateHoldTime = 0;
            vStateHoldTime2 = 0;
            vStateHoldTimeM = 0;

            //オートモード関連
            autoTime1 = 0;
            autoTime2 = 0;
            autoTime3 = 0;
            autoTime4 = 0;
            autoModeReset();
            //amlBack = new string[]{"0","0","0"};

        }



        private void MaidDataClear()
        {
            maidDataList.Clear();
            iCurrentMaid = 0;
        }

        private void GetMaidCount()
        {
            List<int> _maidList = new List<int>();
            Maid[] _SubMaids = new Maid[20];

            for (int i = 0; i < cm.GetMaidCount(); i++)
            {
                Maid maids = cm.GetMaid(i);
                if (!ChkMaid(maids))
                    continue;
                _maidList.Add(i);
                if (i < 20) { _SubMaids[i] = maids; }

                //　メイドの状態セーブスロット作成
                string MaidName = maids.status.lastName + " " + maids.status.firstName;
                int mn = MaidNameSave.IndexOf(MaidName);

                if (mn == -1)
                {
                    MaidNameSave.Add(MaidName);
                    vBoostBaseSave.Add(vBoostDef);
                    vOrgasmCountSave.Add(0);
                    VLevelSave.Add(0);
                    FaceBackupSave.Add("");
                    MotionBackupSave.Add("");
                    Console.WriteLine("セーブスロット作成:" + MaidName);
                }

            }

            maidDataList = _maidList;
            SubMaids = _SubMaids;
        }

        private bool ChkMaid(Maid m)
        {
            return m != null && m.Visible && m.AudioMan != null;
        }




        private List<string> ReadTextFaile(string file, string section)
        {

            System.IO.StreamReader sr = new System.IO.StreamReader(file);
            bool ReadFlag = false;
            List<string> _ListData = new List<string>();

            while (sr.Peek() > -1)
            {
                string m = sr.ReadLine();

                if (!ReadFlag && m == "[" + section + "]")
                {
                    //Console.WriteLine("読み込み開始:" + m);
                    ReadFlag = true;
                    continue;
                }
                if (ReadFlag && m == "[end]")
                {
                    //Console.WriteLine("読み込み終了:" + m);
                    ReadFlag = false;
                    break;
                }

                if (ReadFlag)
                {
                    _ListData.Add(m);
                    //Console.WriteLine(m);
                }

            }

            return _ListData;
        }


        private bool isExistVertexMorph(TBody body, string sTag)
        {

            for (int i = 0; i < body.goSlot.Count; i++)
            {
                TMorph morph = body.goSlot[i].morph;
                if (morph != null)
                {
                    if (morph.Contains(sTag))
                    {
                        return true;
                    }
                }
            }
            return false;
        }        //----


        /*private bool VertexMorph_FromProcItem(TBody body, string sTag, float f)
        {
            bool bFace = false;
            for (int i = 0; i < body.goSlot.Count; i++)
            {
                TMorph morph = body.goSlot[i].morph;
                if (morph != null)
                {
                    if (morph.Contains(sTag))
                    {
                        if (i == 1)
                        {
                            bFace = true;
                        }
                        int h = (int)body.goSlot[i].morph.hash[sTag];
                        body.goSlot[i].morph.BlendValues[h] = f;
                        //body.goSlot[i].morph.FixBlendValues();
                    }
                }
            }
            return bFace;
        }


        private void VertexMorph_FromProcItem_Fix(TBody body)
        {
            for (int i = 0; i < body.goSlot.Count; i++)
            {
                TMorph morph = body.goSlot[i].morph;
                if (morph != null)
                {
                    body.goSlot[i].morph.FixBlendValues();
                }
            }
        }*/


        static List<TMorph> m_NeedFixTMorphs = new List<TMorph>();

        //シェイプキー操作
        //戻り値はsTagの存在有無にしているので必要に応じて変更してください
        static public bool VertexMorph_FromProcItem(TBody body, string sTag, float f)
        {
            bool bRes = false;

            if (!body || sTag == null || sTag == "")
                return false;

            for (int i = 0; i < body.goSlot.Count; i++)
            {
                TMorph morph = body.goSlot[i].morph;
                if (morph != null)
                {
                    if (morph.Contains(sTag))
                    {
                        /*if (i == 1)
                        {
                            bFace = true;
                        }*/
                        bRes = true;
                        int h = (int)morph.hash[sTag];
                        //morph.BlendValues[h] = f;
                        morph.SetBlendValues(h, f);
                        //後でまとめて更新する
                        //body.goSlot[i].morph.FixBlendValues();

                        //更新リストに追加
                        if (!m_NeedFixTMorphs.Contains(morph))
                            m_NeedFixTMorphs.Add(morph);
                    }
                }
            }
            return bRes;
        }

        //シェイプキー操作Fix(基本はUpdate等の最後に一度呼ぶだけで良いはず）
        static public void VertexMorph_FixBlendValues()
        {
            foreach (TMorph tm in m_NeedFixTMorphs)
            {
                tm.FixBlendValues();
            }

            m_NeedFixTMorphs.Clear();
        }



        private int intCnv(string s)
        {
            if (Regex.IsMatch(s, "[^0-9]")) s = "0";
            int i = int.Parse(s);

            return i;

        }


        //　デバッグ用コンソール出力メソッド
//        [Conditional("DEBUG")]
        private void debugPrintConsole(string s)
        {
            Console.WriteLine(s);
        }


        //@API実装関係 追加->// 
        /*テスト用
        public object getCfgObj()
        {
            return cfg;
        }*/
        //////////////////////////////////////////////////////////////////////////////
        //////  パラメータ読み出し用のAPI関数(@API実装)
        //////　※動的にリンクするプラグインによってはこちらが直接呼ばれるかもしれません
        //////　※使用方法などはAPIクラスのGetVYM_Valueのコメントを参照ください
        public object getVYM_Value(API.VYM_IO_ID id)
        {
            switch (id)
            {
                case API.VYM_IO_ID.b_PluginEnabledV:
                    //debugPrintConsole("getVYM_Value b_PluginEnabledV :" + cfg.bPluginEnabledV);
                    return cfg.bPluginEnabledV;

                case API.VYM_IO_ID.i_GetCurrentMaidNo:  //実際のオブジェクトのメイドNo
                    if (cfg.bPluginEnabledV)
                    {
                        if (maidDataList.Count > iCurrentMaid)
                        {
                            return maidDataList[iCurrentMaid];
                        }
                    }
                    return -1;

                case API.VYM_IO_ID.i_CurrentMaid:  //'17.04.07追加//内部配列のメイドNo
                    return iCurrentMaid;

                case API.VYM_IO_ID.i_VLevel:
                    return VLevel;
                case API.VYM_IO_ID.i_vState:
                    return vState;
                case API.VYM_IO_ID.i_vStateMajor:
                    return vStateMajor;

                case API.VYM_IO_ID.i_vExciteLevel:    //　０～３００の興奮度を、１～４の興奮レベルに変換した値
                    return vExciteLevel;
                case API.VYM_IO_ID.d_iCurrentExcite:
                    return iCurrentExcite;
                case API.VYM_IO_ID.d_vResistGet:   //　現在抵抗値
                    return vResistGet;
                case API.VYM_IO_ID.d_vResistBase:   //　抵抗値のベース値
                    return vResistBase;
                case API.VYM_IO_ID.d_vResistBonus:  //　抵抗の特別加算値
                    return vResistBonus;
                case API.VYM_IO_ID.d_vBoostGet:   //　現在感度
                    return vBoostGet;
                case API.VYM_IO_ID.d_vBoostBase:   //　感度のベース値
                    return vBoostBase;
                case API.VYM_IO_ID.d_vBoostBonus:   //　感度の特別加算値
                    return vBoostBonus;
                case API.VYM_IO_ID.d_vMaidStamina:   //　スタミナ
                    return vMaidStamina;
                case API.VYM_IO_ID.b_vMaidStun:
                    return vMaidStun;

                case API.VYM_IO_ID.d_vOrgasmValue:   //　現在絶頂値　100になると絶頂
                    return vOrgasmValue;
                case API.VYM_IO_ID.i_vOrgasmCount:   //　絶頂回数
                    return vOrgasmCount;
                case API.VYM_IO_ID.i_vOrgasmCmb:   //　連続絶頂回数
                    return vOrgasmCmb;

                case API.VYM_IO_ID.i_OrgasmVoice:   //　絶頂音声再生フラグ(1再生開始、2再生中)
                    return OrgasmVoice;
                case API.VYM_IO_ID.f_vOrgasmHoldTime:  //絶頂後ボーナスタイム（残り時間。MAX600）
                    return vOrgasmHoldTime;

                case API.VYM_IO_ID.b_BreastFlag:  //噴乳（胸）開始フラグ
                    return BreastFlag;
                case API.VYM_IO_ID.b_EnemaFlag:  //噴乳（尻）開始フラグ
                    return EnemaFlag;
                case API.VYM_IO_ID.b_ChinpoFlag:  //射精開始フラグ
                    return ChinpoFlag;
                case API.VYM_IO_ID.b_SioFlag:   //潮開始フラグ
                    return SioFlag;

                case API.VYM_IO_ID.d_AheValue:  //　アヘ値
                    return AheValue;
                case API.VYM_IO_ID.d_AheValue2:
                    return AheValue2;

                case API.VYM_IO_ID.d_vJirashi:  //　焦らし度
                    return vJirashi;
                case API.VYM_IO_ID.b_ExciteLock:    //　興奮度ロック
                    return ExciteLock;
                case API.VYM_IO_ID.b_OrgasmLock:    //　絶頂度ロック
                    return OrgasmLock;

                case API.VYM_IO_ID.b_RankoEnabled:  //　乱交モード
                    return RankoEnabled;

                case API.VYM_IO_ID.obj_GetSaveSlot: //セーブスロットデータ取得
                    {//元データへの参照を切るため新しいObjを作ってリストを複製
                        API.SaveSlot oSL = new API.SaveSlot();
                        oSL.SaveFlag = SaveFlag;
                        oSL.vBoostBaseSave = new List<double>(vBoostBaseSave);
                        oSL.vOrgasmCountSave = new List<int>(vOrgasmCountSave);
                        oSL.MaidNameSave = new List<string>(MaidNameSave);

                        oSL.VLevelSave = new List<int>(VLevelSave);
                        oSL.FaceBackupSave = new List<string>(FaceBackupSave);
                        oSL.MotionBackupSave = new List<string>(MotionBackupSave);

                        return oSL;
                    }

                case API.VYM_IO_ID.i_GuiFlag: //　GUIの表示フラグ（0：非表示、1：表示、2：最小化）
                    return cfg.GuiFlag;
                case API.VYM_IO_ID.b_GuiFlag2:    //　設定画面の表示フラグ
                    return cfg.GuiFlag2;
                case API.VYM_IO_ID.b_GuiFlag3:    //　命令画面の表示フラグ
                    return cfg.GuiFlag3;
                case API.VYM_IO_ID.obj_VibeYourMaidConfig:      //設定ファイル項目(荒業/参照渡し注意)
                    return cfg;
                case API.VYM_IO_ID.obj_VibeYourMaidCfgWriting:  //GUI設定項目(荒業/参照渡し注意)
                    return cfgw;

                case API.VYM_IO_ID.b_StartFlag:  //スタート状態@1.0.1.2追加
                    return StartFlag;

                //API連動用
                case API.VYM_IO_ID.s_API_mMaidLastZcVoiceFN:    //夜伽側の絶頂音声取得用
                    return API.mMaidLastZcVoiceFN;

                default:
                    break;

            }
            return null;
        }
        //////////////////////////////////////////////////////////////////////////////
        //////  パラメータ書き込み用のAPI関数(@API実装)
        //////　※動的にリンクするプラグインによってはこちらが直接呼ばれるかもしれません
        //////　※使用方法などはAPIクラスのSetVYM_Valueのコメントを参照ください
        public int setVYM_Value(API.VYM_IO_ID id, object objVar)
        {
            // シーンの有効状態チェック
            bool bCheckEnabled = cfg.bPluginEnabledV && maid && maidActive && SceneLevelEnable;
            try
            {
                switch (id)
                {
                    case API.VYM_IO_ID.b_PluginEnabledV:
                        if (objVar is bool)
                        {
                            cfg.bPluginEnabledV = (bool)objVar;
                            return 0;
                        }
                        break;

                    case API.VYM_IO_ID.i_CurrentMaid:  //'17.04.07追加//内部配列のメイドNo
                        if (objVar is int)
                        {
                            if (bCheckEnabled)
                            {
                                if (maidDataList.Count > (int)objVar)
                                {
                                    iCurrentMaid = (int)objVar;
                                    return 0;
                                }
                                return 2;
                            }
                            else { return 1; }
                        }
                        return -1;

                    case API.VYM_IO_ID.i_VLevel:
                        if (objVar is int)
                        {
                            if (bCheckEnabled)
                            {
                                VLevel = (int)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;
                    case API.VYM_IO_ID.i_vState:
                        return -1; //不可
                    case API.VYM_IO_ID.i_vStateMajor:
                        //return -1; //不可 → SEを再生したくないときに変更必須
                        if (objVar is int)
                        {
                            debugPrintConsole("case API.VYM_IO_ID.i_vStateMajor:" + (int)objVar);
                            if (bCheckEnabled)
                            {
                                vStateMajor = (int)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;
                    case API.VYM_IO_ID.i_vExciteLevel:    //　０～３００の興奮度を、１～４の興奮レベルに変換した値
                        return -1; //不可
                    case API.VYM_IO_ID.d_iCurrentExcite:
                        if (objVar is double)
                        {
                            if (bCheckEnabled)
                            {
                                iCurrentExcite = (double)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;

                    case API.VYM_IO_ID.d_vResistGet:   //　現在抵抗値
                        return -1; //不可
                    case API.VYM_IO_ID.d_vResistBase:   //　抵抗値のベース値
                        if (objVar is double)
                        {
                            if (bCheckEnabled)
                            {
                                vResistBase = (double)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;
                    case API.VYM_IO_ID.d_vResistBonus:  //　抵抗の特別加算値
                        if (objVar is double)
                        {
                            if (bCheckEnabled)
                            {
                                vResistBonus = (double)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;

                    case API.VYM_IO_ID.d_vBoostGet:   //　現在感度
                        return -1; //不可
                    case API.VYM_IO_ID.d_vBoostBase:   //　感度のベース値
                        if (objVar is double)
                        {
                            if (bCheckEnabled)
                            {
                                vBoostBase = (double)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;
                    case API.VYM_IO_ID.d_vBoostBonus:   //　感度の特別加算値
                        if (objVar is double)
                        {
                            if (bCheckEnabled)
                            {
                                vBoostBonus = (double)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;

                    case API.VYM_IO_ID.d_vMaidStamina:   //　スタミナ
                        if (objVar is double)
                        {
                            if (bCheckEnabled)
                            {
                                vMaidStamina = (double)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;
                    case API.VYM_IO_ID.b_vMaidStun:
                        if (objVar is bool)
                        {
                            if (bCheckEnabled)
                            {
                                vMaidStun = (bool)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;

                    case API.VYM_IO_ID.d_vOrgasmValue:   //　現在絶頂値　100になると絶頂
                        if (objVar is double)
                        {
                            if (bCheckEnabled)
                            {
                                vOrgasmValue = (double)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;
                    case API.VYM_IO_ID.i_OrgasmVoice:   //　絶頂音声再生フラグ(1再生開始、2再生中)
                        if (objVar is int)
                        {
                            if (bCheckEnabled)
                            {
                                OrgasmVoice = (int)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;

                    case API.VYM_IO_ID.i_vOrgasmCount:   //　絶頂回数
                        if (objVar is int)
                        {
                            if (bCheckEnabled)
                            {
                                vOrgasmCount = (int)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;
                    case API.VYM_IO_ID.i_vOrgasmCmb:   //　連続絶頂回数
                        if (objVar is int)
                        {
                            if (bCheckEnabled)
                            {
                                vOrgasmCmb = (int)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;
                    case API.VYM_IO_ID.b_BreastFlag:  //噴乳（胸）開始フラグ
                        if (objVar is bool)
                        {
                            if (bCheckEnabled)
                            {
                                BreastFlag = (bool)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;
                    case API.VYM_IO_ID.b_EnemaFlag:  //噴乳（尻）開始フラグ
                        if (objVar is bool)
                        {
                            if (bCheckEnabled)
                            {
                                EnemaFlag = (bool)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;
                    case API.VYM_IO_ID.b_ChinpoFlag:  //射精開始フラグ
                        if (objVar is bool)
                        {
                            if (bCheckEnabled)
                            {
                                ChinpoFlag = (bool)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;
                    case API.VYM_IO_ID.d_AheValue:  //　アヘ値
                        if (objVar is double)
                        {
                            if (bCheckEnabled)
                            {
                                AheValue = (double)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;
                    case API.VYM_IO_ID.d_AheValue2:
                        if (objVar is double)
                        {
                            if (bCheckEnabled)
                            {
                                AheValue2 = (double)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;

                    case API.VYM_IO_ID.d_vJirashi:  //　焦らし度
                        if (objVar is double)
                        {
                            if (bCheckEnabled)
                            {
                                vJirashi = (double)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;
                    case API.VYM_IO_ID.b_ExciteLock:    //　興奮度ロック
                        if (objVar is bool)
                        {
                            if (bCheckEnabled)
                            {
                                ExciteLock = (bool)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;
                    case API.VYM_IO_ID.b_OrgasmLock:    //　絶頂度ロック
                        if (objVar is bool)
                        {
                            if (bCheckEnabled)
                            {
                                OrgasmLock = (bool)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;

                    case API.VYM_IO_ID.b_StartFlag:  //スタート状態@1.0.1.2追加
                        if (objVar is bool)
                        {
                            if (bCheckEnabled)
                            {
                                //有効シーンのみ
                                StartFlag = (bool)objVar;
                                return 0;
                            }
                            else { return 1; }
                        }
                        break;

                    //API連動用
                    case API.VYM_IO_ID.s_API_mMaidLastZcVoiceFN:    //夜伽側の絶頂音声取得用@1.0.1.2追加
                        if (objVar != null && objVar is string)
                        {
                            API.mMaidLastZcVoiceFN = (string)objVar;
                            return 0;
                        }
                        break;
                    default:
                        break;

                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("setVYM_Value Error:" + e);
            }
            return -1;
        }

        /// <summary>
        /// APIエントリ（主に動的リンク向け）
        /// </summary>
        /// <param name="mode">0ならコマンドモード、1がgetVYM_Value、2がsetVYM_Value</param>
        /// <param name="param1">mode0の時はコマンド文字列、1～2の時はAPI.VYM_IO_IDの値をintで渡す</param>
        /// <param name="param2">mode2の時のみ使用、setVYM_Value参照。その他の場合はnullを指定</param>
        /// <returns>結果の値、nullか-1～-2だと失敗。getVYM_Value、setVYM_Value参照</returns>
        public readonly string[] API_Command_Names = { "バイブ弱", "バイブ強", "バイブ停止", "GUI_ON", "GUI_OFF", "GUI_トグル", "有効無効_トグル", "SE切替_バイブ音", "SE切替_抽挿音" };
        public object API_Entry(int mode, object param1, object param2)
        {
            if (mode == 0)
            {
                if (param1 is string)
                {
                    string comand = param1 as string;
                    if (comand == "GET_CMD")
                    {
                        return API_Command_Names;
                    }
                    if (comand == "バイブ弱")
                    {
                        return setVYM_Value(API.VYM_IO_ID.i_VLevel, 1);
                    }
                    if (comand == "バイブ強")
                    {
                        return setVYM_Value(API.VYM_IO_ID.i_VLevel, 2);
                    }
                    if (comand == "バイブ停止")
                    {
                        return setVYM_Value(API.VYM_IO_ID.i_VLevel, 0);
                    }
                    if (comand == "GUI_ON")
                    {
                        return cfg.GuiFlag = 1;
                    }
                    if (comand == "GUI_OFF")
                    {
                        return cfg.GuiFlag = 0;
                    }
                    if (comand == "GUI_トグル")
                    {
                        return cfg.GuiFlag = cfg.GuiFlag == 0 ? 1 : 0;
                    }
                    if (comand == "有効無効_トグル")
                    {
                        return cfg.bPluginEnabledV = !cfg.bPluginEnabledV;
                    }
                    if (comand == "SE切替_バイブ音")
                    {
                        int prev = cfgw.SelectSE;
                        cfgw.SelectSE = 0;

                        return cfgw.SelectSE;
                    }
                    if (comand == "SE切替_抽挿音")
                    {
                        int prev = cfgw.SelectSE;
                        cfgw.SelectSE = 1;

                        return cfgw.SelectSE;
                    }
                    return null;
                }
            }
            else if (mode == 1)
            {
                API.VYM_IO_ID id = 0;
                if (param1 is int)
                {
                    id = (API.VYM_IO_ID)((int)param1);
                    return getVYM_Value(id);
                }
            }
            else if (mode == 2)
            {
                API.VYM_IO_ID id = 0;
                if (param1 is int)
                {
                    id = (API.VYM_IO_ID)((int)param1);
                    return setVYM_Value(id, param2);
                }
            }
            return null;
        }
        //-<@API実装//追加ここまで

    }

    //@API実装->// API実装用クラス
    public static class API
    {
        //夜伽連動用
        public static string mMaidLastZcVoiceFN = "";

        private static VibeYourMaid objVYM = null;

        private static bool checkVYM()
        {
            try
            {
                if (objVYM) return true;
                else
                {
                    GameObject go = UnityEngine.GameObject.Find("UnityInjector");
                    objVYM = go.GetComponent<VibeYourMaid>();
                    if (objVYM) return true;
                }
            }
            catch
            {
            }
            return false;
        }

        //////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// getVYM_ValueやsetVYM_Value関数でターゲットとするパラメータの指定用
        /// </summary>
        public enum VYM_IO_ID
        { //WindowsMessage的ID値割り当て。一意の数値さえ変えなければ行間に追加可
            b_PluginEnabledV = 100 + 10,     //有効状態
            i_GetCurrentMaidNo = 100 + 20,     //現在操作中のMaidNo//返り値：-1 未定義/null エラー
            i_CurrentMaid = 100 + 30,  //内部配列のメイドNo   //'17.04.07追加

            i_VLevel = 200 + 10,    //　バイブ状態 1=弱 2=強 0=停止
            i_vState = 200 + 20,    //　現状vStateMajorとほぼ同じ
            i_vStateMajor = 200 + 30,    //　強弱によるメイドステート//10 …停止(余韻もなし)//20 …弱//30 …強

            i_vExciteLevel = 300 + 10,    //　０～３００の興奮度を、１～４の興奮レベルに変換した値
            d_iCurrentExcite = 300 + 20,  //　現在興奮値
            d_vResistGet = 400 + 10,   //　現在抵抗値
            d_vResistBase = 400 + 20,   //　抵抗値のベース値
            d_vResistBonus = 400 + 30,   //　抵抗の特別加算値
            d_vBoostGet = 500 + 10,   //　現在感度
            d_vBoostBase = 500 + 20,   //　感度のベース値
            d_vBoostBonus = 500 + 30,   //　感度の特別加算値(今は使われていないみたい)
            d_vMaidStamina = 600 + 10,   //　スタミナ値
            b_vMaidStun = 600 + 20,   //  スタン状態（trueでたたき起こす＝ON）

            d_vOrgasmValue = 700 + 10,   //　現在絶頂値　100になると絶頂
            i_vOrgasmCount = 700 + 20,   //　絶頂回数
            i_vOrgasmCmb = 700 + 30,   //　連続絶頂回数
            i_OrgasmVoice = 700 + 40,   //　絶頂音声再生フラグ(1再生開始、2再生中)
            f_vOrgasmHoldTime = 700 + 50,   //絶頂後ボーナスタイム（残り時間。MAX600）

            b_BreastFlag = 1000 + 10,  //噴乳（胸）開始フラグ
            b_EnemaFlag = 1000 + 20,  //噴乳（尻）開始フラグ
            b_ChinpoFlag = 1000 + 30,  //射精開始フラグ
            b_SioFlag = 1000 + 40,  //潮開始フラグ

            d_AheValue = 1100 + 10,  //　アヘ値＝アヘ有効での瞳の上昇値
            d_AheValue2 = 1100 + 20,

            d_vJirashi = 1300 + 10,  //　焦らし度
            b_ExciteLock = 1500 + 10,  //　興奮度ロック
            b_OrgasmLock = 1500 + 20,  //　絶頂度ロック

            b_RankoEnabled = 1600 + 10,  //　乱交モード



            obj_GetSaveSlot = 90000 + 1,             //セーブスロット項目、一日でリセット(値渡し)

            i_GuiFlag = 98000 + 1,        //　GUIの表示フラグ（0：非表示、1：表示、2：最小化）
            b_GuiFlag2 = 98000 + 2,         //　設定画面の表示フラグ
            b_GuiFlag3 = 98000 + 3,         //　命令画面の表示フラグ

            b_StartFlag = 98100 + 1,         //#1.0.1.2で追加#シーン開始後の有効状態フラグ(通常操作があるまでFalse)

            obj_VibeYourMaidConfig = 99800 + 1,      //設定ファイル項目(荒業/非推奨/参照渡し)
            obj_VibeYourMaidCfgWriting = 99900 + 2,  //GUI設定項目(荒業/非推奨/参照渡し)

            // API設定用
            s_API_mMaidLastZcVoiceFN = 500000 + 10,   //夜伽リンク用に追加、夜伽側の再生したカレントメイドの絶頂音声ファイル名
        };

        public class SaveSlot
        {
            public bool SaveFlag = false;
            public List<double> vBoostBaseSave = new List<double>();
            public List<int> vOrgasmCountSave = new List<int>();
            public List<string> MaidNameSave = new List<string>();

            public List<int> VLevelSave = new List<int>();
            public List<string> FaceBackupSave = new List<string>();
            public List<string> MotionBackupSave = new List<string>();
        }

        //////////////////////////////////////////////////////////////////////////////
        //
        //プラグインの有効状態を取得　0：無効、1：有効、-1：取得失敗
        //
        /// <summary>
        /// （ViveYourMaid.API関数）プラグインの有効状態を取得
        /// </summary>
        /// <returns></returns>
        public static int GetPluginEnabled()
        {
            try
            {

                object o = GetVYM_Value(VYM_IO_ID.b_PluginEnabledV);
                if (o is bool)
                    return (bool)o ? 1 : 0;
            }
            catch (Exception e) { UnityEngine.Debug.Log("GetPluginEnabled Error:" + e); }
            return -1;
            /*
            VibeYourMaid.VibeYourMaidConfig tmpcfg = null;
            try
            {
                if ( checkVYM() )
                    tmpcfg = (VibeYourMaid.VibeYourMaidConfig)objVYM.getCfgObj();
            } catch
            {
                return -1;
            }

            if ( tmpcfg != null && tmpcfg.bPluginEnabledV) return 1;
            return 0;*/
        }

        //////////////////////////////////////////////////////////////////////////////
        //
        //　API.VYM_IO_IDで指定した数値の読み出し
        //　成功ならオブジェクト型で返るので、VYM_IO_IDアイテムの最初の文字を参考にキャストして使用
        //　失敗：null
        //
        /// <summary>
        /// （ViveYourMaid.API関数）プラグインからのパラメータ読み出し用
        ///  成功ならオブジェクト型で返るので、VYM_IO_IDアイテムの最初の文字を参考にキャストして使用
        ///  [頭文字：i=int、d=double、f=float、b=bool、obj=特殊・固有のオブジェクト]
        /// <param name="id">パラメータID(ターゲット指定)</param>
        /// <returns>成功：指定の値(キャストして使用)、失敗：nullまたは数値型なら-1以下</returns>
        /// </summary>
        public static object GetVYM_Value(API.VYM_IO_ID id)
        {
            try
            {
                if (checkVYM())
                    return objVYM.getVYM_Value(id);
            }
            catch (Exception e) { UnityEngine.Debug.Log("GetVYM_Value Error:" + e); }

            return null;
        }

        //////////////////////////////////////////////////////////////////////////////
        //
        //　API.VYM_IO_IDで指定した数値の書き込み（処理的に問題なさそうな物だけ実装）
        //
        //  objVarはVYM_IO_IDアイテムの最初の文字を参考に指定（型が違うと失敗します）
        //　成功なら0、メイドの状態などで設定できなかった場合は1
        //　失敗：-1以下（-1は本体クラス側…書き込み拒否含む、-2はAPIクラス側でエラー）
        //
        /// <summary>
        /// （ViveYourMaid.API関数）プラグインへのパラメータ書き込み用
        ///  ※objVarはVYM_IO_IDアイテムの最初の文字を参考に指定（型が違うと失敗します）
        ///  [頭文字：i=int、d=double、f=float、b=bool、obj=特殊・固有のオブジェクト]
        /// </summary>
        /// <param name="id">パラメータID(ターゲット指定)</param>
        /// <param name="objVar">書き込みたい値</param>
        /// <returns>成功：0、メイドやシーン状態などで設定不可:1、エラーや書込不可：-1以下</returns>
        public static int SetVYM_Value(API.VYM_IO_ID id, object objVar)
        {
            try
            {
                if (checkVYM())
                    return objVYM.setVYM_Value(id, objVar);
            }
            catch (Exception e) { UnityEngine.Debug.Log("SetVYM_Value Error:" + e); }

            return -2;
        }
    }
    //<-@API実装//APIクラス追加ここまで
}




//　以下、cm3d2.reflectiontest.plugin.cs より拝借（ありがとうございました）

namespace Util
{
    public static class GetObject
    {
        /// <summary>
        /// static変数を取得する
        /// </summary>
        /// <example>
        /// <code>
        /// var cm = GetObject.ByString("GameMain.m_objInstance.m_CharacterMgr") as CharacterMgr;
        /// Console.WriteLine("cm = {0}", cm);
        /// </code>
        /// </example>
        public static object ByString(string longFieldName)
        {
            string[] ss = longFieldName.Split('.');
            return ByString(TypeByString(ss[0]), string.Join(".", ss, 1, ss.Length - 1));
        }

        /// <summary>
        /// 型情報とフィールド名を元にstatic変数を取得する
        /// </summary>
        /// <example>
        /// 「var cm = GameMain.Instance.CharacterMgr」と同等のコード例
        /// <code>
        /// var cm = GetObject.ByString(typeof(GameMain), "m_objInstance.m_CharacterMgr") as CharacterMgr;
        /// Console.WriteLine("cm = {0}", cm);
        /// </code>
        /// </example>
        public static object ByString(Type type, string longFieldName)
        {
            return ByString(type, null, longFieldName);
        }

        /// <summary>
        /// インスタンス変数を取得する
        /// </summary>
        /// <example>
        /// 「var first_name = maid.Param.status.first_name」と同等のコード例
        /// <code>
        /// Maid maid = GameMain.Instance.CharacterMgr.GetStockMaid(0);
        /// var first_name = GetObject.ByString(maid, "m_Param.status_.first_name") as string;
        /// Console.WriteLine("first_name = {0}", first_name);
        /// </code>
        /// </example>
        public static object ByString(object instance, string longFieldName)
        {
            return ByString(null, instance, longFieldName);
        }

        //
        public static object ByString(Type type, object instance, string longFieldName)
        {
            string[] fieldNames = longFieldName.Split('.');
            foreach (string fieldName in fieldNames)
            {
                if (instance != null)
                {
                    type = instance.GetType();
                }
                if (type == null)
                {
                    return null;
                }

                FieldInfo fi = type.GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.Static |
                    BindingFlags.Public | BindingFlags.NonPublic);
                if (fi == null)
                {
                    return null;
                }
                instance = fi.GetValue(instance);
            }
            return instance;
        }

        public static Type TypeByString(string typeName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }
    }
}
