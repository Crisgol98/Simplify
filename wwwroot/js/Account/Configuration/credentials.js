$(document).ready(function () {
    $('#userSettingsForm').on('submit', function (e) {
        e.preventDefault();

        var formData = $(this).serialize();

        $.ajax({
            url: '/Account/EditCredentials',
            type: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    $('#confirmationModal').modal('show');
                    // Vaciar formulario
                    $('#userSettingsForm')[0].reset();
                    // Recargar pagina
                    setTimeout(function () {
                        location.reload();
                    }, 2000);
                }
            },
            error: function (xhr, status, error) {
                alert("Error al guardar los cambios. Por favor, intenta de nuevo.");
            }
        });
    });
});