function callFunction(action) {
    fetch('index.php', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: new URLSearchParams({ action: action })
    })
    .then(response => response.text())
    .then(data => {
        document.getElementById('result').innerHTML = data; //show the result on the page 
    })
    .catch(error => {
        document.getElementById('result').innerHTML = 'Error:' + error;
    });
}
