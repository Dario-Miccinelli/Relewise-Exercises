<?php
require __DIR__ . '/vendor/autoload.php';
use Relewise\Models\Product;
use Relewise\Models\Multilingual;
use Relewise\Models\MultiCurrency;

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
    echo "Mapped " . $mappedCount . " products from " . $url;
}

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
    echo "Mapped " . count($mappedProducts) . " products from " . $url;
}

function mapRawProductData()
{
    $url = 'https://cdn.relewise.com/academy/productdata/raw';
    $data = file_get_contents($url);

    $lines = explode("\n", $data);
    $mappedProducts = [];

    // Skip the header lines and map each product
    foreach (array_slice($lines, 2) as $line) {
        $fields = str_getcsv($line);
        $product = new Product();
        $product->setId($fields[0]);
        $product->setDisplayName(new Multilingual($fields[1]));
        $product->setSalesPrice(new MultiCurrency([['Currency' => 'USD', 'Value' => (float)trim($fields[3], '$')]])); // Sales Price
        $product->setListPrice(new MultiCurrency(['Currency' => 'USD', 'Value' => (float)trim($fields[4], '$')]));  // List Price

        $mappedProducts[] = $product;
    }
    echo "Mapped " . count($mappedProducts) . " products from " . $url;
}

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $action = $_POST['action'] ?? '';
    if ($action === 'mapProducts') {
        echo mapProducts();
    } elseif ($action === 'mapGoogleShoppingFeed') {
        echo mapGoogleShoppingFeed();
    } elseif ($action === 'mapRawProductData') {
        echo mapRawProductData();
    }
    exit;
}
?>

<!DOCTYPE html>
<html lang="it">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Mapping Products</title>
    <script src="script.js" defer></script>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH" crossorigin="anonymous">
</head>

<body class="container d-flex flex-column align-items-center justify-content-center min-vh-100 bg-light text-dark p-5">
    <div>
        <h1 class="display-2">Mapping Products</h1>
        <p class="lead text-center">Select an action to perform</p>
    </div>
    <div class="">
        <button onclick="callFunction('mapProducts')" class="btn btn-dark ">Map JSON Product Feed</button>
        <button onclick="callFunction('mapGoogleShoppingFeed')" class="btn btn-dark">Map Google Shopping Feed</button>
        <button onclick="callFunction('mapRawProductData')" class="btn btn-dark">Map Raw Product Data</button>
    </div>
    <div id="result" class="mt-5 w-100 d-flex justify-content-center"></div>  <!-- div for the results -->
</body>

</html>