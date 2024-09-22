const axios = require('axios');
const Relewise = require('@relewise/client');
const { MultiCurrencyDataValue, MultilingualDataValue } = Relewise;

 class Product { 
     constructor() {
         this.id = String;
         this.displayName = MultilingualDataValue;
         this.listPrice = MultiCurrencyDataValue;
         this.salesPrice = MultiCurrencyDataValue;
     }
 } //couldnt figure out how to use this from relewise

// Exercise 1, mapProducts function from JSON data
async function mapProducts() {
    const url = "https://cdn.relewise.com/academy/productdata/customjsonfeed";
    try {
        const response = await axios.get(url);
        const productData = response.data;
        let mappedCount = 0;

        for (const localProduct of productData) {
            if (localProduct.productId && localProduct.productName && localProduct.listPrice && localProduct.salesPrice) {
                new Product(
                    localProduct.productId,
                    new MultilingualDataValue([{ value: localProduct.productName, language: "en" }]),
                    new MultiCurrencyDataValue([{ amount: localProduct.listPrice, currency: "DKK" }]),
                    new MultiCurrencyDataValue([{ amount: localProduct.salesPrice, currency: "DKK" }])
                );
                mappedCount++;
            } else {
                console.log(`Missing required fields for product: ${localProduct.productId || 'Unknown ID'}`);
            }
        }
        return `Mapped ${mappedCount} products from ${url}`;
    } catch (error) {
        return `Error loading JSON: ${url}`;
    }
}

// Exercise 2, mapGoogleShoppingFeed function from XML data
async function mapGoogleShoppingFeed() {
    const url = 'https://cdn.relewise.com/academy/productdata/googleshoppingfeed';
    try {
        const response = await axios.get(url);
        const xmlData = response.data;
        const xml = require('xml2js').parseStringPromise; // For parsing XML

        const parsedData = await xml(xmlData);
        const mappedProducts = [];

        for (const item of parsedData.rss.channel[0].item) {
            const product = new Product(
                item['g:id'][0],
                new MultilingualDataValue([{ value: item.title[0], language: "en" }]),
                new MultiCurrencyDataValue([{ amount: parseFloat(item['g:price'][0].replace(' USD', '').trim()), currency: 'USD' }]),
                new MultiCurrencyDataValue([{ amount: parseFloat(item['g:sale_price'][0].replace(' USD', '').trim()), currency: 'USD' }])
            );
            mappedProducts.push(product);
        }
        return `Mapped ${mappedProducts.length} products from ${url}`;
    } catch (error) {
        return `Error loading XML: ${url}`;
    }
}

// Exercise 3 - Raw Product Data 
async function mapRawProductData() {
    const url = 'https://cdn.relewise.com/academy/productdata/raw';
    try {
        const response = await axios.get(url); 
        const lines = response.data.split('\n'); // Split by new line
        const mappedProducts = []; // Create an array to store the mapped products


        for (const line of lines.slice(2)) { // Skip the header lines
            const fields = line.split('|').map(value => value.trim()).filter(value => value !== ''); // Use regex to remove empty fields, and split by '|'
            if (fields.length < 9) { // Ensure the correct number of fields (at least 9 expected), | 1 | Smart TV 32"|Samsung|$349.99|$399.99|Full HD Smart TV|Yes|Black|Electronics>TVs|(Example line)
                console.log(`Error: the line ID: ${fields[0] || "Unknown"} is not valid or incomplete (From url: ${url}).`); 
                continue; // Skip invalid lines
            }
            const product = new Product( 
                fields[0], // Set product ID
                new MultilingualDataValue([{ value: fields[1], language: "en" }]), // Set product name
                new MultiCurrencyDataValue([{ amount: parseFloat(fields[3].replace('$', '').trim()), currency: 'USD' }]), // Sales Price
                new MultiCurrencyDataValue([{ amount: parseFloat(fields[4].replace('$', '').trim()), currency: 'USD' }]) // List Price
            );
            mappedProducts.push(product);
        }
        return `Mapped ${mappedProducts.length} products from ${url}`;
    } catch (error) {
        return `Failed to fetch data from ${url}`; 
    }
}

(async () => { // Testing the functions in the console 
    console.log(await mapProducts());
    console.log(await mapGoogleShoppingFeed());
    console.log(await mapRawProductData());
})();

