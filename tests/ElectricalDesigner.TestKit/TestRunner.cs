using System.Reflection;

namespace ElectricalDesigner.TestKit;

/// <summary>
/// Découvre par réflexion toutes les méthodes publiques marquées [Fact] dans
/// l'assembly courant, les exécute, et affiche un rapport façon xUnit.
/// Retourne un code de sortie non nul si au moins un test échoue, pour que
/// `dotnet run` (et donc la CI) détecte correctement l'échec.
/// </summary>
public static class TestRunner
{
    public static int RunAll(Assembly assembly)
    {
        var testMethods = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            .Where(m => m.GetCustomAttribute<FactAttribute>() is not null)
            .OrderBy(m => m.DeclaringType!.FullName)
            .ThenBy(m => m.Name)
            .ToList();

        Console.WriteLine($"Découverte : {testMethods.Count} test(s).");
        Console.WriteLine();

        var passed = 0;
        var failed = 0;
        var skipped = 0;

        foreach (var method in testMethods)
        {
            var declaringType = method.DeclaringType!;
            var testName = $"{declaringType.Name}.{method.Name}";
            var factAttribute = method.GetCustomAttribute<FactAttribute>()!;

            if (factAttribute.Skip is not null)
            {
                skipped++;
                Console.WriteLine($"  [SKIP] {testName} — {factAttribute.Skip}");
                continue;
            }

            try
            {
                var instance = Activator.CreateInstance(declaringType);
                method.Invoke(instance, parameters: null);
                passed++;
                Console.WriteLine($"  [ OK ] {testName}");
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                failed++;
                Console.WriteLine($"  [FAIL] {testName} — {ex.InnerException.Message}");
            }
            catch (Exception ex)
            {
                failed++;
                Console.WriteLine($"  [FAIL] {testName} — {ex.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Résultat : {passed} réussi(s), {failed} échoué(s), {skipped} ignoré(s).");

        return failed == 0 ? 0 : 1;
    }
}
