using MediatR;

namespace PentaBoard.Api.Features.Files.PreviewFiles;

public record PreviewProjectFileQuery(Guid ProjectId, Guid Id) : IRequest<PreviewFileResult>;

public sealed record PreviewFileResult(string AbsPath, string ContentType);
