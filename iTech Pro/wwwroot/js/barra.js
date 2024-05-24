
document.addEventListener("DOMContentLoaded", function () {
    const scrollContent = document.getElementById('scrollContent');

    // Función para obtener las tasas de cambio y actualizar el contenido del scroll
    function getExchangeRates() {
        const url = `https://openexchangerates.org/api/latest.json?app_id=1ffb95655dfb4c7d9068ef8aa90a09ec`;

        fetch(url)
            .then(response => response.json())
            .then(data => {
                const rates = data.rates;
                simulateFluctuations(rates); // Simular fluctuaciones en las tasas de cambio
                updateScrollContent(rates);
            })
            .catch(error => console.error('Error fetching exchange rates:', error));
    }

    // Función para simular fluctuaciones en las tasas de cambio
    function simulateFluctuations(rates) {
        for (let currency in rates) {
            let rate = rates[currency];
            let changePercentage = Math.random() * 0.05 - 0.025; // Genera un porcentaje de cambio aleatorio entre -2.5% y 2.5%
            rate *= 1 + changePercentage; // Aplica el cambio a la tasa de cambio actual
            rates[currency] = rate; // Actualiza la tasa de cambio en el objeto rates
        }
    }

    // Función para actualizar el contenido del scroll
    function updateScrollContent(rates) {
        let contentHTML = '<div>';
        for (let currency in rates) {
            let rate = rates[currency];
            let changeClass = rate > 1 ? 'up' : 'down'; // Determina si la tasa de cambio ha subido o bajado
            contentHTML += `<p><span class="${changeClass}">${rate.toFixed(2)}</span> ${currency}</p>`; // Agrega iconos de subida y bajada
        }
        contentHTML += '</div>';
        scrollContent.innerHTML = contentHTML;
    }

    getExchangeRates();  // Llamada inicial para poblar el contenido del scroll

    // Establece un intervalo para actualizar las tasas periódicamente
    setInterval(getExchangeRates, 60000); // Actualiza cada minuto (60000 ms)
});
