using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace zhw_MaxPlayers;

[MinimumApiVersion(147)]
public class zhw_MaxPlayers : BasePlugin
{
    public override string ModuleName => "zhw_MaxPlayers";
    public override string ModuleVersion => "1.0.1";
    public override string ModuleAuthor => "zhw1nq";
    public override string ModuleDescription => "Giới hạn số lượng người chơi trong server";

    private Config _config = new();
    private string _configPath = "";

    public override void Load(bool hotReload)
    {
        _configPath = Path.Combine(ModuleDirectory, "zhw_MaxPlayers.json");
        LoadConfig();

        RegisterListener<Listeners.OnClientConnected>(OnClientConnected);
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam);

        Console.WriteLine($"[zhw_MaxPlayers] Plugin loaded - Max Players: {_config.MaxPlayers}, Max Spectators: {_config.MaxSpectators}");
    }

    private void LoadConfig()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                string json = File.ReadAllText(_configPath);
                _config = JsonSerializer.Deserialize<Config>(json) ?? new Config();
            }
            else
            {
                _config = new Config();
                SaveConfig();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[zhw_MaxPlayers] Error loading config: {ex.Message}");
            _config = new Config();
        }
    }

    private void SaveConfig()
    {
        try
        {
            string json = JsonSerializer.Serialize(_config, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[zhw_MaxPlayers] Error saving config: {ex.Message}");
        }
    }

    private void OnClientConnected(int playerSlot)
    {
        var player = Utilities.GetPlayerFromSlot(playerSlot);
        if (player == null || !player.IsValid) return;

        Server.NextFrame(() =>
        {
            CheckPlayerLimits(player);
        });
    }

    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid) return HookResult.Continue;

        Server.NextFrame(() =>
        {
            CheckPlayerLimits(player);
        });

        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        return HookResult.Continue;
    }

    private HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid) return HookResult.Continue;

        var newTeam = @event.Team;
        var oldTeam = @event.Oldteam;

        if (newTeam == (int)CsTeam.Terrorist || newTeam == (int)CsTeam.CounterTerrorist)
        {
            Server.NextFrame(() =>
            {
                CheckTeamJoin(player, (CsTeam)newTeam, (CsTeam)oldTeam);
            });
        }

        return HookResult.Continue;
    }

    private void CheckTeamJoin(CCSPlayerController player, CsTeam newTeam, CsTeam oldTeam)
    {
        if (player == null || !player.IsValid) return;

        var allPlayers = Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot).ToList();
        var tPlayers = allPlayers.Where(p => p.TeamNum == (int)CsTeam.Terrorist).Count();
        var ctPlayers = allPlayers.Where(p => p.TeamNum == (int)CsTeam.CounterTerrorist).Count();
        var spectators = allPlayers.Where(p => p.TeamNum == (int)CsTeam.Spectator).Count();

        bool shouldBlock = false;
        string blockReason = "";

        if (newTeam == CsTeam.Terrorist && tPlayers > _config.MaxPlayersPerTeam)
        {
            shouldBlock = true;
            blockReason = $"Team Terrorist đã đầy ({_config.MaxPlayersPerTeam}/{_config.MaxPlayersPerTeam})!";
        }
        else if (newTeam == CsTeam.CounterTerrorist && ctPlayers > _config.MaxPlayersPerTeam)
        {
            shouldBlock = true;
            blockReason = $"Team Counter-Terrorist đã đầy ({_config.MaxPlayersPerTeam}/{_config.MaxPlayersPerTeam})!";
        }

        if (shouldBlock)
        {
            if (newTeam == CsTeam.Terrorist && ctPlayers < _config.MaxPlayersPerTeam)
            {
                Server.NextFrame(() =>
                {
                    if (player.IsValid)
                    {
                        player.ChangeTeam(CsTeam.CounterTerrorist);
                        player.PrintToChat($" {ChatColors.Yellow}[zhw_MaxPlayers] Team T đã đầy, bạn được chuyển sang CT!");
                    }
                });
            }
            else if (newTeam == CsTeam.CounterTerrorist && tPlayers < _config.MaxPlayersPerTeam)
            {
                Server.NextFrame(() =>
                {
                    if (player.IsValid)
                    {
                        player.ChangeTeam(CsTeam.Terrorist);
                        player.PrintToChat($" {ChatColors.Yellow}[zhw_MaxPlayers] Team CT đã đầy, bạn được chuyển sang T!");
                    }
                });
            }
            else if (spectators < _config.MaxSpectators)
            {
                Server.NextFrame(() =>
                {
                    if (player.IsValid)
                    {
                        player.ChangeTeam(CsTeam.Spectator);
                        player.PrintToChat($" {ChatColors.Red}[zhw_MaxPlayers] Cả 2 team đã đầy, bạn được chuyển sang spectator!");
                    }
                });
            }
            else
            {
                if (_config.EnableCustomCommand)
                {
                    ExecuteCustomCommand(player, _config.CustomCommands.ServerFull);
                }
                else
                {
                    KickPlayer(player, _config.Messages.ServerFull);
                }
            }
        }
    }

    private void CheckPlayerLimits(CCSPlayerController player)
    {
        if (player == null || !player.IsValid) return;

        var allPlayers = Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot).ToList();
        var totalPlayers = allPlayers.Count;

        if (totalPlayers > _config.MaxPlayers)
        {
            if (_config.EnableCustomCommand)
            {
                ExecuteCustomCommand(player, _config.CustomCommands.ServerFull);
            }
            else
            {
                KickPlayer(player, _config.Messages.ServerFull);
            }
            return;
        }

        var spectators = allPlayers.Where(p => p.TeamNum == (int)CsTeam.Spectator).ToList();
        var tPlayers = allPlayers.Where(p => p.TeamNum == (int)CsTeam.Terrorist).Count();
        var ctPlayers = allPlayers.Where(p => p.TeamNum == (int)CsTeam.CounterTerrorist).Count();

        if (player.TeamNum == (int)CsTeam.Spectator && spectators.Count > _config.MaxSpectators)
        {
            if (tPlayers < _config.MaxPlayersPerTeam && ctPlayers < _config.MaxPlayersPerTeam)
            {
                if (tPlayers < ctPlayers)
                {
                    ExecuteCustomCommandOrDefault(player, _config.CustomCommands.JoinTerrorist, () => {
                        player.ChangeTeam(CsTeam.Terrorist);
                        player.PrintToChat($" {ChatColors.Green}[zhw_MaxPlayers] Bạn được thêm vào team Terrorist!");
                    });
                }
                else if (ctPlayers < tPlayers)
                {
                    ExecuteCustomCommandOrDefault(player, _config.CustomCommands.JoinCounterTerrorist, () => {
                        player.ChangeTeam(CsTeam.CounterTerrorist);
                        player.PrintToChat($" {ChatColors.Green}[zhw_MaxPlayers] Bạn được thêm vào team Counter-Terrorist!");
                    });
                }
                else
                {
                    var random = new Random();
                    if (random.Next(0, 2) == 0)
                    {
                        ExecuteCustomCommandOrDefault(player, _config.CustomCommands.JoinTerrorist, () => {
                            player.ChangeTeam(CsTeam.Terrorist);
                            player.PrintToChat($" {ChatColors.Green}[zhw_MaxPlayers] Bạn được thêm vào team Terrorist!");
                        });
                    }
                    else
                    {
                        ExecuteCustomCommandOrDefault(player, _config.CustomCommands.JoinCounterTerrorist, () => {
                            player.ChangeTeam(CsTeam.CounterTerrorist);
                            player.PrintToChat($" {ChatColors.Green}[zhw_MaxPlayers] Bạn được thêm vào team Counter-Terrorist!");
                        });
                    }
                }
            }
            else if (tPlayers < _config.MaxPlayersPerTeam)
            {
                ExecuteCustomCommandOrDefault(player, _config.CustomCommands.JoinTerrorist, () => {
                    player.ChangeTeam(CsTeam.Terrorist);
                    player.PrintToChat($" {ChatColors.Green}[zhw_MaxPlayers] Bạn được thêm vào team Terrorist!");
                });
            }
            else if (ctPlayers < _config.MaxPlayersPerTeam)
            {
                ExecuteCustomCommandOrDefault(player, _config.CustomCommands.JoinCounterTerrorist, () => {
                    player.ChangeTeam(CsTeam.CounterTerrorist);
                    player.PrintToChat($" {ChatColors.Green}[zhw_MaxPlayers] Bạn được thêm vào team Counter-Terrorist!");
                });
            }
            else
            {
                if (_config.EnableCustomCommand)
                {
                    ExecuteCustomCommand(player, _config.CustomCommands.TooManySpectators);
                }
                else
                {
                    KickPlayer(player, _config.Messages.TooManySpectators);
                }
            }
        }
    }

    private void ExecuteCustomCommandOrDefault(CCSPlayerController player, string command, Action defaultAction)
    {
        if (_config.EnableCustomCommand && !string.IsNullOrEmpty(command))
        {
            ExecuteCustomCommand(player, command);
        }
        else
        {
            defaultAction?.Invoke();
        }
    }

    private void ExecuteCustomCommand(CCSPlayerController player, string command)
    {
        if (string.IsNullOrEmpty(command)) return;

        // Thay thế placeholder
        string finalCommand = command
            .Replace("{playername}", player.PlayerName)
            .Replace("{steamid}", player.SteamID.ToString())
            .Replace("{userid}", player.UserId.ToString())
            .Replace("{slot}", player.Slot.ToString());

        // Gửi thông báo cho người chơi trước khi thực hiện lệnh
        player.PrintToChat($" {ChatColors.Yellow}[zhw_MaxPlayers] Đang thực hiện lệnh...");

        // Thực hiện lệnh
        Server.NextFrame(() =>
        {
            Server.ExecuteCommand(finalCommand);
            Console.WriteLine($"[zhw_MaxPlayers] Executed command: {finalCommand}");
        });
    }

    private void KickPlayer(CCSPlayerController player, string reason)
    {
        player.PrintToChat($" {ChatColors.Red}[zhw_MaxPlayers] {reason}");

        Server.NextFrame(() =>
        {
            if (player.IsValid)
            {
                Server.ExecuteCommand($"kickid {player.UserId} \"{reason}\"");
            }
        });
    }

    [ConsoleCommand("zhw_reload_config", "Reload zhw_MaxPlayers config")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/generic")]
    public void OnReloadConfig(CCSPlayerController? player, CommandInfo command)
    {
        LoadConfig();

        string message = $"[zhw_MaxPlayers] Config reloaded! Max Players: {_config.MaxPlayers}, Max Spectators: {_config.MaxSpectators}";

        if (player != null)
        {
            player.PrintToChat($" {ChatColors.Green}{message}");
        }
        else
        {
            Console.WriteLine(message);
        }
    }

    [ConsoleCommand("zhw_player_info", "Show current player count")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/generic")]
    public void OnPlayerInfo(CCSPlayerController? player, CommandInfo command)
    {
        var allPlayers = Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot).ToList();
        var totalPlayers = allPlayers.Count;
        var spectators = allPlayers.Where(p => p.TeamNum == (int)CsTeam.Spectator).Count();
        var tPlayers = allPlayers.Where(p => p.TeamNum == (int)CsTeam.Terrorist).Count();
        var ctPlayers = allPlayers.Where(p => p.TeamNum == (int)CsTeam.CounterTerrorist).Count();

        string info = $"Players: {totalPlayers}/{_config.MaxPlayers} | T: {tPlayers}/{_config.MaxPlayersPerTeam} | CT: {ctPlayers}/{_config.MaxPlayersPerTeam} | Spec: {spectators}/{_config.MaxSpectators}";

        if (player != null)
        {
            player.PrintToChat($" {ChatColors.Yellow}[zhw_MaxPlayers] {info}");
        }
        else
        {
            Console.WriteLine($"[zhw_MaxPlayers] {info}");
        }
    }
}

public class Config
{
    [JsonPropertyName("MaxPlayers")]
    public int MaxPlayers { get; set; } = 12;

    [JsonPropertyName("MaxPlayersPerTeam")]
    public int MaxPlayersPerTeam { get; set; } = 5;

    [JsonPropertyName("MaxSpectators")]
    public int MaxSpectators { get; set; } = 2;

    [JsonPropertyName("EnableCustomCommand")]
    public bool EnableCustomCommand { get; set; } = true;

    [JsonPropertyName("Messages")]
    public ConfigMessages Messages { get; set; } = new();

    [JsonPropertyName("CustomCommands")]
    public CustomCommands CustomCommands { get; set; } = new();
}

public class ConfigMessages
{
    [JsonPropertyName("ServerFull")]
    public string ServerFull { get; set; } = "Server đã đầy! Vui lòng thử lại sau.";

    [JsonPropertyName("TooManySpectators")]
    public string TooManySpectators { get; set; } = "Quá nhiều spectator! Tối đa 2 người.";

    [JsonPropertyName("TeamFull")]
    public string TeamFull { get; set; } = "Team đã đầy! Bạn được chuyển sang spectator.";

    [JsonPropertyName("TeamBalanced")]
    public string TeamBalanced { get; set; } = "Team đã được cân bằng!";
}

public class CustomCommands
{
    [JsonPropertyName("ServerFull")]
    public string ServerFull { get; set; } = "kickid {userid} \"Server đã đầy!\"";

    [JsonPropertyName("TooManySpectators")]
    public string TooManySpectators { get; set; } = "kickid {userid} \"Quá nhiều spectator!\"";

    [JsonPropertyName("TeamFull")]
    public string TeamFull { get; set; } = "css_team \"{playername}\" spec";

    [JsonPropertyName("TeamBalanced")]
    public string TeamBalanced { get; set; } = "css_team \"{playername}\" ct";

    [JsonPropertyName("JoinTerrorist")]
    public string JoinTerrorist { get; set; } = "css_team \"{playername}\" t";

    [JsonPropertyName("JoinCounterTerrorist")]
    public string JoinCounterTerrorist { get; set; } = "css_team \"{playername}\" ct";
}