using System;
using System.Reflection; // add
using System.Linq; // add
using System.Collections.Generic; // add
using Vector.Tools;
using Vector.CANoe.Runtime;
using Vector.CANoe.Sockets;
using Vector.CANoe.Threading;
using Vector.Diagnostics;


public class voltage_control_scenario : MeasurementScript
{
    static Vector.Tools.Timer 2sec_off_tm;

    public override void Initialize()
    {
        2sec_off_tm = new Timer(new TimeSpan(20*1000*1000), On2secOffTimerElapsed); // 2秒後にoffする
        // public TimeSpan (long ticks); tickの単位は 100 nano-sec [ref4]
    }
    
    public override void Start()
    {

    }
    
    public override void Stop()
    {

    }
    
    public override void Shutdown()
    {
        
    }

    // CANframeの内容を出力する (デバッグ用)
    // e.g. 16.1702 1 123 Rx d 8 01 02 03 04 05 06 07 08
    private void printCANFrame(CANFrame frame)
    {
        Vector.Tools.Output.WriteLine(
            ((double)frame.TimeNS / 1000000000.0).ToString("F4") + " "
            + frame.Channel.ToString() + " "
            + frame.ID.ToString("X3") + " " // [ref3]
            + "Rx d " + frame.DLC.ToString() + " " 
            + string.Join(" ", frame.Bytes.Select(b => b.ToString("X2")).ToArray()) //[ref1],[ref2]
        );
    }

/////
// ユーザ定義イベントハンドラ
/////
    /////
    // 2secタイマ経過ハンドラ
    private static void On2secOffTimerElapsed(object sender, ElapsedEventArgs e)
    {
        Texio.RequestVoltage.Value = 0.0;
        2sec_off_tm.Stop();
    }

    /////
    // CAN ID:0x123受信ハンドラ
    [OnCANFrame(1, 0x123)] // [OnCANFrame(byte channnel, int32 id)] @ OnCANFrameAttribute Constructor (Byte, Int32) 
    public void CANFrameReceived(CANFrame frame)
    {
        // printCANFrame(frame);
        if(frame.Bytes[0] == 0x0)
        {
            2sec_off_tm.Stop();
            2sec_off_tm.Start();
        }
        else
        {
            2sec_off_tm.Stop();
            Texio.RequestVoltage.Value = 13.0;
        }

    }
}


// [ref1]:C#_配列要素を List に入れ替える - …Inertia
// http://koshinran.hateblo.jp/entry/2018/01/30/200236

// [ref2]:c# - string.Join on a List_int_ or other type - Stack Overflow
// https://stackoverflow.com/questions/3610431/string-join-on-a-listint-or-other-type

// [ref3]:書式を指定して数値を文字列に変換する - .NET Tips (VB.NET,C#...)
// https://dobon.net/vb/dotnet/string/inttostring.html

// [ref4]:TimeSpan Constructor (System) _ Microsoft Docs
// https://docs.microsoft.com/en-us/dotnet/api/system.timespan.-ctor?view=netframework-4.7.2#System_TimeSpan__ctor_System_Int64_
// public TimeSpan (long ticks);
//  ticksは、100-nanosecond units単位なので、10*1000*1000 = 1sec