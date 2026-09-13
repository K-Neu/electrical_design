using System.Diagnostics.CodeAnalysis;

namespace ElectricalDesigner.TestKit;

/// <summary>Exception levée lorsqu'une assertion échoue.</summary>
public sealed class AssertionFailedException(string message) : Exception(message);

/// <summary>
/// Sous-ensemble volontairement restreint de l'API Assert de xUnit.
/// Voir la remarque de migration dans <see cref="FactAttribute"/>.
/// </summary>
public static class Assert
{
    public static void True(bool condition, string? message = null)
    {
        if (!condition)
        {
            throw new AssertionFailedException(message ?? "Attendu : true, obtenu : false.");
        }
    }

    public static void False(bool condition, string? message = null)
    {
        if (condition)
        {
            throw new AssertionFailedException(message ?? "Attendu : false, obtenu : true.");
        }
    }

    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new AssertionFailedException(
                $"Attendu : '{expected}', obtenu : '{actual}'.");
        }
    }

    public static void NotNull([NotNull] object? value, string? message = null)
    {
        if (value is null)
        {
            throw new AssertionFailedException(message ?? "Attendu : une valeur non nulle.");
        }
    }

    public static void Contains(string expectedSubstring, string actual)
    {
        if (!actual.Contains(expectedSubstring, StringComparison.Ordinal))
        {
            throw new AssertionFailedException(
                $"Attendu que '{actual}' contienne '{expectedSubstring}'.");
        }
    }

    /// <summary>Vérifie qu'appeler <paramref name="action"/> lève bien une exception de type <typeparamref name="TException"/>.</summary>
    public static TException Throws<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException ex)
        {
            return ex;
        }
        catch (Exception ex)
        {
            throw new AssertionFailedException(
                $"Attendu une exception de type '{typeof(TException).Name}', obtenu '{ex.GetType().Name}' : {ex.Message}");
        }

        throw new AssertionFailedException(
            $"Attendu une exception de type '{typeof(TException).Name}', mais aucune exception n'a été levée.");
    }
}
