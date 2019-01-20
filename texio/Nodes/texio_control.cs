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
    // System.IO.Ports.SerialPort (COM�|�[�g����N���X)
    // https://docs.microsoft.com/ja-jp/dotnet/api/system.io.ports.serialport?view=netframework-4.7.2
    static SerialPort serialPort;

    public override void Initialize()
    {
        serialPort = new SerialPort("COM26", 9600, System.IO.Ports.Parity.Even, 7, System.IO.Ports.StopBits.One);

        serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

        // COM�|�[�g���I�[�v��
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
// �V�X�e���ϐ�(Texio.RequestVoltage)�̕ω����o�n���h��
// Texio.RequestVoltage : �O������̓d���ݒ�v��
[OnChange(typeof(Texio.RequestVoltage))]
    public void TexioRequestVoltageChanged()
    {
       serialPort.WriteLine("VOLT "+ Texio.RequestVoltage.Value.ToString("F2"));
       serialPort.WriteLine("VOLT?");
    }

/////
// �V���A����M�n���h��
// ����: DataReceivedHandler���ł�CANoe��.Net API�����������B[1]
    private static void DataReceivedHandler(
                        object sender,
                        SerialDataReceivedEventArgs e)
    {
        string response = ((SerialPort)sender).ReadExisting();

        response = response.Trim(); // �����ɕt���Ă���LF('\r', 0x0A)�𗎂Ƃ��Be.g."VOLT 13.00\r"==>"VOLT 13.00"

        // response���
        if(response.Contains("VOLT ")) // �d���₢���킹"VOLT?"�ɑ΂��鉞��
        {
            string[] response_arr = response.Split(' '); //' '����؂�Ƃ��Ĕz��ɂ���B e.g."VOLT 13.00"==>["VOLT","13.00"]
            if(response_arr.Length == 2)
            {
                Texio.ActualVoltage.Value = Convert.ToDouble(response_arr[1]); // �V�X�e���ϐ�Texio.ActualVoltage.Value�ւ̏�������
            }
            else // ���҂��Ȃ��`����
            {
                Vector.Tools.Output.WriteLine("Data Received(Length error, ignored):"+ response + ", response_arr.Length=" + response_arr.Length.ToString());
            }
        }
        else // �������̉����͖�������
        {
            Vector.Tools.Output.WriteLine("Data Received(Unexpected, ignored):" + response);
        }
    }
}



// [1]
// ���ϐ�(Environment variable)�𗘗p���悤�Ƃ������A
// CANoe 10.0��Help(CANoeCANalyzer.chm)��

// Start Page
// �� Programming @Further Topics
//   �� .NET @CANoe Programming
//     �� .NET Programming: Notes for Advanced Users @Further Information
//       �� Concurrency

// ���A

// - �X�N���v�g���ō쐬�����X���b�h����͊��ϐ�(DBC��Environment variable)�ɏ������݁E�ǂݍ��݂ł��Ȃ����ACAN�V�O�i�����M�Ȃǂ��ł��Ȃ��B
// - CANoe API�͈ȉ���3�Ɋ֘A������݂̂̂����p�ł���B
//     1. Writing system variables (not reading!) ��
//     2. Writing to log file
//     3. Writing messages to output window 
// �Ƃ̂��ƂȂ̂ŁA�u�V�X�e���ϐ��̏������݁v�ɂ��Ă���B

// (����)
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
