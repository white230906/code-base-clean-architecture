namespace CodeBase.Domain.Common;

public abstract class BaseEntity<TKey> : IAuditableEntity
    where TKey : notnull
{
    public TKey Id { get; protected set; } = default!;
    public bool IsDeleted { get; protected set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public void MarkDeleted() => IsDeleted = true;

    public void Restore() => IsDeleted = false;
}
