using System.Text;
using System.Net.Sockets;
using MinecraftLaunch.Classes.Interfaces;
using System.Diagnostics;
using System.Text.Json;
using MinecraftLaunch.Classes.Models.ServerPing;
using MinecraftLaunch.Classes.Models.Event;

namespace MinecraftLaunch.Components.Watcher;

/// <summary>
/// 服务器 Ping 监视器
/// </summary>
public sealed class ServerPingWatcher(ushort port, string address, int versionId, CancellationTokenSource tokenSource = default) : IWatcher {
    private int _offset;
    private List<byte> _buffer;
    private NetworkStream _stream;
    private CancellationTokenSource _cancellationTokenSource = tokenSource;

    public event EventHandler<ServerLatencyChangedEventArgs> ServerLatencyChanged;
    public event EventHandler<ProgressChangedEventArgs> ServerConnectionProgressChanged;

    public ushort Port => port;
    public string Address => address;
    public int VersionId => versionId;

    public void Cancel() {
        _cancellationTokenSource?.Cancel();
    }

    public void Start() => _ = StartAsync();

    public override string ToString() => $"{Address}:{Port}";

    public async ValueTask StartAsync() {
        if (_cancellationTokenSource is null) {
            _cancellationTokenSource = new();
        }

        while (!_cancellationTokenSource.IsCancellationRequested) {
            await PingAsync();
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }

    public async ValueTask PingAsync() {
        using var client = new TcpClient {
            SendTimeout = 5000,
            ReceiveTimeout = 5000
        };

        var sw = new Stopwatch();
        var timeOut = TimeSpan.FromSeconds(3);
        using var cts = new CancellationTokenSource(timeOut);

        sw.Start();
        cts.CancelAfter(timeOut);

        try {
            await client.ConnectAsync(Address, Port, cts.Token);
        } catch (TaskCanceledException) {
            throw new OperationCanceledException($"Server {this} connection failed, connection timed out ({timeOut.Seconds}s)。", cts.Token);
        }

        sw.Stop();

        ReportProgress(0.1d, "Connecting to server", TaskStatus.Created);

        if (!client.Connected) {
            ReportProgress(0.1d, "Unable to connect to server", TaskStatus.Faulted);
            return;
        }

        _buffer = new List<byte>();
        _stream = client.GetStream();

        ReportProgress(0.3d, "Sending request", TaskStatus.Running);

        /*
         * Send a "Handshake" packet
         * http://wiki.vg/Server_List_Ping#Ping_Process
         */
        WriteVarInt(VersionId == 0 ? 47 : VersionId);
        WriteString(Address);
        WriteShort(Port);
        WriteVarInt(1);
        await Flush(0);

        /*
         * Send a "Status Request" packet
         * http://wiki.vg/Server_List_Ping#Ping_Process
         */
        await Flush(0);

        /*
         * If you are using a modded server then use a larger buffer to account, 
         * see link for explanation and a motd to HTML snippet
         * https://gist.github.com/csh/2480d14fbbb33b4bbae3#gistcomment-2672658
         */
        var batch = new byte[1024];
        await using var ms = new MemoryStream();
        var remaining = 0;
        var flag = false;

        var latency = sw.ElapsedMilliseconds;

        do {
            _offset = 0;
            var readLength = await _stream.ReadAsync(batch.AsMemory());
            await ms.WriteAsync(batch.AsMemory(0, readLength), cts.Token);
            if (!flag) {
                var packetLength = ReadVarInt(ms.ToArray());
                remaining = packetLength - _offset;
                flag = true;
            }

            if (readLength == 0 && remaining != 0)
                continue;

            remaining -= readLength;
        } while (remaining > 0);

        var buffer = ms.ToArray();
        var length = ReadVarInt(buffer);
        var packet = ReadVarInt(buffer);
        var jsonLength = ReadVarInt(buffer);

        var json = ReadString(buffer, jsonLength);
        var ping = JsonSerializer.Deserialize(json, PingPayloadContext.Default.PingPayload);

        ReportProgress(1.0d, $"Server {this} successfully connected", TaskStatus.Canceled);
        ReportLatency(latency, ping);
    }

    #region Read/Write methods

    private byte ReadByte(IReadOnlyList<byte> buffer) {
        var b = buffer[_offset];
        _offset += 1;
        return b;
    }

    private byte[] Read(byte[] buffer, int length) {
        var data = new byte[length];
        Array.Copy(buffer, _offset, data, 0, length);
        _offset += length;
        return data;
    }

    private int ReadVarInt(IReadOnlyList<byte> buffer) {
        var value = 0;
        var size = 0;
        int b;
        while (((b = ReadByte(buffer)) & 0x80) == 0x80) {
            value |= (b & 0x7F) << (size++ * 7);
            if (size > 5) throw new IOException("This VarInt is an imposter!");
        }

        return value | ((b & 0x7F) << (size * 7));
    }

    private string ReadString(byte[] buffer, int length) {
        var data = Read(buffer, length);
        return Encoding.UTF8.GetString(data);
    }

    private void WriteVarInt(int value) {
        while ((value & 128) != 0) {
            _buffer.Add((byte)((value & 127) | 128));
            value = (int)(uint)value >> 7;
        }

        _buffer.Add((byte)value);
    }

    private void WriteShort(ushort value) {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }

    private void WriteString(string data) {
        var buffer = Encoding.UTF8.GetBytes(data);
        WriteVarInt(buffer.Length);
        _buffer.AddRange(buffer);
    }

    private async Task Flush(int id = -1) {
        var buffer = _buffer.ToArray();
        _buffer.Clear();

        var add = 0;
        var packetData = new[] { (byte)0x00 };
        if (id >= 0) {
            WriteVarInt(id);
            packetData = _buffer.ToArray();
            add = packetData.Length;
            _buffer.Clear();
        }

        WriteVarInt(buffer.Length + add);
        var bufferLength = _buffer.ToArray();
        _buffer.Clear();

        await _stream.WriteAsync(bufferLength.AsMemory());
        await _stream.WriteAsync(packetData.AsMemory());
        await _stream.WriteAsync(buffer.AsMemory());
    }

    private void ReportLatency(long latency, PingPayload pingPayload) {
        ServerLatencyChanged?.Invoke(this, new() {
            Latency = latency,
            Response = pingPayload
        });
    }

    private void ReportProgress(double progress, string progressStatus, TaskStatus status) {
        ServerConnectionProgressChanged?.Invoke(this, new(status, progress, progressStatus));
    }

    #endregion
}