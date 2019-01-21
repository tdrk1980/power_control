// 自動で追加された
using System;
using System.Reflection;
using Vector.Tools;
using Vector.CANoe.Runtime;
using Vector.CANoe.Sockets;
// using Vector.CANoe.Threading;
// using Vector.Diagnostics;


using System.IO.Ports; 

public class texio_control : MeasurementScript
{
// System.IO.Ports.SerialPort (COMポート制御クラス)
// https://docs.microsoft.com/ja-jp/dotnet/api/system.io.ports.serialport?view=netframework-4.7.2
    static SerialPort serialPort;

// システム変数(TexioRequestVoltage)の変化フラグ
    static bool isTexioRequestVoltageChanged = false;

/////
// スクリプト作成時に自動生成されるイベントハンドラ
/////
    /////
    // シミュレーション開始直前に呼ばれる初期化ハンドラ
    // 注: 必要なインスタンスはここで全て生成しておく。
    public override void Initialize()
    {
        // ポート名、ビットレートなど各種設定をする。
        serialPort = new SerialPort("COM2", 9600, System.IO.Ports.Parity.Even, 7, System.IO.Ports.StopBits.One);

        // COMポート受信ハンドラの設定
        serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        serialPort.ReadTimeout = 100;
        serialPort.WriteTimeout = 100;
        // serialPort.NewLine = "\r";

        // COMポートのオープン
        serialPort.Open();

        // 外部からの電圧設定要求の初期値を設定しておく
        Texio.RequestVoltage.Value = 13.0;
    }

    /////
    // シミュレーション終了直後に呼ばれるハンドラ
    public override void Shutdown()
    {
        serialPort.Close();
    }

    // "VOLT XX.XX"を送信するメソッド
    private void SendSetVoltage(){
        string request = "VOLT "+ Texio.RequestVoltage.Value.ToString("F2");
        Vector.Tools.Output.WriteLine(request);
        serialPort.WriteLine(request);
    }

    // "VOLT?"を送信するメソッド
    private void SendQueryVoltage(){
        string request = "VOLT?";
        Vector.Tools.Output.WriteLine(request);
        serialPort.WriteLine(request);
    }

/////
// ユーザ定義イベントハンドラ
/////
    /////
    // システム変数(Texio.RequestVoltage)の変化検出ハンドラ - CANoeクラスライブラリ
    // Texio.RequestVoltage : 外部からの電圧設定要求
    [OnChange(typeof(Texio.RequestVoltage))]
    public void TexioRequestVoltageChanged()
    {
       isTexioRequestVoltageChanged = true; // OnTimerSendSetVoltageのタイミングで送信され、falaseとなる。
    }

    /////
    // SetVoltage("VOLT XX.XX")送信タイマ時メソッド
    // 注 : システム変数の変化に応じて連続送信するとCANoe/Texioの動作が怪しくなる。
    //      連続的に要求された場合でもタイマ分間をあけるために利用する
    [OnTimer(50)]
    public void OnTimerSendSetVoltage()
    {
        if(isTexioRequestVoltageChanged){
            SendSetVoltage();
            isTexioRequestVoltageChanged = false;
        }
    }

    /////
    // COMポート受信ハンドラ - .Netクラスライブラリ
    // 注 : DataReceivedHandlerは別スレッドからコールされるためCANoeのAPIが制限される。[ref1]
    private static void DataReceivedHandler(
                        object sender,
                        SerialDataReceivedEventArgs e)
    {
        // バッファから受信データを読み出す
        string response = ((SerialPort)sender).ReadLine();

        // 末尾に付いているLF('\r', 0x0A)を落とす。e.g."VOLT 13.00\r"==>"VOLT 13.00"
        response = response.Trim(); 

        /////
        // 応答(response)の解析
        /////
        
        if(response.Contains("VOLT ")) // 電圧応答
        {
            //' 'を区切りとして配列にする。 e.g."VOLT 13.00"==>["VOLT","13.00"]
            string[] response_arr = response.Split(' '); 
            if(response_arr.Length == 2)
            {
                // 文字列をdouble型に変換し、システム変数Texio.ActualVoltage.Valueに書き込み
                Texio.ActualVoltage.Value = Convert.ToDouble(response_arr[1]);
            }
            else // 期待しないフォーマットを破棄
            {
                Vector.Tools.Output.WriteLine("Data Received(Length error, ignored):"+ response + ", response_arr.Length=" + response_arr.Length.ToString());
            }
        }
        else // 未実装の応答を破棄
        {
            Vector.Tools.Output.WriteLine("Data Received(Unexpected, ignored):" + response);
        }
    }
}







// [ref1]
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
