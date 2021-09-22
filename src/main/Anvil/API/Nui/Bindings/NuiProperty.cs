namespace Anvil.API
{
  /// <summary>
  /// A NUI property that can be configured as a static readonly value, or a property that can be updated at runtime.
  /// </summary>
  /// <typeparam name="T">The underlying type of the property.</typeparam>
  public abstract class NuiProperty<T>
  {
    public static implicit operator NuiProperty<T>(T value)
    {
      return CreateValue(value);
    }

    /// <summary>
    /// Creates a Nui variable binding that can be changed at runtime.
    /// </summary>
    /// <param name="key">The key to use for the binding.</param>
    /// <returns>A NuiBind object.</returns>
    public static NuiBind<T> CreateBind(string key)
    {
      return new NuiBind<T>(key);
    }

    /// <summary>
    /// Creates a readonly Nui variable that cannot be changed at runtime.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static NuiValue<T> CreateValue(T value)
    {
      return new NuiValue<T>(value);
    }
  }
}
