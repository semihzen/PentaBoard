using MediatR;
using PentaBoard.Api.Features.Boards.Common;

namespace PentaBoard.Api.Features.Boards.RenameBoardColumn;

public sealed record RenameBoardColumnCommand(
    Guid ProjectId,
    Guid ColumnId,
    string Name
) : IRequest<BoardColumnDto>;
