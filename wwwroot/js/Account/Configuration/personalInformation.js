function cleanFormInformation() {
    $(".invalid-feedback").hide();
    $('#Name').removeClass('is-invalid');
    $('#Name').removeClass('is-valid');
    $('#Email').removeClass('is-invalid');
    $('#Email').removeClass('is-valid');
}

$(document).ready(function () {
    $(document).on('submit', "#personalDataForm", function (e) {
        e.preventDefault();

        var formData = $(this).serialize();


        cleanFormInformation();


        var isValid = this.checkValidity();


        var email = $('#Email').val();
        if (!isValid) {
            $(this).addClass('was-validated');
        }

        if (isValid) {
            $.ajax({
                url: '/Account/UpdateInformation',
                type: 'POST',
                data: formData,
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Información actualizada',
                            text: 'Tu información ha sido actualizada correctamente.',
                            confirmButtonText: 'Aceptar'
                        }).then(() => {
                            location.reload();
                        });
                    } else {
                        if (response.message === "El correo electrónico ya está siendo utilizado.") {
                            $('#Email').addClass('is-invalid');
                            $('#Email').siblings('.invalid-feedback').text(response.message).show();
                        } else {
                            Swal.fire({
                                icon: 'error',
                                title: 'Error',
                                text: response.message,
                                confirmButtonText: 'Aceptar'
                            });

                            $(formName)[0].reset();
                            cleanFormInformation();
                        }
                    }
                },
                error: function (xhr, status, error) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error en la solicitud',
                        text: 'Hubo un problema al procesar tu solicitud. Por favor, contacta con soporte técnico.',
                        confirmButtonText: 'Aceptar'
                    });

                    $(formName)[0].reset();
                    cleanFormInformation();
                }
            });

        }
    });
    $(document).on('submit', "#credentialsForm", function (e) {
        e.preventDefault();

        var formData = $(this).serialize();
        cleanFormInformation();
        var isValid = this.checkValidity();
        if (!isValid) {
            $(this).addClass('was-validated');
        }

        if (isValid) {
            $.ajax({
                url: '/Account/UpdateCredentials',
                type: 'POST',
                data: formData,
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Información actualizada',
                            text: 'Tu información ha sido actualizada correctamente.',
                            confirmButtonText: 'Aceptar'
                        }).then(() => {
                            location.reload();
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: response.message,
                            confirmButtonText: 'Aceptar'
                        });

                        $(formName)[0].reset();
                        cleanFormInformation();
                    }
                },
                error: function (xhr, status, error) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error en la solicitud',
                        text: 'Hubo un problema al procesar tu solicitud. Por favor, contacta con soporte técnico.',
                        confirmButtonText: 'Aceptar'
                    });

                    $(formName)[0].reset();
                    cleanFormInformation();
                }
            });

        }
    });
});
