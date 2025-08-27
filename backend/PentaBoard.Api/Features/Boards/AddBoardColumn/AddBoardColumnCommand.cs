using MediatR;
using PentaBoard.Api.Features.Boards.Common;

namespace PentaBoard.Api.Features.Boards.AddBoardColumn;

public sealed record AddBoardColumnCommand(
    Guid ProjectId,
    string Name,
    string? Color,
    int? WipLimit,
    bool? IsDoneLike,
    Guid? InsertAfterColumnId
) : IRequest<BoardColumnDto>;
