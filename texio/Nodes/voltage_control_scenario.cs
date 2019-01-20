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

    public override void Initialize()
    {
    
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

// CAN ID:0x123受信ハンドラ
[OnCANFrame(1, 0x123)] // [OnCANFrame(byte channnel, int32 id)] @ OnCANFrameAttribute Constructor (Byte, Int32) 
    public void CANFrameReceived(CANFrame frame)
    {

        printCANFrame(frame);

    }
}


// [ref1]:C#_配列要素を List に入れ替える - …Inertia
// http://koshinran.hateblo.jp/entry/2018/01/30/200236

// [ref2]:c# - string.Join on a List_int_ or other type - Stack Overflow
// https://stackoverflow.com/questions/3610431/string-join-on-a-listint-or-other-type

// [ref3]:書式を指定して数値を文字列に変換する - .NET Tips (VB.NET,C#...)
// https://dobon.net/vb/dotnet/string/inttostring.html
