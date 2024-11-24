$(document).ready(function () {
    $('#disposicionTable').DataTable({
        "language": {
            "zeroRecords": "No tienes ningun quimico pendiente de disposicion",
            "info": "_PAGE_ de _PAGES_",
            "infoEmpty": "No hay registros disponibles",
            "infoFiltered": "(filtrado de _MAX_ registros totales)",
            "search": "Buscar:",
            "paginate": {
                "first": "Primero",
                "last": "Último",
                "next": "Siguiente",
                "previous": "Anterior"
            }
        }
    });

    // Evento para cambio de estado con confirmación personalizada
    $('.estado-select').change(function () {
        const id = $(this).data('id');
        const estado = $(this).val();

        // Mensaje de confirmación según el estado seleccionado
        let mensajeConfirmacion;
        if (estado === "EnRevision") {
            mensajeConfirmacion = "Actualizarás el estado de este químico a En Revisión. Posteriormente tendrás que volver a actualizarlo para asegurarte que está fuera del almacén.";
        } else if (estado === "FueraDelAlmacen") {
            mensajeConfirmacion = "¿Estás seguro de que este químico fue segregado y está fuera del almacén?";
        } else {
            mensajeConfirmacion = `¿Está seguro de que desea cambiar el estado de la disposición a "${estado}"?`;
        }

        // Solicitar confirmación al usuario
        if (confirm(mensajeConfirmacion)) {
            $.ajax({
                url: '/Disposicion/UpdateEstado',
                type: 'POST',
                data: { id: id, estado: estado },
                success: function (response) {
                    alert(response.message);
                    location.reload(); // Recargar la página para reflejar los cambios
                },
                error: function (xhr, status, error) {
                    alert("Ocurrió un error al actualizar el estado.");
                }
            });
        } else {
            // Si el usuario cancela, revertir el cambio en el select
            $(this).val($(this).data('original'));
        }
    });

   
});
