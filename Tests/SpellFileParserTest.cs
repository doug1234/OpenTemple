using System.IO;
using System.Linq;
using FluentAssertions;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Spells;
using NUnit.Framework;

namespace OpenTemple.Tests;

public class SpellFileParserTest
{
    [Test]
    public void CanParseSchoolOfMagic()
    {
        Parse("School: Abjuration").spellSchoolEnum.Should().Be(SchoolOfMagic.Abjuration);
    }

    [Test]
    public void CanParseSpellLists()
    {
        var entry = Parse(
            "Level: Brd 4",
            "Level: Clr 5",
            "Level: Luck 5"
        );

        entry.spellLvls.Should().Equal(
            new SpellEntryLevelSpec(Stat.level_bard, 4),
            new SpellEntryLevelSpec(Stat.level_cleric, 5),
            new SpellEntryLevelSpec(DomainId.Luck, 5)
        );
    }

    [Test]
    public void CanParseSpellComponents()
    {
        var entry = Parse(
            "Component: V",
            "Component: S",
            "Component: GP 250"
        );
        entry.spellComponentBitmask.Should()
            .Be(SpellComponent.Verbal | SpellComponent.Somatic | SpellComponent.Material);
        entry.costGp.Should().Be(250);
    }

    [Test]
    public void CanParseCastingTime()
    {
        Parse("Casting Time: Full Round").castingTimeType.Should().Be(SpellCastingTime.FullRoundAction);
    }

    [Test]
    public void CanParseMinTargets()
    {
        Parse("min_Target: 1").minTarget.Should().Be(1);
    }

    [Test]
    public void CanParseMaxTargets()
    {
        Parse("max_Target: 1").maxTarget.Should().Be(1);
    }

    private SpellEntry Parse(params string[] lines)
    {
        var content = string.Join("\r\n", lines) + "\r\n";
        return SpellFileParser.Parse(0, "path", new StringReader(content));
    }
}