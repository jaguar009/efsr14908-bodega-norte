using System.ComponentModel.DataAnnotations;

namespace BodegaNorte.Api.Models;

public sealed record ProductDto(string Id, string Name, string Category, decimal Stock, decimal MinStock,
    decimal Cost, decimal Price, string Supplier, string Unit, DateTimeOffset Updated, long Version);
public sealed record ProductInput(
    [Required, StringLength(160)] string Name,
    [Required, StringLength(80)] string Category,
    decimal Stock, decimal MinStock, decimal Cost, decimal Price,
    [StringLength(120)] string? Supplier,
    [Required, StringLength(30)] string Unit, long? ExpectedVersion = null);
public sealed record StockInput(decimal Quantity, [Required, StringLength(300)] string Reason, long ExpectedVersion);
public sealed record SaleLineDto(string ProductId, string Name, decimal Quantity, decimal UnitPrice, decimal? UnitCost, decimal Total);
public sealed record SaleDto(string Id, DateTimeOffset SoldAt, string Customer, decimal Items, decimal Total,
    string Method, string Status, string CreatedBy, string? CancelReason, IReadOnlyList<SaleLineDto> Lines, Guid? RequestId);
public sealed record SaleItemInput([Required] string ProductId, decimal Quantity, decimal? ExpectedPrice = null);
public sealed record SaleInput([Required] string PaymentMethod, [Required] IReadOnlyList<SaleItemInput> Items,
    Guid RequestId, [StringLength(120)] string Customer = "Venta mostrador");
public sealed record CancelSaleInput([Required, StringLength(300)] string Reason);
public sealed record SupplierDto(int Id, string Name, string Contact, string Phone, int Products);
public sealed record SupplierInput([Required, StringLength(120)] string Name,
    [StringLength(120)] string Contact, [StringLength(30)] string Phone);
public sealed record SettingsDto([Required, StringLength(120)] string Name,
    [StringLength(11)] string Ruc, [StringLength(200)] string Address, [StringLength(30)] string Phone, bool StockAlerts);
public sealed record MemberDto(Guid UserId, string Email, string Role, bool Active);
public sealed record MemberInput([Required, EmailAddress] string Email, [Required] string Role, bool Active);
public sealed record StockMovementDto(long Id, string ProductId, string Name, decimal Quantity,
    decimal Balance, string Type, string Reason, string Actor, DateTimeOffset CreatedAt);
public sealed record AuditDto(long Id, string Action, string Entity, string EntityId, string Actor, DateTimeOffset CreatedAt);
public sealed record Actor(Guid Id, string Email, string Role);
public sealed class BusinessException(string message, int status = 400) : Exception(message)
{
    public int Status { get; } = status;
}
