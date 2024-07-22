document.addEventListener("DOMContentLoaded", function () {
    // Seleccionar todos los botones de aprobación y agregar un evento de clic a cada uno
    document.querySelectorAll(".approve-btn").forEach(button => {
        button.addEventListener("click", function () {
            // Obtener la fecha y el rol del botón presionado
            const date = this.getAttribute("data-date");
            const role = this.getAttribute("data-role");

            // Determinar la URL de aprobación según el rol
            const url = role === "Incoming" ? "/Supervisor/ApproveIncoming" : "/Supervisor/ApproveStorage";

            // Realizar la solicitud de aprobación al servidor
            fetch(url, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(date) // Enviar la fecha en el cuerpo de la solicitud
            })
                .then(response => response.json()) // Convertir la respuesta a JSON
                .then(data => {
                    alert(data.message); // Mostrar el mensaje de éxito
                    location.reload(); // Recargar la página para reflejar los cambios
                })
                .catch(error => console.error("Error:", error)); // Manejar errores
        });
    });
});
