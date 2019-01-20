using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
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

    private void printCANFrame(CANFrame frame)
    {
        Vector.Tools.Output.WriteLine(string.Join(" ", frame.Bytes.Select(b => b.ToString("X2")).ToArray()));
    }

[OnCANFrame(1, 0x123)] // [OnCANFrame(byte channnel, int32 id)] @ OnCANFrameAttribute Constructor (Byte, Int32) 
    public void CANFrameReceived(CANFrame frame)
    {

        printCANFrame(frame);

        // Vector.Tools.Output.WriteLine(MethodBase.GetCurrentMethod().Name);
        // if(frame.Bytes[0]==0x1)
        // {
        //     Vector.Tools.Output.WriteLine("0x123 recevied byte[0]:"+frame.Bytes[0].ToString("X2"));
        // }

       
    }
}

// 書式を指定して数値を文字列に変換する - .NET Tips (VB.NET,C#...)
// https://dobon.net/vb/dotnet/string/inttostring.html
