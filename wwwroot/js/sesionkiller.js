document.addEventListener("DOMContentLoaded", function () {
    // Obtiene el botón de logout
    const logoutButton = document.getElementById("logoutButton");

    // Verifica si el botón existe antes de agregar el evento
    if (logoutButton) {
        // Agrega un evento de click al botón de logout
        logoutButton.addEventListener("click", function () {
            // Envía una solicitud POST para cerrar sesión
            fetch('/Account/Logout', { method: 'POST' })
                .then(response => {
                    // Si la respuesta es exitosa, redirige a la página de login
                    if (response.ok) {
                        window.location.href = '/Account/Login';
                    } else {
                        // Si hay un error en la respuesta, muestra un mensaje de error en la consola
                        console.error("Error al cerrar sesión:", response.statusText);
                    }
                })
                // Captura y muestra cualquier error que ocurra en la solicitud
                .catch(error => console.error("Error al cerrar sesión:", error));
        });
    } else {
        // Si el botón de logout no existe, muestra un mensaje de error en la consola
        console.error("El botón de cerrar sesión no se encontró.");
    }
});
