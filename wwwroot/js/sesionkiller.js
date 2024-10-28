document.addEventListener("DOMContentLoaded", function () {
    const logoutButton = document.getElementById("logoutButton");

    if (logoutButton) {
        logoutButton.addEventListener("click", function () {
            fetch('/Account/Logout', { method: 'POST' })
                .then(response => {
                    if (response.ok) {
                        window.location.href = '/Account/Login'; // Redirigir al login después de cerrar sesión
                    } else {
                        console.error("Error al cerrar sesión:", response.statusText);
                    }
                })
                .catch(error => console.error("Error al cerrar sesión:", error));
        });
    } else {
        console.error("El botón de cerrar sesión no se encontró.");
    }
});
