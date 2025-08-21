using FluentAssertions;
using NSubstitute;
using Million.Application.Common;
using Million.Application.DTOs;
using Million.Application.Interfaces;
using Million.Application.Services;
using Million.Domain.Entities;

namespace Million.Tests;

public class PropertyServiceTests
{
    [Test]
    public async Task GetById_returns_null_when_not_found()
    {
        var repo = Substitute.For<IPropertyRepository>();
        repo.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Property?)null);
        var svc = new PropertyService(repo);
        var res = await svc.GetByIdAsync("id", CancellationToken.None);
        res.Should().BeNull();
    }

    [Test]
    public async Task List_maps_entities_to_dtos_and_pagination()
    {
        var repo = Substitute.For<IPropertyRepository>();
        var items = new List<Property> { new Property { Id = "1", IdOwner = "o", Name = "n", AddressProperty = "a", PriceProperty = 10, Image = "i" } };
        repo.FindAsync(Arg.Any<PropertyListQuery>(), Arg.Any<CancellationToken>()).Returns((items, 1));
        var svc = new PropertyService(repo);
        var result = await svc.GetPropertiesAsync(new PropertyListQuery{ Page = 2, PageSize = 5 }, CancellationToken.None);
        result.Total.Should().Be(1);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.Items.Should().HaveCount(1);
        result.Items.First().Id.Should().Be("1");
    }
}

