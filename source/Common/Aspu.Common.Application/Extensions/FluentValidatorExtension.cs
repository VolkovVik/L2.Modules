using FluentValidation;

namespace Aspu.Common.Application.Extensions;

public static class FluentValidatorExtension
{
    extension<T>(IRuleBuilder<T, string> ruleBuilder)
    {
        public IRuleBuilderOptions<T, string> MustBeNotEmpty() =>
            ruleBuilder.Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("The value is null or empty");
    }

    public static IRuleBuilderOptions<T, IList<TElement>> ListMustContainFewerThan<T, TElement>(this IRuleBuilder<T, IList<TElement>> ruleBuilder, int num) =>
        ruleBuilder.Must(list => list is not null && list.Count < num).WithMessage("The list contains too many items");
}
