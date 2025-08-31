using MediatR;

namespace PentaBoard.Api.Features.Files.DeleteFiles;

public record DeleteProjectFileCommand(Guid ProjectId, Guid Id) : IRequest<bool>;
