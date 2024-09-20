using System.Xml.Linq;
using Relewise.Client.DataTypes;

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

// Class for products
public class ProductXml
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public decimal ListPrice { get; set; }
    public decimal SalesPrice { get; set; }
}

public class GoogleShoppingFeedJob : IJob
{
    public async Task<string> Execute(
        JobArguments arguments,
        Func<string, Task> info,
        Func<string, Task> warn,
        CancellationToken token)
    {
        string url = "https://cdn.relewise.com/academy/productdata/googleshoppingfeed"; // Download Google Shopping Feed data from the Relewise URL
        using HttpClient client = new HttpClient();

        await info("Downloading Google Shopping Feed...");

        string xmlData = await client.GetStringAsync(url, token); // Download the XML data as a string, token is used to check if the operation was cancelled
        // Check if data was downloaded successfully
        if (string.IsNullOrWhiteSpace(xmlData))
        {
            return "No data received.";
        }
        // Parse the XML data into an XDocument object
        XDocument xmlDoc = XDocument.Parse(xmlData);
        // Define the namespace for the Google Shopping Feed elements
        XNamespace g = "http://base.google.com/ns/1.0"; 
        // Create a list to store the mapped products
        var products = new List<Product>();
        // Iterate through each <item> element in the XML data
        foreach (var item in xmlDoc.Descendants("item"))
        {   
            token.ThrowIfCancellationRequested(); // Check if the operation was cancelled
            // Get the product ID, title, price, and sale price from the <item> element
            var productId = item.Element(g + "id")?.Value;
            var title = item.Element("title")?.Value;
            var price = item.Element(g + "price")?.Value;
            var salePrice = item.Element(g + "sale_price")?.Value;

            // Validate required fields
            // It is checked that productId, title and price are not null or empty. If any of these are missing, the product is not processed.
            if (!string.IsNullOrEmpty(productId) && !string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(price))
            {
                // Parse prices, assuming format is "value currency"
                decimal listPriceValue = decimal.Parse(price.Split(' ')[0]);
                decimal salesPriceValue = !string.IsNullOrEmpty(salePrice) ? decimal.Parse(salePrice.Split(' ')[0]) : listPriceValue;

                 // Create a new Product object and map the properties
                var productXml = new ProductXml
                {
                    Id = productId,
                    Title = title,
                    ListPrice = listPriceValue,
                    SalesPrice = salesPriceValue
                };

                /// Create a new Product object and map the properties
                var product = new Product(productXml.Id)
                {
                    DisplayName = new Multilingual(new Multilingual.Value(new Language("en"), productXml.Title)),
                    ListPrice = new MultiCurrency("USD", productXml.ListPrice),
                    SalesPrice = new MultiCurrency("USD", productXml.SalesPrice),
                };
                // Add the product to the list
                products.Add(product);
            }
        }
        // Return a message containing the count of mapped products
        return $"Mapped {products.Count} products successfully."; // 71 products mapped
    }
}
class Program
{
    static async Task Main(string[] args)
    {
        var job = new GoogleShoppingFeedJob();
        var jobConfiguration = new Dictionary<string, string>();
        var arguments = new JobArguments(Guid.NewGuid(), "API_KEY", jobConfiguration);
        
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(30)); // Timeout after 30 seconds if the operation is taking too long
 
        try
        {
            string result = await job.Execute(arguments,
                msg => Task.Run(() => Console.WriteLine(msg)),
                msg => Task.Run(() => Console.WriteLine($"Warning: {msg}")),
                cts.Token); // Cancellation Token
            Console.WriteLine(result); // Console result (Mapped 71 products successfully.)
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("The operation was cancelled. Timeout exceeded."); // Only displayed if the operation is taking too long (>30 seconds)
        }
    }
}
