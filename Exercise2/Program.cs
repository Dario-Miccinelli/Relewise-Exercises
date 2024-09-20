using Newtonsoft.Json;
using Relewise.Client.DataTypes;

// Write a C# Class, that implements the IJob -interface (see Appendix-section)
// - The Execute(…)-method should:
// o Download data from this URL:
// ▪ https://cdn.relewise.com/academy/productdata/googleshoppingfeed
// o The data represents a Google Shopping Feed
// (https://support.google.com/merchants/answer/7052112?hl=en), where you need to
// map each <item>-element into an instance of Relewise.Client.DataTypes.Product
// (available on NuGet: Install-Package Relewise.Client)
// ▪ The minimum properties that you should map are:
// • <g:id> should map to string Id
// • <title> should map to Multilingual DisplayName
// • <g:price> should map to MultiCurrency ListPrice
// • <g:sale_price> should map to MultiCurrency SalesPrice
// o The method, which returns a string, should return a message containing a count of how
// many products got mapped.

public interface IJob
{
    Task<string> Execute(
    JobArguments arguments,
    Func<string, Task> info,
    Func<string, Task> warn,
    CancellationToken token);
}
public class JobArguments
{
    public JobArguments(
    Guid datasetId,
    string apiKey,
    IReadOnlyDictionary<string, string> jobConfiguration)
    {
        DatasetId = datasetId;
        ApiKey = apiKey;
        JobConfiguration = jobConfiguration;
    }
    public Guid DatasetId { get; }
    public string ApiKey { get; }
    public IReadOnlyDictionary<string, string> JobConfiguration { get; }
}