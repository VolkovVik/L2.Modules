using System.Diagnostics.CodeAnalysis;
using Aspu.Common.Domain;
using Aspu.Common.Domain.Errors;
using Aspu.Common.Domain.Results;
using Aspu.Modules.Orders.Domain.Model.SharedKernel;

namespace Aspu.Modules.Orders.Domain.Model.CodeAggregate;

/// <summary>
///     Marking code
/// </summary>
public sealed class Code : Aggregate
{
    /// <summary>
    ///     Ctr
    /// </summary>
    [ExcludeFromCodeCoverage]
    private Code() { }

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <param name="codeId">Marking code identifier</param>
    /// <param name="orderId">Order identifier</param>
    /// <param name="orderUnitId">Order unit identifier</param>
    /// <param name="value">Marking code value</param>
    private Code(Guid codeId, Guid orderId, Guid orderUnitId, string value) : this()
    {
        Id = codeId;
        OrderId = orderId;
        OrderUnitId = orderUnitId;
        Value = value;
        PrintedStatus = CodePrintedStatus.Unprinted;
        AggregatedStatus = CodeAggregatedStatus.None;
    }

    /// <summary>
    ///     Value
    /// </summary>
    public string Value { get; private set; }

    /// <summary>
    ///     Order identifier
    /// </summary>
    public Guid OrderId { get; private set; }

    /// <summary>
    ///     Order unit identifier
    /// </summary>
    public Guid OrderUnitId { get; private set; }

    /// <summary>
    ///     Aggregate identifier
    /// </summary>
    public Guid? ParentId { get; }

    /// <summary>
    ///     Client identifier
    /// </summary>
    public Guid? ClientId { get; }

    /// <summary>
    ///     Print status
    /// </summary>
    public CodePrintedStatus PrintedStatus { get; private set; }

    /// <summary>
    ///     Aggregation status
    /// </summary>
    public CodeAggregatedStatus AggregatedStatus { get; private set; }

    /// <summary>
    ///     Index
    /// </summary>
    public int? Index { get; }

    /// <summary>
    ///     Volume, milliliters
    /// </summary>
    public Volume? Volume { get; }

    /// <summary>
    ///     Weight, gramms
    /// </summary>
    public Weight? Weight { get; }

    /// <summary>
    ///     Grade (code quality)
    /// </summary>
    public Grade? Grade { get; }

    /// <summary>
    ///     Service code
    /// </summary>
    public string? ServiceCode { get; }

    /// <summary>
    ///     Aggregate marking code
    /// </summary>
    public string? ParentValue { get; }

    /// <summary>
    ///     Use-by date
    /// </summary>
    public DateTime? UseUntil { get; }

    /// <summary>
    ///     Print date
    /// </summary>
    public DateTime? PrintedOn { get; private set; }

    /// <summary>
    ///     Validation date
    /// </summary>
    public DateTime? ValidatedOn { get; private set; }

    /// <summary>
    ///     Defect write-off date
    /// </summary>
    public DateTime? DefectedOn { get; private set; }

    /// <summary>
    ///     Date added to aggregate
    /// </summary>
    public DateTime? AggregatingOn { get; }

    /// <summary>
    ///     Aggregation date
    /// </summary>
    public DateTime? AggregatedOn { get; }

    /// <summary>
    ///     Date added to package
    /// </summary>
    public DateTime? PackingOn { get; }

    /// <summary>
    ///     Packing date
    /// </summary>
    public DateTime? PackedOn { get; }

    /// <summary>
    ///     Utilization report identifier
    /// </summary>
    public Guid? UtilizationReportId { get; }

    /// <summary>
    ///     Aggregation report identifier
    /// </summary>
    public Guid? AggregationReportId { get; }

    /// <summary>
    ///     Defect report identifier
    /// </summary>
    public Guid? DefectReportId { get; }

    /// <summary>
    ///     Unused codes report identifier
    /// </summary>
    public Guid? ReleaseReportId { get; }

    /// <summary>
    ///     Factory Method
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="orderUnitId"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Result<Code, Error> Create(Guid orderId, Guid orderUnitId, string value)
    {
        var id = Guid.NewGuid();

        if (orderId == Guid.Empty)
            return Error.ValueIsInvalid(nameof(orderId));
        if (orderUnitId == Guid.Empty)
            return Error.ValueIsInvalid(nameof(orderUnitId));
        if (string.IsNullOrWhiteSpace(value))
            return Error.ValueIsInvalid(nameof(value));

        var code = new Code(id, orderId, orderUnitId, value);

        ///code.RaiseDomainEvent(new CodeCreatedDomainEvent(id, orderId, orderUnitId, value));

        return code;
    }

    /// <summary>
    ///     Converts to string
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Value;

    /// <summary>
    ///     Sets print status to "Printing"
    /// </summary>
    /// <param name="orderId">Order identifier</param>
    /// <param name="orderUnitId">Product type identifier</param>
    /// <returns></returns>
    public Result<object, Error> SetPrinting(Guid orderId, Guid orderUnitId)
    {
        if (OrderId != orderId)
            return Errors.CodeHasOtherOrder(Value);
        if (OrderUnitId != orderUnitId)
            return Errors.CodeHasOtherOrderUnit(Value);
        if (UseUntil.HasValue && DateTime.UtcNow >= UseUntil.Value)
            return Errors.CodeHasExpiredStatus(Value);
        if (PrintedStatus != CodePrintedStatus.Unprinted)
            return Errors.CodeHasNotUnprintedStatus(Value);

        PrintedOn = null;
        PrintedStatus = CodePrintedStatus.Printing;

        ///RaiseDomainEvent(new CodePrintingDomainEvent(Id, OrderId, OrderUnitId, Value));

        return new object();
    }

    /// <summary>
    ///     Sets print status to "Printed"
    /// </summary>
    /// <param name="orderId">Order identifier</param>
    /// <param name="orderUnitId">Product type identifier</param>
    /// <param name="printedOn">Print date</param>
    /// <returns></returns>
    public Result<object, Error> SetPrinted(Guid orderId, Guid orderUnitId, DateTime? printedOn = null)
    {
        printedOn ??= DateTime.UtcNow;

        if (OrderId != orderId)
            return Errors.CodeHasOtherOrder(Value);
        if (OrderUnitId != orderUnitId)
            return Errors.CodeHasOtherOrderUnit(Value);
        if (UseUntil.HasValue && printedOn >= UseUntil.Value)
            return Errors.CodeHasExpiredStatus(Value);
        if (PrintedStatus == CodePrintedStatus.Printed)
            return Errors.CodeAlreadyHasPrintedStatus(Value);

        PrintedOn = printedOn;
        PrintedStatus = CodePrintedStatus.Printed;

        ///RaiseDomainEvent(new CodePrintedDomainEvent(Id, OrderId, OrderUnitId, Value));

        return new object();
    }

    /// <summary>
    ///     Sets print status to "Unprinted"
    /// </summary>
    /// <param name="orderId">Order identifier</param>
    /// <param name="orderUnitId">Product type identifier</param>
    /// <returns></returns>
    public Result<object, Error> SetUnprinted(Guid orderId, Guid orderUnitId)
    {
        if (OrderId != orderId)
            return Errors.CodeHasOtherOrder(Value);
        if (OrderUnitId != orderUnitId)
            return Errors.CodeHasOtherOrderUnit(Value);
        if (UseUntil.HasValue && DateTime.UtcNow >= UseUntil.Value)
            return Errors.CodeHasExpiredStatus(Value);
        if (PrintedStatus == CodePrintedStatus.Unprinted)
            return Errors.CodeAlreadyHasUnprintedStatus(Value);

        PrintedOn = null;
        PrintedStatus = CodePrintedStatus.Unprinted;

        ///RaiseDomainEvent(new CodeUnprintedDomainEvent(Id, OrderId, OrderUnitId, Value));

        return new object();
    }

    /// <summary>
    ///     Sets aggregation status to "Defected"
    /// </summary>
    /// <param name="orderId">Order identifier</param>
    /// <param name="orderUnitId">Product type identifier</param>
    /// <param name="defectedOn">Defect write-off date</param>
    /// <returns></returns>
    public Result<object, Error> SetDefected(Guid orderId, Guid orderUnitId, DateTime? defectedOn = null)
    {
        defectedOn ??= DateTime.UtcNow;

        if (OrderId != orderId)
            return Errors.CodeHasOtherOrder(Value);
        if (OrderUnitId != orderUnitId)
            return Errors.CodeHasOtherOrderUnit(Value);
        if (UseUntil.HasValue && defectedOn >= UseUntil.Value)
            return Errors.CodeHasExpiredStatus(Value);
        if (AggregatedStatus == CodeAggregatedStatus.Defected)
            return Errors.CodeAlreadyHasDefectedStatus(Value);
        if (AggregatedStatus == CodeAggregatedStatus.Aggregating)
            return Errors.CodeHasAggregatingStatus(Value);
        if (AggregatedStatus == CodeAggregatedStatus.Aggregated)
            return Errors.CodeHasAggregatedStatus(Value);
        if (AggregatedStatus == CodeAggregatedStatus.Packing)
            return Errors.CodeHasPackingStatus(Value);
        if (AggregatedStatus == CodeAggregatedStatus.Packed)
            return Errors.CodeHasPackedStatus(Value);

        DefectedOn = defectedOn;
        PrintedOn ??= defectedOn;
        ValidatedOn ??= defectedOn;
        PrintedStatus = CodePrintedStatus.Printed;
        AggregatedStatus = CodeAggregatedStatus.Defected;

        /// RaiseDomainEvent(new CodeDefectedDomainEvent(Id, OrderId, OrderUnitId, Value));

        return new object();
    }

    /// <summary>
    ///     Sets aggregation status to "Validated"
    /// </summary>
    /// <param name="orderId">Order identifier</param>
    /// <param name="orderUnitId">Product type identifier</param>
    /// <param name="validatedOn">Validation date</param>
    /// <param name="isResetDefected">Flag to reset defect write-off</param>
    /// <returns></returns>
    public Result<object, Error> SetValidated(Guid orderId, Guid orderUnitId, DateTime? validatedOn = null, bool isResetDefected = false)
    {
        validatedOn ??= DateTime.UtcNow;

        if (OrderId != orderId)
            return Errors.CodeHasOtherOrder(Value);
        if (OrderUnitId != orderUnitId)
            return Errors.CodeHasOtherOrderUnit(Value);
        if (UseUntil.HasValue && validatedOn >= UseUntil.Value)
            return Errors.CodeHasExpiredStatus(Value);
        if (!isResetDefected && AggregatedStatus == CodeAggregatedStatus.Defected)
            return Errors.CodeHasDefectedStatus(Value);
        if (AggregatedStatus == CodeAggregatedStatus.Validated)
            return Errors.CodeHasValidatedStatus(Value);
        if (AggregatedStatus == CodeAggregatedStatus.Aggregating)
            return Errors.CodeHasAggregatingStatus(Value);
        if (AggregatedStatus == CodeAggregatedStatus.Aggregated)
            return Errors.CodeHasAggregatedStatus(Value);
        if (AggregatedStatus == CodeAggregatedStatus.Packing)
            return Errors.CodeHasPackingStatus(Value);
        if (AggregatedStatus == CodeAggregatedStatus.Packed)
            return Errors.CodeHasPackedStatus(Value);

        DefectedOn = null;
        ValidatedOn = validatedOn;
        PrintedOn ??= validatedOn;
        PrintedStatus = CodePrintedStatus.Printed;
        AggregatedStatus = CodeAggregatedStatus.Validated;

        ///RaiseDomainEvent(new CodeValidatedDomainEvent(Id, OrderId, OrderUnitId, Value));

        return new object();
    }

    private static class Errors
    {
        private static readonly string Name = $"{nameof(Code).ToLowerInvariant()}";

        public static Error CodeHasOtherOrder(string value) =>
            Error.Failure($"{Name}.has.other.order.status", $"Код маркировки {value} принадлежит другому заказу");

        public static Error CodeHasOtherOrderUnit(string value) =>
           Error.Failure($"{Name}.has.overdue.status", $"Код маркировки {value} принадлежит другому продуктовому типу");

        public static Error CodeHasExpiredStatus(string value) =>
            Error.Failure($"{Name}.has.expired.status", $"Код маркировки {value} находится в статусе \"Просрочен\"");

        public static Error CodeHasNotUnprintedStatus(string value) =>
            Error.Failure($"{Name}.has.not.unprinted.status", $"Код маркировки {value} не находится в статусе \"Ненапечатан\"");

        public static Error CodeAlreadyHasPrintedStatus(string value) =>
            Error.Failure($"{Name}.already.has.printed.status", $"Код маркировки {value} уже находится в статусе \"Напечатан\"");

        public static Error CodeAlreadyHasUnprintedStatus(string value) =>
            Error.Failure($"{Name}.already.has.unprinted.status", $"Код маркировки {value} уже находится в статусе \"Ненапечатан\"");

        public static Error CodeAlreadyHasDefectedStatus(string value) =>
            Error.Failure($"{Name}.already.has.defected.status", $"Код маркировки {value} уже находится в статусе \"Дефектован\"");

        public static Error CodeHasValidatedStatus(string value) =>
            Error.Failure($"{Name}.has.validated.status", $"Код маркировки {value} находится в статусе \"Валидирован\"");

        public static Error CodeHasDefectedStatus(string value) =>
            Error.Failure($"{Name}.has.defected.status", $"Код маркировки {value} находится в статусе \"Дефектован\"");

        public static Error CodeHasAggregatingStatus(string value) =>
            Error.Failure($"{Name}.has.aggregating.status", $"Код маркировки {value} находится в статусе \"Агрегируется\"");

        public static Error CodeHasAggregatedStatus(string value) =>
            Error.Failure($"{Name}.has.aggregated.status", $"Код маркировки {value} находится в статусе \"Агрегирован\"");

        public static Error CodeHasPackingStatus(string value) =>
            Error.Failure($"{Name}.has.packing.status", $"Код маркировки {value} находится в статусе \"Упаковывается\"");

        public static Error CodeHasPackedStatus(string value) =>
            Error.Failure($"{Name}.has.packed.status", $"Код маркировки {value} находится в статусе \"Упакован\"");
    }
}
