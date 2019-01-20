// �����Œǉ����ꂽ
using System;
using System.Reflection;
using Vector.Tools;
using Vector.CANoe.Runtime;
using Vector.CANoe.Sockets;
using Vector.CANoe.Threading;
using Vector.Diagnostics;


using System.IO.Ports; 

public class texio_control : MeasurementScript
{
// System.IO.Ports.SerialPort (COM�|�[�g����N���X)
// https://docs.microsoft.com/ja-jp/dotnet/api/system.io.ports.serialport?view=netframework-4.7.2
    static SerialPort serialPort;

/////
// �X�N���v�g�쐬���Ɏ������������C�x���g�n���h��
/////
    /////
    // �V�~�����[�V�����J�n���O�ɌĂ΂�鏉�����n���h��
    // ��: �K�v�ȃC���X�^���X�͂����őS�Đ������Ă����B
    public override void Initialize()
    {
        // �|�[�g���A�r�b�g���[�g�ȂǊe��ݒ������B
        serialPort = new SerialPort("COM26", 9600, System.IO.Ports.Parity.Even, 7, System.IO.Ports.StopBits.One);

        // COM�|�[�g��M�n���h���̐ݒ�
        serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

        // COM�|�[�g�̃I�[�v��
        serialPort.Open();

        // �O������̓d���ݒ�v���̏����l��ݒ肵�Ă���
        Texio.RequestVoltage.Value = 13.0;
    }

    /////
    // �V�~�����[�V�����I������ɌĂ΂��n���h��
    public override void Shutdown()
    {
        serialPort.Close();
    }

/////
// ���[�U��`�C�x���g�n���h��
/////
    /////
    // �V�X�e���ϐ�(Texio.RequestVoltage)�̕ω����o�n���h�� - CANoe�N���X���C�u����
    // Texio.RequestVoltage : �O������̓d���ݒ�v��
    [OnChange(typeof(Texio.RequestVoltage))]
    public void TexioRequestVoltageChanged()
    {
       serialPort.WriteLine("VOLT "+ Texio.RequestVoltage.Value.ToString("F2"));
       serialPort.WriteLine("VOLT?");
    }

    /////
    // COM�|�[�g��M�n���h�� - .Net�N���X���C�u����
    // �� : DataReceivedHandler�͕ʃX���b�h����R�[������邽��CANoe��API�����������B[1]
    private static void DataReceivedHandler(
                        object sender,
                        SerialDataReceivedEventArgs e)
    {
        // �o�b�t�@�����M�f�[�^��ǂݏo��
        string response = ((SerialPort)sender).ReadExisting();

        // �����ɕt���Ă���LF('\r', 0x0A)�𗎂Ƃ��Be.g."VOLT 13.00\r"==>"VOLT 13.00"
        response = response.Trim(); 

        /////
        // ����(response)�̉��
        /////
        
        if(response.Contains("VOLT ")) // �d���₢���킹"VOLT?"�ɑ΂��鉞��
        {
            //' '����؂�Ƃ��Ĕz��ɂ���B e.g."VOLT 13.00"==>["VOLT","13.00"]
            string[] response_arr = response.Split(' '); 
            if(response_arr.Length == 2)
            {
                // �������double�^�ɕϊ����A�V�X�e���ϐ�Texio.ActualVoltage.Value�ɏ�������
                Texio.ActualVoltage.Value = Convert.ToDouble(response_arr[1]);
            }
            else // ���҂��Ȃ��t�H�[�}�b�g��j��
            {
                Vector.Tools.Output.WriteLine("Data Received(Length error, ignored):"+ response + ", response_arr.Length=" + response_arr.Length.ToString());
            }
        }
        else // �������̉�����j��
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
