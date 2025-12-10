using System;

readonly record struct CommandResult(bool Success = false, string? Message = null, Exception? Exception = null) {
    internal bool Success { get; init; } = Success;
    internal string? Message { get; init; } = Message;
    internal Exception? Exception { get; init; } = Exception;
}
