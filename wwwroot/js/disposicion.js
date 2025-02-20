$(function () {
    const table = $('#disposicionTable').DataTable({
        "language": {
            "zeroRecords": "No tienes ningún químico pendiente de disposición",
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
    $('#disposicionTable').on('change', '.estado-select', function () {
        const select = $(this);
        const id = select.data('id');
        const estado = select.val();
        const original = select.data('original');

        let mensajeConfirmacion;
        if (estado === "EnRevision") {
            mensajeConfirmacion = "Estás por actualizar el estado a 'En Revisión'. " +
                "Posteriormente tendrás que actualizarlo a 'Fuera del Almacén' cuando confirmes su salida.";
        } else if (estado === "FueraDelAlmacen") {
            mensajeConfirmacion = "¿Estás seguro de que este químico fue segregado y ya no se encuentra en el almacén?";
        } else {
            mensajeConfirmacion = `¿Está seguro de que desea cambiar el estado de la disposición a "${estado}"?`;
        }

        if (confirm(mensajeConfirmacion)) {
            // Enviar la solicitud AJAX
            $.ajax({
                url: '/Disposicion/UpdateEstado',
                type: 'POST',
                data: { id: id, estado: estado },
                success: function (response) {
                    showMessage(response.message, 'success');
                    // Actualizar el valor original a la nueva selección
                    select.data('original', estado);

                    // Si el estado es "FueraDelAlmacen", mostrar input DMR
                    const dmrInput = $(`#dmrNumber-${id}`);
                    const dmrButton = $(`.dmr-submit[data-id="${id}"]`);
                    if (estado === "FueraDelAlmacen") {
                        dmrInput.show().attr('required', true);
                        dmrButton.show();
                    } else {
                        dmrInput.hide().removeAttr('required');
                        dmrButton.hide();
                    }
                },
                error: function () {
                    showMessage("Ocurrió un error al actualizar el estado.", 'danger');
                    // Revertir el cambio
                    select.val(original);
                }
            });
        } else {
            // Si el usuario cancela, revertir el cambio en el select
            select.val(original);
        }
    });

    // Evento para guardar DMR
    $('#disposicionTable').on('click', '.dmr-submit', function () {
        const disposicionId = $(this).data('id');
        const dmrNumber = $(`#dmrNumber-${disposicionId}`).val().trim();

        if (!dmrNumber) {
            showMessage('Debe ingresar un número de DMR antes de guardar.', 'danger');
            return;
        }

        $.ajax({
            url: '/Disposicion/CompletarDisposicion',
            method: 'POST',
            data: { id: disposicionId, dmrNumber: dmrNumber },
            success: function (response) {
                showMessage(response.message, 'success');
                setTimeout(() => {
                    location.reload();
                }, 2000);
            },
            error: function (xhr) {
                const errorMessage = xhr.responseJSON?.message || 'Error al actualizar la disposición.';
                showMessage(errorMessage, 'danger');
            }
        });
    });

    function showMessage(message, type) {
        const messageContainer = $('#message-container');
        const messageElement = $('#message');

        messageElement.text(message).removeClass().addClass(`alert alert-${type}`);
        messageContainer.show();

        setTimeout(() => {
            messageContainer.fadeOut();
        }, 5000);
    }
});
