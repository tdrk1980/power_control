using System;
using System.IO.Ports;
using System.Reflection;
using Vector.Tools;
using Vector.CANoe.Runtime;
using Vector.CANoe.Sockets;
using Vector.CANoe.Threading;
using Vector.Diagnostics;

public class texio : MeasurementScript
{
    // System.IO.Ports.SerialPort (COMポート制御クラス)
    // https://docs.microsoft.com/ja-jp/dotnet/api/system.io.ports.serialport?view=netframework-4.7.2
    static SerialPort serialPort;

    public override void Initialize()
    {
        serialPort = new SerialPort("COM26", 9600, System.IO.Ports.Parity.Even, 7, System.IO.Ports.StopBits.One);

        serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

        // COMポートをオープン
        serialPort.Open();
    }
    
    public override void Shutdown()
    {
        serialPort.Close();
    }

    public override void Start()
    {

    }

    public override void Stop()
    {

    }

/////
// システム変数(Texio.RequestVoltage)の変化検出ハンドラ
// Texio.RequestVoltage : 外部からの電圧設定要求
[OnChange(typeof(Texio.RequestVoltage))]
    public void TexioRequestVoltageChanged()
    {
       serialPort.WriteLine("VOLT "+ Texio.RequestVoltage.Value.ToString("F2"));
       serialPort.WriteLine("VOLT?");
    }

/////
// シリアル受信ハンドラ
// 注意: DataReceivedHandler内ではCANoeの.Net APIが制限される。[1]
    private static void DataReceivedHandler(
                        object sender,
                        SerialDataReceivedEventArgs e)
    {
        string response = ((SerialPort)sender).ReadExisting();

        response = response.Trim(); // 末尾に付いているLF('\r', 0x0A)を落とす。e.g."VOLT 13.00\r"==>"VOLT 13.00"

        // response解析
        if(response.Contains("VOLT ")) // 電圧問い合わせ"VOLT?"に対する応答
        {
            string[] response_arr = response.Split(' '); //' 'を区切りとして配列にする。 e.g."VOLT 13.00"==>["VOLT","13.00"]
            if(response_arr.Length == 2)
            {
                Texio.ActualVoltage.Value = Convert.ToDouble(response_arr[1]); // システム変数Texio.ActualVoltage.Valueへの書き込み
            }
            else // 期待しない形式は
            {
                Vector.Tools.Output.WriteLine("Data Received(Length error, ignored):"+ response + ", response_arr.Length=" + response_arr.Length.ToString());
            }
        }
        else // 未実装の応答は無視する
        {
            Vector.Tools.Output.WriteLine("Data Received(Unexpected, ignored):" + response);
        }
    }
}



// [1]
// 環境変数(Environment variable)を利用しようとしたが、
// CANoe 10.0のHelp(CANoeCANalyzer.chm)の

// Start Page
// → Programming @Further Topics
//   → .NET @CANoe Programming
//     → .NET Programming: Notes for Advanced Users @Further Information
//       → Concurrency

// より、

// - スクリプト側で作成したスレッドからは環境変数(DBCのEnvironment variable)に書き込み・読み込みできないし、CANシグナル送信などもできない。
// - CANoe APIは以下の3つに関連するもののみが利用できる。
//     1. Writing system variables (not reading!) ★
//     2. Writing to log file
//     3. Writing messages to output window 
// とのことなので、「システム変数の書き込み」にしている。

// (抜粋)
// ## Concurrency
// Although there can be several test modules running in parallel, these modules are not executed in separate threads. 
// This means you don't have to be concerned with synchronization mechanisms.
// While a test module is waiting for an event, other test modules or CAPL code are carried out. This is a form of cooperative multitasking; 
// for this reason you should make sure not to program long running operations in a test module without using intermediate Wait calls.
// Because of the CANoe internal concurrency architecture, you are not permitted to use synchronization primitives (such as locks) or
// start additional threads in a .NET program.

// Because of the CANoe internal concurrency architecture, you are not permitted to use synchronization primitives (such as locks) or
// start additional threads in a .NET program.
// If you need to execute potentially long lasting operations you can use the CANoe WaitForTask function.
// It is also possible to use of the System.Thread class.
// In both cases only a very limited number of API functions are available in background threads:

//   - Writing system variables (not reading!)
//   - Writing to log file 
//   - Writing messages to output window 

// All other API function will throw an exception.
