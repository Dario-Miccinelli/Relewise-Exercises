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
