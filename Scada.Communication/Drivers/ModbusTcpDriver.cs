using System.Net.Sockets;
using Scada.Communication.Abstractions;
using Scada.Core.Models;

namespace Scada.Communication.Drivers;

public class ModbusTcpDriver : ICommunicationDriver
{
    private readonly string _ipAddress;
    private readonly int _port;
    private readonly byte _slaveId;
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private int _transactionId;
    private readonly object _lockObj = new();
    public string DriverName => "ModbusTCP";
    public bool IsConnected => _tcpClient?.Connected ?? false;
    public event EventHandler<DataReceivedEventArgs>? DataReceived;
    public event EventHandler? ConnectionStateChanged;

    public ModbusTcpDriver(string ipAddress, int port = 502, byte slaveId = 1)
    {
        _ipAddress = ipAddress;
        _port = port;
        _slaveId = slaveId;
    }

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(_ipAddress, _port, cancellationToken);
            _stream = _tcpClient.GetStream();
            ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
        catch
        {
            _tcpClient?.Dispose();
            _tcpClient = null;
            _stream = null;
            return false;
        }
    }

    public Task DisconnectAsync()
    {
        _stream?.Close();
        _tcpClient?.Close();
        _stream?.Dispose();
        _tcpClient?.Dispose();
        _stream = null;
        _tcpClient = null;
        ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }


    public async Task<object?> ReadAsync(string address, TagDataType
        dataType, CancellationToken cancellationToken = default)
    {
        if (!IsConnected || _stream == null) return null;
        try
        {
// 解析地址格式: HR:100 保持寄存器起始地址100
            var parts = address.Split(':');
            if (parts.Length != 2) return null;
            var registerType = parts[0];
            var startAddr = ushort.Parse(parts[1]);
            ushort count = dataType switch
            {
                TagDataType.Bool => 1,
                TagDataType.Int16 => 1,
                TagDataType.Int32 => 2,
                TagDataType.Float => 2,
                TagDataType.Double => 4,
                _ => 1
            };
            byte[] response;
            lock (_lockObj)
            {
                var request = BuildReadRequest(registerType, startAddr,
                    count);
                _stream.Write(request, 0, request.Length);
                _stream.Flush();
                var buffer = new byte[256];
                var read = _stream.Read(buffer, 0, buffer.Length);
                response = buffer.Take(read).ToArray();
            }

            var value = ParseResponse(response, dataType);
            DataReceived?.Invoke(this, new
                DataReceivedEventArgs(address, value, TagQuality.Good, DateTime.Now));
            return value;
        }
        catch
        {
            DataReceived?.Invoke(this, new
                DataReceivedEventArgs(address, null, TagQuality.Bad, DateTime.Now));
            return null;
        }
    }

    private byte[] BuildReadRequest(string registerType, ushort
        startAddr, ushort count)
    {
        byte functionCode = registerType switch
        {
            "HR" => 0x03, // 保持寄存器
            "IR" => 0x04, // 输入寄存器
            "DI" => 0x02, // 离散输入
            "CO" => 0x01, // 线圈
            _ => 0x03
        };
        var tid = Interlocked.Increment(ref _transactionId);
        var request = new byte[12];
// MBAP Header
        request[0] = (byte)(tid >> 8);
        request[1] = (byte)(tid & 0xFF);
        request[2] = 0x00;
        request[3] = 0x00;
        request[4] = 0x00;
        request[5] = 0x06;
        request[6] = _slaveId;
// PDU
        request[7] = functionCode;
        request[8] = (byte)(startAddr >> 8);
        request[9] = (byte)(startAddr & 0xFF);
        request[10] = (byte)(count >> 8);
        request[11] = (byte)(count & 0xFF);
        return request;
    }


    private static object? ParseResponse(byte[] response, TagDataType
        dataType)
    {
        if (response.Length < 9) return null;
        var data = response.Skip(9).ToArray();
        return dataType switch
        {
            TagDataType.Int16 => (short)((data[0] << 8) | data[1]),
            TagDataType.Int32 => (data[0] << 24) | (data[1] << 16) |
                                 (data[2] << 8) | data[3],
            TagDataType.Float => BitConverter.ToSingle(new[]
            {
                data[3],
                data[2], data[1], data[0]
            }, 0),
            TagDataType.Double =>
                BitConverter.ToDouble(data.Reverse().ToArray(), 0),
            TagDataType.Bool => data[0] != 0,
            _ => null
        };
    }


    public Task<bool> WriteAsync(string address, object value,
        TagDataType dataType, CancellationToken cancellationToken = default)
    {
    // 写寄存器实现（略，与读类似，功能码0x06/0x10）
        return Task.FromResult(true);
    }

    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }

}