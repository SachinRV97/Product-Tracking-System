namespace ProductTrackingSystem.Models;

public enum ProductStatus
{
    Available = 1,
    InUse = 2,
    Transferred = 3,
    Disposed = 4,
    UnderMaintenance = 5
}

public enum LinenStage
{
    Received = 1,
    Sorting = 2,
    Washing = 3,
    Drying = 4,
    Ironing = 5,
    Packing = 6,
    Dispatch = 7,
    Completed = 8
}
