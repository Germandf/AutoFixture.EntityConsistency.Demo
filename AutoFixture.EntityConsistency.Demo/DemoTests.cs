using AutoFixture.Kernel;
using FluentAssertions;

namespace AutoFixture.EntityConsistency.Demo;

public class City
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public CityInfo? CityInfo { get; set; }
}

public class CityInfo
{
    public int CityId { get; set; }
    public City? City { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
}

public class DemoTests
{
    private IFixture fixture = new Fixture().ApplyCustomizations();

    [Fact]
    public void City_Id_Should_Match_CityInfo_CityId()
    {
        var city = fixture.Create<City>();
        city.Id.Should().Be(city.CityInfo!.CityId);
    }

    [Fact]
    public void City_Instances_Should_Have_Unique_Ids()
    {
        var city = fixture.Create<City>();
        var city2 = fixture.Create<City>();
        city.Id.Should().NotBe(city2.Id);
    }

    [Fact]
    public void CityInfo_CityId_Should_Match_City_Id()
    {
        var cityInfo = fixture.Create<CityInfo>();
        cityInfo.CityId.Should().Be(cityInfo.City!.Id);
    }

    [Fact]
    public void CityInfo_Instances_Should_Have_Unique_CityIds()
    {
        var cityInfo = fixture.Create<CityInfo>();
        var cityInfo2 = fixture.Create<CityInfo>();
        cityInfo.CityId.Should().NotBe(cityInfo2.CityId);
    }

    [Fact]
    public void City_Should_Use_Auto_Properties()
    {
        var city = fixture.Create<City>();
        city.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CityInfo_Should_Use_Auto_Properties()
    {
        var cityInfo = fixture.Create<CityInfo>();
        cityInfo.Latitude.Should().NotBe(0);
        cityInfo.Longitude.Should().NotBe(0);
    }

    [Fact]
    public void City_Should_Be_Able_To_Use_Build_And_Still_Apply_Customization()
    {
        var name = "Buenos Aires";
        var city = fixture.Build<City>().With(c => c.Name, name).Create();
        city.Name.Should().Be(name);
        city.Id.Should().Be(city.CityInfo!.CityId);
    }

    [Fact]
    public void CityInfo_Should_Be_Able_To_Use_Build_And_Still_Apply_Customization()
    {
        var latitude = 34.6037f;
        var cityInfo = fixture.Build<CityInfo>().With(ci => ci.Latitude, latitude).Create();
        cityInfo.Latitude.Should().Be(latitude);
        cityInfo.CityId.Should().Be(cityInfo.City!.Id);
    }
}

/*
 * Only modify code beyond this point to make the tests pass.
 * The code above should remain unchanged to demonstrate
 * that the customizations are working as expected.
 */

public static class FixtureExtensions
{
    public static IFixture ApplyCustomizations(this IFixture fixture)
    {
        fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        fixture.Behaviors.Add(new NullRecursionBehavior());
        fixture.Customize(new CityCustomization());
        fixture.Customize(new CityInfoCustomization());
        return fixture;
    }
}

public class CityCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<City>(composer => new Postprocessor(composer,
            new ActionSpecimenCommand<City>(city =>
            {
                if (city.CityInfo != null)
                {
                    city.CityInfo.City = city;
                    city.CityInfo.CityId = city.Id;
                }
            }))
        );
    }
}

public class CityInfoCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<CityInfo>(composer => new Postprocessor(composer,
            new ActionSpecimenCommand<CityInfo>(cityInfo =>
            {
                if (cityInfo.City != null)
                {
                    cityInfo.City.Id = cityInfo.CityId;
                    cityInfo.City.CityInfo = cityInfo;
                }
            }))
        );
    }
}