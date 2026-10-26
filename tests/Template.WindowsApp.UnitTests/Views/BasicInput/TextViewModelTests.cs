namespace Template.WindowsApp.Views.BasicInput;

using Template.WindowsApp.Controls;

[Collection(ViewModelTestGroup.Name)]
public sealed class TextViewModelTests
{
    [Fact]
    public void CitySuggestionsPutWordStartsFirst()
    {
        // Arrange
        using var viewModel = new TextViewModel();

        // Act
        viewModel.CityText = "to";

        // Assert
        Assert.Equal(["Tokyo", "Toronto", "Boston", "Kyoto", "Stockholm"], CityNames(viewModel));
    }

    [Fact]
    public void CitySuggestionsContainEveryWord()
    {
        // Arrange
        using var viewModel = new TextViewModel();

        // Act
        viewModel.CityText = "san fran";

        // Assert
        Assert.Equal(["San Francisco"], CityNames(viewModel));
    }

    [Fact]
    public void CitySuggestionsMarkMatchedCharacters()
    {
        // Arrange
        using var viewModel = new TextViewModel();

        // Act
        viewModel.CityText = "kyo";

        // Assert
        Assert.Equal(["Kyoto", "Tokyo"], CityNames(viewModel));
        Assert.Equal([new HighlightRun("To", false), new HighlightRun("kyo", true)], viewModel.CitySuggestions[1].NameRuns);
    }

    [Fact]
    public void CitySuggestionsIgnoreCaseAndWidth()
    {
        // Arrange
        using var viewModel = new TextViewModel();

        // Act
        viewModel.CityText = "ＴＯＫ";

        // Assert
        Assert.Equal(["Tokyo"], CityNames(viewModel));
    }

    [Fact]
    public void CitySuggestionsAreEmptyWithoutMatch()
    {
        // Arrange
        using var viewModel = new TextViewModel();

        // Act
        viewModel.CityText = "xyz";
        var unmatched = CityNames(viewModel);
        viewModel.CityText = "  ";
        var blank = CityNames(viewModel);

        // Assert
        Assert.Empty(unmatched);
        Assert.Empty(blank);
    }

    [Fact]
    public void SubmitCityUsesChosenSuggestion()
    {
        // Arrange
        using var viewModel = new TextViewModel();
        viewModel.OnActivated();
        viewModel.CityText = "to";
        var chosen = viewModel.CitySuggestions[1];

        // Act
        viewModel.SubmitCityCommand.Execute(new AutoSuggestBoxQuery("to", chosen));

        // Assert
        Assert.Equal("Toronto", viewModel.SelectedCity);
        Assert.Equal("Toronto", viewModel.CityText);
    }

    [Fact]
    public void SubmitCityWithoutChoiceUsesFirstSuggestion()
    {
        // Arrange
        using var viewModel = new TextViewModel();
        viewModel.OnActivated();
        viewModel.CityText = "par";

        // Act
        viewModel.SubmitCityCommand.Execute(new AutoSuggestBoxQuery("par", null));

        // Assert
        Assert.Equal("Paris", viewModel.SelectedCity);
        Assert.Equal("Paris", viewModel.CityText);
    }

    [Fact]
    public void SubmitCityWithoutMatchSelectsNothing()
    {
        // Arrange
        using var viewModel = new TextViewModel();
        viewModel.OnActivated();
        viewModel.CityText = "xyz";

        // Act
        viewModel.SubmitCityCommand.Execute(new AutoSuggestBoxQuery("xyz", null));

        // Assert
        Assert.Null(viewModel.SelectedCity);
        Assert.Equal("xyz", viewModel.CityText);
    }

    [Fact]
    public void CustomerSuggestionsSearchNameAndCompany()
    {
        // Arrange
        using var viewModel = new TextViewModel();

        // Act
        viewModel.CustomerText = "kenji fab";

        // Assert
        var suggestion = Assert.Single(viewModel.CustomerSuggestions);
        Assert.Equal("Kenji Tanaka", suggestion.Customer.Name);
        Assert.Equal([new HighlightRun("Kenji", true), new HighlightRun(" Tanaka", false)], suggestion.NameRuns);
        Assert.Equal([new HighlightRun("Fab", true), new HighlightRun("rikam", false)], suggestion.CompanyRuns);
    }

    [Fact]
    public void SubmitCustomerWithoutChoiceUsesFirstSuggestion()
    {
        // Arrange
        using var viewModel = new TextViewModel();
        viewModel.OnActivated();
        viewModel.CustomerText = "con";
        var names = viewModel.CustomerSuggestions.Select(static x => x.Customer.Name).ToArray();

        // Act
        viewModel.SubmitCustomerCommand.Execute(new AutoSuggestBoxQuery("con", null));

        // Assert
        Assert.Equal(["Ava Brown", "Hana Sato"], names);
        Assert.Equal("Ava Brown", viewModel.SelectedCustomer?.Name);
        Assert.Equal("Ava Brown", viewModel.CustomerText);
    }

    [Fact]
    public void SubmitCustomerWithoutMatchKeepsQuery()
    {
        // Arrange
        using var viewModel = new TextViewModel();
        viewModel.OnActivated();
        viewModel.CustomerText = "xyz";

        // Act
        viewModel.SubmitCustomerCommand.Execute(new AutoSuggestBoxQuery("xyz", null));

        // Assert
        Assert.Null(viewModel.SelectedCustomer);
        Assert.Equal("xyz", viewModel.CustomerQuery);
    }

    [Fact]
    public void ResetClearsSuggestBoxes()
    {
        // Arrange
        using var viewModel = new TextViewModel();
        viewModel.OnActivated();
        viewModel.CityText = "to";
        viewModel.SubmitCityCommand.Execute(new AutoSuggestBoxQuery("to", null));
        viewModel.CustomerText = "con";
        viewModel.SubmitCustomerCommand.Execute(new AutoSuggestBoxQuery("con", null));

        // Act
        viewModel.ResetCommand.Execute(null);

        // Assert
        Assert.Equal(string.Empty, viewModel.CityText);
        Assert.Empty(viewModel.CitySuggestions);
        Assert.Null(viewModel.SelectedCity);
        Assert.Equal(string.Empty, viewModel.CustomerText);
        Assert.Empty(viewModel.CustomerSuggestions);
        Assert.Null(viewModel.SelectedCustomer);
        Assert.Null(viewModel.CustomerQuery);
    }

    private static string[] CityNames(TextViewModel viewModel) =>
        viewModel.CitySuggestions.Select(static x => x.Name).ToArray();
}
