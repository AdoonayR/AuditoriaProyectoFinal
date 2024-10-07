window.onpageshow = function (event) {
    if (event.persisted) {
        window.location.reload(); // Fuerza la recarga para verificar sesión
    }
};

// Evitar que el usuario navegue hacia atrás en la sesión
window.history.pushState(null, "", window.location.href);
window.onpopstate = function () {
    window.history.pushState(null, "", window.location.href);
};
