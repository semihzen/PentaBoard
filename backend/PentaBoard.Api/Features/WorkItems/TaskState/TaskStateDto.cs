// Features/WorkItems/TaskState/TaskStateDto.cs
namespace PentaBoard.Api.Features.WorkItems.TaskState;

public sealed record TaskStateDto(
    int Created,        // seçilen gün aralığında oluşturulan iş sayısı
    int Completed,      // şu an done kolondakiler
    int InProgress,     // şu an diğer kolonlardakiler
    IReadOnlyList<RecentItemDto> Recent // son oluşturulan 10 iş
);

public sealed record RecentItemDto(
    Guid Id,
    string Title,
    DateTime CreatedAt,
    string Status // "completed" | "işlemde"
);
