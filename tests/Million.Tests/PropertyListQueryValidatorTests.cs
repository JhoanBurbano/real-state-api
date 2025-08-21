using FluentAssertions;
using Million.Application.DTOs;
using Million.Application.Validation;

namespace Million.Tests;

public class PropertyListQueryValidatorTests
{
    [Test]
    public void Should_fail_when_page_less_than_1()
    {
        var v = new PropertyListQueryValidator();
        var res = v.Validate(new PropertyListQuery { Page = 0 });
        res.IsValid.Should().BeFalse();
    }

    [Test]
    public void Should_fail_when_pageSize_out_of_bounds()
    {
        var v = new PropertyListQueryValidator();
        v.Validate(new PropertyListQuery { PageSize = 0 }).IsValid.Should().BeFalse();
        v.Validate(new PropertyListQuery { PageSize = 101 }).IsValid.Should().BeFalse();
    }

    [Test]
    public void Should_fail_when_min_greater_than_max()
    {
        var v = new PropertyListQueryValidator();
        var res = v.Validate(new PropertyListQuery { MinPrice = 100, MaxPrice = 50 });
        res.IsValid.Should().BeFalse();
    }

    [Test]
    public void Should_fail_when_sort_invalid()
    {
        var v = new PropertyListQueryValidator();
        var res = v.Validate(new PropertyListQuery { Sort = "invalid" });
        res.IsValid.Should().BeFalse();
    }
}

