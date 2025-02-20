document.addEventListener("DOMContentLoaded", function () {
    // Seleccionar todos los botones de aprobación y agregar un evento de clic a cada uno
    document.querySelectorAll(".approve-btn").forEach(button => {
        button.addEventListener("click", function () {
            // Obtener la fecha y el rol del botón presionado
            const date = this.getAttribute("data-date");  // Valor en formato yyyy-MM-dd
            const role = this.getAttribute("data-role");

            console.log(`Fecha seleccionada (enviada al servidor): ${date}`);
            console.log(`Rol seleccionado: ${role}`);

            // Verificar si la fecha está presente
            if (!date) {
                alert("La fecha no está disponible. Intenta nuevamente.");
                return;
            }

            // Obtener la fecha actual 
            const currentDate = new Date();

            // Extraer mes, día y año 
            const month = String(currentDate.getMonth() + 1).padStart(2, '0');
            const day = String(currentDate.getDate()).padStart(2, '0');
            const year = currentDate.getFullYear();

            // Construir string en formato mm-dd-yyyy
            const formattedDate = `${month}-${day}-${year}`;

            // Confirmar con la fecha formateada
            const userConfirmed = confirm(`Firmarás la auditoría con fecha ${formattedDate}, ¿estás de acuerdo?`);

            if (!userConfirmed) {
                return; 
            }

            // Realizar la solicitud de aprobación al servidor usando Axios
            axios.post("/Supervisor/Approve", { date: date }, { params: { role: role } })
                .then(response => {
                    console.log(response.data.message);
                    alert(response.data.message);
                    location.reload(); // Recargar para reflejar los cambios
                })
                .catch(error => {
                    console.error("Error:", error.response?.data || error.message);
                    alert(`Error al aprobar químicos: ${error.response?.data || "Error desconocido"}`);
                });
        });
    });
});

document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("search-date"); 

    searchInput.addEventListener("input", function () {
        const filter = searchInput.value.toUpperCase();
        const rows = document.querySelectorAll(".audit-row");

        rows.forEach(row => {
            const dateCell = row.querySelector(".audit-date").textContent;
            if (dateCell.toUpperCase().includes(filter)) {
                row.style.display = "";
            } else {
                row.style.display = "none";
            }
        });
    });
});
