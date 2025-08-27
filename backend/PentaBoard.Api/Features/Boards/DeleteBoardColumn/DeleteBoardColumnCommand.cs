using MediatR;
using PentaBoard.Api.Features.Boards.Common;

namespace PentaBoard.Api.Features.Boards.DeleteBoardColumn;

public sealed record DeleteBoardColumnCommand(
    Guid ProjectId,
    Guid ColumnId
) : IRequest<IReadOnlyList<BoardColumnDto>>;
