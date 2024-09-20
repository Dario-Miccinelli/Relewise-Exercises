using Relewise.Client.DataTypes;

// Exercise 3: Download Raw Product Data and Map to Relewise Products
// - Write a C# Class, that implements the IJob -interface (see Appendix-section)
// - The Execute(…)-method should:
// o Download data from this URL:
// ▪ https://cdn.relewise.com/academy/productdata/raw
// o Parse the raw data and map each row into an instance of
// Relewise.Client.DataTypes.Product (available on NuGet: Install-Package
// Relewise.Client)
// ▪ The minimum properties that you should map are:
// • ProductId should map to string Id
// • ProductName should map to Multilingual DisplayName
// • List Price should map to MultiCurrency ListPrice
// • Sales Price should map to MultiCurrency SalesPrice
// o The method, which returns a string, should return a message containing a count of how
// many products got mapped.



// IJob interface
public interface IJob
{
    Task<string> Execute(
        JobArguments arguments,
        Func<string, Task> info,
        Func<string, Task> warn,
        CancellationToken token);
}

// JobArguments class
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



// Define the ProductRaw class
public class ProductRaw
{
    public required string ProductId { get; set; } 
    public required string ProductName { get; set; }
    public decimal SalesPrice { get; set; }
    public decimal ListPrice { get; set; }
}

public class RawProductJob : IJob
{
    // EXECUTE METHOD
    public async Task<string> Execute(
        JobArguments arguments,
        Func<string, Task> info,
        Func<string, Task> warn,
        CancellationToken token)
    {
        string url = "https://cdn.relewise.com/academy/productdata/raw"; // Download raw product data from the Relewise URL
        using HttpClient client = new HttpClient(); // Create an HttpClient object

        await info("Downloading raw product data..."); 

        string rawData = await client.GetStringAsync(url, token);     // Download the raw data as a string

        if (string.IsNullOrWhiteSpace(rawData))    // Check if data was downloaded successfully
        {
            return "No data received.";   
        }

        var products = new List<Product>(); // Create a list to store the mapped products
        var lines = rawData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries); // Split the raw data into lines

        // Skip the header line
        for (int i = 1; i < lines.Length; i++) 
        {
            token.ThrowIfCancellationRequested(); // Check if the operation was cancelled

            var fields = lines[i].Split('|', StringSplitOptions.RemoveEmptyEntries); // Split the line into fields
            // Trim each field
            for (int j = 0; j < fields.Length; j++) 
            {
                fields[j] = fields[j].Trim();  
            }

            if (fields.Length < 5) 
                continue; // Skip incomplete lines

            var productId = fields[0]; 
            var productName = fields[1] + " " +  fields[2];
            var salesPriceString = fields[3].Replace("$", "").Trim(); // Trim the sales price and convert it to a string
            var listPriceString = fields[4].Replace("$", "").Trim(); // Trim the list price and convert it to a string
            // Log the values for debugging
            // await info (productName); // Log the product name (for debugging purposes)
            await info($"Processing Product ID: {productId}, Sales Price: {salesPriceString}, List Price: {listPriceString}"); // Log the product ID, sales price, and list price (for debugging purposes)

            // Validate that the sales price and list price are valid decimals
            if (decimal.TryParse(salesPriceString, out decimal salesPrice) &&  // Check if the sales price is a valid decimal
                decimal.TryParse(listPriceString, out decimal listPrice)) // Check if the list price is a valid decimal
            {
                var product = new Product(productId) // Create a new product with the product ID as  key
                {
                    DisplayName = new Multilingual(new Multilingual.Value(new Language("en"), productName)), 
                    ListPrice = new MultiCurrency("USD", listPrice),
                    SalesPrice = new MultiCurrency("USD", salesPrice),
                };

                products.Add(product); // Add the product to the list of products  
            }
            else
            {
                await warn($"Invalid price format for Product ID: {productId}, Sales Price: {salesPriceString}, List Price: {listPriceString}");
            } // Log an error message if the sales price or list price is not valid
        }

        return $"Mapped {products.Count} products successfully.";
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var job = new RawProductJob();
        var jobConfiguration = new Dictionary<string, string>(); 
        var arguments = new JobArguments(Guid.NewGuid(), "your_api_key", jobConfiguration); 
        
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(30));

        try
        {
            string result = await job.Execute(arguments,
                msg => Task.Run(() => Console.WriteLine(msg)),
                msg => Task.Run(() => Console.WriteLine($"Warning: {msg}")),
                cts.Token);

            Console.WriteLine(result);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("The operation was cancelled. Timeout exceeded.");
        }
    }
}
