namespace CodeBase.Domain.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; }

    void MarkDeleted();
}
