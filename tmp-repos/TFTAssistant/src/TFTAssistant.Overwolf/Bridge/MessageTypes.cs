namespace TFTAssistant.Overwolf.Bridge;

public static class MessageTypes
{
    public const string OverwolfInfoUpdate = "overwolf_info_update";
    public const string OverwolfGameEvent = "overwolf_game_event";
    public const string UiReady = "ui_ready";
    public const string RequestRecommendations = "request_recommendations";
    public const string ToggleOverlay = "toggle_overlay";
    public const string StateChanged = "state_changed";
    public const string FullState = "full_state";
    public const string Recommendations = "recommendations";
}

public class OWMessage
{
    public string? Type { get; set; }
    public object? Payload { get; set; }
}
