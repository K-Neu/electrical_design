using ElectricalDesigner.Domain.Common;
using ElectricalDesigner.Domain.Installations;
using ElectricalDesigner.TestKit;

namespace ElectricalDesigner.Domain.Tests.Installations;

public class ElectricalInstallationTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 10, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_AvecTensionPositive_Reussit()
    {
        var installation = ElectricalInstallation.Create(
            EntityId<ElectricalInstallation>.New(),
            InstallationCategory.Domestic,
            CurrentType.AcSinglePhase,
            EarthingSystem.Tt,
            nominalVoltageV: 230,
            frequencyHz: 50,
            Now);

        Assert.Equal(230, installation.NominalVoltageV);
    }

    [Fact]
    public void Create_AvecTensionNegativeOuNulle_Echoue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ElectricalInstallation.Create(
                EntityId<ElectricalInstallation>.New(),
                InstallationCategory.Domestic,
                CurrentType.AcSinglePhase,
                EarthingSystem.Tt,
                nominalVoltageV: 0,
                frequencyHz: 50,
                Now));
    }

    [Fact]
    public void Create_EnCourantContinuAvecFrequenceNonNulle_Echoue()
    {
        // Cahier §21.2 : le type de courant est une donnée explicite, jamais déduite.
        // Une installation DC déclarée avec une fréquence non nulle est incohérente.
        Assert.Throws<ArgumentException>(() =>
            ElectricalInstallation.Create(
                EntityId<ElectricalInstallation>.New(),
                InstallationCategory.Domestic,
                CurrentType.Dc,
                EarthingSystem.Tt,
                nominalVoltageV: 48,
                frequencyHz: 50,
                Now));
    }

    [Fact]
    public void Validate_InstallationValide_NeSignaleAucunProbleme()
    {
        var installation = ElectricalInstallation.Create(
            EntityId<ElectricalInstallation>.New(),
            InstallationCategory.Domestic,
            CurrentType.AcSinglePhase,
            EarthingSystem.Tt,
            nominalVoltageV: 230,
            frequencyHz: 50,
            Now);

        var issues = installation.Validate();

        Assert.Equal(0, issues.Count);
    }

    [Fact]
    public void Validate_InstallationDcAFrequenceNulle_NeSignaleAucunProbleme()
    {
        var installation = ElectricalInstallation.Create(
            EntityId<ElectricalInstallation>.New(),
            InstallationCategory.Domestic,
            CurrentType.Dc,
            EarthingSystem.Tt,
            nominalVoltageV: 48,
            frequencyHz: 0,
            Now);

        var issues = installation.Validate();

        Assert.Equal(0, issues.Count);
    }
}
