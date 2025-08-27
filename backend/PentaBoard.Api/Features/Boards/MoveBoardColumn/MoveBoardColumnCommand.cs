using MediatR;
using PentaBoard.Api.Features.Boards.Common;

namespace PentaBoard.Api.Features.Boards.MoveBoardColumn;

public sealed record MoveBoardColumnCommand(
    Guid ProjectId,
    Guid ColumnId,
    int TargetIndex  // 0-based: 0 = en başa, 1 = ikinci sıraya…
) : IRequest<IReadOnlyList<BoardColumnDto>>;
