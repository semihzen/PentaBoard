using MediatR;
using PentaBoard.Api.Features.Boards.Common;

namespace PentaBoard.Api.Features.Boards.GetBoard;

public sealed record GetBoardQuery(Guid ProjectId) : IRequest<BoardDto>;
