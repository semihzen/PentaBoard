namespace PentaBoard.Api.Features.Boards.Common;

public sealed record BoardDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    IReadOnlyList<BoardColumnDto> Columns
);

public sealed record BoardColumnDto(
    Guid Id,
    string Name,
    int OrderKey,
    string? Color,
    int? WipLimit,
    bool IsDefault,
    bool IsDoneLike
);
