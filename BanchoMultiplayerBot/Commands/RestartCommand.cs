using BanchoMultiplayerBot.Data;
using BanchoMultiplayerBot.Interfaces;

namespace BanchoMultiplayerBot.Commands;

public class RestartCommand : IPlayerCommand
{
    public string Command => "restart";

    public List<string>? Aliases => null;

    public bool AllowGlobal => true;

    public bool Administrator => true;

    public int MinimumArguments => 1;

    public string Usage => "!restart <lobby_id>";

    private static bool _isRestarting;

    public Task ExecuteAsync(CommandEventContext message)
    {
        // Race condition can probably appear here, but... no
        if (_isRestarting)
        {
            message.Reply("Restart currently in progress.");
            return Task.CompletedTask;
        }

        _isRestarting = true;

        if (!int.TryParse(message.Arguments[0], out var lobbyId))
        {
            message.Reply("Usage: !restart <lobby_id>");
            return Task.CompletedTask;
        }
        
        message.Reply("Restarting, this might take a second or two...");

        _ = Task.Run(async () =>
        {
            var lobby = message.Bot.Lobbies.FirstOrDefault(lobby => lobby.LobbyConfigurationId == lobbyId);

            if (lobby == null)
            {
                return;
            }
            
            await lobby.ConnectAsync();

            var attempts = 0;
            while (lobby.Health is not (LobbyHealth.Ok or LobbyHealth.Idle))
            {
                if (attempts++ > 10)
                {
                    break;
                }

                await Task.Delay(1000);
            }
            
            _isRestarting = false;
        });

        return Task.CompletedTask;
    }
}