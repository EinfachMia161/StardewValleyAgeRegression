namespace AgeRegression.Items;

/// <summary>
/// Orchestrates the basic wardrobe item acquisition pipeline.
/// </summary>
public sealed class ItemAcquisitionService
{
    private readonly WardrobeItemResolver _resolver;
    private readonly ItemUnlockService _unlockService;
    private readonly IItemFactory _itemFactory;

    public ItemAcquisitionService(
        WardrobeItemResolver resolver,
        ItemUnlockService unlockService,
        IItemFactory itemFactory)
    {
        _resolver = resolver;
        _unlockService = unlockService;
        _itemFactory = itemFactory;
    }

    public AcquisitionResult Acquire(string itemId, AcquisitionContext? context = null)
    {
        var effectiveContext = context ?? new AcquisitionContext();

        var resolved = _resolver.Resolve(itemId);
        if (resolved is null)
            return AcquisitionResult.Failed(AcquisitionFailureReason.UnknownItem);

        if (!_unlockService.IsUnlocked(resolved))
            return AcquisitionResult.Failed(AcquisitionFailureReason.Locked);

        var item = _itemFactory.CreateFromResolved(
            resolved,
            effectiveContext.HasBooster,
            effectiveContext.CurrentAbsoluteDay);

        return item is null
            ? AcquisitionResult.Failed(AcquisitionFailureReason.CreationFailed)
            : AcquisitionResult.Succeeded(resolved, item);
    }
}
