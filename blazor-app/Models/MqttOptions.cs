namespace BlazorApp.Models;

public class MqttOptions
{
    public const string SectionName = "Mqtt";

    public bool Enabled { get; set; } = true;

    /// <summary>Topic for PackML / process status JSON (camelCase, same shape as GET /api/process/status).</summary>
    public string StatusTopic { get; set; } = "gateway/process/status";

    /// <summary>ASP.NET WebSocket path for browser MQTT clients (mqtt.js).</summary>
    public string WebSocketPath { get; set; } = "/mqtt";

    /// <summary>Also listen on TCP (e.g. MQTT Explorer). Disable if the port is taken.</summary>
    public bool EnableTcp { get; set; } = true;

    public int TcpPort { get; set; } = 1883;
}
