window.onpageshow = function (event) {
    if (event.persisted) {
        window.location.reload(); // Fuerza la recarga para verificar sesión
    }
};


window.onpopstate = function () {
    window.history.pushState(null, "", window.location.href);
};
