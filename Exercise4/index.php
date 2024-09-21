<?php

require __DIR__ . '/vendor/autoload.php';
use Relewise\Models\Product;
use Relewise\Models\Multilingual;
use Relewise\Models\MultiCurrency;

function mapProducts() {
    $url = "https://cdn.relewise.com/academy/productdata/customjsonfeed"; // Custom Relewise URL for JSON feed
    $jsonData = file_get_contents($url); // Download JSON data
    // echo $jsonData; // Print JSON data (for debugging purposes)
    if (!$jsonData) {
        return "JSON data not found.";
    }
    $productData = json_decode($jsonData, true); // Decode JSON data into an associative array
    // var_dump($productData); // Print associative array (for debugging purposes)
    $mappedCount = 0; 
    $mappedProducts = [];
    foreach ($productData as $productInfo) {

        //check if all required fields are present
        if (isset($productInfo['productId'], $productInfo['productName'], $productInfo['listPrice'], $productInfo['salesPrice'])) {
            $product = new Product();
            $product->setId($productInfo['productId']); 
            $product->setDisplayName(new Multilingual($productInfo['productName']));
            $product->setListPrice(new MultiCurrency($productInfo['listPrice']));
            $product->setSalesPrice(new MultiCurrency($productInfo['salesPrice']));
            $mappedProducts[] = $product;
            $mappedCount++;
        } else {
            echo "Missing required fields for product: " . $productInfo['productId'] . "\n";
            continue;
        }
    }
    return "Mapped " . $mappedCount . " products :) ";
}


echo mapProducts();
