namespace Chat.Api;

public record MessageDto(string Text, UserDto Sender);

public record UserDto(string Name, string Email);