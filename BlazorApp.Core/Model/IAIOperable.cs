using BlazorApp.Core.Component;

namespace BlazorApp.Core.Model
{
    public interface IAIOperable
    {
        string DisplayName { get; set; }

        void SetValue(object? value);

        object CreateOperationInfo();
    }

    public interface IOperationScopeOwner
    {
        void Register(AIInteractionScope scope);
    }
}
