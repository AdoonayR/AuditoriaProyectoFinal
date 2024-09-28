document.addEventListener("DOMContentLoaded", function () {
    // Seleccionar todos los botones de aprobación y agregar un evento de clic a cada uno
    document.querySelectorAll(".approve-btn").forEach(button => {
        button.addEventListener("click", function () {
            // Obtener la fecha y el rol del botón presionado
            const date = this.getAttribute("data-date");  // Ahora el valor ya está en formato yyyy-MM-dd
            const role = this.getAttribute("data-role");

            console.log(`Fecha seleccionada (enviada al servidor): ${date}`);
            console.log(`Rol seleccionado: ${role}`);

            // Verificar si la fecha está presente
            if (!date) {
                alert("La fecha no está disponible. Intenta nuevamente.");
                return;
            }

            // Mostrar un mensaje de confirmación al usuario antes de proceder
            const userConfirmed = confirm(`¿Seguro que deseas aprobar los químicos para la fecha ${date}?`);
            if (!userConfirmed) {
                return; // El usuario canceló, no hacemos nada
            }

            // Realizar la solicitud de aprobación al servidor usando Axios
            axios.post("/Supervisor/Approve", { date: date }, { params: { role: role } })
                .then(response => {
                    console.log(response.data.message); // Mostrar el mensaje de éxito
                    alert(response.data.message); // Mostrar el mensaje en una alerta
                    location.reload(); // Recargar la página para reflejar los cambios
                })
                .catch(error => {
                    console.error("Error:", error.response?.data || error.message); // Manejar errores
                    // Mostrar un mensaje de error amigable al usuario
                    alert(`Error al aprobar químicos: ${error.response?.data || "Error desconocido"}`);
                });
        });
    });
});
