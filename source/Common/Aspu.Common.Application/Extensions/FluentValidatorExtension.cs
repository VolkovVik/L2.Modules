using FluentValidation;

namespace Aspu.Common.Application.Extensions;

public static class FluentValidatorExtension
{
    extension<T>(IRuleBuilder<T, string> ruleBuilder)
    {
        public IRuleBuilderOptions<T, string> StringMustBeNotWhiteSpace(string name = "Value") =>
            ruleBuilder
                .NotNull().WithMessage($"The {name} is null")
                .NotEmpty().WithMessage($"The {name} is empty")
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage($"The {name} is white space");
    }

    extension<T>(IRuleBuilder<T, Guid?> ruleBuilder)
    {
        public IRuleBuilderOptions<T, Guid?> GuidMustBeNotDefault(string name = "Value") =>
            ruleBuilder
                .NotNull().WithMessage($"The {name} is null")
                .NotEqual(Guid.Empty).WithMessage($"The {name} is default");
    }

    public static IRuleBuilderOptions<T, IList<TElement>> ListMustContainFewerThan<T, TElement>(this IRuleBuilder<T, IList<TElement>> ruleBuilder, int num) =>
        ruleBuilder.Must(list => list is not null && list.Count < num).WithMessage("The list contains too many items");
}
