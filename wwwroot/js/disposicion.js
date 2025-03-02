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
        // Suponiendo que guardaste el "original" en data-original
        const original = select.data('original');

        let mensajeConfirmacion;
        if (estado === "EnRevision") {
            mensajeConfirmacion = "Estás por actualizar el estado a 'En Revisión'...";
        } else if (estado === "FueraDelAlmacen") {
            mensajeConfirmacion = "¿Estás seguro de que este químico salió?";
        } else {
            mensajeConfirmacion = `¿Desea cambiar el estado a "${estado}"?`;
        }

        if (confirm(mensajeConfirmacion)) {
            // Llamada AJAX a /Disposicion/UpdateEstado
            $.ajax({
                url: '/Disposicion/UpdateEstado',
                type: 'POST',
                data: { id: id, estado: estado },
                success: function (response) {
                    showMessage(response.message, 'success');
                    // Se actualiza el original
                    select.data('original', estado);

                    // Manejar visualización DMR
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
                    // Revertir cambio
                    select.val(original);
                }
            });
        } else {
            // Usuario canceló => revertir select
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
