function cleanFormInformation() {
    $(".invalid-feedback").hide();
    $('#Name').removeClass('is-invalid');
    $('#Name').removeClass('is-valid');
    $('#Email').removeClass('is-invalid');
    $('#Email').removeClass('is-valid');
}

$(document).ready(function () {
    let formName = '#personalDataForm';

    $(document).on('submit', formName, function (e) {
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
                        $('#confirmationModal').modal('show');

                        setTimeout(function () {
                            location.reload();
                        }, 5000);
                    } else {

                        if (response.message === "El correo electrónico ya está siendo utilizado.") {
                            $('#Email').addClass('is-invalid');
                            $('#Email').siblings('.invalid-feedback').text(response.message).show();
                        } else {
                            $('#errorModal').find(".modal-body").text(response.message);
                            $('#errorModal').modal('show');

                            $(formName)[0].reset();
                            cleanFormInformation();
                        }
                    }
                },
                error: function (xhr, status, error) {
                    $('#errorModal').find(".modal-body").text("Error al intentar acceder al controlador, por favor contacta con soporte técnico.");
                    $('#errorModal').modal('show');

                    $(formName)[0].reset();
                    cleanFormInformation();
                }
            });
        }
    });
});
