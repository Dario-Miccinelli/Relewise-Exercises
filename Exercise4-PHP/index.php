<?php
require __DIR__ . '/vendor/autoload.php';
use Relewise\Models\Product;
use Relewise\Models\Multilingual;
use Relewise\Models\MultiCurrency;


// Exercise1
function mapProducts()
{
    $url = "https://cdn.relewise.com/academy/productdata/customjsonfeed";
    $jsonData = file_get_contents($url);
    if (!$jsonData) {
        return "Error loading JSON: " . $url;
    }
    $productData = json_decode($jsonData, true);
    $mappedCount = 0;
    $mappedProducts = [];

    foreach ($productData as $localProduct) {
        if (isset($localProduct['productId'], $localProduct['productName'], $localProduct['listPrice'], $localProduct['salesPrice'])) {
            $product = new Product();
            $product->setId($localProduct['productId']);
            $product->setDisplayName(new Multilingual($localProduct['productName']));
            $product->setListPrice(new MultiCurrency($localProduct['listPrice']));
            $product->setSalesPrice(new MultiCurrency($localProduct['salesPrice']));
            $mappedProducts[] = $product;
            $mappedCount++;
        } else {
            echo "Missing required fields for product: " . ($localProduct['productId'] ?? 'Unknown ID') . "\n";
            continue;
        }
    }
    return "Mapped " . $mappedCount . " products from " . $url;
}

// Exercise 2
function mapGoogleShoppingFeed()
{
    $url = 'https://cdn.relewise.com/academy/productdata/googleshoppingfeed';
    $data = file_get_contents($url);
    $xml = simplexml_load_string($data);
    if ($xml === false) {
        return "Error loading XML: " . $url;
    }
    $mappedProducts = [];

    foreach ($xml->channel->item as $item) {
        $product = new Product();
        $product->setId((string)$item->children('g', true)->id);
        $product->setDisplayName(new Multilingual((string)$item->title));
        $product->setListPrice(new MultiCurrency([['Currency' => 'USD', 'Value' => (float)trim($item->children('g', true)->price, ' USD')]]));
        $product->setSalesPrice(new MultiCurrency([['Currency' => 'USD', 'Value' => (float)trim($item->children('g', true)->sale_price, ' USD')]]));
        $mappedProducts[] = $product;
    }
    return "Mapped " . count($mappedProducts) . " products from " . $url;
}


// Exercise 3 
function mapRawProductData()
{
    $url = 'https://cdn.relewise.com/academy/productdata/raw';
    $data = file_get_contents($url);
    if ($data === false) {
        return "Failed to fetch data from " . $url;
    }
    $lines = explode("\n", $data);
    $mappedProducts = [];

    // Skip the header lines and map each product
    foreach (array_slice($lines, 2) as $line) {
        // Use preg_split to split fields by '|' and remove empty fields
        $fields = array_values(array_filter(preg_split('/\|/', $line), function($value) {
            return trim($value) !== '';
        }));

        // Ensure the correct number of fields (at least 9 expected)
        // | 1 | Smart TV 32" | Samsung | $349.99 | $399.99| Full HD Smart TV| Yes | Black| Electronics>TVs|  (Example line)
        if (count($fields) < 9) {
            echo "Error: the line ID: " . (isset($fields[0]) ? $fields[0] : "Unknown") . " is not valid or incomplete (From url:" . $url . ").\n";
            continue;
        }
        // Create a new product and map the fields
        $product = new Product();
        $product->setId($fields[0]); // Set product ID
        $product->setDisplayName(new Multilingual($fields[1])); // Set product name
        $product->setSalesPrice(new MultiCurrency([['Currency' => 'USD', 'Value' => (float)trim($fields[3], '$')]])); // Sales Price
        $product->setListPrice(new MultiCurrency([['Currency' => 'USD', 'Value' => (float)trim($fields[4], '$')]]));  // List Price
        $mappedProducts[] = $product;
    }
    return "Mapped " . count($mappedProducts) . " products from " . $url;
}

 echo mapProducts() . "\n";
 echo mapGoogleShoppingFeed() . "\n";
 echo mapRawProductData() . "\n";
